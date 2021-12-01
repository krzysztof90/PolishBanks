namespace BankService
{
    public abstract class PrepaidOperatorComboBoxItemBase
    {
        public string Name { get; set; }
    }

    public class PrepaidOperatorComboBoxItem<T> : PrepaidOperatorComboBoxItemBase
    {
        public T Data { get; set; }

        public PrepaidOperatorComboBoxItem()
        {
        }

        public PrepaidOperatorComboBoxItem(string name, T data)
        {
            Name = name;
            Data = data;
        }
    }
}
