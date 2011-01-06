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
using System.Linq;
using System.Windows.Markup;
using System.Xaml;
using System.Xaml.Schema;

//
// This implementation can be compiled under .NET, using different namespace
// (Mono.Xaml). To compile it into a usable library, use the following compile
// optons and sources:
//
//	dmcs -d:DOTNET -t:library -r:System.Xaml.dll \
//		System.Xaml/XamlObjectReader.cs \
//		System.Xaml/XamlObjectNodeIterator.cs \
//		System.Xaml/XamlNode.cs \
//		System.Xaml/PrefixLookup.cs \
//		System.Xaml/ValueSerializerContext.cs \
//		System.Xaml/TypeExtensionMethods.cs
//
// (At least it should compile as of the revision that this comment is added.)
//
// Adding Test/System.Xaml/TestedTypes.cs might also be useful to examine this
// reader behavior under .NET and see where bugs are alive.
//

#if DOTNET
namespace Mono.Xaml
#else
namespace System.Xaml
#endif
{
	public class XamlObjectReader : XamlReader
	{
		public XamlObjectReader (object instance)
			: this (instance, new XamlSchemaContext (null, null), null)
		{
		}

		public XamlObjectReader (object instance, XamlObjectReaderSettings settings)
			: this (instance, new XamlSchemaContext (null, null), settings)
		{
		}

		public XamlObjectReader (object instance, XamlSchemaContext schemaContext)
			: this (instance, schemaContext, null)
		{
		}

		public XamlObjectReader (object instance, XamlSchemaContext schemaContext, XamlObjectReaderSettings settings)
		{
			if (schemaContext == null)
				throw new ArgumentNullException ("schemaContext");
			// FIXME: special case? or can it be generalized? In .NET, For Type instance Instance returns TypeExtension at root StartObject, while for Array it remains to return Array.
			if (instance is Type)
				instance = new TypeExtension ((Type) instance);

			// See also Instance property for this weirdness.
			this.root_raw = instance;
			instance = TypeExtensionMethods.GetExtensionWrapped (instance);
			this.root = instance;

			sctx = schemaContext;
			this.settings = settings;

			// check type validity. Note that some checks also needs done at Read() phase. (it is likely FIXME:)
			if (instance != null) {
				var type = new InstanceContext (instance).GetRawValue ().GetType ();
				if (!type.IsPublic)
					throw new XamlObjectReaderException (String.Format ("instance type '{0}' must be public and non-nested.", type));
				var xt = SchemaContext.GetXamlType (type);
				if (xt.ConstructionRequiresArguments && !xt.GetConstructorArguments ().Any () && xt.TypeConverter == null)
					throw new XamlObjectReaderException (String.Format ("instance type '{0}' has no default constructor.", type));
			}

			value_serializer_context = new ValueSerializerContext (new PrefixLookup (sctx), sctx);
			new XamlObjectNodeIterator (instance, sctx, value_serializer_context).PrepareReading ();
		}
		
		bool is_eof;
		object root, root_raw;
		XamlSchemaContext sctx;
		XamlObjectReaderSettings settings;
		IValueSerializerContext value_serializer_context;

		IEnumerator<NamespaceDeclaration> ns_iterator;
		IEnumerator<XamlNodeInfo> nodes;

		PrefixLookup PrefixLookup {
			get { return (PrefixLookup) value_serializer_context.GetService (typeof (INamespacePrefixLookup)); }
		}

		// This property value is weird.
		// - For root Type it returns TypeExtension.
		// - For root Array it returns Array.
		// - For non-root Type it returns Type.
		// - For IXmlSerializable, it does not either return the raw IXmlSerializable or interpreted XData (it just returns null).
		public virtual object Instance {
			get {
				var cur = NodeType == XamlNodeType.StartObject ? nodes.Current.Object.GetRawValue () : null;
				return cur == root ? root_raw : cur is XData ? null : cur;
			}
		}

		public override bool IsEof {
			get { return is_eof; }
		}

		public override XamlMember Member {
			get { return NodeType == XamlNodeType.StartMember ? nodes.Current.Member.Member : null; }
		}

		public override NamespaceDeclaration Namespace {
			get { return NodeType == XamlNodeType.NamespaceDeclaration ? ns_iterator.Current : null; }
		}

		public override XamlNodeType NodeType {
			get {
				if (is_eof)
					return XamlNodeType.None;
				else if (nodes != null)
					return nodes.Current.NodeType;
				else if (ns_iterator != null)
					return XamlNodeType.NamespaceDeclaration;
				else
					return XamlNodeType.None;
			}
		}

		public override XamlSchemaContext SchemaContext {
			get { return sctx; }
		}

		public override XamlType Type {
			get { return NodeType == XamlNodeType.StartObject ? nodes.Current.Object.Type : null; }
		}

		public override object Value {
			get {
				if (NodeType != XamlNodeType.Value)
					return null;
				return nodes.Current.Value;
			}
		}

		public override bool Read ()
		{
			if (IsDisposed)
				throw new ObjectDisposedException ("reader");
			if (IsEof)
				return false;
			
			if (ns_iterator == null)
				ns_iterator = PrefixLookup.Namespaces.GetEnumerator ();
			if (ns_iterator.MoveNext ())
				return true;
			if (nodes == null)
				nodes = new XamlObjectNodeIterator (root, sctx, value_serializer_context).GetNodes ().GetEnumerator ();
			if (nodes.MoveNext ())
				return true;
			if (!is_eof)
				is_eof = true;
			return false;
		}
	}
}
