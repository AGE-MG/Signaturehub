using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Application.Contracts.Persistence;
using AGE.SignatureHub.Infrastructure.Persistence.Repositories;

namespace AGE.SignatureHub.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDBContext _context;
        private IDocumentRepository? _documents;
        private ISignatureFlowRepository? _signatureFlows; 
        private ISignerRepository? _signers;
        private IAuditLogRepository? _auditLogs;
        public UnitOfWork(ApplicationDBContext context)
        {
            _context = context;
        }

        public IDocumentRepository Documents => _documents ??= new DocumentRepository(_context);

        public ISignatureFlowRepository SignatureFlows => _signatureFlows ??= new SignatureFlowRepository(_context);

        public ISignerRepository Signers => _signers ??= new SignerRepository(_context);
        public IAuditLogRepository AuditLogs => _auditLogs ??= new AuditLogRepository(_context);

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            await _context.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            await _context.CommitTransactionAsync(cancellationToken);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            await _context.RollbackTransactionAsync(cancellationToken);
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}