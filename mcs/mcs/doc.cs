//
// doc.cs: Support for XML documentation comment.
//
// Authors:
//	Atsushi Enomoto <atsushi@ximian.com>
//  Marek Safar (marek.safar@gmail.com>
//
// Dual licensed under the terms of the MIT X11 or GNU GPL
//
// Copyright 2004 Novell, Inc.
// Copyright 2011 Xamarin Inc
//
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Linq;

namespace Mono.CSharp
{
	//
	// Implements XML documentation generation.
	//
	class DocumentationBuilder
	{
		//
		// Used to create element which helps well-formedness checking.
		//
		readonly XmlDocument XmlDocumentation;

		readonly ModuleContainer module;
		readonly ModuleContainer doc_module;

		//
		// The output for XML documentation.
		//
		XmlWriter XmlCommentOutput;

		static readonly string line_head = Environment.NewLine + "            ";

		//
		// Stores XmlDocuments that are included in XML documentation.
		// Keys are included filenames, values are XmlDocuments.
		//
		Dictionary<string, XmlDocument> StoredDocuments = new Dictionary<string, XmlDocument> ();

		ParserSession session;

		public DocumentationBuilder (ModuleContainer module)
		{
			doc_module = new ModuleContainer (module.Compiler);
			doc_module.DocumentationBuilder = this;

			this.module = module;
			XmlDocumentation = new XmlDocument ();
			XmlDocumentation.PreserveWhitespace = false;
		}

		Report Report {
			get {
				return module.Compiler.Report;
			}
		}

		public MemberName ParsedName {
			get; set;
		}

		public List<DocumentationParameter> ParsedParameters {
			get; set;
		}

		public TypeExpression ParsedBuiltinType {
			get; set;
		}

		public Operator.OpType? ParsedOperator {
			get; set;
		}

		XmlNode GetDocCommentNode (MemberCore mc, string name)
		{
			// FIXME: It could be even optimizable as not
			// to use XmlDocument. But anyways the nodes
			// are not kept in memory.
			XmlDocument doc = XmlDocumentation;
			try {
				XmlElement el = doc.CreateElement ("member");
				el.SetAttribute ("name", name);
				string normalized = mc.DocComment;
				el.InnerXml = normalized;

				string [] split = normalized.Split ('\n');
				el.InnerXml = line_head + String.Join (line_head, split);
				return el;
			} catch (Exception ex) {
				Report.Warning (1570, 1, mc.Location, "XML documentation comment on `{0}' is not well-formed XML markup ({1})",
					mc.GetSignatureForError (), ex.Message);

				return doc.CreateComment (String.Format ("FIXME: Invalid documentation markup was found for member {0}", name));
			}
		}

		//
		// Generates xml doc comments (if any), and if required,
		// handle warning report.
		//
		internal void GenerateDocumentationForMember (MemberCore mc)
		{
			string name = mc.DocCommentHeader + mc.GetSignatureForDocumentation ();

			XmlNode n = GetDocCommentNode (mc, name);

			XmlElement el = n as XmlElement;
			if (el != null) {
				var pm = mc as IParametersMember;
				if (pm != null) {
					CheckParametersComments (mc, pm, el);
				}

				// FIXME: it could be done with XmlReader
				XmlNodeList nl = n.SelectNodes (".//include");
				if (nl.Count > 0) {
					// It could result in current node removal, so prepare another list to iterate.
					var al = new List<XmlNode> (nl.Count);
					foreach (XmlNode inc in nl)
						al.Add (inc);
					foreach (XmlElement inc in al)
						if (!HandleInclude (mc, inc))
							inc.ParentNode.RemoveChild (inc);
				}

				// FIXME: it could be done with XmlReader

				foreach (XmlElement see in n.SelectNodes (".//see"))
					HandleSee (mc, see);
				foreach (XmlElement seealso in n.SelectNodes (".//seealso"))
					HandleSeeAlso (mc, seealso);
				foreach (XmlElement see in n.SelectNodes (".//exception"))
					HandleException (mc, see);
				foreach (XmlElement node in n.SelectNodes (".//typeparam"))
					HandleTypeParam (mc, node);
				foreach (XmlElement node in n.SelectNodes (".//typeparamref"))
					HandleTypeParamRef (mc, node);
			}

			n.WriteTo (XmlCommentOutput);
		}

