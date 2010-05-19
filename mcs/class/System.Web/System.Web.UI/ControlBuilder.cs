//
// System.Web.UI.ControlBuilder.cs
//
// Authors:
// 	Duncan Mak  (duncan@ximian.com)
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//      Marek Habersack <mhabersack@novell.com>
//
// (C) 2002, 2003 Ximian, Inc. (http://www.ximian.com)
// Copyright (C) 2005-2010 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Collections;
using System.Configuration;
using System.CodeDom;
using System.Globalization;
using System.Reflection;
using System.Security.Permissions;
using System.Web.Compilation;
using System.Web.Configuration;
using System.IO;
using System.Web.UI.WebControls;
using System.Web.Util;

using _Location = System.Web.Compilation.Location;

namespace System.Web.UI {

	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class ControlBuilder
	{
		internal static readonly BindingFlags FlagsNoCase = BindingFlags.Public |
			BindingFlags.Instance |
			BindingFlags.Static |
			BindingFlags.IgnoreCase;

		ControlBuilder myNamingContainer;
		TemplateParser parser;
		Type parserType;
		ControlBuilder parentBuilder;
		Type type;	       
		string tagName;
		string originalTagName;
		string id;
		IDictionary attribs;
		int line;
		string fileName;
		bool childrenAsProperties;
		bool isIParserAccessor = true;
		bool hasAspCode;
		ControlBuilder defaultPropertyBuilder;
		ArrayList children;
		ArrayList templateChildren;
		static int nextID;

		bool haveParserVariable;
		CodeMemberMethod method;
		CodeStatementCollection methodStatements;
		CodeMemberMethod renderMethod;
		int renderIndex;
		bool isProperty;
		ILocation location;
		ArrayList otherTags;
		
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
			this.parserType = parser != null ? parser.GetType () : null;
			this.parentBuilder = parentBuilder;
			this.type = type;
			this.tagName = tagName;
			this.id = id;
			this.attribs = attribs;
			this.line = line;
			this.fileName = sourceFileName;
		}

		internal void EnsureOtherTags ()
		{
			if (otherTags == null)
				otherTags = new ArrayList ();
		}

		internal ControlBuilder ParentBuilder {
			get { return parentBuilder; }
		}

		internal IDictionary Attributes {
			get { return attribs; }
		}

		internal int Line {
			get { return line; }
			set { line = value; }
		}

		internal string FileName {
			get { return fileName; }
			set { fileName = value; }
		}

		internal ControlBuilder DefaultPropertyBuilder {
			get { return defaultPropertyBuilder; }
		}

		internal bool HaveParserVariable {
			get { return haveParserVariable; }
			set { haveParserVariable = value; }
		}

		internal CodeMemberMethod Method {
			get { return method; }
			set { method = value; }
		}

		internal CodeMemberMethod DataBindingMethod {
			get;
			set;
		}
			
		internal CodeStatementCollection MethodStatements {
			get { return methodStatements; }
			set { methodStatements = value; }
		}

		internal CodeMemberMethod RenderMethod {
			get { return renderMethod; }
			set { renderMethod = value; }
		}

		internal int RenderIndex {
			get { return renderIndex; }
		}

		internal bool IsProperty {
			get { return isProperty; }
		}

		internal ILocation Location {
			get { return location; }
			set { location = new _Location (value); }
		}
	
		internal ArrayList OtherTags {
			get { return otherTags; }
		}

		public Type ControlType {
			get { return type; }
		}

		protected bool FChildrenAsProperties {
			get { return childrenAsProperties; }
		}

		protected bool FIsNonParserAccessor {
			get { return !isIParserAccessor; }
		}

		public bool HasAspCode {
			get { return hasAspCode; }
		}

		public string ID {
			get { return id; }
			set { id = value; }
		}

		internal ArrayList Children {
			get { return children; }
		}

		internal ArrayList TemplateChildren {
			get { return templateChildren; }
		}
		
		internal void SetControlType (Type t)
		{
			type = t;
		}
		
		protected bool InDesigner {
			get { return false; }
		}

		public Type NamingContainerType {
			get {
				ControlBuilder cb = myNamingContainer;
				
				if (cb == null)
					return typeof (Control);

				return cb.ControlType;
			}
		}

		internal bool IsNamingContainer {
			get {
				if (type == null)
					return false;

				return typeof (INamingContainer).IsAssignableFrom (type);
			}
		}
		
