
using Microsoft.EntityFrameworkCore;
using Database;
using LinkStatusDb.Models;
using LinkStatusDb.Repositories;


namespace Link_statuses
{
    public class Program
    {
        static readonly HttpClient client = new();
        public static async Task Main()
        {
            var options = new DbContextOptionsBuilder<LinkStatusesDbContext>()
                .UseInMemoryDatabase(databaseName: "LinkStatusesDb")
                .Options;

            var dbContext = new LinkStatusesDbContext(options);
            dbContext.Database.EnsureCreated();
            var repository = new DataBaseRepository(dbContext);

            var cts = new CancellationTokenSource();

            Console.WriteLine("TelegramBot token: ");
            string BotToken = Console.ReadLine();

            var telegramBot = new Host(BotToken);
            var handlers = new Handlers(repository);
            telegramBot.OnMessage = handlers.MessageHandle;
            telegramBot.Start();

            _ = Task.Run(() =>
            {
                Console.WriteLine("Press B to cancel");
                var key = Console.ReadKey();

                if (key.Key is ConsoleKey.C)
                {
                    cts.Cancel();
                    Console.WriteLine("Timed Broadcast has stopped");
                }
            });

            await TimedBroadCastAsync(telegramBot, repository, cts);
            Console.ReadLine();
        }

        public static async Task<int> GetLinkStatusAsync(string link) // return status code or 0 if link is invalid or request failed
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(link);
                if (response.IsSuccessStatusCode)
                {
                    return ((int)response.StatusCode);
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        static async Task TimedBroadCastAsync(Host telegramBot, DataBaseRepository repo, CancellationTokenSource source)
        {
            while (!source.Token.IsCancellationRequested)
            {
                await Task.Delay(30000, source.Token); // проверка статусов ссылок каждые 30, при недоступности будет уведомление от бота
                Console.WriteLine("Working");
                var subscribers = await repo.GetSubscribersLinksAsync();

                if (subscribers.Count == 0) continue;

                var unavailableLinks = new Dictionary<long, List<string>>();

                foreach (var subscriber in subscribers)
                {
                    foreach (var info in subscriber.Link)
                    {
                        int response = await GetLinkStatusAsync(info.Url);
                        if (response == 0)
                        {
                            var linkStatus = new LinkStatusEntity
                            {
                                LinkId = info.Id,
                                CheckedAt = DateTime.UtcNow,
                                Status = "Unavailable"
                            };

                            await repo.AddLinkStatusAsync(linkStatus);

                            if (!unavailableLinks.ContainsKey(subscriber.Id))
                            {
                                unavailableLinks[subscriber.Id] = [];
                            }
                            unavailableLinks[subscriber.Id].Add(info.Url);
                        }
                        else
                        {
                            var linkStatus = new LinkStatusEntity
                            {
                                LinkId = info.Id,
                                CheckedAt = DateTime.UtcNow,
                                Status = "Available"
                            };

                            await repo.AddLinkStatusAsync(linkStatus);
                        }
                    }
                }

                await Handlers.SendBroadCastAsync(telegramBot.bot, unavailableLinks);
            }

            source.Dispose();

        }
    }
}