		//
		// Processes "include" element. Check included file and
		// embed the document content inside this documentation node.
		//
		bool HandleInclude (MemberCore mc, XmlElement el)
		{
			bool keep_include_node = false;
			string file = el.GetAttribute ("file");
			string path = el.GetAttribute ("path");

			if (file == "") {
				Report.Warning (1590, 1, mc.Location, "Invalid XML `include' element. Missing `file' attribute");
				el.ParentNode.InsertBefore (el.OwnerDocument.CreateComment (" Include tag is invalid "), el);
				keep_include_node = true;
			} else if (path.Length == 0) {
				Report.Warning (1590, 1, mc.Location, "Invalid XML `include' element. Missing `path' attribute");
				el.ParentNode.InsertBefore (el.OwnerDocument.CreateComment (" Include tag is invalid "), el);
				keep_include_node = true;
			} else {
				XmlDocument doc;
				Exception exception = null;
				var full_path = Path.Combine (Path.GetDirectoryName (mc.Location.NameFullPath), file);

				if (!StoredDocuments.TryGetValue (full_path, out doc)) {
					try {
						doc = new XmlDocument ();
						doc.Load (full_path);
						StoredDocuments.Add (full_path, doc);
					} catch (Exception e) {
						exception = e;
						el.ParentNode.InsertBefore (el.OwnerDocument.CreateComment (String.Format (" Badly formed XML in at comment file `{0}': cannot be included ", file)), el);
					}
				}

				if (doc != null) {
					try {
						XmlNodeList nl = doc.SelectNodes (path);
						if (nl.Count == 0) {
							el.ParentNode.InsertBefore (el.OwnerDocument.CreateComment (" No matching elements were found for the include tag embedded here. "), el);
					
							keep_include_node = true;
						}
						foreach (XmlNode n in nl)
							el.ParentNode.InsertBefore (el.OwnerDocument.ImportNode (n, true), el);
					} catch (Exception ex) {
						exception = ex;
						el.ParentNode.InsertBefore (el.OwnerDocument.CreateComment (" Failed to insert some or all of included XML "), el);
					}
				}

				if (exception != null) {
					Report.Warning (1589, 1, mc.Location, "Unable to include XML fragment `{0}' of file `{1}'. {2}",
						path, file, exception.Message);
				}
			}

			return keep_include_node;
		}

		//
		// Handles <see> elements.
		//
		void HandleSee (MemberCore mc, XmlElement see)
		{
			HandleXrefCommon (mc, see);
		}

		//
		// Handles <seealso> elements.
		//
		void HandleSeeAlso (MemberCore mc, XmlElement seealso)
		{
			HandleXrefCommon (mc, seealso);
		}

		//
		// Handles <exception> elements.
		//
		void HandleException (MemberCore mc, XmlElement seealso)
		{
			HandleXrefCommon (mc, seealso);
		}

		//
		// Handles <typeparam /> node
		//
		static void HandleTypeParam (MemberCore mc, XmlElement node)
		{
			if (!node.HasAttribute ("name"))
				return;

			string tp_name = node.GetAttribute ("name");
			if (mc.CurrentTypeParameters != null) {
				if (mc.CurrentTypeParameters.Find (tp_name) != null)
					return;
			}
			
			// TODO: CS1710, CS1712
			
			mc.Compiler.Report.Warning (1711, 2, mc.Location,
				"XML comment on `{0}' has a typeparam name `{1}' but there is no type parameter by that name",
				mc.GetSignatureForError (), tp_name);
		}

		//
		// Handles <typeparamref /> node
		//
		static void HandleTypeParamRef (MemberCore mc, XmlElement node)
		{
			if (!node.HasAttribute ("name"))
				return;

			string tp_name = node.GetAttribute ("name");
			var member = mc;
			do {
				if (member.CurrentTypeParameters != null) {
					if (member.CurrentTypeParameters.Find (tp_name) != null)
						return;
				}

				member = member.Parent;
			} while (member != null);

			mc.Compiler.Report.Warning (1735, 2, mc.Location,
				"XML comment on `{0}' has a typeparamref name `{1}' that could not be resolved",
				mc.GetSignatureForError (), tp_name);
		}

