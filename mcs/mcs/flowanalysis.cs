//
// flowanalyis.cs: The control flow analysis code
//
// Author:
//   Martin Baulig (martin@ximian.com)
//
// (C) 2001, 2002, 2003 Ximian, Inc.
//

using System;
using System.Text;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;

namespace Mono.CSharp
{
	// <summary>
	//   A new instance of this class is created every time a new block is resolved
	//   and if there's branching in the block's control flow.
	// </summary>
	public abstract class FlowBranching
	{
		// <summary>
		//   The type of a FlowBranching.
		// </summary>
		public enum BranchingType {
			// Normal (conditional or toplevel) block.
			Block,

			// A loop block.
			LoopBlock,

			// Try/Catch block.
			Exception,

			// Switch block.
			Switch,

			// Switch section.
			SwitchSection
		}

		// <summary>
		//   The type of one sibling of a branching.
		// </summary>
		public enum SiblingType {
			Conditional,
			SwitchSection,
			Try,
			Catch,
			Finally
		}

		// <summary>
		//   This is used in the control flow analysis code to specify whether the
		//   current code block may return to its enclosing block before reaching
		//   its end.
		// </summary>
		public enum FlowReturns {
			Undefined = 0,

			// It can never return.
			Never,

			// This means that the block contains a conditional return statement
			// somewhere.
			Sometimes,

			// The code always returns, ie. there's an unconditional return / break
			// statement in it.
			Always,

			// The code always throws an exception.
			Exception,

			// The current code block is unreachable.  This happens if it's immediately
			// following a FlowReturns.Always block.
			Unreachable
		}

		public static FlowBranching CreateBranching (FlowBranching parent, BranchingType type, Block block, Location loc)
		{
			switch (type) {
			case BranchingType.Exception:
				return new FlowBranchingException (parent, type, block, loc);

			case BranchingType.Switch:
				return new FlowBranchingBlock (parent, type, SiblingType.SwitchSection, block, loc);

			default:
				return new FlowBranchingBlock (parent, type, SiblingType.Conditional, block, loc);
			}
		}

		// <summary>
		//   The type of this flow branching.
		// </summary>
		public readonly BranchingType Type;

		// <summary>
		//   The block this branching is contained in.  This may be null if it's not
		//   a top-level block and it doesn't declare any local variables.
		// </summary>
		public readonly Block Block;

		// <summary>
		//   The parent of this branching or null if this is the top-block.
		// </summary>
		public readonly FlowBranching Parent;

		// <summary>
		//   Start-Location of this flow branching.
		// </summary>
		public readonly Location Location;

		// <summary>
		//   If this is an infinite loop.
		// </summary>
		public bool Infinite;

		// <summary>
		//   If we may leave the current loop.
		// </summary>
		public bool MayLeaveLoop;

		//
		// Private
		//
		VariableMap param_map, local_map;

		static int next_id = 0;
		int id;

		// <summary>
		//   Performs an `And' operation on the FlowReturns status
		//   (for instance, a block only returns Always if all its siblings
		//   always return).
		// </summary>
		public static FlowReturns AndFlowReturns (FlowReturns a, FlowReturns b)
		{
			if (a == FlowReturns.Undefined)
				return b;
			if (b == FlowReturns.Unreachable)
				return a;

			switch (a) {
			case FlowReturns.Never:
				if (b == FlowReturns.Never)
					return FlowReturns.Never;
				else
					return FlowReturns.Sometimes;

			case FlowReturns.Sometimes:
				return FlowReturns.Sometimes;

			case FlowReturns.Always:
				if ((b == FlowReturns.Always) || (b == FlowReturns.Exception))
					return FlowReturns.Always;
				else
					return FlowReturns.Sometimes;

			case FlowReturns.Exception:
				if (b == FlowReturns.Exception)
					return FlowReturns.Exception;
				else if (b == FlowReturns.Always)
					return FlowReturns.Always;
				else
					return FlowReturns.Sometimes;
			}

			return b;
		}

		// <summary>
		//   The vector contains a BitArray with information about which local variables
		//   and parameters are already initialized at the current code position.
		// </summary>
		public class UsageVector {
			// <summary>
			//   The type of this branching.
			// </summary>
			public readonly SiblingType Type;

			// <summary>
			//   Start location of this branching.
			// </summary>
			public readonly Location Location;

			// <summary>
			//   If this is true, then the usage vector has been modified and must be
			//   merged when we're done with this branching.
			// </summary>
			public bool IsDirty;

			// <summary>
			//   The number of parameters in this block.
			// </summary>
			public readonly int CountParameters;

			// <summary>
			//   The number of locals in this block.
			// </summary>
			public readonly int CountLocals;

			// <summary>
			//   If not null, then we inherit our state from this vector and do a
			//   copy-on-write.  If null, then we're the first sibling in a top-level
			//   block and inherit from the empty vector.
			// </summary>
			public readonly UsageVector InheritsFrom;

			//
			// Private.
			//
			MyBitVector locals, parameters;
			FlowReturns RealReturns, RealBreaks, RealReachable;
			bool is_finally;

			static int next_id = 0;
			int id;

			//
			// Normally, you should not use any of these constructors.
			//
			public UsageVector (SiblingType type, UsageVector parent, Location loc, int num_params, int num_locals)
			{
				this.Type = type;
				this.Location = loc;
				this.InheritsFrom = parent;
				this.CountParameters = num_params;
				this.CountLocals = num_locals;
				this.RealReturns = FlowReturns.Never;
				this.RealBreaks = FlowReturns.Never;
				this.RealReachable = FlowReturns.Always;

				if (parent != null) {
					locals = new MyBitVector (parent.locals, CountLocals);
					if (num_params > 0)
						parameters = new MyBitVector (parent.parameters, num_params);
					RealReturns = parent.Returns;
					RealBreaks = parent.Breaks;
				} else {
					locals = new MyBitVector (null, CountLocals);
					if (num_params > 0)
						parameters = new MyBitVector (null, num_params);
				}

				id = ++next_id;
			}

			public UsageVector (SiblingType type, UsageVector parent, Location loc)
				: this (type, parent, loc, parent.CountParameters, parent.CountLocals)
			{ }

