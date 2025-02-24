# e-PayRoll System

## 📌 Project Overview
The **e-PayRoll System** is a **mini payroll management application** designed to handle **employee management, salary calculations, and pay slip generation**. This system consists of:
- **Web UI (ASP.NET MVC)** for managing employees and payroll.
- **REST API (C# .NET Web API)** for backend operations.
- **MS SQL Database** to store employee and payroll data.

---
## 🔥 Features
✅ **Employee Management**
- Add, edit, and delete employees.
- Store details like **Full Name, DOB, Gender, Join Date, Resign Date, Basic Salary, Allowances, and Deductions**.
- Apply **validations** (age check, salary as decimal, resign date logic).
- Display employees in a **sortable and filterable table**.

✅ **Salary Calculation**
- Process salary **based on working days**.
- Auto-calculates **allowances & deductions**.
- **Handles mid-month joiners and resignations.**
- Allows **deletion of processed salaries**.

✅ **Pay Slip Generation**
- **Generate pay slips using RDLC reports.**
- **View & Download** pay slips as **PDF files**.

---
## 🛠️ Tech Stack
### **Frontend (Web UI - ASP.NET MVC)**
- **ASP.NET MVC** for UI
- **Bootstrap** for responsive design
- **jQuery & AJAX** for async operations
- **DataTables** for dynamic employee lists

### **Backend (REST API - C# .NET Web API)**
- **C# .NET Web API**
- **Entity Framework (EF) Core**
- **MS SQL Server** for database

### **Reporting & Documentation**
- **RDLC Reports** for pay slip generation

---
## 📌 Installation Guide
### **1️⃣ Clone the Repository**
```sh
 git clone https://github.com/your-repo/e-payroll.git
 cd e-payroll
```

### **2️⃣ Setup Database (MS SQL Server)**
- Create a **new database** named `ePayRollDB`.
- Run the provided `ePayRollDB.sql` script in **SQL Server Management Studio (SSMS)**.

### **3️⃣ Update Connection Strings**
Modify **`Web.config` (Web UI)** and **`appsettings.json` (API)**:
```xml
<connectionStrings>
    <add name="ePayRollDB" connectionString="Server=YOUR_SERVER;Database=ePayRollDB;User Id=YOUR_USER;Password=YOUR_PASSWORD;" providerName="System.Data.SqlClient" />
</connectionStrings>
```

### **4️⃣ Update API Settings in Web UI**
Ensure the Web UI correctly communicates with the API by updating `Web.config`:
```xml
<appSettings>
    <add key="ApiBaseUrl" value="https://localhost:44351/api/" />
</appSettings>
```
✅ **Now, the Web UI will send requests to the API using this base URL.**

### **5️⃣ Set API & Web as Startup Projects**
To run both Web UI & API together in Visual Studio:
1. **Right-click** on the solution (`ePayRoll.sln`).
2. Click **"Set Startup Projects..."**.
3. Select **"Multiple startup projects"**.
4. For **ePayRoll.API**, choose **Start**.
5. For **ePayRoll.Web**, choose **Start**.
6. Click **OK**, then **Run (F5)**.
✅ **Now, both projects will start automatically.**



---
## 📌 API Endpoints
### **Employee Management**
| Method | Endpoint | Description |
|--------|---------|-------------|
| `GET`  | `/api/employee/list` | Get all employees |
| `GET`  | `/api/employee/{id}` | Get employee by ID |
| `POST` | `/api/employee/create` | Add new employee |
| `PUT`  | `/api/employee/update/{id}` | Update employee |
| `DELETE` | `/api/employee/delete/{id}` | Delete employee |

### **Salary Processing**
| Method | Endpoint | Description |
|--------|---------|-------------|
| `GET`  | `/api/payroll/valid/{monthYear}` | List employees eligible for salary processing |
| `POST` | `/api/payroll/calculate` | Process salaries |
| `POST` | `/api/payroll/delete/{id}` | Delete processed salary |
| `GET` | `/api/payroll/processed/{year}/{month}` | Get processed Salaries list for PaySlips |
| `GET` | `/api/payroll/get-payroll/{payrollId}` | Get processed Salary detail for PaySlip |

---
## 📌 Usage Instructions
### **1️⃣ Employee Management**
- Navigate to **Employee List**.
- Click `+ Add New Employee` to create an employee.
- Edit or Delete employees.
- Can search by Name, DOB and Status.

- - Navigate to **Employee Detail**.
- Save or Delete employees.
- Save Allowances and Deductions.

### **2️⃣ Salary Processing**
- Go to **Payroll Section**.
- Select a month
- Select employees & click `Process Salary`.
- Can View or delete processed salaries.

### **3️⃣ Generate & Download Pay Slips**
- Go to **Pay Slip Section**.
- Select a month & employee, then click `View Pay Slip`.
- Click `Download PDF` to save the pay slip.


---
## 📩 Contact
Can reach out to: **nwayooswe.1004@gmial.com**


