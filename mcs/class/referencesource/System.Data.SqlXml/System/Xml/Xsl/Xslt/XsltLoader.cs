//------------------------------------------------------------------------------
// <copyright file="XsltLoader.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------

//#define XSLT2

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.IO;
using System.Xml.XPath;
using System.Xml.Xsl.Qil;
using System.Runtime.Versioning;

namespace System.Xml.Xsl.Xslt {
    using ContextInfo   = XsltInput.ContextInfo;
    using f             = AstFactory;
    using Res           = System.Xml.Utils.Res;
    using TypeFactory   = XmlQueryTypeFactory;
    using QName         = XsltInput.DelayedQName;
    using XsltAttribute = XsltInput.XsltAttribute;

    internal class XsltLoader : IErrorHelper {
        private Compiler                compiler;
        private XmlResolver             xmlResolver;
        private QueryReaderSettings     readerSettings;
        private KeywordsTable           atoms;          // XSLT keywords atomized with QueryReaderSettings.NameTabel
        private XsltInput               input;          // Current input stream
        private Stylesheet              curStylesheet;  // Current stylesheet
        private Template                curTemplate;    // Current template
        private object                  curFunction;    // Current function

        internal static QilName         nullMode        = f.QName(string.Empty);

        // Flags which control attribute versioning
        public static int V1Opt = 1;
        public static int V1Req = 2;
        public static int V2Opt = 4;
        public static int V2Req = 8;

        [ResourceConsumption(ResourceScope.Machine)]
        [ResourceExposure(ResourceScope.Machine)]
        public void Load(Compiler compiler, object stylesheet, XmlResolver xmlResolver) {
            Debug.Assert(compiler != null);
            this.compiler = compiler;
            this.xmlResolver = xmlResolver ?? XmlNullResolver.Singleton;

            XmlReader reader = stylesheet as XmlReader;
            if (reader != null) {
                readerSettings = new QueryReaderSettings(reader);
                Load(reader);
            } else {
                // We should take DefaultReaderSettings from Compiler.Settings.DefaultReaderSettings.

                string uri = stylesheet as string;
                if (uri != null) {
                    // If xmlResolver == null, then the original uri will be resolved using XmlUrlResolver
                    XmlResolver origResolver = xmlResolver;
                    if (xmlResolver == null || xmlResolver == XmlNullResolver.Singleton)
                        origResolver = new XmlUrlResolver();
                    Uri resolvedUri = origResolver.ResolveUri(null, uri);
                    if (resolvedUri == null) {
                        throw new XslLoadException(Res.Xslt_CantResolve, uri);
                    }

                    readerSettings = new QueryReaderSettings(new NameTable());
                    using (reader = CreateReader(resolvedUri, origResolver)) {
                        Load(reader);
                    }
                } else {
                    IXPathNavigable navigable = stylesheet as IXPathNavigable;
                    if (navigable != null) {
                        reader = XPathNavigatorReader.Create(navigable.CreateNavigator());
                        readerSettings = new QueryReaderSettings(reader.NameTable);
                        Load(reader);
                    } else {
                        Debug.Fail("Should never get here");
                    }
                }
            }
            Debug.Assert(compiler.Root != null);
            compiler.StartApplyTemplates = f.ApplyTemplates(nullMode);
            ProcessOutputSettings();
            foreach (AttributeSet attSet in compiler.AttributeSets.Values) {
                CheckAttributeSetsDfs(attSet); // Check attribute sets for circular references using dfs marking method
            }
        }

        private void Load(XmlReader reader) {
            this.atoms = new KeywordsTable(reader.NameTable);
            AtomizeAttributes();
            LoadStylesheet(reader, /*include:*/false);
        }


        void AtomizeAttributes(XsltAttribute[] attributes) {
            for(int i = 0; i < attributes.Length; i ++) {
                attributes[i].name = atoms.NameTable.Add(attributes[i].name);
            }
        }
        void AtomizeAttributes() {
            AtomizeAttributes(stylesheetAttributes);
            AtomizeAttributes(importIncludeAttributes);
            AtomizeAttributes(loadStripSpaceAttributes);
            AtomizeAttributes(outputAttributes);
            AtomizeAttributes(keyAttributes);
            AtomizeAttributes(decimalFormatAttributes);
            AtomizeAttributes(namespaceAliasAttributes);
            AtomizeAttributes(attributeSetAttributes);
            AtomizeAttributes(templateAttributes);
            AtomizeAttributes(scriptAttributes);
            AtomizeAttributes(assemblyAttributes);
            AtomizeAttributes(usingAttributes);
            AtomizeAttributes(applyTemplatesAttributes);
            AtomizeAttributes(callTemplateAttributes);
            AtomizeAttributes(copyAttributes);
            AtomizeAttributes(copyOfAttributes);
            AtomizeAttributes(ifAttributes);
            AtomizeAttributes(forEachAttributes);
            AtomizeAttributes(messageAttributes);
            AtomizeAttributes(numberAttributes);
            AtomizeAttributes(valueOfAttributes);
            AtomizeAttributes(variableAttributes);
            AtomizeAttributes(paramAttributes);
            AtomizeAttributes(withParamAttributes);
            AtomizeAttributes(commentAttributes);
            AtomizeAttributes(processingInstructionAttributes);
            AtomizeAttributes(textAttributes);
            AtomizeAttributes(elementAttributes);
            AtomizeAttributes(attributeAttributes);
            AtomizeAttributes(sortAttributes);
#if XSLT2
            AtomizeAttributes(characterMapAttributes);
            AtomizeAttributes(outputCharacterAttributes);
            AtomizeAttributes(functionAttributes);
            AtomizeAttributes(importSchemaAttributes);
            AtomizeAttributes(documentAttributes);
            AtomizeAttributes(analyzeStringAttributes);
            AtomizeAttributes(namespaceAttributes);
            AtomizeAttributes(performSortAttributes);
            AtomizeAttributes(forEachGroupAttributes);
            AtomizeAttributes(sequenceAttributes);
            AtomizeAttributes(resultDocumentAttributes);
#endif
        }

        private bool V1 { get {
                Debug.Assert(compiler.Version != 0, "Version should be already decided at this point");
                return compiler.Version == 1;
        }}
#if XSLT2
        private bool V2 { get { return ! V1; } }
#endif

        // Import/Include XsltInput management

        private HybridDictionary documentUriInUse = new HybridDictionary();

        [ResourceConsumption(ResourceScope.Machine)]
        [ResourceExposure(ResourceScope.Machine)]        
        private Uri ResolveUri(string relativeUri, string baseUri) {
            Uri resolvedBaseUri = (baseUri.Length != 0) ? xmlResolver.ResolveUri(null, baseUri) : null;
            Uri resolvedUri = xmlResolver.ResolveUri(resolvedBaseUri, relativeUri);
            if (resolvedUri == null) {
                throw new XslLoadException(Res.Xslt_CantResolve, relativeUri);
            }
            return resolvedUri;
        }

        private XmlReader CreateReader(Uri uri, XmlResolver xmlResolver) {
            object input = xmlResolver.GetEntity(uri, null, null);

            Stream stream = input as Stream;
            if (stream != null) {
                return readerSettings.CreateReader(stream, uri.ToString());
            }

            XmlReader reader = input as XmlReader;
            if (reader != null) {
                return reader;
            }

            IXPathNavigable navigable = input as IXPathNavigable;
            if (navigable != null) {
                return XPathNavigatorReader.Create(navigable.CreateNavigator());
            }

            throw new XslLoadException(Res.Xslt_CannotLoadStylesheet, uri.ToString(), input == null ? "null" : input.GetType().ToString());
        }

        private Stylesheet LoadStylesheet(Uri uri, bool include) {
            using (XmlReader reader = CreateReader(uri, this.xmlResolver)) {
                return LoadStylesheet(reader, include);
            }
        }

        private Stylesheet LoadStylesheet(XmlReader reader, bool include) {
            string baseUri = reader.BaseURI;
            Debug.Assert(!documentUriInUse.Contains(baseUri), "Circular references must be checked while processing xsl:include and xsl:import");
            documentUriInUse.Add(baseUri, null);
            compiler.AddModule(baseUri);

            Stylesheet  prevStylesheet  = curStylesheet;
            XsltInput   prevInput       = input;
            Stylesheet  thisStylesheet  = include ? curStylesheet : compiler.CreateStylesheet();

            input = new XsltInput(reader, compiler, atoms);
            curStylesheet = thisStylesheet;

            try {
                LoadDocument();
                if (!include) {
                    compiler.MergeWithStylesheet(curStylesheet);

                    List<Uri> importHrefs = curStylesheet.ImportHrefs;
                    curStylesheet.Imports = new Stylesheet[importHrefs.Count];
                    // Imports should be compiled in the reverse order. Template lookup logic relies on that.
                    for (int i = importHrefs.Count; 0 <= --i; ) {
                        curStylesheet.Imports[i] = LoadStylesheet(importHrefs[i], /*include:*/false);
                    }
                }
            }
            catch (XslLoadException) {
                throw;
            }
            catch (Exception e) {
                if (!XmlException.IsCatchableException(e)) {
                    throw;
                }
                // Note that XmlResolver or XmlReader may throw XmlException with SourceUri == null.
                // In that case we report current line information from XsltInput.
                XmlException ex = e as XmlException;
                ISourceLineInfo lineInfo = (ex != null && ex.SourceUri != null ?
                    new SourceLineInfo(ex.SourceUri, ex.LineNumber, ex.LinePosition, ex.LineNumber, ex.LinePosition) :
                    input.BuildReaderLineInfo()
                );
                throw new XslLoadException(e, lineInfo);
            }
            finally {
                documentUriInUse.Remove(baseUri);
                input         = prevInput;
                curStylesheet = prevStylesheet;
            }
            return thisStylesheet;
        }

        private void LoadDocument() {
            if (!input.FindStylesheetElement()) {
                ReportError(/*[XT_002]*/Res.Xslt_WrongStylesheetElement);
                return;
            }
            Debug.Assert(input.NodeType == XmlNodeType.Element);
            if (input.IsXsltNamespace()) {
                if (
                    input.IsKeyword(atoms.Stylesheet) ||
                    input.IsKeyword(atoms.Transform)
                ) {
                    LoadRealStylesheet();
                } else {
                    ReportError(/*[XT_002]*/Res.Xslt_WrongStylesheetElement);
                    input.SkipNode();
                }
            } else {
                LoadSimplifiedStylesheet();
            }
            input.Finish();
        }

        private void LoadSimplifiedStylesheet() {
            Debug.Assert(!input.IsXsltNamespace());
            Debug.Assert(curTemplate == null);

            // Prefix will be fixed later in LoadLiteralResultElement()
            curTemplate = f.Template(/*name:*/null, /*match:*/"/", /*mode:*/nullMode, /*priority:*/double.NaN, input.XslVersion);

            // This template has mode=null match="/" and no imports
            input.CanHaveApplyImports = true;
            XslNode lre = LoadLiteralResultElement(/*asStylesheet:*/true);
            if (lre != null) {
                SetLineInfo(curTemplate, lre.SourceLine);

                List<XslNode> content = new List<XslNode>();
                content.Add(lre);
                SetContent(curTemplate, content);
                if (!curStylesheet.AddTemplate(curTemplate)) {
                    Debug.Fail("AddTemplate() returned false for simplified stylesheet");
                }
            }
            curTemplate = null;
        }

        XsltAttribute[] stylesheetAttributes = {
            new XsltAttribute("version"               , V1Req | V2Req),
            new XsltAttribute("id"                    , V1Opt | V2Opt),
            new XsltAttribute("default-validation"    ,         V2Opt),
            new XsltAttribute("input-type-annotations",         V2Opt),
        };
        private void LoadRealStylesheet() {
            Debug.Assert(input.IsXsltNamespace() && (input.IsKeyword(atoms.Stylesheet) || input.IsKeyword(atoms.Transform)));
            ContextInfo ctxInfo = input.GetAttributes(stylesheetAttributes);

            ParseValidationAttribute(2, /*defVal:*/true);
            ParseInputTypeAnnotationsAttribute(3);

            QName parentName = input.ElementName;
            if (input.MoveToFirstChild()) {
                bool atTop = true;
                do {
                    bool isImport = false;
                    switch (input.NodeType) {
                    case XmlNodeType.Element:
                        if (input.IsXsltNamespace()) {
                            if (input.IsKeyword(atoms.Import)) {
                                if (!atTop) {
                                    ReportError(/*[XT0200]*/Res.Xslt_NotAtTop, input.QualifiedName, parentName);
                                    input.SkipNode();
                                } else {
                                    isImport = true;
                                    LoadImport();
                                }
                            } else if (input.IsKeyword(atoms.Include)) {
                                LoadInclude();
                            } else if (input.IsKeyword(atoms.StripSpace)) {
                                LoadStripSpace(ctxInfo.nsList);
                            } else if (input.IsKeyword(atoms.PreserveSpace)) {
                                LoadPreserveSpace(ctxInfo.nsList);
                            } else if (input.IsKeyword(atoms.Output)) {
                                LoadOutput();
                            } else if (input.IsKeyword(atoms.Key)) {
                                LoadKey(ctxInfo.nsList);
                            } else if (input.IsKeyword(atoms.DecimalFormat)) {
                                LoadDecimalFormat(ctxInfo.nsList);
                            } else if (input.IsKeyword(atoms.NamespaceAlias)) {
                                LoadNamespaceAlias(ctxInfo.nsList);
                            } else if (input.IsKeyword(atoms.AttributeSet)) {
                                LoadAttributeSet(ctxInfo.nsList);
                            } else if (input.IsKeyword(atoms.Variable)) {
                                LoadGlobalVariableOrParameter(ctxInfo.nsList, XslNodeType.Variable);
                            } else if (input.IsKeyword(atoms.Param)) {
                                LoadGlobalVariableOrParameter(ctxInfo.nsList, XslNodeType.Param);
                            } else if (input.IsKeyword(atoms.Template)) {
                                LoadTemplate(ctxInfo.nsList);
#if XSLT2
                            } else if (V2 && input.IsKeyword(atoms.CharacterMap)) {
                                LoadCharacterMap(ctxInfo.nsList);
                            } else if (V2 && input.IsKeyword(atoms.Function)) {
                                LoadFunction(ctxInfo.nsList);
                            } else if (V2 && input.IsKeyword(atoms.ImportSchema)) {
                                LoadImportSchema();
#endif
                            } else {
                                input.GetVersionAttribute();
                                if (!input.ForwardCompatibility) {
                                    ReportError(/*[XT_003]*/Res.Xslt_UnexpectedElement, input.QualifiedName, parentName);
                                }
                                input.SkipNode();
                            }
                        } else if (input.IsNs(atoms.UrnMsxsl) && input.IsKeyword(atoms.Script)) {
                            LoadMsScript(ctxInfo.nsList);
                        } else {
                            if (input.IsNullNamespace()) {
                                ReportError(/*[XT0130]*/Res.Xslt_NullNsAtTopLevel, input.LocalName);
                            }
                            // Ignoring non-recognized namespace per XSLT spec 2.2
                            input.SkipNode();
                        }
                        atTop = isImport;
                        break;

                    case XmlNodeType.Whitespace:
                    case XmlNodeType.SignificantWhitespace:
                        break;
                    default:
                        Debug.Assert(input.NodeType == XmlNodeType.Text);
                        ReportError(/*[XT0120]*/Res.Xslt_TextNodesNotAllowed, parentName);
                        break;
                    }
                } while (input.MoveToNextSibling());
            }
        }


