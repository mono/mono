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
using System.Xaml.Schema;

namespace System.Xaml
{
	public class XamlObjectReader : XamlReader
	{
		#region nested types

		class NSList : List<NamespaceDeclaration>
		{
			public NSList (XamlNodeType ownerType, IEnumerable<NamespaceDeclaration> nsdecls)
				: base (nsdecls)
			{
				OwnerType = ownerType;
			}
			
			public XamlNodeType OwnerType { get; set; }

			public IEnumerator<NamespaceDeclaration> GetEnumerator ()
			{
				return new NSEnumerator (this, base.GetEnumerator ());
			}
		}

		class NSEnumerator : IEnumerator<NamespaceDeclaration>
		{
			NSList list;
			IEnumerator<NamespaceDeclaration> e;

			public NSEnumerator (NSList list, IEnumerator<NamespaceDeclaration> e)
			{
				this.list= list;
				this.e = e;
			}
			
			public XamlNodeType OwnerType {
				get { return list.OwnerType; }
			}

			public void Dispose ()
			{
			}

			public bool MoveNext ()
			{
				return e.MoveNext ();
			}

			public NamespaceDeclaration Current {
				get { return e.Current; }
			}

			object IEnumerator.Current {
				get { return Current; }
			}

			public void Reset ()
			{
				throw new NotSupportedException ();
			}
		}
	
		class PrefixLookup : INamespacePrefixLookup
		{
			XamlObjectReader source;

			public PrefixLookup (XamlObjectReader source)
			{
				this.source = source;
			}

			public string LookupPrefix (string ns)
			{
				return source.LookupPrefix (ns);
			}
		}

		#endregion nested types

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
			// FIXME: special case? or can it be generalized?
			if (instance is Type)
				instance = new TypeExtension ((Type) instance);

			this.instance = instance;
			sctx = schemaContext;
			this.settings = settings;

			prefix_lookup = new PrefixLookup (this);

			if (instance != null) {
				// check type validity. Note that some checks are done at Read() phase.
				var type = instance.GetType ();
				if (!type.IsPublic)
					throw new XamlObjectReaderException (String.Format ("instance type '{0}' must be public and non-nested.", type));
				root_type = SchemaContext.GetXamlType (instance.GetType ());
				if (root_type.ConstructionRequiresArguments && !root_type.GetConstructorArguments ().Any () && root_type.TypeConverter == null)
					throw new XamlObjectReaderException (String.Format ("instance type '{0}' has no default constructor.", type));
			}
			else
				root_type = XamlLanguage.Null;
		}

		object instance;
		XamlType root_type;
		XamlSchemaContext sctx;
		XamlObjectReaderSettings settings;

		INamespacePrefixLookup prefix_lookup;

		Stack<XamlType> types = new Stack<XamlType> ();
		Stack<object> objects = new Stack<object> ();
		Stack<IEnumerator<XamlMember>> members_stack = new Stack<IEnumerator<XamlMember>> ();
		NSList namespaces;
		IEnumerator<NamespaceDeclaration> ns_iterator;
		XamlNodeType node_type = XamlNodeType.None;
		bool is_eof;

		// Unlike object stack, this can be Dictionary, since an there should not be the same object within the stack, and we can just get current stack with "objects" stack.
		Dictionary<object,IEnumerator<object>> constructor_arguments_stack = new Dictionary<object,IEnumerator<object>> ();

		public virtual object Instance {
			get { return NodeType == XamlNodeType.StartObject && objects.Count > 0 ? objects.Peek () : null; }
		}

		public override bool IsEof {
			get { return is_eof; }
		}

		public override XamlMember Member {
			get { return NodeType == XamlNodeType.StartMember ? members_stack.Peek ().Current : null; }
		}

		public override NamespaceDeclaration Namespace {
			get { return NodeType == XamlNodeType.NamespaceDeclaration ? ns_iterator.Current : null; }
		}

		public override XamlNodeType NodeType {
			get { return node_type; }
		}

		public override XamlSchemaContext SchemaContext {
			get { return sctx; }
		}

