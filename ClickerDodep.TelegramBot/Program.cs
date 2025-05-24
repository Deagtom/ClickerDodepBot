using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Serilog;


Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .MinimumLevel.Information()
    .CreateLogger();

TelegramBotClient bot;

var connectionString = "Host=localhost;Username=postgres;Password=Deagtom;Database=postgres";
var repo = new UserRepository(connectionString);

try
{
    Log.Information("ClickerDodepBot запускается...");

    using var cts = new CancellationTokenSource();
    bot = new TelegramBotClient("7884397200:AAGk5KdZTdpdynX4EiR-tLuELgFgYbWVvzs", cancellationToken: cts.Token);

    var receiverOptions = new ReceiverOptions
    {
        AllowedUpdates = Array.Empty<UpdateType>()
    };

    bot.StartReceiving(
        updateHandler: HandleUpdateAsync,
        errorHandler: HandlePollingErrorAsync,
        receiverOptions: receiverOptions,
        cancellationToken: cts.Token
    );

    Log.Information("ClickerDodepBot запущен");
    Console.WriteLine("Нажмите Enter чтобы закрыть");
    Console.ReadLine();

    cts.Cancel();
}
catch (Exception ex)
{
    Log.Error(ex, "Произошла ошибка в работе бота");
}
finally
{
    Log.CloseAndFlush();
}

async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken token)
{
    if (update.Message != null)
        await OnMessage(update.Message, update.Type);

    if (update.CallbackQuery != null)
        await OnUpdate(update);
}

Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken token)
{
    Console.WriteLine($"Ошибка при получении обновлений: {exception.Message}");
    return Task.CompletedTask;
}

async Task OnMessage(Message msg, UpdateType type)
{
    Log.Information("Получено сообщение от пользователя {UserId}: {Text}", msg.From!.Id, msg.Text);

    switch (msg.Text)
    {
        case "/start":
            Log.Information("Пользователь {UserId} начал работу с ботом", msg.From.Id);
            await repo.CreateUserIfNotExists(msg.From!.Id, msg.From.Username);

            await bot.SendMessage(
                msg.Chat, "Выберите игру 👇",
                replyMarkup: new KeyboardButton[]
                {
                "/clicker",
                "/roulette"
                }
            );
            break;

        case "/clicker":
            Log.Information("Пользователь {UserId} выбрал кликер", msg.From.Id);

            await bot.SendMessage(
                msg.Chat, "Выберите действие 👇",
                replyMarkup: new InlineKeyboardMarkup(
                [
                    [
                        InlineKeyboardButton.WithCallbackData("Получить монетку 💰", "click"),
                        InlineKeyboardButton.WithCallbackData("Баланс 📊", "balance")
                    ]
                ])
            );
            break;

        case "/roulette":
            Log.Information("Пользователь {UserId} выбрал рулетку", msg.From.Id);

            string webAppUrl = $"https://deagtom.github.io/roulette-html?userId={msg.From!.Id}";
            await bot.SendMessage(
                msg.Chat,
                "🎰 Нажмите кнопку ниже, чтобы сыграть в рулетку:",
                replyMarkup: new InlineKeyboardMarkup(
                    InlineKeyboardButton.WithWebApp("🎮 Играть в рулетку", new WebAppInfo(webAppUrl))
                )
            );
            break;

        default:
            Log.Warning("Пользователь {UserId} отправил неизвестную команду: {Text}", msg.From.Id, msg.Text);
            break;
    }
}

async Task OnUpdate(Update update)
{
    if (update is { CallbackQuery: { } query })
    {
        var chatId = query.Message!.Chat.Id;
        var userId = query.From.Id;

        Log.Information("Пользователь {UserId} вызвал callback: {CallbackData}", userId, query.Data);

        switch (query.Data)
        {
            case "click":
                var newBalance = await repo.IncrementBalance(userId);
                await bot.AnswerCallbackQuery(query.Id, "+1 💰");
                break;

            case "balance":
                var balance = await repo.GetBalance(userId);
                await bot.AnswerCallbackQuery(query.Id, "📊 Баланс");
                await bot.SendMessage(chatId, $"Ваш баланс: {balance} монет(ы)");
                break;
        }
    }
}