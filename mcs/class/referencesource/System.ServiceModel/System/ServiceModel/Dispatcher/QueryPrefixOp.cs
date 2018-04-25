//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System.Collections.Generic;
    using System.Runtime;
    using System.Text;

    internal class TrieSegmentComparer : IComparer<TrieSegment>
    {
        public int Compare(TrieSegment t1, TrieSegment t2)
        {
            return ((int)t1.FirstChar) - ((int)t2.FirstChar);
        }

        public bool Equals(TrieSegment t1, TrieSegment t2)
        {
            return t1.FirstChar == t2.FirstChar;
        }

        public int GetHashCode(TrieSegment t)
        {
            return t.GetHashCode();
        }
    }

    internal class TrieSegmentKeyComparer : IItemComparer<char, TrieSegment>
    {
        public int Compare(char c, TrieSegment t)
        {
            return ((int)c) - ((int)t.FirstChar);
        }
    }

    internal class TrieSegment
    {
        static readonly TrieSegmentKeyComparer SegKeyComparer = new TrieSegmentKeyComparer();
        static readonly TrieSegmentComparer SegComparer = new TrieSegmentComparer();

        SortedBuffer<TrieSegment, TrieSegmentComparer> children;
        QueryBranch data;
        TrieSegment parent; // segment's parent
        char segmentFirstChar; // this segment's first character
        string segmentTail; // this segment's tail
        int segmentLength;

        internal TrieSegment()
            : this(char.MinValue)
        {
        }

        internal TrieSegment(char firstChar)
            : this(firstChar, string.Empty)
        {
        }

        internal TrieSegment(char firstChar, string segmentTail)
        {
            Fx.Assert(null != segmentTail, "");
            this.SetSegment(firstChar, segmentTail);
            this.children = new SortedBuffer<TrieSegment, TrieSegmentComparer>(SegComparer);
        }

        internal TrieSegment(string sourceSegment, int offset, int length)
        {
            Fx.Assert(null != sourceSegment && length > 0, "");
            this.SetSegmentString(sourceSegment, offset, length);
            this.children = new SortedBuffer<TrieSegment, TrieSegmentComparer>(SegComparer);
        }

        internal bool CanMerge
        {
            get
            {
                return (null == this.data && 1 == this.children.Count);
            }
        }

        internal bool CanPrune
        {
            get
            {
                return (null == this.data && 0 == this.children.Count);
            }
        }
#if NO
        internal int ChildCount
        {
            get
            {
                return this.children.Count;
            }
        }
#endif
        internal void CollectXPathFilters(ICollection<MessageFilter> filters)
        {
            if (this.data != null)
            {
                (this.data).Branch.CollectXPathFilters(filters);
            }

            for (int i = 0; i < this.children.Count; ++i)
            {
                this.children[i].CollectXPathFilters(filters);
            }
        }

        internal QueryBranch Data
        {
            get
            {
                return this.data;
            }
            set
            {
                this.data = value;
            }
        }

        internal char FirstChar
        {
            get
            {
                return this.segmentFirstChar;
            }
        }

        internal bool HasChildren
        {
            get
            {
                return (this.children.Count > 0);
            }
        }

        internal int Length
        {
            get
            {
                //return (this.segmentFirstChar == char.MinValue) ? 0 : this.segmentTail.Length + 1;
                return this.segmentLength;
            }
        }
#if NO
        internal string Tail
        {
            get
            {
                return this.segmentTail;
            }
        }
#endif
        internal TrieSegment AddChild(TrieSegment segment)
        {
            Fx.Assert(null != segment, "");

            this.children.Insert(segment);
            segment.parent = this;

            return segment;
        }
#if NO
        internal TrieSegment[] CopyChildren()
        {
            return this.children.ToArray();
        }
#endif
        internal int FindDivergence(string compareString, int offset, int length)
        {
            Fx.Assert(null != compareString && length > 0, "");

            if (compareString[offset] != this.segmentFirstChar)
            {
                return 0;
            }

            --length;
            ++offset;

            int charCount = (length <= this.segmentTail.Length) ? length : this.segmentTail.Length;
            for (int iSegment = 0, iCompare = offset; iSegment < charCount; ++iSegment, ++iCompare)
            {
                if (compareString[iCompare] != this.segmentTail[iSegment])
                {
                    return iSegment + 1;
                }
            }

            if (length < this.segmentTail.Length)
            {
                return length + 1;
            }

            return -1;
        }

        internal TrieSegment GetChild(int index)
        {
            Fx.Assert(this.HasChildren, "");
            return this.children[index];
        }

        /// <summary>
        /// Return the index of the child such that matchString == the string segment stored at that child
        /// i.e. matchString[0] == child.segmentFirstChar and matchString[1 -> length] == child.segmentTail[0 -> length]
        /// </summary>
        internal int GetChildPosition(string matchString, int offset, int length)
        {
            Fx.Assert(null != matchString, "");

            if (this.HasChildren)
            {
                char matchChar = matchString[offset];
                int tailLength = length - 1;
                int tailOffset = offset + 1;
                int index = this.children.IndexOfKey(matchChar, SegKeyComparer);
                if (index >= 0)
                {
                    TrieSegment child = this.children[index];
                    if (tailLength >= child.segmentTail.Length && (0 == child.segmentTail.Length || 0 == string.CompareOrdinal(matchString, tailOffset, child.segmentTail, 0, child.segmentTail.Length)))
                    {
                        return index;
                    }
                }
            }
            return -1;
        }

        /// <summary>
        /// Return the index of the child such that child.segmentFirstChar == ch
        /// </summary>
        internal int GetChildPosition(char ch)
        {
            return this.children.IndexOfKey(ch, SegKeyComparer);
        }

        internal int IndexOf(TrieSegment segment)
        {
            return this.children.IndexOf(segment);
        }

        internal void MergeChild(TrieSegment segment)
        {
            int childIndex = this.IndexOf(segment);
            if (childIndex > -1)
            {
                this.MergeChild(childIndex);
            }
        }

        /// <summary>
        /// If the child node has no data associated with it and but one child of its own, it can be
        /// merged with the child, reducing the path by 1
        /// </summary>
        internal void MergeChild(int childIndex)
        {
            Fx.Assert(this.HasChildren, "");

            TrieSegment child = this.children[childIndex];
            if (child.CanMerge)
            {
                TrieSegment grandchild = child.children[0];

                StringBuilder newTail = new StringBuilder();
                newTail.Append(child.segmentTail);
                newTail.Append(grandchild.segmentFirstChar);
                newTail.Append(grandchild.segmentTail);

                grandchild.SetSegment(child.segmentFirstChar, newTail.ToString());
                grandchild.parent = this;

                this.children.Exchange(child, grandchild);
                child.parent = null;
            }
        }

        internal void Remove()
        {
            if (null != this.parent)
            {
                this.parent.RemoveChild(this);
            }
        }

        /// <summary>
        /// Remove the child == segment, and prune the tree
        /// </summary>
        void RemoveChild(TrieSegment segment)
        {
            Fx.Assert(null != segment, "");
            int childIndex = this.IndexOf(segment);
            if (childIndex >= 0)
            {
                this.RemoveChild(childIndex, true);
            }
        }

        /// <summary>
        /// Remove the child at index childIndex.
        /// If fixupTree is true, traverse back up the tree, removing prunable nodes or merging mergable nodes
        /// as appropriate
        /// </summary>
        internal void RemoveChild(int childIndex, bool fixupTree)
        {
            Fx.Assert(this.HasChildren && childIndex >= 0, "");

            TrieSegment child = this.children[childIndex];

            child.parent = null;
            this.children.RemoveAt(childIndex);

            if (0 == this.children.Count)
            {
                if (fixupTree && this.CanPrune)
                {
                    this.Remove();
                }
            }
            else
            {
                if (fixupTree && this.CanMerge && null != this.parent)
                {
                    this.parent.MergeChild(this);
                }
            }
        }

        void SetSegment(char firstChar, string segmentTail)
        {
            this.segmentFirstChar = firstChar;
            this.segmentTail = segmentTail;
            this.segmentLength = firstChar == char.MinValue ? 0 : 1 + segmentTail.Length;
        }

        void SetSegmentString(string segmentString, int offset, int length)
        {
            this.segmentFirstChar = segmentString[offset];
            if (length > 1)
            {
                this.segmentTail = segmentString.Substring(offset + 1, length - 1);
            }
            else
            {
                this.segmentTail = string.Empty;
            }
            this.segmentLength = length;
        }

        TrieSegment SplitAt(int charIndex)
        {
            Fx.Assert(charIndex > 0, "");

            TrieSegment newSegment;
            if (1 == charIndex)
            {
                newSegment = new TrieSegment(this.segmentFirstChar);
            }
            else
            {
                Fx.Assert(this.segmentTail.Length > 0, "");
                newSegment = new TrieSegment(this.segmentFirstChar, this.segmentTail.Substring(0, charIndex - 1));
            }
            --charIndex;
            this.SetSegmentString(this.segmentTail, charIndex, this.segmentTail.Length - charIndex);
            newSegment.AddChild(this);
            return newSegment;
        }

        internal TrieSegment SplitChild(int childIndex, int charIndex)
        {
            Fx.Assert(this.HasChildren, "");

            TrieSegment child = this.children[childIndex];
            this.children.Remove(child);
            TrieSegment newChild = child.SplitAt(charIndex);
            this.children.Insert(newChild);
            newChild.parent = this;
            return newChild;
        }

        internal void Trim()
        {
            this.children.Trim();
            for (int i = 0; i < this.children.Count; ++i)
            {
                this.children[i].Trim();
            }
        }
    }

    internal struct TrieTraverser
    {
        int length;
        int offset;
        string prefix;
        TrieSegment rootSegment;
        TrieSegment segment;
        int segmentIndex;

        internal TrieTraverser(TrieSegment root, string prefix)
        {
            Fx.Assert(null != root && null != prefix, "");
            this.prefix = prefix;
            this.rootSegment = root;
            this.segment = null;
            this.segmentIndex = -1;
            this.offset = 0;
            this.length = prefix.Length;
        }

        /// <summary>
        /// What length of segment that matched
        /// </summary>
        internal int Length
        {
            get
            {
                return this.length;
            }
        }

        /// <summary>
        /// The offset within the original segment where the matching part of the source string began
        /// </summary>
        internal int Offset
        {
            get
            {
                return this.offset;
            }
        }

        /// <summary>
        /// The traverser is currently at this segment node
        /// </summary>
        internal TrieSegment Segment
        {
            get
            {
                return this.segment;
            }
            set
            {
                Fx.Assert(null != value, "");
                this.segment = value;
            }
        }

        internal int SegmentIndex
        {
            get
            {
                return this.segmentIndex;
            }
        }

        /// <summary>
        /// Traverse the prefix tree
        /// </summary>
        internal bool MoveNext()
        {
            if (null != this.segment)
            {
                int segmentLength = this.segment.Length;
                this.offset += segmentLength;
                this.length -= segmentLength;

                if (this.length > 0)
                {
                    this.segmentIndex = this.segment.GetChildPosition(this.prefix, this.offset, this.length);
                    if (this.segmentIndex > -1)
                    {
                        this.segment = this.segment.GetChild(this.segmentIndex);
                        return true;
                    }
                }
                else
                {
                    this.segmentIndex = -1;
                }
                this.segment = null;
            }
            else if (null != this.rootSegment)
            {
                this.segment = this.rootSegment;
                this.rootSegment = null;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Traverse the prefix tree.. using only the first character of each segment to jump from
        /// segment to segment
        /// </summary>
        internal bool MoveNextByFirstChar()
        {
            if (null != this.segment)
            {
                int segmentLength = this.segment.Length;
                this.offset += segmentLength;
                this.length -= segmentLength;

                if (this.length > 0)
                {
                    this.segmentIndex = this.segment.GetChildPosition(this.prefix[this.offset]);
                    if (this.segmentIndex > -1)
                    {
                        this.segment = this.segment.GetChild(this.segmentIndex);
                        return true;
                    }
                }
                else
                {
                    this.segmentIndex = -1;
                }
                this.segment = null;
            }
            else if (null != this.rootSegment)
            {
                this.segment = this.rootSegment;
                this.rootSegment = null;
                return true;
            }

            return false;
        }
    }


    internal class Trie
    {
        TrieSegment root; // prefix tree root
        bool hasDescendants;

        internal Trie()
        {
            this.hasDescendants = false;
        }

        bool HasDescendants
        {
            get
            {
                //return (null != this.root && this.root.ChildCount > 0);
                return this.hasDescendants;
            }
        }
#if NO
        internal bool IsEmpty
        {
            get
            {
                return (null == this.root || (null == this.root.Data && 0 == this.root.ChildCount));
            }
        }
#endif
        internal TrieSegment this[string prefix]
        {
            get
            {
                return this.Find(prefix);
            }
        }

        /// <summary>
        /// Tree root segment
        /// </summary>
        internal TrieSegment Root
        {
            get
            {
                this.EnsureRoot();
                return this.root;
            }
        }

        internal TrieSegment Add(string newPrefix)
        {
            if (newPrefix.Length <= 0)
            {
                return this.Root;
            }

            this.EnsureRoot();
            TrieTraverser traverser = new TrieTraverser(this.root, newPrefix); // struct
            TrieSegment parent;
            int indexDivergence;
            while (true)
            {
                parent = traverser.Segment;
                if (traverser.MoveNextByFirstChar())
                {
                    // There is a child segment that starts with the same character as the remainder of newPrefix
                    // We have a shared segment
                    // How much does newPrefix share with the current segment? Find the point at which they diverge
                    if (null != parent && -1 != (indexDivergence = traverser.Segment.FindDivergence(newPrefix, traverser.Offset, traverser.Length)))
                    {
                        // Segments diverge at character # 'indexDivergence'. newPrefix will share the segment upto
                        // that character. Beyond that character, we will now have 2 child segments:
                        // - one for the portion of the current segment that diverged
                        // - one for the portion of the new segment that diverged

                        // Split the current segment into a shared part and a child containing the remainder of the segment..
                        traverser.Segment = parent.SplitChild(traverser.SegmentIndex, indexDivergence);
                    }
                }
                else
                {
                    if (traverser.Length <= 0)
                    {
                        // The entire new prefix has been added to the tree
                        break;
                    }
                    // No existing segment to share. Add a new one
                    traverser.Segment = parent.AddChild(new TrieSegment(newPrefix, traverser.Offset, traverser.Length));
                }
            }

            this.hasDescendants = true;

            return parent;
        }

        void EnsureRoot()
        {
            if (null == this.root)
            {
                this.root = new TrieSegment();
            }
        }

        TrieSegment Find(string prefix)
        {
            Fx.Assert(null != prefix, "");

            if (0 == prefix.Length)
            {
                return this.Root;
            }

            if (!this.HasDescendants)
            {
                return null;
            }

            TrieTraverser traverser = new TrieTraverser(this.root, prefix); // struct
            TrieSegment foundSegment = null;
            while (traverser.MoveNext())
            {
                foundSegment = traverser.Segment;
            }
            if (traverser.Length > 0)
            {
                // We haven't used up the entire prefix in this search. Clearly, we didn't find the matching node
                return null;
            }
            return foundSegment;
        }

        void PruneRoot()
        {
            if (null != this.root && this.root.CanPrune)
            {
                this.root = null;
            }
        }

        internal void Remove(string segment)
        {
            Fx.Assert(null != segment, "");

            TrieSegment trieSegment = this[segment];
            if (null == trieSegment)
            {
                return;
            }

            if (trieSegment.HasChildren)
            {
                trieSegment.Data = null;
                return;
            }

            if (trieSegment == this.root)
            {
                // Remove the entire tree!
                this.root = null;
                this.hasDescendants = false;
                return;
            }

            trieSegment.Remove();
            this.PruneRoot();
        }

        internal void Trim()
        {
            this.root.Trim();
        }
    }

    internal class StringPrefixOpcode : LiteralRelationOpcode
    {
        string literal;

        internal StringPrefixOpcode(string literal)
            : base(OpcodeID.StringPrefix)
        {
            Fx.Assert(null != literal, "");
            this.literal = literal;
        }
#if NO
        internal override ValueDataType DataType
        {
            get
            {
                return ValueDataType.String;
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

        internal override void Add(Opcode op)
        {
            StringPrefixOpcode prefixOp = op as StringPrefixOpcode;
            if (null == prefixOp)
            {
                base.Add(op);
                return;
            }

            Fx.Assert(null != this.prev, "");

            StringPrefixBranchOpcode branch = new StringPrefixBranchOpcode();
            this.prev.Replace(this, branch);
            branch.Add(this);
            branch.Add(prefixOp);
        }

        internal override bool Equals(Opcode op)
        {
            if (base.Equals(op))
            {
                StringPrefixOpcode prefixOp = (StringPrefixOpcode)op;
                return (prefixOp.literal == this.literal);
            }

            return false;
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            StackFrame arg = context.TopArg;
            if (1 == arg.Count)
            {
                Fx.Assert(context.Values[arg.basePtr].IsType(ValueDataType.String), "");

                string target = context.Values[arg.basePtr].String;
                context.Values[arg.basePtr].Boolean = target.StartsWith(this.literal, StringComparison.Ordinal);
            }
            else
            {
                for (int i = arg.basePtr; i <= arg.endPtr; ++i)
                {
                    Fx.Assert(context.Values[i].IsType(ValueDataType.String), "");
                    string target = context.Values[i].String;
                    context.Values[i].Boolean = target.StartsWith(this.literal, StringComparison.Ordinal);
                }
            }

            return this.next;
        }
    }

    internal class TrieBranchIndex : QueryBranchIndex
    {
        int count;
        Trie trie;

        internal TrieBranchIndex()
        {
            this.count = 0;
            this.trie = new Trie();
        }

        internal override int Count
        {
            get
            {
                return this.count;
            }
        }

        internal override QueryBranch this[object key]
        {
            get
            {
                TrieSegment segment = this.trie[(string)key];
                if (null != segment)
                {
                    return segment.Data;
                }
                return null;
            }
            set
            {
                Fx.Assert(null != key, "");
                TrieSegment segment = this.trie.Add((string)key);
                Fx.Assert(null != segment, "");
                this.count++;
                segment.Data = value;
            }
        }

        internal override void CollectXPathFilters(ICollection<MessageFilter> filters)
        {
            this.trie.Root.CollectXPathFilters(filters);
        }

#if NO
        internal override IEnumerator GetEnumerator()
        {
            //return new TrieBreadthFirstEnum(this.trie);
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException("TODO"));
        }
#endif

        void Match(int valIndex, string segment, QueryBranchResultSet results)
        {
            TrieTraverser traverser = new TrieTraverser(this.trie.Root, segment);
            while (traverser.MoveNext())
            {
                object segmentData = traverser.Segment.Data;
                if (null != segmentData)
                {
                    results.Add((QueryBranch)segmentData, valIndex);
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
                    this.Match(valIndex, sequence.Items[i].StringValue(), results);
                }
            }
            else
            {
                this.Match(valIndex, val.String, results);
            }
        }

        internal override void Remove(object key)
        {
            this.trie.Remove((string)key);
            this.count--;
        }

        internal override void Trim()
        {
            this.trie.Trim();
        }
    }

    internal class StringPrefixBranchOpcode : QueryConditionalBranchOpcode
    {
        internal StringPrefixBranchOpcode()
            : base(OpcodeID.StringPrefixBranch, new TrieBranchIndex())
        {
        }
    }
}