			// <summary>
			//   This does a deep copy of the usage vector.
			// </summary>
			public UsageVector Clone ()
			{
				UsageVector retval = new UsageVector (Type, null, Location, CountParameters, CountLocals);

				retval.locals = locals.Clone ();
				if (parameters != null)
					retval.parameters = parameters.Clone ();
				retval.RealReturns = RealReturns;
				retval.RealBreaks = RealBreaks;
				retval.RealReachable = RealReachable;

				return retval;
			}

			public bool IsAssigned (VariableInfo var)
			{
				if (!var.IsParameter && AlwaysBreaks)
					return true;

				return var.IsAssigned (var.IsParameter ? parameters : locals);
			}

			public void SetAssigned (VariableInfo var)
			{
				if (!var.IsParameter && AlwaysBreaks)
					return;

				var.SetAssigned (var.IsParameter ? parameters : locals);
			}

			public bool IsFieldAssigned (VariableInfo var, string name)
			{
				if (!var.IsParameter && AlwaysBreaks)
					return true;

				return var.IsFieldAssigned (var.IsParameter ? parameters : locals, name);
			}

			public void SetFieldAssigned (VariableInfo var, string name)
			{
				if (!var.IsParameter && AlwaysBreaks)
					return;

				var.SetFieldAssigned (var.IsParameter ? parameters : locals, name);
			}

			// <summary>
			//   Specifies when the current block returns.
			//   If this is FlowReturns.Unreachable, then control can never reach the
			//   end of the method (so that we don't need to emit a return statement).
			//   The same applies for FlowReturns.Exception, but in this case the return
			//   value will never be used.
			// </summary>
			public FlowReturns Returns {
				get {
					return RealReturns;
				}
			}

			// <summary>
			//   Specifies whether control may return to our containing block
			//   before reaching the end of this block.  This happens if there
			//   is a break/continue/goto/return in it.
			//   This can also be used to find out whether the statement immediately
			//   following the current block may be reached or not.
			// </summary>
			public FlowReturns Breaks {
				get {
					return RealBreaks;
				}
			}

			public FlowReturns Reachable {
				get {
					return RealReachable;
				}
			}

			public bool AlwaysBreaks {
				get {
					return (Breaks == FlowReturns.Always) ||
						(Breaks == FlowReturns.Exception) ||
						(Breaks == FlowReturns.Unreachable);
				}
			}

			public bool MayBreak {
				get {
					return Breaks != FlowReturns.Never;
				}
			}

			public bool AlwaysReturns {
				get {
					return (Returns == FlowReturns.Always) ||
						(Returns == FlowReturns.Exception);
				}
			}

			public bool MayReturn {
				get {
					return (Returns == FlowReturns.Sometimes) ||
						(Returns == FlowReturns.Always);
				}
			}

			public void Break ()
			{
				RealBreaks = FlowReturns.Always;
			}

			public void Return ()
			{
				RealReturns = FlowReturns.Always;
			}

			public bool IsUnreachable {
				get {
					return (Reachable == FlowReturns.Exception) ||
						(Reachable == FlowReturns.Unreachable);
				}
			}

			public void Unreachable ()
			{
				// If we're already unreachable, don't modify the reason why.
				if (!IsUnreachable)
					RealReachable = FlowReturns.Unreachable;
			}

			public void NeverReachable ()
			{
				// If we're already unreachable, don't modify the reason why.
				if (!IsUnreachable)
					RealReachable = FlowReturns.Never;
			}

			public void Throw ()
			{
				// If we're already unreachable, don't modify the reason why.
				if (!IsUnreachable)
					RealReachable = FlowReturns.Exception;
			}

			// <summary>
			//   Merges a child branching.
			// </summary>
			public FlowReturns MergeChild (MyBitVector new_params, MyBitVector new_locals,
						       FlowReturns new_returns, FlowReturns new_breaks,
						       FlowReturns new_reachable)
			{
				Report.Debug (2, "MERGING CHILD", this, new_params, new_locals, new_returns, new_breaks,
					      new_reachable);

				RealReturns = new_returns;
				RealBreaks = new_breaks;
				RealReachable = new_reachable;

				//
				// We've now either reached the point after the branching or we will
				// never get there since we always return or always throw an exception.
				//
				// If we can reach the point after the branching, mark all locals and
				// parameters as initialized which have been initialized in all branches
				// we need to look at (see above).
				//

				Report.Debug (2, "MERGING CHILD #1", this, Returns, Breaks, Reachable, new_locals, new_params);

				if ((Reachable == FlowReturns.Always) || (Reachable == FlowReturns.Sometimes) ||
				    (Reachable == FlowReturns.Never)) {
					if ((Returns == FlowReturns.Always) || (Breaks == FlowReturns.Always))
						RealReachable = FlowReturns.Never;
					if ((Type == SiblingType.SwitchSection) && (Reachable != FlowReturns.Never)) {
						Report.Error (163, Location, "Control cannot fall through from one " +
							      "case label to another");
					}

					if (new_locals != null)
						locals.Or (new_locals);

					if (new_params != null)
						parameters.Or (new_params);
				}

				Report.Debug (2, "MERGING CHILD DONE", this);

				return Returns;
			}

			// <summary>
			//   Tells control flow analysis that the current code position may be reached with
			//   a forward jump from any of the origins listed in `origin_vectors' which is a
			//   list of UsageVectors.
			//
			//   This is used when resolving forward gotos - in the following example, the
			//   variable `a' is uninitialized in line 8 becase this line may be reached via
			//   the goto in line 4:
			//
			//      1     int a;
			//
			//      3     if (something)
			//      4        goto World;
			//
			//      6     a = 5;
			//
			//      7  World:
			//      8     Console.WriteLine (a);
			//
			// </summary>
			public void MergeJumpOrigins (ICollection origin_vectors)
			{
				Report.Debug (1, "MERGING JUMP ORIGIN", this);

				RealBreaks = FlowReturns.Never;
				RealReturns = FlowReturns.Never;
				if (Reachable != FlowReturns.Always)
					RealReachable = FlowReturns.Always;

				if (origin_vectors == null)
					return;

				foreach (UsageVector vector in origin_vectors) {
					Report.Debug (1, "  MERGING JUMP ORIGIN", vector);

					locals.And (vector.locals);
					if (parameters != null)
						parameters.And (vector.parameters);
					RealBreaks = AndFlowReturns (RealBreaks, vector.Breaks);
					RealReturns = AndFlowReturns (RealReturns, vector.Returns);
					RealReachable = AndFlowReturns (RealReachable, vector.Reachable);
				}

				Report.Debug (1, "MERGING JUMP ORIGIN DONE", this);
			}

