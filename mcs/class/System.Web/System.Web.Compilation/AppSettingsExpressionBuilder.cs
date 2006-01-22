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

namespace System.Web.Compilation {

	[ExpressionEditor("System.Web.UI.Design.AppSettingsExpressionEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
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
				throw new InvalidOperationException (String.Format ("App setting {0} not found", key));
			return value;
		}

		public static object GetAppSetting (string key, Type targetType, string propertyName)
		{
			try {
				TypeConverter converter = TypeDescriptor.GetConverter (targetType);
				return converter.ConvertFrom (GetAppSetting (key));
			}
			catch (NotSupportedException e) {
				throw new InvalidOperationException (String.Format ("Could not convert app setting {0} to type {1}", key, targetType));
			}
		}


		public override CodeExpression GetCodeExpression (BoundPropertyEntry entry, object parsedData, ExpressionBuilderContext context)
		{
			Type type = entry.DeclaringType;
			PropertyDescriptor descriptor = TypeDescriptor.GetProperties(type)[entry.PropertyInfo.Name];
			CodeExpression[] expressionArray = new CodeExpression[3];
			expressionArray[0] = new CodePrimitiveExpression(entry.Expression.Trim());
			expressionArray[1] = new CodeTypeOfExpression(type);
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


