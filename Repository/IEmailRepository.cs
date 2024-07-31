using Zoom.Dtos;

namespace Zoom.Repository
{
    public interface IEmailRepository
    {
        void SendEmail(Message message);
    }
}
