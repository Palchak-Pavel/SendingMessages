using Telegram.Bot;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotifyApp.Models;

public class TelegramService
{
    private readonly TelegramSettings _settings;
    private readonly ILogger<TelegramService> _logger;
    private readonly TelegramBotClient _client;

    public TelegramService(IOptions<TelegramSettings> settings, ILogger<TelegramService> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        _client = new TelegramBotClient(_settings.BotToken);
    }

    public async Task SendMessageAsync(string message)
    {
        try
        {
           await _client.SendMessage(
                chatId: _settings.ChatId,
                text: message
            );
            _logger.LogInformation("Telegram message sent");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send Telegram message");
            throw;
        }
    }
}
