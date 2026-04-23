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
            var response = new BaseResponse<List<RecentDocumentDto>>();

            try
            {
                var documents = await _unitOfWork.Documents.GetByCreatorAsync(request.UserIdPacket, cancellationToken);

                response.Data = documents
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
                    .ToList();

                response.Success = true;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "An error occurred while retrieving recent documents.";
                response.Errors = new List<string> { ex.Message };
                return response;
            }
        }
    }
}
