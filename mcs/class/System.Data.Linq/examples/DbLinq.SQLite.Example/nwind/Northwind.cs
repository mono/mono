#region Auto-generated classes for Northwind database on 2009-05-19 16:59:42Z

//
//  ____  _     __  __      _        _
// |  _ \| |__ |  \/  | ___| |_ __ _| |
// | | | | '_ \| |\/| |/ _ \ __/ _` | |
// | |_| | |_) | |  | |  __/ || (_| | |
// |____/|_.__/|_|  |_|\___|\__\__,_|_|
//
// Auto-generated from Northwind on 2009-05-19 16:59:42Z
// Please visit http://linq.to/db for more information

#endregion

using System;
using System.Data;
using System.Data.Linq.Mapping;
using System.Diagnostics;
using System.Reflection;
#if MONO_STRICT
using System.Data.Linq;
#else
using DbLinq.Data.Linq;
using DbLinq.Vendor;
#endif
using System.ComponentModel;

namespace nwind
{
	public partial class Northwind : DataContext
	{
		public Northwind(IDbConnection connection)
#if MONO_STRICT
    		: base(connection)
#else
            : base(connection, new DbLinq.Sqlite.SqliteVendor())
#endif
		{
		}

#if !MONO_STRICT
        public Northwind(IDbConnection connection, IVendor vendor)
            : base(connection, vendor)
        {
        }
#endif

		public Table<Category> Categories { get { return GetTable<Category>(); } }
		public Table<Customer> Customers { get { return GetTable<Customer>(); } }
		public Table<Employee> Employees { get { return GetTable<Employee>(); } }
		public Table<EmployeeTerritory> EmployeeTerritories { get { return GetTable<EmployeeTerritory>(); } }
		public Table<OrderDetail> OrderDetails { get { return GetTable<OrderDetail>(); } }
		public Table<Order> Orders { get { return GetTable<Order>(); } }
		public Table<Product> Products { get { return GetTable<Product>(); } }
		public Table<Region> Regions { get { return GetTable<Region>(); } }
		public Table<Shipper> Shippers { get { return GetTable<Shipper>(); } }
		public Table<Supplier> Suppliers { get { return GetTable<Supplier>(); } }
		public Table<Territory> Territories { get { return GetTable<Territory>(); } }

	}

	[Table(Name = "main.Categories")]
	public partial class Category : INotifyPropertyChanged
	{
		#region INotifyPropertyChanged handling

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion

		#region int CategoryID

