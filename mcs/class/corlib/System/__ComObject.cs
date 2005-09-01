//
// System.__ComObject
//
// Authors:
//   Sebastien Pouliot <sebastien@ximian.com>
//   Kornél Pál <http://www.kornelpal.hu/>
//
// Copyright (C) 2004 Novell (http://www.novell.com)
// Copyright (C) 2005 Kornél Pál
//

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

namespace System
{
	// This is a private class that is used as a generic wrapper class
	// for COM objects that have no specific wrapper class.
	//
	// It has no public methods, it's functionality is exposed trough
	// System.Runtime.InteropServices.Marshal class and can be casted to
	// any interface that is implemented by the wrapped COM object.
	//
	// This class is referenced in .NET Framework SDK Documentation so
	// many times that obj.GetType().FullName == "System.__ComObject" and
	// Type.GetType("System.__ComObject") may be used.

	internal class __ComObject : MarshalByRefObject
	{
		private __ComObject ()
		{
		}
	}
}
