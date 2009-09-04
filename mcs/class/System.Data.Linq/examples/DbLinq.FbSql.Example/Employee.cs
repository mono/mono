#region Auto-generated classes for  C:\Program Files\Firebird\Firebird_2_1\examples\empbuild\EMPLOYEE .FDB database on 2008-10-06 11:08:52Z

//
//  ____  _     __  __      _        _
// |  _ \| |__ |  \/  | ___| |_ __ _| |
// | | | | '_ \| |\/| |/ _ \ __/ _` | |
// | |_| | |_) | |  | |  __/ || (_| | |
// |____/|_.__/|_|  |_|\___|\__\__,_|_|
//
// Auto-generated from  C:\Program Files\Firebird\Firebird_2_1\examples\empbuild\EMPLOYEE .FDB on 2008-10-06 11:08:52Z
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

public partial class CpRogramFilesFirebirdFirebird21ExamplesEmpbuildEmployeefdb : DataContext
{
	public CpRogramFilesFirebirdFirebird21ExamplesEmpbuildEmployeefdb(IDbConnection connection)
	: base(connection, new DbLinq.FirebirdSql.FbVendor())
	{
	}

	public CpRogramFilesFirebirdFirebird21ExamplesEmpbuildEmployeefdb(IDbConnection connection, IVendor vendor)
	: base(connection, vendor)
	{
	}

	public Table<Country> Country { get { return GetTable<Country>(); } }
	public Table<Customer> Customer { get { return GetTable<Customer>(); } }
	public Table<Department> Department { get { return GetTable<Department>(); } }
	public Table<Employee> Employee { get { return GetTable<Employee>(); } }
	public Table<EmployeeProject> EmployeeProject { get { return GetTable<EmployeeProject>(); } }
	public Table<Job> Job { get { return GetTable<Job>(); } }
	public Table<PhoneList> PhoneList { get { return GetTable<PhoneList>(); } }
	public Table<ProJDEPtBudget> ProJDEPtBudget { get { return GetTable<ProJDEPtBudget>(); } }
	public Table<Project> Project { get { return GetTable<Project>(); } }
	public Table<SalaryHistory> SalaryHistory { get { return GetTable<SalaryHistory>(); } }
	public Table<Sales> Sales { get { return GetTable<Sales>(); } }

}

[Table(Name = " Foo .COUNTRY")]
public partial class Country : INotifyPropertyChanged
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

	#region string Country1

	private string _country1;
	[DebuggerNonUserCode]
	[Column(Storage = "_country1", Name = "COUNTRY", DbType = null, IsPrimaryKey = true, CanBeNull = false)]
	public string Country1
	{
		get
		{
			return _country1;
		}
		set
		{
			if (value != _country1)
			{
				_country1 = value;
				OnPropertyChanged("Country1");
			}
		}
	}

	#endregion

	#region string Currency

	private string _currency;
	[DebuggerNonUserCode]
	[Column(Storage = "_currency", Name = "CURRENCY", DbType = null, CanBeNull = false)]
	public string Currency
	{
		get
		{
			return _currency;
		}
		set
		{
			if (value != _currency)
			{
				_currency = value;
				OnPropertyChanged("Currency");
			}
		}
	}

	#endregion

	#region Children

	[Association(Storage = null, OtherKey = "Country", Name = " INTEG_61                       ")]
	[DebuggerNonUserCode]
	public EntitySet<Customer> Customer
	{
		get;set;
	}


	#endregion

}

