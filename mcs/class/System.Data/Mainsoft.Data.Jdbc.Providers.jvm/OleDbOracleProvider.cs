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

namespace Mainsoft.Data.Jdbc.Providers
{
	public class OleDbOracleProvider : GenericProvider
	{
		#region Consts

		private const string Port = "Port";

		#endregion //Consts

		#region Fields

		#endregion // Fields

		#region Constructors

		public OleDbOracleProvider (IDictionary providerInfo) : base (providerInfo)
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

		#endregion //Methods
	}
}
