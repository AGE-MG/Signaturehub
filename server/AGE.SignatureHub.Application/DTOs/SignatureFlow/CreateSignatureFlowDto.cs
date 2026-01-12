using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Application.DTOs.Signer;
using AGE.SignatureHub.Domain.Enums;

namespace AGE.SignatureHub.Application.DTOs.SignatureFlow
{
    public class CreateSignatureFlowDto
    {
        public Guid DocumentId { get; set; }
        public string FlowName { get; set; } = string.Empty;
        public FlowType FlowType { get; set; }
        public List<CreateSignerDto> Signers { get; set; } = new();
    }
}