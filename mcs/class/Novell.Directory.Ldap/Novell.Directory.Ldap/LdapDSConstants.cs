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
// Novell.Directory.Ldap.LdapDSConstants.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
namespace Novell.Directory.Ldap
{
	
	/// <summary> LDAPDSConstants.java contains bit values for [Entry Rights], [All attribute
	/// Rights], attribute rights, and entry flags in Novell eDirectory 
	/// </summary>
	public struct LdapDSConstants{
		///////////////////////////////////////////////////////////////////////////
		// bit values for [Entry Rights] of access control in Novell eDirecroty
		///////////////////////////////////////////////////////////////////////////
		/// <summary> Browse right.
		/// 
		/// <p>Allows a trustee to discover objects in the Novell eDirectory tree. 
		/// </p>
		/// </summary>
		public readonly static long LDAP_DS_ENTRY_BROWSE = 0x00000001L;
		/// <summary> Creation right .
		/// 
		/// <p>Allows a trustee to create child objects (new objects that are
		/// subordinate to the object in the Novell eDirectory tree). </p>
		/// </summary>
		public readonly static long LDAP_DS_ENTRY_ADD = 0x00000002L;
		/// <summary> Delete right.
		/// 
		/// <p>Allows a trustee to delete an object. This right does not allow a 
		/// trustee to delete a container object that has subordinate objects. </p>
		/// </summary>
		public readonly static long LDAP_DS_ENTRY_DELETE = 0x00000004L;
		/// <summary> Rename right.
		/// 
		/// <p>Allows a trustee to rename the object.</p>
		/// </summary>
		public readonly static long LDAP_DS_ENTRY_RENAME = 0x00000008L;
		/// <summary> Supercisor rights.
		/// 
		/// <p>Gives a trustee all rights to an object and its attributes.</p>
		/// </summary>
		public readonly static long LDAP_DS_ENTRY_SUPERVISOR = 0x00000010L;
		/// <summary> Inherit ACL.
		/// 
		/// <p>Allows a trustee to inherit the rights granted in the ACL
		/// and exercise them on subordinate objects.</p>
		/// </summary>
		public readonly static long LDAP_DS_ENTRY_INHERIT_CTL = 0x00000040L;
		///////////////////////////////////////////////////////////////////////////
		// bit values for [Attribute Rights] and attribute rights of access control
		// in Novell eDirecroty
		///////////////////////////////////////////////////////////////////////////
		/// <summary> Attribute compare.
		/// 
		/// <p>Allows a trustee to compare a value with an attribute's value. This 
		/// allows the trustee to see if the attribute contains the value without 
		/// having rights to see the value.</p>
		/// </summary>
		public readonly static long LDAP_DS_ATTR_COMPARE = 0x00000001L;
		/// <summary> Attribute read.
		/// 
		/// <p>Allows a trustee to read an attribute value. This right confers
		/// the Compare right.</p>
		/// </summary>
		public readonly static long LDAP_DS_ATTR_READ = 0x00000002L;
		/// <summary> Attribute write.
		/// 
		/// <p>Allows a trustee to add, delete, or modify an attribute value. This 
		/// right also gives the trustee the Self (Add or Delete Self) right.</p>
		/// </summary>
		public readonly static long LDAP_DS_ATTR_WRITE = 0x00000004L;
		/// <summary> Self rights.
		/// 
		/// <p>Allows a trustee to add or delete its name as an attribute value on 
		/// those attributes that take object names as their values.</p>
		/// </summary>
		public readonly static long LDAP_DS_ATTR_SELF = 0x00000008L;
		/// <summary> All attribute rights.
		/// 
		/// <p>Gives a trustee all rights to the object's attributes.</p>
		/// </summary>
		public readonly static long LDAP_DS_ATTR_SUPERVISOR = 0x00000020L;
		/// <summary> inherit the ACL rights.
		/// 
		/// <p>Allows a trustee to inherit the rights granted in the ACL and 
		/// exercise these attribute rights on subordinate objects.</p>
		/// </summary>
		public readonly static long LDAP_DS_ATTR_INHERIT_CTL = 0x00000040L;
		/// <summary> dynamic ACL.
		/// 
		/// <p>This bit will be set if the trustee in the ACL is a dynamic group 
		/// and its dynamic members should be considered for ACL rights 
		/// calculation purposes. If this bit is reset, the trustee's static 
		/// members alone will be considered for rights calculation purposes.</p>
		/// </summary>
		public readonly static long LDAP_DS_DYNAMIC_ACL = 0x40000000L;
		///////////////////////////////////////////////////////////////////////////
		// bit values of entry flag in Novell eDirectory
		///////////////////////////////////////////////////////////////////////////
		/// <summary> Alias object.
		/// 
		/// <p>Indicates that the entry is an alias object.</p>
		/// </summary>
		public readonly static int LDAP_DS_ALIAS_ENTRY = 0x0001;
		/// <summary> Partition root.
		/// 
		/// <p>Indicates that the entry is the root partition.</p>
		/// </summary>
		public readonly static int LDAP_DS_PARTITION_ROOT = 0x0002;
		/// <summary>Container entry.
		/// 
		/// <p>Indicates that the entry is a container object and not a container
		/// alias.</p>
		/// </summary>
		public readonly static int LDAP_DS_CONTAINER_ENTRY = 0x0004;
		/// <summary> Container alias.
		/// 
		/// <p>Indicates that the entry is a container alias.</p>
		/// </summary>
		public readonly static int LDAP_DS_CONTAINER_ALIAS = 0x0008;
		/// <summary> Matches the list.
		/// 
		/// <p>Indicates that the entry matches the List filter.</p>
		/// </summary>
		public readonly static int LDAP_DS_MATCHES_LIST_FILTER = 0x0010;
		/// <summary> Reference entry.
		/// 
		/// <p>Indicates that the entry has been created as a reference rather than 
		/// an entry. The synchronization process is still running and has not 
		/// created an entry for the object on this replica.</p>
		/// </summary>
		public readonly static int LDAP_DS_REFERENCE_ENTRY = 0x0020;
		/// <summary> 4.0x reference entry.
		/// 
		/// <p>Indicates that the entry is a reference rather than the object. The
		/// reference is in the older 4.0x form and appears only when upgrading </p>
		/// </summary>
		public readonly static int LDAP_DS_40X_REFERENCE_ENTRY = 0x0040;
		/// <summary> New entry.
		/// 
		/// <p>Indicates that the entry is being back linked.</p>
		/// </summary>
		public readonly static int LDAP_DS_BACKLINKED = 0x0080;
		/// <summary> Temporary reference.
		/// 
		/// <p>Indicates that the entry is new and replicas are still being updated.
		/// </p>
		/// </summary>
		public readonly static int LDAP_DS_NEW_ENTRY = 0x0100;
		/// <summary> Temporary reference.
		/// 
		/// <p>Indicates that an external reference has been temporarily created for
		/// authentication; when the object logs out, the temporary reference is 
		/// deleted.</p>
		/// </summary>
		public readonly static int LDAP_DS_TEMPORARY_REFERENCE = 0x0200;
		/// <summary> Audited.
		/// 
		/// <p>Indicates that the entry is being audited.</p>
		/// </summary>
		public readonly static int LDAP_DS_AUDITED = 0x0400;
		/// <summary> Entry not present.
		/// 
		/// <p>Indicates that the state of the entry is not present.</p>
		/// </summary>
		public readonly static int LDAP_DS_ENTRY_NOT_PRESENT = 0x0800;
		/// <summary> Verify entry creation timestamp.
		/// 
		/// <p>Indicates the entry's creation timestamp needs to be verified. Novell 
		/// eDirectory sets this flag when a replica is removed or upgraded from 
		/// NetWare 4.11 to NetWare 5.</p>
		/// </summary>
		public readonly static int LDAP_DS_ENTRY_VERIFY_CTS = 0x1000;
		/// <summary> entry damaged.
		/// 
		/// <p>Indicates that the entry's information does not conform to the 
		/// standard format and is therefore damaged.</p>
		/// </summary>
		public readonly static int LDAP_DS_ENTRY_DAMAGED = 0x2000;
	}
}