        XsltAttribute[] importIncludeAttributes = {new XsltAttribute("href" , V1Req | V2Req)};
        // SxS: This method reads resource names from source document and does not expose any resources to the caller.
        // It's OK to suppress the SxS warning.
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        [ResourceExposure(ResourceScope.None)]
        private void LoadImport() {
            ContextInfo ctxInfo = input.GetAttributes(importIncludeAttributes);

            if (input.MoveToXsltAttribute(0, "href")) {
                // Resolve href right away using the current BaseUri (it might change later)
                Uri uri = ResolveUri(input.Value, input.BaseUri);

                // Check for circular references
                if (documentUriInUse.Contains(uri.ToString())) {
                    ReportError(/*[XT0210]*/Res.Xslt_CircularInclude, input.Value);
                } else {
                    curStylesheet.ImportHrefs.Add(uri);
                }
            } else {
                // The error was already reported. Ignore the instruction
            }

            CheckNoContent();
        }

        // SxS: This method reads resource names from source document and does not expose any resources to the caller.
        // It's OK to suppress the SxS warning.
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        [ResourceExposure(ResourceScope.None)]
        private void LoadInclude() {
            ContextInfo ctxInfo = input.GetAttributes(importIncludeAttributes);

            if (input.MoveToXsltAttribute(0, "href")) {

                Uri uri = ResolveUri(input.Value, input.BaseUri);

                // Check for circular references
                if (documentUriInUse.Contains(uri.ToString())) {
                    ReportError(/*[XT0180]*/Res.Xslt_CircularInclude, input.Value);
                } else {
                    LoadStylesheet(uri, /*include:*/ true);
                }
            } else {
                // The error was already reported. Ignore the instruction
            }

            CheckNoContent();
        }

        XsltAttribute[] loadStripSpaceAttributes = {new XsltAttribute("elements" , V1Req | V2Req)};
        private void LoadStripSpace(NsDecl stylesheetNsList) {
            ContextInfo ctxInfo = input.GetAttributes(loadStripSpaceAttributes);
            ctxInfo.nsList = MergeNamespaces(ctxInfo.nsList, stylesheetNsList);

            if (input.MoveToXsltAttribute(0, atoms.Elements)) {
                ParseWhitespaceRules(input.Value, false);
            }
            CheckNoContent();
        }

        private void LoadPreserveSpace(NsDecl stylesheetNsList) {
            ContextInfo ctxInfo = input.GetAttributes(loadStripSpaceAttributes);
            ctxInfo.nsList = MergeNamespaces(ctxInfo.nsList, stylesheetNsList);

            if (input.MoveToXsltAttribute(0, atoms.Elements)) {
                ParseWhitespaceRules(input.Value, true);
            }
            CheckNoContent();
        }

        XsltAttribute[] outputAttributes = {
            new XsltAttribute("name"                  ,         V2Opt),
            new XsltAttribute("method"                , V1Opt | V2Opt),
            new XsltAttribute("byte-order-mark"       ,         V2Opt),
            new XsltAttribute("cdata-section-elements", V1Opt | V2Opt),
            new XsltAttribute("doctype-public"        , V1Opt | V2Opt),
            new XsltAttribute("doctype-system"        , V1Opt | V2Opt),
            new XsltAttribute("encoding"              , V1Opt | V2Opt),
            new XsltAttribute("escape-uri-attributes" ,         V2Opt),
            new XsltAttribute("include-content-type"  ,         V2Opt),
            new XsltAttribute("indent"                , V1Opt | V2Opt),
            new XsltAttribute("media-type"            , V1Opt | V2Opt),
            new XsltAttribute("normalization-form"    ,         V2Opt),
            new XsltAttribute("omit-xml-declaration"  , V1Opt | V2Opt),
            new XsltAttribute("standalone"            , V1Opt | V2Opt),
            new XsltAttribute("undeclare-prefixes"    ,         V2Opt),
            new XsltAttribute("use-character-maps"    ,         V2Opt),
            new XsltAttribute("version"               , V1Opt | V2Opt)
        };
        private void LoadOutput() {
            ContextInfo ctxInfo = input.GetAttributes(outputAttributes);

            Output output = compiler.Output;
            XmlWriterSettings settings = output.Settings;
            int currentPrec = compiler.CurrentPrecedence;
            TriState triState;

            QilName name = ParseQNameAttribute(0);
            if (name != null) ReportNYI("xsl:output/@name");

            if (input.MoveToXsltAttribute(1, "method")) {
                if (output.MethodPrec <= currentPrec) {
                    compiler.EnterForwardsCompatible();
                    XmlOutputMethod outputMethod;
                    XmlQualifiedName method = ParseOutputMethod(input.Value, out outputMethod);
                    if (compiler.ExitForwardsCompatible(input.ForwardCompatibility) && method != null) {
                        if (currentPrec == output.MethodPrec && !output.Method.Equals(method)) {
                            ReportWarning(/*[XT1560]*/Res.Xslt_AttributeRedefinition, "method");
                        }
                        settings.OutputMethod = outputMethod;
                        output.Method = method;
                        output.MethodPrec = currentPrec;
                    }
                }
            }

            TriState byteOrderMask = ParseYesNoAttribute(2, "byte-order-mark");
            if (byteOrderMask != TriState.Unknown) ReportNYI("xsl:output/@byte-order-mark");

            if (input.MoveToXsltAttribute(3, "cdata-section-elements")) {
                // Do not check the import precedence, the effective value is the union of all specified values
                compiler.EnterForwardsCompatible();
                string[] qnames = XmlConvert.SplitString(input.Value);
                List<XmlQualifiedName> list = new List<XmlQualifiedName>();
                for (int i = 0; i < qnames.Length; i++) {
                    list.Add(ResolveQName(/*ignoreDefaultNs:*/false, qnames[i]));
                }
                if (compiler.ExitForwardsCompatible(input.ForwardCompatibility)) {
                    settings.CDataSectionElements.AddRange(list);
                }
            }

            if (input.MoveToXsltAttribute(4, "doctype-public")) {
                if (output.DocTypePublicPrec <= currentPrec) {
                    if (currentPrec == output.DocTypePublicPrec && settings.DocTypePublic != input.Value) {
                        ReportWarning(/*[XT1560]*/Res.Xslt_AttributeRedefinition, "doctype-public");
                    }
                    settings.DocTypePublic = input.Value;
                    output.DocTypePublicPrec = currentPrec;
                }
            }

            if (input.MoveToXsltAttribute(5, "doctype-system")) {
                if (output.DocTypeSystemPrec <= currentPrec) {
                    if (currentPrec == output.DocTypeSystemPrec && settings.DocTypeSystem != input.Value) {
                        ReportWarning(/*[XT1560]*/Res.Xslt_AttributeRedefinition, "doctype-system");
                    }
                    settings.DocTypeSystem = input.Value;
                    output.DocTypeSystemPrec = currentPrec;
                }
            }

            if (input.MoveToXsltAttribute(6, "encoding")) {
                if (output.EncodingPrec <= currentPrec) {
                    try {
                        // Encoding.GetEncoding() should never throw NotSupportedException, only ArgumentException
                        Encoding encoding = Encoding.GetEncoding(input.Value);
                        if (currentPrec == output.EncodingPrec && output.Encoding != input.Value) {
                            ReportWarning(/*[XT1560]*/Res.Xslt_AttributeRedefinition, "encoding");
                        }
                        settings.Encoding = encoding;
                        output.Encoding = input.Value;
                        output.EncodingPrec = currentPrec;
                    } catch (ArgumentException) {
                        if (!input.ForwardCompatibility) {
                            ReportWarning(/*[XT_004]*/Res.Xslt_InvalidEncoding, input.Value);
                        }
                    }
                }
            }

            bool escapeUriAttributes = ParseYesNoAttribute(7, "escape-uri-attributes") != TriState.False;
            if (! escapeUriAttributes) ReportNYI("xsl:output/@escape-uri-attributes == flase()");

            bool includeContentType = ParseYesNoAttribute(8, "include-content-type") != TriState.False;
            if (!includeContentType) ReportNYI("xsl:output/@include-content-type == flase()");

            triState = ParseYesNoAttribute(9, "indent");
            if (triState != TriState.Unknown) {
                if (output.IndentPrec <= currentPrec) {
                    bool indent = (triState == TriState.True);
                    if (currentPrec == output.IndentPrec && settings.Indent != indent) {
                        ReportWarning(/*[XT1560]*/Res.Xslt_AttributeRedefinition, "indent");
                    }
                    settings.Indent = indent;
                    output.IndentPrec = currentPrec;
                }
            }

            if (input.MoveToXsltAttribute(10, "media-type")) {
                if (output.MediaTypePrec <= currentPrec) {
                    if (currentPrec == output.MediaTypePrec && settings.MediaType != input.Value) {
                        ReportWarning(/*[XT1560]*/Res.Xslt_AttributeRedefinition, "media-type");
                    }
                    settings.MediaType = input.Value;
                    output.MediaTypePrec = currentPrec;
                }
            }

            if (input.MoveToXsltAttribute(11, "normalization-form")) {
                ReportNYI("xsl:output/@normalization-form");
            }

            triState = ParseYesNoAttribute(12, "omit-xml-declaration");
            if (triState != TriState.Unknown) {
                if (output.OmitXmlDeclarationPrec <= currentPrec) {
                    bool omitXmlDeclaration = (triState == TriState.True);
                    if (currentPrec == output.OmitXmlDeclarationPrec && settings.OmitXmlDeclaration != omitXmlDeclaration) {
                        ReportWarning(/*[XT1560]*/Res.Xslt_AttributeRedefinition, "omit-xml-declaration");
                    }
                    settings.OmitXmlDeclaration = omitXmlDeclaration;
                    output.OmitXmlDeclarationPrec = currentPrec;
                }
            }

            triState = ParseYesNoAttribute(13, "standalone");
            if (triState != TriState.Unknown) {
                if (output.StandalonePrec <= currentPrec) {
                    XmlStandalone standalone = (triState == TriState.True) ? XmlStandalone.Yes : XmlStandalone.No;
                    if (currentPrec == output.StandalonePrec && settings.Standalone != standalone) {
                        ReportWarning(/*[XT1560]*/Res.Xslt_AttributeRedefinition, "standalone");
                    }
                    settings.Standalone = standalone;
                    output.StandalonePrec = currentPrec;
                }
            }

            bool undeclarePrefixes = ParseYesNoAttribute(14, "undeclare-prefixes") == TriState.True;
            if (undeclarePrefixes) ReportNYI("xsl:output/@undeclare-prefixes == true()");

            List<QilName> useCharacterMaps = ParseUseCharacterMaps(15);
            if (useCharacterMaps.Count != 0) ReportNYI("xsl:output/@use-character-maps");

            if (input.MoveToXsltAttribute(16, "version")) {
                if (output.VersionPrec <= currentPrec) {
                    if (currentPrec == output.VersionPrec && output.Version != input.Value) {
                        ReportWarning(/*[XT1560]*/Res.Xslt_AttributeRedefinition, "version");
                    }
                    // 

                    output.Version = input.Value;
                    output.VersionPrec = currentPrec;
                }
            }

            CheckNoContent();
        }

        /*
            Default values for method="xml" :   version="1.0"   indent="no"     media-type="text/xml"
            Default values for method="html":   version="4.0"   indent="yes"    media-type="text/html"
            Default values for method="text":                                   media-type="text/plain"
        */
        private void ProcessOutputSettings() {
            Output output = compiler.Output;
            XmlWriterSettings settings = output.Settings;

            // version is ignored, indent="no" by default
            if (settings.OutputMethod == XmlOutputMethod.Html && output.IndentPrec == Output.NeverDeclaredPrec) {
                settings.Indent = true;
            }
            if (output.MediaTypePrec == Output.NeverDeclaredPrec) {
                settings.MediaType =
                    settings.OutputMethod == XmlOutputMethod.Xml  ? "text/xml" :
                    settings.OutputMethod == XmlOutputMethod.Html ? "text/html" :
                    settings.OutputMethod == XmlOutputMethod.Text ? "text/plain" : null;
            }
        }

        private void CheckUseAttrubuteSetInList(IList<XslNode> list) {
            foreach (XslNode xslNode in list) {
                switch (xslNode.NodeType) {
                case XslNodeType.UseAttributeSet:
                    AttributeSet usedAttSet;
                    if (compiler.AttributeSets.TryGetValue(xslNode.Name, out usedAttSet)) {
                        CheckAttributeSetsDfs(usedAttSet);
                    } else {
                        // The error will be reported in QilGenerator while compiling this attribute set.
                    }
                    break;
                case XslNodeType.List:
                    CheckUseAttrubuteSetInList(xslNode.Content);
                    break;
                }
            }
        }

        private void CheckAttributeSetsDfs(AttributeSet attSet) {
            Debug.Assert(attSet != null);
            switch (attSet.CycleCheck) {
            case CycleCheck.NotStarted:
                attSet.CycleCheck = CycleCheck.Processing;
                CheckUseAttrubuteSetInList(attSet.Content);
                attSet.CycleCheck = CycleCheck.Completed;
                break;
            case CycleCheck.Completed:
                break;
            default:
                Debug.Assert(attSet.CycleCheck == CycleCheck.Processing);
                Debug.Assert(attSet.Content[0].SourceLine != null);
                compiler.ReportError(/*[XT0720]*/attSet.Content[0].SourceLine, Res.Xslt_CircularAttributeSet, attSet.Name.QualifiedName);
                break;
            }
        }

