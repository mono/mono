//
// System.Web.Compilation.TemplateControlCompiler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//
using System;
using System.CodeDom;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Web.UI;

namespace System.Web.Compilation
{
	class TemplateControlCompiler : BaseCompiler
	{
		static BindingFlags noCaseFlags = BindingFlags.Public | BindingFlags.NonPublic |
						  BindingFlags.Instance | BindingFlags.IgnoreCase;

		static Type styleType = typeof (System.Web.UI.WebControls.Style);
		static Type fontinfoType = typeof (System.Web.UI.WebControls.FontInfo);

		TemplateControlParser parser;
		int dataBoundAtts;
		static TypeConverter colorConverter;

		static CodeVariableReferenceExpression ctrlVar = new CodeVariableReferenceExpression ("__ctrl");
		static Type [] arrayString = new Type [] {typeof (string)};
		static Type [] arrayStringCultureInfo = new Type [] {typeof (string), typeof (CultureInfo)};

		public TemplateControlCompiler (TemplateControlParser parser)
			: base (parser)
		{
			this.parser = parser;
		}

		void EnsureID (ControlBuilder builder)
		{
			if (builder.ID == null || builder.ID.Trim () == "")
				builder.ID = builder.GetNextID (null);
		}

		void CreateField (ControlBuilder builder, bool check)
		{
			if (check && CheckBaseFieldOrProperty (builder.ID, builder.ControlType))
				return; // The field or property already exists in a base class and is accesible.

			CodeMemberField field;
			field = new CodeMemberField (builder.ControlType.FullName, builder.ID);
			field.Attributes = MemberAttributes.Family;
			mainClass.Members.Add (field);
		}

		bool CheckBaseFieldOrProperty (string id, Type type)
		{
			FieldInfo fld = parser.BaseType.GetField (id, noCaseFlags);

			Type other = null;
			if (fld == null || fld.IsPrivate) {
				PropertyInfo prop = parser.BaseType.GetProperty (id, noCaseFlags);
				if (prop != null) {
					MethodInfo setm = prop.GetSetMethod (true);
					if (setm != null)
						other = prop.PropertyType;
				}
			} else {
				other = fld.FieldType;
			}
			
			if (other == null)
				return false;

			if (!other.IsAssignableFrom (type)) {
				string msg = String.Format ("The base class includes the field '{0}', but its " +
							    "type '{1}' is not compatible with {2}",
							    id, other, type);
				throw new HttpException (msg);
			}

			return true;
		}

		void AddParsedSubObjectStmt (ControlBuilder builder, CodeExpression expr) 
		{
			if (!builder.haveParserVariable) {
				CodeVariableDeclarationStatement p = new CodeVariableDeclarationStatement();
				p.Name = "__parser";
				p.Type = new CodeTypeReference (typeof (IParserAccessor));
				p.InitExpression = new CodeCastExpression (typeof (IParserAccessor), ctrlVar);
				builder.method.Statements.Add (p);
				builder.haveParserVariable = true;
			}

			CodeVariableReferenceExpression var = new CodeVariableReferenceExpression ("__parser");
			CodeMethodInvokeExpression invoke = new CodeMethodInvokeExpression (var, "AddParsedSubObject");
			invoke.Parameters.Add (expr);
			builder.method.Statements.Add (invoke);
		}

