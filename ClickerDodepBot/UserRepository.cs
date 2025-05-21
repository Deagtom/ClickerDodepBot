using Npgsql;

namespace ClickerDodepBot
{
    internal class UserRepository
    {
        private readonly string _connectionString;

        public UserRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task CreateUserIfNotExists(long userId)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand(@"
            INSERT INTO users (id) 
            VALUES (@id) 
            ON CONFLICT (id) DO NOTHING", conn);

            cmd.Parameters.AddWithValue("id", userId);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<int> IncrementBalance(long userId)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand(@"
            UPDATE users SET balance = balance + 1 
            WHERE id = @id 
            RETURNING balance", conn);

            cmd.Parameters.AddWithValue("id", userId);
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<int> GetBalance(long userId)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand("SELECT balance FROM users WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("id", userId);

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task SetRouletteColor(long userId, string color)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            var cmd = new NpgsqlCommand("UPDATE users SET roulette_color = @color, awaiting_roulette_amount = true WHERE id = @id ", conn);
            cmd.Parameters.AddWithValue("id", userId);
            cmd.Parameters.AddWithValue("color", color);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<string?> GetAwaitingRouletteColor(long userId)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            var cmd = new NpgsqlCommand("SELECT roulette_color FROM users WHERE id = @id AND awaiting_roulette_amount = true", conn);
            cmd.Parameters.AddWithValue("id", userId);
            var result = await cmd.ExecuteScalarAsync();
            return result as string;
        }

        public async Task ClearRouletteState(long userId)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            var cmd = new NpgsqlCommand("UPDATE users SET roulette_color = NULL, awaiting_roulette_amount = false WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("id", userId);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<bool> TryWithdraw(long userId, int amount)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand("UPDATE users SET balance = balance - @amount WHERE id = @id AND balance >= @amount", conn);

            cmd.Parameters.AddWithValue("id", userId);
            cmd.Parameters.AddWithValue("amount", amount);

            var rowsAffected = await cmd.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task AddBalance(long userId, int amount)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand("UPDATE users SET balance = balance + @amount WHERE id = @id", conn);

            cmd.Parameters.AddWithValue("id", userId);
            cmd.Parameters.AddWithValue("amount", amount);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}