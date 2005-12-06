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
	public static class DocUtil
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
				foreach (FieldBase f in t.Fields)
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
				Report.Warning (1570, 1, mc.Location, "XML comment on `{0}' has non-well-formed XML ({1})", name, ex.Message);
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
			else if (mc.IsExposedFromAssembly (ds)) {
				Constructor c = mc as Constructor;
				if (c == null || !c.IsDefault ())
					Report.Warning (1591, 4, mc.Location,
						"Missing XML comment for publicly visible type or member `{0}'", mc.GetSignatureForError ());
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
				Report.Warning (1590, 1, mc.Location, "Invalid XML `include' element. Missing `file' attribute");
				el.ParentNode.InsertBefore (el.OwnerDocument.CreateComment (" Include tag is invalid "), el);
			}
			else if (path == "") {
				Report.Warning (1590, 1, mc.Location, "Invalid XML `include' element. Missing `path' attribute");
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
						el.ParentNode.InsertBefore (el.OwnerDocument.CreateComment (String.Format (" Badly formed XML in at comment file `{0}': cannot be included ", file)), el);
						Report.Warning (1592, 1, mc.Location, "Badly formed XML in included comments file -- `{0}'", file);
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
						Report.Warning (1589, 1, mc.Location, "Unable to include XML fragment `{0}' of file `{1}' ({2})", path, file, ex.Message);
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
		private static Type FindDocumentedType (MemberCore mc, string name, DeclSpace ds, string cref)
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
			Type t = FindDocumentedTypeNonArray (mc, identifier, ds, cref);
			if (t != null && isArray)
				t = Array.CreateInstance (t, 0).GetType ();
			return t;
		}

		private static Type FindDocumentedTypeNonArray (MemberCore mc, 
			string identifier, DeclSpace ds, string cref)
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
			FullNamedExpression e = ds.LookupType (identifier, mc.Location, false);
			if (e != null) {
				if (!(e is TypeExpr))
					return null;
				return e.Type;
			}
			int index = identifier.LastIndexOf ('.');
			if (index < 0)
				return null;
			int warn;
			Type parent = FindDocumentedType (mc, identifier.Substring (0, index), ds, cref);
			if (parent == null)
				return null;
			// no need to detect warning 419 here
			return FindDocumentedMember (mc, parent,
				identifier.Substring (index + 1),
				Type.EmptyTypes,
				ds, out warn, cref, false, null) as Type;
		}

		private static MemberInfo [] empty_member_infos =
			new MemberInfo [0];

		private static MemberInfo [] FindMethodBase (Type type,
			BindingFlags bindingFlags, MethodSignature signature)
		{
			MemberList ml = TypeManager.FindMembers (
				type,
				MemberTypes.Constructor | MemberTypes.Method | MemberTypes.Property | MemberTypes.Custom,
				bindingFlags,
				MethodSignature.method_signature_filter,
				signature);
			if (ml == null)
				return empty_member_infos;

			return FilterOverridenMembersOut (type, (MemberInfo []) ml);
		}

		static bool IsOverride (PropertyInfo deriv_prop, PropertyInfo base_prop)
		{
			if (!Invocation.IsAncestralType (base_prop.DeclaringType, deriv_prop.DeclaringType))
				return false;

			Type [] deriv_pd = TypeManager.GetArgumentTypes (deriv_prop);
			Type [] base_pd = TypeManager.GetArgumentTypes (base_prop);
		
			if (deriv_pd.Length != base_pd.Length)
				return false;

			for (int j = 0; j < deriv_pd.Length; ++j) {
				if (deriv_pd [j] != base_pd [j])
					return false;
				Type ct = TypeManager.TypeToCoreType (deriv_pd [j]);
				Type bt = TypeManager.TypeToCoreType (base_pd [j]);

				if (ct != bt)
					return false;
			}

			return true;
		}

		private static MemberInfo [] FilterOverridenMembersOut (
			Type type, MemberInfo [] ml)
		{
			if (ml == null)
				return empty_member_infos;
			if (type.IsInterface)
				return ml;

			ArrayList al = new ArrayList (ml.Length);
			for (int i = 0; i < ml.Length; i++) {
				// Interface methods which are returned
				// from the filter must exist in the 
				// target type (if there is only a 
				// private implementation, then the 
				// filter should not return it.)
				// This filtering is required to 
				// deambiguate results.
				//
				// It is common to properties, so check it here.
				if (ml [i].DeclaringType.IsInterface)
					continue;
				MethodBase mx = ml [i] as MethodBase;
				PropertyInfo px = ml [i] as PropertyInfo;
				if (mx != null || px != null) {
					bool overriden = false;
					for (int j = 0; j < ml.Length; j++) {
						if (j == i)
							continue;
						MethodBase my = ml [j] as MethodBase;
						if (mx != null && my != null &&
							Invocation.IsOverride (my, mx)) {
							overriden = true;
							break;
						}
						else if (mx != null)
							continue;
						PropertyInfo py = ml [j] as PropertyInfo;
						if (px != null && py != null &&
							IsOverride (py, px)) {
							overriden = true;
							break;
						}
					}
					if (overriden)
						continue;
				}
				al.Add (ml [i]);
			}
			return al.ToArray (typeof (MemberInfo)) as MemberInfo [];
		}

		//
		// Returns a MemberInfo that is referenced in XML documentation
		// (by "see" or "seealso" elements).
		//
		private static MemberInfo FindDocumentedMember (MemberCore mc,
			Type type, string memberName, Type [] paramList, 
			DeclSpace ds, out int warningType, string cref,
			bool warn419, string nameForError)
		{
			warningType = 0;
			MethodSignature msig = new MethodSignature (memberName, null, paramList);
			MemberInfo [] mis = FindMethodBase (type, 
				BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance,
				msig);

			if (warn419 && mis.Length > 0) {
				if (IsAmbiguous (mis))
					Report419 (mc, nameForError, mis);
				return mis [0];
			}

			if (paramList.Length == 0) {
				// search for fields/events etc.
				mis = TypeManager.MemberLookup (type, null,
					type, MemberTypes.All,
					BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance,
					memberName, null);
				mis = FilterOverridenMembersOut (type, mis);
				if (mis == null || mis.Length == 0)
					return null;
				if (warn419 && IsAmbiguous (mis))
					Report419 (mc, nameForError, mis);
				return mis [0];
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
					Report.Warning (1584, 1, mc.Location, "XML comment on `{0}' has syntactically incorrect cref attribute `{1}'",
						mc.GetSignatureForError (), cref);
					return null;
				}
			}
			// here we still don't consider return type (to
			// detect CS1581 or CS1002+CS1584).
			msig = new MethodSignature (oper, null, paramList);

			mis = FindMethodBase (type, 
				BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance,
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
				Type returnType = FindDocumentedType (mc, returnTypeName, ds, cref);
				if (returnType == null || returnType != expected) {
					warningType = 1581;
					Report.Warning (1581, 1, mc.Location, "Invalid return type in XML comment cref attribute `{0}'", cref);
					return null;
				}
			}
			return mis [0];
		}

		private static bool IsAmbiguous (MemberInfo [] members)
		{
			if (members.Length < 2)
				return false;
			if (members.Length > 2)
				return true;
			if (members [0] is EventInfo && members [1] is FieldInfo)
				return false;
			if (members [1] is EventInfo && members [0] is FieldInfo)
				return false;
			return true;
		}

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
			string parameters; // method parameter list

			// strip 'T:' 'M:' 'F:' 'P:' 'E:' etc.
			// Here, MS ignores its member kind. No idea why.
			if (cref.Length > 2 && cref [1] == ':')
				signature = cref.Substring (2).Trim (wsChars);
			else
				signature = cref;

			int parensPos = signature.IndexOf ('(');
			int bracePos = parensPos >= 0 ? -1 :
				signature.IndexOf ('[');
			if (parensPos > 0 && signature [signature.Length - 1] == ')') {
				name = signature.Substring (0, parensPos).Trim (wsChars);
				parameters = signature.Substring (parensPos + 1, signature.Length - parensPos - 2).Trim (wsChars);
			}
			else if (bracePos > 0 && signature [signature.Length - 1] == ']') {
				name = signature.Substring (0, bracePos).Trim (wsChars);
				parameters = signature.Substring (bracePos + 1, signature.Length - bracePos - 2).Trim (wsChars);
			}
			else {
				name = signature;
				parameters = String.Empty;
			}
			Normalize (mc, ref name);

			string identifier = GetBodyIdentifierFromName (name);

			// Check if identifier is valid.
			// This check is not necessary to mark as error, but
			// csc specially reports CS1584 for wrong identifiers.
			string [] nameElems = identifier.Split ('.');
			for (int i = 0; i < nameElems.Length; i++) {
				string nameElem = GetBodyIdentifierFromName (nameElems [i]);
				if (i > 0)
					Normalize (mc, ref nameElem);
				if (!Tokenizer.IsValidIdentifier (nameElem)
					&& nameElem.IndexOf ("operator") < 0) {
					Report.Warning (1584, 1, mc.Location, "XML comment on `{0}' has syntactically incorrect cref attribute `{1}'",
						mc.GetSignatureForError (), cref);
					xref.SetAttribute ("cref", "!:" + signature);
					return;
				}
			}

			// check if parameters are valid
			Type [] parameterTypes = Type.EmptyTypes;
			if (parameters.Length > 0) {
				string [] paramList = parameters.Split (',');
				ArrayList plist = new ArrayList ();
				for (int i = 0; i < paramList.Length; i++) {
					string paramTypeName = paramList [i].Trim (wsChars);
					Normalize (mc, ref paramTypeName);
					Type paramType = FindDocumentedType (mc, paramTypeName, ds, cref);
					if (paramType == null) {
						Report.Warning (1580, 1, mc.Location, "Invalid type for parameter `{0}' in XML comment cref attribute `{1}'",
							(i + 1).ToString (), cref);
						return;
					}
					plist.Add (paramType);
				}
				parameterTypes = plist.ToArray (typeof (Type)) as Type [];
			}

			Type type = FindDocumentedType (mc, name, ds, cref);
			if (type != null
				// delegate must not be referenced with args
				&& (!type.IsSubclassOf (typeof (System.Delegate))
				|| parameterTypes.Length == 0)) {
				string result = type.FullName.Replace ("+", ".")
					+ (bracePos < 0 ? String.Empty : signature.Substring (bracePos));
				xref.SetAttribute ("cref", "T:" + result);
				return; // a type
			}

			// don't use identifier here. System[] is not alloed.
			if (RootNamespace.Global.IsNamespace (name)) {
				xref.SetAttribute ("cref", "N:" + name);
				return; // a namespace
			}

			int period = name.LastIndexOf ('.');
			if (period > 0) {
				string typeName = name.Substring (0, period);
				string memberName = name.Substring (period + 1);
				Normalize (mc, ref memberName);
				type = FindDocumentedType (mc, typeName, ds, cref);
				int warnResult;
				if (type != null) {
					MemberInfo mi = FindDocumentedMember (mc, type, memberName, parameterTypes, ds, out warnResult, cref, true, name);
					if (warnResult > 0)
						return;
					if (mi != null) {
						xref.SetAttribute ("cref", GetMemberDocHead (mi.MemberType) + type.FullName.Replace ("+", ".") + "." + memberName + GetParametersFormatted (mi));
						return; // a member of a type
					}
				}
			}
			else {
				int warnResult;
				MemberInfo mi = FindDocumentedMember (mc, ds.TypeBuilder, name, parameterTypes, ds, out warnResult, cref, true, name);
				if (warnResult > 0)
					return;
				if (mi != null) {
					xref.SetAttribute ("cref", GetMemberDocHead (mi.MemberType) + ds.TypeBuilder.FullName.Replace ("+", ".") + "." + name + GetParametersFormatted (mi));
					return; // local member name
				}
			}

			Report.Warning (1574, 1, mc.Location, "XML comment on `{0}' has cref attribute `{1}' that could not be resolved",
				mc.GetSignatureForError (), cref);

			xref.SetAttribute ("cref", "!:" + name);
		}

		static string GetParametersFormatted (MemberInfo mi)
		{
			MethodBase mb = mi as MethodBase;
			bool isSetter = false;
			PropertyInfo pi = mi as PropertyInfo;
			if (pi != null) {
				mb = pi.GetGetMethod ();
				if (mb == null) {
					isSetter = true;
					mb = pi.GetSetMethod ();
				}
			}
			if (mb == null)
				return String.Empty;

			ParameterData parameters = TypeManager.GetParameterData (mb);
			if (parameters == null || parameters.Count == 0)
				return String.Empty;

			StringBuilder sb = new StringBuilder ();
			sb.Append ('(');
			for (int i = 0; i < parameters.Count; i++) {
				if (isSetter && i + 1 == parameters.Count)
					break; // skip "value".
				if (i > 0)
					sb.Append (',');
				Type t = parameters.ParameterType (i);
				sb.Append (t.FullName.Replace ('+', '.').Replace ('&', '@'));
			}
			sb.Append (')');
			return sb.ToString ();
		}

		static string GetBodyIdentifierFromName (string name)
		{
			string identifier = name;

			if (name.Length > 0 && name [name.Length - 1] == ']') {
				string tmp = name.Substring (0, name.Length - 1).Trim (wsChars);
				int last = tmp.LastIndexOf ('[');
				if (last > 0)
					identifier = tmp.Substring (0, last).Trim (wsChars);
			}

			return identifier;
		}

		static void Report419 (MemberCore mc, string memberName, MemberInfo [] mis)
		{
			Report.Warning (419, 3, mc.Location, 
				"Ambiguous reference in cref attribute `{0}'. Assuming `{1}' but other overloads including `{2}' have also matched",
				memberName,
				TypeManager.GetFullNameSignature (mis [0]),
				TypeManager.GetFullNameSignature (mis [1]));
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
					psb.Append (p.ExternalType ().FullName.Replace ("+", ".").Replace ('&', '@'));
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
			string suffix = String.Empty;
			Operator op = mc as Operator;
			if (op != null) {
				switch (op.OperatorType) {
				case Operator.OpType.Implicit:
				case Operator.OpType.Explicit:
					suffix = "~" + op.OperatorMethodBuilder.ReturnType.FullName.Replace ('+', '.');
					break;
				}
			}
			return String.Concat (mc.DocCommentHeader, ds.Name, ".", name, paramSpec, suffix);
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
					Report.Warning (1572, 2, mc.Location, "XML comment on `{0}' has a param tag for `{1}', but there is no parameter by that name",
						mc.GetSignatureForError (), xname);
				else if (paramTags [xname] != null)
					Report.Warning (1571, 2, mc.Location, "XML comment on `{0}' has a duplicate param tag for `{1}'",
						mc.GetSignatureForError (), xname);
				paramTags [xname] = xname;
			}
			Parameter [] plist = mc.Parameters.FixedParameters;
			if (plist != null) {
				foreach (Parameter p in plist) {
					if (paramTags.Count > 0 && paramTags [p.Name] == null)
						Report.Warning (1573, 4, mc.Location, "Parameter `{0}' has no matching param tag in the XML comment for `{1}'",
							p.Name, mc.GetSignatureForError ());
				}
			}
		}

		private static void Normalize (MemberCore mc, ref string name)
		{
			if (name.Length > 0 && name [0] == '@')
				name = name.Substring (1);
			else if (name == "this")
				name = "Item";
			else if (Tokenizer.IsKeyword (name) && !IsTypeName (name))
				Report.Warning (1041, 1, mc.Location, "Identifier expected. `{0}' is a keyword", name);
		}

		private static bool IsTypeName (string name)
		{
			switch (name) {
			case "bool":
			case "byte":
			case "char":
			case "decimal":
			case "double":
			case "float":
			case "int":
			case "long":
			case "object":
			case "sbyte":
			case "short":
			case "string":
			case "uint":
			case "ulong":
			case "ushort":
			case "void":
				return true;
			}
			return false;
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
				Report.Error (1569, "Error generating XML documentation file `{0}' (`{1}')", docfilename, ex.Message);
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
					e.GenerateDocComment (null);

			IDictionary table = new ListDictionary ();
			foreach (ClassPart cp in PartialComments.Keys) {
				// FIXME: IDictionary does not guarantee that the keys will be
				//        accessed in the order they were added.
				table [cp.PartialContainer] += cp.DocComment;
			}
			foreach (PartialContainer pc in table.Keys) {
				pc.DocComment = table [pc] as string;
				DocUtil.GenerateDocComment (pc, null);
			}
		}
	}
}
