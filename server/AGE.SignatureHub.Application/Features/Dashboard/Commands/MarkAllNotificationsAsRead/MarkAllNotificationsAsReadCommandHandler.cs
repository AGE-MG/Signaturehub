using AGE.SignatureHub.Application.Contracts.Persistence;
using AGE.SignatureHub.Application.DTOs.Common;
using MediatR;

namespace AGE.SignatureHub.Application.Features.Dashboard.Commands.MarkAllNotificationsAsRead
{
    public class MarkAllNotificationsAsReadCommandHandler : IRequestHandler<MarkAllNotificationsAsReadCommand, BaseResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public MarkAllNotificationsAsReadCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse> Handle(MarkAllNotificationsAsReadCommand request, CancellationToken cancellationToken)
        {
            await _unitOfWork.Notifications.MarkAllAsReadAsync(request.UserIdPacket, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new BaseResponse
            {
                Success = true,
                Message = "All notifications marked as read."
            };
        }
    }
}