			// <summary>
			//   This is used at the beginning of a finally block if there were
			//   any return statements in the try block or one of the catch blocks.
			// </summary>
			public void MergeFinallyOrigins (ICollection finally_vectors)
			{
				Report.Debug (1, "MERGING FINALLY ORIGIN", this);

				RealBreaks = FlowReturns.Never;

				foreach (UsageVector vector in finally_vectors) {
					Report.Debug (1, "  MERGING FINALLY ORIGIN", vector);

					if (parameters != null)
						parameters.And (vector.parameters);
					RealBreaks = AndFlowReturns (Breaks, vector.Breaks);
				}

				is_finally = true;

				Report.Debug (1, "MERGING FINALLY ORIGIN DONE", this);
			}

			public void CheckOutParameters (FlowBranching branching)
			{
				if (parameters != null)
					branching.CheckOutParameters (parameters, branching.Location);
			}

			// <summary>
			//   Performs an `or' operation on the locals and the parameters.
			// </summary>
			public void Or (UsageVector new_vector)
			{
				locals.Or (new_vector.locals);
				if (parameters != null)
					parameters.Or (new_vector.parameters);
			}

			// <summary>
			//   Performs an `and' operation on the locals.
			// </summary>
			public void AndLocals (UsageVector new_vector)
			{
				locals.And (new_vector.locals);
			}

			public bool HasParameters {
				get {
					return parameters != null;
				}
			}

			public bool HasLocals {
				get {
					return locals != null;
				}
			}

			// <summary>
			//   Returns a deep copy of the parameters.
			// </summary>
			public MyBitVector Parameters {
				get {
					if (parameters != null)
						return parameters.Clone ();
					else
						return null;
				}
			}

			// <summary>
			//   Returns a deep copy of the locals.
			// </summary>
			public MyBitVector Locals {
				get {
					return locals.Clone ();
				}
			}

			public MyBitVector ParameterVector {
				get {
					return parameters;
				}
			}

			public MyBitVector LocalVector {
				get {
					return locals;
				}
			}

			//
			// Debugging stuff.
			//

			public override string ToString ()
			{
				StringBuilder sb = new StringBuilder ();

				sb.Append ("Vector (");
				sb.Append (id);
				sb.Append (",");
				sb.Append (Returns);
				sb.Append (",");
				sb.Append (Breaks);
				sb.Append (",");
				sb.Append (Reachable);
				if (parameters != null) {
					sb.Append (" - ");
					sb.Append (parameters);
				}
				sb.Append (" - ");
				sb.Append (locals);
				sb.Append (")");

				return sb.ToString ();
			}
		}

		// <summary>
		//   Creates a new flow branching which is contained in `parent'.
		//   You should only pass non-null for the `block' argument if this block
		//   introduces any new variables - in this case, we need to create a new
		//   usage vector with a different size than our parent's one.
		// </summary>
		protected FlowBranching (FlowBranching parent, BranchingType type, SiblingType stype,
					 Block block, Location loc)
		{
			Parent = parent;
			Block = block;
			Location = loc;
			Type = type;
			id = ++next_id;

			UsageVector vector;
			if (Block != null) {
				param_map = Block.ParameterMap;
				local_map = Block.LocalMap;

				UsageVector parent_vector = parent != null ? parent.CurrentUsageVector : null;
				vector = new UsageVector (stype, parent_vector, loc, param_map.Length, local_map.Length);
			} else {
				param_map = Parent.param_map;
				local_map = Parent.local_map;
				vector = new UsageVector (stype, Parent.CurrentUsageVector, loc);
			}

			AddSibling (vector);
		}

		public abstract UsageVector CurrentUsageVector {
			get;
		}				

		// <summary>
		//   Creates a sibling of the current usage vector.
		// </summary>
		public virtual void CreateSibling (SiblingType type)
		{
			AddSibling (new UsageVector (type, Parent.CurrentUsageVector, Location));

			Report.Debug (1, "CREATED SIBLING", CurrentUsageVector);
		}

		protected abstract void AddSibling (UsageVector uv);

		public abstract void Break ();
		public abstract void Return ();
		public abstract void Goto ();
		public abstract void Throw ();
		public abstract void Label (ArrayList origin_vectors);

		// <summary>
		//   Check whether all `out' parameters have been assigned.
		// </summary>
		public void CheckOutParameters (MyBitVector parameters, Location loc)
		{
			for (int i = 0; i < param_map.Count; i++) {
				VariableInfo var = param_map [i];

				if (var == null)
					continue;

				if (var.IsAssigned (parameters))
					continue;

				Report.Error (177, loc, "The out parameter `" +
					      param_map.VariableNames [i] + "' must be " +
					      "assigned before control leave the current method.");
			}
		}

		protected class MergeResult
		{
			public MyBitVector Parameters;
			public MyBitVector Locals;
			public FlowReturns Returns;
			public FlowReturns Breaks;
			public FlowReturns Reachable;
			public bool MayLeaveLoop;

			public MergeResult (MyBitVector parameters, MyBitVector locals, FlowReturns returns, FlowReturns breaks,
					    FlowReturns reachable, bool may_leave_loop)
			{
				this.Parameters = parameters;
				this.Locals = locals;
				this.Returns = returns;
				this.Breaks = breaks;
				this.Reachable = reachable;
				this.MayLeaveLoop = may_leave_loop;
			}
		}

