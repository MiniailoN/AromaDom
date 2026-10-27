using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AromaDomWpf
{
    public class Material
    {
        public int Id { get; set; }
        public int TypeId { get; set; }
        public decimal PackageQuantity { get; set; }
        public string Name { get; set; }
        public string TypeName { get; set; }
        public string Unit { get; set; }
        public decimal Price { get; set; }
        public decimal StockQuantity { get; set; }
        public decimal MinQuantity { get; set; }
        public decimal RequiredQuantity { get; set; }

        public string Title => TypeName + " | " + Name;
        public string MinimumText => "Минимальное количество: " + Format(MinQuantity) + " " + Unit;
        public string StockText => "Количество на складе: " + Format(StockQuantity) + " " + Unit;
        public string PriceText => "Цена: " + Format(Price, "0.00") + " р / Единица измерения: " + Unit;
        public string RequiredText => Format(RequiredQuantity, "0.00") + " " + Unit;

        public static decimal CalculateRequiredQuantity(IEnumerable<decimal> quantities)
        {
            return quantities.Sum();
        }

        private static string Format(decimal value, string format = "0.###")
        {
            return value.ToString(format, CultureInfo.GetCultureInfo("ru-RU"));
        }
    }
}
