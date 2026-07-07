using AGE.SignatureHub.Application.Contracts.Persistence;
using AGE.SignatureHub.Application.Contracts.Identity;
using AGE.SignatureHub.Application.DTOs.Common;
using AGE.SignatureHub.Application.DTOs.Dashboard;
using MediatR;

namespace AGE.SignatureHub.Application.Features.Dashboard.Queries.GetRecentDocuments
{
    public class GetRecentDocumentsQueryHandler : IRequestHandler<GetRecentDocumentsQuery, BaseResponse<List<RecentDocumentDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserManagementService _userManagementService;

        public GetRecentDocumentsQueryHandler(IUnitOfWork unitOfWork, IUserManagementService userManagementService)
        {
            _unitOfWork = unitOfWork;
            _userManagementService = userManagementService;
        }

        public async Task<BaseResponse<List<RecentDocumentDto>>> Handle(GetRecentDocumentsQuery request, CancellationToken cancellationToken)
        {
            var user = await _userManagementService.GetByIdAsync(request.UserIdPacket, cancellationToken);
            var documents = await _unitOfWork.Documents.GetAccessibleDocumentsAsync(
                request.UserIdPacket,
                user.Email,
                user.Department,
                cancellationToken: cancellationToken);

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
