using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AromaDomWpf
{
    public partial class MainWindow : Window
    {
        private bool isLoading;
        public MainWindow()
        {
            InitializeComponent();
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadMaterials();
        }
        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadMaterials();
        }
        private async Task LoadMaterials()
        {
            if (isLoading)
                return;
            isLoading = true;
            StatusText.Text = "Загрузка...";
            try
            {
                var table = await Task.Run(() => Database.Read(@"SELECT m.*, t.name AS type_name,
                    COALESCE((SELECT SUM(quantity) FROM dbo.material_products mp WHERE mp.material_id=m.material_id),0) AS required
                    FROM dbo.materials m JOIN dbo.material_types t ON t.material_type_id=m.material_type_id ORDER BY m.material_id"));
                MaterialsList.ItemsSource = table.AsEnumerable().Select(row => new Material
                {
                    Id = (int)row["material_id"],
                    TypeId = (int)row["material_type_id"],
                    Name = (string)row["name"],
                    TypeName = (string)row["type_name"],
                    Unit = (string)row["unit"],
                    Price = (decimal)row["price"],
                    PackageQuantity = (decimal)row["package_quantity"],
                    StockQuantity = (decimal)row["stock_quantity"],
                    MinQuantity = (decimal)row["min_quantity"],
                    RequiredQuantity = (decimal)row["required"]
                }).ToList();
                StatusText.Text = "Материалов: " + table.Rows.Count + ". Нажмите карточку для редактирования.";
            }
            catch (Exception ex)
            {
                StatusText.Text = "Нет связи с базой. Проверьте сервер в файле .exe.config и базу AromaDom_V6. Нажмите «Обновить».";
                MessageBox.Show(StatusText.Text + "\n" + ex.Message, "Ошибка подключения", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                isLoading = false;
            }
        }
        private async void Add_Click(object sender, RoutedEventArgs e)
        {
            if (new MaterialWindow(null) { Owner = this }.ShowDialog() == true)
                await LoadMaterials();
        }
        private async void Edit_Click(object sender, MouseButtonEventArgs e)
        {
            var material = (sender as FrameworkElement)?.DataContext as Material;
            if (material != null && new MaterialWindow(material.Id) { Owner = this }.ShowDialog() == true)
                await LoadMaterials();
        }
        private void Products_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            var material = (sender as FrameworkElement)?.DataContext as Material;
            if (material != null)
                new ProductsWindow(material) { Owner = this }.ShowDialog();
        }
        private void Calculate_Click(object sender, RoutedEventArgs e)
        {
            new CalculationWindow { Owner = this }.ShowDialog();
        }
    }
}
