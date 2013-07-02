//
// System.Runtime.InteropServices.ExtensibleClassFactory.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Jonathan Chambers (joncham@gmail.com)
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
// Copyright (C) 2006 Jonathan Chambers
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

using System.Diagnostics;
using System.Collections;
using System.Reflection;

namespace System.Runtime.InteropServices
{
	[ComVisible (true)]
	public sealed class ExtensibleClassFactory
	{
		static readonly Hashtable hashtable = new Hashtable ();

		private ExtensibleClassFactory ()
		{
		}

		internal static ObjectCreationDelegate GetObjectCreationCallback (Type t)
		{
			return hashtable[t] as ObjectCreationDelegate;
		}

		public static void RegisterObjectCreationCallback (ObjectCreationDelegate callback) {
			int i = 1;
			StackTrace trace = new StackTrace (false);
			while (i < trace.FrameCount) {
				StackFrame frame = trace.GetFrame (i);
				MethodBase m = frame.GetMethod ();
				if (m.MemberType == MemberTypes.Constructor && m.IsStatic) {
					hashtable.Add (m.DeclaringType, callback);
					return;
				}
				i++;
			}
			throw new System.InvalidOperationException (
				"RegisterObjectCreationCallback must be called from .cctor of class derived from ComImport type.");
		}
	}
}
