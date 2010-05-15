//
// RouteUrlExpressionBuilder.cs
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
using System.Web.Routing;
using System.Web.UI;

namespace System.Web.Compilation
{
	public class RouteUrlExpressionBuilder : ExpressionBuilder
	{
		static readonly char[] expressionSplitChars = { ',' };
		static readonly char[] keyValueSplitChars = { '=' };
		
		public override bool SupportsEvaluate { get { return true; } }
		
		public RouteUrlExpressionBuilder ()
		{
		}

		// This method is used only from within pages that aren't compiled
		public override object EvaluateExpression (object target, BoundPropertyEntry entry, object parsedData, ExpressionBuilderContext context)
		{
			if (entry == null)
				throw new NullReferenceException (".NET emulation (entry == null)");

			if (context == null)
				throw new NullReferenceException (".NET emulation (context == null)");
			
			return GetRouteUrl (context.TemplateControl, entry.Expression);
		}

		public override CodeExpression GetCodeExpression (BoundPropertyEntry entry, object parsedData, ExpressionBuilderContext context)
		{
			if (entry == null)
				throw new NullReferenceException (".NET emulation (entry == null)");
			
			var ret = new CodeMethodInvokeExpression ();
			ret.Method = new CodeMethodReferenceExpression (new CodeTypeReferenceExpression (typeof (RouteUrlExpressionBuilder)), "GetRouteUrl");

			CodeExpressionCollection parameters = ret.Parameters;
			parameters.Add (new CodeThisReferenceExpression ());
			parameters.Add (new CodePrimitiveExpression (entry.Expression));

			return ret;
		}

		public static string GetRouteUrl (Control control, string expression)
		{
			if (control == null)
				throw new ArgumentNullException ("control");
			
			string routeName;
			var rvd = new RouteValueDictionary ();
			
			if (!TryParseRouteExpression (expression, rvd, out routeName))
				throw new InvalidOperationException ("Invalid expression, RouteUrlExpressionBuilder expects a string with format: RouteName=route,Key1=Value1,Key2=Value2");

			return control.GetRouteUrl (routeName, rvd);
		}

		public static bool TryParseRouteExpression (string expression, RouteValueDictionary routeValues, out string routeName)
		{
			routeName = null;
			if (String.IsNullOrEmpty (expression))
				return false;

			if (routeValues == null)
				throw new NullReferenceException (".NET emulation (routeValues == null)");

			string[] parts = expression.Split (expressionSplitChars);
			foreach (string part in parts) {
				string[] keyval = part.Split (keyValueSplitChars);
				if (keyval.Length != 2)
					return false;

				string key = keyval [0].Trim ();
				if (key == String.Empty)
					return false;

				if (String.Compare (key, "routename", StringComparison.OrdinalIgnoreCase) == 0) {
					routeName = keyval [1].Trim ();
					continue;
				}
				
				if (routeValues.ContainsKey (key))
					routeValues [key] = keyval [1].Trim ();
				else
					routeValues.Add (key, keyval [1].Trim ());
			}
			
			return true;
		}
	}
}

