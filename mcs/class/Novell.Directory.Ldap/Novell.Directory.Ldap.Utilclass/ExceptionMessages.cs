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
// Novell.Directory.Ldap.Utilclass.ExceptionMessages.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;

namespace Novell.Directory.Ldap.Utilclass
{
	
	/// <summary> This class contains strings that may be associated with Exceptions generated
	/// by the Ldap API libraries.
	/// Two entries are made for each message, a String identifier, and the
	/// actual error string.  Parameters are identified as {0}, {1}, etc.
	/// </summary>
	public class ExceptionMessages:System.Resources.ResourceManager
	{
		public System.Object[][] getContents()
		{
			return contents;
		}
		//static strings to aide lookup and guarantee accuracy:
		//DO NOT include these strings in other Locales
		[CLSCompliantAttribute(false)]
		public const System.String TOSTRING = "TOSTRING";
		public const System.String SERVER_MSG = "SERVER_MSG";
		public const System.String MATCHED_DN = "MATCHED_DN";
		public const System.String FAILED_REFERRAL = "FAILED_REFERRAL";
		public const System.String REFERRAL_ITEM = "REFERRAL_ITEM";
		public const System.String CONNECTION_ERROR = "CONNECTION_ERROR";
		public const System.String CONNECTION_IMPOSSIBLE = "CONNECTION_IMPOSSIBLE";
		public const System.String CONNECTION_WAIT = "CONNECTION_WAIT";
		public const System.String CONNECTION_FINALIZED = "CONNECTION_FINALIZED";
		public const System.String CONNECTION_CLOSED = "CONNECTION_CLOSED";
		public const System.String CONNECTION_READER = "CONNECTION_READER";
		public const System.String DUP_ERROR = "DUP_ERROR";
		public const System.String REFERRAL_ERROR = "REFERRAL_ERROR";
		public const System.String REFERRAL_LOCAL = "REFERRAL_LOCAL";
		public const System.String REFERENCE_ERROR = "REFERENCE_ERROR";
		public const System.String REFERRAL_SEND = "REFERRAL_SEND";
		public const System.String REFERENCE_NOFOLLOW = "REFERENCE_NOFOLLOW";
		public const System.String REFERRAL_BIND = "REFERRAL_BIND";
		public const System.String REFERRAL_BIND_MATCH = "REFERRAL_BIND_MATCH";
		public const System.String NO_DUP_REQUEST = "NO_DUP_REQUEST";
		public const System.String SERVER_CONNECT_ERROR = "SERVER_CONNECT_ERROR";
		public const System.String NO_SUP_PROPERTY = "NO_SUP_PROPERTY";
		public const System.String ENTRY_PARAM_ERROR = "ENTRY_PARAM_ERROR";
		public const System.String DN_PARAM_ERROR = "DN_PARAM_ERROR";
		public const System.String RDN_PARAM_ERROR = "RDN_PARAM_ERROR";
		public const System.String OP_PARAM_ERROR = "OP_PARAM_ERROR";
		public const System.String PARAM_ERROR = "PARAM_ERROR";
		public const System.String DECODING_ERROR = "DECODING_ERROR";
		public const System.String ENCODING_ERROR = "ENCODING_ERROR";
		public const System.String IO_EXCEPTION = "IO_EXCEPTION";
		public const System.String INVALID_ESCAPE = "INVALID_ESCAPE";
		public const System.String SHORT_ESCAPE = "SHORT_ESCAPE";
		public const System.String INVALID_CHAR_IN_FILTER = "INVALID_CHAR_IN_FILTER";
		public const System.String INVALID_CHAR_IN_DESCR = "INVALID_CHAR_IN_DESCR";
		public const System.String INVALID_ESC_IN_DESCR = "INVALID_ESC_IN_DESCR";
		public const System.String UNEXPECTED_END = "UNEXPECTED_END";
		public const System.String MISSING_LEFT_PAREN = "MISSING_LEFT_PAREN";
		public const System.String MISSING_RIGHT_PAREN = "MISSING_RIGHT_PAREN";
		public const System.String EXPECTING_RIGHT_PAREN = "EXPECTING_RIGHT_PAREN";
		public const System.String EXPECTING_LEFT_PAREN = "EXPECTING_LEFT_PAREN";
		public const System.String NO_OPTION = "NO_OPTION";
		public const System.String INVALID_FILTER_COMPARISON = "INVALID_FILTER_COMPARISON";
		public const System.String NO_MATCHING_RULE = "NO_MATCHING_RULE";
		public const System.String NO_ATTRIBUTE_NAME = "NO_ATTRIBUTE_NAME";
		public const System.String NO_DN_NOR_MATCHING_RULE = "NO_DN_NOR_MATCHING_RULE";
		public const System.String NOT_AN_ATTRIBUTE = "NOT_AN_ATTRIBUTE";
		public const System.String UNEQUAL_LENGTHS = "UNEQUAL_LENGTHS";
		public const System.String IMPROPER_REFERRAL = "IMPROPER_REFERRAL";
		public const System.String NOT_IMPLEMENTED = "NOT_IMPLEMENTED";
		public const System.String NO_MEMORY = "NO_MEMORY";
		public const System.String SERVER_SHUTDOWN_REQ = "SERVER_SHUTDOWN_REQ";
		public const System.String INVALID_ADDRESS = "INVALID_ADDRESS";
		public const System.String UNKNOWN_RESULT = "UNKNOWN_RESULT";
		public const System.String OUTSTANDING_OPERATIONS = "OUTSTANDING_OPERATIONS";
		public const System.String WRONG_FACTORY = "WRONG_FACTORY";
		public const System.String NO_TLS_FACTORY = "NO_TLS_FACTORY";
		public const System.String NO_STARTTLS = "NO_STARTTLS";
		public const System.String STOPTLS_ERROR = "STOPTLS_ERROR";
		public const System.String MULTIPLE_SCHEMA = "MULTIPLE_SCHEMA";
		public const System.String NO_SCHEMA = "NO_SCHEMA";
		public const System.String READ_MULTIPLE = "READ_MULTIPLE";
		public const System.String CANNOT_BIND = "CANNOT_BIND";
		public const System.String SSL_PROVIDER_MISSING = "SSL_PROVIDER_MISSING";
		
