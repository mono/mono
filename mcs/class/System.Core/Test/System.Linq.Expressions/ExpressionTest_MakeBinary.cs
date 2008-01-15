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
//
// Authors:
//   Miguel de Icaza <miguel@novell.com>
//
using System;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;

namespace MonoTests.System.Linq.Expressions
{
	[TestFixture]
	public class ExpressionTest_MakeBinary {

		static void PassInt (ExpressionType nt)
		{
			Expression left = Expression.Constant (1);
			Expression right = Expression.Constant (1);

			Expression.MakeBinary (nt, left, right);
		}

		static void FailInt (ExpressionType nt)
		{
			Expression left = Expression.Constant (1);
			Expression right = Expression.Constant (1);

			try {
				Expression.MakeBinary (nt, left, right);
			} catch (ArgumentException){
				return;
			}
			// If we get here, there was an error
			Assert.Fail ("FailInt failed while creating an {0}", nt);
		}
		       
		//
		// Checks that we complain on the proper ExpressionTypes
		//
		[Test]
		public void TestBinaryCtor ()
		{
			PassInt (ExpressionType.Add);
			PassInt (ExpressionType.AddChecked);
			PassInt (ExpressionType.And);
			PassInt (ExpressionType.Divide);
			PassInt (ExpressionType.Equal);
			PassInt (ExpressionType.ExclusiveOr);
			PassInt (ExpressionType.GreaterThan);
			PassInt (ExpressionType.GreaterThanOrEqual);
			PassInt (ExpressionType.LeftShift);
			PassInt (ExpressionType.LessThan);
			PassInt (ExpressionType.LessThanOrEqual);
			PassInt (ExpressionType.Multiply);
			PassInt (ExpressionType.MultiplyChecked);
			PassInt (ExpressionType.NotEqual);
			PassInt (ExpressionType.Or);
			PassInt (ExpressionType.Modulo);
			PassInt (ExpressionType.RightShift);
			PassInt (ExpressionType.Subtract);
			PassInt (ExpressionType.SubtractChecked);

			// Remove comment when the code is implemented:
			//FailInt (ExpressionType.AndAlso);
			//FailInt (ExpressionType.OrElse);

			//This should test for types, not operation: FailInt (ExpressionType.Power);
#if false
	// These currently fail, because it now goes directly to the nodes, instead of doing
	// a first-pass check;   REmove when its all done.
			FailInt (ExpressionType.ArrayLength);
			FailInt (ExpressionType.ArrayIndex);
			FailInt (ExpressionType.Call);
			FailInt (ExpressionType.Coalesce);
			FailInt (ExpressionType.Conditional);
			FailInt (ExpressionType.Constant);
			FailInt (ExpressionType.Convert);
			FailInt (ExpressionType.ConvertChecked);
			FailInt (ExpressionType.Invoke);
			FailInt (ExpressionType.Lambda);
			FailInt (ExpressionType.ListInit);
			FailInt (ExpressionType.MemberAccess);
			FailInt (ExpressionType.MemberInit);
			FailInt (ExpressionType.Negate);
			FailInt (ExpressionType.UnaryPlus);
			FailInt (ExpressionType.NegateChecked);
			FailInt (ExpressionType.New);
			FailInt (ExpressionType.NewArrayInit);
			FailInt (ExpressionType.NewArrayBounds);
			FailInt (ExpressionType.Not);
			FailInt (ExpressionType.Parameter);
			FailInt (ExpressionType.Quote);
			FailInt (ExpressionType.TypeAs);
			FailInt (ExpressionType.TypeIs);
#endif
		}
	}
}
