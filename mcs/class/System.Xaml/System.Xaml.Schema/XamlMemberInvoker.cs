//
// Copyright (C) 2010 Novell Inc. http://novell.com
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
using System.Collections.Generic;
using System.Reflection;

namespace System.Xaml.Schema
{
	public class XamlMemberInvoker
	{
		static readonly XamlMemberInvoker unknown = new XamlMemberInvoker ();
		public static XamlMemberInvoker UnknownInvoker {
			get { return unknown; }
		}

		protected XamlMemberInvoker ()
		{
		}

		public XamlMemberInvoker (XamlMember member)
		{
			if (member == null)
				throw new ArgumentNullException ("member");
			this.member = member;
		}

		XamlMember member;

		public MethodInfo UnderlyingGetter {
			get { return member != null ? member.UnderlyingGetter : null; }
		}

		public MethodInfo UnderlyingSetter {
			get { return member != null ? member.UnderlyingSetter : null; }
		}

		void ThrowIfUnknown ()
		{
			if (member == null)
				throw new NotSupportedException ("Current operation is invalid for unknown member.");
		}

		public virtual object GetValue (object instance)
		{
			ThrowIfUnknown ();
			if (instance == null)
				throw new ArgumentNullException ("instance");
			if (member is XamlDirective)
				throw new NotSupportedException (String.Format ("not supported operation on directive member {0}", member));
			if (UnderlyingGetter == null)
				throw new NotSupportedException (String.Format ("Attempt to get value from write-only property or event {0}", member));
			return UnderlyingGetter.Invoke (instance, new object [0]);
		}
		public virtual void SetValue (object instance, object value)
		{
			ThrowIfUnknown ();
			if (instance == null)
				throw new ArgumentNullException ("instance");
			if (member is XamlDirective)
				throw new NotSupportedException (String.Format ("not supported operation on directive member {0}", member));
			if (UnderlyingSetter == null)
				throw new NotSupportedException (String.Format ("Attempt to set value from read-only property {0}", member));
			UnderlyingSetter.Invoke (instance, new object [] {value});
		}

		public virtual ShouldSerializeResult ShouldSerializeValue (object instance)
		{
			throw new NotImplementedException ();
		}
	}
}
