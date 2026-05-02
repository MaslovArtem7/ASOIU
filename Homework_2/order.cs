class Order
{
    public int Id { get; set; }
    public int ShopId { get; set; }
    public string Name { get; set; }
    private int _amount;
    public int Amount
    {
        get { return _amount; } //уточнить у Юрия Евгеньевича (используем get-set для контроля и проверки данных? Чтобы, например, не могли написать .amount = -500
        set
        {
            if (value < 0)
                throw new ArgumentException("Цена не может быть отрицательной");
            _amount = value;
        }
    }
    public Order(int id, int shopId, string name, int amount)
    {
        Id = id;
        ShopId = shopId;
        Name = name;
        Amount = amount;
        Amount = amount;
    }
    public Order(): this(0, 0, "", 0) { }
    public override string ToString() //переопределяем метод, чтобы при выводе Console.WriteLine(order); вывелись данные заказа
        => $"[{Id}] {Name}, магазин #{ShopId}, сумма: {Amount} руб.";
}