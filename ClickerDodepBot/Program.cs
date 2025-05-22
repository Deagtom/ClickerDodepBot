using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using ClickerDodepBot;

using var cts = new CancellationTokenSource();
var bot = new TelegramBotClient("7884397200:AAGk5KdZTdpdynX4EiR-tLuELgFgYbWVvzs", cancellationToken: cts.Token);

var connectionString = "Host=localhost;Username=postgres;Password=Deagtom;Database=postgres";
var repo = new UserRepository(connectionString);

bot.OnError += OnError;
bot.OnMessage += OnMessage;
bot.OnUpdate += OnUpdate;

string ColorName(string color) => color switch
{
    "red" => "🔴 Красное",
    "black" => "⚫️ Чёрное",
    "green" => "🟢 Зелёное",
    _ => "Неизвестно"
};


Console.WriteLine($"ClickerDodepBot is running... Press Enter to terminate");
Console.ReadLine();
cts.Cancel();

async Task OnError(Exception exception, HandleErrorSource source)
{
    Console.WriteLine(exception);
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
            await bot.SendMessage(
                msg.Chat, "Выберите действие 👇",
                replyMarkup: new InlineKeyboardMarkup(
                [
                    [
                        InlineKeyboardButton.WithCallbackData("Красное 🔴", "red"),
                        InlineKeyboardButton.WithCallbackData("Чёрное ⚫️", "black"),
                        InlineKeyboardButton.WithCallbackData("Зелёное 🟢", "green")
                    ]
                ])
            );
            break;
    }

    var userId = msg.From!.Id;
    var selectedColor = await repo.GetAwaitingRouletteColor(userId);

    if (selectedColor != null)
    {
        if (!int.TryParse(msg.Text, out var amount) || amount <= 0)
        {
            await bot.SendMessage(msg.Chat.Id, "❗ Введите корректную сумму");
            return;
        }

        var success = await repo.TryWithdraw(userId, amount);
        if (!success)
        {
            await bot.SendMessage(msg.Chat.Id, "❌ Недостаточно монет!");
            await repo.ClearRouletteState(userId);
            return;
        }

        var rnd = new Random().Next(0, 37);
        var actualColor = rnd == 0 ? "green" : rnd % 2 == 0 ? "black" : "red";
        bool win = actualColor == selectedColor;

        int multiplier = selectedColor == "green" ? 14 : 2;
        int prize = win ? amount * multiplier : 0;

        if (win)
        {
            await repo.AddBalance(userId, prize);
            await bot.SendMessage(msg.Chat.Id, $"🎉 Выпало: {ColorName(actualColor)} ({rnd})\nВы выиграли {prize} монет!");
        }
        else
        {
            await bot.SendMessage(msg.Chat.Id, $"😢 Выпало: {ColorName(actualColor)} ({rnd})\nВы проиграли {amount} монет.");
        }

        await repo.ClearRouletteState(userId);
        return;
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

            case "red":
            case "black":
            case "green":
                await repo.SetRouletteColor(userId, query.Data);
                await bot.AnswerCallbackQuery(query.Id);
                await bot.SendMessage(chatId, $"Введите сумму ставки на {ColorName(query.Data)}:");
                break;
        }
    }
}