        XsltAttribute[] keyAttributes = {
            new XsltAttribute("name"     , V1Req | V2Req),
            new XsltAttribute("match"    , V1Req | V2Req),
            new XsltAttribute("use"      , V1Req | V2Opt),
            new XsltAttribute("collation",         V2Opt)
        };
        private void LoadKey(NsDecl stylesheetNsList) {
            ContextInfo ctxInfo = input.GetAttributes(keyAttributes);
            ctxInfo.nsList = MergeNamespaces(ctxInfo.nsList, stylesheetNsList);

            QilName keyName   = ParseQNameAttribute(    0);
            string  match     = ParseStringAttribute(   1, "match");
            string  use       = ParseStringAttribute(   2, "use");
            string  collation = ParseCollationAttribute(3);

            input.MoveToElement();

            List<XslNode> content = null;

            if (V1) {
                if (use == null) {
                    input.SkipNode();
                } else {
                    CheckNoContent();
                }
            } else {
                content = LoadInstructions();
                // Load the end tag only if the content is not empty
                if (content.Count != 0) {
                    content = LoadEndTag(content);
                }
                if ((use == null) == (content.Count == 0)) {
                    ReportError(/*[XTSE1205]*/Res.Xslt_KeyCntUse);
                } else {
                    if (use == null) ReportNYI("xsl:key[count(@use) = 0]");
                }
            }

            Key key = (Key)SetInfo(f.Key(keyName, match, use, input.XslVersion), null, ctxInfo);

            if (compiler.Keys.Contains(keyName)) {
                // Add to the list of previous definitions
                compiler.Keys[keyName].Add(key);
            } else {
                // First definition of key with that name
                List<Key> defList = new List<Key>();
                defList.Add(key);
                compiler.Keys.Add(defList);
            }
        }

        XsltAttribute[] decimalFormatAttributes = {
            new XsltAttribute("name"              , V1Opt | V2Opt),
            new XsltAttribute("infinity"          , V1Opt | V2Opt),
            new XsltAttribute("NaN"               , V1Opt | V2Opt),
            new XsltAttribute("decimal-separator" , V1Opt | V2Opt),
            new XsltAttribute("grouping-separator", V1Opt | V2Opt),
            new XsltAttribute("percent"           , V1Opt | V2Opt),
            new XsltAttribute("per-mille"         , V1Opt | V2Opt),
            new XsltAttribute("zero-digit"        , V1Opt | V2Opt),
            new XsltAttribute("digit"             , V1Opt | V2Opt),
            new XsltAttribute("pattern-separator" , V1Opt | V2Opt),
            new XsltAttribute("minus-sign"        , V1Opt | V2Opt)
        };
        private void LoadDecimalFormat(NsDecl stylesheetNsList) {
            const int NumCharAttrs = 8, NumSignAttrs = 7;
            ContextInfo ctxInfo = input.GetAttributes(decimalFormatAttributes);
            ctxInfo.nsList = MergeNamespaces(ctxInfo.nsList, stylesheetNsList);

            XmlQualifiedName name;
            if (input.MoveToXsltAttribute(0, "name")) {
                compiler.EnterForwardsCompatible();
                name = ResolveQName(/*ignoreDefaultNs:*/true, input.Value);
                if (!compiler.ExitForwardsCompatible(input.ForwardCompatibility)) {
                    name = new XmlQualifiedName();
                }
            } else {
                // Use name="" for the default decimal-format
                name = new XmlQualifiedName();
            }

            string infinity = DecimalFormatDecl.Default.InfinitySymbol;
            if (input.MoveToXsltAttribute(1, "infinity")) {
                infinity = input.Value;
            }

            string nan = DecimalFormatDecl.Default.NanSymbol;
            if (input.MoveToXsltAttribute(2, "NaN")) {
                nan = input.Value;
            }

            char[] DefaultValues = DecimalFormatDecl.Default.Characters;
            char[] characters = new char[NumCharAttrs];
            Debug.Assert(NumCharAttrs == DefaultValues.Length);

            for (int idx = 0; idx < NumCharAttrs; idx++) {
                characters[idx] = ParseCharAttribute(3 + idx, decimalFormatAttributes[3 + idx].name, DefaultValues[idx]);
            }

            // Check all NumSignAttrs signs are distinct
            for (int i = 0; i < NumSignAttrs; i++) {
                for (int j = i+1; j < NumSignAttrs; j++) {
                    if (characters[i] == characters[j]) {
                        // Try move to second attribute and if it is missing to first.
                        bool dummy = input.MoveToXsltAttribute(3 + j, decimalFormatAttributes[3 + j].name) || input.MoveToXsltAttribute(3 + i, decimalFormatAttributes[3 + i].name);
                        Debug.Assert(dummy, "One of the atts should have lineInfo. if both are defualt they can't conflict.");
                        ReportError(/*[XT1300]*/Res.Xslt_DecimalFormatSignsNotDistinct, decimalFormatAttributes[3 + i].name, decimalFormatAttributes[3 + j].name);
                        break;
                    }
                }
            }

            if (compiler.DecimalFormats.Contains(name)) {
                // Check all attributes have the same values
                DecimalFormatDecl format = compiler.DecimalFormats[name];
                input.MoveToXsltAttribute(1, "infinity");
                CheckError(infinity != format.InfinitySymbol, /*[XT1290]*/Res.Xslt_DecimalFormatRedefined, "infinity", infinity);
                input.MoveToXsltAttribute(2, "NaN");
                CheckError(nan != format.NanSymbol, /*[XT1290]*/Res.Xslt_DecimalFormatRedefined, "NaN", nan);
                for (int idx = 0; idx < NumCharAttrs; idx++) {
                    input.MoveToXsltAttribute(3 + idx, decimalFormatAttributes[3 + idx].name);
                    CheckError(characters[idx] != format.Characters[idx], /*[XT1290]*/Res.Xslt_DecimalFormatRedefined, decimalFormatAttributes[3 + idx].name, char.ToString(characters[idx]));
                }
                Debug.Assert(name.Equals(format.Name));
            } else {
                // Add format to the global collection
                DecimalFormatDecl format = new DecimalFormatDecl(name, infinity, nan, new string(characters));
                compiler.DecimalFormats.Add(format);
            }
            CheckNoContent();
        }

        XsltAttribute[] namespaceAliasAttributes = {
            new XsltAttribute("stylesheet-prefix", V1Req | V2Req),
            new XsltAttribute("result-prefix"    , V1Req | V2Req)
        };
        private void LoadNamespaceAlias(NsDecl stylesheetNsList) {
            ContextInfo ctxInfo = input.GetAttributes(namespaceAliasAttributes);
            ctxInfo.nsList = MergeNamespaces(ctxInfo.nsList, stylesheetNsList);

            string stylesheetNsUri  = null;
            string resultPrefix = null;
            string resultNsUri  = null;

            if (input.MoveToXsltAttribute(0, "stylesheet-prefix")) {
                if (input.Value.Length == 0) {
                    ReportError(/*[XT_005]*/Res.Xslt_EmptyNsAlias, "stylesheet-prefix");
                } else {
                    stylesheetNsUri = input.LookupXmlNamespace(input.Value == "#default" ? string.Empty : input.Value);
                }
            }

            if (input.MoveToXsltAttribute(1, "result-prefix")) {
                if (input.Value.Length == 0) {
                    ReportError(/*[XT_005]*/Res.Xslt_EmptyNsAlias, "result-prefix");
                } else {
                    resultPrefix = input.Value == "#default" ? string.Empty : input.Value;
                    resultNsUri = input.LookupXmlNamespace(resultPrefix);
                }
            }

            CheckNoContent();

            if (stylesheetNsUri == null || resultNsUri == null) {
                // At least one of attributes is missing or invalid
                return;
            }
            if (compiler.SetNsAlias(stylesheetNsUri, resultNsUri, resultPrefix, curStylesheet.ImportPrecedence)) {
                // Namespace alias redefinition
                input.MoveToElement();
                ReportWarning(/*[XT0810]*/Res.Xslt_DupNsAlias, stylesheetNsUri);
            }
        }

        XsltAttribute[] attributeSetAttributes = {
            new XsltAttribute("name"            , V1Req | V2Req),
            new XsltAttribute("use-attribute-sets", V1Opt | V2Opt)
        };
        private void LoadAttributeSet(NsDecl stylesheetNsList) {
            ContextInfo ctxInfo = input.GetAttributes(attributeSetAttributes);
            ctxInfo.nsList = MergeNamespaces(ctxInfo.nsList, stylesheetNsList);

            QilName setName = ParseQNameAttribute(0);
            Debug.Assert(setName != null, "Required attribute always != null");

            AttributeSet set;
            if (!curStylesheet.AttributeSets.TryGetValue(setName, out set)) {
                set = f.AttributeSet(setName);
                // First definition for setName within this stylesheet
                curStylesheet.AttributeSets[setName] = set;
                if (!compiler.AttributeSets.ContainsKey(setName)) {
                    // First definition for setName overall, adding it to the list here
                    // to ensure stable order of prototemplate functions in QilExpression
                    compiler.AllTemplates.Add(set);
                }
            }

            List<XslNode> content = new List<XslNode>();
            if (input.MoveToXsltAttribute(1, "use-attribute-sets")) {
                AddUseAttributeSets(content);
            }

            QName parentName = input.ElementName;
            if (input.MoveToFirstChild()) {
                do {
                    switch (input.NodeType) {
                    case XmlNodeType.Element:
                        // Only xsl:attribute's are allowed here
                        if (input.IsXsltKeyword(atoms.Attribute)) {
                            AddInstruction(content, XslAttribute());
                        } else {
                            ReportError(/*[XT_006]*/Res.Xslt_UnexpectedElement, input.QualifiedName, parentName);
                            input.SkipNode();
                        }
                        break;
                    case XmlNodeType.Whitespace:
                    case XmlNodeType.SignificantWhitespace:
                        break;
                    default:
                        Debug.Assert(input.NodeType == XmlNodeType.Text);
                        ReportError(/*[XT_006]*/Res.Xslt_TextNodesNotAllowed, parentName);
                        break;
                    }
                } while (input.MoveToNextSibling());
            }
            set.AddContent(SetInfo(f.List(), LoadEndTag(content), ctxInfo));
        }

        private void LoadGlobalVariableOrParameter(NsDecl stylesheetNsList, XslNodeType nodeType) {
            Debug.Assert(curTemplate == null);
            Debug.Assert(input.CanHaveApplyImports == false);
            VarPar var = XslVarPar();
            // Preserving namespaces to parse content later
            var.Namespaces = MergeNamespaces(var.Namespaces, stylesheetNsList);
            CheckError(!curStylesheet.AddVarPar(var), /*[XT0630]*/Res.Xslt_DupGlobalVariable, var.Name.QualifiedName);
        }

        //: http://www.w3.org/TR/xslt#section-Defining-Template-Rules
        XsltAttribute[] templateAttributes = {
            new XsltAttribute("match"   , V1Opt | V2Opt),
            new XsltAttribute("name"    , V1Opt | V2Opt),
            new XsltAttribute("priority", V1Opt | V2Opt),
            new XsltAttribute("mode"    , V1Opt | V2Opt),
            new XsltAttribute("as"      ,         V2Opt)
        };
        private void LoadTemplate(NsDecl stylesheetNsList) {
            Debug.Assert(curTemplate == null);
            ContextInfo ctxInfo = input.GetAttributes(templateAttributes);
            ctxInfo.nsList = MergeNamespaces(ctxInfo.nsList, stylesheetNsList);

            string match = ParseStringAttribute(0, "match");
            QilName name = ParseQNameAttribute(1);
            double priority = double.NaN;
            if (input.MoveToXsltAttribute(2, "priority")) {
                priority = XPathConvert.StringToDouble(input.Value);
                if (double.IsNaN(priority) && !input.ForwardCompatibility) {
                    ReportError(/*[XT0530]*/Res.Xslt_InvalidAttrValue, "priority", input.Value);
                }
            }
            QilName mode = V1 ? ParseModeAttribute(3) : ParseModeListAttribute(3);

            if (match == null) {
                CheckError(! input.AttributeExists(1, "name"), /*[XT_007]*/Res.Xslt_BothMatchNameAbsent);
                CheckError(  input.AttributeExists(3, "mode"), /*[XT_008]*/Res.Xslt_ModeWithoutMatch   );
                mode = nullMode;
                if (input.AttributeExists(2, "priority")) {
                    if (V1) {
                        ReportWarning(/*[XT_008]*/Res.Xslt_PriorityWithoutMatch);
                    } else {
                        ReportError  (/*[XT_008]*/Res.Xslt_PriorityWithoutMatch);
                    }
                }
            }

            if (input.MoveToXsltAttribute(4, "as")) {
                ReportNYI("xsl:template/@as");
            }

            curTemplate = f.Template(name, match, mode, priority, input.XslVersion);

            // Template without match considered to not have mode and can't call xsl:apply-imports
            input.CanHaveApplyImports = (match != null);

            SetInfo(curTemplate,
                LoadEndTag(LoadInstructions(InstructionFlags.AllowParam)), ctxInfo
            );

            if (!curStylesheet.AddTemplate(curTemplate)) {
                ReportError(/*[XT0660]*/Res.Xslt_DupTemplateName, curTemplate.Name.QualifiedName);
            }
            curTemplate = null;
        }

#if XSLT2
        //: http://www.w3.org/TR/xslt20/#element-character-map
        XsltAttribute[] characterMapAttributes = {
            new XsltAttribute("name"            , V2Req),
            new XsltAttribute("use-character-maps", V2Opt)
        };
        XsltAttribute[] outputCharacterAttributes = {
            new XsltAttribute("character", V2Req),
            new XsltAttribute("string"   , V2Req)
        };
        private void LoadCharacterMap(NsDecl stylesheetNsList) {
            ContextInfo ctxInfo = input.GetAttributes(characterMapAttributes);
            ctxInfo.nsList = MergeNamespaces(ctxInfo.nsList, stylesheetNsList);

            QilName name = ParseQNameAttribute(0);
            List<QilName> useCharacterMaps = ParseUseCharacterMaps(1);

            ReportNYI("xsl:character-map");

            QName parentName = input.ElementName;
            if (input.MoveToFirstChild()) {
                do {
                    switch (input.NodeType) {
                    case XmlNodeType.Element:
                        // Only xsl:output-character are allowed here
                        if (input.IsXsltKeyword(atoms.OutputCharacter)) {
                            input.GetAttributes(outputCharacterAttributes);
                            ReportNYI("xsl:output-character");
                            char ch  = ParseCharAttribute(0, "character", /*defVal:*/(char)0);
                            string s = ParseStringAttribute(1, "string");
                            CheckNoContent();
                        } else {
                            ReportError(/*[XT_006]*/Res.Xslt_UnexpectedElement, input.QualifiedName, parentName);
                            input.SkipNode();
                        }
                        break;
                    case XmlNodeType.Whitespace:
                    case XmlNodeType.SignificantWhitespace:
                        break;
                    default:
                        Debug.Assert(input.NodeType == XmlNodeType.Text);
                        ReportError(/*[XT_006]*/Res.Xslt_TextNodesNotAllowed, parentName);
                        break;
                    }
                } while (input.MoveToNextSibling());
            }
        }

