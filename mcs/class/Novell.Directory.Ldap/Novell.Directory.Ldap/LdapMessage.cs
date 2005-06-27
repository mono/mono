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
// Novell.Directory.Ldap.LdapMessage.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using Novell.Directory.Ldap.Rfc2251;
using Novell.Directory.Ldap.Asn1;
using Novell.Directory.Ldap.Utilclass;

namespace Novell.Directory.Ldap
{
	
	/// <summary> The base class for Ldap request and response messages.
	/// 
	/// Subclassed by response messages used in asynchronous operations.
	/// 
	/// 
	/// </summary>
	public class LdapMessage
	{
		/// <summary> Returns the LdapMessage request associated with this response</summary>
		virtual internal LdapMessage RequestingMessage
		{
			/* package */
			
			get
			{
				return message.RequestingMessage;
			}
			
		}
		/// <summary> Returns any controls in the message.</summary>
		virtual public LdapControl[] Controls
		{
			get
			{
				
/*				LdapControl[] controls = null;
				RfcControls asn1Ctrls = message.Controls;
				
				if (asn1Ctrls != null)
				{
					controls = new LdapControl[asn1Ctrls.size()];
					for (int i = 0; i < asn1Ctrls.size(); i++)
					{
						RfcControl rfcCtl = (RfcControl) asn1Ctrls.get_Renamed(i);
						System.String oid = rfcCtl.ControlType.stringValue();
						sbyte[] value_Renamed = rfcCtl.ControlValue.byteValue();
						bool critical = rfcCtl.Criticality.booleanValue();
						
						controls[i] = controlFactory(oid, critical, value_Renamed);
					}
				}

				return controls;
*/
				LdapControl[] controls = null;
				RfcControls asn1Ctrls = message.Controls;
				
				// convert from RFC 2251 Controls to LDAPControl[].
				if (asn1Ctrls != null)
				{
					controls = new LdapControl[asn1Ctrls.size()];
					for (int i = 0; i < asn1Ctrls.size(); i++)
					{
						
						/*
						* At this point we have an RfcControl which needs to be
						* converted to the appropriate Response Control.  This requires
						* calling the constructor of a class that extends LDAPControl.
						* The controlFactory method searches the list of registered
						* controls and if a match is found calls the constructor
						* for that child LDAPControl. Otherwise, it returns a regular
						* LDAPControl object.
						*
						* Question: Why did we not call the controlFactory method when
						* we were parsing the control. Answer: By the time the
						* code realizes that we have a control it is already too late.
						*/
						RfcControl rfcCtl = (RfcControl) asn1Ctrls.get_Renamed(i);
						System.String oid = rfcCtl.ControlType.stringValue();
						sbyte[] value_Renamed = rfcCtl.ControlValue.byteValue();
						bool critical = rfcCtl.Criticality.booleanValue();
						
						/* Return from this call should return either an LDAPControl
						* or a class extending LDAPControl that implements the
						* appropriate registered response control
						*/
						controls[i] = controlFactory(oid, critical, value_Renamed);
					}
				}
				return controls;

			}
			
		}

