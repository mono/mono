//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System;
    using System.CodeDom.Compiler;
    using System.CodeDom;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.ServiceModel;
    using System.Runtime.Serialization;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using WsdlNS = System.Web.Services.Description;

    public class XmlSerializerImportOptions
    {
        CodeCompileUnit codeCompileUnit;
        CodeDomProvider codeProvider;
        string clrNamespace;
        WsdlNS.WebReferenceOptions webReferenceOptions;
        static CodeGenerationOptions defaultCodeGenerationOptions = CodeGenerationOptions.GenerateProperties | CodeGenerationOptions.GenerateOrder;

        public XmlSerializerImportOptions()
            : this(new CodeCompileUnit())
        {
        }

        public XmlSerializerImportOptions(CodeCompileUnit codeCompileUnit)
        {
            this.codeCompileUnit = codeCompileUnit;
        }

        public CodeCompileUnit CodeCompileUnit
        {
            get
            {
                if (codeCompileUnit == null)
                    codeCompileUnit = new CodeCompileUnit();
                return codeCompileUnit;
            }
        }

        public CodeDomProvider CodeProvider
        {
            get
            {
                if (codeProvider == null)
                    codeProvider = CodeDomProvider.CreateProvider("C#");
                return codeProvider;
            }
            set { codeProvider = value; }
        }

        public string ClrNamespace
        {
            get { return clrNamespace; }
            set { clrNamespace = value; }
        }

        public WsdlNS.WebReferenceOptions WebReferenceOptions
        {
            get
            {
                if (webReferenceOptions == null)
                {
                    webReferenceOptions = new WsdlNS.WebReferenceOptions();
                    webReferenceOptions.CodeGenerationOptions = defaultCodeGenerationOptions;
                }
                return webReferenceOptions;
            }
            set { webReferenceOptions = value; }
        }
    }
}
