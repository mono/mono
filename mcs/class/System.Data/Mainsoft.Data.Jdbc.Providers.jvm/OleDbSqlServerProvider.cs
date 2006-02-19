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
using System.Data.Common;
using System.Data.Configuration;
using Mainsoft.Data.Configuration;

using java.net;

namespace Mainsoft.Data.Jdbc.Providers
{
	public class OleDbSqlServerProvider : GenericProvider
	{
		#region Consts

		private const string Port = "Port";
		private const string ServerName = "ServerName";
		private const string DefaultInstanceName = "MSSQLSERVER";
		private const string Timeout = "Timeout";
		private const int DefaultTimeout = 15;

		#endregion //Consts

		#region Fields

		#endregion // Fields

		#region Constructors

		public OleDbSqlServerProvider (IDictionary providerInfo) : base (providerInfo)
		{
		}

		#endregion // Constructors

		#region Properties

		#endregion // Properties

		#region Methods

		public override IConnectionStringDictionary GetConnectionStringBuilder (string connectionString)
		{
			//TBD: should wrap the IConnectionStringDictionary
			IConnectionStringDictionary conectionStringBuilder = base.GetConnectionStringBuilder (connectionString);
			if (!conectionStringBuilder.Contains("jndi-datasource-name")) {

				string port = (string) conectionStringBuilder [Port];
				if (port == null || port.Length == 0) {
					port = GetMSSqlPort (GetInstanceName (conectionStringBuilder), GetDataSource (conectionStringBuilder), GetTimeout (conectionStringBuilder));
					conectionStringBuilder.Add (Port, port);
				}
			}

			return conectionStringBuilder;
		}

		static string GetMSSqlPort(string instanceName, string dataSource, int timeout) {
			string port = String.Empty;
			try {
				DatagramSocket socket = new DatagramSocket();

				// send request
				sbyte[] buf = new sbyte[] {2};
				InetAddress address = InetAddress.getByName(dataSource);
				DatagramPacket packet = new DatagramPacket(buf, buf.Length, address, 1434);
				socket.send(packet);
				sbyte[] recbuf = new sbyte[1024];
				packet = new DatagramPacket(recbuf, recbuf.Length, packet.getAddress(), packet.getPort());

				// try to receive from socket while increasing timeouts in geometric progression
				int iterationTimeout = 1;
				int totalTimeout = 0;
				for(;;) {
					socket.setSoTimeout(iterationTimeout);
					try {
						socket.receive(packet);
						break;
					}
					catch (SocketTimeoutException e) {
						totalTimeout += iterationTimeout;
						iterationTimeout *= 2;
						if (totalTimeout >= timeout*1000) {
							throw new java.sql.SQLException(
								String.Format ("Unable to retrieve the port number for {0} using UDP on port 1434. Please see your network administrator to solve this problem or add the port number of your SQL server instance to your connection string (i.e. port=1433).", dataSource)
								);
						}
					}
				}
				sbyte[] rcvdSbytes = packet.getData();
				char[] rcvdChars = new char[rcvdSbytes.Length];
				for(int i=0; i < rcvdSbytes.Length; i++) {
					rcvdChars[i] = (char)rcvdSbytes[i];
				}
				String received = new String(rcvdChars);

				java.util.StringTokenizer st = new java.util.StringTokenizer(received, ";");
				String prev = "";
				bool instanceReached = instanceName == null || instanceName.Length == 0;
				while (st.hasMoreTokens()) {
					if (!instanceReached) {
						if (prev.Trim().Equals("InstanceName")) {
							if (String.Compare(instanceName,st.nextToken().Trim(),true) == 0) {
								instanceReached = true;
							}
						}
					}
					else {
						if (prev.Trim().Equals("tcp")) {
							port = st.nextToken().Trim();
							//ensure we got a valid int
							java.lang.Integer.parseInt(port);
							break;
						}
					}
					prev = st.nextToken();
				}
				socket.close();

				if (!instanceReached)
					throw new java.sql.SQLException(
						String.Format ("Specified SQL Server '{0}\\{1}' not found.", dataSource, instanceName)
						);
				return port;

			}
			catch (java.sql.SQLException) {
				throw;
			}
			catch (Exception e) {
				throw new java.sql.SQLException(e.Message);
			}
		}

		// TBD : refactor GetInstanceName and GetDataSource to single method
		private string GetInstanceName (IDictionary keyMapper)
		{
			string dataSource = (string) keyMapper [ServerName];
			string instanceName = String.Empty;
			int instanceIdx;
			if ((instanceIdx = dataSource.IndexOf ("\\")) == -1) 
				// no named instance specified - use a default name
				return DefaultInstanceName;
			else 
				// get named instance name
				return dataSource.Substring (instanceIdx + 1);
		}

		private string GetDataSource (IDictionary keyMapper)
		{
			string dataSource = (string) keyMapper [ServerName];
			int instanceIdx;
			if ((instanceIdx = dataSource.IndexOf ("\\")) != -1)
				// throw out named instance name
				dataSource = dataSource.Substring (0,instanceIdx);

			if (dataSource != null && dataSource.StartsWith ("(") && dataSource.EndsWith (")"))					
				dataSource = dataSource.Substring (1,dataSource.Length - 2);

			if (String.Empty.Equals (dataSource) || (String.Compare ("local",dataSource,true) == 0)) 
				dataSource = "localhost";

			return dataSource;
		}

		private int GetTimeout (IDictionary keyMapper)
		{
			string timeoutStr = (string) keyMapper [Timeout];
			if ((timeoutStr != null) && (timeoutStr.Length != 0)) {
				try {
					return Convert.ToInt32(timeoutStr);
				}
				catch(FormatException e) {
					throw ExceptionHelper.InvalidValueForKey("connect timeout");
				}
				catch (OverflowException e) {
					throw ExceptionHelper.InvalidValueForKey("connect timeout");
				}
			}
			return DefaultTimeout;
		}

		#endregion // Methods
	}
}
