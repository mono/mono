//
// System.ObsoleteAttribute.cs
//
// Author:
//   Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Runtime.InteropServices;

namespace System
{
	[AttributeUsage (AttributeTargets.Class | AttributeTargets.Struct |
		AttributeTargets.Enum | AttributeTargets.Constructor |
		AttributeTargets.Method | AttributeTargets.Property |
		AttributeTargets.Field | AttributeTargets.Event |
		AttributeTargets.Interface | AttributeTargets.Delegate,
		Inherited=false)]
	[Serializable]
#if NET_2_0
	[ComVisible (true)]
#endif
	public sealed class ObsoleteAttribute : Attribute
	{
		private string _message;
		private bool _error;

		//	 Constructors
		public ObsoleteAttribute ()
			: base ()
		{
		}

		public ObsoleteAttribute (string message)
		{
			_message = message;
		}

		public ObsoleteAttribute (string message, bool error)
		{
			_message = message;
			_error = error;
		}

		// Properties
		public string Message {
			get { return _message; }
		}

		public bool IsError {
			get { return _error; }
		}
	}
}