		ControlBuilder MyNamingContainer {
			get {
				if (myNamingContainer == null) {
					Type controlType = parentBuilder != null ? parentBuilder.ControlType : null;
					
					if (parentBuilder == null && controlType == null)
						myNamingContainer = null;
					else if (parentBuilder is TemplateBuilder)
						myNamingContainer = parentBuilder;
					else if (controlType != null && typeof (INamingContainer).IsAssignableFrom (controlType))
						myNamingContainer = parentBuilder;
					else
						myNamingContainer = parentBuilder.MyNamingContainer;
				}

				return myNamingContainer;
			}
		}
			
		public virtual Type BindingContainerType {
			get {
				ControlBuilder cb = (this is TemplateBuilder && !(this is RootBuilder)) ? this : MyNamingContainer;
				
				if (cb == null) {
					if (this is RootBuilder && parserType == typeof (PageParser)) 
						return typeof (Page);
					
					return typeof (Control);
				}

				if (cb != this && cb is ContentBuilderInternal && !typeof (INonBindingContainer).IsAssignableFrom (cb.BindingContainerType))
					return cb.BindingContainerType;

				Type ct;
				if (cb is TemplateBuilder) {
					ct = ((TemplateBuilder) cb).ContainerType;
					if (typeof (INonBindingContainer).IsAssignableFrom (ct))
						return MyNamingContainer.BindingContainerType;
					
					if (ct != null)
						return ct;

					ct = cb.ControlType;
					if (ct == null)
						return typeof (Control);
					
					if (typeof (INonBindingContainer).IsAssignableFrom (ct) || !typeof (INamingContainer).IsAssignableFrom (ct))
						return MyNamingContainer.BindingContainerType;

					return ct;
				}

				ct = cb.ControlType;
				if (ct == null)
					return typeof (Control);
				
				if (typeof (INonBindingContainer).IsAssignableFrom (ct) || !typeof (INamingContainer).IsAssignableFrom (ct))
					return MyNamingContainer.BindingContainerType;
				
				return cb.ControlType;
			}
		}

		internal TemplateBuilder ParentTemplateBuilder {
			get {
				if (parentBuilder == null)
					return null;
				else if (parentBuilder is TemplateBuilder)
					return (TemplateBuilder) parentBuilder;
				else
					return parentBuilder.ParentTemplateBuilder;
			}
		}

		protected TemplateParser Parser {
			get { return parser; }
		}

		public string TagName {
			get { return tagName; }
		}

		internal string OriginalTagName {
			get {
				if (originalTagName == null || originalTagName.Length == 0)
					return TagName;
				return originalTagName;
			}
		}
		
		internal RootBuilder Root {
			get {
				if (typeof (RootBuilder).IsAssignableFrom (GetType ()))
					return (RootBuilder) this;

				return (RootBuilder) parentBuilder.Root;
			}
		}

		internal bool ChildrenAsProperties {
			get { return childrenAsProperties; }
		}

		internal string GetAttribute (string name)
		{
			if (attribs == null)
				return null;

			return attribs [name] as string;
		}

		internal void IncreaseRenderIndex ()
		{
			renderIndex++;
		}
		
		void AddChild (object child)
		{
			if (children == null)
				children = new ArrayList ();

			children.Add (child);
			ControlBuilder cb = child as ControlBuilder;
			if (cb != null && cb is TemplateBuilder) {
				if (templateChildren == null)
					templateChildren = new ArrayList ();
				templateChildren.Add (child);
			}

			if (parser == null)
				return;
			
			string tag = cb != null ? cb.TagName : null;
			if (String.IsNullOrEmpty (tag))
				return;

			RootBuilder rb = Root;
			AspComponentFoundry foundry = rb != null ? rb.Foundry : null;
			if (foundry == null)
				return;
			AspComponent component = foundry.GetComponent (tag);
			if (component == null || !component.FromConfig)
				return;
			
			parser.AddImport (component.Namespace);
			parser.AddDependency (component.Source);
		}
		
		public virtual bool AllowWhitespaceLiterals ()
		{
			return true;
		}

		public virtual void AppendLiteralString (string s)
		{
			if (s == null || s.Length == 0)
				return;

			if (childrenAsProperties || !isIParserAccessor) {
				if (defaultPropertyBuilder != null) {
					defaultPropertyBuilder.AppendLiteralString (s);
				} else if (s.Trim ().Length != 0) {
					throw new HttpException (String.Format ("Literal content not allowed for '{0}' {1} \"{2}\"",
										tagName, GetType (), s));
				}

				return;
			}
			
			if (!AllowWhitespaceLiterals () && s.Trim ().Length == 0)
				return;

			if (HtmlDecodeLiterals ())
				s = HttpUtility.HtmlDecode (s);

			AddChild (s);
		}

