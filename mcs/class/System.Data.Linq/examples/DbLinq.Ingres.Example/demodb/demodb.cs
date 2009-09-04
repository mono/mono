#region Auto-generated classes for demodb database on 2008-03-27 14:55:28Z

//
//  ____  _     __  __      _        _
// |  _ \| |__ |  \/  | ___| |_ __ _| |
// | | | | '_ \| |\/| |/ _ \ __/ _` | |
// | |_| | |_) | |  | |  __/ || (_| | |
// |____/|_.__/|_|  |_|\___|\__\__,_|_|
//
// Auto-generated from demodb on 2008-03-27 14:55:28Z
// Please visit http://linq.to/db for more information

#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Linq.Mapping;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using DbLinq.Data.Linq;

public partial class Demodb : DbLinq.Ingres.IngresDataContext
{
	//public demodb(string connectionString)
	//    : base(connectionString)
	//{
	//}

	public Demodb(IDbConnection connection)
	    : base(connection)
	{
	}

	public Table<AdminAirline> AdminAirline { get { return GetTable<AdminAirline>(); } }
	public Table<AdminAirport> AdminAirport { get { return GetTable<AdminAirport>(); } }
	public Table<AdminCountry> AdminCountry { get { return GetTable<AdminCountry>(); } }
	public Table<AdminFlightDay> AdminFlightDay { get { return GetTable<AdminFlightDay>(); } }
	public Table<AdminFullRoute> AdminFullRoute { get { return GetTable<AdminFullRoute>(); } }
	public Table<AdminIietabF5f6> AdminIietabF5f6 { get { return GetTable<AdminIietabF5f6>(); } }
	public Table<AdminRoute> AdminRoute { get { return GetTable<AdminRoute>(); } }
	public Table<AdminTz> AdminTz { get { return GetTable<AdminTz>(); } }
	public Table<AdminUserProfile> AdminUserProfile { get { return GetTable<AdminUserProfile>(); } }
	public Table<AdminVersion> AdminVersion { get { return GetTable<AdminVersion>(); } }

}

