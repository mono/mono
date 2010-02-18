//
// XsdDataContractImporter.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
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
#if NET_2_0
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

using QName = System.Xml.XmlQualifiedName;

namespace System.Runtime.Serialization
{
	public class XsdDataContractImporter
	{
		ImportOptions options;
		CodeCompileUnit ccu;
		Dictionary<QName, QName> imported_names = new Dictionary<QName, QName> ();

		public XsdDataContractImporter ()
			: this (null)
		{
		}

		public XsdDataContractImporter (CodeCompileUnit ccu)
		{
			this.ccu = ccu;
			this.imported_names = new Dictionary<QName, QName> ();
		}

		public CodeCompileUnit CodeCompileUnit {
			get { 
				if (ccu == null)
					ccu = new CodeCompileUnit ();
			
				return ccu; 
			}
		}

		public ImportOptions Options {
			get { return options; }
			set { options = value; }
		}

		[MonoTODO]
		public ICollection<CodeTypeReference> GetKnownTypeReferences (QName typeName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public CodeTypeReference GetCodeTypeReference (QName typeName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public CodeTypeReference GetCodeTypeReference (QName typeName,
			XmlSchemaElement element)
		{
			throw new NotImplementedException ();
		}

		public bool CanImport (XmlSchemaSet schemas)
		{
			foreach (XmlSchemaElement e in schemas.GlobalElements)
				if (!CanImport (schemas, e))
					return false;
			return true;
		}

		public bool CanImport (XmlSchemaSet schemas,
			ICollection<QName> typeNames)
		{
			foreach (QName name in typeNames)
				if (!CanImport (schemas, name))
					return false;
			return true;
		}

		public bool CanImport (XmlSchemaSet schemas, QName name)
		{
			return CanImport (schemas,
				(XmlSchemaElement) schemas.GlobalElements [name]);
		}

		[MonoTODO]
		public bool CanImport (XmlSchemaSet schemas, XmlSchemaElement element)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Import (XmlSchemaSet schemas)
		{
			if (schemas == null)
				throw new ArgumentNullException ("schemas");

			schemas.Compile ();
			foreach (XmlSchemaElement e in schemas.GlobalElements.Values)
				ImportInternal (schemas, e.QualifiedName);
		}

		public void Import (XmlSchemaSet schemas,
			ICollection<QName> typeNames)
		{
			if (schemas == null)
				throw new ArgumentNullException ("schemas");
			if (typeNames == null)
				throw new ArgumentNullException ("typeNames");

			schemas.Compile ();
			foreach (QName name in typeNames)
				ImportInternal (schemas, name);
		}

		public void Import (XmlSchemaSet schemas, QName name)
		{
			if (schemas == null)
				throw new ArgumentNullException ("schemas");
			if (name == null)
				throw new ArgumentNullException ("name");

			schemas.Compile ();
			
			if (schemas.GlobalTypes [name] == null)
				throw new InvalidDataContractException (String.Format (
						"Type with name '{0}' not found in schema with namespace '{1}'", 
						name.Name, name.Namespace));

			ImportInternal (schemas, name);
		}

		[MonoTODO]
		public QName Import (XmlSchemaSet schemas, XmlSchemaElement element)
		{
			if (schemas == null)
				throw new ArgumentNullException ("schemas");
			if (element == null)
				throw new ArgumentNullException ("element");

			schemas.Compile ();
			QName ret = ImportInternal (schemas, element.QualifiedName);

			foreach (QName qname in schemas.GlobalTypes.Names)
				ImportInternal (schemas, qname);

			return ret;
		}

		private QName ImportInternal (XmlSchemaSet schemas, QName qname)
		{
			if (qname.Namespace == KnownTypeCollection.MSSimpleNamespace)
				//Primitive type
				return qname;

			if (imported_names.ContainsKey (qname))
				return imported_names [qname];

			XmlSchemas xss = new XmlSchemas ();
			foreach (XmlSchema schema in schemas.Schemas ())
				xss.Add (schema);
			
			XmlSchemaImporter xsi = new XmlSchemaImporter (xss);
			XmlTypeMapping xtm = xsi.ImportTypeMapping (qname);

			ImportFromTypeMapping (xtm);
			return qname;
		}

		//Duplicate code from ServiceContractGenerator.ExportDataContract
		private void ImportFromTypeMapping (XmlTypeMapping mapping)
		{
			if (mapping == null)
				return;

			QName qname = new QName (mapping.TypeName, mapping.Namespace);
			if (imported_names.ContainsKey (qname))
				return;

			CodeNamespace cns = new CodeNamespace ();
			cns.Name = FromXmlnsToClrName (mapping.Namespace);

			XmlCodeExporter xce = new XmlCodeExporter (cns);
			xce.ExportTypeMapping (mapping);

			List <CodeTypeDeclaration> to_remove = new List <CodeTypeDeclaration> ();
			
			//Process the types just generated
			//FIXME: Iterate and assign the types to correct namespaces
			//At the end, add all those namespaces to the ccu
			foreach (CodeTypeDeclaration type in cns.Types) {
				string ns = GetNamespace (type);
				if (ns == null)
					//FIXME: do what here?
					continue;

				QName type_name = new QName (type.Name, ns);
				if (imported_names.ContainsKey (type_name)) {
					//Type got reemitted, so remove it!
					to_remove.Add (type);
					continue;
				}
				if (type_name.Namespace == KnownTypeCollection.MSArraysNamespace) {
					to_remove.Add (type);
					continue;
				}

				imported_names [type_name] = type_name;

				type.Comments.Clear ();
				//Custom Attributes
				type.CustomAttributes.Clear ();

				type.CustomAttributes.Add (
					new CodeAttributeDeclaration (
						new CodeTypeReference ("System.CodeDom.Compiler.GeneratedCodeAttribute"),
						new CodeAttributeArgument (new CodePrimitiveExpression ("System.Runtime.Serialization")),
						new CodeAttributeArgument (new CodePrimitiveExpression ("3.0.0.0"))));
			
				type.CustomAttributes.Add (
					new CodeAttributeDeclaration (
						new CodeTypeReference ("System.Runtime.Serialization.DataContractAttribute")));

				if (type.IsEnum)
					//FIXME: Add test case for this
					continue;
	
				//BaseType and interface
				type.BaseTypes.Add (new CodeTypeReference (typeof (object)));
				type.BaseTypes.Add (new CodeTypeReference ("System.Runtime.Serialization.IExtensibleDataObject"));

				foreach (CodeTypeMember mbr in type.Members) {
					CodeMemberProperty p = mbr as CodeMemberProperty;
					if (p == null)
						continue;

					if ((p.Attributes & MemberAttributes.Public) == MemberAttributes.Public) {
						//FIXME: Clear all attributes or only XmlElementAttribute?
						p.CustomAttributes.Clear ();
						p.CustomAttributes.Add (new CodeAttributeDeclaration (
							new CodeTypeReference ("System.Runtime.Serialization.DataMemberAttribute")));

						p.Comments.Clear ();
					}
				}

				//Fields
				CodeMemberField field = new CodeMemberField (
					new CodeTypeReference ("System.Runtime.Serialization.ExtensionDataObject"),
					"extensionDataField");
				field.Attributes = MemberAttributes.Private | MemberAttributes.Final;
				type.Members.Add (field);

				//Property 
				CodeMemberProperty prop = new CodeMemberProperty ();
				prop.Type = new CodeTypeReference ("System.Runtime.Serialization.ExtensionDataObject");
				prop.Name = "ExtensionData";
				prop.Attributes = MemberAttributes.Public | MemberAttributes.Final;

				//Get
				prop.GetStatements.Add (new CodeMethodReturnStatement (
					new CodeFieldReferenceExpression (
					new CodeThisReferenceExpression (),
					"extensionDataField")));

				//Set
				prop.SetStatements.Add (new CodeAssignStatement (
					new CodeFieldReferenceExpression (
					new CodeThisReferenceExpression (),
					"extensionDataField"),
					new CodePropertySetValueReferenceExpression ()));

				type.Members.Add (prop);
			}

			foreach (CodeTypeDeclaration type in to_remove)
				cns.Types.Remove (type);

			if (cns.Types.Count > 0)
				CodeCompileUnit.Namespaces.Add (cns);
		}

		const string default_ns_prefix = "http://schemas.datacontract.org/2004/07/";

		string FromXmlnsToClrName (string xns)
		{
			if (xns.StartsWith (default_ns_prefix, StringComparison.Ordinal))
				xns = xns.Substring (default_ns_prefix.Length);
			else {
				Uri u;
				string tmp;
				if (Uri.TryCreate (xns, UriKind.Absolute, out u) && (tmp = MakeStringNamespaceComponentsValid (u.GetComponents (UriComponents.Host | UriComponents.Path, UriFormat.Unescaped))).Length > 0)
					xns = tmp;
			}
			return MakeStringNamespaceComponentsValid (xns);
		}

		static readonly char [] split_tokens = new char [] {'/', '.'};

		string MakeStringNamespaceComponentsValid (string ns)
		{
			var arr = ns.Split (split_tokens, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < arr.Length; i++)
				arr [i] = CodeIdentifier.MakeValid (arr [i]);
			return String.Join (".", arr);
		}

		private string GetNamespace (CodeTypeDeclaration type)
		{
			foreach (CodeAttributeDeclaration attr in type.CustomAttributes) {
				if (attr.Name == "System.Xml.Serialization.XmlTypeAttribute" ||
					attr.Name == "System.Xml.Serialization.XmlRootAttribute") {

					foreach (CodeAttributeArgument arg in attr.Arguments)
						if (arg.Name == "Namespace")
							return ((CodePrimitiveExpression)arg.Value).Value as string;

					//Could not find Namespace arg!
					return null;	
				}
			}
			
			return null;
		}

	}
}
#endif
