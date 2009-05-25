//
// PatternToken.cs
//
// Author:
//	Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2009 Novell Inc. http://novell.com
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
using System.Text;
using System.Web;

namespace System.Web.Routing
{
	sealed class PatternToken
	{
		public PatternTokenType Type {
			get;
			private set;
		}

		public string Name {
			get;
			private set;
		}

		public PatternToken (PatternTokenType type, string name)
		{
			this.Type = type;
			this.Name = name;
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();

			switch (Type) {
				case PatternTokenType.Standard:
					sb.Append ("PatternToken_Standard");
					break;

				case PatternTokenType.Literal:
					sb.Append ("PatternToken_Literal");
					break;

				case PatternTokenType.CatchAll:
					sb.Append ("PatternToken_CatchAll");
					break;

				default:
					sb.Append ("PatternToken_UNKNOWN");
					break;
			}

			sb.AppendFormat (" [Name = '{0}']", Name);

			return sb.ToString ();
		}
	}
}
