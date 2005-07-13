//
// block.cs: Block representation for the IL tree.
//
// Author:
//   Rafael Teixeira (rafaelteixeirabr@hotmail.com)
//   Miguel de Icaza (miguel@ximian.com)
//   Martin Baulig (martin@gnome.org)
//	 Anirban Bhattacharjee (banirban@novell.com)
//   Manjula GHM (mmanjula@novell.com)
//   Satya Sudha K (ksathyasudha@novell.com)
//
// (C) 2001, 2002, 2003, 2004, 2005 Ximian, Inc.
//

using System;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;

namespace Mono.MonoBASIC {

	using System.Collections;

	/// <summary>
	///   Block represents a VB.NET block.
	/// </summary>
	///
	/// <remarks>
	///   This class is used in a number of places: either to represent
	///   explicit blocks that the programmer places or implicit blocks.
	///
	///   Implicit blocks are used as labels or to introduce variable
	///   declarations.
	/// </remarks>
	public class Block : Statement {
		public readonly Block     Parent;
		public readonly bool      Implicit;
		public readonly Location  StartLocation;
		public Location	   EndLocation;

		//
		// The statements in this block
		//
		public ArrayList statements;

		//
		// An array of Blocks.  We keep track of children just
		// to generate the local variable declarations.
		//
		// Statements and child statements are handled through the
		// statements.
		//
		ArrayList children;
		
		//
		// Labels.  (label, block) pairs.
		//
		CaseInsensitiveHashtable labels;

		//
		// Keeps track of (name, type) pairs
		//
		CaseInsensitiveHashtable variables;

		//
		// Keeps track of constants
		CaseInsensitiveHashtable constants;

		//
		// Maps variable names to ILGenerator.LocalBuilders
		//
		//CaseInsensitiveHashtable local_builders;

		// to hold names of variables required for late binding
		public const string lateBindingArgs = "1_LBArgs";
		public const string lateBindingArgNames = "1_LBArgsNames";
		public const string lateBindingCopyBack = "1_LBCopyBack";

		bool isLateBindingRequired = false;

		bool used = false;

		static int id;

		int this_id;
		
		public Block (Block parent)
			: this (parent, false, Location.Null, Location.Null)
		{ }

		public Block (Block parent, bool implicit_block)
			: this (parent, implicit_block, Location.Null, Location.Null)
		{ }

		public Block (Block parent, bool implicit_block, Parameters parameters)
			: this (parent, implicit_block, parameters, Location.Null, Location.Null)
		{ }

		public Block (Block parent, Location start, Location end)
			: this (parent, false, start, end)
		{ }

		public Block (Block parent, Parameters parameters, Location start, Location end)
			: this (parent, false, parameters, start, end)
		{ }

		public Block (Block parent, bool implicit_block, Location start, Location end)
			: this (parent, implicit_block, Parameters.EmptyReadOnlyParameters,
				start, end)
		{ }

		public Block (Block parent, bool implicit_block, Parameters parameters,
			      Location start, Location end)
		{
			if (parent != null)
				parent.AddChild (this);
			else {
				// Top block
				// Add variables that may be required for late binding
				variables = new CaseInsensitiveHashtable ();
				ArrayList rank_specifier = new ArrayList ();
				ArrayList element = new ArrayList ();
				element.Add (new EmptyExpression ());
				rank_specifier.Add (element);
				Expression e = Mono.MonoBASIC.Parser.DecomposeQI ("System.Object[]", start);
				AddVariable (e, Block.lateBindingArgs, null, start);
				e = Mono.MonoBASIC.Parser.DecomposeQI ("System.String[]", start);
				AddVariable (e, Block.lateBindingArgNames, null, start);
				e = Mono.MonoBASIC.Parser.DecomposeQI ("System.Boolean[]", start);
				AddVariable (e, Block.lateBindingCopyBack, null, start);
			}
			
			this.Parent = parent;
			this.Implicit = implicit_block;
			this.parameters = parameters;
			this.StartLocation = start;
			this.EndLocation = end;
			this.loc = start;
			this_id = id++;
			statements = new ArrayList ();
		}

