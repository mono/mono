// 
// System.Web.Services.Description.BasicProfileViolation.cs
//
// Author:
//   Lluis Sanchez (lluis@novell.com)
//
// Copyright (C) Novell, Inc., 2004
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

#if NET_2_0

using System.Collections.Specialized;
using System.Web.Services;

namespace System.Web.Services.Description 
{
	public class BasicProfileViolation
	{
		WsiProfiles _claims;
		StringCollection _elements;
		ConformanceRule _rule;
		
		internal BasicProfileViolation (WsiProfiles claims, ConformanceRule rule)
		{
			_claims = claims;
			_rule = rule;
			_elements = new StringCollection ();
		}
		
		public WsiProfiles Claims {
			get { return _claims; }
		}
		
		public string Details {
			get { return _rule.Details; }
		}
		
		public StringCollection Elements {
			get { return _elements; }
		}
		
		public string NormativeStatement {
			get { return _rule.NormativeStatement ; }
		}

		public string Recommendation {
			get { return _rule.Recommendation; }
		}
		
		public override string ToString ()
		{
			string res = NormativeStatement + ": " + Details;
			foreach (string s in Elements)
				res += "\n  -  " + s;
			return res;
		}
	}
}

#endif
