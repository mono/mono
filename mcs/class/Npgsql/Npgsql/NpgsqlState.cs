// created on 6/14/2002 at 7:56 PM

// Npgsql.NpgsqlState.cs
// 
// Author:
// 	Dave Joyner <d4ljoyn@yahoo.com>
//
//	Copyright (C) 2002 The Npgsql Development Team
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


namespace Npgsql
{
	///<summary> This class represents the base class for the state pattern design pattern
	/// implementation.
	/// </summary>
	/// 
	
	internal abstract class NpgsqlState
 	{
 		public virtual void Open(NpgsqlConnection context) {}
 		public virtual void Startup(NpgsqlConnection context) {}
 		public virtual void Authenticate(NpgsqlConnection context){}
 		public virtual void Query(NpgsqlConnection context, NpgsqlCommand command) {}
 		public virtual void Ready( NpgsqlConnection context ) {}
 		public virtual void FunctionCall(NpgsqlConnection context){}
		
		
		public virtual void Close( NpgsqlConnection context )
		{
			if ( context.State == ConnectionState.Open )
			{
				NetworkStream stream = context.TcpClient.GetStream();
				if ( stream.CanWrite )
				{
					stream.WriteByte((Byte)'X');
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
			context.CurrentState = newState;
		}
		
		//public delegate void ProcessBackendMessage( NpgsqlConnection context, byte[] message );
 		
 		public delegate void ProcessBackendMessage( NpgsqlConnection context, Object message );
		
		///<summary>
		/// This method is responsible to handle all protocol messages sent from the backend.
		/// It holds all the logic to do it.
		/// To exchange data, it uses a Mediator object from which it read/write information
		/// to handle backend requests.
		/// </summary>
		/// 
		
		protected virtual void ProcessBackendResponses( NpgsqlConnection context, ProcessBackendMessage handler )
		{
			NetworkStream 	stream = context.TcpClient.GetStream(); 
			Int32 bytesRead;
			Int32	authType;
			Boolean readyForQuery = false;
			String errorMessage = null;
			
			NpgsqlMediator mediator = context.Mediator;
			
			// Reset the mediator.
			mediator.Reset();
			
			Int16 rowDescNumFields = 0;
			
			//NpgsqlRowDescription 	rd = null;		
			//ArrayList							rows = null;	// Rows associated with the row description.
			
			Byte[] inputBuffer = new Byte[ 500 ];
			
			NpgsqlEventLog.LogMsg( this.ToString(), LogLevel.Debug);
			
			while (!readyForQuery)
			{
				// Check the first Byte of response.
				switch ( stream.ReadByte() )
				{
					case NpgsqlMessageTypes.ErrorResponse :
						
						NpgsqlEventLog.LogMsg("ErrorResponse message from Server", LogLevel.Debug);
						errorMessage = PGUtil.ReadString(stream, context.Encoding );
						
						mediator.Errors.Add(errorMessage);
					
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

						NpgsqlEventLog.LogMsg("AuthenticationRequest message from Server", LogLevel.Debug);
						bytesRead = stream.Read(inputBuffer, 0, 4);
						authType = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(inputBuffer, 0));

						if ( authType == NpgsqlMessageTypes.AuthenticationOk )
						{
							NpgsqlEventLog.LogMsg("AuthenticationOK received", LogLevel.Debug);
						
							break;
						}

						if ( authType == NpgsqlMessageTypes.AuthenticationClearTextPassword )
						{
							NpgsqlEventLog.LogMsg("Server requested cleartext password authentication.", LogLevel.Debug);

							// Send the PasswordPacket.

							ChangeState( context, NpgsqlStartupState.Instance );
							context.Authenticate();
							
					  	break;
						}
					
						
						// Only AuthenticationClearTextPassword supported for now.
						mediator.Errors.Add("Only AuthenticationClearTextPassword supported for now.");
						return;
						
					case NpgsqlMessageTypes.RowDescription:
						// This is the RowDescription message.
						
						NpgsqlRowDescription rd = new NpgsqlRowDescription();
						rd.ReadFromStream(stream, context.Encoding);
						
						// Initialize the array list which will contain the data from this rowdescription.
						//rows = new ArrayList();
					
						rowDescNumFields = rd.NumFields;
						mediator.AddRowDescription(rd);
					
											
						// Now wait for the AsciiRow messages.
						break;
						
					case NpgsqlMessageTypes.AsciiRow:
					
						// This is the AsciiRow message.
						
						NpgsqlAsciiRow asciiRow = new NpgsqlAsciiRow(rowDescNumFields);
						asciiRow.ReadFromStream(stream, context.Encoding);
						
						
						// Add this row to the rows array.
						//rows.Add(ascii_row);
						mediator.AddAsciiRow(asciiRow);
					
						// Now wait for CompletedResponse message.
						break;
					
										
					case NpgsqlMessageTypes.ReadyForQuery :

						NpgsqlEventLog.LogMsg("ReadyForQuery message from Server", LogLevel.Debug);
						readyForQuery = true;
						ChangeState( context, NpgsqlReadyState.Instance );
						break;

					case NpgsqlMessageTypes.BackendKeyData :
					
						NpgsqlEventLog.LogMsg("BackendKeyData message from Server", LogLevel.Debug);
						// BackendKeyData message.
						NpgsqlBackEndKeyData backend_keydata = new NpgsqlBackEndKeyData();
						backend_keydata.ReadFromStream(stream);
						mediator.AddBackendKeydata(backend_keydata);
						
					
						NpgsqlEventLog.LogMsg("Listening for next message", LogLevel.Debug);
						// Wait for ReadForQuery message
						break;;
					
					case NpgsqlMessageTypes.NoticeResponse :

						NpgsqlEventLog.LogMsg("NoticeResponse message from Server", LogLevel.Debug);
						String noticeResponse = PGUtil.ReadString( stream, context.Encoding );
						NpgsqlEventLog.LogMsg("Listening for next message", LogLevel.Debug);
						// Wait for ReadForQuery message
						break;

					case NpgsqlMessageTypes.CompletedResponse :
						// This is the CompletedResponse message.
						// Get the string returned.
												
						String result = PGUtil.ReadString(stream, context.Encoding);
						
						NpgsqlEventLog.LogMsg("CompletedResponse message from Server: " + result, LogLevel.Debug);
						// Add result from the processing.
						
						// Check if there were processed any rowdescription.
						/*if (rd != null)
							context.AddStateProcessData(new NpgsqlResultSet(rd, rows));
												
						context.AddStateProcessData(result);
						*/
						
						mediator.AddCompletedResponse(result);
					
						// Now wait for ReadyForQuery message.
						break;
					
					case NpgsqlMessageTypes.CursorResponse :
						// This is the cursor response message. 
						// It is followed by a C NULL terminated string with the name of 
						// the cursor in a FETCH case or 'blank' otherwise.
						// In this case it should be always 'blank'.
						// [FIXME] Get another name for this function.
						
						//String cursor_name = GetStringFromNetStream(networkStream);
						String cursorName = PGUtil.ReadString(stream, context.Encoding);
						// Continue wainting for ReadyForQuery message.
						break;

					case NpgsqlMessageTypes.EmptyQueryResponse :
						// This is the EmptyQueryResponse.
						// [FIXME] Just ignore it this way?
						// networkStream.Read(inputBuffer, 0, 1);
						//GetStringFromNetStream(networkStream);
						PGUtil.ReadString(stream, context.Encoding);
						break;
				}
			}
			
			// Check if there was an error. If so, throw an exception.
			/*if (errorMessage != null)
				throw new NpgsqlException(errorMessage);*/
				
			
			
		}
 	}
 	
 	
 
}
