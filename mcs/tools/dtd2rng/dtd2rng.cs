using System;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using Commons.Xml.Relaxng;
using Commons.Xml.Relaxng.Rnc;

using BF = System.Reflection.BindingFlags;

namespace Mono.XmlTools
{
	public class Dtd2Rng
	{
		public static int Main (string [] args)
		{
			if (args.Length == 0) {
				Usage ();
				return 1;
			}

			return new Dtd2Rng ().Process (args);
		}

		static void Usage ()
		{
			Console.Error.WriteLine (@"
Usage dtd2rng [options] dtdfile [ns]

options:
	--help : show this message.
	--compact, -c : output compact syntax.
");
		}

		public int Process (string [] args)
		{
			string file = null;
			bool compact = false;
			string ns = String.Empty;
			foreach (string arg in args) {
				if (arg == "--help") {
					Usage ();
					return 1;
				}
				if (arg == "--compact" || arg == "-c")
					compact = true;
				else if (file == null)
					file = arg;
				else if (ns != String.Empty) {
					Usage ();
					Console.Error.WriteLine ("Extra command line argument.");
					return 1;
				}
				else
					ns = arg;
			}

			XmlTextReader xtr;
			if (file.EndsWith (".dtd")) {
				xtr = new XmlTextReader (
					"<!DOCTYPE dummy SYSTEM '" + file + "'>",
					XmlNodeType.Document, null);
			} else {
				xtr = new XmlTextReader (file);
			}
			xtr.Read ();
			if (xtr.NodeType == XmlNodeType.XmlDeclaration)
				xtr.Read ();

			XmlSchema xsd = GetXmlSchema (xtr);

			RelaxngPattern rng = DtdXsd2Rng (xsd, ns);
			if (compact)
				rng.WriteCompact (Console.Out);
			else {
				XmlTextWriter w = new XmlTextWriter (Console.Out);
				w.Formatting = Formatting.Indented;
				rng.Write (w);
				w.Close ();
			}
			return 0;
		}

		XmlSchema GetXmlSchema (XmlTextReader xtr)
		{
			// Hacky reflection part
			object impl = xtr;
			BF flag = BF.NonPublic | BF.Instance;

			// In Mono NET_2_0 XmlTextReader is just a wrapper which 
			// does not contain DTD directly.
			FieldInfo fi = typeof (XmlTextReader).GetField ("source", flag);
			if (fi != null)
				impl = fi.GetValue (xtr);

			PropertyInfo pi = impl.GetType ().GetProperty ("DTD", flag);
			object dtd = pi.GetValue (impl, null);
			MethodInfo mi =
				dtd.GetType ().GetMethod ("CreateXsdSchema", flag);
			object o = mi.Invoke (dtd, null);
			return (XmlSchema) o;
		}

		RelaxngGrammar g;

		RelaxngGrammar DtdXsd2Rng (XmlSchema xsd, string ns)
		{
			g = new RelaxngGrammar ();
			g.DefaultNamespace = ns;
			RelaxngStart start = new RelaxngStart ();
			g.Starts.Add (start);
			RelaxngChoice choice = new RelaxngChoice ();
			start.Pattern = choice;

			// There are only elements.
			foreach (XmlSchemaElement el in xsd.Items) {
				RelaxngDefine def = DefineElement (el);
				g.Defines.Add (def);
				RelaxngRef dref = new RelaxngRef ();
				dref.Name = def.Name;
				choice.Patterns.Add (dref);
			}

			return g;
		}

		RelaxngDefine DefineElement (XmlSchemaElement el)
		{
			RelaxngDefine def = new RelaxngDefine ();
			def.Patterns.Add (CreateElement (el));
			def.Name = el.Name;

			return def;
		}

		RelaxngPattern CreateElement (XmlSchemaElement xse)
		{
			if (xse.RefName != XmlQualifiedName.Empty) {
				RelaxngRef r = new RelaxngRef ();
				r.Name = xse.RefName.Name;
				// namespace means nothing here.
				return r;
			}

			RelaxngElement re = new RelaxngElement ();
			RelaxngName name = new RelaxngName ();
			name.LocalName = xse.Name;
			re.NameClass = name;

			XmlSchemaComplexType ct = xse.SchemaType as XmlSchemaComplexType;

			foreach (XmlSchemaAttribute a in ct.Attributes)
				re.Patterns.Add (CreateAttribute (a));

			RelaxngPattern rpart;
			if (ct.Particle == null)
				rpart = new RelaxngEmpty ();
			else
				rpart = CreatePatternFromParticle (ct.Particle);

			if (ct.IsMixed) {
				if (rpart.PatternType != RelaxngPatternType.Empty) {
					RelaxngMixed mixed = new RelaxngMixed ();
					mixed.Patterns.Add (rpart);
					rpart = mixed;
				} else {
					rpart = new RelaxngText ();
				}
			}

			re.Patterns.Add (rpart);

			return re;
		}

		RelaxngPattern CreateAttribute (XmlSchemaAttribute attr)
		{
			RelaxngAttribute ra = new RelaxngAttribute ();
			RelaxngName name = new RelaxngName ();
			name.LocalName = attr.Name;
			ra.NameClass = name;
			ra.Pattern = attr.SchemaType != null ?
				CreatePatternFromType (attr.SchemaType) :
				CreatePatternFromTypeName (attr.SchemaTypeName);

			RelaxngPattern ret = ra;

			if (attr.Use == XmlSchemaUse.Optional) {
				RelaxngOptional opt = new RelaxngOptional ();
				opt.Patterns.Add (ra);
				ret = opt;
			}
			return ret;
		}

		RelaxngPattern CreatePatternFromParticle (XmlSchemaParticle xsdp)
		{
			RelaxngSingleContentPattern rngp = null;
			if (xsdp.MinOccurs == 0 && xsdp.MaxOccursString == "unbounded")
				rngp = new RelaxngZeroOrMore ();
			else if (xsdp.MinOccurs == 1 && xsdp.MaxOccursString == "unbounded")
				rngp = new RelaxngOneOrMore ();
			else if (xsdp.MinOccurs == 0)
				rngp = new RelaxngOptional ();

			RelaxngPattern child = CreatePatternFromParticleCore (xsdp);
			if (rngp == null)
				return child;
			rngp.Patterns.Add (child);
			return rngp;
		}

		RelaxngPattern CreatePatternFromParticleCore (XmlSchemaParticle xsdp)
		{
			XmlSchemaGroupBase gb = xsdp as XmlSchemaGroupBase;
			if (xsdp is XmlSchemaAny) {
				RelaxngRef r = new RelaxngRef ();
				r.Name = "anyType";
				return r;
			}
			if (gb is XmlSchemaSequence) {
				RelaxngGroup grp = new RelaxngGroup ();
				foreach (XmlSchemaParticle xsdc in gb.Items)
					grp.Patterns.Add (CreatePatternFromParticle (xsdc));
				return grp;
			}
			if (gb is XmlSchemaChoice) {
				RelaxngChoice rc = new RelaxngChoice ();
				foreach (XmlSchemaParticle xsdc in gb.Items)
					rc.Patterns.Add (CreatePatternFromParticle (xsdc));
				return rc;
			}
			return CreateElement ((XmlSchemaElement) xsdp);
		}

		RelaxngPattern CreatePatternFromType (XmlSchemaType type)
		{
			XmlSchemaSimpleType st = type as XmlSchemaSimpleType;
			if (st == null)
				throw new NotSupportedException ("Complex types are not supported as an attribute type.");
			XmlSchemaSimpleTypeRestriction r =
				st.Content as XmlSchemaSimpleTypeRestriction;
			if (r == null)
				throw new NotSupportedException ("Only simple type restriction is supported as an attribute type.");

			RelaxngChoice c = new RelaxngChoice ();
			foreach (XmlSchemaFacet f in r.Facets) {
				XmlSchemaEnumerationFacet en =
					f as XmlSchemaEnumerationFacet;
				if (en == null)
					throw new NotSupportedException ("Only enumeration facet is supported.");
				RelaxngValue v = new RelaxngValue ();
				v.Type = r.BaseTypeName.Name;
				v.DatatypeLibrary = RemapDatatypeLibrary (
					r.BaseTypeName.Namespace);
				v.Value = en.Value;
				c.Patterns.Add (v);
			}
			return c;
		}

		RelaxngPattern CreatePatternFromTypeName (XmlQualifiedName name)
		{
			if (name == XmlQualifiedName.Empty)
				return new RelaxngText ();
			RelaxngData data = new RelaxngData ();
			data.Type = name.Name;
			data.DatatypeLibrary = RemapDatatypeLibrary (
				name.Namespace);
			return data;
		}

		string RemapDatatypeLibrary (string ns)
		{
			return ns == XmlSchema.Namespace ?
				"http://www.w3.org/2001/XMLSchema-datatypes" :
				ns;
		}
	}
}
