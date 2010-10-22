//
// XsdDataContractImporter.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2010 Novell, Inc.  http://www.novell.com
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
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace System.Runtime.Serialization
{
	[MonoTODO ("Support ImportXmlType option; support arrays; CanImport is not up to date with Import")]
	public class XsdDataContractImporter
	{
		static readonly XmlQualifiedName qname_anytype = new XmlQualifiedName ("anyType", XmlSchema.Namespace);

		public XsdDataContractImporter ()
			: this (null)
		{
		}

		public XsdDataContractImporter (CodeCompileUnit codeCompileUnit)
		{
			// null argument is ok.
			CodeCompileUnit = codeCompileUnit ?? new CodeCompileUnit ();

			// Options is null by default
		}

		public CodeCompileUnit CodeCompileUnit { get; private set; }

		CodeDomProvider code_provider = CodeDomProvider.CreateProvider ("csharp");
		Dictionary<CodeNamespace,CodeIdentifiers> identifiers_table = new Dictionary<CodeNamespace,CodeIdentifiers> ();
		ImportOptions import_options;

		public ImportOptions Options {
			get { return import_options; }
			set {
				import_options = value;
				code_provider = value.CodeProvider ?? code_provider;
			}
		}

		// CanImport

		public bool CanImport (XmlSchemaSet schemas)
		{
			if (schemas == null)
				throw new ArgumentNullException ("schemas");

			if (!schemas.IsCompiled)
				schemas.Compile ();

			foreach (XmlSchemaElement xe in schemas.GlobalElements.Values)
				if (!CanImport (schemas, xe))
					return false;
			return true;
		}

		public bool CanImport (XmlSchemaSet schemas, ICollection<XmlQualifiedName> typeNames)
		{
			if (schemas == null)
				throw new ArgumentNullException ("schemas");
			if (typeNames == null)
				throw new ArgumentNullException ("typeNames");

			if (!schemas.IsCompiled)
				schemas.Compile ();

			foreach (var name in typeNames)
				if (!CanImport (schemas, name))
					return false;
			return true;
		}

		public bool CanImport (XmlSchemaSet schemas, XmlQualifiedName typeName)
		{
			if (schemas == null)
				throw new ArgumentNullException ("schemas");
			if (typeName == null)
				throw new ArgumentNullException ("typeName");

			if (!schemas.IsCompiled)
				schemas.Compile ();

			if (!schemas.GlobalTypes.Contains (typeName))
				throw new InvalidDataContractException (String.Format ("Type {0} is not found in the schemas", typeName));

			return CanImport (schemas, schemas.GlobalTypes [typeName] as XmlSchemaComplexType);
		}

		public bool CanImport (XmlSchemaSet schemas, XmlSchemaElement element)
		{
			if (schemas == null)
				throw new ArgumentNullException ("schemas");

			if (!schemas.IsCompiled)
				schemas.Compile ();

			return CanImport (schemas, element.ElementSchemaType as XmlSchemaComplexType);
		}

		bool CanImport (XmlSchemaSet schemas, XmlSchemaComplexType type)
		{
			if (type == null || type.QualifiedName.Namespace == XmlSchema.Namespace) // xs:anyType -> not supported.
				return false;

			if (type.ContentModel is XmlSchemaSimpleContent) // simple content derivation is not supported.
				return false;
			if (type.ContentModel != null && type.ContentModel.Content != null) {
				var xscce = type.ContentModel.Content as XmlSchemaComplexContentExtension;
				if (xscce == null) // complex DBR is not supported.
					return false;
				// check base type
				if (xscce.BaseTypeName != qname_anytype && !CanImport (schemas, xscce.BaseTypeName))
					return false;
			}

			return true;
		}

		// Import

		public void Import (XmlSchemaSet schemas)
		{
			if (schemas == null)
				throw new ArgumentNullException ("schemas");

			if (!schemas.IsCompiled)
				schemas.Compile ();

			foreach (XmlSchemaElement xe in schemas.GlobalElements.Values)
				Import (schemas, xe);
		}

		public void Import (XmlSchemaSet schemas, ICollection<XmlQualifiedName> typeNames)
		{
			if (schemas == null)
				throw new ArgumentNullException ("schemas");
			if (typeNames == null)
				throw new ArgumentNullException ("typeNames");
			foreach (var name in typeNames)
				Import (schemas, name);
		}

		// This checks type existence and raises an error if it is missing.
		public void Import (XmlSchemaSet schemas, XmlQualifiedName typeName)
		{
			if (schemas == null)
				throw new ArgumentNullException ("schemas");
			if (typeName == null)
				throw new ArgumentNullException ("typeName");

			if (IsPredefinedType (typeName))
				return;

			if (!schemas.GlobalTypes.Contains (typeName))
				throw new InvalidDataContractException (String.Format ("Type {0} is not found in the schemas", typeName));

			Import (schemas, schemas.GlobalTypes [typeName] as XmlSchemaType, typeName);
		}

		public XmlQualifiedName Import (XmlSchemaSet schemas, XmlSchemaElement element)
		{
			if (schemas == null)
				throw new ArgumentNullException ("schemas");
			if (element == null)
				throw new ArgumentNullException ("element");

			var elname = element.QualifiedName;

			switch (elname.Namespace) {
			case KnownTypeCollection.MSSimpleNamespace:
				switch (elname.Name) {
				case "char":
				case "duration":
				case "guid":
					return elname;
				}
				break;
			}

			// FIXME: use element to fill nillable and arrays.
			var qname = element.SchemaType != null ? element.QualifiedName : element.ElementSchemaType.QualifiedName;
			Import (schemas, element.ElementSchemaType, qname);
			return qname;
		}

		void Import (XmlSchemaSet schemas, XmlSchemaType type)
		{
			Import (schemas, type, type.QualifiedName);
		}

		void Import (XmlSchemaSet schemas, XmlSchemaType type, XmlQualifiedName qname)
		{
			var existing = imported_types.FirstOrDefault (it => it.XsdType == type);
			if (existing != null)
				return;// existing.XsdTypeName;

			if (IsPredefinedType (type.QualifiedName))
				return;

			DoImport (schemas, type, qname);
		}

		string GetUniqueName (string name, CodeNamespace cns)
		{
			CodeIdentifiers i;
			if (!identifiers_table.TryGetValue (cns, out i)) {
				i = new CodeIdentifiers ();
				identifiers_table.Add (cns, i);
			}
			return i.AddUnique (name, null);
		}

		void DoImport (XmlSchemaSet schemas, XmlSchemaType type, XmlQualifiedName qname)
		{
			CodeNamespace cns = null;
			CodeTypeReference clrRef;
			cns = GetCodeNamespace (qname);
			clrRef = new CodeTypeReference (cns.Name.Length > 0 ? cns.Name + "." + qname.Name : qname.Name);

			var td = new CodeTypeDeclaration () {
				Name = GetUniqueName (CodeIdentifier.MakeValid (qname.Name), cns),
				TypeAttributes = GenerateInternal ? TypeAttributes.NotPublic : TypeAttributes.Public };
			cns.Types.Add (td);

			var info = new TypeImportInfo () { ClrType = clrRef, XsdType = type,  XsdTypeName = qname };
			imported_types.Add (info);

			var st = type as XmlSchemaSimpleType;
			if (st != null) {
				ImportSimpleType (td, schemas, st, qname);
			} else {
				var ct = (XmlSchemaComplexType) type;
				var sc = ct.ContentModel as XmlSchemaSimpleContent;
				if (sc != null) {
					if (sc.Content is XmlSchemaSimpleContentExtension)
						throw new InvalidDataContractException (String.Format ("complex type '{0}' with simple content extension is not supported", type.QualifiedName));
				}
				if (!ImportComplexType (td, schemas, ct, qname)) {
					cns.Types.Remove (td);
					if (cns.Types.Count == 0)
						CodeCompileUnit.Namespaces.Remove (cns);
				}
			}

			foreach (var impinfo in imported_types)
				for (; impinfo.KnownTypeOutputIndex < impinfo.KnownClrTypes.Count; impinfo.KnownTypeOutputIndex++)
					td.CustomAttributes.Add (new CodeAttributeDeclaration (
						new CodeTypeReference (typeof (KnownTypeAttribute)),
						new CodeAttributeArgument (new CodeTypeOfExpression (impinfo.KnownClrTypes [impinfo.KnownTypeOutputIndex]))));
		}

		static readonly string ass_name = typeof (DataContractAttribute).Assembly.GetName ().Name;
		static readonly string ass_version = typeof (DataContractAttribute).Assembly.GetName ().Version.ToString ();
		static readonly CodeTypeReference typeref_data_contract = new CodeTypeReference (typeof (DataContractAttribute));
		static readonly CodeTypeReference typeref_coll_contract = new CodeTypeReference (typeof (CollectionDataContractAttribute));

		void AddTypeAttributes (CodeTypeDeclaration td, XmlSchemaType type, params XmlSchemaElement [] collectionArgs)
		{
			var name = type.QualifiedName;
			// [GeneratedCode (assembly_name, assembly_version)]
			td.CustomAttributes.Add (new CodeAttributeDeclaration (
				new CodeTypeReference (typeof (GeneratedCodeAttribute)),
				new CodeAttributeArgument (new CodePrimitiveExpression (ass_name)),
				new CodeAttributeArgument (new CodePrimitiveExpression (ass_version))));

			var ct = type as XmlSchemaComplexType;

			// [DataContract(Name="foobar",Namespace="urn:foobar")] (optionally IsReference=true),
			// or [CollectionDataContract(ditto, ItemType/KeyType/ValueType)]
			var dca = new CodeAttributeDeclaration (
				collectionArgs != null && collectionArgs.Length > 0 ? typeref_coll_contract : typeref_data_contract,
				new CodeAttributeArgument ("Name", new CodePrimitiveExpression (name.Name)),
				new CodeAttributeArgument ("Namespace", new CodePrimitiveExpression (name.Namespace)));
			if (collectionArgs != null) {
				if (collectionArgs.Length > 0)
					dca.Arguments.Add (new CodeAttributeArgument ("ItemName", new CodePrimitiveExpression (CodeIdentifier.MakeValid (collectionArgs [0].QualifiedName.Name))));
				if (collectionArgs.Length > 2) {
					dca.Arguments.Add (new CodeAttributeArgument ("KeyName", new CodePrimitiveExpression (CodeIdentifier.MakeValid (collectionArgs [1].QualifiedName.Name))));
					dca.Arguments.Add (new CodeAttributeArgument ("ValueName", new CodePrimitiveExpression (CodeIdentifier.MakeValid (collectionArgs [2].QualifiedName.Name))));
				}
			}
			if (ct != null && ct.AttributeUses [new XmlQualifiedName ("Ref", KnownTypeCollection.MSSimpleNamespace)] != null)
				dca.Arguments.Add (new CodeAttributeArgument ("IsReference", new CodePrimitiveExpression (true)));
			td.CustomAttributes.Add (dca);

			// optional [Serializable]
			if (Options != null && Options.GenerateSerializable)
				td.CustomAttributes.Add (new CodeAttributeDeclaration ("System.SerializableAttribute"));
		}

		static readonly CodeTypeReference typeref_ext_iface = new CodeTypeReference ("System.Runtime.Serialization.IExtensibleDataObject");
		static readonly CodeTypeReference typeref_ext_class = new CodeTypeReference ("System.Runtime.Serialization.ExtensionDataObject");

		void AddExtensionData (CodeTypeDeclaration td)
		{
			td.BaseTypes.Add (typeref_ext_iface);

			var field = new CodeMemberField (typeref_ext_class, "extensionDataField");
			td.Members.Add (field);

			var prop = new CodeMemberProperty () { Type = field.Type, Name = "ExtensionData", Attributes = (GenerateInternal ? MemberAttributes.Assembly : MemberAttributes.Public) | MemberAttributes.Final };
			prop.GetStatements.Add (new CodeMethodReturnStatement (
				new CodeFieldReferenceExpression (
				new CodeThisReferenceExpression (),
				"extensionDataField")));
			prop.SetStatements.Add (new CodeAssignStatement (
				new CodeFieldReferenceExpression (
				new CodeThisReferenceExpression (),
				"extensionDataField"),
				new CodePropertySetValueReferenceExpression ()));

			td.Members.Add (prop);
		}

		void ImportSimpleType (CodeTypeDeclaration td, XmlSchemaSet schemas, XmlSchemaSimpleType type, XmlQualifiedName qname)
		{
			var scl = type.Content as XmlSchemaSimpleTypeList;
			if (scl != null) {
				if (scl.ItemType == null)
					throw new InvalidDataContractException (String.Format ("simple type list is allowed only with an anonymous simple type with enumeration restriction content as its item type definition (type is {0})", type.QualifiedName));
				var itemType = scl.ItemType as XmlSchemaSimpleType;
				var ir = itemType.Content as XmlSchemaSimpleTypeRestriction;
				if (ir == null)
					throw new InvalidDataContractException (String.Format ("simple type list is allowed only with an anonymous simple type with enumeration restriction content as its item type definition (type is {0})", type.QualifiedName));
				ImportEnum (td, schemas, ir, type, qname, true);
				return;
			}
			var scr = type.Content as XmlSchemaSimpleTypeRestriction;
			if (scr != null) {
				ImportEnum (td, schemas, scr, type, qname, false);
				return;
			}

			throw new InvalidDataContractException (String.Format ("simple type is supported only if it has enumeration or list of an anonymous simple type with enumeration restriction content as its item type definition (type is {0})", qname));
		}

		static readonly CodeTypeReference enum_member_att_ref = new CodeTypeReference (typeof (EnumMemberAttribute));

		void ImportEnum (CodeTypeDeclaration td, XmlSchemaSet schemas, XmlSchemaSimpleTypeRestriction r, XmlSchemaType type, XmlQualifiedName qname, bool isFlag)
		{
			if (isFlag && !r.BaseTypeName.Equals (new XmlQualifiedName ("string", XmlSchema.Namespace)))
				throw new InvalidDataContractException (String.Format ("For flags enumeration '{0}', the base type for the simple type restriction must be XML schema string", qname));

			td.IsEnum = true;
			AddTypeAttributes (td, type);
			if (isFlag)
				td.CustomAttributes.Add (new CodeAttributeDeclaration (new CodeTypeReference (typeof (FlagsAttribute))));

			foreach (var facet in r.Facets) {
				var e = facet as XmlSchemaEnumerationFacet;
				if (e == null)
					throw new InvalidDataContractException (String.Format ("Invalid simple type restriction (type {0}). Only enumeration is allowed.", qname));
				var em = new CodeMemberField () { Name = CodeIdentifier.MakeValid (e.Value) };
				var ea = new CodeAttributeDeclaration (enum_member_att_ref);
				if (e.Value != em.Name)
					ea.Arguments.Add (new CodeAttributeArgument ("Value", new CodePrimitiveExpression (e.Value)));
				em.CustomAttributes.Add (ea);
				td.Members.Add (em);
			}
		}

		// Returns false if it should remove the imported type.
		// FIXME: also support ImportXmlType
		bool ImportComplexType (CodeTypeDeclaration td, XmlSchemaSet schemas, XmlSchemaComplexType type, XmlQualifiedName qname)
		{
			foreach (XmlSchemaAttribute att in type.AttributeUses.Values)
				if (att.Use != XmlSchemaUse.Optional || att.QualifiedName.Namespace != KnownTypeCollection.MSSimpleNamespace)
					throw new InvalidDataContractException (String.Format ("attribute in DataContract complex type '{0}' is limited to those in {1} namespace, and optional.", qname, KnownTypeCollection.MSSimpleNamespace));

			CodeTypeReference baseClrType = null;
			var particle = type.Particle;
			if (type.ContentModel != null) {
				var xsscr = type.ContentModel.Content as XmlSchemaSimpleContentRestriction;
				if (xsscr != null) {
					if (xsscr.BaseType != null)
						Import (schemas, xsscr.BaseType);
					else
						Import (schemas, xsscr.BaseTypeName);
					// The above will result in an error, but make sure to show we don't support it.
					throw new InvalidDataContractException (String.Format ("complex type simple content restriction is not supported in DataContract (type '{0}')", qname));
				}
				var xscce = type.ContentModel.Content as XmlSchemaComplexContentExtension;
				if (xscce != null) {
					Import (schemas, xscce.BaseTypeName);
					baseClrType = GetCodeTypeReferenceInternal (xscce.BaseTypeName, false);
					if (baseClrType != null)
						td.BaseTypes.Add (baseClrType);

					var baseInfo = GetTypeInfo (xscce.BaseTypeName, false);
					if (baseInfo != null)
						baseInfo.KnownClrTypes.Add (imported_types.First (it => it.XsdType == type).ClrType);
					particle = xscce.Particle;
				}
				var xsccr = type.ContentModel.Content as XmlSchemaComplexContentRestriction;
				if (xsccr != null)
					throw new InvalidDataContractException (String.Format ("complex content type (for type '{0}') has a restriction content model, which is not supported in DataContract.", qname));
			}

			var seq = particle as XmlSchemaSequence;
			if (seq == null && particle != null)
				throw new InvalidDataContractException (String.Format ("Not supported particle {1}. In DataContract, only sequence particle is allowed as the top-level content of a complex type (type '{0}')", qname, particle));

			if (seq != null) {

			foreach (var child in seq.Items)
				if (!(child is XmlSchemaElement))
					throw new InvalidDataContractException (String.Format ("Only local element is allowed as the content of the sequence of the top-level content of a complex type '{0}'. Other particles (sequence, choice, all, any, group ref) are not supported.", qname));

			bool isDictionary = false;
			if (type.Annotation != null) {
				foreach (var ann in type.Annotation.Items) {
					var ai = ann as XmlSchemaAppInfo;
					if (ai != null && ai.Markup != null &&
					    ai.Markup.Length > 0 &&
					    ai.Markup [0].NodeType == XmlNodeType.Element &&
					    ai.Markup [0].LocalName == "IsDictionary" &&
					    ai.Markup [0].NamespaceURI == KnownTypeCollection.MSSimpleNamespace)
						isDictionary = true;
				}
			}

			if (seq.Items.Count == 1) {
				var xe = (XmlSchemaElement) seq.Items [0];
				if (xe.MaxOccursString == "unbounded") {
					// import as a collection contract.
					if (isDictionary) {
						var kvt = xe.ElementSchemaType as XmlSchemaComplexType;
						var seq2 = kvt != null ? kvt.Particle as XmlSchemaSequence : null;
						var k = seq2 != null && seq2.Items.Count == 2 ? seq2.Items [0] as XmlSchemaElement : null;
						var v = seq2 != null && seq2.Items.Count == 2 ? seq2.Items [1] as XmlSchemaElement : null;
						if (k == null || v == null)
							throw new InvalidDataContractException (String.Format ("Invalid Dictionary contract type '{0}'. A Dictionary schema type must have a sequence particle which contains exactly two schema elements for key and value.", type.QualifiedName));
						Import (schemas, k.ElementSchemaType);
						Import (schemas, v.ElementSchemaType);
						td.BaseTypes.Add (new CodeTypeReference ("System.Collections.Generic.Dictionary", GetCodeTypeReference (k.ElementSchemaType.QualifiedName), GetCodeTypeReference (v.ElementSchemaType.QualifiedName)));
						AddTypeAttributes (td, type, xe, k, v);
						return true;
					} else if (type.QualifiedName.Namespace == KnownTypeCollection.MSArraysNamespace &&
						   IsPredefinedType (xe.ElementSchemaType.QualifiedName)) {
						// then this CodeTypeDeclaration is to be removed, and CodeTypeReference to this type should be an array instead.
						var cti = imported_types.First (i => i.XsdType == type);
						cti.ClrType = new CodeTypeReference (GetCodeTypeReference (xe.ElementSchemaType.QualifiedName), 1);
					
						return false;
					}
					else
						Import (schemas, xe.ElementSchemaType);
					td.BaseTypes.Add (new CodeTypeReference ("System.Collections.Generic.List", GetCodeTypeReference (xe.ElementSchemaType.QualifiedName)));
					AddTypeAttributes (td, type, xe);
					return true;
				}
			}
			if (isDictionary)
				throw new InvalidDataContractException (String.Format ("complex type '{0}' is an invalid Dictionary type definition. A Dictionary must have a sequence particle with exactly two child elements", qname));

			// import as a (normal) contract.
			var elems = new List<XmlSchemaElement> ();
			foreach (XmlSchemaElement xe in seq.Items) {
				if (xe.MaxOccurs != 1)
					throw new InvalidDataContractException (String.Format ("schema complex type '{0}' has a content sequence containing an element '{1}' with 'maxOccurs' value as more than 1, which is not supported in DataContract.", qname, xe.QualifiedName));

				if (elems.Any (e => e.QualifiedName.Name == xe.QualifiedName.Name))
					throw new InvalidDataContractException (String.Format ("In schema type '{0}', there already is an element whose name is {1}, where duplicate of element names are not supported.", qname, xe.QualifiedName.Name));

				elems.Add (xe);
			}
			foreach (var xe in elems) {
				// import property type in prior.
				Import (schemas, xe.ElementSchemaType.QualifiedName);
				AddProperty (td, xe);
			}

			} // if (seq != 0)

			AddTypeAttributes (td, type);
			AddExtensionData (td);

			return true;
		}

		static readonly CodeExpression this_expr = new CodeThisReferenceExpression ();
		static readonly CodeExpression arg_value_expr = new CodePropertySetValueReferenceExpression ();

		bool GenerateInternal {
			get { return Options != null && Options.GenerateInternal; }
		}

		void AddProperty (CodeTypeDeclaration td, XmlSchemaElement xe)
		{
			var att = GenerateInternal ? MemberAttributes.Assembly : MemberAttributes.Public;
			var fi = new CodeMemberField () { Name = CodeIdentifier.MakeValid (xe.QualifiedName.Name + "Field"), Type = GetCodeTypeReference (xe.ElementSchemaType.QualifiedName, xe) };
			td.Members.Add (fi);
			var pi = new CodeMemberProperty () { Name = xe.QualifiedName.Name, Attributes = att, HasGet = true, HasSet = true, Type = fi.Type };
			// [DataMember(Name=foobar, IsRequired=!nillable)]
			var dma = new CodeAttributeDeclaration (
				new CodeTypeReference (typeof (DataMemberAttribute)));
			if (fi.Name != xe.QualifiedName.Name)
				new CodeAttributeArgument ("Name", new CodePrimitiveExpression (xe.QualifiedName.Name));
			if (!xe.IsNillable)
				new CodeAttributeArgument ("IsRequired", new CodePrimitiveExpression (true));
			pi.CustomAttributes.Add (dma);

			pi.GetStatements.Add (new CodeMethodReturnStatement () { Expression = new CodeFieldReferenceExpression (this_expr, fi.Name) });
			pi.SetStatements.Add (new CodeAssignStatement (new CodeFieldReferenceExpression (this_expr, fi.Name), arg_value_expr));


			td.Members.Add (pi);
		}

		bool IsPredefinedType (XmlQualifiedName qname)
		{
			// FIXME: support char, guid and duration (MSSimpleNamespace); fix GetPrimitiveTypeFromName() first and then this at a time.
			switch (qname.Namespace) {
			case KnownTypeCollection.MSSimpleNamespace:
				switch (qname.Name) {
				case "char":
				case "guid":
				case "duration":
					return true;
				}
				return false;
			case XmlSchema.Namespace:
				return KnownTypeCollection.GetPrimitiveTypeFromName (qname.Name) != null;
			}
			return false;
		}

		CodeNamespace GetCodeNamespace (XmlQualifiedName name)
		{
			string ns = null;
			if (Options == null || !Options.Namespaces.TryGetValue (name.Namespace, out ns))
				ns = GetCodeNamespaceFromXmlns (name.Namespace);

			foreach (CodeNamespace cns in CodeCompileUnit.Namespaces)
				if (cns.Name == ns)
					return cns;
			var newCns = new CodeNamespace () { Name = ns };
			CodeCompileUnit.Namespaces.Add (newCns);
			return newCns;
		}

		const string default_ns_prefix = "http://schemas.datacontract.org/2004/07/";

		string GetCodeNamespaceFromXmlns (string xns)
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

		// Post-compilation information retrieval

		TypeImportInfo GetTypeInfo (XmlQualifiedName typeName, bool throwError)
		{
			var info = imported_types.FirstOrDefault (i => i.XsdTypeName.Equals (typeName));
			if (info == null) {
				if (throwError)
					throw new InvalidOperationException (String.Format ("schema type '{0}' has not been imported yet. Import it first.", typeName));
				return null;
			}
			return info;
		}

		public CodeTypeReference GetCodeTypeReference (XmlQualifiedName typeName)
		{
			return GetCodeTypeReferenceInternal (typeName, true);
		}

		CodeTypeReference GetCodeTypeReferenceInternal (XmlQualifiedName typeName, bool throwError)
		{
			if (typeName == null)
				throw new ArgumentNullException ("typeName");

			switch (typeName.Namespace) {
			case XmlSchema.Namespace:
				return new CodeTypeReference (KnownTypeCollection.GetPrimitiveTypeFromName (typeName.Name));
			case KnownTypeCollection.MSSimpleNamespace:
				switch (typeName.Name) {
				case "guid":
					return new CodeTypeReference (typeof (Guid));
				case "duration":
					return new CodeTypeReference (typeof (TimeSpan));
				}
				break;
			}

			var info = GetTypeInfo (typeName, throwError);
			return info != null ? info.ClrType : null;
		}

		[MonoTODO ("use element argument and fill Nullable etc.")]
		public CodeTypeReference GetCodeTypeReference (XmlQualifiedName typeName, XmlSchemaElement element)
		{
			if (typeName == null)
				throw new ArgumentNullException ("typeName");
			if (element == null)
				throw new ArgumentNullException ("element");

			return GetCodeTypeReference (typeName);
		}

		public ICollection<CodeTypeReference> GetKnownTypeReferences (XmlQualifiedName typeName)
		{
			if (typeName == null)
				throw new ArgumentNullException ("typeName");

			return GetTypeInfo (typeName, true).KnownClrTypes;
		}

		List<TypeImportInfo> imported_types = new List<TypeImportInfo> ();

		class TypeImportInfo
		{
			public TypeImportInfo ()
			{
				KnownClrTypes = new List<CodeTypeReference> ();
			}

			public CodeTypeReference ClrType { get; set; }
			public XmlQualifiedName XsdTypeName { get; set; }
			public XmlSchemaType XsdType { get; set; }
			public List<CodeTypeReference> KnownClrTypes { get; private set; }
			public int KnownTypeOutputIndex { get; set; } // updated while importing.
		}
	}
}
