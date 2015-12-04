//------------------------------------------------------------------------------
// <copyright file="QilXmlReader.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Xsl;

namespace System.Xml.Xsl.Qil {

    /// <summary>
    /// Read the output of QilXmlWriter.
    /// </summary>
    /// <remarks>This internal class allows roundtripping between the Xml serialization format for
    /// QIL and the in-memory data structure.</remarks>
    internal sealed class QilXmlReader {
        private static Regex  lineInfoRegex = new Regex(@"\[(\d+),(\d+) -- (\d+),(\d+)\]");
        private static Regex  typeInfoRegex = new Regex(@"(\w+);([\w|\|]+);(\w+)");
        private static Dictionary<string, MethodInfo> nameToFactoryMethod;

        private QilFactory f;
        private XmlReader r;
        private Stack<QilList> stk;
        private bool inFwdDecls;
        private Dictionary<string, QilNode> scope, fwdDecls;

        static QilXmlReader() {
            nameToFactoryMethod = new Dictionary<string, MethodInfo>();

            // Build table that maps QilNodeType name to factory method info
            foreach (MethodInfo mi in typeof(QilFactory).GetMethods(BindingFlags.Public | BindingFlags.Instance)) {
                ParameterInfo[] parms = mi.GetParameters();
                int i;

                // Only match methods that take QilNode parameters
                for (i = 0; i < parms.Length; i++) {
                    if (parms[i].ParameterType != typeof(QilNode))
                        break;
                }

                if (i == parms.Length) {
                    // Enter the method that takes the maximum number of parameters
                    if (!nameToFactoryMethod.ContainsKey(mi.Name) || nameToFactoryMethod[mi.Name].GetParameters().Length < parms.Length)
                        nameToFactoryMethod[mi.Name] = mi;
                }
            }
        }

        public QilXmlReader(XmlReader r) {
            this.r = r;
            this.f = new QilFactory();
        }

        public QilExpression Read() {
            this.stk = new Stack<QilList>();
            this.inFwdDecls = false;
            this.scope = new Dictionary<string, QilNode>();
            this.fwdDecls = new Dictionary<string, QilNode>();

            this.stk.Push(f.Sequence());

            while (r.Read()) {
                switch (r.NodeType) {
                    case XmlNodeType.Element:
                        bool emptyElem = r.IsEmptyElement;

                        // XmlReader does not give an event for empty elements, so synthesize one
                        if (StartElement() && emptyElem)
                            EndElement();
                        break;

                    case XmlNodeType.EndElement:
                        EndElement();
                        break;

                    case XmlNodeType.Whitespace:
                    case XmlNodeType.SignificantWhitespace:
                    case XmlNodeType.XmlDeclaration:
                    case XmlNodeType.Comment:
                    case XmlNodeType.ProcessingInstruction:
                        break;

                    default:
                        Debug.Fail("Unexpected event " + r.NodeType + ", value " + r.Value);
                        break;
                }
            }

            Debug.Assert(this.fwdDecls.Keys.Count == 0, "One or more forward declarations were never defined");
            Debug.Assert(this.stk.Peek()[0].NodeType == QilNodeType.QilExpression, "Serialized qil tree did not contain a QilExpression node");
            return (QilExpression) this.stk.Peek()[0];
        }

        private bool StartElement() {
            QilNode nd;
            ReaderAnnotation ann = new ReaderAnnotation();
            string s;

            // Special case certain element names
            s = r.LocalName;
            switch (r.LocalName) {
                case "LiteralString":
                    nd = f.LiteralString(ReadText());
                    break;

                case "LiteralInt32":
                    nd = f.LiteralInt32(Int32.Parse(ReadText(), CultureInfo.InvariantCulture));
                    break;

                case "LiteralInt64":
                    nd = f.LiteralInt64(Int64.Parse(ReadText(), CultureInfo.InvariantCulture));
                    break;

                case "LiteralDouble":
                    nd = f.LiteralDouble(Double.Parse(ReadText(), CultureInfo.InvariantCulture));
                    break;

                case "LiteralDecimal":
                    nd = f.LiteralDecimal(Decimal.Parse(ReadText(), CultureInfo.InvariantCulture));
                    break;

                case "LiteralType":
                    nd = f.LiteralType(ParseType(ReadText()));
                    break;

                case "LiteralQName":
                    nd = ParseName(r.GetAttribute("name"));
                    Debug.Assert(nd != null, "LiteralQName element must have a name attribute");
                    Debug.Assert(r.IsEmptyElement, "LiteralQName element must be empty");
                    break;

                case "For":
                case "Let":
                case "Parameter":
                case "Function":
                case "RefTo":
                    ann.Id = r.GetAttribute("id");
                    ann.Name = ParseName(r.GetAttribute("name"));
                    goto default;

                case "XsltInvokeEarlyBound":
                    ann.ClrNamespace = r.GetAttribute("clrNamespace");
                    goto default;

                case "ForwardDecls":
                    this.inFwdDecls = true;
                    goto default;

                default:
                    // Create sequence
                    nd = f.Sequence();
                    break;
            }

            // Save xml type and source line information
            ann.XmlType = ParseType(r.GetAttribute("xmlType"));;
            nd.SourceLine = ParseLineInfo(r.GetAttribute("lineInfo"));
            nd.Annotation = ann;

            if (nd is QilList) {
                // Push new parent list onto stack
                this.stk.Push((QilList) nd);
                return true;
            }

            // Add node to its parent's list
            this.stk.Peek().Add(nd);
            return false;
        }

