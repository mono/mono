///
/// MonoXSD.cs -- A reflection-based tool for dealing with XML Schema.
///
/// Authors: Duncan Mak (duncan@ximian.com)
///          Lluis Sanchez Gual (lluis@ximian.com)
///          Atsushi Enomoto (atsushi@ximian.com)
///
/// Copyright (C) 2003, Duncan Mak,
///                     Ximian, Inc.
///

using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using Microsoft.VisualBasic;

namespace Mono.Util {

	public class Driver
	{
		public static readonly string helpString =
			"xsd.exe - a utility for generating schema or class files\n\n" +
			"xsd.exe <schema>.xsd /classes [/element:NAME] [/language:NAME]\n" +
			"            [/namespace:NAME] [/outputdir:PATH] [/uri:NAME]\n\n" +
			"xsd.exe <schema>.xsd /dataset [/element:NAME] [/language:NAME]\n" +
			"            [/namespace:NAME] [/outputdir:PATH] [/uri:NAME]\n\n" +

			"xsd.exe <assembly>.dll|<assembly>.exe [/outputdir:PATH] [/type:NAME]\n\n" +
			"xsd.exe <instance>.xml [<instance>.xml ...] [/outputdir:PATH]\n\n" +
			"   /c /classes        Generate classes for the specified schema.\n" +
			"   /d /dataset        Generate typed dataset classes for the specified schema.\n" +
			"   /e /element:NAME   Element from schema to generate code for.\n" +
			"                      Multiple elements can be specified.\n" +
			"   /u /uri:NAME       Namespace uri of the elements to generate code for.\n" +
			"   /l /language:NAME  The language, or type name of custom CodeDomProvider\n" +
			"                      to use for the generated code.\n" +
			"                      Shorthand specifiers are: \"CS\" (C#) and \"VB\" (VB.NET).\n" +
			"                      For type name, assembly qualified name is required.\n" +
			"   /g /generator:TYPE Code Generator type name, followed by ','\n" + 
			"                      and assembly file name.\n" +
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
		static readonly string missingOutputForXsdInput = "Can only generate one of classes or datasets.";
		static readonly string generatorAssemblyNotFound = "Could not load code provider assembly file: {0}";
		static readonly string generatorTypeNotFound = "Could not find specified code provider type: {0}";
		static readonly string generatorTypeIsNotCodeGenerator = "Specified code provider type was not CodeDomProvider: {0}";
		static readonly string generatorThrewException = "Specified CodeDomProvider raised an error while creating its instance: {0}";

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
			catch (ApplicationException ex)
			{
				Console.WriteLine (ex.Message);
			}
			catch (Exception ex)
			{
				Console.WriteLine (ex);
			}
		}

		string outputDir = null;


		ArrayList lookupTypes = new ArrayList();
		ArrayList assemblies = new ArrayList();

		ArrayList schemaNames = new ArrayList();
		ArrayList inferenceNames = new ArrayList();
		ArrayList elements = new ArrayList();
		string language = null;
		string namesp = null;
		string uri = null;
		string providerOption = null;
		CodeDomProvider provider;

