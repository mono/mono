#region Auto-generated classes for  C:\Program Files\Firebird\Firebird_2_1\examples\nwind\NORTHWIND .FDB database on 2008-10-08 09:28:07Z

//
//  ____  _     __  __      _        _
// |  _ \| |__ |  \/  | ___| |_ __ _| |
// | | | | '_ \| |\/| |/ _ \ __/ _` | |
// | |_| | |_) | |  | |  __/ || (_| | |
// |____/|_.__/|_|  |_|\___|\__\__,_|_|
//
// Auto-generated from  C:\Program Files\Firebird\Firebird_2_1\examples\nwind\NORTHWIND .FDB on 2008-10-08 09:28:07Z
// Please visit http://linq.to/db for more information

#endregion

using System;
using System.Data;
using System.Data.Linq.Mapping;
using System.Diagnostics;
using System.Reflection;
using DbLinq.Data.Linq;
using DbLinq.Vendor;
using System.ComponentModel;

public partial class CpRogramFilesFirebirdFirebird21ExamplesNwindNorthwindfdb : DataContext
{
	public CpRogramFilesFirebirdFirebird21ExamplesNwindNorthwindfdb(IDbConnection connection)
	: base(connection, new DbLinq.FirebirdSql.FbVendor())
	{
	}

	public CpRogramFilesFirebirdFirebird21ExamplesNwindNorthwindfdb(IDbConnection connection, IVendor vendor)
	: base(connection, vendor)
	{
	}

	public Table<Categories> Categories { get { return GetTable<Categories>(); } }
	public Table<Customers> Customers { get { return GetTable<Customers>(); } }
	public Table<Employees> Employees { get { return GetTable<Employees>(); } }
	public Table<EmployeeTerritories> EmployeeTerritories { get { return GetTable<EmployeeTerritories>(); } }
	public Table<OrderDetails> OrderDetails { get { return GetTable<OrderDetails>(); } }
	public Table<Orders> Orders { get { return GetTable<Orders>(); } }
	public Table<Products> Products { get { return GetTable<Products>(); } }
	public Table<Region> Region { get { return GetTable<Region>(); } }
	public Table<Shippers> Shippers { get { return GetTable<Shippers>(); } }
	public Table<Suppliers> Suppliers { get { return GetTable<Suppliers>(); } }
	public Table<Territories> Territories { get { return GetTable<Territories>(); } }

}

[Table(Name = " Foo .CATEGORIES")]
public partial class Categories : INotifyPropertyChanged
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
	[Column(Storage = "_categoryID", Name = "CATEGORYID", DbType = null, IsPrimaryKey = true, CanBeNull = false)]
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
	[Column(Storage = "_categoryName", Name = "CATEGORYNAME", DbType = null, CanBeNull = false)]
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

	#region Byte[] Description

	private Byte[] _description;
	[DebuggerNonUserCode]
	[Column(Storage = "_description", Name = "DESCRIPTION", DbType = null)]
	public Byte[] Description
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
	[Column(Storage = "_picture", Name = "PICTURE", DbType = null)]
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

	[Association(Storage = null, OtherKey = "CategoryID", Name = " FK_PROD_CATG                   ")]
	[DebuggerNonUserCode]
	public EntitySet<Products> Products
	{
		get;set;
	}


	#endregion

}

