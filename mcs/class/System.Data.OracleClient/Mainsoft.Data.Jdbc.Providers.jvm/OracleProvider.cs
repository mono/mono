//
// Mainsoft.Data.Jdbc.Providers.OracleProvider
//
// Authors:
//	Konstantin Triger <kostat@mainsoft.com>
//	Boris Kirzner <borisk@mainsoft.com>
//	
// (C) 2006 Mainsoft Corporation (http://www.mainsoft.com)
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

using System;
using System.Collections;
using Mainsoft.Data.Configuration;
using System.Reflection;

namespace Mainsoft.Data.Jdbc.Providers
{
	public class OracleProvider : GenericProvider
	{
		#region Consts

		private const string Port = "Port";

		#endregion //Consts

		#region Fields

		#endregion // Fields

		#region Constructors

		public OracleProvider (IDictionary providerInfo) : base (providerInfo)
		{
		}

		#endregion // Constructors

		#region Properties

		#endregion // Properties

		#region Methods

		public override IConnectionStringDictionary GetConnectionStringBuilder (string connectionString)
		{
			IConnectionStringDictionary conectionStringBuilder = base.GetConnectionStringBuilder (connectionString);

			string port = (string) conectionStringBuilder [Port];
			if (port == null || port.Length == 0) {
				port = (string) ProviderInfo [Port];
				conectionStringBuilder.Add (Port, port);
			}
			
			return conectionStringBuilder;
		}

		public override java.sql.Connection GetConnection(IConnectionStringDictionary conectionStringBuilder) {
			return new OracleConnection(base.GetConnection (conectionStringBuilder));
		}


		#endregion //Methods

		#region OracleConnection

		sealed class OracleConnection : Connection {

			public OracleConnection(java.sql.Connection connection)
				: base(connection) {}

			public override java.sql.CallableStatement prepareCall(string arg_0) {
				return base.prepareCall (arg_0);
			}

			public override java.sql.CallableStatement prepareCall(string arg_0, int arg_1, int arg_2) {
				return new OracleCallableStatement(base.prepareCall (arg_0, arg_1, arg_2));
			}

			public override java.sql.CallableStatement prepareCall(string arg_0, int arg_1, int arg_2, int arg_3) {
				return new OracleCallableStatement(base.prepareCall (arg_0, arg_1, arg_2, arg_3));
			}

			public override java.sql.PreparedStatement prepareStatement(string arg_0) {
				return new OraclePreparedStatement(base.prepareStatement (arg_0));
			}

			public override java.sql.PreparedStatement prepareStatement(string arg_0, int arg_1) {
				return new OraclePreparedStatement(base.prepareStatement (arg_0, arg_1));
			}

			public override java.sql.PreparedStatement prepareStatement(string arg_0, int arg_1, int arg_2) {
				return new OraclePreparedStatement(base.prepareStatement (arg_0, arg_1, arg_2));
			}

			public override java.sql.PreparedStatement prepareStatement(string arg_0, int arg_1, int arg_2, int arg_3) {
				return new OraclePreparedStatement(base.prepareStatement (arg_0, arg_1, arg_2, arg_3));
			}

			public override java.sql.PreparedStatement prepareStatement(string arg_0, int[] arg_1) {
				return new OraclePreparedStatement(base.prepareStatement (arg_0, arg_1));
			}

			public override java.sql.PreparedStatement prepareStatement(string arg_0, string[] arg_1) {
				return new OraclePreparedStatement(base.prepareStatement (arg_0, arg_1));
			}
		}

		#endregion

		sealed class OraclePreparedStatement : PreparedStatement, IPreparedStatement {
			readonly MethodInfo _info;

			public OraclePreparedStatement(java.sql.PreparedStatement statement)
				: base(statement) {
				_info = Wrapped.GetType().GetMethod("setFixedCHAR");
			}

			#region IPreparedStatement Members

			public void setBit(int parameterIndex, int value) {
				base.setInt(parameterIndex, value);
			}

			public void setChar(int parameterIndex, string value) {
				if (_info == null) {
					base.setString(parameterIndex, value);
					return;
				}

				_info.Invoke(Wrapped, new object[] {
							new java.lang.Integer(parameterIndex),
							value});
			}

			public void setNumeric(int parameterIndex, java.math.BigDecimal value) {
				base.setBigDecimal(parameterIndex, value);
			}

			public void setReal(int parameterIndex, double value) {
				base.setDouble(parameterIndex, value);
			}

			#endregion
		}

		sealed class OracleCallableStatement : CallableStatement, IPreparedStatement {
			readonly MethodInfo _info;

			public OracleCallableStatement(java.sql.CallableStatement statement)
				: base(statement) {
				_info = Wrapped.GetType().GetMethod("setFixedCHAR");
			}

			#region IPreparedStatement Members

			public void setBit(int parameterIndex, int value) {
				base.setInt(parameterIndex, value);
			}

			public void setChar(int parameterIndex, string value) {
				if (_info == null) {
					base.setString(parameterIndex, value);
					return;
				}

				_info.Invoke(Wrapped, new object[] {
								new java.lang.Integer(parameterIndex),
								value});
			}

			public void setNumeric(int parameterIndex, java.math.BigDecimal value) {
				base.setBigDecimal(parameterIndex, value);
			}

			public void setReal(int parameterIndex, double value) {
				base.setDouble(parameterIndex, value);
			}

			#endregion

		}

	}
}