		protected MergeResult Merge (ArrayList children)
		{
			MyBitVector locals = null;
			MyBitVector parameters = null;

			FlowReturns returns = FlowReturns.Undefined;
			FlowReturns breaks = FlowReturns.Undefined;
			FlowReturns reachable = FlowReturns.Undefined;

			Report.Debug (2, "MERGING CHILDREN", this, Type, children.Count);

			int children_count = children.Count;
			for (int ix = 0; ix < children_count; ix++){
				UsageVector child = (UsageVector) children [ix];
				
				Report.Debug (2, "  MERGING CHILD", child, child.AlwaysBreaks, child.AlwaysReturns,
					      child.IsUnreachable, child.Locals, child.Parameters,
					      child.Returns, child.Breaks, child.Reachable);

				reachable = AndFlowReturns (reachable, child.Reachable);

				// Ignore unreachable children.
				if (child.IsUnreachable)
					continue;

				returns = AndFlowReturns (returns, child.Returns);
				breaks = AndFlowReturns (breaks, child.Breaks);
					
				// A local variable is initialized after a flow branching if it
				// has been initialized in all its branches which do neither
				// always return or always throw an exception.
				//
				// If a branch may return, but does not always return, then we
				// can treat it like a never-returning branch here: control will
				// only reach the code position after the branching if we did not
				// return here.
				//
				// It's important to distinguish between always and sometimes
				// returning branches here:
				//
				//    1   int a;
				//    2   if (something) {
				//    3      return;
				//    4      a = 5;
				//    5   }
				//    6   Console.WriteLine (a);
				//
				// The if block in lines 3-4 always returns, so we must not look
				// at the initialization of `a' in line 4 - thus it'll still be
				// uninitialized in line 6.
				//
				// On the other hand, the following is allowed:
				//
				//    1   int a;
				//    2   if (something)
				//    3      a = 5;
				//    4   else
				//    5      return;
				//    6   Console.WriteLine (a);
				//
				// Here, `a' is initialized in line 3 and we must not look at
				// line 5 since it always returns.
				// 
				if (!child.AlwaysReturns && !child.AlwaysBreaks)
					MyBitVector.And (ref locals, child.LocalVector);

				// An `out' parameter must be assigned in all branches which do
				// not always throw an exception.
				if ((child.Type != SiblingType.Catch) &&
				    (child.ParameterVector != null) && (child.Breaks != FlowReturns.Exception))
					MyBitVector.And (ref parameters, child.ParameterVector);
			}

			Report.Debug (2, "MERGING CHILDREN DONE", Type, parameters, locals, returns, breaks, reachable,
				      Infinite, MayLeaveLoop, this);

			if (Infinite && !MayLeaveLoop) {
				Report.Debug (1, "INFINITE", returns, breaks, this);

				if (returns == FlowReturns.Never) {
					// We're actually infinite.
					breaks = FlowReturns.Unreachable;
					returns = FlowReturns.Unreachable;
				} else if ((returns == FlowReturns.Sometimes) || (returns == FlowReturns.Always)) {
					// If we're an infinite loop and do not break, the code after
					// the loop can never be reached.  However, if we may return
					// from the loop, then we do always return (or stay in the loop
					// forever).
					returns = FlowReturns.Always;
				}
			}

			if (returns == FlowReturns.Undefined)
				returns = FlowReturns.Never;
			if (breaks == FlowReturns.Undefined)
				breaks = FlowReturns.Never;

			return new MergeResult (parameters, locals, returns, breaks, reachable, MayLeaveLoop);
		}

		protected abstract MergeResult Merge ();

		// <summary>
		//   Merge a child branching.
		// </summary>
		public FlowReturns MergeChild (FlowBranching child)
		{
			MergeResult result = child.Merge ();

			CurrentUsageVector.MergeChild (
				result.Parameters, result.Locals, result.Returns, result.Breaks, result.Reachable);

			if ((child.Type != BranchingType.LoopBlock) && (child.Type != BranchingType.SwitchSection))
				MayLeaveLoop |= child.MayLeaveLoop;

			if (result.Reachable == FlowReturns.Exception)
				return FlowReturns.Exception;
			else
				return result.Returns;
 		}

		// <summary>
		//   Does the toplevel merging.
		// </summary>
		public FlowReturns MergeTopBlock ()
		{
			if ((Type != BranchingType.Block) || (Block == null))
				throw new NotSupportedException ();

			UsageVector vector = new UsageVector (
				SiblingType.Conditional, null, Location, param_map.Length, local_map.Length);

			MergeResult result = Merge ();
			vector.MergeChild (result.Parameters, result.Locals, result.Returns, result.Breaks, result.Reachable);

			if (vector.Reachable != FlowReturns.Exception)
				CheckOutParameters (vector.Parameters, Location);
			else
				return FlowReturns.Exception;

			return result.Returns;
		}

		public virtual bool InTryBlock ()
		{
			if (Parent != null)
				return Parent.InTryBlock ();
			else
				return false;
		}

		public virtual void AddFinallyVector (UsageVector vector)
		{
			if (Parent != null)
				Parent.AddFinallyVector (vector);
			else
				throw new NotSupportedException ();
		}

		public bool IsAssigned (VariableInfo vi)
		{
			return CurrentUsageVector.IsAssigned (vi);
		}

		public bool IsFieldAssigned (VariableInfo vi, string field_name)
		{
			if (CurrentUsageVector.IsAssigned (vi))
				return true;

			return CurrentUsageVector.IsFieldAssigned (vi, field_name);
		}

		public void SetAssigned (VariableInfo vi)
		{
			CurrentUsageVector.SetAssigned (vi);
		}

		public void SetFieldAssigned (VariableInfo vi, string name)
		{
			CurrentUsageVector.SetFieldAssigned (vi, name);
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append (GetType ());
			sb.Append (" (");

			sb.Append (id);
			sb.Append (",");
			sb.Append (Type);
			if (Block != null) {
				sb.Append (" - ");
				sb.Append (Block.ID);
				sb.Append (" - ");
				sb.Append (Block.StartLocation);
			}
			sb.Append (" - ");
			// sb.Append (Siblings.Length);
			// sb.Append (" - ");
			sb.Append (CurrentUsageVector);
			sb.Append (")");
			return sb.ToString ();
		}
	}

	public class FlowBranchingBlock : FlowBranching
	{
		UsageVector current_vector;
		ArrayList siblings = new ArrayList ();

		public FlowBranchingBlock (FlowBranching parent, BranchingType type, SiblingType stype,
					   Block block, Location loc)
			: base (parent, type, stype, block, loc)
		{ }

		public override UsageVector CurrentUsageVector {
			get { return current_vector; }
		}

