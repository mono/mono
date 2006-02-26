//
// System.Data.OracleClient.OracleDataReader
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

using System.Data.Common;
using System.Data.ProviderBase;

using java.sql;

namespace System.Data.OracleClient {
	public sealed class OracleDataReader : AbstractDataReader {
		#region Fields

		#endregion // Fields

		#region Constructors

		internal OracleDataReader(OracleCommand command) : base(command) {
		}

		#endregion // Constructors

		#region Methods

		protected sealed override SystemException CreateException(string message, SQLException e) {
			return new OracleException(message,e, (OracleConnection)_command.Connection);		
		}

		protected sealed override SystemException CreateException(java.io.IOException e) {
			return new OracleException(e, (OracleConnection)_command.Connection);		
		}

		public override String GetDataTypeName(int columnIndex) {
			try {
				string jdbcTypeName = Results.getMetaData().getColumnTypeName(columnIndex + 1);
				
				return OracleConvert.JdbcTypeNameToDbTypeName(jdbcTypeName);
			}
			catch (SQLException e) {
				throw CreateException(e);
			}
		}

		protected override int GetProviderType(int jdbcType) {
			return (int)OracleConvert.JdbcTypeToOracleType(jdbcType);   
		}

		public override decimal GetDecimal(int i) {
			if (IsNumeric(i))
				return GetDecimalSafe(i);

			return base.GetDecimal(i);
		}

		public override double GetDouble(int i) {
			if (IsNumeric(i))
				return GetDoubleSafe(i);

			return base.GetDouble(i);
		}

		public override float GetFloat(int i) {
			if (IsNumeric(i))
				return GetFloatSafe(i);

			return base.GetFloat(i);
		}
//
//		OracleClient does not "excuse" for Int16
//
//		public override short GetInt16(int i) {
//			if (IsNumeric(i))
//				return GetInt16Safe(i);
//
//			return base.GetInt16(i);
//		}


		public override int GetInt32(int i) {
			if (IsNumeric(i))
				return GetInt32Safe(i);

			return base.GetInt32(i);
		}

		public override long GetInt64(int i) {
			if (IsNumeric(i))
				return GetInt64Safe(i);

			return base.GetInt64(i);
		}



		#endregion // Methods
	}
}