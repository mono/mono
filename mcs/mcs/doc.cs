//
// doc.cs: Support for XML documentation comment.
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Licensed under the terms of the GNU GPL
//
// (C) 2004 Novell, Inc.
//
//

#if ! BOOTSTRAP_WITH_OLDLIB
using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Xml;

using Mono.CompilerServices.SymbolWriter;

namespace Mono.CSharp {

	//
	// Support class for XML documentation.
	//
	public class DocUtil
	{
		// TypeContainer

		//
		// Generates xml doc comments (if any), and if required,
		// handle warning report.
		//
		internal static void GenerateTypeDocComment (TypeContainer t,
			DeclSpace ds)
		{
			GenerateDocComment (t, ds);

			if (t.DefaultStaticConstructor != null)
				t.DefaultStaticConstructor.GenerateDocComment (t);

			if (t.InstanceConstructors != null)
				foreach (Constructor c in t.InstanceConstructors)
					c.GenerateDocComment (t);

			if (t.Types != null)
				foreach (TypeContainer tc in t.Types)
					tc.GenerateDocComment (t);

			if (t.Parts != null) {
				IDictionary comments = RootContext.Documentation.PartialComments;
				foreach (ClassPart cp in t.Parts) {
					if (cp.DocComment == null)
						continue;
					comments [cp] = cp;
				}
			}

			if (t.Enums != null)
				foreach (Enum en in t.Enums)
					en.GenerateDocComment (t);

			if (t.Constants != null)
				foreach (Const c in t.Constants)
					c.GenerateDocComment (t);

			if (t.Fields != null)
				foreach (Field f in t.Fields)
					f.GenerateDocComment (t);

			if (t.Events != null)
				foreach (Event e in t.Events)
					e.GenerateDocComment (t);

			if (t.Indexers != null)
				foreach (Indexer ix in t.Indexers)
					ix.GenerateDocComment (t);

			if (t.Properties != null)
				foreach (Property p in t.Properties)
					p.GenerateDocComment (t);

			if (t.Methods != null)
				foreach (Method m in t.Methods)
					m.GenerateDocComment (t);

			if (t.Operators != null)
				foreach (Operator o in t.Operators)
					o.GenerateDocComment (t);
		}

		// MemberCore
		private static readonly string lineHead =
			Environment.NewLine + "            ";

		private static XmlNode GetDocCommentNode (MemberCore mc,
			string name)
		{
			// FIXME: It could be even optimizable as not
			// to use XmlDocument. But anyways the nodes
			// are not kept in memory.
			XmlDocument doc = RootContext.Documentation.XmlDocumentation;
			try {
				XmlElement el = doc.CreateElement ("member");
				el.SetAttribute ("name", name);
				string normalized = mc.DocComment;
				el.InnerXml = normalized;
				// csc keeps lines as written in the sources
				// and inserts formatting indentation (which 
				// is different from XmlTextWriter.Formatting
				// one), but when a start tag contains an 
				// endline, it joins the next line. We don't
				// have to follow such a hacky behavior.
				string [] split =
					normalized.Split ('\n');
				int j = 0;
				for (int i = 0; i < split.Length; i++) {
					string s = split [i].TrimEnd ();
					if (s.Length > 0)
						split [j++] = s;
				}
				el.InnerXml = lineHead + String.Join (
					lineHead, split, 0, j);
				return el;
			} catch (XmlException ex) {
				Report.Warning (1570, 1, mc.Location, "XML comment on '{0}' has non-well-formed XML ({1}).", name, ex.Message);
				XmlComment com = doc.CreateComment (String.Format ("FIXME: Invalid documentation markup was found for member {0}", name));
				return com;
			}
		}

