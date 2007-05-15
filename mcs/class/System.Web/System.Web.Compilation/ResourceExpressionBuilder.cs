//
// System.Web.Compilation.ResourceExpressionBuilder
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
using System.Reflection;
using System.Web;
using System.Web.UI;

namespace System.Web.Compilation {

	[ExpressionEditor("System.Web.UI.Design.ResourceExpressionEditor, " + Consts.AssemblySystem_Design)]
	[ExpressionPrefix("Resources")]
	public class ResourceExpressionBuilder : ExpressionBuilder {

		public ResourceExpressionBuilder ()
		{
		}

		public override object EvaluateExpression (object target, BoundPropertyEntry entry, object parsedData, ExpressionBuilderContext context)
		{
			ResourceExpressionFields fields = parsedData as ResourceExpressionFields;
			return HttpContext.GetGlobalResourceObject (fields.ClassKey, fields.ResourceKey);
		}

		public override CodeExpression GetCodeExpression (BoundPropertyEntry entry, object parsedData, ExpressionBuilderContext context)
		{
			ResourceExpressionFields fields = parsedData as ResourceExpressionFields;
			CodeExpression[] expr;
			
			if (!String.IsNullOrEmpty (fields.ClassKey)) {
				expr = new CodeExpression [] {
					new CodePrimitiveExpression (fields.ClassKey),
					new CodePrimitiveExpression (fields.ResourceKey)
				};
				return new CodeMethodInvokeExpression (new CodeThisReferenceExpression (), "GetGlobalResourceObject", expr);
			} else
				return CreateGetLocalResourceObject (entry.PropertyInfo, fields.ResourceKey);
		}

		public static ResourceExpressionFields ParseExpression (string expression)
		{
			int comma = expression.IndexOf (',');
			if (comma == -1)
				return new ResourceExpressionFields (expression.Trim ());
			else
				return new ResourceExpressionFields (expression.Substring (0, comma).Trim (),
								     expression.Substring (comma + 1).Trim ());
		}

		public override object ParseExpression (string expression, Type propertyType, ExpressionBuilderContext context)
		{
			//FIXME: not sure what the propertyType should be used for
			return ParseExpression (expression);
		}

		public override bool SupportsEvaluate {
			get { return true; }
		}

		internal static CodeExpression CreateGetLocalResourceObject (MemberInfo mi, string resname)
		{
			Type member_type = null;
			if (mi is PropertyInfo)
				member_type = ((PropertyInfo)mi).PropertyType;
			else if (mi is FieldInfo)
				member_type = ((FieldInfo)mi).FieldType;
			else
				return null; // an "impossible" case

			string memberName = mi.Name;
			Type declaringType = mi.DeclaringType;
			TypeConverter converter = TypeDescriptor.GetProperties (declaringType) [memberName].Converter;

			if (member_type != typeof (System.Drawing.Color) &&
			    (converter == null || converter.CanConvertFrom (typeof (String)))) {
				CodeMethodInvokeExpression getlro = new CodeMethodInvokeExpression (
					new CodeThisReferenceExpression (),
					"GetLocalResourceObject",
					new CodeExpression [] { new CodePrimitiveExpression (resname) });
			
				CodeMethodInvokeExpression convert = new CodeMethodInvokeExpression ();
				convert.Method = new CodeMethodReferenceExpression (
					new CodeTypeReferenceExpression (typeof (System.Convert)),
					"ToString");
				convert.Parameters.Add (getlro);
				return convert;
			} else {
				CodeMethodInvokeExpression getlro = new CodeMethodInvokeExpression (
					new CodeThisReferenceExpression (),
					"GetLocalResourceObject",
					new CodeExpression [] {
						new CodePrimitiveExpression (resname),
						new CodeTypeOfExpression (new CodeTypeReference (declaringType)),
						new CodePrimitiveExpression (memberName)
					}
				);

				return new CodeCastExpression (member_type, getlro);
			}
		}
	}

}

#endif


