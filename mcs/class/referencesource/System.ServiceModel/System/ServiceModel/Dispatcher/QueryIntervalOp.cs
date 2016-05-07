//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime;

    // Based on the Paper: The IBS Tree: A Datastructure for finding all intervals that overlap a point.
    // by Eric Hanson & Moez Chaabouni, Nov, 1994

    internal enum IntervalOp : byte
    {
        LessThan,
        LessThanEquals
    }

    internal class Interval
    {
        QueryBranch branch;
        double lowerBound;
        IntervalOp lowerOp;
        double upperBound;
        IntervalOp upperOp;

        // Converts expressions of the form:
        // x < 5 => -infinity <= x and x < 5
        // x <= 5 => -infinity <= x and x <= 5
        // x > 5 => 5 < x <= infinity
        // x >= 5 => 5 <= x <= infinity
        //
        // The variable is always to the left
        internal Interval(double literal, RelationOperator op)
        {
            this.lowerBound = double.MinValue;
            this.upperBound = double.MaxValue;
            this.lowerOp = IntervalOp.LessThanEquals;
            this.upperOp = IntervalOp.LessThanEquals;

            Fx.Assert(RelationOperator.Eq != op && RelationOperator.Ne != op, "");
            switch (op)
            {
                case RelationOperator.Lt:
                    this.upperBound = literal;
                    this.upperOp = IntervalOp.LessThan;
                    break;
                case RelationOperator.Le:
                    this.upperBound = literal;
                    break;
                case RelationOperator.Gt:
                    this.lowerBound = literal;
                    this.lowerOp = IntervalOp.LessThan;
                    break;
                case RelationOperator.Ge:
                    this.lowerBound = literal;
                    break;
            }
        }

#if NO
        internal Interval(double lowerBound, IntervalOp lowerOp, double upperBound, IntervalOp upperOp)
        {
            Fx.Assert(lowerBound <= upperBound, "");

            this.lowerBound = lowerBound;
            this.upperBound = upperBound;
            if (this.lowerBound == this.upperBound)
            {
                this.lowerOp = IntervalOp.LessThanEquals;
                this.upperOp = IntervalOp.LessThanEquals;
            }
            else
            {
                this.lowerOp = lowerOp;
                this.upperOp = upperOp;
            }
        }
#endif
        internal QueryBranch Branch
        {
            get
            {
                return this.branch;
            }
            set
            {
                this.branch = value;
            }
        }

        internal double LowerBound
        {
            get
            {
                return this.lowerBound;
            }
        }

        internal IntervalOp LowerOp
        {
            get
            {
                return this.lowerOp;
            }
        }

        internal double UpperBound
        {
            get
            {
                return this.upperBound;
            }
        }

        internal IntervalOp UpperOp
        {
            get
            {
                return this.upperOp;
            }
        }

#if NO
        internal bool Equals(Interval interval)
        {
            Fx.Assert(null != interval, "");
            return this.Equals(interval.lowerBound, interval.lowerOp, interval.upperBound, interval.upperOp);
        }
#endif
        internal bool Equals(double lowerBound, IntervalOp lowerOp, double upperBound, IntervalOp upperOp)
        {
            return (this.lowerBound == lowerBound && this.lowerOp == lowerOp && this.upperBound == upperBound && this.upperOp == upperOp);
        }

        internal bool HasMatchingEndPoint(double endpoint)
        {
            return (this.lowerBound == endpoint || this.upperBound == endpoint);
        }
    }

    /// <summary>
    /// IntervalCollection
    /// All Contains/Find operations are currently linear, since they are required only during
    /// Inserts/Removes from the Interval Tree, which is not anticipated to be performance critical.
    /// </summary>
    internal class IntervalCollection : ArrayList
    {
        internal IntervalCollection()
            : base(1)
        {
        }

        internal bool HasIntervals
        {
            get
            {
                return (this.Count > 0);
            }
        }

        internal new Interval this[int index]
        {
            get
            {
                return (Interval)base[index];
            }
        }

        internal int Add(Interval interval)
        {
            Fx.Assert(null != interval, "");
            this.Capacity = this.Count + 1;
            return base.Add(interval);
        }

        internal int AddUnique(Interval interval)
        {
            Fx.Assert(null != interval, "");
            int index = this.IndexOf(interval);
            if (-1 == index)
            {
                return this.Add(interval);
            }
            return index;
        }

        internal IntervalCollection GetIntervalsWithEndPoint(double endPoint)
        {
            IntervalCollection matches = new IntervalCollection();

            int count = this.Count;
            for (int i = 0; i < count; ++i)
            {
                Interval interval = this[i];
                if (interval.HasMatchingEndPoint(endPoint))
                {
                    matches.Add(interval);
                }
            }

            return matches;
        }

        internal int IndexOf(Interval interval)
        {
            Fx.Assert(null != interval, "");
            return base.IndexOf(interval);
        }

        internal int IndexOf(double endPoint)
        {
            int count = this.Count;
            for (int i = 0; i < count; ++i)
            {
                Interval interval = this[i];
                if (interval.HasMatchingEndPoint(endPoint))
                {
                    return i;
                }
            }

            return -1;
        }

        internal int IndexOf(double lowerBound, IntervalOp lowerOp, double upperBound, IntervalOp upperOp)
        {
            int count = this.Count;
            for (int i = 0; i < count; ++i)
            {
                if (this[i].Equals(lowerBound, lowerOp, upperBound, upperOp))
                {
                    return i;
                }
            }
            return -1;
        }

        internal void Remove(Interval interval)
        {
            Fx.Assert(null != interval, "");
            base.Remove(interval);
            this.TrimToSize();
        }

        internal void Trim()
        {
            this.TrimToSize();
        }
    }

    internal class IntervalBoundary
    {
        IntervalCollection eqSlot;
        IntervalCollection gtSlot;
        IntervalBoundary left;
        IntervalCollection ltSlot;
        IntervalBoundary parent;
        IntervalBoundary right;
        double val;

        internal IntervalBoundary(double val, IntervalBoundary parent)
        {
            this.val = val;
            this.parent = parent;
        }

        internal IntervalCollection EqSlot
        {
            get
            {
                return this.eqSlot;
            }
        }

        internal IntervalCollection GtSlot
        {
            get
            {
                return this.gtSlot;
            }
        }

        internal IntervalBoundary Left
        {
            get
            {
                return this.left;
            }
            set
            {
                this.left = value;
            }
        }

        internal IntervalCollection LtSlot
        {
            get
            {
                return this.ltSlot;
            }
        }

        internal IntervalBoundary Parent
        {
            get
            {
                return this.parent;
            }
            set
            {
                this.parent = value;
            }
        }

        internal IntervalBoundary Right
        {
            get
            {
                return this.right;
            }
            set
            {
                this.right = value;
            }
        }

        internal double Value
        {
            get
            {
                return this.val;
            }
            set
            {
                this.val = value;
            }
        }

        internal void AddToEqSlot(Interval interval)
        {
            Fx.Assert(null != interval, "");
            this.AddToSlot(ref this.eqSlot, interval);
        }

        internal void AddToGtSlot(Interval interval)
        {
            Fx.Assert(null != interval, "");
            this.AddToSlot(ref this.gtSlot, interval);
        }

        internal void AddToLtSlot(Interval interval)
        {
            Fx.Assert(null != interval, "");
            this.AddToSlot(ref this.ltSlot, interval);
        }

        void AddToSlot(ref IntervalCollection slot, Interval interval)
        {
            if (null == slot)
            {
                slot = new IntervalCollection();
            }
            slot.AddUnique(interval);
        }

        internal IntervalBoundary EnsureLeft(double val)
        {
            if (null == this.left)
            {
                this.left = new IntervalBoundary(val, this);
            }

            return this.left;
        }

        internal IntervalBoundary EnsureRight(double val)
        {
            if (null == this.right)
            {
                this.right = new IntervalBoundary(val, this);
            }

            return this.right;
        }
#if NO
        internal Interval GetInterval(double lowerBound, IntervalOp lowerOp, double upperBound, IntervalOp upperOp)
        {
            Interval interval;
            if (
                null != (interval = this.GetIntervalFromSlot(this.eqSlot, lowerBound, lowerOp, upperBound, upperOp))
                || null != (interval = this.GetIntervalFromSlot(this.ltSlot, lowerBound, lowerOp, upperBound, upperOp))
                || null != (interval = this.GetIntervalFromSlot(this.gtSlot, lowerBound, lowerOp, upperBound, upperOp))
                )
            {
                return interval;
            }

            return null;
        }

        internal Interval GetIntervalByData(object data)
        {
            Interval interval;
            if (
                null != (interval = this.GetIntervalFromSlot(this.eqSlot, data))
                || null != (interval = this.GetIntervalFromSlot(this.ltSlot, data))
                || null != (interval = this.GetIntervalFromSlot(this.gtSlot, data))
                )
            {
                return interval;
            }

            return null;
        }

        Interval GetIntervalFromSlot(IntervalCollection slot, object data)
        {
            int index;
            if (null != slot && -1 != (index = slot.IndexOf(data)))
            {
                return slot[index];
            }
            return null;
        }

        Interval GetIntervalFromSlot(IntervalCollection slot, double lowerBound, IntervalOp lowerOp, double upperBound, IntervalOp upperOp)
        {
            int index;
            if (null != slot && -1 != (index = slot.IndexOf(lowerBound, lowerOp, upperBound, upperOp)))
            {
                return slot[index];
            }
            return null;
        }
#endif
        internal void RemoveFromEqSlot(Interval interval)
        {
            Fx.Assert(null != interval, "");
            this.RemoveFromSlot(ref this.eqSlot, interval);
        }

        internal void RemoveFromGtSlot(Interval interval)
        {
            Fx.Assert(null != interval, "");
            this.RemoveFromSlot(ref this.gtSlot, interval);
        }

        internal void RemoveFromLtSlot(Interval interval)
        {
            Fx.Assert(null != interval, "");
            this.RemoveFromSlot(ref this.ltSlot, interval);
        }

        void RemoveFromSlot(ref IntervalCollection slot, Interval interval)
        {
            if (null != slot)
            {
                slot.Remove(interval);
                if (!slot.HasIntervals)
                {
                    slot = null;
                }
            }
        }

        internal void Trim()
        {
            if (this.eqSlot != null)
            {
                this.eqSlot.Trim();
            }

            if (this.gtSlot != null)
            {
                this.gtSlot.Trim();
            }

            if (this.ltSlot != null)
            {
                this.ltSlot.Trim();
            }

            if (this.left != null)
            {
                this.left.Trim();
            }

            if (this.right != null)
            {
                this.right.Trim();
            }
        }
    }

    internal struct IntervalTreeTraverser
    {
        IntervalBoundary currentNode;
        IntervalBoundary nextNode;
        IntervalCollection slot;
        double val;

        internal IntervalTreeTraverser(double val, IntervalBoundary root)
        {
            Fx.Assert(null != root, "");
            this.currentNode = null;
            this.slot = null;
            this.nextNode = root;
            this.val = val;
        }

        internal IntervalCollection Slot
        {
            get
            {
                return this.slot;
            }
        }

        internal bool MoveNext()
        {
            while (null != this.nextNode)
            {
                this.currentNode = this.nextNode;
                double currentVal = this.currentNode.Value;
                if (val < currentVal)
                {
                    this.slot = this.currentNode.LtSlot;
                    this.nextNode = this.currentNode.Left;
                }
                else if (val > currentVal)
                {
                    this.slot = this.currentNode.GtSlot;
                    this.nextNode = this.currentNode.Right;
                }
                else
                {
                    this.slot = this.currentNode.EqSlot;
                    this.nextNode = null;
                }
                if (null != this.slot)
                {
                    return true;
                }
            }

            return false;
        }
    }

    internal class IntervalTree
    {
        IntervalCollection intervals;
        IntervalBoundary root;

        internal IntervalTree()
        {
        }

        internal int Count
        {
            get
            {
                return (null != this.intervals) ? this.intervals.Count : 0;
            }
        }

        internal IntervalCollection Intervals
        {
            get
            {
                if (null == this.intervals)
                {
                    return new IntervalCollection();
                }
                return this.intervals;
            }
        }

#if NO
        internal bool IsEmpty
        {
            get
            {
                return (this.root == null);
            }
        }
#endif
        internal IntervalBoundary Root
        {
            get
            {
                return this.root;
            }
        }

        internal void Add(Interval interval)
        {
            Fx.Assert(null != interval, "");

            this.AddIntervalToTree(interval);
            this.EnsureIntervals();
            this.intervals.Add(interval);
        }

        void AddIntervalToTree(Interval interval)
        {
            this.EditLeft(interval, true);
            this.EditRight(interval, true);
        }

#if NO
        internal bool Contains(double lowerBound, IntervalOp lowerOp, double upperBound, IntervalOp upperOp)
        {
            if (null != this.intervals)
            {
                return (this.intervals.IndexOf(lowerBound, lowerOp, upperBound, upperOp) >= 0);
            }

            return false;
        }
#endif
        void EditLeft(Interval interval, bool add)
        {
            if (add)
            {
                this.EnsureRoot(interval.LowerBound);
            }

            IntervalBoundary root = this.root;
            IntervalBoundary leftAncestor = null;

            while (true)
            {
                double rootVal = root.Value;

                if (rootVal < interval.LowerBound)
                {
                    // root is outside the interval range because it is < the lower bound
                    root = add ? root.EnsureRight(interval.LowerBound) : root.Right;
                    continue;
                }

                // rootVal is >= to interval.lowerBound
                //
                // All values in thee subtree at 'root' are < leftAncestor.Value
                // All values to the left of this node cannot be in the interval because they are < the interval's
                // lower bound.
                // Values to the right of this node lie in range (root.Value to leftAncestor.Value)
                // Thus, the entire right subtree of root will be inside the range if the interval.upperBound
                // is >= leftAncestor.Value
                if (null != leftAncestor && leftAncestor.Value <= interval.UpperBound)
                {
                    if (add)
                    {
                        root.AddToGtSlot(interval);
                    }
                    else
                    {
                        root.RemoveFromGtSlot(interval);
                    }
                }

                if (rootVal > interval.LowerBound)
                { // This node itself lies in the range if it is also < the upper bound
                    if (rootVal < interval.UpperBound)
                    {
                        if (add)
                        {
                            root.AddToEqSlot(interval);
                        }
                        else
                        {
                            root.RemoveFromEqSlot(interval);
                        }
                    }
                    leftAncestor = root;
                    root = add ? root.EnsureLeft(interval.LowerBound) : root.Left;
                    continue;
                }

                // lowerBound == rootVal. We're done.
                if (IntervalOp.LessThanEquals == interval.LowerOp)
                {
                    // If the range is inclusive of the lower bound (>=), then since this node == lowerBound,
                    // it must be in the range.
                    if (add)
                    {
                        root.AddToEqSlot(interval);
                    }
                    else
                    {
                        root.RemoveFromEqSlot(interval);
                    }
                }
                break;
            }
        }

        void EditRight(Interval interval, bool add)
        {
            if (add)
            {
                this.EnsureRoot(interval.UpperBound);
            }

            IntervalBoundary root = this.root;
            IntervalBoundary rightAncestor = null;

            while (true)
            {
                double rootVal = root.Value;

                if (rootVal > interval.UpperBound)
                {
                    // root is outside the interval range because it is > the upper bound
                    root = add ? root.EnsureLeft(interval.UpperBound) : root.Left;
                    continue;
                }

                // rootVal is <= to interval.UpperBound
                //
                // All values in the subtree at 'root' are > leftAncestor.Value
                // All values to the right of this node cannot be in the interval because they are > the interval's
                // upper bound.
                // Values to the left of this node lie in range (rightAncestor.Value to root.Value)
                // Thus, the entire left subtree of root will be inside the range if the interval.lowerBound
                // is <= rightAncestor.Value
                if (null != rightAncestor && rightAncestor.Value >= interval.LowerBound)
                {
                    if (add)
                    {
                        root.AddToLtSlot(interval);
                    }
                    else
                    {
                        root.RemoveFromLtSlot(interval);
                    }
                }

                if (rootVal < interval.UpperBound)
                {
                    // This node itself lies in the range if it is also > the lower bound
                    if (rootVal > interval.LowerBound)
                    {
                        if (add)
                        {
                            root.AddToEqSlot(interval);
                        }
                        else
                        {
                            root.RemoveFromEqSlot(interval);
                        }
                    }
                    rightAncestor = root;
                    root = add ? root.EnsureRight(interval.UpperBound) : root.Right;
                    continue;
                }

                // upperBound == rootVal. We're done.
                // If upperBound == lowerBound, we already inserted this when doing AddLeft
                if (IntervalOp.LessThanEquals == interval.UpperOp)
                {
                    // If the range is inclusive of the upper bound, then since this node == upperBound,
                    // it must be in the range.
                    if (add)
                    {
                        root.AddToEqSlot(interval);
                    }
                    else
                    {
                        root.RemoveFromEqSlot(interval);
                    }
                }
                break;
            }
        }

        void EnsureIntervals()
        {
            if (null == this.intervals)
            {
                this.intervals = new IntervalCollection();
            }
        }

        void EnsureRoot(double val)
        {
            if (null == this.root)
            {
                this.root = new IntervalBoundary(val, null);
            }
        }

        internal IntervalBoundary FindBoundaryNode(double val)
        {
            return this.FindBoundaryNode(root, val);
        }

        internal IntervalBoundary FindBoundaryNode(IntervalBoundary root, double val)
        {
            IntervalBoundary boundary = null;
            if (null != root)
            {
                if (root.Value == val)
                {
                    boundary = root;
                }
                else
                {
                    if (null == (boundary = this.FindBoundaryNode(root.Left, val)))
                    {
                        boundary = this.FindBoundaryNode(root.Right, val);
                    }
                }
            }
            return boundary;
        }

        internal Interval FindInterval(Interval interval)
        {
            return this.FindInterval(interval.LowerBound, interval.LowerOp, interval.UpperBound, interval.UpperOp);
        }

        internal Interval FindInterval(double lowerBound, IntervalOp lowerOp, double upperBound, IntervalOp upperOp)
        {
            if (null != this.intervals)
            {
                int index;
                if (-1 != (index = this.intervals.IndexOf(lowerBound, lowerOp, upperBound, upperOp)))
                {
                    return this.intervals[index];
                }
            }
            return null;
        }

        /// <summary>
        /// An interval has been removed. Prune the tree appropriately
        /// </summary>
        /// <param name="intervalRemoved">interval that was removed</param>
        void PruneTree(Interval intervalRemoved)
        {
            // Delete endpoints if no other intervals have them
            int index;
            if (-1 == (index = this.intervals.IndexOf(intervalRemoved.LowerBound)))
            {
                this.RemoveBoundary(this.FindBoundaryNode(intervalRemoved.LowerBound));
            }
            if (intervalRemoved.LowerBound != intervalRemoved.UpperBound && -1 == (index = this.intervals.IndexOf(intervalRemoved.UpperBound)))
            {
                this.RemoveBoundary(this.FindBoundaryNode(intervalRemoved.UpperBound));
            }
        }

        internal void Remove(Interval interval)
        {
            Fx.Assert(null != interval, "");
            Fx.Assert(null != this.intervals, "");

            // First, delete all occurences of interval in the tree. Note: we do a reference equals
            this.RemoveIntervalFromTree(interval);
            // Remove interval from interval collection
            this.intervals.Remove(interval);
            // It may be possible to prune the tree.. this will do the necessary, if required
            this.PruneTree(interval);
        }

        void RemoveBoundary(IntervalBoundary boundary)
        {
            IntervalCollection replacementIntervals = null;
            int replacementCount = 0;

            if (null != boundary.Left && null != boundary.Right)
            {
                // Neither left/right are null. Typical binary tree node removal - replace the removed node
                // with the symmetric order predecessor
                IntervalBoundary replacement = boundary.Left;
                while (null != replacement.Right)
                {
                    replacement = replacement.Right;
                }
                // Find all intervals with endpoint y in the tree
                replacementIntervals = this.intervals.GetIntervalsWithEndPoint(replacement.Value);

                // Remove the intervals from the tree
                replacementCount = replacementIntervals.Count;
                for (int i = 0; i < replacementCount; ++i)
                {
                    this.RemoveIntervalFromTree(replacementIntervals[i]);
                }

                double val = boundary.Value;
                boundary.Value = replacement.Value;
                replacement.Value = val;
                boundary = replacement;
            }

            if (null != boundary.Left)
            {
                this.Replace(boundary, boundary.Left);
            }
            else
            {
                this.Replace(boundary, boundary.Right);
            }

            // Discard the node
            boundary.Parent = null;
            boundary.Left = null;
            boundary.Right = null;

            // Reinstall Intervals
            for (int i = 0; i < replacementCount; ++i)
            {
                this.AddIntervalToTree(replacementIntervals[i]);
            }
        }

        void RemoveIntervalFromTree(Interval interval)
        {
            this.EditLeft(interval, false);
            this.EditRight(interval, false);
        }

        void Replace(IntervalBoundary replace, IntervalBoundary with)
        {
            IntervalBoundary parent = replace.Parent;
            if (null != parent)
            {
                if (replace == parent.Left)
                {
                    parent.Left = with;
                }
                else if (replace == parent.Right)
                {
                    parent.Right = with;
                }
            }
            else
            {
                // Replacing root
                this.root = with;
            }
            if (null != with)
            {
                with.Parent = parent;
            }
        }

        internal void Trim()
        {
            this.intervals.Trim();
            this.root.Trim();
        }
    }

    internal class NumberRelationOpcode : LiteralRelationOpcode
    {
        double literal;
        RelationOperator op;

        internal NumberRelationOpcode(double literal, RelationOperator op)
            : this(OpcodeID.NumberRelation, literal, op)
        {
        }

        protected NumberRelationOpcode(OpcodeID id, double literal, RelationOperator op)
            : base(id)
        {
            this.literal = literal;
            this.op = op;
        }
#if NO
        internal override ValueDataType DataType
        {
            get
            {
                return ValueDataType.Double;
            }
        }
#endif

        internal override object Literal
        {
            get
            {
                return this.literal;
            }
        }
#if NO
        internal RelationOperator Op
        {
            get
            {
                return this.op;
            }
        }
#endif
        internal override bool Equals(Opcode opcode)
        {
            if (base.Equals(opcode))
            {
                NumberRelationOpcode numOp = (NumberRelationOpcode)opcode;
                return (numOp.op == this.op && numOp.literal == this.literal);
            }

            return false;
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            Value[] values = context.Values;
            StackFrame arg = context.TopArg;

            for (int i = arg.basePtr; i <= arg.endPtr; ++i)
            {
                values[i].Update(context, values[i].CompareTo(this.literal, op));
            }
            return this.next;
        }

        internal Interval ToInterval()
        {
            return new Interval(this.literal, this.op);
        }
    }

    internal class NumberIntervalOpcode : NumberRelationOpcode
    {
        Interval interval;

        internal NumberIntervalOpcode(double literal, RelationOperator op)
            : base(OpcodeID.NumberInterval, literal, op)
        {
        }

        internal override object Literal
        {
            get
            {
                if (null == this.interval)
                {
                    this.interval = this.ToInterval();
                }

                return this.interval;
            }
        }

        internal override void Add(Opcode op)
        {
            NumberIntervalOpcode intervalOp = op as NumberIntervalOpcode;
            if (null == intervalOp)
            {
                base.Add(op);
                return;
            }

            Fx.Assert(null != this.prev, "");

            NumberIntervalBranchOpcode branch = new NumberIntervalBranchOpcode();
            this.prev.Replace(this, branch);
            branch.Add(this);
            branch.Add(intervalOp);
        }
    }

    internal class IntervalBranchIndex : QueryBranchIndex
    {
        IntervalTree intervalTree;

        internal IntervalBranchIndex()
        {
            this.intervalTree = new IntervalTree();
        }

        internal override int Count
        {
            get
            {
                return this.intervalTree.Count;
            }
        }

        internal override QueryBranch this[object key]
        {
            get
            {
                Interval interval = this.intervalTree.FindInterval((Interval)key);
                if (null != interval)
                {
                    return interval.Branch;
                }
                return null;
            }
            set
            {
                Interval interval = (Interval)key;
                interval.Branch = value;
                this.intervalTree.Add(interval);
            }
        }

        internal override void CollectXPathFilters(ICollection<MessageFilter> filters)
        {
            for (int i = 0; i < this.intervalTree.Intervals.Count; ++i)
            {
                this.intervalTree.Intervals[i].Branch.Branch.CollectXPathFilters(filters);
            }
        }

#if NO

        internal override IEnumerator GetEnumerator()
        {
            return this.intervalTree.Intervals.GetEnumerator();
        }
        
#endif

        void Match(int valIndex, double point, QueryBranchResultSet results)
        {
            IntervalTreeTraverser traverser = new IntervalTreeTraverser(point, this.intervalTree.Root);
            while (traverser.MoveNext())
            {
                IntervalCollection matches = traverser.Slot;
                for (int i = 0, count = matches.Count; i < count; ++i)
                {
                    QueryBranch branch = matches[i].Branch;
                    if (null != branch)
                    {
                        results.Add(branch, valIndex);
                    }
                }
            }
        }

        internal override void Match(int valIndex, ref Value val, QueryBranchResultSet results)
        {
            if (ValueDataType.Sequence == val.Type)
            {
                NodeSequence sequence = val.Sequence;
                for (int i = 0; i < sequence.Count; ++i)
                {
                    this.Match(valIndex, sequence.Items[i].NumberValue(), results);
                }
            }
            else
            {
                this.Match(valIndex, val.ToDouble(), results);
            }
        }

        internal override void Remove(object key)
        {
            this.intervalTree.Remove((Interval)key);
        }

        internal override void Trim()
        {
            this.intervalTree.Trim();
        }
    }

    internal class NumberIntervalBranchOpcode : QueryConditionalBranchOpcode
    {
        internal NumberIntervalBranchOpcode()
            : base(OpcodeID.NumberIntervalBranch, new IntervalBranchIndex())
        {
        }

        internal override LiteralRelationOpcode ValidateOpcode(Opcode opcode)
        {
            NumberIntervalOpcode numOp = opcode as NumberIntervalOpcode;
            if (null != numOp)
            {
                return numOp;
            }

            return null;
        }

    }
}
