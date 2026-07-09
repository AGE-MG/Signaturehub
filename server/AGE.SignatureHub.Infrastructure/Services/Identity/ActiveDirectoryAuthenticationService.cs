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

        public async Task<ActiveDirectoryAuthenticationResult?> LookupUserAsync(string login, string? email = null, CancellationToken cancellationToken = default)
        {
            if (!IsEnabled || string.IsNullOrWhiteSpace(login))
            {
                return null;
            }

            return await Task.Run(() =>
            {
                var normalizedLogin = login.Trim();
                var accountName = ExtractAccountName(normalizedLogin);
                var upn = BuildUserPrincipalName(accountName);

                try
                {
                    using var connection = CreateLookupConnection();
                    connection.Bind();

                    var userInfo = TryLoadUserInfo(connection, normalizedLogin, upn, accountName);
                    if (userInfo.IsEmpty)
                    {
                        return null;
                    }

                    return new ActiveDirectoryAuthenticationResult
                    {
                        AccountName = userInfo.AccountName ?? accountName,
                        UserPrincipalName = userInfo.UserPrincipalName ?? upn,
                        Email = userInfo.Email ?? BuildEmail(accountName, email ?? upn),
                        DisplayName = userInfo.DisplayName ?? accountName,
                        Department = userInfo.Department ?? userInfo.OrganizationalPath,
                        Position = userInfo.Position,
                        RegistrationNumber = userInfo.RegistrationNumber,
                        DistinguishedName = userInfo.DistinguishedName,
                        CanonicalName = userInfo.CanonicalName,
                        OrganizationalUnits = userInfo.OrganizationalUnits,
                        OrganizationalPath = userInfo.OrganizationalPath
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to look up Active Directory attributes for {Login}", normalizedLogin);
                    return null;
                }
            }, cancellationToken);
        }

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
                        Department = userInfo.Department ?? userInfo.OrganizationalPath,
                        Position = userInfo.Position,
                        RegistrationNumber = userInfo.RegistrationNumber,
                        DistinguishedName = userInfo.DistinguishedName,
                        CanonicalName = userInfo.CanonicalName,
                        OrganizationalUnits = userInfo.OrganizationalUnits,
                        OrganizationalPath = userInfo.OrganizationalPath
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

        private LdapConnection CreateLookupConnection()
        {
            var identifier = new LdapDirectoryIdentifier(_settings.Server, _settings.Port);

            LdapConnection connection;

            if (!string.IsNullOrWhiteSpace(_settings.LookupBindUser) && !string.IsNullOrWhiteSpace(_settings.LookupBindPassword))
            {
                var bindUser = BuildUserPrincipalName(_settings.LookupBindUser.Trim());
                var credential = new NetworkCredential(bindUser, _settings.LookupBindPassword);
                connection = new LdapConnection(identifier, credential, AuthType.Basic);
            }
            else if (_settings.LookupUseDefaultCredentials)
            {
                connection = new LdapConnection(identifier)
                {
                    AuthType = AuthType.Negotiate,
                    Credential = CredentialCache.DefaultNetworkCredentials
                };
            }
            else
            {
                connection = new LdapConnection(identifier);
            }

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
                var requestedAttributes = BuildRequestedAttributes();

                var request = new SearchRequest(
                    searchBase,
                    filter,
                    SearchScope.Subtree,
                    requestedAttributes);

                var response = (SearchResponse)connection.SendRequest(request);
                var entry = response.Entries.Cast<SearchResultEntry>().FirstOrDefault();
                if (entry == null)
                {
                    return new ActiveDirectoryUserInfo();
                }

                return new ActiveDirectoryUserInfo
                {
                    DisplayName = GetFirstAttributeValue(entry, _settings.DisplayNameAttributes),
                    Email = GetAttributeValue(entry, "mail"),
                    Department = GetFirstAttributeValue(entry, _settings.DepartmentAttributes),
                    Position = GetFirstAttributeValue(entry, _settings.PositionAttributes),
                    RegistrationNumber = GetFirstAttributeValue(entry, _settings.RegistrationNumberAttributes),
                    UserPrincipalName = GetAttributeValue(entry, "userPrincipalName"),
                    AccountName = GetAttributeValue(entry, "sAMAccountName"),
                    DistinguishedName = GetAttributeValue(entry, "distinguishedName"),
                    CanonicalName = GetAttributeValue(entry, "canonicalName"),
                    OrganizationalUnits = ExtractOrganizationalUnits(GetAttributeValue(entry, "distinguishedName")),
                    OrganizationalPath = BuildOrganizationalPath(ExtractOrganizationalUnits(GetAttributeValue(entry, "distinguishedName")))
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

        private string[] BuildRequestedAttributes()
        {
            return _settings.DisplayNameAttributes
                .Concat(_settings.DepartmentAttributes)
                .Concat(_settings.PositionAttributes)
                .Concat(_settings.RegistrationNumberAttributes)
                .Concat(["mail", "userPrincipalName", "sAMAccountName", "distinguishedName", "canonicalName"])
                .Where(attribute => !string.IsNullOrWhiteSpace(attribute))
                .Select(attribute => attribute.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        private List<string> ExtractOrganizationalUnits(string? distinguishedName)
        {
            if (string.IsNullOrWhiteSpace(distinguishedName))
            {
                return [];
            }

            return distinguishedName
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(part => part.Trim())
                .Where(part => part.StartsWith("OU=", StringComparison.OrdinalIgnoreCase))
                .Select(part => part[3..].Trim())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Reverse()
                .ToList();
        }

        private string? BuildOrganizationalPath(List<string> organizationalUnits)
        {
            if (organizationalUnits.Count == 0)
            {
                return null;
            }

            var filtered = organizationalUnits
                .Where(ou => !_settings.DepartmentOuIgnoreList.Any(ignore => string.Equals(ignore, ou, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (filtered.Count == 0)
            {
                filtered = organizationalUnits;
            }

            return filtered.Count == 0
                ? null
                : string.Join(" / ", filtered);
        }

        private static string? GetFirstAttributeValue(SearchResultEntry entry, IEnumerable<string> attributeNames)
        {
            foreach (var attributeName in attributeNames.Where(attribute => !string.IsNullOrWhiteSpace(attribute)))
            {
                var value = GetAttributeValue(entry, attributeName.Trim());
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            return null;
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
            public string? DistinguishedName { get; init; }
            public string? CanonicalName { get; init; }
            public List<string> OrganizationalUnits { get; init; } = [];
            public string? OrganizationalPath { get; init; }
            public bool IsEmpty =>
                string.IsNullOrWhiteSpace(AccountName) &&
                string.IsNullOrWhiteSpace(UserPrincipalName) &&
                string.IsNullOrWhiteSpace(Email) &&
                string.IsNullOrWhiteSpace(DisplayName) &&
                string.IsNullOrWhiteSpace(Department) &&
                string.IsNullOrWhiteSpace(Position) &&
                string.IsNullOrWhiteSpace(RegistrationNumber) &&
                string.IsNullOrWhiteSpace(DistinguishedName) &&
                string.IsNullOrWhiteSpace(CanonicalName) &&
                OrganizationalUnits.Count == 0 &&
                string.IsNullOrWhiteSpace(OrganizationalPath);
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
        public string? DistinguishedName { get; init; }
        public string? CanonicalName { get; init; }
        public List<string> OrganizationalUnits { get; init; } = [];
        public string? OrganizationalPath { get; init; }
    }
}
