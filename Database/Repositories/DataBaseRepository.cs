using LinkStatusDb.DTOs;
using LinkStatusDb.Models;
using Microsoft.EntityFrameworkCore;

namespace LinkStatusDb.Repositories
{
    public class DataBaseRepository : IDataBaseRepository
    {
        private readonly LinkStatusesDbContext _dbContext;
        public DataBaseRepository(LinkStatusesDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddUserAsync(long userId)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                user = new UserEntity { Id = userId };
                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync();
                Console.WriteLine("User added to DB");
            }
            else Console.WriteLine("User already in DB");

        }

        public async Task AddLinkAsync(long userId, string url)
        {
            var linkToAdd = new LinkEntity { Url = url, UserId = userId };
            _dbContext.Links.Add(linkToAdd);
            await _dbContext.SaveChangesAsync();
            Console.WriteLine("Link added to DB");
        }

        public async Task AddLinkStatusAsync(LinkStatusEntity LinkStatus)
        {
            _dbContext.LinkStatuses.Add(LinkStatus);
            await _dbContext.SaveChangesAsync();
            Console.WriteLine("Link status added");
        }

        public async Task AddSubscriberAsync(long userId)
        {
            var userToSubscribe = await _dbContext.Users
                .Include(u => u.Link)
                .FirstAsync(u => u.Id == userId);

            userToSubscribe.Subscription = true;
            await _dbContext.SaveChangesAsync();
            Console.WriteLine("User subscribed to broadcast");
        }

        public async Task DeleteSubscriberAsync(long userId)
        {
            var subscriberToDelete = await _dbContext.Users
                .Include(u => u.Link)
                .FirstAsync(u => u.Id == userId);

            subscriberToDelete.Subscription = false;
            await _dbContext.SaveChangesAsync();
            Console.WriteLine("User unsubscribed from broadcast");
        }

        public async Task DeleteLinkAsync(long userId, string url)
        {
            var linkToDelete = await _dbContext.Links
                .FirstOrDefaultAsync(l => l.UserId == userId && l.Url == url);

            _dbContext.Links.Remove(linkToDelete!);
            await _dbContext.SaveChangesAsync();
            Console.WriteLine("Link deleted");
        }

        public async Task DeleteLogByIdAsync(int logId)
        {
            var logToDelete = await _dbContext.LinkStatuses
                .FirstAsync(ls => ls.Id == logId);

            _dbContext.LinkStatuses.Remove(logToDelete);
            await _dbContext.SaveChangesAsync();
            Console.WriteLine("Log removed");
        }

        public async Task<UserEntity> GetUserByIdAsync(long userId)
        {
            return await _dbContext.Users
                .AsNoTracking()
                .FirstAsync(u => u.Id == userId);
        }

        public async Task<List<LinkEntity>> GetLinksByUserIdAsync(long userId)
        {
            var userLinks = await _dbContext.Links
                .AsNoTracking()
                .Where(l => l.UserId == userId)
                .ToListAsync();

            return userLinks;
        }

        public async Task<List<LinkLogDto>> GetLogsByUserIdAsync(long userId)
        {
            var userLogs = await _dbContext.Links
                .AsNoTracking()
                .Where(l => l.UserId == userId)
                .Where(l => l.LinkStatus.Any())
                .Include(l => l.LinkStatus)
                .Select(l => new LinkLogDto
                {
                    LinkUrl = l.Url,

                    Status = l.LinkStatus
                    .OrderByDescending(ls => ls.CheckedAt)
                    .Select(ls => ls.Status)
                    .ToList(),

                    LogId = l.LinkStatus
                    .OrderByDescending(ls => ls.CheckedAt)
                    .Select(ls => ls.Id)
                    .ToList(),

                    LogTime = l.LinkStatus
                    .OrderByDescending(ls => ls.CheckedAt)
                    .Select(ls => ls.CheckedAt)
                    .ToList()

                })
                .ToListAsync();

            return userLogs;
        }

        public async Task<List<UserEntity>> GetSubscribersLinksAsync()
        {
            return await _dbContext.Users
                .AsNoTracking()
                .Where(u => u.Subscription)
                .Include(u => u.Link)
                .ToListAsync();
        }


    }
}