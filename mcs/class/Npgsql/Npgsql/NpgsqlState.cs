// created on 6/14/2002 at 7:56 PM

// Npgsql.NpgsqlState.cs
// 
// Author:
// 	Dave Joyner <d4ljoyn@yahoo.com>
//
//	Copyright (C) 2002 The Npgsql Development Team
//	npgsql-general@gborg.postgresql.org
//	http://gborg.postgresql.org/project/npgsql/projdisplay.php
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA


using System;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Text;
using System.Resources;

namespace Npgsql
{
	///<summary> This class represents the base class for the state pattern design pattern
	/// implementation.
	/// </summary>
	/// 
	
	internal abstract class NpgsqlState
 	{
        private readonly String CLASSNAME = "NpgsqlState";
        protected ResourceManager resman = null;
        
 		public virtual void Open(NpgsqlConnection context) {}
 		public virtual void Startup(NpgsqlConnection context) {}
 		public virtual void Authenticate(NpgsqlConnection context, string password){}
 		public virtual void Query(NpgsqlConnection context, NpgsqlCommand command) {}
 		public virtual void Ready( NpgsqlConnection context ) {}
 		public virtual void FunctionCall(NpgsqlConnection context, NpgsqlCommand command){}
 		public virtual void Parse(NpgsqlConnection context, NpgsqlParse parse){}
 		public virtual void Flush(NpgsqlConnection context){}
 		public virtual void Sync(NpgsqlConnection context){}
 		public virtual void Bind(NpgsqlConnection context, NpgsqlBind bind){}
 		public virtual void Execute(NpgsqlConnection context, NpgsqlExecute execute){}
		
        public NpgsqlState()
        {
            resman = new ResourceManager(this.GetType());
        }
        
		public virtual void Close( NpgsqlConnection context )
		{
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Close");
			if ( context.State == ConnectionState.Open )
			{
				Stream stream = context.NormalStream;
				if ( stream.CanWrite )
				{
					stream.WriteByte((Byte)'X');
				  if (context.BackendProtocolVersion >= ProtocolVersion.Version3)
				    PGUtil.WriteInt32(stream, 4);
					stream.Flush();
				}
			}
			ChangeState( context, NpgsqlClosedState.Instance );
		}
		
		///<summary> This method is used by the states to change the state of the context.
		/// </summary>
		/// 
		
 		protected virtual void ChangeState(NpgsqlConnection context, NpgsqlState newState) 
 		{
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ChangeState");
			context.CurrentState = newState;
		}
		
		///<summary>
		/// This method is responsible to handle all protocol messages sent from the backend.
		/// It holds all the logic to do it.
		/// To exchange data, it uses a Mediator object from which it reads/writes information
		/// to handle backend requests.
		/// </summary>
		/// 
		
