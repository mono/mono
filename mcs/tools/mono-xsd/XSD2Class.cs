//
// XSD2Class - xml schema based class generator
//
// Author
//	Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//
// (C)2003 Atsushi Enomoto
//
// TODO:
//	* Currently it is developed with MS.NET and Microsoft.CSharp namespace.
//	* Handling members of choice fields are on changing. No enum types are
//	  generated and/or collected and registered as fields of the enum.
//	* maxOccurs should be considered (and should emit member as array, if
//	  required).
//
//	* It is desirable to have an alternative generator that generates
//	  property members instead of simple fields, which checks their values 
//	  in relation to their facets. (It may contradict XmlTypeMapping way,
//	  which seems not to have xml schema type itself, so we (they?) cannot 
//	  get any facets from typemapping).
//

using System;
using System.CodeDom;
using System.Collections;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Microsoft.CSharp;

namespace Commons.Xml.XSD2ClassLib
{
	/*
	public class Driver
	{
		public static void Main (string [] args)
		{
			if (args.Length < 1) {
				Console.WriteLine ("usage: xsd2class [filename]");
				return;
			}
			XmlTextReader xtr = new XmlTextReader (args [0]);
//			xtr.XmlResolver = null;
			ValidationEventHandler errorHandler = new ValidationEventHandler (OnValidationError);
			XmlSchema xs = XmlSchema.Read (xtr, errorHandler);
			xs.Compile (errorHandler);
			XmlSchemas schemas = new XmlSchemas ();
			schemas.Add (xs);
			new XSD2Class ().Generate (schemas);
		}

		static void OnValidationError (object o, ValidationEventArgs e)
		{
			// Hey... is it sane doing !?
			Console.WriteLine (e.Exception.ToString ());
		}

	}
	*/

	public class XSD2Class
	{
		XmlSchemas schemas;
		CodeCompileUnit codeCompileUnit;
		CodeNamespace codeNamespace;
		CodeTypeDeclaration currentType;
		Hashtable codeTypes = new Hashtable ();

		// Constructor

		public XSD2Class ()
		{
		}

		// Main process

		public void Generate (XmlSchemas schemas)
		{
			Generate (schemas, new CodeNamespace ());
		}

		public void Generate (XmlSchemas schemas, CodeNamespace codeNamespace)
		{
			Generate (schemas, codeNamespace, new CodeCompileUnit ());
		}

		public void Generate (XmlSchemas schemas,
			CodeNamespace codeNamespace,
			CodeCompileUnit codeCompileUnit)
		{
			this.schemas = schemas;
			this.codeCompileUnit = codeCompileUnit;
			this.codeNamespace = codeNamespace;
			codeCompileUnit.Namespaces.Add (codeNamespace);

			foreach (XmlSchema schema in schemas)
				GenerateSchemaTypes (schema);

			new CSharpCodeProvider ().CreateGenerator ().GenerateCodeFromCompileUnit (codeCompileUnit, Console.Out, null);
		}

		public void GenerateSchemaTypes (XmlSchema schema)
		{
			foreach (XmlSchemaObject sob in schema.Items) {
				XmlSchemaElement element = sob as XmlSchemaElement;
				if (element == null)
					continue;
				XmlSchemaComplexType xsType = element.ElementType as XmlSchemaComplexType;
				if (xsType == null)
					continue;

				GenerateComplexType (element.QualifiedName.Name, xsType);
			}
		}

		// Type generation

		private void GenerateComplexType (XmlSchemaComplexType xsType)
		{
			GenerateComplexType ("", xsType);
		}

