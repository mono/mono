//
// COMException.cs - COM Exception
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004, 2008 Novell, Inc (http://www.novell.com)
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

using System.Runtime.Serialization;

namespace System.Runtime.InteropServices {

[Serializable]
#if NET_2_0
[ComVisible (true)]
#endif
public class COMException : ExternalException {

	public COMException () 
		: base () {}

	public COMException (string message) 
		: base (message) {}

	public COMException (string message, Exception inner) 
		: base (message, inner) {}

	public COMException (string message, int errorCode) 
		: base (message, errorCode) {}

	protected COMException (SerializationInfo info, StreamingContext context) 
		: base (info, context) {}

	public override string ToString ()
	{
		return String.Format (
			"{0} (0x{1:x}): {2} {3}{4}{5}",
			GetType (), HResult, Message, InnerException == null ? String.Empty : InnerException.ToString (),
			Environment.NewLine, StackTrace != null ? StackTrace : String.Empty);
	}
} 

}
