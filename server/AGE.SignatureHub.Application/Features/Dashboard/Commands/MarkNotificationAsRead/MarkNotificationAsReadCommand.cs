using AGE.SignatureHub.Application.DTOs.Common;
using MediatR;

namespace AGE.SignatureHub.Application.Features.Dashboard.Commands.MarkNotificationAsRead
{
    public class MarkNotificationAsReadCommand : IRequest<BaseResponse>
    {
        public Guid NotificationId { get; set; }
    }
}