        //: http://www.w3.org/TR/xslt20/#stylesheet-functions
        XsltAttribute[] functionAttributes = {
            new XsltAttribute("name"    , V2Req),
            new XsltAttribute("as"      , V2Opt),
            new XsltAttribute("override", V2Opt)
        };
        private void LoadFunction(NsDecl stylesheetNsList) {
            Debug.Assert(curTemplate == null);
            ContextInfo ctxInfo = input.GetAttributes(functionAttributes);
            ctxInfo.nsList = MergeNamespaces(ctxInfo.nsList, stylesheetNsList);

            QilName name  = ParseQNameAttribute(0);
            string asType = ParseStringAttribute(1, "as");
            bool over     = ParseYesNoAttribute(2, "override") == TriState.True;

            ReportNYI("xsl:function");

            Debug.Assert(input.CanHaveApplyImports == false);

            curFunction = new Object();
            LoadInstructions(InstructionFlags.AllowParam);
            curFunction = null;
        }

        //: http://www.w3.org/TR/xslt20/#element-import-schema
        XsltAttribute[] importSchemaAttributes = {
            new XsltAttribute("namespace"     , V2Opt),
            new XsltAttribute("schema-location", V2Opt)
        };
        private void LoadImportSchema() {
            ContextInfo ctxInfo = input.GetAttributes(importSchemaAttributes);
            ReportError(/*[XTSE1650]*/Res.Xslt_SchemaDeclaration, input.ElementName);
            input.SkipNode();
        }
#endif

        XsltAttribute[] scriptAttributes = {
            new XsltAttribute("implements-prefix", V1Req | V2Req),
            new XsltAttribute("language"         , V1Opt | V2Opt)
        };
        private void LoadMsScript(NsDecl stylesheetNsList) {
            ContextInfo ctxInfo = input.GetAttributes(scriptAttributes);
            ctxInfo.nsList = MergeNamespaces(ctxInfo.nsList, stylesheetNsList);


            string scriptNs = null;
            if (input.MoveToXsltAttribute(0, "implements-prefix")) {
                if (input.Value.Length == 0) {
                    ReportError(/*[XT_009]*/Res.Xslt_EmptyAttrValue, "implements-prefix", input.Value);
                } else {
                    scriptNs = input.LookupXmlNamespace(input.Value);
                    if (scriptNs == XmlReservedNs.NsXslt) {
                        ReportError(/*[XT_036]*/Res.Xslt_ScriptXsltNamespace);
                        scriptNs = null;
                    }
                }
            }

            if (scriptNs == null) {
                scriptNs = compiler.CreatePhantomNamespace();
            }
            string language = ParseStringAttribute(1, "language");
            if (language == null) {
                language = "jscript";
            }

            if (! compiler.Settings.EnableScript) {
                compiler.Scripts.ScriptClasses[scriptNs] = null;
                input.SkipNode();
                return;
            }

            ScriptClass     scriptClass;
            StringBuilder   scriptCode  = new StringBuilder();
            string          uriString   = input.Uri;
            int             lineNumber  = 0;
            int             lastEndLine = 0;

            scriptClass = compiler.Scripts.GetScriptClass(scriptNs, language, (IErrorHelper)this);
            if (scriptClass == null) {
                input.SkipNode();
                return;
            }

            QName parentName = input.ElementName;
            if (input.MoveToFirstChild()) {
                do {
                    switch (input.NodeType) {
                    case XmlNodeType.Text:
                        int startLine = input.Start.Line;
                        int endLine   = input.End.Line;
                        if (scriptCode.Length == 0) {
                            lineNumber = startLine;
                        } else if (lastEndLine < startLine) {
                            // A multiline comment, a PI, or an unrecognized element encountered within
                            // this script block. Insert missed '\n' characters here; otherwise line numbers
                            // in error messages and in the debugger will be ----ed up. This action may spoil
                            // the script if the current position is situated in the middle of some identifier
                            // or string literal; however we hope users will not put XML nodes there.
                            scriptCode.Append('\n', startLine - lastEndLine);
                        }
                        scriptCode.Append(input.Value);
                        lastEndLine = endLine;
                        break;
                    case XmlNodeType.Element:
                        if (input.IsNs(atoms.UrnMsxsl) && (input.IsKeyword(atoms.Assembly) || input.IsKeyword(atoms.Using))) {
                            if (scriptCode.Length != 0) {
                                ReportError(/*[XT_012]*/Res.Xslt_ScriptNotAtTop, input.QualifiedName);
                                input.SkipNode();
                            } else if (input.IsKeyword(atoms.Assembly)) {
                                LoadMsAssembly(scriptClass);
                            } else if (input.IsKeyword(atoms.Using)) {
                                LoadMsUsing(scriptClass);
                            }
                        } else {
                            ReportError(/*[XT_012]*/Res.Xslt_UnexpectedElement, input.QualifiedName, parentName);
                            input.SkipNode();
                        }
                        break;
                    default:
                        Debug.Assert(
                            input.NodeType == XmlNodeType.SignificantWhitespace ||
                            input.NodeType == XmlNodeType.Whitespace
                        );
                        // Skip leading whitespaces
                        if (scriptCode.Length != 0) {
                            goto case XmlNodeType.Text;
                        }
                        break;
                    }
                } while (input.MoveToNextSibling());
            }

            if (scriptCode.Length == 0) {
                lineNumber = input.Start.Line;
            }
            scriptClass.AddScriptBlock(scriptCode.ToString(), uriString, lineNumber, input.Start);
        }

        XsltAttribute[] assemblyAttributes = {
            new XsltAttribute("name", V1Opt | V2Opt),
            new XsltAttribute("href", V1Opt | V2Opt)
        };
        // SxS: This method reads resource names from source document and does not expose any resources to the caller.
        // It's OK to suppress the SxS warning.
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        [ResourceExposure(ResourceScope.None)]
        private void LoadMsAssembly(ScriptClass scriptClass) {
            input.GetAttributes(assemblyAttributes);

            string name = ParseStringAttribute(0, "name");
            string href = ParseStringAttribute(1, "href");

            if ((name != null) == (href != null)) {
                ReportError(/*[XT_046]*/Res.Xslt_AssemblyNameHref);
            } else {
                string asmLocation = null;
                if (name != null) {
                    try {
                        asmLocation = Assembly.Load(name).Location;
                    }
                    catch {
                        AssemblyName asmName = new AssemblyName(name);

                        // If the assembly is simply named, let CodeDomProvider and Fusion resolve it
                        byte[] publicKeyToken = asmName.GetPublicKeyToken();
                        if ((publicKeyToken == null || publicKeyToken.Length == 0) && asmName.Version == null) {
                            asmLocation = asmName.Name + ".dll";
                        } else {
                            throw;
                        }
                    }
                } else {
                    Debug.Assert(href != null);
                    asmLocation = Assembly.LoadFrom(ResolveUri(href, input.BaseUri).ToString()).Location;
                    scriptClass.refAssembliesByHref = true;
                }

                if (asmLocation != null) {
                    scriptClass.refAssemblies.Add(asmLocation);
                }
            }

            CheckNoContent();
        }

        XsltAttribute[] usingAttributes = {
            new XsltAttribute("namespace", V1Req | V2Req)
        };
        private void LoadMsUsing(ScriptClass scriptClass) {
            input.GetAttributes(usingAttributes);

            if (input.MoveToXsltAttribute(0, "namespace")) {
                scriptClass.nsImports.Add(input.Value);
            }
            CheckNoContent();
        }

        // ----------------- Template level methods --------------------------
        // Each instruction in AST tree has nsdecl list attuched to it.
        // Load*() methods do this treek. Xsl*() methods rely on LoadOneInstruction() to do this.
        // ToDo: check how LoadUnknown*() follows this gideline!

        private enum InstructionFlags {
            None          = 0x00,
            AllowParam    = 0x01,
            AllowSort     = 0x02,
            AllowFallback = 0x04,
        }

        private List<XslNode> LoadInstructions() {
            return LoadInstructions(new List<XslNode>(), InstructionFlags.None);
        }

        private List<XslNode> LoadInstructions(InstructionFlags flags) {
            return LoadInstructions(new List<XslNode>(), flags);
        }

        private List<XslNode> LoadInstructions(List<XslNode> content) {
            return LoadInstructions(content, InstructionFlags.None);
        }

        const int MAX_LOADINSTRUCTIONS_DEPTH = 1024;
        private int loadInstructionsDepth = 0;
        private List<XslNode> LoadInstructions(List<XslNode> content, InstructionFlags flags) {
            if (++loadInstructionsDepth > MAX_LOADINSTRUCTIONS_DEPTH) {
                if (System.Xml.XmlConfiguration.XsltConfigSection.LimitXPathComplexity) {
                    throw XslLoadException.Create(Res.Xslt_InputTooComplex);
                }
            }
            QName parentName = input.ElementName;
            if (input.MoveToFirstChild()) {
                bool    atTop = true;
                int     sortNumber = 0;
                XslNode result;

                do {
                    switch (input.NodeType) {
                    case XmlNodeType.Element:
                        string nspace = input.NamespaceUri;
                        string name   = input.LocalName;
                        if (nspace == atoms.UriXsl) {
                            InstructionFlags instrFlag = (
                                Ref.Equal(name, atoms.Param) ? InstructionFlags.AllowParam :
                                Ref.Equal(name, atoms.Sort ) ? InstructionFlags.AllowSort  :
                                /*else */ InstructionFlags.None
                            );
                            if (instrFlag != InstructionFlags.None) {
                                string error = (
                                    (flags & instrFlag) == 0 ? /*[XT_013]*/Res.Xslt_UnexpectedElement :
                                    !atTop                   ? /*[XT_014]*/Res.Xslt_NotAtTop :
                                    /*else*/ null
                                );
                                if (error != null) {
                                    ReportError(error, input.QualifiedName, parentName);
                                    atTop = false;
                                    input.SkipNode();
                                    continue;
                                }
                            } else {
                                atTop = false;
                            }
                            result = (
                                Ref.Equal(name, atoms.ApplyImports         ) ? XslApplyImports() :
                                Ref.Equal(name, atoms.ApplyTemplates       ) ? XslApplyTemplates() :
                                Ref.Equal(name, atoms.CallTemplate         ) ? XslCallTemplate() :
                                Ref.Equal(name, atoms.Copy                 ) ? XslCopy() :
                                Ref.Equal(name, atoms.CopyOf               ) ? XslCopyOf() :
                                Ref.Equal(name, atoms.Fallback             ) ? XslFallback() :
                                Ref.Equal(name, atoms.If                   ) ? XslIf() :
                                Ref.Equal(name, atoms.Choose               ) ? XslChoose() :
                                Ref.Equal(name, atoms.ForEach              ) ? XslForEach() :
                                Ref.Equal(name, atoms.Message              ) ? XslMessage() :
                                Ref.Equal(name, atoms.Number               ) ? XslNumber() :
                                Ref.Equal(name, atoms.ValueOf              ) ? XslValueOf() :
                                Ref.Equal(name, atoms.Comment              ) ? XslComment() :
                                Ref.Equal(name, atoms.ProcessingInstruction) ? XslProcessingInstruction() :
                                Ref.Equal(name, atoms.Text                 ) ? XslText() :
                                Ref.Equal(name, atoms.Element              ) ? XslElement() :
                                Ref.Equal(name, atoms.Attribute            ) ? XslAttribute() :
                                Ref.Equal(name, atoms.Variable             ) ? XslVarPar() :
                                Ref.Equal(name, atoms.Param                ) ? XslVarPar() :
                                Ref.Equal(name, atoms.Sort                 ) ? XslSort(sortNumber ++) :
#if XSLT2
                                V2 && Ref.Equal(name, atoms.AnalyzeString  ) ? XslAnalyzeString() :
                                V2 && Ref.Equal(name, "namespace"      ) ? XslNamespace() :
                                V2 && Ref.Equal(name, atoms.PerformSort    ) ? XslPerformSort() :
                                V2 && Ref.Equal(name, atoms.Document       ) ? XslDocument() :
                                V2 && Ref.Equal(name, atoms.ForEachGroup   ) ? XslForEachGroup() :
                                V2 && Ref.Equal(name, atoms.NextMatch      ) ? XslNextMatch() :
                                V2 && Ref.Equal(name, atoms.Sequence       ) ? XslSequence() :
                                V2 && Ref.Equal(name, atoms.ResultDocument ) ? XslResultDocument() :
#endif
                                /*default:*/                                   LoadUnknownXsltInstruction(parentName)
                            );
                        } else {
                            atTop = false;
                            result = LoadLiteralResultElement(/*asStylesheet:*/false);
                        }
                        break;
                    case XmlNodeType.SignificantWhitespace:
                        result = SetLineInfo(f.Text(input.Value), input.BuildLineInfo());
                        break;
                    case XmlNodeType.Whitespace:
                        continue;
                    default:
                        Debug.Assert(input.NodeType == XmlNodeType.Text);
                        atTop = false;
                        goto case XmlNodeType.SignificantWhitespace;
                    }
                    AddInstruction(content, result);
                } while (input.MoveToNextSibling());
            }
            --loadInstructionsDepth;
            return content;
        }

        private List<XslNode> LoadWithParams(InstructionFlags flags) {
            QName parentName = input.ElementName;
            List<XslNode> content = new List<XslNode>();
            /* Process children */
            if (input.MoveToFirstChild()) {
                int sortNumber = 0;
                do {
                    switch (input.NodeType) {
                    case XmlNodeType.Element:
                        if (input.IsXsltKeyword(atoms.WithParam)) {
                            XslNode withParam = XslVarPar();
                            CheckWithParam(content, withParam);
                            AddInstruction(content, withParam);
                        } else if (flags == InstructionFlags.AllowSort && input.IsXsltKeyword(atoms.Sort)) {
                            AddInstruction(content, XslSort(sortNumber++));
                        } else if (flags == InstructionFlags.AllowFallback && input.IsXsltKeyword(atoms.Fallback)) {
                            XslFallback();
                        } else {
                            ReportError(/*[XT_016]*/Res.Xslt_UnexpectedElement, input.QualifiedName, parentName);
                            input.SkipNode();
                        }
                        break;
                    case XmlNodeType.Whitespace:
                    case XmlNodeType.SignificantWhitespace:
                        break;
                    default:
                        Debug.Assert(input.NodeType == XmlNodeType.Text);
                        ReportError(/*[XT_016]*/Res.Xslt_TextNodesNotAllowed, parentName);
                        break;
                    }
                } while (input.MoveToNextSibling());
            }
            return content;
        }

