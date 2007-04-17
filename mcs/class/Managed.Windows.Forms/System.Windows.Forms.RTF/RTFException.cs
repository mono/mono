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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Bartok	(pbartok@novell.com)
//
//

// COMPLETE

using System;
using System.Text;

namespace System.Windows.Forms.RTF {

#if RTF_LIB
	public
#else
	internal
#endif
	class RTFException : ApplicationException {
		#region Local Variables
		private int		pos;
		private int		line;
		private TokenClass	token_class;
		private Major		major;
		private Minor		minor;
		private int		param;
		private string		text;
		private string		error_message;
		#endregion	// Local Variables

		#region Constructors
		public RTFException(RTF rtf, string error_message) {
			this.pos = rtf.LinePos;
			this.line = rtf.LineNumber;
			this.token_class = rtf.TokenClass;
			this.major = rtf.Major;
			this.minor = rtf.Minor;
			this.param = rtf.Param;
			this.text = rtf.Text;
			this.error_message = error_message;
		}
		#endregion	// Constructors

		#region Properties
		public override string Message {
			get {
				StringBuilder	sb;

				sb = new StringBuilder();
				sb.Append(error_message);
				sb.Append("\n");

				sb.Append("RTF Stream Info: Pos:" + pos + " Line:" + line);
				sb.Append("\n");

				sb.Append("TokenClass:" + token_class + ", ");
				sb.Append("Major:" + String.Format("{0}", (int)major) + ", ");
				sb.Append("Minor:" + String.Format("{0}", (int)minor) + ", ");
				sb.Append("Param:" + String.Format("{0}", param) + ", ");
				sb.Append("Text:" + text);

				return sb.ToString();
			}
		}
		#endregion	// Properties
	}
}
