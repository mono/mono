/******************************************************************************
* The MIT License
* Copyright (c) 2003 Novell Inc.  www.novell.com
* 
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
* copies of the Software, and to  permit persons to whom the Software is 
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in 
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/
//
// Novell.Directory.Ldap.LdapMessageQueue.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using RfcLdapMessage = Novell.Directory.Ldap.Rfc2251.RfcLdapMessage;
using ExtResponseFactory = Novell.Directory.Ldap.Utilclass.ExtResponseFactory;
using IntermediateResponseFactory = Novell.Directory.Ldap.Utilclass.IntermediateResponseFactory;

namespace Novell.Directory.Ldap
{
	
	/// <summary>  Represents a queue of incoming asynchronous messages from the server.
	/// It is the common interface for {@link LdapResponseQueue} and
	/// {@link LdapSearchQueue}.
	/// </summary>
	public abstract class LdapMessageQueue
	{
		/// <summary> Returns the name used for debug
		/// 
		/// </summary>
		/// <returns> name of object instance used for debug
		/// </returns>
		virtual internal System.String DebugName
		{
			/* package */
			
			get
			{
				return name;
			}
			
		}
		/// <summary> Returns the internal client message agent
		/// 
		/// </summary>
		/// <returns> The internal client message agent
		/// </returns>
		virtual internal MessageAgent MessageAgent
		{
			/* package */
			
			get
			{
				return agent;
			}
			
		}
		/// <summary> Returns the message IDs for all outstanding requests. These are requests
		/// for which a response has not been received from the server or which
		/// still have messages to be retrieved with getResponse.
		/// 
		/// The last ID in the array is the messageID of the last submitted
		/// request.
		/// 
		/// </summary>
		/// <returns> The message IDs for all outstanding requests.
		/// </returns>
		virtual public int[] MessageIDs
		{
			get
			{
				return agent.MessageIDs;
			}
			
		}
		/// <summary> The message agent object associated with this queue</summary>
		/* package */
		internal MessageAgent agent;
		
		// Queue name used only for debug
		/* package */
		internal System.String name = "";
		
		// nameLock used to protect queueNum during increment
		/* package */
		internal static System.Object nameLock;
		
		// Queue number used only for debug
		/* package */
		internal static int queueNum = 0;
		
		/// <summary> Constructs a response queue using the specified message agent
		/// 
		/// </summary>
		/// <param name="agent">The message agent to associate with this conneciton
		/// </param>
		/* package */
		internal LdapMessageQueue(System.String myname, MessageAgent agent)
		{
			// Get a unique connection name for debug
			this.agent = agent;
			return ;
		}
		
		/// <summary> Returns the response from an Ldap request.
		/// 
		/// The getResponse method blocks until a response is available, or until
		/// all operations associated with the object have completed or been
		/// canceled, and then returns the response.
		/// 
		/// The application is responsible to determine the type of message
		/// returned.
		/// 
		/// </summary>
		/// <returns> The response.
		/// 
		/// </returns>
		/// <seealso cref="LdapResponse">
		/// </seealso>
		/// <seealso cref="LdapSearchResult">
		/// </seealso>
		/// <seealso cref="LdapSearchResultReference">
		/// 
		/// </seealso>
		/// <exception> LdapException A general exception which includes an error
		/// message and an Ldap error code.
		/// </exception>
		public virtual LdapMessage getResponse()
		{
			return getResponse(null);
		}
		
		/// <summary> Returns the response from an Ldap request for a particular message ID.
		/// 
		/// The getResponse method blocks until a response is available
		/// for a particular message ID, or until all operations associated
		/// with the object have completed or been canceled, and
		/// then returns the response.  If there is no outstanding operation for
		/// the message ID (or if it is zero or a negative number),
		/// IllegalArgumentException is thrown.
		/// 
		/// The application is responsible to determine the type of message
		/// returned.
		/// 
		/// </summary>
		/// <param name="msgid">query for responses for a specific message request
		/// 
		/// </param>
		/// <returns> The response from the server.
		/// 
		/// </returns>
		/// <seealso cref="LdapResponse">
		/// </seealso>
		/// <seealso cref="LdapSearchResult">
		/// </seealso>
		/// <seealso cref="LdapSearchResultReference">
		/// 
		/// </seealso>
		/// <exception> LdapException A general exception which includes an error
		/// message and an Ldap error code.
		/// </exception>
		public virtual LdapMessage getResponse(System.Int32 msgid)
		{
			return getResponse(new Integer32(msgid));
		}
		
		/// <summary> Private implementation of getResponse.
		/// Has an Integer object as a parameter so we can distinguish
		/// the null and the message number case
		/// </summary>
//		private LdapMessage getResponse(System.Int32 msgid)
		private LdapMessage getResponse(Integer32 msgid)
		{
			System.Object resp;
			RfcLdapMessage message;
			LdapMessage response;
			if ((resp = agent.getLdapMessage(msgid)) == null)
			{
				// blocks
				return null; // no messages from this agent
			}
			// Local error occurred, contains a LocalException
			if (resp is LdapResponse)
			{
				return (LdapMessage) resp;
			}
			// Normal message handling
			message = (RfcLdapMessage) resp;
			switch (message.Type)
			{
				
				case LdapMessage.SEARCH_RESPONSE: 
					response = new LdapSearchResult(message);
					break;
				
				case LdapMessage.SEARCH_RESULT_REFERENCE: 
					response = new LdapSearchResultReference(message);
					break;
				
				case LdapMessage.EXTENDED_RESPONSE: 
					ExtResponseFactory fac = new ExtResponseFactory();
					response = ExtResponseFactory.convertToExtendedResponse(message);
					break;

				case LdapMessage.INTERMEDIATE_RESPONSE                       : 
					response = IntermediateResponseFactory.convertToIntermediateResponse(message);
					break;				

				default: 
					response = new LdapResponse(message);
					break;
				
			}
			return response;
		}
		
		/// <summary> Reports true if any response has been received from the server and not
		/// yet retrieved with getResponse.  If getResponse has been used to
		/// retrieve all messages received to this point, then isResponseReceived
		/// returns false.
		/// 
		/// </summary>
		/// <returns> true if a response is available to be retrieved via getResponse,
		/// otherwise false.
		/// 
		/// </returns>
		public virtual bool isResponseReceived()
		{
			return agent.isResponseReceived();
		}
		
		/// <summary> Reports true if a response has been received from the server for
		/// a particular message ID but not yet retrieved with getResponse.  If
		/// there is no outstanding operation for the message ID (or if it is
		/// zero or a negative number), IllegalArgumentException is thrown.
		/// 
		/// </summary>
		/// <param name="msgid">   A particular message ID to query for available responses.
		/// 
		/// </param>
		/// <returns> true if a response is available to be retrieved via getResponse
		/// for the specified message ID, otherwise false.
		/// 
		/// </returns>
		public virtual bool isResponseReceived(int msgid)
		{
			return agent.isResponseReceived(msgid);
		}
		
		/// <summary> Reports true if all results have been received for a particular
		/// message id.
		/// 
		/// If the search result done has been received from the server for the
		/// message id, it reports true.  There may still be messages waiting to be
		/// retrieved by the applcation with getResponse.
		/// 
		/// @throws IllegalArgumentException if there is no outstanding operation
		/// for the message ID,
		/// </summary>
		public virtual bool isComplete(int msgid)
		{
			return agent.isComplete(msgid);
		}
		static LdapMessageQueue()
		{
			nameLock = new System.Object();
		}
	}
}
