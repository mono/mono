//
// BinaryOperator.cs
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

namespace PlayScript.Runtime
{
	public enum BinaryOperator
	{
		Multiply	= 1,
		Division	= 2,
		Modulus		= 3,
		Addition	= 4,
		Subtraction = 5,

		LeftShift	= 10,
		RightShift	= 11,
		UnsignedRightShift	= 12,

		LessThan	= 20,
		GreaterThan	= 21,
		LessThanOrEqual		= 22,
		GreaterThanOrEqual	= 23,
		Equality	= 24,
		Inequality	= 25,

		BitwiseAnd	= 30,
		ExclusiveOr	= 31,
		BitwiseOr	= 32,

		LogicalAnd	= 40,
		LogicalOr	= 41,
		
		StrictEquality		= 50 | StrictMask,
		StrictInequality	= 51 | StrictMask,

		StrictMask = 1 >> 16
	}
}