// 
//  ____  _     __  __      _        _ 
// |  _ \| |__ |  \/  | ___| |_ __ _| |
// | | | | '_ \| |\/| |/ _ \ __/ _` | |
// | |_| | |_) | |  | |  __/ || (_| | |
// |____/|_.__/|_|  |_|\___|\__\__,_|_|
//
// Auto-generated from Northwind on [TIMESTAMP].
// Please visit http://code.google.com/p/dblinq2007/ for more information.
//
namespace nwind
{
	using System;
	using System.ComponentModel;
	using System.Data;
#if MONO_STRICT
	using System.Data.Linq;
#else   // MONO_STRICT
	using DbLinq.Data.Linq;
	using DbLinq.Vendor;
#endif  // MONO_STRICT
	using System.Data.Linq.Mapping;
	using System.Diagnostics;
	
	
	public partial class Northwind : DataContext
	{
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		#endregion
		
		
		public Northwind(string connectionString) : 
				base(connectionString)
		{
			this.OnCreated();
		}
		
		public Northwind(string connection, MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			this.OnCreated();
		}
		
		public Northwind(IDbConnection connection, MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			this.OnCreated();
		}
		
		public Table<Category> Categories
		{
			get
			{
				return this.GetTable<Category>();
			}
		}
		
		public Table<Customer> Customers
		{
			get
			{
				return this.GetTable<Customer>();
			}
		}
		
		public Table<CustomerCustomerDemo> CustomerCustomerDemo
		{
			get
			{
				return this.GetTable<CustomerCustomerDemo>();
			}
		}
		
		public Table<CustomerDemographic> CustomerDemographics
		{
			get
			{
				return this.GetTable<CustomerDemographic>();
			}
		}
		
		public Table<Employee> Employees
		{
			get
			{
				return this.GetTable<Employee>();
			}
		}
		
		public Table<EmployeeTerritory> EmployeeTerritories
		{
			get
			{
				return this.GetTable<EmployeeTerritory>();
			}
		}
		
		public Table<Order> Orders
		{
			get
			{
				return this.GetTable<Order>();
			}
		}
		
		public Table<OrderDetail> OrderDetails
		{
			get
			{
				return this.GetTable<OrderDetail>();
			}
		}
		
		public Table<Product> Products
		{
			get
			{
				return this.GetTable<Product>();
			}
		}
		
		public Table<Region> Regions
		{
			get
			{
				return this.GetTable<Region>();
			}
		}
		
		public Table<Shipper> Shippers
		{
			get
			{
				return this.GetTable<Shipper>();
			}
		}
		
		public Table<Supplier> Suppliers
		{
			get
			{
				return this.GetTable<Supplier>();
			}
		}
		
		public Table<Territory> Territories
		{
			get
			{
				return this.GetTable<Territory>();
			}
		}
	}
	
	#region Start MONO_STRICT
#if MONO_STRICT

	public partial class Northwind
	{
		
		public Northwind(IDbConnection connection) : 
				base(connection)
		{
			this.OnCreated();
		}
	}
	#region End MONO_STRICT
	#endregion
#else     // MONO_STRICT
	
	public partial class Northwind
	{
		
		public Northwind(IDbConnection connection) : 
				base(connection, new DbLinq.Sqlite.SqliteVendor())
		{
			this.OnCreated();
		}
		
		public Northwind(IDbConnection connection, IVendor sqlDialect) : 
				base(connection, sqlDialect)
		{
			this.OnCreated();
		}
		
		public Northwind(IDbConnection connection, MappingSource mappingSource, IVendor sqlDialect) : 
				base(connection, mappingSource, sqlDialect)
		{
			this.OnCreated();
		}
	}
	#region End Not MONO_STRICT
	#endregion
#endif     // MONO_STRICT
	#endregion
	
	[Table(Name="main.Categories")]
	public partial class Category : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged, System.IEquatable<Category>
	{
		
		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");
		
		private int _categoryID;
		
		private string _categoryName;
		
		private string _description;
		
		private byte[] _picture;
		
		private EntitySet<Product> _products;
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		
		partial void OnCategoryIDChanged();
		
		partial void OnCategoryIDChanging(int value);
		
		partial void OnCategoryNameChanged();
		
		partial void OnCategoryNameChanging(string value);
		
		partial void OnDescriptionChanged();
		
		partial void OnDescriptionChanging(string value);
		
		partial void OnPictureChanged();
		
		partial void OnPictureChanging(byte[] value);
		#endregion
		
		
		public Category()
		{
			_products = new EntitySet<Product>(new Action<Product>(this.Products_Attach), new Action<Product>(this.Products_Detach));
			this.OnCreated();
		}
		
