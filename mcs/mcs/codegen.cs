//
// codegen.cs: The code generator
//
// Authors:
//   Miguel de Icaza (miguel@ximian.com)
//   Marek Safar (marek.safar@gmail.com)
//
// Copyright 2001, 2002, 2003 Ximian, Inc.
// Copyright 2004 Novell, Inc.
//

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Mono.CSharp {

	/// <summary>
	///    Code generator class.
	/// </summary>
	public class CodeGen {
		static AppDomain current_domain;

		// Breaks dynamic and repl
		public static AssemblyClass Assembly;

		static CodeGen ()
		{
			Reset ();
		}

		public static void Reset ()
		{
			Assembly = new AssemblyClass ();
		}

		public static string Basename (string name)
		{
			int pos = name.LastIndexOf ('/');

			if (pos != -1)
				return name.Substring (pos + 1);

			pos = name.LastIndexOf ('\\');
			if (pos != -1)
				return name.Substring (pos + 1);

			return name;
		}

		public static string Dirname (string name)
		{
			int pos = name.LastIndexOf ('/');

			if (pos != -1)
				return name.Substring (0, pos);

			pos = name.LastIndexOf ('\\');
			if (pos != -1)
				return name.Substring (0, pos);

			return ".";
		}

		static public string FileName;
				
		//
		// Initializes the code generator variables for interactive use (repl)
		//
		static public void InitDynamic (CompilerContext ctx, string name)
		{
			current_domain = AppDomain.CurrentDomain;
			AssemblyName an = Assembly.GetAssemblyName (name, name);
			
			Assembly.Builder = current_domain.DefineDynamicAssembly (an, AssemblyBuilderAccess.Run);
			RootContext.ToplevelTypes = new ModuleCompiled (ctx, true);
			RootContext.ToplevelTypes.Builder = Assembly.Builder.DefineDynamicModule (Basename (name), false);
			Assembly.Name = Assembly.Builder.GetName ();
		}

		//
		// Initializes the code generator variables
		//
		static public bool Init (string name, string output, bool want_debugging_support, CompilerContext ctx)
		{
			FileName = output;
			AssemblyName an = Assembly.GetAssemblyName (name, output);
			if (an == null)
				return false;

			if (an.KeyPair != null) {
				// If we are going to strong name our assembly make
				// sure all its refs are strong named
				foreach (Assembly a in ctx.GlobalRootNamespace.Assemblies) {
					AssemblyName ref_name = a.GetName ();
					byte [] b = ref_name.GetPublicKeyToken ();
					if (b == null || b.Length == 0) {
						ctx.Report.Error (1577, "Assembly generation failed " +
								"-- Referenced assembly '" +
								ref_name.Name +
								"' does not have a strong name.");
						//Environment.Exit (1);
					}
				}
			}
			
			current_domain = AppDomain.CurrentDomain;

			try {
				Assembly.Builder = current_domain.DefineDynamicAssembly (an,
					AssemblyBuilderAccess.RunAndSave, Dirname (name));
			}
			catch (ArgumentException) {
				// specified key may not be exportable outside it's container
				if (RootContext.StrongNameKeyContainer != null) {
					ctx.Report.Error (1548, "Could not access the key inside the container `" +
						RootContext.StrongNameKeyContainer + "'.");
					Environment.Exit (1);
				}
				throw;
			}
			catch (CryptographicException) {
				if ((RootContext.StrongNameKeyContainer != null) || (RootContext.StrongNameKeyFile != null)) {
					ctx.Report.Error (1548, "Could not use the specified key to strongname the assembly.");
					Environment.Exit (1);
				}
				return false;
			}

			// Get the complete AssemblyName from the builder
			// (We need to get the public key and token)
			Assembly.Name = Assembly.Builder.GetName ();

			//
			// Pass a path-less name to DefineDynamicModule.  Wonder how
			// this copes with output in different directories then.
			// FIXME: figure out how this copes with --output /tmp/blah
			//
			// If the third argument is true, the ModuleBuilder will dynamically
			// load the default symbol writer.
			//
			try {
				RootContext.ToplevelTypes.Builder = Assembly.Builder.DefineDynamicModule (
					Basename (name), Basename (output), want_debugging_support);

#if !MS_COMPATIBLE
				// TODO: We should use SymbolWriter from DefineDynamicModule
				if (want_debugging_support && !SymbolWriter.Initialize (RootContext.ToplevelTypes.Builder, output)) {
					ctx.Report.Error (40, "Unexpected debug information initialization error `{0}'",
						"Could not find the symbol writer assembly (Mono.CompilerServices.SymbolWriter.dll)");
					return false;
				}
#endif
			} catch (ExecutionEngineException e) {
				ctx.Report.Error (40, "Unexpected debug information initialization error `{0}'",
					e.Message);
				return false;
			}

			return true;
		}

		public static void Save (string name, Report Report)
		{
			PortableExecutableKinds pekind;
			ImageFileMachine machine;

			switch (RootContext.Platform) {
			case Platform.X86:
				pekind = PortableExecutableKinds.Required32Bit | PortableExecutableKinds.ILOnly;
				machine = ImageFileMachine.I386;
				break;
			case Platform.X64:
				pekind = PortableExecutableKinds.ILOnly;
				machine = ImageFileMachine.AMD64;
				break;
			case Platform.IA64:
				pekind = PortableExecutableKinds.ILOnly;
				machine = ImageFileMachine.IA64;
				break;
			case Platform.AnyCPU:
			default:
				pekind = PortableExecutableKinds.ILOnly;
				machine = ImageFileMachine.I386;
				break;
			}
			try {
				Assembly.Builder.Save (Basename (name), pekind, machine);
			}
			catch (COMException) {
				if ((RootContext.StrongNameKeyFile == null) || (!RootContext.StrongNameDelaySign))
					throw;

				// FIXME: it seems Microsoft AssemblyBuilder doesn't like to delay sign assemblies 
				Report.Error (1548, "Couldn't delay-sign the assembly with the '" +
					RootContext.StrongNameKeyFile +
					"', Use MCS with the Mono runtime or CSC to compile this assembly.");
			}
			catch (System.IO.IOException io) {
				Report.Error (16, "Could not write to file `"+name+"', cause: " + io.Message);
				return;
			}
			catch (System.UnauthorizedAccessException ua) {
				Report.Error (16, "Could not write to file `"+name+"', cause: " + ua.Message);
				return;
			}
			catch (System.NotImplementedException nie) {
				Report.RuntimeMissingSupport (Location.Null, nie.Message);
				return;
			}
		}
	}

	/// <summary>
	///   An Emit Context is created for each body of code (from methods,
	///   properties bodies, indexer bodies or constructor bodies)
	/// </summary>
	public class EmitContext : BuilderContext
	{
		// TODO: Has to be private
		public ILGenerator ig;

		/// <summary>
		///   The value that is allowed to be returned or NULL if there is no
		///   return type.
		/// </summary>
		TypeSpec return_type;

		/// <summary>
		///   Keeps track of the Type to LocalBuilder temporary storage created
		///   to store structures (used to compute the address of the structure
		///   value on structure method invocations)
		/// </summary>
		Dictionary<TypeSpec, object> temporary_storage;

		/// <summary>
		///   The location where we store the return value.
		/// </summary>
		public LocalBuilder return_value;

		/// <summary>
		///   The location where return has to jump to return the
		///   value
		/// </summary>
		public Label ReturnLabel;

		/// <summary>
		///   If we already defined the ReturnLabel
		/// </summary>
		public bool HasReturnLabel;

		/// <summary>
		///   Current loop begin and end labels.
		/// </summary>
		public Label LoopBegin, LoopEnd;

		/// <summary>
		///   Default target in a switch statement.   Only valid if
		///   InSwitch is true
		/// </summary>
		public Label DefaultTarget;

		/// <summary>
		///   If this is non-null, points to the current switch statement
		/// </summary>
		public Switch Switch;

		/// <summary>
		///  Whether we are inside an anonymous method.
		/// </summary>
		public AnonymousExpression CurrentAnonymousMethod;
		
		public readonly IMemberContext MemberContext;

		DynamicSiteClass dynamic_site_container;

		public EmitContext (IMemberContext rc, ILGenerator ig, TypeSpec return_type)
		{
			this.MemberContext = rc;
			this.ig = ig;

			this.return_type = return_type;
		}

		#region Properties

		public TypeSpec CurrentType {
			get { return MemberContext.CurrentType; }
		}

		public TypeParameter[] CurrentTypeParameters {
			get { return MemberContext.CurrentTypeParameters; }
		}

		public MemberCore CurrentTypeDefinition {
			get { return MemberContext.CurrentMemberDefinition; }
		}

		public bool IsStatic {
			get { return MemberContext.IsStatic; }
		}

		bool IsAnonymousStoreyMutateRequired {
			get {
				return CurrentAnonymousMethod != null &&
					CurrentAnonymousMethod.Storey != null &&
					CurrentAnonymousMethod.Storey.Mutator != null;
			}
		}

		// Has to be used for emitter errors only
		public Report Report {
			get { return MemberContext.Compiler.Report; }
		}

		public TypeSpec ReturnType {
			get {
				return return_type;
			}
		}
#endregion

		/// <summary>
		///   This is called immediately before emitting an IL opcode to tell the symbol
		///   writer to which source line this opcode belongs.
		/// </summary>
		public void Mark (Location loc)
		{
			if (!SymbolWriter.HasSymbolWriter || HasSet (Options.OmitDebugInfo) || loc.IsNull)
				return;

			SymbolWriter.MarkSequencePoint (ig, loc);
		}

		public void DefineLocalVariable (string name, LocalBuilder builder)
		{
			SymbolWriter.DefineLocalVariable (name, builder);
		}

		public void BeginCatchBlock (TypeSpec type)
		{
			ig.BeginCatchBlock (type.GetMetaInfo ());
		}

		public void BeginExceptionBlock ()
		{
			ig.BeginExceptionBlock ();
		}

		public void BeginFinallyBlock ()
		{
			ig.BeginFinallyBlock ();
		}

		public void BeginScope ()
		{
			ig.BeginScope();
			SymbolWriter.OpenScope(ig);
		}

		public void EndExceptionBlock ()
		{
			ig.EndExceptionBlock ();
		}

		public void EndScope ()
		{
			ig.EndScope();
			SymbolWriter.CloseScope(ig);
		}

		//
		// Creates a nested container in this context for all dynamic compiler generated stuff
		//
		public DynamicSiteClass CreateDynamicSite ()
		{
			if (dynamic_site_container == null) {
				var mc = MemberContext.CurrentMemberDefinition as MemberBase;
				dynamic_site_container = new DynamicSiteClass (CurrentTypeDefinition.Parent.PartialContainer, mc, CurrentTypeParameters);

				RootContext.ToplevelTypes.AddCompilerGeneratedClass (dynamic_site_container);
				dynamic_site_container.CreateType ();
				dynamic_site_container.DefineType ();
				dynamic_site_container.ResolveTypeParameters ();
				dynamic_site_container.Define ();
			}

			return dynamic_site_container;
		}

		public LocalBuilder DeclareLocal (TypeSpec type, bool pinned)
		{
			if (IsAnonymousStoreyMutateRequired)
				type = CurrentAnonymousMethod.Storey.Mutator.Mutate (type);

			return ig.DeclareLocal (type.GetMetaInfo (), pinned);
		}

		public Label DefineLabel ()
		{
			return ig.DefineLabel ();
		}

		public void MarkLabel (Label label)
		{
			ig.MarkLabel (label);
		}

		public void Emit (OpCode opcode)
		{
			ig.Emit (opcode);
		}

		public void Emit (OpCode opcode, LocalBuilder local)
		{
			ig.Emit (opcode, local);
		}

		public void Emit (OpCode opcode, string arg)
		{
			ig.Emit (opcode, arg);
		}

		public void Emit (OpCode opcode, double arg)
		{
			ig.Emit (opcode, arg);
		}

		public void Emit (OpCode opcode, float arg)
		{
			ig.Emit (opcode, arg);
		}

		public void Emit (OpCode opcode, int arg)
		{
			ig.Emit (opcode, arg);
		}

		public void Emit (OpCode opcode, byte arg)
		{
			ig.Emit (opcode, arg);
		}

		public void Emit (OpCode opcode, Label label)
		{
			ig.Emit (opcode, label);
		}

		public void Emit (OpCode opcode, Label[] labels)
		{
			ig.Emit (opcode, labels);
		}

		public void Emit (OpCode opcode, TypeSpec type)
		{
			if (IsAnonymousStoreyMutateRequired)
				type = CurrentAnonymousMethod.Storey.Mutator.Mutate (type);

			ig.Emit (opcode, type.GetMetaInfo ());
		}

		public void Emit (OpCode opcode, FieldSpec field)
		{
			if (IsAnonymousStoreyMutateRequired)
				field = field.Mutate (CurrentAnonymousMethod.Storey.Mutator);

			ig.Emit (opcode, field.GetMetaInfo ());
		}

		public void Emit (OpCode opcode, MethodSpec method)
		{
			if (IsAnonymousStoreyMutateRequired)
				method = method.Mutate (CurrentAnonymousMethod.Storey.Mutator);

			if (method.IsConstructor)
				ig.Emit (opcode, (ConstructorInfo) method.GetMetaInfo ());
			else
				ig.Emit (opcode, (MethodInfo) method.GetMetaInfo ());
		}

		// TODO: REMOVE breaks mutator
		public void Emit (OpCode opcode, MethodInfo method)
		{
			ig.Emit (opcode, method);
		}

		// TODO: REMOVE breaks mutator
		public void Emit (OpCode opcode, FieldBuilder field)
		{
			ig.Emit (opcode, field);
		}

		public void Emit (OpCode opcode, MethodSpec method, Type[] vargs)
		{
			// TODO MemberCache: This should mutate too
			ig.EmitCall (opcode, (MethodInfo) method.GetMetaInfo (), vargs);
		}

		public void EmitArrayNew (ArrayContainer ac)
		{
			if (ac.Rank == 1) {
				Emit (OpCodes.Newarr, ac.Element);
			} else {
				if (IsAnonymousStoreyMutateRequired)
					ac = (ArrayContainer) ac.Mutate (CurrentAnonymousMethod.Storey.Mutator);

				ig.Emit (OpCodes.Newobj, ac.GetConstructor ());
			}
		}

		public void EmitArrayAddress (ArrayContainer ac)
		{
			if (ac.Element.IsGenericParameter)
				ig.Emit (OpCodes.Readonly);

			if (ac.Rank > 1) {
				if (IsAnonymousStoreyMutateRequired)
					ac = (ArrayContainer) ac.Mutate (CurrentAnonymousMethod.Storey.Mutator);

				ig.Emit (OpCodes.Call, ac.GetAddressMethod ());
			} else {
				Emit (OpCodes.Ldelema, ac.Element);
			}
		}

		//
		// Emits the right opcode to load from an array
		//
		public void EmitArrayLoad (ArrayContainer ac)
		{
			if (ac.Rank > 1) {
				if (IsAnonymousStoreyMutateRequired)
					ac = (ArrayContainer) ac.Mutate (CurrentAnonymousMethod.Storey.Mutator);

				ig.Emit (OpCodes.Call, ac.GetGetMethod ());
				return;
			}

			var type = ac.Element;
			if (TypeManager.IsEnumType (type))
				type = EnumSpec.GetUnderlyingType (type);

			if (type == TypeManager.byte_type || type == TypeManager.bool_type)
				Emit (OpCodes.Ldelem_U1);
			else if (type == TypeManager.sbyte_type)
				Emit (OpCodes.Ldelem_I1);
			else if (type == TypeManager.short_type)
				Emit (OpCodes.Ldelem_I2);
			else if (type == TypeManager.ushort_type || type == TypeManager.char_type)
				Emit (OpCodes.Ldelem_U2);
			else if (type == TypeManager.int32_type)
				Emit (OpCodes.Ldelem_I4);
			else if (type == TypeManager.uint32_type)
				Emit (OpCodes.Ldelem_U4);
			else if (type == TypeManager.uint64_type)
				Emit (OpCodes.Ldelem_I8);
			else if (type == TypeManager.int64_type)
				Emit (OpCodes.Ldelem_I8);
			else if (type == TypeManager.float_type)
				Emit (OpCodes.Ldelem_R4);
			else if (type == TypeManager.double_type)
				Emit (OpCodes.Ldelem_R8);
			else if (type == TypeManager.intptr_type)
				Emit (OpCodes.Ldelem_I);
			else if (TypeManager.IsStruct (type)) {
				Emit (OpCodes.Ldelema, type);
				Emit (OpCodes.Ldobj, type);
			} else if (type.IsGenericParameter) {
				Emit (OpCodes.Ldelem, type);
			} else if (type.IsPointer)
				Emit (OpCodes.Ldelem_I);
			else
				Emit (OpCodes.Ldelem_Ref);
		}

		//
		// Emits the right opcode to store to an array
		//
		public void EmitArrayStore (ArrayContainer ac)
		{
			if (ac.Rank > 1) {
				if (IsAnonymousStoreyMutateRequired)
					ac = (ArrayContainer) ac.Mutate (CurrentAnonymousMethod.Storey.Mutator);

				ig.Emit (OpCodes.Call, ac.GetSetMethod ());
				return;
			}

			var type = ac.Element;

			if (type.IsEnum)
				type = EnumSpec.GetUnderlyingType (type);

			if (type == TypeManager.byte_type || type == TypeManager.sbyte_type || type == TypeManager.bool_type)
				Emit (OpCodes.Stelem_I1);
			else if (type == TypeManager.short_type || type == TypeManager.ushort_type || type == TypeManager.char_type)
				Emit (OpCodes.Stelem_I2);
			else if (type == TypeManager.int32_type || type == TypeManager.uint32_type)
				Emit (OpCodes.Stelem_I4);
			else if (type == TypeManager.int64_type || type == TypeManager.uint64_type)
				Emit (OpCodes.Stelem_I8);
			else if (type == TypeManager.float_type)
				Emit (OpCodes.Stelem_R4);
			else if (type == TypeManager.double_type)
				Emit (OpCodes.Stelem_R8);
			else if (type == TypeManager.intptr_type)
				Emit (OpCodes.Stobj, type);
			else if (TypeManager.IsStruct (type))
				Emit (OpCodes.Stobj, type);
			else if (type.IsGenericParameter)
				Emit (OpCodes.Stelem, type);
			else if (type.IsPointer)
				Emit (OpCodes.Stelem_I);
			else
				Emit (OpCodes.Stelem_Ref);
		}

		public void EmitInt (int i)
		{
			switch (i) {
			case -1:
				ig.Emit (OpCodes.Ldc_I4_M1);
				break;

			case 0:
				ig.Emit (OpCodes.Ldc_I4_0);
				break;

			case 1:
				ig.Emit (OpCodes.Ldc_I4_1);
				break;

			case 2:
				ig.Emit (OpCodes.Ldc_I4_2);
				break;

			case 3:
				ig.Emit (OpCodes.Ldc_I4_3);
				break;

			case 4:
				ig.Emit (OpCodes.Ldc_I4_4);
				break;

			case 5:
				ig.Emit (OpCodes.Ldc_I4_5);
				break;

			case 6:
				ig.Emit (OpCodes.Ldc_I4_6);
				break;

			case 7:
				ig.Emit (OpCodes.Ldc_I4_7);
				break;

			case 8:
				ig.Emit (OpCodes.Ldc_I4_8);
				break;

			default:
				if (i >= -128 && i <= 127) {
					ig.Emit (OpCodes.Ldc_I4_S, (sbyte) i);
				} else
					ig.Emit (OpCodes.Ldc_I4, i);
				break;
			}
		}

		public void EmitLong (long l)
		{
			if (l >= int.MinValue && l <= int.MaxValue) {
				EmitInt (unchecked ((int) l));
				ig.Emit (OpCodes.Conv_I8);
				return;
			}

			if (l >= 0 && l <= uint.MaxValue) {
				EmitInt (unchecked ((int) l));
				ig.Emit (OpCodes.Conv_U8);
				return;
			}

			ig.Emit (OpCodes.Ldc_I8, l);
		}

		//
		// Load the object from the pointer.  
		//
		public void EmitLoadFromPtr (TypeSpec t)
		{
			if (t == TypeManager.int32_type)
				ig.Emit (OpCodes.Ldind_I4);
			else if (t == TypeManager.uint32_type)
				ig.Emit (OpCodes.Ldind_U4);
			else if (t == TypeManager.short_type)
				ig.Emit (OpCodes.Ldind_I2);
			else if (t == TypeManager.ushort_type)
				ig.Emit (OpCodes.Ldind_U2);
			else if (t == TypeManager.char_type)
				ig.Emit (OpCodes.Ldind_U2);
			else if (t == TypeManager.byte_type)
				ig.Emit (OpCodes.Ldind_U1);
			else if (t == TypeManager.sbyte_type)
				ig.Emit (OpCodes.Ldind_I1);
			else if (t == TypeManager.uint64_type)
				ig.Emit (OpCodes.Ldind_I8);
			else if (t == TypeManager.int64_type)
				ig.Emit (OpCodes.Ldind_I8);
			else if (t == TypeManager.float_type)
				ig.Emit (OpCodes.Ldind_R4);
			else if (t == TypeManager.double_type)
				ig.Emit (OpCodes.Ldind_R8);
			else if (t == TypeManager.bool_type)
				ig.Emit (OpCodes.Ldind_I1);
			else if (t == TypeManager.intptr_type)
				ig.Emit (OpCodes.Ldind_I);
			else if (t.IsEnum) {
				if (t == TypeManager.enum_type)
					ig.Emit (OpCodes.Ldind_Ref);
				else
					EmitLoadFromPtr (EnumSpec.GetUnderlyingType (t));
			} else if (TypeManager.IsStruct (t) || TypeManager.IsGenericParameter (t))
				Emit (OpCodes.Ldobj, t);
			else if (t.IsPointer)
				ig.Emit (OpCodes.Ldind_I);
			else
				ig.Emit (OpCodes.Ldind_Ref);
		}

		//
		// The stack contains the pointer and the value of type `type'
		//
		public void EmitStoreFromPtr (TypeSpec type)
		{
			if (type.IsEnum)
				type = EnumSpec.GetUnderlyingType (type);

			if (type == TypeManager.int32_type || type == TypeManager.uint32_type)
				ig.Emit (OpCodes.Stind_I4);
			else if (type == TypeManager.int64_type || type == TypeManager.uint64_type)
				ig.Emit (OpCodes.Stind_I8);
			else if (type == TypeManager.char_type || type == TypeManager.short_type ||
				 type == TypeManager.ushort_type)
				ig.Emit (OpCodes.Stind_I2);
			else if (type == TypeManager.float_type)
				ig.Emit (OpCodes.Stind_R4);
			else if (type == TypeManager.double_type)
				ig.Emit (OpCodes.Stind_R8);
			else if (type == TypeManager.byte_type || type == TypeManager.sbyte_type ||
				 type == TypeManager.bool_type)
				ig.Emit (OpCodes.Stind_I1);
			else if (type == TypeManager.intptr_type)
				ig.Emit (OpCodes.Stind_I);
			else if (TypeManager.IsStruct (type) || TypeManager.IsGenericParameter (type))
				ig.Emit (OpCodes.Stobj, type.GetMetaInfo ());
			else
				ig.Emit (OpCodes.Stind_Ref);
		}

		/// <summary>
		///   Returns a temporary storage for a variable of type t as 
		///   a local variable in the current body.
		/// </summary>
		public LocalBuilder GetTemporaryLocal (TypeSpec t)
		{
			if (temporary_storage != null) {
				object o;
				if (temporary_storage.TryGetValue (t, out o)) {
					if (o is Stack<LocalBuilder>) {
						var s = (Stack<LocalBuilder>) o;
						o = s.Count == 0 ? null : s.Pop ();
					} else {
						temporary_storage.Remove (t);
					}
				}
				if (o != null)
					return (LocalBuilder) o;
			}
			return DeclareLocal (t, false);
		}

		public void FreeTemporaryLocal (LocalBuilder b, TypeSpec t)
		{
			if (temporary_storage == null) {
				temporary_storage = new Dictionary<TypeSpec, object> (ReferenceEquality<TypeSpec>.Default);
				temporary_storage.Add (t, b);
				return;
			}
			object o;
			
			if (!temporary_storage.TryGetValue (t, out o)) {
				temporary_storage.Add (t, b);
				return;
			}
			var s = o as Stack<LocalBuilder>;
			if (s == null) {
				s = new Stack<LocalBuilder> ();
				s.Push ((LocalBuilder)o);
				temporary_storage [t] = s;
			}
			s.Push (b);
		}

		/// <summary>
		///   ReturnValue creates on demand the LocalBuilder for the
		///   return value from the function.  By default this is not
		///   used.  This is only required when returns are found inside
		///   Try or Catch statements.
		///
		///   This method is typically invoked from the Emit phase, so
		///   we allow the creation of a return label if it was not
		///   requested during the resolution phase.   Could be cleaned
		///   up, but it would replicate a lot of logic in the Emit phase
		///   of the code that uses it.
		/// </summary>
		public LocalBuilder TemporaryReturn ()
		{
			if (return_value == null){
				return_value = DeclareLocal (return_type, false);
				if (!HasReturnLabel){
					ReturnLabel = DefineLabel ();
					HasReturnLabel = true;
				}
			}

			return return_value;
		}
	}
}
