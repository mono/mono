//
// GenericParser.cs: The Base Parser for the Mono compilers
//
// Author: A Rafael D Teixeira (rafaelteixeirabr@hotmail.com)
//
// Licensed under the terms of the GNU GPL
//
// Copyright (C) 2001 Ximian, Inc.
//

namespace Mono.Languages
{
	using System;
	using System.Reflection;
	using System.Collections;
	using System.IO;
	using Mono.CSharp;

	/// <summary>
	/// Base class to support multiple Jay generated parsers
	/// </summary>
	public abstract class GenericParser
	{
		// ---------------------------------------------------
		// Class state

		// Count of errors found while parsing
		static protected int global_errors;

		// Maps extensions to specific parsers
		static private Hashtable mapOfParsers;

		// Indicates if parsing should be verbose
		static public bool yacc_verbose_flag = false;

		// Context to use
		static public ArrayList defines;

		// ---------------------------------------------------
		// Instance state

		// Name of the file we are parsing
		protected string name;

		// Input stream to parse from.
		protected System.IO.TextReader input;

		// Current namespace definition
		protected Namespace     current_namespace;

		// Current typecontainer definition
		protected TypeContainer current_container;
		
		// ---------------------------------------------------
		// What the descendants MUST reimplement

		/// <summary>
		/// Parses the current "input"
		/// </summary>
		public abstract int parse();

		/// <summary>
		/// Lists the extensions this parser can handle
		/// </summary>
		public abstract string[] extensions();
		/* {
			string [] list = { ".cs" };
			return list;
		} */

		// ---------------------------------------------------
		// What the descendants DONT HAVE to reimplement

		/// <summary>
		/// Initializes this parser from a file and parses it
		/// </summary>
		/// <param name="fileName">Name of the file to be parsed</param>
		/// <param name="context">Context to output the parsed tree</param>
		public int ParseFile(string fileName)
		{
			// file exceptions must be caught by caller

			global_errors = 0;
			name = fileName;
			// TODO: Encoding switching as needed
			//   We are here forcing StreamReader to assume current system codepage,
			//   because normally it defaults to UTF-8
			input = new StreamReader(fileName, System.Text.Encoding.Default); 
			//rc = context;
			return parse();
		}

		/// <summary>
		/// Initializes this parser from a string and parses it
		/// </summary>
		/// <param name="source">String to be parsed</param>
		/// <param name="sourceName">Name of the source to be parsed (just for error reporting)</param>
		/// <param name="context">Context to output the parsed tree</param>
		public int ParseString(string source, string sourceName)
		{
			global_errors = 0;
			name = sourceName;
			input = new StringReader(source);
			//rc = context;
			return parse();
		}

		// ---------------------------------------------------
		// Class methods

		static private void MapParsers()
		{

			mapOfParsers = new Hashtable();

			Assembly thisAssembly = Assembly.GetExecutingAssembly();
			foreach(Type type in thisAssembly.GetTypes())
			{
				if (type.BaseType != null)
					if (type.BaseType.FullName == "Mono.Languages.GenericParser")
					{
						GenericParser parser = (GenericParser)Activator.CreateInstance(type);
						foreach(string fileExtension in parser.extensions())
						{												
							string theFileExtension = fileExtension.ToLower();
							if (mapOfParsers.Contains(theFileExtension))
								Console.WriteLine("[TRACE] " + type.FullName + " can't try to parse '" + theFileExtension + "' files too");
							else
								mapOfParsers.Add(theFileExtension, parser);
						}
					}
			}
		}

