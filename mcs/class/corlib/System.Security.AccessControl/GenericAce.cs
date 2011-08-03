//
// System.Security.AccessControl.GenericAce implementation
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
using System.Text;

namespace System.Security.AccessControl {
	public abstract class GenericAce
	{
		private AceFlags ace_flags;
		private AceType ace_type;
		
		internal GenericAce (AceType type, AceFlags flags)
		{
			if (type > AceType.MaxDefinedAceType) {
				throw new ArgumentOutOfRangeException ("type");
			}
			
			this.ace_type = type;
			this.ace_flags = flags;
		}
		
		internal GenericAce(byte[] binaryForm, int offset)
		{
			if (binaryForm == null)
				throw new ArgumentNullException("binaryForm");
			
			if (offset < 0 || offset > binaryForm.Length - 2)
				throw new ArgumentOutOfRangeException("offset", offset, "Offset out of range");
			
			ace_type = (AceType)binaryForm[offset];
			ace_flags = (AceFlags)binaryForm[offset + 1];
		}
		
		public AceFlags AceFlags {
			get { return ace_flags; }
			set { ace_flags = value; }
		}
		
		public AceType AceType {
			get { return ace_type; }
		}
		
		public AuditFlags AuditFlags {
			get {
				AuditFlags ret = AuditFlags.None;
				if ((ace_flags & AceFlags.SuccessfulAccess) != 0)
					ret |= AuditFlags.Success;
				if ((ace_flags & AceFlags.FailedAccess) != 0)
					ret |= AuditFlags.Failure;
				return ret;
			}
		}
		
		public abstract int BinaryLength { get; }

		public InheritanceFlags InheritanceFlags {
			get {
				InheritanceFlags ret = InheritanceFlags.None;
				if ((ace_flags & AceFlags.ObjectInherit) != 0)
					ret |= InheritanceFlags.ObjectInherit;
				if ((ace_flags & AceFlags.ContainerInherit) != 0)
					ret |= InheritanceFlags.ContainerInherit;
				return ret;
			}
		}

		public bool IsInherited {
			get { return (ace_flags & AceFlags.Inherited) != AceFlags.None; }
		}

		public PropagationFlags PropagationFlags {
			get {
				PropagationFlags ret = PropagationFlags.None;
				if ((ace_flags & AceFlags.InheritOnly) != 0)
					ret |= PropagationFlags.InheritOnly;
				if ((ace_flags & AceFlags.NoPropagateInherit) != 0)
					ret |= PropagationFlags.NoPropagateInherit;
				return ret;
			}
		}
		
		public GenericAce Copy ()
		{
			byte[] buffer = new byte[BinaryLength];
			GetBinaryForm(buffer, 0);
			return GenericAce.CreateFromBinaryForm(buffer, 0);
		}
		
		public static GenericAce CreateFromBinaryForm (byte[] binaryForm, int offset)
		{
			if (binaryForm == null)
				throw new ArgumentNullException("binaryForm");
			
			if (offset < 0 || offset > binaryForm.Length - 1)
				throw new ArgumentOutOfRangeException("offset", offset, "Offset out of range");
			
			AceType type = (AceType)binaryForm[offset];
			if (IsObjectType(type))
				return new ObjectAce(binaryForm, offset);
			else
				return new CommonAce(binaryForm, offset);
		}

		public override sealed bool Equals (object o)
		{
			return this == (o as GenericAce);
		}

		public abstract void GetBinaryForm (byte[] binaryForm, int offset);

		public override sealed int GetHashCode ()
		{
			byte[] buffer = new byte[BinaryLength];
			GetBinaryForm(buffer, 0);
			
			int code = 0;
			for(int i = 0; i < buffer.Length; ++i)
			{
				code = (code << 3) | ((code >> 29) & 0x7);
				code ^= ((int)buffer[i]) & 0xff;
			}
			
			return code;
		}

		public static bool operator== (GenericAce left, GenericAce right)
		{
			if(((object)left) == null)
				return((object)right) == null;
			
			if(((object)right) == null)
				return false;
			
			int leftLen = left.BinaryLength;
			int rightLen = right.BinaryLength;
			if( leftLen != rightLen)
				return false;
			
			byte[] leftBuffer = new byte[leftLen];
			byte[] rightBuffer = new byte[rightLen];
			left.GetBinaryForm(leftBuffer, 0);
			right.GetBinaryForm(rightBuffer, 0);
			
			for(int i = 0; i < leftLen; ++i) {
				if(leftBuffer[i] != rightBuffer[i])
					return false;
			}
			
			return true;
		}

