//------------------------------------------------------------------------------
// <copyright file="QilGeneratorEnv.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.XPath;
using System.Xml.Schema;
using System.Xml.Xsl.Qil;
using System.Xml.Xsl.Runtime;
using System.Xml.Xsl.XPath;

namespace System.Xml.Xsl.Xslt {
    using FunctionInfo  = XPathBuilder.FunctionInfo<QilGenerator.FuncId>;
    using Res           = System.Xml.Utils.Res;
    using T             = XmlQueryTypeFactory;

    internal partial class QilGenerator : IXPathEnvironment {

        // Everywhere in this code in case of error in the stylesheet we should throw XslLoadException.
        // This helper IErrorHelper implementation is used to wrap XmlException's into XslLoadException's.

        private struct ThrowErrorHelper : IErrorHelper {
            public void ReportError(string res, params string[] args) {
                Debug.Assert(args == null || args.Length == 0, "Error message must already be composed in res");
                throw new XslLoadException(Res.Xml_UserException, res);
            }

            public void ReportWarning(string res, params string[] args) {
                Debug.Fail("Should never get here");
            }
        }

        // -------------------------------- IXPathEnvironment --------------------------------
        // IXPathEnvironment represents static context (namespaces, focus) and most naturaly implemented by QilGenerator itself
        //                        $var current() key()
        // */@select or */@test     +     +       +
        // template/@match          -     -       +
        // key/@match               -     -       -
        // key/@use                 -     +       -
        // number/@count            +     -       +
        // number/@from             +     -       +

        private bool allowVariables = true;
        private bool allowCurrent   = true;
        private bool allowKey       = true;

        private void SetEnvironmentFlags(bool allowVariables, bool allowCurrent, bool allowKey) {
            this.allowVariables = allowVariables;
            this.allowCurrent   = allowCurrent  ;
            this.allowKey       = allowKey      ;
        }

        XPathQilFactory IXPathEnvironment.Factory { get { return f; } }

        // IXPathEnvironment interface
        QilNode IFocus.GetCurrent()  { return this.GetCurrentNode();     }
        QilNode IFocus.GetPosition() { return this.GetCurrentPosition(); }
        QilNode IFocus.GetLast()     { return this.GetLastPosition();    }

        string IXPathEnvironment.ResolvePrefix(string prefix) {
            return ResolvePrefixThrow(true, prefix);
        }

        QilNode IXPathEnvironment.ResolveVariable(string prefix, string name) {
            if (!allowVariables) {
                throw new XslLoadException(Res.Xslt_VariablesNotAllowed);
            }
            string ns = ResolvePrefixThrow(/*ignoreDefaultNs:*/true, prefix);
            Debug.Assert(ns != null);

            // Look up in params and variables of the current scope and all outer ones
            QilNode var = this.scope.LookupVariable(name, ns);

            if (var == null) {
                throw new XslLoadException(Res.Xslt_InvalidVariable, Compiler.ConstructQName(prefix, name));
            }

            // All Node* parameters are guaranteed to be in document order with no duplicates, so TypeAssert
            // this so that optimizer can use this information to avoid redundant sorts and duplicate removal.
            XmlQueryType varType = var.XmlType;
            if (var.NodeType == QilNodeType.Parameter && varType.IsNode && varType.IsNotRtf && varType.MaybeMany && !varType.IsDod) {
                var = f.TypeAssert(var, XmlQueryTypeFactory.NodeSDod);
            }

            return var;
        }

