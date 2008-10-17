//
// System.Web.Compilation.AppSettingsExpressionBuilder
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
using System.CodeDom;
using System.ComponentModel;
using System.Configuration;
using System.Web.Configuration;
using System.Web.UI;
using System.Reflection;

namespace System.Web.Compilation {

	[ExpressionEditor("System.Web.UI.Design.AppSettingsExpressionEditor, " + Consts.AssemblySystem_Design)]
	[ExpressionPrefix("AppSettings")]
	public class AppSettingsExpressionBuilder : ExpressionBuilder {

		public override object EvaluateExpression (object target, BoundPropertyEntry entry, object parsedData, ExpressionBuilderContext context)
		{
			return GetAppSetting (entry.Expression.Trim ());
		}

		public static object GetAppSetting (string key)
		{
			string value = WebConfigurationManager.AppSettings [key];

			if (value == null)
				throw new InvalidOperationException (String.Format ("The application setting '{0}' was not found.", key));
			return value;
		}

		public static object GetAppSetting (string key, Type targetType, string propertyName)
		{
			object value = GetAppSetting (key);

			if (targetType == null)
				return value.ToString ();

			PropertyInfo pi = targetType.GetProperty(propertyName);
			if (pi == null)
				return value.ToString ();

			try {
				TypeConverter converter = TypeDescriptor.GetConverter (pi.PropertyType);
				return converter.ConvertFrom (value);
			} catch (NotSupportedException) {
				throw new InvalidOperationException (String.Format (
					"Could not convert application setting '{0}' " +
					" to type '{1}' for property '{2}'.", value,
					pi.PropertyType.Name, pi.Name));
			}
		}


		public override CodeExpression GetCodeExpression (BoundPropertyEntry entry, object parsedData, ExpressionBuilderContext context)
		{
			Type type = entry.DeclaringType;
			PropertyDescriptor descriptor = TypeDescriptor.GetProperties(type)[entry.PropertyInfo.Name];
			CodeExpression[] expressionArray = new CodeExpression[3];
			expressionArray[0] = new CodePrimitiveExpression(entry.Expression.Trim());
			expressionArray[1] = new CodeTypeOfExpression(entry.Type);
			expressionArray[2] = new CodePrimitiveExpression(entry.Name);
			return new CodeCastExpression(descriptor.PropertyType, new CodeMethodInvokeExpression(new 
								       CodeTypeReferenceExpression(base.GetType()), "GetAppSetting", expressionArray));
		}

		public override bool SupportsEvaluate {
			get { return true; }
		}
	}

}

#endif
