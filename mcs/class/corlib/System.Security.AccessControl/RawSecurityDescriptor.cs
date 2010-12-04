//
// System.Security.AccessControl.RawSecurityDescriptor implementation
//
// Author:
//	Dick Porter  <dick@ximian.com>
//	Kenneth Bell
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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


using System.Security.Principal;

namespace System.Security.AccessControl {
	public sealed class RawSecurityDescriptor : GenericSecurityDescriptor {
		private ControlFlags control_flags;
		private SecurityIdentifier owner_sid;
		private SecurityIdentifier group_sid;
		private RawAcl system_acl;
		private RawAcl discretionary_acl;
		private byte resourcemgr_control;

		public RawSecurityDescriptor (string sddlForm)
		{
			if (sddlForm == null)
				throw new ArgumentNullException ("sddlForm");
			
			ParseSddl (sddlForm.Replace (" ", ""));
			
			control_flags |= ControlFlags.SelfRelative;
		}

		public RawSecurityDescriptor (byte[] binaryForm, int offset)
		{
			if (binaryForm == null)
				throw new ArgumentNullException("binaryForm");
			
			if (offset < 0 || offset > binaryForm.Length - 0x14)
				throw new ArgumentOutOfRangeException("offset", offset, "Offset out of range");
			
			if (binaryForm[offset] != 1)
				throw new ArgumentException("Unrecognized Security Descriptor revision.", "binaryForm");
			
			resourcemgr_control = binaryForm[offset + 0x01];
			control_flags = (ControlFlags)ReadUShort(binaryForm, offset + 0x02);
			
			int ownerPos = ReadInt(binaryForm, offset + 0x04);
			int groupPos = ReadInt(binaryForm, offset + 0x08);
			int saclPos = ReadInt(binaryForm, offset + 0x0C);
			int daclPos = ReadInt(binaryForm, offset + 0x10);
			
			if (ownerPos != 0)
				owner_sid = new SecurityIdentifier(binaryForm, ownerPos);
			
			if (groupPos != 0)
				group_sid = new SecurityIdentifier(binaryForm, groupPos);
			
			if (saclPos != 0)
				system_acl = new RawAcl(binaryForm, saclPos);
			
			if (daclPos != 0)
				discretionary_acl = new RawAcl(binaryForm, daclPos);
		}

		public RawSecurityDescriptor (ControlFlags flags,
		                              SecurityIdentifier owner,
		                              SecurityIdentifier @group,
		                              RawAcl systemAcl,
		                              RawAcl discretionaryAcl)
		{
			control_flags = flags;
			owner_sid = owner;
			group_sid = @group;
			system_acl = systemAcl;
			discretionary_acl = discretionaryAcl;
		}

		public override ControlFlags ControlFlags {
			get { return control_flags; }
		}

		public RawAcl DiscretionaryAcl {
			get { return discretionary_acl; }
			set { discretionary_acl = value; }
		}

		public override SecurityIdentifier Group {
			get { return group_sid; }
			set { group_sid = value; }
		}

		public override SecurityIdentifier Owner {
			get { return owner_sid; }
			set { owner_sid = value; }
		}

		public byte ResourceManagerControl {
			get { return resourcemgr_control; }
			set { resourcemgr_control = value; }
		}

		public RawAcl SystemAcl {
			get { return system_acl; }
			set { system_acl = value; }
		}

		public void SetFlags (ControlFlags flags)
		{
			control_flags = flags | ControlFlags.SelfRelative;
		}

		internal override GenericAcl InternalDacl {
			get { return this.DiscretionaryAcl; }
		}

		internal override GenericAcl InternalSacl {
			get { return this.SystemAcl; }
		}

		internal override byte InternalReservedField {
			get { return this.ResourceManagerControl; }
		}

		private void ParseSddl (string sddlForm)
		{
			ControlFlags flags = ControlFlags.None;
			
			int pos = 0;
			while (pos < sddlForm.Length - 2) {
				switch (sddlForm.Substring (pos, 2)) {
				case "O:":
					pos += 2;
					Owner = SecurityIdentifier.ParseSddlForm (sddlForm, ref pos);
					break;
				
				case "G:":
					pos += 2;
					Group = SecurityIdentifier.ParseSddlForm (sddlForm, ref pos);
					break;
				
				case "D:":
					pos += 2;
					DiscretionaryAcl = RawAcl.ParseSddlForm (sddlForm, true, ref flags, ref pos);
					flags |= ControlFlags.DiscretionaryAclPresent;
					break;
				
				case "S:":
					pos += 2;
					SystemAcl = RawAcl.ParseSddlForm (sddlForm, false, ref flags, ref pos);
					flags |= ControlFlags.SystemAclPresent;
					break;
				default:
					
					throw new ArgumentException ("Invalid SDDL.", "sddlForm");
				}
			}
			
			if (pos != sddlForm.Length) {
				throw new ArgumentException ("Invalid SDDL.", "sddlForm");
			}
			
			SetFlags (flags);
		}
		
		private ushort ReadUShort (byte[] buffer, int offset)
		{
			return (ushort)((((int)buffer[offset + 0]) << 0)
			                | (((int)buffer[offset + 1]) << 8));
		}
		
		private int ReadInt (byte[] buffer, int offset)
		{
			return (((int)buffer[offset + 0]) << 0)
				| (((int)buffer[offset + 1]) << 8)
				| (((int)buffer[offset + 2]) << 16)
				| (((int)buffer[offset + 3]) << 24);
		}
	}
}

