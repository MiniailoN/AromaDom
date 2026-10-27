using System;
using System.Globalization;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AromaDomWpf
{
    public class MaterialWindow : Window
    {
        private readonly int? materialId;
        private readonly ComboBox types = new ComboBox();
        private readonly TextBox name = new TextBox { MaxLength = 150 };
        private readonly TextBox price = new TextBox();
        private readonly TextBox unit = new TextBox { MaxLength = 20 };
        private readonly TextBox package = new TextBox();
        private readonly TextBox stock = new TextBox();
        private readonly TextBox minimum = new TextBox();
        private readonly Button save;

        public MaterialWindow(int? id)
        {
            materialId = id;
            Title = id.HasValue ? "Редактирование материала" : "Добавление материала";
            Width = 620;
            Height = 650;
            MinWidth = 520;
            MinHeight = 500;
            FontFamily = new FontFamily("Georgia");
            FontSize = 16;
            Background = Brushes.White;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            var panel = new StackPanel { Margin = new Thickness(24) };
            Content = new ScrollViewer { Content = panel, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            panel.Children.Add(new TextBlock { Text = Title, FontSize = 24, Margin = new Thickness(0,0,0,14) });
            AddField(panel, "Тип материала", types);
            AddField(panel, "Наименование", name);
            AddField(panel, "Цена за единицу", price);
            AddField(panel, "Единица измерения", unit);
            AddField(panel, "Количество в упаковке", package);
            AddField(panel, "Количество на складе", stock);
            AddField(panel, "Минимальное количество", minimum);
            var buttons = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0,18,0,0) };
            save = MakeButton("Сохранить");
            save.Click += Save_Click;
            var back = MakeButton("Назад");
            back.Click += (s,e) => Close();
            buttons.Children.Add(save);
            buttons.Children.Add(back);
            panel.Children.Add(buttons);
            Loaded += Load;
        }
        public static Button MakeButton(string text)
        {
            return new Button { Content = text, Padding = new Thickness(16,8,16,8), Margin = new Thickness(0,0,10,0),
                Background = (Brush)new BrushConverter().ConvertFromString("#8A5A44"), Foreground = Brushes.White };
        }
        public static void AddField(Panel panel, string title, Control field)
        {
            panel.Children.Add(new TextBlock { Text = title, Margin = new Thickness(0,6,0,4) });
            field.Padding = new Thickness(6);
            panel.Children.Add(field);
        }
        private void Load(object sender, RoutedEventArgs e)
        {
            try
            {
                types.ItemsSource = Database.Read("SELECT material_type_id,name FROM dbo.material_types ORDER BY name").DefaultView;
                types.DisplayMemberPath = "name";
                types.SelectedValuePath = "material_type_id";
                types.SelectedIndex = 0;
                price.Text = package.Text = stock.Text = minimum.Text = "0";
                if (!materialId.HasValue)
                    return;
                var table = Database.Read("SELECT * FROM dbo.materials WHERE material_id=@id", new SqlParameter("@id",materialId.Value));
                if (table.Rows.Count == 0)
                    throw new InvalidOperationException("Материал не найден. Вернитесь назад и обновите список.");
                var row = table.Rows[0];
                types.SelectedValue = row["material_type_id"];
                name.Text = row["name"].ToString();
                price.Text = row["price"].ToString();
                unit.Text = row["unit"].ToString();
                package.Text = row["package_quantity"].ToString();
                stock.Text = row["stock_quantity"].ToString();
                minimum.Text = row["min_quantity"].ToString();
            }
            catch (Exception ex)
            {
                save.IsEnabled = false;
                MessageBox.Show(ex.Message + "\nВернитесь назад и проверьте подключение к БД.", "Ошибка загрузки", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public static decimal ReadNumber(string text, string title, int decimals)
        {
            decimal value;
            if (!decimal.TryParse(text.Replace(',', '.'), NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite, CultureInfo.InvariantCulture, out value) || value < 0)
                throw new ArgumentException(title + ": введите неотрицательное число.");
            value = Math.Round(value, decimals, MidpointRounding.AwayFromZero);
            decimal limit = decimals == 2 ? 10000000000m : 1000000000m;
            if (value >= limit)
                throw new ArgumentException(title + ": слишком большое число.");
            return value;
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (types.SelectedValue == null || string.IsNullOrWhiteSpace(name.Text) || string.IsNullOrWhiteSpace(unit.Text))
                    throw new ArgumentException("Выберите тип материала, заполните наименование и единицу измерения.");
                Database.Save(materialId, name.Text.Trim(), (int)types.SelectedValue, unit.Text.Trim(),
                    ReadNumber(price.Text,"Цена",2), ReadNumber(package.Text,"Количество в упаковке",3),
                    ReadNumber(stock.Text,"Количество на складе",3), ReadNumber(minimum.Text,"Минимальное количество",3));
                MessageBox.Show("Материал сохранён.", "Сохранено", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message,"Проверьте данные",MessageBoxButton.OK,MessageBoxImage.Warning);
            }
            catch (SqlException ex)
            {
                var text = ex.Number == 2601 || ex.Number == 2627
                    ? "Такое название уже есть. Введите другое название материала."
                    : "Не удалось сохранить. Проверьте подключение и тип материала.\n" + ex.Message;
                MessageBox.Show(text,"Ошибка сохранения",MessageBoxButton.OK,MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,"Ошибка",MessageBoxButton.OK,MessageBoxImage.Error);
            }
        }
    }
}