        // NOTE: DO NOT call QilNode.Clone() while executing this method since fixup nodes cannot be cloned
        QilNode IXPathEnvironment.ResolveFunction(string prefix, string name, IList<QilNode> args, IFocus env) {
            Debug.Assert(!args.IsReadOnly, "Writable collection expected");
            if (prefix.Length == 0) {
                FunctionInfo func;
                if (FunctionTable.TryGetValue(name, out func)) {
                    func.CastArguments(args, name, f);

                    switch (func.id) {
                    case FuncId.Current             :
                        if (!allowCurrent) {
                            throw new XslLoadException(Res.Xslt_CurrentNotAllowed);
                        }
                        // NOTE: This is the only place where the current node (and not the context node) must be used
                        return ((IXPathEnvironment) this).GetCurrent();
                    case FuncId.Key                 :
                        if (!allowKey) {
                            throw new XslLoadException(Res.Xslt_KeyNotAllowed);
                        }
                        return CompileFnKey(args[0], args[1], env);
                    case FuncId.Document            : return CompileFnDocument(args[0], args.Count > 1 ? args[1] : null);
                    case FuncId.FormatNumber        : return CompileFormatNumber(args[0], args[1], args.Count > 2 ? args[2] : null);
                    case FuncId.UnparsedEntityUri   : return CompileUnparsedEntityUri(args[0]);
                    case FuncId.GenerateId          : return CompileGenerateId(args.Count > 0 ? args[0] : env.GetCurrent());
                    case FuncId.SystemProperty      : return CompileSystemProperty(args[0]);
                    case FuncId.ElementAvailable    : return CompileElementAvailable(args[0]);
                    case FuncId.FunctionAvailable   : return CompileFunctionAvailable(args[0]);
                    default:
                        Debug.Fail(func.id + " is present in the function table, but absent from the switch");
                        return null;
                    }
                } else {
                    throw new XslLoadException(Res.Xslt_UnknownXsltFunction, Compiler.ConstructQName(prefix, name));
                }
            } else {
                string ns = ResolvePrefixThrow(/*ignoreDefaultNs:*/true, prefix);
                Debug.Assert(ns != null);
                if (ns == XmlReservedNs.NsMsxsl) {
                    if (name == "node-set") {
                        FunctionInfo.CheckArity(/*minArg:*/1, /*maxArg:*/1, name, args.Count);
                        return CompileMsNodeSet(args[0]);
                    } else if (name == "string-compare") {
                        FunctionInfo.CheckArity(/*minArg:*/2, /*maxArg:*/4, name, args.Count);
                        return f.InvokeMsStringCompare(
                            /*x:      */f.ConvertToString(args[0]),
                            /*y:      */f.ConvertToString(args[1]),
                            /*lang:   */2 < args.Count ? f.ConvertToString(args[2]) : f.String(string.Empty),
                            /*options:*/3 < args.Count ? f.ConvertToString(args[3]) : f.String(string.Empty)
                        );
                    } else if (name == "utc") {
                        FunctionInfo.CheckArity(/*minArg:*/1, /*maxArg:*/1, name, args.Count);
                        return f.InvokeMsUtc(/*datetime:*/f.ConvertToString(args[0]));
                    } else if (name == "format-date" || name == "format-time") {
                        FunctionInfo.CheckArity(/*minArg:*/1, /*maxArg:*/3, name, args.Count);
                        bool fwdCompat = (xslVersion == XslVersion.ForwardsCompatible);
                        return f.InvokeMsFormatDateTime(
                            /*datetime:*/f.ConvertToString(args[0]),
                            /*format:  */1 < args.Count ? f.ConvertToString(args[1]) : f.String(string.Empty),
                            /*lang:    */2 < args.Count ? f.ConvertToString(args[2]) : f.String(string.Empty),
                            /*isDate:  */f.Boolean(name == "format-date")
                        );
                    } else if (name == "local-name") {
                        FunctionInfo.CheckArity(/*minArg:*/1, /*maxArg:*/1, name, args.Count);
                        return f.InvokeMsLocalName(f.ConvertToString(args[0]));
                    } else if (name == "namespace-uri") {
                        FunctionInfo.CheckArity(/*minArg:*/1, /*maxArg:*/1, name, args.Count);
                        return f.InvokeMsNamespaceUri(f.ConvertToString(args[0]), env.GetCurrent());
                    } else if (name == "number") {
                        FunctionInfo.CheckArity(/*minArg:*/1, /*maxArg:*/1, name, args.Count);
                        return f.InvokeMsNumber(args[0]);
                    }
                }

                if (ns == XmlReservedNs.NsExsltCommon) {
                    if (name == "node-set") {
                        FunctionInfo.CheckArity(/*minArg:*/1, /*maxArg:*/1, name, args.Count);
                        return CompileMsNodeSet(args[0]);
                    } else if (name == "object-type") {
                        FunctionInfo.CheckArity(/*minArg:*/1, /*maxArg:*/1, name, args.Count);
                        return EXslObjectType(args[0]);
                    }
                }

                // NOTE: If you add any function here, add it to IsFunctionAvailable as well

                // Ensure that all node-set parameters are DocOrderDistinct
                for (int i = 0; i < args.Count; i++)
                    args[i] = f.SafeDocOrderDistinct(args[i]);

                if (compiler.Settings.EnableScript) {
                    XmlExtensionFunction scrFunc = compiler.Scripts.ResolveFunction(name, ns, args.Count, (IErrorHelper)this);
                    if (scrFunc != null) {
                        return GenerateScriptCall(f.QName(name, ns, prefix), scrFunc, args);
                    }
                } else {
                    if (compiler.Scripts.ScriptClasses.ContainsKey(ns)) {
                        ReportWarning(Res.Xslt_ScriptsProhibited);
                        return f.Error(lastScope.SourceLine, Res.Xslt_ScriptsProhibited);
                    }
                }

                return f.XsltInvokeLateBound(f.QName(name, ns, prefix), args);
            }
        }

