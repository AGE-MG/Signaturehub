using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Application.DTOs.Signer;
using AGE.SignatureHub.Domain.Enums;

namespace AGE.SignatureHub.Application.DTOs.SignatureFlow
{
    public class SignatureFlowDto
    {
        public Guid Id { get; set; }
        public Guid DocumentId { get; set; }
        public string FlowName { get; set; } = string.Empty;
        public FlowType FlowType { get; set; }
        public int CurrentStep { get; set; }
        public int TotalSteps { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
        public List<SignerDto> Signers { get; set; } = new();
    }
}