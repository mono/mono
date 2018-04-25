//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System.Runtime;

    class QueryTreeBuilder
    {
        Diverger diverger;
        Opcode lastOpcode;

        internal QueryTreeBuilder()
        {
        }

        internal Opcode LastOpcode
        {
            get
            {
                return this.lastOpcode;
            }
        }

        internal Opcode Build(Opcode tree, OpcodeBlock newBlock)
        {
            if (null == tree)
            {
                this.lastOpcode = newBlock.Last;
                return newBlock.First;
            }

            this.diverger = new Diverger(tree, newBlock.First);

            if (!this.diverger.Find())
            {
                // The opcodes in newBlock already have equivalents or identical opcodes
                // in the query tree that can do the job
                Fx.Assert(this.diverger.TreePath.Count > 0, "");
                this.lastOpcode = this.diverger.TreePath[this.diverger.TreePath.Count - 1];
                return tree;
            }

            Fx.Assert(this.diverger.TreePath.Count == this.diverger.InsertPath.Count, "");

            // We can reuse opcodes upto this.diverger.TreePath[this.diverger.TreePath.Count - 1]
            // The remainder of the code in newBlock must be executed as is...
            if (null == this.diverger.TreeOpcode)
            {
                // We reached a leaf in the query tree
                // Simply add the remainder of the inserted code to the end of the tree path..
                this.diverger.TreePath[this.diverger.TreePath.Count - 1].Attach(this.diverger.InsertOpcode);
            }
            else
            {
                // Merge in the remaider of the new code block into the query tree
                // The first diverging opcodes follow the last entry in each path
                this.diverger.TreeOpcode.Add(this.diverger.InsertOpcode);
            }
            
            this.lastOpcode = newBlock.Last;
            if (this.diverger.InsertOpcode.IsMultipleResult())
            {
                // The complete new block was merged in, except for the the actual result opcode, which never
                // automatically merges. This means that the new block found all of its opcodes in common with 
                // the tree
                if (OpcodeID.Branch == this.diverger.TreeOpcode.ID)
                {
                    OpcodeList branches = (((BranchOpcode) this.diverger.TreeOpcode).Branches); 
                    for (int i = 0, count = branches.Count; i < count; ++i)
                    {
                        if (branches[i].IsMultipleResult())
                        {
                            this.lastOpcode = branches[i];
                            break;
                        }
                    }
                }
                else if (this.diverger.TreeOpcode.IsMultipleResult())
                {
                    this.lastOpcode = this.diverger.TreeOpcode;
                }
            }

            // Since we'll be diverging, any jumps that preceeded and leapt past the divergence point
            // will have to be branched
            this.FixupJumps();

            return tree;
        }

        void FixupJumps()
        {
            QueryBuffer<Opcode> treePath = this.diverger.TreePath;
            QueryBuffer<Opcode> insertPath = this.diverger.InsertPath;

            for (int i = 0; i < insertPath.Count; ++i)
            {
                if (insertPath[i].TestFlag(OpcodeFlags.Jump))
                {
                    Fx.Assert(treePath[i].ID == insertPath[i].ID, "");
                    JumpOpcode insertJump = (JumpOpcode) insertPath[i];
                    // Opcodes in 'insertPath' have equivalent opcodes in the query tree: i.e. the query tree contains an
                    // an equivalent execution path (upto the point of divergence naturally) that will produce in an identical
                    // result. The remainder of the query tree (anything that lies beyond the point of divergence) represents
                    // a distinct execution path and is grafted onto the tree as a new branch. In fact, we simply break off
                    // the remainder from the query being inserted and graft it onto the query tree.
                    // If there are jumps on the insert path that jump to opcodes NOT in the insert path, then the jumps
                    // will reach opcodes in the new branch we will add(see above). However, because the actual jump opcodes
                    // are shared (used as is from the query tree), the actual jump must also be branched. One jump will
                    // continue to jump to the original opcode and the second new one will jump to an opcode in the grafted branch.
                    if (-1 == insertPath.IndexOf(insertJump.Jump, i + 1))
                    {
                        Fx.Assert(insertJump.Jump.ID == OpcodeID.BlockEnd, "");

                        BlockEndOpcode jumpTo = (BlockEndOpcode) insertJump.Jump;
                        // no longer jumping from insertJump to jumpTo
                        insertJump.RemoveJump(jumpTo);

                        // Instead, jumping from treePath[i] to jumpTo
                        JumpOpcode treeJump = (JumpOpcode) treePath[i];
                        treeJump.AddJump(jumpTo);
                    }
                }
            }
        }

        // Can handle queries being merged into trees but not trees merged into trees.
        // In other words, no branch opcodes in the query being inserted
        internal struct Diverger
        {
            Opcode treeOpcode;
            QueryBuffer<Opcode> treePath;
            QueryBuffer<Opcode> insertPath;
            Opcode insertOpcode;

            internal Diverger(Opcode tree, Opcode insert)
            {
                this.treePath = new QueryBuffer<Opcode>(16);
                this.insertPath = new QueryBuffer<Opcode>(16);
                this.treeOpcode = tree;
                this.insertOpcode = insert;
            }

            internal Opcode InsertOpcode
            {
                get
                {
                    return this.insertOpcode;
                }
            }

            internal QueryBuffer<Opcode> InsertPath
            {
                get
                {
                    return this.insertPath;
                }
            }

            internal Opcode TreeOpcode
            {
                get
                {
                    return this.treeOpcode;
                }
            }

            internal QueryBuffer<Opcode> TreePath
            {
                get
                {
                    return this.treePath;
                }
            }

            // Stops at the last common node on each
            internal bool Find()
            {
                Opcode treeNext = null;
                while (true)
                {
                    if (null == this.treeOpcode && null == this.insertOpcode)
                    {
                        return false; // no diverge. both ran out at the same time
                    }
                    if (null == this.insertOpcode)
                    {
                        return false; // Ran out before tree did. No divergence.
                    }
                    if (null == this.treeOpcode)
                    {
                        return true; // tree ran out before insert. Divergence
                    }
                    if (this.treeOpcode.TestFlag(OpcodeFlags.Branch))
                    {
                        treeNext = this.treeOpcode.Locate(this.insertOpcode);
                        if (null == treeNext)
                        {
                            return true; // divergence
                        }
                        this.treeOpcode = treeNext;
                        treeNext = treeNext.Next;
                    }
                    else
                    {
                        if (!this.treeOpcode.Equals(this.insertOpcode))
                        {
                            return true; // divergence, obviously
                        }
                        treeNext = this.treeOpcode.Next;
                    }
                    // No divergence. Add to paths
                    this.treePath.Add(this.treeOpcode);
                    this.insertPath.Add(this.insertOpcode);
                    this.insertOpcode = this.insertOpcode.Next;
                    this.treeOpcode = treeNext;
                }
            }
        }
    }
}
