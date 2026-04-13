using Microsoft.Data.Sqlite;

// ==========================================
// Семинар №3. Работа с CSV и SQLite в C#
// ==========================================

// Файлы, с которыми будем работать
const string dbFile = "developers.db";
const string devCsv = @"C:\Users\Артём\source\repos\Sem3\Sem3\dev.csv";
const string depCsv = @"C:\Users\Артём\source\repos\Sem3\Sem3\dep.csv";

// 1. Создаем новую базу данных
CreateDatabase(dbFile);

// 2. Загружаем данные из CSV-файлов в таблицы
LoadData(dbFile, devCsv, depCsv);

// 3. Печатаем содержимое таблиц
PrintData(dbFile, "dep");
PrintData(dbFile, "dev");

// 4. Проекция: получаем только имена разработчиков
List<string> names = Projection(dbFile, "dev", "dev_name");
Console.WriteLine("\n=== Результат Projection(dev, dev_name) ===");
foreach (var name in names)
{
    Console.WriteLine(name);
}

// 5. Выборка: получаем только разработчиков из отдела с dep_id = 2
List<string[]> filteredRows = Where(dbFile, "dev", "dep_id", "2");
Console.WriteLine("\n=== Результат Where(dev, dep_id, 2) ===");
foreach (var row in filteredRows)
{
    Console.WriteLine(string.Join(" | ", row));
}

// 6. Соединение таблиц dev и dep по полю dep_id
var (joinColumns, joinRows) = Join(dbFile, "dev", "dep", "dep_id", "dep_id");
Console.WriteLine("\n=== Результат Join(dev, dep, dep_id, dep_id) ===");
Console.WriteLine(string.Join(" | ", joinColumns));
Console.WriteLine(new string('-', 80));
foreach (var row in joinRows)
{
    Console.WriteLine(string.Join(" | ", row));
}

// 7. Группировка по отделам и вычисление среднего числа коммитов
var (avgColumns, avgRows) = GroupAvg(dbFile, "dev", "dep_id", "dev_commits");
Console.WriteLine("\n=== Результат GroupAvg(dev, dep_id, dev_commits) ===");
Console.WriteLine(string.Join(" | ", avgColumns));
Console.WriteLine(new string('-', 40));
foreach (var row in avgRows)
{
    Console.WriteLine(string.Join(" | ", row));
}


// ==========================================
// ФУНКЦИИ
// ==========================================

/// <summary>
/// Создает новую SQLite-базу и две таблицы:
/// dep - отделы
/// dev - разработчики
/// </summary>
static void CreateDatabase(string dbPath)
{
    // Если файл БД уже есть, удаляем его,
    // чтобы начать с чистого состояния
    if (File.Exists(dbPath))
    {
        File.Delete(dbPath);
    }

    using var connection = new SqliteConnection($"Data Source={dbPath}");
    connection.Open();

    var command = connection.CreateCommand();

    // Создаем таблицу отделов
    command.CommandText = @"
        CREATE TABLE dep (
            dep_id   INTEGER PRIMARY KEY,
            dep_name TEXT NOT NULL
        );";
    command.ExecuteNonQuery();

    // Создаем таблицу разработчиков
    command.CommandText = @"
        CREATE TABLE dev (
            dev_id      INTEGER PRIMARY KEY,
            dep_id      INTEGER NOT NULL,
            dev_name    TEXT    NOT NULL,
            dev_commits INTEGER NOT NULL,
            FOREIGN KEY (dep_id) REFERENCES dep(dep_id)
        );";
    command.ExecuteNonQuery();

    Console.WriteLine($"[OK] База данных \"{dbPath}\" создана.");
    Console.WriteLine("[OK] Таблицы dep и dev готовы.");
}

/// <summary>
/// Загружает данные из CSV-файлов в таблицы dep и dev.
/// </summary>
static void LoadData(string dbPath, string devCsvPath, string depCsvPath)
{
    using var connection = new SqliteConnection($"Data Source={dbPath}");
    connection.Open();

    // Сначала загружаем таблицу dep
    using (var transaction = connection.BeginTransaction())
    {
        var lines = File.ReadAllLines(depCsvPath);

        // Пропускаем первую строку, потому что это заголовок
        for (int i = 1; i < lines.Length; i++)
        {
            var parts = lines[i].Split(';');

            // Ожидаем 2 поля: dep_id;dep_name
            if (parts.Length < 2)
            {
                continue;
            }

            var cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO dep (dep_id, dep_name) VALUES (@id, @name);";
            cmd.Parameters.AddWithValue("@id", int.Parse(parts[0]));
            cmd.Parameters.AddWithValue("@name", parts[1]);
            cmd.ExecuteNonQuery();
        }

        transaction.Commit();
        Console.WriteLine($"[OK] Загружены данные из файла \"{depCsvPath}\".");
    }

    // Затем загружаем таблицу dev
    using (var transaction = connection.BeginTransaction())
    {
        var lines = File.ReadAllLines(devCsvPath);

        // Пропускаем строку заголовков
        for (int i = 1; i < lines.Length; i++)
        {
            var parts = lines[i].Split(';');

            // Ожидаем 4 поля: dev_id;dep_id;dev_name;dev_commits
            if (parts.Length < 4)
            {
                continue;
            }

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO dev (dev_id, dep_id, dev_name, dev_commits)
                VALUES (@devId, @depId, @name, @commits);";

            cmd.Parameters.AddWithValue("@devId", int.Parse(parts[0]));
            cmd.Parameters.AddWithValue("@depId", int.Parse(parts[1]));
            cmd.Parameters.AddWithValue("@name", parts[2]);
            cmd.Parameters.AddWithValue("@commits", int.Parse(parts[3]));
            cmd.ExecuteNonQuery();
        }

        transaction.Commit();
        Console.WriteLine($"[OK] Загружены данные из файла \"{devCsvPath}\".");
    }
}