        private QilNode GenerateScriptCall(QilName name, XmlExtensionFunction scrFunc, IList<QilNode> args) {
            XmlQueryType xmlTypeFormalArg;

            for (int i = 0; i < args.Count; i++) {
                xmlTypeFormalArg = scrFunc.GetXmlArgumentType(i);
                switch (xmlTypeFormalArg.TypeCode) {
                    case XmlTypeCode.Boolean:       args[i] = f.ConvertToBoolean(args[i]); break;
                    case XmlTypeCode.Double:        args[i] = f.ConvertToNumber(args[i]); break;
                    case XmlTypeCode.String:        args[i] = f.ConvertToString(args[i]); break;
                    case XmlTypeCode.Node:          args[i] = xmlTypeFormalArg.IsSingleton ? f.ConvertToNode(args[i]) : f.ConvertToNodeSet(args[i]); break;
                    case XmlTypeCode.Item:          break;
                    default: Debug.Fail("This XmlTypeCode should never be inferred from a Clr type: " + xmlTypeFormalArg.TypeCode); break;
                }
            }

            return f.XsltInvokeEarlyBound(name, scrFunc.Method, scrFunc.XmlReturnType, args);
        }

        private string ResolvePrefixThrow(bool ignoreDefaultNs, string prefix) {
            if (ignoreDefaultNs && prefix.Length == 0) {
                return string.Empty;
            } else {
                string ns = scope.LookupNamespace(prefix);
                if (ns == null) {
                    if (prefix.Length != 0) {
                        throw new XslLoadException(Res.Xslt_InvalidPrefix, prefix);
                    }
                    ns = string.Empty;
                }
                return ns;
            }
        }

        //------------------------------------------------
        // XSLT Functions
        //------------------------------------------------

        public enum FuncId {
            Current,
            Document,
            Key,
            FormatNumber,
            UnparsedEntityUri,
            GenerateId,
            SystemProperty,
            ElementAvailable,
            FunctionAvailable,
        }

        private static readonly XmlTypeCode[] argFnDocument     = {XmlTypeCode.Item,   XmlTypeCode.Node};
        private static readonly XmlTypeCode[] argFnKey          = {XmlTypeCode.String, XmlTypeCode.Item};
        private static readonly XmlTypeCode[] argFnFormatNumber = {XmlTypeCode.Double, XmlTypeCode.String, XmlTypeCode.String};

