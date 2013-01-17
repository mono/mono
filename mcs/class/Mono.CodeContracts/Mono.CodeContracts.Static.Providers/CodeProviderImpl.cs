// 
// CodeProviderImpl.cs
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
using System.Collections.Generic;
using System.Linq;
using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.AST.Visitors;
using Mono.CodeContracts.Static.ControlFlow;
using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Providers {
	class CodeProviderImpl : IMethodCodeProvider<CodeProviderImpl.PC, ExceptionHandler> {
		public static readonly CodeProviderImpl Instance = new CodeProviderImpl ();

		#region IMethodCodeProvider<PC,ExceptionHandler> Members
		public Result Decode<Visitor, Data, Result> (PC pc, Visitor visitor, Data data)
			where Visitor : IAggregateVisitor<PC, Data, Result>
		{
			Node nested;
			Node node = Decode (pc, out nested);
			if (IsAtomicNested (nested))
				node = nested;
			else if (nested != null)
				return visitor.Aggregate (pc, new PC (nested), nested is Block, data);

			if (node == null)
				return visitor.Nop (pc, data);

			switch (node.NodeType) {
			case NodeType.Block:
			case NodeType.Nop:
				return visitor.Nop (pc, data);
			case NodeType.Clt:
			case NodeType.Lt:
				return visitor.Binary (pc, BinaryOperator.Clt, Dummy.Value, Dummy.Value, Dummy.Value, data);
			case NodeType.Cgt:
			case NodeType.Gt:
				return visitor.Binary (pc, BinaryOperator.Cgt, Dummy.Value, Dummy.Value, Dummy.Value, data);
			case NodeType.Ceq:
			case NodeType.Eq:
				return visitor.Binary (pc, BinaryOperator.Ceq, Dummy.Value, Dummy.Value, Dummy.Value, data);
			case NodeType.Ne:
				return visitor.Binary (pc, BinaryOperator.Cne_Un, Dummy.Value, Dummy.Value, Dummy.Value, data);
			case NodeType.Ge:
				return visitor.Binary (pc, BinaryOperator.Cge, Dummy.Value, Dummy.Value, Dummy.Value, data);
			case NodeType.Le:
				return visitor.Binary (pc, BinaryOperator.Cle, Dummy.Value, Dummy.Value, Dummy.Value, data);
			case NodeType.Add:
				return visitor.Binary (pc, BinaryOperator.Add, Dummy.Value, Dummy.Value, Dummy.Value, data);
			case NodeType.Sub:
				return visitor.Binary (pc, BinaryOperator.Sub, Dummy.Value, Dummy.Value, Dummy.Value, data);
			case NodeType.Rem:
				return visitor.Binary (pc, BinaryOperator.Rem, Dummy.Value, Dummy.Value, Dummy.Value, data);
			case NodeType.Rem_Un:
				return visitor.Binary (pc, BinaryOperator.Rem_Un, Dummy.Value, Dummy.Value, Dummy.Value, data);
			case NodeType.Mul:
				return visitor.Binary (pc, BinaryOperator.Mul, Dummy.Value, Dummy.Value, Dummy.Value, data);
			case NodeType.Div:
				return visitor.Binary (pc, BinaryOperator.Div, Dummy.Value, Dummy.Value, Dummy.Value, data);
			case NodeType.Div_Un:
				return visitor.Binary (pc, BinaryOperator.Div_Un, Dummy.Value, Dummy.Value, Dummy.Value, data);
			case NodeType.And:
				return visitor.Binary (pc, BinaryOperator.And, Dummy.Value, Dummy.Value, Dummy.Value, data);
			case NodeType.Or:
				return visitor.Binary (pc, BinaryOperator.Or, Dummy.Value, Dummy.Value, Dummy.Value, data);
			case NodeType.Shr:
				return visitor.Binary (pc, BinaryOperator.Shr, Dummy.Value, Dummy.Value, Dummy.Value, data);
			case NodeType.Xor:
				return visitor.Binary (pc, BinaryOperator.Xor, Dummy.Value, Dummy.Value, Dummy.Value, data);
			case NodeType.Shl:
				return visitor.Binary (pc, BinaryOperator.Shl, Dummy.Value, Dummy.Value, Dummy.Value, data);
			case NodeType.Shr_Un:
				return visitor.Binary (pc, BinaryOperator.Shr_Un, Dummy.Value, Dummy.Value, Dummy.Value, data);
			case NodeType.Literal:
				var literal = (Literal) node;
				if (literal.Value == null)
					return visitor.LoadNull (pc, Dummy.Value, data);
				if (literal.Type == CoreSystemTypes.Instance.TypeBoolean && (bool) literal.Value)
					return visitor.LoadConst (pc, CoreSystemTypes.Instance.TypeInt32, 1, Dummy.Value, data);

				return visitor.LoadConst (pc, literal.Type, literal.Value, Dummy.Value, data);
			case NodeType.This:
			case NodeType.Parameter:
				return visitor.LoadArg (pc, (Parameter) node, false, Dummy.Value, data);
			case NodeType.Local:
				return visitor.LoadLocal (pc, (Local) node, Dummy.Value, data);
			case NodeType.Branch:
				var branch = (Branch) node;
				if (branch.Condition != null)
					return visitor.BranchTrue (pc, new PC (branch.Target), Dummy.Value, data);
				return visitor.Branch (pc, new PC (branch.Target), branch.LeavesExceptionBlock, data);
			case NodeType.ExpressionStatement:
				break;
			case NodeType.Box:
				break;
			case NodeType.Return:
				return visitor.Return (pc, Dummy.Value, data);
			case NodeType.Neg:
				return visitor.Unary (pc, UnaryOperator.Neg, false, Dummy.Value, Dummy.Value, data);
			case NodeType.Not:
			case NodeType.LogicalNot:
				return visitor.Unary (pc, UnaryOperator.Not, false, Dummy.Value, Dummy.Value, data);
			case NodeType.Conv:
				break;
			case NodeType.Conv_I1:
				return visitor.Unary (pc, UnaryOperator.Conv_i1, false, Dummy.Value, Dummy.Value, data);
			case NodeType.Conv_I2:
				return visitor.Unary (pc, UnaryOperator.Conv_i2, false, Dummy.Value, Dummy.Value, data);
			case NodeType.Conv_I4:
				return visitor.Unary (pc, UnaryOperator.Conv_i4, false, Dummy.Value, Dummy.Value, data);
			case NodeType.Conv_I8:
				return visitor.Unary (pc, UnaryOperator.Conv_i8, false, Dummy.Value, Dummy.Value, data);
			case NodeType.Conv_R4:
				return visitor.Unary (pc, UnaryOperator.Conv_r4, false, Dummy.Value, Dummy.Value, data);
			case NodeType.Conv_R8:
				return visitor.Unary (pc, UnaryOperator.Conv_r8, false, Dummy.Value, Dummy.Value, data);
			case NodeType.MethodContract:
				return visitor.Nop (pc, data);
			case NodeType.Requires:
				return visitor.Assume (pc, EdgeTag.Requires, Dummy.Value, data);
			case NodeType.Call:
				var call = (MethodCall) node;
				Method method = GetMethodFrom (call.Callee);
				if (method.HasGenericParameters)
					throw new NotImplementedException ();
				if (method.Name != null && method.DeclaringType.Name != null && method.DeclaringType.Name.EndsWith ("Contract")) {
					switch (method.Name) {
					case "Assume":
						if (method.Parameters.Count == 1)
							return visitor.Assume (pc, EdgeTag.Assume, Dummy.Value, data);
						break;
					case "Assert":
						if (method.Parameters.Count == 1)
							return visitor.Assert (pc, EdgeTag.Assert, Dummy.Value, data);
						break;
					}
				}
				Indexable<Dummy> parameters = DummyIndexable (method);
				return visitor.Call (pc, method, false, GetVarargs (call, method), Dummy.Value, parameters, data);

			case NodeType.AssignmentStatement:
				var assign = ((AssignmentStatement) node);
				var local = assign.Target as Local;
				if (local != null)
					return visitor.StoreLocal (pc, local, Dummy.Value, data);
				var parameter = assign.Target as Parameter;
				if (parameter != null)
					return visitor.StoreArg (pc, parameter, Dummy.Value, data);

				var binding = assign.Target as MemberBinding;
				if (binding != null) {
					if (binding.BoundMember.IsStatic)
						return visitor.StoreStaticField (pc, (Field) binding.BoundMember, Dummy.Value, data);
					else
						return visitor.StoreField (pc, (Field) binding.BoundMember, Dummy.Value, Dummy.Value, data);
				}

				throw new NotImplementedException ();
			case NodeType.Construct:
				Method ctor = GetMethodFrom (((Construct) node).Constructor);
				if (!(ctor.DeclaringType is ArrayTypeNode))
					return visitor.NewObj (pc, ctor, Dummy.Value, DummyIndexable (ctor), data);
				var arrayType = (ArrayTypeNode) ctor.DeclaringType;
				return visitor.NewArray (pc, arrayType, Dummy.Value, DummyIndexable (ctor), data);
			default:
				return visitor.Nop (pc, data);
			}

			throw new NotImplementedException ();
		}

		public bool Next (PC pc, out PC nextLabel)
		{
			Node nested;
			if (Decode (pc, out nested) == null && pc.Node != null) {
				nextLabel = new PC (pc.Node, pc.Index + 1);
				return true;
			}
			nextLabel = new PC ();
			return false;
		}

		public int GetILOffset (PC current)
		{
			throw new NotImplementedException ();
		}
		#endregion

		private static Indexable<Dummy> DummyIndexable (Method method)
		{
			return new Indexable<Dummy> (Enumerable.Range (0, method.Parameters.Count).Select (it => Dummy.Value).ToList ());
		}

		private static Indexable<TypeNode> GetVarargs (MethodCall call, Method method)
		{
			int methodCount = method.Parameters.Count;
			int callCount = call.Arguments.Count;

			if (callCount <= methodCount)
				return new Indexable<TypeNode> (null);

			var array = new TypeNode[callCount - methodCount];
			for (int i = methodCount; i < callCount; i++)
				array [i - methodCount] = call.Arguments [i - methodCount].Type;

			return new Indexable<TypeNode> (array);
		}

		private Method GetMethodFrom (Expression callee)
		{
			return (Method) ((MemberBinding) callee).BoundMember;
		}

		private static bool IsAtomicNested (Node nested)
		{
			if (nested == null)
				return false;
			switch (nested.NodeType) {
			case NodeType.Local:
			case NodeType.Parameter:
			case NodeType.Literal:
			case NodeType.This:
				return true;
			default:
				return false;
			}
		}

		private Node Decode (PC pc, out Node nested)
		{
			Node node = DecodeInflate (pc, out nested);

			return node;
		}

		/// <summary>
		/// Decodes pc
		/// </summary>
		/// <param name="pc"></param>
		/// <param name="nested"></param>
		/// <returns>If node has nested, returns null and (nested = child). If last child given, node equals pc.Node</returns>
		private static Node DecodeInflate (PC pc, out Node nested)
		{
			Node node = pc.Node;
			if (node == null) {
				nested = null;
				return null;
			}

			int index = pc.Index;
			switch (node.NodeType) {
			case NodeType.MethodContract:
				var methodContract = (MethodContract) node;
				if (index < methodContract.RequiresCount) {
					nested = methodContract.Requires [index];
					return null;
				}
				if (index == methodContract.RequiresCount) {
					nested = null;
					return methodContract;
				}

				//todo: aggregate ensures
				nested = null;
				return methodContract;

			case NodeType.Requires:
				var requires = (Requires) node;
				if (index == 0) {
					nested = requires.Assertion;
					return null;
				}
				nested = null;
				return requires;
			case NodeType.Block:
				var block = (Block) node;
				if (block.Statements == null) {
					nested = null;
					return block;
				}
				nested = index >= block.Statements.Count ? null : block.Statements [index];
				return index + 1 < block.Statements.Count ? null : block;
			case NodeType.Return:
				var ret = (Return) node;
				if (ret.Expression != null && index == 0) {
					nested = ret.Expression;
					return null;
				}
				nested = null;
				return ret;
			case NodeType.AssignmentStatement:
				var assign = (AssignmentStatement) node;
				int innerIndex = index;
				{
					var bind = assign.Target as MemberBinding;
					if (bind != null) {
						++innerIndex;
						if (bind.BoundMember.IsStatic)
							++innerIndex;
						if (innerIndex == 1) {
							nested = bind.TargetObject;
							return null;
						}
					} else if (assign.Target is Variable)
						innerIndex += 2;
					else {
						nested = null;
						return assign;
					}
				}
				if (innerIndex == 2) {
					nested = assign.Source;
					return null;
				}

				nested = null;
				return assign;
			case NodeType.ExpressionStatement:
				var expressionStatement = (ExpressionStatement) node;
				nested = expressionStatement.Expression;
				return expressionStatement;
			case NodeType.MethodCall:
			case NodeType.Call:
			case NodeType.Calli:
			case NodeType.CallVirt:
				var methodCall = (MethodCall) node;
				var binding = (MemberBinding) methodCall.Callee;
				if (binding.BoundMember.IsStatic) {
					if (index < methodCall.Arguments.Count) {
						nested = methodCall.Arguments [index];
						return null;
					}

					nested = null;
					return methodCall;
				}

				if (index == 0) {
					nested = binding.TargetObject;
					return null;
				}
				if (index < methodCall.Arguments.Count + 1) {
					nested = methodCall.Arguments [index - 1];
					return null;
				}
				nested = null;
				return methodCall;
			case NodeType.MemberBinding:
				var bind1 = ((MemberBinding) node);
				if (index == 0 && !bind1.BoundMember.IsStatic) {
					nested = bind1.TargetObject;
					return null;
				}
				nested = null;
				return bind1;
			case NodeType.Construct:
				var construct = (Construct) node;
				if (index < construct.Arguments.Count) {
					nested = construct.Arguments [index];
					return null;
				}
				nested = null;
				return construct;
			case NodeType.Branch:
				var branch = ((Branch) node);
				if (branch.Condition != null && index == 0) {
					nested = branch.Condition;
					return null;
				}
				nested = null;
				return branch;
			default:
				var binary = node as BinaryExpression;
				if (binary != null) {
					if (index == 0) {
						nested = binary.Left;
						return null;
					}
					if (index == 1) {
						nested = binary.Right;
						return null;
					}
					nested = null;
					return binary;
				}

				var unary = node as UnaryExpression;
				if (unary != null) {
					if (index == 0) {
						nested = unary.Operand;
						return null;
					}

					nested = null;
					return unary;
				}

				//todo: ternary
				nested = null;
				return node;
			}
		}

		public PC Entry (Method method)
		{
			return new PC (method.Body);
		}

		#region Implementation of IMethodCodeProvider<PC,Local,Parameter,Method,FieldReference,TypeReference,Dummy>
		public bool IsFaultHandler (ExceptionHandler handler)
		{
			return handler.HandlerType == NodeType.FaultHandler;
		}

		public bool IsFilterHandler (ExceptionHandler handler)
		{
			return handler.HandlerType == NodeType.Filter;
		}

		public bool IsFinallyHandler (ExceptionHandler handler)
		{
			return handler.HandlerType == NodeType.Finally;
		}

		public PC FilterExpressionStart (ExceptionHandler handler)
		{
			return new PC (handler.FilterExpression);
		}

		public PC HandlerEnd (ExceptionHandler handler)
		{
			throw new NotImplementedException ();
		}

		public PC HandlerStart (ExceptionHandler handler)
		{
			throw new NotImplementedException ();
		}

		public PC TryStart (ExceptionHandler handler)
		{
			throw new NotImplementedException ();
		}

		public PC TryEnd (ExceptionHandler handler)
		{
			throw new NotImplementedException ();
		}

		public bool IsCatchHandler (ExceptionHandler handler)
		{
			return handler.HandlerType == NodeType.Catch;
		}

		public TypeNode CatchType (ExceptionHandler handler)
		{
			return handler.FilterType;
		}

		public bool IsCatchAllHandler (ExceptionHandler handler)
		{
			if (!IsCatchHandler (handler))
				return false;
			if (handler.FilterType != null)
				return false;

			return true;
		}

		public IEnumerable<ExceptionHandler> GetTryBlocks (Method method)
		{
			yield break;
		}
		#endregion

		#region Nested type: PC
		public struct PC : IEquatable<PC> {
			public readonly int Index;
			public readonly Node Node;

			public PC (Node Node)
				: this (Node, 0)
			{
			}

			public PC (Node node, int index)
			{
				this.Node = node;
				this.Index = index;
			}

			#region IEquatable<PC> Members
			public bool Equals (PC other)
			{
				return Equals (other.Node, this.Node) && other.Index == this.Index;
			}
			#endregion

			public override bool Equals (object obj)
			{
				if (ReferenceEquals (null, obj))
					return false;
				if (obj.GetType () != typeof (PC))
					return false;
				return Equals ((PC) obj);
			}

			public override int GetHashCode ()
			{
				unchecked {
					return ((this.Node != null ? this.Node.GetHashCode () : 0)*397) ^ this.Index;
				}
			}
		}
		#endregion
	}
}
