using System.Data.SqlClient;
using Microsoft.Data.SqlClient;
using APBD_Tutorial9.Model;

namespace APBD_Tutorial9.Services
{
    public class WarehouseService : IWarehouseService
    {
        private readonly string _connectionString;

        public WarehouseService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<int> AddProductManually(ProductWarehouseRequest request)
        {
            if (request.Amount <= 0)
                throw new ArgumentException("Amount must be greater than 0");

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            // Check if product exists
            var checkProductCmd = new SqlCommand("SELECT 1 FROM Product WHERE IdProduct = @IdProduct", connection);
            checkProductCmd.Parameters.AddWithValue("@IdProduct", request.IdProduct);
            var productExists = await checkProductCmd.ExecuteScalarAsync() != null;
            if (!productExists)
                throw new ArgumentException("Product does not exist");

            // Check if warehouse exists
            var checkWarehouseCmd =
                new SqlCommand("SELECT 1 FROM Warehouse WHERE IdWarehouse = @IdWarehouse", connection);
            checkWarehouseCmd.Parameters.AddWithValue("@IdWarehouse", request.IdWarehouse);
            var warehouseExists = await checkWarehouseCmd.ExecuteScalarAsync() != null;
            if (!warehouseExists)
                throw new ArgumentException("Warehouse does not exist");

            // Check if order exists and is valid
            var orderCmd = new SqlCommand(@"
                SELECT TOP 1 IdOrder 
                FROM [Order] o
                LEFT JOIN Product_Warehouse pw ON o.IdOrder = pw.IdOrder
                WHERE o.IdProduct = @IdProduct 
                  AND o.Amount = @Amount 
                  AND o.CreatedAt < @CreatedAt 
                  AND pw.IdProductWarehouse IS NULL
            ", connection);
            orderCmd.Parameters.AddWithValue("@IdProduct", request.IdProduct);
            orderCmd.Parameters.AddWithValue("@Amount", request.Amount);
            orderCmd.Parameters.AddWithValue("@CreatedAt", request.CreatedAt);

            var idOrderObj = await orderCmd.ExecuteScalarAsync();
            if (idOrderObj == null)
                throw new ArgumentException("No matching order to fulfill");

            int idOrder = (int)idOrderObj;

            // Check if already fulfilled
            var alreadyFulfilledCmd =
                new SqlCommand("SELECT 1 FROM Product_Warehouse WHERE IdOrder = @IdOrder", connection);
            alreadyFulfilledCmd.Parameters.AddWithValue("@IdOrder", idOrder);
            var alreadyFulfilled = await alreadyFulfilledCmd.ExecuteScalarAsync() != null;
            if (alreadyFulfilled)
                throw new ArgumentException("Order already fulfilled");

            // Get product price
            var priceCmd = new SqlCommand("SELECT Price FROM Product WHERE IdProduct = @IdProduct", connection);
            priceCmd.Parameters.AddWithValue("@IdProduct", request.IdProduct);
            var price = (decimal)(await priceCmd.ExecuteScalarAsync() ??
                                  throw new ArgumentException("Product price not found"));

            // Begin transaction
            var transaction = connection.BeginTransaction();

            try
            {
                // Update order
                var updateCmd = new SqlCommand("UPDATE [Order] SET FulfilledAt = @Now WHERE IdOrder = @IdOrder",
                    connection, transaction);
                updateCmd.Parameters.AddWithValue("@Now", DateTime.Now);
                updateCmd.Parameters.AddWithValue("@IdOrder", idOrder);
                await updateCmd.ExecuteNonQueryAsync();

                // Insert into Product_Warehouse
                var insertCmd = new SqlCommand(@"
                    INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)
                    VALUES (@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Price, @CreatedAt);
                    SELECT CAST(SCOPE_IDENTITY() as int);
                ", connection, transaction);

                insertCmd.Parameters.AddWithValue("@IdWarehouse", request.IdWarehouse);
                insertCmd.Parameters.AddWithValue("@IdProduct", request.IdProduct);
                insertCmd.Parameters.AddWithValue("@IdOrder", idOrder);
                insertCmd.Parameters.AddWithValue("@Amount", request.Amount);
                insertCmd.Parameters.AddWithValue("@Price", request.Amount * price);
                insertCmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

                int newId = (int)await insertCmd.ExecuteScalarAsync();

                await transaction.CommitAsync();
                return newId;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<int> AddProductWithProcedure(ProductWarehouseRequest request)
        {
            if (request.Amount <= 0)
                throw new ArgumentException("Amount must be greater than 0");

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand("AddProductToWarehouse", connection);
            command.CommandType = System.Data.CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@IdProduct", request.IdProduct);
            command.Parameters.AddWithValue("@IdWarehouse", request.IdWarehouse);
            command.Parameters.AddWithValue("@Amount", request.Amount);
            command.Parameters.AddWithValue("@CreatedAt", request.CreatedAt);

            try
            {
                var result = await command.ExecuteScalarAsync();
                if (result == null)
                    throw new ArgumentException("Procedure failed to return ID.");

                return Convert.ToInt32(result);
            }
            catch (SqlException ex)
            {
                
                throw new ArgumentException(ex.Message);
            }
        }
    }
}