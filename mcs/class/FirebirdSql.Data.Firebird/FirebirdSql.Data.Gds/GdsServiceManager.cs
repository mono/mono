/*
 *	Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *	   The contents of this file are subject to the Initial 
 *	   Developer's Public License Version 1.0 (the "License"); 
 *	   you may not use this file except in compliance with the 
 *	   License. You may obtain a copy of the License at 
 *	   http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *	   Software distributed under the License is distributed on 
 *	   an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *	   express or implied. See the License for the specific 
 *	   language governing rights and limitations under the License.
 * 
 *	Copyright (c) 2002, 2005 Carlos Guzman Alvarez
 *	All Rights Reserved.
 */

using System;
using System.IO;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Gds
{
	internal sealed class GdsServiceManager : IServiceManager
	{
		#region Fields

		private int handle;
		private GdsConnection connection;

		#endregion

		#region Properties

		public int Handle
		{
			get { return this.handle; }
		}

		public bool IsLittleEndian
		{
			get { return false; }
		}

		#endregion

		#region Constructors

		public GdsServiceManager()
		{
		}

		#endregion

		#region Methods

		public void Attach(ServiceParameterBuffer spb, string dataSource, int port, string service)
		{
			lock (this)
			{
				try
				{
					if (this.connection == null)
					{
						this.connection = new GdsConnection();
					}

					this.connection.Connect(dataSource, port, 8192, Charset.DefaultCharset);

					this.connection.Send.Write(IscCodes.op_service_attach);
					this.connection.Send.Write(0);
					this.connection.Send.Write(service);
					this.connection.Send.WriteBuffer(spb.ToArray());
					this.connection.Send.Flush();

					try
					{
						this.handle = this.connection.ReadGenericResponse().ObjectHandle;
					}
					catch (IscException)
					{
						try
						{
							this.Detach();
						}
						catch
						{
						}

						throw;
					}
				}
				catch (IOException)
				{
					this.connection.Disconnect();

					throw new IscException(IscCodes.isc_net_write_err);
				}
			}
		}

		public void Detach()
		{
			lock (this)
			{
				try
				{
					this.connection.Send.Write(IscCodes.op_service_detach);
					this.connection.Send.Write(this.Handle);
					this.connection.Send.Flush();

					this.connection.ReadGenericResponse();

					this.handle = 0;
				}
				catch (IOException)
				{
					throw new IscException(IscCodes.isc_network_error);
				}
				finally
				{
					try
					{
						this.connection.Disconnect();
					}
					catch (IOException)
					{
						throw new IscException(IscCodes.isc_network_error);
					}
				}
			}
		}

		public void Start(ServiceParameterBuffer spb)
		{
			lock (this)
			{
				try
				{
					this.connection.Send.Write(IscCodes.op_service_start);
					this.connection.Send.Write(this.Handle);
					this.connection.Send.Write(0);
					this.connection.Send.WriteBuffer(spb.ToArray(), spb.Length);
					this.connection.Send.Flush();

					try
					{
						this.connection.ReadGenericResponse();
					}
					catch (IscException)
					{
						throw;
					}
				}
				catch (IOException)
				{
					throw new IscException(IscCodes.isc_net_write_err);
				}
			}
		}

		public void Query(
			ServiceParameterBuffer	spb,
			int						requestLength,
			byte[]					requestBuffer,
			int						bufferLength,
			byte[]					buffer)
		{
			lock (this)
			{
				try
				{
					this.connection.Send.Write(IscCodes.op_service_info);	//	operation
					this.connection.Send.Write(this.Handle);				//	db_handle
					this.connection.Send.Write((int)0);						//	incarnation					
					this.connection.Send.WriteTyped(
						IscCodes.isc_spb_version, spb.ToArray());			//	Service parameter buffer
					this.connection.Send.WriteBuffer(
						requestBuffer, requestLength);						//	request	buffer
					this.connection.Send.Write(bufferLength);				//	result buffer length

					this.connection.Send.Flush();

					GdsResponse r = this.connection.ReadGenericResponse();

					Buffer.BlockCopy(r.Data, 0, buffer, 0, bufferLength);
				}
				catch (IOException)
				{
					throw new IscException(IscCodes.isc_network_error);
				}
			}
		}

		#endregion

		#region Buffer creation	methods

		public ServiceParameterBuffer CreateParameterBuffer()
		{
			return new ServiceParameterBuffer();
		}

		#endregion
	}
}