		private void GenerateComplexType (string elementName, XmlSchemaComplexType xsType)
		{
			string typeName = xsType.QualifiedName.Name;
			if (typeName == "")
				typeName = elementName;
			if (codeTypes.Contains (typeName))
				return;

			currentType = CreateType (typeName);
			codeTypes.Add (xsType.QualifiedName.Name, currentType);
			codeNamespace.Types.Add (currentType);
			// base type
			XmlSchemaComplexType baseComplexType = xsType.BaseSchemaType as XmlSchemaComplexType;
			if (baseComplexType != null) {
				GenerateComplexType (baseComplexType);
//				currentType.BaseTypes = new CodeTypeReferenceCollection ();
				currentType.BaseTypes.Add (new CodeTypeReference (((CodeTypeDeclaration) codeTypes [baseComplexType.QualifiedName.Name]).Name));
			} else if (xsType.BaseSchemaType != null) {
				// TODO: insufficient. e.g. XmlQualifiedName
				currentType.BaseTypes.Add (new CodeTypeReference (((XmlSchemaSimpleType) xsType.BaseSchemaType).Name));
			}

			// anyAttribute
			if (xsType.AnyAttribute != null)
				currentType.Members.Add (CreateMemberField (
					typeof (XmlAttribute).FullName, "AnyAttr", XmlStructureType.AnyAttribute));

			// attributes
			foreach (XmlSchemaAttribute schemaAtt in xsType.Attributes)
				GenerateAttributeField (schemaAtt);

			// elements
			if (xsType.Particle != null) {
				// particle
				GenerateParticleField (xsType.Particle);
			} else if (xsType.ContentModel != null) {
				XmlSchemaComplexContentExtension ce = xsType.ContentModel.Content as XmlSchemaComplexContentExtension;
				XmlSchemaComplexContentRestriction cr = xsType.ContentModel.Content as XmlSchemaComplexContentRestriction;
				if (ce != null)
					GenerateParticleField (ce.Particle);
				else if (cr != null)
					GenerateParticleField (cr.Particle);
				// TODO: handle simpleContent (how to?)
			}
		}

		// Field generation

		private void GenerateAttributeField (XmlSchemaAttribute schemaAtt)
		{
			XmlSchemaDatatype primitive = schemaAtt.AttributeType 
				as XmlSchemaDatatype;
			XmlSchemaSimpleType simple = schemaAtt.AttributeType 
				as XmlSchemaSimpleType;
			XmlSchemaDerivationMethod deriv =
				XmlSchemaDerivationMethod.None;

			while (primitive == null) {
				if (simple == null)	// maybe union
					break;
				primitive = simple.BaseSchemaType 
					as XmlSchemaDatatype;
				if (primitive == null) {
					simple = simple.BaseSchemaType 
						as XmlSchemaSimpleType;
					if (simple != null && simple.DerivedBy != XmlSchemaDerivationMethod.None)
						deriv = simple.DerivedBy;
				}
			}

			Type type = primitive != null ?
				primitive.ValueType : typeof (object);
			bool isList = (simple != null && simple.DerivedBy == XmlSchemaDerivationMethod.List);
			CodeTypeReference cType = new CodeTypeReference (type);
			cType.ArrayRank = isList ? 1 : 0;

			CodeMemberField cmf = CreateMemberField (cType, schemaAtt.QualifiedName.Name, XmlStructureType.Attribute);
			currentType.Members.Add (cmf);
		}

		private void GenerateElementField (XmlSchemaElement schemaElem)
		{
			CodeMemberField cmf;

			XmlSchemaDatatype dt = 
				schemaElem.ElementType as XmlSchemaDatatype;
			XmlSchemaSimpleType st = 
				schemaElem.ElementType as XmlSchemaSimpleType;
			// TODO: see GenerateAttributeField to know how to get correct type.
			if (st != null)
				dt = st.Datatype;
			bool isList = (st != null && st.DerivedBy == XmlSchemaDerivationMethod.List);

			if (schemaElem.ElementType == null) {
				CodeTypeReference cType = new CodeTypeReference (typeof (object));
				cType.ArrayRank = isList ? 1 : 0;
				cmf = CreateMemberField (cType,
					schemaElem.QualifiedName.Name);
				currentType.Members.Add (cmf);
			} else if (dt != null) {
				// simple type member.
				CodeTypeReference cType = new CodeTypeReference (dt.ValueType);
				cType.ArrayRank = isList ? 1 : 0;
				cmf = CreateMemberField (cType,
					schemaElem.QualifiedName.Name);
				currentType.Members.Add (cmf);
			} else {
				// complex type member.
				XmlSchemaComplexType ct = schemaElem.ElementType
					as XmlSchemaComplexType;

				CodeTypeDeclaration ctd = currentType;
				GenerateComplexType (ct);
				currentType = ctd;
				CodeTypeDeclaration cType = codeTypes [ct.QualifiedName.Name] as CodeTypeDeclaration;
				cmf = CreateMemberField (cType.Name, schemaElem.QualifiedName.Name);
				currentType.Members.Add (cmf);
			}
		}

