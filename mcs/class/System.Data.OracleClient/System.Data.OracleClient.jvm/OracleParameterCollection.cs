//
// System.Data.Common.OracleParameterCollection
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

namespace System.Data.OracleClient {
	public sealed class OracleParameterCollection :  AbstractDbParameterCollection {
		#region Constructors

		public OracleParameterCollection(OracleCommand parent): base(parent) {
		}

		#endregion // Constructors

		#region Properties
        
		public new OracleParameter this[int index] {
			get { return (OracleParameter)base[index]; }
			set { 
				OnSchemaChanging();
				base[index] = value; 
			}
		}

		public new OracleParameter this[string parameterName] {
			get { return (OracleParameter)base[parameterName]; }
			set { 
				OnSchemaChanging();
				base[parameterName] = value; 
			}
		}

		protected override Type ItemType { 
			get { return typeof(OracleParameter); }
		}

		#endregion // Properties

		#region Methods

		public OracleParameter Add(OracleParameter value) {
			base.Add(value);
			return value;
		}

		public OracleParameter Add(string parameterName, object value) {
			OracleParameter param = new OracleParameter(parameterName, value);
			return Add(param);
		}

		public OracleParameter Add(string parameterName, OracleType sqlDbType) {
			OracleParameter param = new OracleParameter(parameterName, sqlDbType);
			return Add(param);
		}

		public OracleParameter Add(string parameterName, OracleType sqlDbType, int size) {
			OracleParameter param = new OracleParameter(parameterName, sqlDbType, size);
			return Add(param);
		}

		public OracleParameter Add(string parameterName, OracleType sqlDbType, int size, string sourceColumn) {
			OracleParameter param = new OracleParameter(parameterName, sqlDbType, size, sourceColumn);
			return Add(param);
		}

#if NET_2_0
		public OracleParameter AddWithValue (string parameterName, object value) {
			return Add (parameterName, value);
		}

		public bool Contains (OracleParameter value) {
			return base.Contains (value);
		}

		public void CopyTo (OracleParameter [] array, int index) {
			base.CopyTo (array, index);
		}

		public void Insert (int index, OracleParameter value) {
			base.Insert (index, value);
		}

		public void Remove (OracleParameter value) {
			base.Remove (value);
		}
#endif

		#endregion // Methods

	}
}
