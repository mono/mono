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
	[MarkupExtensionReturnType (typeof (Type))]
	public class TypeExtension : MarkupExtension
	{
		public TypeExtension ()
		{
		}

		public TypeExtension (string typeName)
		{
			TypeName = typeName;
		}

		public TypeExtension (Type type)
		{
			Type = type;
		}

		public Type Type { get; set; }
		public string TypeName { get; set; }

		public override object ProvideValue (IServiceProvider serviceProvider)
		{
			if (Type != null)
				return Type;

			if (serviceProvider == null) // it can be null when Type is supplied.
				throw new ArgumentNullException ("serviceProvider");
			if (TypeName == null)
				throw new InvalidOperationException ("Either TypeName or Type must be filled before calling ProvideValue method");

			var p = ((object) serviceProvider) as IXamlTypeResolver;
			if (p == null)
				throw new ArgumentException ("serviceProvider does not implement IXamlTypeResolver.");

			return p.Resolve (TypeName);
		}
	}
}