		/// <summary> Returns the message ID.  The message ID is an integer value
		/// identifying the Ldap request and its response.
		/// </summary>
		virtual public int MessageID
		{
			get
			{
				if (imsgNum == - 1)
				{
					imsgNum = message.MessageID;
				}
				return imsgNum;
			}
			
		}
		/// <summary> Returns the Ldap operation type of the message.
		/// 
		/// The type is one of the following:
		/// <ul>
		/// <li>BIND_REQUEST            = 0;</li>
		/// <li>BIND_RESPONSE           = 1;</li>
		/// <li>UNBIND_REQUEST          = 2;</li>
		/// <li>SEARCH_REQUEST          = 3;</li>
		/// <li>SEARCH_RESPONSE         = 4;</li>
		/// <li>SEARCH_RESULT           = 5;</li>
		/// <li>MODIFY_REQUEST          = 6;</li>
		/// <li>MODIFY_RESPONSE         = 7;</li>
		/// <li>ADD_REQUEST             = 8;</li>
		/// <li>ADD_RESPONSE            = 9;</li>
		/// <li>DEL_REQUEST             = 10;</li>
		/// <li>DEL_RESPONSE            = 11;</li>
		/// <li>MODIFY_RDN_REQUEST      = 12;</li>
		/// <li>MODIFY_RDN_RESPONSE     = 13;</li>
		/// <li>COMPARE_REQUEST         = 14;</li>
		/// <li>COMPARE_RESPONSE        = 15;</li>
		/// <li>ABANDON_REQUEST         = 16;</li>
		/// <li>SEARCH_RESULT_REFERENCE = 19;</li>
		/// <li>EXTENDED_REQUEST        = 23;</li>
		/// <li>EXTENDED_RESPONSE       = 24;</li>
		/// <li>INTERMEDIATE_RESPONSE   = 25;</li>
		/// </ul>
		/// 
		/// </summary>
		/// <returns> The operation type of the message.
		/// </returns>
		virtual public int Type
		{
			get
			{
				if (messageType == - 1)
				{
					messageType = message.Type;
				}
				return messageType;
			}
			
		}
		/// <summary> Indicates whether the message is a request or a response
		/// 
		/// </summary>
		/// <returns> true if the message is a request, false if it is a response,
		/// a search result, or a search result reference.
		/// </returns>
		virtual public bool Request
		{
			get
			{
				return message.isRequest();
			}
			
		}
		/// <summary> Returns the RFC 2251 LdapMessage composed in this object.</summary>
		virtual internal RfcLdapMessage Asn1Object
		{
			/* package */
			
			get
			{
				return message;
			}
			
		}
		private System.String Name
		{
			get
			{
				System.String name;
				switch (Type)
				{
					
					case SEARCH_RESPONSE: 
						name = "LdapSearchResponse";
						break;
					
					case SEARCH_RESULT: 
						name = "LdapSearchResult";
						break;
					
					case SEARCH_REQUEST: 
						name = "LdapSearchRequest";
						break;
					
					case MODIFY_REQUEST: 
						name = "LdapModifyRequest";
						break;
					
					case MODIFY_RESPONSE: 
						name = "LdapModifyResponse";
						break;
					
					case ADD_REQUEST: 
						name = "LdapAddRequest";
						break;
					
					case ADD_RESPONSE: 
						name = "LdapAddResponse";
						break;
					
					case DEL_REQUEST: 
						name = "LdapDelRequest";
						break;
					
					case DEL_RESPONSE: 
						name = "LdapDelResponse";
						break;
					
					case MODIFY_RDN_REQUEST: 
						name = "LdapModifyRDNRequest";
						break;
					
					case MODIFY_RDN_RESPONSE: 
						name = "LdapModifyRDNResponse";
						break;
					
					case COMPARE_REQUEST: 
						name = "LdapCompareRequest";
						break;
					
					case COMPARE_RESPONSE: 
						name = "LdapCompareResponse";
						break;
					
					case BIND_REQUEST: 
						name = "LdapBindRequest";
						break;
					
					case BIND_RESPONSE: 
						name = "LdapBindResponse";
						break;
					
					case UNBIND_REQUEST: 
						name = "LdapUnbindRequest";
						break;
					
					case ABANDON_REQUEST: 
						name = "LdapAbandonRequest";
						break;
					
					case SEARCH_RESULT_REFERENCE: 
						name = "LdapSearchResultReference";
						break;
					
					case EXTENDED_REQUEST: 
						name = "LdapExtendedRequest";
						break;
					
					case EXTENDED_RESPONSE: 
						name = "LdapExtendedResponse";
						break;

				        case INTERMEDIATE_RESPONSE:
					        name = "LdapIntermediateResponse";
						break;
					
					default: 
						throw new System.SystemException("LdapMessage: Unknown Type " + Type);
					
				}
				return name;
			}
			
		}
		/// <summary> Retrieves the identifier tag for this message.
		/// 
		/// An identifier can be associated with a message with the
		/// <code>setTag</code> method.
		/// Tags are set by the application and not by the API or the server.
		/// If a server response <code>isRequest() == false</code> has no tag,
		/// the tag associated with the corresponding server request is used.
		/// 
		/// </summary>
		/// <returns> the identifier associated with this message or <code>null</code>
		/// if none.
		/// 
		/// </returns>
		/// <summary> Sets a string identifier tag for this message.
		/// 
		/// This method allows an API to set a tag and later identify messages
		/// by retrieving the tag associated with the message.
		/// Tags are set by the application and not by the API or the server.
		/// Message tags are not included with any message sent to or received
		/// from the server.
		/// 
		/// Tags set on a request to the server
		/// are automatically associated with the response messages when they are
		/// received by the API and transferred to the application.
		/// The application can explicitly set a different value in a
		/// response message.
		/// 
		/// To set a value in a server request, for example an
		/// {@link LdapSearchRequest}, you must create the object,
		/// set the tag, and use the
		/// {@link LdapConnection.SendRequest LdapConnection.sendRequest()}
		/// method to send it to the server.
		/// 
		/// </summary>
		/// <param name="stringTag"> the String assigned to identify this message.
		/// 
		/// </param>
		virtual public System.String Tag
		{
			get
			{
				if ((System.Object) this.stringTag != null)
				{
					return this.stringTag;
				}
				if (Request)
				{
					return null;
				}
				LdapMessage m = this.RequestingMessage;
				if (m == null)
				{
					return null;
				}
				return m.stringTag;
			}
			
			set
			{
				this.stringTag = value;
				return ;
			}
			
		}
		
