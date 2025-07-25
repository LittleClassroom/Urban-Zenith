# Urban Zenith

A command-line interface (CLI) application for managing a restaurant.

## Features

- **Menu Management**: Add, update, remove, and view menu items.
- **Order Management**: Create, list, complete, and cancel orders.
- **Table Management**: Add, remove, and manage restaurant tables.
- **Staff Management**: Add, remove, and manage staff members.
- **Payment Processing**: Process payments for orders.
- **Reporting**: Generate reports on sales and top-selling items.

## Getting Started

Follow these instructions to get a copy of the project up and running on your local machine.

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later.

### Installation & Running

1.  **Clone the repository:**
    ```sh
    git clone https://github.com/your-username/Urban-Zenith.git
    cd Urban-Zenith
    ```

2.  **Build the project:**
    ```sh
    dotnet build
    ```

3.  **Run the application:**
    ```sh
    dotnet run
    ```
    The application will start and prompt you to choose between Text Command Mode and Menu Navigation Mode.

## Project Structure

The project is organized into the following directories:

- **`/Commands`**: Contains the implementation for each CLI command.
- **`/Database`**: Manages the SQLite database connection and initialization.
- **`/Interfaces`**: Defines interfaces for commands (`ICommand`, `IMenuCommand`).
- **`/Models`**: Contains the data models for the application (e.g., `Order`, `MenuItem`).
- **`/Services`**: Contains the business logic for handling operations related to models.
- **`/Utils`**: Contains utility classes, such as for styled input.

## Dependencies

This project relies on the following NuGet packages:

- **Spectre.Console**: For creating beautiful, interactive console applications.
- **System.Data.SQLite**: The ADO.NET provider for SQLite.
- **ConsoleTableExt**: For creating simple console tables (used in some services).

## Commands

The application supports a total of **36** commands to manage different aspects of the restaurant, available in both a direct text-based mode and an interactive menu mode.

### General Commands

- `help`: Displays a list of available commands.
- `exit` / `quit`: Exits the application.

### Menu Commands

- `menu add`: Add a new menu item.
- `menu list [page]`: List all menu items.
- `menu info <id>`: View details of a specific menu item.
- `menu update <id>`: Update an existing menu item.
- `menu remove <id>`: Remove a menu item.

### Order Commands

- `order new <tableId>`: Create a new order for a table.
- `order list [page]`: List all active orders.
- `order complete <orderId>`: Mark an order as completed.
- `order cancel <orderId>`: Cancel an existing order.
- `order additem`: Add items to an order.
- `order viewitems <tableId>`: View all items on a table's active order.
- `order removeitem <orderItemId>`: Remove a specific item from an order.
- `order updateitem <orderItemId> <newQuantity>`: Update the quantity of an item on an order.

### Table Commands

- `table list`: List all tables.
- `table available`: List all available tables.
- `table add`: Add a new table.
- `table remove <id>`: Remove a table by ID.
- `table reset <id>`: Reset a table's status to available.
- `table status <id> <new_status>`: Update a table's status.
- `table assign <tableId> <staffId>`: Assign a staff member to a table.
- `table unassign <tableId>`: Unassign staff from a table.
- `table update <id>`: Update table details.

### Staff Commands

- `staff list`: List all staff members.
- `staff add`: Add a new staff member.
- `staff remove <id>`: Remove a staff member by ID.
- `staff info <id>`: View detailed information for a staff member.
- `staff update <id>`: Update details for a staff member.

### Payment Commands

- `payment process <tableId>`: Process a payment for a table.
- `payment history [page] [pageSize]`: Show payment history.
- `payment info <paymentId>`: Show detailed information about a payment.

### Report Commands

- `report daily [YYYY-MM-DD]`: Show today's or a specific day's sales summary.
- `report method`: Show revenue grouped by payment method.
- `report items`: Show quantity sold per menu item.

## Usage Example

Here is a simple workflow for taking an order and processing a payment:

1.  **Create a new order for Table 5:**
    ```
    order new 5
    ```
2.  **Add items to the new order:**
    ```
    order additem
    > Enter Table ID: 5
    > Enter Menu Item ID: 1
    > Enter quantity: 2
    > Enter Menu Item ID: 3
    > Enter quantity: 1
    > Enter Menu Item ID: done
    ```
3.  **View the items on the table's order:**
    ```
    order viewitems 5
    ```
4.  **Process the payment for Table 5:**
    ```
    payment process 5
    > Enter payment amount: 55.00
    > Select payment method: Card
    > Confirm payment...
    ```

## Database

The application uses a SQLite database to store all data. The database file (`UrbanZenith.db`) is created in the `Database` directory.

The database schema includes the following tables:

- `MenuItems`: Stores information about menu items.
- `Tables`: Stores information about restaurant tables.
- `Orders`: Stores information about customer orders.
- `OrderItems`: Stores the items included in each order.
- `Payments`: Stores payment information for orders.
- `Staff`: Stores information about staff members.

## Contributing

Contributions are welcome! Please feel free to submit a pull request.

1.  Fork the Project
2.  Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3.  Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4.  Push to the Branch (`git push origin feature/AmazingFeature`)
5.  Open a Pull Request

## License

Distributed under the MIT License.