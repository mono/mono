//
// System.Web.UI.Design.ExpressionEditor
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2006 Novell, Inc (http://www.novell.com)
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
using System.Collections;
using System.Reflection;
using System.Web.Compilation;
using System.Web.Configuration;
using _Configuration = System.Configuration.Configuration;

namespace System.Web.UI.Design {

	public abstract class ExpressionEditor
	{
		Type expressionBuilderType;
		string prefixFromReflection;

		protected ExpressionEditor ()
		{
		}

		public string ExpressionPrefix {
			get { return prefixFromReflection; }
		}

		Type ExpressionBuilderType {
			set {
				expressionBuilderType = value;

				prefixFromReflection = "";
				object[] attrs = expressionBuilderType.GetCustomAttributes (typeof (ExpressionPrefixAttribute), false);
				if (attrs != null && attrs.Length > 0) {
					ExpressionPrefixAttribute pa = (ExpressionPrefixAttribute)attrs[0];

					prefixFromReflection = pa.ExpressionPrefix;
				}
			}
		}

		public abstract object EvaluateExpression (string expression, object parseTimeData, Type propertyType, IServiceProvider serviceProvider);

		public static ExpressionEditor GetExpressionEditor (string expressionPrefix, IServiceProvider serviceProvider)
		{
			if (serviceProvider == null)
				return null;

			IWebApplication webApp = (IWebApplication)serviceProvider.GetService(typeof(IWebApplication));
			if (webApp == null)
				return null;

			_Configuration config = webApp.OpenWebConfiguration(true);
			if (config == null)
				return null;

			CompilationSection sec = (CompilationSection) config.GetSection ("system.web/compilation");
			System.Web.Configuration.ExpressionBuilder builder = sec.ExpressionBuilders [expressionPrefix];

			if (builder == null)
				return null;

			return GetExpressionEditor (Type.GetType (builder.Type), serviceProvider);
		}

		[MonoTODO ("the docs make it sound like this still requires accessing <expressionBuilders>")]
		public static ExpressionEditor GetExpressionEditor (Type expressionBuilderType, IServiceProvider serviceProvider)
		{
			object[] attrs = expressionBuilderType.GetCustomAttributes (typeof (ExpressionEditorAttribute), false);

			if (attrs == null || attrs.Length == 0)
				return null;

			ExpressionEditorAttribute ee = (ExpressionEditorAttribute) attrs[0];

			Type editor_type = Type.GetType (ee.EditorTypeName);
			ExpressionEditor editor = (ExpressionEditor) Activator.CreateInstance (editor_type);

			editor.ExpressionBuilderType = expressionBuilderType;

			return editor;
		}

		public virtual ExpressionEditorSheet GetExpressionEditorSheet (string expression, IServiceProvider serviceProvider)
		{
			throw new NotImplementedException ();
		}
	}

}

#endif