		protected virtual void ProcessBackendResponses( NpgsqlConnection context )
		{
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ProcessBackendResponses");
            
			BufferedStream 	stream = context.BufferedStream;
			Int32	authType;
			Boolean readyForQuery = false;
						
			NpgsqlMediator mediator = context.Mediator;
			
			// Reset the mediator.
			mediator.Reset();
			
			Int16 rowDescNumFields = 0;
			NpgsqlRowDescription rd = null;
						
			Byte[] inputBuffer = new Byte[ 500 ];
			
						
			while (!readyForQuery)
			{
				// Check the first Byte of response.
				switch ( stream.ReadByte() )
				{
					case NpgsqlMessageTypes.ErrorResponse :
					
                        NpgsqlError error = new NpgsqlError(context.BackendProtocolVersion);
                        error.ReadFromStream(stream, context.Encoding);
                            
                        //mediator.Errors.Add(errorMessage);
                        mediator.Errors.Add(error.Message);
					
                        NpgsqlEventLog.LogMsg(resman, "Log_ErrorResponse", LogLevel.Debug, error.Message);
                        
						// Return imediately if it is in the startup state or connected state as
						// there is no more messages to consume.
						// Possible error in the NpgsqlStartupState:
						//		Invalid password.
						// Possible error in the NpgsqlConnectedState:
						//		No pg_hba.conf configured.
						
						if ((context.CurrentState == NpgsqlStartupState.Instance) ||
						    (context.CurrentState == NpgsqlConnectedState.Instance))
							return;
						
						break;
						

					case NpgsqlMessageTypes.AuthenticationRequest :
                    
                        NpgsqlEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "AuthenticationRequest");

						stream.Read(inputBuffer, 0, 4);
				  
				    if (context.BackendProtocolVersion >= ProtocolVersion.Version3)
				      stream.Read(inputBuffer, 0, 4);
				  
						authType = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(inputBuffer, 0));

						if ( authType == NpgsqlMessageTypes.AuthenticationOk )
						{
							NpgsqlEventLog.LogMsg(resman, "Log_AuthenticationOK", LogLevel.Debug);
                            						
							break;
						}

						if ( authType == NpgsqlMessageTypes.AuthenticationClearTextPassword )
						{
							NpgsqlEventLog.LogMsg(resman, "Log_AuthenticationClearTextRequest", LogLevel.Debug);
                            
							// Send the PasswordPacket.

							ChangeState( context, NpgsqlStartupState.Instance );
							context.Authenticate(context.ServerPassword);
							
					  	break;
						}
					
						
						if ( authType == NpgsqlMessageTypes.AuthenticationMD5Password )
						{
							NpgsqlEventLog.LogMsg(resman, "Log_AuthenticationMD5Request", LogLevel.Debug);
                            // Now do the "MD5-Thing"
							// for this the Password has to be:
							// 1. md5-hashed with the username as salt
							// 2. md5-hashed again with the salt we get from the backend
							
							
							MD5 md5 = MD5.Create();
							
							
							// 1.
							byte[] passwd = context.Encoding.GetBytes(context.ServerPassword);
							byte[] saltUserName = context.Encoding.GetBytes(context.UserName);
							
							byte[] crypt_buf = new byte[passwd.Length + saltUserName.Length];
							
							passwd.CopyTo(crypt_buf, 0);
							saltUserName.CopyTo(crypt_buf, passwd.Length);							
							
							
							
							StringBuilder sb = new StringBuilder ();
							byte[] hashResult = md5.ComputeHash(crypt_buf);
							foreach (byte b in hashResult)
								sb.Append (b.ToString ("x2"));
							
							
							String prehash = sb.ToString();
							
							byte[] prehashbytes = context.Encoding.GetBytes(prehash);
							
							
							
							byte[] saltServer = new byte[4];
							stream.Read(saltServer, 0, 4);
							// Send the PasswordPacket.
							ChangeState( context, NpgsqlStartupState.Instance );
							
							
							// 2.
							
							crypt_buf = new byte[prehashbytes.Length + saltServer.Length];
							prehashbytes.CopyTo(crypt_buf, 0);
							saltServer.CopyTo(crypt_buf, prehashbytes.Length);
							
							sb = new StringBuilder ("md5"); // This is needed as the backend expects md5 result starts with "md5"
							hashResult = md5.ComputeHash(crypt_buf);
							foreach (byte b in hashResult)
								sb.Append (b.ToString ("x2"));
							
							context.Authenticate(sb.ToString ());
							
					  	break;
						}

						// Only AuthenticationClearTextPassword and AuthenticationMD5Password supported for now.
                        NpgsqlEventLog.LogMsg(resman, "Log_AuthenticationOK", LogLevel.Debug);
						mediator.Errors.Add(String.Format(resman.GetString("Exception_AuthenticationMethodNotSupported"), authType));
 						return;
												
					case NpgsqlMessageTypes.RowDescription:
						// This is the RowDescription message.
						NpgsqlEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "RowDescription");
						rd = new NpgsqlRowDescription(context.BackendProtocolVersion);
						rd.ReadFromStream(stream, context.Encoding);
						
						// Initialize the array list which will contain the data from this rowdescription.
						//rows = new ArrayList();
					
						rowDescNumFields = rd.NumFields;
						mediator.AddRowDescription(rd);
					
											
						// Now wait for the AsciiRow messages.
						break;
						
					case NpgsqlMessageTypes.AsciiRow:
					
						// This is the AsciiRow message.
						NpgsqlEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "AsciiRow");
						NpgsqlAsciiRow asciiRow = new NpgsqlAsciiRow(rd, context.OidToNameMapping, context.BackendProtocolVersion);
						asciiRow.ReadFromStream(stream, context.Encoding);
						
						
						// Add this row to the rows array.
						//rows.Add(ascii_row);
						mediator.AddAsciiRow(asciiRow);
					
						// Now wait for CompletedResponse message.
						break;
					
					case NpgsqlMessageTypes.BinaryRow:
						
