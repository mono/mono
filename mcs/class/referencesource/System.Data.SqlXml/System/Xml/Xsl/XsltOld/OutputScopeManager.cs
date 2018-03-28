//------------------------------------------------------------------------------
// <copyright file="OutputScopeManager.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Xml.Xsl.XsltOld {
    using Res = System.Xml.Utils.Res;
    using System;
    using System.Globalization;
    using System.Diagnostics;
    using System.Xml;

    internal class OutputScopeManager {
        private const int STACK_INCREMENT = 10;

        private HWStack         elementScopesStack;
        private string          defaultNS;
        private OutKeywords     atoms;
        private XmlNameTable    nameTable;
        private int             prefixIndex;

        internal string DefaultNamespace {
            get { return this.defaultNS; }
        }

        internal OutputScope CurrentElementScope {
            get {
                Debug.Assert(this.elementScopesStack.Peek() != null); // We adding rootElementScope to garantee this
                return (OutputScope) this.elementScopesStack.Peek();
            }
        }

        internal XmlSpace XmlSpace {
            get { return CurrentElementScope.Space; }
        }

        internal string XmlLang {
            get { return CurrentElementScope.Lang; }
        }

        internal OutputScopeManager(XmlNameTable nameTable, OutKeywords atoms) {
            Debug.Assert(nameTable != null);
            Debug.Assert(atoms     != null);

            this.elementScopesStack = new HWStack(STACK_INCREMENT);
            this.nameTable          = nameTable;
            this.atoms              = atoms;
            this.defaultNS          = this.atoms.Empty;

            // We always adding rootElementScope to garantee that CurrentElementScope != null
            // This context is active between PI and first element for example
            OutputScope rootElementScope = (OutputScope) this.elementScopesStack.Push();
            if(rootElementScope == null) {
                rootElementScope = new OutputScope();
                this.elementScopesStack.AddToTop(rootElementScope);
            }
            rootElementScope.Init(string.Empty, string.Empty, string.Empty, /*space:*/XmlSpace.None, /*lang:*/string.Empty, /*mixed:*/false);
        }

        internal void PushNamespace(string prefix, string nspace) {
            Debug.Assert(prefix != null);
            Debug.Assert(nspace != null);
            CurrentElementScope.AddNamespace(prefix, nspace, this.defaultNS);

            if (prefix == null || prefix.Length == 0) {
                this.defaultNS = nspace;
            }
        }

        internal void PushScope(string name, string nspace, string prefix) {
            Debug.Assert(name != null);
            Debug.Assert(nspace != null);
            Debug.Assert(prefix != null);
            OutputScope parentScope  = CurrentElementScope;
            OutputScope elementScope = (OutputScope) this.elementScopesStack.Push();

            if (elementScope == null) {
                elementScope = new OutputScope();
                this.elementScopesStack.AddToTop(elementScope);
            }

            Debug.Assert(elementScope != null);
            elementScope.Init(name, nspace, prefix, parentScope.Space, parentScope.Lang, parentScope.Mixed);
        }

        internal void PopScope() {
            OutputScope elementScope = (OutputScope) this.elementScopesStack.Pop();

            Debug.Assert(elementScope != null); // We adding rootElementScope to garantee this

            for (NamespaceDecl scope = elementScope.Scopes; scope != null; scope = scope.Next) {
                this.defaultNS = scope.PrevDefaultNsUri;
            }
        }

        internal string ResolveNamespace(string prefix) {
            bool thisScope;
            return ResolveNamespace(prefix, out thisScope);
        }

        internal string ResolveNamespace(string prefix, out bool thisScope) {
            Debug.Assert(prefix != null);
            thisScope = true;

            if (prefix == null || prefix.Length == 0) {
                return this.defaultNS;
            }
            else {
                if (Ref.Equal(prefix, this.atoms.Xml)) {
                    return this.atoms.XmlNamespace;
                }
                else if (Ref.Equal(prefix, this.atoms.Xmlns)) {
                    return this.atoms.XmlnsNamespace;
                }

                for (int i = this.elementScopesStack.Length - 1; i >= 0; i --) {
                    Debug.Assert(this.elementScopesStack[i] is OutputScope);
                    OutputScope elementScope = (OutputScope) this.elementScopesStack[i];

                    string nspace = elementScope.ResolveAtom(prefix);
                    if (nspace != null) {
                        thisScope = (i == this.elementScopesStack.Length - 1);
                        return nspace;
                    }
                }
            }

            return null;
        }

        internal bool FindPrefix(string nspace, out string prefix) {
            Debug.Assert(nspace != null);
            for (int i = this.elementScopesStack.Length - 1; 0 <= i; i--) {
                Debug.Assert(this.elementScopesStack[i] is OutputScope);

                OutputScope elementScope = (OutputScope) this.elementScopesStack[i];
                string      pfx          = null;
                if (elementScope.FindPrefix(nspace, out pfx)) {
                    string testNspace = ResolveNamespace(pfx);
                    if(testNspace != null && Ref.Equal(testNspace, nspace)) {
                        prefix = pfx;
                        return true;
                    }
                    else {
                        break;
                    }
                }
            }
            prefix = null;
            return false;
        }

        internal string GeneratePrefix(string format) {
            string prefix;

            do {
                prefix = String.Format(CultureInfo.InvariantCulture, format, this.prefixIndex ++);
            }while(this.nameTable.Get(prefix) != null);

            return this.nameTable.Add(prefix);
        }
    }
}
