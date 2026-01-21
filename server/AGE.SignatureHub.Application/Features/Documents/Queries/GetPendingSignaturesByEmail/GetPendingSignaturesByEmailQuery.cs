using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Application.DTOs.Common;
using AGE.SignatureHub.Application.DTOs.Signer;
using MediatR;

namespace AGE.SignatureHub.Application.Features.Documents.Queries.GetPendingSignaturesByEmail
{
    public class GetPendingSignaturesByEmailQuery : IRequest<BaseResponse<List<SignerDto>>>
    {
        public string Email { get; set; }

        public GetPendingSignaturesByEmailQuery(string email)
        {
            Email = email;
        }
    }
}