[Table(Name = " Foo .CUSTOMER")]
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

	#region string AddressLine1

	private string _addressLine1;
	[DebuggerNonUserCode]
	[Column(Storage = "_addressLine1", Name = "ADDRESS_LINE1", DbType = null)]
	public string AddressLine1
	{
		get
		{
			return _addressLine1;
		}
		set
		{
			if (value != _addressLine1)
			{
				_addressLine1 = value;
				OnPropertyChanged("AddressLine1");
			}
		}
	}

	#endregion

	#region string AddressLine2

	private string _addressLine2;
	[DebuggerNonUserCode]
	[Column(Storage = "_addressLine2", Name = "ADDRESS_LINE2", DbType = null)]
	public string AddressLine2
	{
		get
		{
			return _addressLine2;
		}
		set
		{
			if (value != _addressLine2)
			{
				_addressLine2 = value;
				OnPropertyChanged("AddressLine2");
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

	#region string ContactFirst

	private string _contactFirst;
	[DebuggerNonUserCode]
	[Column(Storage = "_contactFirst", Name = "CONTACT_FIRST", DbType = null)]
	public string ContactFirst
	{
		get
		{
			return _contactFirst;
		}
		set
		{
			if (value != _contactFirst)
			{
				_contactFirst = value;
				OnPropertyChanged("ContactFirst");
			}
		}
	}

	#endregion

	#region string ContactLast

	private string _contactLast;
	[DebuggerNonUserCode]
	[Column(Storage = "_contactLast", Name = "CONTACT_LAST", DbType = null)]
	public string ContactLast
	{
		get
		{
			return _contactLast;
		}
		set
		{
			if (value != _contactLast)
			{
				_contactLast = value;
				OnPropertyChanged("ContactLast");
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

	#region int CuSTNo

	private int _cuStnO;
	[DebuggerNonUserCode]
	[Column(Storage = "_cuStnO", Name = "CUST_NO", DbType = null, IsPrimaryKey = true, CanBeNull = false)]
	public int CuSTNo
	{
		get
		{
			return _cuStnO;
		}
		set
		{
			if (value != _cuStnO)
			{
				_cuStnO = value;
				OnPropertyChanged("CuSTNo");
			}
		}
	}

	#endregion

	#region string Customer1

	private string _customer1;
	[DebuggerNonUserCode]
	[Column(Storage = "_customer1", Name = "CUSTOMER", DbType = null, CanBeNull = false)]
	public string Customer1
	{
		get
		{
			return _customer1;
		}
		set
		{
			if (value != _customer1)
			{
				_customer1 = value;
				OnPropertyChanged("Customer1");
			}
		}
	}

	#endregion

	#region string OnHold

	private string _onHold;
	[DebuggerNonUserCode]
	[Column(Storage = "_onHold", Name = "ON_HOLD", DbType = null)]
	public string OnHold
	{
		get
		{
			return _onHold;
		}
		set
		{
			if (value != _onHold)
			{
				_onHold = value;
				OnPropertyChanged("OnHold");
			}
		}
	}

	#endregion

	#region string PhoneNo

	private string _phoneNo;
	[DebuggerNonUserCode]
	[Column(Storage = "_phoneNo", Name = "PHONE_NO", DbType = null)]
	public string PhoneNo
	{
		get
		{
			return _phoneNo;
		}
		set
		{
			if (value != _phoneNo)
			{
				_phoneNo = value;
				OnPropertyChanged("PhoneNo");
			}
		}
	}

	#endregion

	#region string PostalCode

	private string _postalCode;
	[DebuggerNonUserCode]
	[Column(Storage = "_postalCode", Name = "POSTAL_CODE", DbType = null)]
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

	#region string StateProvince

	private string _stateProvince;
	[DebuggerNonUserCode]
	[Column(Storage = "_stateProvince", Name = "STATE_PROVINCE", DbType = null)]
	public string StateProvince
	{
		get
		{
			return _stateProvince;
		}
		set
		{
			if (value != _stateProvince)
			{
				_stateProvince = value;
				OnPropertyChanged("StateProvince");
			}
		}
	}

	#endregion

	#region Children

	[Association(Storage = null, OtherKey = "CuSTNo", Name = " INTEG_77                       ")]
	[DebuggerNonUserCode]
	public EntitySet<Sales> Sales
	{
		get;set;
	}


	#endregion

	#region Parents

	private EntityRef<Country> _countryCountry;
	[Association(Storage = "_countryCountry", ThisKey = "Country", Name = " INTEG_61                       ", IsForeignKey = true)]
	[DebuggerNonUserCode]
	public Country CountryCountry
	{
		get
		{
			return _countryCountry.Entity;
		}
		set
		{
			_countryCountry.Entity = value;
		}
	}


	#endregion

}

[Table(Name = " Foo .DEPARTMENT")]
public partial class Department : INotifyPropertyChanged
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

	#region long? Budget

	private long? _budget;
	[DebuggerNonUserCode]
	[Column(Storage = "_budget", Name = "BUDGET", DbType = null)]
	public long? Budget
	{
		get
		{
			return _budget;
		}
		set
		{
			if (value != _budget)
			{
				_budget = value;
				OnPropertyChanged("Budget");
			}
		}
	}

	#endregion

	#region string Department1

	private string _department1;
	[DebuggerNonUserCode]
	[Column(Storage = "_department1", Name = "DEPARTMENT", DbType = null, CanBeNull = false)]
	public string Department1
	{
		get
		{
			return _department1;
		}
		set
		{
			if (value != _department1)
			{
				_department1 = value;
				OnPropertyChanged("Department1");
			}
		}
	}

	#endregion

	#region string DEPtNo

	private string _depTNo;
	[DebuggerNonUserCode]
	[Column(Storage = "_depTNo", Name = "DEPT_NO", DbType = null, IsPrimaryKey = true, CanBeNull = false)]
	public string DEPtNo
	{
		get
		{
			return _depTNo;
		}
		set
		{
			if (value != _depTNo)
			{
				_depTNo = value;
				OnPropertyChanged("DEPtNo");
			}
		}
	}

	#endregion

	#region string HeadDEPt

	private string _headDepT;
	[DebuggerNonUserCode]
	[Column(Storage = "_headDepT", Name = "HEAD_DEPT", DbType = null)]
	public string HeadDEPt
	{
		get
		{
			return _headDepT;
		}
		set
		{
			if (value != _headDepT)
			{
				_headDepT = value;
				OnPropertyChanged("HeadDEPt");
			}
		}
	}

	#endregion

	#region string Location

	private string _location;
	[DebuggerNonUserCode]
	[Column(Storage = "_location", Name = "LOCATION", DbType = null)]
	public string Location
	{
		get
		{
			return _location;
		}
		set
		{
			if (value != _location)
			{
				_location = value;
				OnPropertyChanged("Location");
			}
		}
	}

	#endregion

	#region short? MNGRNo

	private short? _mngrnO;
	[DebuggerNonUserCode]
	[Column(Storage = "_mngrnO", Name = "MNGR_NO", DbType = null)]
	public short? MNGRNo
	{
		get
		{
			return _mngrnO;
		}
		set
		{
			if (value != _mngrnO)
			{
				_mngrnO = value;
				OnPropertyChanged("MNGRNo");
			}
		}
	}

	#endregion

	#region string PhoneNo

	private string _phoneNo;
	[DebuggerNonUserCode]
	[Column(Storage = "_phoneNo", Name = "PHONE_NO", DbType = null)]
	public string PhoneNo
	{
		get
		{
			return _phoneNo;
		}
		set
		{
			if (value != _phoneNo)
			{
				_phoneNo = value;
				OnPropertyChanged("PhoneNo");
			}
		}
	}

	#endregion

	#region Children

	[Association(Storage = null, OtherKey = "DEPtNo", Name = " INTEG_28                       ")]
	[DebuggerNonUserCode]
	public EntitySet<Employee> Employee
	{
		get;set;
	}

	[Association(Storage = null, OtherKey = "DEPtNo", Name = " INTEG_47                       ")]
	[DebuggerNonUserCode]
	public EntitySet<ProJDEPtBudget> ProJDEPtBudget
	{
		get;set;
	}


	#endregion

}

[Table(Name = " Foo .EMPLOYEE")]
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

	#region string DEPtNo

	private string _depTNo;
	[DebuggerNonUserCode]
	[Column(Storage = "_depTNo", Name = "DEPT_NO", DbType = null, CanBeNull = false)]
	public string DEPtNo
	{
		get
		{
			return _depTNo;
		}
		set
		{
			if (value != _depTNo)
			{
				_depTNo = value;
				OnPropertyChanged("DEPtNo");
			}
		}
	}

	#endregion

	#region short EmPNo

	private short _emPnO;
	[DebuggerNonUserCode]
	[Column(Storage = "_emPnO", Name = "EMP_NO", DbType = null, IsPrimaryKey = true, CanBeNull = false)]
	public short EmPNo
	{
		get
		{
			return _emPnO;
		}
		set
		{
			if (value != _emPnO)
			{
				_emPnO = value;
				OnPropertyChanged("EmPNo");
			}
		}
	}

	#endregion

	#region string FirstName

	private string _firstName;
	[DebuggerNonUserCode]
	[Column(Storage = "_firstName", Name = "FIRST_NAME", DbType = null, CanBeNull = false)]
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

	#region string FullName

	private string _fullName;
	[DebuggerNonUserCode]
	[Column(Storage = "_fullName", Name = "FULL_NAME", DbType = null, IsDbGenerated = true)]
	public string FullName
	{
		get
		{
			return _fullName;
		}
		set
		{
			if (value != _fullName)
			{
				_fullName = value;
				OnPropertyChanged("FullName");
			}
		}
	}

	#endregion

	#region DateTime HireDate

	private DateTime _hireDate;
	[DebuggerNonUserCode]
	[Column(Storage = "_hireDate", Name = "HIRE_DATE", DbType = null, CanBeNull = false)]
	public DateTime HireDate
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

	#region string JobCode

	private string _jobCode;
	[DebuggerNonUserCode]
	[Column(Storage = "_jobCode", Name = "JOB_CODE", DbType = null, CanBeNull = false)]
	public string JobCode
	{
		get
		{
			return _jobCode;
		}
		set
		{
			if (value != _jobCode)
			{
				_jobCode = value;
				OnPropertyChanged("JobCode");
			}
		}
	}

	#endregion

	#region string JobCountry

	private string _jobCountry;
	[DebuggerNonUserCode]
	[Column(Storage = "_jobCountry", Name = "JOB_COUNTRY", DbType = null, CanBeNull = false)]
	public string JobCountry
	{
		get
		{
			return _jobCountry;
		}
		set
		{
			if (value != _jobCountry)
			{
				_jobCountry = value;
				OnPropertyChanged("JobCountry");
			}
		}
	}

	#endregion

	#region short JobGrade

	private short _jobGrade;
	[DebuggerNonUserCode]
	[Column(Storage = "_jobGrade", Name = "JOB_GRADE", DbType = null, CanBeNull = false)]
	public short JobGrade
	{
		get
		{
			return _jobGrade;
		}
		set
		{
			if (value != _jobGrade)
			{
				_jobGrade = value;
				OnPropertyChanged("JobGrade");
			}
		}
	}

	#endregion

	#region string LastName

	private string _lastName;
	[DebuggerNonUserCode]
	[Column(Storage = "_lastName", Name = "LAST_NAME", DbType = null, CanBeNull = false)]
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

	#region string PhoneExT

	private string _phoneExT;
	[DebuggerNonUserCode]
	[Column(Storage = "_phoneExT", Name = "PHONE_EXT", DbType = null)]
	public string PhoneExT
	{
		get
		{
			return _phoneExT;
		}
		set
		{
			if (value != _phoneExT)
			{
				_phoneExT = value;
				OnPropertyChanged("PhoneExT");
			}
		}
	}

	#endregion

	#region long Salary

	private long _salary;
	[DebuggerNonUserCode]
	[Column(Storage = "_salary", Name = "SALARY", DbType = null, CanBeNull = false)]
	public long Salary
	{
		get
		{
			return _salary;
		}
		set
		{
			if (value != _salary)
			{
				_salary = value;
				OnPropertyChanged("Salary");
			}
		}
	}

	#endregion

	#region Children

	[Association(Storage = null, OtherKey = "EmPNo", Name = " INTEG_40                       ")]
	[DebuggerNonUserCode]
	public EntitySet<EmployeeProject> EmployeeProject
	{
		get;set;
	}

	[Association(Storage = null, OtherKey = "EmPNo", Name = " INTEG_56                       ")]
	[DebuggerNonUserCode]
	public EntitySet<SalaryHistory> SalaryHistory
	{
		get;set;
	}


	#endregion

	#region Parents

	private EntityRef<Department> _department;
	[Association(Storage = "_department", ThisKey = "DEPtNo", Name = " INTEG_28                       ", IsForeignKey = true)]
	[DebuggerNonUserCode]
	public Department Department
	{
		get
		{
			return _department.Entity;
		}
		set
		{
			_department.Entity = value;
		}
	}

	private EntityRef<Job> _job;
	[Association(Storage = "_job", ThisKey = "JobCode", Name = " INTEG_29                       ", IsForeignKey = true)]
	[DebuggerNonUserCode]
	public Job Job
	{
		get
		{
			return _job.Entity;
		}
		set
		{
			_job.Entity = value;
		}
	}

	private EntityRef<Job> _job;
	[Association(Storage = "_job", ThisKey = "JobCountry", Name = " INTEG_29                       ", IsForeignKey = true)]
	[DebuggerNonUserCode]
	public Job Job
	{
		get
		{
			return _job.Entity;
		}
		set
		{
			_job.Entity = value;
		}
	}

	private EntityRef<Job> _job;
	[Association(Storage = "_job", ThisKey = "JobGrade", Name = " INTEG_29                       ", IsForeignKey = true)]
	[DebuggerNonUserCode]
	public Job Job
	{
		get
		{
			return _job.Entity;
		}
		set
		{
			_job.Entity = value;
		}
	}


	#endregion

}

[Table(Name = " Foo .EMPLOYEE_PROJECT")]
public partial class EmployeeProject : INotifyPropertyChanged
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

	#region short EmPNo

	private short _emPnO;
	[DebuggerNonUserCode]
	[Column(Storage = "_emPnO", Name = "EMP_NO", DbType = null, IsPrimaryKey = true, CanBeNull = false)]
	public short EmPNo
	{
		get
		{
			return _emPnO;
		}
		set
		{
			if (value != _emPnO)
			{
				_emPnO = value;
				OnPropertyChanged("EmPNo");
			}
		}
	}

	#endregion

	#region string ProJID

	private string _proJid;
	[DebuggerNonUserCode]
	[Column(Storage = "_proJid", Name = "PROJ_ID", DbType = null, IsPrimaryKey = true, CanBeNull = false)]
	public string ProJID
	{
		get
		{
			return _proJid;
		}
		set
		{
			if (value != _proJid)
			{
				_proJid = value;
				OnPropertyChanged("ProJID");
			}
		}
	}

	#endregion

	#region Parents

	private EntityRef<Employee> _employee;
	[Association(Storage = "_employee", ThisKey = "EmPNo", Name = " INTEG_40                       ", IsForeignKey = true)]
	[DebuggerNonUserCode]
	public Employee Employee
	{
		get
		{
			return _employee.Entity;
		}
		set
		{
			_employee.Entity = value;
		}
	}

	private EntityRef<Project> _project;
	[Association(Storage = "_project", ThisKey = "ProJID", Name = " INTEG_41                       ", IsForeignKey = true)]
	[DebuggerNonUserCode]
	public Project Project
	{
		get
		{
			return _project.Entity;
		}
		set
		{
			_project.Entity = value;
		}
	}


	#endregion

}

[Table(Name = " Foo .JOB")]
public partial class Job : INotifyPropertyChanged
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

	#region string JobCode

	private string _jobCode;
	[DebuggerNonUserCode]
	[Column(Storage = "_jobCode", Name = "JOB_CODE", DbType = null, IsPrimaryKey = true, CanBeNull = false)]
	public string JobCode
	{
		get
		{
			return _jobCode;
		}
		set
		{
			if (value != _jobCode)
			{
				_jobCode = value;
				OnPropertyChanged("JobCode");
			}
		}
	}

	#endregion

	#region string JobCountry

	private string _jobCountry;
	[DebuggerNonUserCode]
	[Column(Storage = "_jobCountry", Name = "JOB_COUNTRY", DbType = null, IsPrimaryKey = true, CanBeNull = false)]
	public string JobCountry
	{
		get
		{
			return _jobCountry;
		}
		set
		{
			if (value != _jobCountry)
			{
				_jobCountry = value;
				OnPropertyChanged("JobCountry");
			}
		}
	}

	#endregion

	#region short JobGrade

	private short _jobGrade;
	[DebuggerNonUserCode]
	[Column(Storage = "_jobGrade", Name = "JOB_GRADE", DbType = null, IsPrimaryKey = true, CanBeNull = false)]
	public short JobGrade
	{
		get
		{
			return _jobGrade;
		}
		set
		{
			if (value != _jobGrade)
			{
				_jobGrade = value;
				OnPropertyChanged("JobGrade");
			}
		}
	}

	#endregion

	#region Byte[] JobRequirement

	private Byte[] _jobRequirement;
	[DebuggerNonUserCode]
	[Column(Storage = "_jobRequirement", Name = "JOB_REQUIREMENT", DbType = null)]
	public Byte[] JobRequirement
	{
		get
		{
			return _jobRequirement;
		}
		set
		{
			if (value != _jobRequirement)
			{
				_jobRequirement = value;
				OnPropertyChanged("JobRequirement");
			}
		}
	}

	#endregion

	#region string JobTitle

	private string _jobTitle;
	[DebuggerNonUserCode]
	[Column(Storage = "_jobTitle", Name = "JOB_TITLE", DbType = null, CanBeNull = false)]
	public string JobTitle
	{
		get
		{
			return _jobTitle;
		}
		set
		{
			if (value != _jobTitle)
			{
				_jobTitle = value;
				OnPropertyChanged("JobTitle");
			}
		}
	}

	#endregion

	#region string LanguageReQ

	private string _languageReQ;
	[DebuggerNonUserCode]
	[Column(Storage = "_languageReQ", Name = "LANGUAGE_REQ", DbType = null)]
	public string LanguageReQ
	{
		get
		{
			return _languageReQ;
		}
		set
		{
			if (value != _languageReQ)
			{
				_languageReQ = value;
				OnPropertyChanged("LanguageReQ");
			}
		}
	}

	#endregion

	#region long MaXSalary

	private long _maXsAlary;
	[DebuggerNonUserCode]
	[Column(Storage = "_maXsAlary", Name = "MAX_SALARY", DbType = null, CanBeNull = false)]
	public long MaXSalary
	{
		get
		{
			return _maXsAlary;
		}
		set
		{
			if (value != _maXsAlary)
			{
				_maXsAlary = value;
				OnPropertyChanged("MaXSalary");
			}
		}
	}

	#endregion

	#region long MInSalary

	private long _miNSalary;
	[DebuggerNonUserCode]
	[Column(Storage = "_miNSalary", Name = "MIN_SALARY", DbType = null, CanBeNull = false)]
	public long MInSalary
	{
		get
		{
			return _miNSalary;
		}
		set
		{
			if (value != _miNSalary)
			{
				_miNSalary = value;
				OnPropertyChanged("MInSalary");
			}
		}
	}

	#endregion

	#region Children

	[Association(Storage = null, OtherKey = "JobCode", Name = " INTEG_29                       ")]
	[DebuggerNonUserCode]
	public EntitySet<Employee> Employee
	{
		get;set;
	}

	[Association(Storage = null, OtherKey = "JobCountry", Name = " INTEG_29                       ")]
	[DebuggerNonUserCode]
	public EntitySet<Employee> Employee
	{
		get;set;
	}

	[Association(Storage = null, OtherKey = "JobGrade", Name = " INTEG_29                       ")]
	[DebuggerNonUserCode]
	public EntitySet<Employee> Employee
	{
		get;set;
	}


	#endregion

}

[Table(Name = " Foo .PHONE_LIST")]
public partial class PhoneList : INotifyPropertyChanged
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

	#region short? EmPNo

	private short? _emPnO;
	[DebuggerNonUserCode]
	[Column(Storage = "_emPnO", Name = "EMP_NO", DbType = null)]
	public short? EmPNo
	{
		get
		{
			return _emPnO;
		}
		set
		{
			if (value != _emPnO)
			{
				_emPnO = value;
				OnPropertyChanged("EmPNo");
			}
		}
	}

	#endregion

	#region string FirstName

	private string _firstName;
	[DebuggerNonUserCode]
	[Column(Storage = "_firstName", Name = "FIRST_NAME", DbType = null)]
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

	#region string LastName

	private string _lastName;
	[DebuggerNonUserCode]
	[Column(Storage = "_lastName", Name = "LAST_NAME", DbType = null)]
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

	#region string Location

	private string _location;
	[DebuggerNonUserCode]
	[Column(Storage = "_location", Name = "LOCATION", DbType = null)]
	public string Location
	{
		get
		{
			return _location;
		}
		set
		{
			if (value != _location)
			{
				_location = value;
				OnPropertyChanged("Location");
			}
		}
	}

	#endregion

	#region string PhoneExT

	private string _phoneExT;
	[DebuggerNonUserCode]
	[Column(Storage = "_phoneExT", Name = "PHONE_EXT", DbType = null)]
	public string PhoneExT
	{
		get
		{
			return _phoneExT;
		}
		set
		{
			if (value != _phoneExT)
			{
				_phoneExT = value;
				OnPropertyChanged("PhoneExT");
			}
		}
	}

	#endregion

	#region string PhoneNo

	private string _phoneNo;
	[DebuggerNonUserCode]
	[Column(Storage = "_phoneNo", Name = "PHONE_NO", DbType = null)]
	public string PhoneNo
	{
		get
		{
			return _phoneNo;
		}
		set
		{
			if (value != _phoneNo)
			{
				_phoneNo = value;
				OnPropertyChanged("PhoneNo");
			}
		}
	}

	#endregion

}

