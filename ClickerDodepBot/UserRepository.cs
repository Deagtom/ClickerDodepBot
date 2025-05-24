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

        public async Task CreateUserIfNotExists(long userId, string? username)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand(@"
                                        INSERT INTO users (id, username) 
                                        VALUES (@id, @username) 
                                        ON CONFLICT (id) 
                                        DO UPDATE SET username = EXCLUDED.username", conn);

            cmd.Parameters.AddWithValue("id", userId);
            cmd.Parameters.AddWithValue("username", (object?)username ?? DBNull.Value);

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

            var cmd = new NpgsqlCommand(@"
                                        SELECT balance 
                                        FROM users 
                                        WHERE id = @id", conn);

            cmd.Parameters.AddWithValue("id", userId);

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<bool> TryWithdraw(long userId, int amount)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand(@"
                                        UPDATE users 
                                        SET balance = balance - @amount 
                                        WHERE id = @id 
                                        AND balance >= @amount", conn);

            cmd.Parameters.AddWithValue("id", userId);
            cmd.Parameters.AddWithValue("amount", amount);

            var rowsAffected = await cmd.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task AddBalance(long userId, int amount)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand(@"
                                        UPDATE users 
                                        SET balance = balance + @amount 
                                        WHERE id = @id", conn);

            cmd.Parameters.AddWithValue("id", userId);
            cmd.Parameters.AddWithValue("amount", amount);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}