		public bool IsLateBindingRequired {
			get {
				return isLateBindingRequired;
			}
			set {
				isLateBindingRequired = value;
			}
		}

		public int ID {
			get {
				return this_id;
			}
		}

		void AddChild (Block b)
		{
			if (children == null)
				children = new ArrayList ();
			
			children.Add (b);
		}

		public void SetEndLocation (Location loc)
		{
			EndLocation = loc;
		}

		/// <summary>
		///   Adds a label to the current block. 
		/// </summary>
		///
		/// <returns>
		///   false if the name already exists in this block. true
		///   otherwise.
		/// </returns>
		///
/**
		public bool AddLabel (string name, LabeledStatement target)
		{
			if (labels == null)
				labels = new CaseInsensitiveHashtable ();
			if (labels.Contains (name))
				return false;
			
			labels.Add (name, target);
			return true;
		}
**/


		 public bool AddLabel (string name, LabeledStatement target, Location loc)
		{
/**
			if (switch_block != null)
				return switch_block.AddLabel (name, target, loc);
**/
			Block cur = this;
			while (cur != null) {

				if (cur.DoLookupLabel (name) != null) {
					Report.Error (
						140, loc, "The label '" + name +"' is a duplicate");
					return false;
				}

				if (!Implicit)
					break;

				cur = cur.Parent;
			}

			 while (cur != null) {
				if (cur.DoLookupLabel (name) != null) {
					Report.Error (
						158, loc,
						"The label '"+ name +"' shadows another label " +
						"by the same name in a containing scope.");
					return false;
				}

				if (children != null) {
					foreach (Block b in children) {
						LabeledStatement s = b.DoLookupLabel (name);
						if (s == null)
							continue;
						Report.Error (
							158, s.Location,
							"The label '"+ name +"' shadows another " +
							"label by the same name in a " +
							"containing scope.");
						return false;
					}
				}
				cur = cur.Parent;
			}
			 if (labels == null)
				labels = new CaseInsensitiveHashtable ();
			if (labels.Contains (name))
				return false;

			labels.Add (name, target);
			return true;

		}
		
		public LabeledStatement LookupLabel (string name)
		{
			LabeledStatement s = DoLookupLabel (name);
			if (s != null) 
				return s;
			
			if (children == null)
				return null;

			foreach (Block child in children) {
				if (!child.Implicit)
					continue;

				s = child.LookupLabel (name);
				if (s != null) 
					return s;
			}

			return null;
		}

		public LabeledStatement DoLookupLabel (string name)
		{
			if (labels != null){
				if (labels.Contains (name))
					return ((LabeledStatement) labels [name]);
			}
/**
			if (Parent != null)
				return Parent.LookupLabel (name);
**/
			return null;
		}

		VariableInfo this_variable = null;

		// <summary>
		//   Returns the "this" instance variable of this block.
		//   See AddThisVariable() for more information.
		// </summary>
		public VariableInfo ThisVariable {
			get {
				if (this_variable != null)
					return this_variable;
				else if (Parent != null)
					return Parent.ThisVariable;
				else
					return null;
			}
		}

		Hashtable child_variable_names;

		// <summary>
		//   Marks a variable with name @name as being used in a child block.
		//   If a variable name has been used in a child block, it's illegal to
		//   declare a variable with the same name in the current block.
		// </summary>
		public void AddChildVariableName (string name)
		{
			if (child_variable_names == null)
				child_variable_names = new CaseInsensitiveHashtable ();

			if (!child_variable_names.Contains (name))
				child_variable_names.Add (name, true);
		}

		// <summary>
		//   Marks all variables from block @block and all its children as being
		//   used in a child block.
		// </summary>
		public void AddChildVariableNames (Block block)
		{
			if (block.Variables != null) {
				foreach (string name in block.Variables.Keys)
					AddChildVariableName (name);
			}

			foreach (Block child in block.children) {
				if (child.Variables != null) {
					foreach (string name in child.Variables.Keys)
						AddChildVariableName (name);
				}
			}
		}