		//
		// Generates xml doc comments (if any), and if required,
		// handle warning report.
		//
		internal static void GenerateDocComment (MemberCore mc,
			DeclSpace ds)
		{
			if (mc.DocComment != null) {
				string name = mc.GetDocCommentName (ds);

				XmlNode n = GetDocCommentNode (mc, name);

				XmlElement el = n as XmlElement;
				if (el != null) {
					mc.OnGenerateDocComment (ds, el);

					// FIXME: it could be done with XmlReader
					foreach (XmlElement inc in n.SelectNodes (".//include"))
						HandleInclude (mc, inc);

					// FIXME: it could be done with XmlReader
					DeclSpace dsTarget = mc as DeclSpace;
					if (dsTarget == null)
						dsTarget = ds;

					foreach (XmlElement see in n.SelectNodes (".//see"))
						HandleSee (mc, dsTarget, see);
					foreach (XmlElement seealso in n.SelectNodes (".//seealso"))
						HandleSeeAlso (mc, dsTarget, seealso);
					foreach (XmlElement see in n.SelectNodes (".//exception"))
						HandleException (mc, dsTarget, see);
				}

				n.WriteTo (RootContext.Documentation.XmlCommentOutput);
			}
			else if (mc.IsExposedFromAssembly (ds) &&
				// There are no warnings when the container also
				// misses documentations.
				(ds == null || ds.DocComment != null))
			{
				Report.Warning (1591, 4, mc.Location,
					"Missing XML comment for publicly visible type or member '{0}'", mc.GetSignatureForError ());
			}
		}

		//
		// Processes "include" element. Check included file and
		// embed the document content inside this documentation node.
		//
		private static void HandleInclude (MemberCore mc, XmlElement el)
		{
			string file = el.GetAttribute ("file");
			string path = el.GetAttribute ("path");
			if (file == "") {
				Report.Warning (1590, 1, mc.Location, "Invalid XML 'include' element; Missing 'file' attribute.");
				el.ParentNode.InsertBefore (el.OwnerDocument.CreateComment (" Include tag is invalid "), el);
			}
			else if (path == "") {
				Report.Warning (1590, 1, mc.Location, "Invalid XML 'include' element; Missing 'path' attribute.");
				el.ParentNode.InsertBefore (el.OwnerDocument.CreateComment (" Include tag is invalid "), el);
			}
			else {
				XmlDocument doc = RootContext.Documentation.StoredDocuments [file] as XmlDocument;
				if (doc == null) {
					try {
						doc = new XmlDocument ();
						doc.Load (file);
						RootContext.Documentation.StoredDocuments.Add (file, doc);
					} catch (Exception) {
						el.ParentNode.InsertBefore (el.OwnerDocument.CreateComment (String.Format (" Badly formed XML in at comment file '{0}': cannot be included ", file)), el);
						Report.Warning (1592, 1, mc.Location, "Badly formed XML in included comments file -- '{0}'", file);
					}
				}
				bool keepIncludeNode = false;
				if (doc != null) {
					try {
						XmlNodeList nl = doc.SelectNodes (path);
						if (nl.Count == 0) {
							el.ParentNode.InsertBefore (el.OwnerDocument.CreateComment (" No matching elements were found for the include tag embedded here. "), el);
					
							keepIncludeNode = true;
						}
						foreach (XmlNode n in nl)
							el.ParentNode.InsertBefore (el.OwnerDocument.ImportNode (n, true), el);
					} catch (Exception ex) {
						el.ParentNode.InsertBefore (el.OwnerDocument.CreateComment (" Failed to insert some or all of included XML "), el);
						Report.Warning (1589, 1, mc.Location, "Unable to include XML fragment '{0}' of file {1} -- {2}.", path, file, ex.Message);
					}
				}
				if (!keepIncludeNode)
					el.ParentNode.RemoveChild (el);
			}
		}

		//
		// Handles <see> elements.
		//
		private static void HandleSee (MemberCore mc,
			DeclSpace ds, XmlElement see)
		{
			HandleXrefCommon (mc, ds, see);
		}

		//
		// Handles <seealso> elements.
		//
		private static void HandleSeeAlso (MemberCore mc,
			DeclSpace ds, XmlElement seealso)
		{
			HandleXrefCommon (mc, ds, seealso);
		}

