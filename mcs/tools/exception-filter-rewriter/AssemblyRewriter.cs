using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

namespace ExceptionRewriter {
	public class RewriteOptions {
		public bool Mono = false;
		public bool EnableGenerics = false;
		public bool Verbose = false;
		public bool Audit = false;
		public bool AbortOnError = true;
		public bool EnableSymbols = false;
		public bool Mark = false;
		internal bool Overwrite;
	}

	public class AssemblyRewriter {
		public readonly RewriteOptions Options;
		public readonly AssemblyDefinition Assembly;

		private int ClosureIndex, FilterIndex;

		private readonly Dictionary<Code, OpCode> ShortFormRemappings = new Dictionary<Code, OpCode> ();
		private readonly Dictionary<Code, OpCode> Denumberings = new Dictionary<Code, OpCode> {
			{Code.Ldarg_0, OpCodes.Ldarg },
			{Code.Ldarg_1, OpCodes.Ldarg },
			{Code.Ldarg_2, OpCodes.Ldarg },
			{Code.Ldarg_3, OpCodes.Ldarg },
			{Code.Ldloc_0, OpCodes.Ldloc },
			{Code.Ldloc_1, OpCodes.Ldloc },
			{Code.Ldloc_2, OpCodes.Ldloc },
			{Code.Ldloc_3, OpCodes.Ldloc },
			{Code.Stloc_0, OpCodes.Stloc },
			{Code.Stloc_1, OpCodes.Stloc },
			{Code.Stloc_2, OpCodes.Stloc },
			{Code.Stloc_3, OpCodes.Stloc },
			{Code.Ldarg_S, OpCodes.Ldarg },
			{Code.Ldarga_S, OpCodes.Ldarga },
			{Code.Starg_S, OpCodes.Starg },
			{Code.Ldloc_S, OpCodes.Ldloc },
			{Code.Ldloca_S, OpCodes.Ldloca }
		};
		private readonly Dictionary<Code, OpCode> LocalParameterRemappings = new Dictionary<Code, OpCode> {
			{Code.Ldloc, OpCodes.Ldarg },
			{Code.Ldloca, OpCodes.Ldarga },
			{Code.Ldloc_S, OpCodes.Ldarg },
			{Code.Ldloca_S, OpCodes.Ldarga },
			{Code.Stloc, OpCodes.Starg },
			{Code.Stloc_S, OpCodes.Starg },
			{Code.Ldarg, OpCodes.Ldloc },
			{Code.Ldarga, OpCodes.Ldloca },
			{Code.Ldarg_S, OpCodes.Ldloc },
			{Code.Ldarga_S, OpCodes.Ldloca },
			{Code.Starg, OpCodes.Stloc },
			{Code.Starg_S, OpCodes.Stloc }
		};

		private Dictionary<MethodDefinition, ClosureInfo> ClosureInfos = new Dictionary<MethodDefinition, ClosureInfo> ();

		public AssemblyRewriter (AssemblyDefinition assembly, RewriteOptions options)
		{
			Assembly = assembly;
			Options = options;

			var tOpcodes = typeof (OpCodes);

			// Table to convert Br_S, Brfalse_S, etc into full-length forms
			//  because if you don't do this mono.cecil will silently generate bad IL
			foreach (var n in typeof (Code).GetEnumNames ()) {
				if (!n.EndsWith ("_S"))
					continue;
				if (n.StartsWith ("Ld") || n.StartsWith ("St"))
					continue;

				var full = n.Replace ("_S", "");
				var m = tOpcodes.GetField (full);
				ShortFormRemappings[(Code)Enum.Parse (typeof (Code), n)] = (OpCode)m.GetValue (null);
			}
		}