[Table(Name = " Foo .CUSTOMERS")]
public partial class Customers : INotifyPropertyChanged
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
	[Column(Storage = "_address", Name = "ADDRESS", DbType = null)]
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
	[Column(Storage = "_city", Name = "CITY", DbType = null)]
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
	[Column(Storage = "_companyName", Name = "COMPANYNAME", DbType = null, CanBeNull = false)]
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
	[Column(Storage = "_contactName", Name = "CONTACTNAME", DbType = null)]
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
	[Column(Storage = "_contactTitle", Name = "CONTACTTITLE", DbType = null)]
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
	[Column(Storage = "_country", Name = "COUNTRY", DbType = null)]
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
	[Column(Storage = "_customerID", Name = "CUSTOMERID", DbType = null, IsPrimaryKey = true, CanBeNull = false)]
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
	[Column(Storage = "_fax", Name = "FAX", DbType = null)]
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
	[Column(Storage = "_phone", Name = "PHONE", DbType = null)]
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
	[Column(Storage = "_postalCode", Name = "POSTALCODE", DbType = null)]
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
	[Column(Storage = "_region", Name = "REGION", DbType = null)]
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

	[Association(Storage = null, OtherKey = "CustomerID", Name = " FK_ORDERS_CUST                 ")]
	[DebuggerNonUserCode]
	public EntitySet<Orders> Orders
	{
		get;set;
	}


	#endregion

}

[Table(Name = " Foo .EMPLOYEES")]
public partial class Employees : INotifyPropertyChanged
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
	[Column(Storage = "_address", Name = "ADDRESS", DbType = null)]
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
	[Column(Storage = "_birthDate", Name = "BIRTHDATE", DbType = null)]
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
	[Column(Storage = "_city", Name = "CITY", DbType = null)]
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
	[Column(Storage = "_country", Name = "COUNTRY", DbType = null)]
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
	[Column(Storage = "_employeeID", Name = "EMPLOYEEID", DbType = null, IsPrimaryKey = true, CanBeNull = false)]
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
	[Column(Storage = "_extension", Name = "EXTENSION", DbType = null)]
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
	[Column(Storage = "_firstName", Name = "FIRSTNAME", DbType = null, CanBeNull = false)]
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
	[Column(Storage = "_hireDate", Name = "HIREDATE", DbType = null)]
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
	[Column(Storage = "_homePhone", Name = "HOMEPHONE", DbType = null)]
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
	[Column(Storage = "_lastName", Name = "LASTNAME", DbType = null, CanBeNull = false)]
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

	#region Byte[] Notes

	private Byte[] _notes;
	[DebuggerNonUserCode]
	[Column(Storage = "_notes", Name = "NOTES", DbType = null)]
	public Byte[] Notes
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
	[Column(Storage = "_photo", Name = "PHOTO", DbType = null)]
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
	[Column(Storage = "_photoPath", Name = "PHOTOPATH", DbType = null)]
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
	[Column(Storage = "_postalCode", Name = "POSTALCODE", DbType = null)]
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
	[Column(Storage = "_region", Name = "REGION", DbType = null)]
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
	[Column(Storage = "_reportsTo", Name = "REPORTSTO", DbType = null)]
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
				_reportsTo = value;
				OnPropertyChanged("ReportsTo");
			}
		}
	}

	#endregion

	#region string Title

	private string _title;
	[DebuggerNonUserCode]
	[Column(Storage = "_title", Name = "TITLE", DbType = null)]
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
	[Column(Storage = "_titleOfCourtesy", Name = "TITLEOFCOURTESY", DbType = null)]
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

	[Association(Storage = null, OtherKey = "EmployeeID", Name = " FK_EMPTERR_EMP                 ")]
	[DebuggerNonUserCode]
	public EntitySet<EmployeeTerritories> EmployeeTerritories
	{
		get;set;
	}

	[Association(Storage = null, OtherKey = "EmployeeID", Name = " FK_ORDERS_EMP                  ")]
	[DebuggerNonUserCode]
	public EntitySet<Orders> Orders
	{
		get;set;
	}


	#endregion

}

