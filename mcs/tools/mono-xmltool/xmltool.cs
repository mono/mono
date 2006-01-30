using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Xsl;
using System.Xml.XPath;
using Commons.Xml.Nvdl;
using Commons.Xml.Relaxng;
using Commons.Xml.Relaxng.Rnc;

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
					Console.WriteLine (ex);
				else
					Console.WriteLine (ex.Message);
			}
		}

		static void Usage ()
		{
			Console.WriteLine (@"
Usage: mono-xmltool [options]

options:

	--validate [*.rng | *.rnc | *.nvdl | *.xsd] [instance]
	--validate-rng relax-ng-grammar-xml instance-xml
	--validate-rnc relax-ng-compact-grammar-file instance-xml
	--validate-nvdl nvdl-script-xml instance-xml
	--validate-xsd xml-schema instance-xml
	--transform stylesheet instance-xml

environment variable that affects on the behavior:

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
			case "--validate-rnc":
				ValidateRelaxngCompact (args);
				return;
			case "--validate-rng":
				ValidateRelaxngXml (args);
				return;
			case "--validate-nvdl":
				ValidateNvdl (args);
				return;
			case "--validate-xsd":
				ValidateXsd (args);
				return;
			case "--validate":
				ValidateAuto (args);
				return;
			case "--transform":
				Transform (args);
				return;
			}
		}

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
			RelaxngPattern p = RncParser.ParseRnc (sr);
			sr.Close ();
			ValidateRelaxng (p, args);
		}

		static void ValidateRelaxng (RelaxngPattern p, string [] args)
		{
			XmlTextReader xtr = new XmlTextReader (args [2]);
			RelaxngValidatingReader vr = 
				new RelaxngValidatingReader (xtr, p);
			vr.ReportDetails = true;

			while (!vr.EOF)
				vr.Read ();
		}

		static void ValidateNvdl (string [] args)
		{
			XmlTextReader nvdlxtr = new XmlTextReader (args [1]);
			NvdlRules nvdl = NvdlReader.Read (nvdlxtr);
			nvdlxtr.Close ();
			XmlTextReader xtr = new XmlTextReader (args [2]);
			NvdlValidatingReader nvr = new NvdlValidatingReader (
				xtr, nvdl);
			while (!nvr.EOF)
				nvr.Read ();
			xtr.Close ();
		}

		static void ValidateXsd (string [] args)
		{
			XmlTextReader schemaxml = new XmlTextReader (args [1]);
			XSchema xsd = XSchema.Read (schemaxml, null);
			schemaxml.Close ();
			XmlTextReader xtr = new XmlTextReader (args [2]);
			XmlValidatingReader xvr = new XmlValidatingReader (xtr);
			xvr.Schemas.Add (xsd);
			while (!xvr.EOF)
				xvr.Read ();
			xvr.Close ();
		}

		static void Transform (string [] args)
		{
			XslTransform t = new XslTransform ();
			t.Load (args [1]);
			XmlTextWriter xw = new XmlTextWriter (Console.Out);
			t.Transform (new XPathDocument (args [2], XmlSpace.Preserve), null, xw, null);
		}
	}
}

