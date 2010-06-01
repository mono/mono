//
// System.Web.Compilation.TemplateControlCompiler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//	Marek Habersack (mhabersack@novell.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
// (C) 2004-2008 Novell, Inc (http://novell.com)

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
using System;
using System.CodeDom;
using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Util;
using System.ComponentModel.Design.Serialization;
using System.Text.RegularExpressions;

namespace System.Web.Compilation
{
	class TemplateControlCompiler : BaseCompiler
	{
		static BindingFlags noCaseFlags = BindingFlags.Public | BindingFlags.NonPublic |
						  BindingFlags.Instance | BindingFlags.IgnoreCase;
		static Type monoTypeType = Type.GetType ("System.MonoType");
		
		TemplateControlParser parser;
		int dataBoundAtts;
		internal ILocation currentLocation;

		static TypeConverter colorConverter;

		internal static CodeVariableReferenceExpression ctrlVar = new CodeVariableReferenceExpression ("__ctrl");
		
		List <string> masterPageContentPlaceHolders;
		static Regex startsWithBindRegex = new Regex (@"^Bind\s*\(", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		// When modifying those, make sure to look at the SanitizeBindCall to make sure it
		// picks up correct groups.
		static Regex bindRegex = new Regex (@"Bind\s*\(\s*[""']+(.*?)[""']+((\s*,\s*[""']+(.*?)[""']+)?)\s*\)\s*%>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		static Regex bindRegexInValue = new Regex (@"Bind\s*\(\s*[""']+(.*?)[""']+((\s*,\s*[""']+(.*?)[""']+)?)\s*\)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		static Regex evalRegexInValue = new Regex (@"(.*)Eval\s*\(\s*[""']+(.*?)[""']+((\s*,\s*[""']+(.*?)[""']+)?)\s*\)(.*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		List <string> MasterPageContentPlaceHolders {
			get {
				if (masterPageContentPlaceHolders == null)
					masterPageContentPlaceHolders = new List <string> ();
				return masterPageContentPlaceHolders;
			}
		}

		public TemplateControlCompiler (TemplateControlParser parser)
			: base (parser)
		{
			this.parser = parser;
		}

		protected void EnsureID (ControlBuilder builder)
		{
			if (builder.ID == null || builder.ID.Trim () == "")
				builder.ID = builder.GetNextID (null);
		}

		void CreateField (ControlBuilder builder, bool check)
		{
			if (builder == null || builder.ID == null || builder.ControlType == null)
				return;

			if (partialNameOverride [builder.ID] != null)
				return;

			MemberAttributes ma = MemberAttributes.Family;
			currentLocation = builder.Location;
			if (check && CheckBaseFieldOrProperty (builder.ID, builder.ControlType, ref ma))
				return; // The field or property already exists in a base class and is accesible.

			CodeMemberField field;
			field = new CodeMemberField (builder.ControlType.FullName, builder.ID);
			field.Attributes = ma;
			field.Type.Options |= CodeTypeReferenceOptions.GlobalReference;

			if (partialClass != null)
				partialClass.Members.Add (AddLinePragma (field, builder));
			else
				mainClass.Members.Add (AddLinePragma (field, builder));
		}

		bool CheckBaseFieldOrProperty (string id, Type type, ref MemberAttributes ma)
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
				ma |= MemberAttributes.New;
				return false;
			}

			return true;
		}
		
		void AddParsedSubObjectStmt (ControlBuilder builder, CodeExpression expr) 
		{
			if (!builder.HaveParserVariable) {
				CodeVariableDeclarationStatement p = new CodeVariableDeclarationStatement();
				p.Name = "__parser";
				p.Type = new CodeTypeReference (typeof (IParserAccessor));
				p.InitExpression = new CodeCastExpression (typeof (IParserAccessor), ctrlVar);
				builder.MethodStatements.Add (p);
				builder.HaveParserVariable = true;
			}

			CodeVariableReferenceExpression var = new CodeVariableReferenceExpression ("__parser");
			CodeMethodInvokeExpression invoke = new CodeMethodInvokeExpression (var, "AddParsedSubObject");
			invoke.Parameters.Add (expr);
			builder.MethodStatements.Add (AddLinePragma (invoke, builder));
		}
		
		void InitMethod (ControlBuilder builder, bool isTemplate, bool childrenAsProperties)
		{
			currentLocation = builder.Location;
			bool inBuildControlTree = builder is RootBuilder;
			string tailname = (inBuildControlTree ? "Tree" : ("_" + builder.ID));
			CodeMemberMethod method = new CodeMemberMethod ();
			builder.Method = method;
			builder.MethodStatements = method.Statements;

			method.Name = "__BuildControl" + tailname;
			method.Attributes = MemberAttributes.Private | MemberAttributes.Final;
			Type type = builder.ControlType;

			/* in the case this is the __BuildControlTree
			 * method, allow subclasses to insert control
			 * specific code. */
			if (inBuildControlTree) {
				SetCustomAttributes (method);
				AddStatementsToInitMethodTop (builder, method);
			}
			
			if (builder.HasAspCode) {
				CodeMemberMethod renderMethod = new CodeMemberMethod ();
				builder.RenderMethod = renderMethod;
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
			
			if (childrenAsProperties || builder.ControlType == null) {
				string typeString;
				if (builder is RootBuilder)
					typeString = parser.ClassName;
				else {
					if (builder.ControlType != null && builder.IsProperty &&
					    !typeof (ITemplate).IsAssignableFrom (builder.ControlType))
						typeString = builder.ControlType.FullName;
					else 
						typeString = "System.Web.UI.Control";
					ProcessTemplateChildren (builder);
				}

				method.Parameters.Add (new CodeParameterDeclarationExpression (typeString, "__ctrl"));
			} else {
				
				if (typeof (Control).IsAssignableFrom (type))
					method.ReturnType = new CodeTypeReference (typeof (Control));

				// _ctrl = new $controlType ($parameters);
				//
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
				method.Statements.Add (AddLinePragma (assign, builder));
								
				// this.$builderID = _ctrl;
				//
				CodeFieldReferenceExpression builderID = new CodeFieldReferenceExpression ();
				builderID.TargetObject = thisRef;
				builderID.FieldName = builder.ID;
				assign = new CodeAssignStatement ();
				assign.Left = builderID;
				assign.Right = ctrlVar;
				method.Statements.Add (AddLinePragma (assign, builder));

				if (typeof (UserControl).IsAssignableFrom (type)) {
					CodeMethodReferenceExpression mref = new CodeMethodReferenceExpression ();
					mref.TargetObject = builderID;
					mref.MethodName = "InitializeAsUserControl";
					CodeMethodInvokeExpression initAsControl = new CodeMethodInvokeExpression (mref);
					initAsControl.Parameters.Add (new CodePropertyReferenceExpression (thisRef, "Page"));
					method.Statements.Add (initAsControl);
				}

				if (builder.ParentTemplateBuilder is System.Web.UI.WebControls.ContentBuilderInternal) {
					PropertyInfo pi;

					try {
						pi = type.GetProperty ("TemplateControl");
					} catch (Exception) {
						pi = null;
					}

					if (pi != null && pi.CanWrite) {
						// __ctrl.TemplateControl = this;
						assign = new CodeAssignStatement ();
						assign.Left = new CodePropertyReferenceExpression (ctrlVar, "TemplateControl");;
						assign.Right = thisRef;
						method.Statements.Add (assign);
					}
				}
				
				// _ctrl.SkinID = $value
				// _ctrl.ApplyStyleSheetSkin (this);
				//
				// the SkinID assignment needs to come
				// before the call to
				// ApplyStyleSheetSkin, for obvious
				// reasons.  We skip SkinID in
				// CreateAssignStatementsFromAttributes
				// below.
				// 
				string skinid = builder.GetAttribute ("skinid");
				if (!String.IsNullOrEmpty (skinid))
					CreateAssignStatementFromAttribute (builder, "skinid");

				if (typeof (WebControl).IsAssignableFrom (type)) {
					CodeMethodInvokeExpression applyStyleSheetSkin = new CodeMethodInvokeExpression (ctrlVar, "ApplyStyleSheetSkin");
					if (typeof (Page).IsAssignableFrom (parser.BaseType))
						applyStyleSheetSkin.Parameters.Add (thisRef);
					else
						applyStyleSheetSkin.Parameters.Add (new CodePropertyReferenceExpression (thisRef, "Page"));
					method.Statements.Add (applyStyleSheetSkin);
				}

				// Process template children before anything else
				ProcessTemplateChildren (builder);

				// process ID here. It should be set before any other attributes are
				// assigned, since the control code may rely on ID being set. We
				// skip ID in CreateAssignStatementsFromAttributes
				string ctl_id = builder.GetAttribute ("id");
				if (ctl_id != null && ctl_id.Length != 0)
					CreateAssignStatementFromAttribute (builder, "id");
				
				if (typeof (ContentPlaceHolder).IsAssignableFrom (type)) {
					List <string> placeHolderIds = MasterPageContentPlaceHolders;
					string cphID = builder.ID;
					
					if (!placeHolderIds.Contains (cphID))
						placeHolderIds.Add (cphID);

					CodeConditionStatement condStatement;

					// Add the __Template_* field
					string templateField = "__Template_" + cphID;
					CodeMemberField fld = new CodeMemberField (typeof (ITemplate), templateField);
					fld.Attributes = MemberAttributes.Private;
					mainClass.Members.Add (fld);

					CodeFieldReferenceExpression templateID = new CodeFieldReferenceExpression ();
					templateID.TargetObject = thisRef;
					templateID.FieldName = templateField;

					CreateContentPlaceHolderTemplateProperty (templateField, "Template_" + cphID);
					
					// if ((this.ContentTemplates != null)) {
					// 	this.__Template_$builder.ID = ((System.Web.UI.ITemplate)(this.ContentTemplates["$builder.ID"]));
					// }
					//
					CodeFieldReferenceExpression contentTemplates = new CodeFieldReferenceExpression ();
					contentTemplates.TargetObject = thisRef;
					contentTemplates.FieldName = "ContentTemplates";

					CodeIndexerExpression indexer = new CodeIndexerExpression ();
					indexer.TargetObject = new CodePropertyReferenceExpression (thisRef, "ContentTemplates");
					indexer.Indices.Add (new CodePrimitiveExpression (cphID));

					assign = new CodeAssignStatement ();
					assign.Left = templateID;
					assign.Right = new CodeCastExpression (new CodeTypeReference (typeof (ITemplate)), indexer);

					condStatement = new CodeConditionStatement (new CodeBinaryOperatorExpression (contentTemplates,
														      CodeBinaryOperatorType.IdentityInequality,
														      new CodePrimitiveExpression (null)),
										    assign);

					method.Statements.Add (condStatement);

					// if ((this.__Template_mainContent != null)) {
					// 	this.__Template_mainContent.InstantiateIn(__ctrl);
					// }
					// and also set things up such that any additional code ends up in:
					// else {
					// 	...
					// }
					//
					CodeMethodReferenceExpression methodRef = new CodeMethodReferenceExpression ();
					methodRef.TargetObject = templateID;
					methodRef.MethodName = "InstantiateIn";

					CodeMethodInvokeExpression instantiateInInvoke;
					instantiateInInvoke = new CodeMethodInvokeExpression (methodRef, ctrlVar);

					condStatement = new CodeConditionStatement (new CodeBinaryOperatorExpression (templateID,
														      CodeBinaryOperatorType.IdentityInequality,
														      new CodePrimitiveExpression (null)),
										    new CodeExpressionStatement (instantiateInInvoke));
					method.Statements.Add (condStatement);

					// this is the bit that causes the following stuff to end up in the else { }
					builder.MethodStatements = condStatement.FalseStatements;
				}
			}

			if (inBuildControlTree)
				AddStatementsToInitMethodBottom (builder, method);
			
			mainClass.Members.Add (method);
		}

		void ProcessTemplateChildren (ControlBuilder builder)
		{
			ArrayList templates = builder.TemplateChildren;
			if (templates != null && templates.Count > 0) {
				foreach (TemplateBuilder tb in templates) {
					CreateControlTree (tb, true, false);
					if (tb.BindingDirection == BindingDirection.TwoWay) {
						string extractMethod = CreateExtractValuesMethod (tb);
						AddBindableTemplateInvocation (builder, tb.TagName, tb.Method.Name, extractMethod);
					} else
						AddTemplateInvocation (builder, tb.TagName, tb.Method.Name);
				}
			}
		}
		
		void SetCustomAttribute (CodeMemberMethod method, UnknownAttributeDescriptor uad)
		{
			CodeAssignStatement assign = new CodeAssignStatement ();
			assign.Left = new CodePropertyReferenceExpression (
				new CodeArgumentReferenceExpression("__ctrl"),
				uad.Info.Name);
			assign.Right = GetExpressionFromString (uad.Value.GetType (), uad.Value.ToString (), uad.Info);
			
			method.Statements.Add (assign);
		}
		
		void SetCustomAttributes (CodeMemberMethod method)
		{
			Type baseType = parser.BaseType;
			if (baseType == null)
				return;
			
			List <UnknownAttributeDescriptor> attrs = parser.UnknownMainAttributes;
			if (attrs == null || attrs.Count == 0)
				return;

			foreach (UnknownAttributeDescriptor uad in attrs)
				SetCustomAttribute (method, uad);
		}
		
		protected virtual void AddStatementsToInitMethodTop (ControlBuilder builder, CodeMemberMethod method)
		{
		}

		protected virtual void AddStatementsToInitMethodBottom (ControlBuilder builder, CodeMemberMethod method)
		{
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
				builder.RenderMethod.Statements.Add (expr);
			}
		}