[Table(Name = " Foo .EMPLOYEETERRITORIES")]
public partial class EmployeeTerritories : INotifyPropertyChanged
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
	[Column(Storage = "_employeeID", Name = "EMPLOYEEID", DbType = null, IsPrimaryKey = true, CanBeNull = false)]
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

	#region string TerritoryID

	private string _territoryID;
	[DebuggerNonUserCode]
	[Column(Storage = "_territoryID", Name = "TERRITORYID", DbType = null, IsPrimaryKey = true, CanBeNull = false)]
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

	#region Parents

	private EntityRef<Employees> _employees;
	[Association(Storage = "_employees", ThisKey = "EmployeeID", Name = " FK_EMPTERR_EMP                 ", IsForeignKey = true)]
	[DebuggerNonUserCode]
	public Employees Employees
	{
		get
		{
			return _employees.Entity;
		}
		set
		{
			_employees.Entity = value;
		}
	}

	private EntityRef<Territories> _territories;
	[Association(Storage = "_territories", ThisKey = "TerritoryID", Name = " FK_EMPTERR_TERR                ", IsForeignKey = true)]
	[DebuggerNonUserCode]
	public Territories Territories
	{
		get
		{
			return _territories.Entity;
		}
		set
		{
			_territories.Entity = value;
		}
	}


	#endregion

}

[Table(Name = " Foo . Order Details ")]
public partial class OrderDetails : INotifyPropertyChanged
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
	[Column(Storage = "_discount", Name = "DISCOUNT", DbType = null, CanBeNull = false)]
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
	[Column(Storage = "_orderID", Name = "ORDERID", DbType = null, IsPrimaryKey = true, CanBeNull = false)]
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

	#region int ProductID

	private int _productID;
	[DebuggerNonUserCode]
	[Column(Storage = "_productID", Name = "PRODUCTID", DbType = null, IsPrimaryKey = true, CanBeNull = false)]
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

	#region short Quantity

	private short _quantity;
	[DebuggerNonUserCode]
	[Column(Storage = "_quantity", Name = "QUANTITY", DbType = null, CanBeNull = false)]
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

	#region int UnitPrice

	private int _unitPrice;
	[DebuggerNonUserCode]
	[Column(Storage = "_unitPrice", Name = "UNITPRICE", DbType = null, CanBeNull = false)]
	public int UnitPrice
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

	private EntityRef<Orders> _orders;
	[Association(Storage = "_orders", ThisKey = "OrderID", Name = " FK_ORDERDET_ORD                ", IsForeignKey = true)]
	[DebuggerNonUserCode]
	public Orders Orders
	{
		get
		{
			return _orders.Entity;
		}
		set
		{
			_orders.Entity = value;
		}
	}

	private EntityRef<Products> _products;
	[Association(Storage = "_products", ThisKey = "ProductID", Name = " FK_ORDERDET_PROD               ", IsForeignKey = true)]
	[DebuggerNonUserCode]
	public Products Products
	{
		get
		{
			return _products.Entity;
		}
		set
		{
			_products.Entity = value;
		}
	}


	#endregion

}

