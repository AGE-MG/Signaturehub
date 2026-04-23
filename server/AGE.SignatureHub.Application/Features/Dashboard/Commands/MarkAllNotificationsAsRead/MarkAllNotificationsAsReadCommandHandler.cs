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
            var response = new BaseResponse();

            try
            {
                await _unitOfWork.Notifications.MarkAllAsReadAsync(request.UserIdPacket, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                response.Success = true;
                response.Message = "All notifications marked as read.";
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "An error occurred while marking all notifications as read.";
                response.Errors = new List<string> { ex.Message };
                return response;
            }
        }
    }
}
