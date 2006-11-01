using System;
using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.Schema;

public class Test
{
	public static void Main (string [] args)
	{
		if (args.Length == 0) {
			Console.WriteLine ("USAGE: xsdump masterlistname");
			return;
		}

		try {
			SchemaDumper.TestDir (args [0], Console.Out);
		} catch (Exception ex) {
			Console.WriteLine (ex);
		}
	}
}

public class SchemaDumper
{
	public static void TestDir (string masterlist, TextWriter w)
	{
		FileInfo fi = new FileInfo (masterlist);
		string dirname = fi.Directory.Parent.FullName;

		SchemaDumper d = new SchemaDumper (w);
#if false
		foreach (DirectoryInfo di in new DirectoryInfo (dirname).GetDirectories ())
			foreach (FileInfo fi in di.GetFiles ("*.xsd")) {
				try {
					d.IndentLine ("**** File : " + fi.Name);
					d.DumpSchema (XmlSchema.Read (new XmlTextReader (fi.FullName), null));
				} catch (Exception ex) {
					d.IndentLine ("**** Error in " + fi.Name);
				}
			}
#else
		XmlDocument doc = new XmlDocument ();
		doc.Load (fi.FullName);

		foreach (XmlElement test in doc.SelectNodes ("/tests/test")) {
			// Test schema
			string schemaFile = test.SelectSingleNode ("@schema").InnerText;
			if (schemaFile.Length > 2)
				schemaFile = schemaFile.Substring (2);
			bool isValidSchema = test.SelectSingleNode ("@out_s").InnerText == "1";

			if (!isValidSchema)
				continue;
#endif
			try {
				d.IndentLine ("**** File : " + schemaFile);
				d.depth++;
				XmlTextReader xtr = new XmlTextReader (dirname + "/" + schemaFile);
				d.DumpSchema (XmlSchema.Read (xtr, null));
				xtr.Close ();
			} catch (Exception ex) {
				d.IndentLine ("**** Error in " + schemaFile);
			} finally {
				d.depth--;
			}
		}
	}

	public int depth;
	TextWriter w;
	public SchemaDumper (TextWriter w)
	{
		this.w = w;
	}

	public void IndentLine (object s)
	{
		for (int i = 0; i < depth * 2; i++)
			w.Write (' ');
		w.WriteLine (s);
	}

	public void DumpSchema (XmlSchema schema)
	{
		schema.Compile (null);

		SortedList sl = new SortedList ();

		IndentLine ("**XmlSchema**");
		IndentLine ("TargetNamespace: " + schema.TargetNamespace);
		IndentLine ("AttributeGroups:");
		foreach (DictionaryEntry entry in schema.AttributeGroups)
			sl.Add (entry.Key.ToString (), entry.Value);
		foreach (DictionaryEntry entry in sl)
			DumpAttributeGroup ((XmlSchemaAttributeGroup) entry.Value);
		sl.Clear ();

		IndentLine ("Attributes:");
		foreach (DictionaryEntry entry in schema.Attributes)
			sl.Add (entry.Key.ToString (), entry.Value);
		foreach (DictionaryEntry entry in sl)
			DumpAttribute ((XmlSchemaAttribute) entry.Value);
		sl.Clear ();

		IndentLine ("Elements:");
		foreach (DictionaryEntry entry in schema.Elements)
			sl.Add (entry.Key.ToString (), entry.Value);
		foreach (DictionaryEntry entry in sl)
			DumpElement ((XmlSchemaElement) entry.Value);
		sl.Clear ();

		IndentLine ("Groups");
		foreach (DictionaryEntry entry in schema.Groups)
			sl.Add (entry.Key.ToString (), entry.Value);
		foreach (DictionaryEntry entry in sl)
			DumpGroup ((XmlSchemaGroup) entry.Value);
		sl.Clear ();

		IndentLine ("IsCompiled: " + schema.IsCompiled);

		IndentLine ("Notations");
		foreach (DictionaryEntry entry in schema.Notations)
			sl.Add (entry.Key.ToString (), entry.Value);
		foreach (DictionaryEntry entry in sl)
			DumpNotation ((XmlSchemaNotation) entry.Value);
		sl.Clear ();

		IndentLine ("SchemaTypes:");
		foreach (DictionaryEntry entry in schema.Notations)
			sl.Add (entry.Key.ToString (), entry.Value);
		foreach (DictionaryEntry entry in sl)
			if (entry.Value is XmlSchemaSimpleType)
				DumpSimpleType ((XmlSchemaSimpleType) entry.Value);
			else
				DumpComplexType ((XmlSchemaComplexType) entry.Value);
		sl.Clear ();

	}

