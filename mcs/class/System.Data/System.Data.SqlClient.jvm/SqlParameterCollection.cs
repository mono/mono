//
// System.Data.Common.SqlParameterCollection
//
// Authors:
//	Konstantin Triger <kostat@mainsoft.com>
//	Boris Kirzner <borisk@mainsoft.com>
//	
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
//

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
using System.Data.ProviderBase;

namespace System.Data.SqlClient
{
    public class SqlParameterCollection : AbstractDbParameterCollection
    {
		#region Constructors

        public SqlParameterCollection(SqlCommand parent): base(parent)
        {
        }

		#endregion // Constructors

		#region Properties

        public SqlParameter this[string parameterName]
        {
            get { return (SqlParameter)base[parameterName]; }
            set { 
				OnSchemaChanging();
				base[parameterName] = value; 
			}
        }

        public SqlParameter this[int index]
        {
            get { return (SqlParameter)base[index]; }
            set { 
				base.OnSchemaChanging();
				base[index] = value; 
			}
        }

		protected override Type ItemType { 
			get { return typeof(SqlParameter); }
		}

		#endregion // Properties

		#region Methods       

        public SqlParameter Add(SqlParameter value)
        {
            base.Add(value);
            return value;
        }

        public SqlParameter Add(string parameterName, object value)
        {
            SqlParameter param = new SqlParameter(parameterName,value);
            return Add(param);
        }

        public SqlParameter Add(string parameterName, SqlDbType sqlDbType)
        {
            SqlParameter param = new SqlParameter(parameterName,sqlDbType);
            return Add(param);
        }

        public SqlParameter Add(string parameterName, SqlDbType sqlDbType, int size)
        {
            SqlParameter param = new SqlParameter(parameterName,sqlDbType,size);
            return Add(param);
        }

        public SqlParameter Add(string parameterName, SqlDbType sqlDbType, int size, string sourceColumn)
        {
            SqlParameter param = new SqlParameter(parameterName,sqlDbType,size,sourceColumn);
            return Add(param);
        }

#if NET_2_0
		public void AddRange (SqlParameter [] values)
		{
			base.AddRange (values);
		}

		public SqlParameter AddWithValue (string parameterName, object value)
		{
			return Add (parameterName, value);
		}

		public bool Contains (SqlParameter value)
		{
			return base.Contains (value);
		}

		public int IndexOf (SqlParameter value)
		{
			return base.IndexOf (value);
		}

		public void CopyTo (SqlParameter [] array, int index)
		{
			base.CopyTo (array, index);
		}

		public void Insert (int index, SqlParameter value)
		{
			base.Insert (index, value);
		}

		public void Remove (SqlParameter value)
		{
			base.Remove (value);
		}
#endif
		#endregion // Methods

    }
}