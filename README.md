
# Urban Zenith

**Urban Zenith** is a modern command-line interface (CLI) application for managing restaurant operations — from menu and table management to orders, staff, payments, and reports. It supports both **Text Command Mode** and **Menu Navigation Mode**, built using C# and SQLite.

![.NET](https://img.shields.io/badge/.NET-8.0-blueviolet)
![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20Linux%20%7C%20macOS-informational)
![License](https://img.shields.io/github/license/LittleClassroom/Urban-Zenith)
![Contributions Welcome](https://img.shields.io/badge/contributions-welcome-brightgreen)
![Last Commit](https://img.shields.io/github/last-commit/LittleClassroom/Urban-Zenith)

---

## 🚀 Features

- **Menu Management**: Add, update, remove, and view menu items.
- **Order Management**: Create, list, complete, and cancel orders.
- **Table Management**: Add, remove, and manage restaurant tables.
- **Staff Management**: Add, remove, and manage staff members.
- **Payment Processing**: Process payments for orders.
- **Reporting**: Generate reports on sales and top-selling items.

---

## ⚙️ Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later

### Installation & Running

1. **Clone the repository:**
    ```sh
    git clone https://github.com/LittleClassroom/Urban-Zenith.git
    cd Urban-Zenith
    ```

2. **Build the project:**
    ```sh
    dotnet build
    ```

3. **Run the application:**
    ```sh
    dotnet run
    ```
    The app will prompt you to choose between **Text Command Mode** and **Menu Navigation Mode**.

---

## 🧱 Project Structure

- `/Commands`: CLI command implementations
- `/Database`: SQLite database and helper classes
- `/Interfaces`: Core interfaces (`ICommand`, `IMenuCommand`)
- `/Models`: Data models (e.g., `Order`, `MenuItem`, `Staff`)
- `/Services`: Business logic for features
- `/Utils`: Console input/output helpers, validation, etc.

---

## 📦 Dependencies

This project uses the following NuGet packages:

- `Spectre.Console` – For building rich console UIs
- `System.Data.SQLite` – ADO.NET provider for SQLite
- `ConsoleTableExt` – Console table formatter

---

## 💻 Commands Overview

The application supports **39 commands** to manage the restaurant in both modes.

### General
- `help`
- `exit` / `quit`

### Menu
- `menu add`
- `menu list [page]`
- `menu info <id>`
- `menu update <id>`
- `menu remove <id>`

### Orders
- `order new <tableId>`
- `order list [page]`
- `order complete <orderId>`
- `order cancel <orderId>`
- `order additem`
- `order viewitems <tableId>`
- `order removeitem <orderItemId>`
- `order updateitem <orderItemId> <newQuantity>`

### Tables
- `table list`
- `table available`
- `table add`
- `table remove <id>`
- `table reset <id>`
- `table status <id> <new_status>`
- `table assign <tableId> <staffId>`
- `table unassign <tableId>`
- `table update <id>`

### Staff
- `staff list`
- `staff add`
- `staff remove <id>`
- `staff info <id>`
- `staff update <id>`

### Payments
- `payment process <tableId>`
- `payment history [page] [pageSize]`
- `payment info <paymentId>`

### Reports
- `report daily [YYYY-MM-DD]`
- `report method`
- `report items`

---

## 🧪 Usage Example

```bash
# Create a new order for Table 5
order new 5

# Add items to the order
order additem
> Enter Table ID: 5
> Enter Menu Item ID: 1
> Enter quantity: 2
> Enter Menu Item ID: 3
> Enter quantity: 1
> Enter Menu Item ID: done

# View current order items
order viewitems 5

# Process the payment
payment process 5
> Enter payment amount: 55.00
> Select payment method: Card
> Confirm payment...
````

---

## 🗄 Database

The database file is located at `Database/UrbanZenith.db`. It includes:

* `MenuItems`
* `Tables`
* `Orders`
* `OrderItems`
* `Payments`
* `Staff`

---

## 🤝 Contributing

Contributions are welcome!

1. Fork the repository
2. Create a new branch:

   ```sh
   git checkout -b feature/YourFeature
   ```
3. Commit your changes:

   ```sh
   git commit -m "Add YourFeature"
   ```
4. Push and open a pull request:

   ```sh
   git push origin feature/YourFeature
   ```

---

## 📄 License

Distributed under the **MIT License**. See the [`LICENSE`](LICENSE) file for details.