[Table(Name = " Foo .PROJ_DEPT_BUDGET")]
public partial class ProJDEPtBudget : INotifyPropertyChanged
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

	#region string DEPtNo

	private string _depTNo;
	[DebuggerNonUserCode]
	[Column(Storage = "_depTNo", Name = "DEPT_NO", DbType = null, IsPrimaryKey = true, CanBeNull = false)]
	public string DEPtNo
	{
		get
		{
			return _depTNo;
		}
		set
		{
			if (value != _depTNo)
			{
				_depTNo = value;
				OnPropertyChanged("DEPtNo");
			}
		}
	}

	#endregion

	#region int FiscalYear

	private int _fiscalYear;
	[DebuggerNonUserCode]
	[Column(Storage = "_fiscalYear", Name = "FISCAL_YEAR", DbType = null, IsPrimaryKey = true, CanBeNull = false)]
	public int FiscalYear
	{
		get
		{
			return _fiscalYear;
		}
		set
		{
			if (value != _fiscalYear)
			{
				_fiscalYear = value;
				OnPropertyChanged("FiscalYear");
			}
		}
	}

	#endregion

	#region long? ProjectedBudget

	private long? _projectedBudget;
	[DebuggerNonUserCode]
	[Column(Storage = "_projectedBudget", Name = "PROJECTED_BUDGET", DbType = null)]
	public long? ProjectedBudget
	{
		get
		{
			return _projectedBudget;
		}
		set
		{
			if (value != _projectedBudget)
			{
				_projectedBudget = value;
				OnPropertyChanged("ProjectedBudget");
			}
		}
	}

	#endregion

	#region string ProJID

	private string _proJid;
	[DebuggerNonUserCode]
	[Column(Storage = "_proJid", Name = "PROJ_ID", DbType = null, IsPrimaryKey = true, CanBeNull = false)]
	public string ProJID
	{
		get
		{
			return _proJid;
		}
		set
		{
			if (value != _proJid)
			{
				_proJid = value;
				OnPropertyChanged("ProJID");
			}
		}
	}

	#endregion

	#region int? QuartHeadCNt

	private int? _quartHeadCnT;
	[DebuggerNonUserCode]
	[Column(Storage = "_quartHeadCnT", Name = "QUART_HEAD_CNT", DbType = null)]
	public int? QuartHeadCNt
	{
		get
		{
			return _quartHeadCnT;
		}
		set
		{
			if (value != _quartHeadCnT)
			{
				_quartHeadCnT = value;
				OnPropertyChanged("QuartHeadCNt");
			}
		}
	}

	#endregion

	#region Parents

	private EntityRef<Department> _department;
	[Association(Storage = "_department", ThisKey = "DEPtNo", Name = " INTEG_47                       ", IsForeignKey = true)]
	[DebuggerNonUserCode]
	public Department Department
	{
		get
		{
			return _department.Entity;
		}
		set
		{
			_department.Entity = value;
		}
	}

	private EntityRef<Project> _project;
	[Association(Storage = "_project", ThisKey = "ProJID", Name = " INTEG_48                       ", IsForeignKey = true)]
	[DebuggerNonUserCode]
	public Project Project
	{
		get
		{
			return _project.Entity;
		}
		set
		{
			_project.Entity = value;
		}
	}


	#endregion

}

