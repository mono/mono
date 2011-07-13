// 
// HelperMethods.cs
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

namespace Mono.CodeContracts.Static.ContractExtraction {
	static class HelperMethods {
		public static Method IsMethodCall (Statement s)
		{
			if (s == null)
				return null;

			var expressionStatement = s as ExpressionStatement;
			if (expressionStatement == null)
				return null;

			var methodCall = expressionStatement.Expression as MethodCall;
			if (methodCall == null)
				return null;

			var binding = methodCall.Callee as MemberBinding;
			if (binding == null)
				return null;

			return binding.BoundMember as Method;
		}

		public static Local ExtractPreamble (Method method, ContractNodes contractNodes, Block contractInitializer, out Block postPreamble)
		{
			postPreamble = null;
			return null;
		}

		public static List<Statement> ExtractContractBlocks (List<Statement> blocks, int firstBlockIndex, int firstStmtIndex, int lastBlockIndex, int lastStmtIndex)
		{
			var result = new List<Statement> ();
			var firstBlock = (Block) blocks [firstBlockIndex];
			var block = new Block (new List<Statement> ());
			if (firstBlock != null) {
				int cnt = firstBlockIndex == lastBlockIndex ? lastStmtIndex + 1 : firstBlock.Statements.Count;
				for (int i = firstStmtIndex; i < cnt; i++) {
					Statement stmt = firstBlock.Statements [i];
					block.Statements.Add (stmt);
					if (stmt != null)
						firstBlock.Statements [i] = null;
				}
			}
			result.Add (block);
			int nextIndex = firstBlockIndex + 1;
			if (nextIndex > lastBlockIndex)
				return result;
			Block newLastBlock = null;
			int lastFullBlockIndex = lastBlockIndex - 1;
			var lastBlock = (Block) blocks [lastBlockIndex];
			if (lastBlock != null && lastStmtIndex == lastBlock.Statements.Count - 1)
				lastFullBlockIndex = lastBlockIndex;
			else {
				newLastBlock = new Block (new List<Statement> ());
				if (block.Statements != null && block.Statements.Count > 0) {
					var branch = block.Statements [block.Statements.Count - 1] as Branch;
					if (branch != null && branch.Target != null && branch.Target == lastBlock)
						branch.Target = newLastBlock;
				}
			}

			for (; nextIndex < lastFullBlockIndex; ++nextIndex) {
				var curBlock = (Block) blocks [nextIndex];
				result.Add (curBlock);
				if (curBlock != null) {
					blocks [nextIndex] = null;
					if (newLastBlock != null && curBlock.Statements != null && curBlock.Statements.Count > 0) {
						var branch = curBlock.Statements [curBlock.Statements.Count - 1] as Branch;
						if (branch != null && branch.Target != null && branch.Target == lastBlock)
							branch.Target = newLastBlock;
					}
				}
			}

			if (newLastBlock != null) {
				for (int i = 0; i < lastStmtIndex + 1; i++) {
					newLastBlock.Statements.Add (lastBlock.Statements [i]);
					lastBlock.Statements [i] = null;
				}

				result.Add (newLastBlock);
			}
			return result;
		}

		public static bool IsCompilerGenerated (TypeNode type)
		{
			throw new NotImplementedException ();
		}

		public static int FindNextRealStatement (List<Statement> stmts, int beginIndex)
		{
			if (stmts == null || stmts.Count <= beginIndex)
				return -1;
			int index = beginIndex;
			while (index < stmts.Count && (stmts [index] == null || stmts [index].NodeType == NodeType.Nop))
				++index;
			return index;
		}

		public static bool IsReferenceAsVisibleAs (Member member, Member asThisMember)
		{
			var type = member as TypeNode;
			if (type != null)
				return IsTypeAsVisibleAs (type, asThisMember);
			var method = member as Method;
			Member member1;
			if (method != null) {
				if (method.HasGenericParameters)
					throw new NotImplementedException ();
				member1 = method;
			} else
				member1 = Unspecialize (member);

			return IsDefinitionAsVisibleAs (member1, asThisMember);
		}

		private static bool IsDefinitionAsVisibleAs (this Member member, Member asThisMember)
		{
			Module memberModule = member.Module;
			Module asThisMemberModule = asThisMember.Module;

			for (Member mbr = member; mbr != null; mbr = mbr.DeclaringType) {
				if (!mbr.IsPublic) {
					bool visible = false;
					for (Member mbr1 = asThisMember; mbr1 != null; mbr1 = mbr1.DeclaringType) {
						if (mbr1.IsAssembly) {
							if ((mbr1.IsPrivate || mbr1.IsAssembly) && memberModule == asThisMemberModule)
								visible = true;
						} else if (mbr1.IsFamily) {
							if (mbr.IsPrivate) {
								if (IsInsideOf (mbr, mbr1) || IsInsideSubclass (mbr, mbr1.DeclaringType))
									visible = true;
							} else if (mbr.IsFamily && (mbr.DeclaringType == mbr1.DeclaringType || IsSubclassOf (mbr.DeclaringType, mbr1.DeclaringType)))
								visible = true;
						} else if (mbr1.IsFamilyOrAssembly) {
							if (mbr.IsPrivate) {
								if (memberModule == asThisMemberModule || IsInsideSubclass (mbr, mbr1.DeclaringType))
									visible = true;
							} else if (mbr.IsAssembly) {
								if (memberModule == asThisMemberModule)
									visible = true;
							} else if (mbr.IsFamily) {
								if (IsSubclassOf (mbr.DeclaringType, mbr1.DeclaringType))
									visible = true;
							} else if (mbr.IsFamilyOrAssembly && memberModule == asThisMemberModule && IsSubclassOf (mbr.DeclaringType, mbr1.DeclaringType))
								visible = true;
						} else if (mbr1.IsPrivate && mbr.IsPrivate && IsInsideOf (mbr, mbr1.DeclaringType))
							visible = true;
					}
					if (!visible)
						return false;
				}
			}
			return true;
		}

