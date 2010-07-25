
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Data;
using System.Collections;
using System.Globalization;


namespace IBM.Data.DB2
{

	public class DB2ParameterCollection : ArrayList, IDataParameterCollection
	{
		IntPtr hwndStmt = IntPtr.Zero;
		
		internal IntPtr HwndStmt
		{
			set
			{
				hwndStmt = value;
			}
		}
		public new DB2Parameter this[int index]
		{
			get 
			{
				return (DB2Parameter)base[index];
			}
			set
			{
				base[index] = value;
			}
		}
		public DB2Parameter this[string index]
		{
			get 
			{
				return (DB2Parameter)base[IndexOf(index)];
			}
			set
			{
				base[IndexOf(index)] = value;
			}
		}
		object IDataParameterCollection.this[string index]
		{
			get 
			{
				return this[IndexOf(index)];
			}
			set
			{
				this[IndexOf(index)] = (DB2Parameter)value;
			}
		}
		public bool Contains(string paramName)
		{
			return(-1 != IndexOf(paramName));
		}

		public int IndexOf(string paramName)
		{
			int index = 0;
			for(index = 0; index < Count; index++) 
			{
				if (0 == _cultureAwareCompare(((DB2Parameter)this[index]).ParameterName, paramName))
				{
					return index;
				}
			}
			return -1;
		}

		public void RemoveAt(string paramName)
		{
			RemoveAt(IndexOf(paramName));
		}

		public override int Add(object obj)
		{
			DB2Parameter value = (DB2Parameter)obj;
			if(value.ParameterName == null)
				throw new ArgumentException("parameter must be named");
			if(IndexOf(value.ParameterName) >= 0)
				throw new ArgumentException("parameter name is already in collection");
			return base.Add(value);
		}

		public DB2Parameter Add(DB2Parameter value)
		{
			if(value.ParameterName == null)
				throw new ArgumentException("parameter must be named");
			if(IndexOf(value.ParameterName) >= 0)
				throw new ArgumentException("parameter name is already in collection");
			base.Add(value);
			return value;
		}

		public DB2Parameter Add(string paramName, DB2Type type)
		{
			return Add(new DB2Parameter(paramName, type));
		}

		public DB2Parameter Add(string paramName, object value)
		{
			return Add(new DB2Parameter(paramName, value));
		}

		public DB2Parameter Add(string paramName, DB2Type dbType, int size)
		{
			return Add(new DB2Parameter(paramName, dbType, size));
		}

		public DB2Parameter Add(string paramName, DB2Type dbType, int size, string sourceColumn)
		{
			return Add(new DB2Parameter(paramName, dbType, size, sourceColumn));
		}

		private int _cultureAwareCompare(string strA, string strB)
		{
			return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, strB, CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth | CompareOptions.IgnoreCase);
		}
		
		internal void GetOutValues()
		{
			foreach(DB2Parameter param in this)
			{
				if(ParameterDirection.Output == param.Direction || ParameterDirection.InputOutput == param.Direction)
				{
					param.GetOutValue();
					//Console.WriteLine(param.ParameterName);
				}
			}
		}
	}
}

