using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using ClickerDodepBot;

var connectionString = "Host=localhost;Username=postgres;Password=Deagtom;Database=postgres";
var repo = new UserRepository(connectionString);

using var cts = new CancellationTokenSource();
var bot = new TelegramBotClient("7884397200:AAGk5KdZTdpdynX4EiR-tLuELgFgYbWVvzs", cancellationToken: cts.Token);

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

Console.WriteLine("ClickerDodepBot is running... Press Enter to exit");
Console.ReadLine();

cts.Cancel();

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
    switch (msg.Text)
    {
        case "/start":
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
            string webAppUrl = $"https://deagtom.github.io/roulette-html?userId={msg.From!.Id}";
            await bot.SendMessage(
                msg.Chat,
                "🎰 Нажмите кнопку ниже, чтобы сыграть в рулетку:",
                replyMarkup: new InlineKeyboardMarkup(
                    InlineKeyboardButton.WithWebApp("🎮 Играть в рулетку", new WebAppInfo(webAppUrl))
                )
            );
            break;
    }
}

async Task OnUpdate(Update update)
{
    if (update is { CallbackQuery: { } query })
    {
        var chatId = query.Message!.Chat.Id;
        var userId = query.From.Id;

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