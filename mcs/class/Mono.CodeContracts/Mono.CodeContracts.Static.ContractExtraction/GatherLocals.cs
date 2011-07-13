// 
// GatherLocals.cs
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
using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.AST.Visitors;

namespace Mono.CodeContracts.Static.ContractExtraction {
	class GatherLocals : NodeInspector {
		public HashSet<Local> Locals = new HashSet<Local> ();
		private Local exempt_result_local;

		public override void VisitLocal (Local node)
		{
			if (!IsLocalExempt (node) && !this.Locals.Contains (node))
				this.Locals.Add (node);
			base.VisitLocal (node);
		}

		public override void VisitAssignmentStatement (AssignmentStatement node)
		{
			if (node.Target is Local && IsResultExpression (node.Source))
				this.exempt_result_local = (Local) node.Target;
			base.VisitAssignmentStatement (node);
		}

		private bool IsResultExpression (Expression expression)
		{
			var methodCall = expression as MethodCall;
			if (methodCall == null)
				return false;

			var memberBinding = methodCall.Callee as MemberBinding;
			if (memberBinding == null)
				return false;

			var method = memberBinding.BoundMember as Method;
			if (method == null)
				return false;

			return method.HasGenericParameters && method.Name == "Result" && method.DeclaringType != null && method.DeclaringType.Name == "Contract";
		}

		private bool IsLocalExempt (Local local)
		{
			if (local == this.exempt_result_local)
				return true;
			bool result = false;
			if (local.Name != null && !local.Name.StartsWith ("local"))
				result = true;
			TypeNode type = local.Type;
			if (type == null || HelperMethods.IsCompilerGenerated (type) || local.Name == "_preconditionHolds")
				return true;

			if (result)
				return LocalNameIsExempt (local.Name);

			return true;
		}

		private bool LocalNameIsExempt (string name)
		{
			return name.StartsWith ("CS$") || name.StartsWith ("VB$");
		}
	}
}
