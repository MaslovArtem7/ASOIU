using Microsoft.Data.Sqlite;

class DatabaseManager
{
    private string _connectionString; // создаю переменную для строки подключения, в ней будет хранится путь к бд
    public DatabaseManager(string dbPath)
    {
        _connectionString = $"Data Source={dbPath}";
    }
    private void CreateTables()
    {
        using var conn = new SqliteConnection(_connectionString); //using закроет соединение после работы
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
        CREATE TABLE IF NOT EXISTS shops (
        shop_id INTEGER PRIMARY KEY AUTOINCREMENT,
        shop_name TEXT NOT NULL
        );

        CREATE TABLE IF NOT EXISTS orders (
        order_id INTEGER PRIMARY KEY AUTOINCREMENT,
        shop_id INTEGER NOT NULL,
        order_name TEXT NOT NULL,
        amount INTEGER NOT NULL,
        FOREIGN KEY (shop_id) REFERENCES shops(shop_id)
        );";
        cmd.ExecuteNonQuery();
        
    }
    public void InitializeDatabase(string shopsCsvPath, string ordersCsvPath)
    {
        CreateTables();
        if (GetAllShops().Count == 0 && File.Exists(shopsCsvPath))
        {
            ImportShopsFromCsv(shopsCsvPath);
            Console.WriteLine($"[ОК] Загружены магазины из {shopsCsvPath}");
        }
        if (GetAllOrders().Count == 0 && File.Exists(ordersCsvPath))
        {
            ImportOrdersFromCsv(ordersCsvPath);
            Console.WriteLine($"[ОК] Загружены заказы из {ordersCsvPath}");
        }
    }
    public List<Shop> GetAllShops()
    {
        var result = new List<Shop>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT shop_id, shop_name FROM shops ORDER BY shop_id";
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
            result.Add(new Shop(reader.GetInt32(0), reader.GetString(1)));
        
        return result;
    }
    public List<Order> GetAllOrders()
    {
        var result = new List<Order>();
        
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT order_id, shop_id, order_name, amount FROM orders ORDER BY order_id";
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
            result.Add(new Order(reader.GetInt32(0), reader.GetInt32(1), reader.GetString(2), reader.GetInt32(3)));
        return result;
    }
    public void AddOrder(Order order)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
        INSERT INTO orders (shop_id, order_name, amount)
        VALUES (@shopId, @name, @amount)";

        cmd.Parameters.AddWithValue("@shopId", order.ShopId);
        cmd.Parameters.AddWithValue("@name", order.Name);
        cmd.Parameters.AddWithValue("@amount", order.Amount);

        cmd.ExecuteNonQuery();
    }
    public void DeleteOrder(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();  
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM orders WHERE order_id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }
    public Order GetOrderById(int id)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
        SELECT order_id, shop_id, order_name, amount
        FROM orders
        WHERE order_id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new Order(
                reader.GetInt32(0),
                reader.GetInt32(1),
                reader.GetString(2),
                reader.GetInt32(3)
            );
        }
        return null;
    }



    private void ImportShopsFromCsv(string path)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        string[] lines = File.ReadAllLines(path);
        for (int i = 1; i < lines.Length; i++)
        {
            string[] parts = lines[i].Split(';');
            if (parts.Length < 2)
                    continue;
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
            INSERT INTO shops (shop_id, shop_name)
            VALUES (@id, @name)";

            cmd.Parameters.AddWithValue("@id", int.Parse(parts[0]));
            cmd.Parameters.AddWithValue("@name", parts[1]);

            cmd.ExecuteNonQuery();
        }
    }

    private void ImportOrdersFromCsv(string path)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        string[] lines = File.ReadAllLines(path);
        for (int i = 1; i < lines.Length; i++)
        {
            string[] parts = lines[i].Split(';');
            if (parts.Length < 4)
                continue;
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
            INSERT INTO orders (order_id, shop_id, order_name, amount)
            VALUES (@id, @shopId, @name, @amount)";
            cmd.Parameters.AddWithValue("@id", int.Parse(parts[0]));
            cmd.Parameters.AddWithValue("@shopId", int.Parse(parts[1]));
            cmd.Parameters.AddWithValue("@name", parts[2]);
            cmd.Parameters.AddWithValue("@amount", int.Parse(parts[3]));
            cmd.ExecuteNonQuery();
        }
    }
    public (string[] columns, List<string[]> rows) ExecuteQuery(string sql)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        using var reader = cmd.ExecuteReader();
        // массив названий колонок
        string[] columns = new string[reader.FieldCount];
        for (int i = 0; i < reader.FieldCount; i++)
            columns[i] = reader.GetName(i);
        // список строк
        var rows = new List<string[]>();
        while (reader.Read())
        {
            string[] row = new string[reader.FieldCount];

            for (int i = 0; i < reader.FieldCount; i++)
                row[i] = reader.GetValue(i)?.ToString() ?? "";

            rows.Add(row);
        }
        return (columns, rows);
    }
}