		FullNamedExpression ResolveMemberName (IMemberContext context, MemberName mn)
		{
			if (mn.Left == null)
				return context.LookupNamespaceOrType (mn.Name, mn.Arity, LookupMode.Probing, Location.Null);

			var left = ResolveMemberName (context, mn.Left);
			var ns = left as NamespaceExpression;
			if (ns != null)
				return ns.LookupTypeOrNamespace (context, mn.Name, mn.Arity, LookupMode.Probing, Location.Null);

			TypeExpr texpr = left as TypeExpr;
			if (texpr != null) {
				var found = MemberCache.FindNestedType (texpr.Type, mn.Name, mn.Arity);
				if (found != null)
					return new TypeExpression (found, Location.Null);

				return null;
			}

			return left;
		}

		//
		// Processes "see" or "seealso" elements from cref attribute.
		//
		void HandleXrefCommon (MemberCore mc, XmlElement xref)
		{
			string cref = xref.GetAttribute ("cref");
			// when, XmlReader, "if (cref == null)"
			if (!xref.HasAttribute ("cref"))
				return;

			// Nothing to be resolved the reference is marked explicitly
			if (cref.Length > 2 && cref [1] == ':')
				return;

			// Additional symbols for < and > are allowed for easier XML typing
			cref = cref.Replace ('{', '<').Replace ('}', '>');

			var encoding = module.Compiler.Settings.Encoding;
			var s = new MemoryStream (encoding.GetBytes (cref));

			var source_file = new CompilationSourceFile (doc_module, mc.Location.SourceFile);
			var report = new Report (doc_module.Compiler, new NullReportPrinter ());

			if (session == null)
				session = new ParserSession {
					UseJayGlobalArrays = true
				};

			SeekableStreamReader seekable = new SeekableStreamReader (s, encoding, session.StreamReaderBuffer);

			var parser = new CSharpParser (seekable, source_file, report, session);
			ParsedParameters = null;
			ParsedName = null;
			ParsedBuiltinType = null;
			ParsedOperator = null;
			parser.Lexer.putback_char = Tokenizer.DocumentationXref;
			parser.Lexer.parsing_generic_declaration_doc = true;
			parser.parse ();
			if (report.Errors > 0) {
				Report.Warning (1584, 1, mc.Location, "XML comment on `{0}' has syntactically incorrect cref attribute `{1}'",
					mc.GetSignatureForError (), cref);

				xref.SetAttribute ("cref", "!:" + cref);
				return;
			}

			MemberSpec member;
			string prefix = null;
			FullNamedExpression fne = null;

			//
			// Try built-in type first because we are using ParsedName as identifier of
			// member names on built-in types
			//
			if (ParsedBuiltinType != null && (ParsedParameters == null || ParsedName != null)) {
				member = ParsedBuiltinType.Type;
			} else {
				member = null;
			}

			if (ParsedName != null || ParsedOperator.HasValue) {
				TypeSpec type = null;
				string member_name = null;

				if (member == null) {
					if (ParsedOperator.HasValue) {
						type = mc.CurrentType;
					} else if (ParsedName.Left != null) {
						fne = ResolveMemberName (mc, ParsedName.Left);
						if (fne != null) {
							var ns = fne as NamespaceExpression;
							if (ns != null) {
								fne = ns.LookupTypeOrNamespace (mc, ParsedName.Name, ParsedName.Arity, LookupMode.Probing, Location.Null);
								if (fne != null) {
									member = fne.Type;
								}
							} else {
								type = fne.Type;
							}
						}
					} else {
						fne = ResolveMemberName (mc, ParsedName);
						if (fne == null) {
							type = mc.CurrentType;
						} else if (ParsedParameters == null) {
							member = fne.Type;
						} else if (fne.Type.MemberDefinition == mc.CurrentType.MemberDefinition) {
							member_name = Constructor.ConstructorName;
							type = fne.Type;
						}
					}
				} else {
					type = (TypeSpec) member;
					member = null;
				}

				if (ParsedParameters != null) {
					var old_printer = mc.Module.Compiler.Report.SetPrinter (new NullReportPrinter ());
					try {
						var context = new DocumentationMemberContext (mc, ParsedName ?? MemberName.Null);

						foreach (var pp in ParsedParameters) {
							pp.Resolve (context);
						}
					} finally {
						mc.Module.Compiler.Report.SetPrinter (old_printer);
					}
				}

				if (type != null) {
					if (member_name == null)
						member_name = ParsedOperator.HasValue ?
							Operator.GetMetadataName (ParsedOperator.Value) : ParsedName.Name;

					int parsed_param_count;
					if (ParsedOperator == Operator.OpType.Explicit || ParsedOperator == Operator.OpType.Implicit) {
						parsed_param_count = ParsedParameters.Count - 1;
					} else if (ParsedParameters != null) {
						parsed_param_count = ParsedParameters.Count;
					} else {
						parsed_param_count = 0;
					}

					int parameters_match = -1;
					do {
						var members = MemberCache.FindMembers (type, member_name, true);
						if (members != null) {
							foreach (var m in members) {
								if (ParsedName != null && m.Arity != ParsedName.Arity)
									continue;

								if (ParsedParameters != null) {
									IParametersMember pm = m as IParametersMember;
									if (pm == null)
										continue;

									if (m.Kind == MemberKind.Operator && !ParsedOperator.HasValue)
										continue;

									var pm_params = pm.Parameters;

									int i;
									for (i = 0; i < parsed_param_count; ++i) {
										var pparam = ParsedParameters[i];

										if (i >= pm_params.Count || pparam == null || pparam.TypeSpec == null ||
											!TypeSpecComparer.Override.IsEqual (pparam.TypeSpec, pm_params.Types[i]) ||
											(pparam.Modifier & Parameter.Modifier.RefOutMask) != (pm_params.FixedParameters[i].ModFlags & Parameter.Modifier.RefOutMask)) {

											if (i > parameters_match) {
												parameters_match = i;
											}

											i = -1;
											break;
										}
									}

									if (i < 0)
										continue;

									if (ParsedOperator == Operator.OpType.Explicit || ParsedOperator == Operator.OpType.Implicit) {
										if (pm.MemberType != ParsedParameters[parsed_param_count].TypeSpec) {
											parameters_match = parsed_param_count + 1;
											continue;
										}
									} else {
										if (parsed_param_count != pm_params.Count)
											continue;
									}
								}

								if (member != null) {
									Report.Warning (419, 3, mc.Location,
										"Ambiguous reference in cref attribute `{0}'. Assuming `{1}' but other overloads including `{2}' have also matched",
										cref, member.GetSignatureForError (), m.GetSignatureForError ());

									break;
								}

								member = m;
							}
						}

						// Continue with parent type for nested types
						if (member == null) {
							type = type.DeclaringType;
						} else {
							type = null;
						}
					} while (type != null);

					if (member == null && parameters_match >= 0) {
						for (int i = parameters_match; i < parsed_param_count; ++i) {
							Report.Warning (1580, 1, mc.Location, "Invalid type for parameter `{0}' in XML comment cref attribute `{1}'",
									(i + 1).ToString (), cref);
						}

						if (parameters_match == parsed_param_count + 1) {
							Report.Warning (1581, 1, mc.Location, "Invalid return type in XML comment cref attribute `{0}'", cref);
						}
					}
				}
			}

			if (member == null) {
				Report.Warning (1574, 1, mc.Location, "XML comment on `{0}' has cref attribute `{1}' that could not be resolved",
					mc.GetSignatureForError (), cref);
				cref = "!:" + cref;
			} else if (member == InternalType.Namespace) {
				cref = "N:" + fne.GetSignatureForError ();
			} else {
				prefix = GetMemberDocHead (member);
				cref = prefix + member.GetSignatureForDocumentation ();
			}

			xref.SetAttribute ("cref", cref);
		}