[Table(Name = " Foo .PROJECT")]
public partial class Project : INotifyPropertyChanged
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

	#region string Product

	private string _product;
	[DebuggerNonUserCode]
	[Column(Storage = "_product", Name = "PRODUCT", DbType = null)]
	public string Product
	{
		get
		{
			return _product;
		}
		set
		{
			if (value != _product)
			{
				_product = value;
				OnPropertyChanged("Product");
			}
		}
	}

	#endregion

	#region Byte[] ProJDesC

	private Byte[] _proJdEsC;
	[DebuggerNonUserCode]
	[Column(Storage = "_proJdEsC", Name = "PROJ_DESC", DbType = null)]
	public Byte[] ProJDesC
	{
		get
		{
			return _proJdEsC;
		}
		set
		{
			if (value != _proJdEsC)
			{
				_proJdEsC = value;
				OnPropertyChanged("ProJDesC");
			}
		}
	}

	#endregion

	#region string ProJID

	private string _proJid;
	[DebuggerNonUserCode]
	[Column(Storage = "_proJid", Name = "PROJ_ID", DbType = null, IsPrimaryKey = true, CanBeNull = false)]
	public string ProJID
	{
		get
		{
			return _proJid;
		}
		set
		{
			if (value != _proJid)
			{
				_proJid = value;
				OnPropertyChanged("ProJID");
			}
		}
	}

	#endregion

	#region string ProJName

	private string _proJnAme;
	[DebuggerNonUserCode]
	[Column(Storage = "_proJnAme", Name = "PROJ_NAME", DbType = null, CanBeNull = false)]
	public string ProJName
	{
		get
		{
			return _proJnAme;
		}
		set
		{
			if (value != _proJnAme)
			{
				_proJnAme = value;
				OnPropertyChanged("ProJName");
			}
		}
	}

	#endregion

	#region short? TeamLeader

	private short? _teamLeader;
	[DebuggerNonUserCode]
	[Column(Storage = "_teamLeader", Name = "TEAM_LEADER", DbType = null)]
	public short? TeamLeader
	{
		get
		{
			return _teamLeader;
		}
		set
		{
			if (value != _teamLeader)
			{
				_teamLeader = value;
				OnPropertyChanged("TeamLeader");
			}
		}
	}

	#endregion

	#region Children

	[Association(Storage = null, OtherKey = "ProJID", Name = " INTEG_41                       ")]
	[DebuggerNonUserCode]
	public EntitySet<EmployeeProject> EmployeeProject
	{
		get;set;
	}

	[Association(Storage = null, OtherKey = "ProJID", Name = " INTEG_48                       ")]
	[DebuggerNonUserCode]
	public EntitySet<ProJDEPtBudget> ProJDEPtBudget
	{
		get;set;
	}


	#endregion

}

