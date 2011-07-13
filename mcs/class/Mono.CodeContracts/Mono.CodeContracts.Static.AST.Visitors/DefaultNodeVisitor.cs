// 
// DefaultNodeVisitor.cs
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
	class DefaultNodeVisitor : NodeVisitor {
		#region Overrides of NodeVisitor
		public override Node Visit (Node node)
		{
			if (node == null)
				return null;
			switch (node.NodeType) {
			case NodeType.Nop:
				return node;

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
				return VisitBinaryExpression ((BinaryExpression) node);
				#endregion

			case NodeType.Call:
			case NodeType.Jmp:
			case NodeType.MethodCall:
				return VisitMethodCall ((MethodCall) node);

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
				return VisitUnaryExpression ((UnaryExpression) node);

			case NodeType.Literal:
				return VisitLiteral ((Literal) node);
			case NodeType.This:
				return VisitThis ((This) node);

			case NodeType.Block:
				return VisitBlock ((Block) node);
			case NodeType.Branch:
				return VisitBranch ((Branch) node);
			case NodeType.Return:
				return VisitReturn ((Return) node);
			case NodeType.AssignmentStatement:
				return VisitAssignmentStatement ((AssignmentStatement) node);
			case NodeType.Local:
				return VisitLocal ((Local) node);
			case NodeType.Parameter:
				return VisitParameter ((Parameter) node);
			case NodeType.ExpressionStatement:
				return VisitExpressionStatement ((ExpressionStatement) node);
			case NodeType.Method:
				return VisitMethod ((Method) node);
			case NodeType.MethodContract:
				return VisitMethodContract ((MethodContract) node);
			case NodeType.Requires:
				return VisitRequires ((Requires) node);
			case NodeType.Ensures:
				return VisitEnsures ((Ensures) node);
			case NodeType.TypeNode:
				return VisitTypeNode ((TypeNode) node);
			case NodeType.Assembly:
				return VisitAssembly ((AssemblyNode) node);
			case NodeType.Module:
				return VisitModule ((Module) node);
			case NodeType.MemberBinding:
				return VisitMemberBinding ((MemberBinding) node);
			case NodeType.Construct:
				return VisitConstruct ((Construct) node);
			}

			return VisitUnknownNodeType (node);
		}

		public virtual AssemblyNode VisitAssembly (AssemblyNode node)
		{
			if (node == null)
				return null;

			VisitModuleList (node.Modules);


			return node;
		}

		public virtual void VisitModuleList (IEnumerable<Module> node)
		{
			if (node == null)
				return;

			foreach (Module module in node)
				VisitModule (module);
		}

		private Module VisitModule (Module node)
		{
			if (node == null)
				return null;

			VisitTypeNodeList (node.Types);

			return node;
		}

		public virtual Statement VisitAssignmentStatement (AssignmentStatement node)
		{
			if (node == null)
				return node;
			node.Target = VisitTargetExpression (node.Target);
			node.Source = VisitExpression (node.Source);

			return node;
		}

		public virtual Expression VisitBinaryExpression (BinaryExpression node)
		{
			if (node == null)
				return node;

			node.Operand1 = VisitExpression (node.Operand1);
			node.Operand2 = VisitExpression (node.Operand2);

			return node;
		}

		public virtual Block VisitBlock (Block node)
		{
			if (node == null)
				return null;

			node.Statements = VisitStatementList (node.Statements);

			return node;
		}

		public virtual List<Statement> VisitStatementList (List<Statement> node)
		{
			if (node == null)
				return null;

			for (int i = 0; i < node.Count; i++)
				node [i] = (Statement) Visit (node [i]);

			return node;
		}

		public virtual Statement VisitBranch (Branch node)
		{
			if (node == null)
				return null;

			node.Condition = VisitExpression (node.Condition);

			return node;
		}

		public virtual Expression VisitConstruct (Construct node)
		{
			if (node == null)
				return null;

			node.Constructor = VisitExpression (node.Constructor);
			node.Arguments = VisitExpressionList (node.Arguments);

			return node;
		}

		public virtual Ensures VisitEnsures (Ensures node)
		{
			if (node == null)
				return null;

			node.Assertion = VisitExpression (node.Assertion);
			node.UserMessage = VisitExpression (node.UserMessage);

			return node;
		}

		public virtual Expression VisitExpression (Expression node)
		{
			if (node == null)
				return null;

			return node;
		}

		public virtual Statement VisitExpressionStatement (ExpressionStatement node)
		{
			if (node == null)
				return null;

			node.Expression = VisitExpression (node.Expression);

			return node;
		}

		public virtual Expression VisitLiteral (Literal node)
		{
			return node;
		}

		public virtual Expression VisitLocal (Local node)
		{
			if (node == null)
				return null;

			node.Type = VisitTypeNode (node.Type);

			//todo: maybe there should be something else

			return node;
		}

		public virtual Expression VisitMemberBinding (MemberBinding node)
		{
			if (node == null)
				return null;

			node.TargetObject = VisitExpression (node.TargetObject);

			return node;
		}

		public virtual Method VisitMethod (Method node)
		{
			if (node == null)
				return null;

			node.ReturnType = VisitTypeNode (node.ReturnType);
			node.Parameters = VisitParameterList (node.Parameters);
			node.MethodContract = VisitMethodContract (node.MethodContract);
			node.Body = VisitBlock (node.Body);

			return node;
		}

		public virtual List<Parameter> VisitParameterList (List<Parameter> node)
		{
			if (node == null)
				return null;

			for (int i = 0; i < node.Count; i++)
				node [i] = VisitParameter (node [i]);

			return node;
		}

		public virtual Expression VisitMethodCall (MethodCall node)
		{
			if (node == null)
				return null;

			node.Callee = VisitExpression (node.Callee);
			node.Arguments = VisitExpressionList (node.Arguments);

			return node;
		}

		public virtual MethodContract VisitMethodContract (MethodContract node)
		{
			if (node == null)
				return null;

			node.Requires = VisitRequiresList (node.Requires);
			node.Ensures = VisitEnsuresList (node.Ensures);

			return node;
		}

		public virtual List<Ensures> VisitEnsuresList (List<Ensures> node)
		{
			if (node == null)
				return null;

			for (int i = 0; i < node.Count; i++)
				node [i] = (Ensures) Visit (node [i]);

			return node;
		}

		public virtual List<Requires> VisitRequiresList (List<Requires> node)
		{
			if (node == null)
				return null;

			for (int i = 0; i < node.Count; i++)
				node [i] = (Requires) Visit (node [i]);

			return node;
		}

		public virtual Parameter VisitParameter (Parameter node)
		{
			if (node == null)
				return null;

			node.Type = VisitTypeNode (node.Type);

			//todo: there may be something else

			return node;
		}

		public virtual Requires VisitRequires (Requires node)
		{
			if (node == null)
				return null;

			node.Assertion = VisitExpression (node.Assertion);
			node.UserMessage = VisitExpression (node.UserMessage);

			return node;
		}

		public virtual Return VisitReturn (Return node)
		{
			if (node == null)
				return null;

			node.Expression = VisitExpression (node.Expression);

			return node;
		}

		public virtual Expression VisitTargetExpression (Expression node)
		{
			return VisitExpression (node);
		}

		public virtual Expression VisitThis (This node)
		{
			if (node == null)
				return null;

			node.Type = VisitTypeNode (node.Type);

			return node;
		}

		public virtual TypeNode VisitTypeNode (TypeNode node)
		{
			if (node == null)
				return null;

			var clazz = node as Class;
			if (clazz != null)
				clazz.BaseType = VisitTypeNode (clazz.BaseType);

			VisitPropertiesList (node.Properties);
			VisitMethodsList (node.Methods);
			VisitTypeNodeList (node.NestedTypes);

			return node;
		}

		public virtual List<Property> VisitPropertiesList (List<Property> node)
		{
			if (node == null)
				return null;

			for (int i = 0; i < node.Count; i++) {
				Property property = node [i];
				if (property != null)
					node [i] = (Property) Visit (node [i]);
			}

			return node;
		}

		public virtual List<Method> VisitMethodsList (List<Method> node)
		{
			if (node == null)
				return null;

			for (int i = 0; i < node.Count; i++) {
				Method method = node [i];
				if (method != null)
					node [i] = (Method) Visit (node [i]);
			}

			return node;
		}

		public virtual List<TypeNode> VisitTypeNodeList (List<TypeNode> node)
		{
			if (node == null)
				return null;

			for (int i = 0; i < node.Count; i++) {
				TypeNode method = node [i];
				if (method != null)
					node [i] = (TypeNode) Visit (node [i]);
			}

			return node;
		}

		public virtual Expression VisitUnaryExpression (UnaryExpression node)
		{
			if (node == null)
				return null;

			node.Operand = VisitExpression (node.Operand);

			return node;
		}

		public virtual Node VisitUnknownNodeType (Node node)
		{
			return node;
		}
		#endregion
	}
}