		//
		// Get a prefix from member type for XML documentation (used
		// to formalize cref target name).
		//
		static string GetMemberDocHead (MemberSpec type)
		{
			if (type is FieldSpec)
				return "F:";
			if (type is MethodSpec)
				return "M:";
			if (type is EventSpec)
				return "E:";
			if (type is PropertySpec)
				return "P:";
			if (type is TypeSpec)
				return "T:";

			throw new NotImplementedException (type.GetType ().ToString ());
		}

		//
		// Raised (and passed an XmlElement that contains the comment)
		// when GenerateDocComment is writing documentation expectedly.
		//
		// FIXME: with a few effort, it could be done with XmlReader,
		// that means removal of DOM use.
		//
		void CheckParametersComments (MemberCore member, IParametersMember paramMember, XmlElement el)
		{
			HashSet<string> found_tags = null;
			foreach (XmlElement pelem in el.SelectNodes ("param")) {
				string xname = pelem.GetAttribute ("name");
				if (xname.Length == 0)
					continue; // really? but MS looks doing so

				if (found_tags == null) {
					found_tags = new HashSet<string> ();
				}

				if (xname != "" && paramMember.Parameters.GetParameterIndexByName (xname) < 0) {
					Report.Warning (1572, 2, member.Location,
						"XML comment on `{0}' has a param tag for `{1}', but there is no parameter by that name",
						member.GetSignatureForError (), xname);
					continue;
				}

				if (found_tags.Contains (xname)) {
					Report.Warning (1571, 2, member.Location,
						"XML comment on `{0}' has a duplicate param tag for `{1}'",
						member.GetSignatureForError (), xname);
					continue;
				}

				found_tags.Add (xname);
			}

			if (found_tags != null) {
				foreach (Parameter p in paramMember.Parameters.FixedParameters) {
					if (!found_tags.Contains (p.Name) && !(p is ArglistParameter))
						Report.Warning (1573, 4, member.Location,
							"Parameter `{0}' has no matching param tag in the XML comment for `{1}'",
							p.Name, member.GetSignatureForError ());
				}
			}
		}

