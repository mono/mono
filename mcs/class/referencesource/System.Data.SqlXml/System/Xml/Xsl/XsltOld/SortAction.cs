//------------------------------------------------------------------------------
// <copyright file="SortAction.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Xml.Xsl.XsltOld {
    using Res = System.Xml.Utils.Res;
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Xml;
    using System.Xml.XPath;

    internal class SortAction : CompiledAction {
        private int selectKey  = Compiler.InvalidQueryKey;
        private Avt langAvt;
        private Avt dataTypeAvt;
        private Avt orderAvt;
        private Avt caseOrderAvt;
        // Compile time precalculated AVTs
        private string        lang;
        private XmlDataType   dataType  = XmlDataType.Text;
        private XmlSortOrder  order     = XmlSortOrder.Ascending;
        private XmlCaseOrder  caseOrder = XmlCaseOrder.None;
        private Sort          sort; //When we not have AVTs at all we can do this. null otherwise.
        private bool      forwardCompatibility;
        private InputScopeManager  manager;

        private string ParseLang(string value) {
            if(value == null) { // Avt is not constant, or attribute wasn't defined
                return null; 
            }
            // XmlComplianceUtil.IsValidLanguageID uses the outdated RFC 1766. It would be
            // better to remove this method completely and not call it here, but that may
            // change exception types for some stylesheets.
            if (! XmlComplianceUtil.IsValidLanguageID(value.ToCharArray(), 0, value.Length)
                && (value.Length == 0 || CultureInfo.GetCultureInfo(value) == null)
            ) {
                if (this.forwardCompatibility) {
                    return null;
                }
                throw XsltException.Create(Res.Xslt_InvalidAttrValue, "lang", value);
            }
            return value;
        }

        private XmlDataType ParseDataType(string value, InputScopeManager manager) {
            if(value == null) { // Avt is not constant, or attribute wasn't defined
                return XmlDataType.Text; 
            }
            if (value == "text") {
                return XmlDataType.Text;
            }
            if (value == "number") {
                return XmlDataType.Number; 
            }
            String prefix, localname;
            PrefixQName.ParseQualifiedName(value, out prefix, out localname);
			manager.ResolveXmlNamespace(prefix);
            if (prefix.Length == 0 && ! this.forwardCompatibility) {
                throw XsltException.Create(Res.Xslt_InvalidAttrValue, "data-type", value); 
            }
            return XmlDataType.Text;
        }

        private XmlSortOrder ParseOrder(string value) {
            if(value == null) { // Avt is not constant, or attribute wasn't defined
                return XmlSortOrder.Ascending; 
            }
            if (value == "ascending") {
                return XmlSortOrder.Ascending;
            }
            if (value == "descending") {
                return XmlSortOrder.Descending;
            }
            if (this.forwardCompatibility) {
                return XmlSortOrder.Ascending;
            }
            throw XsltException.Create(Res.Xslt_InvalidAttrValue, "order", value);        
        }

        private XmlCaseOrder ParseCaseOrder(string value) {
            if(value == null) { // Avt is not constant, or attribute wasn't defined
                return XmlCaseOrder.None; 
            }
            if (value == "upper-first") {
                return XmlCaseOrder.UpperFirst;
            }
            if (value == "lower-first") {
                return XmlCaseOrder.LowerFirst;
            }
            if (this.forwardCompatibility) {
                return XmlCaseOrder.None; 
            }
            throw XsltException.Create(Res.Xslt_InvalidAttrValue, "case-order", value);        
        }
        
        internal override void Compile(Compiler compiler) {
            CompileAttributes(compiler);
            CheckEmpty(compiler);
            if (selectKey == Compiler.InvalidQueryKey) {
                selectKey = compiler.AddQuery(".");
            }

            this.forwardCompatibility = compiler.ForwardCompatibility;
            this.manager = compiler.CloneScopeManager();

            this.lang      = ParseLang(     PrecalculateAvt(ref this.langAvt     ));
            this.dataType  = ParseDataType( PrecalculateAvt(ref this.dataTypeAvt ), manager);
            this.order     = ParseOrder(    PrecalculateAvt(ref this.orderAvt    ));
            this.caseOrder = ParseCaseOrder(PrecalculateAvt(ref this.caseOrderAvt));

            if(this.langAvt == null && this.dataTypeAvt == null && this.orderAvt == null && this.caseOrderAvt == null) {
                this.sort = new Sort(this.selectKey, this.lang, this.dataType, this.order, this.caseOrder);
            }
        }

        internal override bool CompileAttribute(Compiler compiler) {
            string name   = compiler.Input.LocalName;
            string value  = compiler.Input.Value;

            if (Ref.Equal(name, compiler.Atoms.Select)) {
                this.selectKey = compiler.AddQuery(value);
            }
            else if (Ref.Equal(name, compiler.Atoms.Lang)) {
                this.langAvt = Avt.CompileAvt(compiler, value);
            }
            else if (Ref.Equal(name, compiler.Atoms.DataType)) {
                this.dataTypeAvt = Avt.CompileAvt(compiler, value);
            }
            else if (Ref.Equal(name, compiler.Atoms.Order)) {
                this.orderAvt = Avt.CompileAvt(compiler, value);
            }    
            else if (Ref.Equal(name, compiler.Atoms.CaseOrder)) {
                this.caseOrderAvt = Avt.CompileAvt(compiler, value);
            }    
            else {
                return false;
            }
            return true;
        }
        
        internal override void Execute(Processor processor, ActionFrame frame) {
            Debug.Assert(processor != null && frame != null);
            Debug.Assert(frame.State == Initialized);
            
            processor.AddSort(this.sort != null ? 
                this.sort : 
                new Sort(
                    this.selectKey,
                    this.langAvt      == null ? this.lang      : ParseLang(     this.langAvt     .Evaluate(processor, frame)),
                    this.dataTypeAvt  == null ? this.dataType  : ParseDataType( this.dataTypeAvt .Evaluate(processor, frame), manager),
                    this.orderAvt     == null ? this.order     : ParseOrder(    this.orderAvt    .Evaluate(processor, frame)),
                    this.caseOrderAvt == null ? this.caseOrder : ParseCaseOrder(this.caseOrderAvt.Evaluate(processor, frame))
                )
            );
            frame.Finished();
        }
    }
}