		//
		// Handles <exception> elements.
		//
		private static void HandleException (MemberCore mc,
			DeclSpace ds, XmlElement seealso)
		{
			HandleXrefCommon (mc, ds, seealso);
		}

		static readonly char [] wsChars =
			new char [] {' ', '\t', '\n', '\r'};

		//
		// returns a full runtime type name from a name which might
		// be C# specific type name.
		//
		private static Type FindDocumentedType (MemberCore mc,
			string name, DeclSpace ds, bool allowAlias)
		{
			bool isArray = false;
			string identifier = name;
			if (name [name.Length - 1] == ']') {
				string tmp = name.Substring (0, name.Length - 1).Trim (wsChars);
				if (tmp [tmp.Length - 1] == '[') {
					identifier = tmp.Substring (0, tmp.Length - 1).Trim (wsChars);
					isArray = true;
				}
			}
			Type t = FindDocumentedTypeNonArray (mc, identifier,
				ds, allowAlias);
			if (t != null && isArray)
				t = Array.CreateInstance (t, 0).GetType ();
			return t;
		}

		private static Type FindDocumentedTypeNonArray (MemberCore mc,
			string identifier, DeclSpace ds, bool allowAlias)
		{
			switch (identifier) {
			case "int":
				return typeof (int);
			case "uint":
				return typeof (uint);
			case "short":
				return typeof (short);
			case "ushort":
				return typeof (ushort);
			case "long":
				return typeof (long);
			case "ulong":
				return typeof (ulong);
			case "float":
				return typeof (float);
			case "double":
				return typeof (double);
			case "char":
				return typeof (char);
			case "decimal":
				return typeof (decimal);
			case "byte":
				return typeof (byte);
			case "sbyte":
				return typeof (sbyte);
			case "object":
				return typeof (object);
			case "bool":
				return typeof (bool);
			case "string":
				return typeof (string);
			case "void":
				return typeof (void);
			}
			if (allowAlias) {
				IAlias alias = ds.LookupAlias (identifier);
				if (alias != null)
					identifier = alias.Name;
			}
			Type t = ds.FindType (mc.Location, identifier);
			if (t == null)
				t = TypeManager.LookupTypeDirect (identifier);
			return t;
		}

