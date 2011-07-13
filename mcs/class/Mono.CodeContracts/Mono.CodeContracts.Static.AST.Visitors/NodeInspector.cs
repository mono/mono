// 
// NodeInspector.cs
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

using System.Collections.Generic;

namespace Mono.CodeContracts.Static.AST.Visitors {
	class NodeInspector {
		public virtual void Visit (Node node)
		{
			if (node == null)
				return;
			switch (node.NodeType) {
			case NodeType.Nop:
				break;

				#region Binary
			case NodeType.Add:
			case NodeType.Sub:
			case NodeType.Rem:
			case NodeType.Clt:
			case NodeType.Cgt:
			case NodeType.Ceq:
			case NodeType.Box:
			case NodeType.Le:
			case NodeType.Mul:
			case NodeType.Div:
			case NodeType.Div_Un:
			case NodeType.Rem_Un:
			case NodeType.And:
			case NodeType.Or:
			case NodeType.Shr:
			case NodeType.Xor:
			case NodeType.Shl:
			case NodeType.Shr_Un:
			case NodeType.Ne:
			case NodeType.Ge:
			case NodeType.Gt:
			case NodeType.Lt:
			case NodeType.Eq:
				VisitBinaryExpression ((BinaryExpression) node);
				break;
				#endregion

			case NodeType.Call:
			case NodeType.Jmp:
			case NodeType.MethodCall:
				VisitMethodCall ((MethodCall) node);
				break;
			case NodeType.Conv:
			case NodeType.Conv_I1:
			case NodeType.Conv_I2:
			case NodeType.Conv_I8:
			case NodeType.Conv_I4:
			case NodeType.Conv_R4:
			case NodeType.Conv_R8:
			case NodeType.Neg:
			case NodeType.Not:
			case NodeType.LogicalNot:
				VisitUnaryExpression ((UnaryExpression) node);
				break;
			case NodeType.Literal:
				VisitLiteral ((Literal) node);
				break;
			case NodeType.This:
				VisitThis ((This) node);
				break;
			case NodeType.Block:
				VisitBlock ((Block) node);
				break;
			case NodeType.Branch:
				VisitBranch ((Branch) node);
				break;
			case NodeType.Return:
				VisitReturn ((Return) node);
				break;
			case NodeType.AssignmentStatement:
				VisitAssignmentStatement ((AssignmentStatement) node);
				break;
			case NodeType.Local:
				VisitLocal ((Local) node);
				break;
			case NodeType.Parameter:
				VisitParameter ((Parameter) node);
				break;
			case NodeType.ExpressionStatement:
				VisitExpressionStatement ((ExpressionStatement) node);
				break;
			case NodeType.Method:
				VisitMethod ((Method) node);
				break;
			case NodeType.MethodContract:
				VisitMethodContract ((MethodContract) node);
				break;
			case NodeType.Requires:
				VisitRequires ((Requires) node);
				break;
			case NodeType.Ensures:
				VisitEnsures ((Ensures) node);
				break;
			case NodeType.TypeNode:
				VisitTypeNode ((TypeNode) node);
				break;
			case NodeType.Assembly:
				VisitAssembly ((AssemblyNode) node);
				break;
			case NodeType.Module:
				VisitModule ((Module) node);
				break;
			case NodeType.MemberBinding:
				VisitMemberBinding ((MemberBinding) node);
				break;
			case NodeType.Construct:
				VisitConstruct ((Construct) node);
				break;
			default:
				VisitUnknownNodeType (node);
				break;
			}
		}

		public virtual void VisitAssembly (AssemblyNode node)
		{
			if (node == null)
				return;

			VisitModuleList (node.Modules);
		}

		public virtual void VisitModuleList (IEnumerable<Module> node)
		{
			if (node == null)
				return;

			foreach (Module module in node)
				VisitModule (module);
		}

		public virtual void VisitModule (Module node)
		{
			if (node == null)
				return;

			VisitTypeNodeList (node.Types);
		}

		public virtual void VisitAssignmentStatement (AssignmentStatement node)
		{
			if (node == null)
				return;
			VisitTargetExpression (node.Target);
			VisitExpression (node.Source);
		}

		public virtual void VisitBinaryExpression (BinaryExpression node)
		{
			if (node == null)
				return;

			VisitExpression (node.Operand1);
			VisitExpression (node.Operand2);
		}

		public virtual void VisitBlock (Block node)
		{
			if (node == null)
				return;

			VisitStatementList (node.Statements);
		}

		public virtual void VisitStatementList (List<Statement> node)
		{
			if (node == null)
				return;

			for (int i = 0; i < node.Count; i++)
				Visit (node [i]);
		}