        // http://www.w3.org/TR/xslt#apply-imports
        private XslNode XslApplyImports() {
            ContextInfo ctxInfo = input.GetAttributes();
            if (!input.CanHaveApplyImports) {
                ReportError(/*[XT_015]*/Res.Xslt_InvalidApplyImports);
                input.SkipNode();
                return null;
            }

            List<XslNode> content = LoadWithParams(InstructionFlags.None);

            ctxInfo.SaveExtendedLineInfo(input);

            if (V1) {
                if (content.Count != 0) {
                    ISourceLineInfo contentInfo = content[0].SourceLine;
                    if (!input.ForwardCompatibility) {
                        compiler.ReportError(contentInfo, /*[XT0260]*/Res.Xslt_NotEmptyContents, atoms.ApplyImports);
                    } else {
                        return SetInfo(f.Error(XslLoadException.CreateMessage(contentInfo, /*[XT0260]*/Res.Xslt_NotEmptyContents, atoms.ApplyImports)), null, ctxInfo);
                    }
                }
                content = null;
            } else {
                if (content.Count != 0) ReportNYI("xsl:apply-imports/xsl:with-param");
                content = null;
            }

            return SetInfo(f.ApplyImports(/*Mode:*/curTemplate.Mode, curStylesheet, input.XslVersion), content, ctxInfo);
        }

        // http://www.w3.org/TR/xslt#section-Applying-Template-Rules
        XsltAttribute[] applyTemplatesAttributes = {
            new XsltAttribute("select", V1Opt | V2Opt),
            new XsltAttribute("mode"  , V1Opt | V2Opt)
        };
        private XslNode XslApplyTemplates() {
            ContextInfo ctxInfo = input.GetAttributes(applyTemplatesAttributes);

            string select = ParseStringAttribute(0, "select");
            if (select == null) {
                select = "node()";
            }
            QilName mode = ParseModeAttribute(1);

            List<XslNode> content = LoadWithParams(InstructionFlags.AllowSort);
            ctxInfo.SaveExtendedLineInfo(input);
            return SetInfo(f.ApplyTemplates(mode, select, ctxInfo, input.XslVersion),
                content, ctxInfo
            );
        }

        // http://www.w3.org/TR/xslt#named-templates
        // http://www.w3.org/TR/xslt#element-call-template
        XsltAttribute[] callTemplateAttributes = {
            new XsltAttribute("name", V1Req | V2Req)
        };
        private XslNode XslCallTemplate() {
            ContextInfo ctxInfo = input.GetAttributes(callTemplateAttributes);
            QilName name = ParseQNameAttribute(0);

            List<XslNode> content = LoadWithParams(InstructionFlags.None);
            ctxInfo.SaveExtendedLineInfo(input);
            return SetInfo(f.CallTemplate(name, ctxInfo), content, ctxInfo);
        }

        // http://www.w3.org/TR/xslt#copying
        // http://www.w3.org/TR/xslt20/#element-copy
        XsltAttribute[] copyAttributes = {
            new XsltAttribute("copy-namespaces"   ,         V2Opt),
            new XsltAttribute("inherit-namespaces",         V2Opt),
            new XsltAttribute("use-attribute-sets", V1Opt | V2Opt),
            new XsltAttribute("type"              ,         V2Opt),
            new XsltAttribute("validation"        ,         V2Opt)
        };
        private XslNode XslCopy() {
            ContextInfo ctxInfo = input.GetAttributes(copyAttributes);

            bool copyNamespaces    = ParseYesNoAttribute(0, "copy-namespaces"   ) != TriState.False;
            bool inheritNamespaces = ParseYesNoAttribute(1, "inherit-namespaces") != TriState.False;
            if (! copyNamespaces   ) ReportNYI("xsl:copy[@copy-namespaces    = 'no']");
            if (! inheritNamespaces) ReportNYI("xsl:copy[@inherit-namespaces = 'no']");

            List<XslNode> content = new List<XslNode>();
            if (input.MoveToXsltAttribute(2, "use-attribute-sets")) {
                AddUseAttributeSets(content);
            }

            ParseTypeAttribute(3);
            ParseValidationAttribute(4, /*defVal:*/false);

            return SetInfo(f.Copy(), LoadEndTag(LoadInstructions(content)), ctxInfo);
        }

        XsltAttribute[] copyOfAttributes = {
            new XsltAttribute("select"         , V1Req | V2Req),
            new XsltAttribute("copy-namespaces",         V2Opt),
            new XsltAttribute("type"           ,         V2Opt),
            new XsltAttribute("validation"     ,         V2Opt)
        };
        private XslNode XslCopyOf() {
            ContextInfo ctxInfo = input.GetAttributes(copyOfAttributes);
            string select = ParseStringAttribute(0, "select");
            bool copyNamespaces = ParseYesNoAttribute(1, "copy-namespaces") != TriState.False;
            if (!copyNamespaces) ReportNYI("xsl:copy-of[@copy-namespaces    = 'no']");

            ParseTypeAttribute(2);
            ParseValidationAttribute(3, /*defVal:*/false);

            CheckNoContent();
            return SetInfo(f.CopyOf(select, input.XslVersion), null, ctxInfo);
        }

        // http://www.w3.org/TR/xslt#fallback
        // See LoadFallbacks() for real fallback implementation
        private XslNode XslFallback() {
            input.GetAttributes();
            input.SkipNode();
            return null;
        }

        XsltAttribute[] ifAttributes = {
            new XsltAttribute("test", V1Req | V2Req)
        };
        private XslNode XslIf() {
            ContextInfo ctxInfo = input.GetAttributes(ifAttributes);
            string test = ParseStringAttribute(0, "test");

            return SetInfo(f.If(test, input.XslVersion), LoadInstructions(), ctxInfo);
        }

        private XslNode XslChoose() {
            ContextInfo ctxInfo = input.GetAttributes();

            List<XslNode> content   = new List<XslNode>();
            bool        otherwise = false;
            bool        when      = false;

            QName parentName = input.ElementName;
            if (input.MoveToFirstChild()) {
                do {
                    switch (input.NodeType) {
                    case XmlNodeType.Element:
                        XslNode node = null;
                        if (Ref.Equal(input.NamespaceUri, atoms.UriXsl)) {
                            if (Ref.Equal(input.LocalName, atoms.When)) {
                                if (otherwise) {
                                    ReportError(/*[XT_018]*/Res.Xslt_WhenAfterOtherwise);
                                    input.SkipNode();
                                    continue;
                                } else {
                                    when = true;
                                    node = XslIf();
                                }
                            } else if (Ref.Equal(input.LocalName, atoms.Otherwise)) {
                                if (otherwise) {
                                    ReportError(/*[XT_019]*/Res.Xslt_DupOtherwise);
                                    input.SkipNode();
                                    continue;
                                } else {
                                    otherwise = true;
                                    node = XslOtherwise();
                                }
                            }
                        }
                        if (node == null) {
                            ReportError(/*[XT_020]*/Res.Xslt_UnexpectedElement, input.QualifiedName, parentName);
                            input.SkipNode();
                            continue;
                        }
                        AddInstruction(content, node);
                        break;
                    case XmlNodeType.Whitespace:
                    case XmlNodeType.SignificantWhitespace:
                        break;
                    default:
                        Debug.Assert(input.NodeType == XmlNodeType.Text);
                        ReportError(/*[XT_020]*/Res.Xslt_TextNodesNotAllowed, parentName);
                        break;
                    }
                } while (input.MoveToNextSibling());
            }
            CheckError(!when, /*[XT_021]*/Res.Xslt_NoWhen);
            return SetInfo(f.Choose(), content, ctxInfo);
        }

        private XslNode XslOtherwise() {
            ContextInfo ctxInfo = input.GetAttributes();
            return SetInfo(f.Otherwise(), LoadInstructions(), ctxInfo);
        }

        XsltAttribute[] forEachAttributes = {
            new XsltAttribute("select", V1Req | V2Req)
        };
        private XslNode XslForEach() {
            ContextInfo ctxInfo = input.GetAttributes(forEachAttributes);

            string select = ParseStringAttribute(0, "select");
            // The current template rule becomes null, so we must not allow xsl:apply-import's within this element
            input.CanHaveApplyImports = false;
            List<XslNode> content = LoadInstructions(InstructionFlags.AllowSort);
            ctxInfo.SaveExtendedLineInfo(input);

            return SetInfo(f.ForEach(select, ctxInfo, input.XslVersion),
                content, ctxInfo
            );
        }

        // http://www.w3.org/TR/xslt#message
        // http://www.w3.org/TR/xslt20/#element-message
        XsltAttribute[] messageAttributes = {
            new XsltAttribute("select"   ,         V2Opt),
            new XsltAttribute("terminate", V1Opt | V2Opt)
        };
        private XslNode XslMessage() {
            ContextInfo ctxInfo = input.GetAttributes(messageAttributes);

            string select = ParseStringAttribute(0, "select");
            bool terminate = ParseYesNoAttribute(1, /*attName:*/"terminate") == TriState.True;

            List<XslNode> content = LoadInstructions();
            if (content.Count != 0) {
                content = LoadEndTag(content);
            }
            if (select != null) {
                content.Insert(0, f.CopyOf(select, input.XslVersion));
            }

            return SetInfo(f.Message(terminate), content, ctxInfo);
        }

        // http://www.w3.org/TR/xslt#number
        // http://www.w3.org/TR/xslt20/#element-number
        XsltAttribute[] numberAttributes = {
            new XsltAttribute("value"             , V1Opt | V2Opt),
            new XsltAttribute("select"            ,         V2Opt),
            new XsltAttribute("level"             , V1Opt | V2Opt),
            new XsltAttribute("count"             , V1Opt | V2Opt),
            new XsltAttribute("from"              , V1Opt | V2Opt),
            new XsltAttribute("format"            , V1Opt | V2Opt),
            new XsltAttribute("lang"              , V1Opt | V2Opt),
            new XsltAttribute("letter-value"      , V1Opt | V2Opt),
            new XsltAttribute("ordinal"           ,         V2Opt),
            new XsltAttribute("grouping-separator", V1Opt | V2Opt),
            new XsltAttribute("grouping-size"     , V1Opt | V2Opt)
        };
        private XslNode XslNumber() {
            ContextInfo ctxInfo = input.GetAttributes(numberAttributes);

            string value  = ParseStringAttribute(0, "value");
            string select = ParseStringAttribute(1, "select");
            if (select != null) ReportNYI("xsl:number/@select");
            NumberLevel level = NumberLevel.Single;
            if (input.MoveToXsltAttribute(2, "level")) {
                switch (input.Value) {
                case "single"  : level = NumberLevel.Single  ; break;
                case "multiple": level = NumberLevel.Multiple; break;
                case "any"     : level = NumberLevel.Any     ; break;
                default:
                    if (!input.ForwardCompatibility) {
                        ReportError(/*[XT_022]*/Res.Xslt_InvalidAttrValue, "level", input.Value);
                    }
                    break;
                }
            }
            string count             = ParseStringAttribute(3, "count" );
            string from              = ParseStringAttribute(4, "from"  );
            string format            = ParseStringAttribute(5, "format");
            string lang              = ParseStringAttribute(6, "lang"  );
            string letterValue       = ParseStringAttribute(7, "letter-value");
            string ordinal           = ParseStringAttribute(8, "ordinal");
            if (!string.IsNullOrEmpty(ordinal)) ReportNYI("xsl:number/@ordinal");
            string groupingSeparator = ParseStringAttribute(9, "grouping-separator");
            string groupingSize      = ParseStringAttribute(10, "grouping-size"  );

            // Default values for xsl:number :  level="single"  format="1"
            if (format == null) {
                format = "1";
            }

            CheckNoContent();
            return SetInfo(
                f.Number(level, count, from, value,
                    format, lang, letterValue, groupingSeparator, groupingSize,
                    input.XslVersion
                ),
                null, ctxInfo
            );
        }

        // http://www.w3.org/TR/xslt#value-of
        XsltAttribute[] valueOfAttributes = {
            new XsltAttribute("select"                 , V1Req | V2Opt),
            new XsltAttribute("separator"              ,         V2Opt),
            new XsltAttribute("disable-output-escaping", V1Opt | V2Opt)
        };
        private XslNode XslValueOf() {
            ContextInfo ctxInfo = input.GetAttributes(valueOfAttributes);

            string select    = ParseStringAttribute(0, "select");
            string separator = ParseStringAttribute(1, "separator");
            bool doe = ParseYesNoAttribute(2, /*attName:*/"disable-output-escaping") == TriState.True;

            if (separator == null) {
                if (!input.BackwardCompatibility) {
                    separator = select != null ? " " : string.Empty;
                }
            } else {
                ReportNYI("xsl:value-of/@separator");
            }

            List<XslNode> content = null;

            if (V1) {
                if (select == null) {
                    input.SkipNode();
                    return SetInfo(f.Error(XslLoadException.CreateMessage(ctxInfo.lineInfo, Res.Xslt_MissingAttribute, "select")), null, ctxInfo);
                }
                CheckNoContent();
            } else {
                content = LoadContent(select != null);
                CheckError(select == null && content.Count == 0, /*[???]*/Res.Xslt_NoSelectNoContent, input.ElementName);
                if (content.Count != 0) {
                    ReportNYI("xsl:value-of/*");
                    content = null;
                }
            }

            return SetInfo(f.XslNode(doe ? XslNodeType.ValueOfDoe : XslNodeType.ValueOf, null, select, input.XslVersion),
                null, ctxInfo
            );
        }

