Here's a complete `README.md` for your **UrbanZenith** CLI-based restaurant management system, formatted in Markdown and ready for GitHub:

---

# 🍽️ UrbanZenith

**UrbanZenith** is a modular, CLI-based restaurant management system built in **C#**. Designed for local restaurants, cafés, or food courts, it handles orders, payments, reports, and table assignments with ease — all without requiring a graphical UI or external dependencies.

---

## 🚀 Features

### ✅ Table Management
- List, add, update, remove, reset tables
- Assign/unassign tables to staff
- Track table status (Available, Occupied, Broken)

### 🧑‍🍳 Staff Management
- Add, list, update, and remove staff
- Staff roles and login credentials supported
- Unique usernames per staff

### 🧾 Order Management
- Create new orders per table
- Add/remove/update items to/from orders
- View active items for each table
- Supports multiple items and quantities per order
- Cancel Order ( New )

### 💵 Payment System
- Process payments per table
- View payment history and details
- Supports various methods: Cash, Card, QR, E-wallet
- Tracks paid amounts and timestamps

### 📊 Reports
- Daily Sales Report (today or custom date)
- Revenue grouped by payment method
- Top-selling menu items (with quantity and revenue)
- All reports support formatted CLI output

---

## 🗃️ Database Schema

Using **SQLite**, UrbanZenith includes the following tables:

```sql
MenuItems(Id, Name, Description, Price)
Tables(Id, Name, Type, Status, StaffId)
Orders(Id, TableId, OrderDate, Status)
OrderItems(Id, OrderId, MenuItemId, Quantity, Price)
Payments(Id, OrderId, PaymentMethod, PaidAmount, PaidAt)
Staff(Id, Name, Role, Username, Password)
````

Great call — here's how to enhance your `README.md` with:

---

## 📦 NuGet Packages Used

UrbanZenith uses the following core packages:

| Package              | Purpose                               |
| -------------------- | ------------------------------------- |
| `System.Data.SQLite` | SQLite database access                |
| `ConsoleTableExt`    | Pretty table formatting in CLI output |


Install via .NET CLI:

```bash
dotnet add package System.Data.SQLite
dotnet add package ConsoleTableExt
```

---

## 🔧 CLI Command List

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
| `order list`                           | List all orders								|
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


---

## 🧩 Architecture

* **Command Pattern** for CLI commands (`table`, `order`, `report`, etc.)
* **Service Layer** for business logic (`OrderService`, `ReportService`, etc.)
* **Interfaces** like `ICommand`, `IMenuCommand` ensure consistency
* **SQLite** database with safe parameterized queries
* **ConsoleTableExt** used for table-style outputs

---

## 💻 How to Run

### 1. Clone the Repository

```bash
git clone https://github.com/LittleClassroom/UrbanZenith.git
cd UrbanZenith
```

### 2. Build the Project

Using .NET CLI:

```bash
dotnet build
```

### 3. Run the CLI App

```bash
dotnet run
```

> 🛠 Make sure SQLite is available and the app has permission to create/write to `urbanzenith.db`.

---

## 📅 Roadmap

* [ ] Authentication system for staff login
* [ ] Role-based access control
* [ ] Export reports as PDF/CSV
* [ ] Offline desktop version via Tauri or Electron
* [ ] Cross-platform testing (Linux/Mac)

---

## 📸 Screenshots

> Coming soon — CLI UI previews

---

## 🧑‍💻 Author

**Meng Seang (Twilight)**
📎 Full-stack Developer & Graphic Designer
🌐 [Portfolio](https://mengseang.netlify.app)

---

## 📄 License

This project is licensed under the MIT License.