[Table(Name = " Foo .SALARY_HISTORY")]
public partial class SalaryHistory : INotifyPropertyChanged
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

	#region DateTime ChangeDate

	private DateTime _changeDate;
	[DebuggerNonUserCode]
	[Column(Storage = "_changeDate", Name = "CHANGE_DATE", DbType = null, IsPrimaryKey = true, CanBeNull = false)]
	public DateTime ChangeDate
	{
		get
		{
			return _changeDate;
		}
		set
		{
			if (value != _changeDate)
			{
				_changeDate = value;
				OnPropertyChanged("ChangeDate");
			}
		}
	}

	#endregion

	#region short EmPNo

	private short _emPnO;
	[DebuggerNonUserCode]
	[Column(Storage = "_emPnO", Name = "EMP_NO", DbType = null, IsPrimaryKey = true, CanBeNull = false)]
	public short EmPNo
	{
		get
		{
			return _emPnO;
		}
		set
		{
			if (value != _emPnO)
			{
				_emPnO = value;
				OnPropertyChanged("EmPNo");
			}
		}
	}

	#endregion

	#region double? NewSalary

	private double? _newSalary;
	[DebuggerNonUserCode]
	[Column(Storage = "_newSalary", Name = "NEW_SALARY", DbType = null, IsDbGenerated = true)]
	public double? NewSalary
	{
		get
		{
			return _newSalary;
		}
		set
		{
			if (value != _newSalary)
			{
				_newSalary = value;
				OnPropertyChanged("NewSalary");
			}
		}
	}

	#endregion

	#region long OldSalary

	private long _oldSalary;
	[DebuggerNonUserCode]
	[Column(Storage = "_oldSalary", Name = "OLD_SALARY", DbType = null, CanBeNull = false)]
	public long OldSalary
	{
		get
		{
			return _oldSalary;
		}
		set
		{
			if (value != _oldSalary)
			{
				_oldSalary = value;
				OnPropertyChanged("OldSalary");
			}
		}
	}

	#endregion

	#region double PercentChange

	private double _percentChange;
	[DebuggerNonUserCode]
	[Column(Storage = "_percentChange", Name = "PERCENT_CHANGE", DbType = null, CanBeNull = false)]
	public double PercentChange
	{
		get
		{
			return _percentChange;
		}
		set
		{
			if (value != _percentChange)
			{
				_percentChange = value;
				OnPropertyChanged("PercentChange");
			}
		}
	}

	#endregion

	#region string UpdaterID

	private string _updaterID;
	[DebuggerNonUserCode]
	[Column(Storage = "_updaterID", Name = "UPDATER_ID", DbType = null, IsPrimaryKey = true, CanBeNull = false)]
	public string UpdaterID
	{
		get
		{
			return _updaterID;
		}
		set
		{
			if (value != _updaterID)
			{
				_updaterID = value;
				OnPropertyChanged("UpdaterID");
			}
		}
	}

	#endregion

	#region Parents

	private EntityRef<Employee> _employee;
	[Association(Storage = "_employee", ThisKey = "EmPNo", Name = " INTEG_56                       ", IsForeignKey = true)]
	[DebuggerNonUserCode]
	public Employee Employee
	{
		get
		{
			return _employee.Entity;
		}
		set
		{
			_employee.Entity = value;
		}
	}


	#endregion

}