                        NpgsqlEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "BinaryRow");
						NpgsqlBinaryRow binaryRow = new NpgsqlBinaryRow(rd);
						binaryRow.ReadFromStream(stream, context.Encoding);
					
						mediator.AddBinaryRow(binaryRow);
					
						break;
					
					case NpgsqlMessageTypes.ReadyForQuery :

						NpgsqlEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "ReadyForQuery");
						readyForQuery = true;
						ChangeState( context, NpgsqlReadyState.Instance );
						break;

					case NpgsqlMessageTypes.BackendKeyData :
					
						NpgsqlEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "BackendKeyData");
						// BackendKeyData message.
						NpgsqlBackEndKeyData backend_keydata = new NpgsqlBackEndKeyData(context.BackendProtocolVersion);
						backend_keydata.ReadFromStream(stream);
						mediator.AddBackendKeydata(backend_keydata);
						
					
						// Wait for ReadForQuery message
						break;;
					
					case NpgsqlMessageTypes.NoticeResponse :

						NpgsqlEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "NoticeResponse");
						String noticeResponse = PGUtil.ReadString( stream, context.Encoding );
						
						// Wait for ReadForQuery message
						break;

					case NpgsqlMessageTypes.CompletedResponse :
						// This is the CompletedResponse message.
						// Get the string returned.
						
						
						if (context.BackendProtocolVersion >= ProtocolVersion.Version3)
						  PGUtil.ReadInt32(stream, new Byte[4]);
				  
						String result = PGUtil.ReadString(stream, context.Encoding);
						
						NpgsqlEventLog.LogMsg(resman, "Log_CompletedResponse", LogLevel.Debug, result);
						// Add result from the processing.
												
						mediator.AddCompletedResponse(result);
					
						// Now wait for ReadyForQuery message.
						break;
					
					case NpgsqlMessageTypes.CursorResponse :
						// This is the cursor response message. 
						// It is followed by a C NULL terminated string with the name of 
						// the cursor in a FETCH case or 'blank' otherwise.
						// In this case it should be always 'blank'.
						// [FIXME] Get another name for this function.
						NpgsqlEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "CursorResponse");
						
						String cursorName = PGUtil.ReadString(stream, context.Encoding);
						// Continue waiting for ReadyForQuery message.
						break;

                    case NpgsqlMessageTypes.ParseComplete :
                        NpgsqlEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "ParseComplete");
                        // Just read up the message length.
                        PGUtil.ReadInt32(stream, new Byte[4]);
                        readyForQuery = true;
                        break;
                  
                    case NpgsqlMessageTypes.BindComplete :
                        NpgsqlEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "BindComplete");
                        // Just read up the message length.
                        PGUtil.ReadInt32(stream, new Byte[4]);
                        readyForQuery = true;
                        break;  
				  
					case NpgsqlMessageTypes.EmptyQueryResponse :
                        NpgsqlEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "EmptyQueryResponse");
						// This is the EmptyQueryResponse.
						// [FIXME] Just ignore it this way?
						// networkStream.Read(inputBuffer, 0, 1);
						//GetStringFromNetStream(networkStream);
						PGUtil.ReadString(stream, context.Encoding);
						break;
					
					case NpgsqlMessageTypes.NotificationResponse  :
 
 						NpgsqlEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "NotificationResponse");
 
 						Byte[] input_buffer = new Byte[4];
 						stream.Read(input_buffer, 0, 4);
 						Int32 PID = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(input_buffer, 0));
 						String notificationResponse = PGUtil.ReadString( stream, context.Encoding );
 						mediator.AddNotification(new NpgsqlNotificationEventArgs(PID, notificationResponse));
 
 						// Wait for ReadForQuery message
 						break;
				  
                    case NpgsqlMessageTypes.ParameterStatus :
                        NpgsqlEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "ParameterStatus");
                        NpgsqlParameterStatus parameterStatus = new NpgsqlParameterStatus();
                        parameterStatus.ReadFromStream(stream, context.Encoding);
                      
                        NpgsqlEventLog.LogMsg(resman, "Log_ParameterStatus", LogLevel.Debug, parameterStatus.Parameter, parameterStatus.ParameterValue);
                        if (parameterStatus.Parameter == "server_version")
                            context.ServerVersion = parameterStatus.ParameterValue;
                            break;				
				}
			}
			
		}
 	}
 	
 	
 
}
