namespace BankService
{
    public abstract class SelectComboBoxItemBase
    {
        public string Name { get; set; }
    }

    public class SelectComboBoxItem<T> : SelectComboBoxItemBase
    {
        public T Data { get; set; }

        public SelectComboBoxItem()
        {
        }

        public SelectComboBoxItem(string name, T data)
        {
            Name = name;
            Data = data;
        }
    }
}
