//
// System.Security.AccessControl.QualifiedAce implementation
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

namespace System.Security.AccessControl {
	public abstract class QualifiedAce : KnownAce
	{
		private byte [] opaque;
		
		internal QualifiedAce (AceType type, AceFlags flags,
		                       byte[] opaque)
			: base (type, flags)
		{
			SetOpaque (opaque);
		}
		
		internal QualifiedAce (byte[] binaryForm, int offset)
			: base(binaryForm, offset)
		{
		}

		public AceQualifier AceQualifier {
			get {
				switch(AceType)
				{
				case AceType.AccessAllowed:
				case AceType.AccessAllowedCallback:
				case AceType.AccessAllowedCallbackObject:
				case AceType.AccessAllowedCompound:
				case AceType.AccessAllowedObject:
					return AceQualifier.AccessAllowed;
				
				case AceType.AccessDenied:
				case AceType.AccessDeniedCallback:
				case AceType.AccessDeniedCallbackObject:
				case AceType.AccessDeniedObject:
					return AceQualifier.AccessDenied;
					
				case AceType.SystemAlarm:
				case AceType.SystemAlarmCallback:
				case AceType.SystemAlarmCallbackObject:
				case AceType.SystemAlarmObject:
					return AceQualifier.SystemAlarm;
					
				case AceType.SystemAudit:
				case AceType.SystemAuditCallback:
				case AceType.SystemAuditCallbackObject:
				case AceType.SystemAuditObject:
					return AceQualifier.SystemAudit;
					
				default:
					throw new ArgumentException("Unrecognised ACE type: " + AceType);
				}
			}
		}
		
		public bool IsCallback {
			get {
				return AceType == AceType.AccessAllowedCallback
					|| AceType == AceType.AccessAllowedCallbackObject
					|| AceType == AceType.AccessDeniedCallback
					|| AceType == AceType.AccessDeniedCallbackObject
					|| AceType == AceType.SystemAlarmCallback
					|| AceType == AceType.SystemAlarmCallbackObject
					|| AceType == AceType.SystemAuditCallback
					|| AceType == AceType.SystemAuditCallbackObject;
			}
		}
		
		public int OpaqueLength {
			get {
				if (opaque == null)
					return  0;
				return opaque.Length;
			}
		}
		
		public byte[] GetOpaque ()
		{
			if (opaque == null)
				return null;
			return (byte []) opaque.Clone();
		}
		
		public void SetOpaque (byte[] opaque)
		{
			if (opaque == null)
				this.opaque = null;
			else
				this.opaque = (byte []) opaque.Clone();
		}
	}
}

