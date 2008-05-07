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
#define PRODUCTION

using System;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
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
		public static ModuleClass Module;

		static CodeGen ()
		{
			Reset ();
		}

		public static void Reset ()
		{
			Assembly = new AssemblyClass ();
			Module = new ModuleClass (RootContext.Unsafe);
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
		// Initializes the symbol writer
		//
		static void InitializeSymbolWriter (string filename)
		{
			if (!SymbolWriter.Initialize (Module.Builder, filename)) {
				Report.Warning (
					-18, 1, "Could not find the symbol writer assembly (Mono.CompilerServices.SymbolWriter.dll). This is normally an installation problem. Please make sure to compile and install the mcs/class/Mono.CompilerServices.SymbolWriter directory.");
				return;
			}
		}

		//
		// Initializes the code generator variables
		//
		static public bool Init (string name, string output, bool want_debugging_support)
		{
			FileName = output;
			AssemblyName an = Assembly.GetAssemblyName (name, output);
			if (an == null)
				return false;

			if (an.KeyPair != null) {
				// If we are going to strong name our assembly make
				// sure all its refs are strong named
				foreach (Assembly a in RootNamespace.Global.Assemblies) {
					AssemblyName ref_name = a.GetName ();
					byte [] b = ref_name.GetPublicKeyToken ();
					if (b == null || b.Length == 0) {
						Report.Error (1577, "Assembly generation failed " +
								"-- Referenced assembly '" +
								ref_name.Name +
								"' does not have a strong name.");
						//Environment.Exit (1);
					}
				}
			}
			
			current_domain = AppDomain.CurrentDomain;

			try {
#if MS_COMPATIBLE
				const AssemblyBuilderAccess COMPILER_ACCESS = 0;
#else
				/* Keep this in sync with System.Reflection.Emit.AssemblyBuilder */
				const AssemblyBuilderAccess COMPILER_ACCESS = (AssemblyBuilderAccess) 0x800;
#endif
				
				Assembly.Builder = current_domain.DefineDynamicAssembly (an,
					AssemblyBuilderAccess.Save | COMPILER_ACCESS, Dirname (name));
			}
			catch (ArgumentException) {
				// specified key may not be exportable outside it's container
				if (RootContext.StrongNameKeyContainer != null) {
					Report.Error (1548, "Could not access the key inside the container `" +
						RootContext.StrongNameKeyContainer + "'.");
					Environment.Exit (1);
				}
				throw;
			}
			catch (CryptographicException) {
				if ((RootContext.StrongNameKeyContainer != null) || (RootContext.StrongNameKeyFile != null)) {
					Report.Error (1548, "Could not use the specified key to strongname the assembly.");
					Environment.Exit (1);
				}
				return false;
			}

#if GMCS_SOURCE
			// Get the complete AssemblyName from the builder
			// (We need to get the public key and token)
			Assembly.Name = Assembly.Builder.GetName ();
#endif

			//
			// Pass a path-less name to DefineDynamicModule.  Wonder how
			// this copes with output in different directories then.
			// FIXME: figure out how this copes with --output /tmp/blah
			//
			// If the third argument is true, the ModuleBuilder will dynamically
			// load the default symbol writer.
			//
			Module.Builder = Assembly.Builder.DefineDynamicModule (
				Basename (name), Basename (output), false);

			if (want_debugging_support)
				InitializeSymbolWriter (output);

			return true;
		}

		static public void Save (string name)
		{
			try {
				Assembly.Builder.Save (Basename (name));

				SymbolWriter.WriteSymbolFile ();
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
			}
			catch (System.UnauthorizedAccessException ua) {
				Report.Error (16, "Could not write to file `"+name+"', cause: " + ua.Message);
			}
		}
	}


	public interface IResolveContext
	{
		DeclSpace DeclContainer { get; }
		bool IsInObsoleteScope { get; }
		bool IsInUnsafeScope { get; }

		// the declcontainer to lookup for type-parameters.  Should only use LookupGeneric on it.
		//
		// FIXME: This is somewhat of a hack.  We don't need a full DeclSpace for this.  We just need the
		//        current type parameters in scope. IUIC, that will require us to rewrite GenericMethod.
		//        Maybe we can replace this with a 'LookupGeneric (string)' instead, but we'll have to 
		//        handle generic method overrides differently
		DeclSpace GenericDeclContainer { get; }
	}

	/// <summary>
	///   An Emit Context is created for each body of code (from methods,
	///   properties bodies, indexer bodies or constructor bodies)
	/// </summary>
	public class EmitContext : IResolveContext {

		//
		// Holds a varible used during collection or object initialization.
		//
		public Expression CurrentInitializerVariable;

		DeclSpace decl_space;
		
		public DeclSpace TypeContainer;
		public ILGenerator ig;

		[Flags]
		public enum Flags : int {
			/// <summary>
			///   This flag tracks the `checked' state of the compilation,
			///   it controls whether we should generate code that does overflow
			///   checking, or if we generate code that ignores overflows.
			///
			///   The default setting comes from the command line option to generate
			///   checked or unchecked code plus any source code changes using the
			///   checked/unchecked statements or expressions.   Contrast this with
			///   the ConstantCheckState flag.
			/// </summary>
			CheckState = 1 << 0,

			/// <summary>
			///   The constant check state is always set to `true' and cant be changed
			///   from the command line.  The source code can change this setting with
			///   the `checked' and `unchecked' statements and expressions. 
			/// </summary>
			ConstantCheckState = 1 << 1,

			AllCheckStateFlags = CheckState | ConstantCheckState,

			/// <summary>
			///  Whether we are inside an unsafe block
			/// </summary>
			InUnsafe = 1 << 2,

			InCatch = 1 << 3,
			InFinally = 1 << 4,

			/// <summary>
			///   Whether control flow analysis is enabled
			/// </summary>
			DoFlowAnalysis = 1 << 5,

			/// <summary>
			///   Whether control flow analysis is disabled on structs
			///   (only meaningful when DoFlowAnalysis is set)
			/// </summary>
			OmitStructFlowAnalysis = 1 << 6,

			///
			/// Indicates the current context is in probing mode, no errors are reported. 
			///
			ProbingMode = 1	<<	7,

			//
			// Inside field intializer expression
			//
			InFieldInitializer = 1 << 8,
			
			InferReturnType = 1 << 9,
			
			InCompoundAssignment = 1 << 10,

			OmitDebuggingInfo = 1 << 11
		}

		Flags flags;

		/// <summary>
		///   Whether we are emitting code inside a static or instance method
		/// </summary>
		public bool IsStatic;

		/// <summary>
		///   Whether the actual created method is static or instance method.
		///   Althoug the method might be declared as `static', if an anonymous
		///   method is involved, we might turn this into an instance method.
		///
		///   So this reflects the low-level staticness of the method, while
		///   IsStatic represents the semantic, high-level staticness.
		/// </summary>
		public bool MethodIsStatic;

		/// <summary>
		///   The value that is allowed to be returned or NULL if there is no
		///   return type.
		/// </summary>
		Type return_type;

		/// <summary>
		///   Points to the Type (extracted from the TypeContainer) that
		///   declares this body of code
		/// </summary>
		public readonly Type ContainerType;
		
		/// <summary>
		///   Whether this is generating code for a constructor
		/// </summary>
		public bool IsConstructor;

		/// <summary>
		///   Keeps track of the Type to LocalBuilder temporary storage created
		///   to store structures (used to compute the address of the structure
		///   value on structure method invocations)
		/// </summary>
		public Hashtable temporary_storage;

		public Block CurrentBlock;

		public int CurrentFile;

		/// <summary>
		///   The location where we store the return value.
		/// </summary>
		LocalBuilder return_value;

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
		///   Whether we are inside an iterator block.
		/// </summary>
		public bool InIterator;

		/// <summary>
		///  Whether we are in a `fixed' initialization
		/// </summary>
		public bool InFixedInitializer;

		/// <summary>
		///  Whether we are inside an anonymous method.
		/// </summary>
		public AnonymousContainer CurrentAnonymousMethod;
		
		/// <summary>
		///   Location for this EmitContext
		/// </summary>
		public Location loc;

		/// <summary>
		///   Inside an enum definition, we do not resolve enumeration values
		///   to their enumerations, but rather to the underlying type/value
		///   This is so EnumVal + EnumValB can be evaluated.
		///
		///   There is no "E operator + (E x, E y)", so during an enum evaluation
		///   we relax the rules
		/// </summary>
		public bool InEnumContext;

		public readonly IResolveContext ResolveContext;

		/// <summary>
		///    The current iterator
		/// </summary>
		public Iterator CurrentIterator {
			get { return CurrentAnonymousMethod as Iterator; }
		}

		/// <summary>
		///    Whether we are in the resolving stage or not
		/// </summary>
		enum Phase {
			Created,
			Resolving,
			Emitting
		}

		bool isAnonymousMethodAllowed = true;

		Phase current_phase;
		FlowBranching current_flow_branching;

		static int next_id = 0;
		int id = ++next_id;

		public override string ToString ()
		{
			return String.Format ("EmitContext ({0}:{1})", id,
					      CurrentAnonymousMethod, loc);
		}
		
		public EmitContext (IResolveContext rc, DeclSpace parent, DeclSpace ds, Location l, ILGenerator ig,
				    Type return_type, int code_flags, bool is_constructor)
		{
			this.ResolveContext = rc;
			this.ig = ig;

			TypeContainer = parent;
			this.decl_space = ds;
			if (RootContext.Checked)
				flags |= Flags.CheckState;
			flags |= Flags.ConstantCheckState;

			if (return_type == null)
				throw new ArgumentNullException ("return_type");
#if GMCS_SOURCE
			if ((return_type is TypeBuilder) && return_type.IsGenericTypeDefinition)
				throw new InternalErrorException ();
#endif

			IsStatic = (code_flags & Modifiers.STATIC) != 0;
			MethodIsStatic = IsStatic;
			InIterator = (code_flags & Modifiers.METHOD_YIELDS) != 0;
			ReturnType = return_type;
			IsConstructor = is_constructor;
			CurrentBlock = null;
			CurrentFile = 0;
			current_phase = Phase.Created;

			if (parent != null){
				// Can only be null for the ResolveType contexts.
				ContainerType = parent.TypeBuilder;
				if (rc.IsInUnsafeScope)
					flags |= Flags.InUnsafe;
			}
			loc = l;
		}

		public EmitContext (IResolveContext rc, DeclSpace ds, Location l, ILGenerator ig,
				    Type return_type, int code_flags, bool is_constructor)
			: this (rc, ds, ds, l, ig, return_type, code_flags, is_constructor)
		{
		}

		public EmitContext (IResolveContext rc, DeclSpace ds, Location l, ILGenerator ig,
				    Type return_type, int code_flags)
			: this (rc, ds, ds, l, ig, return_type, code_flags, false)
		{
		}

		public DeclSpace DeclContainer { 
			get { return decl_space; }
			set { decl_space = value; }
		}

		public DeclSpace GenericDeclContainer {
			get { return DeclContainer; }
		}

		public bool CheckState {
			get { return (flags & Flags.CheckState) != 0; }
		}

		public bool ConstantCheckState {
			get { return (flags & Flags.ConstantCheckState) != 0; }
		}

		public bool InUnsafe {
			get { return (flags & Flags.InUnsafe) != 0; }
		}

		public bool InCatch {
			get { return (flags & Flags.InCatch) != 0; }
		}

		public bool InFinally {
			get { return (flags & Flags.InFinally) != 0; }
		}

		public bool DoFlowAnalysis {
			get { return (flags & Flags.DoFlowAnalysis) != 0; }
		}

		public bool OmitStructFlowAnalysis {
			get { return (flags & Flags.OmitStructFlowAnalysis) != 0; }
		}

		// utility helper for CheckExpr, UnCheckExpr, Checked and Unchecked statements
		// it's public so that we can use a struct at the callsite
		public struct FlagsHandle : IDisposable
		{
			EmitContext ec;
			readonly Flags invmask, oldval;

			public FlagsHandle (EmitContext ec, Flags flagsToSet)
				: this (ec, flagsToSet, flagsToSet)
			{
			}

			internal FlagsHandle (EmitContext ec, Flags mask, Flags val)
			{
				this.ec = ec;
				invmask = ~mask;
				oldval = ec.flags & mask;
				ec.flags = (ec.flags & invmask) | (val & mask);

				if ((mask & Flags.ProbingMode) != 0)
					Report.DisableReporting ();
			}

			public void Dispose ()
			{
				if ((invmask & Flags.ProbingMode) == 0)
					Report.EnableReporting ();

				ec.flags = (ec.flags & invmask) | oldval;
			}
		}

		// Temporarily set all the given flags to the given value.  Should be used in an 'using' statement
		public FlagsHandle Set (Flags flagsToSet)
		{
			return new FlagsHandle (this, flagsToSet);
		}

		public FlagsHandle With (Flags bits, bool enable)
		{
			return new FlagsHandle (this, bits, enable ? bits : 0);
		}

		public FlagsHandle WithFlowAnalysis (bool do_flow_analysis, bool omit_struct_analysis)
		{
			Flags newflags = 
				(do_flow_analysis ? Flags.DoFlowAnalysis : 0) |
				(omit_struct_analysis ? Flags.OmitStructFlowAnalysis : 0);
			return new FlagsHandle (this, Flags.DoFlowAnalysis | Flags.OmitStructFlowAnalysis, newflags);
		}
		
		/// <summary>
		///   If this is true, then Return and ContextualReturn statements
		///   will set the ReturnType value based on the expression types
		///   of each return statement instead of the method return type
		///   (which is initially null).
		/// </summary>
		public bool InferReturnType {
			get { return (flags & Flags.InferReturnType) != 0; }
		}

		public bool IsInObsoleteScope {
			get {
				// Disables obsolete checks when probing is on
				return IsInProbingMode || ResolveContext.IsInObsoleteScope;
			}
		}

		public bool IsInProbingMode {
			get { return (flags & Flags.ProbingMode) != 0; }
		}

		public bool IsInUnsafeScope {
			get { return InUnsafe || ResolveContext.IsInUnsafeScope; }
		}

		public bool IsAnonymousMethodAllowed {
			get { return isAnonymousMethodAllowed; }
			set { isAnonymousMethodAllowed = value; }
		}

		public bool IsInFieldInitializer {
			get { return (flags & Flags.InFieldInitializer) != 0; }
		}
		
		public bool IsInCompoundAssignment {
			get { return (flags & Flags.InCompoundAssignment) != 0; }
		}		

		public FlowBranching CurrentBranching {
			get { return current_flow_branching; }
		}

		public bool OmitDebuggingInfo {
			get { return (flags & Flags.OmitDebuggingInfo) != 0; }
			set {
				if (value)
					flags |= Flags.OmitDebuggingInfo;
				else
					flags &= ~Flags.OmitDebuggingInfo;
			}
		}

		// <summary>
		//   Starts a new code branching.  This inherits the state of all local
		//   variables and parameters from the current branching.
		// </summary>
		public FlowBranching StartFlowBranching (FlowBranching.BranchingType type, Location loc)
		{
			current_flow_branching = FlowBranching.CreateBranching (CurrentBranching, type, null, loc);
			return current_flow_branching;
		}

		// <summary>
		//   Starts a new code branching for block `block'.
		// </summary>
		public FlowBranching StartFlowBranching (Block block)
		{
			flags |= Flags.DoFlowAnalysis;

			current_flow_branching = FlowBranching.CreateBranching (
				CurrentBranching, FlowBranching.BranchingType.Block, block, block.StartLocation);
			return current_flow_branching;
		}

		public FlowBranchingTryCatch StartFlowBranching (TryCatch stmt)
		{
			FlowBranchingTryCatch branching = new FlowBranchingTryCatch (CurrentBranching, stmt);
			current_flow_branching = branching;
			return branching;
		}

		public FlowBranchingException StartFlowBranching (ExceptionStatement stmt)
		{
			FlowBranchingException branching = new FlowBranchingException (CurrentBranching, stmt);
			current_flow_branching = branching;
			return branching;
		}

		public FlowBranchingLabeled StartFlowBranching (LabeledStatement stmt)
		{
			FlowBranchingLabeled branching = new FlowBranchingLabeled (CurrentBranching, stmt);
			current_flow_branching = branching;
			return branching;
		}

		public FlowBranchingIterator StartFlowBranching (Iterator iterator)
		{
			FlowBranchingIterator branching = new FlowBranchingIterator (CurrentBranching, iterator);
			current_flow_branching = branching;
			return branching;
		}

		public FlowBranchingToplevel StartFlowBranching (ToplevelBlock stmt)
		{
			FlowBranchingToplevel branching = new FlowBranchingToplevel (CurrentBranching, stmt);
			current_flow_branching = branching;
			return branching;
		}

		// <summary>
		//   Ends a code branching.  Merges the state of locals and parameters
		//   from all the children of the ending branching.
		// </summary>
		public bool EndFlowBranching ()
		{
			FlowBranching old = current_flow_branching;
			current_flow_branching = current_flow_branching.Parent;

			FlowBranching.UsageVector vector = current_flow_branching.MergeChild (old);
			return vector.IsUnreachable;
		}

		// <summary>
		//   Kills the current code branching.  This throws away any changed state
		//   information and should only be used in case of an error.
		// </summary>
		// FIXME: this is evil
		public void KillFlowBranching ()
		{
			current_flow_branching = current_flow_branching.Parent;
		}

		public bool MustCaptureVariable (LocalInfo local)
		{
			if (CurrentAnonymousMethod == null)
				return false;
			if (CurrentAnonymousMethod.IsIterator)
				return true;
			return local.Block.Toplevel != CurrentBlock.Toplevel;
		}
		
		public void EmitMeta (ToplevelBlock b)
		{
			b.EmitMeta (this);

			if (HasReturnLabel)
				ReturnLabel = ig.DefineLabel ();
		}

		//
		// Here until we can fix the problem with Mono.CSharp.Switch, which
		// currently can not cope with ig == null during resolve (which must
		// be fixed for switch statements to work on anonymous methods).
		//
		public void EmitTopBlock (IMethodData md, ToplevelBlock block)
		{
			if (block == null)
				return;
			
			bool unreachable;
			
			if (ResolveTopBlock (null, block, md.ParameterInfo, md, out unreachable)){
				if (Report.Errors > 0)
					return;

				EmitMeta (block);

				current_phase = Phase.Emitting;
#if PRODUCTION
				try {
#endif
				EmitResolvedTopBlock (block, unreachable);
#if PRODUCTION
				} catch (Exception e){
					Console.WriteLine ("Exception caught by the compiler while emitting:");
					Console.WriteLine ("   Block that caused the problem begin at: " + block.loc);
					
					Console.WriteLine (e.GetType ().FullName + ": " + e.Message);
					throw;
				}
#endif
			}
		}

		bool resolved;

		public bool ResolveTopBlock (EmitContext anonymous_method_host, ToplevelBlock block,
					     Parameters ip, IMethodData md, out bool unreachable)
		{
			current_phase = Phase.Resolving;
			
			unreachable = false;

			if (resolved)
				return true;

			if (!loc.IsNull)
				CurrentFile = loc.File;

#if PRODUCTION
			try {
#endif
				if (!block.ResolveMeta (this, ip))
					return false;

				if ((md != null) && (md.Iterator != null)) {
					if (!md.Iterator.Define (this))
						return false;
				}

				using (this.With (EmitContext.Flags.DoFlowAnalysis, true)) {
					FlowBranchingToplevel top_level;
					if (anonymous_method_host != null)
						top_level = new FlowBranchingToplevel (anonymous_method_host.CurrentBranching, block);
					else 
						top_level = block.TopLevelBranching;

					current_flow_branching = top_level;
					bool ok = block.Resolve (this);
					current_flow_branching = null;

					if (!ok)
						return false;

					bool flow_unreachable = top_level.End ();
					if (flow_unreachable)
						unreachable = true;
				}
#if PRODUCTION
			} catch (Exception e) {
				Console.WriteLine ("Exception caught by the compiler while compiling:");
				Console.WriteLine ("   Block that caused the problem begin at: " + loc);

				if (CurrentBlock != null){
					Console.WriteLine ("                     Block being compiled: [{0},{1}]",
							   CurrentBlock.StartLocation, CurrentBlock.EndLocation);
				}
				Console.WriteLine (e.GetType ().FullName + ": " + e.Message);
				throw;
			}
#endif

			if (return_type != TypeManager.void_type && !unreachable) {
				if (CurrentAnonymousMethod == null) {
					Report.Error (161, md.Location, "`{0}': not all code paths return a value", md.GetSignatureForError ());
					return false;
				} else if (!CurrentAnonymousMethod.IsIterator) {
					Report.Error (1643, CurrentAnonymousMethod.Location, "Not all code paths return a value in anonymous method of type `{0}'",
						      CurrentAnonymousMethod.GetSignatureForError ());
					return false;
				}
			}

			if (!block.CompleteContexts (this))
				return false;

			resolved = true;
			return true;
		}

		public Type ReturnType {
			set {
				return_type = value;
			}
			get {
				return return_type;
			}
		}

		public void EmitResolvedTopBlock (ToplevelBlock block, bool unreachable)
		{
			if (block != null)
				block.Emit (this);

			if (HasReturnLabel)
				ig.MarkLabel (ReturnLabel);

			if (return_value != null){
				ig.Emit (OpCodes.Ldloc, return_value);
				ig.Emit (OpCodes.Ret);
			} else {
				//
				// If `HasReturnLabel' is set, then we already emitted a
				// jump to the end of the method, so we must emit a `ret'
				// there.
				//
				// Unfortunately, System.Reflection.Emit automatically emits
				// a leave to the end of a finally block.  This is a problem
				// if no code is following the try/finally block since we may
				// jump to a point after the end of the method.
				// As a workaround, we're always creating a return label in
				// this case.
				//

				bool in_iterator = (CurrentAnonymousMethod != null) &&
					CurrentAnonymousMethod.IsIterator && InIterator;

				if ((block != null) && block.IsDestructor) {
					// Nothing to do; S.R.E automatically emits a leave.
				} else if (HasReturnLabel || (!unreachable && !in_iterator)) {
					if (return_type != TypeManager.void_type)
						ig.Emit (OpCodes.Ldloc, TemporaryReturn ());
					ig.Emit (OpCodes.Ret);
				}
			}
		}

		/// <summary>
		///   This is called immediately before emitting an IL opcode to tell the symbol
		///   writer to which source line this opcode belongs.
		/// </summary>
		public void Mark (Location loc, bool check_file)
		{
			if (!SymbolWriter.HasSymbolWriter || OmitDebuggingInfo || loc.IsNull)
				return;

			if (check_file && (CurrentFile != loc.File))
				return;

			SymbolWriter.MarkSequencePoint (ig, loc.Row, loc.Column);
		}

		public void DefineLocalVariable (string name, LocalBuilder builder)
		{
			SymbolWriter.DefineLocalVariable (name, builder);
		}

		public void BeginScope ()
		{
			ig.BeginScope();
			SymbolWriter.OpenScope(ig);
		}

		public void EndScope ()
		{
			ig.EndScope();
			SymbolWriter.CloseScope(ig);
		}

		/// <summary>
		///   Returns a temporary storage for a variable of type t as 
		///   a local variable in the current body.
		/// </summary>
		public LocalBuilder GetTemporaryLocal (Type t)
		{
			if (temporary_storage != null) {
				object o = temporary_storage [t];
				if (o != null) {
					if (o is Stack) {
						Stack s = (Stack) o;
						o = s.Count == 0 ? null : s.Pop ();
					} else {
						temporary_storage.Remove (t);
					}
				}
				if (o != null)
					return (LocalBuilder) o;
			}
			return ig.DeclareLocal (t);
		}

		public void FreeTemporaryLocal (LocalBuilder b, Type t)
		{
			Stack s;

			if (temporary_storage == null) {
				temporary_storage = new Hashtable ();
				temporary_storage [t] = b;
				return;
			}
			object o = temporary_storage [t];
			if (o == null) {
				temporary_storage [t] = b;
				return;
			}
			if (o is Stack) {
				s = (Stack) o;
			} else {
				s = new Stack ();
				s.Push (o);
				temporary_storage [t] = s;
			}
			s.Push (b);
		}

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
				return_value = ig.DeclareLocal (return_type);
				if (!HasReturnLabel){
					ReturnLabel = ig.DefineLabel ();
					HasReturnLabel = true;
				}
			}

			return return_value;
		}

		/// <summary>
		///   This method is used during the Resolution phase to flag the
		///   need to define the ReturnLabel
		/// </summary>
		public void NeedReturnLabel ()
		{
			if (current_phase != Phase.Resolving){
				//
				// The reason is that the `ReturnLabel' is declared between
				// resolution and emission
				// 
				throw new Exception ("NeedReturnLabel called from Emit phase, should only be called during Resolve");
			}
			
			if (!InIterator && !HasReturnLabel) 
				HasReturnLabel = true;
		}


		public Expression GetThis (Location loc)
		{
			This my_this;
			if (CurrentBlock != null)
				my_this = new This (CurrentBlock, loc);
			else
				my_this = new This (loc);

			if (!my_this.ResolveBase (this))
				my_this = null;

			return my_this;
		}
	}


	public abstract class CommonAssemblyModulClass : Attributable, IResolveContext {

		protected CommonAssemblyModulClass ():
			base (null)
		{
		}

		public void AddAttributes (ArrayList attrs)
		{
			foreach (Attribute a in attrs)
				a.AttachTo (this);

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

		protected Attribute ResolveAttribute (Type a_type)
		{
			Attribute a = OptAttributes.Search (a_type);
			if (a != null) {
				a.Resolve ();
			}
			return a;
		}

		public override IResolveContext ResolveContext {
			get { return this; }
		}

		#region IResolveContext Members

		public DeclSpace DeclContainer {
			get { return RootContext.ToplevelTypes; }
		}

		public DeclSpace GenericDeclContainer {
			get { return DeclContainer; }
		}

		public bool IsInObsoleteScope {
			get { return false; }
		}

		public bool IsInUnsafeScope {
			get { return false; }
		}

		#endregion
	}
                
	public class AssemblyClass : CommonAssemblyModulClass {
		// TODO: make it private and move all builder based methods here
		public AssemblyBuilder Builder;
		bool is_cls_compliant;
		bool wrap_non_exception_throws;
		Type runtime_compatibility_attr_type;

		public Attribute ClsCompliantAttribute;

		ListDictionary declarative_security;
#if GMCS_SOURCE
		bool has_extension_method;		
		public AssemblyName Name;
		MethodInfo add_type_forwarder;
		ListDictionary emitted_forwarders;
#endif

		// Module is here just because of error messages
		static string[] attribute_targets = new string [] { "assembly", "module" };

		public AssemblyClass (): base ()
		{
#if GMCS_SOURCE
			wrap_non_exception_throws = true;
#endif
		}

		public bool HasExtensionMethods {
			set {
#if GMCS_SOURCE				
				has_extension_method = value;
#endif
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

		public void Resolve ()
		{
			runtime_compatibility_attr_type = TypeManager.CoreLookupType (
				"System.Runtime.CompilerServices", "RuntimeCompatibilityAttribute", Kind.Class, false);

			if (OptAttributes == null)
				return;

			// Ensure that we only have GlobalAttributes, since the Search isn't safe with other types.
			if (!OptAttributes.CheckTargets())
				return;

			if (TypeManager.cls_compliant_attribute_type != null)
				ClsCompliantAttribute = ResolveAttribute (TypeManager.cls_compliant_attribute_type);

			if (ClsCompliantAttribute != null) {
				is_cls_compliant = ClsCompliantAttribute.GetClsCompliantAttributeValue ();
			}

			if (runtime_compatibility_attr_type != null) {
				Attribute a = ResolveAttribute (runtime_compatibility_attr_type);
				if (a != null) {
					object val = a.GetPropertyValue ("WrapNonExceptionThrows");
					if (val != null)
						wrap_non_exception_throws = (bool) val;
				}
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
								Report.SymbolRelatedToPreviousError (a.Location, a.Name);
								Report.Warning (1616, 1, "Option `{0}' overrides attribute `{1}' given in a source file or added module",
                                    "keyfile", "System.Reflection.AssemblyKeyFileAttribute");
							}
							else {
								string value = a.GetString ();
								if (value.Length != 0)
									RootContext.StrongNameKeyFile = value;
							}
							break;
						case "AssemblyKeyName":
						case "AssemblyKeyNameAttribute":
						case "System.Reflection.AssemblyKeyNameAttribute":
							if (RootContext.StrongNameKeyContainer != null) {
								Report.SymbolRelatedToPreviousError (a.Location, a.Name);
								Report.Warning (1616, 1, "Option `{0}' overrides attribute `{1}' given in a source file or added module",
									"keycontainer", "System.Reflection.AssemblyKeyNameAttribute");
							}
							else {
								string value = a.GetString ();
								if (value.Length != 0)
									RootContext.StrongNameKeyContainer = value;
							}
							break;
						case "AssemblyDelaySign":
						case "AssemblyDelaySignAttribute":
						case "System.Reflection.AssemblyDelaySignAttribute":
							RootContext.StrongNameDelaySign = a.GetBoolean ();
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

#if GMCS_SOURCE
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
#endif

		static bool IsValidAssemblyVersion (string version)
		{
			Version v;
			try {
				v = new Version (version);
			} catch {
				try {
					int major = int.Parse (version, CultureInfo.InvariantCulture);
					v = new Version (major, 0);
				} catch {
					return false;
				}
			}

			foreach (int candidate in new int [] { v.Major, v.Minor, v.Build, v.Revision }) {
				if (candidate > ushort.MaxValue)
					return false;
			}

			return true;
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder customBuilder)
		{
			if (a.IsValidSecurityAttribute ()) {
				if (declarative_security == null)
					declarative_security = new ListDictionary ();

				a.ExtractSecurityPermissionSet (declarative_security);
				return;
			}

			if (a.Type == TypeManager.assembly_culture_attribute_type) {
				string value = a.GetString ();
				if (value == null || value.Length == 0)
					return;

				if (RootContext.Target == Target.Exe) {
					a.Error_AttributeEmitError ("The executables cannot be satelite assemblies, remove the attribute or keep it empty");
					return;
				}
			}

			if (a.Type == TypeManager.assembly_version_attribute_type) {
				string value = a.GetString ();
				if (value == null || value.Length == 0)
					return;

				value = value.Replace ('*', '0');

				if (!IsValidAssemblyVersion (value)) {
					a.Error_AttributeEmitError (string.Format ("Specified version `{0}' is not valid", value));
					return;
				}
			}

#if GMCS_SOURCE
			if (a.Type == TypeManager.internals_visible_attr_type && !CheckInternalsVisibleAttribute (a))
				return;

			if (a.Type == TypeManager.type_forwarder_attr_type) {
				Type t = a.GetArgumentType ();
				if (t == null || TypeManager.HasElementType (t)) {
					Report.Error (735, a.Location, "Invalid type specified as an argument for TypeForwardedTo attribute");
					return;
				}

				if (emitted_forwarders == null) {
					emitted_forwarders = new ListDictionary();
				} else if (emitted_forwarders.Contains(t)) {
					Report.SymbolRelatedToPreviousError(((Attribute)emitted_forwarders[t]).Location, null);
					Report.Error(739, a.Location, "A duplicate type forward of type `{0}'",
						TypeManager.CSharpName(t));
					return;
				}

				emitted_forwarders.Add(t, a);

				if (TypeManager.LookupDeclSpace (t) != null) {
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

				if (t.IsGenericType) {
					Report.Error (733, a.Location, "Cannot forward generic type `{0}'", TypeManager.CSharpName (t));
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

				add_type_forwarder.Invoke (Builder, new object[] { t });
				return;
			}
			
			if (a.Type == TypeManager.extension_attribute_type) {
				a.Error_MisusedExtensionAttribute ();
				return;
			}
#endif
			Builder.SetCustomAttribute (customBuilder);
		}

		public override void Emit (TypeContainer tc)
		{
			base.Emit (tc);

#if GMCS_SOURCE
			if (has_extension_method)
				Builder.SetCustomAttribute (TypeManager.extension_attribute_attr);
#endif

			if (runtime_compatibility_attr_type != null) {
				// FIXME: Does this belong inside SRE.AssemblyBuilder instead?
				if (OptAttributes == null || !OptAttributes.Contains (runtime_compatibility_attr_type)) {
					ConstructorInfo ci = TypeManager.GetPredefinedConstructor (
						runtime_compatibility_attr_type, Location.Null, Type.EmptyTypes);
					PropertyInfo [] pis = new PropertyInfo [1];
					pis [0] = TypeManager.GetPredefinedProperty (runtime_compatibility_attr_type,
						"WrapNonExceptionThrows", Location.Null, TypeManager.bool_type);
					object [] pargs = new object [1];
					pargs [0] = true;
					Builder.SetCustomAttribute (new CustomAttributeBuilder (ci, new object [0], pis, pargs));
				}
			}

			if (declarative_security != null) {

				MethodInfo add_permission = typeof (AssemblyBuilder).GetMethod ("AddPermissionRequests", BindingFlags.Instance | BindingFlags.NonPublic);
				object builder_instance = Builder;

				try {
					// Microsoft runtime hacking
					if (add_permission == null) {
						Type assembly_builder = typeof (AssemblyBuilder).Assembly.GetType ("System.Reflection.Emit.AssemblyBuilderData");
						add_permission = assembly_builder.GetMethod ("AddPermissionRequests", BindingFlags.Instance | BindingFlags.NonPublic);

						FieldInfo fi = typeof (AssemblyBuilder).GetField ("m_assemblyData", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
						builder_instance = fi.GetValue (Builder);
					}

					object[] args = new object [] { declarative_security [SecurityAction.RequestMinimum],
												  declarative_security [SecurityAction.RequestOptional],
												  declarative_security [SecurityAction.RequestRefuse] };
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

	public class ModuleClass : CommonAssemblyModulClass {
		// TODO: make it private and move all builder based methods here
		public ModuleBuilder Builder;
		bool m_module_is_unsafe;
		bool has_default_charset;

		public CharSet DefaultCharSet = CharSet.Ansi;
		public TypeAttributes DefaultCharSetType = TypeAttributes.AnsiClass;

		static string[] attribute_targets = new string [] { "module" };

		public ModuleClass (bool is_unsafe)
		{
			m_module_is_unsafe = is_unsafe;
		}

 		public override AttributeTargets AttributeTargets {
 			get {
 				return AttributeTargets.Module;
 			}
		}

		public override bool IsClsComplianceRequired ()
		{
			return CodeGen.Assembly.IsClsCompliant;
		}

		public override void Emit (TypeContainer tc) 
		{
			base.Emit (tc);

			if (m_module_is_unsafe) {
				Type t = TypeManager.CoreLookupType ("System.Security", "UnverifiableCodeAttribute", Kind.Class, true);
				if (t != null) {
					ConstructorInfo unverifiable_code_ctor = TypeManager.GetPredefinedConstructor (t, Location.Null, Type.EmptyTypes);
					if (unverifiable_code_ctor != null)
						Builder.SetCustomAttribute (new CustomAttributeBuilder (unverifiable_code_ctor, new object [0]));
				}
			}
		}
                
		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder customBuilder)
		{
			if (a.Type == TypeManager.cls_compliant_attribute_type) {
				if (CodeGen.Assembly.ClsCompliantAttribute == null) {
					Report.Warning (3012, 1, a.Location, "You must specify the CLSCompliant attribute on the assembly, not the module, to enable CLS compliance checking");
				}
				else if (CodeGen.Assembly.IsClsCompliant != a.GetBoolean ()) {
					Report.SymbolRelatedToPreviousError (CodeGen.Assembly.ClsCompliantAttribute.Location, CodeGen.Assembly.ClsCompliantAttribute.GetSignatureForError ());
					Report.Error (3017, a.Location, "You cannot specify the CLSCompliant attribute on a module that differs from the CLSCompliant attribute on the assembly");
					return;
				}
			}

			Builder.SetCustomAttribute (customBuilder);
		}

		public bool HasDefaultCharSet {
			get {
				return has_default_charset;
			}
		}

		/// <summary>
		/// It is called very early therefore can resolve only predefined attributes
		/// </summary>
		public void Resolve ()
		{
#if GMCS_SOURCE
			if (OptAttributes == null)
				return;

			if (!OptAttributes.CheckTargets())
				return;

			if (TypeManager.default_charset_type == null)
				return;

			Attribute a = ResolveAttribute (TypeManager.default_charset_type);
			if (a != null) {
				has_default_charset = true;
				DefaultCharSet = a.GetCharSetValue ();
				switch (DefaultCharSet) {
					case CharSet.Ansi:
					case CharSet.None:
						break;
					case CharSet.Auto:
						DefaultCharSetType = TypeAttributes.AutoClass;
						break;
					case CharSet.Unicode:
						DefaultCharSetType = TypeAttributes.UnicodeClass;
						break;
					default:
						Report.Error (1724, a.Location, "Value specified for the argument to 'System.Runtime.InteropServices.DefaultCharSetAttribute' is not valid");
						break;
				}
			}
#endif
		}

		public override string[] ValidAttributeTargets {
			get {
				return attribute_targets;
			}
		}
	}
}
