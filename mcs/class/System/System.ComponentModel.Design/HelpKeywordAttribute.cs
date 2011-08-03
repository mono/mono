//
// System.ComponentModel.Design.HelpKeywordAttribute.cs
//
// Author:
//	Miguel de Icaza  <miguel@ximian.com>
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
using System;

namespace System.ComponentModel.Design {

	[AttributeUsage (AttributeTargets.All, AllowMultiple = false, Inherited = false)]	
	[Serializable]
	public sealed class HelpKeywordAttribute : Attribute {

		public static readonly HelpKeywordAttribute Default = null;

		string contextKeyword;
		
		public HelpKeywordAttribute ()
		{
		}

		public HelpKeywordAttribute (string keyword)
		{
			this.contextKeyword = keyword;
		}

		public HelpKeywordAttribute (Type t)
		{
			if (t == null)
				throw new ArgumentNullException ("t");
			this.contextKeyword = t.FullName;
		}

		public override bool Equals (object other)
		{
			if (other == null)
				return false;

			HelpKeywordAttribute o = other as HelpKeywordAttribute;
			if (o == null)
				return false;
			
			return (o.contextKeyword == contextKeyword);
		}

		public override int GetHashCode ()
		{
			return contextKeyword != null ? contextKeyword.GetHashCode () : 0;
		}

		public override bool IsDefaultAttribute ()
		{
			return contextKeyword == null;
		}

		public string HelpKeyword {
			get {
				return contextKeyword;
			}
		}
	}
}