		protected override void AddSibling (UsageVector sibling)
		{
			siblings.Add (sibling);
			current_vector = sibling;
		}

		public override void Break ()
		{
			if (Type == BranchingType.SwitchSection)
				CurrentUsageVector.NeverReachable ();
			else {
				if (Type == BranchingType.LoopBlock)
					MayLeaveLoop = true;
				CurrentUsageVector.Break ();
			}
		}

		public override void Return ()
		{
			CurrentUsageVector.Return ();
		}

		public override void Goto ()
		{
			CurrentUsageVector.Unreachable ();
		}

		public override void Throw ()
		{
			CurrentUsageVector.Throw ();
		}

		public override void Label (ArrayList origin_vectors)
		{
			CurrentUsageVector.MergeJumpOrigins (origin_vectors);
		}

		protected override MergeResult Merge ()
		{
			MergeResult result = Merge (siblings);
			if (Type == BranchingType.LoopBlock)
				result.MayLeaveLoop = false;
			return result;
		}
	}

	public class FlowBranchingException : FlowBranching
	{
		ArrayList finally_vectors;

		bool has_params;
		UsageVector current_vector;
		UsageVector try_vector;
		ArrayList catch_vectors = new ArrayList ();
		UsageVector finally_vector;

		public FlowBranchingException (FlowBranching parent, BranchingType type, Block block, Location loc)
			: base (parent, type, SiblingType.Try, block, loc)
		{
			finally_vectors = new ArrayList ();
			has_params = current_vector.HasParameters;
		}

		protected override void AddSibling (UsageVector sibling)
		{
			if (sibling.Type == SiblingType.Try) {
				try_vector = sibling;
				catch_vectors.Add (sibling);
			} else if (sibling.Type == SiblingType.Catch)
				catch_vectors.Add (sibling);
			else if (sibling.Type == SiblingType.Finally) {
				// sibling.MergeFinallyOrigins (finally_vectors);
				finally_vector = sibling;
			} else
				throw new InvalidOperationException ();

			current_vector = sibling;
		}

		public override UsageVector CurrentUsageVector {
			get { return current_vector; }
		}

		public override bool InTryBlock ()
		{
			return true;
		}

		public override void AddFinallyVector (UsageVector vector)
		{
			finally_vectors.Add (vector.Clone ());
		}

		public override void Break ()
		{
			CurrentUsageVector.Break ();
		}

		public override void Return ()
		{
			CurrentUsageVector.Return ();
		}

		public override void Goto ()
		{
			CurrentUsageVector.Unreachable ();
		}

		public override void Throw ()
		{
			CurrentUsageVector.Throw ();
		}

		public override void Label (ArrayList origin_vectors)
		{
			CurrentUsageVector.MergeJumpOrigins (origin_vectors);
		}

		protected void MergeFinally (MyBitVector f_params, ref MergeResult result)
		{
			foreach (UsageVector vector in finally_vectors) {
				MyBitVector temp_params = f_params.Clone ();
				temp_params.Or (vector.Parameters);

				CheckOutParameters (temp_params, Location);
			}
		}

		protected override MergeResult Merge ()
		{
			MergeResult result = Merge (catch_vectors);

			if (has_params) {
				if (finally_vector != null) {
					MergeFinally (finally_vector.Parameters, ref result);
					MyBitVector.Or (ref result.Parameters, finally_vector.ParameterVector);
				} else
					MergeFinally (result.Parameters, ref result);
			}

			if (finally_vector != null)
				MyBitVector.Or (ref result.Locals, finally_vector.LocalVector);

			return result;
		}
	}

	// <summary>
	//   This is used by the flow analysis code to keep track of the type of local variables
	//   and variables.
	//
	//   The flow code uses a BitVector to keep track of whether a variable has been assigned
	//   or not.  This is easy for fundamental types (int, char etc.) or reference types since
	//   you can only assign the whole variable as such.
	//
	//   For structs, we also need to keep track of all its fields.  To do this, we allocate one
	//   bit for the struct itself (it's used if you assign/access the whole struct) followed by
	//   one bit for each of its fields.
	//
	//   This class computes this `layout' for each type.
	// </summary>
	public class TypeInfo
	{
		public readonly Type Type;

		// <summary>
		//   Total number of bits a variable of this type consumes in the flow vector.
		// </summary>
		public readonly int TotalLength;

		// <summary>
		//   Number of bits the simple fields of a variable of this type consume
		//   in the flow vector.
		// </summary>
		public readonly int Length;

		// <summary>
		//   This is only used by sub-structs.
		// </summary>
		public readonly int Offset;

		// <summary>
		//   If this is a struct.
		// </summary>
		public readonly bool IsStruct;	     

		// <summary>
		//   If this is a struct, all fields which are structs theirselves.
		// </summary>
		public TypeInfo[] SubStructInfo;

		protected readonly StructInfo struct_info;
		private static Hashtable type_hash = new Hashtable ();

		public static TypeInfo GetTypeInfo (Type type)
		{
			TypeInfo info = (TypeInfo) type_hash [type];
			if (info != null)
				return info;

			info = new TypeInfo (type);
			type_hash.Add (type, info);
			return info;
		}

		public static TypeInfo GetTypeInfo (TypeContainer tc)
		{
			TypeInfo info = (TypeInfo) type_hash [tc.TypeBuilder];
			if (info != null)
				return info;

			info = new TypeInfo (tc);
			type_hash.Add (tc.TypeBuilder, info);
			return info;
		}

		private TypeInfo (Type type)
		{
			this.Type = type;

			struct_info = StructInfo.GetStructInfo (type);
			if (struct_info != null) {
				Length = struct_info.Length;
				TotalLength = struct_info.TotalLength;
				SubStructInfo = struct_info.StructFields;
				IsStruct = true;
			} else {
				Length = 0;
				TotalLength = 1;
				IsStruct = false;
			}
		}

		private TypeInfo (TypeContainer tc)
		{
			this.Type = tc.TypeBuilder;

			struct_info = StructInfo.GetStructInfo (tc);
			if (struct_info != null) {
				Length = struct_info.Length;
				TotalLength = struct_info.TotalLength;
				SubStructInfo = struct_info.StructFields;
				IsStruct = true;
			} else {
				Length = 0;
				TotalLength = 1;
				IsStruct = false;
			}
		}

