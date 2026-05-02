using System.Text;

Console.OutputEncoding = Encoding.UTF8;

// путь к базе и CSV
string dbPath = "shop.db";
string shopsCsv = @"C:\Users\Артём\source\repos\homework_2\homework_2\shop.csv";
string ordersCsv = @"C:\Users\Артём\source\repos\homework_2\homework_2\order.csv";

// создаём менеджер БД
var db = new DatabaseManager(dbPath);

// инициализация
db.InitializeDatabase(shopsCsv, ordersCsv);

string choice;

do
{
    Console.WriteLine("\n1 - Показать магазины");
    Console.WriteLine("2 - Показать заказы");
    Console.WriteLine("3 - Добавить заказ");
    Console.WriteLine("4 - Удалить заказ");
    Console.WriteLine("5 - Отчёт");
    Console.WriteLine("0 - Выход");

    Console.Write("Выбор: ");
    choice = Console.ReadLine();

    switch (choice)
    {
        case "1":
            ShowShops(db);
            break;

        case "2":
            ShowOrders(db);
            break;

        case "3":
            AddOrder(db);
            break;

        case "4":
            DeleteOrder(db);
            break;

        case "5":
            ShowReport(db);
            break;
    }

} while (choice != "0");


static void ShowShops(DatabaseManager db)
{
    var shops = db.GetAllShops();

    foreach (var s in shops)
        Console.WriteLine(s);
}

static void ShowOrders(DatabaseManager db)
{
    var orders = db.GetAllOrders();

    foreach (var o in orders)
        Console.WriteLine(o);
}

static void AddOrder(DatabaseManager db)
{
    Console.Write("ID магазина: ");
    int shopId = int.Parse(Console.ReadLine());

    Console.Write("Название: ");
    string name = Console.ReadLine();

    Console.Write("Сумма: ");
    int amount = int.Parse(Console.ReadLine());

    var order = new Order(0, shopId, name, amount);

    db.AddOrder(order);

    Console.WriteLine("Добавлено!");
}


static void DeleteOrder(DatabaseManager db)
{
    Console.Write("ID заказа: ");
    int id = int.Parse(Console.ReadLine());

    db.DeleteOrder(id);

    Console.WriteLine("Удалено!");
}

static void ShowReport(DatabaseManager db)
{
    new ReportBuilder(db)
        .Query(@"SELECT order_name, amount FROM orders")
        .Title("Все заказы")
        .Header("Название", "Сумма")
        .ColumnWidths(30, 10)
        .SaveToFile(@"C:\Users\Артём\source\repos\homework_2\homework_2\отчёт.txt");
}