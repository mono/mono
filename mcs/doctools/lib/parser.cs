// parser.cs - Mono Documentation Lib
//
// Author: Adam Treat <manyoso@yahoo.com>
// (c) 2002 Adam Treat
// Licensed under the terms of the GNU GPL

using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Collections;

namespace Mono.Util.MonoDoc.Lib {

	public class DocParser {

		XmlTextReader xtr;
		ArrayList types;
		DocType document;

		public DocParser (string xmlfile)
		{
			xtr = new XmlTextReader (xmlfile);
			types = new ArrayList ();
			ParseDoc (xmlfile);
			types.Sort ();
		}

		public static DocType GetDoc (string xmlfile)
		{
			DocParser inst = new DocParser (xmlfile);
			return inst.document;
		}

		public ArrayList DocTypes
		{
			get {return types;}
		}

		public void ParseDoc (string xmlfile)
		{
			while(xtr.Read ()) {
				DocType type;
				if (xtr.NodeType != XmlNodeType.EndElement) {
					switch (xtr.Name) {
						case "class":
							type = new DocType ();
							type.IsClass = true;
							ParseType (type);
							continue;
						case "structure":
							type = new DocType ();
							type.IsStructure = true;
							ParseType (type);
							continue;
						case "interface":
							type = new DocType ();
							type.IsInterface = true;
							ParseType (type);
							continue;
						case "enum":
							type = new DocType ();
							type.IsEnum = true;
							ParseType (type);
							continue;
						case "delegate":
							type = new DocType ();
							type.IsDelegate = true;
							ParseType (type);
							continue;
						default:
							continue;
					}
				}
			}
		}

		public void ParseType (DocType type)
		{
			if (xtr.MoveToAttribute("name")) {
				type.Name = xtr.Value;
			}

			if (xtr.MoveToAttribute("namespace")) {
				type.Namespace = xtr.Value;
			}

			ParseTypeBody (type);
			document = type;
			type.Sort ();
			types.Add (type);
		}

		public void ParseTypeBody (DocType type)
		{
			while(xtr.Read ()) {
				DocMember member;
				if (
					xtr.Name == "class"
					|| xtr.Name == "structure"
					|| xtr.Name == "interface"
					|| xtr.Name == "enum"
					|| xtr.Name == "delegate"
				) return;
				switch (xtr.Name) {
					case "constructor":
						member = new DocMember();
						member.IsCtor = true;
						type.AddCtor (ParseMember (member));
						continue;
					case "method":
						member = new DocMember();
						member.IsMethod = true;
						type.AddMethod (ParseMember (member));
						continue;
					case "field":
						member = new DocMember();
						member.IsField = true;
						type.AddField (ParseMember (member));
						continue;
					case "property":
						member = new DocMember();
						member.IsProperty = true;
						type.AddProperty (ParseMember (member));
						continue;
					case "dtor":
						member = new DocMember();
						member.IsDtor = true;
						type.AddDtor (ParseMember (member));
						continue;
					case "event":
						member = new DocMember();
						member.IsEvent = true;
						type.AddEvent (ParseMember (member));
						continue;
					case "summary":
						if (xtr.NodeType != XmlNodeType.EndElement && xtr.Depth == 2) {
							xtr.Read ();
							type.Summary = xtr.Value;
      					}
						continue;
					case "remarks":
						if (xtr.NodeType != XmlNodeType.EndElement && xtr.Depth == 2) {
							xtr.Read ();
							type.Remarks = xtr.Value;
						}
						continue;
					default:
						continue;
				}
			}
		}

		public DocMember ParseMember (DocMember member)
		{
			if (xtr.MoveToAttribute("name")) {
				string [] s = xtr.Value.Split ('(');
				if (s.Length > 1) {
					member.Name = s[0];
					member.Args = "("+ShortArgs (member, s[1])+")";
					member.FullArgs = "("+s[1];
				} else
					member.Name = xtr.Value;
			}
			return member;
		}

		public string ShortArgs (DocMember member, string args)
		{
			StringBuilder builder = new StringBuilder ();
			string [] s = args.TrimEnd (')').Split (',');
			foreach (string st in s) {
				string [] str = st.Trim (' ').Split ('.');
				string stri = str[str.Length -1];
				builder.Append (stri+", ");
				DocParam param = new DocParam ();
				param.Name = stri;
				member.AddParam (param);
			}
			return builder.ToString ().TrimEnd (',', ' ');
		}
	}
}
