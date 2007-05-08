//
// System.Data.Common.OleDbParameterCollection
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

namespace System.Data.OleDb
{
    public sealed class OleDbParameterCollection :  AbstractDbParameterCollection
    {
		#region Constructors

        public OleDbParameterCollection(OleDbCommand parent): base(parent)
        {
        }

		#endregion // Constructors

		#region Properties
        
        public OleDbParameter this[int index]
        {
            get { return (OleDbParameter)base[index]; }
            set { 
				OnSchemaChanging();
				base[index] = value; 
			}
        }

        public OleDbParameter this[string parameterName]
        {
            get { return (OleDbParameter)base[parameterName]; }
            set { 
				OnSchemaChanging();
				base[parameterName] = value; 
			}
        }

		protected override Type ItemType { 
			get { return typeof(OleDbParameter); }
		}

		#endregion // Properties

		#region Methods

        public OleDbParameter Add(OleDbParameter value)
        {
            base.Add(value);
            return value;
        }

        public OleDbParameter Add(string parameterName, object value)
        {
            OleDbParameter param = new OleDbParameter(parameterName, value);
            return Add(param);
        }

        public OleDbParameter Add(string parameterName, OleDbType sqlDbType)
        {
            OleDbParameter param = new OleDbParameter(parameterName, sqlDbType);
            return Add(param);
        }

        public OleDbParameter Add(string parameterName, OleDbType sqlDbType, int size)
        {
            OleDbParameter param = new OleDbParameter(parameterName, sqlDbType, size);
            return Add(param);
        }

        public OleDbParameter Add(string parameterName, OleDbType sqlDbType, int size, string sourceColumn)
        {
            OleDbParameter param = new OleDbParameter(parameterName, sqlDbType, size, sourceColumn);
            return Add(param);
		}

#if NET_2_0
		public void AddRange (OleDbParameter [] values)
		{
			base.AddRange (values);
		}

		public OleDbParameter AddWithValue (string parameterName, object value)
		{
			return Add (parameterName, value);
		}

		public bool Contains (OleDbParameter value)
		{
			return base.Contains (value);
		}

		public void CopyTo (OleDbParameter [] array, int index)
		{
			base.CopyTo (array, index);
		}

		public void Insert (int index, OleDbParameter value)
		{
			base.Insert (index, value);
		}

		public void Remove (OleDbParameter value)
		{
			base.Remove (value);
		}

		public int IndexOf (OleDbParameter value)
		{
			return base.IndexOf (value);
		}

#endif
		#endregion // Methods        
        
    }
}