		private static bool IsSubclassOf (this TypeNode thisType, TypeNode thatType)
		{
			if (thatType == null)
				return false;
			return thisType.IsAssignableTo (thatType);
		}

		private static bool IsInsideSubclass (this Member member, Member thatValue)
		{
			var targetType = thatValue as TypeNode;
			if (targetType == null)
				return false;

			for (TypeNode declaringType = member.DeclaringType; declaringType != null; declaringType = declaringType.DeclaringType) {
				if (declaringType.IsAssignableTo (targetType))
					return true;
			}
			return false;
		}

		private static bool IsInsideOf (this Member thisValue, Member thatValue)
		{
			var typeNode = thatValue as TypeNode;
			if (typeNode == null)
				return false;
			for (TypeNode declaringType = thisValue.DeclaringType; declaringType != null; declaringType = declaringType.DeclaringType) {
				if (declaringType == typeNode)
					return true;
			}
			return false;
		}

		private static Member Unspecialize (Member member)
		{
			return member;
		}

		private static bool IsTypeAsVisibleAs (this TypeNode type, Member asThisMember)
		{
			if (type == null)
				return true;

			switch (type.NodeType) {
			case NodeType.Reference:
				return ((Reference) type).ElementType.IsTypeAsVisibleAs (asThisMember);
			default:
				if (type.HasGenericParameters)
					throw new NotImplementedException ();

				return IsDefinitionAsVisibleAs (type, asThisMember);
			}
		}

		public static bool IsVisibleFrom (this TypeNode type, TypeNode from)
		{
			TypeNode declaringType = type.DeclaringType;
			if (declaringType != null) {
				if (IsContainedIn (from, declaringType) || IsInheritedFrom (from, type))
					return true;
				if (type.IsNestedFamily)
					return IsInheritedFrom (from, declaringType);
				if (type.IsNestedPublic)
					return IsVisibleFrom (declaringType, from);
				if (type.IsNestedInternal) {
					if (IsInheritedFrom (from, declaringType))
						return true;
					if (declaringType.Module == from.Module)
						return IsVisibleFrom (declaringType, from);

					return false;
				}
				if (type.IsNestedFamilyAndAssembly)
					return from.Module == declaringType.Module && IsInheritedFrom (from, declaringType);
				if ((type.IsAssembly || type.IsNestedAssembly) && declaringType.Module == from.Module)
					return IsVisibleFrom (declaringType, from);

				return false;
			}

			return type.Module == from.Module || type.IsPublic;
		}

		public static bool IsVisibleFrom (this Member member, TypeNode from)
		{
			var type = member as TypeNode;
			if (type != null)
				return type.IsVisibleFrom (from);

			TypeNode declaringType = member.DeclaringType;
			if (from.IsContainedIn (declaringType))
				return true;
			if (member.IsPublic)
				return declaringType.IsVisibleFrom (from);
			if (member.IsFamily)
				return from.IsInheritedFrom (declaringType);
			if (member.IsFamilyAndAssembly)
				return from.Module == declaringType.Module && from.IsInheritedFrom (declaringType);
			if (member.IsFamilyOrAssembly) {
				if (from.IsInheritedFrom (declaringType))
					return true;
				if (from.Module == declaringType.Module)
					return declaringType.IsVisibleFrom (from);

				return false;
			}

			return member.IsAssembly && declaringType.Module == from.Module && declaringType.IsVisibleFrom (from);
		}

		public static bool IsInheritedFrom (this TypeNode type, TypeNode from)
		{
			TypeNode baseClass;
			if (type.HasBaseClass (out baseClass)) {
				if (baseClass == from || baseClass.IsInheritedFrom (from))
					return true;
			}

			return false;
		}

		private static bool HasBaseClass (this TypeNode type, out TypeNode baseClass)
		{
			var clazz = type as Class;
			if (clazz != null && clazz.BaseType != null) {
				baseClass = clazz.BaseType;
				return true;
			}

			baseClass = default(TypeNode);
			return false;
		}

		public static bool IsContainedIn (this TypeNode inner, TypeNode outer)
		{
			if (inner == outer)
				return true;

			if (inner.DeclaringType != null)
				return inner.DeclaringType.IsContainedIn (outer);

			return false;
		}
	}
}
