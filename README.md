# APBD-Tutorial9 – ASP.NET Core Web API: Warehouse Management

This project was created as part of the laboratory assignment for the course **Advanced Database Programming (APBD)**.

The goal of the assignment is to implement a REST API that allows interaction with a SQL Server database simulating a warehouse system. The application supports adding products to the warehouse using both inline SQL logic and a stored procedure.

## Functionality

The `WarehouseController` contains two endpoints:

### `POST /api/warehouse/add-manual`

Adds a product to the warehouse using inline SQL commands.

- Verifies the existence of the product and the warehouse.
- Checks if a matching order exists in the `Order` table (same product and amount, earlier creation date).
- Ensures that the order has not yet been fulfilled.
- Updates the `FulfilledAt` field of the order.
- Inserts a new row into the `Product_Warehouse` table.
- Returns the ID of the newly inserted row.

### `POST /api/warehouse/add-procedure`

Adds a product to the warehouse using a stored procedure named `AddProductToWarehouse`.

- Accepts the same input as the manual endpoint.
- Delegates logic to the stored procedure defined in `proc.sql`.

## How to Run

1. Install [.NET SDK 7](https://dotnet.microsoft.com/en-us/download).
2. Create a new SQL Server database.
3. Execute the `create.sql` script to generate tables and test data.
4. In `appsettings.json`, configure the connection string:

```json
"ConnectionStrings": {
  "Default": "Server=localhost;Database=WarehouseDb;Trusted_Connection=True;TrustServerCertificate=True"
}

## Run the application

```bash
dotnet run --project Tutorial9


POST /api/warehouse/add-manual
Content-Type: application/json
{
  "idProduct": 1,
  "idWarehouse": 1,
  "amount": 10,
  "createdAt": "2025-05-10T14:00:00"
}

## Project Structure

Controllers/WarehouseController.cs – API logic
Model/WarehouseRequest.cs – input model
Services/DbService.cs – (optional business logic helper)
create.sql – SQL script to create and populate the database
proc.sql – stored procedure implementation
Validation and Requirements

## The project fulfills the following requirements from the assignment:

Input validation and error handling
Manual SQL logic for inserting warehouse data
Use of a stored procedure for equivalent logic
Proper HTTP status code responses
Modular structure and clean code

## Author

Vladislav Dobriyan
GitHub: Desstori15
