//
// RouteValueExpressionBuilder.cs
//
// Authors:
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2010 Novell, Inc (http://novell.com)
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

using System;
using System.CodeDom;
using System.ComponentModel;
using System.Reflection;
using System.Web.UI;
using System.Web.Routing;

namespace System.Web.Compilation
{
	[ExpressionEditor ("System.Web.UI.Design.RouteValueExpressionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	[ExpressionPrefix ("Routes")]
	public class RouteValueExpressionBuilder : ExpressionBuilder
	{
		public override bool SupportsEvaluate { get { return true; } }
		
		public RouteValueExpressionBuilder ()
		{
		}

		// This method is used only from within pages that aren't compiled
		public override object EvaluateExpression (object target, BoundPropertyEntry entry, object parsedData, ExpressionBuilderContext context)
		{
			// Mono doesn't use this, so let's leave it like that for now
			throw new NotImplementedException ();
		}

		public override CodeExpression GetCodeExpression (BoundPropertyEntry entry, object parsedData, ExpressionBuilderContext context)
		{
			if (entry == null)
				throw new NullReferenceException (".NET emulation (entry == null)");
			
			var ret = new CodeMethodInvokeExpression ();
			ret.Method = new CodeMethodReferenceExpression (new CodeTypeReferenceExpression (typeof (RouteValueExpressionBuilder)), "GetRouteValue");

			var thisref = new CodeThisReferenceExpression ();
			CodeExpressionCollection parameters = ret.Parameters;
			parameters.Add (new CodePropertyReferenceExpression (thisref, "Page"));
			parameters.Add (new CodePrimitiveExpression (entry.Expression));
			parameters.Add (new CodeTypeOfExpression (new CodeTypeReference (entry.DeclaringType)));
			parameters.Add (new CodePrimitiveExpression (entry.Name));
			
			return ret;
		}

		public static object GetRouteValue (Page page, string key, Type controlType, string propertyName)
		{
			RouteData rd = page != null ? page.RouteData : null;
			if (rd == null || String.IsNullOrEmpty (key))
				return null;
			
			object value = rd.Values [key];
			if (value == null)
				return null;

			if (controlType == null || String.IsNullOrEmpty (propertyName) || !(value is string))
				return value;

			PropertyDescriptorCollection pcoll = TypeDescriptor.GetProperties (controlType);
			if (pcoll == null || pcoll.Count == 0)
				return value;

			PropertyDescriptor pdesc = pcoll [propertyName];
			if (pdesc == null)
				return value;

			TypeConverter cvt = pdesc.Converter;
			if (cvt == null || !cvt.CanConvertFrom (typeof (string)))
				return value;

			return cvt.ConvertFrom (value);
		}
	}
}