		public static bool operator!= (GenericAce left, GenericAce right)
		{
			if(((object)left) == null)
				return((object)right) != null;
			
			if(((object)right) == null)
				return true;
			
			int leftLen = left.BinaryLength;
			int rightLen = right.BinaryLength;
			if( leftLen != rightLen)
				return true;
			
			byte[] leftBuffer = new byte[leftLen];
			byte[] rightBuffer = new byte[rightLen];
			left.GetBinaryForm(leftBuffer, 0);
			right.GetBinaryForm(rightBuffer, 0);
			
			for(int i = 0; i < leftLen; ++i) {
				if(leftBuffer[i] != rightBuffer[i])
					return true;
			}
			
			return false;
		}
		
		internal abstract string GetSddlForm();
		
		static internal GenericAce CreateFromSddlForm (string sddlForm, ref int pos)
		{
			if (sddlForm[pos] != '(')
				throw new ArgumentException ("Invalid SDDL string.", "sddlForm");
			
			int endPos = sddlForm.IndexOf (')', pos);
			if (endPos < 0)
				throw new ArgumentException ("Invalid SDDL string.", "sddlForm");
			
			int count = endPos - (pos + 1);
			string elementsStr = sddlForm.Substring (pos + 1,
			                                         count);
			elementsStr = elementsStr.ToUpperInvariant ();
			string[] elements = elementsStr.Split (';');
			if (elements.Length != 6)
				throw new ArgumentException ("Invalid SDDL string.", "sddlForm");
			

			ObjectAceFlags objFlags = ObjectAceFlags.None;
				
			AceType type = ParseSddlAceType (elements[0]);

			AceFlags flags = ParseSddlAceFlags (elements[1]);

			int accessMask = ParseSddlAccessRights (elements[2]);

			Guid objectType = Guid.Empty;
			if (!string.IsNullOrEmpty (elements[3])) {
				objectType = new Guid(elements[3]);
				objFlags |= ObjectAceFlags.ObjectAceTypePresent;
			}
			
			Guid inhObjectType = Guid.Empty;
			if (!string.IsNullOrEmpty (elements[4])) {
				inhObjectType = new Guid(elements[4]);
				objFlags |= ObjectAceFlags.InheritedObjectAceTypePresent;
			}
			
			SecurityIdentifier sid
				= new SecurityIdentifier (elements[5]);
			
			if (type == AceType.AccessAllowedCallback
			    || type == AceType.AccessDeniedCallback)
				throw new NotImplementedException ("Conditional ACEs not supported");
			
			pos = endPos + 1;
			
			if (IsObjectType(type))
				return new ObjectAce(type, flags, accessMask, sid, objFlags, objectType, inhObjectType, null);
			else {
				if (objFlags != ObjectAceFlags.None)
					throw new ArgumentException( "Invalid SDDL string.", "sddlForm");
				return new CommonAce (type, flags, accessMask, sid, null);
			}
		}
		
		private static bool IsObjectType(AceType type)
		{
			return type == AceType.AccessAllowedCallbackObject
				|| type == AceType.AccessAllowedObject
				|| type == AceType.AccessDeniedCallbackObject
				|| type == AceType.AccessDeniedObject
				|| type == AceType.SystemAlarmCallbackObject
				|| type == AceType.SystemAlarmObject
				|| type == AceType.SystemAuditCallbackObject
				|| type == AceType.SystemAuditObject;
		}

		protected static string GetSddlAceType (AceType type)
		{
			switch (type) {
			case AceType.AccessAllowed:
				return "A";
			case AceType.AccessDenied:
				return "D";
			case AceType.AccessAllowedObject:
				return "OA";
			case AceType.AccessDeniedObject:
				return "OD";
			case AceType.SystemAudit:
				return "AU";
			case AceType.SystemAlarm:
				return "AL";
			case AceType.SystemAuditObject:
				return "OU";
			case AceType.SystemAlarmObject:
				return "OL";
			case AceType.AccessAllowedCallback:
				return "XA";
			case AceType.AccessDeniedCallback:
				return "XD";
			default:
				throw new ArgumentException ("Unable to convert to SDDL ACE type: " + type, "type");
			}
		}

