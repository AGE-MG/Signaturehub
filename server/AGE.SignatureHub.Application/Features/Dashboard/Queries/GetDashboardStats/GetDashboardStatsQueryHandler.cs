using AGE.SignatureHub.Application.Contracts.Persistence;
using AGE.SignatureHub.Application.Contracts.Identity;
using AGE.SignatureHub.Application.DTOs.Common;
using AGE.SignatureHub.Application.DTOs.Dashboard;
using AGE.SignatureHub.Domain.Enums;
using MediatR;

namespace AGE.SignatureHub.Application.Features.Dashboard.Queries.GetDashboardStats
{
    public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, BaseResponse<DashboardStatsDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserManagementService _userManagementService;

        public GetDashboardStatsQueryHandler(IUnitOfWork unitOfWork, IUserManagementService userManagementService)
        {
            _unitOfWork = unitOfWork;
            _userManagementService = userManagementService;
        }

        public async Task<BaseResponse<DashboardStatsDto>> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
        {
            var user = await _userManagementService.GetByIdAsync(request.UserIdPacket, cancellationToken);
            var documents = await _unitOfWork.Documents.GetAccessibleDocumentsAsync(
                request.UserIdPacket,
                user.Email,
                user.Department,
                cancellationToken: cancellationToken);
            var unreadCount = await _unitOfWork.Notifications.CountUnreadByUserIdAsync(request.UserIdPacket, cancellationToken);

            return new BaseResponse<DashboardStatsDto>
            {
                Success = true,
                Data = new DashboardStatsDto
                {
                    TotalDocuments = documents.Count,
                    DraftDocuments = documents.Count(d => d.Status == DocumentStatus.Draft),
                    PendingDocuments = documents.Count(d => d.Status == DocumentStatus.PendingSignatures || d.Status == DocumentStatus.PartiallyCompleted),
                    CompletedDocuments = documents.Count(d => d.Status == DocumentStatus.Completed),
                    RejectedDocuments = documents.Count(d => d.Status == DocumentStatus.Rejected),
                    ExpiredDocuments = documents.Count(d => d.Status == DocumentStatus.Expired),
                    UnreadNotifications = unreadCount
                }
            };
        }
    }
}
