//
// System.Data.OleDb.OleDbConnection
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

using java.sql;

namespace Mainsoft.Data.Jdbc.Providers
{
	sealed class ConfigurationConsts {
		#region Constructors

		ConfigurationConsts() {			
		}

		#endregion // Constructors

		#region Consts

		public const string Name = "id";
		public const string KeyMapping = "keyMapping";
		public const string KeyMappingExcludes = "keyExclude";
		public const string KeyMappingUnsupported = "keyUnsupported";
		public const string JdbcUrlPattern = "url";
		public const string JdbcDriverClassName = "driverClassName";
		public const string ProviderType = "type";

		public static readonly char [] SemicolonArr = new char [] { ';' };
		public static readonly char [] CommaArr = new char [] { ',' };

		#endregion // Consts

	}

	public interface IConnectionProvider
	{
		java.sql.Connection GetConnection (IConnectionStringDictionary connectionStringBuilder);
		IConnectionStringDictionary GetConnectionStringBuilder (string connectionString);
	}

	public interface IPreparedStatement : java.sql.PreparedStatement {
		void setBit(int parameterIndex, int value);
		void setChar(int parameterIndex, string value);
		void setNumeric(int parameterIndex, java.math.BigDecimal value);
		void setReal(int parameterIndex, double value);
	}
}
