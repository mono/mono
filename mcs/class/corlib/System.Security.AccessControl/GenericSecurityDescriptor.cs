//
// System.Security.AccessControl.GenericSecurityDescriptor implementation
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

using System.Globalization;
using System.Security.Principal;
using System.Text;

namespace System.Security.AccessControl {
	public abstract class GenericSecurityDescriptor {

		protected GenericSecurityDescriptor ()
		{
		}

		public int BinaryLength {
			get {
				int len = 0x14;
				if (Owner != null)
					len += Owner.BinaryLength;
				if (Group != null)
					len += Group.BinaryLength;
				if (DaclPresent && !DaclIsUnmodifiedAefa)
					len += InternalDacl.BinaryLength;
				if (SaclPresent)
					len += InternalSacl.BinaryLength;
				return len;
			}
		}

		public abstract ControlFlags ControlFlags { get; }

		public abstract SecurityIdentifier Group { get; set; }

		public abstract SecurityIdentifier Owner { get; set; }

		public static byte Revision {
			get { return 1; }
		}

		internal virtual GenericAcl InternalDacl {
			get { return null; }
		}

		internal virtual GenericAcl InternalSacl {
			get { return null; }
		}

		internal virtual byte InternalReservedField {
			get { return 0; }
		}

		public void GetBinaryForm (byte[] binaryForm, int offset)
		{
			if (null == binaryForm)
				throw new ArgumentNullException ("binaryForm");

			int binaryLength = BinaryLength;
			if (offset < 0 || offset > binaryForm.Length - binaryLength)
				throw new ArgumentOutOfRangeException ("offset");
			
			ControlFlags controlFlags = ControlFlags;
			if (DaclIsUnmodifiedAefa) { controlFlags &= ~ControlFlags.DiscretionaryAclPresent; }
			binaryForm[offset + 0x00] = Revision;
			binaryForm[offset + 0x01] = InternalReservedField;
			WriteUShort ((ushort)controlFlags, binaryForm,
			             offset + 0x02);
			
			// Skip 'offset' fields (will fill later)
			int pos = 0x14;
			
			if (Owner != null) {
				WriteInt (pos, binaryForm, offset + 0x04);
				Owner.GetBinaryForm (binaryForm, offset + pos);
				pos += Owner.BinaryLength;
			} else {
				WriteInt (0, binaryForm, offset + 0x04);
			}
			
			if (Group != null) {
				WriteInt (pos, binaryForm, offset + 0x08);
				Group.GetBinaryForm (binaryForm, offset + pos);
				pos += Group.BinaryLength;
			} else {
				WriteInt (0, binaryForm, offset + 0x08);
			}
			
			GenericAcl sysAcl = InternalSacl;
			if (SaclPresent) {
				WriteInt (pos, binaryForm, offset + 0x0C);
				sysAcl.GetBinaryForm (binaryForm, offset + pos);
				pos += InternalSacl.BinaryLength;
			} else {
				WriteInt (0, binaryForm, offset + 0x0C);
			}
			
			GenericAcl discAcl = InternalDacl;
			if (DaclPresent && !DaclIsUnmodifiedAefa) {
				WriteInt (pos, binaryForm, offset + 0x10);
				discAcl.GetBinaryForm (binaryForm, offset + pos);
				pos += InternalDacl.BinaryLength;
			} else {
				WriteInt (0, binaryForm, offset + 0x10);
			}
		}

		public string GetSddlForm (AccessControlSections includeSections)
		{
			StringBuilder result = new StringBuilder ();
			
			if ((includeSections & AccessControlSections.Owner) != 0
			    && Owner != null) {
				result.AppendFormat (
					CultureInfo.InvariantCulture,
					"O:{0}", Owner.GetSddlForm ());
			}
			
			if ((includeSections & AccessControlSections.Group) != 0
			    && Group != null) {
				result.AppendFormat (
					CultureInfo.InvariantCulture,
					"G:{0}", Group.GetSddlForm ());
			}
			
			if ((includeSections & AccessControlSections.Access) != 0
			    && DaclPresent && !DaclIsUnmodifiedAefa) {
				result.AppendFormat (
					CultureInfo.InvariantCulture,
					"D:{0}",
					InternalDacl.GetSddlForm (ControlFlags,
				                                  true));
			}
			
			if ((includeSections & AccessControlSections.Audit) != 0
			    && SaclPresent) {
				result.AppendFormat (
					CultureInfo.InvariantCulture,
					"S:{0}",
					InternalSacl.GetSddlForm (ControlFlags,
				                                  false));
			}
			
			return result.ToString ();
		}

		public static bool IsSddlConversionSupported ()
		{
			return true;
		}
		
		// See CommonSecurityDescriptor constructor regarding this persistence detail.
		internal virtual bool DaclIsUnmodifiedAefa {
			get { return false; }
		}
		
		bool DaclPresent {
			get {
				return InternalDacl != null
				    && (ControlFlags & ControlFlags.DiscretionaryAclPresent) != 0;
			}
		}
		
		bool SaclPresent {
			get {
				return InternalSacl != null
				    && (ControlFlags & ControlFlags.SystemAclPresent) != 0;
			}
		}
				
		void WriteUShort (ushort val, byte[] buffer, int offset)
		{
			buffer[offset] = (byte)val;
			buffer[offset + 1] = (byte)(val >> 8);
		}

		void WriteInt (int val, byte[] buffer, int offset)
		{
			buffer[offset] = (byte)val;
			buffer[offset + 1] = (byte)(val >> 8);
			buffer[offset + 2] = (byte)(val >> 16);
			buffer[offset + 3] = (byte)(val >> 24);
		}
		
	}
}

