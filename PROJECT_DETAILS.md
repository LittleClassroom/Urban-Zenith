# 🍽️ Urban Zenith: Project Details

## Project Overview

**Urban Zenith** is a comprehensive, text-based (CLI) restaurant management system built with **C#** and **.NET 9.0**. It's designed to be a lightweight, powerful tool for managing all core aspects of a restaurant's operations—from table assignments to financial reports—directly from a command line, without needing a graphical user interface (GUI).

The application uses a local **SQLite** database, making it self-contained and easy to run on any system with the .NET runtime.

---

## Core Features

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

## Technical Architecture

- **Language & Framework:** C# on .NET 9.0.
- **User Interface:** The app features a rich CLI built with the **Spectre.Console** library, providing formatted tables, color-coded text, rules, and interactive prompts.
- **Database:** It uses **SQLite** for local, file-based data storage. The database schema is automatically created and initialized on the first run.
- **Design Patterns:**
    - **Command Pattern:** Each primary action (e.g., `table`, `order`) is implemented as a command that conforms to an `ICommand` interface. This makes the system modular and easy to extend.
    - **Service Layer:** Business logic is cleanly separated from the UI and database. For example, `OrderService` contains the logic for managing orders, while `OrderCommand` handles the user input.
- **Dual-Mode Operation:** The application can be used in two ways:
    1.  **Text Command Mode:** For power users who prefer typing commands (e.g., `order new 5`).
    2.  **Menu Navigation Mode:** A user-friendly, menu-driven interface for navigating through options with number inputs.

---

## Application Workflow

The application follows a clear, structured workflow from launch to command execution.

1.  **Entry Point (`Program.cs`)**
    - The application starts in the `Main` method.
    - It first displays a welcome banner ("Urban Zenith").
    - **Database Initialization:** It calls `DatabaseContext.Initialize()`, which checks for the `UrbanZenith.db` file. If the file doesn't exist, it's created, and the necessary tables (`MenuItems`, `Orders`, etc.) are set up.
    - **Command Registration:** The `RegisterCommands()` method is called to instantiate all available command classes (e.g., `OrderCommand`, `TableCommand`) and store them in a dictionary for quick access.

2.  **Mode Selection**
    - The user is prompted to choose between two operational modes:
        - **Text Command Mode:** For direct command input.
        - **Menu Navigation Mode:** For a guided, menu-based experience.

3.  **Command Handling (Text Mode)**
    - The application enters a loop, waiting for user input.
    - When a user types a command (e.g., `order new 5`):
        - The input is split into the main command (`order`) and its arguments (`new 5`).
        - The application looks up "order" in the commands dictionary to find the `OrderCommand` object.
        - It then calls the `Execute("new 5")` method on that object.

4.  **Command Handling (Menu Mode)**
    - The application displays a numbered list of all available commands.
    - When a user selects a number (e.g., corresponding to "order"):
        - The application retrieves the `OrderCommand` object.
        - It calls the `ShowMenu()` method, which displays a secondary menu with order-specific actions (e.g., "1. Create new order").
        - The user makes a selection, and the application prompts for any required details (like a table ID).

5.  **Execution Flow (Example: Creating an Order)**
    - **`OrderCommand`:** Whether in text or menu mode, the command class receives the user's intent. It parses the arguments or menu choice and determines that it needs to create a new order.
    - **`OrderService`:** The `OrderCommand` delegates the actual work to the `OrderService`. It calls a method like `OrderService.CreateNewOrder(5)`.
    - **Business Logic:** The `OrderService` contains the core logic. It checks if the table is available, creates the new order, and updates the table's status to "Occupied."
    - **`DatabaseContext`:** The `OrderService` uses the `DatabaseContext` to execute the necessary SQL queries against the database (e.g., `INSERT` into `Orders`, `UPDATE` `Tables`). All database interactions are centralized here for consistency and safety.
    - **Feedback:** The result of the operation (success or failure) is passed back up the chain and displayed to the user in a clean, formatted way using **Spectre.Console**.

This layered architecture ensures a clean separation of concerns, making the application robust, maintainable, and easy to extend with new features.

---

## Database Schema

The application relies on a simple yet effective relational database schema:

- `MenuItems`: Stores food and drink items with their price and description.
- `Tables`: Manages physical tables, their type, status, and assigned staff.
- `Staff`: Contains records for employees, including their role and credentials.
- `Orders`: Tracks customer orders, linking them to a table and status.
- `OrderItems`: A junction table detailing which menu items are in which order.
- `Payments`: Records all financial transactions, linked to an order.