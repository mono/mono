using System;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;

namespace Mono.Documentation
{
	public class Validater
	{
		static XmlValidatingReader reader;
		static XmlSchema schema;
		static long errors = 0;
		static bool IsValid = true;
	
		public static void Main (string[] args)
		{
			if (args.Length < 2) {
				PrintUsage ();
			}

			Stream s = null;

			switch (args[0]) {
				case "ecma":
					s = Assembly.GetExecutingAssembly ().GetManifestResourceStream ("monodoc-ecma.xsd");
					break;
				default:
					Console.WriteLine ("Unknown provider: {0}", args[0]);
					Environment.Exit (0);
					break;
			}

			if (s == null) {
				Console.WriteLine ("ERROR: schema for {0} was not found", args[0]);
				Environment.Exit (1);
			}

			schema = XmlSchema.Read (s, null);
			schema.Compile (null);

			// skip args[0] because it is the provider name
			for (int i = 1; i < args.Length; i++) {
				string arg = args[i];
				if (IsMonodocFile (arg))
					ValidateFile (arg);

				if (Directory.Exists (arg))
				{
					RecurseDirectory (arg);
				}
			}

			Console.WriteLine ("Total validation errors: {0}", errors);
		}

		static void PrintUsage ()
		{
			Console.WriteLine ("usage: normalize.exe <provider> <files>");
			Environment.Exit (0);
		}

		static void ValidateFile (string file)
		{
			IsValid = true;
			try {
				reader = new XmlValidatingReader (new XmlTextReader (file));
				reader.ValidationType = ValidationType.Schema;
				reader.Schemas.Add (schema);
				reader.ValidationEventHandler += new ValidationEventHandler (OnValidationEvent);
				while (reader.Read ()) {
					// do nothing
				}
				reader.Close ();
			}
			catch (Exception e) {
				Console.WriteLine ("mdvalidator: error: " + e.ToString ());
			}
		}

		static void RecurseDirectory (string dir)
		{
			string[] files = Directory.GetFiles (dir, "*.xml");
			foreach (string f in files)
			{
				if (IsMonodocFile (f))
					ValidateFile (f);
			}

			string[] dirs = Directory.GetDirectories (dir);
			foreach (string d in dirs)
				RecurseDirectory (d);
		}

		static void OnValidationEvent (object sender, ValidationEventArgs a)
		{
			if (IsValid)
				IsValid = false;
			errors ++;
			Console.WriteLine (a.Message);
		}

		static bool IsMonodocFile (string file)
		{
				if (File.Exists (file) && Path.GetExtension (file).ToLower () == ".xml")
					return true;
				else
					return false;
			
		}
	}
}