[Table(Name = " Foo .SALES")]
public partial class Sales : INotifyPropertyChanged
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

	#region long? Aged

	private long? _aged;
	[DebuggerNonUserCode]
	[Column(Storage = "_aged", Name = "AGED", DbType = null, IsDbGenerated = true)]
	public long? Aged
	{
		get
		{
			return _aged;
		}
		set
		{
			if (value != _aged)
			{
				_aged = value;
				OnPropertyChanged("Aged");
			}
		}
	}

	#endregion

	#region int CuSTNo

	private int _cuStnO;
	[DebuggerNonUserCode]
	[Column(Storage = "_cuStnO", Name = "CUST_NO", DbType = null, CanBeNull = false)]
	public int CuSTNo
	{
		get
		{
			return _cuStnO;
		}
		set
		{
			if (value != _cuStnO)
			{
				_cuStnO = value;
				OnPropertyChanged("CuSTNo");
			}
		}
	}

	#endregion

	#region DateTime? DateNeeded

	private DateTime? _dateNeeded;
	[DebuggerNonUserCode]
	[Column(Storage = "_dateNeeded", Name = "DATE_NEEDED", DbType = null)]
	public DateTime? DateNeeded
	{
		get
		{
			return _dateNeeded;
		}
		set
		{
			if (value != _dateNeeded)
			{
				_dateNeeded = value;
				OnPropertyChanged("DateNeeded");
			}
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

	#region string ItemType

	private string _itemType;
	[DebuggerNonUserCode]
	[Column(Storage = "_itemType", Name = "ITEM_TYPE", DbType = null)]
	public string ItemType
	{
		get
		{
			return _itemType;
		}
		set
		{
			if (value != _itemType)
			{
				_itemType = value;
				OnPropertyChanged("ItemType");
			}
		}
	}

	#endregion

	#region DateTime OrderDate

	private DateTime _orderDate;
	[DebuggerNonUserCode]
	[Column(Storage = "_orderDate", Name = "ORDER_DATE", DbType = null, CanBeNull = false)]
	public DateTime OrderDate
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

	#region string OrderStatus

	private string _orderStatus;
	[DebuggerNonUserCode]
	[Column(Storage = "_orderStatus", Name = "ORDER_STATUS", DbType = null, CanBeNull = false)]
	public string OrderStatus
	{
		get
		{
			return _orderStatus;
		}
		set
		{
			if (value != _orderStatus)
			{
				_orderStatus = value;
				OnPropertyChanged("OrderStatus");
			}
		}
	}

	#endregion

	#region string Paid

	private string _paid;
	[DebuggerNonUserCode]
	[Column(Storage = "_paid", Name = "PAID", DbType = null)]
	public string Paid
	{
		get
		{
			return _paid;
		}
		set
		{
			if (value != _paid)
			{
				_paid = value;
				OnPropertyChanged("Paid");
			}
		}
	}

	#endregion

	#region string PONumber

	private string _ponUmber;
	[DebuggerNonUserCode]
	[Column(Storage = "_ponUmber", Name = "PO_NUMBER", DbType = null, IsPrimaryKey = true, CanBeNull = false)]
	public string PONumber
	{
		get
		{
			return _ponUmber;
		}
		set
		{
			if (value != _ponUmber)
			{
				_ponUmber = value;
				OnPropertyChanged("PONumber");
			}
		}
	}

	#endregion

	#region int QTYOrdered

	private int _qtyoRdered;
	[DebuggerNonUserCode]
	[Column(Storage = "_qtyoRdered", Name = "QTY_ORDERED", DbType = null, CanBeNull = false)]
	public int QTYOrdered
	{
		get
		{
			return _qtyoRdered;
		}
		set
		{
			if (value != _qtyoRdered)
			{
				_qtyoRdered = value;
				OnPropertyChanged("QTYOrdered");
			}
		}
	}

	#endregion

	#region short? SalesRep

	private short? _salesRep;
	[DebuggerNonUserCode]
	[Column(Storage = "_salesRep", Name = "SALES_REP", DbType = null)]
	public short? SalesRep
	{
		get
		{
			return _salesRep;
		}
		set
		{
			if (value != _salesRep)
			{
				_salesRep = value;
				OnPropertyChanged("SalesRep");
			}
		}
	}

	#endregion

	#region DateTime? ShipDate

	private DateTime? _shipDate;
	[DebuggerNonUserCode]
	[Column(Storage = "_shipDate", Name = "SHIP_DATE", DbType = null)]
	public DateTime? ShipDate
	{
		get
		{
			return _shipDate;
		}
		set
		{
			if (value != _shipDate)
			{
				_shipDate = value;
				OnPropertyChanged("ShipDate");
			}
		}
	}

	#endregion

	#region int TotalValue

	private int _totalValue;
	[DebuggerNonUserCode]
	[Column(Storage = "_totalValue", Name = "TOTAL_VALUE", DbType = null, CanBeNull = false)]
	public int TotalValue
	{
		get
		{
			return _totalValue;
		}
		set
		{
			if (value != _totalValue)
			{
				_totalValue = value;
				OnPropertyChanged("TotalValue");
			}
		}
	}

	#endregion

	#region Parents

	private EntityRef<Customer> _customer;
	[Association(Storage = "_customer", ThisKey = "CuSTNo", Name = " INTEG_77                       ", IsForeignKey = true)]
	[DebuggerNonUserCode]
	public Customer Customer
	{
		get
		{
			return _customer.Entity;
		}
		set
		{
			_customer.Entity = value;
		}
	}


	#endregion

}