        public  static Dictionary<string, FunctionInfo> FunctionTable = CreateFunctionTable();
        private static Dictionary<string, FunctionInfo> CreateFunctionTable() {
            Dictionary<string, FunctionInfo> table = new Dictionary<string, FunctionInfo>(16);
            table.Add("current"            , new FunctionInfo(FuncId.Current          , 0, 0, null));
            table.Add("document"           , new FunctionInfo(FuncId.Document         , 1, 2, argFnDocument));
            table.Add("key"                , new FunctionInfo(FuncId.Key              , 2, 2, argFnKey));
            table.Add("format-number"      , new FunctionInfo(FuncId.FormatNumber     , 2, 3, argFnFormatNumber));
            table.Add("unparsed-entity-uri", new FunctionInfo(FuncId.UnparsedEntityUri, 1, 1, XPathBuilder.argString));
            table.Add("generate-id"        , new FunctionInfo(FuncId.GenerateId       , 0, 1, XPathBuilder.argNodeSet));
            table.Add("system-property"    , new FunctionInfo(FuncId.SystemProperty   , 1, 1, XPathBuilder.argString));
            table.Add("element-available"  , new FunctionInfo(FuncId.ElementAvailable , 1, 1, XPathBuilder.argString));
            table.Add("function-available" , new FunctionInfo(FuncId.FunctionAvailable, 1, 1, XPathBuilder.argString));
            return table;
        }

        public static bool IsFunctionAvailable(string localName, string nsUri) {
            if (XPathBuilder.IsFunctionAvailable(localName, nsUri)) {
                return true;
            }
            if (nsUri.Length == 0) {
                return FunctionTable.ContainsKey(localName) && localName != "unparsed-entity-uri";
            }
            if (nsUri == XmlReservedNs.NsMsxsl) {
                return (
                    localName == "node-set"       ||
                    localName == "format-date"    ||
                    localName == "format-time"    ||
                    localName == "local-name"     ||
                    localName == "namespace-uri"  ||
                    localName == "number"         ||
                    localName == "string-compare" ||
                    localName == "utc"
                );
            }
            if (nsUri == XmlReservedNs.NsExsltCommon) {
                return localName == "node-set" || localName == "object-type";
            }
            return false;
        }

        public static bool IsElementAvailable(XmlQualifiedName name) {
            if (name.Namespace == XmlReservedNs.NsXslt) {
                string localName = name.Name;
                return (
                    localName == "apply-imports"            ||
                    localName == "apply-templates"          ||
                    localName == "attribute"                ||
                    localName == "call-template"            ||
                    localName == "choose"                   ||
                    localName == "comment"                  ||
                    localName == "copy"                     ||
                    localName == "copy-of"                  ||
                    localName == "element"                  ||
                    localName == "fallback"                 ||
                    localName == "for-each"                 ||
                    localName == "if"                       ||
                    localName == "message"                  ||
                    localName == "number"                   ||
                    localName == "processing-instruction"   ||
                    localName == "text"                     ||
                    localName == "value-of"                 ||
                    localName == "variable"
                );
            }
            // NOTE: msxsl:script is not an "instruction", so we return false for it
            return false;
        }

        private QilNode CompileFnKey(QilNode name, QilNode keys, IFocus env) {
            QilNode result;
            QilIterator i, n, k;
            if (keys.XmlType.IsNode) {
                if (keys.XmlType.IsSingleton) {
                    result = CompileSingleKey(name, f.ConvertToString(keys), env);
                } else {
                    result = f.Loop(i = f.For(keys), CompileSingleKey(name, f.ConvertToString(i), env));
                }
            }
            else if (keys.XmlType.IsAtomicValue) {
                result = CompileSingleKey(name, f.ConvertToString(keys), env);
            }
            else {
                result = f.Loop(n = f.Let(name), f.Loop(k = f.Let(keys),
                    f.Conditional(f.Not(f.IsType(k, T.AnyAtomicType)),
                        f.Loop(i = f.For(f.TypeAssert(k, T.NodeS)), CompileSingleKey(n, f.ConvertToString(i), env)),
                        CompileSingleKey(n, f.XsltConvert(k, T.StringX), env)
                    )
                ));
            }
            return f.DocOrderDistinct(result);
        }