		string TrimDB (string value, bool trimTail)
		{
			string str = value.Trim ();
			int len = str.Length;
			int idx = str.IndexOf ('#', 2) + 1;
			if (idx >= len)
				return String.Empty;
			if (trimTail)
				len -= 2;
			
			return str.Substring (idx, len - idx).Trim ();
		}

		CodeExpression CreateEvalInvokeExpression (Regex regex, string value, bool isBind)
		{
			Match match = regex.Match (value);
			if (!match.Success) {
				if (isBind)
					throw new HttpParseException ("Bind invocation wasn't formatted properly.");
				return null;
			}
			
			string sanitizedSnippet;
			if (isBind)
				sanitizedSnippet = SanitizeBindCall (match);
			else
				sanitizedSnippet = value;
			
			return new CodeSnippetExpression (sanitizedSnippet);
		}

		string SanitizeBindCall (Match match)
		{
			GroupCollection groups = match.Groups;
			StringBuilder sb = new StringBuilder ("Eval(\"" + groups [1] + "\"");
			Group second = groups [4];
			if (second != null) {
				string v = second.Value;
				if (v != null && v.Length > 0)
					sb.Append (",\"" + second + "\"");
			}
			
			sb.Append (")");
			return sb.ToString ();
		}
		
		string DataBoundProperty (ControlBuilder builder, Type type, string varName, string value)
		{
			value = TrimDB (value, true);
			CodeMemberMethod method;
			string dbMethodName = builder.Method.Name + "_DB_" + dataBoundAtts++;
			CodeExpression valueExpression = null;
			value = value.Trim ();
			
			bool need_if = false;
			if (startsWithBindRegex.Match (value).Success) {
				valueExpression = CreateEvalInvokeExpression (bindRegexInValue, value, true);
				if (valueExpression != null)
					need_if = true;
			} else
				if (StrUtils.StartsWith (value, "Eval", true))
					valueExpression = CreateEvalInvokeExpression (evalRegexInValue, value, false);
			
			if (valueExpression == null)
				valueExpression = new CodeSnippetExpression (value);
			
			method = CreateDBMethod (builder, dbMethodName, GetContainerType (builder), builder.ControlType);
			CodeVariableReferenceExpression targetExpr = new CodeVariableReferenceExpression ("target");

			// This should be a CodePropertyReferenceExpression for properties... but it works anyway
			CodeFieldReferenceExpression field = new CodeFieldReferenceExpression (targetExpr, varName);

			CodeExpression expr;
			if (type == typeof (string)) {
				CodeMethodInvokeExpression tostring = new CodeMethodInvokeExpression ();
				CodeTypeReferenceExpression conv = new CodeTypeReferenceExpression (typeof (Convert));
				tostring.Method = new CodeMethodReferenceExpression (conv, "ToString");
				tostring.Parameters.Add (valueExpression);
				expr = tostring;
			} else
				expr = new CodeCastExpression (type, valueExpression);

			CodeAssignStatement assign = new CodeAssignStatement (field, expr);
			if (need_if) {
				CodeExpression page = new CodePropertyReferenceExpression (thisRef, "Page");
				CodeExpression left = new CodeMethodInvokeExpression (page, "GetDataItem");
				CodeBinaryOperatorExpression ce = new CodeBinaryOperatorExpression (left, CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression (null));
				CodeConditionStatement ccs = new CodeConditionStatement (ce, assign);
				method.Statements.Add (ccs);
			} else
				method.Statements.Add (assign);

			mainClass.Members.Add (method);
			return method.Name;
		}

		void AddCodeForPropertyOrField (ControlBuilder builder, Type type, string var_name, string att, MemberInfo member, bool isDataBound, bool isExpression)
		{
			CodeMemberMethod method = builder.Method;
			bool isWritable = IsWritablePropertyOrField (member);
			
			if (isDataBound && isWritable) {
				string dbMethodName = DataBoundProperty (builder, type, var_name, att);
				AddEventAssign (method, builder, "DataBinding", typeof (EventHandler), dbMethodName);
				return;
			} else if (isExpression && isWritable) {
				AddExpressionAssign (method, builder, member, type, var_name, att);
				return;
			}

			CodeAssignStatement assign = new CodeAssignStatement ();
			assign.Left = new CodePropertyReferenceExpression (ctrlVar, var_name);
			currentLocation = builder.Location;
			assign.Right = GetExpressionFromString (type, att, member);

			method.Statements.Add (AddLinePragma (assign, builder));
		}