[Table(Name = " Foo .ORDERS")]
public partial class Orders : INotifyPropertyChanged
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
	[Column(Storage = "_customerID", Name = "CUSTOMERID", DbType = null)]
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

	#region int? EmployeeID

	private int? _employeeID;
	[DebuggerNonUserCode]
	[Column(Storage = "_employeeID", Name = "EMPLOYEEID", DbType = null)]
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
				_employeeID = value;
				OnPropertyChanged("EmployeeID");
			}
		}
	}

	#endregion

	#region int? Freight

	private int? _freight;
	[DebuggerNonUserCode]
	[Column(Storage = "_freight", Name = "FREIGHT", DbType = null)]
	public int? Freight
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
	[Column(Storage = "_orderDate", Name = "ORDERDATE", DbType = null)]
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
	[Column(Storage = "_orderID", Name = "ORDERID", DbType = null, IsPrimaryKey = true, CanBeNull = false)]
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
	[Column(Storage = "_requiredDate", Name = "REQUIREDDATE", DbType = null)]
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
	[Column(Storage = "_shipAddress", Name = "SHIPADDRESS", DbType = null)]
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
	[Column(Storage = "_shipCity", Name = "SHIPCITY", DbType = null)]
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
	[Column(Storage = "_shipCountry", Name = "SHIPCOUNTRY", DbType = null)]
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
	[Column(Storage = "_shipName", Name = "SHIPNAME", DbType = null)]
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
	[Column(Storage = "_shippedDate", Name = "SHIPPEDDATE", DbType = null)]
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
	[Column(Storage = "_shipPostalCode", Name = "SHIPPOSTALCODE", DbType = null)]
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
	[Column(Storage = "_shipRegion", Name = "SHIPREGION", DbType = null)]
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
	[Column(Storage = "_shipVia", Name = "SHIPVIA", DbType = null)]
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

	[Association(Storage = null, OtherKey = "OrderID", Name = " FK_ORDERDET_ORD                ")]
	[DebuggerNonUserCode]
	public EntitySet<OrderDetails> OrderDetails
	{
		get;set;
	}


	#endregion

	#region Parents

	private EntityRef<Customers> _customers;
	[Association(Storage = "_customers", ThisKey = "CustomerID", Name = " FK_ORDERS_CUST                 ", IsForeignKey = true)]
	[DebuggerNonUserCode]
	public Customers Customers
	{
		get
		{
			return _customers.Entity;
		}
		set
		{
			_customers.Entity = value;
		}
	}

	private EntityRef<Employees> _employees;
	[Association(Storage = "_employees", ThisKey = "EmployeeID", Name = " FK_ORDERS_EMP                  ", IsForeignKey = true)]
	[DebuggerNonUserCode]
	public Employees Employees
	{
		get
		{
			return _employees.Entity;
		}
		set
		{
			_employees.Entity = value;
		}
	}


	#endregion

}

[Table(Name = " Foo .PRODUCTS")]
public partial class Products : INotifyPropertyChanged
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
	[Column(Storage = "_categoryID", Name = "CATEGORYID", DbType = null)]
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
				_categoryID = value;
				OnPropertyChanged("CategoryID");
			}
		}
	}

	#endregion

	#region short Discontinued

	private short _discontinued;
	[DebuggerNonUserCode]
	[Column(Storage = "_discontinued", Name = "DISCONTINUED", DbType = null, CanBeNull = false)]
	public short Discontinued
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
	[Column(Storage = "_productID", Name = "PRODUCTID", DbType = null, IsPrimaryKey = true, CanBeNull = false)]
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
	[Column(Storage = "_productName", Name = "PRODUCTNAME", DbType = null, CanBeNull = false)]
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
	[Column(Storage = "_quantityPerUnit", Name = "QUANTITYPERUNIT", DbType = null)]
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
	[Column(Storage = "_reorderLevel", Name = "REORDERLEVEL", DbType = null)]
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
	[Column(Storage = "_supplierID", Name = "SUPPLIERID", DbType = null)]
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
				_supplierID = value;
				OnPropertyChanged("SupplierID");
			}
		}
	}

	#endregion

	#region int? UnitPrice

	private int? _unitPrice;
	[DebuggerNonUserCode]
	[Column(Storage = "_unitPrice", Name = "UNITPRICE", DbType = null)]
	public int? UnitPrice
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
	[Column(Storage = "_unitsInStock", Name = "UNITSINSTOCK", DbType = null)]
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
	[Column(Storage = "_unitsOnOrder", Name = "UNITSONORDER", DbType = null)]
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

	[Association(Storage = null, OtherKey = "ProductID", Name = " FK_ORDERDET_PROD               ")]
	[DebuggerNonUserCode]
	public EntitySet<OrderDetails> OrderDetails
	{
		get;set;
	}


	#endregion

	#region Parents

	private EntityRef<Categories> _categories;
	[Association(Storage = "_categories", ThisKey = "CategoryID", Name = " FK_PROD_CATG                   ", IsForeignKey = true)]
	[DebuggerNonUserCode]
	public Categories Categories
	{
		get
		{
			return _categories.Entity;
		}
		set
		{
			_categories.Entity = value;
		}
	}

	private EntityRef<Suppliers> _suppliers;
	[Association(Storage = "_suppliers", ThisKey = "SupplierID", Name = " FK_PROD_SUPP                   ", IsForeignKey = true)]
	[DebuggerNonUserCode]
	public Suppliers Suppliers
	{
		get
		{
			return _suppliers.Entity;
		}
		set
		{
			_suppliers.Entity = value;
		}
	}


	#endregion

}