        private QilNode CompileSingleKey(QilNode name, QilNode key, IFocus env) {
            Debug.Assert(name.XmlType == T.StringX && key.XmlType == T.StringX);
            QilNode     result;

            if (name.NodeType == QilNodeType.LiteralString) {
                string keyName = (string)(QilLiteral)name;
                string prefix, local, nsUri;

                compiler.ParseQName(keyName, out prefix, out local, new ThrowErrorHelper());
                nsUri = ResolvePrefixThrow(/*ignoreDefaultNs:*/true, prefix);
                QilName qname = f.QName(local, nsUri, prefix);

                if (!compiler.Keys.Contains(qname)) {
                    throw new XslLoadException(Res.Xslt_UndefinedKey, keyName);
                }
                result = CompileSingleKey(compiler.Keys[qname], key, env);
            } else {
                if (generalKey == null) {
                    generalKey = CreateGeneralKeyFunction();
                }
                QilIterator i = f.Let(name);
                QilNode resolvedName = ResolveQNameDynamic(/*ignoreDefaultNs:*/true, i);
                result = f.Invoke(generalKey, f.ActualParameterList(i, resolvedName, key, env.GetCurrent()));
                result = f.Loop(i, result);
            }
            return result;
        }

        private QilNode CompileSingleKey(List<Key> defList, QilNode key, IFocus env) {
            Debug.Assert(defList != null && defList.Count > 0);
            if (defList.Count == 1) {
                return f.Invoke(defList[0].Function, f.ActualParameterList(env.GetCurrent(), key));
            }

            QilIterator i  = f.Let(key);
            QilNode result = f.Sequence();
            foreach (Key keyDef in defList) {
                result.Add(f.Invoke(keyDef.Function, f.ActualParameterList(env.GetCurrent(), i)));
            }
            return f.Loop(i, result);
        }

        private QilNode CompileSingleKey(List<Key> defList, QilIterator key, QilIterator context) {
            Debug.Assert(defList != null && defList.Count > 0);
            QilList result = f.BaseFactory.Sequence();
            QilNode keyRef = null;

            foreach (Key keyDef in defList) {
                keyRef = f.Invoke(keyDef.Function, f.ActualParameterList(context, key));
                result.Add(keyRef);
            }
            return defList.Count == 1 ? keyRef : result;
        }

        private QilFunction CreateGeneralKeyFunction() {
            QilIterator name          = f.Parameter(T.StringX);
            QilIterator resolvedName  = f.Parameter(T.QNameX);
            QilIterator key           = f.Parameter(T.StringX);
            QilIterator context       = f.Parameter(T.NodeNotRtf);

            QilNode fdef = f.Error(Res.Xslt_UndefinedKey, name);
            for (int idx = 0; idx < compiler.Keys.Count; idx++) {
                fdef = f.Conditional(f.Eq(resolvedName, compiler.Keys[idx][0].Name.DeepClone(f.BaseFactory)),
                    CompileSingleKey(compiler.Keys[idx], key, context),
                    fdef
                );
            }

            QilFunction result = f.Function(f.FormalParameterList(name, resolvedName, key, context), fdef, f.False());
            result.DebugName = "key";
            this.functions.Add(result);
            return result;
        }

        private QilNode CompileFnDocument(QilNode uris, QilNode baseNode) {
            QilNode result;
            QilIterator i, j, u;

            if (! compiler.Settings.EnableDocumentFunction) {
                ReportWarning(Res.Xslt_DocumentFuncProhibited);
                return f.Error(lastScope.SourceLine, Res.Xslt_DocumentFuncProhibited);
            }
            if (uris.XmlType.IsNode) {
                result = f.DocOrderDistinct(f.Loop(i = f.For(uris),
                    CompileSingleDocument(f.ConvertToString(i), baseNode ?? i)
                ));
            } else if (uris.XmlType.IsAtomicValue) {
                result = CompileSingleDocument(f.ConvertToString(uris), baseNode);
            } else {
                u = f.Let(uris);
                j = (baseNode != null) ? f.Let(baseNode) : null;
                result = f.Conditional(f.Not(f.IsType(u, T.AnyAtomicType)),
                    f.DocOrderDistinct(f.Loop(i = f.For(f.TypeAssert(u, T.NodeS)),
                        CompileSingleDocument(f.ConvertToString(i), j ?? i)
                    )),
                    CompileSingleDocument(f.XsltConvert(u, T.StringX), j)
                );
                result = (baseNode != null) ? f.Loop(j, result) : result;
                result = f.Loop(u, result);
            }
            return result;
        }

