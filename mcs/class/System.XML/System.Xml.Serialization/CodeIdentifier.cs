//
// System.Xml.Serialization.CodeIdentifier.cs
//
// Author: 
//    Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;

namespace System.Xml.Serialization {
	public class CodeIdentifier {

		public CodeIdentifier ()
		{
		}

		public static string MakeCamel (string identifier)
		{
			string validIdentifier = MakeValid (identifier);
			return (Char.ToLower (validIdentifier[0]) + validIdentifier.Substring (1));
		}

		public static string MakePascal (string identifier)
		{
			string validIdentifier = MakeValid (identifier);
			return (Char.ToUpper (validIdentifier[0]) + validIdentifier.Substring (1));
		}

		public static string MakeValid (string identifier)
		{
			if (identifier == null)
				throw new NullReferenceException ();
			if (identifier.Length == 0)
				return identifier;

			string output = "";

			if (!Char.IsLetter (identifier[0]) && (identifier[0]!='_') )
				output = "Item";

			foreach (char c in identifier) 
				if (Char.IsLetterOrDigit (c) || c == '_')
					output += c;

			return output;
		}
	}
}
