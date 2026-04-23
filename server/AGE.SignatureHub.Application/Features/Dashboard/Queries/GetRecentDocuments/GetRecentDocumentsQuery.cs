using AGE.SignatureHub.Application.DTOs.Common;
using AGE.SignatureHub.Application.DTOs.Dashboard;
using MediatR;

namespace AGE.SignatureHub.Application.Features.Dashboard.Queries.GetRecentDocuments
{
    public class GetRecentDocumentsQuery : IRequest<BaseResponse<List<RecentDocumentDto>>>
    {
        public Guid UserIdPacket { get; set; }
        public int Count { get; set; } = 5;
    }
}