        private QilNode CompileSingleDocument(QilNode uri, QilNode baseNode) {
            f.CheckString(uri);
            QilNode baseUri;

            if (baseNode == null) {
                baseUri = f.String(lastScope.SourceLine.Uri);
            } else {
                f.CheckNodeSet(baseNode);
                if (baseNode.XmlType.IsSingleton) {
                    baseUri = f.InvokeBaseUri(baseNode);
                } else {
                    // According to errata E14, it is an error if the second argument node-set is empty
                    // and the URI reference is relative. We pass an empty string as a baseUri to indicate
                    // that case.
                    QilIterator i;
                    baseUri = f.StrConcat(f.Loop(i = f.FirstNode(baseNode), f.InvokeBaseUri(i)));
                }
            }

            f.CheckString(baseUri);
            return f.DataSource(uri, baseUri);
        }

        private QilNode CompileFormatNumber(QilNode value, QilNode formatPicture, QilNode formatName) {
            f.CheckDouble(value);
            f.CheckString(formatPicture);
            XmlQualifiedName resolvedName;

            if (formatName == null) {
                resolvedName = new XmlQualifiedName();
                // formatName must be non-null in the f.InvokeFormatNumberDynamic() call below
                formatName = f.String(string.Empty);
            } else {
                f.CheckString(formatName);
                if (formatName.NodeType == QilNodeType.LiteralString) {
                    resolvedName = ResolveQNameThrow(/*ignoreDefaultNs:*/true, formatName);
                } else {
                    resolvedName = null;
                }
            }

            if (resolvedName != null) {
                DecimalFormatDecl format;
                if (compiler.DecimalFormats.Contains(resolvedName)) {
                    format = compiler.DecimalFormats[resolvedName];
                } else {
                    if (resolvedName != DecimalFormatDecl.Default.Name) {
                        throw new XslLoadException(Res.Xslt_NoDecimalFormat, (string)(QilLiteral)formatName);
                    }
                    format = DecimalFormatDecl.Default;
                }

                // If both formatPicture and formatName are literal strings, there is no need to reparse
                // formatPicture on every execution of this format-number(). Instead, we create a DecimalFormatter
                // object on the first execution, save its index into a global variable, and reuse that object
                // on all subsequent executions.
                if (formatPicture.NodeType == QilNodeType.LiteralString) {
                    QilIterator fmtIdx = f.Let(f.InvokeRegisterDecimalFormatter(formatPicture, format));
                    fmtIdx.DebugName = f.QName("formatter" + this.formatterCnt++, XmlReservedNs.NsXslDebug).ToString();
                    this.gloVars.Add(fmtIdx);
                    return f.InvokeFormatNumberStatic(value, fmtIdx);
                }

                formatNumberDynamicUsed = true;
                QilNode name = f.QName(resolvedName.Name, resolvedName.Namespace);
                return f.InvokeFormatNumberDynamic(value, formatPicture, name, formatName);
            } else {
                formatNumberDynamicUsed = true;
                QilIterator i = f.Let(formatName);
                QilNode name = ResolveQNameDynamic(/*ignoreDefaultNs:*/true, i);
                return f.Loop(i, f.InvokeFormatNumberDynamic(value, formatPicture, name, i));
            }
        }

        private QilNode CompileUnparsedEntityUri(QilNode n) {
            f.CheckString(n);
            return f.Error(lastScope.SourceLine, Res.Xslt_UnsupportedXsltFunction, "unparsed-entity-uri");
        }

