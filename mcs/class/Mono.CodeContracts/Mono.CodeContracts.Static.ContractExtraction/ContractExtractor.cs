// 
// ContractExtractor.cs
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
using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.AST.Visitors;

namespace Mono.CodeContracts.Static.ContractExtraction {
	class ContractExtractor : DefaultNodeVisitor {
		private readonly AssemblyNode assembly;
		private readonly ContractNodes contract_nodes;
		private readonly bool verbose;
		private readonly Dictionary<Method, Method> visited_methods;

		public ContractExtractor (ContractNodes contractNodes, AssemblyNode assembly)
			: this (contractNodes, assembly, DebugOptions.Debug)
		{
		}

		public ContractExtractor (ContractNodes contractNodes, AssemblyNode assembly, bool verbose)
		{
			this.visited_methods = new Dictionary<Method, Method> ();
			this.contract_nodes = contractNodes;
			this.assembly = assembly;
			this.verbose = verbose;
		}

		public override AssemblyNode VisitAssembly (AssemblyNode node)
		{
			if (node == null)
				return null;
			if (this.verbose)
				Console.WriteLine ("Extracting from '{0}'", this.assembly.FullName);

			return base.VisitAssembly (node);
		}

		public override Method VisitMethod (Method node)
		{
			if (node == null)
				return null;

			if (this.visited_methods.ContainsKey (node))
				return node;

			this.visited_methods.Add (node, node);
			node.ContractProvider = ExtractContractsFromMethod;

			return node;
		}

		public void ExtractContractsFromMethod (Method method)
		{
			MethodContract contract = method.MethodContract = new MethodContract (method);
			List<Requires> preconditions = null;
			List<Ensures> postconditions = null;

			if (method.IsAbstract)
				return;

			if (method.Body != null && method.Body.Statements != null) {
				if (this.verbose)
					Console.WriteLine (method.FullName);

				ExtractContractsInternal (method, ref preconditions, ref postconditions);

				contract.Requires = preconditions;
				contract.Ensures = postconditions;
			}
		}

		private void ExtractContractsInternal (Method method, ref List<Requires> preconditions, ref List<Ensures> postconditions)
		{
			if (method == null)
				return;
			if (this.verbose)
				Console.WriteLine ("Method: " + method.FullName);

			Block body = method.Body;
			if (body == null || body.Statements == null || body.Statements.Count <= 0)
				return;

			int lastBlockContainingContract;
			int lastStatementContainingContract;

			int begin = 0;
			bool contractsFound = FindLastBlockWithContracts (body.Statements, begin,
			                                                  out lastBlockContainingContract, out lastStatementContainingContract);
			if (!contractsFound) {
				if (this.verbose)
					Console.WriteLine ("\tNo contracts found");
				return;
			}

			List<Statement> contractSection = HelperMethods.ExtractContractBlocks (body.Statements, begin, 0,
			                                                                       lastBlockContainingContract, lastStatementContainingContract);

			preconditions = new List<Requires> ();
			postconditions = new List<Ensures> ();

			ExtractPrePostConditionsFromContractSection (contractSection, preconditions, postconditions);
		}

		private bool ExtractPrePostConditionsFromContractSection (List<Statement> contractSection,
		                                                          List<Requires> preconditions, List<Ensures> postconditions)
		{
			List<Statement> blocks = contractSection;
			int firstBlockIndex = 0;
			int blocksCount = blocks.Count;
			int firstStmtIndex = HelperMethods.FindNextRealStatement (((Block) blocks [firstBlockIndex]).Statements, 0);

			bool wasEndContractBlock = false;

			for (int lastBlockIndex = firstBlockIndex; lastBlockIndex < blocksCount; ++lastBlockIndex) {
				var block = (Block) blocks [lastBlockIndex];
				if (block == null)
					continue;

				int cnt = block.Statements == null ? 0 : block.Statements.Count;
				for (int lastStmtIndex = 0; lastStmtIndex < cnt; ++lastStmtIndex) {
					Statement s = block.Statements [lastStmtIndex];
					if (s == null)
						continue;

					Method calledMethod = HelperMethods.IsMethodCall (s);
					if (!this.contract_nodes.IsContractMethod (calledMethod))
						continue;

					if (wasEndContractBlock) {
						if (DebugOptions.Debug)
							Console.WriteLine ("Contract call after prior ContractBlock");
						break;
					}
					if (this.contract_nodes.IsEndContractBlock (calledMethod)) {
						wasEndContractBlock = true;
						continue;
					}

					//here we definitely know that s is ExpressionStatement of (MethodCall). see HelperMethods.IsMethodCall(s)
					var methodCall = ((ExpressionStatement) s).Expression as MethodCall;
					Expression assertionExpression = methodCall.Arguments [0];
					Expression expression;
					if (firstBlockIndex == lastBlockIndex && firstStmtIndex == lastStmtIndex)
						expression = assertionExpression;
					else {
						block.Statements [lastStmtIndex] = new ExpressionStatement (assertionExpression);
						List<Statement> contractBlocks = HelperMethods.ExtractContractBlocks (blocks, firstBlockIndex, firstStmtIndex,
						                                                                      lastBlockIndex, lastStmtIndex);
						var b = new Block (contractBlocks);

						expression = new BlockExpression (b);
					}

					MethodContractElement methodContractElement = null;
					if (this.contract_nodes.IsPlainPrecondition (calledMethod)) {
						var requires = new Requires (expression);

						methodContractElement = requires;
					} else if (this.contract_nodes.IsPostCondition (calledMethod))
						methodContractElement = new Ensures (expression);

					if (methodContractElement == null)
						throw new InvalidOperationException ("Unrecognized contract method");

					if (methodCall.Arguments.Count > 1) {
						Expression userMessage = methodCall.Arguments [1];
						methodContractElement.UserMessage = userMessage;
					}

					switch (methodContractElement.NodeType) {
					case NodeType.Requires:
						var requires = (Requires) methodContractElement;
						preconditions.Add (requires);
						break;
					case NodeType.Ensures:
						var ensures = (Ensures) methodContractElement;
						postconditions.Add (ensures);
						break;
					}
				}
			}

			return true;
		}

		private bool FindLastBlockWithContracts (List<Statement> statements, int beginning,
		                                         out int lastBlockContainingContract, out int lastStatementContainingContract)
		{
			lastBlockContainingContract = -1;
			lastStatementContainingContract = -1;
			for (int i = statements.Count - 1; i >= beginning; i--) {
				var block = statements [i] as Block;
				if (block == null || block.Statements == null || block.Statements.Count <= 0)
					continue;

				for (int j = block.Statements.Count - 1; j >= 0; j--) {
					if (this.contract_nodes.IsContractCall (block.Statements [j]) == null)
						continue;

					lastBlockContainingContract = i;
					lastStatementContainingContract = j;
					return true;
				}
			}
			return false;
		}
	}
}
