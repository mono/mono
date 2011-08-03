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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Markup;
using System.Xaml;
using System.Xaml.Schema;
using System.Xml;

namespace System.Xaml
{
	internal class ValueSerializerContext : IValueSerializerContext, IXamlSchemaContextProvider
	{
		XamlNameResolver name_resolver = new XamlNameResolver ();
		XamlTypeResolver type_resolver;
		NamespaceResolver namespace_resolver;
		PrefixLookup prefix_lookup;
		XamlSchemaContext sctx;
		IAmbientProvider ambient_provider;

		public ValueSerializerContext (PrefixLookup prefixLookup, XamlSchemaContext schemaContext, IAmbientProvider ambientProvider)
		{
			if (prefixLookup == null)
				throw new ArgumentNullException ("prefixLookup");
			if (schemaContext == null)
				throw new ArgumentNullException ("schemaContext");
			prefix_lookup = prefixLookup;
			namespace_resolver = new NamespaceResolver (prefix_lookup.Namespaces);
			type_resolver = new XamlTypeResolver (namespace_resolver, schemaContext);
			sctx = schemaContext;
			ambient_provider = ambientProvider;
		}

		public object GetService (Type serviceType)
		{
			if (serviceType == typeof (INamespacePrefixLookup))
				return prefix_lookup;
			if (serviceType == typeof (IXamlNamespaceResolver))
				return namespace_resolver;
			if (serviceType == typeof (IXamlNameResolver))
				return name_resolver;
			if (serviceType == typeof (IXamlNameProvider))
				return name_resolver;
			if (serviceType == typeof (IXamlTypeResolver))
				return type_resolver;
			if (serviceType == typeof (IAmbientProvider))
				return ambient_provider;
			if (serviceType == typeof (IXamlSchemaContextProvider))
				return this;
			return null;
		}
		
		XamlSchemaContext IXamlSchemaContextProvider.SchemaContext {
			get { return sctx; }
		}
		
		public IContainer Container {
			get { throw new NotImplementedException (); }
		}
		public object Instance {
			get { throw new NotImplementedException (); }
		}
		public PropertyDescriptor PropertyDescriptor {
			get { throw new NotImplementedException (); }
		}
		public void OnComponentChanged ()
		{
			throw new NotImplementedException ();
		}
		public bool OnComponentChanging ()
		{
			throw new NotImplementedException ();
		}
		public ValueSerializer GetValueSerializerFor (PropertyDescriptor descriptor)
		{
			throw new NotImplementedException ();
		}
		public ValueSerializer GetValueSerializerFor (Type type)
		{
			throw new NotImplementedException ();
		}
	}

	internal class XamlTypeResolver : IXamlTypeResolver
	{
		NamespaceResolver ns_resolver;
		XamlSchemaContext schema_context;

		public XamlTypeResolver (NamespaceResolver namespaceResolver, XamlSchemaContext schemaContext)
		{
			ns_resolver = namespaceResolver;
			schema_context = schemaContext;
		}

		public Type Resolve (string typeName)
		{
			var tn = XamlTypeName.Parse (typeName, ns_resolver);
			var xt = schema_context.GetXamlType (tn);
			return xt != null ? xt.UnderlyingType : null;
		}
	}

	internal class NamespaceResolver : IXamlNamespaceResolver
	{
		public NamespaceResolver (IList<NamespaceDeclaration> source)
		{
			this.source = source;
		}
	
		IList<NamespaceDeclaration> source;
	
		public string GetNamespace (string prefix)
		{
			foreach (var nsd in source)
				if (nsd.Prefix == prefix)
					return nsd.Namespace;
			return null;
		}
	
		public IEnumerable<NamespaceDeclaration> GetNamespacePrefixes ()
		{
			return source;
		}
	}

	internal class AmbientProvider : IAmbientProvider
	{
		List<AmbientPropertyValue> values = new List<AmbientPropertyValue> ();
		Stack<AmbientPropertyValue> live_stack = new Stack<AmbientPropertyValue> ();

		public void Push (AmbientPropertyValue v)
		{
			live_stack.Push (v);
			values.Add (v);
		}

		public void Pop ()
		{
			live_stack.Pop ();
		}

		public IEnumerable<object> GetAllAmbientValues (params XamlType [] types)
		{
			return GetAllAmbientValues (null, false, types);
		}
		
		public IEnumerable<AmbientPropertyValue> GetAllAmbientValues (IEnumerable<XamlType> ceilingTypes, params XamlMember [] properties)
		{
			return GetAllAmbientValues (ceilingTypes, false, null, properties);
		}
		
		public IEnumerable<AmbientPropertyValue> GetAllAmbientValues (IEnumerable<XamlType> ceilingTypes, bool searchLiveStackOnly, IEnumerable<XamlType> types, params XamlMember [] properties)
		{
			return DoGetAllAmbientValues (ceilingTypes, searchLiveStackOnly, types, properties).ToList ();
		}
		
		IEnumerable<AmbientPropertyValue> DoGetAllAmbientValues (IEnumerable<XamlType> ceilingTypes, bool searchLiveStackOnly, IEnumerable<XamlType> types, params XamlMember [] properties)
		{
			if (searchLiveStackOnly) {
				if (live_stack.Count > 0) {
					// pop, call recursively, then push back.
					var p = live_stack.Pop ();
					if (p.RetrievedProperty != null && ceilingTypes != null && ceilingTypes.Contains (p.RetrievedProperty.Type))
						yield break;
					if (DoesAmbientPropertyApply (p, types, properties))
						yield return p;

					foreach (var i in GetAllAmbientValues (ceilingTypes, searchLiveStackOnly, types, properties))
						yield return i;

					live_stack.Push (p);
				}
			} else {
				// FIXME: does ceilingTypes matter?
				foreach (var p in values)
					if (DoesAmbientPropertyApply (p, types, properties))
						yield return p;
			}
		}
		
		bool DoesAmbientPropertyApply (AmbientPropertyValue p, IEnumerable<XamlType> types, params XamlMember [] properties)
		{
			if (types == null || !types.Any () || types.Any (xt => xt.UnderlyingType != null && xt.UnderlyingType.IsInstanceOfType (p.Value)))
				if (properties == null || !properties.Any () || properties.Contains (p.RetrievedProperty))
					return true;
			return false;
		}
		
		public object GetFirstAmbientValue (params XamlType [] types)
		{
			foreach (var obj in GetAllAmbientValues (types))
				return obj;
			return null;
		}
		
		public AmbientPropertyValue GetFirstAmbientValue (IEnumerable<XamlType> ceilingTypes, params XamlMember [] properties)
		{
			foreach (var obj in GetAllAmbientValues (ceilingTypes, properties))
				return obj;
			return null;
		}
	}
}