		void RegisterBindingInfo (ControlBuilder builder, string propName, ref string value)
		{
			string str = TrimDB (value, false);
			if (StrUtils.StartsWith (str, "Bind", true)) {
				Match match = bindRegex.Match (str);
				if (match.Success) {
					string bindingName = match.Groups [1].Value;
					TemplateBuilder templateBuilder = builder.ParentTemplateBuilder;
					
					if (templateBuilder == null)
						throw new HttpException ("Bind expression not allowed in this context.");

					if (templateBuilder.BindingDirection == BindingDirection.OneWay)
						return;
					
					string id = builder.GetAttribute ("ID");
					if (String.IsNullOrEmpty (id))
						throw new HttpException ("Control of type '" + builder.ControlType + "' using two-way binding on property '" + propName + "' must have an ID.");
					
					templateBuilder.RegisterBoundProperty (builder.ControlType, propName, id, bindingName);
				}
			}
		}

		/*
		static bool InvariantCompare (string a, string b)
		{
			return (0 == String.Compare (a, b, false, Helpers.InvariantCulture));
		}
		*/

		static bool InvariantCompareNoCase (string a, string b)
		{
			return (0 == String.Compare (a, b, true, Helpers.InvariantCulture));
		}

		internal static MemberInfo GetFieldOrProperty (Type type, string name)
		{
			MemberInfo member = null;
			try {
				member = type.GetProperty (name, noCaseFlags & ~BindingFlags.NonPublic);
			} catch {}
			
			if (member != null)
				return member;

			try {
				member = type.GetField (name, noCaseFlags & ~BindingFlags.NonPublic);
			} catch {}

			return member;
		}

		static bool IsWritablePropertyOrField (MemberInfo member)
		{
			PropertyInfo pi = member as PropertyInfo;
			if (pi != null)
				return pi.GetSetMethod (false) != null;
			FieldInfo fi = member as FieldInfo;
			if (fi != null)
				return !fi.IsInitOnly;
			throw new ArgumentException ("Argument must be of PropertyInfo or FieldInfo type", "member");
		}

		bool ProcessPropertiesAndFields (ControlBuilder builder, MemberInfo member, string id,
						 string attValue, string prefix)
		{
			int hyphen = id.IndexOf ('-');
			bool isPropertyInfo = (member is PropertyInfo);
			bool isDataBound = BaseParser.IsDataBound (attValue);
			bool isExpression = !isDataBound && BaseParser.IsExpression (attValue);
			Type type;
			if (isPropertyInfo) {
				type = ((PropertyInfo) member).PropertyType;
			} else {
				type = ((FieldInfo) member).FieldType;
			}

			if (InvariantCompareNoCase (member.Name, id)) {
				if (isDataBound)
					RegisterBindingInfo (builder, member.Name, ref attValue);				

				if (!IsWritablePropertyOrField (member))
					return false;
				
				AddCodeForPropertyOrField (builder, type, member.Name, attValue, member, isDataBound, isExpression);
				return true;
			}
			
			if (hyphen == -1)
				return false;

			string prop_field = id.Replace ('-', '.');
			string [] parts = prop_field.Split (new char [] {'.'});
			int length = parts.Length;
			
			if (length < 2 || !InvariantCompareNoCase (member.Name, parts [0]))
				return false;

			if (length > 2) {
				MemberInfo sub_member = GetFieldOrProperty (type, parts [1]);
				if (sub_member == null)
					return false;

				string new_prefix = prefix + member.Name + ".";
				string new_id = id.Substring (hyphen + 1);
				return ProcessPropertiesAndFields (builder, sub_member, new_id, attValue, new_prefix);
			}

			MemberInfo subpf = GetFieldOrProperty (type, parts [1]);
			if (!(subpf is PropertyInfo))
				return false;

			PropertyInfo subprop = (PropertyInfo) subpf;
			if (subprop.CanWrite == false)
				return false;

			bool is_bool = (subprop.PropertyType == typeof (bool));
			if (!is_bool && attValue == null)
				return false; // Font-Size -> Font-Size="" as html

			string val = attValue;
			if (attValue == null && is_bool)
				val = "true"; // Font-Bold <=> Font-Bold="true"

			if (isDataBound)
				RegisterBindingInfo (builder, prefix + member.Name + "." + subprop.Name, ref attValue);

			AddCodeForPropertyOrField (builder, subprop.PropertyType,
						   prefix + member.Name + "." + subprop.Name,
						   val, subprop, isDataBound, isExpression);

			return true;
		}

		internal CodeExpression CompileExpression (MemberInfo member, Type type, string value, bool useSetAttribute)
		{
			// First let's find the correct expression builder
			value = value.Substring (3, value.Length - 5).Trim ();
			int colon = value.IndexOf (':');
			if (colon == -1)
				return null;
			string prefix = value.Substring (0, colon).Trim ();
			string expr = value.Substring (colon + 1).Trim ();
			
			CompilationSection cs = (CompilationSection)WebConfigurationManager.GetWebApplicationSection ("system.web/compilation");
			if (cs == null)
				return null;
			
			if (cs.ExpressionBuilders == null || cs.ExpressionBuilders.Count == 0)
				return null;

			System.Web.Configuration.ExpressionBuilder ceb = cs.ExpressionBuilders[prefix];
			if (ceb == null)
				return null;
			
			string builderType = ceb.Type;
			Type t;
			
			try {
				t = HttpApplication.LoadType (builderType, true);
			} catch (Exception e) {
				throw new HttpException (String.Format ("Failed to load expression builder type `{0}'", builderType), e);
			}

			if (!typeof (System.Web.Compilation.ExpressionBuilder).IsAssignableFrom (t))
				throw new HttpException (String.Format ("Type {0} is not descendant from System.Web.Compilation.ExpressionBuilder", builderType));

			System.Web.Compilation.ExpressionBuilder eb = null;
			object parsedData;
			ExpressionBuilderContext ctx;
			
			try {
				eb = Activator.CreateInstance (t) as System.Web.Compilation.ExpressionBuilder;
				ctx = new ExpressionBuilderContext (HttpContext.Current.Request.FilePath);
				parsedData = eb.ParseExpression (expr, type, ctx);
			} catch (Exception e) {
				throw new HttpException (String.Format ("Failed to create an instance of type `{0}'", builderType), e);
			}
			
			BoundPropertyEntry bpe = CreateBoundPropertyEntry (member as PropertyInfo, prefix, expr, useSetAttribute);
			return eb.GetCodeExpression (bpe, parsedData, ctx);
		}
		
		void AddExpressionAssign (CodeMemberMethod method, ControlBuilder builder, MemberInfo member, Type type, string name, string value)
		{
			CodeExpression expr = CompileExpression (member, type, value, false);

			if (expr == null)
				return;
			
			CodeAssignStatement assign = new CodeAssignStatement ();
			assign.Left = new CodePropertyReferenceExpression (ctrlVar, name);
			assign.Right = expr;
			
			builder.Method.Statements.Add (AddLinePragma (assign, builder));
		}

		BoundPropertyEntry CreateBoundPropertyEntry (PropertyInfo pi, string prefix, string expr, bool useSetAttribute)
		{
			BoundPropertyEntry ret = new BoundPropertyEntry ();
			ret.Expression = expr;
			ret.ExpressionPrefix = prefix;
			ret.Generated = false;
			if (pi != null) {
				ret.Name = pi.Name;
				ret.PropertyInfo = pi;
				ret.Type = pi.PropertyType;
			}
			ret.UseSetAttribute = useSetAttribute;
			
			return ret;
		}

		bool ResourceProviderHasObject (string key)
		{
			IResourceProvider rp = HttpContext.GetResourceProvider (InputVirtualPath.Absolute, true);
			if (rp == null)
				return false;

			IResourceReader rr = rp.ResourceReader;
			if (rr == null)
				return false;

			try {
				IDictionaryEnumerator ide = rr.GetEnumerator ();
				if (ide == null)
					return false;
			
				string dictKey;
				while (ide.MoveNext ()) {
					dictKey = ide.Key as string;
					if (String.IsNullOrEmpty (dictKey))
						continue;
					if (String.Compare (key, dictKey, StringComparison.Ordinal) == 0)
						return true;
				}
			} finally {
				rr.Close ();
			}
			
			return false;
		}
		
		void AssignPropertyFromResources (ControlBuilder builder, MemberInfo mi, string attvalue)
		{
			bool isProperty = mi.MemberType == MemberTypes.Property;
			bool isField = !isProperty && (mi.MemberType == MemberTypes.Field);

			if (!isProperty && !isField || !IsWritablePropertyOrField (mi))
				return;			

			object[] attrs = mi.GetCustomAttributes (typeof (LocalizableAttribute), true);
			if (attrs != null && attrs.Length > 0 && !((LocalizableAttribute)attrs [0]).IsLocalizable)
				return;
			
			string memberName = mi.Name;
			string resname = String.Concat (attvalue, ".", memberName);

			if (!ResourceProviderHasObject (resname))
				return;
			
			// __ctrl.Text = System.Convert.ToString(HttpContext.GetLocalResourceObject("ButtonResource1.Text"));
			string inputFile = parser.InputFile;
			string physPath = HttpContext.Current.Request.PhysicalApplicationPath;
	
			if (StrUtils.StartsWith (inputFile, physPath))
				inputFile = parser.InputFile.Substring (physPath.Length - 1);
			else
				return;

			char dsc = System.IO.Path.DirectorySeparatorChar;
			if (dsc != '/')
				inputFile = inputFile.Replace (dsc, '/');

			object obj = HttpContext.GetLocalResourceObject (inputFile, resname);
			if (obj == null)
				return;

			if (!isProperty && !isField)
				return; // an "impossible" case
			
			CodeAssignStatement assign = new CodeAssignStatement ();
			
			assign.Left = new CodePropertyReferenceExpression (ctrlVar, memberName);
			assign.Right = ResourceExpressionBuilder.CreateGetLocalResourceObject (mi, resname);
			
			builder.Method.Statements.Add (AddLinePragma (assign, builder));
		}

