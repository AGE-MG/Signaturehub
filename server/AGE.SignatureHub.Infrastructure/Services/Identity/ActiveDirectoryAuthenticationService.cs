using System.DirectoryServices.Protocols;
using System.Net;
using AGE.SignatureHub.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AGE.SignatureHub.Infrastructure.Services.Identity
{
    public class ActiveDirectoryAuthenticationService
    {
        private readonly ActiveDirectorySettings _settings;
        private readonly ILogger<ActiveDirectoryAuthenticationService> _logger;

        public ActiveDirectoryAuthenticationService(
            IOptions<ActiveDirectorySettings> settings,
            ILogger<ActiveDirectoryAuthenticationService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public bool IsEnabled => _settings.Enabled;

        public bool AllowLocalFallback => _settings.AllowLocalFallback;

        public async Task<ActiveDirectoryAuthenticationResult?> AuthenticateAsync(string login, string password, CancellationToken cancellationToken = default)
        {
            if (!IsEnabled)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            return await Task.Run(() =>
            {
                var normalizedLogin = login.Trim();
                var upn = BuildUserPrincipalName(normalizedLogin);
                var accountName = ExtractAccountName(normalizedLogin);

                using var connection = CreateConnection(upn, password);

                try
                {
                    connection.Bind();
                    _logger.LogInformation("Active Directory authentication succeeded for {Login}", normalizedLogin);

                    var userInfo = TryLoadUserInfo(connection, normalizedLogin, upn, accountName);

                    return new ActiveDirectoryAuthenticationResult
                    {
                        AccountName = userInfo.AccountName ?? accountName,
                        UserPrincipalName = userInfo.UserPrincipalName ?? upn,
                        Email = userInfo.Email ?? BuildEmail(accountName, upn),
                        DisplayName = userInfo.DisplayName ?? accountName,
                        Department = userInfo.Department,
                        Position = userInfo.Position,
                        RegistrationNumber = userInfo.RegistrationNumber
                    };
                }
                catch (LdapException ex)
                {
                    _logger.LogWarning(ex, "Active Directory authentication failed for {Login}", normalizedLogin);
                    return null;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected Active Directory authentication error for {Login}", normalizedLogin);
                    return null;
                }
            }, cancellationToken);
        }

        private LdapConnection CreateConnection(string upn, string password)
        {
            var identifier = new LdapDirectoryIdentifier(_settings.Server, _settings.Port);
            var credential = new NetworkCredential(upn, password);
            var connection = new LdapConnection(identifier, credential, AuthType.Basic);

            connection.SessionOptions.ProtocolVersion = 3;
            connection.SessionOptions.ReferralChasing = ReferralChasingOptions.None;

            if (_settings.UseSsl)
            {
                connection.SessionOptions.SecureSocketLayer = true;
            }

            return connection;
        }

        private ActiveDirectoryUserInfo TryLoadUserInfo(LdapConnection connection, string login, string upn, string accountName)
        {
            try
            {
                var searchBase = ResolveSearchBase();
                if (string.IsNullOrWhiteSpace(searchBase))
                {
                    return new ActiveDirectoryUserInfo();
                }

                var loginEscaped = EscapeLdapFilter(login);
                var upnEscaped = EscapeLdapFilter(upn);
                var accountEscaped = EscapeLdapFilter(accountName);
                var filter = $"(&(objectClass=user)(|(userPrincipalName={upnEscaped})(sAMAccountName={accountEscaped})(mail={loginEscaped})))";

                var request = new SearchRequest(
                    searchBase,
                    filter,
                    SearchScope.Subtree,
                    new[] { "displayName", "mail", "department", "title", "employeeID", "userPrincipalName", "sAMAccountName" });

                var response = (SearchResponse)connection.SendRequest(request);
                var entry = response.Entries.Cast<SearchResultEntry>().FirstOrDefault();
                if (entry == null)
                {
                    return new ActiveDirectoryUserInfo();
                }

                return new ActiveDirectoryUserInfo
                {
                    DisplayName = GetAttributeValue(entry, "displayName"),
                    Email = GetAttributeValue(entry, "mail"),
                    Department = GetAttributeValue(entry, "department"),
                    Position = GetAttributeValue(entry, "title"),
                    RegistrationNumber = GetAttributeValue(entry, "employeeID"),
                    UserPrincipalName = GetAttributeValue(entry, "userPrincipalName"),
                    AccountName = GetAttributeValue(entry, "sAMAccountName")
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load Active Directory attributes for {Login}", login);
                return new ActiveDirectoryUserInfo();
            }
        }

        private string ResolveSearchBase()
        {
            if (!string.IsNullOrWhiteSpace(_settings.SearchBase))
            {
                return _settings.SearchBase;
            }

            var domain = !string.IsNullOrWhiteSpace(_settings.UserPrincipalNameSuffix)
                ? _settings.UserPrincipalNameSuffix
                : _settings.Domain;

            if (string.IsNullOrWhiteSpace(domain))
            {
                return string.Empty;
            }

            return string.Join(",", domain
                .Split('.', StringSplitOptions.RemoveEmptyEntries)
                .Select(part => $"DC={part}"));
        }

        private string BuildUserPrincipalName(string login)
        {
            if (login.Contains('@'))
            {
                return login;
            }

            var suffix = !string.IsNullOrWhiteSpace(_settings.UserPrincipalNameSuffix)
                ? _settings.UserPrincipalNameSuffix
                : _settings.Domain;

            return string.IsNullOrWhiteSpace(suffix)
                ? login
                : $"{login}@{suffix}";
        }

        private string ExtractAccountName(string login)
        {
            return login.Contains('@')
                ? login[..login.IndexOf('@')]
                : login;
        }

        private string BuildEmail(string accountName, string upn)
        {
            if (upn.Contains('@'))
            {
                return upn;
            }

            if (!string.IsNullOrWhiteSpace(_settings.EmailDomain))
            {
                return $"{accountName}@{_settings.EmailDomain}";
            }

            if (!string.IsNullOrWhiteSpace(_settings.UserPrincipalNameSuffix))
            {
                return $"{accountName}@{_settings.UserPrincipalNameSuffix}";
            }

            if (!string.IsNullOrWhiteSpace(_settings.Domain))
            {
                return $"{accountName}@{_settings.Domain}";
            }

            return upn;
        }

        private static string? GetAttributeValue(SearchResultEntry entry, string attributeName)
        {
            if (!entry.Attributes.Contains(attributeName))
            {
                return null;
            }

            var values = entry.Attributes[attributeName];
            return values == null || values.Count == 0
                ? null
                : values[0]?.ToString();
        }

        private static string EscapeLdapFilter(string value)
        {
            return value
                .Replace("\\", "\\5c")
                .Replace("*", "\\2a")
                .Replace("(", "\\28")
                .Replace(")", "\\29")
                .Replace("\0", "\\00");
        }

        private sealed class ActiveDirectoryUserInfo
        {
            public string? AccountName { get; init; }
            public string? UserPrincipalName { get; init; }
            public string? Email { get; init; }
            public string? DisplayName { get; init; }
            public string? Department { get; init; }
            public string? Position { get; init; }
            public string? RegistrationNumber { get; init; }
        }
    }

    public sealed class ActiveDirectoryAuthenticationResult
    {
        public string AccountName { get; init; } = string.Empty;
        public string UserPrincipalName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string DisplayName { get; init; } = string.Empty;
        public string? Department { get; init; }
        public string? Position { get; init; }
        public string? RegistrationNumber { get; init; }
    }
}
