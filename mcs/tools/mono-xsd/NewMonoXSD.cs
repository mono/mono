///
/// MonoXSD.cs -- A reflection-based tool for dealing with XML Schema.
///
/// Author: Duncan Mak (duncan@ximian.com)
///         Lluis Sanchez Gual (lluis@ximian.com)
///
/// Copyright (C) 2003, Duncan Mak,
///                     Ximian, Inc.
///

using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;

namespace Mono.Util {

	public class Driver
	{
		public static readonly string helpString =
			"xsd.exe - a utility for generating schema or class files\n\n" +
			"xsd.exe <schema>.xsd /classes [/element:NAME] [/language:NAME]\n" +
			"            [/namespace:NAME] [/outputdir:PATH] [/uri:NAME]\n\n" +
			"xsd.exe <assembly>.dll|<assembly>.exe [/outputdir:PATH] [/type:NAME]\n\n" +
			"   /c /classes        Generate classes for the specified schema.\n" +
			"   /e /element:NAME   Element from schema to generate code for.\n" +
			"                      Multiple elements can be specified.\n" +
			"   /u /uri:NAME       Namespace uri of the elements to generate code for.\n" +
			"   /l /language:NAME  The language to use for the generated code.\n" +
			"                      Currently, the only supported language is CS (C#).\n" +
			"   /o /outputdir:PATH The directory where to generate the code or schemas.\n" +
			"   /n /namespace:NAME Namespace for the generated code.\n" +
			"   /t /type:NAME      Type for which to generate an xml schema.\n" +
			"                      Multiple types can be specified.\n" +
			"   /h /help           Output this help.\n";

		static readonly string incorrectOrder = "Options must be specified after Assembly or schema file names";
		static readonly string duplicatedParam = "The option {0} cannot be specified more that once";
		static readonly string unknownOption = "Unknown option {0}";
		static readonly string incompatibleArgs = "Cannot mix options for generating schemas and for generatic classes";
		static readonly string invalidParams = "Invalid parameters";
		static readonly string tooManyAssem = "Only one assembly name can be specified";
		static readonly string errLoadAssembly = "Could not load assembly: {0}";
		static readonly string typeNotFound = "Type {0} not found in the specified assembly";
		static readonly string languageNotSupported = "The language {0} is not supported";
		

		static void Main (string [] args)
		{
			if (args.Length < 1) {
				Console.WriteLine (helpString);
				Environment.Exit (0);
			}

			try
			{
				new Driver().Run (args);
			}
			catch (Exception ex)
			{
				Console.WriteLine (ex.Message);
				Console.WriteLine (ex);
			}
		}

		string outputDir = null;


		ArrayList lookupTypes = new ArrayList();
		ArrayList assemblies = new ArrayList();

		ArrayList schemaNames = new ArrayList();
		ArrayList elements = new ArrayList();
		string language = null;
		string namesp = null;
		string uri = null;

		public void Run (string[] args)
		{
			ArrayList unknownFiles = new ArrayList();
			bool generateClasses = false;
			bool readingFiles = true;
			bool schemasOptions = false;
			bool assemblyOptions = false;

			foreach (string arg in args)
			{
				if (arg.EndsWith (".dll") || arg.EndsWith (".exe"))
				{
					if (!readingFiles) throw new Exception (incorrectOrder);
					assemblies.Add (arg);
					assemblyOptions = true;
					continue;
				}
				else if (arg.EndsWith (".xsd"))
				{
					if (!readingFiles) Error (incorrectOrder);
					schemaNames.Add (arg);
					schemasOptions = true;
					continue;
				}
				else if (!arg.StartsWith ("/") && !arg.StartsWith ("-"))
				{
					if (!readingFiles) Error (incorrectOrder);
					unknownFiles.Add (arg);
					continue;
				}

				readingFiles = false;

				int i = arg.IndexOf (":");
				if (i == -1) i = arg.Length;
				string option = arg.Substring (1,i-1);
				string param = (i<arg.Length-1) ? arg.Substring (i+1) : "";

				if (option == "classes" || option == "c")
				{
					if (generateClasses) Error (duplicatedParam, option);
					generateClasses = true;
					schemasOptions = true;
				}
				else if (option == "element" || option == "e")
				{
					elements.Add (param);
					schemasOptions = true;
				}
				else if (option == "language" || option == "l")
				{
					if (language != null) Error (duplicatedParam, option);
					language = param;
					schemasOptions = true;
				}
				else if (option == "namespace" || option == "n")
				{
					if (namesp != null) Error (duplicatedParam, option);
					namesp = param;
					schemasOptions = true;
				}
				else if (option == "outputdir" || option == "o")
				{
					if (outputDir != null) Error (duplicatedParam, option);
					outputDir = param;
				}
				else if (option == "uri" || option == "u")
				{
					if (uri != null) Error (duplicatedParam, option);
					uri = param;
					schemasOptions = true;
				}
				else if (option == "type" || option == "t")
				{
					lookupTypes.Add (param);
					assemblyOptions = true;
				}
				else if (option == "help" || option == "h")
				{
					Console.WriteLine (helpString);
					return;
				}
				else
					Error (unknownOption, option);
			}

			if (!schemasOptions && !assemblyOptions)
				Error (invalidParams);

			if (schemasOptions && assemblyOptions)
				Error (incompatibleArgs);

			if (assemblies.Count > 1)
				Error (tooManyAssem);

			if (outputDir == null) outputDir = ".";
				
			if (schemasOptions)
			{
				schemaNames.AddRange (unknownFiles);
				GenerateClasses ();
			}
			else
			{
				assemblies.AddRange (unknownFiles);
				GenerateSchemas ();
			}
		}

