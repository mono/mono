//------------------------------------------------------------------------------
// <copyright file="RootAction.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Xml.Xsl.XsltOld {
    using Res = System.Xml.Utils.Res;
    using System;
    using System.Diagnostics;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Xml;
    using System.Xml.XPath;
    using System.Xml.Xsl.Runtime;
    using MS.Internal.Xml.XPath;
    using System.Security;

    internal class Key {
        XmlQualifiedName name;
        int              matchKey;
        int              useKey;
        ArrayList        keyNodes;

        public Key(XmlQualifiedName name, int matchkey, int usekey) {
            this.name     = name;
            this.matchKey = matchkey;
            this.useKey   = usekey;
            this.keyNodes = null;
        }

        public XmlQualifiedName Name { get { return this.name;     } }
        public int MatchKey          { get { return this.matchKey; } }
        public int UseKey            { get { return this.useKey;   } }

        public void AddKey(XPathNavigator root, Hashtable table) {
            if (this.keyNodes == null) {
                this.keyNodes = new ArrayList();
            }
            this.keyNodes.Add(new DocumentKeyList(root, table));
        }

        public Hashtable GetKeys(XPathNavigator root) {
            if (this.keyNodes != null) {
                for(int i=0; i < keyNodes.Count; i++) {
                    if (((DocumentKeyList)keyNodes[i]).RootNav.IsSamePosition(root)) {
                        return ((DocumentKeyList)keyNodes[i]).KeyTable;
                    }
                }
            }
            return null;
        }

        public Key Clone() {
            return new Key(name, matchKey, useKey);
        }
    }

    internal struct DocumentKeyList {
        XPathNavigator rootNav;
        Hashtable   keyTable;

        public DocumentKeyList(XPathNavigator rootNav, Hashtable keyTable) {
            this.rootNav = rootNav;
            this.keyTable = keyTable;
        }

        public XPathNavigator RootNav  { get { return this.rootNav ; } }
        public Hashtable      KeyTable { get { return this.keyTable; } }
    }
    
    internal class RootAction : TemplateBaseAction {
        private  const int    QueryInitialized = 2;
        private  const int    RootProcessed    = 3;
        
        private  Hashtable  attributeSetTable  = new Hashtable();
        private  Hashtable  decimalFormatTable = new Hashtable();
        private  List<Key>  keyList;
        private  XsltOutput output;
        public   Stylesheet builtInSheet;
        public   PermissionSet   permissions;

        internal XsltOutput Output {
            get { 
                if (this.output == null) {
                    this.output = new XsltOutput();
                }
                return this.output; 
            }
        }

        /*
         * Compile
         */
        internal override void Compile(Compiler compiler) {
            CompileDocument(compiler, /*inInclude*/ false);
        }

        internal void InsertKey(XmlQualifiedName name, int MatchKey, int UseKey){
            if (this.keyList == null) {
                this.keyList = new List<Key>();
            }
            this.keyList.Add(new Key(name, MatchKey, UseKey));
        }

        internal AttributeSetAction GetAttributeSet(XmlQualifiedName name) {
            AttributeSetAction action = (AttributeSetAction) this.attributeSetTable[name];
            if(action == null) {
                throw XsltException.Create(Res.Xslt_NoAttributeSet, name.ToString());
            }
            return action;
        }


        public void PorcessAttributeSets(Stylesheet rootStylesheet) {
            MirgeAttributeSets(rootStylesheet);

            // As we mentioned we need to invert all lists.
            foreach (AttributeSetAction attSet in this.attributeSetTable.Values) {
                if (attSet.containedActions != null) {
                    attSet.containedActions.Reverse();
                }
            }

            //  ensures there are no cycles in the attribute-sets use dfs marking method
            CheckAttributeSets_RecurceInList(new Hashtable(), this.attributeSetTable.Keys);
        }

        private void MirgeAttributeSets(Stylesheet stylesheet) {
            // mirge stylesheet.AttributeSetTable to this.AttributeSetTable

            if (stylesheet.AttributeSetTable != null) {
                foreach (AttributeSetAction srcAttSet in stylesheet.AttributeSetTable.Values) {
                    ArrayList srcAttList = srcAttSet.containedActions;
                    AttributeSetAction dstAttSet = (AttributeSetAction) this.attributeSetTable[srcAttSet.Name];
                    if (dstAttSet == null) {
                        dstAttSet = new AttributeSetAction(); {
                            dstAttSet.name             = srcAttSet.Name;
                            dstAttSet.containedActions = new ArrayList();
                        }
                        this.attributeSetTable[srcAttSet.Name] = dstAttSet;
                    }
                    ArrayList dstAttList = dstAttSet.containedActions;
                    // We adding attributes in reverse order for purpuse. In the mirged list most importent attset shoud go last one
                    // so we'll need to invert dstAttList finaly. 
                    if (srcAttList != null) {
                        for(int src = srcAttList.Count - 1; 0 <= src; src --) {
                            // We can ignore duplicate attibutes here.
                            dstAttList.Add(srcAttList[src]);
                        }
                    }
                }
            }

            foreach (Stylesheet importedStylesheet in stylesheet.Imports) {
                MirgeAttributeSets(importedStylesheet);
            }
        }

        private void CheckAttributeSets_RecurceInList(Hashtable markTable, ICollection setQNames) {
            const string PROCESSING = "P";
            const string DONE       = "D";

            foreach (XmlQualifiedName qname in setQNames) {
                object mark = markTable[qname];
                if (mark == (object) PROCESSING) {
                    throw XsltException.Create(Res.Xslt_CircularAttributeSet, qname.ToString());
                } else if (mark == (object) DONE) {
                    continue; // optimization: we already investigated this attribute-set.
                } else {
                    Debug.Assert(mark == null);

                    markTable[qname] = (object) PROCESSING;
                    CheckAttributeSets_RecurceInContainer(markTable, GetAttributeSet(qname));
                    markTable[qname] = (object) DONE;
                }
            }
        }

        private void CheckAttributeSets_RecurceInContainer(Hashtable markTable, ContainerAction container) {
            if (container.containedActions == null) {
                return;
            }
            foreach(Action action in container.containedActions) {
                if(action is UseAttributeSetsAction) {
                    CheckAttributeSets_RecurceInList(markTable, ((UseAttributeSetsAction)action).UsedSets);
                } else if(action is ContainerAction) {
                    CheckAttributeSets_RecurceInContainer(markTable, (ContainerAction)action);
                }
            }
        }
        
        internal void AddDecimalFormat(XmlQualifiedName name, DecimalFormat formatinfo) { 
            DecimalFormat exist = (DecimalFormat) this.decimalFormatTable[name];
            if (exist != null) {
                NumberFormatInfo info    = exist.info;
                NumberFormatInfo newinfo = formatinfo.info;
                if (info.NumberDecimalSeparator   != newinfo.NumberDecimalSeparator   ||
                    info.NumberGroupSeparator     != newinfo.NumberGroupSeparator     ||
                    info.PositiveInfinitySymbol   != newinfo.PositiveInfinitySymbol   ||
                    info.NegativeSign             != newinfo.NegativeSign             ||
                    info.NaNSymbol                != newinfo.NaNSymbol                ||
                    info.PercentSymbol            != newinfo.PercentSymbol            ||
                    info.PerMilleSymbol           != newinfo.PerMilleSymbol           ||
                    exist.zeroDigit               != formatinfo.zeroDigit             ||
                    exist.digit                   != formatinfo.digit                 ||
                    exist.patternSeparator        != formatinfo.patternSeparator 
                ) {
                    throw XsltException.Create(Res.Xslt_DupDecimalFormat, name.ToString());
                }
            }
            this.decimalFormatTable[name] = formatinfo;
        }

        internal DecimalFormat GetDecimalFormat(XmlQualifiedName name) {
            return this.decimalFormatTable[name] as DecimalFormat;
        }

        internal List<Key> KeyList{
            get { return this.keyList; }
        }

       internal override void Execute(Processor processor, ActionFrame frame) {
            Debug.Assert(processor != null && frame != null);

            switch (frame.State) {
            case Initialized:
                frame.AllocateVariables(variableCount);
                XPathNavigator root = processor.Document.Clone();
                root.MoveToRoot();
                frame.InitNodeSet(new XPathSingletonIterator(root));
               
                if (this.containedActions != null && this.containedActions.Count > 0) {
                    processor.PushActionFrame(frame);
                }
                frame.State = QueryInitialized;
                break;
            case QueryInitialized:
                Debug.Assert(frame.State == QueryInitialized);
                frame.NextNode(processor);
                Debug.Assert(Processor.IsRoot(frame.Node));
                if (processor.Debugger != null) {
                    // this is like apply-templates, but we don't have it on stack. 
                    // Pop the stack, otherwise last instruction will be on it.
                    processor.PopDebuggerStack();
                }
                processor.PushTemplateLookup(frame.NodeSet, /*mode:*/null, /*importsOf:*/null);

                frame.State = RootProcessed;
                break;

            case RootProcessed:
                Debug.Assert(frame.State == RootProcessed);
                frame.Finished();
                break;
            default:
                Debug.Fail("Invalid RootAction execution state");
		        break;
            }
        }
    }
}
