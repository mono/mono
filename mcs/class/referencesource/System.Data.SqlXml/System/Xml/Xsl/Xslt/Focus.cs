//------------------------------------------------------------------------------
// <copyright file="Focus.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Xsl.XPath;
using System.Xml.Xsl.Qil;

namespace System.Xml.Xsl.Xslt {
    using T = XmlQueryTypeFactory;

    // <spec>http://www.w3.org/TR/xslt20/#dt-singleton-focus</spec>
    internal enum SingletonFocusType {
        // No context set
        // Used to prevent bugs
        None,

        // Document node of the document containing the initial context node
        // Used while compiling global variables and params
        InitialDocumentNode,

        // Initial context node for the transformation
        // Used while compiling initial apply-templates
        InitialContextNode,

        // Context node is specified by iterator
        // Used while compiling keys
        Iterator,
    }

    internal struct SingletonFocus : IFocus {
        private XPathQilFactory     f;
        private SingletonFocusType  focusType;
        private QilIterator         current;

        public SingletonFocus(XPathQilFactory f) {
            this.f    = f;
            focusType = SingletonFocusType.None;
            current   = null;
        }

        public void SetFocus(SingletonFocusType focusType) {
            Debug.Assert(focusType != SingletonFocusType.Iterator);
            this.focusType = focusType;
        }

        public void SetFocus(QilIterator current) {
            if (current != null) {
                this.focusType = SingletonFocusType.Iterator;
                this.current   = current;
            } else {
                this.focusType = SingletonFocusType.None;
                this.current   = null;
            }
        }

        [Conditional("DEBUG")]
        private void CheckFocus() {
            Debug.Assert(focusType != SingletonFocusType.None, "Focus is not set, call SetFocus first");
        }

        public QilNode GetCurrent() {
            CheckFocus();
            switch (focusType) {
            case SingletonFocusType.InitialDocumentNode: return f.Root(f.XmlContext());
            case SingletonFocusType.InitialContextNode : return f.XmlContext();
            default:
                Debug.Assert(focusType == SingletonFocusType.Iterator && current != null, "Unexpected singleton focus type");
                return current;
            }
        }

        public QilNode GetPosition() {
            CheckFocus();
            return f.Double(1);
        }

        public QilNode GetLast() {
            CheckFocus();
            return f.Double(1);
        }
    }

    internal struct FunctionFocus : IFocus {
        private bool isSet;
        private QilParameter  current, position, last;

        public void StartFocus(IList<QilNode> args, XslFlags flags) {
            Debug.Assert(! IsFocusSet, "Focus was already set");
            int argNum = 0;
            if ((flags & XslFlags.Current) != 0) {
                this.current = (QilParameter)args[argNum ++];
                Debug.Assert(this.current.Name.NamespaceUri == XmlReservedNs.NsXslDebug && this.current.Name.LocalName == "current");
            }
            if ((flags & XslFlags.Position) != 0) {
                this.position = (QilParameter)args[argNum ++];
                Debug.Assert(this.position.Name.NamespaceUri == XmlReservedNs.NsXslDebug && this.position.Name.LocalName == "position");
            }
            if ((flags & XslFlags.Last) != 0) {
                this.last = (QilParameter)args[argNum ++];
                Debug.Assert(this.last.Name.NamespaceUri == XmlReservedNs.NsXslDebug && this.last.Name.LocalName == "last");
            }
            this.isSet = true;
        }
        public void StopFocus() {
            Debug.Assert(IsFocusSet, "Focus was not set");
            isSet = false;
            this.current = this.position = this.last = null;
        }
        public bool IsFocusSet {
            get { return this.isSet; }
        }

        public QilNode GetCurrent() {
            Debug.Assert(this.current != null, "---- current() is not expected in this function");
            return this.current;
        }

        public QilNode GetPosition() {
            Debug.Assert(this.position != null, "---- position() is not expected in this function");
            return this.position;
        }

        public QilNode GetLast() {
            Debug.Assert(this.last != null, "---- last() is not expected in this function");
            return this.last;
        }
    }

    internal struct LoopFocus : IFocus {
        private XPathQilFactory f;
        private QilIterator     current, cached, last;

        public LoopFocus(XPathQilFactory f) {
            this.f = f;
            current = cached = last = null;
        }

        public void SetFocus(QilIterator current) {
            this.current = current;
            cached = last = null;
        }

        public bool IsFocusSet {
            get { return current != null; }
        }

        public QilNode GetCurrent() {
            return current;
        }

        public QilNode GetPosition() {
            return f.XsltConvert(f.PositionOf(current), T.DoubleX);
        }

        public QilNode GetLast() {
            if (last == null) {
                // Create a let that will be fixed up later in ConstructLoop or by LastFixupVisitor
                last = f.Let(f.Double(0));
            }
            return last;
        }

        public void EnsureCache() {
            if (cached == null) {
                cached = f.Let(current.Binding);
                current.Binding = cached;
            }
        }

        public void Sort(QilNode sortKeys) {
            if (sortKeys != null) {
                // If sorting is required, cache the input node-set to support last() within sort key expressions
                EnsureCache();
                // The rest of the loop content must be compiled in the context of already sorted node-set
                current = f.For(f.Sort(current, sortKeys));
            }
        }

        public QilLoop ConstructLoop(QilNode body) {
            QilLoop result;
            if (last != null) {
                // last() encountered either in the sort keys or in the body of the current loop
                EnsureCache();
                last.Binding = f.XsltConvert(f.Length(cached), T.DoubleX);
            }
            result = f.BaseFactory.Loop(current, body);
            if (last != null) {
                result = f.BaseFactory.Loop(last, result);
            }
            if (cached != null) {
                result = f.BaseFactory.Loop(cached, result);
            }
            return result;
        }
    }
}
