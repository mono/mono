//
// assembly:	System
// namespace:	System.Text.RegularExpressions
// file:	regex.cs
//
// author:	Dan Lewis (dlewis@gmx.co.uk)
// 		(c) 2002
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

namespace System.Text.RegularExpressions {

	[Serializable]
	public class RegexCompilationInfo {

		public RegexCompilationInfo (string pattern, RegexOptions options, string name, string fullnamespace, bool ispublic)
		{
			Pattern = pattern;
			Options = options;
			Name = name;
			Namespace = fullnamespace;
			IsPublic = ispublic;
		}

		public bool IsPublic {
			get { return isPublic; }
			set { isPublic = value; }
		}

		public string Name {
			get { return name; }
			set {
				if (value == null)
					throw new ArgumentNullException ("Name");
				if (value.Length == 0)
					throw new ArgumentException ("Name");
				name = value;
			}
		}

		public string Namespace {
			get { return nspace; }
			set {
				if (value == null)
					throw new ArgumentNullException ("Namespace");
				nspace = value;
			}
		}

		public RegexOptions Options {
			get { return options; }
			set { options = value; }
		}

		public string Pattern {
			get { return pattern; }
			set {
				if (value == null)
					throw new ArgumentNullException ("pattern");
				pattern = value;
			}
		}

		// private

		private string pattern, name, nspace;
		private RegexOptions options;
		private bool isPublic;
	}
}
