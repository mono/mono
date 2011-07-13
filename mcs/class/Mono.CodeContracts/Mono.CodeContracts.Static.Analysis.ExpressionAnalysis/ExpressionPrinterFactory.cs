// 
// ExpressionPrinterFactory.cs
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
using System.Text;
using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.AST.Visitors;
using Mono.CodeContracts.Static.Analysis.Drivers;
using Mono.CodeContracts.Static.Analysis.HeapAnalysis;
using Mono.CodeContracts.Static.ControlFlow;
using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Analysis.ExpressionAnalysis {
	static class ExpressionPrinterFactory {
		public static Func<LabeledSymbol<APC, SymbolicValue>, string> Printer<SymbolicValue>
			(IExpressionContextProvider<LabeledSymbol<APC, SymbolicValue>, SymbolicValue> contextProvider,
			 IMethodDriver<LabeledSymbol<APC, SymbolicValue>, SymbolicValue> methodDriver)
			where SymbolicValue : IEquatable<SymbolicValue>
		{
			return new PrinterImpl<SymbolicValue> (contextProvider, methodDriver).PrintAt;
		}

		#region Nested type: PrinterImpl
		private class PrinterImpl<SymbolicValue> :
			ISymbolicExpressionVisitor<LabeledSymbol<APC, SymbolicValue>, LabeledSymbol<APC, SymbolicValue>, SymbolicValue, StringBuilder, Dummy>
			where SymbolicValue : IEquatable<SymbolicValue> {
			private readonly IExpressionContextProvider<LabeledSymbol<APC, SymbolicValue>, SymbolicValue> context_provider;
			private readonly IMethodDriver<LabeledSymbol<APC, SymbolicValue>, SymbolicValue> method_driver;

			public PrinterImpl (IExpressionContextProvider<LabeledSymbol<APC, SymbolicValue>, SymbolicValue> contextProvider,
			                    IMethodDriver<LabeledSymbol<APC, SymbolicValue>, SymbolicValue> methodDriver)
			{
				this.context_provider = contextProvider;
				this.method_driver = methodDriver;
			}

			public string PrintAt (LabeledSymbol<APC, SymbolicValue> expr)
			{
				var sb = new StringBuilder ();
				Recurse (sb, expr);
				return sb.ToString ();
			}

			private void Recurse (StringBuilder sb, LabeledSymbol<APC, SymbolicValue> expr)
			{
				if (expr.Symbol.Equals (default(SymbolicValue)))
					sb.Append ("<!null!>");
				else
					this.context_provider.ExpressionContext.Decode<StringBuilder, Dummy, PrinterImpl<SymbolicValue>> (expr, this, sb);
			}

			#region Implementation of IExpressionILVisitor<ExternalExpression<APC,SymbolicValue>,ExternalExpression<APC,SymbolicValue>,SymbolicValue,StringBuilder,Dummy>
			public Dummy Binary (LabeledSymbol<APC, SymbolicValue> pc, BinaryOperator op, SymbolicValue dest, LabeledSymbol<APC, SymbolicValue> operand1, LabeledSymbol<APC, SymbolicValue> operand2,
			                     StringBuilder data)
			{
				data.Append ('(');
				Recurse (data, operand1);
				data.AppendFormat (" {0} ", op);
				Recurse (data, operand2);
				data.Append (')');
				return Dummy.Value;
			}

			public Dummy Isinst (LabeledSymbol<APC, SymbolicValue> pc, TypeNode type, SymbolicValue dest, LabeledSymbol<APC, SymbolicValue> obj, StringBuilder data)
			{
				data.AppendFormat ("IsInst({0}) ", this.method_driver.MetaDataProvider.FullName (type));
				Recurse (data, obj);
				return Dummy.Value;
			}

			public Dummy LoadNull (LabeledSymbol<APC, SymbolicValue> pc, SymbolicValue dest, StringBuilder polarity)
			{
				polarity.Append ("NULL");
				return Dummy.Value;
			}

			public Dummy LoadConst (LabeledSymbol<APC, SymbolicValue> pc, TypeNode type, object constant, SymbolicValue dest, StringBuilder data)
			{
				data.Append (constant.ToString ());
				return Dummy.Value;
			}

			public Dummy Sizeof (LabeledSymbol<APC, SymbolicValue> pc, TypeNode type, SymbolicValue dest, StringBuilder data)
			{
				data.AppendFormat ("sizeof({0})", this.method_driver.MetaDataProvider.FullName (type));
				return Dummy.Value;
			}

			public Dummy Unary (LabeledSymbol<APC, SymbolicValue> pc, UnaryOperator op, bool unsigned, SymbolicValue dest, LabeledSymbol<APC, SymbolicValue> source, StringBuilder data)
			{
				data.AppendFormat ("{0} ", op.ToString ());
				Recurse (data, source);
				return Dummy.Value;
			}
			#endregion

			#region Implementation of ISymbolicExpressionVisitor<ExternalExpression<APC,SymbolicValue>,ExternalExpression<APC,SymbolicValue>,SymbolicValue,StringBuilder,Dummy>
			public Dummy SymbolicConstant (LabeledSymbol<APC, SymbolicValue> pc, SymbolicValue variable, StringBuilder data)
			{
				data.Append (variable.ToString ());
				return Dummy.Value;
			}
			#endregion
		}
		#endregion
	}
}