		void AssignPropertiesFromResources (ControlBuilder builder, Type controlType, string attvalue)
		{
			// Process all public fields and properties of the control. We don't use GetMembers to make the code
			// faster
			FieldInfo [] fields = controlType.GetFields (
				BindingFlags.Instance | BindingFlags.Static |
				BindingFlags.Public | BindingFlags.FlattenHierarchy);
			PropertyInfo [] properties = controlType.GetProperties (
				BindingFlags.Instance | BindingFlags.Static |
				BindingFlags.Public | BindingFlags.FlattenHierarchy);

			foreach (FieldInfo fi in fields)
				AssignPropertyFromResources (builder, fi, attvalue);
			foreach (PropertyInfo pi in properties)
				AssignPropertyFromResources (builder, pi, attvalue);
		}
		
		void AssignPropertiesFromResources (ControlBuilder builder, string attvalue)
		{
			if (attvalue == null || attvalue.Length == 0)
				return;
			
			Type controlType = builder.ControlType;
			if (controlType == null)
				return;

			AssignPropertiesFromResources (builder, controlType, attvalue);
		}
		
		void AddEventAssign (CodeMemberMethod method, ControlBuilder builder, string name, Type type, string value)
		{
			//"__ctrl.{0} += new {1} (this.{2});"
			CodeEventReferenceExpression evtID = new CodeEventReferenceExpression (ctrlVar, name);

			CodeDelegateCreateExpression create;
			create = new CodeDelegateCreateExpression (new CodeTypeReference (type), thisRef, value);

			CodeAttachEventStatement attach = new CodeAttachEventStatement (evtID, create);
			method.Statements.Add (attach);
		}
		
		void CreateAssignStatementFromAttribute (ControlBuilder builder, string id)
		{
			EventInfo [] ev_info = null;
			Type type = builder.ControlType;
			
			string attvalue = builder.GetAttribute (id);
			if (id.Length > 2 && String.Compare (id.Substring (0, 2), "ON", true, Helpers.InvariantCulture) == 0){
				if (ev_info == null)
					ev_info = type.GetEvents ();

				string id_as_event = id.Substring (2);
				foreach (EventInfo ev in ev_info){
					if (InvariantCompareNoCase (ev.Name, id_as_event)){
						AddEventAssign (builder.Method,
								builder,
								ev.Name,
								ev.EventHandlerType,
								attvalue);
						
						return;
					}
				}

			}

			if (String.Compare (id, "meta:resourcekey", StringComparison.OrdinalIgnoreCase) == 0) {
				AssignPropertiesFromResources (builder, attvalue);
				return;
			}
			
			int hyphen = id.IndexOf ('-');
			string alt_id = id;
			if (hyphen != -1)
				alt_id = id.Substring (0, hyphen);

			MemberInfo fop = GetFieldOrProperty (type, alt_id);
			if (fop != null) {
				if (ProcessPropertiesAndFields (builder, fop, id, attvalue, null))
					return;
			}

			if (!typeof (IAttributeAccessor).IsAssignableFrom (type))
				throw new ParseException (builder.Location, "Unrecognized attribute: " + id);

			CodeMemberMethod method = builder.Method;
			bool isDatabound = BaseParser.IsDataBound (attvalue);
			bool isExpression = !isDatabound && BaseParser.IsExpression (attvalue);

			if (isDatabound) {
				string value = attvalue.Substring (3, attvalue.Length - 5).Trim ();
				CodeExpression valueExpression = null;
				if (startsWithBindRegex.Match (value).Success)
					valueExpression = CreateEvalInvokeExpression (bindRegexInValue, value, true);
				else
					if (StrUtils.StartsWith (value, "Eval", true))
						valueExpression = CreateEvalInvokeExpression (evalRegexInValue, value, false);
				
				if (valueExpression == null && value != null && value.Trim () != String.Empty)
					valueExpression = new CodeSnippetExpression (value);
				
				CreateDBAttributeMethod (builder, id, valueExpression);
			} else {
				CodeCastExpression cast;
				CodeMethodReferenceExpression methodExpr;
				CodeMethodInvokeExpression expr;

				cast = new CodeCastExpression (typeof (IAttributeAccessor), ctrlVar);
				methodExpr = new CodeMethodReferenceExpression (cast, "SetAttribute");
				expr = new CodeMethodInvokeExpression (methodExpr);
				expr.Parameters.Add (new CodePrimitiveExpression (id));

				CodeExpression valueExpr = null;
				if (isExpression)
					valueExpr = CompileExpression (null, typeof (string), attvalue, true);

				if (valueExpr == null)
					valueExpr = new CodePrimitiveExpression (attvalue);
				
				expr.Parameters.Add (valueExpr);
				method.Statements.Add (AddLinePragma (expr, builder));
			}
		}

		protected void CreateAssignStatementsFromAttributes (ControlBuilder builder)
		{
			this.dataBoundAtts = 0;
			IDictionary atts = builder.Attributes;
			if (atts == null || atts.Count == 0)
				return;
			
			foreach (string id in atts.Keys) {
				if (InvariantCompareNoCase (id, "runat"))
					continue;
				// ID is assigned in BuildControltree
				if (InvariantCompareNoCase (id, "id"))
					continue;				

				/* we skip SkinID here as it's assigned in BuildControlTree */
				if (InvariantCompareNoCase (id, "skinid"))
					continue;
				if (InvariantCompareNoCase (id, "meta:resourcekey"))
					continue; // ignore, this one's processed at the very end of
						  // the method
				CreateAssignStatementFromAttribute (builder, id);
			}
		}

		void CreateDBAttributeMethod (ControlBuilder builder, string attr, CodeExpression code)
		{
			if (code == null)
				return;

			string id = builder.GetNextID (null);
			string dbMethodName = "__DataBind_" + id;
			CodeMemberMethod method = builder.Method;
			AddEventAssign (method, builder, "DataBinding", typeof (EventHandler), dbMethodName);

			method = CreateDBMethod (builder, dbMethodName, GetContainerType (builder), builder.ControlType);
			builder.DataBindingMethod = method;

			CodeCastExpression cast;
			CodeMethodReferenceExpression methodExpr;
			CodeMethodInvokeExpression expr;

			CodeVariableReferenceExpression targetExpr = new CodeVariableReferenceExpression ("target");
			cast = new CodeCastExpression (typeof (IAttributeAccessor), targetExpr);
			methodExpr = new CodeMethodReferenceExpression (cast, "SetAttribute");
			expr = new CodeMethodInvokeExpression (methodExpr);
			expr.Parameters.Add (new CodePrimitiveExpression (attr));
			CodeMethodInvokeExpression tostring = new CodeMethodInvokeExpression ();
			tostring.Method = new CodeMethodReferenceExpression (
							new CodeTypeReferenceExpression (typeof (Convert)),
							"ToString");
			tostring.Parameters.Add (code);
			expr.Parameters.Add (tostring);
			method.Statements.Add (expr);
			mainClass.Members.Add (method);
		}

		void AddRenderControl (ControlBuilder builder)
		{
			CodeIndexerExpression indexer = new CodeIndexerExpression ();
			indexer.TargetObject = new CodePropertyReferenceExpression (
							new CodeArgumentReferenceExpression ("parameterContainer"),
							"Controls");
							
			indexer.Indices.Add (new CodePrimitiveExpression (builder.RenderIndex));
			
			CodeMethodInvokeExpression invoke = new CodeMethodInvokeExpression (indexer, "RenderControl");
			invoke.Parameters.Add (new CodeArgumentReferenceExpression ("__output"));
			builder.RenderMethod.Statements.Add (invoke);
			builder.IncreaseRenderIndex ();
		}

