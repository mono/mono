//
// Operations.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2013 Xamarin, Inc (http://www.xamarin.com)
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

namespace PlayScript.Runtime
{
	public static class Operations
	{
		public static bool Comparison (BinaryOperator binaryOperator, object left, object right)
		{
			if (right is int)
				return Comparison (binaryOperator, left, (int) right);

			return false;
		}

		public static bool Comparison (BinaryOperator binaryOperator, object left, int right)
		{
			if (left is int) {
				var l = (int) left;
				switch (binaryOperator) {
				case BinaryOperator.Equality:
					return l == right;
				case BinaryOperator.Inequality:
					return l != right;
				case BinaryOperator.GreaterThan:
					return l > right;
				case BinaryOperator.GreaterThanOrEqual:
					return l >= right;
				case BinaryOperator.LessThan:
					return l < right;
				case BinaryOperator.LessThanOrEqual:
					return l <= right;
				default:
					throw new NotImplementedException (binaryOperator.ToString ());
				}
			}

			// TODO: uint, string, double, etc

			return false;
		}

		public static bool Comparison (BinaryOperator binaryOperator, int left, object right)
		{
			if (right is int) {
				var r = (int) right;
				switch (binaryOperator) {
				case BinaryOperator.Equality:
					return left == r;
				case BinaryOperator.Inequality:
					return left != r;
				case BinaryOperator.GreaterThan:
					return left > r;
				case BinaryOperator.GreaterThanOrEqual:
					return left >= r;
				case BinaryOperator.LessThan:
					return left < r;
				case BinaryOperator.LessThanOrEqual:
					return left <= r;
				default:
					throw new NotImplementedException (binaryOperator.ToString ());
				}
			}

			// TODO: uint, string, double, etc

			return false;
		}	

		public static string Typeof (object instance)
		{
			if (instance == null)
				return "object";

			if (instance is string)
				return "string";

			if (instance is int || instance is uint || instance is double)
				return "number";

			if (instance is bool)
				return "boolean";

			// TODO: Wrong, it has to be of a special type
			if (instance == Undefined.Value) 
				return "undefined";

			// TODO: Wrong, it has to be of a special type			
			if (instance is _root.XML || instance is _root.XMLList)
				return "xml";
			
			if (instance is Delegate)
				return "function";
			
			return "object";
		}
	}				
}
