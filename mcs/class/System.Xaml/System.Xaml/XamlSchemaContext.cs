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
using System.Windows.Markup;
using System.Xaml.Schema;

namespace System.Xaml
{
	public class XamlSchemaContext
	{
		public XamlSchemaContext (IEnumerable<Assembly> referenceAssemblies)
			: this (referenceAssemblies, null)
		{
		}

		public XamlSchemaContext (XamlSchemaContextSettings settings)
			: this (null, settings)
		{
		}

		public XamlSchemaContext (IEnumerable<Assembly> referenceAssemblies, XamlSchemaContextSettings settings)
		{
			var l = new List<Assembly> ();
			if (referenceAssemblies != null)
				l.AddRange (referenceAssemblies);
			ReferenceAssemblies = l;

			if (settings == null)
				return;

			FullyQualifyAssemblyNamesInClrNamespaces = settings.FullyQualifyAssemblyNamesInClrNamespaces;
			SupportMarkupExtensionsWithDuplicateArity = settings.SupportMarkupExtensionsWithDuplicateArity;
		}

		public ~XamlSchemaContext ()
		{
			// what to do here?
		}

		public bool FullyQualifyAssemblyNamesInClrNamespaces { get; private set; }
		public IList<Assembly> ReferenceAssemblies { get; private set; }
		public bool SupportMarkupExtensionsWithDuplicateArity { get; private set; }

		public virtual IEnumerable<string> GetAllXamlNamespaces ()
		{
			throw new NotImplementedException ();
		}

		public virtual ICollection<XamlType> GetAllXamlTypes (string xamlNamespace)
		{
			throw new NotImplementedException ();
		}

		public virtual string GetPreferredPrefix (string xmlns)
		{
			throw new NotImplementedException ();
		}

		protected internal XamlValueConverter<TConverterBase> GetValueConverter<TConverterBase> (Type converterType, XamlType targetType)
			where TConverterBase : class
		{
			throw new NotImplementedException ();
		}
		
		public virtual XamlDirective GetXamlDirective (string xamlNamespace, string name)
		{
			throw new NotImplementedException ();
		}
		
		public virtual XamlType GetXamlType (Type type)
		{
			throw new NotImplementedException ();
		}
		
		public XamlType GetXamlType (XamlTypeName xamlTypeName)
		{
			throw new NotImplementedException ();
		}
		
		protected internal virtual XamlType GetXamlType (string xamlNamespace, string name, params XamlType[] typeArguments)
		{
			throw new NotImplementedException ();
		}

		protected internal virtual Assembly OnAssemblyResolve (string assemblyName)
		{
			throw new NotImplementedException ();
		}

		public virtual bool TryGetCompatibleXamlNamespace (string xamlNamespace, out string compatibleNamespace)
		{
			throw new NotImplementedException ();
		}
	}
}