        private QilNode CompileGenerateId(QilNode n) {
            f.CheckNodeSet(n);
            if (n.XmlType.IsSingleton) {
                return f.XsltGenerateId(n);
            } else {
                QilIterator i;
                return f.StrConcat(f.Loop(i = f.FirstNode(n), f.XsltGenerateId(i)));
            }
        }

        private XmlQualifiedName ResolveQNameThrow(bool ignoreDefaultNs, QilNode qilName) {
            string name = (string)(QilLiteral)qilName;
            string prefix, local, nsUri;

            compiler.ParseQName(name, out prefix, out local, new ThrowErrorHelper());
            nsUri = ResolvePrefixThrow(/*ignoreDefaultNs:*/ignoreDefaultNs, prefix);

            return new XmlQualifiedName(local, nsUri);
        }

        private QilNode CompileSystemProperty(QilNode name) {
            f.CheckString(name);
            if (name.NodeType == QilNodeType.LiteralString) {
                XmlQualifiedName qname = ResolveQNameThrow(/*ignoreDefaultNs:*/true, name);
                if (EvaluateFuncCalls) {
                    XPathItem propValue = XsltFunctions.SystemProperty(qname);
                    if (propValue.ValueType == XsltConvert.StringType) {
                        return f.String(propValue.Value);
                    } else {
                        Debug.Assert(propValue.ValueType == XsltConvert.DoubleType);
                        return f.Double((double)propValue.ValueAsDouble);
                    }
                }
                name = f.QName(qname.Name, qname.Namespace);
            } else {
                name = ResolveQNameDynamic(/*ignoreDefaultNs:*/true, name);
            }
            return f.InvokeSystemProperty(name);
        }

        private QilNode CompileElementAvailable(QilNode name) {
            f.CheckString(name);
            if (name.NodeType == QilNodeType.LiteralString) {
                XmlQualifiedName qname = ResolveQNameThrow(/*ignoreDefaultNs:*/false, name);
                if (EvaluateFuncCalls) {
                    return f.Boolean(IsElementAvailable(qname));
                }
                name = f.QName(qname.Name, qname.Namespace);
            } else {
                name = ResolveQNameDynamic(/*ignoreDefaultNs:*/false, name);
            }
            return f.InvokeElementAvailable(name);
        }

        private QilNode CompileFunctionAvailable(QilNode name) {
            f.CheckString(name);
            if (name.NodeType == QilNodeType.LiteralString) {
                XmlQualifiedName qname = ResolveQNameThrow(/*ignoreDefaultNs:*/true, name);
                if (EvaluateFuncCalls) {
                    // Script blocks and extension objects cannot implement neither null nor XSLT namespace
                    if (qname.Namespace.Length == 0 || qname.Namespace == XmlReservedNs.NsXslt) {
                        return f.Boolean(QilGenerator.IsFunctionAvailable(qname.Name, qname.Namespace));
                    }
                    // We might precalculate the result for script namespaces as well
                }
                name = f.QName(qname.Name, qname.Namespace);
            } else {
                name = ResolveQNameDynamic(/*ignoreDefaultNs:*/true, name);
            }
            return f.InvokeFunctionAvailable(name);
        }

        private QilNode CompileMsNodeSet(QilNode n) {
            if (n.XmlType.IsNode && n.XmlType.IsNotRtf) {
                return n;
            }

            return f.XsltConvert(n, T.NodeSDod);
        }

        //------------------------------------------------
        // EXSLT Functions
        //------------------------------------------------

        private QilNode EXslObjectType(QilNode n) {
            if (EvaluateFuncCalls) {
                switch (n.XmlType.TypeCode) {
                case XmlTypeCode.Boolean :  return f.String("boolean");
                case XmlTypeCode.Double  :  return f.String("number");
                case XmlTypeCode.String  :  return f.String("string");
                default:
                    if (n.XmlType.IsNode && n.XmlType.IsNotRtf) {
                        return f.String("node-set");
                    }
                    break;
                }
            }
            return f.InvokeEXslObjectType(n);
        }
    }
}
