using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AGE.SignatureHub.Domain.Entities;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace AGE.SignatureHub.Infrastructure.Persistence
{
    public class ApplicationDBContext : IdentityDbContext<
    ApplicationUser,
    ApplicationRole,
    Guid,
    IdentityUserClaim<Guid>,
    IdentityUserRole<Guid>,
    IdentityUserLogin<Guid>,
    IdentityRoleClaim<Guid>,
    IdentityUserToken<Guid>>
    {
        private IDbContextTransaction _currentTransaction;
        private readonly ILogger<ApplicationDBContext> _logger;

        public ApplicationDBContext(
            DbContextOptions<ApplicationDBContext> options,
            ILogger<ApplicationDBContext> logger)
            : base(options)
        {
            _logger = logger;
        }

        public DbSet<Document> Documents { get; set; }
        public DbSet<DocumentVersion> DocumentVersions { get; set; }
        public DbSet<SignatureFlow> SignatureFlows { get; set; }
        public DbSet<Signer> Signers { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<ExternalServiceConnection> ExternalServiceConnections { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            modelBuilder.Entity<Document>().
                HasQueryFilter(d => !d.IsDeleted);

            modelBuilder.Entity<SignatureFlow>().
                HasQueryFilter(sf => !sf.IsDeleted);

            modelBuilder.Entity<Signer>().HasQueryFilter(s => !s.IsDeleted);
            modelBuilder.Entity<ExternalServiceConnection>().HasQueryFilter(connection => !connection.IsDeleted);

            modelBuilder.Entity<ApplicationUser>().ToTable("Users");
            modelBuilder.Entity<ApplicationRole>().ToTable("Roles");
            modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
            modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
            modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
            modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
            modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");
        }

        public async Task BeginTransactionAsync( CancellationToken cancellationToken)
        {
            if (_currentTransaction != null)
            {
                return;
            }

            _currentTransaction = await Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken)
        {
            try
            {
                await SaveChangesAsync(cancellationToken);

                await _currentTransaction?.CommitAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                LogConcurrencyConflict(ex);
                await RollbackTransactionAsync(cancellationToken);
                throw;
            }
            catch
            {
                await RollbackTransactionAsync(cancellationToken);
                throw;
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }

        private void LogConcurrencyConflict(DbUpdateConcurrencyException ex)
        {
            foreach (var entry in ex.Entries)
            {
                var primaryKey = entry.Metadata.FindPrimaryKey();
                var keyValues = primaryKey == null
                    ? string.Empty
                    : string.Join(
                        ", ",
                        primaryKey.Properties.Select(property =>
                        {
                            var currentValue = entry.Property(property.Name).CurrentValue;
                            return $"{property.Name}={currentValue ?? "null"}";
                        }));

                var modifiedProperties = entry.Properties
                    .Where(property => property.IsModified)
                    .Select(property => $"{property.Metadata.Name}={property.CurrentValue ?? "null"}")
                    .ToArray();

                _logger.LogError(
                    ex,
                    "Concurrency conflict for entity {EntityType} with state {EntityState}. Keys: {Keys}. Modified properties: {ModifiedProperties}",
                    entry.Metadata.ClrType.Name,
                    entry.State,
                    keyValues,
                    modifiedProperties.Length == 0 ? "[none]" : string.Join("; ", modifiedProperties));
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.RollbackAsync(cancellationToken);
                }
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }
    }
}
