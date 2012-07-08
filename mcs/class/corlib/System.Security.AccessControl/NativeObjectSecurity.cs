//
// System.Security.AccessControl.NativeObjectSecurity implementation
//
// Authors:
//	Dick Porter  <dick@ximian.com>
//	James Bellinger  <jfb@zer7.com>
//
// Copyright (C) 2005, 2006 Novell, Inc (http://www.novell.com)
// Copyright (C) 2012       James Bellinger
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
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace System.Security.AccessControl
{
	public abstract class NativeObjectSecurity : CommonObjectSecurity
	{
		ExceptionFromErrorCode exception_from_error_code;
		ResourceType resource_type;
		
		protected internal delegate Exception ExceptionFromErrorCode (int errorCode,
									      string name, SafeHandle handle,
									      object context);
		
		internal NativeObjectSecurity (CommonSecurityDescriptor securityDescriptor, ResourceType resourceType)
			: base (securityDescriptor)
		{
			resource_type = resourceType;
		}

		protected NativeObjectSecurity (bool isContainer,
						ResourceType resourceType)
			: this (isContainer, resourceType, null, null)
		{
		}

		protected NativeObjectSecurity (bool isContainer,
						ResourceType resourceType,
						ExceptionFromErrorCode exceptionFromErrorCode,
						object exceptionContext)
			: base (isContainer)
		{
			exception_from_error_code = exceptionFromErrorCode;
			resource_type = resourceType;
		}
		
		protected NativeObjectSecurity (bool isContainer,
						ResourceType resourceType,
						SafeHandle handle,
						AccessControlSections includeSections)
			: this (isContainer, resourceType, handle, includeSections, null, null)
		{
		}
		
		protected NativeObjectSecurity (bool isContainer,
						ResourceType resourceType,
						string name,
						AccessControlSections includeSections)
			: this (isContainer, resourceType, name, includeSections, null, null)
		{
		}
		
		protected NativeObjectSecurity (bool isContainer,
						ResourceType resourceType,
						SafeHandle handle,
						AccessControlSections includeSections,
						ExceptionFromErrorCode exceptionFromErrorCode,
						object exceptionContext)
			: this (isContainer, resourceType, exceptionFromErrorCode, exceptionContext)
		{
			RaiseExceptionOnFailure (InternalGet (handle, includeSections),
						 null, handle, exceptionContext);
			 ClearAccessControlSectionsModified ();
		}
		
		protected NativeObjectSecurity (bool isContainer,
						ResourceType resourceType,
						string name,
						AccessControlSections includeSections,
						ExceptionFromErrorCode exceptionFromErrorCode,
						object exceptionContext)
			: this (isContainer, resourceType, exceptionFromErrorCode, exceptionContext)
		{
			RaiseExceptionOnFailure (InternalGet (name, includeSections),
						 name, null, exceptionContext);
			ClearAccessControlSectionsModified ();
		}

		void ClearAccessControlSectionsModified ()
		{
			WriteLock ();
			try {
				AccessControlSectionsModified = AccessControlSections.None;
			} finally {
				WriteUnlock ();
			}
		}
		
		protected override sealed void Persist (SafeHandle handle,
							AccessControlSections includeSections)
		{
			Persist (handle, includeSections, null);
		}
		
		protected override sealed void Persist (string name,
							AccessControlSections includeSections)
		{
			Persist (name, includeSections, null);
		}
		
		internal void PersistModifications (SafeHandle handle)
		{
			WriteLock();
			try {
				Persist (handle, AccessControlSectionsModified, null);
			} finally {
				WriteUnlock ();
			}
		}
		
		protected void Persist (SafeHandle handle,
					AccessControlSections includeSections,
					object exceptionContext)
		{
			WriteLock ();
			try {
				RaiseExceptionOnFailure (InternalSet (handle, includeSections), null, handle, exceptionContext);
				AccessControlSectionsModified &= ~includeSections;
			} finally {
				WriteUnlock ();
			}
		}

		internal void PersistModifications (string name)
		{
			WriteLock();
			try {
				Persist (name, AccessControlSectionsModified, null);
			} finally {
				WriteUnlock ();
			}
		}
		
		protected void Persist (string name,
					AccessControlSections includeSections,
					object exceptionContext)
		{
			if (null == name)
				throw new ArgumentNullException ("name");

			WriteLock ();
			try {
				RaiseExceptionOnFailure (InternalSet (name, includeSections), name, null, exceptionContext);
				AccessControlSectionsModified &= ~includeSections;
			} finally {
				WriteUnlock ();
			}
		}
		
		internal static Exception DefaultExceptionFromErrorCode (int errorCode,
									 string name, SafeHandle handle,
									 object context)
		{
			switch (errorCode) {
				case 2: return new FileNotFoundException ();
				case 3: return new DirectoryNotFoundException ();
				case 5: return new UnauthorizedAccessException ();
				case 1314: return new PrivilegeNotHeldException (); // happens with audit rules
				default: return new InvalidOperationException ("OS error code " + errorCode.ToString());
			}
		}
		
		void RaiseExceptionOnFailure (int errorCode, string name, SafeHandle handle, object context)
		{
			if (errorCode == 0) return;
			throw (exception_from_error_code ?? DefaultExceptionFromErrorCode)(errorCode, name, handle, context);
		}
		
		// InternalGet/InternalSet are virtual so that non-Windows platforms which do not share an
		// API between files, mutexes, etc. can override in the subclass and do their own thing.
		internal virtual int InternalGet (SafeHandle handle,
						  AccessControlSections includeSections)
		{
			if (Environment.OSVersion.Platform != PlatformID.Win32NT)
				throw new PlatformNotSupportedException ();

			return Win32GetHelper (delegate (SecurityInfos securityInfos,
							 out IntPtr owner, out IntPtr group,
							 out IntPtr dacl, out IntPtr sacl, out IntPtr descriptor)
				{
					return GetSecurityInfo (handle, ResourceType, securityInfos,
								out owner, out group,
								out dacl, out sacl, out descriptor);
				}, includeSections);
		}
		
		internal virtual int InternalGet (string name,
						  AccessControlSections includeSections)
		{
			if (Environment.OSVersion.Platform != PlatformID.Win32NT)
				throw new PlatformNotSupportedException ();

			return Win32GetHelper (delegate (SecurityInfos securityInfos,
							 out IntPtr owner, out IntPtr group,
							 out IntPtr dacl, out IntPtr sacl, out IntPtr descriptor)
				{
					return GetNamedSecurityInfo (Win32FixName (name), ResourceType, securityInfos,
								     out owner, out group,
								     out dacl, out sacl, out descriptor);
				}, includeSections);
		}
		
		internal virtual int InternalSet (SafeHandle handle,
						  AccessControlSections includeSections)
		{
			if (Environment.OSVersion.Platform != PlatformID.Win32NT)
				throw new PlatformNotSupportedException ();

			return Win32SetHelper ((securityInfos, owner, group, dacl, sacl) =>
				SetSecurityInfo (handle, ResourceType, securityInfos, owner, group, dacl, sacl),
				includeSections);
		}
		
		internal virtual int InternalSet (string name,
						  AccessControlSections includeSections)
		{	
			if (Environment.OSVersion.Platform != PlatformID.Win32NT)
				throw new PlatformNotSupportedException ();
		
			return Win32SetHelper ((securityInfos, owner, group, dacl, sacl) =>
				SetNamedSecurityInfo (Win32FixName (name), ResourceType, securityInfos, owner, group, dacl, sacl),
				includeSections);
		}
		
		internal ResourceType ResourceType {
			get { return resource_type; }
		}
		
		#region Win32 Details		
		int Win32GetHelper (GetSecurityInfoNativeCall nativeCall,
				    AccessControlSections includeSections)
		{
			bool getOwner = 0 != (includeSections & AccessControlSections.Owner);
			bool getGroup = 0 != (includeSections & AccessControlSections.Group);
			bool getDacl = 0 != (includeSections & AccessControlSections.Access);
			bool getSacl = 0 != (includeSections & AccessControlSections.Audit);
			
			SecurityInfos securityInfos = 0;
			if (getOwner) securityInfos |= SecurityInfos.Owner;
			if (getGroup) securityInfos |= SecurityInfos.Group;
			if (getDacl ) securityInfos |= SecurityInfos.DiscretionaryAcl;
			if (getSacl ) securityInfos |= SecurityInfos.SystemAcl;
			
			IntPtr owner, group, dacl, sacl, descriptor;
			int result = nativeCall (securityInfos,
						 out owner, out group, out dacl, out sacl, out descriptor);
			if (0 != result) return result;
			
			try {
				int binaryLength = 0;
				if (IsValidSecurityDescriptor (descriptor))
					binaryLength = GetSecurityDescriptorLength (descriptor);
					
				byte[] binaryForm = new byte[binaryLength];
				Marshal.Copy (descriptor, binaryForm, 0, binaryLength);
				SetSecurityDescriptorBinaryForm (binaryForm, includeSections);
			} finally {
				LocalFree (descriptor);
			}
			return 0;
		}
		
		int Win32SetHelper (SetSecurityInfoNativeCall nativeCall,
				    AccessControlSections includeSections)
		{
			// SE_REGISTRY_KEY will fail UnauthorizedAccessException without this check.
			if (AccessControlSections.None == includeSections) return 0;
			
			SecurityInfos securityInfos = 0;
			byte[] owner = null, group = null, dacl = null, sacl = null;
			
			if (0 != (includeSections & AccessControlSections.Owner)) {
				securityInfos |= SecurityInfos.Owner;
				SecurityIdentifier ownerSid = (SecurityIdentifier)GetOwner (typeof (SecurityIdentifier));
				if (null != ownerSid) {
					owner = new byte[ownerSid.BinaryLength];
					ownerSid.GetBinaryForm (owner, 0);
				}
			}
			
			if (0 != (includeSections & AccessControlSections.Group)) {
				securityInfos |= SecurityInfos.Group;
				SecurityIdentifier groupSid = (SecurityIdentifier)GetGroup (typeof (SecurityIdentifier));
				if (null != groupSid) {
					group = new byte[groupSid.BinaryLength];
					groupSid.GetBinaryForm (group, 0);
				}
			}
			
			if (0 != (includeSections & AccessControlSections.Access)) {
				securityInfos |= SecurityInfos.DiscretionaryAcl;
				if (AreAccessRulesProtected)
					securityInfos |= unchecked((SecurityInfos)0x80000000);
				else
					securityInfos |= (SecurityInfos)0x20000000;
				dacl = new byte[descriptor.DiscretionaryAcl.BinaryLength];
				descriptor.DiscretionaryAcl.GetBinaryForm (dacl, 0);
			}
			
			if (0 != (includeSections & AccessControlSections.Audit)) {
				if (null != descriptor.SystemAcl) {
					securityInfos |= SecurityInfos.SystemAcl;
					if (AreAuditRulesProtected)
						securityInfos |= (SecurityInfos)0x40000000;
					else
						securityInfos |= (SecurityInfos)0x10000000;
					sacl = new byte[descriptor.SystemAcl.BinaryLength];
					descriptor.SystemAcl.GetBinaryForm (sacl, 0);
				}
			}
			
			return nativeCall (securityInfos, owner, group, dacl, sacl);
		}
		
		string Win32FixName (string name)
		{
			if (ResourceType == ResourceType.RegistryKey) {
				// For (Get|Set)NamedSecurityInfo, registry paths lack the HKEY_ prefix.
				if (!name.StartsWith ("HKEY_")) throw new InvalidOperationException ();
				name = name.Substring ("HKEY_".Length);
			}
			
			return name;
		}
		#endregion
		
		#region Win32 P/Invokes
		delegate int GetSecurityInfoNativeCall (SecurityInfos securityInfos,
							out IntPtr owner, out IntPtr group, out IntPtr dacl, out IntPtr sacl,
							out IntPtr descriptor);

		[DllImport ("advapi32.dll", CharSet=CharSet.Unicode, EntryPoint="GetSecurityInfo")]
		static extern int GetSecurityInfo (SafeHandle handle, ResourceType resourceType, SecurityInfos securityInfos,
						   out IntPtr owner, out IntPtr group, out IntPtr dacl, out IntPtr sacl,
						   out IntPtr descriptor);

		[DllImport ("advapi32.dll", CharSet=CharSet.Unicode, EntryPoint="GetNamedSecurityInfo")]
		static extern int GetNamedSecurityInfo (string name, ResourceType resourceType, SecurityInfos securityInfos,
							out IntPtr owner, out IntPtr group, out IntPtr dacl, out IntPtr sacl,
							out IntPtr descriptor);
							
		[DllImport ("kernel32.dll", EntryPoint="LocalFree")]
		static extern IntPtr LocalFree (IntPtr handle);

		delegate int SetSecurityInfoNativeCall (SecurityInfos securityInfos,
							byte[] owner, byte[] group, byte[] dacl, byte[] sacl);

		[DllImport ("advapi32.dll", CharSet=CharSet.Unicode, EntryPoint="SetSecurityInfo")]
		static extern int SetSecurityInfo (SafeHandle handle, ResourceType resourceType, SecurityInfos securityInfos,
						   byte[] owner, byte[] group, byte[] dacl, byte[] sacl);
						
		[DllImport ("advapi32.dll", CharSet=CharSet.Unicode, EntryPoint="SetNamedSecurityInfo")]
		static extern int SetNamedSecurityInfo (string name, ResourceType resourceType, SecurityInfos securityInfos,
							byte[] owner, byte[] group, byte[] dacl, byte[] sacl);

		[DllImport ("advapi32.dll", CharSet=CharSet.Unicode, EntryPoint="GetSecurityDescriptorLength")]
		static extern int GetSecurityDescriptorLength (IntPtr descriptor);
		
		[DllImport ("advapi32.dll", CharSet=CharSet.Unicode, EntryPoint="IsValidSecurityDescriptor")]
		[return: MarshalAs (UnmanagedType.Bool)]
		static extern bool IsValidSecurityDescriptor (IntPtr descriptor);
		
		struct SecurityDescriptor
		{
			public byte Revision, Size;
			public ushort ControlFlags;
			public IntPtr Owner, Group, Sacl, Dacl;
		}
		#endregion
	}
}

