//
// System.Web.UI.ControlBuilder.cs
//
// Authors:
// 	Duncan Mak  (duncan@ximian.com)
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002, 2003 Ximian, Inc. (http://www.ximian.com)
//

using System;
using System.Collections;
using System.Reflection;

namespace System.Web.UI {

	public class ControlBuilder
	{
		internal static BindingFlags flagsNoCase = BindingFlags.Public |
							   BindingFlags.Instance |
							   BindingFlags.Static |
							   BindingFlags.IgnoreCase;

		TemplateParser parser;
		ControlBuilder parentBuilder;
		Type type;	       
		string tagName;
		string id;
		IDictionary attribs;
		protected int line;
		protected string fileName;
		bool childrenAsProperties;
		bool isIParserAccessor;
		bool hasAspCode;
		ControlBuilder defaultPropertyBuilder;
		ArrayList children;

		public ControlBuilder ()
		{
		}

		internal ControlBuilder (TemplateParser parser,
					 ControlBuilder parentBuilder,
					 Type type,
					 string tagName,
					 string id,
					 IDictionary attribs,
					 int line,
					 string sourceFileName)

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

		public bool FChildrenAsProperties {
			get { return childrenAsProperties; }
		}

		public bool FIsNonParserAccessor {
			get { return isIParserAccessor; }
		}

		public bool HasAspCode {
			get { return hasAspCode; }
		}

		public string ID {
			get { return id; }
			set { id = value; }
		}

		protected void SetControlType (Type t)
		{
			type = t;
		}
		
		[MonoTODO]
		public bool InDesigner {
			get { return false; }
		}

		public Type NamingContainerType {
			get {
				if (parentBuilder == null)
					return typeof (Control);

				Type ptype = parentBuilder.ControlType;
				if (ptype == null)
					return typeof (Control);

				if (!typeof (INamingContainer).IsAssignableFrom (type))
					return parentBuilder.NamingContainerType;

				return type;
			}
		}

		protected TemplateParser Parser {
			get { return parser; }
		}

		public string TagName {
			get { return tagName; }
		}

		public virtual bool AllowWhitespaceLiterals ()
		{
			return true;
		}

		[MonoTODO]
		public virtual void AppendLiteralString (string s)
		{
			throw new NotImplementedException ();
		}

		public virtual void AppendSubBuilder (ControlBuilder subBuilder)
		{
			if (children == null)
				children = new ArrayList ();

			subBuilder.OnAppendToParentBuilder (this);
			children.Add (subBuilder);
		}

		public virtual void CloseControl ()
		{
		}

		public static ControlBuilder CreateBuilderFromType (TemplateParser parser,
								    ControlBuilder parentBuilder,
								    Type type,
								    string tagName,
								    string id,
								    IDictionary attribs,
								    int line,
								    string sourceFileName)
		{
			ControlBuilder  builder;
			object [] atts = type.GetCustomAttributes (typeof (ControlBuilderAttribute), true);
			if (atts != null && atts.Length > 0) {
				ControlBuilderAttribute att = (ControlBuilderAttribute) atts [0];
				builder = (ControlBuilder) Activator.CreateInstance (att.BuilderType);
			} else {
				builder = new ControlBuilder ();
			}

			builder.Init (parser, parentBuilder, type, tagName, id, attribs);
			builder.line = line;
			builder.fileName = sourceFileName;
			return builder;
		}

		public virtual Type GetChildControlType (string tagName, IDictionary attribs)
		{
			return null;
		}

		public virtual bool HasBody ()
		{
			return true;
		}

		public virtual bool HtmlDecodeLiterals ()
		{
			return false;
		}

		ControlBuilder CreatePropertyBuilder (string propName, TemplateParser parser)
		{
			PropertyInfo prop = type.GetProperty (propName, flagsNoCase);
			if (prop == null) {
				string msg = String.Format ("Property {0} not found in type {1}", propName, type);
				throw new HttpException (msg);
			}

			Type propType = prop.PropertyType;
			ControlBuilder builder = null;
			if (typeof (ICollection).IsAssignableFrom (propType))
				builder = new CollectionBuilder ();
			else if (typeof (ITemplate).IsAssignableFrom (propType))
				builder = new TemplateBuilder ();
			else
				return CreateBuilderFromType (parser,
							      parentBuilder,
							      propType,
							      propName,
							      null,
							      null,
							      line,
							      fileName);

			builder.Init (parser, this, null, tagName, null, null);
			builder.fileName = fileName;
			builder.line = line;
			return builder;
		}
		
		public virtual void Init (TemplateParser parser,
					  ControlBuilder parentBuilder,
					  Type type,
					  string tagName,
					  string id,
					  IDictionary attribs)
		{
			this.parser = parser;
			this.parentBuilder = parentBuilder;
			this.type = type;
			this.tagName = tagName;
			this.id = id;
			this.attribs = attribs;
			if (type == null)
				return;

			if (!typeof (IParserAccessor).IsAssignableFrom (type)) {
				isIParserAccessor = false;
				childrenAsProperties = true;
			} else {
				object [] atts = type.GetCustomAttributes (typeof (ParseChildrenAttribute), true);
				if (atts != null && atts.Length > 0) {
					ParseChildrenAttribute att = (ParseChildrenAttribute) atts [0];
					childrenAsProperties = att.ChildrenAsProperties;
					if (childrenAsProperties && att.DefaultProperty != "")
						defaultPropertyBuilder = CreatePropertyBuilder (att.DefaultProperty,
												parser);
				}
			}
		}

		public virtual bool NeedsTagInnerText ()
		{
			return false;
		}

		public virtual void OnAppendToParentBuilder (ControlBuilder parentBuilder)
		{
			if (parentBuilder.defaultPropertyBuilder == null)
				return;

			ControlBuilder old = parentBuilder.defaultPropertyBuilder;
			defaultPropertyBuilder = null;
			AppendSubBuilder (old);
		}

		public virtual void SetTagInnerText (string text)
		{
		}

		internal virtual object CreateInstance ()
		{
			// HtmlGenericControl, HtmlTableCell...
			object [] atts = type.GetCustomAttributes (typeof (ConstructorNeedsTagAttribute), true);
			object [] args = null;
			if (atts != null && atts.Length > 0) {
				ConstructorNeedsTagAttribute att = (ConstructorNeedsTagAttribute) atts [0];
				if (att.NeedsTag)
					args = new object [] {tagName};
			}

			return Activator.CreateInstance (type, args);
		}

		internal virtual void CreateChildren (object parent) 
		{
			if (children == null || children.Count == 0)
				return;

			IParserAccessor parser = parent as IParserAccessor;
			if (parser == null)
				return;

			foreach (object o in children) {
				if (o is string) {
					parser.AddParsedSubObject (new LiteralControl ((string) o));
				} else {
					parser.AddParsedSubObject (((ControlBuilder) o).CreateInstance ());
				}
			}
		}
	}
}

