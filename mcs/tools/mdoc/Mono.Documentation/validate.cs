using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;

using Mono.Options;

namespace Mono.Documentation
{
	public class MDocValidator : MDocCommand
	{
		XmlReaderSettings settings;
		long errors = 0;

		public override void Run (IEnumerable<string> args)
		{
			string[] validFormats = {
				"ecma",
			};
			string format = "ecma";
			var p = new OptionSet () {
				{ "f|format=",
					"The documentation {0:FORMAT} used within PATHS.  " + 
						"Valid formats include:\n  " +
						string.Join ("\n  ", validFormats) + "\n" +
						"If no format provided, `ecma' is used.",
					v => format = v },
			};
			List<string> files = Parse (p, args, "validate", 
					"[OPTIONS]+ PATHS",
					"Validate PATHS against the specified format schema.");
			if (files == null)
				return;
			if (Array.IndexOf (validFormats, format) < 0)
				Error ("Invalid documentation format: {0}.", format);
			Run (format, files);
		}
	
		public void Run (string format, IEnumerable<string> files)
		{
			Stream s = null;

			switch (format) {
				case "ecma":
					s = Assembly.GetExecutingAssembly ().GetManifestResourceStream ("monodoc-ecma.xsd");
					break;

				default:
					throw new NotSupportedException (string.Format ("The format `{0}' is not suppoted.", format));
			}

			if (s == null)
				throw new NotSupportedException (string.Format ("The schema for `{0}' was not found.", format));

			settings = new XmlReaderSettings ();
			settings.Schemas.Add (XmlSchema.Read (s, null));
			settings.Schemas.Compile ();
			settings.ValidationType = ValidationType.Schema;
			settings.ValidationEventHandler += OnValidationEvent;

			// skip args[0] because it is the provider name
			foreach (string arg in files) {
				if (IsMonodocFile (arg))
					ValidateFile (arg);

				if (Directory.Exists (arg))
				{
					RecurseDirectory (arg);
				}
			}

			Message (errors == 0 ? TraceLevel.Info : TraceLevel.Error, 
					"Total validation errors: {0}", errors);
		}

		void ValidateFile (string file)
		{
			try {
				using (var reader = XmlReader.Create (new XmlTextReader (file), settings)) {
					while (reader.Read ()) {
						// do nothing
					}
				}
			}
			catch (Exception e) {
				Message (TraceLevel.Error, "mdoc: {0}", e.ToString ());
			}
		}

		void RecurseDirectory (string dir)
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

		void OnValidationEvent (object sender, ValidationEventArgs a)
		{
			errors ++;
			Message (TraceLevel.Error, "mdoc: {0}", a.Message);
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

