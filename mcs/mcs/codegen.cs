//
// codegen.cs: The code generator
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

using System;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CSharp {

	/// <summary>
	///    Code generator class.
	/// </summary>
	public class CodeGen {
		static AppDomain current_domain;
		public static AssemblyBuilder AssemblyBuilder;
		public static ModuleBuilder   ModuleBuilder;

		static public SymbolWriter SymbolWriter;

		public static string Basename (string name)
		{
			int pos = name.LastIndexOf ("/");

			if (pos != -1)
				return name.Substring (pos + 1);

			pos = name.LastIndexOf ("\\");
			if (pos != -1)
				return name.Substring (pos + 1);

			return name;
		}

		public static string Dirname (string name)
		{
			int pos = name.LastIndexOf ("/");

			if (pos != -1)
				return name.Substring (0, pos);

			pos = name.LastIndexOf ("\\");
			if (pos != -1)
				return name.Substring (0, pos);

			return ".";
		}

		static string TrimExt (string name)
		{
			int pos = name.LastIndexOf (".");

			return name.Substring (0, pos);
		}

		static public string FileName;

		//
		// Initializes the symbol writer
		//
		static void InitializeSymbolWriter ()
		{
			SymbolWriter = SymbolWriter.GetSymbolWriter (ModuleBuilder);

			//
			// If we got an ISymbolWriter instance, initialize it.
			//
			if (SymbolWriter == null) {
				Report.Warning (
					-18, "Could not find the symbol writer assembly (Mono.CSharp.Debugger.dll). This is normally an installation problem. Please make sure to compile and install the mcs/class/Mono.CSharp.Debugger directory.");
				return;
			}
		}

		//
		// Initializes the code generator variables
		//
		static public void Init (string name, string output, bool want_debugging_support)
		{
			AssemblyName an;

			FileName = output;
			an = new AssemblyName ();
			an.Name = Path.GetFileNameWithoutExtension (name);
			
			current_domain = AppDomain.CurrentDomain;
			AssemblyBuilder = current_domain.DefineDynamicAssembly (
				an, AssemblyBuilderAccess.RunAndSave, Dirname (name));

			//
			// Pass a path-less name to DefineDynamicModule.  Wonder how
			// this copes with output in different directories then.
			// FIXME: figure out how this copes with --output /tmp/blah
			//
			// If the third argument is true, the ModuleBuilder will dynamically
			// load the default symbol writer.
			//
			ModuleBuilder = AssemblyBuilder.DefineDynamicModule (
				Basename (name), Basename (output), want_debugging_support);

			if (want_debugging_support)
				InitializeSymbolWriter ();
		}

		static public void Save (string name)
		{
			try {
				AssemblyBuilder.Save (Basename (name));
			} catch (System.IO.IOException io){
				Report.Error (16, "Could not write to file `"+name+"', cause: " + io.Message);
			}
		}
	}

	/// <summary>
	///   An Emit Context is created for each body of code (from methods,
	///   properties bodies, indexer bodies or constructor bodies)
	/// </summary>
	public class EmitContext {
		public DeclSpace DeclSpace;
		public DeclSpace TypeContainer;
		public ILGenerator   ig;

		/// <summary>
		///   This variable tracks the `checked' state of the compilation,
		///   it controls whether we should generate code that does overflow
		///   checking, or if we generate code that ignores overflows.
		///
		///   The default setting comes from the command line option to generate
		///   checked or unchecked code plus any source code changes using the
		///   checked/unchecked statements or expressions.   Contrast this with
		///   the ConstantCheckState flag.
		/// </summary>
		
		public bool CheckState;

		/// <summary>
		///   The constant check state is always set to `true' and cant be changed
		///   from the command line.  The source code can change this setting with
		///   the `checked' and `unchecked' statements and expressions. 
		/// </summary>
		public bool ConstantCheckState;

		/// <summary>
		///   Whether we are emitting code inside a static or instance method
		/// </summary>
		public bool IsStatic;

		/// <summary>
		///   Whether we are emitting a field initializer
		/// </summary>
		public bool IsFieldInitializer;

		/// <summary>
		///   The value that is allowed to be returned or NULL if there is no
		///   return type.
		/// </summary>
		public Type ReturnType;

		/// <summary>
		///   Points to the Type (extracted from the TypeContainer) that
		///   declares this body of code
		/// </summary>
		public Type ContainerType;
		
		/// <summary>
		///   Whether this is generating code for a constructor
		/// </summary>
		public bool IsConstructor;

		/// <summary>
		///   Whether we're control flow analysis enabled
		/// </summary>
		public bool DoFlowAnalysis;
		
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
		///   Whether we are in a Finally block
		/// </summary>
		public bool InFinally;

		/// <summary>
		///   Whether we are in a Try block
		/// </summary>
		public bool InTry;

		/// <summary>
		///   Whether we are inside an iterator block.
		/// </summary>
		public bool InIterator;

		/// <summary>
		///   Whether we need an explicit return statement at the end of the method.
		/// </summary>
		public bool NeedExplicitReturn;
		
		/// <summary>
		///   Whether remapping of locals, parameters and fields is turned on.
		///   Used by iterators and anonymous methods.
		/// </summary>
		public bool RemapToProxy;

		/// <summary>
		///   Whether we are in a Catch block
		/// </summary>
		public bool InCatch;

		/// <summary>
		///  Whether we are inside an unsafe block
		/// </summary>
		public bool InUnsafe;

		/// <summary>
		///  Whether we are in a `fixed' initialization
		/// </summary>
		public bool InFixedInitializer;

		/// <summary>
		///  Whether we are inside an anonymous method.
		/// </summary>
		public bool InAnonymousMethod;
		
		/// <summary>
		///   Location for this EmitContext
		/// </summary>
		public Location loc;

		/// <summary>
		///   Used to flag that it is ok to define types recursively, as the
		///   expressions are being evaluated as part of the type lookup
		///   during the type resolution process
		/// </summary>
		public bool ResolvingTypeTree;
		
		/// <summary>
		///   Inside an enum definition, we do not resolve enumeration values
		///   to their enumerations, but rather to the underlying type/value
		///   This is so EnumVal + EnumValB can be evaluated.
		///
		///   There is no "E operator + (E x, E y)", so during an enum evaluation
		///   we relax the rules
		/// </summary>
		public bool InEnumContext;

		protected Stack FlowStack;
		
		public EmitContext (DeclSpace parent, DeclSpace ds, Location l, ILGenerator ig,
				    Type return_type, int code_flags, bool is_constructor)
		{
			this.ig = ig;

			TypeContainer = parent;
			DeclSpace = ds;
			CheckState = RootContext.Checked;
			ConstantCheckState = true;
			
			IsStatic = (code_flags & Modifiers.STATIC) != 0;
			InIterator = (code_flags & Modifiers.METHOD_YIELDS) != 0;
			RemapToProxy = InIterator;
			ReturnType = return_type;
			IsConstructor = is_constructor;
			CurrentBlock = null;
			CurrentFile = 0;
			
			if (parent != null){
				// Can only be null for the ResolveType contexts.
				ContainerType = parent.TypeBuilder;
				if (parent.UnsafeContext)
					InUnsafe = true;
				else
					InUnsafe = (code_flags & Modifiers.UNSAFE) != 0;
			}
			loc = l;

			FlowStack = new Stack ();
			
			if (ReturnType == TypeManager.void_type)
				ReturnType = null;
		}

		public EmitContext (TypeContainer tc, Location l, ILGenerator ig,
				    Type return_type, int code_flags, bool is_constructor)
			: this (tc, tc, l, ig, return_type, code_flags, is_constructor)
		{
		}

		public EmitContext (TypeContainer tc, Location l, ILGenerator ig,
				    Type return_type, int code_flags)
			: this (tc, tc, l, ig, return_type, code_flags, false)
		{
		}

		public FlowBranching CurrentBranching {
			get {
				return (FlowBranching) FlowStack.Peek ();
			}
		}

		// <summary>
		//   Starts a new code branching.  This inherits the state of all local
		//   variables and parameters from the current branching.
		// </summary>
		public FlowBranching StartFlowBranching (FlowBranchingType type, Location loc)
		{
			FlowBranching cfb = new FlowBranching (CurrentBranching, type, null, loc);

			FlowStack.Push (cfb);

			return cfb;
		}

		// <summary>
		//   Starts a new code branching for block `block'.
		// </summary>
		public FlowBranching StartFlowBranching (Block block)
		{
			FlowBranching cfb;
			FlowBranchingType type;

			if (CurrentBranching.Type == FlowBranchingType.SWITCH)
				type = FlowBranchingType.SWITCH_SECTION;
			else
				type = FlowBranchingType.BLOCK;

			cfb = new FlowBranching (CurrentBranching, type, block, block.StartLocation);

			FlowStack.Push (cfb);

			return cfb;
		}

		// <summary>
		//   Ends a code branching.  Merges the state of locals and parameters
		//   from all the children of the ending branching.
		// </summary>
		public FlowReturns EndFlowBranching ()
		{
			FlowBranching cfb = (FlowBranching) FlowStack.Pop ();

			return CurrentBranching.MergeChild (cfb);
		}

		// <summary>
		//   Kills the current code branching.  This throws away any changed state
		//   information and should only be used in case of an error.
		// </summary>
		public void KillFlowBranching ()
		{
			FlowBranching cfb = (FlowBranching) FlowStack.Pop ();
		}

		public void EmitTopBlock (Block block, InternalParameters ip, Location loc)
		{
			bool has_ret = false;

			if (!Location.IsNull (loc))
				CurrentFile = loc.File;

			if (block != null){
			    try {
				int errors = Report.Errors;

				block.EmitMeta (this, ip, block);

				if (Report.Errors == errors){
					bool old_do_flow_analysis = DoFlowAnalysis;
					DoFlowAnalysis = true;

					FlowBranching cfb = new FlowBranching (block, loc);
					FlowStack.Push (cfb);

					if (!block.Resolve (this)) {
						FlowStack.Pop ();
						DoFlowAnalysis = old_do_flow_analysis;
						return;
					}

					cfb = (FlowBranching) FlowStack.Pop ();
					FlowReturns returns = cfb.MergeTopBlock ();

					DoFlowAnalysis = old_do_flow_analysis;

					has_ret = block.Emit (this);

					if ((returns == FlowReturns.ALWAYS) ||
					    (returns == FlowReturns.EXCEPTION) ||
					    (returns == FlowReturns.UNREACHABLE))
						has_ret = true;

					if (Report.Errors == errors){
						if (RootContext.WarningLevel >= 3)
							block.UsageWarning ();
					}
				}
			    } catch {
					Console.WriteLine ("Exception caught by the compiler while compiling:");
					Console.WriteLine ("   Block that caused the problem begin at: " + loc);
					
					if (CurrentBlock != null){
						Console.WriteLine ("                     Block being compiled: [{0},{1}]",
								   CurrentBlock.StartLocation, CurrentBlock.EndLocation);
					}
					throw;
			    }
			}

			if (ReturnType != null && !has_ret){
				//
				// FIXME: we need full flow analysis to implement this
				// correctly and emit an error instead of a warning.
				//
				//
				if (!InIterator){
					Report.Error (161, loc, "Not all code paths return a value");
					return;
				}
			}

			if (HasReturnLabel)
				ig.MarkLabel (ReturnLabel);
			if (return_value != null){
				ig.Emit (OpCodes.Ldloc, return_value);
				ig.Emit (OpCodes.Ret);
			} else {
				if (!InTry){
					if (InIterator)
						has_ret = true;
					
					if (!has_ret || HasReturnLabel) {
						ig.Emit (OpCodes.Ret);
						NeedExplicitReturn = false;
					}
				}

				// Unfortunately, System.Reflection.Emit automatically emits a leave
				// to the end of a finally block.  This is a problem if no code is
				// following the try/finally block since we may jump to a point after
				// the end of the method. As a workaround, emit an explicit ret here.

				if (NeedExplicitReturn) {
					if (ReturnType != null)
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
			if ((CodeGen.SymbolWriter == null) || Location.IsNull (loc))
				return;

			if (check_file && (CurrentFile != loc.File))
				return;

			ig.MarkSequencePoint (null, loc.Row, 0, 0, 0);
		}

		/// <summary>
		///   Returns a temporary storage for a variable of type t as 
		///   a local variable in the current body.
		/// </summary>
		public LocalBuilder GetTemporaryStorage (Type t)
		{
			LocalBuilder location;
			
			if (temporary_storage != null){
				location = (LocalBuilder) temporary_storage [t];
				if (location != null)
					return location;
			}
			
			location = ig.DeclareLocal (t);
			
			return location;
		}

		public void FreeTemporaryStorage (LocalBuilder b)
		{
			// Empty for now.
		}

		/// <summary>
		///   Current loop begin and end labels.
		/// </summary>
		public Label LoopBegin, LoopEnd;

		/// <summary>
		///   Whether we are inside a loop and break/continue are possible.
		/// </summary>
		public bool  InLoop;

		/// <summary>
		///   This is incremented each time we enter a try/catch block and
		///   decremented if we leave it.
		/// </summary>
		public int   TryCatchLevel;

		/// <summary>
		///   The TryCatchLevel at the begin of the current loop.
		/// </summary>
		public int   LoopBeginTryCatchLevel;

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
		/// </summary>
		public LocalBuilder TemporaryReturn ()
		{
			if (return_value == null){
				return_value = ig.DeclareLocal (ReturnType);
				ReturnLabel = ig.DefineLabel ();
				HasReturnLabel = true;
			}

			return return_value;
		}

		//
		// Creates a field `name' with the type `t' on the proxy class
		//
		public FieldBuilder MapVariable (string name, Type t)
		{
			if (InIterator){
				return IteratorHandler.Current.MapVariable (name, t);
			}

			throw new Exception ("MapVariable for an unknown state");
		}

		//
		// Invoke this routine to remap a VariableInfo into the
		// proper MemberAccess expression
		//
		public Expression RemapLocal (LocalInfo local_info)
		{
			FieldExpr fe = new FieldExpr (local_info.FieldBuilder, loc);
			fe.InstanceExpression = new ProxyInstance ();
			return fe.DoResolve (this);
		}

		public Expression RemapLocalLValue (LocalInfo local_info, Expression right_side)
		{
			FieldExpr fe = new FieldExpr (local_info.FieldBuilder, loc);
			fe.InstanceExpression = new ProxyInstance ();
			return fe.DoResolveLValue (this, right_side);
		}

		public Expression RemapParameter (int idx)
		{
			FieldExpr fe = new FieldExprNoAddress (IteratorHandler.Current.parameter_fields [idx], loc);
			fe.InstanceExpression = new ProxyInstance ();
			return fe.DoResolve (this);
		}

		public Expression RemapParameterLValue (int idx, Expression right_side)
		{
			FieldExpr fe = new FieldExprNoAddress (IteratorHandler.Current.parameter_fields [idx], loc);
			fe.InstanceExpression = new ProxyInstance ();
			return fe.DoResolveLValue (this, right_side);
		}
		
		//
		// Emits the proper object to address fields on a remapped
		// variable/parameter to field in anonymous-method/iterator proxy classes.
		//
		public void EmitThis ()
		{
			ig.Emit (OpCodes.Ldarg_0);

			if (!IsStatic){
				if (InIterator)
					ig.Emit (OpCodes.Ldfld, IteratorHandler.Current.this_field);
				else
					throw new Exception ("EmitThis for an unknown state");
			}
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
}
