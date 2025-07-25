# 🍽️ Urban Zenith: Restaurant Management CLI

**Urban Zenith** is a comprehensive, text-based (CLI) restaurant management system built with **C#** and **.NET 9.0**. It's designed to be a lightweight, powerful tool for managing all core aspects of a restaurant's operations—from table assignments to financial reports—directly from a command line, without needing a graphical user interface (GUI).

The application uses a local **SQLite** database, making it self-contained and easy to run on any system with the .NET runtime.

---

## 🚀 Core Features

The application is broken down into several key modules:

**1. Table Management**
- **Functionality:** Allows staff to manage restaurant tables.
- **Actions:**
    - List all or only available tables.
    - Add, update, and remove tables.
    - Assign a staff member to a table.
    - Update a table's status (`Available`, `Occupied`, `Broken`).
    - Reset a table to its default state.

**2. Staff Management**
- **Functionality:** Manages employee records and credentials.
- **Actions:**
    - Add, update, and remove staff members.
    - List all staff.
    - View detailed information for a specific staff member.
    - Includes fields for `Name`, `Role`, and a unique `Username`.

**3. Order Management**
- **Functionality:** Handles the entire lifecycle of a customer's order.
- **Actions:**
    - Create a new order and link it to a table.
    - Add, update, or remove items from an active order.
    - View all items on a specific table's active order.
    - Mark orders as `Completed` or `Cancelled`.
    - List all orders with pagination.

**4. Payment System**
- **Functionality:** Processes customer payments and tracks transactions.
- **Actions:**
    - Process a payment for a table's active order.
    - Supports multiple payment methods: `Cash`, `Card`, `QR`, `E-wallet`.
    - Calculates the total due and the change to be returned.
    - View a paginated history of all payments.
    - Look up detailed information for a specific payment.

**5. Reporting**
- **Functionality:** Generates key business insights and financial reports.
- **Reports Available:**
    - **Daily Sales Report:** Shows total revenue and number of payments for today or a specified date.
    - **Sales by Payment Method:** Breaks down revenue by how customers paid.
    - **Top-Selling Items:** Lists menu items by quantity sold and total revenue generated.

---

## 🛠️ Technical Stack

| Component          | Technology                               |
| ------------------ | ---------------------------------------- |
| **Language**       | C#                                       |
| **Framework**      | .NET 9.0                                 |
| **Database**       | SQLite                                   |
| **UI**             | Spectre.Console (for rich CLI)           |
| **Data Display**   | ConsoleTableExt (for formatted tables)   |

---

## 🏛️ Architecture

- **Command Pattern:** Each primary action (e.g., `table`, `order`) is implemented as a command that conforms to an `ICommand` interface. This makes the system modular and easy to extend.
- **Service Layer:** Business logic is cleanly separated from the UI and database. For example, `OrderService` contains the logic for managing orders, while `OrderCommand` handles the user input.
- **Dual-Mode Operation:** The application can be used in two ways:
    1.  **Text Command Mode:** For power users who prefer typing commands (e.g., `order new 5`).
    2.  **Menu Navigation Mode:** A user-friendly, menu-driven interface for navigating through options with number inputs.

---

## 🗃️ Database Schema

The application relies on a simple yet effective relational database schema:

- `MenuItems`: Stores food and drink items with their price and description.
- `Tables`: Manages physical tables, their type, status, and assigned staff.
- `Staff`: Contains records for employees, including their role and credentials.
- `Orders`: Tracks customer orders, linking them to a table and status.
- `OrderItems`: A junction table detailing which menu items are in which order.
- `Payments`: Records all financial transactions, linked to an order.

---

## 🏁 Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

### 1. Clone the Repository

```bash
git clone https://github.com/your-username/Urban-Zenith.git
cd Urban-Zenith
```

### 2. Build the Project

```bash
dotnet build
```

### 3. Run the Application

```bash
dotnet run
```

The application will start and prompt you to choose between **Text Command Mode** and **Menu Navigation Mode**.

---

## ⌨️ CLI Commands

Here’s a quick reference of all supported CLI commands:

### 📋 Table Commands

| Command                            | Description                             |
| ---------------------------------- | --------------------------------------- |
| `table list`                       | List all tables                         |
| `table available`                  | List available tables                   |
| `table add`                        | Add a new table                         |
| `table remove <id>`                | Remove a table                          |
| `table reset <id>`                 | Reset table (clear assignments/orders)  |
| `table status <id> <status>`       | Set status: Available, Occupied, Broken |
| `table assign <tableId> <staffId>` | Assign staff to table                   |
| `table unassign <tableId>`         | Unassign staff                          |
| `table update <id>`                | Update table information                |

### 👨‍🍳 Staff Commands

| Command             | Description            |
| ------------------- | ---------------------- |
| `staff list`        | List all staff         |
| `staff add`         | Add a new staff member |
| `staff remove <id>` | Remove a staff member  |
| `staff info <id>`   | View staff details     |
| `staff update <id>` | Update staff info      |

### 🧾 Order Commands

| Command                                | Description									|
| -------------------------------------- | -------------------------------------------- |
| `order new <tableId>`                  | Create a new order							|
| `order list`                           | List order < default page 1 >							|
| `order list <pageNum>`                 | List order for specific Paga   |
| `order list <pageNum> <pageSize>`      | List order with parameter Page number and page size |
| `order complete <orderId>`             | Mark order as completed						|
| `order additem`                        | Add items to active order by table			|
| `order viewitems <tableId>`            | View items for a table’s current order		|
| `order removeitem <orderItemId>`       | Remove an item from an order					|
| `order updateitem <orderItemId> <qty>` | Update item quantity							|
| `order cancel <orderId>`				 | Cancel Specific Order that currently active	|

### 💵 Payment Commands

| Command                             | Description                       |
| ----------------------------------- | --------------------------------- |
| `payment process <tableId>`         | Process payment for table's order |
| `payment history`                   | Show latest 10 payments           |
| `payment history <page>`            | Show payments by page             |
| `payment history <page> <pageSize>` | Paginated payment history         |
| `payment info <paymentId>`          | View detailed payment info        |

### 📊 Report Commands

| Command                     | Description                           |
| --------------------------- | ------------------------------------- |
| `report daily`              | Today’s sales summary                 |
| `report daily <YYYY-MM-DD>` | Summary for specific date             |
| `report method`             | Revenue by payment method             |
| `report items`              | Top-selling menu items (quantity/rev) |

---

## 🗺️ Roadmap

- [ ] Authentication system for staff login
- [ ] Role-based access control
- [ ] Export reports as PDF/CSV
- [ ] Cross-platform testing (Linux/Mac)

---

## 🧑‍💻 Author

**Meng Seang (Twilight)**
- Full-stack Developer & Graphic Designer
- [Portfolio](https://mengseang.netlify.app)

---

## 📄 License

This project is licensed under the MIT License.