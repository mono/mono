//
// System.Security.AccessControl.CommonAce implementation
//
// Authors:
//	Dick Porter  <dick@ximian.com>
//	Atsushi Enomoto  <atsushi@ximian.com>
//	Kenneth Bell
//
// Copyright (C) 2006-2007 Novell, Inc (http://www.novell.com)
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

namespace System.Security.AccessControl {
	public sealed class CommonAce : QualifiedAce {
		public CommonAce (AceFlags flags, AceQualifier qualifier,
		                  int accessMask, SecurityIdentifier sid,
		                  bool isCallback, byte[] opaque)
			: base(ConvertType (qualifier, isCallback),
			       flags,
			       opaque)
		{
			AccessMask = accessMask;
			SecurityIdentifier = sid;
		}

		internal CommonAce (AceType type, AceFlags flags, int accessMask,
		                    SecurityIdentifier sid, byte[] opaque)
			: base(type, flags, opaque)
		{
			AccessMask = accessMask;
			SecurityIdentifier = sid;
		}

		internal CommonAce(byte[] binaryForm, int offset)
			: base(binaryForm, offset)
		{
			int len = ReadUShort(binaryForm, offset + 2);
			if (offset > binaryForm.Length - len)
				throw new ArgumentException("Invalid ACE - truncated", "binaryForm");
			if (len < 8 + SecurityIdentifier.MinBinaryLength)
				throw new ArgumentException("Invalid ACE", "binaryForm");
			
			AccessMask = ReadInt(binaryForm, offset + 4);
			SecurityIdentifier = new SecurityIdentifier(binaryForm,
			                                            offset + 8);
			
			int opaqueLen = len - (8 + SecurityIdentifier.BinaryLength);
			if (opaqueLen > 0) {
				byte[] opaque = new byte[opaqueLen];
				Array.Copy(binaryForm,
				           offset + 8 + SecurityIdentifier.BinaryLength,
				           opaque, 0, opaqueLen);
				SetOpaque (opaque);
			}
		}

		public override int BinaryLength {
			get {
				return 8 + SecurityIdentifier.BinaryLength
					+ OpaqueLength;
			}
		}

		public override void GetBinaryForm (byte[] binaryForm,
		                                    int offset)
		{
			int len = BinaryLength;
			binaryForm[offset] = (byte)this.AceType;
			binaryForm[offset + 1] = (byte)this.AceFlags;
			WriteUShort ((ushort)len, binaryForm, offset + 2);
			WriteInt (AccessMask, binaryForm, offset + 4);
			
			SecurityIdentifier.GetBinaryForm (binaryForm,
			                                  offset + 8);
			
			byte[] opaque = GetOpaque ();
			if (opaque != null)
				Array.Copy (opaque, 0, binaryForm,
				            offset + 8 + SecurityIdentifier.BinaryLength,
				            opaque.Length);
		}

		public static int MaxOpaqueLength (bool isCallback)
		{
			// Varies by platform?
			return 65459;
		}

		internal override string GetSddlForm ()
		{
			if (OpaqueLength != 0)
				throw new NotImplementedException (
					"Unable to convert conditional ACEs to SDDL");
			
			return string.Format (CultureInfo.InvariantCulture,
			                      "({0};{1};{2};;;{3})",
			                      GetSddlAceType (AceType),
			                      GetSddlAceFlags (AceFlags),
			                      GetSddlAccessRights (AccessMask),
			                      SecurityIdentifier.GetSddlForm ());
		}

		private static AceType ConvertType (AceQualifier qualifier,
		                                    bool isCallback)
		{
			switch (qualifier) {
			case AceQualifier.AccessAllowed:
				if (isCallback)
					return AceType.AccessAllowedCallback;
				else
					return AceType.AccessAllowed;
				
			case AceQualifier.AccessDenied:
				if (isCallback)
					return AceType.AccessDeniedCallback;
				else
					return AceType.AccessDenied;
				
			case AceQualifier.SystemAlarm:
				if (isCallback)
					return AceType.SystemAlarmCallback;
				else
					return AceType.SystemAlarm;
				
			case AceQualifier.SystemAudit:
				if (isCallback)
					return AceType.SystemAuditCallback;
				else
					return AceType.SystemAudit;
				
			default:
				throw new ArgumentException ("Unrecognized ACE qualifier: " + qualifier, "qualifier");
			}
		}
	}
}
