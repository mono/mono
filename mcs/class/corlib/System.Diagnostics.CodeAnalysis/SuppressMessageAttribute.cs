//
// System.Diagnostics.CodeAnalysis.SuppressMessageAttribute.cs
//
// Author: Zoltan Varga (vargaz@gmail.com)
//
// (C) Ximian, Inc. http://www.ximian.com
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

using System;
using System.Runtime.InteropServices;

namespace System.Diagnostics.CodeAnalysis
{
	[AttributeUsageAttribute(AttributeTargets.All, Inherited=false, AllowMultiple=true)] 
	[ConditionalAttribute("CODE_ANALYSIS")] 
	public sealed class SuppressMessageAttribute : Attribute
	{
		private String category;
		private String checkId;
		private String justification;
		private String messageId;
		private String scope;
		private String target;

		public SuppressMessageAttribute (String category, String checkId) {
			this.category = category;
			this.checkId = checkId;
		}

		public String Category {
			get {
				return category;
			}
		}

		public String CheckId {
			get {
				return checkId;
			}
		}

		public String Justification {
			get {
				return justification;
			}
			set {
				justification = value;
			}
		}

		public String MessageId {
			get {
				return messageId;
			}
			set {
				messageId = value;
			}
		}

		public String Scope {
			get {
				return scope;
			}
			set {
				scope = value;
			}
		}

		public String Target {
			get {
				return target;
			}
			set {
				target = value;
			}
		}
	}
}

#endif
