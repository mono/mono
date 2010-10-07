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
using System.ComponentModel;
using System.Reflection;
using System.Xaml;
using System.Xaml.Schema;

namespace System.Windows.Markup
{
	[ContentProperty ("Name")]
	public class Reference : MarkupExtension
	{
		public Reference ()
		{
		}

		public Reference (string name)
		{
			Name = name;
		}

		[ConstructorArgument ("name")]
		public string Name { get; set; }

		public override object ProvideValue (IServiceProvider serviceProvider)
		{
			if (serviceProvider == null)
				throw new ArgumentNullException ("serviceProvider");
			if (Name == null)
				throw new InvalidOperationException ("Name property is not set");
			var r = serviceProvider.GetService (typeof (IXamlNameResolver)) as IXamlNameResolver;
			if (r == null)
				throw new InvalidOperationException ("serviceProvider does not implement IXamlNameResolver");
			var ret = r.Resolve (Name);
			if (ret == null)
				ret = r.GetFixupToken (new string [] {Name}, true);
			return ret;
		}
	}
}
