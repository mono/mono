//
// System.Reflection/ParameterModifier.cs
//
// Author:
// Paolo Molaro (lupus@ximian.com)
//
// Copyright (c) 2001 Ximian, Inc

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

using System;
using System.Runtime.InteropServices;

namespace System.Reflection {
#if NET_2_0
	[ComVisible (true)]
#endif
	[Serializable]
	public struct ParameterModifier {
		private bool[] _byref;
		
		public ParameterModifier (int parameterCount) {
			if (parameterCount <= 0)
				throw new ArgumentException ("Must specify one or more parameters.");

			_byref = new bool[parameterCount];
		}

		public bool this [int index] {
			get {return _byref [index];} 
			set {_byref [index] = value;}
		}

	}

}