		public virtual void VisitBranch (Branch node)
		{
			if (node == null)
				return;

			VisitExpression (node.Condition);
		}

		public virtual void VisitConstruct (Construct node)
		{
			if (node == null)
				return;

			VisitExpression (node.Constructor);
			VisitExpressionList (node.Arguments);
		}

		public virtual void VisitExpressionList (List<Expression> list)
		{
			if (list == null)
				return;

			for (int i = 0; i < list.Count; ++i)
				Visit (list [i]);
		}

		public virtual void VisitEnsures (Ensures node)
		{
			if (node == null)
				return;

			VisitExpression (node.Assertion);
			VisitExpression (node.UserMessage);
		}

		public virtual void VisitExpression (Expression node)
		{
			if (node == null)
				return;

			//todo: maybe there will be something
		}

		public virtual void VisitExpressionStatement (ExpressionStatement node)
		{
			if (node == null)
				return;

			VisitExpression (node.Expression);
		}

		public virtual void VisitLiteral (Literal node)
		{
		}

		public virtual void VisitLocal (Local node)
		{
			if (node == null)
				return;

			VisitTypeNode (node.Type);

			//todo: maybe there should be something else
		}

		public virtual void VisitMemberBinding (MemberBinding node)
		{
			if (node == null)
				return;

			VisitExpression (node.TargetObject);
		}

		public virtual void VisitMethod (Method node)
		{
			if (node == null)
				return;

			VisitTypeNode (node.ReturnType);
			VisitParameterList (node.Parameters);
			VisitMethodContract (node.MethodContract);
			VisitBlock (node.Body);
		}

		public virtual void VisitParameterList (List<Parameter> node)
		{
			if (node == null)
				return;

			for (int i = 0; i < node.Count; i++)
				VisitParameter (node [i]);
		}

		public virtual void VisitMethodCall (MethodCall node)
		{
			if (node == null)
				return;

			VisitExpression (node.Callee);
			VisitExpressionList (node.Arguments);
		}

		public virtual void VisitMethodContract (MethodContract node)
		{
			if (node == null)
				return;

			VisitRequiresList (node.Requires);
			VisitEnsuresList (node.Ensures);
		}

		public virtual void VisitEnsuresList (List<Ensures> node)
		{
			if (node == null)
				return;

			for (int i = 0; i < node.Count; i++)
				Visit (node [i]);
		}

		public virtual void VisitRequiresList (List<Requires> node)
		{
			if (node == null)
				return;

			for (int i = 0; i < node.Count; i++)
				Visit (node [i]);
		}

		public virtual void VisitParameter (Parameter node)
		{
			if (node == null)
				return;

			VisitTypeNode (node.Type);

			//todo: there may be something else
		}

		public virtual void VisitRequires (Requires node)
		{
			if (node == null)
				return;

			VisitExpression (node.Assertion);
			VisitExpression (node.UserMessage);
		}

		public virtual void VisitReturn (Return node)
		{
			if (node == null)
				return;

			VisitExpression (node.Expression);
		}

		public virtual void VisitTargetExpression (Expression node)
		{
			VisitExpression (node);
		}

		public virtual void VisitThis (This node)
		{
			if (node == null)
				return;

			VisitTypeNode (node.Type);
		}

		public virtual void VisitTypeNode (TypeNode node)
		{
			if (node == null)
				return;

			var clazz = node as Class;
			if (clazz != null)
				VisitTypeNode (clazz.BaseType);

			VisitPropertiesList (node.Properties);
			VisitMethodsList (node.Methods);
			VisitTypeNodeList (node.NestedTypes);
		}

		public virtual void VisitPropertiesList (List<Property> node)
		{
			if (node == null)
				return;

			for (int i = 0; i < node.Count; i++) {
				Property property = node [i];
				if (property != null)
					Visit (node [i]);
			}
		}

		public virtual void VisitMethodsList (List<Method> node)
		{
			if (node == null)
				return;

			for (int i = 0; i < node.Count; i++) {
				Method method = node [i];
				if (method != null)
					Visit (node [i]);
			}
		}

		public virtual void VisitTypeNodeList (List<TypeNode> node)
		{
			if (node == null)
				return;

			for (int i = 0; i < node.Count; i++) {
				TypeNode typeNode = node [i];
				if (typeNode != null)
					Visit (typeNode);
			}
		}

		public virtual void VisitUnaryExpression (UnaryExpression node)
		{
			if (node == null)
				return;

			VisitExpression (node.Operand);
		}

		public virtual void VisitUnknownNodeType (Node node)
		{
		}
	}
}