		[Column(Storage="_categoryID", Name="CategoryID", DbType="INTEGER", IsPrimaryKey=true, IsDbGenerated=true, AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int CategoryID
		{
			get
			{
				return this._categoryID;
			}
			set
			{
				if ((_categoryID != value))
				{
					this.OnCategoryIDChanging(value);
					this.SendPropertyChanging();
					this._categoryID = value;
					this.SendPropertyChanged("CategoryID");
					this.OnCategoryIDChanged();
				}
			}
		}
		
		[Column(Storage="_categoryName", Name="CategoryName", DbType="nvarchar (15)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public string CategoryName
		{
			get
			{
				return this._categoryName;
			}
			set
			{
				if (((_categoryName == value) 
							== false))
				{
					this.OnCategoryNameChanging(value);
					this.SendPropertyChanging();
					this._categoryName = value;
					this.SendPropertyChanged("CategoryName");
					this.OnCategoryNameChanged();
				}
			}
		}
		
		[Column(Storage="_description", Name="Description", DbType="ntext", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string Description
		{
			get
			{
				return this._description;
			}
			set
			{
				if (((_description == value) 
							== false))
				{
					this.OnDescriptionChanging(value);
					this.SendPropertyChanging();
					this._description = value;
					this.SendPropertyChanged("Description");
					this.OnDescriptionChanged();
				}
			}
		}
		
		[Column(Storage="_picture", Name="Picture", DbType="image", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public byte[] Picture
		{
			get
			{
				return this._picture;
			}
			set
			{
				if (((_picture == value) 
							== false))
				{
					this.OnPictureChanging(value);
					this.SendPropertyChanging();
					this._picture = value;
					this.SendPropertyChanged("Picture");
					this.OnPictureChanged();
				}
			}
		}
		
		#region Children
		[Association(Storage="_products", OtherKey="CategoryID", ThisKey="CategoryID", Name="fk_Products_1")]
		[DebuggerNonUserCode()]
		public EntitySet<Product> Products
		{
			get
			{
				return this._products;
			}
			set
			{
				this._products = value;
			}
		}
		#endregion
		
		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
		
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null))
			{
				h(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null))
			{
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
		
		public override int GetHashCode()
		{
			int hc = 0;
			hc = (hc 
						| (_categoryID.GetHashCode() * 1));
			return hc;
		}
		
		public override bool Equals(object value)
		{
			if ((value == null))
			{
				return false;
			}
			if (((value.GetType() == this.GetType()) 
						== false))
			{
				return false;
			}
			Category other = ((Category)(value));
			return this.Equals(other);
		}
		
		public virtual bool Equals(Category value)
		{
			if ((value == null))
			{
				return false;
			}
			return System.Collections.Generic.EqualityComparer<int>.Default.Equals(this._categoryID, value._categoryID);
		}
		
		#region Attachment handlers
		private void Products_Attach(Product entity)
		{
			this.SendPropertyChanging();
			entity.Category = this;
		}
		
		private void Products_Detach(Product entity)
		{
			this.SendPropertyChanging();
			entity.Category = null;
		}
		#endregion
	}
	
	[Table(Name="main.Customers")]
	public partial class Customer : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged, System.IEquatable<Customer>
	{
		
		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");
		
		private string _address;
		
		private string _city;
		
		private string _companyName;
		
		private string _contactName;
		
		private string _contactTitle;
		
		private string _country;
		
		private string _customerID;
		
		private string _fax;
		
		private string _phone;
		
		private string _postalCode;
		
		private string _region;
		
		private EntitySet<CustomerCustomerDemo> _customerCustomerDemo;
		
		private EntitySet<Order> _orders;
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		
		partial void OnAddressChanged();
		
		partial void OnAddressChanging(string value);
		
		partial void OnCityChanged();
		
		partial void OnCityChanging(string value);
		
		partial void OnCompanyNameChanged();
		
		partial void OnCompanyNameChanging(string value);
		
		partial void OnContactNameChanged();
		
		partial void OnContactNameChanging(string value);
		
		partial void OnContactTitleChanged();
		
		partial void OnContactTitleChanging(string value);
		
		partial void OnCountryChanged();
		
		partial void OnCountryChanging(string value);
		
		partial void OnCustomerIDChanged();
		
		partial void OnCustomerIDChanging(string value);
		
		partial void OnFaxChanged();
		
		partial void OnFaxChanging(string value);
		
		partial void OnPhoneChanged();
		
		partial void OnPhoneChanging(string value);
		
		partial void OnPostalCodeChanged();
		
		partial void OnPostalCodeChanging(string value);
		
		partial void OnRegionChanged();
		
		partial void OnRegionChanging(string value);
		#endregion
		
		
		public Customer()
		{
			_customerCustomerDemo = new EntitySet<CustomerCustomerDemo>(new Action<CustomerCustomerDemo>(this.CustomerCustomerDemo_Attach), new Action<CustomerCustomerDemo>(this.CustomerCustomerDemo_Detach));
			_orders = new EntitySet<Order>(new Action<Order>(this.Orders_Attach), new Action<Order>(this.Orders_Detach));
			this.OnCreated();
		}
		
		[Column(Storage="_address", Name="Address", DbType="nvarchar (60)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string Address
		{
			get
			{
				return this._address;
			}
			set
			{
				if (((_address == value) 
							== false))
				{
					this.OnAddressChanging(value);
					this.SendPropertyChanging();
					this._address = value;
					this.SendPropertyChanged("Address");
					this.OnAddressChanged();
				}
			}
		}
		
		[Column(Storage="_city", Name="City", DbType="nvarchar (15)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string City
		{
			get
			{
				return this._city;
			}
			set
			{
				if (((_city == value) 
							== false))
				{
					this.OnCityChanging(value);
					this.SendPropertyChanging();
					this._city = value;
					this.SendPropertyChanged("City");
					this.OnCityChanged();
				}
			}
		}
		
		[Column(Storage="_companyName", Name="CompanyName", DbType="nvarchar (40)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public string CompanyName
		{
			get
			{
				return this._companyName;
			}
			set
			{
				if (((_companyName == value) 
							== false))
				{
					this.OnCompanyNameChanging(value);
					this.SendPropertyChanging();
					this._companyName = value;
					this.SendPropertyChanged("CompanyName");
					this.OnCompanyNameChanged();
				}
			}
		}
		
		[Column(Storage="_contactName", Name="ContactName", DbType="nvarchar (30)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string ContactName
		{
			get
			{
				return this._contactName;
			}
			set
			{
				if (((_contactName == value) 
							== false))
				{
					this.OnContactNameChanging(value);
					this.SendPropertyChanging();
					this._contactName = value;
					this.SendPropertyChanged("ContactName");
					this.OnContactNameChanged();
				}
			}
		}
		
		[Column(Storage="_contactTitle", Name="ContactTitle", DbType="nvarchar (30)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string ContactTitle
		{
			get
			{
				return this._contactTitle;
			}
			set
			{
				if (((_contactTitle == value) 
							== false))
				{
					this.OnContactTitleChanging(value);
					this.SendPropertyChanging();
					this._contactTitle = value;
					this.SendPropertyChanged("ContactTitle");
					this.OnContactTitleChanged();
				}
			}
		}
		
		[Column(Storage="_country", Name="Country", DbType="nvarchar (15)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string Country
		{
			get
			{
				return this._country;
			}
			set
			{
				if (((_country == value) 
							== false))
				{
					this.OnCountryChanging(value);
					this.SendPropertyChanging();
					this._country = value;
					this.SendPropertyChanged("Country");
					this.OnCountryChanged();
				}
			}
		}
		
		[Column(Storage="_customerID", Name="CustomerID", DbType="nchar (5)", IsPrimaryKey=true, AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public string CustomerID
		{
			get
			{
				return this._customerID;
			}
			set
			{
				if (((_customerID == value) 
							== false))
				{
					this.OnCustomerIDChanging(value);
					this.SendPropertyChanging();
					this._customerID = value;
					this.SendPropertyChanged("CustomerID");
					this.OnCustomerIDChanged();
				}
			}
		}
		
		[Column(Storage="_fax", Name="Fax", DbType="nvarchar (24)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string Fax
		{
			get
			{
				return this._fax;
			}
			set
			{
				if (((_fax == value) 
							== false))
				{
					this.OnFaxChanging(value);
					this.SendPropertyChanging();
					this._fax = value;
					this.SendPropertyChanged("Fax");
					this.OnFaxChanged();
				}
			}
		}
		
		[Column(Storage="_phone", Name="Phone", DbType="nvarchar (24)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string Phone
		{
			get
			{
				return this._phone;
			}
			set
			{
				if (((_phone == value) 
							== false))
				{
					this.OnPhoneChanging(value);
					this.SendPropertyChanging();
					this._phone = value;
					this.SendPropertyChanged("Phone");
					this.OnPhoneChanged();
				}
			}
		}
		
		[Column(Storage="_postalCode", Name="PostalCode", DbType="nvarchar (10)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string PostalCode
		{
			get
			{
				return this._postalCode;
			}
			set
			{
				if (((_postalCode == value) 
							== false))
				{
					this.OnPostalCodeChanging(value);
					this.SendPropertyChanging();
					this._postalCode = value;
					this.SendPropertyChanged("PostalCode");
					this.OnPostalCodeChanged();
				}
			}
		}
		
		[Column(Storage="_region", Name="Region", DbType="nvarchar (15)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string Region
		{
			get
			{
				return this._region;
			}
			set
			{
				if (((_region == value) 
							== false))
				{
					this.OnRegionChanging(value);
					this.SendPropertyChanging();
					this._region = value;
					this.SendPropertyChanged("Region");
					this.OnRegionChanged();
				}
			}
		}
		
		#region Children
		[Association(Storage="_customerCustomerDemo", OtherKey="CustomerID", ThisKey="CustomerID", Name="fk_CustomerCustomerDemo_0")]
		[DebuggerNonUserCode()]
		public EntitySet<CustomerCustomerDemo> CustomerCustomerDemo
		{
			get
			{
				return this._customerCustomerDemo;
			}
			set
			{
				this._customerCustomerDemo = value;
			}
		}
		
		[Association(Storage="_orders", OtherKey="CustomerID", ThisKey="CustomerID", Name="fk_Orders_2")]
		[DebuggerNonUserCode()]
		public EntitySet<Order> Orders
		{
			get
			{
				return this._orders;
			}
			set
			{
				this._orders = value;
			}
		}
		#endregion
		
		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
		
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null))
			{
				h(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null))
			{
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
		
		public override int GetHashCode()
		{
			int hc = 0;
			if ((_customerID != null))
			{
				hc = (hc 
							| (_customerID.GetHashCode() * 1));
			}
			return hc;
		}
		
		public override bool Equals(object value)
		{
			if ((value == null))
			{
				return false;
			}
			if (((value.GetType() == this.GetType()) 
						== false))
			{
				return false;
			}
			Customer other = ((Customer)(value));
			return this.Equals(other);
		}
		
		public virtual bool Equals(Customer value)
		{
			if ((value == null))
			{
				return false;
			}
			return System.Collections.Generic.EqualityComparer<string>.Default.Equals(this._customerID, value._customerID);
		}
		
		#region Attachment handlers
		private void CustomerCustomerDemo_Attach(CustomerCustomerDemo entity)
		{
			this.SendPropertyChanging();
			entity.Customer = this;
		}
		
		private void CustomerCustomerDemo_Detach(CustomerCustomerDemo entity)
		{
			this.SendPropertyChanging();
			entity.Customer = null;
		}
		
		private void Orders_Attach(Order entity)
		{
			this.SendPropertyChanging();
			entity.Customer = this;
		}
		
		private void Orders_Detach(Order entity)
		{
			this.SendPropertyChanging();
			entity.Customer = null;
		}
		#endregion
	}
	
	[Table(Name="main.CustomerCustomerDemo")]
	public partial class CustomerCustomerDemo : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged, System.IEquatable<CustomerCustomerDemo>
	{
		
		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");
		
		private string _customerID;
		
		private string _customerTypeID;
		
		private EntityRef<Customer> _customer = new EntityRef<Customer>();
		
		private EntityRef<CustomerDemographic> _customerDemographic = new EntityRef<CustomerDemographic>();
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		
		partial void OnCustomerIDChanged();
		
		partial void OnCustomerIDChanging(string value);
		
		partial void OnCustomerTypeIDChanged();
		
		partial void OnCustomerTypeIDChanging(string value);
		#endregion
		
		
		public CustomerCustomerDemo()
		{
			this.OnCreated();
		}
		
		[Column(Storage="_customerID", Name="CustomerID", DbType="nchar (5)", IsPrimaryKey=true, AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public string CustomerID
		{
			get
			{
				return this._customerID;
			}
			set
			{
				if (((_customerID == value) 
							== false))
				{
					if (_customer.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnCustomerIDChanging(value);
					this.SendPropertyChanging();
					this._customerID = value;
					this.SendPropertyChanged("CustomerID");
					this.OnCustomerIDChanged();
				}
			}
		}
		
		[Column(Storage="_customerTypeID", Name="CustomerTypeID", DbType="nchar", IsPrimaryKey=true, AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public string CustomerTypeID
		{
			get
			{
				return this._customerTypeID;
			}
			set
			{
				if (((_customerTypeID == value) 
							== false))
				{
					if (_customerDemographic.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnCustomerTypeIDChanging(value);
					this.SendPropertyChanging();
					this._customerTypeID = value;
					this.SendPropertyChanged("CustomerTypeID");
					this.OnCustomerTypeIDChanged();
				}
			}
		}
		
		#region Parents
		[Association(Storage="_customer", OtherKey="CustomerID", ThisKey="CustomerID", Name="fk_CustomerCustomerDemo_0", IsForeignKey=true)]
		[DebuggerNonUserCode()]
		public Customer Customer
		{
			get
			{
				return this._customer.Entity;
			}
			set
			{
				if (((this._customer.Entity == value) 
							== false))
				{
					if ((this._customer.Entity != null))
					{
						Customer previousCustomer = this._customer.Entity;
						this._customer.Entity = null;
						previousCustomer.CustomerCustomerDemo.Remove(this);
					}
					this._customer.Entity = value;
					if ((value != null))
					{
						value.CustomerCustomerDemo.Add(this);
						_customerID = value.CustomerID;
					}
					else
					{
						_customerID = default(string);
					}
				}
			}
		}
		
		[Association(Storage="_customerDemographic", OtherKey="CustomerTypeID", ThisKey="CustomerTypeID", Name="fk_CustomerCustomerDemo_1", IsForeignKey=true)]
		[DebuggerNonUserCode()]
		public CustomerDemographic CustomerDemographic
		{
			get
			{
				return this._customerDemographic.Entity;
			}
			set
			{
				if (((this._customerDemographic.Entity == value) 
							== false))
				{
					if ((this._customerDemographic.Entity != null))
					{
						CustomerDemographic previousCustomerDemographic = this._customerDemographic.Entity;
						this._customerDemographic.Entity = null;
						previousCustomerDemographic.CustomerCustomerDemo.Remove(this);
					}
					this._customerDemographic.Entity = value;
					if ((value != null))
					{
						value.CustomerCustomerDemo.Add(this);
						_customerTypeID = value.CustomerTypeID;
					}
					else
					{
						_customerTypeID = default(string);
					}
				}
			}
		}
		#endregion
		
		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
		
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null))
			{
				h(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null))
			{
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
		
		public override int GetHashCode()
		{
			int hc = 0;
			if ((_customerID != null))
			{
				hc = (hc 
							| (_customerID.GetHashCode() * 1));
			}
			if ((_customerTypeID != null))
			{
				hc = (hc 
							| (_customerTypeID.GetHashCode() * 65536));
			}
			return hc;
		}
		
		public override bool Equals(object value)
		{
			if ((value == null))
			{
				return false;
			}
			if (((value.GetType() == this.GetType()) 
						== false))
			{
				return false;
			}
			CustomerCustomerDemo other = ((CustomerCustomerDemo)(value));
			return this.Equals(other);
		}
		
		public virtual bool Equals(CustomerCustomerDemo value)
		{
			if ((value == null))
			{
				return false;
			}
			return (System.Collections.Generic.EqualityComparer<string>.Default.Equals(this._customerID, value._customerID) && System.Collections.Generic.EqualityComparer<string>.Default.Equals(this._customerTypeID, value._customerTypeID));
		}
	}
	
	[Table(Name="main.CustomerDemographics")]
	public partial class CustomerDemographic : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged, System.IEquatable<CustomerDemographic>
	{
		
		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");
		
		private string _customerDesc;
		
		private string _customerTypeID;
		
		private EntitySet<CustomerCustomerDemo> _customerCustomerDemo;
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		
		partial void OnCustomerDescChanged();
		
		partial void OnCustomerDescChanging(string value);
		
		partial void OnCustomerTypeIDChanged();
		
		partial void OnCustomerTypeIDChanging(string value);
		#endregion
		
		
		public CustomerDemographic()
		{
			_customerCustomerDemo = new EntitySet<CustomerCustomerDemo>(new Action<CustomerCustomerDemo>(this.CustomerCustomerDemo_Attach), new Action<CustomerCustomerDemo>(this.CustomerCustomerDemo_Detach));
			this.OnCreated();
		}
		
		[Column(Storage="_customerDesc", Name="CustomerDesc", DbType="ntext", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string CustomerDesc
		{
			get
			{
				return this._customerDesc;
			}
			set
			{
				if (((_customerDesc == value) 
							== false))
				{
					this.OnCustomerDescChanging(value);
					this.SendPropertyChanging();
					this._customerDesc = value;
					this.SendPropertyChanged("CustomerDesc");
					this.OnCustomerDescChanged();
				}
			}
		}
		
		[Column(Storage="_customerTypeID", Name="CustomerTypeID", DbType="nchar", IsPrimaryKey=true, AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public string CustomerTypeID
		{
			get
			{
				return this._customerTypeID;
			}
			set
			{
				if (((_customerTypeID == value) 
							== false))
				{
					this.OnCustomerTypeIDChanging(value);
					this.SendPropertyChanging();
					this._customerTypeID = value;
					this.SendPropertyChanged("CustomerTypeID");
					this.OnCustomerTypeIDChanged();
				}
			}
		}
		
		#region Children
		[Association(Storage="_customerCustomerDemo", OtherKey="CustomerTypeID", ThisKey="CustomerTypeID", Name="fk_CustomerCustomerDemo_1")]
		[DebuggerNonUserCode()]
		public EntitySet<CustomerCustomerDemo> CustomerCustomerDemo
		{
			get
			{
				return this._customerCustomerDemo;
			}
			set
			{
				this._customerCustomerDemo = value;
			}
		}
		#endregion
		
		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
		
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null))
			{
				h(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null))
			{
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
		
		public override int GetHashCode()
		{
			int hc = 0;
			if ((_customerTypeID != null))
			{
				hc = (hc 
							| (_customerTypeID.GetHashCode() * 1));
			}
			return hc;
		}
		
		public override bool Equals(object value)
		{
			if ((value == null))
			{
				return false;
			}
			if (((value.GetType() == this.GetType()) 
						== false))
			{
				return false;
			}
			CustomerDemographic other = ((CustomerDemographic)(value));
			return this.Equals(other);
		}
		
		public virtual bool Equals(CustomerDemographic value)
		{
			if ((value == null))
			{
				return false;
			}
			return System.Collections.Generic.EqualityComparer<string>.Default.Equals(this._customerTypeID, value._customerTypeID);
		}
		
		#region Attachment handlers
		private void CustomerCustomerDemo_Attach(CustomerCustomerDemo entity)
		{
			this.SendPropertyChanging();
			entity.CustomerDemographic = this;
		}
		
		private void CustomerCustomerDemo_Detach(CustomerCustomerDemo entity)
		{
			this.SendPropertyChanging();
			entity.CustomerDemographic = null;
		}
		#endregion
	}
	
	[Table(Name="main.Employees")]
	public partial class Employee : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged, System.IEquatable<Employee>
	{
		
		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");
		
		private string _address;
		
		private System.Nullable<System.DateTime> _birthDate;
		
		private string _city;
		
		private string _country;
		
		private int _employeeID;
		
		private string _extension;
		
		private string _firstName;
		
		private System.Nullable<System.DateTime> _hireDate;
		
		private string _homePhone;
		
		private string _lastName;
		
		private string _notes;
		
		private byte[] _photo;
		
		private string _photoPath;
		
		private string _postalCode;
		
		private string _region;
		
		private System.Nullable<int> _reportsTo;
		
		private string _title;
		
		private string _titleOfCourtesy;
		
		private EntitySet<EmployeeTerritory> _employeeTerritories;
		
		private EntitySet<Employee> _employees;
		
		private EntitySet<Order> _orders;
		
		private EntityRef<Employee> _reportsToEmployee = new EntityRef<Employee>();
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		
		partial void OnAddressChanged();
		
		partial void OnAddressChanging(string value);
		
		partial void OnBirthDateChanged();
		
		partial void OnBirthDateChanging(System.Nullable<System.DateTime> value);
		
		partial void OnCityChanged();
		
		partial void OnCityChanging(string value);
		
		partial void OnCountryChanged();
		
		partial void OnCountryChanging(string value);
		
		partial void OnEmployeeIDChanged();
		
		partial void OnEmployeeIDChanging(int value);
		
		partial void OnExtensionChanged();
		
		partial void OnExtensionChanging(string value);
		
		partial void OnFirstNameChanged();
		
		partial void OnFirstNameChanging(string value);
		
		partial void OnHireDateChanged();
		
		partial void OnHireDateChanging(System.Nullable<System.DateTime> value);
		
		partial void OnHomePhoneChanged();
		
		partial void OnHomePhoneChanging(string value);
		
		partial void OnLastNameChanged();
		
		partial void OnLastNameChanging(string value);
		
		partial void OnNotesChanged();
		
		partial void OnNotesChanging(string value);
		
		partial void OnPhotoChanged();
		
		partial void OnPhotoChanging(byte[] value);
		
		partial void OnPhotoPathChanged();
		
		partial void OnPhotoPathChanging(string value);
		
		partial void OnPostalCodeChanged();
		
		partial void OnPostalCodeChanging(string value);
		
		partial void OnRegionChanged();
		
		partial void OnRegionChanging(string value);
		
		partial void OnReportsToChanged();
		
		partial void OnReportsToChanging(System.Nullable<int> value);
		
		partial void OnTitleChanged();
		
		partial void OnTitleChanging(string value);
		
		partial void OnTitleOfCourtesyChanged();
		
		partial void OnTitleOfCourtesyChanging(string value);
		#endregion
		
		
		public Employee()
		{
			_employeeTerritories = new EntitySet<EmployeeTerritory>(new Action<EmployeeTerritory>(this.EmployeeTerritories_Attach), new Action<EmployeeTerritory>(this.EmployeeTerritories_Detach));
			_employees = new EntitySet<Employee>(new Action<Employee>(this.Employees_Attach), new Action<Employee>(this.Employees_Detach));
			_orders = new EntitySet<Order>(new Action<Order>(this.Orders_Attach), new Action<Order>(this.Orders_Detach));
			this.OnCreated();
		}
		
		[Column(Storage="_address", Name="Address", DbType="nvarchar (60)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string Address
		{
			get
			{
				return this._address;
			}
			set
			{
				if (((_address == value) 
							== false))
				{
					this.OnAddressChanging(value);
					this.SendPropertyChanging();
					this._address = value;
					this.SendPropertyChanged("Address");
					this.OnAddressChanged();
				}
			}
		}
		
		[Column(Storage="_birthDate", Name="BirthDate", DbType="datetime", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<System.DateTime> BirthDate
		{
			get
			{
				return this._birthDate;
			}
			set
			{
				if ((_birthDate != value))
				{
					this.OnBirthDateChanging(value);
					this.SendPropertyChanging();
					this._birthDate = value;
					this.SendPropertyChanged("BirthDate");
					this.OnBirthDateChanged();
				}
			}
		}
		
		[Column(Storage="_city", Name="City", DbType="nvarchar (15)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string City
		{
			get
			{
				return this._city;
			}
			set
			{
				if (((_city == value) 
							== false))
				{
					this.OnCityChanging(value);
					this.SendPropertyChanging();
					this._city = value;
					this.SendPropertyChanged("City");
					this.OnCityChanged();
				}
			}
		}
		
		[Column(Storage="_country", Name="Country", DbType="nvarchar (15)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string Country
		{
			get
			{
				return this._country;
			}
			set
			{
				if (((_country == value) 
							== false))
				{
					this.OnCountryChanging(value);
					this.SendPropertyChanging();
					this._country = value;
					this.SendPropertyChanged("Country");
					this.OnCountryChanged();
				}
			}
		}
		
		[Column(Storage="_employeeID", Name="EmployeeID", DbType="INTEGER", IsPrimaryKey=true, IsDbGenerated=true, AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int EmployeeID
		{
			get
			{
				return this._employeeID;
			}
			set
			{
				if ((_employeeID != value))
				{
					this.OnEmployeeIDChanging(value);
					this.SendPropertyChanging();
					this._employeeID = value;
					this.SendPropertyChanged("EmployeeID");
					this.OnEmployeeIDChanged();
				}
			}
		}
		
		[Column(Storage="_extension", Name="Extension", DbType="nvarchar (4)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string Extension
		{
			get
			{
				return this._extension;
			}
			set
			{
				if (((_extension == value) 
							== false))
				{
					this.OnExtensionChanging(value);
					this.SendPropertyChanging();
					this._extension = value;
					this.SendPropertyChanged("Extension");
					this.OnExtensionChanged();
				}
			}
		}
		
		[Column(Storage="_firstName", Name="FirstName", DbType="nvarchar (10)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public string FirstName
		{
			get
			{
				return this._firstName;
			}
			set
			{
				if (((_firstName == value) 
							== false))
				{
					this.OnFirstNameChanging(value);
					this.SendPropertyChanging();
					this._firstName = value;
					this.SendPropertyChanged("FirstName");
					this.OnFirstNameChanged();
				}
			}
		}
		
		[Column(Storage="_hireDate", Name="HireDate", DbType="datetime", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<System.DateTime> HireDate
		{
			get
			{
				return this._hireDate;
			}
			set
			{
				if ((_hireDate != value))
				{
					this.OnHireDateChanging(value);
					this.SendPropertyChanging();
					this._hireDate = value;
					this.SendPropertyChanged("HireDate");
					this.OnHireDateChanged();
				}
			}
		}
		
		[Column(Storage="_homePhone", Name="HomePhone", DbType="nvarchar (24)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string HomePhone
		{
			get
			{
				return this._homePhone;
			}
			set
			{
				if (((_homePhone == value) 
							== false))
				{
					this.OnHomePhoneChanging(value);
					this.SendPropertyChanging();
					this._homePhone = value;
					this.SendPropertyChanged("HomePhone");
					this.OnHomePhoneChanged();
				}
			}
		}
		
		[Column(Storage="_lastName", Name="LastName", DbType="nvarchar (20)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public string LastName
		{
			get
			{
				return this._lastName;
			}
			set
			{
				if (((_lastName == value) 
							== false))
				{
					this.OnLastNameChanging(value);
					this.SendPropertyChanging();
					this._lastName = value;
					this.SendPropertyChanged("LastName");
					this.OnLastNameChanged();
				}
			}
		}
		
		[Column(Storage="_notes", Name="Notes", DbType="ntext", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string Notes
		{
			get
			{
				return this._notes;
			}
			set
			{
				if (((_notes == value) 
							== false))
				{
					this.OnNotesChanging(value);
					this.SendPropertyChanging();
					this._notes = value;
					this.SendPropertyChanged("Notes");
					this.OnNotesChanged();
				}
			}
		}
		
		[Column(Storage="_photo", Name="Photo", DbType="image", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public byte[] Photo
		{
			get
			{
				return this._photo;
			}
			set
			{
				if (((_photo == value) 
							== false))
				{
					this.OnPhotoChanging(value);
					this.SendPropertyChanging();
					this._photo = value;
					this.SendPropertyChanged("Photo");
					this.OnPhotoChanged();
				}
			}
		}
		
		[Column(Storage="_photoPath", Name="PhotoPath", DbType="nvarchar (255)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string PhotoPath
		{
			get
			{
				return this._photoPath;
			}
			set
			{
				if (((_photoPath == value) 
							== false))
				{
					this.OnPhotoPathChanging(value);
					this.SendPropertyChanging();
					this._photoPath = value;
					this.SendPropertyChanged("PhotoPath");
					this.OnPhotoPathChanged();
				}
			}
		}
		
		[Column(Storage="_postalCode", Name="PostalCode", DbType="nvarchar (10)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string PostalCode
		{
			get
			{
				return this._postalCode;
			}
			set
			{
				if (((_postalCode == value) 
							== false))
				{
					this.OnPostalCodeChanging(value);
					this.SendPropertyChanging();
					this._postalCode = value;
					this.SendPropertyChanged("PostalCode");
					this.OnPostalCodeChanged();
				}
			}
		}
		
		[Column(Storage="_region", Name="Region", DbType="nvarchar (15)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string Region
		{
			get
			{
				return this._region;
			}
			set
			{
				if (((_region == value) 
							== false))
				{
					this.OnRegionChanging(value);
					this.SendPropertyChanging();
					this._region = value;
					this.SendPropertyChanged("Region");
					this.OnRegionChanged();
				}
			}
		}
		
		[Column(Storage="_reportsTo", Name="ReportsTo", DbType="INTEGER", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<int> ReportsTo
		{
			get
			{
				return this._reportsTo;
			}
			set
			{
				if ((_reportsTo != value))
				{
					if (_reportsToEmployee.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnReportsToChanging(value);
					this.SendPropertyChanging();
					this._reportsTo = value;
					this.SendPropertyChanged("ReportsTo");
					this.OnReportsToChanged();
				}
			}
		}
		
		[Column(Storage="_title", Name="Title", DbType="nvarchar (30)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string Title
		{
			get
			{
				return this._title;
			}
			set
			{
				if (((_title == value) 
							== false))
				{
					this.OnTitleChanging(value);
					this.SendPropertyChanging();
					this._title = value;
					this.SendPropertyChanged("Title");
					this.OnTitleChanged();
				}
			}
		}
		
		[Column(Storage="_titleOfCourtesy", Name="TitleOfCourtesy", DbType="nvarchar (25)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string TitleOfCourtesy
		{
			get
			{
				return this._titleOfCourtesy;
			}
			set
			{
				if (((_titleOfCourtesy == value) 
							== false))
				{
					this.OnTitleOfCourtesyChanging(value);
					this.SendPropertyChanging();
					this._titleOfCourtesy = value;
					this.SendPropertyChanged("TitleOfCourtesy");
					this.OnTitleOfCourtesyChanged();
				}
			}
		}
		
		#region Children
		[Association(Storage="_employeeTerritories", OtherKey="EmployeeID", ThisKey="EmployeeID", Name="fk_EmployeeTerritories_1")]
		[DebuggerNonUserCode()]
		public EntitySet<EmployeeTerritory> EmployeeTerritories
		{
			get
			{
				return this._employeeTerritories;
			}
			set
			{
				this._employeeTerritories = value;
			}
		}
		
		[Association(Storage="_employees", OtherKey="ReportsTo", ThisKey="EmployeeID", Name="fk_Employees_0")]
		[DebuggerNonUserCode()]
		public EntitySet<Employee> Employees
		{
			get
			{
				return this._employees;
			}
			set
			{
				this._employees = value;
			}
		}
		
		[Association(Storage="_orders", OtherKey="EmployeeID", ThisKey="EmployeeID", Name="fk_Orders_1")]
		[DebuggerNonUserCode()]
		public EntitySet<Order> Orders
		{
			get
			{
				return this._orders;
			}
			set
			{
				this._orders = value;
			}
		}
		#endregion
		
		#region Parents
		[Association(Storage="_reportsToEmployee", OtherKey="EmployeeID", ThisKey="ReportsTo", Name="fk_Employees_0", IsForeignKey=true)]
		[DebuggerNonUserCode()]
		public Employee ReportsToEmployee
		{
			get
			{
				return this._reportsToEmployee.Entity;
			}
			set
			{
				if (((this._reportsToEmployee.Entity == value) 
							== false))
				{
					if ((this._reportsToEmployee.Entity != null))
					{
						Employee previousEmployee = this._reportsToEmployee.Entity;
						this._reportsToEmployee.Entity = null;
						previousEmployee.Employees.Remove(this);
					}
					this._reportsToEmployee.Entity = value;
					if ((value != null))
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
		
		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
		
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null))
			{
				h(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null))
			{
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
		
		public override int GetHashCode()
		{
			int hc = 0;
			hc = (hc 
						| (_employeeID.GetHashCode() * 1));
			return hc;
		}
		
		public override bool Equals(object value)
		{
			if ((value == null))
			{
				return false;
			}
			if (((value.GetType() == this.GetType()) 
						== false))
			{
				return false;
			}
			Employee other = ((Employee)(value));
			return this.Equals(other);
		}
		
		public virtual bool Equals(Employee value)
		{
			if ((value == null))
			{
				return false;
			}
			return System.Collections.Generic.EqualityComparer<int>.Default.Equals(this._employeeID, value._employeeID);
		}
		
		#region Attachment handlers
		private void EmployeeTerritories_Attach(EmployeeTerritory entity)
		{
			this.SendPropertyChanging();
			entity.Employee = this;
		}
		
		private void EmployeeTerritories_Detach(EmployeeTerritory entity)
		{
			this.SendPropertyChanging();
			entity.Employee = null;
		}
		
		private void Employees_Attach(Employee entity)
		{
			this.SendPropertyChanging();
			entity.ReportsToEmployee = this;
		}
		
		private void Employees_Detach(Employee entity)
		{
			this.SendPropertyChanging();
			entity.ReportsToEmployee = null;
		}
		
		private void Orders_Attach(Order entity)
		{
			this.SendPropertyChanging();
			entity.Employee = this;
		}
		
		private void Orders_Detach(Order entity)
		{
			this.SendPropertyChanging();
			entity.Employee = null;
		}
		#endregion
	}
	
	[Table(Name="main.EmployeeTerritories")]
	public partial class EmployeeTerritory : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged, System.IEquatable<EmployeeTerritory>
	{
		
		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");
		
		private int _employeeID;
		
		private string _territoryID;
		
		private EntityRef<Territory> _territory = new EntityRef<Territory>();
		
		private EntityRef<Employee> _employee = new EntityRef<Employee>();
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		
		partial void OnEmployeeIDChanged();
		
		partial void OnEmployeeIDChanging(int value);
		
		partial void OnTerritoryIDChanged();
		
		partial void OnTerritoryIDChanging(string value);
		#endregion
		
		
		public EmployeeTerritory()
		{
			this.OnCreated();
		}
		
		[Column(Storage="_employeeID", Name="EmployeeID", DbType="INTEGER", IsPrimaryKey=true, IsDbGenerated=true, AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int EmployeeID
		{
			get
			{
				return this._employeeID;
			}
			set
			{
				if ((_employeeID != value))
				{
					if (_employee.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnEmployeeIDChanging(value);
					this.SendPropertyChanging();
					this._employeeID = value;
					this.SendPropertyChanged("EmployeeID");
					this.OnEmployeeIDChanged();
				}
			}
		}
		
		[Column(Storage="_territoryID", Name="TerritoryID", DbType="nvarchar", IsPrimaryKey=true, AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public string TerritoryID
		{
			get
			{
				return this._territoryID;
			}
			set
			{
				if (((_territoryID == value) 
							== false))
				{
					if (_territory.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnTerritoryIDChanging(value);
					this.SendPropertyChanging();
					this._territoryID = value;
					this.SendPropertyChanged("TerritoryID");
					this.OnTerritoryIDChanged();
				}
			}
		}
		
		#region Parents
		[Association(Storage="_territory", OtherKey="TerritoryID", ThisKey="TerritoryID", Name="fk_EmployeeTerritories_0", IsForeignKey=true)]
		[DebuggerNonUserCode()]
		public Territory Territory
		{
			get
			{
				return this._territory.Entity;
			}
			set
			{
				if (((this._territory.Entity == value) 
							== false))
				{
					if ((this._territory.Entity != null))
					{
						Territory previousTerritory = this._territory.Entity;
						this._territory.Entity = null;
						previousTerritory.EmployeeTerritories.Remove(this);
					}
					this._territory.Entity = value;
					if ((value != null))
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
		
		[Association(Storage="_employee", OtherKey="EmployeeID", ThisKey="EmployeeID", Name="fk_EmployeeTerritories_1", IsForeignKey=true)]
		[DebuggerNonUserCode()]
		public Employee Employee
		{
			get
			{
				return this._employee.Entity;
			}
			set
			{
				if (((this._employee.Entity == value) 
							== false))
				{
					if ((this._employee.Entity != null))
					{
						Employee previousEmployee = this._employee.Entity;
						this._employee.Entity = null;
						previousEmployee.EmployeeTerritories.Remove(this);
					}
					this._employee.Entity = value;
					if ((value != null))
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
		
		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
		
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null))
			{
				h(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null))
			{
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
		
		public override int GetHashCode()
		{
			int hc = 0;
			hc = (hc 
						| (_employeeID.GetHashCode() * 1));
			if ((_territoryID != null))
			{
				hc = (hc 
							| (_territoryID.GetHashCode() * 65536));
			}
			return hc;
		}
		
		public override bool Equals(object value)
		{
			if ((value == null))
			{
				return false;
			}
			if (((value.GetType() == this.GetType()) 
						== false))
			{
				return false;
			}
			EmployeeTerritory other = ((EmployeeTerritory)(value));
			return this.Equals(other);
		}
		
		public virtual bool Equals(EmployeeTerritory value)
		{
			if ((value == null))
			{
				return false;
			}
			return (System.Collections.Generic.EqualityComparer<int>.Default.Equals(this._employeeID, value._employeeID) && System.Collections.Generic.EqualityComparer<string>.Default.Equals(this._territoryID, value._territoryID));
		}
	}
	
	[Table(Name="main.Orders")]
	public partial class Order : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged, System.IEquatable<Order>
	{
		
		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");
		
		private string _customerID;
		
		private System.Nullable<int> _employeeID;
		
		private System.Nullable<decimal> _freight;
		
		private System.Nullable<System.DateTime> _orderDate;
		
		private int _orderID;
		
		private System.Nullable<System.DateTime> _requiredDate;
		
		private string _shipAddress;
		
		private string _shipCity;
		
		private string _shipCountry;
		
		private string _shipName;
		
		private System.Nullable<System.DateTime> _shippedDate;
		
		private string _shipPostalCode;
		
		private string _shipRegion;
		
		private System.Nullable<int> _shipVia;
		
		private EntitySet<OrderDetail> _orderDetails;
		
		private EntityRef<Shipper> _shipper = new EntityRef<Shipper>();
		
		private EntityRef<Employee> _employee = new EntityRef<Employee>();
		
		private EntityRef<Customer> _customer = new EntityRef<Customer>();
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		
		partial void OnCustomerIDChanged();
		
		partial void OnCustomerIDChanging(string value);
		
		partial void OnEmployeeIDChanged();
		
		partial void OnEmployeeIDChanging(System.Nullable<int> value);
		
		partial void OnFreightChanged();
		
		partial void OnFreightChanging(System.Nullable<decimal> value);
		
		partial void OnOrderDateChanged();
		
		partial void OnOrderDateChanging(System.Nullable<System.DateTime> value);
		
		partial void OnOrderIDChanged();
		
		partial void OnOrderIDChanging(int value);
		
		partial void OnRequiredDateChanged();
		
		partial void OnRequiredDateChanging(System.Nullable<System.DateTime> value);
		
		partial void OnShipAddressChanged();
		
		partial void OnShipAddressChanging(string value);
		
		partial void OnShipCityChanged();
		
		partial void OnShipCityChanging(string value);
		
		partial void OnShipCountryChanged();
		
		partial void OnShipCountryChanging(string value);
		
		partial void OnShipNameChanged();
		
		partial void OnShipNameChanging(string value);
		
		partial void OnShippedDateChanged();
		
		partial void OnShippedDateChanging(System.Nullable<System.DateTime> value);
		
		partial void OnShipPostalCodeChanged();
		
		partial void OnShipPostalCodeChanging(string value);
		
		partial void OnShipRegionChanged();
		
		partial void OnShipRegionChanging(string value);
		
		partial void OnShipViaChanged();
		
		partial void OnShipViaChanging(System.Nullable<int> value);
		#endregion
		
		
		public Order()
		{
			_orderDetails = new EntitySet<OrderDetail>(new Action<OrderDetail>(this.OrderDetails_Attach), new Action<OrderDetail>(this.OrderDetails_Detach));
			this.OnCreated();
		}
		
		[Column(Storage="_customerID", Name="CustomerID", DbType="nchar (5)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string CustomerID
		{
			get
			{
				return this._customerID;
			}
			set
			{
				if (((_customerID == value) 
							== false))
				{
					if (_customer.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnCustomerIDChanging(value);
					this.SendPropertyChanging();
					this._customerID = value;
					this.SendPropertyChanged("CustomerID");
					this.OnCustomerIDChanged();
				}
			}
		}
		
		[Column(Storage="_employeeID", Name="EmployeeID", DbType="INTEGER", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<int> EmployeeID
		{
			get
			{
				return this._employeeID;
			}
			set
			{
				if ((_employeeID != value))
				{
					if (_employee.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnEmployeeIDChanging(value);
					this.SendPropertyChanging();
					this._employeeID = value;
					this.SendPropertyChanged("EmployeeID");
					this.OnEmployeeIDChanged();
				}
			}
		}
		
		[Column(Storage="_freight", Name="Freight", DbType="money", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<decimal> Freight
		{
			get
			{
				return this._freight;
			}
			set
			{
				if ((_freight != value))
				{
					this.OnFreightChanging(value);
					this.SendPropertyChanging();
					this._freight = value;
					this.SendPropertyChanged("Freight");
					this.OnFreightChanged();
				}
			}
		}
		
		[Column(Storage="_orderDate", Name="OrderDate", DbType="datetime", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<System.DateTime> OrderDate
		{
			get
			{
				return this._orderDate;
			}
			set
			{
				if ((_orderDate != value))
				{
					this.OnOrderDateChanging(value);
					this.SendPropertyChanging();
					this._orderDate = value;
					this.SendPropertyChanged("OrderDate");
					this.OnOrderDateChanged();
				}
			}
		}
		
		[Column(Storage="_orderID", Name="OrderID", DbType="INTEGER", IsPrimaryKey=true, IsDbGenerated=true, AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int OrderID
		{
			get
			{
				return this._orderID;
			}
			set
			{
				if ((_orderID != value))
				{
					this.OnOrderIDChanging(value);
					this.SendPropertyChanging();
					this._orderID = value;
					this.SendPropertyChanged("OrderID");
					this.OnOrderIDChanged();
				}
			}
		}
		
		[Column(Storage="_requiredDate", Name="RequiredDate", DbType="datetime", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<System.DateTime> RequiredDate
		{
			get
			{
				return this._requiredDate;
			}
			set
			{
				if ((_requiredDate != value))
				{
					this.OnRequiredDateChanging(value);
					this.SendPropertyChanging();
					this._requiredDate = value;
					this.SendPropertyChanged("RequiredDate");
					this.OnRequiredDateChanged();
				}
			}
		}
		
		[Column(Storage="_shipAddress", Name="ShipAddress", DbType="nvarchar (60)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string ShipAddress
		{
			get
			{
				return this._shipAddress;
			}
			set
			{
				if (((_shipAddress == value) 
							== false))
				{
					this.OnShipAddressChanging(value);
					this.SendPropertyChanging();
					this._shipAddress = value;
					this.SendPropertyChanged("ShipAddress");
					this.OnShipAddressChanged();
				}
			}
		}
		
		[Column(Storage="_shipCity", Name="ShipCity", DbType="nvarchar (15)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string ShipCity
		{
			get
			{
				return this._shipCity;
			}
			set
			{
				if (((_shipCity == value) 
							== false))
				{
					this.OnShipCityChanging(value);
					this.SendPropertyChanging();
					this._shipCity = value;
					this.SendPropertyChanged("ShipCity");
					this.OnShipCityChanged();
				}
			}
		}
		
		[Column(Storage="_shipCountry", Name="ShipCountry", DbType="nvarchar (15)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string ShipCountry
		{
			get
			{
				return this._shipCountry;
			}
			set
			{
				if (((_shipCountry == value) 
							== false))
				{
					this.OnShipCountryChanging(value);
					this.SendPropertyChanging();
					this._shipCountry = value;
					this.SendPropertyChanged("ShipCountry");
					this.OnShipCountryChanged();
				}
			}
		}
		
		[Column(Storage="_shipName", Name="ShipName", DbType="nvarchar (40)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string ShipName
		{
			get
			{
				return this._shipName;
			}
			set
			{
				if (((_shipName == value) 
							== false))
				{
					this.OnShipNameChanging(value);
					this.SendPropertyChanging();
					this._shipName = value;
					this.SendPropertyChanged("ShipName");
					this.OnShipNameChanged();
				}
			}
		}
		
		[Column(Storage="_shippedDate", Name="ShippedDate", DbType="datetime", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<System.DateTime> ShippedDate
		{
			get
			{
				return this._shippedDate;
			}
			set
			{
				if ((_shippedDate != value))
				{
					this.OnShippedDateChanging(value);
					this.SendPropertyChanging();
					this._shippedDate = value;
					this.SendPropertyChanged("ShippedDate");
					this.OnShippedDateChanged();
				}
			}
		}
		
		[Column(Storage="_shipPostalCode", Name="ShipPostalCode", DbType="nvarchar (10)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string ShipPostalCode
		{
			get
			{
				return this._shipPostalCode;
			}
			set
			{
				if (((_shipPostalCode == value) 
							== false))
				{
					this.OnShipPostalCodeChanging(value);
					this.SendPropertyChanging();
					this._shipPostalCode = value;
					this.SendPropertyChanged("ShipPostalCode");
					this.OnShipPostalCodeChanged();
				}
			}
		}
		
		[Column(Storage="_shipRegion", Name="ShipRegion", DbType="nvarchar (15)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string ShipRegion
		{
			get
			{
				return this._shipRegion;
			}
			set
			{
				if (((_shipRegion == value) 
							== false))
				{
					this.OnShipRegionChanging(value);
					this.SendPropertyChanging();
					this._shipRegion = value;
					this.SendPropertyChanged("ShipRegion");
					this.OnShipRegionChanged();
				}
			}
		}
		
		[Column(Storage="_shipVia", Name="ShipVia", DbType="INTEGER", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<int> ShipVia
		{
			get
			{
				return this._shipVia;
			}
			set
			{
				if ((_shipVia != value))
				{
					if (_shipper.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnShipViaChanging(value);
					this.SendPropertyChanging();
					this._shipVia = value;
					this.SendPropertyChanged("ShipVia");
					this.OnShipViaChanged();
				}
			}
		}
		
		#region Children
		[Association(Storage="_orderDetails", OtherKey="OrderID", ThisKey="OrderID", Name="fk_Order Details_1")]
		[DebuggerNonUserCode()]
		public EntitySet<OrderDetail> OrderDetails
		{
			get
			{
				return this._orderDetails;
			}
			set
			{
				this._orderDetails = value;
			}
		}
		#endregion
		
		#region Parents
		[Association(Storage="_shipper", OtherKey="ShipperID", ThisKey="ShipVia", Name="fk_Orders_0", IsForeignKey=true)]
		[DebuggerNonUserCode()]
		public Shipper Shipper
		{
			get
			{
				return this._shipper.Entity;
			}
			set
			{
				if (((this._shipper.Entity == value) 
							== false))
				{
					if ((this._shipper.Entity != null))
					{
						Shipper previousShipper = this._shipper.Entity;
						this._shipper.Entity = null;
						previousShipper.Orders.Remove(this);
					}
					this._shipper.Entity = value;
					if ((value != null))
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
		
		[Association(Storage="_employee", OtherKey="EmployeeID", ThisKey="EmployeeID", Name="fk_Orders_1", IsForeignKey=true)]
		[DebuggerNonUserCode()]
		public Employee Employee
		{
			get
			{
				return this._employee.Entity;
			}
			set
			{
				if (((this._employee.Entity == value) 
							== false))
				{
					if ((this._employee.Entity != null))
					{
						Employee previousEmployee = this._employee.Entity;
						this._employee.Entity = null;
						previousEmployee.Orders.Remove(this);
					}
					this._employee.Entity = value;
					if ((value != null))
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
		
		[Association(Storage="_customer", OtherKey="CustomerID", ThisKey="CustomerID", Name="fk_Orders_2", IsForeignKey=true)]
		[DebuggerNonUserCode()]
		public Customer Customer
		{
			get
			{
				return this._customer.Entity;
			}
			set
			{
				if (((this._customer.Entity == value) 
							== false))
				{
					if ((this._customer.Entity != null))
					{
						Customer previousCustomer = this._customer.Entity;
						this._customer.Entity = null;
						previousCustomer.Orders.Remove(this);
					}
					this._customer.Entity = value;
					if ((value != null))
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
		
		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
		
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null))
			{
				h(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null))
			{
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
		
		public override int GetHashCode()
		{
			int hc = 0;
			hc = (hc 
						| (_orderID.GetHashCode() * 1));
			return hc;
		}
		
		public override bool Equals(object value)
		{
			if ((value == null))
			{
				return false;
			}
			if (((value.GetType() == this.GetType()) 
						== false))
			{
				return false;
			}
			Order other = ((Order)(value));
			return this.Equals(other);
		}
		
		public virtual bool Equals(Order value)
		{
			if ((value == null))
			{
				return false;
			}
			return System.Collections.Generic.EqualityComparer<int>.Default.Equals(this._orderID, value._orderID);
		}
		
		#region Attachment handlers
		private void OrderDetails_Attach(OrderDetail entity)
		{
			this.SendPropertyChanging();
			entity.Order = this;
		}
		
		private void OrderDetails_Detach(OrderDetail entity)
		{
			this.SendPropertyChanging();
			entity.Order = null;
		}
		#endregion
	}
	
	[Table(Name="main.Order Details")]
	public partial class OrderDetail : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged, System.IEquatable<OrderDetail>
	{
		
		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");
		
		private float _discount;
		
		private int _orderID;
		
		private int _productID;
		
		private short _quantity;
		
		private decimal _unitPrice;
		
		private EntityRef<Product> _product = new EntityRef<Product>();
		
		private EntityRef<Order> _order = new EntityRef<Order>();
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		
		partial void OnDiscountChanged();
		
		partial void OnDiscountChanging(float value);
		
		partial void OnOrderIDChanged();
		
		partial void OnOrderIDChanging(int value);
		
		partial void OnProductIDChanged();
		
		partial void OnProductIDChanging(int value);
		
		partial void OnQuantityChanged();
		
		partial void OnQuantityChanging(short value);
		
		partial void OnUnitPriceChanged();
		
		partial void OnUnitPriceChanging(decimal value);
		#endregion
		
		
		public OrderDetail()
		{
			this.OnCreated();
		}
		
		[Column(Storage="_discount", Name="Discount", DbType="real", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public float Discount
		{
			get
			{
				return this._discount;
			}
			set
			{
				if ((_discount != value))
				{
					this.OnDiscountChanging(value);
					this.SendPropertyChanging();
					this._discount = value;
					this.SendPropertyChanged("Discount");
					this.OnDiscountChanged();
				}
			}
		}
		
		[Column(Storage="_orderID", Name="OrderID", DbType="INTEGER", IsPrimaryKey=true, IsDbGenerated=true, AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int OrderID
		{
			get
			{
				return this._orderID;
			}
			set
			{
				if ((_orderID != value))
				{
					if (_order.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnOrderIDChanging(value);
					this.SendPropertyChanging();
					this._orderID = value;
					this.SendPropertyChanged("OrderID");
					this.OnOrderIDChanged();
				}
			}
		}
		
		[Column(Storage="_productID", Name="ProductID", DbType="INTEGER", IsPrimaryKey=true, IsDbGenerated=true, AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int ProductID
		{
			get
			{
				return this._productID;
			}
			set
			{
				if ((_productID != value))
				{
					if (_product.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnProductIDChanging(value);
					this.SendPropertyChanging();
					this._productID = value;
					this.SendPropertyChanged("ProductID");
					this.OnProductIDChanged();
				}
			}
		}
		
		[Column(Storage="_quantity", Name="Quantity", DbType="smallint", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public short Quantity
		{
			get
			{
				return this._quantity;
			}
			set
			{
				if ((_quantity != value))
				{
					this.OnQuantityChanging(value);
					this.SendPropertyChanging();
					this._quantity = value;
					this.SendPropertyChanged("Quantity");
					this.OnQuantityChanged();
				}
			}
		}
		
		[Column(Storage="_unitPrice", Name="UnitPrice", DbType="money", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public decimal UnitPrice
		{
			get
			{
				return this._unitPrice;
			}
			set
			{
				if ((_unitPrice != value))
				{
					this.OnUnitPriceChanging(value);
					this.SendPropertyChanging();
					this._unitPrice = value;
					this.SendPropertyChanged("UnitPrice");
					this.OnUnitPriceChanged();
				}
			}
		}
		
		#region Parents
		[Association(Storage="_product", OtherKey="ProductID", ThisKey="ProductID", Name="fk_Order Details_0", IsForeignKey=true)]
		[DebuggerNonUserCode()]
		public Product Product
		{
			get
			{
				return this._product.Entity;
			}
			set
			{
				if (((this._product.Entity == value) 
							== false))
				{
					if ((this._product.Entity != null))
					{
						Product previousProduct = this._product.Entity;
						this._product.Entity = null;
						previousProduct.OrderDetails.Remove(this);
					}
					this._product.Entity = value;
					if ((value != null))
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
		
		[Association(Storage="_order", OtherKey="OrderID", ThisKey="OrderID", Name="fk_Order Details_1", IsForeignKey=true)]
		[DebuggerNonUserCode()]
		public Order Order
		{
			get
			{
				return this._order.Entity;
			}
			set
			{
				if (((this._order.Entity == value) 
							== false))
				{
					if ((this._order.Entity != null))
					{
						Order previousOrder = this._order.Entity;
						this._order.Entity = null;
						previousOrder.OrderDetails.Remove(this);
					}
					this._order.Entity = value;
					if ((value != null))
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
		
		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
		
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null))
			{
				h(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null))
			{
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
		
		public override int GetHashCode()
		{
			int hc = 0;
			hc = (hc 
						| (_orderID.GetHashCode() * 1));
			hc = (hc 
						| (_productID.GetHashCode() * 65536));
			return hc;
		}
		
		public override bool Equals(object value)
		{
			if ((value == null))
			{
				return false;
			}
			if (((value.GetType() == this.GetType()) 
						== false))
			{
				return false;
			}
			OrderDetail other = ((OrderDetail)(value));
			return this.Equals(other);
		}
		
		public virtual bool Equals(OrderDetail value)
		{
			if ((value == null))
			{
				return false;
			}
			return (System.Collections.Generic.EqualityComparer<int>.Default.Equals(this._orderID, value._orderID) && System.Collections.Generic.EqualityComparer<int>.Default.Equals(this._productID, value._productID));
		}
	}
	
	[Table(Name="main.Products")]
	public partial class Product : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged, System.IEquatable<Product>
	{
		
		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");
		
		private System.Nullable<int> _categoryID;
		
		private bool _discontinued;
		
		private int _productID;
		
		private string _productName;
		
		private string _quantityPerUnit;
		
		private System.Nullable<short> _reorderLevel;
		
		private System.Nullable<int> _supplierID;
		
		private System.Nullable<decimal> _unitPrice;
		
		private System.Nullable<short> _unitsInStock;
		
		private System.Nullable<short> _unitsOnOrder;
		
		private EntitySet<OrderDetail> _orderDetails;
		
		private EntityRef<Supplier> _supplier = new EntityRef<Supplier>();
		
		private EntityRef<Category> _category = new EntityRef<Category>();
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		
		partial void OnCategoryIDChanged();
		
		partial void OnCategoryIDChanging(System.Nullable<int> value);
		
		partial void OnDiscontinuedChanged();
		
		partial void OnDiscontinuedChanging(bool value);
		
		partial void OnProductIDChanged();
		
		partial void OnProductIDChanging(int value);
		
		partial void OnProductNameChanged();
		
		partial void OnProductNameChanging(string value);
		
		partial void OnQuantityPerUnitChanged();
		
		partial void OnQuantityPerUnitChanging(string value);
		
		partial void OnReorderLevelChanged();
		
		partial void OnReorderLevelChanging(System.Nullable<short> value);
		
		partial void OnSupplierIDChanged();
		
		partial void OnSupplierIDChanging(System.Nullable<int> value);
		
		partial void OnUnitPriceChanged();
		
		partial void OnUnitPriceChanging(System.Nullable<decimal> value);
		
		partial void OnUnitsInStockChanged();
		
		partial void OnUnitsInStockChanging(System.Nullable<short> value);
		
		partial void OnUnitsOnOrderChanged();
		
		partial void OnUnitsOnOrderChanging(System.Nullable<short> value);
		#endregion
		
		
		public Product()
		{
			_orderDetails = new EntitySet<OrderDetail>(new Action<OrderDetail>(this.OrderDetails_Attach), new Action<OrderDetail>(this.OrderDetails_Detach));
			this.OnCreated();
		}
		
		[Column(Storage="_categoryID", Name="CategoryID", DbType="INTEGER", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<int> CategoryID
		{
			get
			{
				return this._categoryID;
			}
			set
			{
				if ((_categoryID != value))
				{
					if (_category.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnCategoryIDChanging(value);
					this.SendPropertyChanging();
					this._categoryID = value;
					this.SendPropertyChanged("CategoryID");
					this.OnCategoryIDChanged();
				}
			}
		}
		
		[Column(Storage="_discontinued", Name="Discontinued", DbType="bit", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public bool Discontinued
		{
			get
			{
				return this._discontinued;
			}
			set
			{
				if ((_discontinued != value))
				{
					this.OnDiscontinuedChanging(value);
					this.SendPropertyChanging();
					this._discontinued = value;
					this.SendPropertyChanged("Discontinued");
					this.OnDiscontinuedChanged();
				}
			}
		}
		
		[Column(Storage="_productID", Name="ProductID", DbType="INTEGER", IsPrimaryKey=true, IsDbGenerated=true, AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int ProductID
		{
			get
			{
				return this._productID;
			}
			set
			{
				if ((_productID != value))
				{
					this.OnProductIDChanging(value);
					this.SendPropertyChanging();
					this._productID = value;
					this.SendPropertyChanged("ProductID");
					this.OnProductIDChanged();
				}
			}
		}
		
		[Column(Storage="_productName", Name="ProductName", DbType="nvarchar (40)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public string ProductName
		{
			get
			{
				return this._productName;
			}
			set
			{
				if (((_productName == value) 
							== false))
				{
					this.OnProductNameChanging(value);
					this.SendPropertyChanging();
					this._productName = value;
					this.SendPropertyChanged("ProductName");
					this.OnProductNameChanged();
				}
			}
		}
		
		[Column(Storage="_quantityPerUnit", Name="QuantityPerUnit", DbType="nvarchar (20)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string QuantityPerUnit
		{
			get
			{
				return this._quantityPerUnit;
			}
			set
			{
				if (((_quantityPerUnit == value) 
							== false))
				{
					this.OnQuantityPerUnitChanging(value);
					this.SendPropertyChanging();
					this._quantityPerUnit = value;
					this.SendPropertyChanged("QuantityPerUnit");
					this.OnQuantityPerUnitChanged();
				}
			}
		}
		
		[Column(Storage="_reorderLevel", Name="ReorderLevel", DbType="smallint", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<short> ReorderLevel
		{
			get
			{
				return this._reorderLevel;
			}
			set
			{
				if ((_reorderLevel != value))
				{
					this.OnReorderLevelChanging(value);
					this.SendPropertyChanging();
					this._reorderLevel = value;
					this.SendPropertyChanged("ReorderLevel");
					this.OnReorderLevelChanged();
				}
			}
		}
		
		[Column(Storage="_supplierID", Name="SupplierID", DbType="INTEGER", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<int> SupplierID
		{
			get
			{
				return this._supplierID;
			}
			set
			{
				if ((_supplierID != value))
				{
					if (_supplier.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnSupplierIDChanging(value);
					this.SendPropertyChanging();
					this._supplierID = value;
					this.SendPropertyChanged("SupplierID");
					this.OnSupplierIDChanged();
				}
			}
		}
		
		[Column(Storage="_unitPrice", Name="UnitPrice", DbType="money", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<decimal> UnitPrice
		{
			get
			{
				return this._unitPrice;
			}
			set
			{
				if ((_unitPrice != value))
				{
					this.OnUnitPriceChanging(value);
					this.SendPropertyChanging();
					this._unitPrice = value;
					this.SendPropertyChanged("UnitPrice");
					this.OnUnitPriceChanged();
				}
			}
		}
		
		[Column(Storage="_unitsInStock", Name="UnitsInStock", DbType="smallint", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<short> UnitsInStock
		{
			get
			{
				return this._unitsInStock;
			}
			set
			{
				if ((_unitsInStock != value))
				{
					this.OnUnitsInStockChanging(value);
					this.SendPropertyChanging();
					this._unitsInStock = value;
					this.SendPropertyChanged("UnitsInStock");
					this.OnUnitsInStockChanged();
				}
			}
		}
		
		[Column(Storage="_unitsOnOrder", Name="UnitsOnOrder", DbType="smallint", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<short> UnitsOnOrder
		{
			get
			{
				return this._unitsOnOrder;
			}
			set
			{
				if ((_unitsOnOrder != value))
				{
					this.OnUnitsOnOrderChanging(value);
					this.SendPropertyChanging();
					this._unitsOnOrder = value;
					this.SendPropertyChanged("UnitsOnOrder");
					this.OnUnitsOnOrderChanged();
				}
			}
		}
		
		#region Children
		[Association(Storage="_orderDetails", OtherKey="ProductID", ThisKey="ProductID", Name="fk_Order Details_0")]
		[DebuggerNonUserCode()]
		public EntitySet<OrderDetail> OrderDetails
		{
			get
			{
				return this._orderDetails;
			}
			set
			{
				this._orderDetails = value;
			}
		}
		#endregion
		
		#region Parents
		[Association(Storage="_supplier", OtherKey="SupplierID", ThisKey="SupplierID", Name="fk_Products_0", IsForeignKey=true)]
		[DebuggerNonUserCode()]
		public Supplier Supplier
		{
			get
			{
				return this._supplier.Entity;
			}
			set
			{
				if (((this._supplier.Entity == value) 
							== false))
				{
					if ((this._supplier.Entity != null))
					{
						Supplier previousSupplier = this._supplier.Entity;
						this._supplier.Entity = null;
						previousSupplier.Products.Remove(this);
					}
					this._supplier.Entity = value;
					if ((value != null))
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
		
		[Association(Storage="_category", OtherKey="CategoryID", ThisKey="CategoryID", Name="fk_Products_1", IsForeignKey=true)]
		[DebuggerNonUserCode()]
		public Category Category
		{
			get
			{
				return this._category.Entity;
			}
			set
			{
				if (((this._category.Entity == value) 
							== false))
				{
					if ((this._category.Entity != null))
					{
						Category previousCategory = this._category.Entity;
						this._category.Entity = null;
						previousCategory.Products.Remove(this);
					}
					this._category.Entity = value;
					if ((value != null))
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
		#endregion
		
		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
		
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null))
			{
				h(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null))
			{
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
		
		public override int GetHashCode()
		{
			int hc = 0;
			hc = (hc 
						| (_productID.GetHashCode() * 1));
			return hc;
		}
		
		public override bool Equals(object value)
		{
			if ((value == null))
			{
				return false;
			}
			if (((value.GetType() == this.GetType()) 
						== false))
			{
				return false;
			}
			Product other = ((Product)(value));
			return this.Equals(other);
		}
		
		public virtual bool Equals(Product value)
		{
			if ((value == null))
			{
				return false;
			}
			return System.Collections.Generic.EqualityComparer<int>.Default.Equals(this._productID, value._productID);
		}
		
		#region Attachment handlers
		private void OrderDetails_Attach(OrderDetail entity)
		{
			this.SendPropertyChanging();
			entity.Product = this;
		}
		
		private void OrderDetails_Detach(OrderDetail entity)
		{
			this.SendPropertyChanging();
			entity.Product = null;
		}
		#endregion
	}
	
	[Table(Name="main.Region")]
	public partial class Region : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged, System.IEquatable<Region>
	{
		
		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");
		
		private string _regionDescription;
		
		private int _regionID;
		
		private EntitySet<Territory> _territories;
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		
		partial void OnRegionDescriptionChanged();
		
		partial void OnRegionDescriptionChanging(string value);
		
		partial void OnRegionIDChanged();
		
		partial void OnRegionIDChanging(int value);
		#endregion
		
		
		public Region()
		{
			_territories = new EntitySet<Territory>(new Action<Territory>(this.Territories_Attach), new Action<Territory>(this.Territories_Detach));
			this.OnCreated();
		}
		
		[Column(Storage="_regionDescription", Name="RegionDescription", DbType="nchar", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public string RegionDescription
		{
			get
			{
				return this._regionDescription;
			}
			set
			{
				if (((_regionDescription == value) 
							== false))
				{
					this.OnRegionDescriptionChanging(value);
					this.SendPropertyChanging();
					this._regionDescription = value;
					this.SendPropertyChanged("RegionDescription");
					this.OnRegionDescriptionChanged();
				}
			}
		}
		
		[Column(Storage="_regionID", Name="RegionID", DbType="INTEGER", IsPrimaryKey=true, IsDbGenerated=true, AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int RegionID
		{
			get
			{
				return this._regionID;
			}
			set
			{
				if ((_regionID != value))
				{
					this.OnRegionIDChanging(value);
					this.SendPropertyChanging();
					this._regionID = value;
					this.SendPropertyChanged("RegionID");
					this.OnRegionIDChanged();
				}
			}
		}
		
		#region Children
		[Association(Storage="_territories", OtherKey="RegionID", ThisKey="RegionID", Name="fk_Territories_0")]
		[DebuggerNonUserCode()]
		public EntitySet<Territory> Territories
		{
			get
			{
				return this._territories;
			}
			set
			{
				this._territories = value;
			}
		}
		#endregion
		
		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
		
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null))
			{
				h(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null))
			{
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
		
		public override int GetHashCode()
		{
			int hc = 0;
			hc = (hc 
						| (_regionID.GetHashCode() * 1));
			return hc;
		}
		
		public override bool Equals(object value)
		{
			if ((value == null))
			{
				return false;
			}
			if (((value.GetType() == this.GetType()) 
						== false))
			{
				return false;
			}
			Region other = ((Region)(value));
			return this.Equals(other);
		}
		
		public virtual bool Equals(Region value)
		{
			if ((value == null))
			{
				return false;
			}
			return System.Collections.Generic.EqualityComparer<int>.Default.Equals(this._regionID, value._regionID);
		}
		
		#region Attachment handlers
		private void Territories_Attach(Territory entity)
		{
			this.SendPropertyChanging();
			entity.Region = this;
		}
		
		private void Territories_Detach(Territory entity)
		{
			this.SendPropertyChanging();
			entity.Region = null;
		}
		#endregion
	}
	
	[Table(Name="main.Shippers")]
	public partial class Shipper : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged, System.IEquatable<Shipper>
	{
		
		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");
		
		private string _companyName;
		
		private string _phone;
		
		private int _shipperID;
		
		private EntitySet<Order> _orders;
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		
		partial void OnCompanyNameChanged();
		
		partial void OnCompanyNameChanging(string value);
		
		partial void OnPhoneChanged();
		
		partial void OnPhoneChanging(string value);
		
		partial void OnShipperIDChanged();
		
		partial void OnShipperIDChanging(int value);
		#endregion
		
		
		public Shipper()
		{
			_orders = new EntitySet<Order>(new Action<Order>(this.Orders_Attach), new Action<Order>(this.Orders_Detach));
			this.OnCreated();
		}
		
		[Column(Storage="_companyName", Name="CompanyName", DbType="nvarchar (40)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public string CompanyName
		{
			get
			{
				return this._companyName;
			}
			set
			{
				if (((_companyName == value) 
							== false))
				{
					this.OnCompanyNameChanging(value);
					this.SendPropertyChanging();
					this._companyName = value;
					this.SendPropertyChanged("CompanyName");
					this.OnCompanyNameChanged();
				}
			}
		}
		
		[Column(Storage="_phone", Name="Phone", DbType="nvarchar (24)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string Phone
		{
			get
			{
				return this._phone;
			}
			set
			{
				if (((_phone == value) 
							== false))
				{
					this.OnPhoneChanging(value);
					this.SendPropertyChanging();
					this._phone = value;
					this.SendPropertyChanged("Phone");
					this.OnPhoneChanged();
				}
			}
		}
		
		[Column(Storage="_shipperID", Name="ShipperID", DbType="INTEGER", IsPrimaryKey=true, IsDbGenerated=true, AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int ShipperID
		{
			get
			{
				return this._shipperID;
			}
			set
			{
				if ((_shipperID != value))
				{
					this.OnShipperIDChanging(value);
					this.SendPropertyChanging();
					this._shipperID = value;
					this.SendPropertyChanged("ShipperID");
					this.OnShipperIDChanged();
				}
			}
		}
		
		#region Children
		[Association(Storage="_orders", OtherKey="ShipVia", ThisKey="ShipperID", Name="fk_Orders_0")]
		[DebuggerNonUserCode()]
		public EntitySet<Order> Orders
		{
			get
			{
				return this._orders;
			}
			set
			{
				this._orders = value;
			}
		}
		#endregion
		
		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
		
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null))
			{
				h(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null))
			{
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
		
		public override int GetHashCode()
		{
			int hc = 0;
			hc = (hc 
						| (_shipperID.GetHashCode() * 1));
			return hc;
		}
		
		public override bool Equals(object value)
		{
			if ((value == null))
			{
				return false;
			}
			if (((value.GetType() == this.GetType()) 
						== false))
			{
				return false;
			}
			Shipper other = ((Shipper)(value));
			return this.Equals(other);
		}
		
		public virtual bool Equals(Shipper value)
		{
			if ((value == null))
			{
				return false;
			}
			return System.Collections.Generic.EqualityComparer<int>.Default.Equals(this._shipperID, value._shipperID);
		}
		
		#region Attachment handlers
		private void Orders_Attach(Order entity)
		{
			this.SendPropertyChanging();
			entity.Shipper = this;
		}
		
		private void Orders_Detach(Order entity)
		{
			this.SendPropertyChanging();
			entity.Shipper = null;
		}
		#endregion
	}
	
	[Table(Name="main.Suppliers")]
	public partial class Supplier : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged, System.IEquatable<Supplier>
	{
		
		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");
		
		private string _address;
		
		private string _city;
		
		private string _companyName;
		
		private string _contactName;
		
		private string _contactTitle;
		
		private string _country;
		
		private string _fax;
		
		private string _homePage;
		
		private string _phone;
		
		private string _postalCode;
		
		private string _region;
		
		private int _supplierID;
		
		private EntitySet<Product> _products;
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		
		partial void OnAddressChanged();
		
		partial void OnAddressChanging(string value);
		
		partial void OnCityChanged();
		
		partial void OnCityChanging(string value);
		
		partial void OnCompanyNameChanged();
		
		partial void OnCompanyNameChanging(string value);
		
		partial void OnContactNameChanged();
		
		partial void OnContactNameChanging(string value);
		
		partial void OnContactTitleChanged();
		
		partial void OnContactTitleChanging(string value);
		
		partial void OnCountryChanged();
		
		partial void OnCountryChanging(string value);
		
		partial void OnFaxChanged();
		
		partial void OnFaxChanging(string value);
		
		partial void OnHomePageChanged();
		
		partial void OnHomePageChanging(string value);
		
		partial void OnPhoneChanged();
		
		partial void OnPhoneChanging(string value);
		
		partial void OnPostalCodeChanged();
		
		partial void OnPostalCodeChanging(string value);
		
		partial void OnRegionChanged();
		
		partial void OnRegionChanging(string value);
		
		partial void OnSupplierIDChanged();
		
		partial void OnSupplierIDChanging(int value);
		#endregion
		
		
		public Supplier()
		{
			_products = new EntitySet<Product>(new Action<Product>(this.Products_Attach), new Action<Product>(this.Products_Detach));
			this.OnCreated();
		}
		
		[Column(Storage="_address", Name="Address", DbType="nvarchar (60)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string Address
		{
			get
			{
				return this._address;
			}
			set
			{
				if (((_address == value) 
							== false))
				{
					this.OnAddressChanging(value);
					this.SendPropertyChanging();
					this._address = value;
					this.SendPropertyChanged("Address");
					this.OnAddressChanged();
				}
			}
		}
		
		[Column(Storage="_city", Name="City", DbType="nvarchar (15)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string City
		{
			get
			{
				return this._city;
			}
			set
			{
				if (((_city == value) 
							== false))
				{
					this.OnCityChanging(value);
					this.SendPropertyChanging();
					this._city = value;
					this.SendPropertyChanged("City");
					this.OnCityChanged();
				}
			}
		}
		
		[Column(Storage="_companyName", Name="CompanyName", DbType="nvarchar (40)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public string CompanyName
		{
			get
			{
				return this._companyName;
			}
			set
			{
				if (((_companyName == value) 
							== false))
				{
					this.OnCompanyNameChanging(value);
					this.SendPropertyChanging();
					this._companyName = value;
					this.SendPropertyChanged("CompanyName");
					this.OnCompanyNameChanged();
				}
			}
		}
		
		[Column(Storage="_contactName", Name="ContactName", DbType="nvarchar (30)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string ContactName
		{
			get
			{
				return this._contactName;
			}
			set
			{
				if (((_contactName == value) 
							== false))
				{
					this.OnContactNameChanging(value);
					this.SendPropertyChanging();
					this._contactName = value;
					this.SendPropertyChanged("ContactName");
					this.OnContactNameChanged();
				}
			}
		}
		
		[Column(Storage="_contactTitle", Name="ContactTitle", DbType="nvarchar (30)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string ContactTitle
		{
			get
			{
				return this._contactTitle;
			}
			set
			{
				if (((_contactTitle == value) 
							== false))
				{
					this.OnContactTitleChanging(value);
					this.SendPropertyChanging();
					this._contactTitle = value;
					this.SendPropertyChanged("ContactTitle");
					this.OnContactTitleChanged();
				}
			}
		}
		
		[Column(Storage="_country", Name="Country", DbType="nvarchar (15)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string Country
		{
			get
			{
				return this._country;
			}
			set
			{
				if (((_country == value) 
							== false))
				{
					this.OnCountryChanging(value);
					this.SendPropertyChanging();
					this._country = value;
					this.SendPropertyChanged("Country");
					this.OnCountryChanged();
				}
			}
		}
		
		[Column(Storage="_fax", Name="Fax", DbType="nvarchar (24)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string Fax
		{
			get
			{
				return this._fax;
			}
			set
			{
				if (((_fax == value) 
							== false))
				{
					this.OnFaxChanging(value);
					this.SendPropertyChanging();
					this._fax = value;
					this.SendPropertyChanged("Fax");
					this.OnFaxChanged();
				}
			}
		}
		
		[Column(Storage="_homePage", Name="HomePage", DbType="ntext", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string HomePage
		{
			get
			{
				return this._homePage;
			}
			set
			{
				if (((_homePage == value) 
							== false))
				{
					this.OnHomePageChanging(value);
					this.SendPropertyChanging();
					this._homePage = value;
					this.SendPropertyChanged("HomePage");
					this.OnHomePageChanged();
				}
			}
		}
		
		[Column(Storage="_phone", Name="Phone", DbType="nvarchar (24)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string Phone
		{
			get
			{
				return this._phone;
			}
			set
			{
				if (((_phone == value) 
							== false))
				{
					this.OnPhoneChanging(value);
					this.SendPropertyChanging();
					this._phone = value;
					this.SendPropertyChanged("Phone");
					this.OnPhoneChanged();
				}
			}
		}
		
		[Column(Storage="_postalCode", Name="PostalCode", DbType="nvarchar (10)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string PostalCode
		{
			get
			{
				return this._postalCode;
			}
			set
			{
				if (((_postalCode == value) 
							== false))
				{
					this.OnPostalCodeChanging(value);
					this.SendPropertyChanging();
					this._postalCode = value;
					this.SendPropertyChanged("PostalCode");
					this.OnPostalCodeChanged();
				}
			}
		}
		
		[Column(Storage="_region", Name="Region", DbType="nvarchar (15)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string Region
		{
			get
			{
				return this._region;
			}
			set
			{
				if (((_region == value) 
							== false))
				{
					this.OnRegionChanging(value);
					this.SendPropertyChanging();
					this._region = value;
					this.SendPropertyChanged("Region");
					this.OnRegionChanged();
				}
			}
		}
		
		[Column(Storage="_supplierID", Name="SupplierID", DbType="INTEGER", IsPrimaryKey=true, IsDbGenerated=true, AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int SupplierID
		{
			get
			{
				return this._supplierID;
			}
			set
			{
				if ((_supplierID != value))
				{
					this.OnSupplierIDChanging(value);
					this.SendPropertyChanging();
					this._supplierID = value;
					this.SendPropertyChanged("SupplierID");
					this.OnSupplierIDChanged();
				}
			}
		}
		
		#region Children
		[Association(Storage="_products", OtherKey="SupplierID", ThisKey="SupplierID", Name="fk_Products_0")]
		[DebuggerNonUserCode()]
		public EntitySet<Product> Products
		{
			get
			{
				return this._products;
			}
			set
			{
				this._products = value;
			}
		}
		#endregion
		
		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
		
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null))
			{
				h(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null))
			{
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
		
		public override int GetHashCode()
		{
			int hc = 0;
			hc = (hc 
						| (_supplierID.GetHashCode() * 1));
			return hc;
		}
		
		public override bool Equals(object value)
		{
			if ((value == null))
			{
				return false;
			}
			if (((value.GetType() == this.GetType()) 
						== false))
			{
				return false;
			}
			Supplier other = ((Supplier)(value));
			return this.Equals(other);
		}
		
		public virtual bool Equals(Supplier value)
		{
			if ((value == null))
			{
				return false;
			}
			return System.Collections.Generic.EqualityComparer<int>.Default.Equals(this._supplierID, value._supplierID);
		}
		
		#region Attachment handlers
		private void Products_Attach(Product entity)
		{
			this.SendPropertyChanging();
			entity.Supplier = this;
		}
		
		private void Products_Detach(Product entity)
		{
			this.SendPropertyChanging();
			entity.Supplier = null;
		}
		#endregion
	}
	
	[Table(Name="main.Territories")]
	public partial class Territory : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged, System.IEquatable<Territory>
	{
		
		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");
		
		private int _regionID;
		
		private string _territoryDescription;
		
		private string _territoryID;
		
		private EntitySet<EmployeeTerritory> _employeeTerritories;
		
		private EntityRef<Region> _region = new EntityRef<Region>();
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		
		partial void OnRegionIDChanged();
		
		partial void OnRegionIDChanging(int value);
		
		partial void OnTerritoryDescriptionChanged();
		
		partial void OnTerritoryDescriptionChanging(string value);
		
		partial void OnTerritoryIDChanged();
		
		partial void OnTerritoryIDChanging(string value);
		#endregion
		
		
		public Territory()
		{
			_employeeTerritories = new EntitySet<EmployeeTerritory>(new Action<EmployeeTerritory>(this.EmployeeTerritories_Attach), new Action<EmployeeTerritory>(this.EmployeeTerritories_Detach));
			this.OnCreated();
		}
		
		[Column(Storage="_regionID", Name="RegionID", DbType="INTEGER", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int RegionID
		{
			get
			{
				return this._regionID;
			}
			set
			{
				if ((_regionID != value))
				{
					if (_region.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnRegionIDChanging(value);
					this.SendPropertyChanging();
					this._regionID = value;
					this.SendPropertyChanged("RegionID");
					this.OnRegionIDChanged();
				}
			}
		}
		
		[Column(Storage="_territoryDescription", Name="TerritoryDescription", DbType="nchar", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public string TerritoryDescription
		{
			get
			{
				return this._territoryDescription;
			}
			set
			{
				if (((_territoryDescription == value) 
							== false))
				{
					this.OnTerritoryDescriptionChanging(value);
					this.SendPropertyChanging();
					this._territoryDescription = value;
					this.SendPropertyChanged("TerritoryDescription");
					this.OnTerritoryDescriptionChanged();
				}
			}
		}
		
		[Column(Storage="_territoryID", Name="TerritoryID", DbType="nvarchar", IsPrimaryKey=true, AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public string TerritoryID
		{
			get
			{
				return this._territoryID;
			}
			set
			{
				if (((_territoryID == value) 
							== false))
				{
					this.OnTerritoryIDChanging(value);
					this.SendPropertyChanging();
					this._territoryID = value;
					this.SendPropertyChanged("TerritoryID");
					this.OnTerritoryIDChanged();
				}
			}
		}
		
		#region Children
		[Association(Storage="_employeeTerritories", OtherKey="TerritoryID", ThisKey="TerritoryID", Name="fk_EmployeeTerritories_0")]
		[DebuggerNonUserCode()]
		public EntitySet<EmployeeTerritory> EmployeeTerritories
		{
			get
			{
				return this._employeeTerritories;
			}
			set
			{
				this._employeeTerritories = value;
			}
		}
		#endregion
		
		#region Parents
		[Association(Storage="_region", OtherKey="RegionID", ThisKey="RegionID", Name="fk_Territories_0", IsForeignKey=true)]
		[DebuggerNonUserCode()]
		public Region Region
		{
			get
			{
				return this._region.Entity;
			}
			set
			{
				if (((this._region.Entity == value) 
							== false))
				{
					if ((this._region.Entity != null))
					{
						Region previousRegion = this._region.Entity;
						this._region.Entity = null;
						previousRegion.Territories.Remove(this);
					}
					this._region.Entity = value;
					if ((value != null))
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
		
		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
		
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null))
			{
				h(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null))
			{
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
		
		public override int GetHashCode()
		{
			int hc = 0;
			if ((_territoryID != null))
			{
				hc = (hc 
							| (_territoryID.GetHashCode() * 1));
			}
			return hc;
		}
		
		public override bool Equals(object value)
		{
			if ((value == null))
			{
				return false;
			}
			if (((value.GetType() == this.GetType()) 
						== false))
			{
				return false;
			}
			Territory other = ((Territory)(value));
			return this.Equals(other);
		}
		
		public virtual bool Equals(Territory value)
		{
			if ((value == null))
			{
				return false;
			}
			return System.Collections.Generic.EqualityComparer<string>.Default.Equals(this._territoryID, value._territoryID);
		}
		
		#region Attachment handlers
		private void EmployeeTerritories_Attach(EmployeeTerritory entity)
		{
			this.SendPropertyChanging();
			entity.Territory = this;
		}
		
		private void EmployeeTerritories_Detach(EmployeeTerritory entity)
		{
			this.SendPropertyChanging();
			entity.Territory = null;
		}
		#endregion
	}
}
