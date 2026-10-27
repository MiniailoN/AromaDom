using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Data.SqlClient;

namespace AromaDomWpf
{
    public class ProductsWindow : Window
    {
        public ProductsWindow(Material material)
        {
            Title = "Продукция по материалу";
            Width = 850;
            Height = 550;
            MinWidth = 600;
            MinHeight = 400;
            FontFamily = new FontFamily("Georgia");
            FontSize = 16;
            Background = Brushes.White;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            var panel = new DockPanel { Margin = new Thickness(24) };
            Content = panel;
            var title = new TextBlock { Text = material.Name, FontSize = 22, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0,0,0,18) };
            DockPanel.SetDock(title,Dock.Top);
            panel.Children.Add(title);
            var back = MaterialWindow.MakeButton("Назад");
            back.HorizontalAlignment = HorizontalAlignment.Left;
            back.Margin = new Thickness(0,15,0,0);
            back.Click += (s,e) => Close();
            DockPanel.SetDock(back,Dock.Bottom);
            panel.Children.Add(back);
            var table = new DataGrid { IsReadOnly = true, AutoGenerateColumns = false, CanUserAddRows = false, Background = Brushes.White };
            table.Columns.Add(new DataGridTextColumn { Header = "Наименование продукции", MinWidth = 350, Binding = new System.Windows.Data.Binding("name"), Width = new DataGridLength(1,DataGridLengthUnitType.Star) });
            table.Columns.Add(new DataGridTextColumn { Header = "Количество, " + material.Unit, Binding = new System.Windows.Data.Binding("quantity") { StringFormat = "0.###" }, Width = 190 });
            panel.Children.Add(table);
            Loaded += (s,e) =>
            {
                try
                {
                    var data = Database.Read(@"SELECT p.name,mp.quantity FROM dbo.material_products mp
                        JOIN dbo.products p ON p.product_id=mp.product_id WHERE mp.material_id=@id ORDER BY p.name",new SqlParameter("@id",material.Id));
                    table.ItemsSource = data.DefaultView;
                    if (data.Rows.Count == 0)
                        MessageBox.Show("Материал пока не используется в рецептурах.","Продукция",MessageBoxButton.OK,MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Проверьте подключение к базе и повторите открытие окна.\n" + ex.Message,"Ошибка",MessageBoxButton.OK,MessageBoxImage.Error);
                }
            };
        }
    }
    public class CalculationWindow : Window
    {
        public CalculationWindow()
        {
            Title = "Расчёт количества продукции";
            Width = 620;
            Height = 610;
            MinWidth = 500;
            MinHeight = 450;
            FontFamily = new FontFamily("Georgia");
            FontSize = 16;
            Background = Brushes.White;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            var panel = new StackPanel { Margin = new Thickness(24) };
            Content = new ScrollViewer { Content = panel, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            panel.Children.Add(new TextBlock { Text = Title, FontSize = 23, Margin = new Thickness(0,0,0,16) });
            var products = new ComboBox { DisplayMemberPath = "name", SelectedValuePath = "product_type_id" };
            var materials = new ComboBox { DisplayMemberPath = "name", SelectedValuePath = "material_type_id" };
            var raw = new TextBox();
            var first = new TextBox();
            var second = new TextBox();
            MaterialWindow.AddField(panel,"Тип продукции",products);
            MaterialWindow.AddField(panel,"Тип материала",materials);
            MaterialWindow.AddField(panel,"Количество сырья (целое)",raw);
            MaterialWindow.AddField(panel,"Параметр продукции 1",first);
            MaterialWindow.AddField(panel,"Параметр продукции 2",second);
            var result = new TextBlock { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0,18,0,18) };
            panel.Children.Add(result);
            var buttons = new StackPanel { Orientation = Orientation.Horizontal };
            panel.Children.Add(buttons);
            var calculate = MaterialWindow.MakeButton("Рассчитать");
            var back = MaterialWindow.MakeButton("Назад");
            buttons.Children.Add(calculate);
            buttons.Children.Add(back);
            back.Click += (s,e) => Close();
            Loaded += (s,e) =>
            {
                try
                {
                    products.ItemsSource = Database.Read("SELECT * FROM dbo.product_types ORDER BY name").DefaultView;
                    materials.ItemsSource = Database.Read("SELECT * FROM dbo.material_types ORDER BY name").DefaultView;
                    products.SelectedIndex = materials.SelectedIndex = 0;
                }
                catch (Exception ex)
                {
                    calculate.IsEnabled = false;
                    MessageBox.Show("Проверьте подключение к базе и откройте окно заново.\n" + ex.Message,"Ошибка",MessageBoxButton.OK,MessageBoxImage.Error);
                }
            };
            calculate.Click += (s,e) =>
            {
                result.Text = "";
                try
                {
                    int quantity;
                    double p1;
                    double p2;
                    if (products.SelectedValue == null || materials.SelectedValue == null || !int.TryParse(raw.Text,out quantity) ||
                        !double.TryParse(first.Text.Replace(',','.'),NumberStyles.Float,CultureInfo.InvariantCulture,out p1) ||
                        !double.TryParse(second.Text.Replace(',','.'),NumberStyles.Float,CultureInfo.InvariantCulture,out p2))
                        throw new ArgumentException("Выберите оба типа, введите целое количество сырья и два положительных параметра.");
                    int count = ProductionCalculator.CalculateProducts((int)products.SelectedValue,(int)materials.SelectedValue,quantity,p1,p2);
                    if (count == -1)
                        throw new ArgumentException("Проверьте исходные данные: сырьё не может быть отрицательным, параметры должны быть положительными и не слишком большими.");
                    result.Text = "Можно изготовить: " + count + " шт. с учётом потерь сырья.";
                }
                catch (ArgumentException ex)
                {
                    MessageBox.Show(ex.Message,"Проверьте данные",MessageBoxButton.OK,MessageBoxImage.Warning);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Не удалось выполнить расчёт. Проверьте соединение с БД.\n" + ex.Message,"Ошибка",MessageBoxButton.OK,MessageBoxImage.Error);
                }
            };
        }
    }
}

