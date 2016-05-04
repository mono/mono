// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace Microsoft.Activities.Presentation.Xaml
{
    using System;
    using System.Activities;
    using System.Activities.Debugger.Symbol;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.ServiceModel.Activities;
    using System.Xaml;
    using System.Xml;

    class DesignTimeXamlWriter : XamlXmlWriter
    {
        //namespaces to ignore (don't load assembilies for) at root node
        HashSet<string> namespacesToIgnore;

        //namespaces we've seen at root level, we use this to figure out appropriate alias for MC namespace
        HashSet<string> rootLevelNamespaces;

        // for duplicate namespace filtering (happens if we're using the local assembly to compile itself)
        HashSet<string> emittedNamespacesInLocalAssembly;

        //For namespace defined in local assembly with assembly info in namespace declaration, we'll strip out the assembly info
        //and hold the namespace temporarily. Before writing the start object, we'll check whether the short version gets written
        //as a separate declaration, if not, we write it out.
        List<NamespaceDeclaration> localNamespacesWithAssemblyInfo;

        WorkflowDesignerXamlSchemaContext schemaContext;

        int currentDepth;
        int debugSymbolDepth;
        bool writeDebugSymbol;
        bool debugSymbolNamespaceAdded;
        bool isWritingElementStyleString;
        internal static readonly string EmptyWorkflowSymbol = (new WorkflowSymbol() { FileName = @"C:\Empty.xaml" }).Encode();
        private bool shouldWriteDebugSymbol;

        public DesignTimeXamlWriter(TextWriter textWriter, WorkflowDesignerXamlSchemaContext context, bool shouldWriteDebugSymbol)
            : this(new NamespaceIndentingXmlWriter(textWriter), context, shouldWriteDebugSymbol)
        {
        }

        DesignTimeXamlWriter(NamespaceIndentingXmlWriter underlyingWriter, WorkflowDesignerXamlSchemaContext context, bool shouldWriteDebugSymbol)
            : base(underlyingWriter, context,
                // Setting AssumeValidInput to true allows to save a document even if it has duplicate members
                new XamlXmlWriterSettings { AssumeValidInput = true })
        {
            underlyingWriter.Parent = this;
            this.namespacesToIgnore = new HashSet<string>();
            this.rootLevelNamespaces = new HashSet<string>();
            this.schemaContext = context;
            this.currentDepth = 0;
            this.shouldWriteDebugSymbol = shouldWriteDebugSymbol;
        }

        public override void WriteNamespace(NamespaceDeclaration namespaceDeclaration)
        {
            if (this.currentDepth == 0)
            {
                //we need to track every namespace alias appeared in root element to figure out right alias for MC namespace
                this.rootLevelNamespaces.Add(namespaceDeclaration.Prefix);

                //Remember namespaces needed to be ignored at top level so we will add ignore attribute for them when we write start object
                if (NameSpaces.ShouldIgnore(namespaceDeclaration.Namespace))
                {
                    this.namespacesToIgnore.Add(namespaceDeclaration.Prefix);
                }

                if (namespaceDeclaration.Namespace == NameSpaces.DebugSymbol)
                {
                    debugSymbolNamespaceAdded = true;
                }
            }

            EmitNamespace(namespaceDeclaration);
        }

        void EmitNamespace(NamespaceDeclaration namespaceDeclaration)
        {
            // Write the namespace, filtering for duplicates in the local assembly because VS might be using it to compile itself.

            if (schemaContext.IsClrNamespaceWithNoAssembly(namespaceDeclaration.Namespace))
            {
                // Might still need to trim a semicolon, even though it shouldn't strictly be there.
                string nonassemblyQualifedNamespace = namespaceDeclaration.Namespace;
                if (nonassemblyQualifedNamespace[nonassemblyQualifedNamespace.Length - 1] == ';')
                {
                    nonassemblyQualifedNamespace = nonassemblyQualifedNamespace.Substring(0, nonassemblyQualifedNamespace.Length - 1);
                    namespaceDeclaration = new NamespaceDeclaration(nonassemblyQualifedNamespace, namespaceDeclaration.Prefix);
                }
                EmitLocalNamespace(namespaceDeclaration);
            }
            else if (schemaContext.IsClrNamespaceInLocalAssembly(namespaceDeclaration.Namespace))
            {
                string nonassemblyQualifedNamespace = schemaContext.TrimLocalAssembly(namespaceDeclaration.Namespace);
                namespaceDeclaration = new NamespaceDeclaration(nonassemblyQualifedNamespace, namespaceDeclaration.Prefix);
                if (this.localNamespacesWithAssemblyInfo == null)
                {
                    this.localNamespacesWithAssemblyInfo = new List<NamespaceDeclaration>();
                }
                this.localNamespacesWithAssemblyInfo.Add(namespaceDeclaration);
            }
            else
            {
                base.WriteNamespace(namespaceDeclaration);
            }
        }

        void EmitLocalNamespace(NamespaceDeclaration namespaceDeclaration)
        {
            if (this.emittedNamespacesInLocalAssembly == null) // lazy initialization
            {
                this.emittedNamespacesInLocalAssembly = new HashSet<string>();
            }

            // Write the namespace only once. Add() returns false if it was already there.
            if (this.emittedNamespacesInLocalAssembly.Add(namespaceDeclaration.Namespace))
            {
                base.WriteNamespace(namespaceDeclaration);
            }
        }

        public override void WriteStartObject(XamlType type)
        {
            if (type.UnderlyingType == typeof(string))
            {
                isWritingElementStyleString = true;
            }
            // this is the top-level object
            if (this.currentDepth == 0)
            {
                if (!this.debugSymbolNamespaceAdded)
                {
                    string sadsNamespaceAlias = GenerateNamespaceAlias(NameSpaces.DebugSymbolPrefix);
                    this.WriteNamespace(new NamespaceDeclaration(NameSpaces.DebugSymbol, sadsNamespaceAlias));
                    this.debugSymbolNamespaceAdded = true;
                }

                // we need to write MC namespace if any namespaces need to be ignored
                if (this.namespacesToIgnore.Count > 0)
                {
                    string mcNamespaceAlias = GenerateNamespaceAlias(NameSpaces.McPrefix);
                    this.WriteNamespace(new NamespaceDeclaration(NameSpaces.Mc, mcNamespaceAlias));
                }


                if (this.localNamespacesWithAssemblyInfo != null)
                {
                    foreach (NamespaceDeclaration xamlNamespace in this.localNamespacesWithAssemblyInfo)
                    {
                        if ((this.emittedNamespacesInLocalAssembly == null) || (!this.emittedNamespacesInLocalAssembly.Contains(xamlNamespace.Namespace)))
                        {
                            base.WriteNamespace(xamlNamespace);
                        }
                    }
                }

                if ((type.UnderlyingType == typeof(Activity)) ||
                    (type.IsGeneric && type.UnderlyingType != null && type.UnderlyingType.GetGenericTypeDefinition() == typeof(Activity<>)) ||
                    (type.UnderlyingType == typeof(WorkflowService)))
                {   // Exist ActivityBuilder, DebugSymbolObject will be inserted at the depth == 1.
                    debugSymbolDepth = 1;
                }
                else
                {
                    debugSymbolDepth = 0;
                }
            }

            if (this.currentDepth == debugSymbolDepth)
            {
                if (type.UnderlyingType != null && type.UnderlyingType.IsSubclassOf(typeof(Activity)) && this.shouldWriteDebugSymbol)
                {
                    this.writeDebugSymbol = true;
                }
            }

            base.WriteStartObject(type);

            if (this.currentDepth == 0)
            {
                // we need to add Ignore attribute for all namespaces which we don't want to load assemblies for
                // this has to be done after WriteStartObject
                if (this.namespacesToIgnore.Count > 0)
                {
                    string nsString = null;
                    foreach (string ns in this.namespacesToIgnore)
                    {
                        if (nsString == null)
                        {
                            nsString = ns;
                        }
                        else
                        {
                            nsString += " " + ns;
                        }
                    }

                    XamlDirective ignorable = new XamlDirective(NameSpaces.Mc, "Ignorable");
                    base.WriteStartMember(ignorable);
                    base.WriteValue(nsString);
                    base.WriteEndMember();
                    this.namespacesToIgnore.Clear();
                }
            }

            ++this.currentDepth;

        }

        public override void WriteGetObject()
        {
            ++this.currentDepth;
            base.WriteGetObject();
        }

        public override void WriteEndObject()
        {
            --this.currentDepth;
            SharedFx.Assert(this.currentDepth >= 0, "Unmatched WriteEndObject");
            if (this.currentDepth == this.debugSymbolDepth && this.writeDebugSymbol)
            {
                base.WriteStartMember(new XamlMember(DebugSymbol.SymbolName.MemberName,
                   this.SchemaContext.GetXamlType(typeof(DebugSymbol)), true));
                base.WriteValue(EmptyWorkflowSymbol);
                base.WriteEndMember();
                this.writeDebugSymbol = false;
            }
            base.WriteEndObject();
            isWritingElementStyleString = false;
        }

        string GenerateNamespaceAlias(string prefix)
        {
            string aliasPostfix = string.Empty;
            //try "mc"~"mc1000" first
            for (int i = 1; i <= 1000; i++)
            {
                string mcAlias = prefix + aliasPostfix;
                if (!this.rootLevelNamespaces.Contains(mcAlias))
                {
                    return mcAlias;
                }
                aliasPostfix = i.ToString(CultureInfo.InvariantCulture);
            }

            //roll the dice
            return prefix + Guid.NewGuid().ToString();
        }

        class NamespaceIndentingXmlWriter : XmlTextWriter
        {
            int currentDepth;
            TextWriter textWriter;

            public NamespaceIndentingXmlWriter(TextWriter textWriter)
                : base(textWriter)
            {
                this.textWriter = textWriter;
                this.Formatting = Formatting.Indented;
            }

            public DesignTimeXamlWriter Parent { get; set; }

            public override void WriteStartElement(string prefix, string localName, string ns)
            {
                base.WriteStartElement(prefix, localName, ns);
                this.currentDepth++;
            }

            public override void WriteStartAttribute(string prefix, string localName, string ns)
            {
                if (prefix == "xmlns" && (this.currentDepth == 1))
                {
                    this.textWriter.Write(new char[] { '\r', '\n' });
                }
                base.WriteStartAttribute(prefix, localName, ns);
            }

            public override void WriteEndElement()
            {
                if (this.Parent.isWritingElementStyleString)
                {
                    base.WriteRaw(string.Empty);
                }
                base.WriteEndElement();
                this.currentDepth--;
            }

            public override void WriteStartDocument()
            {
                // No-op to avoid XmlDeclaration from being written.
                // Overriding this is equivalent of XmlWriterSettings.OmitXmlDeclaration = true.
            }

            public override void WriteStartDocument(bool standalone)
            {
                // No-op to avoid XmlDeclaration from being written.
                // Overriding this is equivalent of XmlWriterSettings.OmitXmlDeclaration = true.
            }

            public override void WriteEndDocument()
            {
                // No-op to avoid end of XmlDeclaration from being written.
                // Overriding this is equivalent of XmlWriterSettings.OmitXmlDeclaration = true.
            }
        }
    }
}
