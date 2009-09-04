#region Auto-generated classes for Northwind database on 2008-10-27 20:16:39Z

//
//  ____  _     __  __      _        _
// |  _ \| |__ |  \/  | ___| |_ __ _| |
// | | | | '_ \| |\/| |/ _ \ __/ _` | |
// | |_| | |_) | |  | |  __/ || (_| | |
// |____/|_.__/|_|  |_|\___|\__\__,_|_|
//
// Auto-generated from Northwind on 2008-10-27 20:16:39Z
// Please visit http://linq.to/db for more information

#endregion

using System;
using System.Data;
using System.Data.Linq.Mapping;
using System.Diagnostics;
using System.Reflection;
using DbLinq.Data.Linq;
using DbLinq.Vendor;

namespace nwind
{
	public partial class Northwind : DataContext
	{
		public Northwind(IDbConnection connection)
		: base(connection, new DbLinq.MySql.MySqlVendor())
		{
		}

		public Northwind(IDbConnection connection, IVendor vendor)
		: base(connection, vendor)
		{
		}

		public Table<Category> Categories { get { return GetTable<Category>(); } }
		public Table<Customer> Customers { get { return GetTable<Customer>(); } }
		public Table<Employee> Employees { get { return GetTable<Employee>(); } }
		public Table<EmployeeTerritory> EmployeeTerritories { get { return GetTable<EmployeeTerritory>(); } }
		public Table<Order> Orders { get { return GetTable<Order>(); } }
		public Table<OrderDetail> OrderDetails { get { return GetTable<OrderDetail>(); } }
		public Table<Product> Products { get { return GetTable<Product>(); } }
		public Table<Region> Regions { get { return GetTable<Region>(); } }
		public Table<Shipper> Shippers { get { return GetTable<Shipper>(); } }
		public Table<Supplier> Suppliers { get { return GetTable<Supplier>(); } }
		public Table<Territory> Territories { get { return GetTable<Territory>(); } }

		[Function(Name = "northwind.getOrderCount", IsComposable = true)]
		public int GetOrderCount([Parameter(Name = "custId", DbType = "VARCHAR(5)")] string custId)
		{
			var result = ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod(), custId);
			return (int)result.ReturnValue;
		}