		// <summary>
		//   Checks whether a variable name has already been used in a child block.
		// </summary>
		public bool IsVariableNameUsedInChildBlock (string name)
		{
			if (child_variable_names == null)
				return false;

			return child_variable_names.Contains (name);
		}

		// <summary>
		//   This is used by non-static 'struct' constructors which do not have an
		//   initializer - in this case, the constructor must initialize all of the
		//   struct's fields.  To do this, we add a "this" variable and use the flow
		//   analysis code to ensure that it's been fully initialized before control
		//   leaves the constructor.
		// </summary>
		public VariableInfo AddThisVariable (TypeContainer tc, Location l)
		{
			if (this_variable != null)
				return this_variable;

			this_variable = new VariableInfo (tc, ID, l);

			if (variables == null)
				variables = new CaseInsensitiveHashtable ();
			variables.Add ("this", this_variable);

			return this_variable;
		}

		public VariableInfo AddVariable (EmitContext ec, Expression type, string name, Location l)
		{
			if (!variables_initialized)
				throw new InvalidOperationException();
				
			VariableInfo vi = AddVariable(type, name, null, loc);

			int priorCount = count_variables;
			DeclSpace ds = ec.DeclSpace;

			if (!vi.Resolve (ds)) {
				vi.Number = -1;
			} else {
				vi.Number = ++count_variables;
				if (vi.StructInfo != null)
					count_variables += vi.StructInfo.Count;
			}
			if (priorCount < count_variables)
				ec.CurrentBranching.CurrentUsageVector.AddExtraLocals(count_variables - priorCount);
				
			return vi;
		}
		
		public VariableInfo AddVariable (Expression type, string name, Parameters pars, Location l)
		{
			if (variables == null)
				variables = new CaseInsensitiveHashtable ();

			VariableInfo vi = GetVariableInfo (name);
			if (vi != null) {
				if (vi.Block != ID)
					Report.Error (30616, l, "A local variable named '" + name + "' " +
						      "cannot be declared in this scope since it would " +
						      "give a different meaning to '" + name + "', which " +
						      "is already used in a 'parent or current' scope to " +
						      "denote something else");
				else
					Report.Error (30290, l, "A local variable '" + name + "' is already " +
						      "defined in this scope");
				return null;
			}

			if (IsVariableNameUsedInChildBlock (name)) {
				Report.Error (136, l, "A local variable named '" + name + "' " +
					      "cannot be declared in this scope since it would " +
					      "give a different meaning to '" + name + "', which " +
					      "is already used in a 'child' scope to denote something " +
					      "else");
				return null;
			}

			if (pars != null) {
				int idx = 0;
				Parameter p = pars.GetParameterByName (name, out idx);
				if (p != null) {
					Report.Error (30616, l, "A local variable named '" + name + "' " +
						      "cannot be declared in this scope since it would " +
						      "give a different meaning to '" + name + "', which " +
						      "is already used in a 'parent or current' scope to " +
						      "denote something else");
					return null;
				}
			}
			
			vi = new VariableInfo (type, name, ID, l);

			variables.Add (name, vi);

			return vi;
		}

		public bool AddConstant (Expression type, string name, Expression value, Parameters pars, Location l)
		{
			if (AddVariable (type, name, pars, l) == null)
				return false;
			
			if (constants == null)
				constants = new CaseInsensitiveHashtable ();

			constants.Add (name, value);
			return true;
		}

		public Hashtable Variables {
			get {
				return variables;
			}
		}

		public VariableInfo GetVariableInfo (string name)
		{
			if (variables != null) {
				object temp;
				temp = variables [name];

				if (temp != null){
					return (VariableInfo) temp;
				}
			}

			if (Parent != null)
				return Parent.GetVariableInfo (name);

			return null;
		}
		