		protected TypeInfo (StructInfo struct_info, int offset)
		{
			this.struct_info = struct_info;
			this.Offset = offset;
			this.Length = struct_info.Length;
			this.TotalLength = struct_info.TotalLength;
			this.SubStructInfo = struct_info.StructFields;
			this.Type = struct_info.Type;
			this.IsStruct = true;
		}

		public int GetFieldIndex (string name)
		{
			if (struct_info == null)
				return 0;

			return struct_info [name];
		}

		public TypeInfo GetSubStruct (string name)
		{
			if (struct_info == null)
				return null;

			return struct_info.GetStructField (name);
		}

		// <summary>
		//   A struct's constructor must always assign all fields.
		//   This method checks whether it actually does so.
		// </summary>
		public bool IsFullyInitialized (FlowBranching branching, VariableInfo vi, Location loc)
		{
			if (struct_info == null)
				return true;

			bool ok = true;
			for (int i = 0; i < struct_info.Count; i++) {
				FieldInfo field = struct_info.Fields [i];

				if (!branching.IsFieldAssigned (vi, field.Name)) {
					Report.Error (171, loc,
						      "Field `" + TypeManager.CSharpName (Type) +
						      "." + field.Name + "' must be fully initialized " +
						      "before control leaves the constructor");
					ok = false;
				}
			}

			return ok;
		}

		public override string ToString ()
		{
			return String.Format ("TypeInfo ({0}:{1}:{2}:{3})",
					      Type, Offset, Length, TotalLength);
		}

		protected class StructInfo {
			public readonly Type Type;
			public readonly FieldInfo[] Fields;
			public readonly TypeInfo[] StructFields;
			public readonly int Count;
			public readonly int CountPublic;
			public readonly int CountNonPublic;
			public readonly int Length;
			public readonly int TotalLength;
			public readonly bool HasStructFields;

			private static Hashtable field_type_hash = new Hashtable ();
			private Hashtable struct_field_hash;
			private Hashtable field_hash;

			protected bool InTransit = false;

			// Private constructor.  To save memory usage, we only need to create one instance
			// of this class per struct type.
			private StructInfo (Type type)
			{
				this.Type = type;

				field_type_hash.Add (type, this);

				if (type is TypeBuilder) {
					TypeContainer tc = TypeManager.LookupTypeContainer (type);

					ArrayList fields = tc.Fields;

					ArrayList public_fields = new ArrayList ();
					ArrayList non_public_fields = new ArrayList ();

					if (fields != null) {
						foreach (Field field in fields) {
							if ((field.ModFlags & Modifiers.STATIC) != 0)
								continue;
							if ((field.ModFlags & Modifiers.PUBLIC) != 0)
								public_fields.Add (field.FieldBuilder);
							else
								non_public_fields.Add (field.FieldBuilder);
						}
					}

					CountPublic = public_fields.Count;
					CountNonPublic = non_public_fields.Count;
					Count = CountPublic + CountNonPublic;

					Fields = new FieldInfo [Count];
					public_fields.CopyTo (Fields, 0);
					non_public_fields.CopyTo (Fields, CountPublic);
				} else {
					FieldInfo[] public_fields = type.GetFields (
						BindingFlags.Instance|BindingFlags.Public);
					FieldInfo[] non_public_fields = type.GetFields (
						BindingFlags.Instance|BindingFlags.NonPublic);

					CountPublic = public_fields.Length;
					CountNonPublic = non_public_fields.Length;
					Count = CountPublic + CountNonPublic;

					Fields = new FieldInfo [Count];
					public_fields.CopyTo (Fields, 0);
					non_public_fields.CopyTo (Fields, CountPublic);
				}

				struct_field_hash = new Hashtable ();
				field_hash = new Hashtable ();

				Length = 0;
				StructFields = new TypeInfo [Count];
				StructInfo[] sinfo = new StructInfo [Count];

				InTransit = true;

				for (int i = 0; i < Count; i++) {
					FieldInfo field = (FieldInfo) Fields [i];

					sinfo [i] = GetStructInfo (field.FieldType);
					if (sinfo [i] == null)
						field_hash.Add (field.Name, ++Length);
					else if (sinfo [i].InTransit) {
						Report.Error (523, String.Format (
								      "Struct member '{0}.{1}' of type '{2}' causes " +
								      "a cycle in the structure layout",
								      type, field.Name, sinfo [i].Type));
						sinfo [i] = null;
						return;
					}
				}

				InTransit = false;

				TotalLength = Length + 1;
				for (int i = 0; i < Count; i++) {
					FieldInfo field = (FieldInfo) Fields [i];

					if (sinfo [i] == null)
						continue;

					field_hash.Add (field.Name, TotalLength);

					HasStructFields = true;
					StructFields [i] = new TypeInfo (sinfo [i], TotalLength);
					struct_field_hash.Add (field.Name, StructFields [i]);
					TotalLength += sinfo [i].TotalLength;
				}
			}

			public int this [string name] {
				get {
					if (field_hash.Contains (name))
						return (int) field_hash [name];
					else
						return 0;
				}
			}

			public TypeInfo GetStructField (string name)
			{
				return (TypeInfo) struct_field_hash [name];
			}

			public static StructInfo GetStructInfo (Type type)
			{
				if (!TypeManager.IsValueType (type) || TypeManager.IsEnumType (type) ||
				    TypeManager.IsBuiltinType (type))
					return null;

				StructInfo info = (StructInfo) field_type_hash [type];
				if (info != null)
					return info;

				return new StructInfo (type);
			}

			public static StructInfo GetStructInfo (TypeContainer tc)
			{
				StructInfo info = (StructInfo) field_type_hash [tc.TypeBuilder];
				if (info != null)
					return info;

				return new StructInfo (tc.TypeBuilder);
			}
		}
	}

	// <summary>
	//   This is used by the flow analysis code to store information about a single local variable
	//   or parameter.  Depending on the variable's type, we need to allocate one or more elements
	//   in the BitVector - if it's a fundamental or reference type, we just need to know whether
	//   it has been assigned or not, but for structs, we need this information for each of its fields.
	// </summary>
	public class VariableInfo {
		public readonly string Name;
		public readonly TypeInfo TypeInfo;

