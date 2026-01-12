using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Domain.Common;
using AGE.SignatureHub.Domain.Enums;

namespace AGE.SignatureHub.Domain.Entities
{
    public class SignatureFlow : BaseEntity
    {
        public Guid DocumentId { get; private set; }
        public Document Document { get; private set; } = null!;
        public string FlowName { get; private set; } = string.Empty;
        public FlowType FlowType { get; private set; }
        public int CurrentStep { get; private set; }
        public int TotalSteps { get; private set; }
        public bool IsCompleted { get; private set; }
        public DateTime? CompletedAt { get; private set; }

        private readonly List<Signer> _signers = new();
        public IReadOnlyCollection<Signer> Signers => _signers.AsReadOnly();

        private SignatureFlow() { }

        public SignatureFlow(
            Guid documentId,
            string flowName,
            FlowType flowType
        )
        {
            DocumentId = documentId;
            FlowName = flowName ?? throw new ArgumentNullException(nameof(flowName));
            IsCompleted = false;
            CurrentStep = 1;
            FlowType = flowType;
        }

        public void AddSigner(Signer signer)
        {
            _signers.Add(signer);
            RecalculateTotalSteps();
            SetUpdatedAt();
        }

        public void UpdateCurrentStep(int step)
        {
            if (step < 1 || step > TotalSteps)
                throw new ArgumentOutOfRangeException(nameof(step), "Step is out of range.");

            CurrentStep = step;
            SetUpdatedAt();
        }

        public void MarkAsCompleted()
        {
            IsCompleted = true;
            CompletedAt = DateTime.UtcNow;
            SetUpdatedAt();
        }

        public bool CanSignerSign(Guid signerId)
        {
            var signer = _signers.FirstOrDefault(s => s.Id == signerId);
            if (signer == null) return false;

            return FlowType switch
            {
                FlowType.Sequential => signer.SignOrder == CurrentStep && signer.Status == SignatureStatus.Pending,
                FlowType.Parallel => signer.Status == SignatureStatus.Pending,
                FlowType.Hybrid => signer.Status == SignatureStatus.Pending,
                _ => false,
            };
        }

        private void RecalculateTotalSteps()
        {
            TotalSteps = FlowType switch
            {
                FlowType.Sequential => _signers.Max(s => s.SignOrder),
                FlowType.Parallel => 1,
                FlowType.Hybrid => _signers.Max(s => s.SignOrder),
                _ => 0,
            };
        }
    }
}