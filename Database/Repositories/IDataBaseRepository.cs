using LinkStatusDb.DTOs;
using LinkStatusDb.Models;

namespace LinkStatusDb.Repositories
{
    public interface IDataBaseRepository
    {
        Task AddUserAsync(long userId);
        Task AddLinkAsync(long userId, string url);
        Task AddLinkStatusAsync(LinkStatusEntity LinkStatus);
        Task AddSubscriberAsync(long userId);
        Task DeleteSubscriberAsync(long userId);
        Task DeleteLinkAsync(long userId, string url);
        Task DeleteLogByIdAsync(int logId);
        Task<UserEntity> GetUserByIdAsync(long userId);
        Task<List<LinkEntity>> GetLinksByUserIdAsync(long userId);
        Task<List<LinkLogDto>> GetLogsByUserIdAsync(long userId);
        Task<List<UserEntity>> GetSubscribersLinksAsync();

    }
}