		//
		// Returns a MemberInfo that is referenced in XML documentation
		// (by "see" or "seealso" elements).
		//
		private static MemberInfo FindDocumentedMember (MemberCore mc,
			Type type, string memberName, Type [] paramList, 
			DeclSpace ds, out int warningType, string cref)
		{
			warningType = 0;
			MethodSignature msig = new MethodSignature (memberName, null, paramList);
			MemberInfo [] mis = type.FindMembers (
				MemberTypes.All,
				BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance,
				MethodSignature.method_signature_filter,
				msig);
			if (mis.Length > 0)
				return mis [0];

			if (paramList.Length == 0) {
				// search for fields/events etc.
				mis = type.FindMembers (
					MemberTypes.All,
					BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance,
					Type.FilterName,
					memberName);
				return (mis.Length > 0) ? mis [0] : null;
			}

			// search for operators (whose parameters exactly
			// matches with the list) and possibly report CS1581.
			string oper = null;
			string returnTypeName = null;
			if (memberName.StartsWith ("implicit operator ")) {
				oper = "op_Implicit";
				returnTypeName = memberName.Substring (18).Trim (wsChars);
			}
			else if (memberName.StartsWith ("explicit operator ")) {
				oper = "op_Explicit";
				returnTypeName = memberName.Substring (18).Trim (wsChars);
			}
			else if (memberName.StartsWith ("operator ")) {
				oper = memberName.Substring (9).Trim (wsChars);
				switch (oper) {
				// either unary or binary
				case "+":
					oper = paramList.Length == 2 ?
						Binary.oper_names [(int) Binary.Operator.Addition] :
						Unary.oper_names [(int) Unary.Operator.UnaryPlus];
					break;
				case "-":
					oper = paramList.Length == 2 ?
						Binary.oper_names [(int) Binary.Operator.Subtraction] :
						Unary.oper_names [(int) Unary.Operator.UnaryNegation];
					break;
				// unary
				case "!":
					oper = Unary.oper_names [(int) Unary.Operator.LogicalNot]; break;
				case "~":
					oper = Unary.oper_names [(int) Unary.Operator.OnesComplement]; break;
					
				case "++":
					oper = "op_Increment"; break;
				case "--":
					oper = "op_Decrement"; break;
				case "true":
					oper = "op_True"; break;
				case "false":
					oper = "op_False"; break;
				// binary
				case "*":
					oper = Binary.oper_names [(int) Binary.Operator.Multiply]; break;
				case "/":
					oper = Binary.oper_names [(int) Binary.Operator.Division]; break;
				case "%":
					oper = Binary.oper_names [(int) Binary.Operator.Modulus]; break;
				case "&":
					oper = Binary.oper_names [(int) Binary.Operator.BitwiseAnd]; break;
				case "|":
					oper = Binary.oper_names [(int) Binary.Operator.BitwiseOr]; break;
				case "^":
					oper = Binary.oper_names [(int) Binary.Operator.ExclusiveOr]; break;
				case "<<":
					oper = Binary.oper_names [(int) Binary.Operator.LeftShift]; break;
				case ">>":
					oper = Binary.oper_names [(int) Binary.Operator.RightShift]; break;
				case "==":
					oper = Binary.oper_names [(int) Binary.Operator.Equality]; break;
				case "!=":
					oper = Binary.oper_names [(int) Binary.Operator.Inequality]; break;
				case "<":
					oper = Binary.oper_names [(int) Binary.Operator.LessThan]; break;
				case ">":
					oper = Binary.oper_names [(int) Binary.Operator.GreaterThan]; break;
				case "<=":
					oper = Binary.oper_names [(int) Binary.Operator.LessThanOrEqual]; break;
				case ">=":
					oper = Binary.oper_names [(int) Binary.Operator.GreaterThanOrEqual]; break;
				default:
					warningType = 1584;
					Report.Warning (1020, 1, mc.Location, "Overloadable {0} operator is expected", paramList.Length == 2 ? "binary" : "unary");
					Report.Warning (1584, 1, mc.Location, "XML comment on '{0}' has syntactically incorrect attribute '{1}'", mc.GetSignatureForError (), cref);
					return null;
				}
			}
			// here we still does not consider return type (to
			// detect CS1581 or CS1002+CS1584).
			msig = new MethodSignature (oper, null, paramList);
			mis = type.FindMembers (
				MemberTypes.Method,
				BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
				MethodSignature.method_signature_filter,
				msig);
			if (mis.Length == 0)
				return null; // CS1574
			MemberInfo mi = mis [0];
			Type expected = mi is MethodInfo ?
				((MethodInfo) mi).ReturnType :
				mi is PropertyInfo ?
				((PropertyInfo) mi).PropertyType :
				null;
			if (returnTypeName != null) {
				Type returnType = FindDocumentedType (mc, returnTypeName, ds, true);
				if (returnType == null || returnType != expected) {
					warningType = 1581;
					Report.Warning (1581, 1, mc.Location, "Invalid return type in XML comment cref attribute '{0}'", cref);
					return null;
				}
			}
			return mis [0];
		}

		private static Type [] emptyParamList = new Type [0];

