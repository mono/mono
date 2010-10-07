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
	[TypeConverter (typeof (TypeExtensionConverter))]
	[System.Runtime.CompilerServices.TypeForwardedFrom (Consts.AssemblyPresentationFramework_3_5)]
	public class TypeExtension : MarkupExtension
	{
		public TypeExtension ()
		{
		}

		public TypeExtension (string typeName)
		{
			if (typeName == null)
				throw new ArgumentNullException ("typeName");
			TypeName = typeName;
		}

		public TypeExtension (Type type)
		{
			if (type == null)
				throw new ArgumentNullException ("type");
			Type = type;
		}

		[ConstructorArgument ("type")]
		[DefaultValue (null)]
		public Type Type { get; set; }
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public string TypeName { get; set; }

		public override object ProvideValue (IServiceProvider serviceProvider)
		{
			if (Type != null)
				return Type;

			if (TypeName == null)
				throw new InvalidOperationException ("Either TypeName or Type must be filled before calling ProvideValue method");

			if (serviceProvider == null) // it can be null when Type is supplied.
				throw new ArgumentNullException ("serviceProvider");

			var p = serviceProvider.GetService (typeof (IXamlTypeResolver)) as IXamlTypeResolver;
			if (p == null)
				throw new InvalidOperationException ("serviceProvider does not provide IXamlTypeResolver service.");

			var ret = p.Resolve (TypeName);
			if (ret == null)
				throw new InvalidOperationException (String.Format ("Type '{0}' is not resolved as a valid type by the type resolver '{1}'.", TypeName, p.GetType ()));
			return ret;
		}
	}
}