[Table(Name = "admin.airline")]
public partial class AdminAirline : INotifyPropertyChanged
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

	#region string AlCcode

	private string alCcode;
	[Column(Storage = "alCcode", Name = "al_ccode", DbType = "NCHAR(2)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public string AlCcode
	{
		get
		{
			return alCcode;
		}
		set
		{
			if (value != alCcode)
			{
				alCcode = value;
				OnPropertyChanged("AlCcode");
			}
		}
	}

	#endregion

	#region string AlIatacode

	private string alIatacode;
	[Column(Storage = "alIatacode", Name = "al_iatacode", DbType = "NCHAR(2)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public string AlIatacode
	{
		get
		{
			return alIatacode;
		}
		set
		{
			if (value != alIatacode)
			{
				alIatacode = value;
				OnPropertyChanged("AlIatacode");
			}
		}
	}

	#endregion

	#region string AlIcaocode

	private string alIcaocode;
	[Column(Storage = "alIcaocode", Name = "al_icaocode", DbType = "NCHAR(3)", IsPrimaryKey = true, CanBeNull = false)]
	[DebuggerNonUserCode]
	public string AlIcaocode
	{
		get
		{
			return alIcaocode;
		}
		set
		{
			if (value != alIcaocode)
			{
				alIcaocode = value;
				OnPropertyChanged("AlIcaocode");
			}
		}
	}

	#endregion

	#region  AlId

	private Int32 alId;
	[Column(Storage = "alId", Name = "al_id", DbType = "INTEGER(4)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public Int32 AlId
	{
		get
		{
			return alId;
		}
		set
		{
			if (value != alId)
			{
				alId = value;
				OnPropertyChanged("AlId");
			}
		}
	}

	#endregion

	#region string AlName

	private string alName;
	[Column(Storage = "alName", Name = "al_name", DbType = "NVARCHAR(60)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public string AlName
	{
		get
		{
			return alName;
		}
		set
		{
			if (value != alName)
			{
				alName = value;
				OnPropertyChanged("AlName");
			}
		}
	}

	#endregion

	#region GetHashCode(), Equals() - uses column AlIcaocode to look up objects in liveObjectMap

	public override int GetHashCode()
	{
		return AlIcaocode.GetHashCode();
	}

	public override bool Equals(object o)
	{
		AdminAirline other = o as AdminAirline;
		if (other == null)
		{
			return false;
		}
		return AlIcaocode.Equals(other.AlIcaocode);
	}

	#endregion

}

[Table(Name = "admin.airport")]
public partial class AdminAirport : INotifyPropertyChanged
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

	#region string ApCcode

	private string apCcode;
	[Column(Storage = "apCcode", Name = "ap_ccode", DbType = "NCHAR(2)")]
	[DebuggerNonUserCode]
	public string ApCcode
	{
		get
		{
			return apCcode;
		}
		set
		{
			if (value != apCcode)
			{
				apCcode = value;
				OnPropertyChanged("ApCcode");
			}
		}
	}

	#endregion

	#region string ApIatacode

	private string apIatacode;
	[Column(Storage = "apIatacode", Name = "ap_iatacode", DbType = "NCHAR(3)", IsPrimaryKey = true, CanBeNull = false)]
	[DebuggerNonUserCode]
	public string ApIatacode
	{
		get
		{
			return apIatacode;
		}
		set
		{
			if (value != apIatacode)
			{
				apIatacode = value;
				OnPropertyChanged("ApIatacode");
			}
		}
	}

	#endregion

	#region  ApId

	private Int32 apId;
	[Column(Storage = "apId", Name = "ap_id", DbType = "INTEGER(4)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public Int32 ApId
	{
		get
		{
			return apId;
		}
		set
		{
			if (value != apId)
			{
				apId = value;
				OnPropertyChanged("ApId");
			}
		}
	}

	#endregion

	#region string ApName

	private string apName;
	[Column(Storage = "apName", Name = "ap_name", DbType = "NVARCHAR(50)")]
	[DebuggerNonUserCode]
	public string ApName
	{
		get
		{
			return apName;
		}
		set
		{
			if (value != apName)
			{
				apName = value;
				OnPropertyChanged("ApName");
			}
		}
	}

	#endregion

	#region string ApPlace

	private string apPlace;
	[Column(Storage = "apPlace", Name = "ap_place", DbType = "NVARCHAR(30)")]
	[DebuggerNonUserCode]
	public string ApPlace
	{
		get
		{
			return apPlace;
		}
		set
		{
			if (value != apPlace)
			{
				apPlace = value;
				OnPropertyChanged("ApPlace");
			}
		}
	}

	#endregion

	#region GetHashCode(), Equals() - uses column ApIatacode to look up objects in liveObjectMap

	public override int GetHashCode()
	{
		return ApIatacode.GetHashCode();
	}

	public override bool Equals(object o)
	{
		AdminAirport other = o as AdminAirport;
		if (other == null)
		{
			return false;
		}
		return ApIatacode.Equals(other.ApIatacode);
	}

	#endregion

}

[Table(Name = "admin.country")]
public partial class AdminCountry : INotifyPropertyChanged
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

	#region string CtCode

	private string ctCode;
	[Column(Storage = "ctCode", Name = "ct_code", DbType = "NCHAR(2)", IsPrimaryKey = true, CanBeNull = false)]
	[DebuggerNonUserCode]
	public string CtCode
	{
		get
		{
			return ctCode;
		}
		set
		{
			if (value != ctCode)
			{
				ctCode = value;
				OnPropertyChanged("CtCode");
			}
		}
	}

	#endregion

	#region  CtId

	private Int32 ctId;
	[Column(Storage = "ctId", Name = "ct_id", DbType = "INTEGER(4)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public Int32 CtId
	{
		get
		{
			return ctId;
		}
		set
		{
			if (value != ctId)
			{
				ctId = value;
				OnPropertyChanged("CtId");
			}
		}
	}

	#endregion

	#region string CtName

	private string ctName;
	[Column(Storage = "ctName", Name = "ct_name", DbType = "NVARCHAR(50)")]
	[DebuggerNonUserCode]
	public string CtName
	{
		get
		{
			return ctName;
		}
		set
		{
			if (value != ctName)
			{
				ctName = value;
				OnPropertyChanged("CtName");
			}
		}
	}

	#endregion

	#region GetHashCode(), Equals() - uses column CtCode to look up objects in liveObjectMap

	public override int GetHashCode()
	{
		return CtCode.GetHashCode();
	}

	public override bool Equals(object o)
	{
		AdminCountry other = o as AdminCountry;
		if (other == null)
		{
			return false;
		}
		return CtCode.Equals(other.CtCode);
	}

	#endregion

}

[Table(Name = "admin.flight_day")]
public partial class AdminFlightDay : INotifyPropertyChanged
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

	#region  DayCode

	private Int16 dayCode;
	[Column(Storage = "dayCode", Name = "day_code", DbType = "INTEGER(2)", IsPrimaryKey = true, CanBeNull = false)]
	[DebuggerNonUserCode]
	public Int16 DayCode
	{
		get
		{
			return dayCode;
		}
		set
		{
			if (value != dayCode)
			{
				dayCode = value;
				OnPropertyChanged("DayCode");
			}
		}
	}

	#endregion

	#region string DayMask

	private string dayMask;
	[Column(Storage = "dayMask", Name = "day_mask", DbType = "NCHAR(7)", IsPrimaryKey = true, CanBeNull = false)]
	[DebuggerNonUserCode]
	public string DayMask
	{
		get
		{
			return dayMask;
		}
		set
		{
			if (value != dayMask)
			{
				dayMask = value;
				OnPropertyChanged("DayMask");
			}
		}
	}

	#endregion

	#region string DayName

	private string dayName;
	[Column(Storage = "dayName", Name = "day_name", DbType = "NCHAR(9)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public string DayName
	{
		get
		{
			return dayName;
		}
		set
		{
			if (value != dayName)
			{
				dayName = value;
				OnPropertyChanged("DayName");
			}
		}
	}

	#endregion

	#region GetHashCode(), Equals() - uses column DayCode, DayMask to look up objects in liveObjectMap

	public override int GetHashCode()
	{
		return DayCode.GetHashCode() ^ DayMask.GetHashCode();
	}

	public override bool Equals(object o)
	{
		AdminFlightDay other = o as AdminFlightDay;
		if (other == null)
		{
			return false;
		}
		return DayCode.Equals(other.DayCode) && DayMask.Equals(other.DayMask);
	}

	#endregion

}

[Table(Name = "admin.full_route")]
public partial class AdminFullRoute : INotifyPropertyChanged
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

	#region string AlCcode

	private string alCcode;
	[Column(Storage = "alCcode", Name = "al_ccode", DbType = "NCHAR(2)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public string AlCcode
	{
		get
		{
			return alCcode;
		}
		set
		{
			if (value != alCcode)
			{
				alCcode = value;
				OnPropertyChanged("AlCcode");
			}
		}
	}

	#endregion

	#region string AlIatacode

	private string alIatacode;
	[Column(Storage = "alIatacode", Name = "al_iatacode", DbType = "NCHAR(2)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public string AlIatacode
	{
		get
		{
			return alIatacode;
		}
		set
		{
			if (value != alIatacode)
			{
				alIatacode = value;
				OnPropertyChanged("AlIatacode");
			}
		}
	}

	#endregion

	#region string AlName

	private string alName;
	[Column(Storage = "alName", Name = "al_name", DbType = "NVARCHAR(60)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public string AlName
	{
		get
		{
			return alName;
		}
		set
		{
			if (value != alName)
			{
				alName = value;
				OnPropertyChanged("AlName");
			}
		}
	}

	#endregion

	#region string RtAirline

	private string rtAirline;
	[Column(Storage = "rtAirline", Name = "rt_airline", DbType = "NCHAR(3)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public string RtAirline
	{
		get
		{
			return rtAirline;
		}
		set
		{
			if (value != rtAirline)
			{
				rtAirline = value;
				OnPropertyChanged("RtAirline");
			}
		}
	}

	#endregion

	#region System.DateTime RtArriveAt

	private DateTime rtArriveAt;
	[Column(Storage = "rtArriveAt", Name = "rt_arrive_at", DbType = "INGRESDATE", CanBeNull = false)]
	[DebuggerNonUserCode]
	public DateTime RtArriveAt
	{
		get
		{
			return rtArriveAt;
		}
		set
		{
			if (value != rtArriveAt)
			{
				rtArriveAt = value;
				OnPropertyChanged("RtArriveAt");
			}
		}
	}

	#endregion

	#region  RtArriveOffset

	private Int16 rtArriveOffset;
	[Column(Storage = "rtArriveOffset", Name = "rt_arrive_offset", DbType = "INTEGER(1)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public Int16 RtArriveOffset
	{
		get
		{
			return rtArriveOffset;
		}
		set
		{
			if (value != rtArriveOffset)
			{
				rtArriveOffset = value;
				OnPropertyChanged("RtArriveOffset");
			}
		}
	}

	#endregion

	#region string RtArriveTo

	private string rtArriveTo;
	[Column(Storage = "rtArriveTo", Name = "rt_arrive_to", DbType = "NCHAR(3)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public string RtArriveTo
	{
		get
		{
			return rtArriveTo;
		}
		set
		{
			if (value != rtArriveTo)
			{
				rtArriveTo = value;
				OnPropertyChanged("RtArriveTo");
			}
		}
	}

	#endregion

	#region System.DateTime RtDepartAt

	private DateTime rtDepartAt;
	[Column(Storage = "rtDepartAt", Name = "rt_depart_at", DbType = "INGRESDATE", CanBeNull = false)]
	[DebuggerNonUserCode]
	public DateTime RtDepartAt
	{
		get
		{
			return rtDepartAt;
		}
		set
		{
			if (value != rtDepartAt)
			{
				rtDepartAt = value;
				OnPropertyChanged("RtDepartAt");
			}
		}
	}

	#endregion

	#region string RtDepartFrom

	private string rtDepartFrom;
	[Column(Storage = "rtDepartFrom", Name = "rt_depart_from", DbType = "NCHAR(3)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public string RtDepartFrom
	{
		get
		{
			return rtDepartFrom;
		}
		set
		{
			if (value != rtDepartFrom)
			{
				rtDepartFrom = value;
				OnPropertyChanged("RtDepartFrom");
			}
		}
	}

	#endregion

	#region string RtFlightDay

	private string rtFlightDay;
	[Column(Storage = "rtFlightDay", Name = "rt_flight_day", DbType = "NCHAR(7)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public string RtFlightDay
	{
		get
		{
			return rtFlightDay;
		}
		set
		{
			if (value != rtFlightDay)
			{
				rtFlightDay = value;
				OnPropertyChanged("RtFlightDay");
			}
		}
	}

	#endregion

	#region  RtFlightNum

	private Int32 rtFlightNum;
	[Column(Storage = "rtFlightNum", Name = "rt_flight_num", DbType = "INTEGER(4)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public Int32 RtFlightNum
	{
		get
		{
			return rtFlightNum;
		}
		set
		{
			if (value != rtFlightNum)
			{
				rtFlightNum = value;
				OnPropertyChanged("RtFlightNum");
			}
		}
	}

	#endregion

	#warning L189 table admin.full_route has no primary key. Multiple C# objects will refer to the same row.
}

[Table(Name = "admin.iietab_f5_f6")]
public partial class AdminIietabF5f6 : INotifyPropertyChanged
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

	#region string PerKey

	private string perKey;
	[Column(Storage = "perKey", Name = "per_key", DbType = "CHAR(8)", IsPrimaryKey = true, CanBeNull = false)]
	[DebuggerNonUserCode]
	public string PerKey
	{
		get
		{
			return perKey;
		}
		set
		{
			if (value != perKey)
			{
				perKey = value;
				OnPropertyChanged("PerKey");
			}
		}
	}

	#endregion

	#region  PerNext

	private Int32 perNext;
	[Column(Storage = "perNext", Name = "per_next", DbType = "INTEGER(4)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public Int32 PerNext
	{
		get
		{
			return perNext;
		}
		set
		{
			if (value != perNext)
			{
				perNext = value;
				OnPropertyChanged("PerNext");
			}
		}
	}

	#endregion

	#region  PerSegment0

	private Int32 perSegment0;
	[Column(Storage = "perSegment0", Name = "per_segment0", DbType = "INTEGER(4)", IsPrimaryKey = true, CanBeNull = false)]
	[DebuggerNonUserCode]
	public Int32 PerSegment0
	{
		get
		{
			return perSegment0;
		}
		set
		{
			if (value != perSegment0)
			{
				perSegment0 = value;
				OnPropertyChanged("PerSegment0");
			}
		}
	}

	#endregion

	#region  PerSegment1

	private Int32 perSegment1;
	[Column(Storage = "perSegment1", Name = "per_segment1", DbType = "INTEGER(4)", IsPrimaryKey = true, CanBeNull = false)]
	[DebuggerNonUserCode]
	public Int32 PerSegment1
	{
		get
		{
			return perSegment1;
		}
		set
		{
			if (value != perSegment1)
			{
				perSegment1 = value;
				OnPropertyChanged("PerSegment1");
			}
		}
	}

	#endregion

	#region System.Byte[] PerValue

	private byte[] perValue;
	[Column(Storage = "perValue", Name = "per_value", DbType = "BYTE VARYING", CanBeNull = false)]
	[DebuggerNonUserCode]
	public byte[] PerValue
	{
		get
		{
			return perValue;
		}
		set
		{
			if (value != perValue)
			{
				perValue = value;
				OnPropertyChanged("PerValue");
			}
		}
	}

	#endregion

	#region GetHashCode(), Equals() - uses column PerKey, PerSegment0, PerSegment1 to look up objects in liveObjectMap

	public override int GetHashCode()
	{
		return PerKey.GetHashCode() ^ PerSegment0.GetHashCode() ^ PerSegment1.GetHashCode();
	}

	public override bool Equals(object o)
	{
		AdminIietabF5f6 other = o as AdminIietabF5f6;
		if (other == null)
		{
			return false;
		}
		return PerKey.Equals(other.PerKey) && PerSegment0.Equals(other.PerSegment0) && PerSegment1.Equals(other.PerSegment1);
	}

	#endregion

}

[Table(Name = "admin.route")]
public partial class AdminRoute : INotifyPropertyChanged
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

	#region string RtAirline

	private string rtAirline;
	[Column(Storage = "rtAirline", Name = "rt_airline", DbType = "NCHAR(3)", IsPrimaryKey = true, CanBeNull = false)]
	[DebuggerNonUserCode]
	public string RtAirline
	{
		get
		{
			return rtAirline;
		}
		set
		{
			if (value != rtAirline)
			{
				rtAirline = value;
				OnPropertyChanged("RtAirline");
			}
		}
	}

	#endregion

	#region System.DateTime RtArriveAt

	private DateTime rtArriveAt;
	[Column(Storage = "rtArriveAt", Name = "rt_arrive_at", DbType = "INGRESDATE", CanBeNull = false)]
	[DebuggerNonUserCode]
	public DateTime RtArriveAt
	{
		get
		{
			return rtArriveAt;
		}
		set
		{
			if (value != rtArriveAt)
			{
				rtArriveAt = value;
				OnPropertyChanged("RtArriveAt");
			}
		}
	}

	#endregion

	#region  RtArriveOffset

	private Int16 rtArriveOffset;
	[Column(Storage = "rtArriveOffset", Name = "rt_arrive_offset", DbType = "INTEGER(1)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public Int16 RtArriveOffset
	{
		get
		{
			return rtArriveOffset;
		}
		set
		{
			if (value != rtArriveOffset)
			{
				rtArriveOffset = value;
				OnPropertyChanged("RtArriveOffset");
			}
		}
	}

	#endregion

	#region string RtArriveTo

	private string rtArriveTo;
	[Column(Storage = "rtArriveTo", Name = "rt_arrive_to", DbType = "NCHAR(3)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public string RtArriveTo
	{
		get
		{
			return rtArriveTo;
		}
		set
		{
			if (value != rtArriveTo)
			{
				rtArriveTo = value;
				OnPropertyChanged("RtArriveTo");
			}
		}
	}

	#endregion

	#region System.DateTime RtDepartAt

	private DateTime rtDepartAt;
	[Column(Storage = "rtDepartAt", Name = "rt_depart_at", DbType = "INGRESDATE", CanBeNull = false)]
	[DebuggerNonUserCode]
	public DateTime RtDepartAt
	{
		get
		{
			return rtDepartAt;
		}
		set
		{
			if (value != rtDepartAt)
			{
				rtDepartAt = value;
				OnPropertyChanged("RtDepartAt");
			}
		}
	}

	#endregion

	#region string RtDepartFrom

	private string rtDepartFrom;
	[Column(Storage = "rtDepartFrom", Name = "rt_depart_from", DbType = "NCHAR(3)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public string RtDepartFrom
	{
		get
		{
			return rtDepartFrom;
		}
		set
		{
			if (value != rtDepartFrom)
			{
				rtDepartFrom = value;
				OnPropertyChanged("RtDepartFrom");
			}
		}
	}

	#endregion

	#region string RtFlightDay

	private string rtFlightDay;
	[Column(Storage = "rtFlightDay", Name = "rt_flight_day", DbType = "NCHAR(7)", IsPrimaryKey = true, CanBeNull = false)]
	[DebuggerNonUserCode]
	public string RtFlightDay
	{
		get
		{
			return rtFlightDay;
		}
		set
		{
			if (value != rtFlightDay)
			{
				rtFlightDay = value;
				OnPropertyChanged("RtFlightDay");
			}
		}
	}

	#endregion

	#region  RtFlightNum

	private Int32 rtFlightNum;
	[Column(Storage = "rtFlightNum", Name = "rt_flight_num", DbType = "INTEGER(4)", IsPrimaryKey = true, CanBeNull = false)]
	[DebuggerNonUserCode]
	public Int32 RtFlightNum
	{
		get
		{
			return rtFlightNum;
		}
		set
		{
			if (value != rtFlightNum)
			{
				rtFlightNum = value;
				OnPropertyChanged("RtFlightNum");
			}
		}
	}

	#endregion

	#region  RtId

	private Int32 rtId;
	[Column(Storage = "rtId", Name = "rt_id", DbType = "INTEGER(4)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public Int32 RtId
	{
		get
		{
			return rtId;
		}
		set
		{
			if (value != rtId)
			{
				rtId = value;
				OnPropertyChanged("RtId");
			}
		}
	}

	#endregion

	#region GetHashCode(), Equals() - uses column RtAirline, RtFlightDay, RtFlightNum to look up objects in liveObjectMap

	public override int GetHashCode()
	{
		return RtAirline.GetHashCode() ^ RtFlightDay.GetHashCode() ^ RtFlightNum.GetHashCode();
	}

	public override bool Equals(object o)
	{
		AdminRoute other = o as AdminRoute;
		if (other == null)
		{
			return false;
		}
		return RtAirline.Equals(other.RtAirline) && RtFlightDay.Equals(other.RtFlightDay) && RtFlightNum.Equals(other.RtFlightNum);
	}

	#endregion

}

[Table(Name = "admin.tz")]
public partial class AdminTz : INotifyPropertyChanged
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

	#region string TzCode

	private string tzCode;
	[Column(Storage = "tzCode", Name = "tz_code", DbType = "NCHAR(5)", IsPrimaryKey = true)]
	[DebuggerNonUserCode]
	public string TzCode
	{
		get
		{
			return tzCode;
		}
		set
		{
			if (value != tzCode)
			{
				tzCode = value;
				OnPropertyChanged("TzCode");
			}
		}
	}

	#endregion

	#region  TzId

	private Int32 tzId;
	[Column(Storage = "tzId", Name = "tz_id", DbType = "INTEGER(4)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public Int32 TzId
	{
		get
		{
			return tzId;
		}
		set
		{
			if (value != tzId)
			{
				tzId = value;
				OnPropertyChanged("TzId");
			}
		}
	}

	#endregion

	#region string TzName

	private string tzName;
	[Column(Storage = "tzName", Name = "tz_name", DbType = "NCHAR(40)")]
	[DebuggerNonUserCode]
	public string TzName
	{
		get
		{
			return tzName;
		}
		set
		{
			if (value != tzName)
			{
				tzName = value;
				OnPropertyChanged("TzName");
			}
		}
	}

	#endregion

	#region  TzUtcOffset

	private Decimal tzUtcOffset;
	[Column(Storage = "tzUtcOffset", Name = "tz_utc_offset", DbType = "DECIMAL(5, 2)")]
	[DebuggerNonUserCode]
	public Decimal TzUtcOffset
	{
		get
		{
			return tzUtcOffset;
		}
		set
		{
			if (value != tzUtcOffset)
			{
				tzUtcOffset = value;
				OnPropertyChanged("TzUtcOffset");
			}
		}
	}

	#endregion

	#region GetHashCode(), Equals() - uses column TzCode to look up objects in liveObjectMap

	public override int GetHashCode()
	{
		return TzCode.GetHashCode();
	}

	public override bool Equals(object o)
	{
		AdminTz other = o as AdminTz;
		if (other == null)
		{
			return false;
		}
		return TzCode.Equals(other.TzCode);
	}

	#endregion

}

[Table(Name = "admin.user_profile")]
public partial class AdminUserProfile : INotifyPropertyChanged
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

	#region string UpAirport

	private string upAirport;
	[Column(Storage = "upAirport", Name = "up_airport", DbType = "NCHAR(3)")]
	[DebuggerNonUserCode]
	public string UpAirport
	{
		get
		{
			return upAirport;
		}
		set
		{
			if (value != upAirport)
			{
				upAirport = value;
				OnPropertyChanged("UpAirport");
			}
		}
	}

	#endregion

	#region string UpEmail

	private string upEmail;
	[Column(Storage = "upEmail", Name = "up_email", DbType = "NVARCHAR(100)", IsPrimaryKey = true, CanBeNull = false)]
	[DebuggerNonUserCode]
	public string UpEmail
	{
		get
		{
			return upEmail;
		}
		set
		{
			if (value != upEmail)
			{
				upEmail = value;
				OnPropertyChanged("UpEmail");
			}
		}
	}

	#endregion

	#region string UpFirst

	private string upFirst;
	[Column(Storage = "upFirst", Name = "up_first", DbType = "NVARCHAR(30)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public string UpFirst
	{
		get
		{
			return upFirst;
		}
		set
		{
			if (value != upFirst)
			{
				upFirst = value;
				OnPropertyChanged("UpFirst");
			}
		}
	}

	#endregion

	#region  UpId

	private Int32 upId;
	[Column(Storage = "upId", Name = "up_id", DbType = "INTEGER(4)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public Int32 UpId
	{
		get
		{
			return upId;
		}
		set
		{
			if (value != upId)
			{
				upId = value;
				OnPropertyChanged("UpId");
			}
		}
	}

	#endregion

	#region System.Byte[] UpImage

	private byte[] upImage;
	[Column(Storage = "upImage", Name = "up_image", DbType = "LONG BYTE")]
	[DebuggerNonUserCode]
	public byte[] UpImage
	{
		get
		{
			return upImage;
		}
		set
		{
			if (value != upImage)
			{
				upImage = value;
				OnPropertyChanged("UpImage");
			}
		}
	}

	#endregion

	#region string UpLast

	private string upLast;
	[Column(Storage = "upLast", Name = "up_last", DbType = "NVARCHAR(30)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public string UpLast
	{
		get
		{
			return upLast;
		}
		set
		{
			if (value != upLast)
			{
				upLast = value;
				OnPropertyChanged("UpLast");
			}
		}
	}

	#endregion

	#region GetHashCode(), Equals() - uses column UpEmail to look up objects in liveObjectMap

	public override int GetHashCode()
	{
		return UpEmail.GetHashCode();
	}

	public override bool Equals(object o)
	{
		AdminUserProfile other = o as AdminUserProfile;
		if (other == null)
		{
			return false;
		}
		return UpEmail.Equals(other.UpEmail);
	}

	#endregion

}

[Table(Name = "admin.version")]
public partial class AdminVersion : INotifyPropertyChanged
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

	#region System.DateTime VerDate

	private DateTime verDate;
	[Column(Storage = "verDate", Name = "ver_date", DbType = "INGRESDATE", CanBeNull = false)]
	[DebuggerNonUserCode]
	public DateTime VerDate
	{
		get
		{
			return verDate;
		}
		set
		{
			if (value != verDate)
			{
				verDate = value;
				OnPropertyChanged("VerDate");
			}
		}
	}

	#endregion

	#region  VerId

	private Int32 verId;
	[Column(Storage = "verId", Name = "ver_id", DbType = "INTEGER(4)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public Int32 VerId
	{
		get
		{
			return verId;
		}
		set
		{
			if (value != verId)
			{
				verId = value;
				OnPropertyChanged("VerId");
			}
		}
	}

	#endregion

	#region System.DateTime VerInstall

	private DateTime verInstall;
	[Column(Storage = "verInstall", Name = "ver_install", DbType = "INGRESDATE", CanBeNull = false)]
	[DebuggerNonUserCode]
	public DateTime VerInstall
	{
		get
		{
			return verInstall;
		}
		set
		{
			if (value != verInstall)
			{
				verInstall = value;
				OnPropertyChanged("VerInstall");
			}
		}
	}

	#endregion

	#region  VerMajor

	private Int32 verMajor;
	[Column(Storage = "verMajor", Name = "ver_major", DbType = "INTEGER(4)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public Int32 VerMajor
	{
		get
		{
			return verMajor;
		}
		set
		{
			if (value != verMajor)
			{
				verMajor = value;
				OnPropertyChanged("VerMajor");
			}
		}
	}

	#endregion

	#region  VerMinor

	private Int32 verMinor;
	[Column(Storage = "verMinor", Name = "ver_minor", DbType = "INTEGER(4)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public Int32 VerMinor
	{
		get
		{
			return verMinor;
		}
		set
		{
			if (value != verMinor)
			{
				verMinor = value;
				OnPropertyChanged("VerMinor");
			}
		}
	}

	#endregion

	#region  VerRelease

	private Int32 verRelease;
	[Column(Storage = "verRelease", Name = "ver_release", DbType = "INTEGER(4)", CanBeNull = false)]
	[DebuggerNonUserCode]
	public Int32 VerRelease
	{
		get
		{
			return verRelease;
		}
		set
		{
			if (value != verRelease)
			{
				verRelease = value;
				OnPropertyChanged("VerRelease");
			}
		}
	}

	#endregion

	#warning L189 table admin.version has no primary key. Multiple C# objects will refer to the same row.
}