		public void GenerateSchemas ()
		{
			Assembly assembly = null;
			try
			{
				assembly = Assembly.LoadFrom ((string) assemblies [0]);
			}
			catch (Exception ex)
			{
				Error (errLoadAssembly, ex.Message);
			}
			
			Type[] types;
			
			if (lookupTypes.Count > 0)
			{
				types = new Type [lookupTypes.Count];
				for (int n=0; n<lookupTypes.Count; n++)
				{
					Type t = assembly.GetType ((string)lookupTypes[n]);
					if (t == null) Error (typeNotFound, (string)lookupTypes[n]);
					types[n] = t;
				}
			}
			else
				types = assembly.GetExportedTypes ();

			XmlReflectionImporter ri = new XmlReflectionImporter ();
			XmlSchemas schemas = new XmlSchemas ();
			XmlSchemaExporter sx = new XmlSchemaExporter (schemas);

			foreach (Type type in types)
			{
				XmlTypeMapping tm = ri.ImportTypeMapping (type);
				sx.ExportTypeMapping (tm);
			}

			if (schemas.Count == 1)
			{
				string fileName = Path.Combine (outputDir, "schema.xsd");
				WriteSchema (fileName, schemas [0]);
			}
			else
			{
				for (int n=0; n<schemas.Count; n++)
				{
					string fileName = Path.Combine (outputDir, "schema" + n + ".xsd");
					WriteSchema (fileName, schemas [n]);
				}
			}
		}

		void WriteSchema (string fileName, XmlSchema schema)
		{
			StreamWriter sw = new StreamWriter (fileName);
			schema.Write (sw);
			sw.Close ();
			Console.WriteLine ("Written file " + fileName);
		}
		
		public void GenerateClasses ()
		{
			if (language != null && language != "CS") Error (languageNotSupported, language);
			if (namesp == null) namesp = "Schemas";
			if (uri == null) uri = "";
			string targetFile = "";

			XmlSchemas schemas = new XmlSchemas();
			foreach (string fileName in schemaNames)
			{
				StreamReader sr = new StreamReader (fileName);
				schemas.Add (XmlSchema.Read (sr, null));
				sr.Close ();

				if (targetFile == "") targetFile = Path.GetFileNameWithoutExtension (fileName);
				else targetFile += "_" + Path.GetFileNameWithoutExtension (fileName);
			}

			targetFile += ".cs";

			CodeCompileUnit cunit = new CodeCompileUnit ();
			CodeNamespace codeNamespace = new CodeNamespace (namesp);
			cunit.Namespaces.Add (codeNamespace);
			codeNamespace.Comments.Add (new CodeCommentStatement ("\nThis source code was auto-generated by MonoXSD\n"));

			// Locate elements to generate

			ArrayList qnames = new ArrayList ();
			if (elements.Count > 0)
			{
				foreach (string name in elements)
					qnames.Add (new XmlQualifiedName (name, uri));
			}
			else
			{
				foreach (XmlSchema schema in schemas) {
					foreach (XmlSchemaElement elem in schema.Elements)
						qnames.Add (elem.QualifiedName);
				}
			}

			// Import schemas and generate the class model

			XmlSchemaImporter importer = new XmlSchemaImporter (schemas);
			XmlCodeExporter sx = new XmlCodeExporter (codeNamespace, cunit);

			ArrayList maps = new ArrayList();

			foreach (XmlQualifiedName qname in qnames)
			{
				XmlTypeMapping tm = importer.ImportTypeMapping (qname);
				if (tm != null) maps.Add (tm);
			}
			
			foreach (XmlTypeMapping tm in maps)
			{
				sx.ExportTypeMapping (tm);
			}

			// Generate the code
			
			CSharpCodeProvider provider = new CSharpCodeProvider();
			ICodeGenerator gen = provider.CreateGenerator();

			string genFile = Path.Combine (outputDir, targetFile);
			StreamWriter sw = new StreamWriter(genFile, false);
			gen.GenerateCodeFromCompileUnit (cunit, sw, new CodeGeneratorOptions());
			sw.Close();

			Console.WriteLine ("Written file " + genFile);
		}

		public void Error (string msg)
		{
			throw new Exception (msg);
		}

		public void Error (string msg, string param)
		{
			throw new Exception (string.Format(msg,param));
		}
	}
}
