//
// GenericParser.cs: The Base Parser for the Mono compilers
//
// Author: A Rafael D Teixeira (rafaelteixeirabr@hotmail.com)
//
// Licensed under the terms of the GNU GPL
//
// Copyright (C) 2001 A Rafael D Teixeira
//

#define TRACE

namespace Mono.Languages
{
	using System;
	using System.Reflection;
	using System.Diagnostics;
	using System.Collections;
	using System.IO;
	using CIR; // FIXME: renaming to Mono.Languages still pending


	/// <summary>
	/// Base class to support multiple Jay generated parsers
	/// </summary>
	public abstract class GenericParser
	{
		// ---------------------------------------------------
		// Class state

		// Count of errors found while parsing
		static protected int global_errors;

		// Indicates if parsing should be verbose
		static private Hashtable mapOfParsers;

		// ---------------------------------------------------
		// Instance state

		// Indicates if parsing should be verbose
		protected bool yacc_verbose_flag = false;

		// Name of the file we are parsing
		protected string name;

		// Input stream to parse from.
		protected System.IO.TextReader input;

		// Context to use
		protected RootContext rc;

		// ---------------------------------------------------
		// What the descendants MUST reimplement

		/// <summary>
		/// Parses the current "input"
		/// </summary>
		public abstract int parse ();

		/// <summary>
		/// Lists the extensions this parser can work
		/// </summary>
		public virtual string[] GetExtensions()
		{
			string [] list = { ".cs" };
			return list;
		}

		// ---------------------------------------------------
		// What the descendants DONT HAVE to reimplement

		/// <summary>
		/// Initializes this parser from a file and parses it
		/// </summary>
		/// <param name="fileName">Name of the file to be parsed</param>
		/// <param name="context">Context to output the parsed tree</param>
		public int ParseFile(string fileName, RootContext context)
		{
			// file exceptions must be caught by caller

			global_errors = 0;
			name = fileName;
			// TODO: Encoding switching as needed
			//       We are here forcing StreamReader to assume current system codepage,
			//		 because normally it defaults to UTF-8
			input = new StreamReader(fileName, System.Text.Encoding.Default); 
			rc = context;
			return parse();
		}

		/// <summary>
		/// Initializes this parser from a string and parses it
		/// </summary>
		/// <param name="source">String to be parsed</param>
		/// <param name="sourceName">Name of the source to be parsed (just for error reporting)</param>
		/// <param name="context">Context to output the parsed tree</param>
		public int ParseString(string source, string sourceName, RootContext context)
		{
			global_errors = 0;
			name = sourceName;
			input = new StringReader(source);
			rc = context;
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
						foreach(string fileExtension in parser.GetExtensions())
						{												
							string theFileExtension = fileExtension.ToLower();
							if (mapOfParsers.Contains(theFileExtension))
								Trace.WriteLine("[TRACE] " + type.FullName + " can't try to parse '" + theFileExtension + "' files too");
							else
							{
								mapOfParsers.Add(theFileExtension, parser);
								Trace.WriteLine("[TRACE] " + type.FullName + " parses '" + theFileExtension + "' files");
							}
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
			if (mapOfParsers == null)
				MapParsers();
			
			string fileExtension = fileName.Substring(fileName.LastIndexOf(".")).ToLower();

			return (GenericParser)mapOfParsers[fileExtension];
		}

		/// <summary>
		/// Emits error messages and increments a global count of them
		/// </summary>
		/// <param name="code"></param>
		/// <param name="desc"></param>
		static public void error (int code, string desc)
		{
			Console.WriteLine ("Error "+code+": "+ desc);
			global_errors++;
		}

		// Emits error messages with location info.
		// FIXME : Ideally, all error reporting should happen
		// with Report.Error but how do you get at that non-static
		// method everywhere you need it ?
		static public void error (int code, CIR.Location l, string text)
		{
			Console.WriteLine (l.Name + "(" + l.Row + /* "," + l.Col + */
					   "): Error CS" + code + ": " + text);
			global_errors++;
		}
		
		// ---------------------------------------------------
		// Constructors

		public GenericParser()
		{
			// DO NOTHING
		}

		// ---------------------------------------------------
		// Properties

		public bool yacc_verbose
		{
			set
			{
				yacc_verbose_flag = value;
			}

			get
			{
				return yacc_verbose_flag;
			}
		}
	}
}



