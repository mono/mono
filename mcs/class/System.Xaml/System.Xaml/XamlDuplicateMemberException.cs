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
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace System.Xaml
{
#if !NET_2_1
	[Serializable]
#endif
	public class XamlDuplicateMemberException : XamlException
	{
		public XamlDuplicateMemberException ()
			: this ("Duplicate members are found in the type")
		{
		}

		public XamlDuplicateMemberException (XamlMember member, XamlType type)
			: this (String.Format ("duplicate member '{0}' in type '{1}'", member, type))
		{
			DuplicateMember = member;
			ParentType = type;
		}

		public XamlDuplicateMemberException (string message)
			: this (message, null)
		{
		}

		public XamlDuplicateMemberException (string message, Exception innerException)
			: base (message, innerException)
		{
		}

#if !NET_2_1
		protected XamlDuplicateMemberException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
			DuplicateMember = (XamlMember) info.GetValue ("member", typeof (XamlMember));
			ParentType = (XamlType) info.GetValue ("type", typeof (XamlType));
		}
#endif

		public XamlMember DuplicateMember { get; set; }
		public XamlType ParentType { get; set; }

#if !NET_2_1
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
			info.AddValue ("member", DuplicateMember);
			info.AddValue ("type", ParentType);
		}
#endif
	}
}