		public void Run (string[] args)
		{
			ArrayList unknownFiles = new ArrayList();
			bool generateClasses = false;
			bool readingFiles = true;
			bool schemasOptions = false;
			bool assemblyOptions = false;
			bool generateDataset = false;
			bool inference = false;

			foreach (string arg in args)
			{
				if (!arg.StartsWith ("--") && !arg.StartsWith ("/") ||
					(arg.StartsWith ("/") && arg.IndexOfAny (Path.InvalidPathChars) == -1)
					) 
				{
					if ((arg.EndsWith (".dll") || arg.EndsWith (".exe")) && !arg.Substring (1).StartsWith ("generator:") && !arg.Substring (1).StartsWith ("g:"))
					{
						if (!readingFiles) throw new ApplicationException (incorrectOrder);
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
					else if (arg.EndsWith (".xml"))
					{
						if (generateClasses || generateDataset) Error (duplicatedParam);
						inferenceNames.Add (arg);
						inference = true;
						continue;
					}
					else if (!arg.StartsWith ("/"))
					{
						if (!readingFiles) Error (incorrectOrder);
						unknownFiles.Add (arg);
						continue;
					}
				}

				readingFiles = false;

				int i = arg.IndexOf (":");
				if (i == -1) i = arg.Length;
				string option = arg.Substring (1,i-1);
				string param = (i<arg.Length-1) ? arg.Substring (i+1) : "";

				if (option == "classes" || option == "c")
				{
					if (generateClasses || generateDataset || inference) Error (duplicatedParam, option);
					generateClasses = true;
					schemasOptions = true;
				}
				else if (option == "dataset" || option == "d")
				{
					if (generateClasses || generateDataset || inference) Error (duplicatedParam, option);
					generateDataset = true;
					schemasOptions = true;
				}
				else if (option == "element" || option == "e")
				{
					elements.Add (param);
					schemasOptions = true;
				}
				else if (option == "language" || option == "l")
				{
					if (provider != null) Error (duplicatedParam, option);
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
				else if (option == "generator" || option == "g")
				{
					providerOption = param;
				}
				else if (option == "help" || option == "h")
				{
					Console.WriteLine (helpString);
					return;
				}
				else if (option == "nologo")
				{
					// ignore, since we do not output a logo anyway
				}
				else
					Error (unknownOption, option);
			}

			if (!schemasOptions && !assemblyOptions && !inference)
				Error (invalidParams);

			if (schemasOptions && assemblyOptions)
				Error (incompatibleArgs);

			if (assemblies.Count > 1)
				Error (tooManyAssem);

			if (outputDir == null) outputDir = ".";

			string typename = null;
			Type generatorType = null;

			if (language != null) {
				switch (language) {
				case "CS":
					provider = new CSharpCodeProvider ();
					break;
				case "VB":
					provider = new VBCodeProvider ();
					break;
				default:
					typename = StripQuot (language);

					generatorType = Type.GetType (typename);
					if (generatorType == null)
						Error (generatorTypeNotFound, typename);
					break;
				}
			}

			if (providerOption != null) {
				string param = providerOption;
				int comma = param.IndexOf (',');
				if (comma < 0) {
					typename = StripQuot (param);
					generatorType = Type.GetType (param);
				} else {
					typename = param.Substring (0, comma);
					string asmName = param.Substring (comma + 1);
					Assembly asm = Assembly.LoadFile (asmName);
					if (asm == null)
						Error (generatorAssemblyNotFound, asmName);
					generatorType = asm.GetType (typename);
				}
				if (generatorType == null)
					Error (generatorTypeNotFound, typename);
			}
			if (generatorType != null) {
				if (!generatorType.IsSubclassOf (typeof (CodeDomProvider)))
					Error (generatorTypeIsNotCodeGenerator, typename);
				try {
					provider = (CodeDomProvider) Activator.CreateInstance (generatorType, null);
				} catch (Exception ex) {
					Error (generatorThrewException, generatorType.AssemblyQualifiedName.ToString () + " --> " + ex.Message);
				}
				Console.WriteLine ("Loaded custom generator type " + generatorType + " .");
			}
			if (provider == null)
				provider = new CSharpCodeProvider ();

			if (schemasOptions)
			{
				if (!generateClasses && !generateDataset)
					Error (missingOutputForXsdInput);
				schemaNames.AddRange (unknownFiles);
				if (generateClasses)
					GenerateClasses ();
				else if (generateDataset)
					GenerateDataset ();
			}
			else if (inference)
			{
				foreach (string xmlfile in inferenceNames) {
					string genFile = Path.Combine (outputDir, Path.GetFileNameWithoutExtension (xmlfile) + ".xsd");
					DataSet ds = new DataSet ();
					ds.InferXmlSchema (xmlfile, null);
					ds.WriteXmlSchema (genFile);
					Console.WriteLine ("Written file " + genFile);
				}
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
			if (namesp == null) namesp = "Schemas";
			if (uri == null) uri = "";
			string targetFile = "";

			XmlSchemas schemas = new XmlSchemas();
			foreach (string fileName in schemaNames)
			{
				StreamReader sr = new StreamReader (fileName);
				schemas.Add (XmlSchema.Read (sr, new ValidationEventHandler (HandleValidationError)));
				sr.Close ();

				if (targetFile == "") targetFile = Path.GetFileNameWithoutExtension (fileName);
				else targetFile += "_" + Path.GetFileNameWithoutExtension (fileName);
			}

			targetFile += "." + provider.FileExtension;

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
					if (!schema.IsCompiled) schema.Compile (new ValidationEventHandler (HandleValidationError));
					foreach (XmlSchemaElement el in schema.Elements.Values)
						if (!qnames.Contains (el.QualifiedName))
							qnames.Add (el.QualifiedName);
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
			
			ICodeGenerator gen = provider.CreateGenerator();

			string genFile = Path.Combine (outputDir, targetFile);
			StreamWriter sw = new StreamWriter(genFile, false);
			gen.GenerateCodeFromCompileUnit (cunit, sw, new CodeGeneratorOptions());
			sw.Close();

			Console.WriteLine ("Written file " + genFile);
		}

		public void GenerateDataset ()
		{
			if (namesp == null) namesp = "";
			if (uri == null) uri = "";
			string targetFile = "";

			DataSet dataset = new DataSet ();
			foreach (string fileName in schemaNames)
			{
				dataset.ReadXmlSchema (fileName);

				if (targetFile == "") targetFile = Path.GetFileNameWithoutExtension (fileName);
				else targetFile += "_" + Path.GetFileNameWithoutExtension (fileName);
			}

			targetFile += "." + provider.FileExtension;

			CodeCompileUnit cunit = new CodeCompileUnit ();
			CodeNamespace codeNamespace = new CodeNamespace (namesp);
			cunit.Namespaces.Add (codeNamespace);
			codeNamespace.Comments.Add (new CodeCommentStatement ("\nThis source code was auto-generated by MonoXSD\n"));

			// Generate the code
			
			ICodeGenerator gen = provider.CreateGenerator ();

			TypedDataSetGenerator.Generate (dataset, codeNamespace, gen);

			string genFile = Path.Combine (outputDir, targetFile);
			StreamWriter sw = new StreamWriter(genFile, false);
			gen.GenerateCodeFromCompileUnit (cunit, sw, new CodeGeneratorOptions());
			sw.Close();

			Console.WriteLine ("Written file " + genFile);
		}

		void HandleValidationError (object o, ValidationEventArgs e)
		{
			Console.WriteLine ("{0}: {1} {2}",
				e.Severity == XmlSeverityType.Error ? "Error" : "Warning",
				e.Message,
				e.Exception != null ? e.Exception.Message : null);
		}

		public void Error (string msg)
		{
			throw new ApplicationException (msg);
		}

		public void Error (string msg, string param)
		{
			throw new ApplicationException (string.Format(msg,param));
		}

		private string StripQuot (string input)
		{
			if (input.Length < 2)
				return input;
			if (input [0] == '"' && input [input.Length -1] == '"' ||
				input [0] == '\'' && input [input.Length - 1] == '\'')
				return input.Substring (1, input.Length - 2);
			else
				return language;
		}
	}
}