		void InitMethod (ControlBuilder builder, bool isTemplate, bool childrenAsProperties)
		{
			string tailname = ((builder is RootBuilder) ? "Tree" : ("_" + builder.ID));
			CodeMemberMethod method = new CodeMemberMethod ();
			builder.method = method;
			method.Name = "__BuildControl" + tailname;
			method.Attributes = MemberAttributes.Private | MemberAttributes.Final;
			Type type = builder.ControlType;

			if (builder.HasAspCode) {
				CodeMemberMethod renderMethod = new CodeMemberMethod ();
				builder.renderMethod = renderMethod;
				renderMethod.Name = "__Render" + tailname;
				renderMethod.Attributes = MemberAttributes.Private | MemberAttributes.Final;
				CodeParameterDeclarationExpression arg1 = new CodeParameterDeclarationExpression ();
				arg1.Type = new CodeTypeReference (typeof (HtmlTextWriter));
				arg1.Name = "__output";
				CodeParameterDeclarationExpression arg2 = new CodeParameterDeclarationExpression ();
				arg2.Type = new CodeTypeReference (typeof (Control));
				arg2.Name = "parameterContainer";
				renderMethod.Parameters.Add (arg1);
				renderMethod.Parameters.Add (arg2);
				mainClass.Members.Add (renderMethod);
			}
			
			if (childrenAsProperties || isTemplate) {
				string typeString;
				if (builder.isProperty && !isTemplate)
					typeString = builder.ControlType.FullName;
				else 
					typeString = "System.Web.UI.Control";

				method.Parameters.Add (new CodeParameterDeclarationExpression (typeString, "__ctrl"));
			} else {
				
				if (typeof (Control).IsAssignableFrom (type))
					method.ReturnType = new CodeTypeReference (typeof (Control));

				CodeObjectCreateExpression newExpr = new CodeObjectCreateExpression (type);

				object [] atts = type.GetCustomAttributes (typeof (ConstructorNeedsTagAttribute), true);
				if (atts != null && atts.Length > 0) {
					ConstructorNeedsTagAttribute att = (ConstructorNeedsTagAttribute) atts [0];
					if (att.NeedsTag)
						newExpr.Parameters.Add (new CodePrimitiveExpression (builder.TagName));
				} else if (builder is DataBindingBuilder) {
					newExpr.Parameters.Add (new CodePrimitiveExpression (0));
					newExpr.Parameters.Add (new CodePrimitiveExpression (1));
				}

				method.Statements.Add (new CodeVariableDeclarationStatement (builder.ControlType, "__ctrl"));
				CodeAssignStatement assign = new CodeAssignStatement ();
				assign.Left = ctrlVar;
				assign.Right = newExpr;
				method.Statements.Add (assign);
				
				CodeFieldReferenceExpression builderID = new CodeFieldReferenceExpression ();
				builderID.TargetObject = thisRef;
				builderID.FieldName = builder.ID;
				assign = new CodeAssignStatement ();
				assign.Left = builderID;
				assign.Right = ctrlVar;
				method.Statements.Add (assign);
				if (typeof (UserControl).IsAssignableFrom (type)) {
					CodeMethodReferenceExpression mref = new CodeMethodReferenceExpression ();
					mref.TargetObject = builderID;
					mref.MethodName = "InitializeAsUserControl";
					CodeMethodInvokeExpression initAsControl = new CodeMethodInvokeExpression (mref);
					initAsControl.Parameters.Add (new CodePropertyReferenceExpression (thisRef, "Page"));
					method.Statements.Add (initAsControl);
				}
			}

			mainClass.Members.Add (method);
		}

		void AddLiteralSubObject (ControlBuilder builder, string str)
		{
			if (!builder.HasAspCode) {
				CodeObjectCreateExpression expr;
				expr = new CodeObjectCreateExpression (typeof (LiteralControl), new CodePrimitiveExpression (str));
				AddParsedSubObjectStmt (builder, expr);
			} else {
				CodeMethodReferenceExpression methodRef = new CodeMethodReferenceExpression ();
				methodRef.TargetObject = new CodeArgumentReferenceExpression ("__output");
				methodRef.MethodName = "Write";

				CodeMethodInvokeExpression expr;
				expr = new CodeMethodInvokeExpression (methodRef, new CodePrimitiveExpression (str));
				builder.renderMethod.Statements.Add (expr);
			}
		}

		string TrimDB (string value)
		{
			string str = value.Trim ();
			str = str.Substring (3);
			return str.Substring (0, str.Length - 2);
		}

		string DataBoundProperty (ControlBuilder builder, Type type, string varName, string value)
		{
			value = TrimDB (value);
			CodeMemberMethod method;
			string dbMethodName = builder.method.Name + "_DB_" + dataBoundAtts++;

			method = CreateDBMethod (dbMethodName, GetContainerType (builder), builder.ControlType);

			CodeVariableReferenceExpression targetExpr = new CodeVariableReferenceExpression ("target");

			// This should be a CodePropertyReferenceExpression for properties... but it works anyway
			CodeFieldReferenceExpression field = new CodeFieldReferenceExpression (targetExpr, varName);

			CodeExpression expr;
			if (type == typeof (string)) {
				CodeMethodInvokeExpression tostring = new CodeMethodInvokeExpression ();
				CodeTypeReferenceExpression conv = new CodeTypeReferenceExpression (typeof (Convert));
				tostring.Method = new CodeMethodReferenceExpression (conv, "ToString");
				tostring.Parameters.Add (new CodeSnippetExpression (value));
				expr = tostring;
			} else {
				CodeSnippetExpression snippet = new CodeSnippetExpression (value);
				expr = new CodeCastExpression (type, snippet);
			}
			
			method.Statements.Add (new CodeAssignStatement (field, expr));
			mainClass.Members.Add (method);
			return method.Name;
		}

