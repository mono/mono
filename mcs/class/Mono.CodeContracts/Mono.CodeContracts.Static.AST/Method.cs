// 
// Method.cs
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
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

namespace Mono.CodeContracts.Static.AST {
	class Method : Member, IEquatable<Method> {
		#region Delegates
		public delegate void MethodContractProvider (Method method);
		#endregion

		private readonly MethodDefinition definition;
		private Block block;
		private MethodContract contract;
		private MethodContractProvider method_contract_provider;
		private List<Parameter> parameters;
		private TypeNode returnType;
		private This thisParameter;

		public Method (MethodDefinition definition)
			: base (NodeType.Method)
		{
			this.definition = definition;
		}

		private Method (MethodDefinition definition, Block block) : base (NodeType.Method)
		{
			this.definition = definition;
			this.block = block;
		}

		public bool HasGenericParameters
		{
			get { return this.definition.HasGenericParameters; }
		}

		public MethodDefinition Definition
		{
			get { return this.definition; }
		}

		public override TypeNode DeclaringType
		{
			get { return TypeNode.Create (this.definition.DeclaringType); }
		}

		public override Module Module
		{
			get { return new Module (this.definition.Module); }
		}

		public Method OverriddenMethod
		{
			get
			{
				if (!this.definition.HasOverrides)
					return null;
				return ParseMethodDefinition (this.definition.Overrides [0].Resolve ());
			}
		}

		public Block Body
		{
			get
			{
				if (this.block == null)
					this.block = ParseMethodBlock (this.definition);
				return this.block;
			}
			set { this.block = value; }
		}

		public MethodContract MethodContract
		{
			get
			{
				if (this.contract == null && ContractProvider != null) {
					MethodContractProvider provider = ContractProvider;
					ContractProvider = null;
					provider (this);
				}
				return this.contract;
			}
			set
			{
				this.contract = value;
				if (value != null)
					this.contract.DeclaringMethod = this;
				ContractProvider = null;
			}
		}

		public MethodContractProvider ContractProvider
		{
			get { return this.method_contract_provider; }
			set
			{
				if (value == null) {
					this.method_contract_provider = null;
					return;
				}

				if (this.method_contract_provider != null)
					this.method_contract_provider += value;
				else
					this.method_contract_provider = value;

				this.contract = null;
			}
		}


		public bool IsFinal
		{
			get { return this.definition.IsFinal; }
		}

		public bool HasBody
		{
			get { return this.definition.HasBody; }
		}

		public override bool IsPrivate
		{
			get { return this.definition.IsPrivate; }
		}

		public override bool IsAssembly
		{
			get { return this.definition.IsAssembly; }
		}

		public override bool IsFamily
		{
			get { return this.definition.IsFamily; }
		}

		public override bool IsFamilyOrAssembly
		{
			get { return this.definition.IsFamilyOrAssembly; }
		}

		public override bool IsFamilyAndAssembly
		{
			get { return this.definition.IsFamilyAndAssembly; }
		}

		public override bool IsPublic
		{
			get { return this.definition.IsPublic; }
		}

		public bool IsProtected
		{
			get { return this.definition.IsFamily; }
		}

		public bool IsProtectedOrInternal
		{
			get { return this.definition.IsFamilyOrAssembly; }
		}

		public bool IsProtectedAndInternal
		{
			get { return this.definition.IsFamilyAndAssembly; }
		}

		public string Name
		{
			get { return this.definition.Name; }
		}

		public string FullName
		{
			get { return this.definition.FullName; }
		}

		public bool HasOverrides
		{
			get { return this.definition.HasOverrides; }
		}

		public bool IsVirtual
		{
			get { return this.definition.IsVirtual; }
		}

		public override bool IsStatic
		{
			get { return this.definition.IsStatic; }
		}

		public bool IsNewSlot
		{
			get { return this.definition.IsNewSlot; }
		}

		public bool IsAbstract
		{
			get { return this.definition.IsAbstract; }
		}

		public bool IsConstructor
		{
			get { return this.definition.IsConstructor; }
		}

		public List<Parameter> Parameters
		{
			get
			{
				if (this.parameters == null)
					this.parameters = this.definition.Parameters.Select (i => new Parameter (i)).ToList ();
				return this.parameters;
			}
			set { this.parameters = value; }
		}

		public bool HasParameters
		{
			get { return Parameters != null && Parameters.Count > 0; }
		}

		public TypeNode ReturnType
		{
			get
			{
				if (this.returnType == null)
					this.returnType = TypeNode.Create (this.definition.ReturnType);
				return this.returnType;
			}
			set { this.returnType = value; }
		}

		public bool IsSetter
		{
			get { return this.definition.IsSetter; }
		}

		public bool IsGetter
		{
			get { return this.definition.IsGetter; }
		}

		public This ThisParameter
		{
			get
			{
				if (this.thisParameter == null && !IsStatic && DeclaringType != null)
					ThisParameter = !DeclaringType.IsValueType ? new This (DeclaringType.SelfInstantiation ()) : new This (DeclaringType.SelfInstantiation ().GetReferenceType ());
				return this.thisParameter;
			}
			private set
			{
				this.thisParameter = value;
				if (value != null)
					this.thisParameter.DeclaringMethod = this;
			}
		}

		public Method DeclaringMethod { get; private set; }

		public List<TypeNode> GenericParameters
		{
			get
			{
				Collection<GenericParameter> genericParameters = this.definition.GenericParameters;
				if (genericParameters == null)
					return null;
				return genericParameters.Select (it => TypeNode.Create (it)).ToList ();
			}
		}

		public IList<Local> Locals
		{
			get
			{
				Collection<VariableDefinition> variables = this.definition.Body.Variables;
				if (variables == null)
					return null;
				return variables.Select (it => new Local (it)).ToList ();
			}
		}

		public bool IsCompilerGenerated
		{
			get { return this.definition.IsCompilerControlled; }
		}

		#region IEquatable<Method> Members
		public bool Equals (Method other)
		{
			return this.definition == other.definition;
		}
		#endregion

		public static Method ParseMethodDefinition (MethodDefinition methodDefinition)
		{
			Block methodBlock = ParseMethodBlock (methodDefinition);

			return new Method (methodDefinition, methodBlock);
		}

		private static Block ParseMethodBlock (MethodDefinition methodDefinition)
		{
			var bp = new BodyParser (methodDefinition);
			return new Block (bp.ParseBlocks ());
		}

		public override string ToString ()
		{
			return string.Format ("Method(Name: {0})", FullName);
		}
	}
}
