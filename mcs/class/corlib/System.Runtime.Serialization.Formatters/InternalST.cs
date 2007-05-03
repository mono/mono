//
// System.Runtime.Serialization.Formatters.InternalST.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Andreas Nahr
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

#if NET_2_0
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Reflection;
#endif

namespace System.Runtime.Serialization.Formatters
{
	//LAMESPEC: Use of this class??
#if NET_2_0
	[ComVisible (true)]
#endif
	public sealed class InternalST
	{
		private InternalST ()
		{
		}

#if NET_2_0
		[Conditional ("_LOGGING")]
		public static void InfoSoap (params object[] messages)
		{
			throw new NotImplementedException ();
		}
		
		public static Assembly LoadAssemblyFromString (string assemblyString)
		{
			throw new NotImplementedException ();
		}
		
		public static void SerializationSetValue (FieldInfo fi,
							  object target,
							  object value)
		{
			throw new NotImplementedException ();
		}
		
		[Conditional ("SER_LOGGING")]
		public static void Soap (params object[] messages)
		{
			throw new NotImplementedException ();
		}
		
		[Conditional ("_DEBUG")]
		public static void SoapAssert (bool condition, string message)
		{
			throw new NotImplementedException ();
		}
		
		public static bool SoapCheckEnabled ()
		{
			throw new NotImplementedException ();
		}
#endif
	}
}
