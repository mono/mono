//
// PerformRewrite.cs
//
// Authors:
//	Chris Bacon (chrisbacon76@gmail.com)
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.CodeContracts.Rewrite.Ast;
using Mono.CodeContracts.Rewrite.AstVisitors;

namespace Mono.CodeContracts.Rewrite {
	class PerformRewrite {

		public PerformRewrite (ISymbolWriter sym, RewriterOptions options)
		{
			this.sym = sym;
			this.options = options;
		}

		private ISymbolWriter sym;
		private RewriterOptions options;
		private Dictionary<MethodDefinition, TransformContractsVisitor> rewrittenMethods = new Dictionary<MethodDefinition, TransformContractsVisitor> ();

		public void Rewrite (AssemblyDefinition assembly)
		{
			foreach (var module in assembly.Modules) {
				ContractsRuntime contractsRuntime = new ContractsRuntime(module, this.options);

				var allMethods =
					from type in module.Types
					from method in type.Methods
					select method;

				foreach (var method in allMethods.ToArray ()) {
					this.RewriteMethod (method, contractsRuntime);
				}
			}
		}

		private void RewriteMethod (MethodDefinition method, ContractsRuntime contractsRuntime)
		{
			if (this.rewrittenMethods.ContainsKey (method)) {
				return;
			}
			var overridden = this.GetOverriddenMethod (method);
			if (overridden != null) {
				this.RewriteMethod (overridden, contractsRuntime);
			}
			bool anyRewrites = false;
			var baseMethod = this.GetBaseOverriddenMethod (method);
			if (baseMethod != method) {
				// Contract inheritance must be used
				var vOverriddenTransform = this.rewrittenMethods [baseMethod];
				// Can be null if overriding an abstract method
				if (vOverriddenTransform != null) {
					if (this.options.Level >= 2) {
						// Only insert re-written contracts if level >= 2
						foreach (var inheritedRequires in vOverriddenTransform.ContractRequiresInfo) {
							this.RewriteIL (method.Body, null, null, inheritedRequires.RewrittenExpr);
							anyRewrites = true;
						}
					}
				}
			}

			TransformContractsVisitor vTransform = null;
			if (method.HasBody) {
				vTransform = this.TransformContracts (method, contractsRuntime);
				if (this.sym != null) {
					this.sym.Write (method.Body);
				}
				if (vTransform.ContractRequiresInfo.Any ()) {
					anyRewrites = true;
				}
			}
			this.rewrittenMethods.Add (method, vTransform);

			if (anyRewrites) {
				Console.WriteLine (method.FullName);
			}
		}

		private TransformContractsVisitor TransformContracts (MethodDefinition method, ContractsRuntime contractsRuntime)
		{
			var body = method.Body;
			Decompile decompile = new Decompile (method);
			var decomp = decompile.Go ();

			TransformContractsVisitor vTransform = new TransformContractsVisitor (method, decompile.Instructions, contractsRuntime);
			vTransform.Visit (decomp);

			foreach (var replacement in vTransform.ContractRequiresInfo) {
				// Only insert re-written contracts if level >= 2
				Expr rewritten = this.options.Level >= 2 ? replacement.RewrittenExpr : null;
				this.RewriteIL (body, decompile.Instructions, replacement.OriginalExpr, rewritten);
			}

			return vTransform;
		}

		private void RewriteIL (MethodBody body, Dictionary<Expr,Instruction> instructionLookup, Expr remove, Expr insert)
		{
			var il = body.GetILProcessor ();
			Instruction instInsertBefore;
			if (remove != null) {
				var vInstExtent = new InstructionExtentVisitor (instructionLookup);
				vInstExtent.Visit (remove);
				instInsertBefore = vInstExtent.Instructions.Last ().Next;
				foreach (var instRemove in vInstExtent.Instructions) {
					il.Remove (instRemove);
				}
			} else {
				instInsertBefore = body.Instructions [0];
			}
			if (insert != null) {
				var compiler = new CompileVisitor (il, instructionLookup, inst => il.InsertBefore (instInsertBefore, inst));
				compiler.Visit (insert);
			}
		}

		private MethodDefinition GetOverriddenMethod (MethodDefinition method)
		{
			if (method.IsNewSlot || !method.IsVirtual) {
				return null;
			}
			var baseType = method.DeclaringType.BaseType;
			if (baseType == null) {
				return null;
			}
			var overridden = baseType.Resolve ().Methods.FirstOrDefault (x => x.Name == method.Name);
			return overridden;
		}

		private MethodDefinition GetBaseOverriddenMethod (MethodDefinition method)
		{
			var overridden = method;
			while (true) {
				var overriddenTemp = this.GetOverriddenMethod (overridden);
				if (overriddenTemp == null) {
					return overridden;
				}
				overridden = overriddenTemp;
			}
		}

	}
}
