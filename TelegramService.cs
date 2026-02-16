using Telegram.Bot;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotifyApp.Models;
using Telegram.Bot.Types;



public class TelegramService
{
    private readonly TelegramSettings _settings;
    private readonly ILogger<TelegramService> _logger;
    private readonly TelegramBotClient _client;
    private const long MaxTelegramFileSize = 50 * 1024 * 1024; // 50MB
 

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

    public async Task SendFilesAsync(IEnumerable<string> filePaths)
    {
        foreach (var path in filePaths)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Файл не найден", path);

            var fileInfo = new FileInfo(path);

            if (fileInfo.Length > MaxTelegramFileSize)
                throw new InvalidOperationException(
                    $"Файл {fileInfo.Name} превышает лимит 50MB");

            await using var stream = File.OpenRead(path);

            await _client.SendDocument(
                chatId: _settings.ChatId,
                document: InputFile.FromStream(stream, fileInfo.Name)
            );

            _logger.LogInformation("Файл {FileName} отправлен в Telegram", fileInfo.Name);
        }
    }
}