		protected void AddChildCall (ControlBuilder parent, ControlBuilder child)
		{
			if (parent == null || child == null)
				return;
			
			CodeMethodReferenceExpression m = new CodeMethodReferenceExpression (thisRef, child.Method.Name);
			CodeMethodInvokeExpression expr = new CodeMethodInvokeExpression (m);

			object [] atts = null;

			if (child.ControlType != null)
				atts = child.ControlType.GetCustomAttributes (typeof (PartialCachingAttribute), true);
			
			if (atts != null && atts.Length > 0) {
				PartialCachingAttribute pca = (PartialCachingAttribute) atts [0];
				CodeTypeReferenceExpression cc = new CodeTypeReferenceExpression("System.Web.UI.StaticPartialCachingControl");
				CodeMethodInvokeExpression build = new CodeMethodInvokeExpression (cc, "BuildCachedControl");
				build.Parameters.Add (new CodeArgumentReferenceExpression("__ctrl"));
				build.Parameters.Add (new CodePrimitiveExpression (child.ID));
#if NET_1_1
				if (pca.Shared)
					build.Parameters.Add (new CodePrimitiveExpression (child.ControlType.GetHashCode ().ToString ()));
				else
#endif
					build.Parameters.Add (new CodePrimitiveExpression (Guid.NewGuid ().ToString ()));
					
				build.Parameters.Add (new CodePrimitiveExpression (pca.Duration));
				build.Parameters.Add (new CodePrimitiveExpression (pca.VaryByParams));
				build.Parameters.Add (new CodePrimitiveExpression (pca.VaryByControls));
				build.Parameters.Add (new CodePrimitiveExpression (pca.VaryByCustom));
				build.Parameters.Add (new CodeDelegateCreateExpression (
							      new CodeTypeReference (typeof (System.Web.UI.BuildMethod)),
							      thisRef, child.Method.Name));

				parent.MethodStatements.Add (AddLinePragma (build, parent));
				if (parent.HasAspCode)
					AddRenderControl (parent);
				return;
			}
                                
			if (child.IsProperty || parent.ChildrenAsProperties) {
				expr.Parameters.Add (new CodeFieldReferenceExpression (ctrlVar, child.TagName));
				parent.MethodStatements.Add (AddLinePragma (expr, parent));
				return;
			}

			parent.MethodStatements.Add (AddLinePragma (expr, parent));
			CodeFieldReferenceExpression field = new CodeFieldReferenceExpression (thisRef, child.ID);
			if (parent.ControlType == null || typeof (IParserAccessor).IsAssignableFrom (parent.ControlType))
				AddParsedSubObjectStmt (parent, field);
			else {
				CodeMethodInvokeExpression invoke = new CodeMethodInvokeExpression (ctrlVar, "Add");
				invoke.Parameters.Add (field);
				parent.MethodStatements.Add (AddLinePragma (invoke, parent));
			}
				
			if (parent.HasAspCode)
				AddRenderControl (parent);
		}

		void AddTemplateInvocation (ControlBuilder builder, string name, string methodName)
		{
			CodePropertyReferenceExpression prop = new CodePropertyReferenceExpression (ctrlVar, name);

			CodeDelegateCreateExpression newBuild = new CodeDelegateCreateExpression (
				new CodeTypeReference (typeof (BuildTemplateMethod)), thisRef, methodName);

			CodeObjectCreateExpression newCompiled = new CodeObjectCreateExpression (typeof (CompiledTemplateBuilder));
			newCompiled.Parameters.Add (newBuild);

			CodeAssignStatement assign = new CodeAssignStatement (prop, newCompiled);
			builder.Method.Statements.Add (AddLinePragma (assign, builder));
		}

		void AddBindableTemplateInvocation (ControlBuilder builder, string name, string methodName, string extractMethodName)
		{
			CodePropertyReferenceExpression prop = new CodePropertyReferenceExpression (ctrlVar, name);

			CodeDelegateCreateExpression newBuild = new CodeDelegateCreateExpression (
				new CodeTypeReference (typeof (BuildTemplateMethod)), thisRef, methodName);

			CodeDelegateCreateExpression newExtract = new CodeDelegateCreateExpression (
				new CodeTypeReference (typeof (ExtractTemplateValuesMethod)), thisRef, extractMethodName);

			CodeObjectCreateExpression newCompiled = new CodeObjectCreateExpression (typeof (CompiledBindableTemplateBuilder));
			newCompiled.Parameters.Add (newBuild);
			newCompiled.Parameters.Add (newExtract);
			
			CodeAssignStatement assign = new CodeAssignStatement (prop, newCompiled);
			builder.Method.Statements.Add (AddLinePragma (assign, builder));
		}
		
		string CreateExtractValuesMethod (TemplateBuilder builder)
		{
			CodeMemberMethod method = new CodeMemberMethod ();
			method.Name = "__ExtractValues_" + builder.ID;
			method.Attributes = MemberAttributes.Private | MemberAttributes.Final;
			method.ReturnType = new CodeTypeReference (typeof(IOrderedDictionary));
			
			CodeParameterDeclarationExpression arg = new CodeParameterDeclarationExpression ();
			arg.Type = new CodeTypeReference (typeof (Control));
			arg.Name = "__container";
			method.Parameters.Add (arg);
			mainClass.Members.Add (method);
			
			CodeObjectCreateExpression newTable = new CodeObjectCreateExpression ();
			newTable.CreateType = new CodeTypeReference (typeof(OrderedDictionary));
			method.Statements.Add (new CodeVariableDeclarationStatement (typeof(OrderedDictionary), "__table", newTable));
			CodeVariableReferenceExpression tableExp = new CodeVariableReferenceExpression ("__table");
			
			if (builder.Bindings != null) {
				Hashtable hash = new Hashtable ();
				foreach (TemplateBinding binding in builder.Bindings) {
					CodeConditionStatement sif;
					CodeVariableReferenceExpression control;
					CodeAssignStatement assign;

					if (hash [binding.ControlId] == null) {

						CodeVariableDeclarationStatement dec = new CodeVariableDeclarationStatement (binding.ControlType, binding.ControlId);
						method.Statements.Add (dec);
						CodeVariableReferenceExpression cter = new CodeVariableReferenceExpression ("__container");
						CodeMethodInvokeExpression invoke = new CodeMethodInvokeExpression (cter, "FindControl");
						invoke.Parameters.Add (new CodePrimitiveExpression (binding.ControlId));

						assign = new CodeAssignStatement ();
						control = new CodeVariableReferenceExpression (binding.ControlId);
						assign.Left = control;
						assign.Right = new CodeCastExpression (binding.ControlType, invoke);
						method.Statements.Add (assign);

						sif = new CodeConditionStatement ();
						sif.Condition = new CodeBinaryOperatorExpression (control, CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression (null));

						method.Statements.Add (sif);

						hash [binding.ControlId] = sif;
					}

					sif = (CodeConditionStatement) hash [binding.ControlId];
					control = new CodeVariableReferenceExpression (binding.ControlId);
					assign = new CodeAssignStatement ();
					assign.Left = new CodeIndexerExpression (tableExp, new CodePrimitiveExpression (binding.FieldName));
					assign.Right = new CodePropertyReferenceExpression (control, binding.ControlProperty);
					sif.TrueStatements.Add (assign);
				}
			}

			method.Statements.Add (new CodeMethodReturnStatement (tableExp));
			return method.Name;
		}

		void AddContentTemplateInvocation (ContentBuilderInternal cbuilder, CodeMemberMethod method, string methodName)
		{
			CodeDelegateCreateExpression newBuild = new CodeDelegateCreateExpression (
				new CodeTypeReference (typeof (BuildTemplateMethod)), thisRef, methodName);

			CodeObjectCreateExpression newCompiled = new CodeObjectCreateExpression (typeof (CompiledTemplateBuilder));
			newCompiled.Parameters.Add (newBuild);
			
			CodeMethodInvokeExpression invoke = new CodeMethodInvokeExpression (thisRef, "AddContentTemplate");
			invoke.Parameters.Add (new CodePrimitiveExpression (cbuilder.ContentPlaceHolderID));
			invoke.Parameters.Add (newCompiled);

			method.Statements.Add (AddLinePragma (invoke, cbuilder));
		}

		void AddCodeRender (ControlBuilder parent, CodeRenderBuilder cr)
		{
			if (cr.Code == null || cr.Code.Trim () == "")
				return;

			if (!cr.IsAssign) {
				CodeSnippetStatement code = new CodeSnippetStatement (cr.Code);
				parent.RenderMethod.Statements.Add (AddLinePragma (code, cr));
				return;
			}

			CodeMethodInvokeExpression expr = new CodeMethodInvokeExpression ();
			expr.Method = new CodeMethodReferenceExpression (
							new CodeArgumentReferenceExpression ("__output"),
							"Write");

			expr.Parameters.Add (GetWrappedCodeExpression (cr));
			parent.RenderMethod.Statements.Add (AddLinePragma (expr, cr));
		}

		CodeExpression GetWrappedCodeExpression (CodeRenderBuilder cr)
		{
			var ret = new CodeSnippetExpression (cr.Code);
#if NET_4_0
			if (cr.HtmlEncode) {
				var encodeRef = new CodeMethodReferenceExpression (new CodeTypeReferenceExpression (typeof (HttpUtility)), "HtmlEncode");
				return new CodeMethodInvokeExpression (encodeRef, new CodeExpression[] { ret });
			} else
#endif
				return ret;
		}
		
		static Type GetContainerType (ControlBuilder builder)
		{
			return builder.BindingContainerType;
		}
		
		CodeMemberMethod CreateDBMethod (ControlBuilder builder, string name, Type container, Type target)
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
			method.Statements.Add (AddLinePragma (assign, builder));

			assign = new CodeAssignStatement ();
			assign.Left = new CodeVariableReferenceExpression ("Container");
			assign.Right = new CodeCastExpression (containerRef,
						new CodePropertyReferenceExpression (targetExpr, "BindingContainer"));
			method.Statements.Add (AddLinePragma (assign, builder));

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
			CodeMemberMethod method = db.Method;
			AddEventAssign (method, builder, "DataBinding", typeof (EventHandler), dbMethodName);
			method.Statements.Add (new CodeMethodReturnStatement (ctrlVar));

