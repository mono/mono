//------------------------------------------------------------------------------
// <copyright file="TemplateAction.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Xml.Xsl.XsltOld {
    using Res = System.Xml.Utils.Res;
    using System;
    using System.Diagnostics;
    using System.Collections;
    using System.Xml;
    using System.Xml.XPath;
    using MS.Internal.Xml.XPath;
    using System.Globalization;

    internal class TemplateAction : TemplateBaseAction {
        private int               matchKey = Compiler.InvalidQueryKey;
        private XmlQualifiedName  name;
        private double            priority = double.NaN;
        private XmlQualifiedName  mode;
        private int               templateId;
        private bool              replaceNSAliasesDone;

        internal int MatchKey {
            get { return this.matchKey; }
        }

        internal XmlQualifiedName Name {
            get { return this.name; }
        }

        internal double Priority {
            get { return this.priority; }
        }

        internal XmlQualifiedName Mode {
            get { return this.mode; }
        }

        internal int TemplateId {
            get { return this.templateId; }
            set {
                Debug.Assert(this.templateId == 0);
                this.templateId = value;
            }
        }

        internal override void Compile(Compiler compiler) {
            CompileAttributes(compiler);
            if (this.matchKey == Compiler.InvalidQueryKey) {
                if (this.name == null) {
                    throw XsltException.Create(Res.Xslt_TemplateNoAttrib);
                }
                if (this.mode != null ) {
                    throw XsltException.Create(Res.Xslt_InvalidModeAttribute);
                }
            }
            compiler.BeginTemplate(this);

            if (compiler.Recurse()) {
                CompileParameters(compiler);
                CompileTemplate(compiler);

                compiler.ToParent();
            }

            compiler.EndTemplate();
            AnalyzePriority(compiler);
        }

        internal virtual void CompileSingle(Compiler compiler) {
            this.matchKey = compiler.AddQuery("/", /*allowVars:*/false, /*allowKey:*/true, /*pattern*/true);
            this.priority   = Compiler.RootPriority;

            CompileOnceTemplate(compiler);
        }

        internal override bool CompileAttribute(Compiler compiler) {
            string name   = compiler.Input.LocalName;
            string value  = compiler.Input.Value;

            if (Ref.Equal(name, compiler.Atoms.Match)) {
                Debug.Assert(this.matchKey == Compiler.InvalidQueryKey);
                this.matchKey = compiler.AddQuery(value, /*allowVars:*/false, /*allowKey:*/true, /*pattern*/true);
            }
            else if (Ref.Equal(name, compiler.Atoms.Name)) {
                Debug.Assert(this.name == null);
                this.name = compiler.CreateXPathQName(value);
            }
            else if (Ref.Equal(name, compiler.Atoms.Priority)) {
                Debug.Assert(Double.IsNaN(this.priority));
                this.priority = XmlConvert.ToXPathDouble(value);
                if (double.IsNaN(this.priority) && ! compiler.ForwardCompatibility) {
                    throw XsltException.Create(Res.Xslt_InvalidAttrValue, "priority", value);
                }
            }
            else if (Ref.Equal(name, compiler.Atoms.Mode)) {
                Debug.Assert(this.mode == null);
                if (compiler.AllowBuiltInMode && value == "*") {
                    this.mode = Compiler.BuiltInMode;
                }
                else {
                    this.mode = compiler.CreateXPathQName(value);
                }
            }
            else {
                return false;
            }

            return true;
        }

        private void AnalyzePriority(Compiler compiler) {
            NavigatorInput input = compiler.Input;

            if (!Double.IsNaN(this.priority) || this.matchKey == Compiler.InvalidQueryKey) {
                return;
            }
            // Split Unions:
            TheQuery theQuery = (TheQuery)compiler.QueryStore[this.MatchKey];
            CompiledXpathExpr expr = (CompiledXpathExpr)theQuery.CompiledQuery;
            Query query = expr.QueryTree;
            UnionExpr union;
            while ((union = query as UnionExpr) != null) {
                Debug.Assert(!(union.qy2 is UnionExpr), "only qy1 can be union");
                TemplateAction copy = this.CloneWithoutName();
                compiler.QueryStore.Add(new TheQuery(
                    new CompiledXpathExpr(union.qy2, expr.Expression, false),
                    theQuery._ScopeManager
                ));
                copy.matchKey = compiler.QueryStore.Count - 1;
                copy.priority = union.qy2.XsltDefaultPriority;
                compiler.AddTemplate(copy);

                query = union.qy1;
            }
            if (expr.QueryTree != query) {
                // query was splitted and we need create new TheQuery for this template
                compiler.QueryStore[this.MatchKey] = new TheQuery(
                    new CompiledXpathExpr(query, expr.Expression, false),
                    theQuery._ScopeManager
                );
            }
            this.priority = query.XsltDefaultPriority;
        }
        
        protected void CompileParameters(Compiler compiler) {
            NavigatorInput input = compiler.Input;
            do {
                switch(input.NodeType) {
                case XPathNodeType.Element:
                    if (Ref.Equal(input.NamespaceURI, input.Atoms.UriXsl) &&
                        Ref.Equal(input.LocalName, input.Atoms.Param)) {
                        compiler.PushNamespaceScope();
                        AddAction(compiler.CreateVariableAction(VariableType.LocalParameter));
                        compiler.PopScope();
                        continue;
                    }
                    else {
                        return;
                    }
                case XPathNodeType.Text:
                    return;
                case XPathNodeType.SignificantWhitespace:
                    this.AddEvent(compiler.CreateTextEvent());
                    continue;
                default :
                    continue;
                }
            }
            while (input.Advance());
        }

        //
        // Priority calculation plus template splitting
        //

        private TemplateAction CloneWithoutName() {
            TemplateAction clone    = new TemplateAction(); {
                clone.containedActions = this.containedActions;
                clone.mode             = this.mode;
                clone.variableCount    = this.variableCount;
                clone.replaceNSAliasesDone = true; // We shouldn't replace NS in clones.
            }
            return clone;
        }

        internal override void ReplaceNamespaceAlias(Compiler compiler) {
            // if template has both name and match it will be twice caled by stylesheet to replace NS aliases.
            if (! replaceNSAliasesDone) {
                base.ReplaceNamespaceAlias(compiler);
                replaceNSAliasesDone = true;
            }
        }
        //
        // Execution
        //

        internal override void Execute(Processor processor, ActionFrame frame) {
            Debug.Assert(processor != null && frame != null);

            switch (frame.State) {
            case Initialized:
                if (this.variableCount > 0) {
                    frame.AllocateVariables(this.variableCount);
                }
                if (this.containedActions != null &&  this.containedActions.Count > 0) {
                    processor.PushActionFrame(frame);
                    frame.State = ProcessingChildren;
                }
                else {
                    frame.Finished();
                }
                break;                              // Allow children to run
            case ProcessingChildren:
                frame.Finished();
                break;
            default:
                Debug.Fail("Invalid Container action execution state");
    		    break;
            }
        }
    }
}