		// <summary>
		//   The bit offset of this variable in the flow vector.
		// </summary>
		public readonly int Offset;

		// <summary>
		//   The number of bits this variable needs in the flow vector.
		//   The first bit always specifies whether the variable as such has been assigned while
		//   the remaining bits contain this information for each of a struct's fields.
		// </summary>
		public readonly int Length;

		// <summary>
		//   If this is a parameter of local variable.
		// </summary>
		public readonly bool IsParameter;

		public readonly LocalInfo LocalInfo;
		public readonly int ParameterIndex;

		readonly VariableInfo Parent;
		VariableInfo[] sub_info;

		protected VariableInfo (string name, Type type, int offset)
		{
			this.Name = name;
			this.Offset = offset;
			this.TypeInfo = TypeInfo.GetTypeInfo (type);

			Length = TypeInfo.TotalLength;

			Initialize ();
		}

		protected VariableInfo (VariableInfo parent, TypeInfo type)
		{
			this.Name = parent.Name;
			this.TypeInfo = type;
			this.Offset = parent.Offset + type.Offset;
			this.Parent = parent;
			this.Length = type.TotalLength;

			this.IsParameter = parent.IsParameter;
			this.LocalInfo = parent.LocalInfo;
			this.ParameterIndex = parent.ParameterIndex;

			Initialize ();
		}

		protected void Initialize ()
		{
			TypeInfo[] sub_fields = TypeInfo.SubStructInfo;
			if (sub_fields != null) {
				sub_info = new VariableInfo [sub_fields.Length];
				for (int i = 0; i < sub_fields.Length; i++) {
					if (sub_fields [i] != null)
						sub_info [i] = new VariableInfo (this, sub_fields [i]);
				}
			} else
				sub_info = new VariableInfo [0];
		}

		public VariableInfo (LocalInfo local_info, int offset)
			: this (local_info.Name, local_info.VariableType, offset)
		{
			this.LocalInfo = local_info;
			this.IsParameter = false;
		}

		public VariableInfo (string name, Type type, int param_idx, int offset)
			: this (name, type, offset)
		{
			this.ParameterIndex = param_idx;
			this.IsParameter = true;
		}

		public bool IsAssigned (EmitContext ec)
		{
			return !ec.DoFlowAnalysis || ec.CurrentBranching.IsAssigned (this);
		}

		public bool IsAssigned (EmitContext ec, Location loc)
		{
			if (IsAssigned (ec))
				return true;

			Report.Error (165, loc,
				      "Use of unassigned local variable `" + Name + "'");
			ec.CurrentBranching.SetAssigned (this);
			return false;
		}

		public bool IsAssigned (MyBitVector vector)
		{
			if (vector [Offset])
				return true;

			for (VariableInfo parent = Parent; parent != null; parent = parent.Parent)
				if (vector [parent.Offset])
					return true;

			// Return unless this is a struct.
			if (!TypeInfo.IsStruct)
				return false;

			// Ok, so each field must be assigned.
			for (int i = 0; i < TypeInfo.Length; i++) {
				if (!vector [Offset + i + 1])
					return false;
			}

			// Ok, now check all fields which are structs.
			for (int i = 0; i < sub_info.Length; i++) {
				VariableInfo sinfo = sub_info [i];
				if (sinfo == null)
					continue;

				if (!sinfo.IsAssigned (vector))
					return false;
			}

			vector [Offset] = true;
			return true;
		}

		public void SetAssigned (EmitContext ec)
		{
			if (ec.DoFlowAnalysis)
				ec.CurrentBranching.SetAssigned (this);
		}

		public void SetAssigned (MyBitVector vector)
		{
			vector [Offset] = true;
		}

		public bool IsFieldAssigned (EmitContext ec, string name, Location loc)
		{
			if (!ec.DoFlowAnalysis || ec.CurrentBranching.IsFieldAssigned (this, name))
				return true;

			Report.Error (170, loc,
				      "Use of possibly unassigned field `" + name + "'");
			ec.CurrentBranching.SetFieldAssigned (this, name);
			return false;
		}

		public bool IsFieldAssigned (MyBitVector vector, string field_name)
		{
			int field_idx = TypeInfo.GetFieldIndex (field_name);

			if (field_idx == 0)
				return true;

			return vector [Offset + field_idx];
		}

		public void SetFieldAssigned (EmitContext ec, string name)
		{
			if (ec.DoFlowAnalysis)
				ec.CurrentBranching.SetFieldAssigned (this, name);
		}

		public void SetFieldAssigned (MyBitVector vector, string field_name)
		{
			int field_idx = TypeInfo.GetFieldIndex (field_name);

			if (field_idx == 0)
				return;

			vector [Offset + field_idx] = true;
		}

		public VariableInfo GetSubStruct (string name)
		{
			TypeInfo type = TypeInfo.GetSubStruct (name);

			if (type == null)
				return null;

			return new VariableInfo (this, type);
		}

		public override string ToString ()
		{
			return String.Format ("VariableInfo ({0}:{1}:{2}:{3}:{4})",
					      Name, TypeInfo, Offset, Length, IsParameter);
		}
	}

	// <summary>
	//   This is used by the flow code to hold the `layout' of the flow vector for
	//   all locals and all parameters (ie. we create one instance of this class for the
	//   locals and another one for the params).
	// </summary>
	public class VariableMap {
		// <summary>
		//   The number of variables in the map.
		// </summary>
		public readonly int Count;

		// <summary>
		//   Total length of the flow vector for this map.
		// <summary>
		public readonly int Length;

		// <summary>
		//   Type and name of all the variables.
		//   Note that this is null for variables for which we do not need to compute
		//   assignment info.
		// </summary>
		public readonly Type[] VariableTypes;
		public readonly string[] VariableNames;

		VariableInfo[] map;

		public VariableMap (InternalParameters ip)
		{
			Count = ip != null ? ip.Count : 0;
			map = new VariableInfo [Count];
			VariableNames = new string [Count];
			VariableTypes = new Type [Count];
			Length = 0;

			for (int i = 0; i < Count; i++) {
				Parameter.Modifier mod = ip.ParameterModifier (i);

				if ((mod & Parameter.Modifier.OUT) == 0)
					continue;

				VariableNames [i] = ip.ParameterName (i);
				VariableTypes [i] = TypeManager.GetElementType (ip.ParameterType (i));

				map [i] = new VariableInfo (VariableNames [i], VariableTypes [i], i, Length);
				Length += map [i].Length;
			}
		}