			// Add the DataBind handler
			method = CreateDBMethod (builder, dbMethodName, GetContainerType (builder), typeof (DataBoundLiteralControl));
			builder.DataBindingMethod = method;

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
			method.Statements.Add (AddLinePragma (invoke, builder));
			
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

		protected void CreateControlTree (ControlBuilder builder, bool inTemplate, bool childrenAsProperties)
		{
			EnsureID (builder);
			bool isTemplate = (typeof (TemplateBuilder).IsAssignableFrom (builder.GetType ()));
			
			if (!isTemplate && !inTemplate) {
				CreateField (builder, true);
			} else if (!isTemplate) {
				bool doCheck = false;				
				bool singleInstance = false;
				ControlBuilder pb = builder.ParentBuilder;
				TemplateBuilder tpb;
				while (pb != null) {
					tpb = pb as TemplateBuilder;
					if (tpb == null) {
						pb = pb.ParentBuilder;
						continue;
					}
					
					if (tpb.TemplateInstance == TemplateInstance.Single)
						singleInstance = true;
					break;
				}
				
				if (!singleInstance)
					builder.ID = builder.GetNextID (null);
				else
					doCheck = true;

				CreateField (builder, doCheck);
			}

			InitMethod (builder, isTemplate, childrenAsProperties);
			if (!isTemplate || builder.GetType () == typeof (RootBuilder))
				CreateAssignStatementsFromAttributes (builder);

			if (builder.Children != null && builder.Children.Count > 0) {
				StringBuilder sb = new StringBuilder ();
				foreach (object b in builder.Children) {
					if (b is string) {
						sb.Append ((string) b);
						continue;
					}

					FlushText (builder, sb);
					if (b is ObjectTagBuilder) {
						ProcessObjectTag ((ObjectTagBuilder) b);
					} else if (b is StringPropertyBuilder) {
						StringPropertyBuilder pb = b as StringPropertyBuilder;
						if (pb.Children != null && pb.Children.Count > 0) {
							StringBuilder asb = new StringBuilder ();
							foreach (string s in pb.Children)
								asb.Append (s);
							CodeMemberMethod method = builder.Method;
							CodeAssignStatement assign = new CodeAssignStatement ();
							assign.Left = new CodePropertyReferenceExpression (ctrlVar, pb.PropertyName);
							assign.Right = new CodePrimitiveExpression (asb.ToString ());
							method.Statements.Add (AddLinePragma (assign, builder));
						}
					} else if (b is ContentBuilderInternal) {
						ContentBuilderInternal cb = (ContentBuilderInternal) b;
						CreateControlTree (cb, false, true);
						AddContentTemplateInvocation (cb, builder.Method, cb.Method.Name);
						continue;
					}

					// Ignore TemplateBuilders - they are processed in InitMethod
					else if (b is TemplateBuilder) {
					} else if (b is CodeRenderBuilder) {
						AddCodeRender (builder, (CodeRenderBuilder) b);
					} else if (b is DataBindingBuilder) {
						AddDataBindingLiteral (builder, (DataBindingBuilder) b);
					} else if (b is ControlBuilder) {
						ControlBuilder child = (ControlBuilder) b;
						CreateControlTree (child, inTemplate, builder.ChildrenAsProperties);
						AddChildCall (builder, child);
						continue;
					} else
						throw new Exception ("???");

					ControlBuilder bldr = b as ControlBuilder;
					bldr.ProcessGeneratedCode (CompileUnit, BaseType, DerivedType, bldr.Method, bldr.DataBindingMethod);
				}

				FlushText (builder, sb);
			}

			ControlBuilder defaultPropertyBuilder = builder.DefaultPropertyBuilder;
			if (defaultPropertyBuilder != null) {
				CreateControlTree (defaultPropertyBuilder, false, true);
				AddChildCall (builder, defaultPropertyBuilder);
			}
			
			if (builder.HasAspCode) {
				CodeMemberMethod renderMethod = builder.RenderMethod;
				CodeMethodReferenceExpression m = new CodeMethodReferenceExpression ();
				m.TargetObject = thisRef;
				m.MethodName = renderMethod.Name;

				CodeDelegateCreateExpression create = new CodeDelegateCreateExpression ();
				create.DelegateType = new CodeTypeReference (typeof (RenderMethod));
				create.TargetObject = thisRef;
				create.MethodName = renderMethod.Name;

				CodeMethodInvokeExpression invoke = new CodeMethodInvokeExpression ();
				invoke.Method = new CodeMethodReferenceExpression (ctrlVar, "SetRenderMethodDelegate");
				invoke.Parameters.Add (create);

				builder.MethodStatements.Add (invoke);
			}

			if (builder is RootBuilder)
				if (!String.IsNullOrEmpty (parser.MetaResourceKey))
					AssignPropertiesFromResources (builder, parser.BaseType, parser.MetaResourceKey);
			
			if ((!isTemplate || builder is RootBuilder) && !String.IsNullOrEmpty (builder.GetAttribute ("meta:resourcekey")))
				CreateAssignStatementFromAttribute (builder, "meta:resourcekey");

			if (!childrenAsProperties && typeof (Control).IsAssignableFrom (builder.ControlType))
				builder.Method.Statements.Add (new CodeMethodReturnStatement (ctrlVar));

			builder.ProcessGeneratedCode (CompileUnit, BaseType, DerivedType, builder.Method, builder.DataBindingMethod);
		}

		protected override void AddStatementsToConstructor (CodeConstructor ctor)
		{
			if (masterPageContentPlaceHolders == null || masterPageContentPlaceHolders.Count == 0)
				return;
			
			var ilist = new CodeVariableDeclarationStatement ();
			ilist.Name = "__contentPlaceHolders";
			ilist.Type = new CodeTypeReference (typeof (IList));
			ilist.InitExpression = new CodePropertyReferenceExpression (thisRef, "ContentPlaceHolders");
			
			var ilistRef = new CodeVariableReferenceExpression ("__contentPlaceHolders");
			CodeStatementCollection statements = ctor.Statements;
			statements.Add (ilist);

			CodeMethodInvokeExpression mcall;
			foreach (string id in masterPageContentPlaceHolders) {
				mcall = new CodeMethodInvokeExpression (ilistRef, "Add");
				mcall.Parameters.Add (new CodePrimitiveExpression (id.ToLowerInvariant ()));
				statements.Add (mcall);
			}
		}
		
		protected internal override void CreateMethods ()
		{
			base.CreateMethods ();

			CreateProperties ();
			CreateControlTree (parser.RootBuilder, false, false);
			CreateFrameworkInitializeMethod ();
		}

		protected override void InitializeType ()
		{
			List <string> registeredTagNames = parser.RegisteredTagNames;
			RootBuilder rb = parser.RootBuilder;
			if (rb == null || registeredTagNames == null || registeredTagNames.Count == 0)
				return;

			AspComponent component;
			foreach (string tagName in registeredTagNames) {
				component = rb.Foundry.GetComponent (tagName);
				if (component == null || component.Type == null) // unlikely
					throw new HttpException ("Custom control '" + tagName + "' cannot be found.");
				if (!(typeof (UserControl).IsAssignableFrom (component.Type)))
					throw new ParseException (parser.Location, "Type '" + component.Type.ToString () + "' does not derive from 'System.Web.UI.UserControl'.");
				AddReferencedAssembly (component.Type.Assembly);
			}
		}
		
		void CallBaseFrameworkInitialize (CodeMemberMethod method)
		{
			CodeBaseReferenceExpression baseRef = new CodeBaseReferenceExpression ();
			CodeMethodInvokeExpression invoke = new CodeMethodInvokeExpression (baseRef, "FrameworkInitialize");
			method.Statements.Add (invoke);
		}
		
		void CallSetStringResourcePointer (CodeMemberMethod method)
		{
			CodeFieldReferenceExpression stringResource = GetMainClassFieldReferenceExpression ("__stringResource");
			method.Statements.Add (
				new CodeMethodInvokeExpression (
					thisRef,
					"SetStringResourcePointer",
					new CodeExpression[] {stringResource, new CodePrimitiveExpression (0)})
			);
		}
		
		void CreateFrameworkInitializeMethod ()
		{
			CodeMemberMethod method = new CodeMemberMethod ();
			method.Name = "FrameworkInitialize";
			method.Attributes = MemberAttributes.Family | MemberAttributes.Override;
			PrependStatementsToFrameworkInitialize (method);
			CallBaseFrameworkInitialize (method);
			CallSetStringResourcePointer (method);
			AppendStatementsToFrameworkInitialize (method);
			mainClass.Members.Add (method);
		}

		protected virtual void PrependStatementsToFrameworkInitialize (CodeMemberMethod method)
		{
		}

		protected virtual void AppendStatementsToFrameworkInitialize (CodeMemberMethod method)
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

		protected override void AddApplicationAndSessionObjects ()
		{
			foreach (ObjectTagBuilder tag in GlobalAsaxCompiler.ApplicationObjects) {
				CreateFieldForObject (tag.Type, tag.ObjectID);
				CreateApplicationOrSessionPropertyForObject (tag.Type, tag.ObjectID, true, false);
			}

			foreach (ObjectTagBuilder tag in GlobalAsaxCompiler.SessionObjects) {
				CreateApplicationOrSessionPropertyForObject (tag.Type, tag.ObjectID, false, false);
			}
		}

