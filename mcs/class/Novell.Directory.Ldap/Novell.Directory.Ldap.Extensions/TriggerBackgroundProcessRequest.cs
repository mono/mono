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
// Novell.Directory.Ldap.Extensions.TriggerBackgroundProcessRequest.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using Novell.Directory.Ldap;
using Novell.Directory.Ldap.Asn1;
using Novell.Directory.Ldap.Utilclass;

namespace Novell.Directory.Ldap.Extensions
{
	
	/// <summary> This API is used to trigger the specified background process on the
	/// Novell eDirectory server.
	/// 
	/// The TriggerBackgroundProcessRequest uses tone of the following OID's
	/// depending on the process being triggered:
	/// 2.16.840.1.113719.1.27.100.43
	/// 2.16.840.1.113719.1.27.100.47
	/// 2.16.840.1.113719.1.27.100.49
	/// 2.16.840.1.113719.1.27.100.51
	/// 2.16.840.1.113719.1.27.100.53
	/// 2.16.840.1.113719.1.27.100.55
	/// 
	/// The requestValue has the following format:
	/// 
	/// requestValue ::=
	///    NULL
	/// 
	/// </summary>
	
	public class TriggerBackgroundProcessRequest:LdapExtendedOperation
	{
		
		/// <summary> Constants used to refer to different Novell eDirectory
		/// background processes
		/// </summary>
		public const int Ldap_BK_PROCESS_BKLINKER = 1;
		public const int Ldap_BK_PROCESS_JANITOR = 2;
		public const int Ldap_BK_PROCESS_LIMBER = 3;
		public const int Ldap_BK_PROCESS_SKULKER = 4;
		public const int Ldap_BK_PROCESS_SCHEMA_SYNC = 5;
		public const int Ldap_BK_PROCESS_PART_PURGE = 6;
		
		/// <summary> 
		/// Based on the process ID specified this constructer cosntructs an
		/// LdapExtendedOperation object with the apppropriate OID.
		/// 
		/// </summary>
		/// <param name="processID">  This id identifies the background process to be triggerd.
		/// 
		/// </param>
		/// <exception> LdapException A general exception which includes an error message
		/// and an Ldap error code.
		/// </exception>
		public TriggerBackgroundProcessRequest(int processID):base(null, null)
		{
			
			switch (processID)
			{
				
				
				case Ldap_BK_PROCESS_BKLINKER: 
					setID(ReplicationConstants.TRIGGER_BKLINKER_REQ);
					break;
				
				case Ldap_BK_PROCESS_JANITOR: 
					setID(ReplicationConstants.TRIGGER_JANITOR_REQ);
					break;
				
				case Ldap_BK_PROCESS_LIMBER: 
					setID(ReplicationConstants.TRIGGER_LIMBER_REQ);
					break;
				
				case Ldap_BK_PROCESS_SKULKER: 
					setID(ReplicationConstants.TRIGGER_SKULKER_REQ);
					break;
				
				case Ldap_BK_PROCESS_SCHEMA_SYNC: 
					setID(ReplicationConstants.TRIGGER_SCHEMA_SYNC_REQ);
					break;
				
				case Ldap_BK_PROCESS_PART_PURGE: 
					setID(ReplicationConstants.TRIGGER_PART_PURGE_REQ);
					break;
				
				default: 
					throw new System.ArgumentException(ExceptionMessages.PARAM_ERROR);
				
			}
		}
	}
}