		void AddCodeForPropertyOrField (ControlBuilder builder, Type type, string var_name, string att, bool isDataBound)
		{
			CodeMemberMethod method = builder.method;
			if (isDataBound) {
				string dbMethodName = DataBoundProperty (builder, type, var_name, att);
				AddEventAssign (method, "DataBinding", typeof (EventHandler), dbMethodName);
				return;
			}

			CodeAssignStatement assign = new CodeAssignStatement ();
			assign.Left = new CodePropertyReferenceExpression (ctrlVar, var_name);
			assign.Right = GetExpressionFromString (type, att);

			method.Statements.Add (assign);
		}

		bool IsDataBound (string value)
		{
			if (value == null || value == "")
				return false;

			string str = value.Trim ();
			return (str.StartsWith ("<%#") && str.EndsWith ("%>"));
		}
		
		bool ProcessPropertiesAndFields (ControlBuilder builder, MemberInfo member, string id, string attValue)
		{
			CodeMemberMethod method = builder.method;
			this.dataBoundAtts = 0;
			int hyphen = id.IndexOf ('-');

			bool isPropertyInfo = (member is PropertyInfo);
			bool is_processed = false;
			bool isDataBound = IsDataBound (attValue);

			Type type;
			if (isPropertyInfo) {
				type = ((PropertyInfo) member).PropertyType;
				if (hyphen == -1 && ((PropertyInfo) member).CanWrite == false)
					return false;
			} else {
				type = ((FieldInfo) member).FieldType;
			}

			if (0 == String.Compare (member.Name, id, true)){
				AddCodeForPropertyOrField (builder, type, member.Name, attValue, isDataBound);
				is_processed = true;
			} else if (hyphen != -1 && (type == fontinfoType || type == styleType || type.IsSubclassOf (styleType))){
				//FIXME: x-y should not be limited to style and font
				string prop_field = id.Replace ("-", ".");
				string [] parts = prop_field.Split (new char [] {'.'});
				if (parts.Length != 2 || 0 != String.Compare (member.Name, parts [0], true))
					return false;

				PropertyInfo [] subprops = type.GetProperties ();
				foreach (PropertyInfo subprop in subprops) {
					if (0 != String.Compare (subprop.Name, parts [1], true))
						continue;

					if (subprop.CanWrite == false)
						return false;

					bool is_bool = subprop.PropertyType == typeof (bool);
					if (!is_bool && attValue == null)
						return false; // Font-Size -> Font-Size="" as html

					string value;
					if (attValue == null && is_bool)
						value = "true"; // Font-Bold <=> Font-Bold="true"
					else
						value = attValue;

					AddCodeForPropertyOrField (builder, subprop.PropertyType,
							 member.Name + "." + subprop.Name,
							 value, isDataBound);
					is_processed = true;
				}
			}

			return is_processed;
		}

		void AddEventAssign (CodeMemberMethod method, string name, Type type, string value)
		{
			//"__ctrl.{0} += new {1} (this.{2});"
			CodeEventReferenceExpression evtID = new CodeEventReferenceExpression (ctrlVar, name);

			CodeDelegateCreateExpression create;
			create = new CodeDelegateCreateExpression (new CodeTypeReference (type), thisRef, value);

			CodeAttachEventStatement attach = new CodeAttachEventStatement (evtID, create);
			method.Statements.Add (attach);
		}
		
