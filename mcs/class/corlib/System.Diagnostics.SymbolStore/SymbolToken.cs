//
// System.Diagnostics.SymbolStore.SymbolToken.cs
//
// Authors:
//   Duco Fijma (duco@lorentz.xs4all.nl)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (c) 2002 Duco Fijma
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
#endif

namespace System.Diagnostics.SymbolStore
{
#if NET_2_0
	[ComVisible (true)]
#endif
	public struct SymbolToken 
	{

		private int _val;

		public SymbolToken (int val)
		{
			_val = val;
		}

		public override bool Equals (object obj) 
		{
			if (!(obj is SymbolToken))
				return false;
			return ((SymbolToken) obj).GetToken() == _val;
		}

#if NET_2_0
		public bool Equals (SymbolToken obj)
		{
			return(obj.GetToken () == _val);
		}
		

		public static bool operator == (SymbolToken a, SymbolToken b)
		{
			return a.Equals (b);
		}

		public static bool operator != (SymbolToken a, SymbolToken b)
		{
			return !a.Equals (b);
		}
#endif

		public override int GetHashCode()
		{
			return _val.GetHashCode(); 
		}

		public int GetToken()
		{
			return _val; 
		}
	}
}
