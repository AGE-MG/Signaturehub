using AGE.SignatureHub.Application.Contracts.Persistence;
using AGE.SignatureHub.Application.DTOs.Common;
using AGE.SignatureHub.Application.DTOs.Dashboard;
using MediatR;

namespace AGE.SignatureHub.Application.Features.Dashboard.Queries.GetRecentDocuments
{
    public class GetRecentDocumentsQueryHandler : IRequestHandler<GetRecentDocumentsQuery, BaseResponse<List<RecentDocumentDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetRecentDocumentsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse<List<RecentDocumentDto>>> Handle(GetRecentDocumentsQuery request, CancellationToken cancellationToken)
        {
            var documents = await _unitOfWork.Documents.GetByCreatorAsync(request.UserIdPacket, cancellationToken);

            return new BaseResponse<List<RecentDocumentDto>>
            {
                Success = true,
                Data = documents
                    .OrderByDescending(d => d.UpdatedAt)
                    .Take(request.Count)
                    .Select(d => new RecentDocumentDto
                    {
                        Id = d.Id,
                        Title = d.Title,
                        OriginalFileName = d.OriginalFileName,
                        FileExtension = d.FileExtension,
                        Status = d.Status,
                        CreatedAt = d.CreatedAt,
                        UpdatedAt = d.UpdatedAt
                    })
                    .ToList()
            };
        }
    }
}
