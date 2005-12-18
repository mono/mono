//
// System.Data.ProviderBase.PDbPortResolver
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


using System;

using java.net;

namespace System.Data.Common
{
	internal sealed  class DbPortResolver
	{
		public static int getMSSqlPort(AbstractDBConnection connection)
		{
			String instanceName = connection.InstanceName;
			int timeout = connection.ConnectionTimeout;
			int port = -1;
			try
			{
				DatagramSocket socket = new DatagramSocket();

				// send request
				sbyte[] buf = new sbyte[] {2};
				InetAddress address = InetAddress.getByName(connection.DataSource);
				DatagramPacket packet = new DatagramPacket(buf, buf.Length, address, 1434);
				socket.send(packet);
				sbyte[] recbuf = new sbyte[1024];
				packet = new DatagramPacket(recbuf, recbuf.Length, packet.getAddress(), packet.getPort());

				// try to receive from socket while increasing timeouts in geometric progression
				int iterationTimeout = 1;
				int totalTimeout = 0;
				for(;;) {
					socket.setSoTimeout(iterationTimeout);
					try
					{
						socket.receive(packet);
						break;
					}
					catch (SocketTimeoutException e)
					{
						totalTimeout += iterationTimeout;
						iterationTimeout *= 2;
						if (totalTimeout >= timeout*1000) {
							throw new java.sql.SQLException(
								String.Format ("Unable to retrieve the port number for {0} using UDP on port 1434. Please see your network administrator to solve this problem or add the port number of your SQL server instance to your connection string (i.e. port=1433).", connection.DataSource)
								);
						}
					}
				}
				sbyte[] rcvdSbytes = packet.getData();
				char[] rcvdChars = new char[rcvdSbytes.Length];
				for(int i=0; i < rcvdSbytes.Length; i++)
				{
					rcvdChars[i] = (char)rcvdSbytes[i];
				}
				String received = new String(rcvdChars);

				java.util.StringTokenizer st = new java.util.StringTokenizer(received, ";");
				String prev = "";
				bool instanceReached = instanceName == null || instanceName.Length == 0;
				while (st.hasMoreTokens())
				{
					if (!instanceReached)
					{
						if (prev.Trim().Equals("InstanceName"))
						{
							if (String.Compare(instanceName,st.nextToken().Trim(),true) == 0)
							{
								instanceReached = true;
							}
						}
					}
					else
					{
						if (prev.Trim().Equals("tcp"))
						{
							port = java.lang.Integer.parseInt(st.nextToken().Trim());
							break;
						}
					}
					prev = st.nextToken();
				}
				socket.close();

				if (!instanceReached)
					throw new java.sql.SQLException(
						String.Format ("Specified SQL Server '{0}\\{1}' not found.", connection.DataSource, instanceName)
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
	    
	}
}