		public override XamlType Type {
			get { return NodeType == XamlNodeType.StartObject ? types.Peek () : null; }
		}

		public override object Value {
			get { return NodeType == XamlNodeType.Value ? objects.Peek () : null; }
		}

		internal string LookupPrefix (string ns)
		{
			foreach (var nsd in namespaces)
				if (nsd.Namespace == ns)
					return nsd.Prefix;
			return null;
		}

		public override bool Read ()
		{
			if (IsDisposed)
				throw new ObjectDisposedException ("reader");
			if (IsEof)
				return false;
			XamlType type;
			IEnumerator<XamlMember> members;
			IEnumerator<object> arguments;
			switch (NodeType) {
			case XamlNodeType.None:
			default:
				// -> namespaces
				var d = new Dictionary<string,string> ();
				//l.Sort ((p1, p2) => String.CompareOrdinal (p1.Key, p2.Key));
				CollectNamespaces (d, instance, root_type);
				var nss = from k in d.Keys select new NamespaceDeclaration (k, d [k]);
				namespaces = new NSList (XamlNodeType.StartObject, nss);
				namespaces.Sort ((n1, n2) => String.CompareOrdinal (n1.Prefix, n2.Prefix));
				ns_iterator = namespaces.GetEnumerator ();

				ns_iterator.MoveNext ();
				node_type = XamlNodeType.NamespaceDeclaration;
				return true;

			case XamlNodeType.NamespaceDeclaration:
				if (ns_iterator.MoveNext ())
					return true;
				node_type = ((NSEnumerator) ns_iterator).OwnerType; // StartObject or StartMember
				if (node_type == XamlNodeType.StartObject)
					StartNextObject ();
				else
					StartNextMemberOrNamespace ();
				return true;

			case XamlNodeType.StartObject:
				var xt = types.Peek ();
				members = xt.GetAllReadWriteMembers ().GetEnumerator ();
				if (members.MoveNext ()) {
					members_stack.Push (members);
					StartNextMemberOrNamespace ();
					return true;
				}
				else
					node_type = XamlNodeType.EndObject;
				return true;

			case XamlNodeType.StartMember:
				var curMember = members_stack.Peek ().Current;
				if (curMember == XamlLanguage.Arguments) {
					type = types.Peek ();
					var args = type.GetSortedConstructorArguments ();
					var obj = objects.Peek ();
					var l = new List<object> ();
					foreach (var arg in args)
						l.Add (arg.Invoker.GetValue (obj));
					arguments = l.GetEnumerator ();
					constructor_arguments_stack [obj] = arguments;
					arguments.MoveNext ();
					StartNextObject (arguments.Current);
				}
				else if (!curMember.IsContentValue ())
					StartNextObject ();
				else {
					var obj = GetMemberValueOrRootInstance ();
					objects.Push (obj);
					node_type = XamlNodeType.Value;
				}
				return true;

			case XamlNodeType.Value:
				objects.Pop ();
				node_type = XamlNodeType.EndMember;
				return true;

			case XamlNodeType.GetObject:
				// how do we get here?
				throw new NotImplementedException ();

			case XamlNodeType.EndMember:
				members = members_stack.Peek ();
				if (members.MoveNext ()) {
					members_stack.Push (members);
					StartNextMemberOrNamespace ();
				} else {
					members_stack.Pop ();
					node_type = XamlNodeType.EndObject;
				}
				return true;

			case XamlNodeType.EndObject:
				// It might be either end of the entire object tree or just the end of an object value.
				types.Pop ();
				objects.Pop ();
				if (objects.Count == 0) {
					node_type = XamlNodeType.None;
					is_eof = true;
					return false;
				}

				if (constructor_arguments_stack.TryGetValue (objects.Peek (), out arguments)) {
					if (arguments.MoveNext ()) {
						StartNextObject (arguments.Current);
						return true;
					}
					// else -> end of Arguments
					constructor_arguments_stack.Remove (objects.Peek ());
				} else {
					members = members_stack.Peek ();
					if (members.MoveNext ()) {
						StartNextMemberOrNamespace ();
						return true;
					}
				}
				// then, move to the end of current object member.
				node_type = XamlNodeType.EndMember;
				return true;
			}
		}