		//
		// Processes "see" or "seealso" elements.
		// Checks cref attribute.
		//
		private static void HandleXrefCommon (MemberCore mc,
			DeclSpace ds, XmlElement xref)
		{
			string cref = xref.GetAttribute ("cref").Trim (wsChars);
			// when, XmlReader, "if (cref == null)"
			if (!xref.HasAttribute ("cref"))
				return;
			if (cref.Length == 0)
				Report.Warning (1001, 1, mc.Location, "Identifier expected");
				// ... and continue until CS1584.

			string signature; // "x:" are stripped
			string name; // method invokation "(...)" are removed
			string identifiers; // array indexer "[]" are removed
			string parameters; // method parameter list

			// strip 'T:' 'M:' 'F:' 'P:' 'E:' etc.
			// Here, MS ignores its member kind. No idea why.
			if (cref.Length > 2 && cref [1] == ':')
				signature = cref.Substring (2).Trim (wsChars);
			else
				signature = cref;

			int parensPos = signature.IndexOf ('(');
			if (parensPos > 0 && signature [signature.Length - 1] == ')') {
				name = signature.Substring (0, parensPos).Trim (wsChars);
				parameters = signature.Substring (parensPos + 1, signature.Length - parensPos - 2);
			}
			else {
				name = signature;
				parameters = String.Empty;
			}

			string identifier = name;

			if (name.Length > 0 && name [name.Length - 1] == ']') {
				string tmp = name.Substring (0, name.Length - 1).Trim (wsChars);
				if (tmp [tmp.Length - 1] == '[')
					identifier = tmp.Substring (0, tmp.Length - 1).Trim (wsChars);
			}

			// Check if identifier is valid.
			// This check is not necessary to mark as error, but
			// csc specially reports CS1584 for wrong identifiers.
			foreach (string nameElem in identifier.Split ('.')) {
				if (!Tokenizer.IsValidIdentifier (nameElem)
					&& nameElem.IndexOf ("operator") < 0) {
					if (nameElem.EndsWith ("[]") &&
						Tokenizer.IsValidIdentifier (
						nameElem.Substring (
						0, nameElem.Length - 2)))
						continue;

					Report.Warning (1584, 1, mc.Location, "XML comment on '{0}' has syntactically incorrect attribute '{1}'", mc.GetSignatureForError (), cref);
					xref.SetAttribute ("cref", "!:" + signature);
					return;
				}
			}

			// check if parameters are valid
			Type [] parameterTypes = emptyParamList;
			if (parameters.Length > 0) {
				string [] paramList = parameters.Split (',');
				ArrayList plist = new ArrayList ();
				for (int i = 0; i < paramList.Length; i++) {
					string paramTypeName = paramList [i].Trim (wsChars);
					Type paramType = FindDocumentedType (mc, paramTypeName, ds, true);
					if (paramType == null) {
						Report.Warning (1580, 1, mc.Location, "Invalid type for parameter '{0}' in XML comment cref attribute '{1}'", i + 1, cref);
						return;
					}
					plist.Add (paramType);
				}
				parameterTypes = plist.ToArray (typeof (Type)) as Type [];
				StringBuilder sb = new StringBuilder ();
				sb.Append ('(');
				for (int i = 0; i < parameterTypes.Length; i++) {
					Type t = parameterTypes [i];
					if (sb.Length > 1)
						sb.Append (',');
					sb.Append (t.FullName.Replace ('+', '.'));
				}
				sb.Append (')');
				parameters = sb.ToString ();
			}

			Type type = FindDocumentedType (mc, name, ds, true);
			if (type != null) {
				xref.SetAttribute ("cref", "T:" + type.FullName.Replace ("+", "."));
				return; // a type
			}

			// don't use identifier here. System[] is not alloed.
			if (Namespace.IsNamespace (name)) {
				xref.SetAttribute ("cref", "N:" + name);
				return; // a namespace
			}

			int period = name.LastIndexOf ('.');
			if (period > 0) {
				string typeName = name.Substring (0, period);
				string memberName = name.Substring (period + 1);
				type = FindDocumentedType (mc, typeName, ds, false);
				int warnResult;
				if (type != null) {
					MemberInfo mi = FindDocumentedMember (mc, type, memberName, parameterTypes, ds, out warnResult, cref);
					if (warnResult > 0)
						return;
					if (mi != null) {
						xref.SetAttribute ("cref", GetMemberDocHead (mi.MemberType) + type.FullName.Replace ("+", ".") + "." + memberName + parameters);
						return; // a member of a type
					}
				}
			}
			else {
				int warnResult;
				MemberInfo mi = FindDocumentedMember (mc, ds.TypeBuilder, name, parameterTypes, ds, out warnResult, cref);
				if (warnResult > 0)
					return;
				if (mi != null) {
					xref.SetAttribute ("cref", GetMemberDocHead (mi.MemberType) + ds.TypeBuilder.FullName.Replace ("+", ".") + "." + name);
					return; // local member name
				}
			}

			Report.Warning (1574, 1, mc.Location, "XML comment on '{0}' has cref attribute '{1}' that could not be resolved in '{2}'.", mc.GetSignatureForError (), cref, ds.GetSignatureForError ());

			xref.SetAttribute ("cref", "!:" + name);
		}