[Table(Name = " Foo .REGION")]
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
	[Column(Storage = "_regionDescription", Name = "REGIONDESCRIPTION", DbType = null, CanBeNull = false)]
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
	[Column(Storage = "_regionID", Name = "REGIONID", DbType = null, IsPrimaryKey = true, CanBeNull = false)]
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

	[Association(Storage = null, OtherKey = "RegionID", Name = " FK_TERR_REGION                 ")]
	[DebuggerNonUserCode]
	public EntitySet<Territories> Territories
	{
		get;set;
	}


	#endregion

}

[Table(Name = " Foo .SHIPPERS")]
public partial class Shippers : INotifyPropertyChanged
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
	[Column(Storage = "_companyName", Name = "COMPANYNAME", DbType = null, CanBeNull = false)]
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
	[Column(Storage = "_phone", Name = "PHONE", DbType = null)]
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
	[Column(Storage = "_shipperID", Name = "SHIPPERID", DbType = null, IsPrimaryKey = true, CanBeNull = false)]
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

}

[Table(Name = " Foo .SUPPLIERS")]
public partial class Suppliers : INotifyPropertyChanged
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
	[Column(Storage = "_address", Name = "ADDRESS", DbType = null)]
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
	[Column(Storage = "_city", Name = "CITY", DbType = null)]
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
	[Column(Storage = "_companyName", Name = "COMPANYNAME", DbType = null, CanBeNull = false)]
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
	[Column(Storage = "_contactName", Name = "CONTACTNAME", DbType = null)]
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
	[Column(Storage = "_contactTitle", Name = "CONTACTTITLE", DbType = null)]
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
	[Column(Storage = "_country", Name = "COUNTRY", DbType = null)]
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
	[Column(Storage = "_fax", Name = "FAX", DbType = null)]
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
	[Column(Storage = "_phone", Name = "PHONE", DbType = null)]
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
	[Column(Storage = "_postalCode", Name = "POSTALCODE", DbType = null)]
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
	[Column(Storage = "_region", Name = "REGION", DbType = null)]
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
	[Column(Storage = "_supplierID", Name = "SUPPLIERID", DbType = null, IsPrimaryKey = true, CanBeNull = false)]
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

	[Association(Storage = null, OtherKey = "SupplierID", Name = " FK_PROD_SUPP                   ")]
	[DebuggerNonUserCode]
	public EntitySet<Products> Products
	{
		get;set;
	}


	#endregion

}

[Table(Name = " Foo .TERRITORIES")]
public partial class Territories : INotifyPropertyChanged
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
	[Column(Storage = "_regionID", Name = "REGIONID", DbType = null, CanBeNull = false)]
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

	#region string TerritoryDescription

	private string _territoryDescription;
	[DebuggerNonUserCode]
	[Column(Storage = "_territoryDescription", Name = "TERRITORYDESCRIPTION", DbType = null, CanBeNull = false)]
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
	[Column(Storage = "_territoryID", Name = "TERRITORYID", DbType = null, IsPrimaryKey = true, CanBeNull = false)]
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

	[Association(Storage = null, OtherKey = "TerritoryID", Name = " FK_EMPTERR_TERR                ")]
	[DebuggerNonUserCode]
	public EntitySet<EmployeeTerritories> EmployeeTerritories
	{
		get;set;
	}


	#endregion

	#region Parents

	private EntityRef<Region> _region;
	[Association(Storage = "_region", ThisKey = "RegionID", Name = " FK_TERR_REGION                 ", IsForeignKey = true)]
	[DebuggerNonUserCode]
	public Region Region
	{
		get
		{
			return _region.Entity;
		}
		set
		{
			_region.Entity = value;
		}
	}


	#endregion

}