        private void EndElement() {
            MethodInfo facMethod = null;
            object[] facArgs;
            QilList list;
            QilNode nd;
            ReaderAnnotation ann;

            list = this.stk.Pop();
            ann = (ReaderAnnotation) list.Annotation;

            // Special case certain element names
            string s = r.LocalName;
            switch (r.LocalName) {
                case "QilExpression": {
                    Debug.Assert(list.Count > 0, "QilExpression node requires a Root expression");
                    QilExpression qil = f.QilExpression(list[list.Count - 1]);

                    // Be flexible on order and presence of QilExpression children
                    for (int i = 0; i < list.Count - 1; i++) {
                        switch (list[i].NodeType) {
                            case QilNodeType.True:
                            case QilNodeType.False:
                                qil.IsDebug = list[i].NodeType == QilNodeType.True;
                                break;

                            case QilNodeType.FunctionList:
                                qil.FunctionList = (QilList) list[i];
                                break;

                            case QilNodeType.GlobalVariableList:
                                qil.GlobalVariableList = (QilList) list[i];
                                break;

                            case QilNodeType.GlobalParameterList:
                                qil.GlobalParameterList = (QilList) list[i];
                                break;
                        }
                    }
                    nd = qil;
                    break;
                }

                case "ForwardDecls":
                    this.inFwdDecls = false;
                    return;

                case "Parameter":
                case "Let":
                case "For":
                case "Function": {
                    string id = ann.Id;
                    QilName name = ann.Name;
                    Debug.Assert(id != null, r.LocalName + " must have an id attribute");
                    Debug.Assert(!this.inFwdDecls || ann.XmlType != null, "Forward decl for " + r.LocalName + " '" + id + "' must have an xmlType attribute");

                    // Create node (may be discarded later if it was already declared in forward declarations section)
                    switch (r.LocalName) {
                        case "Parameter":
                            Debug.Assert(list.Count <= (this.inFwdDecls ? 0 : 1), "Parameter '" + id + "' must have 0 or 1 arguments");
                            Debug.Assert(ann.XmlType != null, "Parameter '" + id + "' must have an xmlType attribute");
                            if (this.inFwdDecls || list.Count == 0)
                                nd = f.Parameter(null, name, ann.XmlType);
                            else
                                nd = f.Parameter(list[0], name, ann.XmlType);
                            break;

                        case "Let":
                            Debug.Assert(list.Count == (this.inFwdDecls ? 0 : 1), "Let '" + id + "' must have 0 or 1 arguments");
                            if (this.inFwdDecls)
                                nd = f.Let(f.Unknown(ann.XmlType));
                            else
                                nd = f.Let(list[0]);
                            break;

                        case "For":
                            Debug.Assert(list.Count == 1, "For '" + id + "' must have 1 argument");
                            nd = f.For(list[0]);
                            break;

                        default:
                            Debug.Assert(list.Count == (this.inFwdDecls ? 2 : 3), "Function '" + id + "' must have 2 or 3 arguments");
                            if (this.inFwdDecls)
                                nd = f.Function(list[0], list[1], ann.XmlType);
                            else
                                nd = f.Function(list[0], list[1], list[2], ann.XmlType != null ? ann.XmlType : list[1].XmlType);
                            break;
                    }

                    // Set DebugName
                    if (name != null)
                        ((QilReference) nd).DebugName = name.ToString();

                    if (this.inFwdDecls) {
                        Debug.Assert(!this.scope.ContainsKey(id), "Multiple nodes have id '" + id + "'");
                        this.fwdDecls[id] = nd;
                        this.scope[id] = nd;
                    }
                    else {
                        if (this.fwdDecls.ContainsKey(id)) {
                            // Replace forward declaration
                            Debug.Assert(r.LocalName == Enum.GetName(typeof(QilNodeType), nd.NodeType), "Id '" + id + "' is not not bound to a " + r.LocalName + " forward decl");
                            nd = this.fwdDecls[id];
                            this.fwdDecls.Remove(id);

                            if (list.Count > 0) nd[0] = list[0];
                            if (list.Count > 1) nd[1] = list[1];
                        }
                        else {
                            // Put reference in scope
                            Debug.Assert(!this.scope.ContainsKey(id), "Id '" + id + "' is already in scope");
                            this.scope[id] = nd;
                        }
                    }
                    nd.Annotation = ann;
                    break;
                }

                case "RefTo": {
                    // Lookup reference
                    string id = ann.Id;
                    Debug.Assert(id != null, r.LocalName + " must have an id attribute");

                    Debug.Assert(this.scope.ContainsKey(id), "Id '" + id + "' is not in scope");
                    this.stk.Peek().Add(this.scope[id]);
                    return;
                }

                case "Sequence":
                    nd = f.Sequence(list);
                    break;

                case "FunctionList":
                    nd = f.FunctionList(list);
                    break;

                case "GlobalVariableList":
                    nd = f.GlobalVariableList(list);
                    break;

                case "GlobalParameterList":
                    nd = f.GlobalParameterList(list);
                    break;

                case "ActualParameterList":
                    nd = f.ActualParameterList(list);
                    break;

                case "FormalParameterList":
                    nd = f.FormalParameterList(list);
                    break;

                case "SortKeyList":
                    nd = f.SortKeyList(list);
                    break;

                case "BranchList":
                    nd = f.BranchList(list);
                    break;

                case "XsltInvokeEarlyBound": {
                    Debug.Assert(ann.ClrNamespace != null, "XsltInvokeEarlyBound must have a clrNamespace attribute");
                    Debug.Assert(list.Count == 2, "XsltInvokeEarlyBound must have exactly 2 arguments");
                    Debug.Assert(list.XmlType != null, "XsltInvokeEarlyBound must have an xmlType attribute");
                    MethodInfo mi = null;
                    QilName name = (QilName) list[0];

                    foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies()) {
                        Type t = asm.GetType(ann.ClrNamespace);
                        if (t != null) {
                            mi = t.GetMethod(name.LocalName);
                            break;
                        }
                    }

                    Debug.Assert(mi != null, "Cannot find method " + ann.ClrNamespace + "." + name.ToString());

                    nd = f.XsltInvokeEarlyBound(name, f.LiteralObject(mi), list[1], ann.XmlType);
                    break;
                }

                default: {
                    // Find factory method which will be used to construct the Qil node
                    Debug.Assert(nameToFactoryMethod.ContainsKey(r.LocalName), "Method " + r.LocalName + " could not be found on QilFactory");
                    facMethod = nameToFactoryMethod[r.LocalName];
                    Debug.Assert(facMethod.GetParameters().Length == list.Count, "NodeType " + r.LocalName + " does not allow " + list.Count + " parameters");

                    // Create factory method arguments
                    facArgs = new object[list.Count];
                    for (int i = 0; i < facArgs.Length; i++)
                        facArgs[i] = list[i];

                    // Create node and set its properties
                    nd = (QilNode) facMethod.Invoke(f, facArgs);
                    break;
                }
            }

