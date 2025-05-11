# APBD-Tutorial9 – ASP.NET Core Web API: Warehouse Management

This project was created as part of the laboratory assignment for the course **Advanced Database Programming (APBD)**.

The goal is to implement a REST API for managing warehouse stock using both inline SQL logic and a stored procedure.

## Functionality

The `WarehouseController` contains two endpoints:

### `POST /api/warehouse/add-manual`

Adds a product to the warehouse using inline SQL logic.

- Verifies the existence of the product and warehouse
- Checks if a matching order exists in the `Order` table
- Ensures the order has not been fulfilled yet
- Updates the `FulfilledAt` field
- Inserts a record into the `Product_Warehouse` table
- Returns the ID of the inserted record

### `POST /api/warehouse/add-procedure`

Same logic as above, implemented using the stored procedure `AddProductToWarehouse`.

## How to Run

1. Install [.NET 7 SDK](https://dotnet.microsoft.com/en-us/download)
2. Create a new database in SQL Server
3. Run `create.sql` to initialize the schema and test data
4. Configure the connection string in `appsettings.json`:

```json
"ConnectionStrings": {
  "Default": "Server=localhost;Database=WarehouseDb;Trusted_Connection=True;TrustServerCertificate=True"
}
Run the application:
dotnet run --project Tutorial9
Sample Request

POST /api/warehouse/add-manual
Content-Type: application/json
{
  "idProduct": 1,
  "idWarehouse": 1,
  "amount": 10,
  "createdAt": "2025-05-10T14:00:00"
}
Project Structure

Controllers/WarehouseController.cs – API logic
Model/WarehouseRequest.cs – input model
Services/DbService.cs – optional business logic helper
create.sql – database structure and test data
proc.sql – stored procedure definition
The project fulfills the following requirements

Input validation and error handling
Manual SQL logic for inserting warehouse data
Use of a stored procedure
Proper HTTP status code responses
Clean and modular code structure
Author

Vladislav Dobriyan
GitHub: Desstori15
