using AGE.SignatureHub.Application.Contracts.Persistence;
using AGE.SignatureHub.Application.DTOs.Common;
using AGE.SignatureHub.Application.Exceptions;
using MediatR;

namespace AGE.SignatureHub.Application.Features.Dashboard.Commands.MarkNotificationAsRead
{
    public class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand, BaseResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public MarkNotificationAsReadCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
        {
            var response = new BaseResponse();

            try
            {
                var notification = await _unitOfWork.Notifications.GetByIdAsync(request.NotificationId, cancellationToken);
                if (notification == null)
                    throw new NotFoundException(nameof(notification), request.NotificationId);

                notification.MarkAsRead();
                await _unitOfWork.Notifications.UpdateAsync(notification, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                response.Success = true;
                response.Message = "Notification marked as read.";
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "An error occurred while marking the notification as read.";
                response.Errors = new List<string> { ex.Message };
                return response;
            }
        }
    }
}