		//
		// Get a prefix from member type for XML documentation (used
		// to formalize cref target name).
		//
		static string GetMemberDocHead (MemberTypes type)
		{
			switch (type) {
			case MemberTypes.Constructor:
			case MemberTypes.Method:
				return "M:";
			case MemberTypes.Event:
				return "E:";
			case MemberTypes.Field:
				return "F:";
			case MemberTypes.NestedType:
			case MemberTypes.TypeInfo:
				return "T:";
			case MemberTypes.Property:
				return "P:";
			}
			return "!:";
		}

		// MethodCore

		//
		// Returns a string that represents the signature for this 
		// member which should be used in XML documentation.
		//
		public static string GetMethodDocCommentName (MethodCore mc, DeclSpace ds)
		{
			Parameter [] plist = mc.Parameters.FixedParameters;
			Parameter parr = mc.Parameters.ArrayParameter;
			string paramSpec = String.Empty;
			if (plist != null) {
				StringBuilder psb = new StringBuilder ();
				foreach (Parameter p in plist) {
					psb.Append (psb.Length != 0 ? "," : "(");
					psb.Append (p.ParameterType.FullName.Replace ("+", "."));
				}
				paramSpec = psb.ToString ();
			}
			if (parr != null)
				paramSpec += String.Concat (
					paramSpec == String.Empty ? "(" : ",",
					parr.ParameterType.FullName.Replace ("+", "."));

			if (paramSpec.Length > 0)
				paramSpec += ")";

			string name = mc is Constructor ? "#ctor" : mc.Name;
			return String.Concat (mc.DocCommentHeader, ds.Name, ".", name, paramSpec);
		}

		//
		// Raised (and passed an XmlElement that contains the comment)
		// when GenerateDocComment is writing documentation expectedly.
		//
		// FIXME: with a few effort, it could be done with XmlReader,
		// that means removal of DOM use.
		//
		internal static void OnMethodGenerateDocComment (
			MethodCore mc, DeclSpace ds, XmlElement el)
		{
			Hashtable paramTags = new Hashtable ();
			foreach (XmlElement pelem in el.SelectNodes ("param")) {
				int i;
				string xname = pelem.GetAttribute ("name");
				if (xname == "")
					continue; // really? but MS looks doing so
				if (xname != "" && mc.Parameters.GetParameterByName (xname, out i) == null)
					Report.Warning (1572, 2, mc.Location, "XML comment on '{0}' has a 'param' tag for '{1}', but there is no such parameter.", mc.Name, xname);
				else if (paramTags [xname] != null)
					Report.Warning (1571, 2, mc.Location, "XML comment on '{0}' has a duplicate param tag for '{1}'", mc.Name, xname);
				paramTags [xname] = xname;
			}
			Parameter [] plist = mc.Parameters.FixedParameters;
			Parameter parr = mc.Parameters.ArrayParameter;
			if (plist != null) {
				foreach (Parameter p in plist) {
					if (paramTags.Count > 0 && paramTags [p.Name] == null)
						Report.Warning (1573, 4, mc.Location, "Parameter '{0}' has no matching param tag in the XML comment for '{1}' (but other parameters do)", mc.Name, p.Name);
				}
			}
		}