		/// <summary> A bind request operation.
		/// 
		/// BIND_REQUEST = 0
		/// </summary>
		public const int BIND_REQUEST = 0;
		
		/// <summary> A bind response operation.
		/// 
		/// BIND_RESPONSE = 1
		/// </summary>
		public const int BIND_RESPONSE = 1;
		
		/// <summary> An unbind request operation.
		/// 
		/// UNBIND_REQUEST = 2
		/// </summary>
		public const int UNBIND_REQUEST = 2;
		
		/// <summary> A search request operation.
		/// 
		/// SEARCH_REQUEST = 3
		/// </summary>
		public const int SEARCH_REQUEST = 3;
		
		/// <summary> A search response containing data.
		/// 
		/// SEARCH_RESPONSE = 4
		/// </summary>
		public const int SEARCH_RESPONSE = 4;
		
		/// <summary> A search result message - contains search status.
		/// 
		/// SEARCH_RESULT = 5
		/// </summary>
		public const int SEARCH_RESULT = 5;
		
		/// <summary> A modify request operation.
		/// 
		/// MODIFY_REQUEST = 6
		/// </summary>
		public const int MODIFY_REQUEST = 6;
		
		/// <summary> A modify response operation.
		/// 
		/// MODIFY_RESPONSE = 7
		/// </summary>
		public const int MODIFY_RESPONSE = 7;
		
		/// <summary> An add request operation.
		/// 
		/// ADD_REQUEST = 8
		/// </summary>
		public const int ADD_REQUEST = 8;
		
		/// <summary> An add response operation.
		/// 
		/// ADD_RESONSE = 9
		/// </summary>
		public const int ADD_RESPONSE = 9;
		
		/// <summary> A delete request operation.
		/// 
		/// DEL_REQUEST = 10
		/// </summary>
		public const int DEL_REQUEST = 10;
		
		/// <summary> A delete response operation.
		/// 
		/// DEL_RESONSE = 11
		/// </summary>
		public const int DEL_RESPONSE = 11;
		
		/// <summary> A modify RDN request operation.
		/// 
		/// MODIFY_RDN_REQUEST = 12
		/// </summary>
		public const int MODIFY_RDN_REQUEST = 12;
		
		/// <summary> A modify RDN response operation.
		/// 
		/// MODIFY_RDN_RESPONSE = 13
		/// </summary>
		public const int MODIFY_RDN_RESPONSE = 13;
		
		/// <summary> A compare result operation.
		/// 
		/// COMPARE_REQUEST = 14
		/// </summary>
		public const int COMPARE_REQUEST = 14;
		
		/// <summary> A compare response operation.
		/// 
		/// COMPARE_RESPONSE = 15
		/// </summary>
		public const int COMPARE_RESPONSE = 15;
		
		/// <summary> An abandon request operation.
		/// 
		/// ABANDON_REQUEST = 16
		/// </summary>
		public const int ABANDON_REQUEST = 16;
		
		
		/// <summary> A search result reference operation.
		/// 
		/// SEARCH_RESULT_REFERENCE = 19
		/// </summary>
		public const int SEARCH_RESULT_REFERENCE = 19;
		
		/// <summary> An extended request operation.
		/// 
		/// EXTENDED_REQUEST = 23
		/// </summary>
		public const int EXTENDED_REQUEST = 23;
		
		/// <summary> An extended response operation.
		/// 
		/// EXTENDED_RESONSE = 24
		/// </summary>
		public const int EXTENDED_RESPONSE = 24;

		/// <summary> An intermediate response operation.
		/// 
		/// INTERMEDIATE_RESONSE = 25
		/// </summary>
	        public const int INTERMEDIATE_RESPONSE = 25;

		/// <summary> A request or response message for an asynchronous Ldap operation.</summary>
		protected internal RfcLdapMessage message;
		
		/// <summary> Lock object to protect counter for message numbers</summary>
		/*
		private static Object msgLock = new Object();
		*/
		
		/// <summary> Counters used to construct request message #'s, unique for each request
		/// Will be enabled after ASN.1 conversion
		/// </summary>
		/*
		private static int msgNum = 0; // Ldap Request counter
		*/
		private int imsgNum = - 1; // This instance LdapMessage number
		
		private int messageType = - 1;
		
