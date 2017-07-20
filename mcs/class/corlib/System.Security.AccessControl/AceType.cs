//
// System.Security.AccessControl.AceType enum
//
// Author:
//	Dick Porter  <dick@ximian.com>
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

namespace System.Security.AccessControl {
	public enum AceType : byte {
		AccessAllowed = 0,
		AccessDenied = 1,
		SystemAudit = 2,
		SystemAlarm = 3,
		AccessAllowedCompound = 4,
		AccessAllowedObject = 5,
		AccessDeniedObject = 6,
		SystemAuditObject = 7,
		SystemAlarmObject = 8,
		AccessAllowedCallback = 9,
		AccessDeniedCallback = 10,
		AccessAllowedCallbackObject = 11,
		AccessDeniedCallbackObject = 12,
		SystemAuditCallback = 13,
		SystemAlarmCallback = 14,
		SystemAuditCallbackObject = 15,
		SystemAlarmCallbackObject = 16,
		MaxDefinedAceType = 16,
	}
}

