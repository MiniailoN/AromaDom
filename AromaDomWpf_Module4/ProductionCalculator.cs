using System;
using System.Data.SqlClient;

namespace AromaDomWpf
{
    public static class ProductionCalculator
    {
        public static int CalculateProducts(int productTypeId, int materialTypeId, int rawQuantity, double parameter1, double parameter2)
        {
            if (productTypeId <= 0 || materialTypeId <= 0 || rawQuantity < 0 ||
                parameter1 <= 0 || parameter2 <= 0 || double.IsNaN(parameter1) || double.IsNaN(parameter2) ||
                double.IsInfinity(parameter1) || double.IsInfinity(parameter2))
                return -1;
            var products = Database.Read("SELECT coefficient FROM dbo.product_types WHERE product_type_id=@id", new SqlParameter("@id", productTypeId));
            var materials = Database.Read("SELECT loss_percent FROM dbo.material_types WHERE material_type_id=@id", new SqlParameter("@id", materialTypeId));
            if (products.Rows.Count == 0 || materials.Rows.Count == 0)
                return -1;
            return Calculate(rawQuantity, parameter1, parameter2, (decimal)products.Rows[0][0], (decimal)materials.Rows[0][0]);
        }
        public static int Calculate(int rawQuantity, double parameter1, double parameter2, decimal coefficient, decimal lossPercent)
        {
            if (rawQuantity < 0 || parameter1 <= 0 || parameter2 <= 0 || coefficient <= 0 || lossPercent < 0 || lossPercent > 100 ||
                double.IsNaN(parameter1) || double.IsNaN(parameter2) || double.IsInfinity(parameter1) || double.IsInfinity(parameter2))
                return -1;
            try
            {
                decimal perItem = (decimal)parameter1 * (decimal)parameter2 * coefficient;
                if (perItem <= 0)
                    return -1;
                decimal available = rawQuantity * (1 - lossPercent / 100);
                decimal result = Math.Floor(available / perItem);
                return result > int.MaxValue ? -1 : (int)result;
            }
            catch (OverflowException)
            {
                return -1;
            }
        }
    }
}