		public VariableMap (LocalInfo[] locals)
			: this (null, locals)
		{ }

		public VariableMap (VariableMap parent, LocalInfo[] locals)
		{
			int offset = 0, start = 0;
			if (parent != null) {
				offset = parent.Length;
				start = parent.Count;
			}

			Count = locals.Length + start;
			map = new VariableInfo [Count];
			VariableNames = new string [Count];
			VariableTypes = new Type [Count];
			Length = offset;

			if (parent != null) {
				parent.map.CopyTo (map, 0);
				parent.VariableNames.CopyTo (VariableNames, 0);
				parent.VariableTypes.CopyTo (VariableTypes, 0);
			}

			for (int i = start; i < Count; i++) {
				LocalInfo li = locals [i-start];

				if (li.VariableType == null)
					continue;

				VariableNames [i] = li.Name;
				VariableTypes [i] = li.VariableType;

				map [i] = li.VariableInfo = new VariableInfo (li, Length);
				Length += map [i].Length;
			}
		}

		// <summary>
		//   Returns the VariableInfo for variable @index or null if we don't need to
		//   compute assignment info for this variable.
		// </summary>
		public VariableInfo this [int index] {
			get {
				return map [index];
			}
		}

		public override string ToString ()
		{
			return String.Format ("VariableMap ({0}:{1})", Count, Length);
		}
	}

	// <summary>
	//   This is a special bit vector which can inherit from another bit vector doing a
	//   copy-on-write strategy.  The inherited vector may have a smaller size than the
	//   current one.
	// </summary>
	public class MyBitVector {
		public readonly int Count;
		public readonly MyBitVector InheritsFrom;

		bool is_dirty;
		BitArray vector;

		public MyBitVector (int Count)
			: this (null, Count)
		{ }

		public MyBitVector (MyBitVector InheritsFrom, int Count)
		{
			this.InheritsFrom = InheritsFrom;
			this.Count = Count;
		}

		// <summary>
		//   Checks whether this bit vector has been modified.  After setting this to true,
		//   we won't use the inherited vector anymore, but our own copy of it.
		// </summary>
		public bool IsDirty {
			get {
				return is_dirty;
			}

			set {
				if (!is_dirty)
					initialize_vector ();
			}
		}

		// <summary>
		//   Get/set bit `index' in the bit vector.
		// </summary>
		public bool this [int index]
		{
			get {
				if (index > Count)
					throw new ArgumentOutOfRangeException ();

				// We're doing a "copy-on-write" strategy here; as long
				// as nobody writes to the array, we can use our parent's
				// copy instead of duplicating the vector.

				if (vector != null)
					return vector [index];
				else if (InheritsFrom != null) {
					BitArray inherited = InheritsFrom.Vector;

					if (index < inherited.Count)
						return inherited [index];
					else
						return false;
				} else
					return false;
			}

			set {
				if (index > Count)
					throw new ArgumentOutOfRangeException ();

				// Only copy the vector if we're actually modifying it.

				if (this [index] != value) {
					initialize_vector ();

					vector [index] = value;
				}
			}
		}

		// <summary>
		//   If you explicitly convert the MyBitVector to a BitArray, you will get a deep
		//   copy of the bit vector.
		// </summary>
		public static explicit operator BitArray (MyBitVector vector)
		{
			vector.initialize_vector ();
			return vector.Vector;
		}

		// <summary>
		//   Performs an `or' operation on the bit vector.  The `new_vector' may have a
		//   different size than the current one.
		// </summary>
		public void Or (MyBitVector new_vector)
		{
			BitArray new_array = new_vector.Vector;

			initialize_vector ();

			int upper;
			if (vector.Count < new_array.Count)
				upper = vector.Count;
			else
				upper = new_array.Count;

			for (int i = 0; i < upper; i++)
				vector [i] = vector [i] | new_array [i];
		}

		// <summary>
		//   Perfonrms an `and' operation on the bit vector.  The `new_vector' may have
		//   a different size than the current one.
		// </summary>
		public void And (MyBitVector new_vector)
		{
			BitArray new_array = new_vector.Vector;

			initialize_vector ();

			int lower, upper;
			if (vector.Count < new_array.Count)
				lower = upper = vector.Count;
			else {
				lower = new_array.Count;
				upper = vector.Count;
			}

			for (int i = 0; i < lower; i++)
				vector [i] = vector [i] & new_array [i];

			for (int i = lower; i < upper; i++)
				vector [i] = false;
		}

		public static void And (ref MyBitVector target, MyBitVector vector)
		{
			if (target != null)
				target.And (vector);
			else
				target = vector.Clone ();
		}

		public static void Or (ref MyBitVector target, MyBitVector vector)
		{
			if (target != null)
				target.Or (vector);
			else
				target = vector.Clone ();
		}

		// <summary>
		//   This does a deep copy of the bit vector.
		// </summary>
		public MyBitVector Clone ()
		{
			MyBitVector retval = new MyBitVector (Count);

			retval.Vector = Vector;

			return retval;
		}

		BitArray Vector {
			get {
				if (vector != null)
					return vector;
				else if (!is_dirty && (InheritsFrom != null))
					return InheritsFrom.Vector;

				initialize_vector ();

				return vector;
			}

			set {
				initialize_vector ();

				for (int i = 0; i < System.Math.Min (vector.Count, value.Count); i++)
					vector [i] = value [i];
			}
		}

		void initialize_vector ()
		{
			if (vector != null)
				return;

			vector = new BitArray (Count, false);
			if (InheritsFrom != null)
				Vector = InheritsFrom.Vector;

			is_dirty = true;
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ("MyBitVector (");

			BitArray vector = Vector;
			sb.Append (Count);
			sb.Append (",");
			if (!IsDirty)
				sb.Append ("INHERITED - ");
			for (int i = 0; i < vector.Count; i++) {
				if (i > 0)
					sb.Append (",");
				sb.Append (vector [i]);
			}
			
			sb.Append (")");
			return sb.ToString ();
		}
	}
}
