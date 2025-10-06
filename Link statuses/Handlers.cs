
using Database;
using LinkStatusDb.Models;
using LinkStatusDb.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Link_statuses
{
    public class Handlers(DataBaseRepository repository)
    {
        private DataBaseRepository repo = repository;

        private static bool IsValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out Uri? uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
        public async Task MessageHandle(ITelegramBotClient bot, Update update) // handle all bot commands
        {
            if (update.Message == null || string.IsNullOrWhiteSpace(update.Message.Text))
            {
                Console.WriteLine("Something went wrong");
                return;
            }

            string command = !string.IsNullOrWhiteSpace(update.Message?.Text)
                             ? update.Message.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()
                             ?? ""
                             : "";

            long userId = update.Message.Chat.Id;
            string userMessage = string.Join(" ", update.Message.Text
                                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                    .Skip(1));

            if (command == "/start")
            {
                await repo.AddUserAsync(userId);
                await bot.SendMessage(userId,
                    "This bot helps you track the status of your favorite links (websites).\n" +
                    "Click /manual to learn more"
                 );
            }
            else if (command == "/manual")
            {
                await bot.SendMessage(userId,
                    "Commands:\n" +
                    "/add <link> - Add a link to tracking\n" +
                    "/delete <link> - Remove a link from tracking\n" +
                    "/show - Show all tracked links\n" +
                    "/logs - Show logs of link status checks\n" +
                    "/deleteLog <index> - Delete a specific log entry by its index\n" +
                    "/subscribe - Subscribe to periodic status updates about your links\n" +
                    "/unsubscribe - Unsubscribe from periodic status updates about your links\n\n" +
                    "The bot will periodically check your links and notify you if any become unreachable."
                );
            }
            else if (command == "/show")
            {
                var userLinks = await repo.GetLinksByUserIdAsync(userId);
                var logsToAdd = new List<LinkStatusEntity>();

                StringBuilder message = new("Currently tracked links:\n");
                if (userLinks.Count == 0)
                {
                    await bot.SendMessage(userId, "No links are currently being tracked.");
                    return;
                }
                foreach (var link in userLinks)
                {
                    message.AppendLine($"{link.Url}\n");
                }
                await bot.SendMessage(userId, message.ToString());

            }
            else if (command == "/logs")
            {
                var userLogs = await repo.GetLogsByUserIdAsync(userId);

                if (userLogs.Count == 0)
                {
                    await bot.SendMessage(userId, "No logs yet.");
                    return;
                }
                StringBuilder message = new("Logs:\n");


                foreach (var log in userLogs)
                {
                    for(int i = 0; i < log.LogId.Count; i++)
                    {
                        message.AppendLine($"{log.LogTime[i]}: Link {log.LinkUrl} returned status {log.Status[i]} - Log index {log.LogId[i]}\n");
                    }                   
                }               

                await bot.SendMessage(userId, message.ToString());
            }
            else if (command == "/add")
            {
                if (string.IsNullOrWhiteSpace(userMessage))
                {
                    await bot.SendMessage(userId, "To add link you need to write like this '/add example.com'");
                    return;
                }
                if (!IsValidUrl(userMessage))
                {
                    await bot.SendMessage(userId, "Invalid link, try again:");
                    return;
                }

                var userLinks = await repo.GetLinksByUserIdAsync(userId);
                var linkToAdd = userLinks.FirstOrDefault(l => l.Url == userMessage);

                if (linkToAdd != null)
                {
                    await bot.SendMessage(userId, "This link is already being tracked.");
                    return;
                }

                await repo.AddLinkAsync(userId, userMessage);
                await bot.SendMessage(userId, "Link added to tracking.");

            }
            else if (command == "/delete")
            {
                if (string.IsNullOrWhiteSpace(userMessage))
                {
                    await bot.SendMessage(userId, "To delete link you need to write like this '/delete example.com'");
                    return;
                }

                if (!IsValidUrl(userMessage))
                {
                    await bot.SendMessage(userId, "Invalid link, try again:");
                    return;
                }

                var userLinks = await repo.GetLinksByUserIdAsync(userId);

                if (userLinks.Count == 0)
                {
                    await bot.SendMessage(userId, "No links are currently being tracked.");
                    return;
                }

                var linkToRemove = userLinks.FirstOrDefault(l => l.Url == userMessage);

                if (linkToRemove == null)
                {
                    await bot.SendMessage(userId, "This link is not being tracked.");
                    return;
                }

                await repo.DeleteLinkAsync(userId, linkToRemove.Url);
                await bot.SendMessage(userId, "Link removed from tracking.");
            }
            else if (command == "/deleteLog")
            {
                var userLogs = await repo.GetLogsByUserIdAsync(userId);

                if (userLogs.Count == 0)
                {
                    await bot.SendMessage(userId, "No logs yet.");
                    return;
                }

                if (int.TryParse(userMessage, out int logIndex))
                {
                    var logToRemove = userLogs
                        .FirstOrDefault(ul => ul.LogId.Contains(logIndex));

                    if (logToRemove == null)
                    {
                        await bot.SendMessage(userId, "There is no log with this Id");
                        return;
                    }

                    await repo.DeleteLogByIdAsync(logIndex);
                    await bot.SendMessage(userId, "Log entry removed.");
                }
                else
                {
                    await bot.SendMessage(userId, "Please provide a valid and correct number for log index to delete.");
                }
            }
            else if (command == "/subscribe")
            {
                var user = await repo.GetUserByIdAsync(userId);
                if (user.Subscription == true)
                {
                    await bot.SendMessage(userId, "You are already subscribed to broadcast messages.");
                    return;
                }

                await repo.AddSubscriberAsync(userId);
                await bot.SendMessage(userId, "You have subscribed to broadcast messages.");
            }
            else if (command == "/unsubscribe")
            {
                var user = await repo.GetUserByIdAsync(userId);
                if (user.Subscription == false)
                {
                    await bot.SendMessage(userId, "You are not subscribed to broadcast messages.");
                    return;
                }

                await repo.DeleteSubscriberAsync(userId);
                await bot.SendMessage(userId, "You have unsubscribed from broadcast messages.");
            }
            else
            {
                await bot.SendMessage(userId, "Unknown command. Please use /manual to see available commands.");
            }
        }
        static public async Task SendBroadCastAsync(ITelegramBotClient bot, Dictionary<long, List<string>> unavailableLinks) // send message to users who want to receive broadcast if any of their links is unreachable
        {
            if (unavailableLinks.Count == 0) return;

            foreach (var subscriber in unavailableLinks)
            {
                StringBuilder message = new("Broadcast - The following links are unreachable:\n");
                foreach (var link in subscriber.Value)
                {
                    message.AppendLine($"Link {link} is currently Unavailable ❌");
                }

                await bot.SendMessage(subscriber.Key, message.ToString());
            }
        }

    }
}