		public Expression GetVariableType (string name)
		{
			VariableInfo vi = GetVariableInfo (name);

			if (vi != null)
				return vi.Type;

			return null;
		}

		public Expression GetConstantExpression (string name)
		{
			if (constants != null) {
				object temp;
				temp = constants [name];
				
				if (temp != null)
					return (Expression) temp;
			}
			
			if (Parent != null)
				return Parent.GetConstantExpression (name);

			return null;
		}
		
		/// <summary>
		///   True if the variable named @name has been defined
		///   in this block
		/// </summary>
		public bool IsVariableDefined (string name)
		{
			// Console.WriteLine ("Looking up {0} in {1}", name, ID);
			if (variables != null) {
				if (variables.Contains (name))
					return true;
			}
			
			if (Parent != null)
				return Parent.IsVariableDefined (name);

			return false;
		}

		/// <summary>
		///   True if the variable named @name is a constant
		///  </summary>
		public bool IsConstant (string name)
		{
			Expression e = null;
			
			e = GetConstantExpression (name);
			
			return e != null;
		}
		
		/// <summary>
		///   Use to fetch the statement associated with this label
		/// </summary>
		public Statement this [string name] {
			get {
				return (Statement) labels [name];
			}
		}

		Parameters parameters = null;
		public Parameters Parameters {
			get {
				if (Parent != null)
					return Parent.Parameters;

				return parameters;
			}
		}

		/// <returns>
		///   A list of labels that were not used within this block
		/// </returns>
		public string [] GetUnreferenced ()
		{
			// FIXME: Implement me
			return null;
		}

		public void AddStatement (Statement s)
		{
			statements.Add (s);
			used = true;
		}

		public bool Used {
			get {
				return used;
			}
		}

		public void Use ()
		{
			used = true;
		}

		bool variables_initialized = false;
		int count_variables = 0, first_variable = 0;

		void UpdateVariableInfo (EmitContext ec)
		{
			DeclSpace ds = ec.DeclSpace;

			first_variable = 0;

			if (Parent != null)
				first_variable += Parent.CountVariables;

			count_variables = first_variable;
			if (variables != null) {
				foreach (VariableInfo vi in variables.Values) {
					if (!vi.Resolve (ds)) {
						vi.Number = -1;
						continue;
					}

					vi.Number = ++count_variables;

					if (vi.StructInfo != null)
						count_variables += vi.StructInfo.Count;
				}
			}

			variables_initialized = true;
		}

		//
		// <returns>
		//   The number of local variables in this block
		// </returns>
		public int CountVariables
		{
			get {
				if (!variables_initialized)
					throw new Exception ();

				return count_variables;
			}
		}

		/// <summary>
		///   Emits the variable declarations and labels.
		/// </summary>
		/// <remarks>
		///   tc: is our typecontainer (to resolve type references)
		///   ig: is the code generator:
		///   toplevel: the toplevel block.  This is used for checking 
		///   		that no two labels with the same name are used.
		/// </remarks>
		public void EmitMeta (EmitContext ec, Block toplevel)
		{
			//DeclSpace ds = ec.DeclSpace;
			ILGenerator ig = ec.ig;

			if (!variables_initialized)
				UpdateVariableInfo (ec);

			//
			// Process this block variables
			//
			if (variables != null){
				//local_builders = new CaseInsensitiveHashtable ();
				
				foreach (DictionaryEntry de in variables){
					string name = (string) de.Key;
					/*
					if (!isLateBindingRequired) {
						if (name.Equals (Block.lateBindingArgs) || 
						    name.Equals (Block.lateBindingArgNames) ||
						    name.Equals (Block.lateBindingCopyBack))
							continue;
					}
					*/
					VariableInfo vi = (VariableInfo) de.Value;

					if (vi.VariableType == null)
						continue;

					vi.LocalBuilder = ig.DeclareLocal (vi.VariableType);

					if (CodeGen.SymbolWriter != null)
						vi.LocalBuilder.SetLocalSymInfo (name);

					if (constants == null)
						continue;

					Expression cv = (Expression) constants [name];
					if (cv == null)
						continue;

					Expression e = cv.Resolve (ec);
					if (e == null)
						continue;

					if (!(e is Constant)){
						Report.Error (133, vi.Location,
							      "The expression being assigned to '" +
							      name + "' must be constant (" + e + ")");
						continue;
					}

					constants.Remove (name);
					constants.Add (name, e);
				}
			}

			//
			// Now, handle the children
			//
			if (children != null){
				foreach (Block b in children)
					b.EmitMeta (ec, toplevel);
			}
		}

