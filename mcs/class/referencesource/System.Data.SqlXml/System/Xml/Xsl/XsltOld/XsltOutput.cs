//------------------------------------------------------------------------------
// <copyright file="XsltOutput.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Xml.Xsl.XsltOld {
    using Res = System.Xml.Utils.Res;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Xml;
    using System.Text;
    using System.Collections;

    internal class XsltOutput : CompiledAction {

        internal enum OutputMethod {
            Xml,
            Html,
            Text,
            Other,
            Unknown,
        }

        private OutputMethod        method             = OutputMethod.Unknown;
        private int                 methodSId          = int.MaxValue;
        private Encoding            encoding           = System.Text.Encoding.UTF8;
        private int                 encodingSId        = int.MaxValue;
        private string              version;
        private int                 versionSId         = int.MaxValue;
        private bool                omitXmlDecl;
        private int                 omitXmlDeclSId     = int.MaxValue;
        private bool                standalone;
        private int                 standaloneSId      = int.MaxValue;
        private string              doctypePublic;
        private int                 doctypePublicSId   = int.MaxValue;
        private string              doctypeSystem;
        private int                 doctypeSystemSId   = int.MaxValue;
        private bool                indent;
        private int                 indentSId          = int.MaxValue;
        private string              mediaType          = "text/html";
        private int                 mediaTypeSId       = int.MaxValue;
        private Hashtable           cdataElements;

        internal OutputMethod Method {
            get { return this.method; }
        }

        internal bool OmitXmlDeclaration {
            get { return this.omitXmlDecl; }
        }

        internal bool HasStandalone {
            get { return this.standaloneSId != int.MaxValue; }
        }

        internal bool Standalone {
            get { return this.standalone; }
        }

        internal string DoctypePublic {
            get { return this.doctypePublic; }
        }

        internal string DoctypeSystem {
            get { return this.doctypeSystem; }
        }

        internal Hashtable CDataElements {
            get { return this.cdataElements; }
        }

        internal bool Indent {
            get { return this.indent; }
        }

        internal Encoding Encoding {
            get { return this.encoding; }
        }

        internal string MediaType {
            get { return this.mediaType; }
        }

        internal XsltOutput CreateDerivedOutput(OutputMethod method) {
            XsltOutput output = (XsltOutput) MemberwiseClone();
            output.method = method;
            if (method == OutputMethod.Html && this.indentSId == int.MaxValue) { // HTML output and Ident wasn't specified
                output.indent = true;
            }
            return output;
        }

        internal override void Compile(Compiler compiler) {
            CompileAttributes(compiler);
            CheckEmpty(compiler);
        }

        internal override bool CompileAttribute(Compiler compiler) {
            string name   = compiler.Input.LocalName;
            string value  = compiler.Input.Value;

            if (Ref.Equal(name, compiler.Atoms.Method)) {
                if (compiler.Stylesheetid <= this.methodSId) {
                    this.method    = ParseOutputMethod(value, compiler);
                    this.methodSId = compiler.Stylesheetid;
                    if (this.indentSId == int.MaxValue) {
                        this.indent = (this.method == OutputMethod.Html);
                    }
                }
            }
            else if (Ref.Equal(name, compiler.Atoms.Version)) {
                if (compiler.Stylesheetid <= this.versionSId) {
                    this.version    = value;
                    this.versionSId = compiler.Stylesheetid;
                }
            }
            else if (Ref.Equal(name, compiler.Atoms.Encoding)) {
                if (compiler.Stylesheetid <= this.encodingSId) {
                    try {
                        this.encoding    = System.Text.Encoding.GetEncoding(value);
                        this.encodingSId = compiler.Stylesheetid;
                    }
                    catch (System.NotSupportedException) {}
                    catch (System.ArgumentException) {}
                    Debug.Assert(this.encoding != null);
                }
            }
            else if (Ref.Equal(name, compiler.Atoms.OmitXmlDeclaration)) {
                if (compiler.Stylesheetid <= this.omitXmlDeclSId) {
                    this.omitXmlDecl    = compiler.GetYesNo(value);
                    this.omitXmlDeclSId = compiler.Stylesheetid;
                }
            }
            else if (Ref.Equal(name, compiler.Atoms.Standalone)) {
                if (compiler.Stylesheetid <= this.standaloneSId) {
                    this.standalone    = compiler.GetYesNo(value);
                    this.standaloneSId = compiler.Stylesheetid;
                }
            }
            else if (Ref.Equal(name, compiler.Atoms.DocTypePublic)) {
                if (compiler.Stylesheetid <= this.doctypePublicSId) {
                    this.doctypePublic    = value;
                    this.doctypePublicSId = compiler.Stylesheetid;
                }
            }
            else if (Ref.Equal(name, compiler.Atoms.DocTypeSystem)) {
                if (compiler.Stylesheetid <= this.doctypeSystemSId) {
                    this.doctypeSystem    = value;
                    this.doctypeSystemSId = compiler.Stylesheetid;
                }
            }
            else if (Ref.Equal(name, compiler.Atoms.Indent)) {
                if (compiler.Stylesheetid <= this.indentSId) {
                    this.indent    = compiler.GetYesNo(value);
                    this.indentSId = compiler.Stylesheetid;
                }
            }
            else if (Ref.Equal(name, compiler.Atoms.MediaType)) {
                if (compiler.Stylesheetid <= this.mediaTypeSId) {
                    this.mediaType    = value;
                    this.mediaTypeSId = compiler.Stylesheetid;
                }
            }
            else if (Ref.Equal(name, compiler.Atoms.CDataSectionElements)) {
                string[] qnames = XmlConvert.SplitString(value);

                if (this.cdataElements == null) {
                    this.cdataElements = new Hashtable(qnames.Length);
                }

                for (int i = 0; i < qnames.Length; i++) {
                    XmlQualifiedName qname = compiler.CreateXmlQName(qnames[i]);
                    this.cdataElements[qname] = qname;
                }
            }
            else {
                return false;
            }
            return true;
        }

        internal override void Execute(Processor processor, ActionFrame frame) {
            Debug.Assert(false);
        }

        private static OutputMethod ParseOutputMethod(string value, Compiler compiler) {
            XmlQualifiedName method = compiler.CreateXPathQName(value);
            if(method.Namespace.Length != 0) {
                return OutputMethod.Other;
            }
            switch(method.Name) {
            case "xml"  :
                return OutputMethod.Xml ;
            case "html" :
                return OutputMethod.Html;
            case "text" :
                return OutputMethod.Text;
            default :
                if (compiler.ForwardCompatibility) {
                    return OutputMethod.Unknown;
                }
                throw XsltException.Create(Res.Xslt_InvalidAttrValue, "method", value);
            }
        }
    }
}