	public void DumpAttributeGroup (XmlSchemaAttributeGroup ag)
	{
		depth++;

		IndentLine ("**AttributeGroup**");
		IndentLine ("Name = " + ag.Name);
		if (ag.RedefinedAttributeGroup != null) {
			IndentLine ("RedefinedGroup:");
			DumpAttributeGroup (ag.RedefinedAttributeGroup);
		}

		depth--;
	}

	public void DumpAttribute (XmlSchemaAttribute a)
	{
		depth++;

		IndentLine ("**Attribute**");
		IndentLine ("QualifiedName: " + a.QualifiedName);
		IndentLine ("RefName: " + a.RefName);
		IndentLine ("AttributeType:");
		DumpType (a.AttributeType);

		depth--;
	}

	public void DumpElement (XmlSchemaElement e)
	{
		depth++;

		IndentLine ("**Element**");
		IndentLine ("QualifiedName: " + e.QualifiedName);
		IndentLine ("ElementType:");
		DumpType (e.ElementType);

		depth--;
	}

	public void DumpGroup (XmlSchemaGroup g)
	{
		depth++;

		IndentLine ("**Group**");
		IndentLine ("Name: " + g.Name);

		depth--;
	}

	public void DumpNotation (XmlSchemaNotation n)
	{
		depth++;

		IndentLine ("**Notation**");
		IndentLine ("Name: " + n.Name);


		depth--;
	}

	public void DumpType (object type)
	{
		depth++;

		if (type is XmlSchemaComplexType)
			DumpComplexType ((XmlSchemaComplexType) type);
		else if (type is XmlSchemaSimpleType)
			DumpSimpleType ((XmlSchemaSimpleType) type);
		else if (type is XmlSchemaDatatype)
			DumpDatatype ((XmlSchemaDatatype) type);
		else
			IndentLine ("Unexpected Type: " + type);

		depth--;
	}

	public void DumpSimpleType (XmlSchemaSimpleType s)
	{
		depth++;

		IndentLine ("**SimpleType**");
		IndentLine ("QualifiedName: " + s.QualifiedName);
		IndentLine ("BaseSchemaType:");
		DumpType (s.BaseSchemaType);

		depth--;
	}

	public void DumpComplexType (XmlSchemaComplexType c)
	{
		depth++;

		IndentLine ("**ComplexType**");
		IndentLine ("QualifiedName: " + c.QualifiedName);
		IndentLine ("ContentType: " + c.ContentType);
		IndentLine ("ContentTypeParticle: ");
		DumpParticle (c.ContentTypeParticle);
		IndentLine ("BaseSchemaType:");
		DumpType (c.BaseSchemaType);

		depth--;
	}

	public void DumpParticle (XmlSchemaParticle p)
	{
		if (p is XmlSchemaGroupBase)
			DumpGroupBase ((XmlSchemaGroupBase) p);
		else if (p is XmlSchemaElement)
			DumpElementNoRecurse ((XmlSchemaElement) p);
		else if (p is XmlSchemaAny)
			DumpAny ((XmlSchemaAny) p);
		else
			IndentLine (p);
	}

	public void DumpDatatype (XmlSchemaDatatype d)
	{
		depth++;

		IndentLine ("**Datatype**");
		IndentLine ("TokenizedType: " + d.TokenizedType);
		IndentLine ("ValueType: " + d.ValueType);

		depth--;
	}

	public void DumpGroupBase (XmlSchemaGroupBase gb)
	{
		depth++;

		IndentLine ("**GroupBase**");
		IndentLine ("Type: " + gb);
		IndentLine ("MinOccurs: " + gb.MinOccurs);
		IndentLine ("MaxOccurs: " + gb.MaxOccurs);
		IndentLine ("Items: ");
		foreach (XmlSchemaParticle p in gb.Items)
			DumpParticle (p);

		depth--;
	}

	public void DumpElementNoRecurse (XmlSchemaElement e)
	{
		depth++;

		IndentLine ("**Element**");
		IndentLine ("QualifiedName: " + e.QualifiedName);

		depth--;
	}

	public void DumpAny (XmlSchemaAny any)
	{
		depth++;

		IndentLine ("**Any**");
//		IndentLine ("Namespace: " + any.Namespace);

		depth--;
	}
}