		private static AceType ParseSddlAceType (string type)
		{
			switch (type) {
			case "A":
				return AceType.AccessAllowed;
			case "D":
				return AceType.AccessDenied;
			case "OA":
				return AceType.AccessAllowedObject;
			case "OD":
				return AceType.AccessDeniedObject;
			case "AU":
				return AceType.SystemAudit;
			case "AL":
				return AceType.SystemAlarm;
			case "OU":
				return AceType.SystemAuditObject;
			case "OL":
				return AceType.SystemAlarmObject;
			case "XA":
				return AceType.AccessAllowedCallback;
			case "XD":
				return AceType.AccessDeniedCallback;
			default:
				throw new ArgumentException ("Unable to convert SDDL to ACE type: " + type, "type");
			}
		}

		protected static string GetSddlAceFlags (AceFlags flags)
		{
			StringBuilder result = new StringBuilder ();
			if ((flags & AceFlags.ObjectInherit) != 0)
				result.Append ("OI");
			if ((flags & AceFlags.ContainerInherit) != 0)
				result.Append ("CI");
			if ((flags & AceFlags.NoPropagateInherit) != 0)
				result.Append ("NP");
			if ((flags & AceFlags.InheritOnly) != 0)
				result.Append ("IO");
			if ((flags & AceFlags.Inherited) != 0)
				result.Append ("ID");
			if ((flags & AceFlags.SuccessfulAccess) != 0)
				result.Append ("SA");
			if ((flags & AceFlags.FailedAccess) != 0)
				result.Append ("FA");
			return result.ToString ();
		}

		private static AceFlags ParseSddlAceFlags (string flags)
		{
			AceFlags ret = AceFlags.None;
			
			int pos = 0;
			while (pos < flags.Length - 1) {
				string flag = flags.Substring (pos, 2);
				switch (flag) {
				case "CI":
					ret |= AceFlags.ContainerInherit;
					break;
				case "OI":
					ret |= AceFlags.ObjectInherit;
					break;
				case "NP":
					ret |= AceFlags.NoPropagateInherit;
					break;
				case "IO":
					ret |= AceFlags.InheritOnly;
					break;
				case "ID":
					ret |= AceFlags.Inherited;
					break;
				case "SA":
					ret |= AceFlags.SuccessfulAccess;
					break;
				case "FA":
					ret |= AceFlags.FailedAccess;
					break;
				default:
					throw new ArgumentException ("Invalid SDDL string.", "flags");
				}
				
				pos += 2;
			}
			
			if (pos != flags.Length)
				throw new ArgumentException ("Invalid SDDL string.", "flags");
			
			return ret;
		}

		private static int ParseSddlAccessRights (string accessMask)
		{
			if (accessMask.StartsWith ("0X")) {
				return int.Parse (accessMask.Substring (2),
				                  NumberStyles.HexNumber,
				                  CultureInfo.InvariantCulture);
			} else if (Char.IsDigit (accessMask, 0)) {
				return int.Parse (accessMask,
				                  NumberStyles.Integer,
				                  CultureInfo.InvariantCulture);
			} else {
				return ParseSddlAliasRights (accessMask);
			}
		}
		
		private static int ParseSddlAliasRights(string accessMask)
		{
			int ret = 0;
			
			int pos = 0;
			while (pos < accessMask.Length - 1) {
				string flag = accessMask.Substring (pos, 2);
				SddlAccessRight right = SddlAccessRight.LookupByName(flag);
				if (right == null)
					throw new ArgumentException ("Invalid SDDL string.", "accessMask");
				
				ret |= right.Value;
				pos += 2;
			}
			
			if (pos != accessMask.Length)
				throw new ArgumentException ("Invalid SDDL string.", "accessMask");
			
			return ret;
		}
		
		internal static ushort ReadUShort (byte[] buffer, int offset)
		{
			return (ushort)((((int)buffer[offset + 0]) << 0)
			                | (((int)buffer[offset + 1]) << 8));
		}
		
		internal static int ReadInt (byte[] buffer, int offset)
		{
			return (((int)buffer[offset + 0]) << 0)
				| (((int)buffer[offset + 1]) << 8)
				| (((int)buffer[offset + 2]) << 16)
				| (((int)buffer[offset + 3]) << 24);
		}
		
		internal static void WriteInt (int val, byte[] buffer, int offset)
		{
			buffer[offset] = (byte)val;
			buffer[offset + 1] = (byte)(val >> 8);
			buffer[offset + 2] = (byte)(val >> 16);
			buffer[offset + 3] = (byte)(val >> 24);
		}

		internal static void WriteUShort (ushort val, byte[] buffer,
		                                  int offset)
		{
			buffer[offset] = (byte)val;
			buffer[offset + 1] = (byte)(val >> 8);
		}
	}
}

