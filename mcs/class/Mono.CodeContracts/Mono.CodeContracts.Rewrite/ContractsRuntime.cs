//
// ContractRuntime.cs
//
// Authors:
//	Chris Bacon (chrisbacon76@gmail.com)
//
// Copyright (C) 2010 Chris Bacon
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
using System.Diagnostics.Contracts;
using Mono.Cecil.Cil;
using System.Diagnostics.Contracts.Internal;
using System.Diagnostics;
using System.Runtime.ConstrainedExecution;
using System.Runtime.CompilerServices;

namespace Mono.CodeContracts.Rewrite {
	class ContractsRuntime {

		private const string Namespace = "System.Diagnostics.Contracts";

		public ContractsRuntime (ModuleDefinition module, RewriterOptions options)
		{
			this.module = module;
			this.options = options;
		}

		private ModuleDefinition module;
		private RewriterOptions options;

		private TypeDefinition typeContractsRuntime = null;
		private TypeDefinition typeContractException = null;
		private MethodDefinition methodContractExceptionCons = null;
		private MethodDefinition methodTriggerFailure = null;
		private MethodDefinition methodReportFailure = null;
		private MethodDefinition methodRequires = null;

		private void EnsureTypeContractRuntime ()
		{
			if (this.typeContractsRuntime == null) {
				// namespace System.Diagnostics.Contracts {
				//     [CompilerGenerated]
				//     private static class __ContractsRuntime {
				//     }
				// }
				
				// Create type
				TypeReference typeObject = this.module.Import (typeof (object));
				TypeDefinition type = new TypeDefinition ("__ContractsRuntime", Namespace,
					TypeAttributes.Abstract | TypeAttributes.Sealed | TypeAttributes.NotPublic | TypeAttributes.AnsiClass | TypeAttributes.AutoClass, // | TypeAttributes.BeforeFieldInit,
					typeObject);
				this.module.Types.Add (type);
				// Attach custom attributes
				var attrCompilerGeneratedCons = typeof (CompilerGeneratedAttribute).GetConstructor (Type.EmptyTypes);
				CustomAttribute attrCompilerGenerated = new CustomAttribute (this.module.Import (attrCompilerGeneratedCons));
				type.CustomAttributes.Add (attrCompilerGenerated);
				// Store type
				this.typeContractsRuntime = type;
			}
		}

		private void EnsureTypeContractException ()
		{
			if (this.options.ThrowOnFailure && this.typeContractException == null) {
				// [CompilerGenerated]
				// private class ContractException : Exception {
				//     internal ContractException(ContractFailureKind kind, string usermsg, string condition, Exception inner)
				//         : base(failure, inner)
				//     {
				//     }
				// }
				
				// Prepare type references
				TypeReference typeVoid = this.module.Import (typeof (void));
				TypeReference typeContractFailureKind = this.module.Import (typeof (ContractFailureKind));
				TypeReference typeString = this.module.Import (typeof (string));
				TypeReference typeException = this.module.Import (typeof (Exception));
				// Create type
				TypeDefinition type = new TypeDefinition ("ContractException", Namespace,
					TypeAttributes.NestedPrivate | TypeAttributes.AnsiClass | TypeAttributes.AutoClass, typeException);
				//this.typeContractsRuntime.NestedTypes.Add (type);
				this.module.Types.Add(type);
				// Create constructor
				MethodDefinition cons = new MethodDefinition (".ctor",
					MethodAttributes.Assembly | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, typeVoid);
				cons.Parameters.Add (new ParameterDefinition ("kind", ParameterAttributes.None, typeContractFailureKind));
				cons.Parameters.Add (new ParameterDefinition ("failure", ParameterAttributes.None, typeString));
				cons.Parameters.Add (new ParameterDefinition ("usermsg", ParameterAttributes.None, typeString));
				cons.Parameters.Add (new ParameterDefinition ("condition", ParameterAttributes.None, typeString));
				cons.Parameters.Add (new ParameterDefinition ("inner", ParameterAttributes.None, typeException));
				var il = cons.Body.GetILProcessor ();
				il.Emit (OpCodes.Ldarg_0);
				il.Emit (OpCodes.Ldarg_2);
				il.Emit (OpCodes.Ldarg_S, cons.Parameters [4]);
				MethodReference mExceptionCons = this.module.Import (typeof (Exception).GetConstructor (new [] { typeof (string), typeof (Exception) }));
				il.Emit (OpCodes.Call, mExceptionCons);
				il.Emit (OpCodes.Ret);
				type.Methods.Add (cons);
				// Attach custom attributes
				var attrCompilerGeneratedCons = typeof (CompilerGeneratedAttribute).GetConstructor (Type.EmptyTypes);
				CustomAttribute attrCompilerGenerated = new CustomAttribute (this.module.Import (attrCompilerGeneratedCons));
				type.CustomAttributes.Add (attrCompilerGenerated);
				// Store constructor and type
				this.methodContractExceptionCons = cons;
				this.typeContractException = type;
			}
		}