        //                    required tunnel select
        // variable              -        -      +
        // with-param            -        +      +
        // stylesheet/param      +        -      +
        // template/param        +        +      +
        // function/param        -        -      -
        // xsl:variable     http://www.w3.org/TR/xslt#local-variables
        // xsl:param        http://www.w3.org/TR/xslt#element-param
        // xsl:with-param   http://www.w3.org/TR/xslt#element-with-param
        XsltAttribute[] variableAttributes = {
            new XsltAttribute("name"    , V1Req | V2Req),
            new XsltAttribute("select"  , V1Opt | V2Opt),
            new XsltAttribute("as"      ,         V2Opt),
            new XsltAttribute("required",             0),
            new XsltAttribute("tunnel"  ,             0)
        };
        XsltAttribute[] paramAttributes = {
            new XsltAttribute("name"    , V1Req | V2Req),
            new XsltAttribute("select"  , V1Opt | V2Opt),
            new XsltAttribute("as"      ,         V2Opt),
            new XsltAttribute("required",         V2Opt),
            new XsltAttribute("tunnel"  ,         V2Opt)
        };
        XsltAttribute[] withParamAttributes = {
            new XsltAttribute("name"    , V1Req | V2Req),
            new XsltAttribute("select"  , V1Opt | V2Opt),
            new XsltAttribute("as"      ,         V2Opt),
            new XsltAttribute("required",             0),
            new XsltAttribute("tunnel"  ,         V2Opt)
        };
        private VarPar XslVarPar() {
            string localName = input.LocalName;
            XslNodeType nodeType = (
                Ref.Equal(localName, atoms.Variable ) ? XslNodeType.Variable  :
                Ref.Equal(localName, atoms.Param    ) ? XslNodeType.Param     :
                Ref.Equal(localName, atoms.WithParam) ? XslNodeType.WithParam :
                XslNodeType.Unknown
            );
            Debug.Assert(nodeType != XslNodeType.Unknown);
            bool isParam = Ref.Equal(localName, atoms.Param);
            ContextInfo ctxInfo = input.GetAttributes(
                nodeType == XslNodeType.Variable ? variableAttributes :
                nodeType == XslNodeType.Param    ? paramAttributes   :
                /*default:*/                       withParamAttributes
            );

            QilName name  = ParseQNameAttribute(0);
            string select = ParseStringAttribute(1, "select");
            string asType = ParseStringAttribute(2, "as");
            TriState required = ParseYesNoAttribute(3, "required");
            if (nodeType == XslNodeType.Param && curFunction != null) {
                if (!input.ForwardCompatibility) {
                    CheckError(required != TriState.Unknown, /*[???]*/Res.Xslt_RequiredOnFunction, name.ToString());
                }
                required = TriState.True;
            } else {
                if (required == TriState.True) ReportNYI("xsl:param/@required == true()");
            }

            if (asType != null) {
                ReportNYI("xsl:param/@as");
            }

            TriState tunnel = ParseYesNoAttribute(4, "tunnel");
            if (tunnel != TriState.Unknown) {
                if (nodeType == XslNodeType.Param && curTemplate == null) {
                    if (!input.ForwardCompatibility) {
                        ReportError(/*[???]*/Res.Xslt_NonTemplateTunnel, name.ToString());
                    }
                } else {
                    if (tunnel == TriState.True) ReportNYI("xsl:param/@tunnel == true()");
                }
            }

            List<XslNode> content = LoadContent(select != null);
            CheckError((required == TriState.True) && (select != null || content.Count != 0), /*[???]*/Res.Xslt_RequiredAndSelect, name.ToString());

            VarPar result = f.VarPar(nodeType, name, select, input.XslVersion);
            SetInfo(result, content, ctxInfo);
            return result;
        }

        // http://www.w3.org/TR/xslt#section-Creating-Comments
        // http://www.w3.org/TR/xslt20/#element-comment
        XsltAttribute[] commentAttributes = {
            new XsltAttribute("select", V2Opt)
        };
        private XslNode XslComment() {
            ContextInfo ctxInfo = input.GetAttributes(commentAttributes);
            string select = ParseStringAttribute(0, "select");
            if (select != null) ReportNYI("xsl:comment/@select");

            return SetInfo(f.Comment(), LoadContent(select != null), ctxInfo);
        }

        private List<XslNode> LoadContent(bool hasSelect) {
            QName parentName = input.ElementName;
            List<XslNode> content = LoadInstructions();
            CheckError(hasSelect && content.Count != 0, /*[XT0620]*/Res.Xslt_ElementCntSel, parentName);
            // Load the end tag only if the content is not empty
            if (content.Count != 0) {
                content = LoadEndTag(content);
            }
            return content;
        }

        // http://www.w3.org/TR/xslt#section-Creating-Processing-Instructions
        // http://www.w3.org/TR/xslt20/#element-processing-instruction
        XsltAttribute[] processingInstructionAttributes = {
            new XsltAttribute("name"  , V1Req | V2Req),
            new XsltAttribute("select",         V2Opt)
        };
        private XslNode XslProcessingInstruction() {
            ContextInfo ctxInfo = input.GetAttributes(processingInstructionAttributes);
            string name = ParseNCNameAttribute(0);
            string select = ParseStringAttribute(1, "select");
            if (select != null) ReportNYI("xsl:processing-instruction/@select");

            return SetInfo(f.PI(name, input.XslVersion), LoadContent(select != null), ctxInfo);
        }

        // http://www.w3.org/TR/xslt#section-Creating-Text
        XsltAttribute[] textAttributes = {
            new XsltAttribute("disable-output-escaping", V1Opt | V2Opt)
        };
        private XslNode XslText() {
            ContextInfo ctxInfo = input.GetAttributes(textAttributes);

            bool doe = ParseYesNoAttribute(0, /*attName:*/ "disable-output-escaping") == TriState.True;
            SerializationHints hints = doe ? SerializationHints.DisableOutputEscaping : SerializationHints.None;

            // We are not using StringBuilder here because in most cases there will be just one text node.
            List<XslNode> content = new List<XslNode>();

            QName parentName = input.ElementName;
            if (input.MoveToFirstChild()) {
                do {
                    switch (input.NodeType) {
                    case XmlNodeType.Text:
                    case XmlNodeType.Whitespace:
                    case XmlNodeType.SignificantWhitespace:
                        // xsl:text may contain multiple child text nodes separated by comments and PIs, which are ignored by XsltInput
                        content.Add(f.Text(input.Value, hints));
                        break;
                    default:
                        Debug.Assert(input.NodeType == XmlNodeType.Element);
                        ReportError(/*[XT_023]*/Res.Xslt_UnexpectedElement, input.QualifiedName, parentName);
                        input.SkipNode();
                        break;
                    }
                } while (input.MoveToNextSibling());
            }

            // Empty xsl:text elements will be ignored
            return SetInfo(f.List(), content, ctxInfo);
        }

        // http://www.w3.org/TR/xslt#section-Creating-Elements-with-xsl:element
        // http://www.w3.org/TR/xslt20/#element-element
        XsltAttribute[] elementAttributes = {
            new XsltAttribute("name"             , V1Req | V2Req),
            new XsltAttribute("namespace"        , V1Opt | V2Opt),
            new XsltAttribute("inherit-namespaces",         V2Opt),
            new XsltAttribute("use-attribute-sets" , V1Opt | V2Opt),
            new XsltAttribute("type"             ,         V2Opt),
            new XsltAttribute("validation"       ,         V2Opt)
        };
        private XslNode XslElement() {
            ContextInfo ctxInfo = input.GetAttributes(elementAttributes);

            string name = ParseNCNameAttribute(0); ;
            string ns = ParseStringAttribute(1, "namespace");
            CheckError(ns == XmlReservedNs.NsXmlNs, /*[XT_024]*/Res.Xslt_ReservedNS, ns);

            bool inheritNamespaces = ParseYesNoAttribute(2, "inherit-namespaces") != TriState.False;
            if (!inheritNamespaces) ReportNYI("xsl:copy[@inherit-namespaces = 'no']");

            ParseTypeAttribute(4);
            ParseValidationAttribute(5, /*defVal:*/false);

            List<XslNode> content = new List<XslNode>();
            if (input.MoveToXsltAttribute(3, "use-attribute-sets")) {
                AddUseAttributeSets(content);
            }
            return SetInfo(f.Element(name, ns, input.XslVersion),
                LoadEndTag(LoadInstructions(content)), ctxInfo
            );
        }

        // http://www.w3.org/TR/xslt#creating-attributes
        // http://www.w3.org/TR/xslt20#creating-attributes
        XsltAttribute[] attributeAttributes = {
            new XsltAttribute("name"      , V1Req | V2Req),
            new XsltAttribute("namespace" , V1Opt | V2Opt),
            new XsltAttribute("select"    ,         V2Opt),
            new XsltAttribute("separator" ,         V2Opt),
            new XsltAttribute("type"      ,         V2Opt),
            new XsltAttribute("validation",         V2Opt)
        };
        private XslNode XslAttribute() {
            ContextInfo ctxInfo = input.GetAttributes(attributeAttributes);

            string name = ParseNCNameAttribute(0);
            string ns = ParseStringAttribute(1, "namespace");
            CheckError(ns == XmlReservedNs.NsXmlNs, /*[XT_024]*/Res.Xslt_ReservedNS, ns);

            string select = ParseStringAttribute(2, "select");
            if (select != null) ReportNYI("xsl:attribute/@select");
            string separator = ParseStringAttribute(3, "separator");
            if (separator != null) ReportNYI("xsl:attribute/@separator");
            separator = separator != null ? separator : (select != null ? " " : string.Empty);

            ParseTypeAttribute(4);
            ParseValidationAttribute(5, /*defVal:*/false);

            return SetInfo(f.Attribute(name, ns, input.XslVersion), LoadContent(select != null), ctxInfo);
        }

        // http://www.w3.org/TR/xslt#sorting
        // http://www.w3.org/TR/xslt20/#element-sort
        XsltAttribute[] sortAttributes = {
            new XsltAttribute("select"    , V1Opt | V2Opt),
            new XsltAttribute("lang"      , V1Opt | V2Opt),
            new XsltAttribute("order"     , V1Opt | V2Opt),
            new XsltAttribute("collation" , V1Opt | V2Opt),
            new XsltAttribute("stable"    , V1Opt | V2Opt),
            new XsltAttribute("case-order", V1Opt | V2Opt),
            new XsltAttribute("data-type" , V1Opt | V2Opt)
        };
        private XslNode XslSort(int sortNumber) {
            ContextInfo ctxInfo = input.GetAttributes(sortAttributes);

            string   select    = ParseStringAttribute(   0, "select"    );
            string   lang      = ParseStringAttribute(   1, "lang"      );
            string   order     = ParseStringAttribute(   2, "order"     );
            string   collation = ParseCollationAttribute(3);
            TriState stable    = ParseYesNoAttribute (   4, "stable"    );
            string   caseOrder = ParseStringAttribute(   5, "case-order");
            string   dataType  = ParseStringAttribute(   6, "data-type" );

            if (stable != TriState.Unknown) {
                CheckError(sortNumber != 0, Res.Xslt_SortStable);
            }

            List<XslNode> content = null;
            if (V1) {
                CheckNoContent();
            } else {
                content = LoadContent(select != null);
                if (content.Count != 0) {
                    ReportNYI("xsl:sort/*");
                    content = null;
                }
            }

            if (select == null /*&& content.Count == 0*/) {
                select = ".";
            }

            return SetInfo(f.Sort(select, lang, dataType, order, caseOrder, input.XslVersion),
                null, ctxInfo
            );
        }

#if XSLT2
        // http://www.w3.org/TR/xslt20/#element-document
        XsltAttribute[] documentAttributes = {
            new XsltAttribute("type"      , V2Opt),
            new XsltAttribute("validation", V2Opt)
        };
        private XslNode XslDocument() {
            ContextInfo ctxInfo = input.GetAttributes(documentAttributes);

            ParseTypeAttribute(0);
            ParseValidationAttribute(1, /*defVal:*/false);

            ReportNYI("xsl:document");

            List<XslNode> content = LoadEndTag(LoadInstructions());

            return null;
        }

        // http://www.w3.org/TR/xslt20/#element-analyze-string
        XsltAttribute[] analyzeStringAttributes = {
            new XsltAttribute("select", V2Req),
            new XsltAttribute("regex" , V2Req),
            new XsltAttribute("flags" , V2Opt)
        };
        private XslNode XslAnalyzeString() {
            ContextInfo ctxInfo = input.GetAttributes(analyzeStringAttributes);

            string select = ParseStringAttribute(0, "select");
            string regex  = ParseStringAttribute(1, "regex" );
            string flags  = ParseStringAttribute(2, "flags" );
            if (flags == null) {
                flags = "";
            }

            ReportNYI("xsl:analyze-string");

            XslNode matching = null;
            XslNode nonMatching = null;
            QName parentName = input.ElementName;
            if (input.MoveToFirstChild()) {
                do {
                    switch (input.NodeType) {
                    case XmlNodeType.Element:
                        if (input.IsXsltKeyword(atoms.MatchingSubstring)) {
                            ContextInfo ctxInfoChld = input.GetAttributes();
                            CheckError(nonMatching != null, /*[???]*/Res.Xslt_AnalyzeStringChildOrder);
                            CheckError(matching    != null, /*[???]*/Res.Xslt_AnalyzeStringDupChild, atoms.MatchingSubstring);
                            // The current template rule becomes null, so we must not allow xsl:apply-import's within this element
                            input.CanHaveApplyImports = false;
                            matching = SetInfo(f.List(), LoadInstructions(), ctxInfoChld);
                        } else if (input.IsXsltKeyword(atoms.NonMatchingSubstring)) {
                            ContextInfo ctxInfoChld = input.GetAttributes();
                            CheckError(nonMatching != null, /*[???]*/Res.Xslt_AnalyzeStringDupChild, atoms.NonMatchingSubstring);
                            input.CanHaveApplyImports = false;
                            nonMatching = SetInfo(f.List(), LoadInstructions(), ctxInfoChld);
                        } else if (input.IsXsltKeyword(atoms.Fallback)) {
                            XslFallback();
                        } else {
                            ReportError(/*[XT_017]*/Res.Xslt_UnexpectedElement, input.QualifiedName, parentName);
                            input.SkipNode();
                        }
                        break;
                    case XmlNodeType.Whitespace:
                    case XmlNodeType.SignificantWhitespace:
                        break;
                    default:
                        Debug.Assert(input.NodeType == XmlNodeType.Text);
                        ReportError(/*[XT_017]*/Res.Xslt_TextNodesNotAllowed, parentName);
                        break;
                    }
                } while (input.MoveToNextSibling());
            }
            CheckError(matching == nonMatching, /*[XTSE1130]*/Res.Xslt_AnalyzeStringEmpty);

            ctxInfo.SaveExtendedLineInfo(input);

            return null;
        }

        // http://www.w3.org/TR/xslt20/#element-namespace
        XsltAttribute[] namespaceAttributes = {
            new XsltAttribute("name"  , V2Req),
            new XsltAttribute("select", V2Opt)
        };
        private XslNode XslNamespace() {
            ContextInfo ctxInfo = input.GetAttributes(namespaceAttributes);
            string name = ParseNCNameAttribute(0);
            string select= ParseStringAttribute(1, "select");

            List<XslNode> content = LoadContent(select != null);
            CheckError(select == null && content.Count == 0, /*[???]*/Res.Xslt_NoSelectNoContent, input.ElementName);

            ReportNYI("xsl:namespace");

            return null;
        }

        // http://www.w3.org/TR/xslt20/#element-perform-sort
        XsltAttribute[] performSortAttributes = {
            new XsltAttribute("select", V2Opt)
        };
        private XslNode XslPerformSort() {
            ContextInfo ctxInfo = input.GetAttributes(performSortAttributes);
            string select = ParseStringAttribute(0, "select");

            List<XslNode> content = LoadInstructions(InstructionFlags.AllowSort);
            ctxInfo.SaveExtendedLineInfo(input);

            if (select != null) {
                foreach (XslNode node in content) {
                    if (node.NodeType != XslNodeType.Sort) {
                        ReportError(Res.Xslt_PerformSortCntSel);
                        break;
                    }
                }
            }

            ReportNYI("xsl:perform-sort");
            return null;
        }