/// <summary>
/// Печатает всю таблицу целиком.
/// Аналог SQL-запроса: SELECT * FROM tableName
/// </summary>
static void PrintData(string dbPath, string tableName)
{
    using var connection = new SqliteConnection($"Data Source={dbPath}");
    connection.Open();

    var cmd = connection.CreateCommand();
    cmd.CommandText = $"SELECT * FROM {tableName} ORDER BY 1;";

    using var reader = cmd.ExecuteReader();

    int columnCount = reader.FieldCount;
    const int colWidth = 20;

    Console.WriteLine($"\n========== Таблица {tableName} ==========");

    // Печатаем имена столбцов
    for (int i = 0; i < columnCount; i++)
    {
        Console.Write($"{reader.GetName(i),-colWidth}");
    }
    Console.WriteLine();

    Console.WriteLine(new string('-', columnCount * colWidth));

    // Печатаем строки таблицы
    while (reader.Read())
    {
        for (int i = 0; i < columnCount; i++)
        {
            Console.Write($"{reader.GetValue(i),-colWidth}");
        }
        Console.WriteLine();
    }
}

/// <summary>
/// Проекция:
/// SELECT columnName FROM tableName
/// Возвращает список значений одного столбца.
/// </summary>
static List<string> Projection(string dbPath, string tableName, string columnName)
{
    var result = new List<string>();

    using var connection = new SqliteConnection($"Data Source={dbPath}");
    connection.Open();

    var cmd = connection.CreateCommand();
    cmd.CommandText = $"SELECT {columnName} FROM {tableName} ORDER BY 1;";

    using var reader = cmd.ExecuteReader();
    while (reader.Read())
    {
        result.Add(reader.GetValue(0).ToString()!);
    }

    return result;
}

/// <summary>
/// Выборка:
/// SELECT * FROM tableName WHERE columnName = value
/// Возвращает строки, которые удовлетворяют условию.
/// </summary>
static List<string[]> Where(string dbPath, string tableName, string columnName, string value)
{
    var result = new List<string[]>();

    using var connection = new SqliteConnection($"Data Source={dbPath}");
    connection.Open();

    var cmd = connection.CreateCommand();

    // Значение передаем через параметр @val
    // Это безопаснее, чем вставлять value прямо в SQL-строку
    cmd.CommandText = $"SELECT * FROM {tableName} WHERE {columnName} = @val ORDER BY 1;";
    cmd.Parameters.AddWithValue("@val", value);

    using var reader = cmd.ExecuteReader();
    while (reader.Read())
    {
        var row = new string[reader.FieldCount];
        for (int i = 0; i < reader.FieldCount; i++)
        {
            row[i] = reader.GetValue(i).ToString()!;
        }

        result.Add(row);
    }

    return result;
}

/// <summary>
/// Соединение двух таблиц по ключам:
/// SELECT *
/// FROM table1
/// INNER JOIN table2 ON table1.key1 = table2.key2
/// </summary>
static (string[] columns, List<string[]> rows) Join(
    string dbPath,
    string table1,
    string table2,
    string key1,
    string key2)
{
    var rows = new List<string[]>();

    using var connection = new SqliteConnection($"Data Source={dbPath}");
    connection.Open();

    var cmd = connection.CreateCommand();
    cmd.CommandText = $@"
        SELECT *
        FROM {table1}
        INNER JOIN {table2}
            ON {table1}.{key1} = {table2}.{key2}
        ORDER BY 1;";

    using var reader = cmd.ExecuteReader();

    // Читаем имена столбцов результата
    var columns = new string[reader.FieldCount];
    for (int i = 0; i < reader.FieldCount; i++)
    {
        columns[i] = reader.GetName(i);
    }

    // Читаем строки результата
    while (reader.Read())
    {
        var row = new string[reader.FieldCount];
        for (int i = 0; i < reader.FieldCount; i++)
        {
            row[i] = reader.GetValue(i).ToString()!;
        }

        rows.Add(row);
    }

    return (columns, rows);
}

/// <summary>
/// Группировка и вычисление среднего:
/// SELECT groupColumn, AVG(avgColumn)
/// FROM tableName
/// GROUP BY groupColumn
/// </summary>
static (string[] columns, List<string[]> rows) GroupAvg(
    string dbPath,
    string tableName,
    string groupColumn,
    string avgColumn)
{
    var rows = new List<string[]>();

    using var connection = new SqliteConnection($"Data Source={dbPath}");
    connection.Open();

    var cmd = connection.CreateCommand();
    cmd.CommandText = $@"
        SELECT {groupColumn}, AVG({avgColumn}) AS avg_{avgColumn}
        FROM {tableName}
        GROUP BY {groupColumn}
        ORDER BY 1;";

    using var reader = cmd.ExecuteReader();

    var columns = new string[reader.FieldCount];
    for (int i = 0; i < reader.FieldCount; i++)
    {
        columns[i] = reader.GetName(i);
    }

    while (reader.Read())
    {
        var row = new string[reader.FieldCount];
        for (int i = 0; i < reader.FieldCount; i++)
        {
            row[i] = reader.GetValue(i).ToString()!;
        }

        rows.Add(row);
    }

    return (columns, rows);
}
