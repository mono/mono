//
// ProjectData.cs
//
// Author:
//   Martin Adoue (martin@cwanet.com)
//
// (C) 2002 Martin Adoue
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

using System;

namespace System.ComponentModel
{

	/// <summary>
	/// Specifies the browsable state of a property or method from within an editor.
	/// </summary>
	public enum EditorBrowsableState 
	{
		/// <summary>
		/// The property or method is always browsable from within an editor.
		/// </summary>
		Always = 0,
		/// <summary>
		/// The property or method is never browsable from within an editor.
		/// </summary>
		Never = 1,
		/// <summary>
		/// The property or method is a feature that only advanced users should see. An editor can either show or hide such properties.
		/// </summary>
		Advanced = 2
	}

}
