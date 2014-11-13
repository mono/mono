//------------------------------------------------------------------------------
// <copyright file="ActionFrame.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Xml.Xsl.XsltOld {
    using Res = System.Xml.Utils.Res;
    using System;
    using System.Xml;
    using System.Xml.XPath;
    using MS.Internal.Xml.XPath;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Xml.Xsl.XsltOld.Debugger;

    internal class ActionFrame : IStackFrame {
        private int               state;         // Action execution state
        private int               counter;       // Counter, for the use of particular action
        private object []         variables;     // Store for template local variable values
        private Hashtable         withParams;
        private Action            action;        // Action currently being executed
        private ActionFrame       container;     // Frame of enclosing container action and index within it
        private int               currentAction;
        private XPathNodeIterator nodeSet;       // Current node set
        private XPathNodeIterator newNodeSet;    // Node set for processing children or other templates

        // Variables to store action data between states:
        private PrefixQName       calulatedName; // Used in ElementAction and AttributeAction
        private string            storedOutput;  // Used in NumberAction, CopyOfAction, ValueOfAction and ProcessingInstructionAction

        internal PrefixQName CalulatedName {
            get { return this.calulatedName; }
            set { this.calulatedName = value; }
        }

        internal string StoredOutput {
            get { return this.storedOutput; }
            set { this.storedOutput = value; }
        }

        internal int State {
            get { return this.state; }
            set { this.state = value; }
        }

        internal int Counter {
            get { return this.counter; }
            set { this.counter = value; }
        }

        internal ActionFrame Container {
            get { return this.container; }
        }

        internal XPathNavigator Node {
            get {
                if (this.nodeSet != null)  {
                    return this.nodeSet.Current;
                }
                return null;
            }
        }

        internal XPathNodeIterator NodeSet {
            get { return this.nodeSet; }
        }

        internal XPathNodeIterator NewNodeSet {
            get { return this.newNodeSet; }
        }

        internal int IncrementCounter() {
            return ++ this.counter;
        }

        [System.Runtime.TargetedPatchingOptOutAttribute("Performance critical to inline across NGen image boundaries")]
        internal void AllocateVariables(int count) {
            if (0 < count) {
                this.variables = new object [count];
            }
            else {
                this.variables = null;
            }
        }

        internal object GetVariable(int index) {
            Debug.Assert(this.variables != null && index < this.variables.Length);
            return this.variables[index];
        }

        internal void SetVariable(int index, object value) {
            Debug.Assert(this.variables != null && index < this.variables.Length);
            this.variables[index] = value;
        }

        internal void SetParameter(XmlQualifiedName name, object value) {
            if (this.withParams == null) {
                this.withParams = new Hashtable();
            }
            Debug.Assert(! this.withParams.Contains(name), "We should check duplicate params at compile time");
            this.withParams[name] = value;
        }

        internal void ResetParams() {
            if (this.withParams != null)
                this.withParams.Clear();
        }

        internal object GetParameter(XmlQualifiedName name) {
            if (this.withParams != null) {
                return this.withParams[name];
            }
            return null;
        }

        internal void InitNodeSet(XPathNodeIterator nodeSet) {
            Debug.Assert(nodeSet != null);
            this.nodeSet = nodeSet;
        }

        internal void InitNewNodeSet(XPathNodeIterator nodeSet) {
            Debug.Assert(nodeSet != null);
            this.newNodeSet = nodeSet;
        }

        internal void SortNewNodeSet(Processor proc, ArrayList sortarray) {
            Debug.Assert(0 < sortarray.Count);
            int numSorts = sortarray.Count;
            XPathSortComparer comparer = new XPathSortComparer(numSorts);
            for (int i = 0; i < numSorts; i++) {
                Sort sort = (Sort) sortarray[i];
                Query expr = proc.GetCompiledQuery(sort.select);
                
                comparer.AddSort(expr, new XPathComparerHelper(sort.order, sort.caseOrder, sort.lang, sort.dataType));
            }
            List<SortKey> results = new List<SortKey>();

            Debug.Assert(proc.ActionStack.Peek() == this, "the trick we are doing with proc.Current will work only if this is topmost frame");

            while (NewNextNode(proc)) {
                XPathNodeIterator savedNodeset = this.nodeSet;
                this.nodeSet = this.newNodeSet;              // trick proc.Current node

                SortKey key = new SortKey(numSorts, /*originalPosition:*/results.Count, this.newNodeSet.Current.Clone());

                for (int j = 0; j < numSorts; j ++) {
                    key[j] = comparer.Expression(j).Evaluate(this.newNodeSet);
                }
                results.Add(key);

                this.nodeSet = savedNodeset;                 // restore proc.Current node
            }
            results.Sort(comparer);
            this.newNodeSet = new XPathSortArrayIterator(results);
        }

        // Finished
        internal void Finished() {
            State = Action.Finished;
        }

        internal void Inherit(ActionFrame parent) {
            Debug.Assert(parent != null);
            this.variables = parent.variables;
        }

        private void Init(Action action, ActionFrame container, XPathNodeIterator nodeSet) {
            this.state         = Action.Initialized;
            this.action        = action;
            this.container     = container;
            this.currentAction = 0;
            this.nodeSet       = nodeSet;
            this.newNodeSet    = null;
        }

        internal void Init(Action action, XPathNodeIterator nodeSet) {
            Init(action, null, nodeSet);
        }

        internal void Init(ActionFrame containerFrame, XPathNodeIterator nodeSet) {
            Init(containerFrame.GetAction(0), containerFrame, nodeSet);
        }

        internal void SetAction(Action action) {
            SetAction(action, Action.Initialized);
        }

        internal void SetAction(Action action, int state) {
            this.action = action;
            this.state  = state;
        }

        private Action GetAction(int actionIndex) {
            Debug.Assert(this.action is ContainerAction);
            return((ContainerAction) this.action).GetAction(actionIndex);
        }

        internal void Exit() {
            Finished();
            this.container = null;
        }

        /*
         * Execute
         *  return values: true - pop, false - nothing
         */
        internal bool Execute(Processor processor) {
            if (this.action == null) {
                return true;
            }

            // Execute the action
            this.action.Execute(processor, this);

            // Process results
            if (State == Action.Finished) {
                // Advanced to next action
                if(this.container != null) {
                    this.currentAction ++;
                    this.action =  this.container.GetAction(this.currentAction);
                    State = Action.Initialized;
                }
                else {
                    this.action = null;
                }
                return this.action == null;
            }

            return false;                       // Do not pop, unless specified otherwise
        }

       internal bool NextNode(Processor proc) {
            bool next = this.nodeSet.MoveNext();
            if (next && proc.Stylesheet.Whitespace) {
                XPathNodeType type = this.nodeSet.Current.NodeType;
                if (type == XPathNodeType.Whitespace) {
                    XPathNavigator nav = this.nodeSet.Current.Clone();
                    bool flag;
                    do {
                        nav.MoveTo(this.nodeSet.Current);
                        nav.MoveToParent();
                        flag = ! proc.Stylesheet.PreserveWhiteSpace(proc, nav) && (next = this.nodeSet.MoveNext());
                        type = this.nodeSet.Current.NodeType;                    
                    }
                    while (flag && (type == XPathNodeType.Whitespace ));
                }
            }
            return next;
        }

        internal bool NewNextNode(Processor proc) {
            bool next = this.newNodeSet.MoveNext();
            if (next && proc.Stylesheet.Whitespace) {
                XPathNodeType type = this.newNodeSet.Current.NodeType;
                if (type == XPathNodeType.Whitespace) {
                    XPathNavigator nav = this.newNodeSet.Current.Clone();
                    bool flag;
                    do {
                        nav.MoveTo(this.newNodeSet.Current);
                        nav.MoveToParent();
                        flag = ! proc.Stylesheet.PreserveWhiteSpace(proc, nav) &&  (next = this.newNodeSet.MoveNext()) ;
                        type = this.newNodeSet.Current.NodeType;
                    }
                    while(flag && (type == XPathNodeType.Whitespace ));
                }                            
            }
            return next;
        }

        // ----------------------- IStackFrame : --------------------
        XPathNavigator    IStackFrame.Instruction { 
            get { 
                if (this.action == null) {
                    return null; // for builtIn action this shoud be null;
                }
                return this.action.GetDbgData(this).StyleSheet; 
            }
        }
        XPathNodeIterator IStackFrame.NodeSet { 
            get { return this.nodeSet.Clone(); }
        }
        // Variables:
        int               IStackFrame.GetVariablesCount() {
            if (this.action == null) {
                return 0;
            }
            return this.action.GetDbgData(this).Variables.Length;
        }
        XPathNavigator    IStackFrame.GetVariable(int varIndex) {
            return this.action.GetDbgData(this).Variables[varIndex].GetDbgData(null).StyleSheet;
        }
        object            IStackFrame.GetVariableValue(int varIndex) { 
            return GetVariable(this.action.GetDbgData(this).Variables[varIndex].VarKey); 
        }

        // special array iterator that iterates over ArrayList of SortKey
        private class XPathSortArrayIterator : XPathArrayIterator {
            public XPathSortArrayIterator(List<SortKey> list) : base(list) { }
            public XPathSortArrayIterator(XPathSortArrayIterator it) : base(it) {}

            public override XPathNodeIterator Clone() {
                return new XPathSortArrayIterator(this);
            }

            public override XPathNavigator Current {
                get {
                    Debug.Assert(index > 0, "MoveNext() wasn't called");
                    return ((SortKey) this.list[this.index - 1]).Node;
                }
            }
        }
    }
}
