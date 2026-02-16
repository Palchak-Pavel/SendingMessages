using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NotifyApp.Models;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.Configure<EmailSettings>(
            context.Configuration.GetSection(nameof(EmailSettings)));

        services.Configure<TelegramSettings>(
            context.Configuration.GetSection(nameof(TelegramSettings)));

        services.AddTransient<EmailService>();
        services.AddTransient<TelegramService>();
    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
    })
    .Build();

using var scope = host.Services.CreateScope();

var emailSvc = scope.ServiceProvider.GetRequiredService<EmailService>();
var telegramSvc = scope.ServiceProvider.GetRequiredService<TelegramService>();

Console.WriteLine("Введите команду (email/tg/exit):");


string? cmd;
while ((cmd = Console.ReadLine()?.ToLowerInvariant()) != "exit")
{
    try
    {
        switch (cmd)
        {

            case "email":
                Console.Write("To: ");
                var to = Console.ReadLine();

                Console.Write("Subject: ");
                var subject = Console.ReadLine();

                Console.Write("Body: ");
                var body = Console.ReadLine();

                Console.WriteLine("Введите пути к файлам через запятую (или пусто):");
                var emailFilesInput = Console.ReadLine();

                var emailFiles = string.IsNullOrWhiteSpace(emailFilesInput)
                    ? new List<string>()
                    : emailFilesInput.Split(',')
                                     .Select(f => f.Trim())
                                     .ToList();

                if (emailFiles.Any())
                    await emailSvc.SendEmailWithAttachmentsAsync(to!, subject!, body!, emailFiles);
                else
                    await emailSvc.SendEmailAsync(to!, subject!, body!);
                break;


            case "tg":
                Console.Write("Сообщение: ");
                var message = Console.ReadLine();

                Console.WriteLine("Введите пути к файлам через запятую (или пусто):");
                var tgFilesInput = Console.ReadLine();

                var tgFiles = string.IsNullOrWhiteSpace(tgFilesInput)
                    ? new List<string>()
                    : tgFilesInput.Split(',')
                                  .Select(f => f.Trim())
                                  .ToList();

                await telegramSvc.SendMessageAsync(message!);

                if (tgFiles.Any())
                    await telegramSvc.SendFilesAsync(tgFiles);

                break;
            default:
                Console.WriteLine("Неизвестная команда");
                break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка: {ex.Message}");
    }

    Console.WriteLine("\nВведите команду (email/tg/exit):");
}