		//End constants
		
		internal static readonly System.Object[][] contents = {new System.Object[]{"TOSTRING", "{0}: {1} ({2}) {3}"}, new System.Object[]{"SERVER_MSG", "{0}: Server Message: {1}"}, new System.Object[]{"MATCHED_DN", "{0}: Matched DN: {1}"}, new System.Object[]{"FAILED_REFERRAL", "{0}: Failed Referral: {1}"}, new System.Object[]{"REFERRAL_ITEM", "{0}: Referral: {1}"}, new System.Object[]{"CONNECTION_ERROR", "Unable to connect to server {0}:{1}"}, new System.Object[]{"CONNECTION_IMPOSSIBLE", "Unable to reconnect to server, application has never called connect()"}, new System.Object[]{"CONNECTION_WAIT", "Connection lost waiting for results from {0}:{1}"}, new System.Object[]{"CONNECTION_FINALIZED", "Connection closed by the application finalizing the object"}, new System.Object[]{"CONNECTION_CLOSED", "Connection closed by the application disconnecting"}, new System.Object[]{"CONNECTION_READER", "Reader thread terminated"}, new System.Object[]{"DUP_ERROR", "RfcLdapMessage: Cannot duplicate message built from the input stream"}, new System.Object[]{"REFERENCE_ERROR", "Error attempting to follow a search continuation reference"}, new System.Object[]{"REFERRAL_ERROR", "Error attempting to follow a referral"}, new System.Object[]{"REFERRAL_LOCAL", "LdapSearchResults.{0}(): No entry found & request is not complete"}, new System.Object[]{"REFERRAL_SEND", "Error sending request to referred server"}, new System.Object[]{"REFERENCE_NOFOLLOW", "Search result reference received, and referral following is off"}, new System.Object[]{"REFERRAL_BIND", "LdapBind.bind() function returned null"}, new System.Object[]{"REFERRAL_BIND_MATCH", "Could not match LdapBind.bind() connection with Server Referral URL list"}, new System.Object[]{"NO_DUP_REQUEST", "Cannot duplicate message to follow referral for {0} request, not allowed"}, new System.Object[]{"SERVER_CONNECT_ERROR", "Error connecting to server {0} while attempting to follow a referral"}, new System.Object[]{"NO_SUP_PROPERTY", "Requested property is not supported."}, new 
			System.Object[]{"ENTRY_PARAM_ERROR", "Invalid Entry parameter"}, new System.Object[]{"DN_PARAM_ERROR", "Invalid DN parameter"}, new System.Object[]{"RDN_PARAM_ERROR", "Invalid DN or RDN parameter"}, new System.Object[]{"OP_PARAM_ERROR", "Invalid extended operation parameter, no OID specified"}, new System.Object[]{"PARAM_ERROR", "Invalid parameter"}, new System.Object[]{"DECODING_ERROR", "Error Decoding responseValue"}, new System.Object[]{"ENCODING_ERROR", "Encoding Error"}, new System.Object[]{"IO_EXCEPTION", "I/O Exception on host {0}, port {1}"}, new System.Object[]{"INVALID_ESCAPE", "Invalid value in escape sequence \"{0}\""}, new System.Object[]{"SHORT_ESCAPE", "Incomplete escape sequence"}, new System.Object[]{"UNEXPECTED_END", "Unexpected end of filter"}, new System.Object[]{"MISSING_LEFT_PAREN", "Unmatched parentheses, left parenthesis missing"}, new System.Object[]{"NO_OPTION", "Semicolon present, but no option specified"}, new System.Object[]{"MISSING_RIGHT_PAREN", "Unmatched parentheses, right parenthesis missing"}, new System.Object[]{"EXPECTING_RIGHT_PAREN", "Expecting right parenthesis, found \"{0}\""}, new System.Object[]{"EXPECTING_LEFT_PAREN", "Expecting left parenthesis, found \"{0}\""}, new System.Object[]{"NO_ATTRIBUTE_NAME", "Missing attribute description"}, new System.Object[]{"NO_DN_NOR_MATCHING_RULE", "DN and matching rule not specified"}, new System.Object[]{"NO_MATCHING_RULE", "Missing matching rule"}, new System.Object[]{"INVALID_FILTER_COMPARISON", "Invalid comparison operator"}, new System.Object[]{"INVALID_CHAR_IN_FILTER", "The invalid character \"{0}\" needs to be escaped as \"{1}\""}, new System.Object[]{"INVALID_ESC_IN_DESCR", "Escape sequence not allowed in attribute description"}, new System.Object[]{"INVALID_CHAR_IN_DESCR", "Invalid character \"{0}\" in attribute description"}, new System.Object[]{"NOT_AN_ATTRIBUTE", "Schema element is not an LdapAttributeSchema object"}, new System.Object[]{"UNEQUAL_LENGTHS", 
			"Length of attribute Name array does not equal length of Flags array"}, new System.Object[]{"IMPROPER_REFERRAL", "Referral not supported for command {0}"}, new System.Object[]{"NOT_IMPLEMENTED", "Method LdapConnection.startTLS not implemented"}, new System.Object[]{"NO_MEMORY", "All results could not be stored in memory, sort failed"}, new System.Object[]{"SERVER_SHUTDOWN_REQ", "Received unsolicited notification from server {0}:{1} to shutdown"}, new System.Object[]{"INVALID_ADDRESS", "Invalid syntax for address with port; {0}"}, new System.Object[]{"UNKNOWN_RESULT", "Unknown Ldap result code {0}"}, new System.Object[]{"OUTSTANDING_OPERATIONS", "Cannot start or stop TLS because outstanding Ldap operations exist on this connection"}, new System.Object[]{"WRONG_FACTORY", "StartTLS cannot use the set socket factory because it does not implement LdapTLSSocketFactory"}, new System.Object[]{"NO_TLS_FACTORY", "StartTLS failed because no LdapTLSSocketFactory has been set for this Connection"}, new System.Object[]{"NO_STARTTLS", "An attempt to stopTLS on a connection where startTLS had not been called"}, new System.Object[]{"STOPTLS_ERROR", "Error stopping TLS: Error getting input & output streams from the original socket"}, new System.Object[]{"MULTIPLE_SCHEMA", "Multiple schema found when reading the subschemaSubentry for {0}"}, new System.Object[]{"NO_SCHEMA", "No schema found when reading the subschemaSubentry for {0}"}, new System.Object[]{"READ_MULTIPLE", "Read response is ambiguous, multiple entries returned"}, new System.Object[]{"CANNOT_BIND", "Cannot bind. Use PoolManager.getBoundConnection()"}};
	} //End ExceptionMessages
}
