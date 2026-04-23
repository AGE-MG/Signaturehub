using AGE.SignatureHub.Application.DTOs.Common;
using AGE.SignatureHub.Application.DTOs.Dashboard;
using MediatR;

namespace AGE.SignatureHub.Application.Features.Dashboard.Queries.GetUserNotifications
{
    public class GetUserNotificationsQuery : IRequest<BaseResponse<List<NotificationDto>>>
    {
        public Guid UserIdPacket { get; set; }
        public bool UnreadOnly { get; set; }
    }
}
