using AGE.SignatureHub.Application.Contracts.Persistence;
using AGE.SignatureHub.Application.DTOs.Common;
using AGE.SignatureHub.Application.DTOs.Dashboard;
using MediatR;

namespace AGE.SignatureHub.Application.Features.Dashboard.Queries.GetUserNotifications
{
    public class GetUserNotificationsQueryHandler : IRequestHandler<GetUserNotificationsQuery, BaseResponse<List<NotificationDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetUserNotificationsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse<List<NotificationDto>>> Handle(GetUserNotificationsQuery request, CancellationToken cancellationToken)
        {
            var response = new BaseResponse<List<NotificationDto>>();

            try
            {
                var notifications = await _unitOfWork.Notifications.GetByUserIdAsync(request.UserIdPacket, request.UnreadOnly, cancellationToken);

                response.Data = notifications
                    .Select(n => new NotificationDto
                    {
                        Id = n.Id,
                        Title = n.Title,
                        Message = n.Message,
                        Type = n.Type,
                        IsRead = n.IsRead,
                        RelatedDocumentId = n.RelatedDocumentId,
                        CreatedAt = n.CreatedAt
                    })
                    .ToList();

                response.Success = true;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "An error occurred while retrieving notifications.";
                response.Errors = new List<string> { ex.Message };
                return response;
            }
        }
    }
}