		// The encouraged typeof() based import isn't valid because it will import
		//  netcore corelib types into netframework apps and vice-versa
		private TypeReference ImportCorlibType (ModuleDefinition module, string @namespace, string name)
		{
			foreach (var m in Assembly.Modules) {
				var ts = m.TypeSystem;
				// Cecil uses this API internally to lookup corlib types by name before exposing them in TypeSystem
				var mLookup = ts.GetType ().GetMethod ("LookupType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				var result = mLookup.Invoke (ts, new object[] { @namespace, name });
				if (result != null)
					return module.ImportReference ((TypeReference)result);
			}

			return null;
		}

		// Locate an existing assembly reference to the specified assembly, then reference the
		//  specified type by name from that assembly and import it
		private TypeReference ImportReferencedType (ModuleDefinition module, string assemblyName, string @namespace, string name)
		{
			var s = module.TypeSystem.String;

			foreach (var m in Assembly.Modules) {
				foreach (var ar in m.AssemblyReferences) {
					if (!ar.FullName.Contains (assemblyName))
						continue;

					var ad = Assembly.MainModule.AssemblyResolver.Resolve (ar);

					var result = new TypeReference (
						@namespace, name, ad.MainModule, ad.MainModule
					);
					return module.ImportReference (result);
				}
			}

			return null;
		}

		private TypeReference GetExceptionFilter (ModuleDefinition module, bool autoAddReference = true)
		{
			TypeReference result;

			if (Options.Mono) {
				result = ImportCorlibType (module, "Mono", "ExceptionFilter");
				if (result != null)
					return result;
			}

			result = ImportReferencedType (module, "ExceptionFilterSupport", "Mono", "ExceptionFilter");
			if (result == null) {
				if (!autoAddReference)
					throw new Exception ("ExceptionFilterSupport is not referenced");

				var anr = new AssemblyNameReference ("ExceptionFilterSupport", new Version (1, 0, 0, 0));
				module.AssemblyReferences.Add (anr);
				return GetExceptionFilter (module, false);
			}

			return result;
		}

		private TypeReference GetException (ModuleDefinition module)
		{
			return ImportCorlibType (module, "System", "Exception");
		}

		private TypeReference GetExceptionDispatchInfo (ModuleDefinition module)
		{
			return ImportCorlibType (module, "System.Runtime.ExceptionServices", "ExceptionDispatchInfo");
		}

		public int Rewrite ()
		{
			int errorCount = 0;

			foreach (var mod in Assembly.Modules) {
				// Make temporary copy of the types and methods lists because we mutate them while iterating
				foreach (var type in mod.GetTypes ()) {
					var queue = new Queue<MethodDefinition> (type.Methods);
					while (queue.Count > 0) {
						var meth = queue.Dequeue ();
						errorCount += RewriteMethod (meth, queue);
					}
				}
			}

			return errorCount;
		}

		private Instruction[] MakeDefault (
			TypeReference t,
			Dictionary<TypeReference, VariableDefinition> tempLocals
		)
		{
			if (t.FullName == "System.Void")
				return new Instruction[0];

			if (t.IsByReference || !t.IsValueType)
				return new[] { Instruction.Create (OpCodes.Ldnull) };

			switch (t.FullName) {
				case "System.Int32":
				case "System.UInt32":
				case "System.Boolean":
					return new[] { Instruction.Create (OpCodes.Ldc_I4_0) };
				default:
					VariableDefinition tempLocal;
					if (!tempLocals.TryGetValue (t, out tempLocal)) {
						tempLocals[t] = tempLocal = new VariableDefinition (t);
						return new[] {
							Instruction.Create (OpCodes.Ldloca, tempLocal),
							Instruction.Create (OpCodes.Initobj, t),
							Instruction.Create (OpCodes.Ldloc, tempLocal)
						};
					} else
						return new[] { Instruction.Create (OpCodes.Ldloc, tempLocal) };
			}
		}

		private Instruction Patch (Instruction i, Instruction old, Instruction replacement)
		{
			if (i == old)
				return replacement;
			else
				return i;
		}

		/// <summary>
		/// Replaces references to 'old' with 'new' within the method body. Will not replace the instruction itself.
		/// </summary>
		private void Patch (MethodDefinition method, RewriteContext context, Instruction old, Instruction replacement)
		{
			context.ReplacedInstructions[old] = replacement;

			var body = method.Body.Instructions;
			for (int i = 0; i < body.Count; i++) {
				if (body[i].Operand == old)
					body[i] = Instruction.Create (body[i].OpCode, replacement);

				var opInsns = body[i].Operand as Instruction[];
				if (opInsns != null) {
					for (int j = 0; j < opInsns.Length; j++) {
						if (opInsns[j] == old)
							opInsns[j] = replacement;
					}
				}
			}

			foreach (var p in context.Pairs) {
				p.A = Patch (p.A, old, replacement);
				p.B = Patch (p.B, old, replacement);
			}

			foreach (var g in context.NewGroups) {
				g.FirstPushInstruction = Patch (g.FirstPushInstruction, old, replacement);
				g.TryStart = Patch (g.TryStart, old, replacement);
				g.TryEnd = Patch (g.TryEnd, old, replacement);
				g.TryEndPredecessor = Patch (g.TryEndPredecessor, old, replacement);
				g.ExitPoint = Patch (g.ExitPoint, old, replacement);

				foreach (var h in g.Blocks) {
					for (int l = 0; l < h.LeaveTargets.Count; l++)
						h.LeaveTargets[l] = Patch (h.LeaveTargets[l], old, replacement);
					h.FirstFilterInsn = Patch (h.FirstFilterInsn, old, replacement);
					Patch (h.Handler, old, replacement);
				}
			}

			foreach (var eh in method.Body.ExceptionHandlers)
				Patch (eh, old, replacement);
		}

		private void Patch (ExceptionHandler eh, Instruction old, Instruction replacement)
		{
			eh.TryStart = Patch (eh.TryStart, old, replacement);
			eh.TryEnd = Patch (eh.TryEnd, old, replacement);
			eh.HandlerStart = Patch (eh.HandlerStart, old, replacement);
			eh.HandlerEnd = Patch (eh.HandlerEnd, old, replacement);
			eh.FilterStart = Patch (eh.FilterStart, old, replacement);
		}

		private Instruction Patch (Instruction i, Dictionary<Instruction, Instruction> pairs)
		{
			if (i == null)
				return null;
			Instruction result;
			if (!pairs.TryGetValue (i, out result))
				result = i;
			return result;
		}

		private void Patch (ExceptionHandler eh, Dictionary<Instruction, Instruction> pairs)
		{
			eh.TryStart = Patch (eh.TryStart, pairs);
			eh.TryEnd = Patch (eh.TryEnd, pairs);
			eh.HandlerStart = Patch (eh.HandlerStart, pairs);
			eh.HandlerEnd = Patch (eh.HandlerEnd, pairs);
			eh.FilterStart = Patch (eh.FilterStart, pairs);
		}

		private void PatchMany (MethodDefinition method, RewriteContext context, Dictionary<Instruction, Instruction> pairs)
		{
			foreach (var kvp in pairs)
				context.ReplacedInstructions[kvp.Key] = kvp.Value;

			var body = method.Body.Instructions;
			Instruction replacement;

			for (int i = 0; i < body.Count; i++) {
				var opInsn = body[i].Operand as Instruction;
				if (opInsn != null && pairs.TryGetValue (opInsn, out replacement))
					body[i].Operand = replacement;

				var opInsns = body[i].Operand as Instruction[];
				if (opInsns != null) {
					for (int j = 0; j < opInsns.Length; j++) {
						if (pairs.TryGetValue (opInsns[j], out replacement))
							opInsns[j] = replacement;
					}
				}
			}

			foreach (var p in context.Pairs) {
				p.A = Patch (p.A, pairs);
				p.B = Patch (p.B, pairs);
			}

			foreach (var g in context.NewGroups) {
				g.FirstPushInstruction = Patch (g.FirstPushInstruction, pairs);
				g.TryStart = Patch (g.TryStart, pairs);
				g.TryEnd = Patch (g.TryEnd, pairs);
				g.TryEndPredecessor = Patch (g.TryEndPredecessor, pairs);
				g.ExitPoint = Patch (g.ExitPoint, pairs);

				foreach (var h in g.Blocks) {
					for (int l = 0; l < h.LeaveTargets.Count; l++)
						h.LeaveTargets[l] = Patch (h.LeaveTargets[l], pairs);
					h.FirstFilterInsn = Patch (h.FirstFilterInsn, pairs);
					Patch (h.Handler, pairs);
				}
			}

			foreach (var eh in method.Body.ExceptionHandlers)
				Patch (eh, pairs);
		}

		private void InsertOps (
			Collection<Instruction> body, int offset, params Instruction[] ops
		)
		{
			for (int i = ops.Length - 1; i >= 0; i--)
				body.Insert (offset, ops[i]);
		}

		private Instruction ExtractExceptionHandlerExitTarget (ExceptionHandler eh)
		{
			var leave = eh.HandlerEnd.Previous;
			if (leave.OpCode == OpCodes.Rethrow)
				return leave;

			var leaveTarget = leave.Operand as Instruction;
			if (leaveTarget == null)
				throw new Exception ("Exception handler did not end with a 'leave'");
			return leaveTarget;
		}

		private bool IsStoreOperation (Code opcode)
		{
			switch (opcode) {
				case Code.Stloc:
				case Code.Stloc_S:
				case Code.Stloc_0:
				case Code.Stloc_1:
				case Code.Stloc_2:
				case Code.Stloc_3:
				case Code.Starg:
				case Code.Starg_S:
					return true;

				case Code.Ldloca:
				case Code.Ldloca_S:
				case Code.Ldloc:
				case Code.Ldloc_S:
				case Code.Ldloc_0:
				case Code.Ldloc_1:
				case Code.Ldloc_2:
				case Code.Ldloc_3:
				case Code.Ldarg:
				case Code.Ldarg_S:
				case Code.Ldarga:
				case Code.Ldarga_S:
				case Code.Ldarg_0:
				case Code.Ldarg_1:
				case Code.Ldarg_2:
				case Code.Ldarg_3:
					return false;
			}

			throw new NotImplementedException (opcode.ToString ());
		}

		private VariableDefinition LookupNumberedVariable (
			Code opcode, Mono.Collections.Generic.Collection<VariableDefinition> variables
		)
		{
			switch (opcode) {
				case Code.Ldloc_0:
				case Code.Stloc_0:
					return variables[0];
				case Code.Ldloc_1:
				case Code.Stloc_1:
					return variables[1];
				case Code.Ldloc_2:
				case Code.Stloc_2:
					return variables[2];
				case Code.Ldloc_3:
				case Code.Stloc_3:
					return variables[3];
			}

			return null;
		}

		private ParameterDefinition LookupNumberedArgument (
			Code opcode, ParameterDefinition fakeThis, Mono.Collections.Generic.Collection<ParameterDefinition> parameters
		)
		{
			int staticOffset = fakeThis == null ? 0 : 1;
			switch (opcode) {
				case Code.Ldarg_0:
					if (fakeThis == null)
						return parameters[0];
					else
						return fakeThis;
				case Code.Ldarg_1:
					return parameters[1 - staticOffset];
				case Code.Ldarg_2:
					return parameters[2 - staticOffset];
				case Code.Ldarg_3:
					return parameters[3 - staticOffset];
			}

			return null;
		}

		private GenericInstanceType FilterGenericInstanceType<T, U> (GenericInstanceType git, Dictionary<T, U> replacementTable)
			where T : TypeReference
			where U : TypeReference
		{
			List<TypeReference> newArgs = null;

			for (int i = 0; i < git.GenericArguments.Count; i++) {
				var ga = git.GenericArguments[i];
				var newGa = FilterTypeReference (ga, replacementTable);

				if (newGa != ga) {
					if (newArgs == null) {
						newArgs = new List<TypeReference> ();
						for (int j = 0; j < i; j++)
							newArgs[j] = git.GenericArguments[j];
					}

					newArgs.Add (newGa);
				} else if (newArgs != null)
					newArgs.Add (ga);
			}

			if (newArgs != null) {
				var result = new GenericInstanceType (git.ElementType);
				foreach (var arg in newArgs)
					result.GenericArguments.Add (arg);

				return result;
			} else {
				return git;
			}
		}

		private TypeReference FilterByReferenceType<T, U> (ByReferenceType brt, Dictionary<T, U> replacementTable)
			where T : TypeReference
			where U : TypeReference
		{
			var et = FilterTypeReference<T, U> (brt.ElementType, replacementTable);
			if (et != brt.ElementType)
				return new ByReferenceType (et);
			else
				return brt;
		}

		private TypeReference FilterPointerType<T, U> (PointerType pt, Dictionary<T, U> replacementTable)
			where T : TypeReference
			where U : TypeReference
		{
			var et = FilterTypeReference<T, U> (pt.ElementType, replacementTable);
			if (et != pt.ElementType)
				return new PointerType (et);
			else
				return pt;
		}

		private TypeReference FilterTypeReference<T, U> (TypeReference tr, Dictionary<T, U> replacementTable)
			where T : TypeReference
			where U : TypeReference
		{
			if ((replacementTable == null) || (replacementTable.Count == 0))
				return tr;

			TypeReference result;
			U temp;

			if (replacementTable.TryGetValue ((T)tr, out temp))
				result = temp;
			else
				result = tr;

			for (int i = 0; i < 50; i++) {
				var prev = result;
				var git = result as GenericInstanceType;
				var brt = result as ByReferenceType;
				var pt = result as PointerType;
				var at = result as ArrayType;

				if (git != null)
					result = FilterGenericInstanceType<T, U> (git, replacementTable);
				else if (brt != null) {
					var newEt = FilterTypeReference<T, U> (brt.ElementType, replacementTable);
					if (newEt != brt.ElementType)
						result = new ByReferenceType (newEt);
				} else if (pt != null) {
					var newEt = FilterTypeReference<T, U> (pt.ElementType, replacementTable);
					if (newEt != pt.ElementType)
						result = new PointerType (newEt);
				} else if (at != null) {
					var newEt = FilterTypeReference<T, U> (at.ElementType, replacementTable);
					if (newEt != at.ElementType)
						result = new ArrayType (newEt, at.Rank);
				}

				if (prev == result)
					return result;
			}

			throw new Exception ("FilterTypeReference iterated 50 times without completing");
		}

		private T FilterMemberReference<T, U, V> (T mr, Dictionary<U, V> replacementTable)
			where T : MemberReference
			where U : TypeReference
			where V : TypeReference
		{
			if ((replacementTable == null) || (replacementTable.Count == 0))
				return mr;

			var field = mr as FieldReference;
			var meth = mr as MethodReference;
			var prop = mr as PropertyReference;

			if (field != null)
				return FilterFieldReference<U, V> (field, replacementTable) as T;
			else if (meth != null)
				return FilterMethodReference<U, V> (meth, replacementTable) as T;
			else if (prop != null)
				return FilterPropertyReference<U, V> (prop, replacementTable) as T;
			else
				throw new Exception ("Unhandled reference type");

			return mr;
		}

		private MemberReference FilterPropertyReference<U, V> (PropertyReference prop, Dictionary<U, V> replacementTable)
			where U : TypeReference
			where V : TypeReference
		{
			throw new NotImplementedException ("FilterPropertyReference not implemented");
		}

		private MemberReference FilterMethodReference<U, V> (MethodReference meth, Dictionary<U, V> replacementTable)
			where U : TypeReference
			where V : TypeReference
		{
			var result = new MethodReference (
				meth.Name,
				FilterTypeReference (meth.ReturnType, replacementTable),
				// FIXME: Is this correct?
				FilterTypeReference (meth.DeclaringType, replacementTable)
			) {
			};
			foreach (var p in meth.Parameters)
				result.Parameters.Add (new ParameterDefinition (p.Name, p.Attributes, FilterTypeReference (p.ParameterType, replacementTable)));
			return result;
		}

		private MemberReference FilterFieldReference<U, V> (FieldReference field, Dictionary<U, V> replacementTable)
			where U : TypeReference
			where V : TypeReference
		{
			var result = new FieldReference (
				field.Name,
				FilterTypeReference (field.FieldType, replacementTable),
				FilterTypeReference (field.DeclaringType, replacementTable)
			) {
			};
			return result;
		}

		private Instruction GeneratedRet ()
		{
			return Instruction.Create (OpCodes.Ret);
		}

		private Instruction Nop (string description = null)
		{
			var result = Instruction.Create (OpCodes.Nop);
			result.Operand = description;
			return result;
		}

		private Instruction Rethrow (string description = null)
		{
			var result = Instruction.Create (OpCodes.Rethrow);
			result.Operand = description;
			return result;
		}

		private MethodDefinition CreateConstructor (TypeDefinition type, bool includeRet)
		{
			var ctorMethod = new MethodDefinition (
				".ctor", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
				type.Module.TypeSystem.Void
			);
			type.Methods.Add (ctorMethod);
			InsertOps (ctorMethod.Body.Instructions, 0, new[] {
				Instruction.Create (OpCodes.Ldarg_0),
				Instruction.Create (OpCodes.Call,
					new MethodReference (
						".ctor", type.Module.TypeSystem.Void,
						type.BaseType
					) { HasThis = true }),
				Nop ()
			});
			if (includeRet)
				ctorMethod.Body.Instructions.Add (Instruction.Create (OpCodes.Ret));
			return ctorMethod;
		}

		public class ClosureInfo {
			public MethodDefinition Method;
			public HashSet<VariableReference> Variables;
			public HashSet<ParameterReference> Parameters;

			public MethodDefinition Constructor;
			public object ClosureStorage;
			public TypeDefinition TypeDefinition;
			public TypeReference TypeReference;

			public Dictionary<FieldDefinition, MethodDefinition> Setters = new Dictionary<FieldDefinition, MethodDefinition> ();
			public Dictionary<FieldDefinition, MethodDefinition> RefSetters = new Dictionary<FieldDefinition, MethodDefinition> ();

			public ClosureInfo Clone ()
			{
				return new ClosureInfo {
					Method = Method,
					Variables = Variables,
					Parameters = Parameters,
					Constructor = Constructor,
					ClosureStorage = null,
					TypeDefinition = TypeDefinition,
					TypeReference = TypeReference,
					Setters = Setters,
					RefSetters = RefSetters
				};
			}
		}

		private ClosureInfo ConvertToClosure (
			MethodDefinition method, ParameterDefinition fakeThis,
			HashSet<VariableReference> variables, HashSet<ParameterReference> parameters,
			RewriteContext context
		)
		{
			var result = new ClosureInfo {
				Method = method,
				Variables = variables,
				Parameters = parameters
			};

			var insns = method.Body.Instructions;
			result.TypeDefinition = new TypeDefinition (
				method.DeclaringType.Namespace, method.Name + "__closure" + (ClosureIndex++).ToString (),
				TypeAttributes.Class | TypeAttributes.NestedPublic
			);
			result.TypeDefinition.BaseType = method.Module.TypeSystem.Object;
			method.DeclaringType.NestedTypes.Add (result.TypeDefinition);

			var functionGpMapping = new Dictionary<TypeReference, GenericParameter> ();
			CopyGenericParameters (method.DeclaringType, result.TypeDefinition, functionGpMapping);
			CopyGenericParameters (method, result.TypeDefinition, functionGpMapping);

			var isGeneric = method.DeclaringType.HasGenericParameters || method.HasGenericParameters;
			var thisGenType = new GenericInstanceType (method.DeclaringType);
			var thisType = isGeneric ? thisGenType : (TypeReference)method.DeclaringType;
			var genClosureTypeReference = new GenericInstanceType (result.TypeDefinition);

			foreach (var p in method.DeclaringType.GenericParameters) {
				thisGenType.GenericArguments.Add (functionGpMapping[p]);
				genClosureTypeReference.GenericArguments.Add (functionGpMapping[p]);
			}

			foreach (var p in method.GenericParameters)
				genClosureTypeReference.GenericArguments.Add (functionGpMapping[p]);

			if ((method.DeclaringType.GenericParameters.Count + method.GenericParameters.Count) > 0)
				result.TypeReference = genClosureTypeReference;
			else
				result.TypeReference = result.TypeDefinition;

			var isStatic = method.IsStatic;

			var localCount = 0;
			var closureVariable = new VariableDefinition (result.TypeReference);
			result.ClosureStorage = closureVariable;

			var usedFieldNames = new HashSet<string> ();

			var extractedVariables = variables.ToDictionary (
				v => (object)v,
				v => {
					var vd = v.Resolve ();

					string variableName;
					if ((method.DebugInformation == null) || !method.DebugInformation.TryGetName (vd, out variableName))
						variableName = "local" + method.Body.Variables.IndexOf (vd);
					else {
						variableName = "local_" + variableName;

						// HACK: Methods compiled in debug mode can end up with multiple variables that all have the
						//  same name according to debug information
						if (usedFieldNames.Contains(variableName))
							variableName += method.Body.Variables.IndexOf (vd);
					}

					usedFieldNames.Add (variableName);
					return new FieldDefinition (variableName, FieldAttributes.Public, FilterTypeReference (v.VariableType, functionGpMapping));
				}
			);

			method.Body.Variables.Add (closureVariable);

			for (int i = 0; i < method.Parameters.Count; i++) {
				var p = method.Parameters[i];
				if (!parameters.Contains (p))
					continue;

				// ref/out parameters cannot be shifted into the closure without introducing
				//  multiple problems
				if (p.ParameterType.IsByReference)
					continue;

				var name = (p.Name != null) ? "arg_" + p.Name : "arg" + i;
				while (usedFieldNames.Contains(name))
					name += "_";

				var fieldType = FilterTypeReference (p.ParameterType, functionGpMapping);
				// FIXME: Is this possible?
				if (fieldType.IsByReference)
					fieldType = fieldType.GetElementType ();

				usedFieldNames.Add (name);
				var fd = new FieldDefinition (name, FieldAttributes.Public, fieldType);
				extractedVariables[p] = fd;
			}

			if (!isStatic)
				extractedVariables[fakeThis] = new FieldDefinition ("__this", FieldAttributes.Public, thisType);

			result.Constructor = CreateConstructor (result.TypeDefinition, false);

			{
				var ctorInsns = result.Constructor.Body.Instructions;

				foreach (var param in parameters) {
					FieldDefinition fd;
					if (!extractedVariables.TryGetValue (param, out fd))
						continue;

					var pd = new ParameterDefinition (param.Name, ParameterAttributes.None, param.ParameterType);
					result.Constructor.Parameters.Add (pd);

					ctorInsns.Add (Instruction.Create (OpCodes.Ldarg_0));
					ctorInsns.Add (Instruction.Create (OpCodes.Ldarg, pd));
					if (pd.ParameterType.IsByReference)
						ctorInsns.Add (Instruction.Create (SelectLdindForOperand (pd)));
					ctorInsns.Add (Instruction.Create (OpCodes.Stfld, fd));
				}

				ctorInsns.Add (Instruction.Create (OpCodes.Ret));
			}

			GenerateClosureSetters (method, result, extractedVariables);

			FilterRange (
				method, 0, insns.Count - 1, context, (insn) => {
					var variable = (insn.Operand as VariableDefinition)
						?? LookupNumberedVariable (insn.OpCode.Code, method.Body.Variables);
					var arg = (insn.Operand as ParameterDefinition)
						?? LookupNumberedArgument (insn.OpCode.Code, isStatic ? null : fakeThis, method.Parameters);

					// FIXME
					if (variable == result.ClosureStorage)
						return null;

					if ((variable == null) && (arg == null))
						return null;

					FieldDefinition matchingField;
					var lookupKey = (object)variable ?? arg;
					// Any ref/out parameters will not have an extracted variable so accesses must not be transformed
					if (!extractedVariables.TryGetValue (lookupKey, out matchingField))
						return null;

					if (IsStoreOperation (insn.OpCode.Code)) {
						// FIXME: We can't reuse an argument as a temporary store point, we need to create a temporary
						if ((insn.OpCode.Code == Code.Starg) || (insn.OpCode.Code == Code.Starg_S))
							;

						if ((arg != null) && arg.ParameterType.IsByReference) {
							// For ref/out parameters, we need to perform a store through into both the closure and the parameter to keep them in sync.
							var setter = result.RefSetters[matchingField];

							return new[] {
								Instruction.Create (OpCodes.Ldarga, arg),
								Instruction.Create (OpCodes.Ldloc, (VariableDefinition)result.ClosureStorage),
								Instruction.Create (OpCodes.Call, setter)
							};
						} else {
							var setter = result.Setters[matchingField];

							return new[] {
								Instruction.Create (OpCodes.Ldloc, (VariableDefinition)result.ClosureStorage),
								Instruction.Create (OpCodes.Call, setter)
							};
						}
					} else {
						// FIXME: If any of the args are ref/out, we need to generate an epilogue that flushes the closure values
						//  into the args at function exit
						var wasRefType = (arg?.ParameterType ?? variable?.VariableType).IsByReference;
						var newInsn = Instruction.Create (OpCodes.Ldloc, (VariableDefinition)result.ClosureStorage);
						var loadOp =
							((insn.OpCode.Code == Code.Ldloca) ||
							(insn.OpCode.Code == Code.Ldloca_S) ||
							(insn.OpCode.Code == Code.Ldarga) ||
							(insn.OpCode.Code == Code.Ldarga_S) ||
							wasRefType)
								? OpCodes.Ldflda
								: OpCodes.Ldfld;
						return new[] {
							newInsn,
							Instruction.Create (loadOp, matchingField)
						};
					}
				}
			);

			var toInject = new List<Instruction> ();
			foreach (var param in parameters) {
				if (!extractedVariables.ContainsKey (param))
					continue;

				toInject.Add (Instruction.Create (OpCodes.Ldarg, (ParameterDefinition)param));
			}

			toInject.AddRange (new[] {
				Instruction.Create(OpCodes.Newobj, result.Constructor),
				Instruction.Create(OpCodes.Stloc, (VariableDefinition)result.ClosureStorage)
			});

			InsertOps (insns, 0, toInject.ToArray ());

			CleanMethodBody (method, null, true);

			return result;
		}

		private static void GenerateClosureSetters (MethodDefinition method, ClosureInfo result, Dictionary<object, FieldDefinition> extractedVariables)
		{
			foreach (var kvp in extractedVariables) {
				result.TypeDefinition.Fields.Add (kvp.Value);

				// We generate a set_ static method for every local that was extracted into the closure.
				// This allows us to transform local set/get operations directly without any temporary loads/stores
				//  because we can leave the new value on the stack and push the closure ref afterward. If we were
				//  using stfld directly, we'd have to first store the new value somewhere, then push the closure,
				//  then push the new value back onto the stack and perform the stfld.
				{
					var paramValue = new ParameterDefinition ("value", ParameterAttributes.None, kvp.Value.FieldType);
					var paramClosure = new ParameterDefinition ("closure", ParameterAttributes.None, result.TypeReference);
					var setMethod = new MethodDefinition ("set_" + kvp.Value.Name, MethodAttributes.Static | MethodAttributes.Public, method.Module.TypeSystem.Void) {
						Parameters = {
							paramValue,
							paramClosure
						}
					};
					result.TypeDefinition.Methods.Add (setMethod);
					result.Setters.Add (kvp.Value, setMethod);

					setMethod.Body = new MethodBody (setMethod);
					var insns = setMethod.Body.Instructions;
					insns.Add (Instruction.Create (OpCodes.Ldarg_1));
					insns.Add (Instruction.Create (OpCodes.Ldarg_0));
					insns.Add (Instruction.Create (OpCodes.Stfld, kvp.Value));
					insns.Add (Instruction.Create (OpCodes.Ret));
				}

				var arg = kvp.Key as ParameterDefinition;
				if (arg == null)
					continue;

				if (!arg.ParameterType.IsByReference)
					continue;

				{
					var paramValue = new ParameterDefinition ("value", ParameterAttributes.None, kvp.Value.FieldType);
					var wrappedType = kvp.Value.FieldType.IsByReference ? kvp.Value.FieldType : new ByReferenceType (kvp.Value.FieldType);
					var paramArg = new ParameterDefinition ("argument", ParameterAttributes.None, wrappedType);
					var paramClosure = new ParameterDefinition ("closure", ParameterAttributes.None, result.TypeReference);
					var setMethod = new MethodDefinition ("setThru_" + kvp.Value.Name, MethodAttributes.Static | MethodAttributes.Public, method.Module.TypeSystem.Void) {
						Parameters = {
							paramValue,
							paramArg,
							paramClosure
						}
					};
					result.TypeDefinition.Methods.Add (setMethod);
					result.RefSetters.Add (kvp.Value, setMethod);

					setMethod.Body = new MethodBody (setMethod);
					var insns = setMethod.Body.Instructions;
					insns.Add (Instruction.Create (OpCodes.Ldarg_2));
					insns.Add (Instruction.Create (OpCodes.Ldarg_0));
					insns.Add (Instruction.Create (OpCodes.Stfld, kvp.Value));
					insns.Add (Instruction.Create (OpCodes.Ldarg_1));
					insns.Add (Instruction.Create (OpCodes.Ldarg_0));
					insns.Add (Instruction.Create (SelectStindForOperand (wrappedType)));
					insns.Add (Instruction.Create (OpCodes.Ret));
				}
			}
		}

		private int CatchCount;

		private Instruction PostFilterRange (
			Dictionary<Instruction, Instruction> remapTable, Instruction oldValue
		)
		{
			if (oldValue == null)
				return null;

			Instruction result;
			if (remapTable.TryGetValue (oldValue, out result))
				return result;

			return oldValue;
		}

		private void FilterRange (
			MethodDefinition method,
			int firstIndex, int lastIndex, RewriteContext context,
			Func<Instruction, Instruction[]> filter
		)
		{
			if (lastIndex < firstIndex)
				throw new ArgumentException ("lastIndex must >= firstIndex");

			CleanMethodBody (method, null, true);

			var remapTableFirst = new Dictionary<Instruction, Instruction> ();
			var remapTableLast = new Dictionary<Instruction, Instruction> ();
			var instructions = method.Body.Instructions;

			var firstRemovedInstruction = instructions[firstIndex];

			for (int i = firstIndex; i <= lastIndex; i++) {
				var insn = instructions[i];
				var result = filter (insn);
				if (result == null)
					continue;
				if (result.Length == 1 && result[0] == insn)
					continue;

				if (insn != result[0])
					remapTableFirst[insn] = result[0];
				for (int j = result.Length - 1; j >= 1; j--)
					instructions.Insert (i + 1, result[j]);

				remapTableLast[insn] = result[result.Length - 1];

				lastIndex += (result.Length - 1);
				i += (result.Length - 1);
			}

			for (int i = 0; i < instructions.Count; i++) {
				var insn = instructions[i];

				Instruction newInsn;
				if (remapTableFirst.TryGetValue (insn, out newInsn)) {
					context.ReplacedInstructions[insn] = newInsn;
					instructions[i] = newInsn;
					insn = newInsn;
				}

				var operand = insn.Operand as Instruction;
				var operandInsns = insn.Operand as Instruction[];
				if (operandInsns != null) {
					var newInsns = new Instruction[operandInsns.Length];
					for (int j = 0; j < newInsns.Length; j++) {
						if (!remapTableFirst.TryGetValue (operandInsns[j], out newInsns[j]))
							newInsns[j] = operandInsns[j];
					}

					insn.Operand = newInsns;
				} else if (operand != null) {
					Instruction newOperand;
					if (!remapTableFirst.TryGetValue (operand, out newOperand))
						continue;

					insn.Operand = newOperand;
				}
			}

			foreach (var eh in method.Body.ExceptionHandlers) {
				eh.FilterStart = PostFilterRange (remapTableFirst, eh.FilterStart);
				eh.TryStart = PostFilterRange (remapTableFirst, eh.TryStart);
				eh.TryEnd = PostFilterRange (remapTableFirst, eh.TryEnd);
				eh.HandlerStart = PostFilterRange (remapTableFirst, eh.HandlerStart);
				eh.HandlerEnd = PostFilterRange (remapTableFirst, eh.HandlerEnd);
			}

			CleanMethodBody (method, null, true);
		}

		private void GenerateParameters (
			MethodDefinition newMethod, HashSet<VariableReference> variables,
			Dictionary<object, object> mapping, HashSet<object> needsLdind
		)
		{
			int i = 0;
			foreach (var vr in variables) {
				var newParamType =
					vr.VariableType.IsByReference
						? vr.VariableType
						: new ByReferenceType (vr.VariableType);
				var newParam = new ParameterDefinition ("loc_" + i++.ToString (), ParameterAttributes.None, newParamType);
				newMethod.Parameters.Add (newParam);
				mapping[vr] = newParam;
				if (newParamType != vr.VariableType)
					needsLdind.Add (newParam);
			}
		}

		private void GenerateParameters (
			MethodDefinition newMethod, HashSet<ParameterReference> parameters,
			Dictionary<object, object> mapping, HashSet<object> needsLdind
		)
		{
			foreach (var pr in parameters) {
				// FIXME: replacementTable
				var filteredParamType = FilterTypeReference<TypeReference, TypeReference> (pr.ParameterType, null);
				var newParamType =
					filteredParamType.IsByReference
						? filteredParamType
						: new ByReferenceType (filteredParamType);
				var newParam = new ParameterDefinition ("arg_" + pr.Name, ParameterAttributes.None, newParamType);
				newMethod.Parameters.Add (newParam);
				mapping[pr] = newParam;
				if (newParamType != filteredParamType)
					needsLdind.Add (newParam);
			}
		}

		private void CopyGenericParameters (TypeDefinition sourceType, TypeDefinition owner, Dictionary<TypeReference, GenericParameter> result)
		{
			foreach (var gp in sourceType.GenericParameters) {
				result[gp] = new GenericParameter (gp.Name, owner);
				owner.GenericParameters.Add (result[gp]);
			}
		}

		private void CopyGenericParameters (MethodDefinition sourceMethod, TypeDefinition owner, Dictionary<TypeReference, GenericParameter> result)
		{
			foreach (var gp in sourceMethod.GenericParameters) {
				result[gp] = new GenericParameter (gp.Name, owner);
				owner.GenericParameters.Add (result[gp]);
			}
		}

		private void CopyGenericParameters (MethodDefinition sourceMethod, MethodDefinition owner, Dictionary<TypeReference, GenericParameter> result)
		{
			foreach (var gp in sourceMethod.GenericParameters) {
				result[gp] = new GenericParameter (gp.Name, owner);
				owner.GenericParameters.Add (result[gp]);
			}
		}

		/// <summary>
		/// Extract a catch block into an independent method that accepts an exception parameter and a closure
		/// The catch method returns 0 to indicate that the exception should be rethrown or [1-n] to indicate a target within the
		///  parent function that should be leave'd to
		/// </summary>
		private ExcBlock ExtractCatch (
			MethodDefinition method, ExceptionHandler eh,
			ClosureInfo closureInfo, ParameterDefinition fakeThis,
			ExcGroup group, RewriteContext context
		)
		{
			var insns = method.Body.Instructions;
			var closure = closureInfo.ClosureStorage;
			var closureType = closureInfo.TypeReference;

			var handlerFirstIndex = Find (context, insns, eh.HandlerStart);
			var handlerLastIndex = Find (context, insns, eh.HandlerEnd) - 1;

			var catchMethod = new MethodDefinition (
				$"{method.Name}__catch{CatchCount++}",
				MethodAttributes.Static | MethodAttributes.Private,
				method.Module.TypeSystem.Int32
			);

			var gpMapping = new Dictionary<TypeReference, GenericParameter> ();
			CopyGenericParameters (method, catchMethod, gpMapping);

			catchMethod.Body.InitLocals = true;
			var closureParam = new ParameterDefinition ("__closure", ParameterAttributes.None, closureType);
			var excParam = new ParameterDefinition ("__exc", ParameterAttributes.None, eh.CatchType ?? method.Module.TypeSystem.Object);
			var paramMapping = new Dictionary<object, object> {
				{closure, closureParam}
			};
			var closureVariable = new VariableDefinition (closureType);
			var needsLdind = new HashSet<object> ();

			catchMethod.Parameters.Add (excParam);
			catchMethod.Parameters.Add (closureParam);

			var catchMethodRefParameters = method.Parameters.Where (p => p.ParameterType.IsByReference).ToList ();

			// Any ref/out parameters to the original method can't be shifted into the closure, so we
			//  must pass them directly to the catch body instead
			foreach (var param in catchMethodRefParameters) {
				var catchParam = new ParameterDefinition (param.Name, ParameterAttributes.None, param.ParameterType);
				catchMethod.Parameters.Add (catchParam);
				paramMapping.Add (param, catchParam);
			}

			var catchInsns = catchMethod.Body.Instructions;

			// CHANGE #4: Adding method earlier
			method.DeclaringType.Methods.Add (catchMethod);

			var i1 = insns.IndexOf (eh.HandlerStart);
			var i2 = insns.IndexOf (eh.HandlerEnd);

			var endsWithRethrow = insns[i2].OpCode.Code == Code.Rethrow;

			if (i2 <= i1)
				throw new Exception ("Hit beginning of handler while rewinding past rethrows");

			var leaveTargets = new List<Instruction> ();
			bool canRethrow = false;

			// FIXME: Use generic parameter mapping to replace GP type references
			var newMapping = ExtractRangeToMethod (
				method, catchMethod, fakeThis,
				i1, i2 - 1,
				variableMapping: paramMapping,
				typeMapping: gpMapping,
				context: context,
				// FIXME: Identify when we want to preserve control flow and when we don't
				filter: (insn, range) => {
					var operandParameter = insn.Operand as ParameterDefinition;
					if ((operandParameter != null) && !operandParameter.Name.Contains ("closure"))
						;

					var operandTr = insn.Operand as TypeReference;
					if (operandTr != null) {
						var newOperandTr = FilterTypeReference (operandTr, gpMapping);
						if (newOperandTr != operandTr)
							insn.Operand = newOperandTr;
					}

					var operandMr = insn.Operand as MemberReference;
					if (operandMr != null) {
						var newOperandMr = FilterMemberReference (operandMr, gpMapping);
						if (newOperandMr != operandMr)
							insn.Operand = newOperandMr;
					}

					var operandInsn = insn.Operand as Instruction;

					switch (insn.OpCode.Code) {
						case Code.Leave:
						case Code.Leave_S:
							if (
								(range == null)
							) {
								var targetIndex = leaveTargets.IndexOf (operandInsn) + 1;
								if (targetIndex <= 0) {
									targetIndex = leaveTargets.Count + 1;
									leaveTargets.Add (operandInsn);
								}

								return new[] {
									Nop ($"Leave to target #{targetIndex} ({operandInsn})"),
									Instruction.Create (OpCodes.Ldc_I4, targetIndex),
									GeneratedRet ()
								};
							} else
								break;

						case Code.Rethrow:
							canRethrow = true;

							if (range == null)
								return new[] {
									Nop ("Rethrow"),
									Instruction.Create (OpCodes.Ldc_I4_0),
									GeneratedRet ()
								};
							else
								break;

						case Code.Ret:
							// It's not valid to ret from inside a filter or catch: https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ret
							throw new Exception ("Unexpected ret inside catch block");
					}

					return null;
				}
			);

			StripUnreferencedNops (catchMethod);
			CleanMethodBody (catchMethod, method, false);

			if (catchInsns.Count > 0) {
				InsertOps (
					catchInsns, 0, new[] {
						Instruction.Create (OpCodes.Ldarg, excParam)
					}
				);

				CleanMethodBody (catchMethod, method, true);
			} else {
				// FIXME
				method.DeclaringType.Methods.Remove (catchMethod);
				catchMethod = null;
			}

			SortExceptionHandlers (catchMethod, context);

			var isCatchAll = (eh.HandlerType == ExceptionHandlerType.Catch) && (eh.CatchType?.FullName == "System.Object");
			var handler = new ExcBlock {
				Handler = eh,
				CatchMethod = catchMethod,
				CatchMethodRefParameters = catchMethodRefParameters,
				IsCatchAll = isCatchAll,
				Mapping = newMapping,
				LeaveTargets = leaveTargets,
				CanRethrow = canRethrow
			};
			return handler;
		}

		private static OpCode SelectStindForOperand (object operand)
		{
			var vr = operand as VariableReference;
			var pr = operand as ParameterReference;
			var operandType = (operand as TypeReference) ?? ((vr != null) ? vr.VariableType : pr.ParameterType);
			while (operandType.IsByReference)
				operandType = operandType.GetElementType ();

			switch (operandType.FullName) {
				case "System.Byte":
					// FIXME
					return OpCodes.Stind_I1;
				case "System.UInt16":
					// FIXME
					return OpCodes.Stind_I2;
				case "System.UInt32":
					// FIXME
					return OpCodes.Stind_I4;
				case "System.UInt64":
					// FIXME
					return OpCodes.Stind_I8;
				case "System.SByte":
					return OpCodes.Stind_I1;
				case "System.Int16":
					return OpCodes.Stind_I2;
				case "System.Int32":
					return OpCodes.Stind_I4;
				case "System.Int64":
					return OpCodes.Stind_I8;
				case "System.Single":
					return OpCodes.Stind_R4;
				case "System.Double":
					return OpCodes.Stind_R8;
				default:
					return OpCodes.Stind_Ref;
			}
		}

		private static OpCode SelectLdindForOperand (object operand)
		{
			var vr = operand as VariableReference;
			var pr = operand as ParameterReference;
			var operandType = (operand as TypeReference) ?? ((vr != null) ? vr.VariableType : pr.ParameterType);
			while (operandType.IsByReference)
				operandType = operandType.GetElementType ();

			switch (operandType.FullName) {
				case "System.Byte":
					return OpCodes.Ldind_U1;
				case "System.UInt16":
					return OpCodes.Ldind_U2;
				case "System.UInt32":
					return OpCodes.Ldind_U4;
				case "System.UInt64":
					// FIXME
					return OpCodes.Ldind_I8;
				case "System.SByte":
					return OpCodes.Ldind_I1;
				case "System.Int16":
					return OpCodes.Ldind_I2;
				case "System.Int32":
					return OpCodes.Ldind_I4;
				case "System.Int64":
					return OpCodes.Ldind_I8;
				case "System.Single":
					return OpCodes.Ldind_R4;
				case "System.Double":
					return OpCodes.Ldind_R8;
				default:
					return OpCodes.Ldind_Ref;
			}
		}

		private bool IsEndfilter (Instruction insn)
		{
			return (insn.OpCode.Code == Code.Endfilter) ||
				(insn.Operand as string == "extracted endfilter");
		}

		private Instruction GenerateSet (object variableOrParameter)
		{
			var loc = variableOrParameter as VariableDefinition;
			if (loc != null)
				return Instruction.Create (OpCodes.Stloc, loc);
			else
				return Instruction.Create (OpCodes.Starg, (ParameterDefinition)variableOrParameter);
		}

		private Instruction GenerateLoad (object variableOrParameter)
		{
			var loc = variableOrParameter as VariableDefinition;
			if (loc != null)
				return Instruction.Create (OpCodes.Ldloc, loc);
			else
				return Instruction.Create (OpCodes.Ldarg, (ParameterDefinition)variableOrParameter);
		}

		private void ExtractFilter (
			MethodDefinition method, ExceptionHandler eh,
			ClosureInfo closureInfo, ParameterDefinition fakeThis,
			ExcGroup group, RewriteContext context,
			ExcBlock catchBlock
		)
		{
			var insns = method.Body.Instructions;
			var closure = closureInfo.ClosureStorage;
			var closureType = closureInfo.TypeReference;
			var filterIndex = FilterIndex++;
			var filterTypeDefinition = new TypeDefinition (
				method.DeclaringType.Namespace, method.Name + "__filter" + filterIndex.ToString (),
				TypeAttributes.NestedPublic | TypeAttributes.Class,
				GetExceptionFilter (method.Module)
			);
			var filterField = new FieldDefinition ("__filter" + filterIndex, FieldAttributes.Public, filterTypeDefinition);

			var gpMapping = new Dictionary<TypeReference, GenericParameter> ();
			CopyGenericParameters (method.DeclaringType, filterTypeDefinition, gpMapping);
			CopyGenericParameters (method, filterTypeDefinition, gpMapping);

			var efilt = GetExceptionFilter (method.Module);
			filterTypeDefinition.BaseType = efilt;
			method.DeclaringType.NestedTypes.Add (filterTypeDefinition);

			var closureField = new FieldDefinition (
				"closure", FieldAttributes.Public, closureType
			);
			filterTypeDefinition.Fields.Add (closureField);

			var filterConstructor = CreateConstructor (filterTypeDefinition, false);
			var closureConstructorParameter = new ParameterDefinition ("closure", ParameterAttributes.None, closureType);
			filterConstructor.Parameters.Add (closureConstructorParameter);
			InsertOps (filterConstructor.Body.Instructions, filterConstructor.Body.Instructions.Count, new[] {
				Instruction.Create(OpCodes.Ldarg_0),
				Instruction.Create(OpCodes.Ldarg, closureConstructorParameter),
				Instruction.Create(OpCodes.Stfld, closureField),
				Instruction.Create(OpCodes.Ret)
			});

			var filterActivationMethod = new MethodDefinition ("Activate", MethodAttributes.Static | MethodAttributes.Public, method.Module.TypeSystem.Void);
			var filterActivationParameter = new ParameterDefinition ("closure", ParameterAttributes.None, closureType);
			filterActivationMethod.Parameters.Add (filterActivationParameter);
			var skipInit = Nop ("Skip initializing filter " + filterTypeDefinition.Name);

			InsertOps (filterActivationMethod.Body.Instructions, 0, new[] {
				// If the filter is already initialized (we're running in a loop, etc) don't create a new instance
				Nop ("Initializing filter " + filterTypeDefinition.Name),
				Instruction.Create (OpCodes.Ldarg, filterActivationParameter),
				Instruction.Create (OpCodes.Ldfld, filterField),
				Instruction.Create (OpCodes.Brtrue, skipInit),

				// Create a new instance of the filter and store it
				Instruction.Create (OpCodes.Ldarg, filterActivationParameter),
				Instruction.Create (OpCodes.Dup),
				Instruction.Create (OpCodes.Newobj, filterConstructor),
				Instruction.Create (OpCodes.Stfld, filterField),

				skipInit,

				// Then call Push on the filter instance to activate it
				Instruction.Create (OpCodes.Ldarg, filterActivationParameter),
				Instruction.Create (OpCodes.Ldfld, filterField),
				Instruction.Create (OpCodes.Castclass, efilt),
				Instruction.Create (OpCodes.Call, new MethodReference (
						"Push", method.Module.TypeSystem.Void, efilt
				) { HasThis = false, Parameters = {
						new ParameterDefinition (efilt)
				} }),

				Instruction.Create (OpCodes.Ret)
			});

			var filterDeactivationMethod = new MethodDefinition ("Deactivate", MethodAttributes.Static | MethodAttributes.Public, method.Module.TypeSystem.Void);
			var filterDeactivationParameter = new ParameterDefinition ("closure", ParameterAttributes.None, closureType);
			filterDeactivationMethod.Parameters.Add (filterActivationParameter);

			InsertOps (filterDeactivationMethod.Body.Instructions, 0, new[] {
				Instruction.Create (OpCodes.Ldarg, filterActivationParameter),
				Instruction.Create (OpCodes.Ldfld, filterField),
				Instruction.Create (OpCodes.Castclass, efilt),
				Instruction.Create (OpCodes.Call, new MethodReference (
						"Pop", method.Module.TypeSystem.Void, efilt
				) { HasThis = false, Parameters = {
						new ParameterDefinition (efilt)
				} }),

				Instruction.Create (OpCodes.Ret)
			});

			var filterMethod = new MethodDefinition (
				"Evaluate",
				MethodAttributes.Virtual | MethodAttributes.Public,
				method.Module.TypeSystem.Int32
			);
			filterMethod.Body.InitLocals = true;

			var excArg = new ParameterDefinition ("exc", default (ParameterAttributes), method.Module.TypeSystem.Object);
			filterMethod.Parameters.Add (excArg);

			GenerateFilterMethodBody (method, eh, fakeThis, context, insns, closure, gpMapping, closureField, filterMethod, excArg);

			filterTypeDefinition.Methods.Add (filterActivationMethod);
			filterTypeDefinition.Methods.Add (filterDeactivationMethod);
			filterTypeDefinition.Methods.Add (filterMethod);
			closureInfo.TypeDefinition.Fields.Add (filterField);

			catchBlock.FilterMethod = filterMethod;
			catchBlock.FilterType = filterTypeDefinition;
			catchBlock.FilterField = filterField;
			catchBlock.FilterConstructor = filterConstructor;
			catchBlock.FirstFilterInsn = eh.FilterStart;
			catchBlock.FilterActivationMethod = filterActivationMethod;
			catchBlock.FilterDeactivationMethod = filterDeactivationMethod;
		}

		private void GenerateFilterMethodBody (
			MethodDefinition method, ExceptionHandler eh, ParameterDefinition fakeThis, 
			RewriteContext context, Collection<Instruction> insns, object closure, 
			Dictionary<TypeReference, GenericParameter> gpMapping, FieldDefinition closureField, 
			MethodDefinition filterMethod, ParameterDefinition excArg
		)
		{
			int i1 = insns.IndexOf (eh.FilterStart), i2 = insns.IndexOf (eh.HandlerStart);
			if (i2 < 0)
				throw new Exception ($"Handler start instruction {eh.HandlerStart} not found in method body");

			if (i2 <= i1)
				throw new Exception ("Handler size was 0 or less");

			var endfilter = insns[i2 - 1];
			if (!IsEndfilter (endfilter))
				throw new Exception ($"Filter did not end with an endfilter, found {endfilter}");

			{
				var variableMapping = new Dictionary<object, object> ();
				var newVariables = ExtractRangeToMethod (
					method, filterMethod, fakeThis, i1, i2 - 1,
					variableMapping: variableMapping, typeMapping: gpMapping,
					context: context
				);
				var newClosureLocal = newVariables[closure];

				var filterInsns = filterMethod.Body.Instructions;
				if (filterInsns.Count <= 0)
					throw new Exception ("Filter body was empty");

				var oldFilterInsn = filterInsns[filterInsns.Count - 1];
				if (!IsEndfilter (oldFilterInsn))
					throw new Exception ($"Unexpected last instruction {oldFilterInsn}");

				foreach (var insn in filterInsns) {
					if (insn.Operand is ParameterDefinition)
						throw new Exception ($"Exception filter references parameter {insn.Operand}, which is not permitted");
				}

				var filterReplacement = Instruction.Create (OpCodes.Ret);
				filterInsns[filterInsns.Count - 1] = filterReplacement;
				Patch (filterMethod, context, oldFilterInsn, filterReplacement);

				InsertOps (
					filterInsns, 0, new[] {
						// Load the closure from this and store it into our temporary
						Instruction.Create (OpCodes.Ldarg_0),
						Instruction.Create (OpCodes.Ldfld, closureField),
						GenerateSet (newClosureLocal),
						// Load the exception from arg1 since exception handlers are entered with it on the stack
						Instruction.Create (OpCodes.Ldarg, excArg)
					}
				);

				// Scan through the extracted method body to find references to the closure object local
				//  and remap them to our new local variable
				for (int i = 0; i < filterInsns.Count; i++) {
					var insn = filterInsns[i];
					if (insn.Operand != closure)
						continue;

					if (insn.OpCode.Code != Code.Ldloc)
						throw new Exception ($"Invalid reference to closure: {insn}");

					// Remap the ldloc to our new local closure variable
					filterInsns[i] = Instruction.Create (OpCodes.Ldloc, (VariableDefinition)newClosureLocal);
				}

				CleanMethodBody (filterMethod, method, true);
			}
		}

		private class EhRange {
			public ExceptionHandler Handler;
			public int MinIndex, MaxIndex;
			public Instruction OldTryStart, OldTryEnd, OldHandlerStart, OldHandlerEnd, OldFilterStart;
			public Instruction NewTryStart, NewTryEnd, NewHandlerStart, NewHandlerEnd, NewFilterStart;
		}

		private EhRange FindRangeForOffset (List<EhRange> ranges, int offset)
		{
			foreach (var range in ranges) {
				if ((offset >= range.MinIndex) && (offset <= range.MaxIndex))
					return range;
			}

			return null;
		}

		private EhRange GetRangeForHandler (
			MethodBody body,
			RewriteContext context,
			ExceptionHandler eh
		)
		{
			var insns = body.Instructions;

			var range = new EhRange {
				Handler = eh,
				OldTryStart = eh.TryStart,
				OldTryEnd = eh.TryEnd,
				OldHandlerStart = eh.HandlerStart,
				OldHandlerEnd = eh.HandlerEnd,
				OldFilterStart = eh.FilterStart
			};

			int
				tryStartIndex = Find (context, insns, eh.TryStart),
				tryEndIndex = Find (context, insns, eh.TryEnd),
				handlerStartIndex = Find (context, insns, eh.HandlerStart),
				handlerEndIndex = Find (context, insns, eh.HandlerEnd);

			range.MinIndex = Math.Min (tryStartIndex, handlerStartIndex);
			range.MaxIndex = Math.Max (tryEndIndex, handlerEndIndex);

			return range;
		}

		private List<EhRange> GetRangesForMethodBody (
			MethodBody body,
			RewriteContext context,
			int? firstIndex = null, int? lastIndex = null
		)
		{
			var ranges = new List<EhRange> ();
			foreach (var eh in body.ExceptionHandlers) {
				var range = GetRangeForHandler (body, context, eh);

				// Skip any handlers that span or contain the region we're extracting
				if (firstIndex.HasValue && (range.MinIndex <= firstIndex.Value))
					continue;
				if (lastIndex.HasValue && (range.MaxIndex >= lastIndex.Value))
					continue;

				ranges.Add (range);
			}

			return ranges;
		}

		private Dictionary<object, object> ExtractRangeToMethod<T, U> (
			MethodDefinition sourceMethod, MethodDefinition targetMethod,
			ParameterDefinition fakeThis,
			int firstIndex, int lastIndex,
			Dictionary<object, object> variableMapping,
			Dictionary<T, U> typeMapping,
			RewriteContext context,
			Func<Instruction, EhRange, Instruction[]> filter = null
		)
			where T : TypeReference
			where U : TypeReference
		{
			var insns = sourceMethod.Body.Instructions;
			var targetInsns = targetMethod.Body.Instructions;

			foreach (var param in sourceMethod.Parameters) {
				if (variableMapping.ContainsKey (param))
					continue;

				// FIXME: A parameter being referenced when the range is extracted indicates that something went wrong,
				//  so this is for diagnostic purposes
				var newParam = new ParameterDefinition ("dead_" + param.Name, param.Attributes, param.ParameterType);
				variableMapping[param] = newParam;
			}

			foreach (var loc in sourceMethod.Body.Variables) {
				if (variableMapping.ContainsKey (loc))
					continue;
				var newLoc = new VariableDefinition (FilterTypeReference (loc.VariableType, typeMapping));
				targetMethod.Body.Variables.Add (newLoc);
				variableMapping[loc] = newLoc;
			}

			var ranges = GetRangesForMethodBody (sourceMethod.Body, context, firstIndex, lastIndex);

			var pairs = new Dictionary<Instruction, Instruction> ();
			var key = "extracted(" + targetMethod.DeclaringType?.Name + "." + targetMethod.Name + ") ";

			// Scan the range we're extracting and prepare to erase it after the clone
			// We need to do this now because the clone process can mutate the instructions being copied (yuck)
			for (int i = firstIndex; i <= lastIndex; i++) {
				var oldInsn = insns[i];

				if (oldInsn.OpCode.Code == Code.Nop)
					continue;

				var nopText = oldInsn.OpCode.Code == Code.Endfilter
					? "extracted endfilter"
					: key + oldInsn.ToString ();
				var newInsn = Nop (nopText);
				pairs.Add (oldInsn, newInsn);
			}

			CloneInstructions (
				sourceMethod, fakeThis, firstIndex, lastIndex - firstIndex + 1,
				targetInsns, variableMapping, typeMapping, ranges, filter
			);

			CleanMethodBody (targetMethod, sourceMethod, false);

			for (int i = firstIndex; i <= lastIndex; i++) {
				var oldInsn = insns[i];
				Instruction newInsn;
				if (!pairs.TryGetValue (oldInsn, out newInsn))
					continue;

				insns[i] = newInsn;
			}

			PatchMany (sourceMethod, context, pairs);

			// Copy over any exception handlers that were contained by the source range, remapping
			//  the start/end instructions of the handler and try blocks appropriately post-transform
			foreach (var range in ranges) {
				var newEh = new ExceptionHandler (range.Handler.HandlerType) {
					CatchType = range.Handler.CatchType,
					HandlerType = range.Handler.HandlerType,
					FilterStart = range.NewFilterStart,
					HandlerStart = range.NewHandlerStart,
					HandlerEnd = range.NewHandlerEnd,
					TryStart = range.NewTryStart,
					TryEnd = range.NewTryEnd
				};

				targetMethod.Body.ExceptionHandlers.Add (newEh);

				// Since the handler was copied over, we want to remove it from the source, 
				//  because replacing the source instruction range with nops has corrupted
				//  any remaining catch or filter blocks
				sourceMethod.Body.ExceptionHandlers.Remove (range.Handler);
				context.RemovedExceptionHandlers?.Add (range.Handler);
			}

			CleanMethodBody (targetMethod, sourceMethod, false);

			return variableMapping;
		}

		public class RewriteContext {
			public List<InstructionPair> Pairs;
			public List<ExcGroup> NewGroups = new List<ExcGroup> ();
			public List<FilterToInsert> FiltersToInsert = new List<FilterToInsert> ();
			public Dictionary<Instruction, Instruction> ReplacedInstructions = new Dictionary<Instruction, Instruction> ();
			public List<ExceptionHandler> RemovedExceptionHandlers = new List<ExceptionHandler> ();
			internal Queue<MethodDefinition> MethodQueue;
		}

		public class ExcGroup {
			private static int NextID = 0;

			public readonly int ID;
			public Instruction TryStart, TryEnd, TryEndPredecessor, OriginalTryEndPredecessor;
			public Instruction ExitPoint, OriginalExitPoint;
			public List<ExcBlock> Blocks = new List<ExcBlock> ();
			internal Instruction FirstPushInstruction;

			public ExcGroup ()
			{
				ID = NextID++;
			}

			public override string ToString ()
			{
				return $"Group #{ID}";
			}
		}

		public class ExcBlock {
			private static int NextID = 0;

			public readonly int ID;
			public bool IsCatchAll;

			public ExceptionHandler Handler;
			public MethodDefinition CatchMethod;
			public List<ParameterDefinition> CatchMethodRefParameters;
			public TypeDefinition FilterType;
			public FieldDefinition FilterField;
			public MethodDefinition FilterConstructor, FilterMethod;
			public MethodDefinition FilterActivationMethod, FilterDeactivationMethod;
			public Instruction FirstFilterInsn;
			public List<Instruction> LeaveTargets;
			public Dictionary<object, object> Mapping;
			public bool CanRethrow;

			public ExcBlock ()
			{
				ID = NextID++;
			}

			public override string ToString ()
			{
				return $"Handler #{ID} {(FilterMethod ?? CatchMethod).FullName}";
			}
		}

		public class InstructionPair {
			public class Comparer : IEqualityComparer<InstructionPair> {
				public bool Equals (InstructionPair lhs, InstructionPair rhs)
				{
					return lhs.Equals (rhs);
				}

				public int GetHashCode (InstructionPair ip)
				{
					return ip.GetHashCode ();
				}
			}

			public Instruction A, B;

			public InstructionPair (Instruction a, Instruction b) {
				A = a;
				B = b;
			}

			public override int GetHashCode ()
			{
				return (A?.GetHashCode () ^ B?.GetHashCode ()) ?? 0;
			}

			public bool Equals (InstructionPair rhs)
			{
				return (A == rhs.A) && (B == rhs.B);
			}

			public override bool Equals (object o)
			{
				var ip = o as InstructionPair;
				if (ip == null)
					return false;
				return Equals (ip);
			}

			public override string ToString ()
			{
				return $"{{{A} {B}}}";
			}
		}

		public class FilterToInsert {
			public TypeDefinition Type;
			public ExceptionHandler Handler;

			public bool Equals (FilterToInsert rhs)
			{
				return (Type == rhs.Type) && (Handler == rhs.Handler);
			}

			public override int GetHashCode ()
			{
				return (Type?.GetHashCode () ^ Handler?.GetHashCode ()) ?? 0;
			}

			public override bool Equals (object o)
			{
				var fk = o as FilterToInsert;
				if (fk == null)
					return false;
				return Equals (fk);
			}
		}

		private int RewriteMethod (MethodDefinition method, Queue<MethodDefinition> queue)
		{
			if (!method.HasBody)
				return 0;

			if (method.Body.ExceptionHandlers.Count == 0)
				return 0;

			if (!method.Body.ExceptionHandlers.Any (eh => eh.FilterStart != null))
				return 0;

			if (!Options.EnableGenerics) {
				if (method.HasGenericParameters || method.DeclaringType.HasGenericParameters) {
					var msg = $"Method {method.FullName} contains an exception filter and generics are disabled";
					if (Options.AbortOnError)
						throw new Exception (msg);

					Console.Error.WriteLine (msg);
					// If abortOnError is off we don't want to abort the rewrite operation, it's safe to skip the method
					return 0;
				}
			}

			if (Options.Audit) {
				Console.WriteLine ($"{method.FullName} contains {method.Body.ExceptionHandlers.Count (eh => eh.FilterStart != null)} exception filter(s)");
				return 0;
			}

			if (Options.Verbose)
				Console.WriteLine ($"// {method.FullName}...");

			try {
				RewriteMethodImpl (method, queue);
				return 0;
			} catch (Exception exc) {
				Console.Error.WriteLine ($"// Error rewriting {method.FullName}:");
				Console.Error.WriteLine (exc);
				Console.Error.WriteLine ();

				if (Options.AbortOnError)
					throw;
				else
					return 1;
			}
		}

		private readonly HashSet<string> TracedMethodNames = new HashSet<string> {
			// "DownloadFile",
			// "TestReturnValueWithFinallyAndDefault"
			"Lopsided"
		};

		private void RewriteMethodImpl (MethodDefinition method, Queue<MethodDefinition> queue)
		{
			if (TracedMethodNames.Contains(method.Name)) {
				Console.WriteLine ("== Original handlers ==");
				DumpExceptionHandlers (method);
			}

			// Clean up the method body and verify it before rewriting so that any existing violations of
			//  expectations aren't erroneously blamed on later transforms
			CleanMethodBody (method, null, true);

			var efilt = GetExceptionFilter (method.Module);
			var excType = GetException (method.Module);

			var fakeThis = method.IsStatic
				? null
				: new ParameterDefinition ("__this", ParameterAttributes.None, method.DeclaringType);

			var context = new RewriteContext {
				MethodQueue = queue
			};

			var filterReferencedVariables = new HashSet<VariableReference> ();
			var filterReferencedArguments = new HashSet<ParameterReference> ();
			CollectReferencedLocals (context, method, fakeThis, filterReferencedVariables, filterReferencedArguments);

			ClosureInfo closureInfo;
			if (!ClosureInfos.TryGetValue (method, out closureInfo)) {
				closureInfo = ConvertToClosure (
					method, fakeThis, filterReferencedVariables, filterReferencedArguments, context
				);
				ClosureInfos[method] = closureInfo;
			}

			CleanMethodBody (method, null, true);

			var insns = method.Body.Instructions;
			/*
			insns.Insert (0, Nop ("header"));
			insns.Append (Nop ("footer"));
			*/

			ExtractFiltersAndCatchBlocks (method, efilt, fakeThis, closureInfo, insns, context);

			// FIXME
			StripUnreferencedNops (method);

			CleanMethodBody (method, null, true);

			SortExceptionHandlers (method, context);

			// FIXME: Cecil currently throws inside the native PDB writer on methods we've modified
			//  presumably because we need to manually update the debugging information after removing
			//  instructions from the method body.
			method.DebugInformation = null;

			if (TracedMethodNames.Contains(method.Name)) {
				Console.WriteLine ("== New handlers ==");
				DumpExceptionHandlers (method);
			}
		}

		private void SortExceptionHandlers (MethodDefinition method, RewriteContext context)
		{
			var insns = method.Body.Instructions;
			// HACK: It's difficult to maintain correct exception handler sort order while rewriting,
			//  so instead, just sort all the exception handlers at the end.
			var sortedEhs = method.Body.ExceptionHandlers.ToList ();
			sortedEhs.Sort ((lhs, rhs) => {
				var firstIndexLhs = Find (context, insns, lhs.TryStart);
				var lastIndexLhs = Find (context, insns, lhs.TryEnd);
				var firstIndexRhs = Find (context, insns, rhs.TryStart);
				var lastIndexRhs = Find (context, insns, rhs.TryEnd);
				var result = firstIndexRhs.CompareTo (firstIndexLhs);
				if (result == 0)
					result = lastIndexLhs.CompareTo (lastIndexRhs);
				return result;
			});

			method.Body.ExceptionHandlers.Clear ();
			foreach (var eh in sortedEhs)
				method.Body.ExceptionHandlers.Add (eh);
		}

		private string FormatInstructionReference (MethodDefinition method, Dictionary<Instruction, int> offsets, Instruction insn) {
			if (insn == null)
				return "null";

			if (!offsets.ContainsKey (insn))
				return "invalid reference";

			var insns = method.Body.Instructions;
			var offsetBytes = offsets[insn];
			return $"IL_{offsetBytes:X4} {insn.OpCode} ({(insn.Operand == null ? "null" : insn.Operand)})";
		}

		private string FormatInstructionRange (MethodDefinition method, Dictionary<Instruction, int> offsets, Instruction first, Instruction afterLast) {
			int offset1, offset2;
			if (!offsets.TryGetValue (first, out offset1) ||
				!offsets.TryGetValue (afterLast, out offset2))
				return "invalid reference";

			return $"IL_{offset1:X4} - IL_{offset2:X4} ({offset2 - offset1} byte(s))";
		}

		private int GetOffsetOfInstruction (Collection<Instruction> instructions, Instruction i) {
			var offset = 0;
			foreach (var insn in instructions) {
				if (insn == i)
					return offset;
				offset += insn.GetSize ();
			}

			return -1;
		}

		private void DumpExceptionHandlers (MethodDefinition method) {
			var offsets = new Dictionary<Instruction, int> ();
			var offset = 0;
			foreach (var insn in method.Body.Instructions) {
				offsets[insn] = offset;
				offset += insn.GetSize ();
			}

			foreach (var eh in method.Body.ExceptionHandlers) {
				Console.WriteLine ($"#{eh.GetHashCode():X8}     {eh.HandlerType} {eh.CatchType}");
				Console.WriteLine ($"try           {FormatInstructionRange (method, offsets, eh.TryStart, eh.TryEnd)}");
				Console.WriteLine ($"try start     {FormatInstructionReference (method, offsets, eh.TryStart)}");
				Console.WriteLine ($"try end       {FormatInstructionReference (method, offsets, eh.TryEnd)}");
				if (eh.FilterStart != null) {
					Console.WriteLine ($"filter        {FormatInstructionRange (method, offsets, eh.FilterStart, eh.HandlerStart)}");
					Console.WriteLine ($"filter start  {FormatInstructionReference (method, offsets, eh.FilterStart)}");
				}
				Console.WriteLine ($"handler       {FormatInstructionRange (method, offsets, eh.HandlerStart, eh.HandlerEnd)}");
				Console.WriteLine ($"handler start {FormatInstructionReference (method, offsets, eh.HandlerStart)}");
				Console.WriteLine ($"handler end   {FormatInstructionReference (method, offsets, eh.HandlerEnd)}");
				Console.WriteLine ();
			}
		}

		private void StripUnreferencedNops (MethodDefinition method) {
			// NOTE: This method may seem cosmetic but stripping unreferenced nops is necessary
			//  to ensure that stray nops do not remain at the end of try and finally blocks
			// If stray nops remain there it becomes fallthrough at the end of the block
			var referenced = new HashSet<Instruction> ();

			foreach (var eh in method.Body.ExceptionHandlers) {
				referenced.Add (eh.HandlerStart);
				referenced.Add (eh.HandlerEnd);
				referenced.Add (eh.FilterStart);
				referenced.Add (eh.TryStart);
				referenced.Add (eh.TryEnd);
			}

			var insns = method.Body.Instructions;
			foreach (var insn in insns) {
				var operand = insn.Operand as Instruction;
				if (operand != null)
					referenced.Add (operand);

				var operands = insn.Operand as Instruction[];
				if (operands != null)
					foreach (var i in operands)
						referenced.Add (i);
			}

			var old = insns.ToArray ();
			insns.Clear ();

			foreach (var insn in old) {
				if (insn.OpCode == OpCodes.Nop && !referenced.Contains (insn))
					continue;

				insns.Add (insn);
			}
		}

		private static ILookup<InstructionPair, ExceptionHandler> GetHandlersByTry (MethodDefinition method)
		{
			return method.Body.ExceptionHandlers.ToLookup (
				eh => {
					var p = new InstructionPair(eh.TryStart, eh.TryEnd);
					return p;
				},
				new InstructionPair.Comparer ()
			);
		}

		private static IOrderedEnumerable<IGrouping<InstructionPair, ExceptionHandler>> GetOrderedHandlers (MethodDefinition method)
		{
			var handlersByTry = GetHandlersByTry (method);

			return handlersByTry.OrderBy (g => {
				return g.Key.B.Offset - g.Key.A.Offset;
			});
		}

		private void ComputeExitPoint (ExcGroup excGroup)
		{
			switch (excGroup.OriginalTryEndPredecessor.OpCode.Code) {
				case Code.Throw:
				case Code.Rethrow:
					excGroup.ExitPoint =
						excGroup.OriginalExitPoint = null;
					break;
				case Code.Leave:
				case Code.Leave_S:
					excGroup.ExitPoint = (excGroup.TryEndPredecessor.Operand as Instruction) ??
						(excGroup.OriginalTryEndPredecessor.Operand as Instruction) ??
						excGroup.ExitPoint;
					excGroup.OriginalExitPoint = (excGroup.OriginalTryEndPredecessor.Operand as Instruction) ??
						excGroup.OriginalExitPoint;
					break;
				default:
					throw new Exception ("Try block does not end with a leave or throw instruction");
					break;
			}
		}

		private Instruction Branch (RewriteContext ctx, OpCode opCode, Instruction target)
		{
			Instruction newTarget;
			while (ctx.ReplacedInstructions.TryGetValue (target, out newTarget))
				target = newTarget;

			return Instruction.Create (opCode, target);
		}

		private int Find (RewriteContext ctx, Collection<Instruction> insns, Instruction instruction)
		{
			var result = insns.IndexOf (instruction);

			if (result < 0) {
				Instruction newInstruction;
				while (ctx.ReplacedInstructions.TryGetValue (instruction, out newInstruction)) {
					result = insns.IndexOf (newInstruction);
					if (result >= 0)
						return result;
					instruction = newInstruction;
				}
			}

			return result;
		}

		private void ExtractFiltersAndCatchBlocks (
			MethodDefinition method, TypeReference efilt,
			ParameterDefinition fakeThis, ClosureInfo closureInfo,
			Collection<Instruction> insns, RewriteContext context
		)
		{
			var closure = closureInfo.ClosureStorage;
			var handlers = GetOrderedHandlers (method).ToList ();
			var groupsContainingFilters = handlers.Where (g => g.Any(h => h.HandlerType == ExceptionHandlerType.Filter))
				.ToList ();
			var pairs = (from k in groupsContainingFilters select k.Key).ToList ();
			context.Pairs = pairs;

			var deadHandlers = new List<ExceptionHandler> ();

			// FIXME: In some methods a try { } catch (...) { } catch { } finally { } chain
			//  will not be grouped up properly and we'll end up inserting our try/finally in 
			//  the middle of it instead of at the end, because the finally roslyn generates is
			//  after the chain of catches
			var excGroups = (from @group in groupsContainingFilters
							 let a = @group.Key.A
							 let b = @group.Key.B
							 let startIndex = Find (context, insns, a)
							 let endIndex = Find (context, insns, b)
							 let predecessor = insns[endIndex - 1]
							 let size = endIndex - startIndex
							 orderby size ascending
							 orderby endIndex descending
							 select new {
								 @group,
								 excGroup = new ExcGroup {
									 TryStart = a,
									 OriginalTryEndPredecessor = predecessor,
									 TryEndPredecessor = predecessor,
									 TryEnd = b,
								 },
								 startIndex,
								 endIndex,
								 size
							 }).ToList ();


			if (TracedMethodNames.Contains (method.Name))
				;

			foreach (var eg in excGroups)
				context.NewGroups.Add (eg.excGroup);

			foreach (var eg in excGroups) {
				var excGroup = eg.excGroup;
				ComputeExitPoint (excGroup);

				foreach (var eh in eg.group) {
					var catchBlock = ExtractCatch (method, eh, closureInfo, fakeThis, excGroup, context);
					excGroup.Blocks.Add (catchBlock);

					if (eh.FilterStart != null)
						ExtractFilter (method, eh, closureInfo, fakeThis, excGroup, context, catchBlock);
				}

				// If we generated no actual catch methods for this group, don't generate a try/catch for it since it'll just
				//  be empty. Don't instantiate a filter either
				if (excGroup.Blocks.All (b => b.CatchMethod == null)) {
					// We still need to remove the exception handlers that previously existed here because they're now
					//  filled with nops and thus invalid
					foreach (var eh in eg.@group)
						method.Body.ExceptionHandlers.Remove (eh);

					// Any filter types associated with dead handlers are unused and should be removed
					foreach (var b in excGroup.Blocks) {
						if (b.FilterType == null)
							continue;

						b.FilterType.DeclaringType.NestedTypes.Remove (b.FilterType);
						closureInfo.TypeDefinition.Fields.Remove (b.FilterField);
					}

					continue;
				}

				var newHandler = new List<Instruction> ();
				var newStart = Nop (
					"Constructed handler start for handlers " +
					string.Join (", ", (from b in excGroup.Blocks select b.Handler.GetHashCode ().ToString ("X8")))
				);
				// Insert a generated NOP at the end of the constructed handler to use as the HandlerEnd.
				// This is simpler and more reliable than trying to pick an existing instruction to use.
				var newEnd = Nop ("Constructed handler end marker");

				newHandler.Add (newStart);

				// The first step is to evaluate all the active exception filters, before doing anything else.
				// This will run all of them (if necessary) and update their result field so we can select which
				//  catch block(s) to run.
				newHandler.Add (Instruction.Create (OpCodes.Dup));
				newHandler.Add (Instruction.Create (OpCodes.Call, new MethodReference (
						"PerformEvaluate", method.Module.TypeSystem.Void, efilt
				) { HasThis = false, Parameters = {
						new ParameterDefinition (method.Module.TypeSystem.Object)
				} }));

				// Next we need to deactivate all the filters we activated before continuing. It's important to 
				//  do this here instead of in a finally block because the catch block itself could throw, at 
				//  which point our filters would erroneously be active.
				// We must reverse the filter list for deactivation so that the push/pop operations line up.
				var relevantFilters = excGroup.Blocks.Where (b => b.FilterType != null);
				DeactivateFilters (closureInfo, relevantFilters.Reverse (), newHandler);

				ConstructNewExceptionHandler (method, eg.@group, excGroup, newHandler, closure, excGroup.ExitPoint, context);
				newHandler.Add (newEnd);

				var targetIndex = Find (context, insns, excGroup.TryEndPredecessor);
				if (targetIndex < 0)
					throw new Exception ("Failed to find TryEndPredecessor");
				targetIndex += 1;

				InsertOps (insns, targetIndex, newHandler.ToArray ());

				var endOffset = Find (context, insns, newHandler[newHandler.Count - 1]);
				// HACK: This ensures that we don't leave any stray nops at the end of a try block
				//  (that were previously actual instructions) because that will be treated as fallthrough
				endOffset = FindNextNonNop (insns, endOffset);

				var newTryStart = excGroup.TryStart;

				// Ensure we initialize and activate all exception filters for the try block before entering it
				var activateInstructions = new List<Instruction> ();
				foreach (var eh in relevantFilters)
					ActivateExceptionFilter (method, eh, closure, activateInstructions);

				var insertOffset = Find (context, insns, excGroup.TryStart);
				var originalInstructionAtOffset = insns[insertOffset];
				InsertOps (insns, insertOffset, activateInstructions.ToArray());
				Patch (method, context, originalInstructionAtOffset, activateInstructions[0]);
				newTryStart = activateInstructions[0];

				var catchEh = new ExceptionHandler (ExceptionHandlerType.Catch) {
					CatchType = method.Module.TypeSystem.Object,
					HandlerStart = newStart,
					HandlerEnd = newEnd,
					TryStart = newTryStart,
					TryEnd = newStart,
					FilterStart = null
				};

				method.Body.ExceptionHandlers.Add (catchEh);

				foreach (var eh in eg.@group)
					method.Body.ExceptionHandlers.Remove (eh);

				// Now that we've constructed our new catch block and updated everything, scan back through the
				//  try block and insert filter deactivation before every leave instruction that exits the block.
				var scratch = new List<Instruction> ();
				FilterRange (
					method, Find (context, insns, newTryStart), Find (context, insns, newStart) - 1, context,
					(insn) => {
						if ((insn.OpCode.Code != Code.Leave) && (insn.OpCode.Code != Code.Leave_S))
							return null;

						var target = (Instruction)insn.Operand;
						scratch.Clear ();
						DeactivateFilters (closureInfo, relevantFilters, scratch);
						scratch.Add (Branch (context, OpCodes.Leave, target));
						return scratch.ToArray ();
					}
				);
			}

			foreach (var eg in context.NewGroups) {
				foreach (var eb in eg.Blocks) {
					if (eb.FilterMethod != null)
						StripUnreferencedNops (eb.FilterMethod);
					if (eb.CatchMethod != null)
						StripUnreferencedNops (eb.CatchMethod);
				}
			}
		}

		private void DeactivateFilters (ClosureInfo closureInfo, IEnumerable<ExcBlock> filters, List<Instruction> output) {
			foreach (var filter in filters) {
				output.Add (GenerateLoad (closureInfo.ClosureStorage));
				output.Add (Instruction.Create (OpCodes.Call, filter.FilterDeactivationMethod));
			}
		}

		private int FindNextNonNop (Collection<Instruction> insns, int offset) {
			for (int i = offset + 1; i < insns.Count; i++) {
				if (insns[i].OpCode == OpCodes.Nop)
					continue;

				return i;
			}

			return -1;
		}

		private void ActivateExceptionFilter (
			MethodDefinition method,
			ExcBlock eh,
			object closureVariable,
			List<Instruction> output
		)
		{
			var filterType = eh.FilterType;
			var efilt = GetExceptionFilter (method.Module);

			output.Add (GenerateLoad (closureVariable));
			output.Add (Instruction.Create (OpCodes.Call, eh.FilterActivationMethod));
		}

		private void ConstructNewExceptionHandler (
			MethodDefinition method, IGrouping<InstructionPair, ExceptionHandler> group,
			ExcGroup excGroup, List<Instruction> newInstructions, object closureVar,
			Instruction exitPoint, RewriteContext context
		)
		{
			// FIXME
			if (excGroup.Blocks.All (b => b.CatchMethod == null))
				throw new Exception ("No actual catches were extracted");

			var excVar = new VariableDefinition (method.Module.TypeSystem.Object);
			method.Body.Variables.Add (excVar);

			newInstructions.Add (Instruction.Create (OpCodes.Stloc, excVar));

			excGroup.Blocks.Sort (
				(lhs, rhs) => {
					var l = (lhs.FilterMethod != null) ? 0 : 1;
					var r = (rhs.FilterMethod != null) ? 0 : 1;
					return l.CompareTo (r);
				}
			);

			var hasFallthrough = excGroup.Blocks.Any (h => h.FilterMethod == null);
			bool rethrowRequired = !hasFallthrough;
			var efilt = GetExceptionFilter (method.Module);

			foreach (var h in excGroup.Blocks) {
				// FIXME
				if (h.CatchMethod == null)
					continue;

				var callCatchPrologue = Nop ("Before call catch " + h.CatchMethod.Name);
				var callCatchEpilogue = Nop ("After call catch " + h.CatchMethod.Name);

				newInstructions.Add (callCatchPrologue);

				if (h.FilterMethod != null) {
					// Invoke the filter method and skip past the catch if it rejected the exception
					var callFilterInsn = Instruction.Create (OpCodes.Call, h.FilterMethod);
					newInstructions.Add (GenerateLoad (closureVar));
					newInstructions.Add (Instruction.Create (OpCodes.Ldfld, h.FilterField));
					newInstructions.Add (Instruction.Create (OpCodes.Castclass, efilt));
					newInstructions.Add (Instruction.Create (OpCodes.Ldloc, excVar));
					var mref = new MethodReference (
						"ShouldRunHandler", method.Module.TypeSystem.Boolean, efilt
					) {
						HasThis = true,
						Parameters = {
							new ParameterDefinition (method.Module.TypeSystem.Object)
						}
					};
					newInstructions.Add (Instruction.Create (OpCodes.Call, method.Module.ImportReference (mref)));
					newInstructions.Add (Instruction.Create (OpCodes.Brfalse, callCatchEpilogue));
				} else if (!h.IsCatchAll) {
					// Skip past the catch if the exception is not an instance of the catch type
					newInstructions.Add (Instruction.Create (OpCodes.Ldloc, excVar));
					newInstructions.Add (Instruction.Create (OpCodes.Isinst, h.Handler.CatchType));
					newInstructions.Add (Instruction.Create (OpCodes.Brfalse, callCatchEpilogue));
				} else {
					// Never skip the catch, it's a catch-all block.
				}

				if (!h.CatchMethod.IsStatic)
					newInstructions.Add (Instruction.Create (OpCodes.Ldarg_0));

				newInstructions.Add (Instruction.Create (OpCodes.Ldloc, excVar));
				if (h.Handler.CatchType != null)
					newInstructions.Add (Instruction.Create (OpCodes.Castclass, h.Handler.CatchType));

				newInstructions.Add (GenerateLoad (closureVar));

				// Any ref/out params need to be passed by-address into the catch block
				foreach (var param in h.CatchMethodRefParameters)
					newInstructions.Add (Instruction.Create (OpCodes.Ldarg, param));

				newInstructions.Add (Instruction.Create (OpCodes.Call, h.CatchMethod));

				// Either rethrow or leave depending on the value returned by the handler
				var rethrow = Instruction.Create (OpCodes.Rethrow);

				if ((h.LeaveTargets.Count == 1) && !h.CanRethrow) {
					// The handler only has one possible leave target and contains no rethrow instructions so
					//  we can just emit a constant leave
					newInstructions.Add (Branch (context, OpCodes.Leave, h.LeaveTargets[0]));
				} else if (h.LeaveTargets.Count > 0) {
					// Create instructions for handling each possible leave target (in addition to 0 which is rethrow)
					int rethrowOffset = 1;
					var switchTargets = new Instruction[h.LeaveTargets.Count + rethrowOffset];
					switchTargets[0] = rethrow;

					for (int l = 0; l < h.LeaveTargets.Count; l++)
						switchTargets[l + rethrowOffset] = Branch (context, OpCodes.Leave, h.LeaveTargets[l]);

					// Use the return value from the handler to select one of the targets we just created
					newInstructions.Add (Instruction.Create (OpCodes.Switch, switchTargets));

					// After the fallback, add each of our leave targets.
					// These need to be after the fallthrough so they aren't hit unless targeted by the switch
					// It's okay to add them by themselves since they're all either rethrow or leave opcodes.
					foreach (var st in switchTargets)
						newInstructions.Add (st);
				} else {
					// If there are no leave targets the only possible outcome is rethrow so just emit that
					newInstructions.Add (rethrow);
				}

				newInstructions.Add (callCatchEpilogue);
			}

			newInstructions.Add (
				(exitPoint != null) && !rethrowRequired
					? Branch (context, OpCodes.Leave, exitPoint)
					: Instruction.Create (OpCodes.Rethrow)
			);
		}

		private void CollectReferencedLocals (
			RewriteContext context, MethodDefinition method, ParameterDefinition fakeThis,
			HashSet<VariableReference> referencedVariables, HashSet<ParameterReference> referencedArguments
		)
		{
			foreach (var eh in method.Body.ExceptionHandlers) {
				if (eh.FilterStart != null)
					CollectReferencedLocals (context, method, fakeThis, eh.FilterStart, eh.HandlerStart, referencedVariables, referencedArguments);

				// Also collect anything referenced by handlers because they get hoisted out
				// FIXME: Only do this to blocks that have a filter hanging off them
				CollectReferencedLocals (context, method, fakeThis, eh.HandlerStart, eh.HandlerEnd, referencedVariables, referencedArguments);
			}
		}

		private void CollectReferencedLocals (
			RewriteContext context, MethodDefinition method, ParameterDefinition fakeThis, Instruction first, Instruction last,
			HashSet<VariableReference> referencedVariables, HashSet<ParameterReference> referencedArguments
		)
		{
			var insns = method.Body.Instructions;
			int i = Find (context, insns, first), i2 = Find (context, insns, last);
			if ((i < 0) || (i2 < 0))
				throw new ArgumentException ("First and/or last instruction(s) not found in method body");

			for (; i <= i2; i++) {
				var insn = insns[i];
				var vd = (insn.Operand as VariableReference)
					?? LookupNumberedVariable (insn.OpCode.Code, method.Body.Variables);
				var pd = insn.Operand as ParameterDefinition
					?? LookupNumberedArgument (insn.OpCode.Code, fakeThis, method.Parameters);

				// FIXME
				if (vd?.VariableType.FullName.Contains ("__closure") ?? false)
					continue;
				if (pd?.Name.Contains ("__closure") ?? false)
					continue;

				if (vd != null)
					referencedVariables.Add (vd);
				if (pd != null)
					referencedArguments.Add (pd);
			}
		}

		private void CheckInRange (Instruction insn, MethodDefinition method, MethodDefinition oldMethod, string context = "")
		{
			if (insn == null)
				return;

			// FIXME
			if (true) {
				if (method.Body.Instructions.IndexOf (insn) < 0)
					throw new Exception ($"Instruction {insn} is missing from method {method.FullName} {context}");
				else if (oldMethod != null && oldMethod.Body.Instructions.IndexOf (insn) >= 0)
					throw new Exception ($"Instruction {insn} is present in old method for method {method.FullName} {context}");
			}
		}

		private void CleanMethodBody (MethodDefinition method, MethodDefinition oldMethod, bool verify)
		{
			var insns = method.Body.Instructions;

			for (int idx = 0; idx < insns.Count; idx++) {
				var i = insns[idx];
				if (i.OpCode.Code == Code.Nop)
					continue;

				OpCode newOpcode;
				if (ShortFormRemappings.TryGetValue (i.OpCode.Code, out newOpcode))
					i.OpCode = newOpcode;

				switch (i.OpCode.Code) {
					case Code.Rethrow: {
							bool foundRange = false;

							foreach (var eh in method.Body.ExceptionHandlers) {
								if (eh.HandlerType == ExceptionHandlerType.Finally)
									continue;

								int startIndex = insns.IndexOf (eh.HandlerStart),
									endIndex = insns.IndexOf (eh.HandlerEnd);

								if ((idx >= startIndex) && (idx <= endIndex)) {
									foundRange = true;
									break;
								}
							}

							if (!foundRange && false)
								throw new Exception ($"Found rethrow instruction outside of catch block: {i}");

							break;
						}
				}

				if (verify)
					VerifyInstruction (method, oldMethod, i);
			}

			if (verify)
				VerifyMethodBody (method, oldMethod, insns);
		}

		private void VerifyMethodBody (MethodDefinition method, MethodDefinition oldMethod, Collection<Instruction> insns)
		{
			foreach (var p in method.Parameters)
				if (p.Index != method.Parameters.IndexOf (p))
					throw new Exception ($"parameter index mismatch for method {method.FullName}");

			foreach (var v in method.Body.Variables)
				if (v.Index != method.Body.Variables.IndexOf (v))
					throw new Exception ($"variable index mismatch for method {method.FullName}");

			foreach (var eh in method.Body.ExceptionHandlers) {
				CheckInRange (eh.HandlerStart, method, oldMethod, "(handler start)");
				CheckInRange (eh.HandlerEnd, method, oldMethod, "(handler end)");
				CheckInRange (eh.FilterStart, method, oldMethod, "(filter start)");
				CheckInRange (eh.TryStart, method, oldMethod, "(try start)");
				CheckInRange (eh.TryEnd, method, oldMethod, "(try end)");

				if (eh.TryStart != null) {
					var tryStartIndex = insns.IndexOf (eh.TryStart);
					var tryEndIndex = insns.IndexOf (eh.TryEnd);
					if (tryEndIndex <= tryStartIndex)
						throw new Exception ($"Try block contains no instructions at {eh.TryStart}");
				}
			}
		}

		private void VerifyInstruction (MethodDefinition method, MethodDefinition oldMethod, Instruction i)
		{
			var opInsn = i.Operand as Instruction;
			var opInsns = i.Operand as Instruction[];
			var opArg = i.Operand as ParameterDefinition;
			var opVar = i.Operand as VariableDefinition;

			if (opInsn != null) {
				CheckInRange (opInsn, method, oldMethod, $"(operand of {i.OpCode} instruction)");

				/*
				if ((i.OpCode.Code == Code.Leave) || (i.OpCode.Code == Code.Leave_S)) {
					if (renumber && (i.Offset > opInsn.Offset))
						throw new Exception ($"Leave target {opInsn} precedes leave instruction");
				}
				*/
			} else if (opArg != null) {
				if ((opArg.Name == "__this") && method.HasThis) {
					// HACK: method.Body.ThisParameter is unreliable for confusing reasons, and isn't
					//  present in .Parameters so just ignore the check here
				} else if (method.Parameters.IndexOf (opArg) < 0) {
					throw new Exception ($"Parameter {opArg.Name} for opcode {i} is missing for method {method.FullName}");
				} else if (oldMethod != null && oldMethod.Parameters.IndexOf (opArg) >= 0)
					throw new Exception ($"Parameter {opArg.Name} for opcode {i} is present in old method for method {method.FullName}");
			} else if (opVar != null) {
				if (method.Body.Variables.IndexOf (opVar) < 0)
					throw new Exception ($"Local {opVar} for opcode {i} is missing for method {method.FullName}");
				else if (oldMethod != null && oldMethod.Body.Variables.IndexOf (opVar) >= 0)
					throw new Exception ($"Local {opVar} for opcode {i} is present in old method for method {method.FullName}");
			} else if (opInsns != null) {
				foreach (var target in opInsns)
					CheckInRange (target, method, oldMethod, $"(operand of {i.OpCode} instruction)");
			}
		}

		private Instruction CreateRemappedInstruction (
			object oldOperand, OpCode oldCode, object operand
		)
		{
			if (operand == null)
				throw new ArgumentNullException ("operand");

			OpCode code = oldCode;
			if (
				(operand != null) &&
				(oldOperand != null) &&
				(operand.GetType () != oldOperand.GetType ())
			) {
				if (!LocalParameterRemappings.TryGetValue (oldCode.Code, out code))
					throw new Exception (oldCode.ToString ());
			}

			if (operand is ParameterDefinition)
				return Instruction.Create (code, (ParameterDefinition)operand);
			else if (operand is VariableDefinition)
				return Instruction.Create (code, (VariableDefinition)operand);
			else
				throw new Exception (operand.ToString ());
		}

		private object RemapOperandForClone<T, U> (
			object operand,
			Dictionary<object, object> variableMapping,
			Dictionary<T, U> typeMapping
		)
			where T : TypeReference
			where U : TypeReference
		{
			object newOperand = operand;
			if (variableMapping != null && variableMapping.ContainsKey (operand))
				newOperand = variableMapping[operand];
			else if (typeMapping != null) {
				var operandTr = operand as T;
				if (operandTr != null && typeMapping.ContainsKey (operandTr))
					newOperand = typeMapping[operandTr];
			}
			return newOperand;
		}

		private Instruction CloneInstruction<T, U> (
			Instruction i,
			ParameterDefinition fakeThis,
			MethodDefinition method,
			Dictionary<object, object> variableMapping,
			Dictionary<T, U> typeMapping
		)
			where T : TypeReference
			where U : TypeReference
		{
			object operand = i.Operand ??
				(object)LookupNumberedVariable (i.OpCode.Code, method.Body.Variables) ??
				(object)LookupNumberedArgument (i.OpCode.Code, fakeThis, method.Parameters);

			var code = i.OpCode;
			if (Denumberings.ContainsKey (i.OpCode.Code))
				code = Denumberings[i.OpCode.Code];

			if (operand == null)
				return Instruction.Create (code);

			var newOperand = RemapOperandForClone (operand, variableMapping, typeMapping);

			if (code.Code == Code.Nop) {
				var result = Instruction.Create (OpCodes.Nop);
				// HACK: Manually preserve any operand that was tucked inside the nop for bookkeeping
				result.Operand = operand;
				return result;
			} else if (code.Code == Code.Rethrow) {
				var result = Instruction.Create (OpCodes.Rethrow);
				// HACK: Manually preserve any operand that was tucked inside the rethrow for bookkeeping
				result.Operand = operand;
				return result;
			} else if (newOperand is FieldReference) {
				FieldReference fref = newOperand as FieldReference;
				return Instruction.Create (code, fref);
			} else if (newOperand is TypeReference) {
				TypeReference tref = newOperand as TypeReference;
				return Instruction.Create (code, tref);
			} else if (newOperand is TypeDefinition) {
				TypeDefinition tdef = newOperand as TypeDefinition;
				return Instruction.Create (code, tdef);
			} else if (newOperand is MethodReference) {
				MethodReference mref = newOperand as MethodReference;
				return Instruction.Create (code, mref);
			} else if (newOperand is Instruction) {
				var insn = newOperand as Instruction;
				return Instruction.Create (code, insn);
			} else if (newOperand is string) {
				var s = newOperand as string;
				return Instruction.Create (code, s);
			} else if (newOperand is VariableReference) {
				var v = newOperand as VariableReference;
				if (operand.GetType () != v.GetType ())
					return CreateRemappedInstruction (operand, code, newOperand);
				else
					return Instruction.Create (code, (VariableDefinition)v);
			} else if (newOperand is ParameterDefinition) {
				var p = newOperand as ParameterDefinition;
				if (operand.GetType () != p.GetType ())
					return CreateRemappedInstruction (operand, code, newOperand);
				else
					return Instruction.Create (code, p);
			} else if ((newOperand != null) && (newOperand.GetType ().IsValueType)) {
				var m = typeof (Instruction).GetMethod ("Create", new Type[] {
					code.GetType(), newOperand.GetType()
				});
				if (m == null)
					throw new Exception ("Could not find Instruction.Create overload for operand " + newOperand.GetType ().Name);
				return (Instruction)m.Invoke (null, new object[] {
					code, newOperand
				});
			} else if (newOperand is Instruction[]) {
				var insns = (Instruction[])newOperand;
				var newInsns = new Instruction[insns.Length];
				insns.CopyTo (newInsns, 0);
				return Instruction.Create (code, newInsns);
			} else if (newOperand != null) {
				throw new NotImplementedException (i.OpCode.ToString () + " " + newOperand.GetType ().FullName);
			} else {
				throw new NotImplementedException (i.OpCode.ToString ());
			}
		}

		private void CloneInstructions<T, U> (
			MethodDefinition sourceMethod,
			ParameterDefinition fakeThis,
			int sourceIndex, int count,
			Mono.Collections.Generic.Collection<Instruction> target,
			Dictionary<object, object> variableMapping,
			Dictionary<T, U> typeMapping,
			List<EhRange> ranges,
			Func<Instruction, EhRange, Instruction[]> filter
		)
			where T : TypeReference
			where U : TypeReference
		{
			var sourceInsns = sourceMethod.Body.Instructions;

			if (sourceIndex < 0)
				throw new ArgumentOutOfRangeException ("sourceIndex");

			var mapping = new Dictionary<Instruction, Instruction> ();

			for (int n = 0; n < count; n++) {
				var absoluteIndex = n + sourceIndex;
				var insn = sourceInsns[absoluteIndex];
				var newInsn = CloneInstruction (insn, fakeThis, sourceMethod, variableMapping, typeMapping);

				if (filter != null) {
					var range = FindRangeForOffset (ranges, absoluteIndex);

					var filtered = filter (newInsn, range);
					if (filtered != null) {
						mapping[insn] = filtered.First ();

						foreach (var filteredInsn in filtered)
							target.Add (filteredInsn);

						UpdateRangeReferences (ranges, insn, filtered.First (), filtered.Last ());
					} else {
						mapping[insn] = newInsn;

						target.Add (newInsn);

						UpdateRangeReferences (ranges, insn, newInsn, newInsn);
					}
				} else {
					mapping[insn] = newInsn;
					target.Add (newInsn);

					UpdateRangeReferences (ranges, insn, newInsn, newInsn);
				}
			}

			// Fixup branches
			for (int i = 0; i < target.Count; i++) {
				var insn = target[i];
				var operand = insn.Operand as Instruction;
				var operands = insn.Operand as Instruction[];

				if (operand != null) {
					Instruction newOperand, newInsn;
					if (!mapping.TryGetValue (operand, out newOperand)) {
						if (insn.OpCode.Code != Code.Nop)
							throw new Exception ("Could not remap instruction operand for " + insn);
					} else {
						insn.Operand = newOperand;
					}
				} else if (operands != null) {
					var newOperands = new Instruction[operands.Length];
					for (int j = 0; j < operands.Length; j++) {
						Instruction newElt, elt = operands[j];
						if (!mapping.TryGetValue (elt, out newElt))
							throw new Exception ($"Switch target {elt} not found in table of cloned instructions");

						var index = target.IndexOf (newElt);
						if (index < 0)
							throw new Exception ($"New switch target {newElt} not found in output method");
						newOperands[j] = newElt;
					}
					insn.Operand = newOperands;
				} else
					continue;
			}
		}

		private void UpdateRangeReference (ref Instruction target, Instruction compareWith, Instruction oldInsn, Instruction newInsn)
		{
			if (oldInsn != compareWith)
				return;

			target = newInsn;
		}

		private void UpdateRangeReferences (List<EhRange> ranges, Instruction oldInsn, Instruction firstNewInsn, Instruction lastNewInsn)
		{
			foreach (var range in ranges) {
				UpdateRangeReference (ref range.NewTryStart, range.OldTryStart, oldInsn, firstNewInsn);
				UpdateRangeReference (ref range.NewHandlerStart, range.OldHandlerStart, oldInsn, firstNewInsn);
				UpdateRangeReference (ref range.NewFilterStart, range.OldFilterStart, oldInsn, firstNewInsn);
				UpdateRangeReference (ref range.NewTryEnd, range.OldTryEnd, oldInsn, lastNewInsn);
				UpdateRangeReference (ref range.NewHandlerEnd, range.OldHandlerEnd, oldInsn, lastNewInsn);
			}
		}
	}
}
