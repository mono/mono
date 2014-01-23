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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Security.Policy;
using System.Xml.Schema;
using System.Xml.XPath;

namespace System.Xml.Schema
{
#if NET_2_0
	public class XmlSchemaSet
#else
	internal sealed class XmlSchemaSet
#endif
	{
		XmlNameTable nameTable;
		XmlResolver xmlResolver = new XmlUrlResolver ();

		ArrayList schemas;
		XmlSchemaObjectTable attributes;
		XmlSchemaObjectTable elements;
		XmlSchemaObjectTable types;

		XmlSchemaCompilationSettings settings =
			new XmlSchemaCompilationSettings ();

		bool isCompiled;

		internal Guid CompilationId { get; private set; }

		public XmlSchemaSet ()
			: this (new NameTable ())
		{
		}

		public XmlSchemaSet (XmlNameTable nameTable)
		{
			if (nameTable == null)
				throw new ArgumentNullException ("nameTable");

			this.nameTable = nameTable;
			schemas = new ArrayList ();
			CompilationId = Guid.NewGuid ();
		}

		public event ValidationEventHandler ValidationEventHandler;

		public int Count {
			get { return schemas.Count; }
		}

		public XmlSchemaObjectTable GlobalAttributes {
			get {
				if (attributes == null)
					attributes = new XmlSchemaObjectTable ();
				return attributes;
			}
		}

		public XmlSchemaObjectTable GlobalElements {
			get {
				if (elements == null)
					elements = new XmlSchemaObjectTable ();
				return elements;
			}
		}

		public XmlSchemaObjectTable GlobalTypes { 
			get {
				if (types == null)
					types = new XmlSchemaObjectTable ();
				return types;
			}
		}

		public bool IsCompiled { 
			get { return isCompiled; }
		}

		public XmlNameTable NameTable { 
			get { return nameTable; }
		}

		public XmlSchemaCompilationSettings CompilationSettings {
			get { return settings; }
			set { settings = value; }
		}

		public XmlResolver XmlResolver {
			set { xmlResolver = value; }
#if NET_2_0
			internal get { return xmlResolver; }
#else
			get { return xmlResolver; }
#endif
		}

		public XmlSchema Add (string targetNamespace, string schemaUri)
		{
			var uri = xmlResolver.ResolveUri (null, schemaUri);
			using (var stream = (Stream) xmlResolver.GetEntity (uri, null, typeof (Stream)))
				using (var r = XmlReader.Create (stream, new XmlReaderSettings () { XmlResolver = xmlResolver, NameTable = nameTable}, uri.ToString ()))
					return Add (targetNamespace, r);
		}

		public XmlSchema Add (string targetNamespace, XmlReader schemaDocument)
		{
			XmlSchema schema = XmlSchema.Read (schemaDocument, ValidationEventHandler);
			if (schema.TargetNamespace == null)
				schema.TargetNamespace = targetNamespace == String.Empty ? null : targetNamespace; // this weirdness is due to bug #571660.
			else if (targetNamespace != null && schema.TargetNamespace != targetNamespace)
				throw new XmlSchemaException ("The actual targetNamespace in the schema does not match the parameter.");
			Add (schema);
			return schema;
		}

		[MonoTODO]
		// FIXME: Check the exact behavior when namespaces are in conflict (but it would be preferable to wait for 2.0 RTM)
		public void Add (XmlSchemaSet schemas)
		{
			ArrayList al = new ArrayList ();
			foreach (XmlSchema schema in schemas.schemas) {
				if (!this.schemas.Contains (schema))
					al.Add (schema);
			}
			foreach (XmlSchema schema in al)
				Add (schema);
		}

		public XmlSchema Add (XmlSchema schema)
		{
			schemas.Add (schema);
			ResetCompile ();
			return schema;
		}

		// FIXME: It should be the actual compilation engine.
		public void Compile ()
		{
			ClearGlobalComponents ();
			ArrayList al = new ArrayList ();
			al.AddRange (schemas);

			var handledUris = new List<CompiledSchemaMemo> ();
			foreach (XmlSchema schema in al)
				if (!schema.IsCompiled)
					schema.CompileSubset (ValidationEventHandler, this, xmlResolver, handledUris);

			// Process substitutionGroup first, as this process
			// involves both substituted and substituting elements
			// and hence it needs to be done before actual
			// validation (by current design of conformance checker).
			foreach (XmlSchema schema in al)
				foreach (XmlSchemaElement elem in schema.Elements.Values)
					elem.FillSubstitutionElementInfo ();


			foreach (XmlSchema schema in al)
				schema.Validate (ValidationEventHandler);

			foreach (XmlSchema schema in al)
				AddGlobalComponents (schema);

			isCompiled = true;
		}

		private void ClearGlobalComponents ()
		{
			GlobalElements.Clear ();
			GlobalAttributes.Clear ();
			GlobalTypes.Clear ();
			global_attribute_groups.Clear ();
			global_groups.Clear ();
			global_notations.Clear ();
			global_ids.Clear ();
			global_identity_constraints.Clear ();
		}
		
		XmlSchemaObjectTable global_attribute_groups = new XmlSchemaObjectTable ();
		XmlSchemaObjectTable global_groups = new XmlSchemaObjectTable ();
		XmlSchemaObjectTable global_notations = new XmlSchemaObjectTable ();
		XmlSchemaObjectTable global_identity_constraints = new XmlSchemaObjectTable ();
		Hashtable global_ids = new Hashtable ();

		private void AddGlobalComponents (XmlSchema schema)
		{
			foreach (XmlSchemaElement el in schema.Elements.Values)
				GlobalElements.Add (el.QualifiedName, el);
			foreach (XmlSchemaAttribute a in schema.Attributes.Values)
				GlobalAttributes.Add (a.QualifiedName, a);
			foreach (XmlSchemaType t in schema.SchemaTypes.Values)
				GlobalTypes.Add (t.QualifiedName, t);
			foreach (XmlSchemaAttributeGroup g in schema.AttributeGroups.Values)
				global_attribute_groups.Add (g.QualifiedName, g);
			foreach (XmlSchemaGroup g in schema.Groups.Values)
				global_groups.Add (g.QualifiedName, g);
			foreach (DictionaryEntry pair in schema.IDCollection)
				global_ids.Add (pair.Key, pair.Value);
			foreach (XmlSchemaIdentityConstraint ic in schema.NamedIdentities.Values)
				global_identity_constraints.Add (ic.QualifiedName, ic);
		}

		public bool Contains (string targetNamespace)
		{
			targetNamespace = GetSafeNs (targetNamespace);
			foreach (XmlSchema schema in schemas)
				if (GetSafeNs (schema.TargetNamespace) == targetNamespace)
					return true;
			return false;
		}

		public bool Contains (XmlSchema schema)
		{
			foreach (XmlSchema s in schemas)
				if (s == schema)
					return true;
			return false;
		}

		public void CopyTo (XmlSchema [] schemas, int index)
		{
			this.schemas.CopyTo (schemas, index);
		}

		internal void CopyTo (Array array, int index)
		{
			schemas.CopyTo (array, index);
		}

		string GetSafeNs (string ns)
		{
			return ns == null ? "" : ns;
		}

		[MonoTODO]
		// FIXME: Check exact behavior
		public XmlSchema Remove (XmlSchema schema)
		{
			if (schema == null)
				throw new ArgumentNullException ("schema");
			ArrayList al = new ArrayList ();
			al.AddRange (schemas);
			if (!al.Contains (schema))
				return null;
			// FIXME: I have no idea why Remove() might throw
			// XmlSchemaException, except for the case it compiles.
			if (!schema.IsCompiled)
				schema.CompileSubset (ValidationEventHandler, this, xmlResolver);
			schemas.Remove (schema);
			ResetCompile ();
			return schema;
		}

		void ResetCompile ()
		{
			isCompiled = false;
			ClearGlobalComponents ();
		}

		public bool RemoveRecursive (XmlSchema schemaToRemove)
		{
			if (schemaToRemove == null)
				throw new ArgumentNullException ("schema");
			ArrayList al = new ArrayList ();
			al.AddRange (schemas);
			if (!al.Contains (schemaToRemove))
				return false;
			al.Remove (schemaToRemove);
			schemas.Remove (schemaToRemove);

			if (!IsCompiled)
				return true;

			ClearGlobalComponents ();
			foreach (XmlSchema s in al) {
				if (s.IsCompiled)
					AddGlobalComponents (schemaToRemove);
			}
			return true;
		}

		public XmlSchema Reprocess (XmlSchema schema)
		{
			if (schema == null)
				throw new ArgumentNullException ("schema");
			ArrayList al = new ArrayList ();
			al.AddRange (schemas);
			if (!al.Contains (schema))
				throw new ArgumentException ("Target schema is not contained in the schema set.");
			ClearGlobalComponents ();
			foreach (XmlSchema s in al) {
				if (schema == s)
					schema.CompileSubset (ValidationEventHandler, this, xmlResolver);
				if (s.IsCompiled)
					AddGlobalComponents (schema);
			}
			return schema.IsCompiled ? schema : null;
		}

		public ICollection Schemas ()
		{
			return schemas;
		}

		public ICollection Schemas (string targetNamespace)
		{
			targetNamespace = GetSafeNs (targetNamespace);
			ArrayList al = new ArrayList ();
			foreach (XmlSchema schema in schemas)
				if (GetSafeNs (schema.TargetNamespace) == targetNamespace)
					al.Add (schema);
			return al;
		}

		internal bool MissedSubComponents (string targetNamespace)
		{
			foreach (XmlSchema s in Schemas (targetNamespace))
				if (s.missedSubComponents)
					return true;
			return false;
		}
	}
}