		//
		// Outputs XML documentation comment from tokenized comments.
		//
		public bool OutputDocComment (string asmfilename, string xmlFileName)
		{
			XmlTextWriter w = null;
			try {
				w = new XmlTextWriter (xmlFileName, null);
				w.Indentation = 4;
				w.Formatting = Formatting.Indented;
				w.WriteStartDocument ();
				w.WriteStartElement ("doc");
				w.WriteStartElement ("assembly");
				w.WriteStartElement ("name");
				w.WriteString (Path.GetFileNameWithoutExtension (asmfilename));
				w.WriteEndElement (); // name
				w.WriteEndElement (); // assembly
				w.WriteStartElement ("members");
				XmlCommentOutput = w;
				module.GenerateDocComment (this);
				w.WriteFullEndElement (); // members
				w.WriteEndElement ();
				w.WriteWhitespace (Environment.NewLine);
				w.WriteEndDocument ();
				return true;
			} catch (Exception ex) {
				Report.Error (1569, "Error generating XML documentation file `{0}' (`{1}')", xmlFileName, ex.Message);
				return false;
			} finally {
				if (w != null)
					w.Close ();
			}
		}
	}

	//
	// Type lookup of documentation references uses context of type where
	// the reference is used but type parameters from cref value
	//
	sealed class DocumentationMemberContext : IMemberContext
	{
		readonly MemberCore host;
		MemberName contextName;

		public DocumentationMemberContext (MemberCore host, MemberName contextName)
		{
			this.host = host;
			this.contextName = contextName;
		}

		public TypeSpec CurrentType {
			get {
				return host.CurrentType;
			}
		}

		public TypeParameters CurrentTypeParameters {
			get {
				return contextName.TypeParameters;
			}
		}

		public MemberCore CurrentMemberDefinition {
			get {
				return host.CurrentMemberDefinition;
			}
		}

		public bool IsObsolete {
			get {
				return false;
			}
		}

		public bool IsUnsafe {
			get {
				return host.IsStatic;
			}
		}

		public bool IsStatic {
			get {
				return host.IsStatic;
			}
		}

		public ModuleContainer Module {
			get {
				return host.Module;
			}
		}

		public string GetSignatureForError ()
		{
			return host.GetSignatureForError ();
		}

		public ExtensionMethodCandidates LookupExtensionMethod (string name, int arity)
		{
			return null;
		}

		public FullNamedExpression LookupNamespaceOrType (string name, int arity, LookupMode mode, Location loc)
		{
			if (arity == 0) {
				var tp = CurrentTypeParameters;
				if (tp != null) {
					for (int i = 0; i < tp.Count; ++i) {
						var t = tp[i];
						if (t.Name == name) {
							t.Type.DeclaredPosition = i;
							return new TypeParameterExpr (t, loc);
						}
					}
				}
			}

			return host.Parent.LookupNamespaceOrType (name, arity, mode, loc);
		}

		public FullNamedExpression LookupNamespaceAlias (string name)
		{
			throw new NotImplementedException ();
		}
	}

	class DocumentationParameter
	{
		public readonly Parameter.Modifier Modifier;
		public FullNamedExpression Type;
		TypeSpec type;

		public DocumentationParameter (Parameter.Modifier modifier, FullNamedExpression type)
			: this (type)
		{
			this.Modifier = modifier;
		}

		public DocumentationParameter (FullNamedExpression type)
		{
			this.Type = type;
		}

		public TypeSpec TypeSpec {
			get {
				return type;
			}
		}

		public void Resolve (IMemberContext context)
		{
			type = Type.ResolveAsType (context);
		}
	}
}