		private void EnsureMethodTriggerFailure ()
		{
			if (this.methodTriggerFailure == null) {
				// if the ThrowOnFailure option is true, then:
				// internal static void TriggerFailure(ContractFailureKind kind, string message, string userMessage, string conditionText, Exception inner)
				// {
				//     throw new ContractException(kind, message, userMessage, conditionText, inner);
				// }
				
				// if the ThrowOnFailure option is false, then:
				// internal static void TriggerFailure(ContractFailureKind kind, string message, string userMessage, string conditionText, Exception inner)
				// {
				//     Debug.Fail(message, userMessage);
				// }
				
				// Prepare type references
				TypeReference typeVoid = this.module.Import (typeof (void));
				TypeReference typeContractFailureKind = this.module.Import (typeof (ContractFailureKind));
				TypeReference typeString = this.module.Import (typeof (string));
				TypeReference typeException = this.module.Import (typeof (Exception));
				// Create method
				MethodDefinition method = new MethodDefinition ("TriggerFailure",
					MethodAttributes.Assembly | MethodAttributes.Static, typeVoid);
				method.Parameters.Add (new ParameterDefinition ("kind", ParameterAttributes.None, typeContractFailureKind));
				method.Parameters.Add (new ParameterDefinition ("message", ParameterAttributes.None, typeString));
				method.Parameters.Add (new ParameterDefinition ("userMessage", ParameterAttributes.None, typeString));
				method.Parameters.Add (new ParameterDefinition ("conditionText", ParameterAttributes.None, typeString));
				method.Parameters.Add (new ParameterDefinition ("inner", ParameterAttributes.None, typeException));
				var il = method.Body.GetILProcessor ();
				if (this.options.ThrowOnFailure) {
					il.Emit (OpCodes.Ldarg_0);
					il.Emit (OpCodes.Ldarg_1);
					il.Emit (OpCodes.Ldarg_2);
					il.Emit (OpCodes.Ldarg_3);
					il.Emit (OpCodes.Ldarg_S, method.Parameters [4]);
					il.Emit (OpCodes.Newobj, this.methodContractExceptionCons);
					il.Emit (OpCodes.Throw);
				} else {
					var mDebugFail = typeof (Debug).GetMethod ("Fail", new [] { typeof (string), typeof(string) });
					MethodReference methodDebugFail = this.module.Import (mDebugFail);
					il.Emit (OpCodes.Ldarg_1);
					il.Emit (OpCodes.Ldarg_2);
					il.Emit (OpCodes.Call, methodDebugFail);
					il.Emit (OpCodes.Ret);
				}
				this.typeContractsRuntime.Methods.Add (method);
				this.methodTriggerFailure = method;
			}
		}