		void CollectNamespaces (Dictionary<string,string> d, object o, XamlType xt)
		{
			if (xt == null)
				return;
			if (o == null) {
				// it becomes NullExtension, so check standard ns.
				CheckAddNamespace (d, XamlLanguage.Xaml2006Namespace);
				return;
			}
			var ns = xt.PreferredXamlNamespace;
			CheckAddNamespace (d, ns);

			foreach (var xm in xt.GetAllMembers ()) {
				ns = xm.PreferredXamlNamespace;
				if (xm is XamlDirective && ns == XamlLanguage.Xaml2006Namespace)
					continue;
				if (xm.Type.IsCollection || xm.Type.IsDictionary || xm.Type.IsArray)
					continue; // FIXME: process them too.
				var mv = GetMemberValueOf (xm, o, xt, d);
				CollectNamespaces (d, mv, xm.Type);
			}
		}

		// This assumes that the next member is already on current position on current iterator.
		void StartNextMemberOrNamespace ()
		{
			// FIXME: there might be NamespaceDeclarations.
			node_type = XamlNodeType.StartMember;
		}

		void StartNextObject ()
		{
			StartNextObject (GetMemberValueOrRootInstance ());
		}

		void StartNextObject (object obj)
		{
			var xt = Object.ReferenceEquals (obj, instance) ? root_type : obj != null ? SchemaContext.GetXamlType (obj.GetType ()) : XamlLanguage.Null;

			// FIXME: enable these lines.
			// FIXME: if there is an applicable instance descriptor, then it could be still valid.
			//var type = xt.UnderlyingType;
			//if (type.GetConstructor (System.Type.EmptyTypes) == null)
			//	throw new XamlObjectReaderException (String.Format ("Type {0} has no default constructor or an instance descriptor.", type));

			objects.Push (obj);
			types.Push (xt);
			node_type = XamlNodeType.StartObject;
		}
		
		object GetMemberValueOrRootInstance ()
		{
			if (objects.Count == 0)
				return instance;

			var xm = members_stack.Peek ().Current;
			var obj = objects.Peek ();
			var xt = types.Peek ();
			return GetMemberValueOf (xm, obj, xt, null);
		}

		object GetMemberValueOf (XamlMember xm, object obj, XamlType xt, Dictionary<string,string> collectingNamespaces)
		{
			object retobj;
			XamlType retxt;
			if (xt.IsContentValue ()) {
				retxt = xt;
				retobj = obj;
			} else {
				retxt = xm.Type;
				retobj = xm.GetMemberValueForObjectReader (xt, obj, prefix_lookup);
			}

			if (collectingNamespaces != null) {
				if (retobj is Type || retobj is TypeExtension) {
					var type = (retobj as Type) ?? ((TypeExtension) retobj).Type;
					if (type == null) // only TypeExtension.TypeName
						return null;
					var xtt = SchemaContext.GetXamlType (type);
					var ns = xtt.PreferredXamlNamespace;
					var nss = collectingNamespaces;
					CheckAddNamespace (collectingNamespaces, ns);
					return null;
				}
				else if (retxt.IsContentValue ())
					return null;
				else
					return retobj;
			} else if (retxt.IsContentValue ()) {
				// FIXME: I'm not sure if this should be really done 
				// here, but every primitive values seem to be exposed
				// as a string, not a typed object in XamlObjectReader.
				return retxt.GetStringValue (retobj, prefix_lookup);
			}
			else
				return retobj;
		}

		void CheckAddNamespace (Dictionary<string,string> d, string ns)
		{
			if (ns == XamlLanguage.Xaml2006Namespace)
				d [XamlLanguage.Xaml2006Namespace] = "x";
			else if (!d.ContainsValue (String.Empty))
				d [ns] = String.Empty;
			else if (!d.ContainsKey (ns))
				d.Add (ns, SchemaContext.GetPreferredPrefix (ns));
		}
	}
}
