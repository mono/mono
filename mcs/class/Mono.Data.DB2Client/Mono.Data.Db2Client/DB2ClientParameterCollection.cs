#region Licence
	/// DB2DriverCS - A DB2 driver for .Net
	/// Copyright 2003 By Christopher Bockner
	/// Released under the terms of the MIT/X11 Licence
	/// Please refer to the Licence.txt file that should be distributed with this package
	/// This software requires that DB2 client software be installed correctly on the machine
	/// (or instance) on which the driver is running.  
#endregion
using System;
using System.Data;
using System.Collections;
using System.Globalization;


namespace DB2ClientCS
{
	/// <summary>
	/// Summary description for DB2ClientParameterCollection.
	/// </summary>
	public class DB2ClientParameterCollection : ArrayList, IDataParameterCollection
	{
		public object this[string index]
		{
			get 
			{
				return this[IndexOf(index)];
			}
			set
			{
				this[IndexOf(index)] = value;
			}
		}
		public bool Contains(string paramName)
		{
			return(-1 != IndexOf(paramName));
		}

		public int IndexOf(string paramName)
		{
			int index = 0;
			foreach(DB2ClientParameter item in this) 
			{
				if (0 == _cultureAwareCompare(item.ParameterName, paramName))
				{
					return index;
				}
				index++;
			}
			return -1;
		}

		public void RemoveAt(string paramName)
		{
			RemoveAt(IndexOf(paramName));
		}

		public override int Add(object value)
		{
			return Add((DB2ClientParameter)value);
		}

		public int Add(DB2ClientParameter value)
		{
			if (((DB2ClientParameter)value).ParameterName != null)
			{
				return base.Add(value);
			}
			else
				throw new ArgumentException("parameter must be named");
		}

		public int Add(string paramName, DbType type)
		{
			return Add(new DB2ClientParameter(paramName, type));
		}

		public int Add(string paramName, object value)
		{
			return Add(new DB2ClientParameter(paramName, value));
		}

		public int Add(string paramName, DbType dbType, string sourceColumn)
		{
			return Add(new DB2ClientParameter(paramName, dbType, sourceColumn));
		}

		private int _cultureAwareCompare(string strA, string strB)
		{
			return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, strB, CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth | CompareOptions.IgnoreCase);
		}
	}
}