		private void EnsureMethodReportFailure ()
		{
			if (this.methodReportFailure == null) {
				// internal static void ReportFailure(ContractFailureKind kind, string message, string conditionText, Exception inner)
				// {
				//     string s = ContractHelper.RaiseContractFailedEvent(kind, message, conditionText, inner);
				//     if (s != null) {
				//         TriggerFailure(kind, s, message, conditionText, inner);
				//     }
				// }
				
				// Prepare type references
				TypeReference typeVoid = this.module.Import (typeof (void));
				TypeReference typeContractFailureKind = this.module.Import (typeof (ContractFailureKind));
				TypeReference typeString = this.module.Import (typeof (string));
				TypeReference typeException = this.module.Import (typeof (Exception));
				MethodReference mRaiseContractFailedEvent = this.module.Import (typeof (System.Runtime.CompilerServices.ContractHelper).GetMethod ("RaiseContractFailedEvent"));
				// Create method
				MethodDefinition method = new MethodDefinition ("ReportFailure",
					MethodAttributes.Assembly | MethodAttributes.Static, typeVoid);
				method.Parameters.Add (new ParameterDefinition ("kind", ParameterAttributes.None, typeContractFailureKind));
				method.Parameters.Add (new ParameterDefinition ("message", ParameterAttributes.None, typeString));
				method.Parameters.Add (new ParameterDefinition ("conditionText", ParameterAttributes.None, typeString));
				method.Parameters.Add (new ParameterDefinition ("inner", ParameterAttributes.None, typeException));
				VariableDefinition vMsg = new VariableDefinition ("sMsg", typeString);
				method.Body.Variables.Add (vMsg);
				method.Body.InitLocals = true;
				var il = method.Body.GetILProcessor ();
				il.Emit (OpCodes.Ldarg_0);
				il.Emit (OpCodes.Ldarg_1);
				il.Emit (OpCodes.Ldarg_2);
				il.Emit (OpCodes.Ldarg_3);
				il.Emit (OpCodes.Call, mRaiseContractFailedEvent);
				il.Emit (OpCodes.Stloc_0);
				il.Emit (OpCodes.Ldloc_0);
				var instRet = il.Create (OpCodes.Ret);
				il.Emit (OpCodes.Brfalse_S, instRet);
				il.Emit (OpCodes.Ldarg_0);
				il.Emit (OpCodes.Ldloc_0);
				il.Emit (OpCodes.Ldarg_1);
				il.Emit (OpCodes.Ldarg_2);
				il.Emit (OpCodes.Ldarg_3);
				il.Emit (OpCodes.Call, this.methodTriggerFailure);
				il.Append (instRet);
				this.typeContractsRuntime.Methods.Add (method);
				this.methodReportFailure = method;
			}
		}

		private void EnsureGlobal ()
		{
			this.EnsureTypeContractRuntime ();
			this.EnsureTypeContractException ();
			this.EnsureMethodTriggerFailure ();
			this.EnsureMethodReportFailure ();
		}

		public MethodDefinition GetRequires ()
		{
			this.EnsureGlobal ();
			if (this.methodRequires == null) {
				// [DebuggerNonUserCode]
				// [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
				// internal static void Requires(bool condition, string message, string conditionText)
				// {
				//     if (!condition) {
				//         ReportFailure(ContractFailureKind.Precondition, message, conditionText, null);
				//     }
				// }
				
				// Prepare type references
				TypeReference typeVoid = this.module.Import (typeof (void));
				TypeReference typeBoolean = this.module.Import (typeof (bool));
				TypeReference typeString = this.module.Import (typeof (string));
				// Create method
				MethodDefinition method = new MethodDefinition ("Requires",
				    MethodAttributes.Assembly | MethodAttributes.Static, typeVoid);
				method.Parameters.Add (new ParameterDefinition ("condition", ParameterAttributes.None, typeBoolean));
				method.Parameters.Add (new ParameterDefinition ("message", ParameterAttributes.None, typeString));
				method.Parameters.Add (new ParameterDefinition ("conditionText", ParameterAttributes.None, typeString));
				var il = method.Body.GetILProcessor ();
				il.Emit (OpCodes.Ldarg_0);
				var instRet = il.Create(OpCodes.Ret);
				il.Emit (OpCodes.Brtrue_S, instRet);
				il.Emit (OpCodes.Ldc_I4_0); // Assumes ContractFailureKind.Precondition == 0
				il.Emit (OpCodes.Ldarg_1);
				il.Emit (OpCodes.Ldarg_2);
				il.Emit (OpCodes.Ldnull);
				il.Emit (OpCodes.Call, this.methodReportFailure);
				il.Append (instRet);
				this.typeContractsRuntime.Methods.Add (method);
				// Attach custom attributes
				var attrDebugNonUserCodeCons = typeof (DebuggerNonUserCodeAttribute).GetConstructor (Type.EmptyTypes);
				CustomAttribute attrDebugNonUserCode = new CustomAttribute (this.module.Import (attrDebugNonUserCodeCons));
				method.CustomAttributes.Add (attrDebugNonUserCode);
				var attrReliabilityContractCons = typeof (ReliabilityContractAttribute).GetConstructor (new [] { typeof (Consistency), typeof (Cer) });
				// Blob for attribute: new ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)
				byte [] blob = new byte [] { 1, 0, 3, 0, 0, 0, 1, 0, 0, 0 };
				CustomAttribute attrReliabilityContract = new CustomAttribute (this.module.Import (attrReliabilityContractCons), blob);
				method.CustomAttributes.Add (attrReliabilityContract);
				// Store method
				this.methodRequires = method;
			}
			return this.methodRequires;
		}

	}
}
