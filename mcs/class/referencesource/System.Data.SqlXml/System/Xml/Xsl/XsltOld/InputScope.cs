//------------------------------------------------------------------------------
// <copyright file="InputScope.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Xml.Xsl.XsltOld {
    using Res = System.Xml.Utils.Res;
    using System;
    using System.Diagnostics;
    using System.Xml;
    using System.Xml.XPath;
    using System.Collections;

    internal class InputScope : DocumentScope {
        private InputScope      parent;
        private bool            forwardCompatibility;
        private bool            canHaveApplyImports;
        private Hashtable       variables;
        private Hashtable       extensionNamespaces;
        private Hashtable       excludedNamespaces;
       
        internal InputScope Parent {
            get { return this.parent; }
        }

        internal Hashtable Variables {
            get { return this.variables; }
        }
        
        internal bool ForwardCompatibility {
            get { return this.forwardCompatibility; }
            set { this.forwardCompatibility = value; }
        }

        internal bool CanHaveApplyImports {
            get { return this.canHaveApplyImports; }
            set { this.canHaveApplyImports = value; }
        }

        internal InputScope(InputScope parent) {
            Init(parent);
        }

        internal void Init(InputScope parent) {
            this.scopes = null;
            this.parent = parent;

            if (this.parent != null) {
                this.forwardCompatibility = this.parent.forwardCompatibility;
                this.canHaveApplyImports  = this.parent.canHaveApplyImports;
            }
        }

        internal void InsertExtensionNamespace(String nspace) {
            if (this.extensionNamespaces == null ) {
                this.extensionNamespaces = new Hashtable();
            }
            this.extensionNamespaces[nspace] = null;
        }
        
        internal bool IsExtensionNamespace(String nspace) {
            if (extensionNamespaces == null ) {
                return false;
            }
            return extensionNamespaces.Contains(nspace);
        }
        
        internal void InsertExcludedNamespace(String nspace) {
            if (this.excludedNamespaces == null ) {
                this.excludedNamespaces = new Hashtable();
            }
            this.excludedNamespaces[nspace] = null;
        }

        internal bool IsExcludedNamespace(String nspace) {
            if (excludedNamespaces == null ) {
                return false;
            }
            return excludedNamespaces.Contains(nspace);
        }
        
        internal void InsertVariable(VariableAction variable) {
            Debug.Assert(variable != null);

            if (this.variables == null) {
                this.variables = new Hashtable();
            }
            this.variables[variable.Name] = variable;
        }
        
        internal int GetVeriablesCount() {
            if (this.variables == null) {
                return 0;
            }
            return this.variables.Count;
        }

        public VariableAction ResolveVariable(XmlQualifiedName qname) {
            for (InputScope inputScope = this; inputScope != null; inputScope = inputScope.Parent) {
                if (inputScope.Variables != null) {
                    VariableAction variable = (VariableAction) inputScope.Variables[qname];
                    if(variable != null) {
                        return variable;
                    }
                }
            }
            return null;
        }

        public VariableAction ResolveGlobalVariable(XmlQualifiedName qname) {
            InputScope prevScope = null;
            for (InputScope inputScope = this; inputScope != null; inputScope = inputScope.Parent) {
                prevScope = inputScope;
            }
            return prevScope.ResolveVariable(qname);
        }
    }
}