        // http://www.w3.org/TR/xslt20/#element-for-each-group
        XsltAttribute[] forEachGroupAttributes = {
            new XsltAttribute("select"             , V2Req),
            new XsltAttribute("group-by"           , V2Opt),
            new XsltAttribute("group-adjacent"     , V2Opt),
            new XsltAttribute("group-starting-with", V2Opt),
            new XsltAttribute("group-ending-with"  , V2Opt),
            new XsltAttribute("collation"          , V2Opt)
        };
        private XslNode XslForEachGroup() {
            ContextInfo ctxInfo = input.GetAttributes(forEachGroupAttributes);

            string select            = ParseStringAttribute(   0, "select"             );
            string groupBy           = ParseStringAttribute(   1, "group-by"           );
            string groupAdjacent     = ParseStringAttribute(   2, "group-adjacent"     );
            string groupStartingWith = ParseStringAttribute(   3, "group-starting-with");
            string groupEndingWith   = ParseStringAttribute(   4, "group-ending-with"  );
            string collation         = ParseCollationAttribute(5);

            ReportNYI("xsl:for-each-group");

            // The current template rule becomes null, so we must not allow xsl:apply-import's within this element
            input.CanHaveApplyImports = false;
            List<XslNode> content = LoadInstructions(InstructionFlags.AllowSort);
            ctxInfo.SaveExtendedLineInfo(input);

            return null;
        }

        // http://www.w3.org/TR/xslt20/#element-next-match
        private XslNode XslNextMatch() {
            ContextInfo ctxInfo = input.GetAttributes();

            // We need to do this dynamic any way:
            //if (!input.CanHaveApplyImports) {
            //    ReportError(/*[XT_015]*/Res.Xslt_InvalidApplyImports);
            //    input.SkipNode();
            //    return null;
            //}

            ReportNYI("xsl:next-match");

            List<XslNode> content = LoadWithParams(InstructionFlags.AllowFallback);
            ctxInfo.SaveExtendedLineInfo(input);

            return null;
        }

        // http://www.w3.org/TR/xslt20/#element-sequence
        XsltAttribute[] sequenceAttributes = {
            new XsltAttribute("select", V2Req)
        };
        private XslNode XslSequence() {
            ContextInfo ctxInfo = input.GetAttributes(sequenceAttributes);
            string select = ParseStringAttribute(0, "select");
            ReportNYI("xsl:sequence");

            QName parentName = input.ElementName;
            if (input.MoveToFirstChild()) {
                do {
                    switch (input.NodeType) {
                    case XmlNodeType.Element:
                        if (input.IsXsltKeyword(atoms.Fallback)) {
                            XslFallback();
                        } else {
                            ReportError(/*[XT_017]*/Res.Xslt_UnexpectedElement, input.QualifiedName, parentName);
                            input.SkipNode();
                        }
                        break;
                    case XmlNodeType.Whitespace:
                    case XmlNodeType.SignificantWhitespace:
                        break;
                    default:
                        Debug.Assert(input.NodeType == XmlNodeType.Text);
                        ReportError(/*[XT_017]*/Res.Xslt_TextNodesNotAllowed, parentName);
                        break;
                    }
                } while (input.MoveToNextSibling());
            }
            return null;
        }

        // http://www.w3.org/TR/xslt20/#element-result-document
        XsltAttribute[] resultDocumentAttributes = {
            new XsltAttribute("format"                , V2Opt), // 0
            new XsltAttribute("href"                  , V2Opt), // 1
            new XsltAttribute("validation"            , V2Opt), // 2
            new XsltAttribute("type"                  , V2Opt), // 3
            new XsltAttribute("name"                  , V2Opt), // 4
            new XsltAttribute("method"                , V2Opt), // 5
            new XsltAttribute("byte-order-mark"       , V2Opt), // 6
            new XsltAttribute("cdata-section-elements", V2Opt), // 7
            new XsltAttribute("doctype-public"        , V2Opt), // 8
            new XsltAttribute("doctype-system"        , V2Opt), // 9
            new XsltAttribute("encoding"              , V2Opt), // 10
            new XsltAttribute("escape-uri-attributes" , V2Opt), // 11
            new XsltAttribute("include-content-type"  , V2Opt), // 12
            new XsltAttribute("indent"                , V2Opt), // 13
            new XsltAttribute("media-type"            , V2Opt), // 14
            new XsltAttribute("normalization-form"    , V2Opt), // 15
            new XsltAttribute("omit-xml-declaration"  , V2Opt), // 16
            new XsltAttribute("standalone"            , V2Opt), // 17
            new XsltAttribute("undeclare-prefixes"    , V2Opt), // 18
            new XsltAttribute("use-character-maps"    , V2Opt), // 19
            new XsltAttribute("output-version"        , V2Opt)  // 20
        };
        private XslNode XslResultDocument() {
            ContextInfo ctxInfo = input.GetAttributes(resultDocumentAttributes);

            string format                  = ParseStringAttribute(0 , "format");
            XmlWriterSettings settings = new XmlWriterSettings(); // we should use attFormat to determing settings
            string href                    = ParseStringAttribute(1 , "href");
            ParseValidationAttribute(2, /*defVal:*/false);
            ParseTypeAttribute(3);
            QilName  name                  = ParseQNameAttribute( 4);
            TriState byteOrderMask         = ParseYesNoAttribute( 6 , "byte-order-mark");
            string   docTypePublic         = ParseStringAttribute(8 , "doctype-public");
            string   docTypeSystem         = ParseStringAttribute(9 , "doctype-system");
            bool     escapeUriAttributes   = ParseYesNoAttribute( 11, "escape-uri-attributes") != TriState.False;
            bool     includeContentType    = ParseYesNoAttribute( 12, "include-content-type") != TriState.False;
            settings.Indent                = ParseYesNoAttribute( 13, "indent") == TriState.True;
            string   mediaType             = ParseStringAttribute(14, "media-type");
            string   normalizationForm     = ParseStringAttribute(15, "normalization-form");
            settings.OmitXmlDeclaration    = ParseYesNoAttribute( 16, "omit-xml-declaration") == TriState.True;
            settings.Standalone            = ParseYesNoAttribute( 17, "standalone"        ) == TriState.True ? XmlStandalone.Yes : XmlStandalone.No;
            bool undeclarePrefixes         = ParseYesNoAttribute( 18, "undeclare-prefixes") == TriState.True;
            List<QilName> useCharacterMaps = ParseUseCharacterMaps(19);
            string   outputVersion         = ParseStringAttribute(20, "output-version");

            ReportNYI("xsl:result-document");

            if (format != null) ReportNYI("xsl:result-document/@format");

            if (href == null) {
                href = string.Empty;
            }
            // attHref is a BaseUri of new output tree. It should be resolved relative to "base output URI"


            if (input.MoveToXsltAttribute(5, "method")) {
                compiler.EnterForwardsCompatible();
                XmlOutputMethod   outputMethod;
                ParseOutputMethod(input.Value, out outputMethod);
                if (compiler.ExitForwardsCompatible(input.ForwardCompatibility)) {
                    settings.OutputMethod = outputMethod;
                }
            }
            if (input.MoveToXsltAttribute(7, "cdata-section-elements")) {
                // Do not check the import precedence, the effective value is the union of all specified values
                compiler.EnterForwardsCompatible();
                string[] qnames = XmlConvert.SplitString(input.Value);
                List<XmlQualifiedName> list = new List<XmlQualifiedName>();
                for (int i = 0; i < qnames.Length; i++) {
                    list.Add(ResolveQName(/*ignoreDefaultNs:*/false, qnames[i]));
                }
                if (compiler.ExitForwardsCompatible(input.ForwardCompatibility)) {
                    foreach (XmlQualifiedName qname in list) {
                        settings.CDataSectionElements.Add(qname);
                    }
                }
            }
            if (input.MoveToXsltAttribute(10, "encoding")) {
                try {
                    // Encoding.GetEncoding() should never throw NotSupportedException, only ArgumentException
                    settings.Encoding = Encoding.GetEncoding(input.Value);
                } catch (ArgumentException) {
                    if (!input.ForwardCompatibility) {
                        ReportWarning(/*[XT_004]*/Res.Xslt_InvalidEncoding, input.Value);
                    }
                }
            }

            if (byteOrderMask != TriState.Unknown) ReportNYI("xsl:result-document/@byte-order-mark");
            if (!escapeUriAttributes) ReportNYI("xsl:result-document/@escape-uri-attributes == flase()");
            if (!includeContentType) ReportNYI("xsl:output/@include-content-type == flase()");
            if (normalizationForm != null) ReportNYI("xsl:result-document/@normalization-form");
            if (undeclarePrefixes) ReportNYI("xsl:result-document/@undeclare-prefixes == true()");

            if (docTypePublic != null) {
                settings.DocTypePublic = docTypePublic;
            }

            if (docTypeSystem != null) {
                settings.DocTypeSystem = docTypeSystem;
            }
            if (mediaType != null) {
                settings.MediaType = mediaType;
            }

            if (useCharacterMaps != null) ReportNYI("xsl:result-document/@use-character-maps");

            if (outputVersion != null) {
                // 

                ReportNYI("xsl:result-document/@output-version");
            }

            LoadInstructions();
            return null;
        }
#endif

        // http://www.w3.org/TR/xslt#literal-result-element
        private XslNode LoadLiteralResultElement(bool asStylesheet) {
            Debug.Assert(input.NodeType == XmlNodeType.Element);
            string prefix   = input.Prefix;
            string name     = input.LocalName;
            string nsUri    = input.NamespaceUri;

            ContextInfo ctxInfo = input.GetLiteralAttributes(asStylesheet);

            if (input.IsExtensionNamespace(nsUri)) {
                // This is not a literal result element, so drop all attributes we have collected
                return SetInfo(f.List(), LoadFallbacks(name), ctxInfo);
            }

            List<XslNode> content = new List<XslNode>();

            for (int i = 1; input.MoveToLiteralAttribute(i); i++) {
                if (input.IsXsltNamespace() && input.IsKeyword(atoms.UseAttributeSets)) {
                    AddUseAttributeSets(content);
                }
            }

            for (int i = 1; input.MoveToLiteralAttribute(i); i++) {
                if (! input.IsXsltNamespace()) {
                    XslNode att = f.LiteralAttribute(f.QName(input.LocalName, input.NamespaceUri, input.Prefix), input.Value, input.XslVersion);
                    // QilGenerator takes care of AVTs, and needs line info
                    AddInstruction(content, SetLineInfo(att, ctxInfo.lineInfo));
                } else {
                    // ignore all other xslt attributes. See XslInput.GetLiteralAttributes()
                }
            }

            content = LoadEndTag(LoadInstructions(content));
            return SetInfo(f.LiteralElement(f.QName(name, nsUri, prefix)), content, ctxInfo);
        }

        private void CheckWithParam(List<XslNode> content, XslNode withParam) {
            Debug.Assert(content != null && withParam != null);
            Debug.Assert(withParam.NodeType == XslNodeType.WithParam);
            foreach (XslNode node in content) {
                if (node.NodeType == XslNodeType.WithParam && node.Name.Equals(withParam.Name)) {
                    ReportError(/*[XT0670]*/Res.Xslt_DuplicateWithParam, withParam.Name.QualifiedName);
                    break;
                }
            }
        }

        private static void AddInstruction(List<XslNode> content, XslNode instruction) {
            Debug.Assert(content != null);
            if (instruction != null) {
                content.Add(instruction);
            }
        }

        private List<XslNode> LoadEndTag(List<XslNode> content) {
            Debug.Assert(content != null);
            if (compiler.IsDebug && !input.IsEmptyElement) {
                AddInstruction(content, SetLineInfo(f.Nop(), input.BuildLineInfo()));
            }
            return content;
        }

        private XslNode LoadUnknownXsltInstruction(string parentName) {
            input.GetVersionAttribute();
            if (!input.ForwardCompatibility) {
                ReportError(/*[XT_026]*/Res.Xslt_UnexpectedElement, input.QualifiedName, parentName);
                input.SkipNode();
                return null;
            } else {
                ContextInfo ctxInfo = input.GetAttributes();
                List<XslNode> fallbacks = LoadFallbacks(input.LocalName);
                return SetInfo(f.List(), fallbacks, ctxInfo);
            }
        }

        private List<XslNode> LoadFallbacks(string instrName) {
            input.MoveToElement();
            ISourceLineInfo extElmLineInfo = input.BuildNameLineInfo();
            List<XslNode> fallbacksArray = new List<XslNode>();
            // 
            /* Process children */
            if (input.MoveToFirstChild()) {
                do {
                    if (input.IsXsltKeyword(atoms.Fallback)) {
                        ContextInfo ctxInfo = input.GetAttributes();
                        fallbacksArray.Add(SetInfo(f.List(), LoadInstructions(), ctxInfo));
                    } else {
                        input.SkipNode();
                    }
                } while (input.MoveToNextSibling());
            }

            // Generate runtime error if there is no fallbacks
            if (fallbacksArray.Count == 0) {
                fallbacksArray.Add(
                    f.Error(XslLoadException.CreateMessage(extElmLineInfo, Res.Xslt_UnknownExtensionElement, instrName))
                );
            }
            return fallbacksArray;
        }

        // ------------------ little helper methods ---------------------

        // Suppresses errors if FCB is enabled
        private QilName ParseModeAttribute(int attNum) {
            //Debug.Assert(
            //    input.IsXsltKeyword(atoms.ApplyTemplates) ||
            //    input.IsXsltKeyword(atoms.Template) && V1
            //);
            if (! input.MoveToXsltAttribute(attNum, "mode")) {
                return nullMode;
            }
            // mode is always optional attribute
            compiler.EnterForwardsCompatible();
            string qname = input.Value;
            QilName mode;
            if (!V1 && qname == "#default") {
                mode = nullMode;
            } else if (!V1 && qname == "#current") {
                ReportNYI("xsl:apply-templates[@mode='#current']");
                mode = nullMode;
            } else if (!V1 && qname == "#all") {
                ReportError(Res.Xslt_ModeListAll);
                mode = nullMode;
            } else {
                mode = CreateXPathQName(qname);
            }
            if (!compiler.ExitForwardsCompatible(input.ForwardCompatibility)) {
                mode = nullMode;
            }
            return mode;
        }

