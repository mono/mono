//
// System.Web.UI.ControlBuilder.cs
//
// Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

using System;
using System.Collections;

namespace System.Web.UI {

	public class ControlBuilder
	{
		TemplateParser parser;
		ControlBuilder parentBuilder;
		Type type;	       
		string tagName;
		string id;
		IDictionary attribs;
		int line;
		string fileName;

		public ControlBuilder ()
		{
		}


		internal ControlBuilder (
			TemplateParser parser, ControlBuilder parentBuilder,
			Type type, string tagName, string id,
			IDictionary attribs, int line, string sourceFileName)

		{
			this.parser = parser;
			this.parentBuilder = parentBuilder;
			this.type = type;
			this.tagName = tagName;
			this.id = id;
			this.attribs = attribs;
			this.line = line;
			this.fileName = sourceFileName;
		}

		public Type ControlType {
			get { return type; }
		}

		[MonoTODO]
		public bool FChildrenAsProperties {
			get { return false; }
		}

		[MonoTODO]
		public bool FIsNonParserAccessor {
			get { return false; }
		}

		[MonoTODO]
		public bool HasAspCode {
			get { return false; }
		}

		public string ID {
			get { return id; }

			set { id = value; }
		}

		[MonoTODO]
		public bool InDesigner {
			get { return false; }
		}

		[MonoTODO]
		public Type NamingContainerType {
			get { return null; }
		}

		protected TemplateParser Parser {
			get { return parser; }
		}

		public string TagName {
			get { return tagName; }
		}

		[MonoTODO]
		public virtual bool AllowWhitespaceLiterals ()
		{
			return false;
		}

		[MonoTODO]
		public virtual void AppendLiteralString (string s)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void AppendSubBuilder (ControlBuilder subBuilder)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void CloseControl ()
		{
		}

		[MonoTODO]
		public static ControlBuilder CreateBuilderFromType (
			TemplateParser parser, ControlBuilder parentBuilder,
			Type type, string tagName, string id,
			IDictionary attribs, int line, string sourceFileName)
		{
			return new ControlBuilder (parser, parentBuilder, type,
						   tagName, id, attribs, line, sourceFileName);
		}

		[MonoTODO]
		public virtual Type GetChildControlType (string tagName, IDictionary attribs)
		{
			return attribs [tagName] as Type;
		}

		[MonoTODO]
		public virtual bool HasBody ()
		{
			return false;
		}

		[MonoTODO]
		public virtual bool HtmlDecodeLiterals ()
		{
			return false;
		}

		[MonoTODO]
		public virtual void Init (
			TemplateParser parser, ControlBuilder parentBuilder,
			Type type, string tagName, string id, IDictionary attribs)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool NeedsTagInnerText ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void OnAppendToParentBuilder ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void SetTagInnerText (string text)
		{
			throw new NotImplementedException ();
		}
	}
}