		[Function(Name = "northwind.hello0", IsComposable = true)]
		public string Hello0()
		{
			var result = ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod());
			return (string)result.ReturnValue;
		}

		[Function(Name = "northwind.hello1", IsComposable = true)]
		public string Hello1([Parameter(Name = "s", DbType = "CHAR(20)")] string s)
		{
			var result = ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod(), s);
			return (string)result.ReturnValue;
		}

		[Function(Name = "northwind.hello2", IsComposable = true)]
		public string Hello2([Parameter(Name = "s", DbType = "CHAR(20)")] string s, [Parameter(Name = "s2", DbType = "int")] int s2)
		{
			var result = ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod(), s, s2);
			return (string)result.ReturnValue;
		}

		[Function(Name = "northwind.sp_selOrders", IsComposable = false)]
		public DataSet SpSelOrders([Parameter(Name = "s", DbType = "CHAR(20)")] string s, [Parameter(Name = "s2", DbType = "int")] out int s2)
		{
			var result = ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod(), s);
			s2 = (System.Int32)result.GetParameterValue(1);
			return (DataSet)result.ReturnValue;
		}

		[Function(Name = "northwind.sp_updOrders", IsComposable = false)]
		public void SpUpdOrders([Parameter(Name = "custID", DbType = "int")] int custID, [Parameter(Name = "prodId", DbType = "int")] int prodId)
		{
			var result = ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod(), custID, prodId);
		}

	}

	[Table(Name = "northwind.categories")]
	public partial class Category
	{
		#region int CategoryID

		private int _categoryID;
		[DebuggerNonUserCode]
		[Column(Storage = "_categoryID", Name = "CategoryID", DbType = "int", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false)]
		public int CategoryID
		{
			get
			{
				return _categoryID;
			}
			set
			{
				if (value != _categoryID)
				{
					_categoryID = value;
				}
			}
		}

		#endregion

		#region string CategoryName

		private string _categoryName;
		[DebuggerNonUserCode]
		[Column(Storage = "_categoryName", Name = "CategoryName", DbType = "varchar(15)", CanBeNull = false)]
		public string CategoryName
		{
			get
			{
				return _categoryName;
			}
			set
			{
				if (value != _categoryName)
				{
					_categoryName = value;
				}
			}
		}

		#endregion

		#region string Description

		private string _description;
		[DebuggerNonUserCode]
		[Column(Storage = "_description", Name = "Description", DbType = "text")]
		public string Description
		{
			get
			{
				return _description;
			}
			set
			{
				if (value != _description)
				{
					_description = value;
				}
			}
		}

		#endregion

		#region Byte[] Picture

		private Byte[] _picture;
		[DebuggerNonUserCode]
		[Column(Storage = "_picture", Name = "Picture", DbType = "blob")]
		public Byte[] Picture
		{
			get
			{
				return _picture;
			}
			set
			{
				if (value != _picture)
				{
					_picture = value;
				}
			}
		}

		#endregion

		#region Children

		private EntitySet<Product> _products;
		[Association(Storage = "_products", OtherKey = "CategoryID", Name = "products_ibfk_1")]
		[DebuggerNonUserCode]
		public EntitySet<Product> Products
		{
			get
			{
				return _products;
			}
			set
			{
				_products = value;
			}
		}


		#endregion

		#region Attachement handlers

		private void Products_Attach(Product entity)
		{
			entity.Category = this;
		}

		private void Products_Detach(Product entity)
		{
			entity.Category = null;
		}


		#endregion

		#region ctor

		public Category()
		{
			_products = new EntitySet<Product>(Products_Attach, Products_Detach);
		}

		#endregion

	}

	[Table(Name = "northwind.customers")]
	public partial class Customer
	{
		#region string Address

		private string _address;
		[DebuggerNonUserCode]
		[Column(Storage = "_address", Name = "Address", DbType = "varchar(60)")]
		public string Address
		{
			get
			{
				return _address;
			}
			set
			{
				if (value != _address)
				{
					_address = value;
				}
			}
		}

		#endregion

		#region string City

		private string _city;
		[DebuggerNonUserCode]
		[Column(Storage = "_city", Name = "City", DbType = "varchar(15)")]
		public string City
		{
			get
			{
				return _city;
			}
			set
			{
				if (value != _city)
				{
					_city = value;
				}
			}
		}

		#endregion

		#region string CompanyName

		private string _companyName;
		[DebuggerNonUserCode]
		[Column(Storage = "_companyName", Name = "CompanyName", DbType = "varchar(40)", CanBeNull = false)]
		public string CompanyName
		{
			get
			{
				return _companyName;
			}
			set
			{
				if (value != _companyName)
				{
					_companyName = value;
				}
			}
		}

		#endregion

		#region string ContactName

		private string _contactName;
		[DebuggerNonUserCode]
		[Column(Storage = "_contactName", Name = "ContactName", DbType = "varchar(30)")]
		public string ContactName
		{
			get
			{
				return _contactName;
			}
			set
			{
				if (value != _contactName)
				{
					_contactName = value;
				}
			}
		}

		#endregion

		#region string ContactTitle

		private string _contactTitle;
		[DebuggerNonUserCode]
		[Column(Storage = "_contactTitle", Name = "ContactTitle", DbType = "varchar(30)")]
		public string ContactTitle
		{
			get
			{
				return _contactTitle;
			}
			set
			{
				if (value != _contactTitle)
				{
					_contactTitle = value;
				}
			}
		}

		#endregion

		#region string Country

		private string _country;
		[DebuggerNonUserCode]
		[Column(Storage = "_country", Name = "Country", DbType = "varchar(15)")]
		public string Country
		{
			get
			{
				return _country;
			}
			set
			{
				if (value != _country)
				{
					_country = value;
				}
			}
		}

		#endregion

		#region string CustomerID

		private string _customerID;
		[DebuggerNonUserCode]
		[Column(Storage = "_customerID", Name = "CustomerID", DbType = "varchar(5)", IsPrimaryKey = true, CanBeNull = false)]
		public string CustomerID
		{
			get
			{
				return _customerID;
			}
			set
			{
				if (value != _customerID)
				{
					_customerID = value;
				}
			}
		}

		#endregion

		#region string Fax

		private string _fax;
		[DebuggerNonUserCode]
		[Column(Storage = "_fax", Name = "Fax", DbType = "varchar(24)")]
		public string Fax
		{
			get
			{
				return _fax;
			}
			set
			{
				if (value != _fax)
				{
					_fax = value;
				}
			}
		}

		#endregion

		#region string Phone

		private string _phone;
		[DebuggerNonUserCode]
		[Column(Storage = "_phone", Name = "Phone", DbType = "varchar(24)")]
		public string Phone
		{
			get
			{
				return _phone;
			}
			set
			{
				if (value != _phone)
				{
					_phone = value;
				}
			}
		}

		#endregion

		#region string PostalCode

		private string _postalCode;
		[DebuggerNonUserCode]
		[Column(Storage = "_postalCode", Name = "PostalCode", DbType = "varchar(10)")]
		public string PostalCode
		{
			get
			{
				return _postalCode;
			}
			set
			{
				if (value != _postalCode)
				{
					_postalCode = value;
				}
			}
		}

		#endregion

		#region string Region

		private string _region;
		[DebuggerNonUserCode]
		[Column(Storage = "_region", Name = "Region", DbType = "varchar(15)")]
		public string Region
		{
			get
			{
				return _region;
			}
			set
			{
				if (value != _region)
				{
					_region = value;
				}
			}
		}

		#endregion

		#region Children

		private EntitySet<Order> _orders;
		[Association(Storage = "_orders", OtherKey = "CustomerID", Name = "orders_ibfk_1")]
		[DebuggerNonUserCode]
		public EntitySet<Order> Orders
		{
			get
			{
				return _orders;
			}
			set
			{
				_orders = value;
			}
		}


		#endregion

		#region Attachement handlers

		private void Orders_Attach(Order entity)
		{
			entity.Customer = this;
		}

		private void Orders_Detach(Order entity)
		{
			entity.Customer = null;
		}


		#endregion

		#region ctor

		public Customer()
		{
			_orders = new EntitySet<Order>(Orders_Attach, Orders_Detach);
		}

		#endregion

	}

	[Table(Name = "northwind.employees")]
	public partial class Employee
	{
		#region string Address

		private string _address;
		[DebuggerNonUserCode]
		[Column(Storage = "_address", Name = "Address", DbType = "varchar(60)")]
		public string Address
		{
			get
			{
				return _address;
			}
			set
			{
				if (value != _address)
				{
					_address = value;
				}
			}
		}

		#endregion

		#region DateTime? BirthDate

		private DateTime? _birthDate;
		[DebuggerNonUserCode]
		[Column(Storage = "_birthDate", Name = "BirthDate", DbType = "datetime")]
		public DateTime? BirthDate
		{
			get
			{
				return _birthDate;
			}
			set
			{
				if (value != _birthDate)
				{
					_birthDate = value;
				}
			}
		}

		#endregion

		#region string City

		private string _city;
		[DebuggerNonUserCode]
		[Column(Storage = "_city", Name = "City", DbType = "varchar(15)")]
		public string City
		{
			get
			{
				return _city;
			}
			set
			{
				if (value != _city)
				{
					_city = value;
				}
			}
		}

		#endregion

		#region string Country

		private string _country;
		[DebuggerNonUserCode]
		[Column(Storage = "_country", Name = "Country", DbType = "varchar(15)")]
		public string Country
		{
			get
			{
				return _country;
			}
			set
			{
				if (value != _country)
				{
					_country = value;
				}
			}
		}

		#endregion

		#region int EmployeeID

		private int _employeeID;
		[DebuggerNonUserCode]
		[Column(Storage = "_employeeID", Name = "EmployeeID", DbType = "int", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false)]
		public int EmployeeID
		{
			get
			{
				return _employeeID;
			}
			set
			{
				if (value != _employeeID)
				{
					_employeeID = value;
				}
			}
		}

		#endregion

		#region string Extension

		private string _extension;
		[DebuggerNonUserCode]
		[Column(Storage = "_extension", Name = "Extension", DbType = "varchar(5)")]
		public string Extension
		{
			get
			{
				return _extension;
			}
			set
			{
				if (value != _extension)
				{
					_extension = value;
				}
			}
		}

		#endregion

		#region string FirstName

		private string _firstName;
		[DebuggerNonUserCode]
		[Column(Storage = "_firstName", Name = "FirstName", DbType = "varchar(10)", CanBeNull = false)]
		public string FirstName
		{
			get
			{
				return _firstName;
			}
			set
			{
				if (value != _firstName)
				{
					_firstName = value;
				}
			}
		}

		#endregion

		#region DateTime? HireDate

		private DateTime? _hireDate;
		[DebuggerNonUserCode]
		[Column(Storage = "_hireDate", Name = "HireDate", DbType = "datetime")]
		public DateTime? HireDate
		{
			get
			{
				return _hireDate;
			}
			set
			{
				if (value != _hireDate)
				{
					_hireDate = value;
				}
			}
		}

		#endregion

		#region string HomePhone

		private string _homePhone;
		[DebuggerNonUserCode]
		[Column(Storage = "_homePhone", Name = "HomePhone", DbType = "varchar(24)")]
		public string HomePhone
		{
			get
			{
				return _homePhone;
			}
			set
			{
				if (value != _homePhone)
				{
					_homePhone = value;
				}
			}
		}

		#endregion

		#region string LastName

		private string _lastName;
		[DebuggerNonUserCode]
		[Column(Storage = "_lastName", Name = "LastName", DbType = "varchar(20)", CanBeNull = false)]
		public string LastName
		{
			get
			{
				return _lastName;
			}
			set
			{
				if (value != _lastName)
				{
					_lastName = value;
				}
			}
		}

		#endregion

		#region string Notes

		private string _notes;
		[DebuggerNonUserCode]
		[Column(Storage = "_notes", Name = "Notes", DbType = "text")]
		public string Notes
		{
			get
			{
				return _notes;
			}
			set
			{
				if (value != _notes)
				{
					_notes = value;
				}
			}
		}

		#endregion

		#region Byte[] Photo

		private Byte[] _photo;
		[DebuggerNonUserCode]
		[Column(Storage = "_photo", Name = "Photo", DbType = "blob")]
		public Byte[] Photo
		{
			get
			{
				return _photo;
			}
			set
			{
				if (value != _photo)
				{
					_photo = value;
				}
			}
		}

		#endregion

		#region string PhotoPath

		private string _photoPath;
		[DebuggerNonUserCode]
		[Column(Storage = "_photoPath", Name = "PhotoPath", DbType = "varchar(255)")]
		public string PhotoPath
		{
			get
			{
				return _photoPath;
			}
			set
			{
				if (value != _photoPath)
				{
					_photoPath = value;
				}
			}
		}

		#endregion

		#region string PostalCode

		private string _postalCode;
		[DebuggerNonUserCode]
		[Column(Storage = "_postalCode", Name = "PostalCode", DbType = "varchar(10)")]
		public string PostalCode
		{
			get
			{
				return _postalCode;
			}
			set
			{
				if (value != _postalCode)
				{
					_postalCode = value;
				}
			}
		}

		#endregion

		#region string Region

		private string _region;
		[DebuggerNonUserCode]
		[Column(Storage = "_region", Name = "Region", DbType = "varchar(15)")]
		public string Region
		{
			get
			{
				return _region;
			}
			set
			{
				if (value != _region)
				{
					_region = value;
				}
			}
		}

		#endregion

		#region int? ReportsTo

		private int? _reportsTo;
		[DebuggerNonUserCode]
		[Column(Storage = "_reportsTo", Name = "ReportsTo", DbType = "int")]
		public int? ReportsTo
		{
			get
			{
				return _reportsTo;
			}
			set
			{
				if (value != _reportsTo)
				{
					if (_reportsToEmployee.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					_reportsTo = value;
				}
			}
		}

		#endregion

		#region string Title

		private string _title;
		[DebuggerNonUserCode]
		[Column(Storage = "_title", Name = "Title", DbType = "varchar(30)")]
		public string Title
		{
			get
			{
				return _title;
			}
			set
			{
				if (value != _title)
				{
					_title = value;
				}
			}
		}

		#endregion

		#region string TitleOfCourtesy

		private string _titleOfCourtesy;
		[DebuggerNonUserCode]
		[Column(Storage = "_titleOfCourtesy", Name = "TitleOfCourtesy", DbType = "varchar(25)")]
		public string TitleOfCourtesy
		{
			get
			{
				return _titleOfCourtesy;
			}
			set
			{
				if (value != _titleOfCourtesy)
				{
					_titleOfCourtesy = value;
				}
			}
		}

		#endregion

		#region Children

		private EntitySet<Employee> _employees;
		[Association(Storage = "_employees", OtherKey = "ReportsTo", Name = "employees_ibfk_1")]
		[DebuggerNonUserCode]
		public EntitySet<Employee> Employees
		{
			get
			{
				return _employees;
			}
			set
			{
				_employees = value;
			}
		}

		private EntitySet<EmployeeTerritory> _employeeTerritories;
		[Association(Storage = "_employeeTerritories", OtherKey = "EmployeeID", Name = "employeeterritories_ibfk_1")]
		[DebuggerNonUserCode]
		public EntitySet<EmployeeTerritory> EmployeeTerritories
		{
			get
			{
				return _employeeTerritories;
			}
			set
			{
				_employeeTerritories = value;
			}
		}

		private EntitySet<Order> _orders;
		[Association(Storage = "_orders", OtherKey = "EmployeeID", Name = "orders_ibfk_2")]
		[DebuggerNonUserCode]
		public EntitySet<Order> Orders
		{
			get
			{
				return _orders;
			}
			set
			{
				_orders = value;
			}
		}


		#endregion

		#region Parents

		private EntityRef<Employee> _reportsToEmployee;
		[Association(Storage = "_reportsToEmployee", ThisKey = "ReportsTo", Name = "employees_ibfk_1", IsForeignKey = true)]
		[DebuggerNonUserCode]
		public Employee ReportsToEmployee
		{
			get
			{
				return _reportsToEmployee.Entity;
			}
			set
			{
				if (value != _reportsToEmployee.Entity)
				{
					if (_reportsToEmployee.Entity != null)
					{
						var previousEmployee = _reportsToEmployee.Entity;
						_reportsToEmployee.Entity = null;
						previousEmployee.Employees.Remove(this);
					}
					_reportsToEmployee.Entity = value;
					if (value != null)
					{
						value.Employees.Add(this);
						_reportsTo = value.EmployeeID;
					}
					else
					{
						_reportsTo = null;
					}
				}
			}
		}


		#endregion

		#region Attachement handlers

		private void Employees_Attach(Employee entity)
		{
			entity.ReportsToEmployee = this;
		}

		private void Employees_Detach(Employee entity)
		{
			entity.ReportsToEmployee = null;
		}

		private void EmployeeTerritories_Attach(EmployeeTerritory entity)
		{
			entity.Employee = this;
		}

		private void EmployeeTerritories_Detach(EmployeeTerritory entity)
		{
			entity.Employee = null;
		}

		private void Orders_Attach(Order entity)
		{
			entity.Employee = this;
		}

		private void Orders_Detach(Order entity)
		{
			entity.Employee = null;
		}


		#endregion

		#region ctor

		public Employee()
		{
			_employees = new EntitySet<Employee>(Employees_Attach, Employees_Detach);
			_employeeTerritories = new EntitySet<EmployeeTerritory>(EmployeeTerritories_Attach, EmployeeTerritories_Detach);
			_orders = new EntitySet<Order>(Orders_Attach, Orders_Detach);
			_reportsToEmployee = new EntityRef<Employee>();
		}

		#endregion

	}

	[Table(Name = "northwind.employeeterritories")]
	public partial class EmployeeTerritory
	{
		#region int EmployeeID

		private int _employeeID;
		[DebuggerNonUserCode]
		[Column(Storage = "_employeeID", Name = "EmployeeID", DbType = "int", IsPrimaryKey = true, CanBeNull = false)]
		public int EmployeeID
		{
			get
			{
				return _employeeID;
			}
			set
			{
				if (value != _employeeID)
				{
					if (_employee.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					_employeeID = value;
				}
			}
		}

		#endregion

		#region string TerritoryID

		private string _territoryID;
		[DebuggerNonUserCode]
		[Column(Storage = "_territoryID", Name = "TerritoryID", DbType = "varchar(20)", IsPrimaryKey = true, CanBeNull = false)]
		public string TerritoryID
		{
			get
			{
				return _territoryID;
			}
			set
			{
				if (value != _territoryID)
				{
					if (_territory.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					_territoryID = value;
				}
			}
		}

		#endregion

		#region Parents

		private EntityRef<Employee> _employee;
		[Association(Storage = "_employee", ThisKey = "EmployeeID", Name = "employeeterritories_ibfk_1", IsForeignKey = true)]
		[DebuggerNonUserCode]
		public Employee Employee
		{
			get
			{
				return _employee.Entity;
			}
			set
			{
				if (value != _employee.Entity)
				{
					if (_employee.Entity != null)
					{
						var previousEmployee = _employee.Entity;
						_employee.Entity = null;
						previousEmployee.EmployeeTerritories.Remove(this);
					}
					_employee.Entity = value;
					if (value != null)
					{
						value.EmployeeTerritories.Add(this);
						_employeeID = value.EmployeeID;
					}
					else
					{
						_employeeID = default(int);
					}
				}
			}
		}

		private EntityRef<Territory> _territory;
		[Association(Storage = "_territory", ThisKey = "TerritoryID", Name = "employeeterritories_ibfk_2", IsForeignKey = true)]
		[DebuggerNonUserCode]
		public Territory Territory
		{
			get
			{
				return _territory.Entity;
			}
			set
			{
				if (value != _territory.Entity)
				{
					if (_territory.Entity != null)
					{
						var previousTerritory = _territory.Entity;
						_territory.Entity = null;
						previousTerritory.EmployeeTerritories.Remove(this);
					}
					_territory.Entity = value;
					if (value != null)
					{
						value.EmployeeTerritories.Add(this);
						_territoryID = value.TerritoryID;
					}
					else
					{
						_territoryID = default(string);
					}
				}
			}
		}


		#endregion

		#region ctor

		public EmployeeTerritory()
		{
			_employee = new EntityRef<Employee>();
			_territory = new EntityRef<Territory>();
		}

		#endregion

	}

	[Table(Name = "northwind.orders")]
	public partial class Order
	{
		#region string CustomerID

		private string _customerID;
		[DebuggerNonUserCode]
		[Column(Storage = "_customerID", Name = "CustomerID", DbType = "varchar(5)")]
		public string CustomerID
		{
			get
			{
				return _customerID;
			}
			set
			{
				if (value != _customerID)
				{
					if (_customer.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					_customerID = value;
				}
			}
		}

		#endregion

		#region int? EmployeeID

		private int? _employeeID;
		[DebuggerNonUserCode]
		[Column(Storage = "_employeeID", Name = "EmployeeID", DbType = "int")]
		public int? EmployeeID
		{
			get
			{
				return _employeeID;
			}
			set
			{
				if (value != _employeeID)
				{
					if (_employee.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					_employeeID = value;
				}
			}
		}

		#endregion

		#region decimal? Freight

		private decimal? _freight;
		[DebuggerNonUserCode]
		[Column(Storage = "_freight", Name = "Freight", DbType = "decimal")]
		public decimal? Freight
		{
			get
			{
				return _freight;
			}
			set
			{
				if (value != _freight)
				{
					_freight = value;
				}
			}
		}

		#endregion

		#region DateTime? OrderDate

		private DateTime? _orderDate;
		[DebuggerNonUserCode]
		[Column(Storage = "_orderDate", Name = "OrderDate", DbType = "datetime")]
		public DateTime? OrderDate
		{
			get
			{
				return _orderDate;
			}
			set
			{
				if (value != _orderDate)
				{
					_orderDate = value;
				}
			}
		}

		#endregion

		#region int OrderID

		private int _orderID;
		[DebuggerNonUserCode]
		[Column(Storage = "_orderID", Name = "OrderID", DbType = "int", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false)]
		public int OrderID
		{
			get
			{
				return _orderID;
			}
			set
			{
				if (value != _orderID)
				{
					_orderID = value;
				}
			}
		}

		#endregion

		#region DateTime? RequiredDate

		private DateTime? _requiredDate;
		[DebuggerNonUserCode]
		[Column(Storage = "_requiredDate", Name = "RequiredDate", DbType = "datetime")]
		public DateTime? RequiredDate
		{
			get
			{
				return _requiredDate;
			}
			set
			{
				if (value != _requiredDate)
				{
					_requiredDate = value;
				}
			}
		}

		#endregion

		#region string ShipAddress

		private string _shipAddress;
		[DebuggerNonUserCode]
		[Column(Storage = "_shipAddress", Name = "ShipAddress", DbType = "varchar(60)")]
		public string ShipAddress
		{
			get
			{
				return _shipAddress;
			}
			set
			{
				if (value != _shipAddress)
				{
					_shipAddress = value;
				}
			}
		}

		#endregion

		#region string ShipCity

		private string _shipCity;
		[DebuggerNonUserCode]
		[Column(Storage = "_shipCity", Name = "ShipCity", DbType = "varchar(15)")]
		public string ShipCity
		{
			get
			{
				return _shipCity;
			}
			set
			{
				if (value != _shipCity)
				{
					_shipCity = value;
				}
			}
		}

		#endregion

		#region string ShipCountry

		private string _shipCountry;
		[DebuggerNonUserCode]
		[Column(Storage = "_shipCountry", Name = "ShipCountry", DbType = "varchar(15)")]
		public string ShipCountry
		{
			get
			{
				return _shipCountry;
			}
			set
			{
				if (value != _shipCountry)
				{
					_shipCountry = value;
				}
			}
		}

		#endregion

		#region string ShipName

		private string _shipName;
		[DebuggerNonUserCode]
		[Column(Storage = "_shipName", Name = "ShipName", DbType = "varchar(40)")]
		public string ShipName
		{
			get
			{
				return _shipName;
			}
			set
			{
				if (value != _shipName)
				{
					_shipName = value;
				}
			}
		}

		#endregion

		#region DateTime? ShippedDate

		private DateTime? _shippedDate;
		[DebuggerNonUserCode]
		[Column(Storage = "_shippedDate", Name = "ShippedDate", DbType = "datetime")]
		public DateTime? ShippedDate
		{
			get
			{
				return _shippedDate;
			}
			set
			{
				if (value != _shippedDate)
				{
					_shippedDate = value;
				}
			}
		}

		#endregion

		#region string ShipPostalCode

		private string _shipPostalCode;
		[DebuggerNonUserCode]
		[Column(Storage = "_shipPostalCode", Name = "ShipPostalCode", DbType = "varchar(10)")]
		public string ShipPostalCode
		{
			get
			{
				return _shipPostalCode;
			}
			set
			{
				if (value != _shipPostalCode)
				{
					_shipPostalCode = value;
				}
			}
		}

		#endregion

		#region string ShipRegion

		private string _shipRegion;
		[DebuggerNonUserCode]
		[Column(Storage = "_shipRegion", Name = "ShipRegion", DbType = "varchar(15)")]
		public string ShipRegion
		{
			get
			{
				return _shipRegion;
			}
			set
			{
				if (value != _shipRegion)
				{
					_shipRegion = value;
				}
			}
		}

		#endregion

		#region int? ShipVia

		private int? _shipVia;
		[DebuggerNonUserCode]
		[Column(Storage = "_shipVia", Name = "ShipVia", DbType = "int")]
		public int? ShipVia
		{
			get
			{
				return _shipVia;
			}
			set
			{
				if (value != _shipVia)
				{
					if (_shipper.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					_shipVia = value;
				}
			}
		}

		#endregion

		#region Children

		private EntitySet<OrderDetail> _orderDetails;
		[Association(Storage = "_orderDetails", OtherKey = "OrderID", Name = "`order details_ibfk_1`")]
		[DebuggerNonUserCode]
		public EntitySet<OrderDetail> OrderDetails
		{
			get
			{
				return _orderDetails;
			}
			set
			{
				_orderDetails = value;
			}
		}


		#endregion

		#region Parents

		private EntityRef<Customer> _customer;
		[Association(Storage = "_customer", ThisKey = "CustomerID", Name = "orders_ibfk_1", IsForeignKey = true)]
		[DebuggerNonUserCode]
		public Customer Customer
		{
			get
			{
				return _customer.Entity;
			}
			set
			{
				if (value != _customer.Entity)
				{
					if (_customer.Entity != null)
					{
						var previousCustomer = _customer.Entity;
						_customer.Entity = null;
						previousCustomer.Orders.Remove(this);
					}
					_customer.Entity = value;
					if (value != null)
					{
						value.Orders.Add(this);
						_customerID = value.CustomerID;
					}
					else
					{
						_customerID = null;
					}
				}
			}
		}

		private EntityRef<Employee> _employee;
		[Association(Storage = "_employee", ThisKey = "EmployeeID", Name = "orders_ibfk_2", IsForeignKey = true)]
		[DebuggerNonUserCode]
		public Employee Employee
		{
			get
			{
				return _employee.Entity;
			}
			set
			{
				if (value != _employee.Entity)
				{
					if (_employee.Entity != null)
					{
						var previousEmployee = _employee.Entity;
						_employee.Entity = null;
						previousEmployee.Orders.Remove(this);
					}
					_employee.Entity = value;
					if (value != null)
					{
						value.Orders.Add(this);
						_employeeID = value.EmployeeID;
					}
					else
					{
						_employeeID = null;
					}
				}
			}
		}

		private EntityRef<Shipper> _shipper;
		[Association(Storage = "_shipper", ThisKey = "ShipVia", Name = "orders_ibfk_3", IsForeignKey = true)]
		[DebuggerNonUserCode]
		public Shipper Shipper
		{
			get
			{
				return _shipper.Entity;
			}
			set
			{
				if (value != _shipper.Entity)
				{
					if (_shipper.Entity != null)
					{
						var previousShipper = _shipper.Entity;
						_shipper.Entity = null;
						previousShipper.Orders.Remove(this);
					}
					_shipper.Entity = value;
					if (value != null)
					{
						value.Orders.Add(this);
						_shipVia = value.ShipperID;
					}
					else
					{
						_shipVia = null;
					}
				}
			}
		}


		#endregion

		#region Attachement handlers

		private void OrderDetails_Attach(OrderDetail entity)
		{
			entity.Order = this;
		}

		private void OrderDetails_Detach(OrderDetail entity)
		{
			entity.Order = null;
		}


		#endregion

		#region ctor

		public Order()
		{
			_orderDetails = new EntitySet<OrderDetail>(OrderDetails_Attach, OrderDetails_Detach);
			_customer = new EntityRef<Customer>();
			_employee = new EntityRef<Employee>();
			_shipper = new EntityRef<Shipper>();
		}

		#endregion

	}

	[Table(Name = "northwind.`order details`")]
	public partial class OrderDetail
	{
		#region float Discount

		private float _discount;
		[DebuggerNonUserCode]
		[Column(Storage = "_discount", Name = "Discount", DbType = "float", CanBeNull = false)]
		public float Discount
		{
			get
			{
				return _discount;
			}
			set
			{
				if (value != _discount)
				{
					_discount = value;
				}
			}
		}

		#endregion

		#region int OrderID

		private int _orderID;
		[DebuggerNonUserCode]
		[Column(Storage = "_orderID", Name = "OrderID", DbType = "int", IsPrimaryKey = true, CanBeNull = false)]
		public int OrderID
		{
			get
			{
				return _orderID;
			}
			set
			{
				if (value != _orderID)
				{
					if (_order.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					_orderID = value;
				}
			}
		}

		#endregion

		#region int ProductID

		private int _productID;
		[DebuggerNonUserCode]
		[Column(Storage = "_productID", Name = "ProductID", DbType = "int", IsPrimaryKey = true, CanBeNull = false)]
		public int ProductID
		{
			get
			{
				return _productID;
			}
			set
			{
				if (value != _productID)
				{
					if (_product.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					_productID = value;
				}
			}
		}

		#endregion

		#region short Quantity

		private short _quantity;
		[DebuggerNonUserCode]
		[Column(Storage = "_quantity", Name = "Quantity", DbType = "smallint(6)", CanBeNull = false)]
		public short Quantity
		{
			get
			{
				return _quantity;
			}
			set
			{
				if (value != _quantity)
				{
					_quantity = value;
				}
			}
		}

		#endregion

		#region decimal UnitPrice

		private decimal _unitPrice;
		[DebuggerNonUserCode]
		[Column(Storage = "_unitPrice", Name = "UnitPrice", DbType = "decimal", CanBeNull = false)]
		public decimal UnitPrice
		{
			get
			{
				return _unitPrice;
			}
			set
			{
				if (value != _unitPrice)
				{
					_unitPrice = value;
				}
			}
		}

		#endregion

		#region Parents

		private EntityRef<Order> _order;
		[Association(Storage = "_order", ThisKey = "OrderID", Name = "`order details_ibfk_1`", IsForeignKey = true)]
		[DebuggerNonUserCode]
		public Order Order
		{
			get
			{
				return _order.Entity;
			}
			set
			{
				if (value != _order.Entity)
				{
					if (_order.Entity != null)
					{
						var previousOrder = _order.Entity;
						_order.Entity = null;
						previousOrder.OrderDetails.Remove(this);
					}
					_order.Entity = value;
					if (value != null)
					{
						value.OrderDetails.Add(this);
						_orderID = value.OrderID;
					}
					else
					{
						_orderID = default(int);
					}
				}
			}
		}

		private EntityRef<Product> _product;
		[Association(Storage = "_product", ThisKey = "ProductID", Name = "`order details_ibfk_2`", IsForeignKey = true)]
		[DebuggerNonUserCode]
		public Product Product
		{
			get
			{
				return _product.Entity;
			}
			set
			{
				if (value != _product.Entity)
				{
					if (_product.Entity != null)
					{
						var previousProduct = _product.Entity;
						_product.Entity = null;
						previousProduct.OrderDetails.Remove(this);
					}
					_product.Entity = value;
					if (value != null)
					{
						value.OrderDetails.Add(this);
						_productID = value.ProductID;
					}
					else
					{
						_productID = default(int);
					}
				}
			}
		}


		#endregion

		#region ctor

		public OrderDetail()
		{
			_order = new EntityRef<Order>();
			_product = new EntityRef<Product>();
		}

		#endregion

	}

	[Table(Name = "northwind.products")]
	public partial class Product
	{
		#region int? CategoryID

		private int? _categoryID;
		[DebuggerNonUserCode]
		[Column(Storage = "_categoryID", Name = "CategoryID", DbType = "int")]
		public int? CategoryID
		{
			get
			{
				return _categoryID;
			}
			set
			{
				if (value != _categoryID)
				{
					if (_category.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					_categoryID = value;
				}
			}
		}

		#endregion

		#region bool Discontinued

		private bool _discontinued;
		[DebuggerNonUserCode]
		[Column(Storage = "_discontinued", Name = "Discontinued", DbType = "bit(1)", CanBeNull = false)]
		public bool Discontinued
		{
			get
			{
				return _discontinued;
			}
			set
			{
				if (value != _discontinued)
				{
					_discontinued = value;
				}
			}
		}

		#endregion

		#region int ProductID

		private int _productID;
		[DebuggerNonUserCode]
		[Column(Storage = "_productID", Name = "ProductID", DbType = "int", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false)]
		public int ProductID
		{
			get
			{
				return _productID;
			}
			set
			{
				if (value != _productID)
				{
					_productID = value;
				}
			}
		}

		#endregion

		#region string ProductName

		private string _productName;
		[DebuggerNonUserCode]
		[Column(Storage = "_productName", Name = "ProductName", DbType = "varchar(40)", CanBeNull = false)]
		public string ProductName
		{
			get
			{
				return _productName;
			}
			set
			{
				if (value != _productName)
				{
					_productName = value;
				}
			}
		}

		#endregion

		#region string QuantityPerUnit

		private string _quantityPerUnit;
		[DebuggerNonUserCode]
		[Column(Storage = "_quantityPerUnit", Name = "QuantityPerUnit", DbType = "varchar(20)")]
		public string QuantityPerUnit
		{
			get
			{
				return _quantityPerUnit;
			}
			set
			{
				if (value != _quantityPerUnit)
				{
					_quantityPerUnit = value;
				}
			}
		}

		#endregion

		#region short? ReorderLevel

		private short? _reorderLevel;
		[DebuggerNonUserCode]
		[Column(Storage = "_reorderLevel", Name = "ReorderLevel", DbType = "smallint(6)")]
		public short? ReorderLevel
		{
			get
			{
				return _reorderLevel;
			}
			set
			{
				if (value != _reorderLevel)
				{
					_reorderLevel = value;
				}
			}
		}

		#endregion

		#region int? SupplierID

		private int? _supplierID;
		[DebuggerNonUserCode]
		[Column(Storage = "_supplierID", Name = "SupplierID", DbType = "int")]
		public int? SupplierID
		{
			get
			{
				return _supplierID;
			}
			set
			{
				if (value != _supplierID)
				{
					if (_supplier.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					_supplierID = value;
				}
			}
		}

		#endregion

		#region decimal? UnitPrice

		private decimal? _unitPrice;
		[DebuggerNonUserCode]
		[Column(Storage = "_unitPrice", Name = "UnitPrice", DbType = "decimal")]
		public decimal? UnitPrice
		{
			get
			{
				return _unitPrice;
			}
			set
			{
				if (value != _unitPrice)
				{
					_unitPrice = value;
				}
			}
		}

		#endregion

		#region short? UnitsInStock

		private short? _unitsInStock;
		[DebuggerNonUserCode]
		[Column(Storage = "_unitsInStock", Name = "UnitsInStock", DbType = "smallint(6)")]
		public short? UnitsInStock
		{
			get
			{
				return _unitsInStock;
			}
			set
			{
				if (value != _unitsInStock)
				{
					_unitsInStock = value;
				}
			}
		}

		#endregion

		#region short? UnitsOnOrder

		private short? _unitsOnOrder;
		[DebuggerNonUserCode]
		[Column(Storage = "_unitsOnOrder", Name = "UnitsOnOrder", DbType = "smallint(6)")]
		public short? UnitsOnOrder
		{
			get
			{
				return _unitsOnOrder;
			}
			set
			{
				if (value != _unitsOnOrder)
				{
					_unitsOnOrder = value;
				}
			}
		}

		#endregion

		#region Children

		private EntitySet<OrderDetail> _orderDetails;
		[Association(Storage = "_orderDetails", OtherKey = "ProductID", Name = "`order details_ibfk_2`")]
		[DebuggerNonUserCode]
		public EntitySet<OrderDetail> OrderDetails
		{
			get
			{
				return _orderDetails;
			}
			set
			{
				_orderDetails = value;
			}
		}


		#endregion

		#region Parents

		private EntityRef<Category> _category;
		[Association(Storage = "_category", ThisKey = "CategoryID", Name = "products_ibfk_1", IsForeignKey = true)]
		[DebuggerNonUserCode]
		public Category Category
		{
			get
			{
				return _category.Entity;
			}
			set
			{
				if (value != _category.Entity)
				{
					if (_category.Entity != null)
					{
						var previousCategory = _category.Entity;
						_category.Entity = null;
						previousCategory.Products.Remove(this);
					}
					_category.Entity = value;
					if (value != null)
					{
						value.Products.Add(this);
						_categoryID = value.CategoryID;
					}
					else
					{
						_categoryID = null;
					}
				}
			}
		}

		private EntityRef<Supplier> _supplier;
		[Association(Storage = "_supplier", ThisKey = "SupplierID", Name = "products_ibfk_2", IsForeignKey = true)]
		[DebuggerNonUserCode]
		public Supplier Supplier
		{
			get
			{
				return _supplier.Entity;
			}
			set
			{
				if (value != _supplier.Entity)
				{
					if (_supplier.Entity != null)
					{
						var previousSupplier = _supplier.Entity;
						_supplier.Entity = null;
						previousSupplier.Products.Remove(this);
					}
					_supplier.Entity = value;
					if (value != null)
					{
						value.Products.Add(this);
						_supplierID = value.SupplierID;
					}
					else
					{
						_supplierID = null;
					}
				}
			}
		}


		#endregion

		#region Attachement handlers

		private void OrderDetails_Attach(OrderDetail entity)
		{
			entity.Product = this;
		}

		private void OrderDetails_Detach(OrderDetail entity)
		{
			entity.Product = null;
		}


		#endregion

		#region ctor

		public Product()
		{
			_orderDetails = new EntitySet<OrderDetail>(OrderDetails_Attach, OrderDetails_Detach);
			_category = new EntityRef<Category>();
			_supplier = new EntityRef<Supplier>();
		}

		#endregion

	}

	[Table(Name = "northwind.region")]
	public partial class Region
	{
		#region string RegionDescription

		private string _regionDescription;
		[DebuggerNonUserCode]
		[Column(Storage = "_regionDescription", Name = "RegionDescription", DbType = "varchar(50)", CanBeNull = false)]
		public string RegionDescription
		{
			get
			{
				return _regionDescription;
			}
			set
			{
				if (value != _regionDescription)
				{
					_regionDescription = value;
				}
			}
		}

		#endregion

		#region int RegionID

		private int _regionID;
		[DebuggerNonUserCode]
		[Column(Storage = "_regionID", Name = "RegionID", DbType = "int", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false)]
		public int RegionID
		{
			get
			{
				return _regionID;
			}
			set
			{
				if (value != _regionID)
				{
					_regionID = value;
				}
			}
		}

		#endregion

		#region Children

		private EntitySet<Territory> _territories;
		[Association(Storage = "_territories", OtherKey = "RegionID", Name = "territories_ibfk_1")]
		[DebuggerNonUserCode]
		public EntitySet<Territory> Territories
		{
			get
			{
				return _territories;
			}
			set
			{
				_territories = value;
			}
		}


		#endregion

		#region Attachement handlers

		private void Territories_Attach(Territory entity)
		{
			entity.Region = this;
		}

		private void Territories_Detach(Territory entity)
		{
			entity.Region = null;
		}


		#endregion

		#region ctor

		public Region()
		{
			_territories = new EntitySet<Territory>(Territories_Attach, Territories_Detach);
		}

		#endregion

	}

	[Table(Name = "northwind.shippers")]
	public partial class Shipper
	{
		#region string CompanyName

		private string _companyName;
		[DebuggerNonUserCode]
		[Column(Storage = "_companyName", Name = "CompanyName", DbType = "varchar(40)", CanBeNull = false)]
		public string CompanyName
		{
			get
			{
				return _companyName;
			}
			set
			{
				if (value != _companyName)
				{
					_companyName = value;
				}
			}
		}

		#endregion

		#region string Phone

		private string _phone;
		[DebuggerNonUserCode]
		[Column(Storage = "_phone", Name = "Phone", DbType = "varchar(24)")]
		public string Phone
		{
			get
			{
				return _phone;
			}
			set
			{
				if (value != _phone)
				{
					_phone = value;
				}
			}
		}

		#endregion

		#region int ShipperID

		private int _shipperID;
		[DebuggerNonUserCode]
		[Column(Storage = "_shipperID", Name = "ShipperID", DbType = "int", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false)]
		public int ShipperID
		{
			get
			{
				return _shipperID;
			}
			set
			{
				if (value != _shipperID)
				{
					_shipperID = value;
				}
			}
		}

		#endregion

		#region Children

		private EntitySet<Order> _orders;
		[Association(Storage = "_orders", OtherKey = "ShipVia", Name = "orders_ibfk_3")]
		[DebuggerNonUserCode]
		public EntitySet<Order> Orders
		{
			get
			{
				return _orders;
			}
			set
			{
				_orders = value;
			}
		}


		#endregion

		#region Attachement handlers

		private void Orders_Attach(Order entity)
		{
			entity.Shipper = this;
		}

		private void Orders_Detach(Order entity)
		{
			entity.Shipper = null;
		}


		#endregion

		#region ctor

		public Shipper()
		{
			_orders = new EntitySet<Order>(Orders_Attach, Orders_Detach);
		}

		#endregion

	}

	[Table(Name = "northwind.suppliers")]
	public partial class Supplier
	{
		#region string Address

		private string _address;
		[DebuggerNonUserCode]
		[Column(Storage = "_address", Name = "Address", DbType = "varchar(60)")]
		public string Address
		{
			get
			{
				return _address;
			}
			set
			{
				if (value != _address)
				{
					_address = value;
				}
			}
		}

		#endregion

		#region string City

		private string _city;
		[DebuggerNonUserCode]
		[Column(Storage = "_city", Name = "City", DbType = "varchar(15)")]
		public string City
		{
			get
			{
				return _city;
			}
			set
			{
				if (value != _city)
				{
					_city = value;
				}
			}
		}

		#endregion

		#region string CompanyName

		private string _companyName;
		[DebuggerNonUserCode]
		[Column(Storage = "_companyName", Name = "CompanyName", DbType = "varchar(40)", CanBeNull = false)]
		public string CompanyName
		{
			get
			{
				return _companyName;
			}
			set
			{
				if (value != _companyName)
				{
					_companyName = value;
				}
			}
		}

		#endregion

		#region string ContactName

		private string _contactName;
		[DebuggerNonUserCode]
		[Column(Storage = "_contactName", Name = "ContactName", DbType = "varchar(30)")]
		public string ContactName
		{
			get
			{
				return _contactName;
			}
			set
			{
				if (value != _contactName)
				{
					_contactName = value;
				}
			}
		}

		#endregion

		#region string ContactTitle

		private string _contactTitle;
		[DebuggerNonUserCode]
		[Column(Storage = "_contactTitle", Name = "ContactTitle", DbType = "varchar(30)")]
		public string ContactTitle
		{
			get
			{
				return _contactTitle;
			}
			set
			{
				if (value != _contactTitle)
				{
					_contactTitle = value;
				}
			}
		}

		#endregion

		#region string Country

		private string _country;
		[DebuggerNonUserCode]
		[Column(Storage = "_country", Name = "Country", DbType = "varchar(15)")]
		public string Country
		{
			get
			{
				return _country;
			}
			set
			{
				if (value != _country)
				{
					_country = value;
				}
			}
		}

		#endregion

		#region string Fax

		private string _fax;
		[DebuggerNonUserCode]
		[Column(Storage = "_fax", Name = "Fax", DbType = "varchar(24)")]
		public string Fax
		{
			get
			{
				return _fax;
			}
			set
			{
				if (value != _fax)
				{
					_fax = value;
				}
			}
		}

		#endregion

		#region string Phone

		private string _phone;
		[DebuggerNonUserCode]
		[Column(Storage = "_phone", Name = "Phone", DbType = "varchar(24)")]
		public string Phone
		{
			get
			{
				return _phone;
			}
			set
			{
				if (value != _phone)
				{
					_phone = value;
				}
			}
		}

		#endregion

		#region string PostalCode

		private string _postalCode;
		[DebuggerNonUserCode]
		[Column(Storage = "_postalCode", Name = "PostalCode", DbType = "varchar(10)")]
		public string PostalCode
		{
			get
			{
				return _postalCode;
			}
			set
			{
				if (value != _postalCode)
				{
					_postalCode = value;
				}
			}
		}

		#endregion

		#region string Region

		private string _region;
		[DebuggerNonUserCode]
		[Column(Storage = "_region", Name = "Region", DbType = "varchar(15)")]
		public string Region
		{
			get
			{
				return _region;
			}
			set
			{
				if (value != _region)
				{
					_region = value;
				}
			}
		}

		#endregion

		#region int SupplierID

		private int _supplierID;
		[DebuggerNonUserCode]
		[Column(Storage = "_supplierID", Name = "SupplierID", DbType = "int", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false)]
		public int SupplierID
		{
			get
			{
				return _supplierID;
			}
			set
			{
				if (value != _supplierID)
				{
					_supplierID = value;
				}
			}
		}

		#endregion

		#region Children

		private EntitySet<Product> _products;
		[Association(Storage = "_products", OtherKey = "SupplierID", Name = "products_ibfk_2")]
		[DebuggerNonUserCode]
		public EntitySet<Product> Products
		{
			get
			{
				return _products;
			}
			set
			{
				_products = value;
			}
		}


		#endregion

		#region Attachement handlers

		private void Products_Attach(Product entity)
		{
			entity.Supplier = this;
		}

		private void Products_Detach(Product entity)
		{
			entity.Supplier = null;
		}


		#endregion

		#region ctor

		public Supplier()
		{
			_products = new EntitySet<Product>(Products_Attach, Products_Detach);
		}

		#endregion

	}

	[Table(Name = "northwind.territories")]
	public partial class Territory
	{
		#region int RegionID

		private int _regionID;
		[DebuggerNonUserCode]
		[Column(Storage = "_regionID", Name = "RegionID", DbType = "int", CanBeNull = false)]
		public int RegionID
		{
			get
			{
				return _regionID;
			}
			set
			{
				if (value != _regionID)
				{
					if (_region.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					_regionID = value;
				}
			}
		}

		#endregion

		#region string TerritoryDescription

		private string _territoryDescription;
		[DebuggerNonUserCode]
		[Column(Storage = "_territoryDescription", Name = "TerritoryDescription", DbType = "varchar(50)", CanBeNull = false)]
		public string TerritoryDescription
		{
			get
			{
				return _territoryDescription;
			}
			set
			{
				if (value != _territoryDescription)
				{
					_territoryDescription = value;
				}
			}
		}

		#endregion

		#region string TerritoryID

		private string _territoryID;
		[DebuggerNonUserCode]
		[Column(Storage = "_territoryID", Name = "TerritoryID", DbType = "varchar(20)", IsPrimaryKey = true, CanBeNull = false)]
		public string TerritoryID
		{
			get
			{
				return _territoryID;
			}
			set
			{
				if (value != _territoryID)
				{
					_territoryID = value;
				}
			}
		}

		#endregion

		#region Children

		private EntitySet<EmployeeTerritory> _employeeTerritories;
		[Association(Storage = "_employeeTerritories", OtherKey = "TerritoryID", Name = "employeeterritories_ibfk_2")]
		[DebuggerNonUserCode]
		public EntitySet<EmployeeTerritory> EmployeeTerritories
		{
			get
			{
				return _employeeTerritories;
			}
			set
			{
				_employeeTerritories = value;
			}
		}


		#endregion

		#region Parents

		private EntityRef<Region> _region;
		[Association(Storage = "_region", ThisKey = "RegionID", Name = "territories_ibfk_1", IsForeignKey = true)]
		[DebuggerNonUserCode]
		public Region Region
		{
			get
			{
				return _region.Entity;
			}
			set
			{
				if (value != _region.Entity)
				{
					if (_region.Entity != null)
					{
						var previousRegion = _region.Entity;
						_region.Entity = null;
						previousRegion.Territories.Remove(this);
					}
					_region.Entity = value;
					if (value != null)
					{
						value.Territories.Add(this);
						_regionID = value.RegionID;
					}
					else
					{
						_regionID = default(int);
					}
				}
			}
		}


		#endregion

		#region Attachement handlers

		private void EmployeeTerritories_Attach(EmployeeTerritory entity)
		{
			entity.Territory = this;
		}

		private void EmployeeTerritories_Detach(EmployeeTerritory entity)
		{
			entity.Territory = null;
		}


		#endregion

		#region ctor

		public Territory()
		{
			_employeeTerritories = new EntitySet<EmployeeTerritory>(EmployeeTerritories_Attach, EmployeeTerritories_Detach);
			_region = new EntityRef<Region>();
		}

		#endregion

	}
}