		void CreateAssignStatementsFromAttributes (ControlBuilder builder)
		{
			IDictionary atts = builder.attribs;
			if (atts == null || atts.Count == 0)
				return;

			EventInfo [] ev_info = null;
			PropertyInfo [] prop_info = null;
			FieldInfo [] field_info = null;
			bool is_processed = false;
			Type type = builder.ControlType;

			foreach (string id in atts.Keys){
				if (0 == String.Compare (id, "runat", true))
					continue;

				is_processed = false;
				string attvalue = atts [id] as string;
				if (id.Length > 2 && id.Substring (0, 2).ToUpper () == "ON"){
					if (ev_info == null)
						ev_info = type.GetEvents ();

					string id_as_event = id.Substring (2);
					foreach (EventInfo ev in ev_info){
						if (0 == String.Compare (ev.Name, id_as_event, true)){
							AddEventAssign (builder.method,
									ev.Name,
									ev.EventHandlerType,
									attvalue);

							is_processed = true;
							break;
						}
					}

					if (is_processed)
						continue;
				} 

				if (prop_info == null)
					prop_info = type.GetProperties ();

				foreach (PropertyInfo prop in prop_info) {
					is_processed = ProcessPropertiesAndFields (builder, prop, id, attvalue);
					if (is_processed)
						break;
				}

				if (is_processed)
					continue;

				if (field_info == null)
					field_info = type.GetFields ();

				foreach (FieldInfo field in field_info){
					is_processed = ProcessPropertiesAndFields (builder, field, id, attvalue);
					if (is_processed)
						break;
				}

				if (is_processed)
					continue;

				if (!typeof (IAttributeAccessor).IsAssignableFrom (type))
					throw new ParseException (builder.location, "Unrecognized attribute: " + id);


				CodeCastExpression cast = new CodeCastExpression (typeof (IAttributeAccessor), ctrlVar);
				CodeMethodReferenceExpression methodExpr;
				methodExpr = new CodeMethodReferenceExpression (cast, "SetAttribute");
				CodeMethodInvokeExpression expr = new CodeMethodInvokeExpression (methodExpr);
				expr.Parameters.Add (new CodePrimitiveExpression (id));
				expr.Parameters.Add (new CodePrimitiveExpression ((string) atts [id]));
				builder.method.Statements.Add (expr);
			}
		}

		void AddRenderControl (ControlBuilder builder)
		{
			CodeIndexerExpression indexer = new CodeIndexerExpression ();
			indexer.TargetObject = new CodePropertyReferenceExpression (
							new CodeArgumentReferenceExpression ("parameterContainer"),
							"Controls");
							
			indexer.Indices.Add (new CodePrimitiveExpression (builder.renderIndex));
			
			CodeMethodInvokeExpression invoke = new CodeMethodInvokeExpression (indexer, "RenderControl");
			invoke.Parameters.Add (new CodeArgumentReferenceExpression ("__output"));
			builder.renderMethod.Statements.Add (invoke);
			builder.renderIndex++;
		}

		void AddChildCall (ControlBuilder parent, ControlBuilder child)
		{
			CodeMethodReferenceExpression m = new CodeMethodReferenceExpression (thisRef, child.method.Name);
			CodeMethodInvokeExpression expr = new CodeMethodInvokeExpression (m);
			if (child.isProperty || parent.FChildrenAsProperties) {
				expr.Parameters.Add (new CodeFieldReferenceExpression (ctrlVar, child.TagName));
				parent.method.Statements.Add (expr);
				return;
			}

			parent.method.Statements.Add (expr);
			CodeFieldReferenceExpression field = new CodeFieldReferenceExpression (thisRef, child.ID);
			if (parent.ControlType == null || typeof (IParserAccessor).IsAssignableFrom (parent.ControlType)) {
				AddParsedSubObjectStmt (parent, field);
			} else {
				CodeMethodInvokeExpression invoke = new CodeMethodInvokeExpression (ctrlVar, "Add");
				invoke.Parameters.Add (field);
				parent.method.Statements.Add (invoke);
			}
				
			if (parent.HasAspCode)
				AddRenderControl (parent);
		}

		void AddTemplateInvocation (CodeMemberMethod method, string name, string methodName)
		{
			CodePropertyReferenceExpression prop = new CodePropertyReferenceExpression (ctrlVar, name);

			CodeObjectCreateExpression newBuild = new CodeObjectCreateExpression (typeof (BuildTemplateMethod));
			newBuild.Parameters.Add (new CodeMethodReferenceExpression (thisRef, methodName));

			CodeObjectCreateExpression newCompiled = new CodeObjectCreateExpression (typeof (CompiledTemplateBuilder));
			newCompiled.Parameters.Add (newBuild);

			CodeAssignStatement assign = new CodeAssignStatement (prop, newCompiled);
			method.Statements.Add (assign);
		}