		// Enum
		public static void GenerateEnumDocComment (Enum e, DeclSpace ds)
		{
			GenerateDocComment (e, ds);
			foreach (string name in e.ordered_enums) {
				MemberCore mc = e.GetDefinition (name);
				GenerateDocComment (mc, e);
			}
		}
	}

	//
	// Implements XML documentation generation.
	//
	public class Documentation
	{
		public Documentation (string xml_output_filename)
		{
			docfilename = xml_output_filename;
			XmlDocumentation = new XmlDocument ();
			XmlDocumentation.PreserveWhitespace = false;
		}

		private string docfilename;

		//
		// Used to create element which helps well-formedness checking.
		//
		public XmlDocument XmlDocumentation;

		//
		// The output for XML documentation.
		//
		public XmlWriter XmlCommentOutput;

		//
		// Stores XmlDocuments that are included in XML documentation.
		// Keys are included filenames, values are XmlDocuments.
		//
		public Hashtable StoredDocuments = new Hashtable ();

		//
		// Stores comments on partial types (should handle uniquely).
		// Keys are PartialContainers, values are comment strings
		// (didn't use StringBuilder; usually we have just 2 or more).
		//
		public IDictionary PartialComments = new ListDictionary ();

		//
		// Outputs XML documentation comment from tokenized comments.
		//
		public bool OutputDocComment (string asmfilename)
		{
			XmlTextWriter w = null;
			try {
				w = new XmlTextWriter (docfilename, null);
				w.Indentation = 4;
				w.Formatting = Formatting.Indented;
				w.WriteStartDocument ();
				w.WriteStartElement ("doc");
				w.WriteStartElement ("assembly");
				w.WriteStartElement ("name");
				w.WriteString (Path.ChangeExtension (asmfilename, null));
				w.WriteEndElement (); // name
				w.WriteEndElement (); // assembly
				w.WriteStartElement ("members");
				XmlCommentOutput = w;
				GenerateDocComment ();
				w.WriteFullEndElement (); // members
				w.WriteEndElement ();
				w.WriteWhitespace (Environment.NewLine);
				w.WriteEndDocument ();
				return true;
			} catch (Exception ex) {
				Report.Error (1569, "Error generating XML documentation file '{0}' ('{1}')", docfilename, ex.Message);
				return false;
			} finally {
				if (w != null)
					w.Close ();
			}
		}

		//
		// Fixes full type name of each documented types/members up.
		//
		public void GenerateDocComment ()
		{
			TypeContainer root = RootContext.Tree.Types;
			if (root.Interfaces != null)
				foreach (Interface i in root.Interfaces) 
					DocUtil.GenerateTypeDocComment (i, null);

			if (root.Types != null)
				foreach (TypeContainer tc in root.Types)
					DocUtil.GenerateTypeDocComment (tc, null);

			if (root.Parts != null) {
				IDictionary comments = PartialComments;
				foreach (ClassPart cp in root.Parts) {
					if (cp.DocComment == null)
						continue;
					comments [cp] = cp;
				}
			}

			if (root.Delegates != null)
				foreach (Delegate d in root.Delegates) 
					DocUtil.GenerateDocComment (d, null);

			if (root.Enums != null)
				foreach (Enum e in root.Enums)
					DocUtil.GenerateEnumDocComment (e, null);

			IDictionary table = new ListDictionary ();
			foreach (ClassPart cp in PartialComments.Keys) {
				table [cp.PartialContainer] += cp.DocComment;
			}
			foreach (PartialContainer pc in table.Keys) {
				pc.DocComment = table [pc] as string;
				DocUtil.GenerateDocComment (pc, null);
			}
		}
	}
}

#endif
