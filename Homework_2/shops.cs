// Создаю файл, где находится класс с мазагином

class Shop
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Shop(int id, string name)
    {
        Id = id;
        Name = name;
    }
    public Shop() : this(0, "") { }
    public override string ToString()
        => $"[{Id}] {Name}";
}