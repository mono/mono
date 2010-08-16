//
// codegen.cs: The code generator
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// Copyright 2001, 2002, 2003 Ximian, Inc.
// Copyright 2004 Novell, Inc.
//

//
// Please leave this defined on SVN: The idea is that when we ship the
// compiler to end users, if the compiler crashes, they have a chance
// to narrow down the problem.   
//
// Only remove it if you need to debug locally on your tree.
//
//#define PRODUCTION

using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Security.Permissions;

using Mono.Security.Cryptography;

namespace Mono.CSharp {

	/// <summary>
	///    Code generator class.
	/// </summary>
	public class CodeGen {
		static AppDomain current_domain;

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
				foreach (Assembly a in GlobalRootNamespace.Instance.Assemblies) {
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

		static public void Save (string name, bool saveDebugInfo, Report Report)
		{
			PortableExecutableKinds pekind;
			ImageFileMachine machine;

			switch (RootContext.Platform) {
			case Platform.X86:
				pekind = PortableExecutableKinds.Required32Bit;
				machine = ImageFileMachine.I386;
				break;
			case Platform.X64:
				pekind = PortableExecutableKinds.PE32Plus;
				machine = ImageFileMachine.AMD64;
				break;
			case Platform.IA64:
				pekind = PortableExecutableKinds.PE32Plus;
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

			//
			// Write debuger symbol file
			//
			if (saveDebugInfo)
				SymbolWriter.WriteSymbolFile ();
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

	public abstract class CommonAssemblyModulClass : Attributable, IMemberContext
	{
		public void AddAttributes (List<Attribute> attrs, IMemberContext context)
		{
			foreach (Attribute a in attrs)
				a.AttachTo (this, context);

			if (attributes == null) {
				attributes = new Attributes (attrs);
				return;
			}
			attributes.AddAttributes (attrs);
		}

		public virtual void Emit (TypeContainer tc) 
		{
			if (OptAttributes == null)
				return;

			OptAttributes.Emit ();
		}

		protected Attribute ResolveAttribute (PredefinedAttribute a_type)
		{
			Attribute a = OptAttributes.Search (a_type);
			if (a != null) {
				a.Resolve ();
			}
			return a;
		}

		#region IMemberContext Members

		public CompilerContext Compiler {
			get { return RootContext.ToplevelTypes.Compiler; }
		}

		public TypeSpec CurrentType {
			get { return null; }
		}

		public TypeParameter[] CurrentTypeParameters {
			get { return null; }
		}

		public MemberCore CurrentMemberDefinition {
			get { return RootContext.ToplevelTypes; }
		}

		public string GetSignatureForError ()
		{
			return "<module>";
		}

		public bool HasUnresolvedConstraints {
			get { return false; }
		}

		public bool IsObsolete {
			get { return false; }
		}

		public bool IsUnsafe {
			get { return false; }
		}

		public bool IsStatic {
			get { return false; }
		}

		public IList<MethodSpec> LookupExtensionMethod (TypeSpec extensionType, string name, int arity, ref NamespaceEntry scope)
		{
			throw new NotImplementedException ();
		}

		public FullNamedExpression LookupNamespaceOrType (string name, int arity, Location loc, bool ignore_cs0104)
		{
			return RootContext.ToplevelTypes.LookupNamespaceOrType (name, arity, loc, ignore_cs0104);
		}

		public FullNamedExpression LookupNamespaceAlias (string name)
		{
			return null;
		}

		#endregion
	}
                
	public class AssemblyClass : CommonAssemblyModulClass {
		// TODO: make it private and move all builder based methods here
		public AssemblyBuilder Builder;
		bool is_cls_compliant;
		bool wrap_non_exception_throws;

		public Attribute ClsCompliantAttribute;

		Dictionary<SecurityAction, PermissionSet> declarative_security;
		bool has_extension_method;		
		public AssemblyName Name;
		MethodInfo add_type_forwarder;
		Dictionary<ITypeDefinition, Attribute> emitted_forwarders;

		// Module is here just because of error messages
		static string[] attribute_targets = new string [] { "assembly", "module" };

		public AssemblyClass ()
		{
			wrap_non_exception_throws = true;
		}

		public bool HasExtensionMethods {
			set {
				has_extension_method = value;
			}
		}

		public bool IsClsCompliant {
			get {
				return is_cls_compliant;
			}
		}

		public bool WrapNonExceptionThrows {
			get {
				return wrap_non_exception_throws;
			}
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Assembly;
			}
		}

		public override bool IsClsComplianceRequired ()
		{
			return is_cls_compliant;
		}

		Report Report {
			get { return Compiler.Report; }
		}

		public void Resolve ()
		{
			if (RootContext.Unsafe) {
				//
				// Emits [assembly: SecurityPermissionAttribute (SecurityAction.RequestMinimum, SkipVerification = true)]
				// when -unsafe option was specified
				//
				
				Location loc = Location.Null;

				MemberAccess system_security_permissions = new MemberAccess (new MemberAccess (
					new QualifiedAliasMember (QualifiedAliasMember.GlobalAlias, "System", loc), "Security", loc), "Permissions", loc);

				Arguments pos = new Arguments (1);
				pos.Add (new Argument (new MemberAccess (new MemberAccess (system_security_permissions, "SecurityAction", loc), "RequestMinimum")));

				Arguments named = new Arguments (1);
				named.Add (new NamedArgument ("SkipVerification", loc, new BoolLiteral (true, loc)));

				GlobalAttribute g = new GlobalAttribute (new NamespaceEntry (null, null, null), "assembly",
					new MemberAccess (system_security_permissions, "SecurityPermissionAttribute"),
					new Arguments[] { pos, named }, loc, false);
				g.AttachTo (this, this);

				if (g.Resolve () != null) {
					declarative_security = new Dictionary<SecurityAction, PermissionSet> ();
					g.ExtractSecurityPermissionSet (declarative_security);
				}
			}

			if (OptAttributes == null)
				return;

			// Ensure that we only have GlobalAttributes, since the Search isn't safe with other types.
			if (!OptAttributes.CheckTargets())
				return;

			ClsCompliantAttribute = ResolveAttribute (PredefinedAttributes.Get.CLSCompliant);

			if (ClsCompliantAttribute != null) {
				is_cls_compliant = ClsCompliantAttribute.GetClsCompliantAttributeValue ();
			}

			Attribute a = ResolveAttribute (PredefinedAttributes.Get.RuntimeCompatibility);
			if (a != null) {
				var val = a.GetPropertyValue ("WrapNonExceptionThrows") as BoolConstant;
				if (val != null)
					wrap_non_exception_throws = val.Value;
			}
		}

		// fix bug #56621
		private void SetPublicKey (AssemblyName an, byte[] strongNameBlob) 
		{
			try {
				// check for possible ECMA key
				if (strongNameBlob.Length == 16) {
					// will be rejected if not "the" ECMA key
					an.SetPublicKey (strongNameBlob);
				}
				else {
					// take it, with or without, a private key
					RSA rsa = CryptoConvert.FromCapiKeyBlob (strongNameBlob);
					// and make sure we only feed the public part to Sys.Ref
					byte[] publickey = CryptoConvert.ToCapiPublicKeyBlob (rsa);
					
					// AssemblyName.SetPublicKey requires an additional header
					byte[] publicKeyHeader = new byte [12] { 0x00, 0x24, 0x00, 0x00, 0x04, 0x80, 0x00, 0x00, 0x94, 0x00, 0x00, 0x00 };

					byte[] encodedPublicKey = new byte [12 + publickey.Length];
					Buffer.BlockCopy (publicKeyHeader, 0, encodedPublicKey, 0, 12);
					Buffer.BlockCopy (publickey, 0, encodedPublicKey, 12, publickey.Length);
					an.SetPublicKey (encodedPublicKey);
				}
			}
			catch (Exception) {
				Error_AssemblySigning ("The specified file `" + RootContext.StrongNameKeyFile + "' is incorrectly encoded");
				Environment.Exit (1);
			}
		}

		void Error_ObsoleteSecurityAttribute (Attribute a, string option)
		{
			Report.Warning (1699, 1, a.Location,
				"Use compiler option `{0}' or appropriate project settings instead of `{1}' attribute",
				option, a.Name);
		}

		// TODO: rewrite this code (to kill N bugs and make it faster) and use standard ApplyAttribute way.
		public AssemblyName GetAssemblyName (string name, string output) 
		{
			if (OptAttributes != null) {
				foreach (Attribute a in OptAttributes.Attrs) {
					// cannot rely on any resolve-based members before you call Resolve
					if (a.ExplicitTarget == null || a.ExplicitTarget != "assembly")
						continue;

					// TODO: This code is buggy: comparing Attribute name without resolving is wrong.
					//       However, this is invoked by CodeGen.Init, when none of the namespaces
					//       are loaded yet.
					// TODO: Does not handle quoted attributes properly
					switch (a.Name) {
					case "AssemblyKeyFile":
					case "AssemblyKeyFileAttribute":
					case "System.Reflection.AssemblyKeyFileAttribute":
						if (RootContext.StrongNameKeyFile != null) {
							Report.SymbolRelatedToPreviousError (a.Location, a.GetSignatureForError ());
							Report.Warning (1616, 1, "Option `{0}' overrides attribute `{1}' given in a source file or added module",
									"keyfile", "System.Reflection.AssemblyKeyFileAttribute");
						} else {
							string value = a.GetString ();
							if (!string.IsNullOrEmpty (value)) {
								Error_ObsoleteSecurityAttribute (a, "keyfile");
								RootContext.StrongNameKeyFile = value;
							}
						}
						break;
					case "AssemblyKeyName":
					case "AssemblyKeyNameAttribute":
					case "System.Reflection.AssemblyKeyNameAttribute":
						if (RootContext.StrongNameKeyContainer != null) {
							Report.SymbolRelatedToPreviousError (a.Location, a.GetSignatureForError ());
							Report.Warning (1616, 1, "Option `{0}' overrides attribute `{1}' given in a source file or added module",
									"keycontainer", "System.Reflection.AssemblyKeyNameAttribute");
						} else {
							string value = a.GetString ();
							if (!string.IsNullOrEmpty (value)) {
								Error_ObsoleteSecurityAttribute (a, "keycontainer");
								RootContext.StrongNameKeyContainer = value;
							}
						}
						break;
					case "AssemblyDelaySign":
					case "AssemblyDelaySignAttribute":
					case "System.Reflection.AssemblyDelaySignAttribute":
						bool b = a.GetBoolean ();
						if (b) {
							Error_ObsoleteSecurityAttribute (a, "delaysign");
						}

						RootContext.StrongNameDelaySign = b;
						break;
					}
				}
			}
			
			AssemblyName an = new AssemblyName ();
			an.Name = Path.GetFileNameWithoutExtension (name);

			// note: delay doesn't apply when using a key container
			if (RootContext.StrongNameKeyContainer != null) {
				an.KeyPair = new StrongNameKeyPair (RootContext.StrongNameKeyContainer);
				return an;
			}

			// strongname is optional
			if (RootContext.StrongNameKeyFile == null)
				return an;

			string AssemblyDir = Path.GetDirectoryName (output);

			// the StrongName key file may be relative to (a) the compiled
			// file or (b) to the output assembly. See bugzilla #55320
			// http://bugzilla.ximian.com/show_bug.cgi?id=55320

			// (a) relative to the compiled file
			string filename = Path.GetFullPath (RootContext.StrongNameKeyFile);
			bool exist = File.Exists (filename);
			if ((!exist) && (AssemblyDir != null) && (AssemblyDir != String.Empty)) {
				// (b) relative to the outputed assembly
				filename = Path.GetFullPath (Path.Combine (AssemblyDir, RootContext.StrongNameKeyFile));
				exist = File.Exists (filename);
			}

			if (exist) {
				using (FileStream fs = new FileStream (filename, FileMode.Open, FileAccess.Read)) {
					byte[] snkeypair = new byte [fs.Length];
					fs.Read (snkeypair, 0, snkeypair.Length);

					if (RootContext.StrongNameDelaySign) {
						// delayed signing - DO NOT include private key
						SetPublicKey (an, snkeypair);
					}
					else {
						// no delay so we make sure we have the private key
						try {
							CryptoConvert.FromCapiPrivateKeyBlob (snkeypair);
							an.KeyPair = new StrongNameKeyPair (snkeypair);
						}
						catch (CryptographicException) {
							if (snkeypair.Length == 16) {
								// error # is different for ECMA key
								Report.Error (1606, "Could not sign the assembly. " + 
									"ECMA key can only be used to delay-sign assemblies");
							}
							else {
								Error_AssemblySigning ("The specified file `" + RootContext.StrongNameKeyFile + "' does not have a private key");
							}
							return null;
						}
					}
				}
			}
			else {
				Error_AssemblySigning ("The specified file `" + RootContext.StrongNameKeyFile + "' does not exist");
				return null;
			}
			return an;
		}

		void Error_AssemblySigning (string text)
		{
			Report.Error (1548, "Error during assembly signing. " + text);
		}

		bool CheckInternalsVisibleAttribute (Attribute a)
		{
			string assembly_name = a.GetString ();
			if (assembly_name.Length == 0)
				return false;
				
			AssemblyName aname = null;
			try {
				aname = new AssemblyName (assembly_name);
			} catch (FileLoadException) {
			} catch (ArgumentException) {
			}
				
			// Bad assembly name format
			if (aname == null)
				Report.Warning (1700, 3, a.Location, "Assembly reference `" + assembly_name + "' is invalid and cannot be resolved");
			// Report error if we have defined Version or Culture
			else if (aname.Version != null || aname.CultureInfo != null)
				throw new Exception ("Friend assembly `" + a.GetString () + 
						"' is invalid. InternalsVisibleTo cannot have version or culture specified.");
			else if (aname.GetPublicKey () == null && Name.GetPublicKey () != null && Name.GetPublicKey ().Length != 0) {
				Report.Error (1726, a.Location, "Friend assembly reference `" + aname.FullName + "' is invalid." +
						" Strong named assemblies must specify a public key in their InternalsVisibleTo declarations");
				return false;
			}

			return true;
		}

		static string IsValidAssemblyVersion (string version)
		{
			Version v;
			try {
				v = new Version (version);
			} catch {
				try {
					int major = int.Parse (version, CultureInfo.InvariantCulture);
					v = new Version (major, 0);
				} catch {
					return null;
				}
			}

			foreach (int candidate in new int [] { v.Major, v.Minor, v.Build, v.Revision }) {
				if (candidate > ushort.MaxValue)
					return null;
			}

			return new Version (v.Major, System.Math.Max (0, v.Minor), System.Math.Max (0, v.Build), System.Math.Max (0, v.Revision)).ToString (4);
		}

		public override void ApplyAttributeBuilder (Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa)
		{
			if (a.IsValidSecurityAttribute ()) {
				if (declarative_security == null)
					declarative_security = new Dictionary<SecurityAction, PermissionSet> ();

				a.ExtractSecurityPermissionSet (declarative_security);
				return;
			}

			if (a.Type == pa.AssemblyCulture) {
				string value = a.GetString ();
				if (value == null || value.Length == 0)
					return;

				if (RootContext.Target == Target.Exe) {
					a.Error_AttributeEmitError ("The executables cannot be satelite assemblies, remove the attribute or keep it empty");
					return;
				}

				try {
					var fi = typeof (AssemblyBuilder).GetField ("culture", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetField);
					fi.SetValue (CodeGen.Assembly.Builder, value == "neutral" ? "" : value);
				} catch {
					Report.RuntimeMissingSupport (a.Location, "AssemblyCultureAttribute setting");
				}

				return;
			}

			if (a.Type == pa.AssemblyVersion) {
				string value = a.GetString ();
				if (value == null || value.Length == 0)
					return;

				var vinfo = IsValidAssemblyVersion (value.Replace ('*', '0'));
				if (vinfo == null) {
					a.Error_AttributeEmitError (string.Format ("Specified version `{0}' is not valid", value));
					return;
				}

				try {
					var fi = typeof (AssemblyBuilder).GetField ("version", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetField);
					fi.SetValue (CodeGen.Assembly.Builder, vinfo);
				} catch {
					Report.RuntimeMissingSupport (a.Location, "AssemblyVersionAttribute setting");
				}

				return;
			}

			if (a.Type == pa.AssemblyAlgorithmId) {
				const int pos = 2; // skip CA header
				uint alg = (uint) cdata [pos];
				alg |= ((uint) cdata [pos + 1]) << 8;
				alg |= ((uint) cdata [pos + 2]) << 16;
				alg |= ((uint) cdata [pos + 3]) << 24;

				try {
					var fi = typeof (AssemblyBuilder).GetField ("algid", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetField);
					fi.SetValue (CodeGen.Assembly.Builder, alg);
				} catch {
					Report.RuntimeMissingSupport (a.Location, "AssemblyAlgorithmIdAttribute setting");
				}

				return;
			}

			if (a.Type == pa.AssemblyFlags) {
				const int pos = 2; // skip CA header
				uint flags = (uint) cdata[pos];
				flags |= ((uint) cdata[pos + 1]) << 8;
				flags |= ((uint) cdata[pos + 2]) << 16;
				flags |= ((uint) cdata[pos + 3]) << 24;

				// Ignore set PublicKey flag if assembly is not strongnamed
				if ((flags & (uint) AssemblyNameFlags.PublicKey) != 0 && (CodeGen.Assembly.Builder.GetName ().KeyPair == null))
					flags &= ~(uint)AssemblyNameFlags.PublicKey;

				try {
					var fi = typeof (AssemblyBuilder).GetField ("flags", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetField);
					fi.SetValue (CodeGen.Assembly.Builder, flags);
				} catch {
					Report.RuntimeMissingSupport (a.Location, "AssemblyFlagsAttribute setting");
				}

				return;
			}

			if (a.Type == pa.InternalsVisibleTo && !CheckInternalsVisibleAttribute (a))
				return;

			if (a.Type == pa.TypeForwarder) {
				TypeSpec t = a.GetArgumentType ();
				if (t == null || TypeManager.HasElementType (t)) {
					Report.Error (735, a.Location, "Invalid type specified as an argument for TypeForwardedTo attribute");
					return;
				}

				if (emitted_forwarders == null) {
					emitted_forwarders = new Dictionary<ITypeDefinition, Attribute>  ();
				} else if (emitted_forwarders.ContainsKey (t.MemberDefinition)) {
					Report.SymbolRelatedToPreviousError(emitted_forwarders[t.MemberDefinition].Location, null);
					Report.Error(739, a.Location, "A duplicate type forward of type `{0}'",
						TypeManager.CSharpName(t));
					return;
				}

				emitted_forwarders.Add(t.MemberDefinition, a);

				if (t.Assembly == Builder) {
					Report.SymbolRelatedToPreviousError (t);
					Report.Error (729, a.Location, "Cannot forward type `{0}' because it is defined in this assembly",
						TypeManager.CSharpName (t));
					return;
				}

				if (t.IsNested) {
					Report.Error (730, a.Location, "Cannot forward type `{0}' because it is a nested type",
						TypeManager.CSharpName (t));
					return;
				}

				if (add_type_forwarder == null) {
					add_type_forwarder = typeof (AssemblyBuilder).GetMethod ("AddTypeForwarder",
						BindingFlags.NonPublic | BindingFlags.Instance);

					if (add_type_forwarder == null) {
						Report.RuntimeMissingSupport (a.Location, "TypeForwardedTo attribute");
						return;
					}
				}

				add_type_forwarder.Invoke (Builder, new object[] { t.GetMetaInfo () });
				return;
			}
			
			if (a.Type == pa.Extension) {
				a.Error_MisusedExtensionAttribute ();
				return;
			}

			Builder.SetCustomAttribute ((ConstructorInfo) ctor.GetMetaInfo (), cdata);
		}

		public override void Emit (TypeContainer tc)
		{
			base.Emit (tc);

			if (has_extension_method)
				PredefinedAttributes.Get.Extension.EmitAttribute (Builder);

			// FIXME: Does this belong inside SRE.AssemblyBuilder instead?
			PredefinedAttribute pa = PredefinedAttributes.Get.RuntimeCompatibility;
			if (pa.IsDefined && (OptAttributes == null || !OptAttributes.Contains (pa))) {
				var ci = TypeManager.GetPredefinedConstructor (pa.Type, Location.Null, TypeSpec.EmptyTypes);
				PropertyInfo [] pis = new PropertyInfo [1];
				pis [0] = TypeManager.GetPredefinedProperty (pa.Type,
					"WrapNonExceptionThrows", Location.Null, TypeManager.bool_type).MetaInfo;
				object [] pargs = new object [1];
				pargs [0] = true;
				Builder.SetCustomAttribute (new CustomAttributeBuilder ((ConstructorInfo) ci.GetMetaInfo (), new object[0], pis, pargs));
			}

			if (declarative_security != null) {

				MethodInfo add_permission = typeof (AssemblyBuilder).GetMethod ("AddPermissionRequests", BindingFlags.Instance | BindingFlags.NonPublic);
				object builder_instance = Builder;

				try {
					// Microsoft runtime hacking
					if (add_permission == null) {
						var assembly_builder = typeof (AssemblyBuilder).Assembly.GetType ("System.Reflection.Emit.AssemblyBuilderData");
						add_permission = assembly_builder.GetMethod ("AddPermissionRequests", BindingFlags.Instance | BindingFlags.NonPublic);

						FieldInfo fi = typeof (AssemblyBuilder).GetField ("m_assemblyData", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
						builder_instance = fi.GetValue (Builder);
					}

					var args = new PermissionSet [3];
					declarative_security.TryGetValue (SecurityAction.RequestMinimum, out args [0]);
					declarative_security.TryGetValue (SecurityAction.RequestOptional, out args [1]);
					declarative_security.TryGetValue (SecurityAction.RequestRefuse, out args [2]);
					add_permission.Invoke (builder_instance, args);
				}
				catch {
					Report.RuntimeMissingSupport (Location.Null, "assembly permission setting");
				}
			}
		}

		public override string[] ValidAttributeTargets {
			get {
				return attribute_targets;
			}
		}

		// Wrapper for AssemblyBuilder.AddModule
		static MethodInfo adder_method;
		static public MethodInfo AddModule_Method {
			get {
				if (adder_method == null)
					adder_method = typeof (AssemblyBuilder).GetMethod ("AddModule", BindingFlags.Instance|BindingFlags.NonPublic);
				return adder_method;
			}
		}
		public Module AddModule (string module)
		{
			MethodInfo m = AddModule_Method;
			if (m == null) {
				Report.RuntimeMissingSupport (Location.Null, "/addmodule");
				Environment.Exit (1);
			}

			try {
				return (Module) m.Invoke (Builder, new object [] { module });
			} catch (TargetInvocationException ex) {
				throw ex.InnerException;
			}
		}		
	}
}
