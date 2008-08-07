/******************************************************************************
* The MIT License
* Copyright (c) 2006 Novell Inc.  www.novell.com
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
// Novell.Directory.Ldap.Extensions.BackupRestoreConstants.cs
//
// Author:
//   Palaniappan N (NPalaniappan@novell.com)
//
// (C) 2006 Novell, Inc (http://www.novell.com)
//

using System;

namespace Novell.Directory.Ldap.Extensions
{
	public class BackupRestoreConstants 
	{
	
		/**
		 * A constant for eDirectory LDAP Based Backup Request OID. 
		 */
		public const String NLDAP_LDAP_BACKUP_REQUEST = "2.16.840.1.113719.1.27.100.96";
    
		/**
		 * A constant for eDirectory LDAP Based Backup Response OID. 
		 */
		public const String NLDAP_LDAP_BACKUP_RESPONSE = "2.16.840.1.113719.1.27.100.97";
    
		/**
		 * A constant for eDirectory LDAP Based Restore Request OID. 
		 */
		public const String NLDAP_LDAP_RESTORE_REQUEST = "2.16.840.1.113719.1.27.100.98";
    
    
		/**
		 * A constant for eDirectory LDAP Based Restore Response OID. 
		 */
		public const String NLDAP_LDAP_RESTORE_RESPONSE = "2.16.840.1.113719.1.27.100.99";
		
		/**
		 * Default constructor
		 */
		public BackupRestoreConstants():base()
		{
			return;
		}

	}
}