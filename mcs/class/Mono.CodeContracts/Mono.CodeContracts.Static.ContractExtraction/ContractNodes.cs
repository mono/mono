// 
// ContractNodes.cs
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
using System.Reflection;
using Mono.CodeContracts.Static.AST;

namespace Mono.CodeContracts.Static.ContractExtraction {
	class ContractNodes {
		public static readonly string ContractNamespace = "System.Diagnostics.Contracts";
		public static readonly string ContractClassName = "Contract";
		public static readonly string RequiresName = "Requires";
		public static readonly string EnsuresName = "Ensures";
		public static readonly string AssertName = "Assert";
		public static readonly string AssumeName = "Assume";
		public static readonly string EndContractBlockName = "EndContractBlock";

		[RepresentationFor ("Contract.Assert(bool)")]
		public readonly Method AssertMethod;

		[RepresentationFor ("Contract.Assert(bool, string)")]
		public readonly Method AssertWithMessageMethod;

		[RepresentationFor ("Contract.Assume(bool)")]
		public readonly Method AssumeMethod;

		[RepresentationFor ("Contract.Assume(bool, string)")]
		public readonly Method AssumeWithMessageMethod;

		[RepresentationFor ("System.Diagnostics.Contracts.Contract")]
		public readonly Class ContractClass;

		[RepresentationFor ("Contract.EndContractBlock()")]
		public readonly Method EndContractBlock;

		[RepresentationFor ("Contract.Ensures(bool)")]
		public readonly Method EnsuresMethod;

		[RepresentationFor ("Contract.Ensures(bool, string)")]
		public readonly Method EnsuresWithMessageMethod;

		[RepresentationFor ("Contract.Requires(bool)")]
		public readonly Method RequiresMethod;

		[RepresentationFor ("Contract.Requires(bool, string)")]
		public readonly Method RequiresWithMessageMethod;

		private ContractNodes (AssemblyNode assembly, Action<string> errorHandler)
		{
			CoreSystemTypes.ModuleDefinition = assembly.Modules.First ().Definition;
			if (errorHandler != null)
				ErrorFound += errorHandler;
			this.ContractClass = assembly.GetType (ContractNamespace, ContractClassName) as Class;
			if (this.ContractClass == null)
				return;


			IEnumerable<Method> methods = this.ContractClass.GetMethods (RequiresName, CoreSystemTypes.Instance.TypeBoolean);
			foreach (Method method in methods) {
				if (method.GenericParameters == null || method.GenericParameters.Count == 0)
					this.RequiresMethod = method;
			}

			if (this.RequiresMethod == null) {
				this.ContractClass = null;
				return;
			}

			methods = this.ContractClass.GetMethods (RequiresName, CoreSystemTypes.Instance.TypeBoolean, CoreSystemTypes.Instance.TypeString);
			foreach (Method method in methods) {
				if (method.GenericParameters == null || method.GenericParameters.Count == 0)
					this.RequiresWithMessageMethod = method;
			}
			this.EnsuresMethod = this.ContractClass.GetMethod (EnsuresName, CoreSystemTypes.Instance.TypeBoolean);
			this.EnsuresWithMessageMethod = this.ContractClass.GetMethod (EnsuresName,
			                                                              CoreSystemTypes.Instance.TypeBoolean, CoreSystemTypes.Instance.TypeString);

			this.AssertMethod = this.ContractClass.GetMethod (AssertName, CoreSystemTypes.Instance.TypeBoolean);
			this.AssertWithMessageMethod = this.ContractClass.GetMethod (AssertName,
			                                                             CoreSystemTypes.Instance.TypeBoolean, CoreSystemTypes.Instance.TypeString);

			this.AssumeMethod = this.ContractClass.GetMethod (AssumeName, CoreSystemTypes.Instance.TypeBoolean);
			this.AssumeWithMessageMethod = this.ContractClass.GetMethod (AssumeName,
			                                                             CoreSystemTypes.Instance.TypeBoolean, CoreSystemTypes.Instance.TypeString);

			this.EndContractBlock = this.ContractClass.GetMethod (EndContractBlockName);

			foreach (FieldInfo fieldInfo in typeof (ContractNodes).GetFields ()) {
				if (fieldInfo.GetValue (this) != null)
					continue;

				string runtimeName = null;
				bool isRequired = false;
				object[] attributes = fieldInfo.GetCustomAttributes (typeof (RepresentationForAttribute), false);
				foreach (object attribute in attributes) {
					var representationForAttribute = attribute as RepresentationForAttribute;
					if (representationForAttribute != null) {
						runtimeName = representationForAttribute.RuntimeName;
						isRequired = representationForAttribute.IsRequired;
						break;
					}
				}
				if (isRequired) {
					string message = string.Format ("Could not find contract node for '{0}'", fieldInfo.Name);
					if (runtimeName != null)
						message = string.Format ("Could not find the method/type '{0}'", runtimeName);

					FireErrorFound (message);
					ClearFields ();
				}
			}
		}

		public static ContractNodes GetContractNodes (AssemblyNode assembly, Action<string> errorHandler)
		{
			var contractNodes = new ContractNodes (assembly, errorHandler);
			if (contractNodes.ContractClass != null)
				return contractNodes;
			return null;
		}

		private void ClearFields ()
		{
			foreach (FieldInfo fieldInfo in typeof (ContractNodes).GetFields ()) {
				object[] customAttributes = fieldInfo.GetCustomAttributes (typeof (RepresentationForAttribute), false);
				if (customAttributes.Length == 1)
					fieldInfo.SetValue (this, null);
			}
		}

		public event Action<string> ErrorFound;

		private void FireErrorFound (string message)
		{
			if (ErrorFound == null)
				throw new InvalidOperationException (message);

			ErrorFound (message);
		}

		public Method IsContractCall (Statement s)
		{
			Method m = HelperMethods.IsMethodCall (s);
			if (IsContractMethod (m))
				return m;

			return null;
		}

		public bool IsContractMethod (Method method)
		{
			if (method == null)
				return false;
			if (IsPlainPrecondition (method) || IsPostCondition (method) || IsEndContractBlock (method))
				return true;

			return false;
		}

		public bool IsPostCondition (Method method)
		{
			TypeNode genericArgument;
			return IsContractMethod (EnsuresName, method, out genericArgument) && genericArgument == null;
		}

		public bool IsPlainPrecondition (Method method)
		{
			TypeNode genericArgument;
			return IsContractMethod (RequiresName, method, out genericArgument);
		}

		private bool IsContractMethod (string methodName, Method m, out TypeNode genericArgument)
		{
			genericArgument = null;
			if (m == null)
				return false;
			if (m.HasGenericParameters) {
				if (m.GenericParameters == null || m.GenericParameters.Count != 1)
					return false;
				genericArgument = m.GenericParameters [0];
			}

			return m.Name != null && m.Name == methodName &&
			       (m.DeclaringType.Equals (this.ContractClass)
			        || (m.Parameters != null && m.Parameters.Count == 3 && m.DeclaringType != null && m.DeclaringType.Name != ContractClassName));
		}

		public bool IsEndContractBlock (Method method)
		{
			TypeNode dummy;
			return IsContractMethod (EndContractBlockName, method, out dummy);
		}
	}
}
