//------------------------------------------------------------------------------
// <copyright file="Stylesheet.cs" company="Microsoft">
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

    internal class Stylesheet {
        private ArrayList       imports           = new ArrayList();
        private Hashtable       modeManagers;
        private Hashtable       templateNameTable = new Hashtable();
        private Hashtable       attributeSetTable;
        private int             templateCount;
        //private ArrayList     preserveSpace;
        private Hashtable       queryKeyTable;
        private ArrayList       whitespaceList;
        private bool            whitespace;
        private Hashtable       scriptObjectTypes = new Hashtable();
        private TemplateManager templates;

        
        private class WhitespaceElement {
            private int    key;
            private double priority;
            private bool   preserveSpace;

            internal double Priority {
                get { return this.priority; }
            }

            internal int Key {
                get { return this.key; }
            }
            
            internal bool PreserveSpace {
                get { return this.preserveSpace; }
            }
            
            internal WhitespaceElement(int Key, double priority, bool PreserveSpace) {
                this.key = Key;
                this.priority = priority;
                this.preserveSpace = PreserveSpace;
            }

            internal void ReplaceValue(bool PreserveSpace) {
                this.preserveSpace = PreserveSpace;
            }
        }

        internal bool      Whitespace        { get { return this.whitespace       ; } }
        internal ArrayList Imports           { get { return this.imports          ; } }
        internal Hashtable AttributeSetTable { get { return this.attributeSetTable; } }
                    
        internal void AddSpace(Compiler compiler, String query, double Priority, bool PreserveSpace) {
            WhitespaceElement elem;
            if (this.queryKeyTable != null) {
                if (this.queryKeyTable.Contains(query)) {
                    elem = (WhitespaceElement) this.queryKeyTable[query];
                    elem.ReplaceValue(PreserveSpace);
                    return;
                }
            }
            else{
                this.queryKeyTable = new Hashtable();
                this.whitespaceList = new ArrayList();
            }
            int key = compiler.AddQuery(query);
            elem = new WhitespaceElement(key, Priority, PreserveSpace);
            this.queryKeyTable[query] = elem;
            this.whitespaceList.Add(elem);
        }

        internal void SortWhiteSpace(){
            if (this.queryKeyTable != null){
                for (int i= 0; i < this.whitespaceList.Count  ; i++ ) {
                    for(int j = this.whitespaceList.Count - 1; j > i; j--) {
                        WhitespaceElement elem1, elem2;
                        elem1 = (WhitespaceElement) this.whitespaceList[j - 1];
                        elem2 = (WhitespaceElement) this.whitespaceList[j];
                        if (elem2.Priority < elem1.Priority) {
                            this.whitespaceList[j - 1] = elem2;
                            this.whitespaceList[j] = elem1;
                        }
                    }
                }
                this.whitespace = true;
            }
            if (this.imports != null) {
                for (int importIndex = this.imports.Count - 1; importIndex >= 0; importIndex --) {
                    Stylesheet stylesheet = (Stylesheet) this.imports[importIndex];
                    if (stylesheet.Whitespace) {
                        stylesheet.SortWhiteSpace();
                        this.whitespace = true;
                    }
                }
            }
        }

        internal bool PreserveWhiteSpace(Processor proc, XPathNavigator node){
            // last one should win. I.E. We starting from the end. I.E. Lowest priority should go first
            if (this.whitespaceList != null) {
                for (int i = this.whitespaceList.Count - 1; 0 <= i; i --) {
                    WhitespaceElement elem = (WhitespaceElement) this.whitespaceList[i];
                    if (proc.Matches(node, elem.Key)) {
                        return elem.PreserveSpace;
                    }
                }
            }
            if (this.imports != null) {
                for (int importIndex = this.imports.Count - 1; importIndex >= 0; importIndex --) {
                    Stylesheet stylesheet = (Stylesheet) this.imports[importIndex];
                    if (! stylesheet.PreserveWhiteSpace(proc, node))
                        return false;
                }
            }
            return true;
        }

        internal void AddAttributeSet(AttributeSetAction attributeSet) {
            Debug.Assert(attributeSet.Name != null);
            if (this.attributeSetTable == null) {
                this.attributeSetTable = new Hashtable();
            }
            Debug.Assert(this.attributeSetTable != null);

            if (this.attributeSetTable.ContainsKey(attributeSet.Name) == false) {
                this.attributeSetTable[attributeSet.Name] = attributeSet;
            }
            else {
                // merge the attribute-sets
                ((AttributeSetAction)this.attributeSetTable[attributeSet.Name]).Merge(attributeSet);
            }
        }

        internal void AddTemplate(TemplateAction template) {
            XmlQualifiedName mode = template.Mode;

            //
            // Ensure template has a unique name
            //

            Debug.Assert(this.templateNameTable != null);

            if (template.Name != null) {
                if (this.templateNameTable.ContainsKey(template.Name) == false) {
                    this.templateNameTable[template.Name] = template;
                }
                else {
                    throw XsltException.Create(Res.Xslt_DupTemplateName, template.Name.ToString());
                }
            }


            if (template.MatchKey != Compiler.InvalidQueryKey) {
                if (this.modeManagers == null) {
                    this.modeManagers = new Hashtable();
                }
                Debug.Assert(this.modeManagers != null);

                if (mode == null) {
                    mode = XmlQualifiedName.Empty;
                }

                TemplateManager manager = (TemplateManager) this.modeManagers[mode];

                if (manager == null) {
                    manager = new TemplateManager(this, mode);

                    this.modeManagers[mode] = manager;

                    if (mode.IsEmpty) {
                        Debug.Assert(this.templates == null);
                        this.templates = manager;
                    }
                }
                Debug.Assert(manager != null);

                template.TemplateId = ++ this.templateCount;
                manager.AddTemplate(template);
            }
        }

        internal void ProcessTemplates() {
            if (this.modeManagers != null) {
                IDictionaryEnumerator enumerator = this.modeManagers.GetEnumerator();
                while (enumerator.MoveNext()) {
                    Debug.Assert(enumerator.Value is TemplateManager);
                    TemplateManager manager = (TemplateManager) enumerator.Value;
                    manager.ProcessTemplates();
                }
            }

            if (this.imports != null) {
                for (int importIndex = this.imports.Count - 1; importIndex >= 0; importIndex --) {
                    Debug.Assert(this.imports[importIndex] is Stylesheet);
                    Stylesheet stylesheet = (Stylesheet) this.imports[importIndex];
                    Debug.Assert(stylesheet != null);

                    //
                    // Process templates in imported stylesheet
                    //

                    stylesheet.ProcessTemplates();
                }
            }
        }


        internal void ReplaceNamespaceAlias(Compiler compiler){
            if (this.modeManagers != null) {
                IDictionaryEnumerator enumerator = this.modeManagers.GetEnumerator();
                while (enumerator.MoveNext()) {
                    TemplateManager manager = (TemplateManager) enumerator.Value;
                    if (manager.templates != null) {
                        for(int i=0 ; i< manager.templates.Count; i++) {
                            TemplateAction template = (TemplateAction) manager.templates[i];
                            template.ReplaceNamespaceAlias(compiler);
                        }
                    }
                }
            }
            if (this.templateNameTable != null) {
                IDictionaryEnumerator enumerator = this.templateNameTable.GetEnumerator();
                while (enumerator.MoveNext()) {
                    TemplateAction template = (TemplateAction) enumerator.Value;
                    template.ReplaceNamespaceAlias(compiler);
                }
            }
            if (this.imports != null) {
                for (int importIndex = this.imports.Count - 1; importIndex >= 0; importIndex --) {
                    Stylesheet stylesheet = (Stylesheet) this.imports[importIndex];
                    stylesheet.ReplaceNamespaceAlias(compiler);
                }
            }
        }

        internal TemplateAction FindTemplate(Processor processor, XPathNavigator navigator, XmlQualifiedName mode) {
            Debug.Assert(processor != null && navigator != null);
            Debug.Assert(mode != null);
            TemplateAction  action  = null;

            //
            // Try to find template within this stylesheet first
            //
            if (this.modeManagers != null) {
                TemplateManager manager = (TemplateManager) this.modeManagers[mode];

                if (manager != null) {
                    Debug.Assert(manager.Mode.Equals(mode));
                    action = manager.FindTemplate(processor, navigator);
                }
            }

            //
            // If unsuccessful, search in imported documents from backwards
            //

            if (action == null) {
                action = FindTemplateImports(processor, navigator, mode);
            }

            return action;
        }

        internal TemplateAction FindTemplateImports(Processor processor, XPathNavigator navigator, XmlQualifiedName mode) {
            TemplateAction action = null;

            //
            // Do we have imported stylesheets?
            //

            if (this.imports != null) {
                for (int importIndex = this.imports.Count - 1; importIndex >= 0; importIndex --) {
                    Debug.Assert(this.imports[importIndex] is Stylesheet);
                    Stylesheet stylesheet = (Stylesheet) this.imports[importIndex];
                    Debug.Assert(stylesheet != null);

                    //
                    // Search in imported stylesheet
                    //

                    action = stylesheet.FindTemplate(processor, navigator, mode);

                    if (action != null) {
                        return action;
                    }
                }
            }

            return action;
        }

        internal TemplateAction FindTemplate(Processor processor, XPathNavigator navigator) {
            Debug.Assert(processor != null && navigator != null);
            Debug.Assert(this.templates == null && this.modeManagers == null || this.templates == this.modeManagers[XmlQualifiedName.Empty]);

            TemplateAction action = null;

            //
            // Try to find template within this stylesheet first
            //

            if (this.templates != null) {
                action = this.templates.FindTemplate(processor, navigator);
            }

            //
            // If unsuccessful, search in imported documents from backwards
            //

            if (action == null) {
                action = FindTemplateImports(processor, navigator);
            }

            return action;
        }

        internal TemplateAction FindTemplate(XmlQualifiedName name) {
            //Debug.Assert(this.templateNameTable == null);

            TemplateAction action = null;

            //
            // Try to find template within this stylesheet first
            //

            if (this.templateNameTable != null) {
                action = (TemplateAction)this.templateNameTable[name];
            }

            //
            // If unsuccessful, search in imported documents from backwards
            //

            if (action == null && this.imports != null) {
                for (int importIndex = this.imports.Count - 1; importIndex >= 0; importIndex --) {
                    Debug.Assert(this.imports[importIndex] is Stylesheet);
                    Stylesheet stylesheet = (Stylesheet) this.imports[importIndex];
                    Debug.Assert(stylesheet != null);

                    //
                    // Search in imported stylesheet
                    //

                    action = stylesheet.FindTemplate(name);

                    if (action != null) {
                        return action;
                    }
                }
            }

            return action;
        }

        internal TemplateAction FindTemplateImports(Processor processor, XPathNavigator navigator) {
            TemplateAction action = null;

            //
            // Do we have imported stylesheets?
            //

            if (this.imports != null) {
                for (int importIndex = this.imports.Count - 1; importIndex >= 0; importIndex --) {
                    Debug.Assert(this.imports[importIndex] is Stylesheet);
                    Stylesheet stylesheet = (Stylesheet) this.imports[importIndex];
                    Debug.Assert(stylesheet != null);

                    //
                    // Search in imported stylesheet
                    //

                    action = stylesheet.FindTemplate(processor, navigator);

                    if (action != null) {
                        return action;
                    }
                }
            }

            return action;
        }

        internal Hashtable ScriptObjectTypes {
            get { return this.scriptObjectTypes; }
        }
    }
}
