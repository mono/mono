//
// GenericParser.cs: The Base Parser for the Mono compilers
//
// Author: A Rafael D Teixeira (rafaelteixeirabr@hotmail.com)
//
// Licensed under the terms of the GNU GPL
//
// Copyright (C) 2001 Ximian, Inc.
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
		// Name of the file we are parsing
		public string name;

		// Input stream to parse from.
		public System.IO.Stream input;

		public abstract void parse ();

		public virtual string[] extensions()
		{
			string [] list = { ".cs" };
			return list;
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



