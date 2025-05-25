using Microsoft.AspNetCore.Mvc;
using ClickerDodep.Data;

namespace ClickerDodep.WebGames.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RouletteController : ControllerBase
    {
        private readonly UserRepository _repo;

        public RouletteController(UserRepository repo)
        {
            _repo = repo;
        }

        public class PlaceBetRequest
        {
            public long UserId { get; set; }
            public string? Color { get; set; }
            public int Amount { get; set; }
        }

        // GET api/roulette/balance/{userId}
        [HttpGet("balance/{userId}")]
        public async Task<IActionResult> GetBalance(long userId)
        {
            var balance = await _repo.GetBalance(userId);
            return Ok(new { Balance = balance });
        }

        // POST api/roulette/placebet
        [HttpPost("placebet")]
        public async Task<IActionResult> PlaceBet([FromBody] PlaceBetRequest request)
        {
            // Получаем баланс именно указанного пользователя
            var balance = await _repo.GetBalance(request.UserId);
            if (request.Amount <= 0 || request.Amount > balance)
                return BadRequest(new { error = "Недостаточно средств или некорректная ставка" });

            // Списываем ставку
            var success = await _repo.TryWithdraw(request.UserId, request.Amount);
            if (!success)
                return BadRequest(new { error = "Не удалось списать средства" });

            // Генерируем результат
            var rnd = new Random();
            int number = rnd.Next(0, 37);
            string actualColor = number == 0 ? "green" : (number % 2 == 0 ? "black" : "red");
            bool win = actualColor == request.Color;
            int multiplier = actualColor == "green" ? 14 : 2;
            int prize = win ? request.Amount * multiplier : 0;

            // Начисляем выигрыш
            if (win)
            {
                await _repo.AddBalance(request.UserId, prize);
            }

            // Новый баланс
            var newBalance = await _repo.GetBalance(request.UserId);

            return Ok(new
            {
                Win = win,
                ActualColor = actualColor,
                Prize = prize,
                NewBalance = newBalance
            });
        }
    }
}