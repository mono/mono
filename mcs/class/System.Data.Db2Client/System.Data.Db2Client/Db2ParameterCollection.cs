using System;
using System.Data;
using System.Collections;
using System.Globalization;


namespace System.Data.Db2Client
{

	public class Db2ParameterCollection : ArrayList, IDataParameterCollection
	{
		IntPtr hwndStmt = IntPtr.Zero;
		
		internal IntPtr HwndStmt{
			set{
				hwndStmt = value;
			}
		}
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
			foreach(Db2Parameter item in this) 
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
			return Add((Db2Parameter)value);
		}

		public int Add(Db2Parameter value)
		{
			if (((Db2Parameter)value).ParameterName != null)
			{
				
				int result = base.Add(value);
				//value.Bind(hwndStmt, (short)this.Count);
				//Console.WriteLine("{0}", this.Count.ToString());
				return result;
			}
			else
				throw new ArgumentException("parameter must be named");
		}

		public int Add(string paramName, DbType type)
		{
			return Add(new Db2Parameter(paramName, type));
		}

		public int Add(string paramName, object value)
		{
			return Add(new Db2Parameter(paramName, value));
		}

		public int Add(string paramName, DbType dbType, string sourceColumn)
		{
			return Add(new Db2Parameter(paramName, dbType, sourceColumn));
		}

		private int _cultureAwareCompare(string strA, string strB)
		{
			return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, strB, CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth | CompareOptions.IgnoreCase);
		}
		
		internal void GetOutValues(){
			foreach(Db2Parameter param in this){
				if(ParameterDirection.Output == param.Direction){
					param.GetOutValue();
					//Console.WriteLine(param.ParameterName);
				}
			}
		}
	}
}

