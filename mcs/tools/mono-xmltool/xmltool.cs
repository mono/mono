using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Xsl;
using System.Xml.XPath;
#if !TARGET_JVM && !MSNET
using Commons.Xml.Nvdl;
using Commons.Xml.Relaxng;
using Commons.Xml.Relaxng.Rnc;
#endif
using XSchema = System.Xml.Schema.XmlSchema;

namespace Commons.Xml.Relaxng
{
	public class Driver
	{
		public static void Main (string [] args)
		{
			try {
				Run (args);
			} catch (Exception ex) {
				if (Environment.GetEnvironmentVariable ("MONO_XMLTOOL_ERROR_DETAILS") == "yes")
					Console.Error.WriteLine (ex);
				else
					Console.Error.WriteLine (ex.Message);
			}
		}

		static void Usage ()
		{
			Console.Error.WriteLine (@"
Usage: mono-xmltool [options]

options:

	--validate [*.rng | *.rnc | *.nvdl | *.xsd] [instances]
	--validate-rng relax-ng-grammar-xml [instances]
	--validate-rnc relax-ng-compact-grammar-file [instances]
	--validate-nvdl nvdl-script-xml [instances]
	--validate-xsd xml-schema [instances]
	--validate-xsd2 xml-schema [instances] (in .NET 2.0 validator)
	--validate-dtd instances
	--transform stylesheet instance-xml [output-xml]
	--prettyprint [source] [result]

environment variable that affects behavior:

	MONO_XMLTOOL_ERROR_DETAILS = yes : to get exception details.
");
		}

		static void Run (string [] args)
		{
			if (args.Length == 0) {
				Usage ();
				return;
			}

			switch (args [0]) {
			default:
			case "--help":
				Usage ();
				return;
#if !TARGET_JVM && !MSNET
			case "--validate":
				ValidateAuto (args);
				return;
			case "--validate-rnc":
				ValidateRelaxngCompact (args);
				return;
			case "--validate-rng":
				ValidateRelaxngXml (args);
				return;
			case "--validate-nvdl":
				ValidateNvdl (args);
				return;
#endif
			case "--validate-xsd2":
				ValidateXsd2 (args);
				return;
			case "--validate-xsd":
				ValidateXsd (args);
				return;
			case "--validate-dtd":
				ValidateDtd (args);
				return;
			case "--transform":
				Transform (args);
				return;
			case "--prettyprint":
				PrettyPrint (args);
				return;
			}
		}

#if !TARGET_JVM && !MSNET
		static void ValidateAuto (string [] args)
		{
			if (args.Length < 1) {
				Usage ();
				return;
			}

			if (args [1].EndsWith ("rng"))
				ValidateRelaxngXml (args);
			else if (args [1].EndsWith ("rnc"))
				ValidateRelaxngCompact (args);
			else if (args [1].EndsWith ("nvdl"))
				ValidateNvdl (args);
			else if (args [1].EndsWith ("xsd"))
				ValidateXsd (args);
		}

		static void ValidateRelaxngXml (string [] args)
		{
			XmlReader xr = new XmlTextReader (args [1]);
			RelaxngPattern p = RelaxngPattern.Read (xr);
			xr.Close ();
			ValidateRelaxng (p, args);
		}

		static void ValidateRelaxngCompact (string [] args)
		{
			StreamReader sr = new StreamReader (args [1]);
			RelaxngPattern p = RncParser.ParseRnc (sr, null, Path.GetFullPath (args [1]));
			sr.Close ();
			ValidateRelaxng (p, args);
		}

		static void ValidateRelaxng (RelaxngPattern p, string [] args)
		{
			p.Compile ();

			if (args.Length < 2)
				return;

			for (int i = 2; i < args.Length; i++) {
				XmlTextReader xtr = new XmlTextReader (args [i]);
				RelaxngValidatingReader vr = 
					new RelaxngValidatingReader (xtr, p);
				if (Environment.GetEnvironmentVariable ("MONO_XMLTOOL_ERROR_DETAILS") == "yes")
					vr.ReportDetails = true;
				else
					vr.InvalidNodeFound += delegate (XmlReader source, string message) {
						IXmlLineInfo li = source as IXmlLineInfo;
						Console.WriteLine ("ERROR: {0} (at {1} line {2} column {3})",
							message,
							source.BaseURI,
							li != null && li.HasLineInfo () ? li.LineNumber : 0,
							li != null && li.HasLineInfo () ? li.LinePosition : 0);
						return true;
					};

				while (!vr.EOF)
					vr.Read ();
			}
		}

		static void ValidateNvdl (string [] args)
		{
			XmlTextReader nvdlxtr = new XmlTextReader (args [1]);
			NvdlRules nvdl = NvdlReader.Read (nvdlxtr);
			nvdlxtr.Close ();
			for (int i = 2; i < args.Length; i++) {
				XmlTextReader xtr = new XmlTextReader (args [i]);
				NvdlValidatingReader nvr = new NvdlValidatingReader (xtr, nvdl);
				while (!nvr.EOF)
					nvr.Read ();
				xtr.Close ();
			}
		}
#endif

		static void ValidateXsd (string [] args)
		{
			XmlTextReader schemaxml = new XmlTextReader (args [1]);
			XSchema xsd = XSchema.Read (schemaxml, null);
			schemaxml.Close ();
			xsd.Compile (null);
			for (int i = 2; i < args.Length; i++) {
				XmlTextReader xtr = new XmlTextReader (args [i]);
				XmlValidatingReader xvr = new XmlValidatingReader (xtr);
				xvr.Schemas.Add (xsd);
				while (!xvr.EOF)
					xvr.Read ();
				xvr.Close ();
			}
		}

		static void ValidateXsd2 (string [] args)
		{
			XmlReaderSettings s = new XmlReaderSettings ();
			s.ValidationType = ValidationType.Schema;
			s.Schemas.Add (null, args [1]);
			for (int i = 2; i < args.Length; i++) {
				XmlReader xr = XmlReader.Create (args [i], s);
				while (!xr.EOF)
					xr.Read ();
				xr.Close ();
			}
		}

		static void ValidateDtd (string [] args)
		{
			for (int i = 1; i < args.Length; i++) {
				XmlValidatingReader xvr = new XmlValidatingReader (
					new XmlTextReader (args [i]));
				xvr.ValidationType = ValidationType.DTD;
				xvr.EntityHandling = EntityHandling.ExpandEntities;
				while (!xvr.EOF)
					xvr.Read ();
				xvr.Close ();
			}
		}

		static void Transform (string [] args)
		{
			XslTransform t = new XslTransform ();
			t.Load (args [1]);
			TextWriter output = args.Length > 3 ?
				File.CreateText (args [3]) : Console.Out;
			t.Transform (new XPathDocument (args [2], XmlSpace.Preserve), null, output, null);
			output.Close ();
		}

		static void PrettyPrint (string [] args)
		{
			XmlTextReader r = null;
			if (args.Length > 1)
				r = new XmlTextReader (args [1]);
			else
				r = new XmlTextReader (Console.In);
			r.WhitespaceHandling = WhitespaceHandling.Significant;
			XmlTextWriter w = null;
			if (args.Length > 2)
				w = new XmlTextWriter (args [1], Encoding.UTF8);
			else
				w = new XmlTextWriter (Console.Out);
			w.Formatting = Formatting.Indented;

			r.Read ();
			while (!r.EOF)
				w.WriteNode (r, false);
			r.Close ();
			w.Close ();
		}
	}
}