		public virtual void AppendSubBuilder (ControlBuilder subBuilder)
		{
			subBuilder.OnAppendToParentBuilder (this);
			
			subBuilder.parentBuilder = this;
			if (childrenAsProperties) {
				AppendToProperty (subBuilder);
				return;
			}

			if (typeof (CodeRenderBuilder).IsAssignableFrom (subBuilder.GetType ())) {
				AppendCode (subBuilder);
				return;
			}

			AddChild (subBuilder);
		}

		void AppendToProperty (ControlBuilder subBuilder)
		{
			if (typeof (CodeRenderBuilder) == subBuilder.GetType ())
				throw new HttpException ("Code render not supported here.");

			if (defaultPropertyBuilder != null) {
				defaultPropertyBuilder.AppendSubBuilder (subBuilder);
				return;
			}

			AddChild (subBuilder);
		}

		void AppendCode (ControlBuilder subBuilder)
		{
			if (type != null && !(typeof (Control).IsAssignableFrom (type)))
				throw new HttpException ("Code render not supported here.");

			if (typeof (CodeRenderBuilder) == subBuilder.GetType ())
				hasAspCode = true;

			AddChild (subBuilder);
		}

		public virtual void CloseControl ()
		{
		}

		static Type MapTagType (Type tagType)
		{
			if (tagType == null)
				return null;
			
			PagesSection ps = WebConfigurationManager.GetSection ("system.web/pages") as PagesSection;
			if (ps == null)
				return tagType;

			TagMapCollection tags = ps.TagMapping;
			if (tags == null || tags.Count == 0)
				return tagType;
			
			string tagTypeName = tagType.ToString ();
			Type mappedType, originalType;
			string originalTypeName = String.Empty, mappedTypeName = String.Empty;
			bool missingType;
			Exception error;
			
			foreach (TagMapInfo tmi in tags) {
				error = null;
				originalType = null;
				
				try {
					originalTypeName = tmi.TagType;
					originalType = HttpApplication.LoadType (originalTypeName);
					if (originalType == null)
						missingType = true;
					else
						missingType = false;
				} catch (Exception ex) {
					missingType = true;
					error = ex;
				}
				if (missingType)
					throw new HttpException (String.Format ("Could not load type {0}", originalTypeName), error);
				
				if (originalTypeName == tagTypeName) {
					mappedTypeName = tmi.MappedTagType;
					error = null;
					mappedType = null;
					
					try {
						mappedType = HttpApplication.LoadType (mappedTypeName);
						if (mappedType == null)
							missingType = true;
						else
							missingType = false;
					} catch (Exception ex) {
						missingType = true;
						error = ex;
					}

					if (missingType)
						throw new HttpException (String.Format ("Could not load type {0}", mappedTypeName),
									 error);
					
					if (!mappedType.IsSubclassOf (originalType))
						throw new ConfigurationErrorsException (
							String.Format ("The specified type '{0}' used for mapping must inherit from the original type '{1}'.", mappedTypeName, originalTypeName));

					return mappedType;
				}
			}
			
			return tagType;
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

			Type tagType = MapTagType (type);
			ControlBuilder builder;
			object [] atts = tagType.GetCustomAttributes (typeof (ControlBuilderAttribute), true);
			if (atts != null && atts.Length > 0) {
				ControlBuilderAttribute att = (ControlBuilderAttribute) atts [0];
				builder = (ControlBuilder) Activator.CreateInstance (att.BuilderType);
			} else {
				builder = new ControlBuilder ();
			}

			builder.Init (parser, parentBuilder, tagType, tagName, id, attribs);
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
		
		ControlBuilder CreatePropertyBuilder (string propName, TemplateParser parser, IDictionary atts)
		{
			int idx;
			string propertyName;
			
			if ((idx = propName.IndexOf (':')) >= 0)
				propertyName = propName.Substring (idx + 1);
			else
				propertyName = propName;
			
			PropertyInfo prop = type.GetProperty (propertyName, FlagsNoCase);
			if (prop == null) {
				string msg = String.Format ("Property {0} not found in type {1}", propertyName, type);
				throw new HttpException (msg);
			}

			Type propType = prop.PropertyType;
			ControlBuilder builder = null;
			if (typeof (ICollection).IsAssignableFrom (propType)) {
				builder = new CollectionBuilder ();
			} else if (typeof (ITemplate).IsAssignableFrom (propType)) {
				builder = new TemplateBuilder (prop);
			} else if (typeof (string) == propType) {
				builder = new StringPropertyBuilder (prop.Name);
			} else {
				builder = CreateBuilderFromType (parser, parentBuilder, propType, prop.Name,
								 null, atts, line, fileName);
				builder.isProperty = true;
				if (idx >= 0)
					builder.originalTagName = propName;
				return builder;
			}

			builder.Init (parser, this, null, prop.Name, null, atts);
			builder.fileName = fileName;
			builder.line = line;
			builder.isProperty = true;
			if (idx >= 0)
				builder.originalTagName = propName;
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
			if (parser != null)
				this.Location = parser.Location;

			this.parentBuilder = parentBuilder;
			this.type = type;
			this.tagName = tagName;
			this.id = id;
			this.attribs = attribs;
			if (type == null)
				return;

			if (this is TemplateBuilder)
				return;

			object [] atts = type.GetCustomAttributes (typeof (ParseChildrenAttribute), true);
			
			if (!typeof (IParserAccessor).IsAssignableFrom (type) && atts.Length == 0) {
				isIParserAccessor = false;
				childrenAsProperties = true;
			} else if (atts.Length > 0) {
				ParseChildrenAttribute att = (ParseChildrenAttribute) atts [0];
				childrenAsProperties = att.ChildrenAsProperties;
				if (childrenAsProperties && att.DefaultProperty.Length != 0)
					defaultPropertyBuilder = CreatePropertyBuilder (att.DefaultProperty,
											parser, null);
			}
		}

		public virtual bool NeedsTagInnerText ()
		{
			return false;
		}

		public virtual void OnAppendToParentBuilder (ControlBuilder parentBuilder)
		{
			if (defaultPropertyBuilder == null)
				return;

			ControlBuilder old = defaultPropertyBuilder;
			defaultPropertyBuilder = null;
			AppendSubBuilder (old);
		}

		internal void SetTagName (string name)
		{
			tagName = name;
		}
		
		public virtual void SetTagInnerText (string text)
		{
		}

		internal string GetNextID (string proposedID)
		{
			if (proposedID != null && proposedID.Trim ().Length != 0)
				return proposedID;

			return "_bctrl_" + nextID++;
		}

		internal virtual ControlBuilder CreateSubBuilder (string tagid,
								  IDictionary atts,
								  Type childType,
								  TemplateParser parser,
								  ILocation location)
		{
			ControlBuilder childBuilder = null;
			if (childrenAsProperties) {
				if (defaultPropertyBuilder == null)
					childBuilder = CreatePropertyBuilder (tagid, parser, atts);
				else {
					if (String.Compare (defaultPropertyBuilder.TagName, tagid, true, Helpers.InvariantCulture) == 0) {
						// The child tag is the same what our default property name. Act as if there was
						// no default property builder, or otherwise we'll end up with invalid nested
						// builder call.
						defaultPropertyBuilder = null;
						childBuilder = CreatePropertyBuilder (tagid, parser, atts);
					} else {
						Type ct = ControlType;
						MemberInfo[] mems = ct != null ? ct.GetMember (tagid, MemberTypes.Property, FlagsNoCase) : null;
						PropertyInfo prop = mems != null && mems.Length > 0 ? mems [0] as PropertyInfo : null;

						if (prop != null && typeof (ITemplate).IsAssignableFrom (prop.PropertyType)) {
							childBuilder = CreatePropertyBuilder (tagid, parser, atts);
							defaultPropertyBuilder = null;
						} else
							childBuilder = defaultPropertyBuilder.CreateSubBuilder (tagid, atts, null, parser, location);
					}
				}

				return childBuilder;
			}

			if (String.Compare (tagName, tagid, true, Helpers.InvariantCulture) == 0)
				return null;
			
			childType = GetChildControlType (tagid, atts);
			if (childType == null)
				return null;

			childBuilder = CreateBuilderFromType (parser, this, childType, tagid, id, atts,
							      location.BeginLine, location.Filename);

			return childBuilder;
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

		[MonoTODO ("unsure, lack documentation")]
		public virtual object BuildObject ()
		{
			return CreateInstance ();
		}
		
		public virtual void ProcessGeneratedCode(CodeCompileUnit codeCompileUnit,
							 CodeTypeDeclaration baseType,
							 CodeTypeDeclaration derivedType,
							 CodeMemberMethod buildMethod,
							 CodeMemberMethod dataBindingMethod)
		{
			// nothing to do
		}

		internal void ResetState()
		{
			renderIndex = 0;
			haveParserVariable = false;

			if (Children != null) {
				foreach (object child in Children) {
					ControlBuilder cb = child as ControlBuilder;
					if (cb != null)
						cb.ResetState ();
				}
			}
		}
	}
}
