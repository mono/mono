//
// QueryableTransformer.cs
//
// Authors:
//	Roei Erez (roeie@mainsoft.com)
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;

namespace System.Linq
{
	internal class QueryableTransformer : ExpressionTransformer
	{		

		internal QueryableTransformer () {}
		
		protected override MethodCallExpression VisitMethodCall (MethodCallExpression methodCall) 
		{			
			if ( IsQueryableExtension ( methodCall.Method ))
			{
				return ReplaceIQueryableMethod (methodCall);
			}
			return base.VisitMethodCall (methodCall);
		}

		protected override LambdaExpression VisitLambda (LambdaExpression lambda) 
		{
			return lambda;
		}			

		bool IsQueryableExtension (MethodInfo method) 
		{
			return	method.GetCustomAttributes(typeof(ExtensionAttribute), false).Count() > 0 &&					
					typeof(IQueryable).IsAssignableFrom( method.GetParameters () [0].ParameterType );
		}

		MethodCallExpression ReplaceIQueryableMethod (MethodCallExpression oldCall)
		{			
			Expression target = null;
			if (oldCall.Object != null){
				target = Visit (oldCall.Object);
			}			
			MethodInfo newMethod = ReplaceIQueryableMethodInfo(oldCall.Method);

			Expression [] args = new Expression [oldCall.Arguments.Count];
			int counter = 0;
			foreach (Expression e in oldCall.Arguments) {				
				Type methodParam = newMethod.GetParameters() [counter].ParameterType;				
				args [counter++] = ReplaceQuotedLambdaIfNeeded(Visit (e), methodParam);
			}
			ReadOnlyCollection<Expression> col = args.ToReadOnlyCollection();
			MethodCallExpression newMethodCall = new MethodCallExpression (target, newMethod, col);
			return newMethodCall;
		}

		static Expression ReplaceQuotedLambdaIfNeeded (Expression e, Type delegateType)
		{
			UnaryExpression unary = e as UnaryExpression;
			if (unary != null) {
				LambdaExpression lambda = unary.Operand as LambdaExpression;
				if (lambda != null && lambda.Type == delegateType)
					return lambda;
			}
			return e;
		}

		static MethodInfo ReplaceIQueryableMethodInfo (MethodInfo qm) 
		{
			Type typeToSearch = qm.DeclaringType == typeof (Queryable) ? typeof (Enumerable) : qm.DeclaringType;
			MethodInfo result = GetMatchingMethod (qm, typeToSearch);			
			if (result == null)
				throw new InvalidOperationException (
					string.Format("There is no method {0} on type {1} that matches the specified arguments", 
						qm.Name, 
						qm.DeclaringType.FullName));
			return result;
		}

		static MethodInfo GetMatchingMethod (MethodInfo qm, Type fromType)
		{
			return (from em in fromType.GetMethods ()
					where Match (em, qm)
					select em.MakeGenericMethod (qm.GetGenericArguments ()))
								 .FirstOrDefault ();
		}

		static bool Match (MethodInfo em, MethodInfo qm) {

			if (em.GetCustomAttributes (typeof (ExtensionAttribute), false).Count() == 0)
				return false;

			if (em.Name != qm.Name)
				return false;			

			if (em.GetGenericArguments ().Length != qm.GetGenericArguments ().Length)
				return false;			

			Type [] parameters = (from p in qm.GetParameters () select p.ParameterType).ToArray ();
			Type returnType = qm.ReturnType;
			
			if (parameters.Length != em.GetParameters ().Length)
				return false;

			MethodInfo instanceMethod = em;
			if (qm.IsGenericMethod) {
				if (!qm.IsGenericMethod)
					return false;
				if (em.GetParameters ().Length != qm.GetParameters ().Length)
					return false;
				Type [] genArgs = qm.GetGenericArguments ();
				instanceMethod = em.MakeGenericMethod (genArgs);
			}

			Type [] enumerableParams = (from p in instanceMethod.GetParameters () select p.ParameterType).ToArray ();

			if (enumerableParams [0] != ConvertParameter (parameters [0]))
				return false;
			for (int i = 1; i < enumerableParams.Length; ++i)
				if (!ArgumentMatch(enumerableParams [i], parameters [i]))
					return false;
			if (!ArgumentMatch(instanceMethod.ReturnType, returnType))
				return false;
			return true;
		}

		static bool ArgumentMatch (Type enumerableParam, Type queryableParam)
		{
			return enumerableParam == queryableParam || enumerableParam == ConvertParameter (queryableParam);
		}

		static Type ConvertParameter (Type type) 
		{
			if (type.IsGenericType && type.GetGenericTypeDefinition () == typeof (IQueryable<>))
				type = typeof (IEnumerable<>).MakeGenericType (type.GetGenericArguments ());
			else if (type.IsGenericType && type.GetGenericTypeDefinition () == typeof (IOrderedQueryable<>))
				type = typeof (IOrderedEnumerable<>).MakeGenericType (type.GetGenericArguments ());
			else if (type.IsGenericType && type.GetGenericTypeDefinition () == typeof (Expression<>))
				type = type.GetGenericArguments () [0];
			else if (type == typeof (IQueryable))
				type = typeof (System.Collections.IEnumerable);
			return type;
		}		
	}
}
