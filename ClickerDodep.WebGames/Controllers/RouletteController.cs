using Microsoft.AspNetCore.Mvc;

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

        [HttpGet("balance/{userId}")]
        public async Task<IActionResult> GetBalance(long userId)
        {
            var balance = await _repo.GetBalance(userId);
            return Ok(new { Balance = balance });
        }

        public class PlaceBetRequest
        {
            public long UserId { get; set; }
            public string? Color { get; set; } // "red", "black", "green"
            public int Amount { get; set; }
        }

        [HttpPost("placebet")]
        public async Task<IActionResult> PlaceBet([FromBody] PlaceBetRequest request)
        {
            var balance = await _repo.GetBalance(request.UserId);
            if (request.Amount <= 0 || request.Amount > balance)
                return BadRequest(new { error = "Недостаточно средств или некорректная ставка" });

            var success = await _repo.TryWithdraw(request.UserId, request.Amount);
            if (!success)
                return BadRequest(new { error = "Не удалось списать средства" });

            var rnd = new Random();
            int number = rnd.Next(0, 37);
            string actualColor = number == 0 ? "green" : (number % 2 == 0 ? "black" : "red");
            bool win = actualColor == request.Color;

            int multiplier = actualColor == "green" ? 14 : 2;
            int prize = win ? request.Amount * multiplier : 0;

            if (win)
            {
                await _repo.AddBalance(request.UserId, prize);
            }

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