		void AddCodeRender (ControlBuilder parent, CodeRenderBuilder cr)
		{
			if (cr.Code == null || cr.Code.Trim () == "")
				return;

			if (!cr.IsAssign) {
				CodeSnippetStatement code = new CodeSnippetStatement (cr.Code);
				parent.renderMethod.Statements.Add (code);
				return;
			}

			CodeMethodInvokeExpression expr = new CodeMethodInvokeExpression ();
			expr.Method = new CodeMethodReferenceExpression (
							new CodeArgumentReferenceExpression ("__output"),
							"Write");

			expr.Parameters.Add (new CodeSnippetExpression (cr.Code));
			parent.renderMethod.Statements.Add (expr);
		}

		static Type GetContainerType (ControlBuilder builder)
		{
			Type type = builder.NamingContainerType;
			if (builder.ControlType != null && !typeof (ITemplate).IsAssignableFrom (builder.ControlType))
				return type;

			PropertyInfo prop = type.GetProperty ("Items", noCaseFlags);
			if (prop == null)
				return type;

			Type ptype = prop.PropertyType;
			if (!typeof (ICollection).IsAssignableFrom (ptype))
				return type;

			prop = ptype.GetProperty ("Item", noCaseFlags);
			if (prop == null)
				return type;

			return prop.PropertyType;
		}
		
		CodeMemberMethod CreateDBMethod (string name, Type container, Type target)
		{
			CodeMemberMethod method = new CodeMemberMethod ();
			method.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			method.Name = name;
			method.Parameters.Add (new CodeParameterDeclarationExpression (typeof (object), "sender"));
			method.Parameters.Add (new CodeParameterDeclarationExpression (typeof (EventArgs), "e"));

			CodeTypeReference containerRef = new CodeTypeReference (container);
			CodeTypeReference targetRef = new CodeTypeReference (target);

			CodeVariableDeclarationStatement decl = new CodeVariableDeclarationStatement();
			decl.Name = "Container";
			decl.Type = containerRef;
			method.Statements.Add (decl);

			decl = new CodeVariableDeclarationStatement();
			decl.Name = "target";
			decl.Type = targetRef;
			method.Statements.Add (decl);

			CodeVariableReferenceExpression targetExpr = new CodeVariableReferenceExpression ("target");
			CodeAssignStatement assign = new CodeAssignStatement ();
			assign.Left = targetExpr;
			assign.Right = new CodeCastExpression (targetRef, new CodeArgumentReferenceExpression ("sender"));
			method.Statements.Add (assign);

			assign = new CodeAssignStatement ();
			assign.Left = new CodeVariableReferenceExpression ("Container");
			assign.Right = new CodeCastExpression (containerRef,
						new CodePropertyReferenceExpression (targetExpr, "BindingContainer"));
			method.Statements.Add (assign);

			return method;
		}

		void AddDataBindingLiteral (ControlBuilder builder, DataBindingBuilder db)
		{
			if (db.Code == null || db.Code.Trim () == "")
				return;

			EnsureID (db);
			CreateField (db, false);

			string dbMethodName = "__DataBind_" + db.ID;
			// Add the method that builds the DataBoundLiteralControl
			InitMethod (db, false, false);
			CodeMemberMethod method = db.method;
			AddEventAssign (method, "DataBinding", typeof (EventHandler), dbMethodName);
			method.Statements.Add (new CodeMethodReturnStatement (ctrlVar));

			// Add the DataBind handler
			method = CreateDBMethod (dbMethodName, GetContainerType (builder), typeof (DataBoundLiteralControl));

			CodeVariableReferenceExpression targetExpr = new CodeVariableReferenceExpression ("target");
			CodeMethodInvokeExpression invoke = new CodeMethodInvokeExpression ();
			invoke.Method = new CodeMethodReferenceExpression (targetExpr, "SetDataBoundString");
			invoke.Parameters.Add (new CodePrimitiveExpression (0));

			CodeMethodInvokeExpression tostring = new CodeMethodInvokeExpression ();
			tostring.Method = new CodeMethodReferenceExpression (
							new CodeTypeReferenceExpression (typeof (Convert)),
							"ToString");
			tostring.Parameters.Add (new CodeSnippetExpression (db.Code));
			invoke.Parameters.Add (tostring);
			method.Statements.Add (invoke);
			
			mainClass.Members.Add (method);

			AddChildCall (builder, db);
		}

