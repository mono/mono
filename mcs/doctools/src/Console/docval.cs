//	docval.cs
//
//	Adam Treat (manyoso@yahoo.com)
//	(C) 2002 Adam Treat
//
//	This program is free software; you can redistribute it and/or modify
//	it under the terms of the GNU General Public License as published by
//	the Free Software Foundation; either version 2 of the License, or
//	(at your option) any later version.

using System;
using System.Xml;

namespace Mono.Util {

	class DocVal {

		string file;

		void Usage()
		{
			Console.Write ("docval [file]\n\n");
		}

		public static void Main(string[] args)
		{
			DocVal val = new DocVal(args);
		}

		public DocVal(string [] args)
		{
			bool pass = true;
			file = null;
			int argc = args.Length;

			for(int i = 0; i < argc; i++) {
				string arg = args[i];
				if(arg.EndsWith(".xml")) {
					file = arg;
				}
			}

			if(file == null) {
				Usage();
				return;
			}

			try {
				XmlTextReader read = new XmlTextReader(file);
				XmlValidatingReader validate = new XmlValidatingReader(read);
				validate.ValidationType = ValidationType.Auto;
				while (validate.Read()) {
					switch (validate.NodeType) {
						case XmlNodeType.XmlDeclaration:
							Console.WriteLine("** XML declaration");
							break;
						case XmlNodeType.DocumentType:
							Console.WriteLine("** DocumentType node");
							break;
						case XmlNodeType.Document:
							Console.WriteLine("** Document node");
							break;
						case XmlNodeType.Element:
							Console.WriteLine("** Element: {0}", validate.Name);
						break;
						case XmlNodeType.EndElement:
							Console.WriteLine("** End Element: {0}", validate.Name);
							break;
						case XmlNodeType.Text:
							Console.WriteLine("** Text: {0}", validate.Value);
							break;
						case XmlNodeType.Comment:
							Console.WriteLine("** Comment: {1}", validate.Name, validate.Value);
							break;
						case XmlNodeType.Whitespace:
							break;
						default:
							pass = false;
							Console.WriteLine("** ERROR: Unknown node type");
							break;
					}
				}
			} catch (Exception e) {
				pass = false;
				Console.WriteLine(e);
			}
			if(pass) {
				Console.Write("\n   Validation: PASSED!\n\n");
			} else {
				Console.Write("\n   Validation: FAILED!\n\n");
			}
		}
	}
}