		protected override void CreateStaticFields ()
		{
			base.CreateStaticFields ();

			CodeMemberField fld = new CodeMemberField (typeof (object), "__stringResource");
			fld.Attributes = MemberAttributes.Private | MemberAttributes.Static;
			fld.InitExpression = new CodePrimitiveExpression (null);
			mainClass.Members.Add (fld);
		}
		
		protected void ProcessObjectTag (ObjectTagBuilder tag)
		{
			string fieldName = CreateFieldForObject (tag.Type, tag.ObjectID);
			CreatePropertyForObject (tag.Type, tag.ObjectID, fieldName, false);
		}

		void CreateProperties ()
		{
			if (!parser.AutoEventWireup) {
				CreateAutoEventWireup ();
			} else {
				CreateAutoHandlers ();
			}

			CreateApplicationInstance ();
		}
		
		void CreateApplicationInstance ()
		{
			CodeMemberProperty prop = new CodeMemberProperty ();
			Type appType = typeof (HttpApplication);
			prop.Type = new CodeTypeReference (appType);
			prop.Name = "ApplicationInstance";
			prop.Attributes = MemberAttributes.Family | MemberAttributes.Final;

			CodePropertyReferenceExpression propRef = new CodePropertyReferenceExpression (thisRef, "Context");

			propRef = new CodePropertyReferenceExpression (propRef, "ApplicationInstance");

			CodeCastExpression cast = new CodeCastExpression (appType.FullName, propRef);
			prop.GetStatements.Add (new CodeMethodReturnStatement (cast));
			if (partialClass != null)
				partialClass.Members.Add (prop);
			else
				mainClass.Members.Add (prop);
		}

		void CreateContentPlaceHolderTemplateProperty (string backingField, string name)
		{
			CodeMemberProperty prop = new CodeMemberProperty ();
			prop.Type = new CodeTypeReference (typeof (ITemplate));
			prop.Name = name;
			prop.Attributes = MemberAttributes.Public;

			var ret = new CodeMethodReturnStatement ();
			var fldRef = new CodeFieldReferenceExpression (thisRef, backingField);
			ret.Expression = fldRef;
			prop.GetStatements.Add (ret);
			prop.SetStatements.Add (new CodeAssignStatement (fldRef, new CodePropertySetValueReferenceExpression ()));

			prop.CustomAttributes.Add (new CodeAttributeDeclaration ("TemplateContainer", new CodeAttributeArgument [] {
						new CodeAttributeArgument (new CodeTypeOfExpression (new CodeTypeReference (typeof (MasterPage))))
					}
				)
			);

			var enumValueRef = new CodeFieldReferenceExpression (new CodeTypeReferenceExpression (typeof (TemplateInstance)), "Single");
			prop.CustomAttributes.Add (new CodeAttributeDeclaration ("TemplateInstanceAttribute", new CodeAttributeArgument [] {
						new CodeAttributeArgument (enumValueRef)
					}
				)
			);

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

			CodeAttributeDeclaration attr = new CodeAttributeDeclaration ("System.Obsolete");
			prop.CustomAttributes.Add (attr);			
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
			prop.Type = new CodeTypeReference (typeof (bool));
			prop.Name = "SupportAutoEvents";
			prop.Attributes = MemberAttributes.Family | MemberAttributes.Override;
			prop.GetStatements.Add (new CodeMethodReturnStatement (new CodePrimitiveExpression (false)));
			mainClass.Members.Add (prop);
		}

		protected virtual string HandleUrlProperty (string str, MemberInfo member)
		{
			return str;
		}

		TypeConverter GetConverterForMember (MemberInfo member)
		{
			TypeDescriptionProvider prov = TypeDescriptor.GetProvider (member.ReflectedType);
			if (prov == null)
				return null;

			ICustomTypeDescriptor desc = prov.GetTypeDescriptor (member.ReflectedType);
			PropertyDescriptorCollection coll = desc != null ? desc.GetProperties () : null;

			if (coll == null || coll.Count == 0)
				return null;

			PropertyDescriptor pd = coll.Find (member.Name, false);
			if (pd == null)
				return null;

			return pd.Converter;
		}
		
		CodeExpression CreateNullableExpression (Type type, CodeExpression inst, bool nullable)
		{
			if (!nullable)
				return inst;
			
			return new CodeObjectCreateExpression (type, new CodeExpression[] {inst});
		}

		bool SafeCanConvertFrom (Type type, TypeConverter cvt)
		{
			try {
				return cvt.CanConvertFrom (type);
			} catch (NotImplementedException) {
				return false;
			}
		}

		bool SafeCanConvertTo (Type type, TypeConverter cvt)
		{
			try {
				return cvt.CanConvertTo (type);
			} catch (NotImplementedException) {
				return false;
			}
		}
		
		CodeExpression GetExpressionFromString (Type type, string str, MemberInfo member)
		{
			TypeConverter cvt = GetConverterForMember (member);
			if (cvt != null && !SafeCanConvertFrom (typeof (string), cvt))
				cvt = null;
			
			object convertedFromAttr = null;
			bool preConverted = false;
			if (cvt != null && str != null) {
				convertedFromAttr = cvt.ConvertFromInvariantString (str);
				if (convertedFromAttr != null) {
					type = convertedFromAttr.GetType ();
					preConverted = true;
				}
			}

			bool wasNullable = false;
			Type originalType = type;

			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)) {
				Type[] types = type.GetGenericArguments();
				originalType = type;
				type = types[0]; // we're interested only in the first type here
				wasNullable = true;
			}

			if (type == typeof (string)) {
				object[] urlAttr = member.GetCustomAttributes (typeof (UrlPropertyAttribute), true);
				if (urlAttr.Length != 0)
					str = HandleUrlProperty ((preConverted && convertedFromAttr is string) ? (string)convertedFromAttr : str, member);
				else if (preConverted)
					return CreateNullableExpression (originalType,
									 new CodePrimitiveExpression ((string) convertedFromAttr),
									 wasNullable);

				return CreateNullableExpression (originalType, new CodePrimitiveExpression (str), wasNullable);
			} else if (type == typeof (bool)) {
				if (preConverted)
					return CreateNullableExpression (originalType,
									 new CodePrimitiveExpression ((bool) convertedFromAttr),
									 wasNullable);
				
				if (str == null || str == "" || InvariantCompareNoCase (str, "true"))
					return CreateNullableExpression (originalType, new CodePrimitiveExpression (true), wasNullable);
				else if (InvariantCompareNoCase (str, "false"))
					return CreateNullableExpression (originalType, new CodePrimitiveExpression (false), wasNullable);
				else if (wasNullable && InvariantCompareNoCase(str, "null"))
					return new CodePrimitiveExpression (null);
				else
					throw new ParseException (currentLocation,
							"Value '" + str  + "' is not a valid boolean.");
			} else if (type == monoTypeType)
				type = typeof (System.Type);
			
			if (str == null)
				return new CodePrimitiveExpression (null);

			if (type.IsPrimitive)
				return CreateNullableExpression (originalType,
								 new CodePrimitiveExpression (
									 Convert.ChangeType (preConverted ? convertedFromAttr : str,
											     type, Helpers.InvariantCulture)),
								 wasNullable);

			if (type == typeof (string [])) {
				string [] subs;

				if (preConverted)
					subs = (string[])convertedFromAttr;
				else
					subs = str.Split (',');
				CodeArrayCreateExpression expr = new CodeArrayCreateExpression ();
				expr.CreateType = new CodeTypeReference (typeof (string));
				foreach (string v in subs)
					expr.Initializers.Add (new CodePrimitiveExpression (v.Trim ()));

				return CreateNullableExpression (originalType, expr, wasNullable);
			}
			
			if (type == typeof (Color)) {
				Color c;
				
				if (!preConverted) {
					if (colorConverter == null)
						colorConverter = TypeDescriptor.GetConverter (typeof (Color));
				
					if (str.Trim().Length == 0) {
						CodeTypeReferenceExpression ft = new CodeTypeReferenceExpression (typeof (Color));
						return CreateNullableExpression (originalType,
										 new CodeFieldReferenceExpression (ft, "Empty"),
										 wasNullable);
					}

					try {
						if (str.IndexOf (',') == -1) {
							c = (Color) colorConverter.ConvertFromString (str);
						} else {
							int [] argb = new int [4];
							argb [0] = 255;

							string [] parts = str.Split (',');
							int length = parts.Length;
							if (length < 3)
								throw new Exception ();

							int basei = (length == 4) ? 0 : 1;
							for (int i = length - 1; i >= 0; i--) {
								argb [basei + i] = (int) Byte.Parse (parts [i]);
							}
							c = Color.FromArgb (argb [0], argb [1], argb [2], argb [3]);
						}
					} catch (Exception e) {
						// Hack: "LightGrey" is accepted, but only for ASP.NET, as the
						// TypeConverter for Color fails to ConvertFromString.
						// Hence this hack...
						if (InvariantCompareNoCase ("LightGrey", str)) {
							c = Color.LightGray;
						} else {
							throw new ParseException (currentLocation,
										  "Color " + str + " is not a valid color.", e);
						}
					}
				} else
					c = (Color)convertedFromAttr;
				
				if (c.IsKnownColor) {
					CodeFieldReferenceExpression expr = new CodeFieldReferenceExpression ();
					if (c.IsSystemColor)
						type = typeof (SystemColors);

					expr.TargetObject = new CodeTypeReferenceExpression (type);
					expr.FieldName = c.Name;
					return CreateNullableExpression (originalType, expr, wasNullable);
				} else {
					CodeMethodReferenceExpression m = new CodeMethodReferenceExpression ();
					m.TargetObject = new CodeTypeReferenceExpression (type);
					m.MethodName = "FromArgb";
					CodeMethodInvokeExpression invoke = new CodeMethodInvokeExpression (m);
					invoke.Parameters.Add (new CodePrimitiveExpression (c.A));
					invoke.Parameters.Add (new CodePrimitiveExpression (c.R));
					invoke.Parameters.Add (new CodePrimitiveExpression (c.G));
					invoke.Parameters.Add (new CodePrimitiveExpression (c.B));
					return CreateNullableExpression (originalType, invoke, wasNullable);
				}
			}