        // Parse mode when it is list. V1: -, V2: xsl:template
        // Suppresses errors if FCB is enabled
        private QilName ParseModeListAttribute(int attNum) {
            //Debug.Assert(input.IsXsltKeyword(atoms.Template) && !V1);
            if (! input.MoveToXsltAttribute(attNum, "mode")) {
                return nullMode;
            }

            string modeList = input.Value;
            if (modeList == "#all") {
                ReportNYI("xsl:template[@mode='#all']");
                return nullMode;
            } else {
                string[] list = XmlConvert.SplitString(modeList);
                List<QilName> modes = new List<QilName>(list.Length);

                compiler.EnterForwardsCompatible();  // mode is always optional attribute

                if (list.Length == 0) {
                    ReportError(Res.Xslt_ModeListEmpty);
                } else {
                    foreach (string qname in list) {
                        QilName mode;
                        if (qname == "#default") {
                            mode = nullMode;
                        } else if (qname == "#current") {
                            ReportNYI("xsl:apply-templates[@mode='#current']");
                            break;
                        } else if (qname == "#all") {
                            ReportError(Res.Xslt_ModeListAll);
                            break;
                        } else {
                            mode = CreateXPathQName(qname);
                        }
                        bool dup = false;
                        foreach (QilName m in modes) {
                            dup |= m.Equals(mode);
                        }
                        if (dup) {
                            ReportError(Res.Xslt_ModeListDup, qname);
                        } else {
                            modes.Add(mode);
                        }
                    }
                }

                if (!compiler.ExitForwardsCompatible(input.ForwardCompatibility)) {
                    modes.Clear();
                    modes.Add(nullMode);
                }
                if (1 < modes.Count) {
                    ReportNYI("Multipe modes");
                    return nullMode;
                }
                if (modes.Count == 0) {
                    return nullMode;
                }
                return modes[0];
            }
        }

        private string ParseCollationAttribute(int attNum) {
            if (input.MoveToXsltAttribute(attNum, "collation")) {
                ReportNYI("@collation");
            }
            return null;
        }

        // Does not suppress errors
        private bool ResolveQName(bool ignoreDefaultNs, string qname, out string localName, out string namespaceName, out string prefix) {
            if (qname == null) {
                // That means stylesheet is incorrect
                prefix = compiler.PhantomNCName;
                localName = compiler.PhantomNCName;
                namespaceName = compiler.CreatePhantomNamespace();
                return false;
            }
            if (!compiler.ParseQName(qname, out prefix, out localName, (IErrorHelper)this)) {
                namespaceName = compiler.CreatePhantomNamespace();
                return false;
            }
            if (ignoreDefaultNs && prefix.Length == 0) {
                namespaceName = string.Empty;
            } else {
                namespaceName = input.LookupXmlNamespace(prefix);
                if (namespaceName == null) {
                    namespaceName = compiler.CreatePhantomNamespace();
                    return false;
                }
            }
            return true;
        }

        // Does not suppress errors
        private QilName ParseQNameAttribute(int attNum) {
            bool required = input.IsRequiredAttribute(attNum);
            QilName result = null;
            if (!required) {
                compiler.EnterForwardsCompatible();
            }
            if (input.MoveToXsltAttribute(attNum, "name")) {
                string prefix, localName, namespaceName;
                if (ResolveQName(/*ignoreDefaultNs:*/true, input.Value, out localName, out namespaceName, out prefix)) {
                    result = f.QName(localName, namespaceName, prefix);
                }
            }
            if (!required) {
                compiler.ExitForwardsCompatible(input.ForwardCompatibility);
            }
            if (result == null && required) {
                result = f.QName(compiler.PhantomNCName, compiler.CreatePhantomNamespace(), compiler.PhantomNCName);
            }
            return result;
        }

        private string ParseNCNameAttribute(int attNum) {
            Debug.Assert(input.IsRequiredAttribute(attNum), "It happened that @name as NCName is always required attribute");
            if (input.MoveToXsltAttribute(attNum, "name")) {
                return input.Value;
            }
            return compiler.PhantomNCName;
        }

        // Does not suppress errors
        private QilName CreateXPathQName(string qname) {
            string prefix, localName, namespaceName;
            ResolveQName(/*ignoreDefaultNs:*/true, qname, out localName, out namespaceName, out prefix);
            return f.QName(localName, namespaceName, prefix);
        }

        // Does not suppress errors
        private XmlQualifiedName ResolveQName(bool ignoreDefaultNs, string qname) {
            string prefix, localName, namespaceName;
            ResolveQName(ignoreDefaultNs, qname, out localName, out namespaceName, out prefix);
            return new XmlQualifiedName(localName, namespaceName);
        }

        // Does not suppress errors
        private void ParseWhitespaceRules(string elements, bool preserveSpace) {
            if (elements != null && elements.Length != 0) {
                string[] tokens = XmlConvert.SplitString(elements);
                for (int i = 0; i < tokens.Length; i++) {
                    string prefix, localName, namespaceName;
                    if (!compiler.ParseNameTest(tokens[i], out prefix, out localName, (IErrorHelper)this)) {
                        namespaceName = compiler.CreatePhantomNamespace();
                    } else if (prefix == null || prefix.Length == 0) {
                        namespaceName = prefix;
                    } else {
                        namespaceName = input.LookupXmlNamespace(prefix);
                        if (namespaceName == null) {
                            namespaceName = compiler.CreatePhantomNamespace();
                        }
                    }
                    int index = (
                        (localName     == null ? 1 : 0) +
                        (namespaceName == null ? 1 : 0)
                    );
                    curStylesheet.AddWhitespaceRule(index, new WhitespaceRule(localName, namespaceName, preserveSpace));
                }
            }
        }

        // Does not suppress errors.  In case of error, null is returned.
        private XmlQualifiedName ParseOutputMethod(string attValue, out XmlOutputMethod method) {
            string prefix, localName, namespaceName;
            ResolveQName(/*ignoreDefaultNs:*/true, attValue, out localName, out namespaceName, out prefix);
            method = XmlOutputMethod.AutoDetect;

            if (compiler.IsPhantomNamespace(namespaceName)) {
                return null;
            } else if (prefix.Length == 0) {
                switch (localName) {
                case "xml"  : method = XmlOutputMethod.Xml;  break;
                case "html" : method = XmlOutputMethod.Html; break;
                case "text" : method = XmlOutputMethod.Text; break;
                default:
                    ReportError(/*[XT1570]*/Res.Xslt_InvalidAttrValue, "method", attValue);
                    return null;
                }
            } else {
                if (!input.ForwardCompatibility) {
                    ReportWarning(/*[XT1570]*/Res.Xslt_InvalidMethod, attValue);
                }
            }
            return new XmlQualifiedName(localName, namespaceName);
        }

        // Suppresses errors if FCB is enabled
        private void AddUseAttributeSets(List<XslNode> list) {
            Debug.Assert(input.LocalName == "use-attribute-sets", "we are positioned on this attribute");
            Debug.Assert(list != null && list.Count == 0, "It happened that we always add use-attribute-sets first. Otherwise we can't call list.Clear()");

            compiler.EnterForwardsCompatible();
            foreach (string qname in XmlConvert.SplitString(input.Value)) {
                AddInstruction(list, SetLineInfo(f.UseAttributeSet(CreateXPathQName(qname)), input.BuildLineInfo()));
            }
            if (!compiler.ExitForwardsCompatible(input.ForwardCompatibility)) {
                // There were errors in the list, ignore the whole list
                list.Clear();
            }
        }

        private List<QilName> ParseUseCharacterMaps(int attNum) {
            List<QilName> useCharacterMaps = new List<QilName>();
            if (input.MoveToXsltAttribute(attNum, "use-character-maps")) {
                compiler.EnterForwardsCompatible();
                foreach (string qname in XmlConvert.SplitString(input.Value)) {
                    useCharacterMaps.Add(CreateXPathQName(qname));
                }
                if (!compiler.ExitForwardsCompatible(input.ForwardCompatibility)) {
                    useCharacterMaps.Clear(); // There were errors in the list, ignore the whole list
                }
            }
            return useCharacterMaps;
        }

        private string ParseStringAttribute(int attNum, string attName) {
            if (input.MoveToXsltAttribute(attNum, attName)) {
                return input.Value;
            }
            return null;
        }

        private char ParseCharAttribute(int attNum, string attName, char defVal) {
            if (input.MoveToXsltAttribute(attNum, attName)) {
                if (input.Value.Length == 1) {
                    return input.Value[0];
                } else {
                    if (input.IsRequiredAttribute(attNum) || !input.ForwardCompatibility) {
                        ReportError(/*[XT_029]*/Res.Xslt_CharAttribute, attName);
                    }
                }
            }
            return defVal;
        }

        // Suppresses errors if FCB is enabled
        private TriState ParseYesNoAttribute(int attNum, string attName) {
            Debug.Assert(!input.IsRequiredAttribute(attNum), "All Yes/No attributes are optional.");
            if (input.MoveToXsltAttribute(attNum, attName)) {
                switch (input.Value) {
                case "yes" : return TriState.True;
                case "no"  : return TriState.False;
                default:
                    if (!input.ForwardCompatibility) {
                        ReportError(/*[XT_028]*/Res.Xslt_BistateAttribute, attName, "yes", "no");
                    }
                    break;
                }
            }
            return TriState.Unknown;
        }

        private void ParseTypeAttribute(int attNum) {
            Debug.Assert(!input.IsRequiredAttribute(attNum), "All 'type' attributes are optional.");
            if (input.MoveToXsltAttribute(attNum, "type")) {
                CheckError(true, /*[???]*/Res.Xslt_SchemaAttribute, "type");
            }
        }

        private void ParseValidationAttribute(int attNum, bool defVal) {
            Debug.Assert(!input.IsRequiredAttribute(attNum), "All 'validation' attributes are optional.");
            string attributeName = defVal ? atoms.DefaultValidation : "validation";
            if (input.MoveToXsltAttribute(attNum, attributeName)) {
                string value = input.Value;
                if (value == "strip") {
                    // no error
                } else if (
                    value == "preserve" ||
                    value == "strict" && !defVal ||
                    value == "lax" && !defVal
                ) {
                    ReportError(/*[???]*/Res.Xslt_SchemaAttributeValue, attributeName, value);
                } else if (!input.ForwardCompatibility) {
                    ReportError(/*[???]*/Res.Xslt_InvalidAttrValue, attributeName, value);
                }
            }
        }

        private void ParseInputTypeAnnotationsAttribute(int attNum) {
            Debug.Assert(!input.IsRequiredAttribute(attNum), "All 'input-type-validation' attributes are optional.");
            if (input.MoveToXsltAttribute(attNum, "input-type-annotations")) {
                string value = input.Value;
                switch (value) {
                case "unspecified":
                    break;
                case "strip":
                case "preserve":
                    if (compiler.inputTypeAnnotations == null) {
                        compiler.inputTypeAnnotations = value;
                    } else {
                        CheckError(compiler.inputTypeAnnotations != value, /*[XTSE0265]*/Res.Xslt_InputTypeAnnotations);
                    }
                    break;
                default:
                    if (!input.ForwardCompatibility) {
                        ReportError(/*[???]*/Res.Xslt_InvalidAttrValue, "input-type-annotations", value);
                    }
                    break;
                }
            }
        }


        // ToDo: We don't need separation on SkipEmptyContent() and CheckNoContent(). Merge them back when we are done with parsing.
        private void CheckNoContent() {
            input.MoveToElement();
            QName parentName = input.ElementName;
            ISourceLineInfo errorLineInfo = SkipEmptyContent();

            if (errorLineInfo != null) {
                compiler.ReportError(errorLineInfo, /*[XT0260]*/Res.Xslt_NotEmptyContents, parentName);
            }
        }

        // Returns ISourceLineInfo of the first violating (non-whitespace) node, or null otherwise
        private ISourceLineInfo SkipEmptyContent() {
            ISourceLineInfo result = null;

            // Really EMPTY means no content at all, but for the sake of compatibility with MSXML we allow whitespaces
            if (input.MoveToFirstChild()) {
                do {
                    // NOTE: XmlNodeType.SignificantWhitespace are not allowed here
                    if (input.NodeType != XmlNodeType.Whitespace) {
                        if (result == null) {
                            result = input.BuildNameLineInfo();
                        }
                        input.SkipNode();
                    }
                } while (input.MoveToNextSibling());
            }
            return result;
        }

        private static XslNode SetLineInfo(XslNode node, ISourceLineInfo lineInfo) {
            Debug.Assert(node != null);
            node.SourceLine = lineInfo;
            return node;
        }

        private static void SetContent(XslNode node, List<XslNode> content) {
            Debug.Assert(node != null);
            if (content != null && content.Count == 0) {
                content = null; // Actualy we can reuse this ArrayList.
            }
            node.SetContent(content);
        }

        internal static XslNode SetInfo(XslNode to, List<XslNode> content, ContextInfo info) {
            Debug.Assert(to != null);
            to.Namespaces = info.nsList;
            SetContent(to, content);
            SetLineInfo(to, info.lineInfo);
            return to;
        }

        // NOTE! We inverting namespace order that is irelevant for namespace of the same node, but
        // for included styleseets we don't keep stylesheet as a node and adding it's namespaces to
        // each toplevel element by MergeNamespaces().
        // Namespaces of stylesheet can be overriden in template and to make this works correclety we
        // should attache them after NsDec of top level elements.
        // Toplevel element almost never contais NsDecl and in practice node duplication will not happened, but if they have
        // we should copy NsDecls of stylesheet localy in toplevel elements.
        private static NsDecl MergeNamespaces(NsDecl thisList, NsDecl parentList) {
            if (parentList == null) {
                return thisList;
            }
            if (thisList == null) {
                return parentList;
            }
            // Clone all nodes and attache them to nodes of thisList;
            while (parentList != null) {
                bool duplicate = false;
                for (NsDecl tmp = thisList; tmp != null; tmp = tmp.Prev) {
                    if (Ref.Equal(tmp.Prefix, parentList.Prefix) && (
                        tmp.Prefix != null ||           // Namespace declaration
                        tmp.NsUri == parentList.NsUri   // Extension or excluded namespace
                    )) {
                        duplicate = true;
                        break;
                    }
                }
                if (!duplicate) {
                    thisList = new NsDecl(thisList, parentList.Prefix, parentList.NsUri);
                }
                parentList = parentList.Prev;
            }
            return thisList;
        }

        // -------------------------------- IErrorHelper --------------------------------

        public void ReportError(string res, params string[] args) {
            compiler.ReportError(input.BuildNameLineInfo(), res, args);
        }

        public void ReportWarning(string res, params string[] args) {
            compiler.ReportWarning(input.BuildNameLineInfo(), res, args);
        }

        private void ReportNYI(string arg) {
            if (! input.ForwardCompatibility) {
                ReportError(Res.Xslt_NotYetImplemented, arg);
            }
        }

        public void CheckError(bool cond, string res, params string[] args) {
            if (cond) {
                compiler.ReportError(input.BuildNameLineInfo(), res, args);
            }
        }
    }
}
