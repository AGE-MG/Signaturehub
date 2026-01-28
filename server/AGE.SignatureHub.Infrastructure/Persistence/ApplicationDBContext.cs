using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace AGE.SignatureHub.Infrastructure.Persistence
{
    public class ApplicationDBContext : DbContext
    {
        private IDbContextTransaction _currentTransaction;

        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options)
            : base(options)
        {
        }

        public DbSet<Document> Documents { get; set; }
        public DbSet<DocumentVersion> DocumentVersions { get; set; }
        public DbSet<SignatureFlow> SignatureFlows { get; set; }
        public DbSet<Signer> Signers { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }


    }
}