		void FlushText (ControlBuilder builder, StringBuilder sb)
		{
			if (sb.Length > 0) {
				AddLiteralSubObject (builder, sb.ToString ());
				sb.Length = 0;
			}
		}

		void CreateControlTree (ControlBuilder builder, bool inTemplate, bool childrenAsProperties)
		{
			EnsureID (builder);
			bool isTemplate = (typeof (TemplateBuilder).IsAssignableFrom (builder.GetType ()));
			if (!isTemplate)
				CreateField (builder, true);

			InitMethod (builder, isTemplate, childrenAsProperties || isTemplate);
			CreateAssignStatementsFromAttributes (builder);

			if (builder.Children != null && builder.Children.Count > 0) {
				ArrayList templates = null;

				StringBuilder sb = new StringBuilder ();
				foreach (object b in builder.Children) {

					if (b is string) {
						sb.Append ((string) b);
						continue;
					}

					FlushText (builder, sb);
					if (b is ObjectTagBuilder) {
						ProcessObjectTag ((ObjectTagBuilder) b);
						continue;
					}

					if (b is TemplateBuilder) {
						if (templates == null)
							templates = new ArrayList ();

						templates.Add (b);
						continue;
					}

					if (b is CodeRenderBuilder) {
						AddCodeRender (builder, (CodeRenderBuilder) b);
						continue;
					}

					if (b is DataBindingBuilder) {
						AddDataBindingLiteral (builder, (DataBindingBuilder) b);
						continue;
					}

					if (b is ControlBuilder) {
						ControlBuilder child = (ControlBuilder) b;
						CreateControlTree (child, inTemplate, builder.FChildrenAsProperties);
						AddChildCall (builder, child);
						continue;
					}

					throw new Exception ("???");
				}

				FlushText (builder, sb);

				if (templates != null) {
					foreach (ControlBuilder b in templates) {
						CreateControlTree (b, true, false);
						AddTemplateInvocation (builder.method, b.TagName, b.method.Name);
					}
				}

			}

			if (builder.defaultPropertyBuilder != null) {
				ControlBuilder b = builder.defaultPropertyBuilder;
				CreateControlTree (b, false, true);
				Console.WriteLine ("Aqui");
				AddChildCall (builder, b);
			}

			if (builder.HasAspCode) {
				CodeMethodReferenceExpression m = new CodeMethodReferenceExpression ();
				m.TargetObject = thisRef;
				m.MethodName = builder.renderMethod.Name;

				CodeObjectCreateExpression create = new CodeObjectCreateExpression ();
				create.CreateType = new CodeTypeReference (typeof (RenderMethod));
				create.Parameters.Add (m);

				CodeMethodInvokeExpression invoke = new CodeMethodInvokeExpression ();
				invoke.Method = new CodeMethodReferenceExpression (ctrlVar, "SetRenderMethodDelegate");
				invoke.Parameters.Add (create);

				builder.method.Statements.Add (invoke);
			}
			
			if (typeof (Control).IsAssignableFrom (builder.ControlType))
				builder.method.Statements.Add (new CodeMethodReturnStatement (ctrlVar));
		}

		protected override void CreateMethods ()
		{
			base.CreateMethods ();

			CreateProperties ();
			CreateControlTree (parser.RootBuilder, false, false);
			CreateFrameworkInitializeMethod ();
		}
		
		void CreateFrameworkInitializeMethod ()
		{
			CodeMemberMethod method = new CodeMemberMethod ();
			method.Name = "FrameworkInitialize";
			method.Attributes = MemberAttributes.Family | MemberAttributes.Override;
			AddStatementsToFrameworkInitialize (method);
			mainClass.Members.Add (method);
		}

		protected virtual void AddStatementsToFrameworkInitialize (CodeMemberMethod method)
		{
			if (!parser.EnableViewState) {
				CodeAssignStatement stmt = new CodeAssignStatement ();
				stmt.Left = new CodePropertyReferenceExpression (thisRef, "EnableViewState");
				stmt.Right = new CodePrimitiveExpression (false);
				method.Statements.Add (stmt);
			}

			CodeMethodReferenceExpression methodExpr;
			methodExpr = new CodeMethodReferenceExpression (thisRef, "__BuildControlTree");
			CodeMethodInvokeExpression expr = new CodeMethodInvokeExpression (methodExpr, thisRef);
			method.Statements.Add (new CodeExpressionStatement (expr));
		}

