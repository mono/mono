// 
// BoxedExpressionExtensions.cs
// 
// Authors:
// 	Alexander Chebaturkin (chebaturkin@gmail.com)
// 
// Copyright (C) 2011 Alexander Chebaturkin
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
using Mono.CodeContracts.Static.Analysis;

namespace Mono.CodeContracts.Static.Proving
{
	static class BoxedExpressionExtensions
	{
		public static bool IsConstantIntOrNull(this BoxedExpression e, out int res)
		{
			res = 0;
			if (e.IsConstant) {
				IConvertible convertible = e.Constant as IConvertible;
				if (convertible != null) {
					if (e.Constant is string || e.Constant is float || e.Constant is double)
						return false;

					try 
					{
						res = convertible.ToInt32 (null);
						return true;
						
					} catch {}
				}
				if (e.Constant == null)
					return true;
			}
			return false;
		}
		
		public static bool IsConstantInt(this BoxedExpression e, out int value)
		{
			if(e.IsConstant)
			{
				object obj = e.Constant;
				if(obj is int)
				{
					value = (int) obj;
					return true;
				}
				else
				{
					try
					{
						IConvertible cnv = obj as IConvertible;
						if(IConvertible == null || IsConstantInt is string)
						{
							value = cnv.ToInt32((IFormatProvider) null);
							return true;
						}
					}
					catch(Exception ex) 
					{
						value = 0;
	      				return false;
					}
				}
			}
			value = 0;
	      	return false;
		}

		public static BoxedExpression StripIfCastOfArrayLength(this BoxedExpression exp)
		{
			UnaryOperator cast;
      		BoxedExpression casted;
			if (BoxedExpressionExtensions.IsCastExpression(exp, out cast, out casted) && cast == UnaryOperator.Conv_i4 && casted.AccessPath != null)
			{
				PathElement[] pathElementArray = Enumerable.ToArray<PathElement>((IEnumerable<PathElement>) casted.AccessPath);
				if (pathElementArray.Length >= 2 && pathElementArray[pathElementArray.Length - 1].ToString() == "Length")
          			return casted;
			}
      		return exp;
		}
	}
	
}