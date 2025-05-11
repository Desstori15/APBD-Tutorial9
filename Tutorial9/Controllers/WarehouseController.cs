
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Tutorial9.Model;

namespace Tutorial9.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WarehouseController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public WarehouseController(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("Default");
        }

        [HttpPost("add-manual")]
        public IActionResult AddProductManual(WarehouseRequest request)
        {
            if (request.Amount <= 0)
                return BadRequest("Amount must be greater than 0.");

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using var transaction = connection.BeginTransaction();
                try
                {
                    var cmd = new SqlCommand("SELECT 1 FROM Product WHERE IdProduct = @id", connection, transaction);
                    cmd.Parameters.AddWithValue("@id", request.IdProduct);
                    if (cmd.ExecuteScalar() == null)
                        return NotFound("Product not found.");

                    cmd = new SqlCommand("SELECT 1 FROM Warehouse WHERE IdWarehouse = @id", connection, transaction);
                    cmd.Parameters.AddWithValue("@id", request.IdWarehouse);
                    if (cmd.ExecuteScalar() == null)
                        return NotFound("Warehouse not found.");

                    cmd = new SqlCommand("SELECT IdOrder FROM [Order] WHERE IdProduct = @id AND Amount = @amount AND CreatedAt < @createdAt", connection, transaction);
                    cmd.Parameters.AddWithValue("@id", request.IdProduct);
                    cmd.Parameters.AddWithValue("@amount", request.Amount);
                    cmd.Parameters.AddWithValue("@createdAt", request.CreatedAt);
                    var idOrder = cmd.ExecuteScalar();
                    if (idOrder == null)
                        return BadRequest("No matching order found.");

                    cmd = new SqlCommand("SELECT 1 FROM Product_Warehouse WHERE IdOrder = @id", connection, transaction);
                    cmd.Parameters.AddWithValue("@id", (int)idOrder);
                    if (cmd.ExecuteScalar() != null)
                        return BadRequest("Order already fulfilled.");

                    cmd = new SqlCommand("UPDATE [Order] SET FulfilledAt = GETDATE() WHERE IdOrder = @id", connection, transaction);
                    cmd.Parameters.AddWithValue("@id", (int)idOrder);
                    cmd.ExecuteNonQuery();

                    cmd = new SqlCommand("SELECT Price FROM Product WHERE IdProduct = @id", connection, transaction);
                    cmd.Parameters.AddWithValue("@id", request.IdProduct);
                    var price = Convert.ToDecimal(cmd.ExecuteScalar());

                    cmd = new SqlCommand(@"
                        INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)
                        VALUES (@wid, @pid, @oid, @amount, @price, GETDATE());
                        SELECT SCOPE_IDENTITY();", connection, transaction);
                    cmd.Parameters.AddWithValue("@wid", request.IdWarehouse);
                    cmd.Parameters.AddWithValue("@pid", request.IdProduct);
                    cmd.Parameters.AddWithValue("@oid", (int)idOrder);
                    cmd.Parameters.AddWithValue("@amount", request.Amount);
                    cmd.Parameters.AddWithValue("@price", price * request.Amount);

                    var insertedId = Convert.ToInt32(cmd.ExecuteScalar());
                    transaction.Commit();
                    return Ok(insertedId);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }
        }

        [HttpPost("add-procedure")]
        public IActionResult AddProductProcedure(WarehouseRequest request)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    var cmd = new SqlCommand("AddProductToWarehouse", connection);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@IdProduct", request.IdProduct);
                    cmd.Parameters.AddWithValue("@IdWarehouse", request.IdWarehouse);
                    cmd.Parameters.AddWithValue("@Amount", request.Amount);
                    cmd.Parameters.AddWithValue("@CreatedAt", request.CreatedAt);

                    connection.Open();
                    var result = cmd.ExecuteScalar();
                    return Ok(result);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Error executing procedure: {ex.Message}");
                }
            }
        }
    }
}