		private void GenerateParticleField (XmlSchemaParticle particle)
		{
			if (particle is XmlSchemaAny)
				GenerateParticleAnyField (particle 
					as XmlSchemaAny);
			else if (particle is XmlSchemaElement)
				GenerateElementField (particle 
					as XmlSchemaElement);
			else if (particle is XmlSchemaAll)
				GenerateParticleAllField (particle 
					as XmlSchemaAll);
			else if (particle is XmlSchemaChoice)
				GenerateParticleChoiceField (particle 
					as XmlSchemaChoice);
			else if (particle is XmlSchemaSequence)
				GenerateParticleSequenceField (particle 
					as XmlSchemaSequence);
			else if (particle is XmlSchemaGroupRef) {
				XmlSchemaGroupRef gRef = particle 
					as XmlSchemaGroupRef;
				GenerateGroupField (FindGroup (gRef.RefName));
			}
		}

		private void GenerateParticleAnyField (XmlSchemaAny xsany)
		{
			CodeMemberField cmf = CreateMemberField (
				typeof (XmlElement).FullName, "Any");
			currentType.Members.Add (cmf);
		}

		private void GenerateParticleAllField (XmlSchemaAll xsall)
		{
			foreach (XmlSchemaParticle cp in xsall.Items)
				GenerateParticleField (cp);
		}

		private void GenerateParticleSequenceField (XmlSchemaSequence sequence)
		{
			foreach (XmlSchemaParticle cp in sequence.Items)
				GenerateParticleField (cp);
		}

		private void GenerateParticleChoiceField (XmlSchemaChoice choice)
		{
#if true
			foreach (XmlSchemaParticle cp in choice.Items)
				GenerateParticleField (cp);
#else
			// TODO: first, collect all choice alternatives that
			// they might be common typed elements. In such case,
			// no enum fields and types should be created.
			Type itemType = typeof (Object);

			// enum type generation
			// [XmlType (IncludeInSchema=false)]
			CodeTypeDeclaration enumType = CreateType ("ItemChoiceType", false);
			enumType.IsEnum = true;
			// TODO: add enum members.
			codeNamespace.Types.Add (enumType);

			// add enum field
			CodeMemberField cid = CreateMemberField (
				enumType.Name, "ItemElementType");
			cid.CustomAttributes.Add (new CodeAttributeDeclaration (
					typeof (XmlIgnoreAttribute).FullName));
			currentType.Members.Add (cid);

			// add item field
			// TODO: type should be computed whether common or not.
			CodeMemberField cmf = CreateMemberField (
				itemType.FullName, "Item");
			CodeAttributeDeclaration choiceIdent =
				new CodeAttributeDeclaration (
				typeof (XmlChoiceIdentifierAttribute).FullName);
			choiceIdent.Arguments.Add (new CodeAttributeArgument (
				"MemberName",
				new CodePrimitiveExpression (cid.Name)));
			cmf.CustomAttributes.Add (choiceIdent);
			currentType.Members.Add (cmf);
#endif
		}

		private void GenerateGroupField (XmlSchemaGroup group)
		{
			GenerateParticleField (group.Particle);
		}

		// CreateMemberField

		private CodeMemberField CreateMemberField (string typeName, string name)
		{
			return CreateMemberField (typeName, name, XmlStructureType.Element);
		}

		private CodeMemberField CreateMemberField (string typeName, string name, XmlStructureType sType)
		{
			return CreateMemberField (new CodeTypeReference (typeName), name, sType);
		}

		private CodeMemberField CreateMemberField (CodeTypeReference reference, string name)
		{
			return CreateMemberField (reference, name, XmlStructureType.Element);
		}

