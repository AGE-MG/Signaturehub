using AGE.SignatureHub.Application.Contracts.Persistence;
using AGE.SignatureHub.Application.DTOs.Common;
using AGE.SignatureHub.Application.DTOs.Dashboard;
using AGE.SignatureHub.Domain.Enums;
using MediatR;

namespace AGE.SignatureHub.Application.Features.Dashboard.Queries.GetDashboardStats
{
    public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, BaseResponse<DashboardStatsDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetDashboardStatsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse<DashboardStatsDto>> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
        {
            var response = new BaseResponse<DashboardStatsDto>();

            try
            {
                var documents = await _unitOfWork.Documents.GetByCreatorAsync(request.UserIdPacket, cancellationToken);
                var unreadCount = await _unitOfWork.Notifications.CountUnreadByUserIdAsync(request.UserIdPacket, cancellationToken);

                response.Data = new DashboardStatsDto
                {
                    TotalDocuments = documents.Count,
                    DraftDocuments = documents.Count(d => d.Status == DocumentStatus.Draft),
                    PendingDocuments = documents.Count(d => d.Status == DocumentStatus.PendingSignatures || d.Status == DocumentStatus.PartiallyCompleted),
                    CompletedDocuments = documents.Count(d => d.Status == DocumentStatus.Completed),
                    RejectedDocuments = documents.Count(d => d.Status == DocumentStatus.Rejected),
                    ExpiredDocuments = documents.Count(d => d.Status == DocumentStatus.Expired),
                    UnreadNotifications = unreadCount
                };

                response.Success = true;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "An error occurred while retrieving dashboard stats.";
                response.Errors = new List<string> { ex.Message };
                return response;
            }
        }
    }
}
