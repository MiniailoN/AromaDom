using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace AromaDomWpf
{
    public static class Database
    {
        public static DataTable Read(string sql, params SqlParameter[] parameters)
        {
            using (var connection = Open())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddRange(parameters);
                var table = new DataTable();
                using (var adapter = new SqlDataAdapter(command))
                    adapter.Fill(table);
                return table;
            }
        }

        public static SqlConnection Open()
        {
            var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["AromaDom"].ConnectionString);
            try
            {
                connection.Open();
                return connection;
            }
            catch
            {
                connection.Dispose();
                throw;
            }
        }

        public static void Save(int? id, string name, int typeId, string unit, decimal price,
            decimal package, decimal stock, decimal minimum)
        {
            using (var connection = Open())
            using (var transaction = connection.BeginTransaction())
            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = id.HasValue
                    ? @"UPDATE dbo.materials SET name=@name,material_type_id=@type,unit=@unit,
                        price=@price,package_quantity=@package,stock_quantity=@stock,min_quantity=@minimum WHERE material_id=@id"
                    : @"INSERT INTO dbo.materials(material_id,name,material_type_id,unit,price,package_quantity,stock_quantity,min_quantity)
                        SELECT COALESCE(MAX(material_id),0)+1,@name,@type,@unit,@price,@package,@stock,@minimum
                        FROM dbo.materials WITH (UPDLOCK,HOLDLOCK)";
                command.Parameters.AddWithValue("@id", (object)id ?? DBNull.Value);
                command.Parameters.AddWithValue("@name", name);
                command.Parameters.AddWithValue("@type", typeId);
                command.Parameters.AddWithValue("@unit", unit);
                command.Parameters.AddWithValue("@price", price);
                command.Parameters.AddWithValue("@package", package);
                command.Parameters.AddWithValue("@stock", stock);
                command.Parameters.AddWithValue("@minimum", minimum);
                if (command.ExecuteNonQuery() != 1)
                    throw new InvalidOperationException("Материал не найден. Обновите список.");
                transaction.Commit();
            }
        }
    }
}