		private CodeMemberField CreateMemberField (CodeTypeReference reference, string xmlName, XmlStructureType sType)
		{
			int i = 1;
			string clrName = xmlName;
			if (CodeMemberContains (clrName)) {
				while (CodeMemberContains (clrName + i))
					i++;
				clrName = clrName + i;
			}

			CodeMemberField cmf = new CodeMemberField (reference, clrName);
			cmf.Attributes = MemberAttributes.Public;

			switch (sType) {
			case XmlStructureType.Element:
				if (clrName != xmlName)
					cmf.CustomAttributes.Add (CreateXmlAttribute (typeof (XmlElementAttribute), xmlName));
				break;
			case XmlStructureType.Attribute:
				cmf.CustomAttributes.Add (CreateXmlAttribute (typeof (XmlAttributeAttribute), clrName != xmlName ? xmlName : null));
				break;
			case XmlStructureType.AnyAttribute:
				cmf.CustomAttributes.Add (CreateXmlAttribute (typeof (XmlAnyAttributeAttribute), null));
				reference.ArrayRank = 1;
				break;
			}

			return cmf;
		}

		// CreateType

		private CodeTypeDeclaration CreateType (string xmlName)
		{
			return CreateType (xmlName, true);
		}

		private CodeTypeDeclaration CreateType (string xmlName, bool includeInSchema)
		{
			int i = 1;
			string clrName = CodeIdentifier.MakeValid (xmlName);
			if (CodeTypeContains (clrName)) {
				while (CodeTypeContains (clrName + i))
					i++;
				clrName = clrName + i;
			}

			CodeTypeDeclaration decl = new CodeTypeDeclaration (clrName);
			if (includeInSchema) {
				if (xmlName != clrName)
					decl.CustomAttributes.Add (CreateXmlAttribute (typeof (XmlTypeAttribute), xmlName));
			} else {
				CodeAttributeDeclaration xt = new CodeAttributeDeclaration (typeof (XmlTypeAttribute).FullName);
				xt.Arguments.Add (new CodeAttributeArgument (
					"IncludeInSchema",
					new CodePrimitiveExpression (false)));
				decl.CustomAttributes.Add (xt);
			}
			return decl;
		}

		// Utilities

		private XmlSchemaGroup FindGroup (XmlQualifiedName qname)
		{
			foreach (XmlSchema schema in schemas) {
				foreach (XmlQualifiedName name in schema.Groups.Names) {
					XmlSchemaGroup group = schema.Groups [name] as XmlSchemaGroup;
					if (group.Name == qname.Name)
						return group;
				}
			}
			return null;
		}

		private bool CodeTypeContains (string name)
		{
			for (int i=0; i<codeNamespace.Types.Count; i++)
				if (codeNamespace.Types [i].Name == name)
					return true;
			return false;
		}

		private bool CodeMemberContains (string name)
		{
			for (int i=0; i<currentType.Members.Count; i++)
				if (currentType.Members [i].Name == name)
					return true;
			return false;
		}

		private CodeAttributeDeclaration CreateXmlAttribute (Type attrType, string name)
		{
			CodeAttributeDeclaration xmlAtt = new CodeAttributeDeclaration (attrType.FullName);
			if (name != null)
				xmlAtt.Arguments.Add (new CodeAttributeArgument ("Name", new CodePrimitiveExpression (name)));

			return xmlAtt;
		}

#if false
		// XmlSchemaImporter emulation

		public XmlTypeMapping ImportTypeMapping (XmlQualifiedName qname)
		{
			XmlSchemaComplexType xsType = FindComplexType (qname);
			if (xsType == null)
				throw new InvalidOperationException ("Type " + qname + " not found.");

			GenerateComplexType (xsType);
			return null;
		}

		private XmlSchemaComplexType FindComplexType (XmlQualifiedName qname)
		{
			foreach (XmlSchema schema in schemas) {
				foreach (XmlQualifiedName name in schema.SchemaTypes.Names) {
					XmlSchemaType xsType = schema.SchemaTypes [name] as XmlSchemaType;
					if (xsType is XmlSchemaSimpleType)
						continue;
					if (xsType.QualifiedName == qname)
							return xsType as XmlSchemaComplexType;
				}
			}
			return null;
		}
#endif
	}

	internal enum XmlStructureType
	{
		Element,
		Attribute,
		AnyAttribute
	}
}
