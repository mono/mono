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
//	* Handling members of choice fields are incomplete. No enum members are
//	  collected and registered as the member.
//	* Members should be checked their names, and should be supplied
//	  XmlAttributeAttribute or XmlElementAttribute when their names already
//	  exists.
//
//	* It is desirable to have an alternative generator that generates
//	  property members instead of simple fields, which checks their values 
//	  in relation to their facets.
//

using System;
using System.CodeDom;
using System.Collections;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Microsoft.CSharp;

namespace Mono.Xml.Schema
{
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
			XmlSchema xs = XmlSchema.Read (xtr, null);
			xs.Compile (null);
			XmlSchemas schemas = new XmlSchemas ();
			schemas.Add (xs);
			new XSD2Class (schemas).Generate ();
		}
	}

	public class XSD2Class
	{
		XmlSchemas schemas;
		CodeCompileUnit codeCompileUnit;
		CodeNamespace codeNamespace;
		CodeTypeDeclaration currentType;
		Hashtable codeTypes = new Hashtable ();

		public XSD2Class (XmlSchemas schemas)
			: this (schemas, new CodeNamespace ())
		{
		}

		public XSD2Class (XmlSchemas schemas, CodeNamespace codeNamespace)
			: this (schemas, codeNamespace, new CodeCompileUnit ())
		{
		}

		public XSD2Class (XmlSchemas schemas,
			CodeNamespace codeNamespace,
			CodeCompileUnit codeCompileUnit)
		{
			this.schemas = schemas;
			this.codeCompileUnit = codeCompileUnit;
			this.codeNamespace = codeNamespace;
			codeCompileUnit.Namespaces.Add (codeNamespace);
		}

		public void Generate ()
		{
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
				XmlSchemaComplexType xsType =
					FindComplexType (element.SchemaTypeName);
				if (xsType != null)
					GenerateComplexTypeCode (xsType);
			}
		}
		
		public XmlTypeMapping ImportTypeMapping (XmlQualifiedName qname)
		{
			XmlSchemaComplexType xsType = FindComplexType (qname);
			if (xsType == null)
				throw new InvalidOperationException ("Type " + qname + " not found.");

			GenerateComplexTypeCode (xsType);
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

		private void GenerateComplexTypeCode (XmlSchemaComplexType xsType)
		{
			string typeName = xsType.QualifiedName.Name;
			if (codeTypes.Contains (typeName))
				return;
Console.Out.WriteLine ("type " + typeName);

			currentType = new CodeTypeDeclaration (typeName);
			codeTypes.Add (xsType.QualifiedName.Name, currentType);
			// anyAttribute
			if (xsType.AnyAttribute != null) {
				CodeMemberField cmf = new CodeMemberField (typeof (XmlAttribute), "AnyAttr");
				cmf.Attributes = MemberAttributes.Public;
				currentType.Members.Add (cmf);
			}

			// attributes
			foreach (XmlSchemaAttribute schemaAtt in xsType.Attributes)
				GenerateAttributeField (schemaAtt);

			// elements
			if (xsType.Particle != null) {
				// particle
				GenerateParticleField (xsType.Particle);
			} else if (xsType.ContentModel != null) {
				// content model
				throw new NotImplementedException ();
			}
			codeNamespace.Types.Add (currentType);
		}

		private void GenerateAttributeField (XmlSchemaAttribute schemaAtt)
		{
			XmlSchemaDatatype primitive = schemaAtt.AttributeType 
				as XmlSchemaDatatype;
			XmlSchemaSimpleType simple = schemaAtt.AttributeType 
				as XmlSchemaSimpleType;
			while (primitive == null) {
				simple = simple.BaseSchemaType 
					as XmlSchemaSimpleType;
				primitive = simple.BaseSchemaType 
					as XmlSchemaDatatype;
			}

			CodeMemberField cmf = new CodeMemberField (primitive.ValueType, schemaAtt.QualifiedName.Name);
			cmf.Attributes = MemberAttributes.Public;
			cmf.CustomAttributes.Add (new CodeAttributeDeclaration (typeof (XmlAttributeAttribute).FullName));
			currentType.Members.Add (cmf);
		}

		private void GenerateElementField (XmlSchemaElement schemaElem)
		{
			CodeMemberField cmf;

			XmlSchemaDatatype dt = 
				schemaElem.ElementType as XmlSchemaDatatype;
			XmlSchemaSimpleType st = 
				schemaElem.ElementType as XmlSchemaSimpleType;
			if (st != null)
				dt = st.Datatype;
			if (dt != null) {
				// simple type member.
				cmf = new CodeMemberField (dt.ValueType,
					schemaElem.QualifiedName.Name);
				cmf.Attributes = MemberAttributes.Public;
				currentType.Members.Add (cmf);
			} else {
				// complex type member.
				XmlSchemaComplexType ct = schemaElem.ElementType
					as XmlSchemaComplexType;

				CodeTypeDeclaration ctd = currentType;
				GenerateComplexTypeCode (ct);
				currentType = ctd;
				cmf = new CodeMemberField (new CodeTypeReference (ct.QualifiedName.Name), schemaElem.QualifiedName.Name);
				cmf.Attributes = MemberAttributes.Public;
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
			CodeMemberField cmf = new CodeMemberField (
				typeof (XmlElement), "Any");
			cmf.Attributes = MemberAttributes.Public;
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

		private void GenerateParticleChoiceField (XmlSchemaChoice choice)
		{
			// TODO: first, check all choice alternatives that
			// they might be common typed elements. In such case,
			// no enum fields and types should be created.

			// generate choice identifier enum type
			int i = 1;
			string itemName = "Item";
			if (CodeMemberContains (itemName)) {
				while (CodeMemberContains (itemName + i))
					i++;
				itemName = itemName + i;
			}
			i = 1;
			string enumFieldName = "ItemElementType";
			if (CodeMemberContains (enumFieldName)) {
				while (CodeMemberContains (enumFieldName  + i))
					i++;
				enumFieldName = enumFieldName + i;
			}
			i = 1;
			string typeName = "ItemChoiceType";
			if (CodeTypeContains (typeName)) {
				while (CodeTypeContains (typeName + i))
					i++;
				typeName = typeName + i;
			}

			// enum type generation
			CodeTypeDeclaration enumType = 
				new CodeTypeDeclaration (typeName);
			enumType.IsEnum = true;
			CodeAttributeDeclaration enumXmlType = 
				new CodeAttributeDeclaration (
					typeof (XmlTypeAttribute).FullName);
			// [XmlType (IncludeInSchema=false)]
			enumXmlType.Arguments.Add (
				new CodeAttributeArgument ("IncludeInSchema",
					new CodePrimitiveExpression (false)));
			enumType.CustomAttributes.Add (enumXmlType);
			// TODO: how to add enum members?

			// add item field
			CodeMemberField cmf = new CodeMemberField (
				typeof (object), itemName);
			cmf.Attributes = MemberAttributes.Public;
			CodeAttributeDeclaration choiceIdent =
				new CodeAttributeDeclaration (
				typeof (XmlChoiceIdentifierAttribute).FullName);
			choiceIdent.Arguments.Add (new CodeAttributeArgument (
				"MemberName",
				new CodePrimitiveExpression (enumFieldName)));
			cmf.CustomAttributes.Add (choiceIdent);
			currentType.Members.Add (cmf);

			// add enum field
			codeNamespace.Types.Add (enumType);

			CodeMemberField cid = new CodeMemberField (
				new CodeTypeReference (typeName), enumFieldName);
			currentType.Members.Add (cid);

		}

		private void GenerateGroupField (XmlSchemaGroup group)
		{
			GenerateParticleField (group.Particle);
		}

/*
		private CodeAttributeDeclaration CreateXmlAttribute (Type attrType, string name)
		{
			CodeAttributeDeclaration xmlAtt = new CodeAttributeDeclaration (attrType.FullName);
			if (name != null)
				xmlAtt.Arguments.Add (new CodeAttributeArgument ("Name", new CodePrimitiveExpression (name)));

			return xmlAtt;
		}
*/
	}
}
