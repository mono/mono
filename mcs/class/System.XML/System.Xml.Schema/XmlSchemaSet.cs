//
// XmlSchemaSet.cs
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// (C)2003 Atsushi Enomoto
// (C)2004 Novell Inc.
//

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
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Security.Policy;
using System.Xml.Schema;
using System.Xml.XPath;

namespace System.Xml.Schema
{
#if NET_2_0
	public sealed class XmlSchemaSet
#else
	internal sealed class XmlSchemaSet
#endif
	{
		XmlNameTable nameTable;
		XmlResolver xmlResolver;

		ListDictionary schemas;
		XmlSchemaObjectTable attributes;
		XmlSchemaObjectTable elements;
		XmlSchemaObjectTable types;
		XmlSchemaCollection col;
		ValidationEventHandler handler;

		bool isCompiled;

		internal Guid CompilationId;

		public XmlSchemaSet ()
			: this (new NameTable ())
		{
			handler = new ValidationEventHandler (this.OnValidationError);
		}

		public XmlSchemaSet (XmlNameTable nameTable)
		{
			if (nameTable == null)
				throw new ArgumentNullException ("nameTable");

			this.nameTable = nameTable;
			schemas = new ListDictionary ();
			CompilationId = Guid.NewGuid ();
		}

		public event ValidationEventHandler ValidationEventHandler;

		public int Count {
			get { return schemas.Count; }
		}

		public XmlSchemaObjectTable GlobalAttributes {
			get {
				if (attributes == null) {
					lock (this) {
						attributes = new XmlSchemaObjectTable ();
						foreach (XmlSchema s in schemas.Values)
							foreach (XmlSchemaAttribute a in s.Attributes.Values)
								attributes.Add (a.QualifiedName, a);
					}
				}
				return attributes;
			}
		}

		public XmlSchemaObjectTable GlobalElements {
			get {
				if (elements == null) {
					lock (this) {
						elements = new XmlSchemaObjectTable ();
						foreach (XmlSchema s in schemas.Values)
							foreach (XmlSchemaElement e in s.Elements.Values)
								elements.Add (e.QualifiedName, e);
					}
				}
				return elements;
			}
		}

		public XmlSchemaObjectTable GlobalTypes { 
			get {
				if (types == null) {
					lock (this) {
						types = new XmlSchemaObjectTable ();
						foreach (XmlSchema s in schemas.Values)
							foreach (XmlSchemaType t in s.SchemaTypes.Values)
								types.Add (t.QualifiedName, t);
					}
				}
				return types;
			}
		}

		public bool IsCompiled { 
			get { return isCompiled; }
		}

		public XmlNameTable NameTable { 
			get { return nameTable; }
		}

		// This is mainly used for event delegating
		internal XmlSchemaCollection SchemaCollection {
			get { return col; }
			set { col = value; }
		}

		public XmlResolver XmlResolver { 
			set { xmlResolver = value; }
		}

		public XmlSchema Add (string targetNamespace, string url)
		{
			XmlTextReader r = null;
			try {
				r = new XmlTextReader (url);
				return Add (targetNamespace, r);
			} finally {
				if (r != null)
					r.Close ();
			}
		}

		// LAMESPEC? MS.NET allows multiple chameleon schema addition,
		// while rejects namespace-targeted schema.
		public XmlSchema Add (string targetNamespace, XmlReader reader)
		{
			XmlSchema schema = XmlSchema.Read (reader, handler);
			if (targetNamespace != null)
				schema.TargetNamespace = targetNamespace;
			XmlSchema existing = schemas [GetSafeNs (schema.TargetNamespace)] as XmlSchema;
			if (existing != null)
				throw new ArgumentException (String.Format ("Item has been already added. Target namespace is \"{0}\"", schema.TargetNamespace));
			return Add (schema);
		}

		[MonoTODO ("Check the exact behavior when namespaces are in conflict")]
		public void Add (XmlSchemaSet schemaSet)
		{
			foreach (XmlSchema schema in schemaSet.schemas)
				Add (schema);
		}

		[MonoTODO ("Currently no way to identify if the argument schema is error-prone or not.")]
		public XmlSchema Add (XmlSchema schema)
		{
			schemas [GetSafeNs (schema.TargetNamespace)] = schema;
			return schema;
		}

		[MonoTODO]
		public void Compile ()
		{
			foreach (XmlSchema schema in schemas.Values)
				schema.Compile (handler, col, xmlResolver);
			isCompiled = true;
			attributes = elements = types = null;
		}

		public bool Contains (string targetNamespace)
		{
			return schemas.Contains (targetNamespace);
		}

		public bool Contains (XmlSchema targetNamespace)
		{
			return schemas.Contains (targetNamespace);
		}

		public void CopyTo (XmlSchema[] array, int index)
		{
			schemas.CopyTo (array, index);
		}

		internal void CopyTo (Array array, int index)
		{
			schemas.CopyTo (array, index);
		}

		internal XmlSchema Get (string ns)
		{
			return (XmlSchema) schemas [GetSafeNs (ns)];
		}

		internal IEnumerator GetEnumerator ()
		{
			return schemas.GetEnumerator ();
		}

		string GetSafeNs (string ns)
		{
			return ns == null ? "" : ns;
		}

		internal void OnValidationError (object o, ValidationEventArgs e)
		{
			if (col != null)
				col.OnValidationError (o, e);
			if (ValidationEventHandler != null)
				ValidationEventHandler (o, e);
			else if (e.Severity == XmlSeverityType.Error)
				throw e.Exception;
		}

		[MonoTODO ("Check exact behavior")]
		public XmlSchema Remove (XmlSchema schema)
		{
			schemas.Remove (schema);
			return schema;
		}

		[MonoTODO]
		public bool RemoveRecursive (XmlSchema schema)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlSchema Reprocess (XmlSchema schema)
		{
			throw new NotImplementedException ();
		}

		public ICollection Schemas ()
		{
			return new ArrayList (schemas.Values);
		}

		[MonoTODO]
		public ICollection Schemas (string targetNamespace)
		{
			throw new NotImplementedException ();
		}
	}
}