		/* application defined tag to identify this message */
		private System.String stringTag = null;
		
		/// <summary> Dummy constuctor</summary>
		/* package */
		internal LdapMessage()
		{
			return ;
		}
		
		/// <summary> Creates an LdapMessage when sending a protocol operation and sends
		/// some optional controls with the message.
		/// 
		/// </summary>
		/// <param name="op">The operation type of message.
		/// 
		/// </param>
		/// <param name="controls">The controls to use with the operation.
		/// 
		/// </param>
		/// <seealso cref="Type">
		/// </seealso>
		/*package*/
		internal LdapMessage(int type, RfcRequest op, LdapControl[] controls)
		{
			
			// Get a unique number for this request message
			
			messageType = type;
			RfcControls asn1Ctrls = null;
			if (controls != null)
			{
				// Move LdapControls into an RFC 2251 Controls object.
				asn1Ctrls = new RfcControls();
				for (int i = 0; i < controls.Length; i++)
				{
//					asn1Ctrls.add(null);
					asn1Ctrls.add(controls[i].Asn1Object);
				}
			}
			
			// create RFC 2251 LdapMessage
			message = new RfcLdapMessage(op, asn1Ctrls);
			return ;
		}
		
		/// <summary> Creates an Rfc 2251 LdapMessage when the libraries receive a response
		/// from a command.
		/// 
		/// </summary>
		/// <param name="message">A response message.
		/// </param>
		protected internal LdapMessage(RfcLdapMessage message)
		{
			this.message = message;
			return ;
		}
		
		/// <summary> Returns a mutated clone of this LdapMessage,
		/// replacing base dn, filter.
		/// 
		/// </summary>
		/// <param name="dn">the base dn
		/// 
		/// </param>
		/// <param name="filter">the filter
		/// 
		/// </param>
		/// <param name="reference">true if a search reference
		/// 
		/// </param>
		/// <returns> the object representing the new message
		/// </returns>
		/* package */
		internal LdapMessage Clone(System.String dn, System.String filter, bool reference)
		{
			return new LdapMessage((RfcLdapMessage) message.dupMessage(dn, filter, reference));
		}
		
		/// <summary> Instantiates an LdapControl.  We search through our list of
		/// registered controls.  If we find a matchiing OID we instantiate
		/// that control by calling its contructor.  Otherwise we default to
		/// returning a regular LdapControl object
		/// 
		/// </summary>
		private LdapControl controlFactory(System.String oid, bool critical, sbyte[] value_Renamed)
		{
//			throw new NotImplementedException();
			RespControlVector regControls = LdapControl.RegisteredControls;
			try
			{
				/*
				* search through the registered extension list to find the
				* response control class
				*/
				System.Type respCtlClass = regControls.findResponseControl(oid);
				
				// Did not find a match so return default LDAPControl
				if (respCtlClass == null)
					return new LdapControl(oid, critical, value_Renamed);
				
				/* If found, get LDAPControl constructor */
				System.Type[] argsClass = new System.Type[]{typeof(System.String), typeof(bool), typeof(sbyte[])};
				System.Object[] args = new System.Object[]{oid, critical, value_Renamed};
				System.Exception ex = null;
				try
				{
					System.Reflection.ConstructorInfo ctlConstructor = respCtlClass.GetConstructor(argsClass);
					
					try
					{
						/* Call the control constructor for a registered Class*/
						System.Object ctl = null;
//						ctl = ctlConstructor.newInstance(args);
						ctl = ctlConstructor.Invoke(args);
						return (LdapControl) ctl;
					}
					catch (System.UnauthorizedAccessException e)
					{
						ex = e;
					}
					catch (System.Reflection.TargetInvocationException e)
					{
						ex = e;
					}
					catch (System.Exception e)
					{
						// Could not create the ResponseControl object
						// All possible exceptions are ignored. We fall through
						// and create a default LDAPControl object
						ex = e;
					}

				}
				catch (System.MethodAccessException e)
				{
					// bad class was specified, fall through and return a
					// default LDAPControl object
					ex = e;
				}
			}
			catch (System.FieldAccessException e)
			{
				// No match with the OID
				// Do nothing. Fall through and construct a default LDAPControl object.
			}
			// If we get here we did not have a registered response control
			// for this oid.  Return a default LDAPControl object.
			return new LdapControl(oid, critical, value_Renamed);

		}
		
		/// <summary> Creates a String representation of this object
		/// 
		/// </summary>
		/// <returns> a String representation for this LdapMessage
		/// </returns>
		public override System.String ToString()
		{
			return Name + "(" + MessageID + "): " + message.ToString();
		}
	}
}
