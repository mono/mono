//
// System.Web.Compilation.PageThemeCompiler
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2006 Novell, Inc (http://www.novell.com/)
//

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

#if NET_2_0

using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using System.Text;
using System.Web.UI;
using System.Web.SessionState;
using System.Web.Util;

namespace System.Web.Compilation
{
	class PageThemeCompiler : TemplateControlCompiler
	{
		PageThemeParser parser;

		public PageThemeCompiler (PageThemeParser parser)
			: base (parser)
		{
			this.parser = parser;
		}

		protected internal override void CreateMethods ()
		{
			CodeMemberField fld;
			CodeMemberProperty prop;

			/* override the following abstract PageTheme properties:
			   protected abstract string AppRelativeTemplateSourceDirectory { get; }
			   protected abstract IDictionary ControlSkins { get; }
			   protected abstract string[] LinkedStyleSheets { get; }
			*/

			/* ControlSkins */
			fld = new CodeMemberField (typeof (HybridDictionary), "__controlSkins");
			fld.Attributes = MemberAttributes.Private;
			fld.InitExpression = new CodeObjectCreateExpression (typeof (HybridDictionary));
			mainClass.Members.Add (fld);

			prop = new CodeMemberProperty ();
			prop.Name = "ControlSkins";
			prop.Attributes = MemberAttributes.Family | MemberAttributes.Override;
			prop.Type = new CodeTypeReference (typeof (IDictionary));
			prop.GetStatements.Add (new CodeMethodReturnStatement (new CodeVariableReferenceExpression ("__controlSkins")));
			mainClass.Members.Add (prop);

			/* LinkedStyleSheets */
			fld = new CodeMemberField (typeof (string[]), "__linkedStyleSheets");
			fld.Attributes = MemberAttributes.Private;
			fld.InitExpression = CreateLinkedStyleSheets ();
			mainClass.Members.Add (fld);

			prop = new CodeMemberProperty ();
			prop.Name = "LinkedStyleSheets";
			prop.Attributes = MemberAttributes.Family | MemberAttributes.Override;
			prop.Type = new CodeTypeReference (typeof (string[]));
			prop.GetStatements.Add (new CodeMethodReturnStatement (new CodeVariableReferenceExpression ("__linkedStyleSheets")));
			mainClass.Members.Add (prop);

			/* AppRelativeTemplateSourceDirectory */
			prop = new CodeMemberProperty ();
			prop.Name = "AppRelativeTemplateSourceDirectory";
			prop.Attributes = MemberAttributes.Family | MemberAttributes.Override;
			prop.Type = new CodeTypeReference (typeof (string));
			prop.GetStatements.Add (new CodeMethodReturnStatement (
							new CodePrimitiveExpression (
								VirtualPathUtility.ToAbsolute (parser.BaseVirtualDir))));
			mainClass.Members.Add (prop);

			ControlBuilder builder = parser.RootBuilder;
			if (builder.Children != null) {
				foreach (object o in builder.Children) {
					if (! (o is ControlBuilder))
						continue;
					if (o is CodeRenderBuilder)
						continue;
					
					ControlBuilder b = (ControlBuilder) o;
					CreateControlSkinMethod (b);
				}
			}
		}

		CodeExpression CreateLinkedStyleSheets ()
		{
			string [] lss = parser.LinkedStyleSheets;
			if (lss == null)
				return new CodePrimitiveExpression (null);
			
			CodeExpression [] initializers = new CodeExpression [lss.Length];
			for (int i = 0; i < lss.Length; i++)
				initializers[i] = new CodePrimitiveExpression (lss[i]);

			return new CodeArrayCreateExpression (typeof (string), initializers);
		}
		
		protected override string HandleUrlProperty (string str, MemberInfo member)
		{
			if (str.StartsWith ("~", StringComparison.Ordinal))
				return str;
			
			return "~/App_Themes/" + UrlUtils.Combine (
				System.IO.Path.GetFileName (parser.InputFile), str);
		}

		void CreateControlSkinMethod (ControlBuilder builder)
		{
			if (builder.ControlType == null)
				return;
			
			EnsureID (builder);

			CodeMemberMethod method = new CodeMemberMethod ();
			method.Name = "__BuildControl_" + builder.ID;
			method.Parameters.Add (new CodeParameterDeclarationExpression (typeof (Control), "ctrl"));

			mainClass.Members.Add (method);

			builder.Method = method;
			builder.MethodStatements = method.Statements;

			method.ReturnType = new CodeTypeReference (typeof (Control));

			// _ctrl = ($controlType)(ctrl);
			//
			CodeCastExpression castExpr = new CodeCastExpression (builder.ControlType, new CodeVariableReferenceExpression ("ctrl"));
			
			method.Statements.Add (new CodeVariableDeclarationStatement (builder.ControlType, "__ctrl"));
			CodeAssignStatement assign = new CodeAssignStatement ();
			assign.Left = ctrlVar;
			assign.Right = castExpr;
			method.Statements.Add (assign);

			CreateAssignStatementsFromAttributes (builder);

			if (builder.Children != null) {
				foreach (object o in builder.Children) {
					if (! (o is ControlBuilder))
						continue;

					ControlBuilder b = (ControlBuilder) o;
					if (b.ControlType == null)
						continue;
					
					if (b is CollectionBuilder) {
						PropertyInfo itemsProp = null;
						
						try {
							itemsProp = b.GetType().GetProperty ("Items");
						} catch (Exception) {}
						
						if (itemsProp != null) {
							/* emit a prop.Clear call before populating the collection */;
							CodePropertyReferenceExpression prop = new CodePropertyReferenceExpression (ctrlVar,
																																													b.TagName);
							CodePropertyReferenceExpression items = new CodePropertyReferenceExpression (prop,
																																													 "Items");
							method.Statements.Add (new CodeMethodInvokeExpression (items, "Clear"));
						}
					}

					CreateControlTree (b, false, builder.ChildrenAsProperties);
					AddChildCall (builder, b);
				}
			}

			builder.Method.Statements.Add (new CodeMethodReturnStatement (ctrlVar));
		}

		protected override void AddClassAttributes ()
		{
			base.AddClassAttributes ();
		}

		protected override void CreateStaticFields ()
		{
			base.CreateStaticFields ();
			ControlBuilder builder = parser.RootBuilder;

			if (builder.Children != null) {
				foreach (object o in builder.Children) {
					if (o is string) /* literal stuff gets ignored */
						continue;
					if (o is CodeRenderBuilder)
						continue;
					ControlBuilder b = (ControlBuilder) o;

					EnsureID (b);
					Type controlType = b.ControlType;
					if (controlType == null)
						continue;
					
					string id = b.ID;
					string skinId = b.Attributes != null ? b.Attributes["skinid"] as string : null;
					if (skinId == null)
						skinId = "";

					// private static object __BuildControl__$id_skinKey = System.Web.UI.PageTheme.CreateSkinKey(typeof($controlType), "$skinID")
					//
					CodeMemberField fld = new CodeMemberField (typeof (object), "__BuildControl_" + id + "_skinKey");
					fld.Attributes = MemberAttributes.Private | MemberAttributes.Static;
					fld.InitExpression = new CodeMethodInvokeExpression (
						new CodeTypeReferenceExpression (typeof (PageTheme)),
						"CreateSkinKey",
						new CodeTypeOfExpression (controlType),
						new CodePrimitiveExpression (skinId));

					mainClass.Members.Add (fld);
				}
			}
		}

		protected override void CreateConstructor (CodeStatementCollection localVars,
							   CodeStatementCollection trueStmt)
		{
			ControlBuilder builder = parser.RootBuilder;

			if (builder.Children != null) {
				foreach (object o in builder.Children) {
					if (o is string) /* literal stuff gets ignored */
						continue;
					if (o is CodeRenderBuilder)
						continue;
					
					ControlBuilder b = (ControlBuilder) o;
					Type controlType = b.ControlType;
					if (controlType == null)
						continue;

					string id = b.ID;
					
					if (localVars == null)
						localVars = new CodeStatementCollection ();

					// this.__controlSkins[__BuildControl_$id_skinKey] = new System.Web.UI.ControlSkin(typeof ($controlType), this.__BuildControl__$id)
					//
					localVars.Add (new CodeAssignStatement (new CodeIndexerExpression (new CodePropertyReferenceExpression (thisRef, "__controlSkins"),
													   new CodeVariableReferenceExpression ("__BuildControl_" + id + "_skinKey")),
										new CodeObjectCreateExpression (typeof (ControlSkin),
														new CodeTypeOfExpression (controlType),
														new CodeDelegateCreateExpression (new CodeTypeReference (typeof (ControlSkinDelegate)),
																		  thisRef, "__BuildControl_" + id))));
				}

				base.CreateConstructor (localVars, trueStmt);
			}
		}
	}
}

#endif
