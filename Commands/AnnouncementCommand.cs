using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramVPNBot.Interfaces;

namespace TelegramVPNBot.Commands;

public class AnnouncementCommand(IAuthorizationService authorizationService) : ICommand
{
    public async Task ExecuteAsync(Update update, ITelegramBotClient botClient)
    {
        if (update.Message?.Text is not { } messageText)
            return;

        var userData = update.Message.From;
        if (userData == null)
            return;

        var user = await authorizationService.GetAuthorizedUserAsync(userData);

        if (!user.IsAdmin)
            return;

        var commandText = messageText.Replace("/announcement ", "").Trim();
        var announcementParts = commandText.Split("||");

        if (announcementParts.Length < 2)
        {
            await botClient.SendMessage(chatId: user.TelegramId,
                text: "Неверный формат. Используйте:\n`URL||Текст`\nили\n`URL||Текст||Кнопки`",
                parseMode: ParseMode.Markdown
            );
            return;
        }

        var photoUrl = announcementParts[0].Trim();
        var caption = announcementParts[1].Trim();

        InlineKeyboardMarkup? replyMarkup = null;
        if (announcementParts.Length > 2)
        {
            replyMarkup = ParseInlineMarkup(announcementParts[2].Trim());
        }

        _ = Task.Run(async () =>
        {
            try
            {
                var users = await authorizationService.GetUsersAsync();
                var successCount = 0;

                foreach (var botUser in users)
                {
                    try
                    {
                        await botClient.SendPhoto(
                            chatId: botUser.TelegramId,
                            photo: new InputFileUrl(photoUrl),
                            caption: caption,
                            parseMode: ParseMode.MarkdownV2,
                            replyMarkup: replyMarkup);
                        successCount++;
                    }
                    catch (ApiRequestException ex) when (ex.ErrorCode == 403)
                    {
                        Console.WriteLine($"User {botUser.TelegramId} blocked the bot.");
                    }
                    catch (ApiRequestException ex) when (ex.ErrorCode == 429)
                    {
                        var retryAfter = ex.Parameters?.RetryAfter ?? 5;
                        Console.WriteLine($"Rate Limit! Waiting {retryAfter} sec.");
                        await Task.Delay(retryAfter * 1000 + 1000);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error sending to {botUser.TelegramId}. Error: {ex.Message}");
                    }

                    await Task.Delay(100);
                }

                await botClient.SendMessage(user.TelegramId, $"Рассылка завершена. Успешно отправлено {successCount} из {users.Count} сообщений.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Критическая ошибка во время рассылки: {ex.Message}");
                await botClient.SendMessage(user.TelegramId, $"Во время рассылки произошла критическая ошибка: {ex.Message}");
            }
        });
    }

    private static InlineKeyboardMarkup? ParseInlineMarkup(string buttonsString)
    {
        if (string.IsNullOrWhiteSpace(buttonsString))
            return null;

        var rows = new List<List<InlineKeyboardButton>>();

        var rowStrings = buttonsString.Split('|');

        foreach (var rowString in rowStrings)
        {
            var buttonRow = new List<InlineKeyboardButton>();
            var buttonStrings = rowString.Split(';');

            foreach (var buttonString in buttonStrings)
            {
                var buttonParts = buttonString.Trim().Split(':', 2);
                if (buttonParts.Length != 2)
                    continue;

                var text = buttonParts[0].Trim();
                var data = buttonParts[1].Trim();

                if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(data))
                    continue;

                if (data.StartsWith("http://") || data.StartsWith("https://"))
                {
                    buttonRow.Add(InlineKeyboardButton.WithUrl(text, data));
                }
                else
                {
                    buttonRow.Add(InlineKeyboardButton.WithCallbackData(text, data));
                }
            }

            if (buttonRow.Any())
            {
                rows.Add(buttonRow);
            }
        }

        return rows.Any() ? new InlineKeyboardMarkup(rows) : null;
    }
}
