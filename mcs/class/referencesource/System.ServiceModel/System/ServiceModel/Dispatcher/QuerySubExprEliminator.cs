//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Xml.XPath;

    internal class SubExpr
    {
        internal int var;
        internal int refCount; // opcodes
        internal bool useSpecial;
        Opcode ops;

        SubExpr parent;
        protected List<SubExpr> children;

        internal SubExpr(SubExpr parent, Opcode ops, int var)
        {
            this.children = new List<SubExpr>(2);
            this.var = var;
            this.parent = parent;

            this.useSpecial = false;
            if (parent != null)
            {
                this.ops = new InternalSubExprOpcode(parent);
                this.ops.Attach(ops);
                this.useSpecial = parent is SubExprHeader && ((SelectOpcode)ops).Criteria.Axis.Type == QueryAxisType.Child;
            }
            else
            {
                this.ops = ops;
            }
        }

        internal Opcode FirstOp
        {
            get
            {
                if (this.parent == null)
                {
                    return this.ops;
                }
                else
                {
                    return this.ops.Next;
                }
            }
        }

        internal int Variable
        {
            get
            {
                return this.var;
            }
        }

        internal SubExprOpcode Add(Opcode opseq, SubExprEliminator elim)
        {
            Opcode start = this.FirstOp;
            Opcode ops = opseq;
            while (start != null && ops != null && start.Equals(ops))
            {
                start = start.Next;
                ops = ops.Next;
            }

            if (ops == null)
            {
                if (start == null)
                {
                    return new SubExprOpcode(this);
                }
                else
                {
                    SubExpr e = this.BranchAt(start, elim);
                    return new SubExprOpcode(e);
                }
            }
            else
            {
                if (start == null)
                {
                    ops.DetachFromParent();
                    for (int i = 0; i < this.children.Count; ++i)
                    {
                        if (this.children[i].FirstOp.Equals(ops))
                        {
                            return this.children[i].Add(ops, elim);
                        }
                    }

                    SubExpr e = new SubExpr(this, ops, elim.NewVarID());
                    this.AddChild(e);
                    return new SubExprOpcode(e);
                }
                else
                {
                    SubExpr e = this.BranchAt(start, elim);
                    ops.DetachFromParent();
                    SubExpr ee = new SubExpr(e, ops, elim.NewVarID());
                    e.AddChild(ee);
                    return new SubExprOpcode(ee);
                }
            }
        }

        internal virtual void AddChild(SubExpr expr)
        {
            this.children.Add(expr);
        }

        SubExpr BranchAt(Opcode op, SubExprEliminator elim)
        {
            Opcode firstOp = this.FirstOp;
            if (this.parent != null)
            {
                this.parent.RemoveChild(this);
            }
            else
            {
                elim.Exprs.Remove(this);
            }
            firstOp.DetachFromParent();
            op.DetachFromParent();

            SubExpr e = new SubExpr(this.parent, firstOp, elim.NewVarID());
            if (this.parent != null)
            {
                this.parent.AddChild(e);
            }
            else
            {
                elim.Exprs.Add(e);
            }
            e.AddChild(this);
            this.parent = e;
            this.ops = new InternalSubExprOpcode(e);
            this.ops.Attach(op);
            return e;
        }

        internal void CleanUp(SubExprEliminator elim)
        {
            if (this.refCount == 0)
            {
                if (this.children.Count == 0)
                {
                    if (this.parent == null)
                    {
                        elim.Exprs.Remove(this);
                    }
                    else
                    {
                        this.parent.RemoveChild(this);
                        this.parent.CleanUp(elim);
                    }
                }
                else if (this.children.Count == 1)
                {
                    SubExpr child = this.children[0];

                    Opcode op = child.FirstOp;
                    op.DetachFromParent();
                    Opcode op2 = this.ops;
                    while (op2.Next != null)
                    {
                        op2 = op2.Next;
                    }
                    op2.Attach(op);
                    child.ops = this.ops;

                    if (this.parent == null)
                    {
                        elim.Exprs.Remove(this);
                        elim.Exprs.Add(child);
                        child.parent = null;
                    }
                    else
                    {
                        this.parent.RemoveChild(this);
                        this.parent.AddChild(child);
                        child.parent = this.parent;
                    }
                }
            }
        }

        internal void DecRef(SubExprEliminator elim)
        {
            this.refCount--;
            CleanUp(elim);
        }

        internal void Eval(ProcessingContext context)
        {
            int count = 0, marker = context.Processor.CounterMarker;

            Opcode op = this.ops;
            if (this.useSpecial)
            {
                op.EvalSpecial(context);
                context.LoadVariable(this.var);
                //context.Processor.CounterMarker = marker;
                return;
            }

            while (op != null)
            {
                op = op.Eval(context);
            }

            count = context.Processor.ElapsedCount(marker);
            //context.Processor.CounterMarker = marker;
            context.SaveVariable(this.var, count);
        }

        internal virtual void EvalSpecial(ProcessingContext context)
        {
            this.Eval(context);
        }

        internal void IncRef()
        {
            this.refCount++;
        }

        internal virtual void RemoveChild(SubExpr expr)
        {
            this.children.Remove(expr);
        }

        internal void Renumber(SubExprEliminator elim)
        {
            this.var = elim.NewVarID();
            for (int i = 0; i < this.children.Count; ++i)
            {
                this.children[i].Renumber(elim);
            }
        }

        internal void Trim()
        {
            this.children.Capacity = this.children.Count;
            this.ops.Trim();
            for (int i = 0; i < this.children.Count; ++i)
            {
                this.children[i].Trim();
            }
        }

#if DEBUG_FILTER
        internal void Write(TextWriter outStream)
        {
            outStream.WriteLine("=======================");
            outStream.WriteLine("= SubExpr #" + this.var.ToString() + " (" + this.refCount.ToString() + ")");
            outStream.WriteLine("=======================");

            for(Opcode o = this.ops; o != null; o = o.Next)
            {
                outStream.WriteLine(o.ToString());
            }
            outStream.WriteLine("");

            for(int i = 0; i < this.children.Count; ++i)
            {
                this.children[i].Write(outStream);
            }
        }
#endif
    }

    internal class SubExprHeader : SubExpr
    {
        // WS, [....], Can probably combine these
        // WS, [....], Make this data structure less ugly (if possible)
        Dictionary<string, Dictionary<string, List<SubExpr>>> nameLookup;
        Dictionary<SubExpr, MyInt> indexLookup;

        internal SubExprHeader(Opcode ops, int var)
            : base(null, ops, var)
        {
            this.nameLookup = new Dictionary<string, Dictionary<string, List<SubExpr>>>();
            this.indexLookup = new Dictionary<SubExpr, MyInt>();
            this.IncRef(); // Prevent cleanup
        }

        internal override void AddChild(SubExpr expr)
        {
            base.AddChild(expr);
            RebuildIndex();

            if (expr.useSpecial)
            {
                NodeQName qname = ((SelectOpcode)(expr.FirstOp)).Criteria.QName;
                string ns = qname.Namespace;
                Dictionary<string, List<SubExpr>> nextLookup;
                if (!this.nameLookup.TryGetValue(ns, out nextLookup))
                {
                    nextLookup = new Dictionary<string, List<SubExpr>>();
                    this.nameLookup.Add(ns, nextLookup);
                }

                string name = qname.Name;
                List<SubExpr> exprs = new List<SubExpr>();
                if (!nextLookup.TryGetValue(name, out exprs))
                {
                    exprs = new List<SubExpr>();
                    nextLookup.Add(name, exprs);
                }

                exprs.Add(expr);
            }
        }

        internal override void EvalSpecial(ProcessingContext context)
        {
            int marker = context.Processor.CounterMarker;

            if (!context.LoadVariable(this.var))
            {
                XPathMessageContext.HeaderFun.InvokeInternal(context, 0);
                context.SaveVariable(this.var, context.Processor.ElapsedCount(marker));
            }

            // WS, [....], see if we can put this array in the processor to save
            //             an allocation.  Perhaps we can use the variables slot we're going to fill
            NodeSequence[] childSequences = new NodeSequence[this.children.Count];
            NodeSequence seq = context.Sequences[context.TopSequenceArg.basePtr].Sequence;
            for (int i = 0; i < this.children.Count; ++i)
            {
                childSequences[i] = context.CreateSequence();
                childSequences[i].StartNodeset();
            }

            // Perform the index
            SeekableXPathNavigator nav = seq[0].GetNavigator();
            if (nav.MoveToFirstChild())
            {
                do
                {
                    if (nav.NodeType == XPathNodeType.Element)
                    {
                        List<SubExpr> lst;
                        string name = nav.LocalName;
                        string ns = nav.NamespaceURI;
                        Dictionary<string, List<SubExpr>> nextLookup;
                        if (this.nameLookup.TryGetValue(ns, out nextLookup))
                        {
                            if (nextLookup.TryGetValue(name, out lst))
                            {
                                for (int i = 0; i < lst.Count; ++i)
                                {
                                    childSequences[this.indexLookup[lst[i]].i].Add(nav);
                                }
                            }

                            if (nextLookup.TryGetValue(QueryDataModel.Wildcard, out lst))
                            {
                                for (int i = 0; i < lst.Count; ++i)
                                {
                                    childSequences[this.indexLookup[lst[i]].i].Add(nav);
                                }
                            }
                        }

                        if (this.nameLookup.TryGetValue(QueryDataModel.Wildcard, out nextLookup))
                        {
                            if (nextLookup.TryGetValue(QueryDataModel.Wildcard, out lst))
                            {
                                for (int i = 0; i < lst.Count; ++i)
                                {
                                    childSequences[this.indexLookup[lst[i]].i].Add(nav);
                                }
                            }
                        }
                    }
                } while (nav.MoveToNext());
            }

            int secondMarker = context.Processor.CounterMarker;
            for (int i = 0; i < this.children.Count; ++i)
            {
                if (this.children[i].useSpecial)
                {
                    childSequences[i].StopNodeset();
                    context.Processor.CounterMarker = secondMarker;
                    context.PushSequenceFrame();
                    context.PushSequence(childSequences[i]);
                    Opcode op = this.children[i].FirstOp.Next;
                    while (op != null)
                    {
                        op = op.Eval(context);
                    }
                    context.SaveVariable(this.children[i].var, context.Processor.ElapsedCount(marker));
                    context.PopSequenceFrame();
                }
                else
                {
                    context.ReleaseSequence(childSequences[i]);
                    //context.SetVariable(this.children[i].Variable, null, 0);
                }
            }

            context.Processor.CounterMarker = marker;
        }

        internal void RebuildIndex()
        {
            this.indexLookup.Clear();
            for (int i = 0; i < this.children.Count; ++i)
            {
                this.indexLookup.Add(this.children[i], new MyInt(i));
            }
        }

        internal override void RemoveChild(SubExpr expr)
        {
            base.RemoveChild(expr);
            RebuildIndex();

            if (expr.useSpecial)
            {
                NodeQName qname = ((SelectOpcode)(expr.FirstOp)).Criteria.QName;
                string ns = qname.Namespace;
                Dictionary<string, List<SubExpr>> nextLookup;
                if (this.nameLookup.TryGetValue(ns, out nextLookup))
                {
                    string name = qname.Name;
                    List<SubExpr> exprs;
                    if (nextLookup.TryGetValue(name, out exprs))
                    {
                        exprs.Remove(expr);
                        if (exprs.Count == 0)
                        {
                            nextLookup.Remove(name);
                        }
                    }

                    if (nextLookup.Count == 0)
                    {
                        this.nameLookup.Remove(ns);
                    }
                }
            }
        }

        internal class MyInt
        {
            internal int i;

            internal MyInt(int i)
            {
                this.i = i;
            }
        }
    }

    internal class SubExprEliminator
    {
        List<SubExpr> exprList;
        int nextVar;
        Dictionary<object, List<SubExpr>> removalMapping;

        internal SubExprEliminator()
        {
            this.removalMapping = new Dictionary<object, List<SubExpr>>();
            this.exprList = new List<SubExpr>();

            Opcode op = new XPathMessageFunctionCallOpcode(XPathMessageContext.HeaderFun, 0);
            SubExprHeader header = new SubExprHeader(op, 0);
            this.exprList.Add(header);
            this.nextVar = 1;
        }

        internal List<SubExpr> Exprs
        {
            get
            {
                return this.exprList;
            }
        }

        internal int VariableCount
        {
            get
            {
                return this.nextVar;
            }
        }

        internal Opcode Add(object item, Opcode ops)
        {
            List<SubExpr> exprs = new List<SubExpr>();
            this.removalMapping.Add(item, exprs);

            while (ops.Next != null)
            {
                ops = ops.Next;
            }

            Opcode res = ops;
            while (ops != null)
            {
                if (IsExprStarter(ops))
                {
                    Opcode start = ops;
                    Opcode p = ops.Prev;
                    ops.DetachFromParent();

                    ops = ops.Next;
                    while (ops.ID == OpcodeID.Select)
                    {
                        ops = ops.Next;
                    }
                    ops.DetachFromParent();

                    SubExpr e = null;
                    for (int i = 0; i < this.exprList.Count; ++i)
                    {
                        if (this.exprList[i].FirstOp.Equals(start))
                        {
                            e = this.exprList[i];
                            break;
                        }
                    }

                    SubExprOpcode o;
                    if (e == null)
                    {
                        e = new SubExpr(null, start, NewVarID());
                        this.exprList.Add(e);
                        o = new SubExprOpcode(e);
                    }
                    else
                    {
                        o = e.Add(start, this);
                    }

                    o.Expr.IncRef();
                    exprs.Add(o.Expr);
                    o.Attach(ops);
                    ops = o;

                    if (p != null)
                    {
                        p.Attach(ops);
                    }
                }

                res = ops;
                ops = ops.Prev;
            }

            return res;
        }

        internal static bool IsExprStarter(Opcode op)
        {
            if (op.ID == OpcodeID.SelectRoot)
            {
                return true;
            }

            if (op.ID == OpcodeID.XsltInternalFunction)
            {
                XPathMessageFunctionCallOpcode fop = (XPathMessageFunctionCallOpcode)op;
                if (fop.ReturnType == XPathResultType.NodeSet && fop.ArgCount == 0)
                {
                    return true;
                }
            }

            return false;
        }

        internal int NewVarID()
        {
            return nextVar++;
        }

        internal void Remove(object item)
        {
            List<SubExpr> exprs;
            if (this.removalMapping.TryGetValue(item, out exprs))
            {
                for (int i = 0; i < exprs.Count; ++i)
                {
                    exprs[i].DecRef(this);
                }

                this.removalMapping.Remove(item);
                Renumber();
            }
        }

        void Renumber()
        {
            this.nextVar = 0;
            for (int i = 0; i < this.exprList.Count; ++i)
            {
                this.exprList[i].Renumber(this);
            }
        }

        internal void Trim()
        {
            this.exprList.Capacity = this.exprList.Count;
            for (int i = 0; i < this.exprList.Count; ++i)
            {
                this.exprList[i].Trim();
            }
        }

#if DEBUG_FILTER
        internal void Write(TextWriter outStream)
        {
            for(int i = 0; i < this.exprList.Count; ++i)
            {
                this.exprList[i].Write(outStream);
            }
        }
#endif
    }

    internal class SubExprOpcode : Opcode
    {
        protected SubExpr expr;

        internal SubExprOpcode(SubExpr expr)
            : base(OpcodeID.SubExpr)
        {
            this.expr = expr;
        }

        internal SubExpr Expr
        {
            get
            {
                return expr;
            }
        }

        internal override bool Equals(Opcode op)
        {
            if (base.Equals(op))
            {
                SubExprOpcode sop = op as SubExprOpcode;
                if (sop != null)
                {
                    return this.expr == sop.expr;
                }
            }
            return false;
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            if (!context.LoadVariable(this.expr.Variable))
            {
                context.PushSequenceFrame();
                NodeSequence seq = context.CreateSequence();
                seq.Add(context.Processor.ContextNode);
                context.PushSequence(seq);

                int marker = context.Processor.CounterMarker;
                try
                {
                    this.expr.Eval(context);
                }
                catch (XPathNavigatorException e)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(e.Process(this));
                }
                catch (NavigatorInvalidBodyAccessException e)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(e.Process(this));
                }
                context.Processor.CounterMarker = marker;
                context.PopSequenceFrame();
                context.PopSequenceFrame();

                context.LoadVariable(this.expr.Variable);
            }
            return this.next;
        }

        internal override Opcode EvalSpecial(ProcessingContext context)
        {
            try
            {
                this.expr.EvalSpecial(context);
            }
            catch (XPathNavigatorException e)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(e.Process(this));
            }
            catch (NavigatorInvalidBodyAccessException e)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(e.Process(this));
            }
            return this.next;
        }

#if DEBUG_FILTER
        public override string ToString()
        {
            return string.Format("{0} #{1}", base.ToString(), this.expr.Variable.ToString());
        }
#endif
    }

    internal class InternalSubExprOpcode : SubExprOpcode
    {
        internal InternalSubExprOpcode(SubExpr expr)
            : base(expr)
        {
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            if (!context.LoadVariable(this.expr.Variable))
            {
                this.expr.Eval(context);
            }
            return this.next;
        }

        internal override Opcode EvalSpecial(ProcessingContext context)
        {
            this.expr.EvalSpecial(context);
            return this.next;
        }
    }
}