		protected override void ProcessObjectTag (ObjectTagBuilder tag)
		{
			string name = tag.ObjectID;
			string className = tag.ClassName;
			Type t = null;
			try {
				t = parser.LoadType (className);
			} catch (Exception e) {
				throw new ParseException (tag.location, "Error loading type " + className);
			}

			string fieldName = "cached" + name;
			CodeMemberField f = new CodeMemberField (t, fieldName);
			f.Attributes = MemberAttributes.Private;
			mainClass.Members.Add (f);

			CodeFieldReferenceExpression field = new CodeFieldReferenceExpression (thisRef, fieldName);

			CodeMemberProperty prop = new CodeMemberProperty ();
			prop.Type = new CodeTypeReference (t);
			prop.Name = name;
			prop.Attributes = MemberAttributes.Private | MemberAttributes.Final;

			CodeConditionStatement stmt = new CodeConditionStatement();
			stmt.Condition = new CodeBinaryOperatorExpression (field,
									   CodeBinaryOperatorType.IdentityEquality,
									   new CodePrimitiveExpression (null));

			CodeObjectCreateExpression create = new CodeObjectCreateExpression (prop.Type);	
			stmt.TrueStatements.Add (new CodeAssignStatement (field, create));
			prop.GetStatements.Add (stmt);
			prop.GetStatements.Add (new CodeMethodReturnStatement (field));
			mainClass.Members.Add (prop);
		}

		void CreateProperties ()
		{
			if (!parser.AutoEventWireup) {
				CreateAutoEventWireup ();
			} else {
				CreateAutoHandlers ();
			}

			CreateApplicationInstance ();
			CreateTemplateSourceDirectory ();
		}

		void CreateTemplateSourceDirectory ()
		{
			CodeMemberProperty prop = new CodeMemberProperty ();
			prop.Type = new CodeTypeReference (typeof (string));
			prop.Name = "TemplateSourceDirectory";
			prop.Attributes = MemberAttributes.Public | MemberAttributes.Override;

			CodePrimitiveExpression expr = new CodePrimitiveExpression (parser.CurrentVirtualPath);
			prop.GetStatements.Add (new CodeMethodReturnStatement (expr));
			mainClass.Members.Add (prop);
		}

		void CreateApplicationInstance ()
		{
			CodeMemberProperty prop = new CodeMemberProperty ();
			Type appType = typeof (HttpApplication);//FIXME: uncomment HttpApplicationFactory.AppType;
			prop.Type = new CodeTypeReference (appType);
			prop.Name = "ApplicationInstance";
			prop.Attributes = MemberAttributes.Family | MemberAttributes.Final;

			CodePropertyReferenceExpression propRef = new CodePropertyReferenceExpression (thisRef, "Context");

			propRef = new CodePropertyReferenceExpression (propRef, "ApplicationInstance");

			CodeCastExpression cast = new CodeCastExpression (appType.FullName, propRef);
			prop.GetStatements.Add (new CodeMethodReturnStatement (cast));
			mainClass.Members.Add (prop);
		}

		void CreateAutoHandlers ()
		{
			// Create AutoHandlers property
			CodeMemberProperty prop = new CodeMemberProperty ();
			prop.Type = new CodeTypeReference (typeof (int));
			prop.Name = "AutoHandlers";
			prop.Attributes = MemberAttributes.Family | MemberAttributes.Override;
			
			CodeMethodReturnStatement ret = new CodeMethodReturnStatement ();
			CodeFieldReferenceExpression fldRef ;
			fldRef = new CodeFieldReferenceExpression (mainClassExpr, "__autoHandlers");
			ret.Expression = fldRef;
			prop.GetStatements.Add (ret);
			
			prop.SetStatements.Add (new CodeAssignStatement (fldRef, new CodePropertySetValueReferenceExpression ()));

			mainClass.Members.Add (prop);

			// Add the __autoHandlers field
			CodeMemberField fld = new CodeMemberField (typeof (int), "__autoHandlers");
			fld.Attributes = MemberAttributes.Private | MemberAttributes.Static;
			mainClass.Members.Add (fld);
		}

