//
// TypeForwarders.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2013 Xamarin Inc. (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

[assembly: System.Runtime.CompilerServices.TypeForwardedToAttribute(typeof(System.Linq.IQueryable))]
[assembly: System.Runtime.CompilerServices.TypeForwardedToAttribute(typeof(System.Linq.IOrderedQueryable))]
[assembly: System.Runtime.CompilerServices.TypeForwardedToAttribute(typeof(System.Linq.IQueryable<>))]
[assembly: System.Runtime.CompilerServices.TypeForwardedToAttribute(typeof(System.Linq.IOrderedQueryable<>))]
[assembly: System.Runtime.CompilerServices.TypeForwardedToAttribute(typeof(System.Linq.IQueryProvider))]
[assembly: System.Runtime.CompilerServices.TypeForwardedToAttribute(typeof(System.Linq.Expressions.Expression))]
[assembly: System.Runtime.CompilerServices.TypeForwardedToAttribute(typeof(System.Linq.Expressions.BinaryExpression))]
[assembly: System.Runtime.CompilerServices.TypeForwardedToAttribute(typeof(System.Linq.Expressions.ConditionalExpression))]
[assembly: System.Runtime.CompilerServices.TypeForwardedToAttribute(typeof(System.Linq.Expressions.ConstantExpression))]
[assembly: System.Runtime.CompilerServices.TypeForwardedToAttribute(typeof(System.Linq.Expressions.ElementInit))]
[assembly: System.Runtime.CompilerServices.TypeForwardedToAttribute(typeof(System.Linq.Expressions.LambdaExpression))]
[assembly: System.Runtime.CompilerServices.TypeForwardedToAttribute(typeof(System.Linq.Expressions.Expression<>))]
[assembly: System.Runtime.CompilerServices.TypeForwardedToAttribute(typeof(System.Linq.Expressions.ExpressionType))]
[assembly: System.Runtime.CompilerServices.TypeForwardedToAttribute(typeof(System.Linq.Expressions.ExpressionVisitor))]
[assembly: System.Runtime.CompilerServices.TypeForwardedToAttribute(typeof(System.Linq.Expressions.InvocationExpression))]
[assembly: System.Runtime.CompilerServices.TypeForwardedToAttribute(typeof(System.Linq.Expressions.ListInitExpression))]
[assembly: System.Runtime.CompilerServices.TypeForwardedToAttribute(typeof(System.Linq.Expressions.MemberBinding))]
[assembly: System.Runtime.CompilerServices.TypeForwardedToAttribute(typeof(System.Linq.Expressions.MemberAssignment))]
[assembly: System.Runtime.CompilerServices.TypeForwardedToAttribute(typeof(System.Linq.Expressions.MemberBindingType))]
[assembly: System.Runtime.CompilerServices.TypeForwardedToAttribute(typeof(System.Linq.Expressions.MemberExpression))]
[assembly: System.Runtime.CompilerServices.TypeForwardedToAttribute(typeof(System.Linq.Expressions.MemberInitExpression))]
[assembly: System.Runtime.CompilerServices.TypeForwardedToAttribute(typeof(System.Linq.Expressions.MemberListBinding))]
[assembly: System.Runtime.CompilerServices.TypeForwardedToAttribute(typeof(System.Linq.Expressions.MemberMemberBinding))]
[assembly: System.Runtime.CompilerServices.TypeForwardedToAttribute(typeof(System.Linq.Expressions.MethodCallExpression))]
[assembly: System.Runtime.CompilerServices.TypeForwardedToAttribute(typeof(System.Linq.Expressions.NewArrayExpression))]
[assembly: System.Runtime.CompilerServices.TypeForwardedToAttribute(typeof(System.Linq.Expressions.NewExpression))]
[assembly: System.Runtime.CompilerServices.TypeForwardedToAttribute(typeof(System.Linq.Expressions.ParameterExpression))]
[assembly: System.Runtime.CompilerServices.TypeForwardedToAttribute(typeof(System.Linq.Expressions.TypeBinaryExpression))]
[assembly: System.Runtime.CompilerServices.TypeForwardedToAttribute(typeof(System.Linq.Expressions.UnaryExpression))]

#if !FULL_AOT_RUNTIME
[assembly: System.Runtime.CompilerServices.TypeForwardedToAttribute(typeof(System.Linq.Expressions.BlockExpression))]
[assembly: System.Runtime.CompilerServices.TypeForwardedToAttribute(typeof(System.Linq.Expressions.CatchBlock))]
[assembly: System.Runtime.CompilerServices.TypeForwardedToAttribute(typeof(System.Linq.Expressions.DebugInfoExpression))]
[assembly: System.Runtime.CompilerServices.TypeForwardedToAttribute(typeof(System.Linq.Expressions.DefaultExpression))]
[assembly: System.Runtime.CompilerServices.TypeForwardedToAttribute(typeof(System.Linq.Expressions.GotoExpression))]
[assembly: System.Runtime.CompilerServices.TypeForwardedToAttribute(typeof(System.Linq.Expressions.GotoExpressionKind))]
[assembly: System.Runtime.CompilerServices.TypeForwardedToAttribute(typeof(System.Linq.Expressions.IndexExpression))]
[assembly: System.Runtime.CompilerServices.TypeForwardedToAttribute(typeof(System.Linq.Expressions.LabelExpression))]
[assembly: System.Runtime.CompilerServices.TypeForwardedToAttribute(typeof(System.Linq.Expressions.LabelTarget))]
[assembly: System.Runtime.CompilerServices.TypeForwardedToAttribute(typeof(System.Linq.Expressions.LoopExpression))]
[assembly: System.Runtime.CompilerServices.TypeForwardedToAttribute(typeof(System.Linq.Expressions.RuntimeVariablesExpression))]
[assembly: System.Runtime.CompilerServices.TypeForwardedToAttribute(typeof(System.Linq.Expressions.SwitchCase))]
[assembly: System.Runtime.CompilerServices.TypeForwardedToAttribute(typeof(System.Linq.Expressions.SwitchExpression))]
[assembly: System.Runtime.CompilerServices.TypeForwardedToAttribute(typeof(System.Linq.Expressions.SymbolDocumentInfo))]
[assembly: System.Runtime.CompilerServices.TypeForwardedToAttribute(typeof(System.Linq.Expressions.TryExpression))]
#endif
