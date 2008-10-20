//
// TraceContextRecord.cs
//
// Author:
//	Daniel Nauck  <dna(at)mono-project(dot)de>
//
// Copyright (C) 2007 Daniel Nauck
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
using System;
using System.Security.Permissions;
using System.Text;
using System.Web;

namespace System.Web
{
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public sealed class TraceContextRecord
	{
		string category = null;
		Exception errorInfo = null;
		bool isWarning = false;
		string message = null;

		public TraceContextRecord (string category, string msg, bool isWarning, Exception errorInfo)
		{
			this.category = category;
			this.message = msg;
			this.isWarning = isWarning;
			this.errorInfo = errorInfo;
		}

		public string Category {
			get {
				return category;
			}
		}

		public Exception ErrorInfo {
			get {
				return errorInfo;
			}
		}

		public bool IsWarning {
			get {
				return isWarning;
			}
		}

		public string Message {
			get {
				return message;
			}
		}
	}
}
#endif
