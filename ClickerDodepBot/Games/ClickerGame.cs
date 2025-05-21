using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace ClickerDodepBot.Games
{
    internal class ClickerGame
    {
        /*private readonly UserRepository _repo;
        private readonly TelegramBotClient _bot;

        public ClickerGame(UserRepository repo, TelegramBotClient bot)
        {
            _repo = repo;
            _bot = bot;
        }

        public async Task OnMessage(Message msg, UpdateType type)
        {
            if (msg.Text == "/clicker")
            {
                await _bot.SendMessage(
                    msg.Chat.Id, "Выберите действие 👇",
                    replyMarkup: new InlineKeyboardMarkup(
                    [
                        [
                            InlineKeyboardButton.WithCallbackData("Получить монетку 💰", "click"),
                            InlineKeyboardButton.WithCallbackData("Балансе 📊", "balance")
                        ]
                    ])
                );
            }
        }

        public async Task OnUpdate(Update update)
        {
            if (update is { CallbackQuery: { } query })
            {
                var chatId = query.Message!.Chat.Id;
                var userId = query.From.Id;

                switch (query.Data)
                {
                    case "clicker":
                        var newBalance = await _repo.IncrementBalance(userId);
                        await _bot.AnswerCallbackQuery(query.Id, "+1 💰");
                        break;

                    case "balance":
                        var balance = await _repo.GetBalance(userId);
                        await _bot.AnswerCallbackQuery(query.Id, "📊 Баланс");
                        await _bot.SendMessage(chatId, $"Ваш баланс: {balance} монет(ы)");
                        break;
                }
            }
        }*/
    }
}
