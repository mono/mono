//
// GenericParser.cs: The Base Parser for the Mono compilers
//
// Author: A Rafael D Teixeira (rafaelteixeirabr@hotmail.com)
//
// Licensed under the terms of the GNU GPL
//
// Copyright (C) 2001 A Rafael D Teixeira
//
using System;
using System.Text;

namespace Mono.Languages
{
	using System.Collections;

	/// <summary>
	/// Base class to support multiple Jay generated parsers
	/// </summary>
	public abstract class GenericParser
	{
		static protected int global_errors;

		// Name of the file we are parsing
		public string name;

		// Input stream to parse from.
		public System.IO.Stream input;

		public abstract int parse ();

		public virtual string[] extensions()
		{
			string [] list = { ".cs" };
			return list;
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
			Console.WriteLine (l.Name + "(" + l.Row + "," + l.Col +
					   "): Error CS" + code + ": " + text);
			global_errors++;
		}
		
		public GenericParser()
		{
			//
			// DO NOTHING: Derived classes should do their iniatilization here duties
			//
		}

		protected bool yacc_verbose_flag = false;

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