		/// <summary>
		/// Find the descendant parser that knows how to parse the specified file
		/// based on the files extension
		/// </summary>
		/// <param name="fileName">Name of the file to be parsed</param>
		public static GenericParser GetSpecificParserFor(string fileName)
		{
			int i;
			string fileExtension;
			
			if (mapOfParsers == null)
				MapParsers();
			
			if ((i = fileName.LastIndexOf(".")) < 0)
				return null;
			else
				fileExtension = fileName.Substring(i).ToLower();

			return (GenericParser)mapOfParsers[fileExtension];
		}

		
		public static int Tokenize(string fileName)
		{
			GenericParser parser = GetSpecificParserFor(fileName);
						
			if (parser == null)
			{
				Console.WriteLine("Do not know how to compile " + fileName);
				return 1;
			}

/*			Stream input;

			try {
				input = File.OpenRead (input_file);

			} catch {
				Report.Error (2001, "Source file '" + input_file + "' could not be opened");
				return 1;
			}

			using (input){
				Tokenizer lexer = new Tokenizer (input, input_file, defines);
				int token, tokens = 0, errors = 0;

				while ((token = lexer.token ()) != Token.EOF){
					Location l = lexer.Location;
					tokens++;
					if (token == Token.ERROR)
						errors++;
				}
				Console.WriteLine ("Tokenized: " + tokens + " found " + errors + " errors");
			}
*/			
			return 0;
		}


		/// <summary>
		/// Find the descendant parser that knows how to parse the specified file
		/// based on the files extension, and parses it using the chosen parser
		/// </summary>
		/// <param name="fileName">Name of the file to be parsed</param>
		/// <param name="context">Context to output the parsed tree</param>
		public static int Parse(string fileName)
		{
			int errors;
			GenericParser parser = GetSpecificParserFor(fileName);
						
			if (parser == null)
			{
				Console.WriteLine("Do not know how to compile " + fileName);
				return 1;
			}
			
			try 
			{
				errors = parser.ParseFile(fileName);
			} 
			catch (FileNotFoundException ex)
			{
				error(2001, "Source file \'" + fileName + "\' could not be found!!!");
				Console.WriteLine (ex);
				return 1;
			}
			catch (Exception ex)
			{
				Console.WriteLine (ex);
				Console.WriteLine ("Compilation aborted");
				return 1;
			}
			
			return errors;
		}

		// <summary>
		//   Given the @class_name name, it creates a fully qualified name
		//   based on the containing declaration space
		// </summary>
		protected string MakeName(string class_name)
		{
			string ns = current_namespace.Name;
			string container_name = current_container.Name;

			if (container_name == "")
			{
				if (ns != "")
					return ns + "." + class_name;
				else
					return class_name;
			} 
			else
				return container_name + "." + class_name;
		}

		// <summary>
		//   Used to report back to the user the result of a declaration
		//   in the current declaration space
		// </summary>
		protected void CheckDef (AdditionResult result, string name, Location l)
		{
			if (result == AdditionResult.Success)
				return;

			switch (result)
			{
				case AdditionResult.NameExists:
					Report.Error (102, l, "The container '" + current_container.Name + 
						"' already contains a definition for '"+
						name + "'");
					break;


					//
					// This is handled only for static Constructors, because
					// in reality we handle these by the semantic analysis later
					//
				case AdditionResult.MethodExists:
					Report.Error (
						111, l, "Class `"+current_container.Name+
						"' already defines a member called '" + 
						name + "' with the same parameter types (more than one default constructor)");
					break;

				case AdditionResult.EnclosingClash:
					Report.Error (542, l, "Member names cannot be the same as their enclosing type");
					break;
		
				case AdditionResult.NotAConstructor:
					Report.Error (1520, l, "Class, struct, or interface method must have a return type");
					break;
			}
		}

		// <summary>
		//   Used to report back to the user the result of a declaration
		//   in the current declaration space
		// </summary>
		protected void CheckDef (bool result, string name, Location l)
		{
			if (result)
				return;
			CheckDef (AdditionResult.NameExists, name, l);
		}


		/// <summary>
		/// Emits error messages and increments a global count of them
		/// </summary>
		/// <param name="code"></param>
		/// <param name="desc"></param>
		static public void error (int code, string desc)
		{
			Console.WriteLine ("error MC"+code+": "+ desc);
			global_errors++;
		}

		// Emits error messages with location info.
		// FIXME : Ideally, all error reporting should happen
		// with Report.Error but how do you get at that non-static
		// method everywhere you need it ?
		static public void error (int code, Mono.CSharp.Location l, string text)
		{
			Console.WriteLine (l.Name + "(" + l.Row + ",?" + /*l.Col +*/
					   "): error MC" + code + ": " + text);
			global_errors++;
		}
		
		// ---------------------------------------------------
		// Constructors

		public GenericParser()
		{
			// DO NOTHING
		}

	}
}