            nd.SourceLine = list.SourceLine;

            // Add node to its parent's list
            this.stk.Peek().Add(nd);
        }

        private string ReadText() {
            string s = string.Empty;

            if (!r.IsEmptyElement) {
                while (r.Read()) {
                    switch (r.NodeType) {
                        case XmlNodeType.Text:
                        case XmlNodeType.SignificantWhitespace:
                        case XmlNodeType.Whitespace:
                            s += r.Value;
                            continue;
                    }

                    break;
                }
            }

            return s;
        }

        private ISourceLineInfo ParseLineInfo(string s) {
            if (s != null && s.Length > 0) {
                Match m = lineInfoRegex.Match(s);
                Debug.Assert(m.Success && m.Groups.Count == 5, "Malformed lineInfo attribute");
                return new SourceLineInfo("",
                    Int32.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture),
                    Int32.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture),
                    Int32.Parse(m.Groups[3].Value, CultureInfo.InvariantCulture),
                    Int32.Parse(m.Groups[4].Value, CultureInfo.InvariantCulture)
                );
            }
            return null;
        }

        private XmlQueryType ParseType(string s) {
            if (s != null && s.Length > 0) {
                Match m = typeInfoRegex.Match(s);
                Debug.Assert(m.Success && m.Groups.Count == 4, "Malformed Type info");

                XmlQueryCardinality qc = new XmlQueryCardinality(m.Groups[1].Value);
                bool strict = bool.Parse(m.Groups[3].Value);

                string[] codes = m.Groups[2].Value.Split('|');
                XmlQueryType[] types = new XmlQueryType[codes.Length];

                for (int i = 0; i < codes.Length; i++)
                    types[i] = XmlQueryTypeFactory.Type((XmlTypeCode)Enum.Parse(typeof(XmlTypeCode), codes[i]), strict);

                return XmlQueryTypeFactory.Product(XmlQueryTypeFactory.Choice(types), qc);
            }
            return null;
        }

        private QilName ParseName(string name) {
            string prefix, local, uri;
            int idx;

            if (name != null && name.Length > 0) {
                // If name contains '}' character, then namespace is non-empty
                idx = name.LastIndexOf('}');
                if (idx != -1 && name[0] == '{') {
                    uri = name.Substring(1, idx - 1);
                    name = name.Substring(idx + 1);
                }
                else {
                    uri = string.Empty;
                }

                // Parse QName
                ValidateNames.ParseQNameThrow(name, out prefix, out local);

                return f.LiteralQName(local, uri, prefix);
            }
            return null;
        }

        private class ReaderAnnotation {
            public string Id;
            public QilName Name;
            public XmlQueryType XmlType;
            public string ClrNamespace;
        }
    }
}