		public void UsageWarning ()
		{
			string name;
			
			if (variables != null){
				foreach (DictionaryEntry de in variables){
					VariableInfo vi = (VariableInfo) de.Value;
					
					if (vi.Used)
						continue;
					
					name = (string) de.Key;
						
					if (vi.Assigned){
						Report.Warning (
							219, vi.Location, "The variable '" + name +
							"' is assigned but its value is never used");
					} else {
						if (!(name.Equals(lateBindingArgs)||name.Equals(lateBindingArgNames)||name.Equals(lateBindingCopyBack)))
						Report.Warning (
							168, vi.Location, "The variable '" +
							name +"' is declared but never used");
					} 
				}
			}

			if (children != null)
				foreach (Block b in children)
					b.UsageWarning ();
		}

		bool has_ret = false;

		public override bool Resolve (EmitContext ec)
		{
			Block prev_block = ec.CurrentBlock;
			bool ok = true;

			ec.CurrentBlock = this;

			if (!variables_initialized)
				UpdateVariableInfo (ec);
				
			ec.StartFlowBranching (this);

			Report.Debug (1, "RESOLVE BLOCK", StartLocation, ec.CurrentBranching);

			ArrayList new_statements = new ArrayList ();
			bool unreachable = false, warning_shown = false;

 			foreach (Statement s in statements){

				if (unreachable && !(s is LabeledStatement)) {
					if (!warning_shown && !(s is EmptyStatement)) {
						warning_shown = true;
						Warning_DeadCodeFound (s.loc);
					}
					continue;
				}

				if (s.Resolve (ec) == false) {
 					ok = false;
					continue;
				}

				if (s is LabeledStatement)
					unreachable = false;
				else
					unreachable = ! ec.CurrentBranching.IsReachable ();

				new_statements.Add (s);
			}

			statements = new_statements;

			Report.Debug (1, "RESOLVE BLOCK DONE", StartLocation, ec.CurrentBranching);

			FlowReturns returns = ec.EndFlowBranching ();
			ec.CurrentBlock = prev_block;

			// If we're a non-static 'struct' constructor which doesn't have an
			// initializer, then we must initialize all of the struct's fields.
			if ((this_variable != null) && (returns != FlowReturns.EXCEPTION) &&
			    !this_variable.IsAssigned (ec, loc))
				ok = false;

			if ((labels != null) && (RootContext.WarningLevel >= 2)) {
				foreach (LabeledStatement label in labels.Values)
					if (!label.HasBeenReferenced)
						Report.Warning (164, label.Location,
								"This label has not been referenced");
			}

			if ((returns == FlowReturns.ALWAYS) ||
			    (returns == FlowReturns.EXCEPTION) ||
			    (returns == FlowReturns.UNREACHABLE))
				has_ret = true;

			return ok;
		}
		
		protected override bool DoEmit (EmitContext ec)
		{
			Block prev_block = ec.CurrentBlock;

			ec.CurrentBlock = this;

			ec.Mark (StartLocation);
			foreach (Statement s in statements)
				s.Emit (ec);
				
			ec.Mark (EndLocation); 
			
			ec.CurrentBlock = prev_block;
			return has_ret;
		}
		
		public override string ToString ()
		{
			return String.Format ("{0} ({1}:{2})", GetType (),ID, StartLocation);
		}

	} // class Block

} // namespace Mono.MonoBASIC
