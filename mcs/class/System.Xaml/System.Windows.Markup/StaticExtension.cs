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
using System.ComponentModel;
using System.Reflection;
using System.Xaml.Schema;

namespace System.Windows.Markup
{
	[MarkupExtensionReturnType (typeof (object))]
	[TypeConverter (typeof (StaticExtensionConverter))]
#if !NET_2_1
	[System.Runtime.CompilerServices.TypeForwardedFrom (Consts.AssemblyPresentationFramework_3_5)]
#endif
	public class StaticExtension : MarkupExtension
	{
		public StaticExtension ()
		{
		}

		public StaticExtension (string member)
		{
			Member = member;
		}

		[ConstructorArgument ("member")]
		public string Member { get; set; }

		[DefaultValue (null)]
		public Type MemberType { get; set; }

		public override object ProvideValue (IServiceProvider serviceProvider)
		{
			if (Member == null)
				throw new InvalidOperationException ("Member property must be set to StaticExtension before calling ProvideValue method.");
			if (MemberType != null) {
				var pi = MemberType.GetProperty (Member, BindingFlags.Public | BindingFlags.Static);
				if (pi != null)
					return pi.GetValue (null, null);
				var fi = MemberType.GetField (Member, BindingFlags.Public | BindingFlags.Static);
				if (fi != null)
					return fi.GetValue (null);
			}
			// there might be some cases that it could still
			// resolve a static member without MemberType, 
			// but we don't know any of such so far.
			throw new ArgumentException (String.Format ("Member '{0}' could not be resolved to a static member", Member));
		}
	}
}