			TypeConverter converter = preConverted ? cvt : wasNullable ? TypeDescriptor.GetConverter (type) : null;
			if (converter == null) {
				PropertyDescriptor pdesc = TypeDescriptor.GetProperties (member.DeclaringType) [member.Name];
				if (pdesc != null)
					converter = pdesc.Converter;
				else {
					Type memberType;
					switch (member.MemberType) {
						case MemberTypes.Field:
							memberType = ((FieldInfo)member).FieldType;
							break;

						case MemberTypes.Property:
							memberType = ((PropertyInfo)member).PropertyType;
							break;

						default:
							memberType = null;
							break;
					}

					if (memberType == null)
						return null;

					converter = TypeDescriptor.GetConverter (memberType);
				}
			}
			
			if (preConverted || (converter != null && SafeCanConvertFrom (typeof (string), converter))) {
				object value = preConverted ? convertedFromAttr : converter.ConvertFromInvariantString (str);

				if (SafeCanConvertTo (typeof (InstanceDescriptor), converter)) {
					InstanceDescriptor idesc = (InstanceDescriptor) converter.ConvertTo (value, typeof(InstanceDescriptor));
					if (wasNullable)
						return CreateNullableExpression (originalType, GenerateInstance (idesc, true),
										 wasNullable);

					CodeExpression instance = GenerateInstance (idesc, true);
					if (type.IsPublic)
						return new CodeCastExpression (type, instance);
					else
						return instance;
				}

				CodeExpression exp = GenerateObjectInstance (value, false);
				if (exp != null)
					return CreateNullableExpression (originalType, exp, wasNullable);
				
				CodeMethodReferenceExpression m = new CodeMethodReferenceExpression ();
				m.TargetObject = new CodeTypeReferenceExpression (typeof (TypeDescriptor));
				m.MethodName = "GetConverter";
				CodeMethodInvokeExpression invoke = new CodeMethodInvokeExpression (m);
				CodeTypeReference tref = new CodeTypeReference (type);
				invoke.Parameters.Add (new CodeTypeOfExpression (tref));
				
				invoke = new CodeMethodInvokeExpression (invoke, "ConvertFrom");
				invoke.Parameters.Add (new CodePrimitiveExpression (str));

				if (wasNullable)
					return CreateNullableExpression (originalType, invoke, wasNullable);

				return new CodeCastExpression (type, invoke);
			}

			Console.WriteLine ("Unknown type: " + type + " value: " + str);
			
			return CreateNullableExpression (originalType, new CodePrimitiveExpression (str), wasNullable);
		}
		
		CodeExpression GenerateInstance (InstanceDescriptor idesc, bool throwOnError)
		{
			CodeExpression[] parameters = new CodeExpression [idesc.Arguments.Count];
			int n = 0;
			foreach (object ob in idesc.Arguments) {
				CodeExpression exp = GenerateObjectInstance (ob, throwOnError);
				if (exp == null) return null;
				parameters [n++] = exp;
			}
			
			switch (idesc.MemberInfo.MemberType) {
			case MemberTypes.Constructor:
				CodeTypeReference tob = new CodeTypeReference (idesc.MemberInfo.DeclaringType);
				return new CodeObjectCreateExpression (tob, parameters);

			case MemberTypes.Method:
				CodeTypeReferenceExpression mt = new CodeTypeReferenceExpression (idesc.MemberInfo.DeclaringType);
				return new CodeMethodInvokeExpression (mt, idesc.MemberInfo.Name, parameters);

			case MemberTypes.Field:
				CodeTypeReferenceExpression ft = new CodeTypeReferenceExpression (idesc.MemberInfo.DeclaringType);
				return new CodeFieldReferenceExpression (ft, idesc.MemberInfo.Name);

			case MemberTypes.Property:
				CodeTypeReferenceExpression pt = new CodeTypeReferenceExpression (idesc.MemberInfo.DeclaringType);
				return new CodePropertyReferenceExpression (pt, idesc.MemberInfo.Name);
			}
			throw new ParseException (currentLocation, "Invalid instance type.");
		}
		
		CodeExpression GenerateObjectInstance (object value, bool throwOnError)
		{
			if (value == null)
				return new CodePrimitiveExpression (null);

			if (value is System.Type) {
				CodeTypeReference tref = new CodeTypeReference (value.ToString ());
				return new CodeTypeOfExpression (tref);
			}
			
			Type t = value.GetType ();

			if (t.IsPrimitive || value is string)
				return new CodePrimitiveExpression (value);
			
			if (t.IsArray) {
				Array ar = (Array) value;
				CodeExpression[] items = new CodeExpression [ar.Length];
				for (int n=0; n<ar.Length; n++) {
					CodeExpression exp = GenerateObjectInstance (ar.GetValue (n), throwOnError);
					if (exp == null) return null; 
					items [n] = exp;
				}
				return new CodeArrayCreateExpression (new CodeTypeReference (t), items);
			}
			
			TypeConverter converter = TypeDescriptor.GetConverter (t);
			if (converter != null && converter.CanConvertTo (typeof (InstanceDescriptor))) {
				InstanceDescriptor idesc = (InstanceDescriptor) converter.ConvertTo (value, typeof(InstanceDescriptor));
				return GenerateInstance (idesc, throwOnError);
			}
			
			InstanceDescriptor desc = GetDefaultInstanceDescriptor (value);
			if (desc != null) return GenerateInstance (desc, throwOnError);
			
			if (throwOnError)
				throw new ParseException (currentLocation, "Cannot generate an instance for the type: " + t);
			else
				return null;
		}
		
		InstanceDescriptor GetDefaultInstanceDescriptor (object value)
		{
			if (value is System.Web.UI.WebControls.Unit) {
				System.Web.UI.WebControls.Unit s = (System.Web.UI.WebControls.Unit) value;
				if (s.IsEmpty) {
					FieldInfo f = typeof (Unit).GetField ("Empty");
					return new InstanceDescriptor (f, null);
				}
				ConstructorInfo c = typeof(System.Web.UI.WebControls.Unit).GetConstructor (
					BindingFlags.Instance | BindingFlags.Public,
					null,
					new Type[] {typeof(double), typeof(System.Web.UI.WebControls.UnitType)},
					null);
				
				return new InstanceDescriptor (c, new object[] {s.Value, s.Type});
			}
			
			if (value is System.Web.UI.WebControls.FontUnit) {
				System.Web.UI.WebControls.FontUnit s = (System.Web.UI.WebControls.FontUnit) value;
				if (s.IsEmpty) {
					FieldInfo f = typeof (FontUnit).GetField ("Empty");
					return new InstanceDescriptor (f, null);
				}

				Type cParamType = null;
				object cParam = null;

				switch (s.Type) {
					case FontSize.AsUnit:
					case FontSize.NotSet:
						cParamType = typeof (System.Web.UI.WebControls.Unit);
						cParam = s.Unit;
						break;

					default:
						cParamType = typeof (string);
						cParam = s.Type.ToString ();
						break;
				}
				
				ConstructorInfo c = typeof(System.Web.UI.WebControls.FontUnit).GetConstructor (
					BindingFlags.Instance | BindingFlags.Public,
					null,
					new Type[] {cParamType},
					null);
				if (c != null)
					return new InstanceDescriptor (c, new object[] {cParam});
			}
			return null;
		}

#if DEBUG
		CodeMethodInvokeExpression CreateConsoleWriteLineCall (string format, params CodeExpression[] parms)
		{
			CodeMethodReferenceExpression cwl = new CodeMethodReferenceExpression (new CodeTypeReferenceExpression (typeof (System.Console)), "WriteLine");
			CodeMethodInvokeExpression cwlCall = new CodeMethodInvokeExpression (cwl);

			cwlCall.Parameters.Add (new CodePrimitiveExpression (format));
			if (parms != null && parms.Length > 0)
				foreach (CodeExpression expr in parms)
					cwlCall.Parameters.Add (expr);

			return cwlCall;
		}
#endif
	}
}



