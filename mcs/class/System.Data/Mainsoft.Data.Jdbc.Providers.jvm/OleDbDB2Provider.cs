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

namespace Mainsoft.Data.Jdbc.Providers {
	public class OleDbDB2Provider : GenericProvider {
		#region Consts

		private const string Port = "port";
		private const string Hostname = "hostname";
		private const string Location = "location";

		#endregion //Consts

		#region Fields

		#endregion // Fields

		#region Constructors

		public OleDbDB2Provider (IDictionary providerInfo) : base (providerInfo) {
		}

		#endregion // Constructors

		#region Properties

		#endregion // Properties

		#region Methods

		public override IConnectionStringDictionary GetConnectionStringBuilder (string connectionString) {
			IConnectionStringDictionary connectionStringBuilder = base.GetConnectionStringBuilder (connectionString);

			string hostname = (string)connectionStringBuilder[Hostname];
			string port = null;
			if (hostname == null || hostname.Length == 0) {
				string location = (string)connectionStringBuilder[Location];
				if (location != null) {
					int semicolumnIndex = location.IndexOf(':');
					if (semicolumnIndex != -1) {
						hostname = location.Substring(0,semicolumnIndex);
						port = location.Substring(semicolumnIndex + 1);
					}
					else
						hostname = location;

					if (hostname != null)
						connectionStringBuilder.Add(Hostname, hostname);

					if (port != null)
						connectionStringBuilder[Port] = port;
				}
			}

			port = (string) connectionStringBuilder [Port];
			if (port == null || port.Length == 0) {
				port = (string) ProviderInfo [Port];
				connectionStringBuilder.Add (Port, port);
			}
			
			return connectionStringBuilder;
		}

		#endregion //Methods
	}
}