		void CreateAutoEventWireup ()
		{
			// The getter returns false
			CodeMemberProperty prop = new CodeMemberProperty ();
			prop.Type = new CodeTypeReference (typeof (int));
			prop.Name = "AutoEventWireup";
			prop.Attributes = MemberAttributes.Family | MemberAttributes.Override;
			prop.GetStatements.Add (new CodeMethodReturnStatement (new CodePrimitiveExpression (false)));
			mainClass.Members.Add (prop);
		}
		
		CodeExpression GetExpressionFromString (Type type, string str)
		{
			if (type == typeof (string))
				return new CodePrimitiveExpression (str);

			if (type == typeof (bool)) {
				if (str == null || str == "" || 0 == String.Compare (str, "true", true))
					return new CodePrimitiveExpression (true);
				else if (0 == String.Compare (str, "false", true))
					return new CodePrimitiveExpression (false);
				else
					throw new HttpException ("Value '" + str  + "' is not a valid boolean.");
			}

			if (str == null)
				return new CodePrimitiveExpression (null);

			if (type.IsPrimitive)
				return new CodePrimitiveExpression (Convert.ChangeType (str, type));

			if (type.IsEnum) {
				object val = null;
				try {
					val = Enum.Parse (type, str, true);
				} catch (Exception) {
					throw new HttpException (str + " is not a valid value for enum '" + type + "'");
				}
				CodeFieldReferenceExpression expr = new CodeFieldReferenceExpression ();
				expr.TargetObject = new CodeTypeReferenceExpression (type);
				expr.FieldName = val.ToString ();
				return expr;
			}

			if (type == typeof (Color)){
				if (colorConverter == null)
					colorConverter = TypeDescriptor.GetConverter (typeof (Color));

				Color c;
				try {
					c = (Color) colorConverter.ConvertFromString (str);
				} catch (Exception e){
					throw new HttpException ("Color " + str + " is not a valid color.", e);
				}

				if (c.IsKnownColor){
					CodeFieldReferenceExpression expr = new CodeFieldReferenceExpression ();
					expr.TargetObject = new CodeTypeReferenceExpression (type);
					expr.FieldName = c.Name;
					return expr;
				} else {
					CodeMethodReferenceExpression m = new CodeMethodReferenceExpression ();
					m.TargetObject = new CodeTypeReferenceExpression (type);
					m.MethodName = "FromArgb";
					CodeMethodInvokeExpression invoke = new CodeMethodInvokeExpression (m);
					invoke.Parameters.Add (new CodePrimitiveExpression (c.A));
					invoke.Parameters.Add (new CodePrimitiveExpression (c.R));
					invoke.Parameters.Add (new CodePrimitiveExpression (c.G));
					invoke.Parameters.Add (new CodePrimitiveExpression (c.B));
					return invoke;
				}
			}

			bool parms = false;
			BindingFlags flags = BindingFlags.Public | BindingFlags.Static;
			MethodInfo parse = type.GetMethod ("Parse", flags, null, arrayStringCultureInfo, null);
			if (parse != null) {
				parms = true;
			} else {
				parse = type.GetMethod ("Parse", flags, null, arrayString, null);
			}

			if (parse != null) {
				object o = null;
				try {
					if (parms)
						o = parse.Invoke (null, new object [] { str, CultureInfo.InvariantCulture });
					else
						o = parse.Invoke (null, new object [] { str });
				} catch (Exception e) {
					throw new HttpException ("Cannot parse " + str + " as " + type, e);
				}
				
				if (o == null)
					throw new HttpException (str + " as " + type + " is null");

				CodeTypeReferenceExpression exprType = new CodeTypeReferenceExpression (type);
				CodeMethodInvokeExpression invoke = new CodeMethodInvokeExpression (exprType, "Parse");
				//FIXME: may be we gotta ensure roundtrip between o.ToString and Parse
				invoke.Parameters.Add (new CodePrimitiveExpression (o.ToString ()));
				if (parms) {
					CodeTypeReferenceExpression texp = new CodeTypeReferenceExpression (typeof (CultureInfo));
					CodePropertyReferenceExpression pexp = new CodePropertyReferenceExpression ();
					pexp.TargetObject = texp;
					pexp.PropertyName = "InvariantCulture";
					invoke.Parameters.Add (pexp);
				}
				return invoke;
			}
				
			// FIXME: Arrays (arrays of string are implemented in old AspGenerator.cs
			Console.WriteLine ("Unknown type: " + type + " value: " + str);

			return new CodePrimitiveExpression (str);
		}
	}
}