		private int _categoryID;
		[DebuggerNonUserCode]
		[Column(Storage = "_categoryID", Name = "CategoryID", DbType = "INTEGER", IsPrimaryKey = true, IsDbGenerated = true)]
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
					OnPropertyChanged("CategoryID");
				}
			}
		}

		#endregion

		#region string CategoryName

		private string _categoryName;
		[DebuggerNonUserCode]
		[Column(Storage = "_categoryName", Name = "CategoryName", DbType = "VARCHAR(15)")]
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
					OnPropertyChanged("CategoryName");
				}
			}
		}

		#endregion

		#region string Description

		private string _description;
		[DebuggerNonUserCode]
		[Column(Storage = "_description", Name = "Description", DbType = "TEXT", CanBeNull = true)]
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
					OnPropertyChanged("Description");
				}
			}
		}

		#endregion

		#region Byte[] Picture

		private Byte[] _picture;
		[DebuggerNonUserCode]
		[Column(Storage = "_picture", Name = "Picture", DbType = "BLOB", CanBeNull = true)]
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
					OnPropertyChanged("Picture");
				}
			}
		}

		#endregion

		#region Children

		private EntitySet<Product> _products;
		[Association(Storage = "_products", OtherKey = "CategoryID", Name = "fk_Products_0")]
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

	[Table(Name = "main.Customers")]
	public partial class Customer : INotifyPropertyChanged
	{
		#region INotifyPropertyChanged handling

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion

		#region string Address

		private string _address;
		[DebuggerNonUserCode]
		[Column(Storage = "_address", Name = "Address", DbType = "VARCHAR(60)", CanBeNull = true)]
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
					OnPropertyChanged("Address");
				}
			}
		}

		#endregion

		#region string City

		private string _city;
		[DebuggerNonUserCode]
		[Column(Storage = "_city", Name = "City", DbType = "VARCHAR(15)", CanBeNull = true)]
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
					OnPropertyChanged("City");
				}
			}
		}

		#endregion

		#region string CompanyName

		private string _companyName;
		[DebuggerNonUserCode]
		[Column(Storage = "_companyName", Name = "CompanyName", DbType = "VARCHAR(40)")]
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
					OnPropertyChanged("CompanyName");
				}
			}
		}

		#endregion

		#region string ContactName

		private string _contactName;
		[DebuggerNonUserCode]
		[Column(Storage = "_contactName", Name = "ContactName", DbType = "VARCHAR(30)", CanBeNull = true)]
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
					OnPropertyChanged("ContactName");
				}
			}
		}

		#endregion

		#region string ContactTitle

		private string _contactTitle;
		[DebuggerNonUserCode]
		[Column(Storage = "_contactTitle", Name = "ContactTitle", DbType = "VARCHAR(30)", CanBeNull = true)]
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
					OnPropertyChanged("ContactTitle");
				}
			}
		}

		#endregion

		#region string Country

		private string _country;
		[DebuggerNonUserCode]
		[Column(Storage = "_country", Name = "Country", DbType = "VARCHAR(15)", CanBeNull = true)]
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
					OnPropertyChanged("Country");
				}
			}
		}

		#endregion

		#region string CustomerID

		private string _customerID;
		[DebuggerNonUserCode]
		[Column(Storage = "_customerID", Name = "CustomerID", DbType = "VARCHAR(5)", IsPrimaryKey = true)]
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
					OnPropertyChanged("CustomerID");
				}
			}
		}

		#endregion

		#region string Fax

		private string _fax;
		[DebuggerNonUserCode]
		[Column(Storage = "_fax", Name = "Fax", DbType = "VARCHAR(24)", CanBeNull = true)]
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
					OnPropertyChanged("Fax");
				}
			}
		}

		#endregion

		#region string Phone

		private string _phone;
		[DebuggerNonUserCode]
		[Column(Storage = "_phone", Name = "Phone", DbType = "VARCHAR(24)", CanBeNull = true)]
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
					OnPropertyChanged("Phone");
				}
			}
		}

		#endregion

		#region string PostalCode

		private string _postalCode;
		[DebuggerNonUserCode]
		[Column(Storage = "_postalCode", Name = "PostalCode", DbType = "VARCHAR(10)", CanBeNull = true)]
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
					OnPropertyChanged("PostalCode");
				}
			}
		}

		#endregion

		#region string Region

		private string _region;
		[DebuggerNonUserCode]
		[Column(Storage = "_region", Name = "Region", DbType = "VARCHAR(15)", CanBeNull = true)]
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
					OnPropertyChanged("Region");
				}
			}
		}

		#endregion

		#region Children

		private EntitySet<Order> _orders;
		[Association(Storage = "_orders", OtherKey = "CustomerID", Name = "fk_Orders_1")]
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

	[Table(Name = "main.Employees")]
	public partial class Employee : INotifyPropertyChanged
	{
		#region INotifyPropertyChanged handling

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion

		#region string Address

		private string _address;
		[DebuggerNonUserCode]
		[Column(Storage = "_address", Name = "Address", DbType = "VARCHAR(60)", CanBeNull = true)]
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
					OnPropertyChanged("Address");
				}
			}
		}

		#endregion

		#region DateTime? BirthDate

		private DateTime? _birthDate;
		[DebuggerNonUserCode]
		[Column(Storage = "_birthDate", Name = "BirthDate", DbType = "DATETIME", CanBeNull = true)]
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
					OnPropertyChanged("BirthDate");
				}
			}
		}

		#endregion

		#region string City

		private string _city;
		[DebuggerNonUserCode]
		[Column(Storage = "_city", Name = "City", DbType = "VARCHAR(15)", CanBeNull = true)]
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
					OnPropertyChanged("City");
				}
			}
		}

		#endregion

		#region string Country

		private string _country;
		[DebuggerNonUserCode]
		[Column(Storage = "_country", Name = "Country", DbType = "VARCHAR(15)", CanBeNull = true)]
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
					OnPropertyChanged("Country");
				}
			}
		}

		#endregion

		#region int EmployeeID

		private int _employeeID;
		[DebuggerNonUserCode]
		[Column(Storage = "_employeeID", Name = "EmployeeID", DbType = "INTEGER", IsPrimaryKey = true, IsDbGenerated = true)]
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
					OnPropertyChanged("EmployeeID");
				}
			}
		}

		#endregion

		#region string Extension

		private string _extension;
		[DebuggerNonUserCode]
		[Column(Storage = "_extension", Name = "Extension", DbType = "VARCHAR(5)", CanBeNull = true)]
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
					OnPropertyChanged("Extension");
				}
			}
		}

		#endregion

		#region string FirstName

		private string _firstName;
		[DebuggerNonUserCode]
		[Column(Storage = "_firstName", Name = "FirstName", DbType = "VARCHAR(10)")]
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
					OnPropertyChanged("FirstName");
				}
			}
		}

		#endregion

		#region DateTime? HireDate

		private DateTime? _hireDate;
		[DebuggerNonUserCode]
		[Column(Storage = "_hireDate", Name = "HireDate", DbType = "DATETIME", CanBeNull = true)]
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
					OnPropertyChanged("HireDate");
				}
			}
		}

		#endregion

		#region string HomePhone

		private string _homePhone;
		[DebuggerNonUserCode]
		[Column(Storage = "_homePhone", Name = "HomePhone", DbType = "VARCHAR(24)", CanBeNull = true)]
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
					OnPropertyChanged("HomePhone");
				}
			}
		}

		#endregion

		#region string LastName

		private string _lastName;
		[DebuggerNonUserCode]
		[Column(Storage = "_lastName", Name = "LastName", DbType = "VARCHAR(20)")]
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
					OnPropertyChanged("LastName");
				}
			}
		}

		#endregion

		#region string Notes

		private string _notes;
		[DebuggerNonUserCode]
		[Column(Storage = "_notes", Name = "Notes", DbType = "TEXT", CanBeNull = true)]
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
					OnPropertyChanged("Notes");
				}
			}
		}

		#endregion

		#region Byte[] Photo

		private Byte[] _photo;
		[DebuggerNonUserCode]
		[Column(Storage = "_photo", Name = "Photo", DbType = "BLOB", CanBeNull = true)]
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
					OnPropertyChanged("Photo");
				}
			}
		}

		#endregion

		#region string PhotoPath

		private string _photoPath;
		[DebuggerNonUserCode]
		[Column(Storage = "_photoPath", Name = "PhotoPath", DbType = "VARCHAR (255)", CanBeNull = true)]
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
					OnPropertyChanged("PhotoPath");
				}
			}
		}

		#endregion

		#region string PostalCode

		private string _postalCode;
		[DebuggerNonUserCode]
		[Column(Storage = "_postalCode", Name = "PostalCode", DbType = "VARCHAR(10)", CanBeNull = true)]
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
					OnPropertyChanged("PostalCode");
				}
			}
		}

		#endregion

		#region string Region

		private string _region;
		[DebuggerNonUserCode]
		[Column(Storage = "_region", Name = "Region", DbType = "VARCHAR(15)", CanBeNull = true)]
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
					OnPropertyChanged("Region");
				}
			}
		}

		#endregion

		#region int? ReportsTo

		private int? _reportsTo;
		[DebuggerNonUserCode]
		[Column(Storage = "_reportsTo", Name = "ReportsTo", DbType = "INTEGER", CanBeNull = true)]
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
					if (_reportsToEmployees.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					_reportsTo = value;
					OnPropertyChanged("ReportsTo");
				}
			}
		}

		#endregion

		#region string Title

		private string _title;
		[DebuggerNonUserCode]
		[Column(Storage = "_title", Name = "Title", DbType = "VARCHAR(30)", CanBeNull = true)]
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
					OnPropertyChanged("Title");
				}
			}
		}

		#endregion

		#region string TitleOfCourtesy

		private string _titleOfCourtesy;
		[DebuggerNonUserCode]
		[Column(Storage = "_titleOfCourtesy", Name = "TitleOfCourtesy", DbType = "VARCHAR(25)", CanBeNull = true)]
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
					OnPropertyChanged("TitleOfCourtesy");
				}
			}
		}

		#endregion

		#region Children

		private EntitySet<EmployeeTerritory> _employeeTerritories;
		[Association(Storage = "_employeeTerritories", OtherKey = "EmployeeID", Name = "fk_EmployeeTerritories_1")]
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

		private EntitySet<Employee> _employeeIdeMployees;
		[Association(Storage = "_employeeIdeMployees", OtherKey = "ReportsTo", Name = "fk_Employees_0")]
		[DebuggerNonUserCode]
		public EntitySet<Employee> Employees
		{
			get
			{
				return _employeeIdeMployees;
			}
			set
			{
				_employeeIdeMployees = value;
			}
		}

		private EntitySet<Order> _orders;
		[Association(Storage = "_orders", OtherKey = "EmployeeID", Name = "fk_Orders_0")]
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

		private EntityRef<Employee> _reportsToEmployees;
		[Association(Storage = "_reportsToEmployees", ThisKey = "ReportsTo", Name = "fk_Employees_0", IsForeignKey = true)]
		[DebuggerNonUserCode]
		public Employee ReportsToEmployee
		{
			get
			{
				return _reportsToEmployees.Entity;
			}
			set
			{
				if (value != _reportsToEmployees.Entity)
				{
					if (_reportsToEmployees.Entity != null)
					{
						var previousEmployees = _reportsToEmployees.Entity;
						_reportsToEmployees.Entity = null;
						previousEmployees.Employees.Remove(this);
					}
					_reportsToEmployees.Entity = value;
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

		private void EmployeeTerritories_Attach(EmployeeTerritory entity)
		{
			entity.Employee = this;
		}

		private void EmployeeTerritories_Detach(EmployeeTerritory entity)
		{
			entity.Employee = null;
		}

		private void EmployeeIDEmployees_Attach(Employee entity)
		{
			entity.ReportsToEmployee = this;
		}

		private void EmployeeIDEmployees_Detach(Employee entity)
		{
			entity.ReportsToEmployee = null;
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
			_employeeTerritories = new EntitySet<EmployeeTerritory>(EmployeeTerritories_Attach, EmployeeTerritories_Detach);
			_employeeIdeMployees = new EntitySet<Employee>(EmployeeIDEmployees_Attach, EmployeeIDEmployees_Detach);
			_orders = new EntitySet<Order>(Orders_Attach, Orders_Detach);
			_reportsToEmployees = new EntityRef<Employee>();
		}

		#endregion

	}

	[Table(Name = "main.EmployeeTerritories")]
	public partial class EmployeeTerritory : INotifyPropertyChanged
	{
		#region INotifyPropertyChanged handling

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion

		#region int EmployeeID

		private int _employeeID;
		[DebuggerNonUserCode]
		[Column(Storage = "_employeeID", Name = "EmployeeID", DbType = "INTEGER", IsPrimaryKey = true, IsDbGenerated = true)]
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
					if (_employees.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					_employeeID = value;
					OnPropertyChanged("EmployeeID");
				}
			}
		}

		#endregion

		#region string TerritoryID

		private string _territoryID;
		[DebuggerNonUserCode]
		[Column(Storage = "_territoryID", Name = "TerritoryID", DbType = "VARCHAR(20)", IsPrimaryKey = true)]
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
					if (_territories.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					_territoryID = value;
					OnPropertyChanged("TerritoryID");
				}
			}
		}

		#endregion

		#region Parents

		private EntityRef<Territory> _territories;
		[Association(Storage = "_territories", ThisKey = "TerritoryID", Name = "fk_EmployeeTerritories_0", IsForeignKey = true)]
		[DebuggerNonUserCode]
		public Territory Territory
		{
			get
			{
				return _territories.Entity;
			}
			set
			{
				if (value != _territories.Entity)
				{
					if (_territories.Entity != null)
					{
						var previousTerritories = _territories.Entity;
						_territories.Entity = null;
						previousTerritories.EmployeeTerritories.Remove(this);
					}
					_territories.Entity = value;
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

		private EntityRef<Employee> _employees;
		[Association(Storage = "_employees", ThisKey = "EmployeeID", Name = "fk_EmployeeTerritories_1", IsForeignKey = true)]
		[DebuggerNonUserCode]
		public Employee Employee
		{
			get
			{
				return _employees.Entity;
			}
			set
			{
				if (value != _employees.Entity)
				{
					if (_employees.Entity != null)
					{
						var previousEmployees = _employees.Entity;
						_employees.Entity = null;
						previousEmployees.EmployeeTerritories.Remove(this);
					}
					_employees.Entity = value;
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


		#endregion

		#region ctor

		public EmployeeTerritory()
		{
			_territories = new EntityRef<Territory>();
			_employees = new EntityRef<Employee>();
		}

		#endregion

	}

	[Table(Name = "main.\"Order Details\"")]
	public partial class OrderDetail : INotifyPropertyChanged
	{
		#region INotifyPropertyChanged handling

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion

		#region float Discount

		private float _discount;
		[DebuggerNonUserCode]
		[Column(Storage = "_discount", Name = "Discount", DbType = "FLOAT")]
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
					OnPropertyChanged("Discount");
				}
			}
		}

		#endregion

		#region int OrderID

		private int _orderID;
		[DebuggerNonUserCode]
		[Column(Storage = "_orderID", Name = "OrderID", DbType = "INTEGER", IsPrimaryKey = true, IsDbGenerated = true)]
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
					if (_orders.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					_orderID = value;
					OnPropertyChanged("OrderID");
				}
			}
		}

		#endregion

		#region int ProductID

		private int _productID;
		[DebuggerNonUserCode]
		[Column(Storage = "_productID", Name = "ProductID", DbType = "INTEGER", IsPrimaryKey = true, IsDbGenerated = true)]
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
					if (_products.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					_productID = value;
					OnPropertyChanged("ProductID");
				}
			}
		}

		#endregion

		#region short Quantity

		private short _quantity;
		[DebuggerNonUserCode]
		[Column(Storage = "_quantity", Name = "Quantity", DbType = "SMALLINT")]
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
					OnPropertyChanged("Quantity");
				}
			}
		}

		#endregion

		#region decimal UnitPrice

		private decimal _unitPrice;
		[DebuggerNonUserCode]
		[Column(Storage = "_unitPrice", Name = "UnitPrice", DbType = "DECIMAL")]
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
					OnPropertyChanged("UnitPrice");
				}
			}
		}

		#endregion

		#region Parents

		private EntityRef<Product> _products;
		[Association(Storage = "_products", ThisKey = "ProductID", Name = "\"fk_Order Details_0\"", IsForeignKey = true)]
		[DebuggerNonUserCode]
		public Product Product
		{
			get
			{
				return _products.Entity;
			}
			set
			{
				if (value != _products.Entity)
				{
					if (_products.Entity != null)
					{
						var previousProducts = _products.Entity;
						_products.Entity = null;
						previousProducts.OrderDetails.Remove(this);
					}
					_products.Entity = value;
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

		private EntityRef<Order> _orders;
		[Association(Storage = "_orders", ThisKey = "OrderID", Name = "\"fk_Order Details_1\"", IsForeignKey = true)]
		[DebuggerNonUserCode]
		public Order Order
		{
			get
			{
				return _orders.Entity;
			}
			set
			{
				if (value != _orders.Entity)
				{
					if (_orders.Entity != null)
					{
						var previousOrders = _orders.Entity;
						_orders.Entity = null;
						previousOrders.OrderDetails.Remove(this);
					}
					_orders.Entity = value;
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


		#endregion

		#region ctor

		public OrderDetail()
		{
			_products = new EntityRef<Product>();
			_orders = new EntityRef<Order>();
		}

		#endregion

	}

	[Table(Name = "main.Orders")]
	public partial class Order : INotifyPropertyChanged
	{
		#region INotifyPropertyChanged handling

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion

		#region string CustomerID

		private string _customerID;
		[DebuggerNonUserCode]
		[Column(Storage = "_customerID", Name = "CustomerID", DbType = "VARCHAR(5)", CanBeNull = true)]
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
					if (_customers.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					_customerID = value;
					OnPropertyChanged("CustomerID");
				}
			}
		}

		#endregion

		#region int? EmployeeID

		private int? _employeeID;
		[DebuggerNonUserCode]
		[Column(Storage = "_employeeID", Name = "EmployeeID", DbType = "INTEGER", CanBeNull = true)]
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
					if (_employees.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					_employeeID = value;
					OnPropertyChanged("EmployeeID");
				}
			}
		}

		#endregion

		#region decimal? Freight

		private decimal? _freight;
		[DebuggerNonUserCode]
		[Column(Storage = "_freight", Name = "Freight", DbType = "DECIMAL", CanBeNull = true)]
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
					OnPropertyChanged("Freight");
				}
			}
		}

		#endregion

		#region DateTime? OrderDate

		private DateTime? _orderDate;
		[DebuggerNonUserCode]
		[Column(Storage = "_orderDate", Name = "OrderDate", DbType = "DATETIME", CanBeNull = true)]
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
					OnPropertyChanged("OrderDate");
				}
			}
		}

		#endregion

		#region int OrderID

		private int _orderID;
		[DebuggerNonUserCode]
		[Column(Storage = "_orderID", Name = "OrderID", DbType = "INTEGER", IsPrimaryKey = true, IsDbGenerated = true)]
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
					OnPropertyChanged("OrderID");
				}
			}
		}

		#endregion

		#region DateTime? RequiredDate

		private DateTime? _requiredDate;
		[DebuggerNonUserCode]
		[Column(Storage = "_requiredDate", Name = "RequiredDate", DbType = "DATETIME", CanBeNull = true)]
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
					OnPropertyChanged("RequiredDate");
				}
			}
		}

		#endregion

		#region string ShipAddress

		private string _shipAddress;
		[DebuggerNonUserCode]
		[Column(Storage = "_shipAddress", Name = "ShipAddress", DbType = "VARCHAR(60)", CanBeNull = true)]
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
					OnPropertyChanged("ShipAddress");
				}
			}
		}

		#endregion

		#region string ShipCity

		private string _shipCity;
		[DebuggerNonUserCode]
		[Column(Storage = "_shipCity", Name = "ShipCity", DbType = "VARCHAR(15)", CanBeNull = true)]
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
					OnPropertyChanged("ShipCity");
				}
			}
		}

		#endregion

		#region string ShipCountry

		private string _shipCountry;
		[DebuggerNonUserCode]
		[Column(Storage = "_shipCountry", Name = "ShipCountry", DbType = "VARCHAR(15)", CanBeNull = true)]
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
					OnPropertyChanged("ShipCountry");
				}
			}
		}

		#endregion

		#region string ShipName

		private string _shipName;
		[DebuggerNonUserCode]
		[Column(Storage = "_shipName", Name = "ShipName", DbType = "VARCHAR(40)", CanBeNull = true)]
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
					OnPropertyChanged("ShipName");
				}
			}
		}

		#endregion

		#region DateTime? ShippedDate

		private DateTime? _shippedDate;
		[DebuggerNonUserCode]
		[Column(Storage = "_shippedDate", Name = "ShippedDate", DbType = "DATETIME", CanBeNull = true)]
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
					OnPropertyChanged("ShippedDate");
				}
			}
		}

		#endregion

		#region string ShipPostalCode

		private string _shipPostalCode;
		[DebuggerNonUserCode]
		[Column(Storage = "_shipPostalCode", Name = "ShipPostalCode", DbType = "VARCHAR(10)", CanBeNull = true)]
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
					OnPropertyChanged("ShipPostalCode");
				}
			}
		}

		#endregion

		#region string ShipRegion

		private string _shipRegion;
		[DebuggerNonUserCode]
		[Column(Storage = "_shipRegion", Name = "ShipRegion", DbType = "VARCHAR(15)", CanBeNull = true)]
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
					OnPropertyChanged("ShipRegion");
				}
			}
		}

		#endregion

		#region int? ShipVia

		private int? _shipVia;
		[DebuggerNonUserCode]
		[Column(Storage = "_shipVia", Name = "ShipVia", DbType = "INT", CanBeNull = true)]
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
					_shipVia = value;
					OnPropertyChanged("ShipVia");
				}
			}
		}

		#endregion

		#region Children

		private EntitySet<OrderDetail> _orderDetails;
		[Association(Storage = "_orderDetails", OtherKey = "OrderID", Name = "\"fk_Order Details_1\"")]
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

		private EntityRef<Employee> _employees;
		[Association(Storage = "_employees", ThisKey = "EmployeeID", Name = "fk_Orders_0", IsForeignKey = true)]
		[DebuggerNonUserCode]
		public Employee Employee
		{
			get
			{
				return _employees.Entity;
			}
			set
			{
				if (value != _employees.Entity)
				{
					if (_employees.Entity != null)
					{
						var previousEmployees = _employees.Entity;
						_employees.Entity = null;
						previousEmployees.Orders.Remove(this);
					}
					_employees.Entity = value;
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

		private EntityRef<Customer> _customers;
		[Association(Storage = "_customers", ThisKey = "CustomerID", Name = "fk_Orders_1", IsForeignKey = true)]
		[DebuggerNonUserCode]
		public Customer Customer
		{
			get
			{
				return _customers.Entity;
			}
			set
			{
				if (value != _customers.Entity)
				{
					if (_customers.Entity != null)
					{
						var previousCustomers = _customers.Entity;
						_customers.Entity = null;
						previousCustomers.Orders.Remove(this);
					}
					_customers.Entity = value;
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
			_employees = new EntityRef<Employee>();
			_customers = new EntityRef<Customer>();
		}

		#endregion

	}

	[Table(Name = "main.Products")]
	public partial class Product : INotifyPropertyChanged
	{
		#region INotifyPropertyChanged handling

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion

		#region int? CategoryID

		private int? _categoryID;
		[DebuggerNonUserCode]
		[Column(Storage = "_categoryID", Name = "CategoryID", DbType = "INTEGER", CanBeNull = true)]
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
					if (_categories.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					_categoryID = value;
					OnPropertyChanged("CategoryID");
				}
			}
		}

		#endregion

		#region bool Discontinued

		private bool _discontinued;
		[DebuggerNonUserCode]
		[Column(Storage = "_discontinued", Name = "Discontinued", DbType = "BIT")]
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
					OnPropertyChanged("Discontinued");
				}
			}
		}

		#endregion

		#region int ProductID

		private int _productID;
		[DebuggerNonUserCode]
		[Column(Storage = "_productID", Name = "ProductID", DbType = "INTEGER", IsPrimaryKey = true, IsDbGenerated = true)]
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
					OnPropertyChanged("ProductID");
				}
			}
		}

		#endregion

		#region string ProductName

		private string _productName;
		[DebuggerNonUserCode]
		[Column(Storage = "_productName", Name = "ProductName", DbType = "VARCHAR(40)")]
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
					OnPropertyChanged("ProductName");
				}
			}
		}

		#endregion

		#region string QuantityPerUnit

		private string _quantityPerUnit;
		[DebuggerNonUserCode]
		[Column(Storage = "_quantityPerUnit", Name = "QuantityPerUnit", DbType = "VARCHAR(20)", CanBeNull = true)]
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
					OnPropertyChanged("QuantityPerUnit");
				}
			}
		}

		#endregion

		#region short? ReorderLevel

		private short? _reorderLevel;
		[DebuggerNonUserCode]
		[Column(Storage = "_reorderLevel", Name = "ReorderLevel", DbType = "SMALLINT", CanBeNull = true)]
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
					OnPropertyChanged("ReorderLevel");
				}
			}
		}

		#endregion

		#region int? SupplierID

		private int? _supplierID;
		[DebuggerNonUserCode]
		[Column(Storage = "_supplierID", Name = "SupplierID", DbType = "INTEGER", CanBeNull = true)]
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
					if (_suppliers.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					_supplierID = value;
					OnPropertyChanged("SupplierID");
				}
			}
		}

		#endregion

		#region decimal? UnitPrice

		private decimal? _unitPrice;
		[DebuggerNonUserCode]
		[Column(Storage = "_unitPrice", Name = "UnitPrice", DbType = "DECIMAL", CanBeNull = true)]
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
					OnPropertyChanged("UnitPrice");
				}
			}
		}

		#endregion

		#region short? UnitsInStock

		private short? _unitsInStock;
		[DebuggerNonUserCode]
		[Column(Storage = "_unitsInStock", Name = "UnitsInStock", DbType = "SMALLINT", CanBeNull = true)]
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
					OnPropertyChanged("UnitsInStock");
				}
			}
		}

		#endregion

		#region short? UnitsOnOrder

		private short? _unitsOnOrder;
		[DebuggerNonUserCode]
		[Column(Storage = "_unitsOnOrder", Name = "UnitsOnOrder", DbType = "SMALLINT", CanBeNull = true)]
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
					OnPropertyChanged("UnitsOnOrder");
				}
			}
		}

		#endregion

		#region Children

		private EntitySet<OrderDetail> _orderDetails;
		[Association(Storage = "_orderDetails", OtherKey = "ProductID", Name = "\"fk_Order Details_0\"")]
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

		private EntityRef<Category> _categories;
		[Association(Storage = "_categories", ThisKey = "CategoryID", Name = "fk_Products_0", IsForeignKey = true)]
		[DebuggerNonUserCode]
		public Category Category
		{
			get
			{
				return _categories.Entity;
			}
			set
			{
				if (value != _categories.Entity)
				{
					if (_categories.Entity != null)
					{
						var previousCategories = _categories.Entity;
						_categories.Entity = null;
						previousCategories.Products.Remove(this);
					}
					_categories.Entity = value;
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

		private EntityRef<Supplier> _suppliers;
		[Association(Storage = "_suppliers", ThisKey = "SupplierID", Name = "fk_Products_1", IsForeignKey = true)]
		[DebuggerNonUserCode]
		public Supplier Supplier
		{
			get
			{
				return _suppliers.Entity;
			}
			set
			{
				if (value != _suppliers.Entity)
				{
					if (_suppliers.Entity != null)
					{
						var previousSuppliers = _suppliers.Entity;
						_suppliers.Entity = null;
						previousSuppliers.Products.Remove(this);
					}
					_suppliers.Entity = value;
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
			_categories = new EntityRef<Category>();
			_suppliers = new EntityRef<Supplier>();
		}

		#endregion

	}

	[Table(Name = "main.Region")]
	public partial class Region : INotifyPropertyChanged
	{
		#region INotifyPropertyChanged handling

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion

		#region string RegionDescription

		private string _regionDescription;
		[DebuggerNonUserCode]
		[Column(Storage = "_regionDescription", Name = "RegionDescription", DbType = "VARCHAR(50)")]
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
					OnPropertyChanged("RegionDescription");
				}
			}
		}

		#endregion

		#region int RegionID

		private int _regionID;
		[DebuggerNonUserCode]
		[Column(Storage = "_regionID", Name = "RegionID", DbType = "INTEGER", IsPrimaryKey = true, IsDbGenerated = true)]
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
					OnPropertyChanged("RegionID");
				}
			}
		}

		#endregion

		#region Children

		private EntitySet<Territory> _territories;
		[Association(Storage = "_territories", OtherKey = "RegionID", Name = "fk_Territories_0")]
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

	[Table(Name = "main.Shippers")]
	public partial class Shipper : INotifyPropertyChanged
	{
		#region INotifyPropertyChanged handling

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion

		#region string CompanyName

		private string _companyName;
		[DebuggerNonUserCode]
		[Column(Storage = "_companyName", Name = "CompanyName", DbType = "VARCHAR(40)")]
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
					OnPropertyChanged("CompanyName");
				}
			}
		}

		#endregion

		#region string Phone

		private string _phone;
		[DebuggerNonUserCode]
		[Column(Storage = "_phone", Name = "Phone", DbType = "VARCHAR(24)", CanBeNull = true)]
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
					OnPropertyChanged("Phone");
				}
			}
		}

		#endregion

		#region int ShipperID

		private int _shipperID;
		[DebuggerNonUserCode]
		[Column(Storage = "_shipperID", Name = "ShipperID", DbType = "INTEGER", IsPrimaryKey = true, IsDbGenerated = true)]
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
					OnPropertyChanged("ShipperID");
				}
			}
		}

		#endregion

		#region ctor

		public Shipper()
		{
		}

		#endregion

	}

	[Table(Name = "main.Suppliers")]
	public partial class Supplier : INotifyPropertyChanged
	{
		#region INotifyPropertyChanged handling

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion

		#region string Address

		private string _address;
		[DebuggerNonUserCode]
		[Column(Storage = "_address", Name = "Address", DbType = "VARCHAR(60)", CanBeNull = true)]
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
					OnPropertyChanged("Address");
				}
			}
		}

		#endregion

		#region string City

		private string _city;
		[DebuggerNonUserCode]
		[Column(Storage = "_city", Name = "City", DbType = "VARCHAR(15)", CanBeNull = true)]
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
					OnPropertyChanged("City");
				}
			}
		}

		#endregion

		#region string CompanyName

		private string _companyName;
		[DebuggerNonUserCode]
		[Column(Storage = "_companyName", Name = "CompanyName", DbType = "VARCHAR(40)")]
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
					OnPropertyChanged("CompanyName");
				}
			}
		}

		#endregion

		#region string ContactName

		private string _contactName;
		[DebuggerNonUserCode]
		[Column(Storage = "_contactName", Name = "ContactName", DbType = "VARCHAR(30)", CanBeNull = true)]
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
					OnPropertyChanged("ContactName");
				}
			}
		}

		#endregion

		#region string ContactTitle

		private string _contactTitle;
		[DebuggerNonUserCode]
		[Column(Storage = "_contactTitle", Name = "ContactTitle", DbType = "VARCHAR(30)", CanBeNull = true)]
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
					OnPropertyChanged("ContactTitle");
				}
			}
		}

		#endregion

		#region string Country

		private string _country;
		[DebuggerNonUserCode]
		[Column(Storage = "_country", Name = "Country", DbType = "VARCHAR(15)", CanBeNull = true)]
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
					OnPropertyChanged("Country");
				}
			}
		}

		#endregion

		#region string Fax

		private string _fax;
		[DebuggerNonUserCode]
		[Column(Storage = "_fax", Name = "Fax", DbType = "VARCHAR(24)", CanBeNull = true)]
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
					OnPropertyChanged("Fax");
				}
			}
		}

		#endregion

		#region string Phone

		private string _phone;
		[DebuggerNonUserCode]
		[Column(Storage = "_phone", Name = "Phone", DbType = "VARCHAR(24)", CanBeNull = true)]
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
					OnPropertyChanged("Phone");
				}
			}
		}

		#endregion

		#region string PostalCode

		private string _postalCode;
		[DebuggerNonUserCode]
		[Column(Storage = "_postalCode", Name = "PostalCode", DbType = "VARCHAR(10)", CanBeNull = true)]
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
					OnPropertyChanged("PostalCode");
				}
			}
		}

		#endregion

		#region string Region

		private string _region;
		[DebuggerNonUserCode]
		[Column(Storage = "_region", Name = "Region", DbType = "VARCHAR(15)", CanBeNull = true)]
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
					OnPropertyChanged("Region");
				}
			}
		}

		#endregion

		#region int SupplierID

		private int _supplierID;
		[DebuggerNonUserCode]
		[Column(Storage = "_supplierID", Name = "SupplierID", DbType = "INTEGER", IsPrimaryKey = true, IsDbGenerated = true)]
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
					OnPropertyChanged("SupplierID");
				}
			}
		}

		#endregion

		#region Children

		private EntitySet<Product> _products;
		[Association(Storage = "_products", OtherKey = "SupplierID", Name = "fk_Products_1")]
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

	[Table(Name = "main.Territories")]
	public partial class Territory : INotifyPropertyChanged
	{
		#region INotifyPropertyChanged handling

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion

		#region int RegionID

		private int _regionID;
		[DebuggerNonUserCode]
		[Column(Storage = "_regionID", Name = "RegionID", DbType = "INTEGER")]
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
					if (_regions.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					_regionID = value;
					OnPropertyChanged("RegionID");
				}
			}
		}

		#endregion

		#region string TerritoryDescription

		private string _territoryDescription;
		[DebuggerNonUserCode]
		[Column(Storage = "_territoryDescription", Name = "TerritoryDescription", DbType = "VARCHAR(50)")]
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
					OnPropertyChanged("TerritoryDescription");
				}
			}
		}

		#endregion

		#region string TerritoryID

		private string _territoryID;
		[DebuggerNonUserCode]
		[Column(Storage = "_territoryID", Name = "TerritoryID", DbType = "VARCHAR(20)", IsPrimaryKey = true)]
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
					OnPropertyChanged("TerritoryID");
				}
			}
		}

		#endregion

		#region Children

		private EntitySet<EmployeeTerritory> _employeeTerritories;
		[Association(Storage = "_employeeTerritories", OtherKey = "TerritoryID", Name = "fk_EmployeeTerritories_0")]
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

		private EntityRef<Region> _regions;
		[Association(Storage = "_regions", ThisKey = "RegionID", Name = "fk_Territories_0", IsForeignKey = true)]
		[DebuggerNonUserCode]
		public Region Region
		{
			get
			{
				return _regions.Entity;
			}
			set
			{
				if (value != _regions.Entity)
				{
					if (_regions.Entity != null)
					{
						var previousRegions = _regions.Entity;
						_regions.Entity = null;
						previousRegions.Territories.Remove(this);
					}
					_regions.Entity = value;
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
			_regions = new EntityRef<Region>();
		}

		#endregion

	}
}
