// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using MS.Internal.Xaml.Parser;

namespace System.Xaml
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Collections;
    using System.Text;
    using System.Globalization;
    using System.Xaml.MS.Impl;
    using System.Reflection;
    using System.ComponentModel;
    using System.Windows.Markup;
    using System.Xaml.Schema;

    public class XamlXmlWriter : XamlWriter
    {
        // Each state of the writer is represented by a singleton that
        // implements an abstract class.
        //
        WriterState currentState;

        XmlWriter output;
        XamlXmlWriterSettings settings;

        Stack<Frame> namespaceScopes;

        // A stack of lists that stores nodes we have tried to write in curly form so far.
        // Each list keeps track of the nodes in a markup extension
        Stack<List<XamlNode>> meNodesStack;
        XamlMarkupExtensionWriter meWriter;

        PositionalParameterStateInfo ppStateInfo;

        string deferredValue;
        bool deferredValueIsME;
        bool isFirstElementOfWhitespaceSignificantCollection;

        XamlSchemaContext schemaContext;

        // a dictionary that keeps track of all the mappings from prefixes to namespaces
        // in the entire writing history.  If a prefix is used for two different namespaces
        // (in different scopes), then the entry for the prefix in the dictionary is null
        Dictionary<string, string> prefixAssignmentHistory;

        public XamlXmlWriter(Stream stream, XamlSchemaContext schemaContext)
            : this(stream, schemaContext, null)
        {
        }

        public XamlXmlWriter(Stream stream, XamlSchemaContext schemaContext, XamlXmlWriterSettings settings)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            if (settings != null && settings.CloseOutput == true)
            {
                InitializeXamlXmlWriter(XmlWriter.Create(stream, new XmlWriterSettings { CloseOutput = true }), schemaContext, settings);
            }
            else
            {
                InitializeXamlXmlWriter(XmlWriter.Create(stream), schemaContext, settings);
            }
        }

        public XamlXmlWriter(TextWriter textWriter, XamlSchemaContext schemaContext)
            : this(textWriter, schemaContext, null)
        {
        }

        public XamlXmlWriter(TextWriter textWriter, XamlSchemaContext schemaContext, XamlXmlWriterSettings settings)
        {
            if (textWriter == null)
            {
                throw new ArgumentNullException("textWriter");
            }

            if (settings != null && settings.CloseOutput == true)
            {
                InitializeXamlXmlWriter(XmlWriter.Create(textWriter, new XmlWriterSettings { CloseOutput = true }), schemaContext, settings);
            }
            else
            {
                InitializeXamlXmlWriter(XmlWriter.Create(textWriter), schemaContext, settings);
            }
        }

        public XamlXmlWriter(XmlWriter xmlWriter, XamlSchemaContext schemaContext)
            : this(xmlWriter, schemaContext, null)
        {
        }

        public XamlXmlWriter(XmlWriter xmlWriter, XamlSchemaContext schemaContext, XamlXmlWriterSettings settings)
        {
            if (xmlWriter == null)
            {
                throw new ArgumentNullException("xmlWriter");
            }

            InitializeXamlXmlWriter(xmlWriter, schemaContext, settings);
        }

        void InitializeXamlXmlWriter(XmlWriter xmlWriter, XamlSchemaContext schemaContext, XamlXmlWriterSettings settings)
        {
            if (schemaContext == null)
            {
                throw new ArgumentNullException("schemaContext");
            }
            this.schemaContext = schemaContext;

            this.output = xmlWriter;
            this.settings = settings == null ? new XamlXmlWriterSettings() : settings.Copy() as XamlXmlWriterSettings;

            this.currentState = Start.State;

            this.namespaceScopes = new Stack<Frame>();
            this.namespaceScopes.Push(new Frame { AllocatingNodeType = XamlNodeType.StartObject });

            this.prefixAssignmentHistory = new Dictionary<string, string>() { {"xml", XamlLanguage.Xml1998Namespace} };
            this.meNodesStack = new Stack<List<XamlNode>>();
            this.meWriter = new XamlMarkupExtensionWriter(this);

            this.ppStateInfo = new PositionalParameterStateInfo(this);
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && !IsDisposed)
                {
                    if (this.settings.CloseOutput)
                    {
                        this.output.Close();
                    }
                    else
                    {
                        Flush();
                    }
                    ((IDisposable)meWriter).Dispose();
                }
            }
            finally
            {
                ((IDisposable)this.output).Dispose();
                base.Dispose(disposing);
            }
        }

        public void Flush()
        {
            this.output.Flush();
        }

        public override void WriteGetObject()
        {
            CheckIsDisposed();

            XamlType type = null;
            Frame frame = namespaceScopes.Peek();

            if (frame.AllocatingNodeType == XamlNodeType.StartMember)
            {
                type = frame.Member.Type;
            }

            this.currentState.WriteObject(this, type, true);
        }

        public override void WriteStartObject(XamlType type)
        {
            CheckIsDisposed();

            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (!type.IsNameValid)
            {
                throw new ArgumentException(SR.Get(SRID.TypeHasInvalidXamlName, type.Name), "type");
            }

            this.currentState.WriteObject(this, type, false);

            if (type.TypeArguments != null)
            {
                WriteTypeArguments(type);
            }
        }

        public override void WriteEndObject()
        {
            CheckIsDisposed();
            this.currentState.WriteEndObject(this);
        }

        public override void WriteStartMember(XamlMember property)
        {
            CheckIsDisposed();

            if (property == null)
            {
                throw new ArgumentNullException("property");
            }

            if (!property.IsNameValid)
            {
                throw new ArgumentException(SR.Get(SRID.MemberHasInvalidXamlName, property.Name), "property");
            }

            this.currentState.WriteStartMember(this, property);
        }

        public override void WriteEndMember()
        {
            CheckIsDisposed();
            this.currentState.WriteEndMember(this);
        }

        public override void WriteValue(object value)
        {
            CheckIsDisposed();
            if (value == null)
            {
                WriteStartObject(XamlLanguage.Null);
                WriteEndObject();
            }
            else
            {
                string s = value as string;
                if (s == null)
                {
                    throw new ArgumentException(SR.Get(SRID.XamlXmlWriterCannotWriteNonstringValue), "value");
                }
                this.currentState.WriteValue(this, s);
            }
        }

        public override void WriteNamespace(NamespaceDeclaration namespaceDeclaration)
        {
            CheckIsDisposed();

            if (namespaceDeclaration == null)
            {
                throw new ArgumentNullException("namespaceDeclaration");
            }

            if (namespaceDeclaration.Prefix == null)
            {
                throw new ArgumentException(SR.Get(SRID.NamespaceDeclarationPrefixCannotBeNull), "namespaceDeclaration");
            }

            if (namespaceDeclaration.Namespace == null)
            {
                throw new ArgumentException(SR.Get(SRID.NamespaceDeclarationNamespaceCannotBeNull), "namespaceDeclaration");
            }

            if (namespaceDeclaration.Prefix == "xml")
            {
                throw new ArgumentException(SR.Get(SRID.NamespaceDeclarationCannotBeXml), "namespaceDeclaration");
            }

            this.currentState.WriteNamespace(this, namespaceDeclaration);
        }

        public XamlXmlWriterSettings Settings
        {
            get { return this.settings.Copy() as XamlXmlWriterSettings; }
        }

        public override XamlSchemaContext SchemaContext
        {
            get
            {
                return schemaContext;
            }
        }

        void CheckIsDisposed()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException("XamlXmlWriter");
            }
        }

        static bool StringStartsWithCurly(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return false;
            }
            if (s[0] == '{')
            {
                return true;
            }
            return false;
        }

        // Implicit directives are part of the node-stream, but not the textual representation
        internal static bool IsImplicit(XamlMember xamlMember)
        {
            return xamlMember.IsDirective &&
                (xamlMember == XamlLanguage.Items ||
                 xamlMember == XamlLanguage.Initialization ||
                 xamlMember == XamlLanguage.PositionalParameters ||
                 xamlMember == XamlLanguage.UnknownContent);
        }

        internal static bool HasSignificantWhitespace(string s)
        {
            if (s == String.Empty)
            {
                return false;
            }
            return ContainsLeadingSpace(s)
                || ContainsTrailingSpace(s)
                || ContainsConsecutiveInnerSpaces(s)
                || ContainsWhitespaceThatIsNotSpace(s);
        }

        internal static bool ContainsLeadingSpace(string s)
        {
            return s[0] == KnownStrings.SpaceChar;
        }

        internal static bool ContainsTrailingSpace(string s)
        {
            return s[s.Length - 1] == KnownStrings.SpaceChar;
        }

        internal static bool ContainsConsecutiveInnerSpaces(string s)
        {
            for (int i = 1; i < s.Length - 1; i++)
            {
                if (s[i] == KnownStrings.SpaceChar && s[i + 1] == KnownStrings.SpaceChar)
                {
                    return true;
                }
            }
            return false;
        }

        internal static bool ContainsWhitespaceThatIsNotSpace(string s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == KnownStrings.TabChar || s[i] == KnownStrings.NewlineChar || s[i] == KnownStrings.ReturnChar)
                {
                    return true;
                }
            }
            return false;
        }

        static void WriteXmlSpace(XamlXmlWriter writer)
        {
            writer.output.WriteAttributeString("xml", "space", "http://www.w3.org/XML/1998/namespace", "preserve");
        }

        static XamlType GetContainingXamlType(XamlXmlWriter writer)
        {
            Debug.Assert(writer.namespaceScopes.Peek().AllocatingNodeType == XamlNodeType.StartMember);
            Stack<Frame>.Enumerator enumerator = writer.namespaceScopes.GetEnumerator();
            XamlType containingXamlType = null;
            while (enumerator.MoveNext())
            {
                if (enumerator.Current.AllocatingNodeType == XamlNodeType.StartMember
                    && enumerator.Current.Member != XamlLanguage.Items)
                {
                    containingXamlType = (enumerator.Current.Member == null) || enumerator.Current.Member.IsUnknown ? null : enumerator.Current.Member.Type;
                    break;
                }
                else if (enumerator.Current.AllocatingNodeType == XamlNodeType.StartObject)
                {
                    containingXamlType = enumerator.Current.Type;
                    break;
                }
            }
            return containingXamlType;
        }

        void AssignNamespacePrefix(string ns, string prefix)
        {
            namespaceScopes.Peek().AssignNamespacePrefix(ns, prefix);

            string registeredNamespace;
            if (prefixAssignmentHistory.TryGetValue(prefix, out registeredNamespace))
            {
                if (registeredNamespace != ns)
                {
                    prefixAssignmentHistory[prefix] = null;
                }
            }
            else
            {
                prefixAssignmentHistory.Add(prefix, ns);
            }
        }

        bool IsShadowed(string ns, string prefix)
        {
            Debug.Assert(ns != null);
            Debug.Assert(prefix != null);

            string registeredNamespace;
            foreach (Frame frame in namespaceScopes)
            {
                if (frame.TryLookupNamespace(prefix, out registeredNamespace))
                {
                    return (registeredNamespace != ns);
                }
            }

            throw new InvalidOperationException(SR.Get(SRID.PrefixNotInFrames, prefix));
        }

        //
        // FindPrefix attempts to look up existing prefixes for the namespaces;
        // if none is found, it will define one for the first namespace in the list.
        // Caveat: if the prefix found is shadowed (by a re-definition), FindPrefix will
        // redefine it.
        //
        string FindPrefix(IList<string> namespaces, out string chosenNamespace)
        {
            string prefix = LookupPrefix(namespaces, out chosenNamespace);

            if (prefix == null)
            {
                chosenNamespace = namespaces[0];
                prefix = DefinePrefix(chosenNamespace);
                AssignNamespacePrefix(chosenNamespace, prefix);
            }
            else if (IsShadowed(chosenNamespace, prefix))
            {
                prefix = DefinePrefix(chosenNamespace);
                AssignNamespacePrefix(chosenNamespace, prefix);
            }

            return prefix;
        }

        //
        // LookupPrefix searches up down the stack, one frame at at time, for prefixes
        // that were defined for the namespaces.  If such a prefix is found,
        // the prefix is returned and the corresponding namespace is the out parameter "chosenNamespace".
        // Otherwise, the function returns null
        //
        internal string LookupPrefix(IList<string> namespaces, out string chosenNamespace)
        {
            string prefix;
            chosenNamespace = null;

            foreach (Frame frame in namespaceScopes)
            {
                foreach (string ns in namespaces)
                {
                    if (frame.TryLookupPrefix(ns, out prefix))
                    {
                        chosenNamespace = ns;
                        return prefix;
                    }
                }
            }
            return null;
        }

        bool IsPrefixEverUsedForAnotherNamespace(string prefix, string ns)
        {
            string registeredNamespace;
            return (prefixAssignmentHistory.TryGetValue(prefix, out registeredNamespace) && (ns != registeredNamespace));
        }

        //
        // DefinePrefix algorithmically generates a "good" prefix for the namespace in question.
        // Caveat: if the default prefix has never been used in the xaml document, DefinePrefix
        // chooses it.
        //
        string DefinePrefix(string ns)
        {
            // default namespace takes precedance if it has not been used, or has been used for the same namespace
            if (!IsPrefixEverUsedForAnotherNamespace(String.Empty, ns))
            {
                return String.Empty;
            }

            string basePrefix = SchemaContext.GetPreferredPrefix(ns);

            string prefix = basePrefix;

            int index = 0;

            while (IsPrefixEverUsedForAnotherNamespace(prefix, ns))
            {
                index += 1;
                prefix = basePrefix + index.ToString(TypeConverterHelper.InvariantEnglishUS);
            }

            if (prefix != String.Empty)
            {
                XmlConvert.VerifyNCName(prefix);
            }

            return prefix;
        }

        void CheckMemberForUniqueness(XamlMember property)
        {
            // If we're not assuming the input is valid, then we need to do the checking...
            if (!this.settings.AssumeValidInput)
            {
                // Find the top most object frame.
                Frame objectFrame = namespaceScopes.Peek();
                if (objectFrame.AllocatingNodeType != XamlNodeType.StartObject &&
                    objectFrame.AllocatingNodeType != XamlNodeType.GetObject)
                {
                    Frame temp = namespaceScopes.Pop();
                    objectFrame = namespaceScopes.Peek();
                    namespaceScopes.Push(temp);
                }

                Debug.Assert(objectFrame.AllocatingNodeType == XamlNodeType.StartObject ||
                             objectFrame.AllocatingNodeType == XamlNodeType.GetObject);

                if (objectFrame.Members == null)
                {
                    objectFrame.Members = new XamlPropertySet();
                }
                else if (objectFrame.Members.Contains(property))
                {
                    throw new XamlXmlWriterException(SR.Get(SRID.XamlXmlWriterDuplicateMember, property.Name));
                }
                objectFrame.Members.Add(property);
            }
        }

        void WriteDeferredNamespaces(XamlNodeType nodeType)
        {
            Frame frame = namespaceScopes.Peek();
            if (frame.AllocatingNodeType != nodeType)
            {
                Frame temp = namespaceScopes.Pop();
                frame = namespaceScopes.Peek();
                Debug.Assert(frame.AllocatingNodeType == nodeType);
                namespaceScopes.Push(temp);
            }

            var prefixMap = frame.GetSortedPrefixMap();

            foreach (var pair in prefixMap)
            {
                output.WriteAttributeString("xmlns", pair.Key, null, pair.Value);
            }
        }

        void WriteTypeArguments(XamlType type)
        {
            if (TypeArgumentsContainNamespaceThatNeedsDefinition(type))
            {
                WriteUndefinedNamespaces(type);
            }

            WriteStartMember(XamlLanguage.TypeArguments);
            WriteValue(BuildTypeArgumentsString(type.TypeArguments));
            WriteEndMember();
        }

        void WriteUndefinedNamespaces(XamlType type)
        {
            string chosenNamespace;
            var namespaces = type.GetXamlNamespaces();
            string prefix = LookupPrefix(namespaces, out chosenNamespace);

            if (prefix == null)
            {
                chosenNamespace = namespaces[0];
                prefix = DefinePrefix(chosenNamespace);
                this.currentState.WriteNamespace(this, new NamespaceDeclaration(chosenNamespace, prefix));
            }
            else if (IsShadowed(chosenNamespace, prefix))
            {
                prefix = DefinePrefix(chosenNamespace);
                this.currentState.WriteNamespace(this, new NamespaceDeclaration(chosenNamespace, prefix));
            }

            if (type.TypeArguments != null)
            {
                foreach (XamlType arg in type.TypeArguments)
                {
                    WriteUndefinedNamespaces(arg);
                }
            }
        }

        bool TypeArgumentsContainNamespaceThatNeedsDefinition(XamlType type)
        {
            string chosenNamespace;
            string prefix = LookupPrefix(type.GetXamlNamespaces(), out chosenNamespace);

            if (prefix == null || IsShadowed(chosenNamespace, prefix))
            {
                // if we found a namespace that is not previously defined,
                // or a namespace with prefix that is shadowed

                return true;
            }

            if (type.TypeArguments != null)
            {
                foreach (XamlType arg in type.TypeArguments)
                {
                    if (TypeArgumentsContainNamespaceThatNeedsDefinition(arg))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        string BuildTypeArgumentsString(IList<XamlType> typeArguments)
        {
            var builder = new StringBuilder();
            foreach (XamlType type in typeArguments)
            {
                if (builder.Length != 0)
                {
                    builder.Append(", ");
                }

                builder.Append(ConvertXamlTypeToString(type));
            }

            return builder.ToString();
        }

        string ConvertXamlTypeToString(XamlType typeArgument)
        {
            var builder = new StringBuilder();
            ConvertXamlTypeToStringHelper(typeArgument, builder);
            return builder.ToString();
        }

        void ConvertXamlTypeToStringHelper(XamlType type, StringBuilder builder)
        {
            string chosenNamespace;
            string prefix = LookupPrefix(type.GetXamlNamespaces(), out chosenNamespace);
            string typeName = XamlXmlWriter.GetTypeName(type);
            string typeNamePrefixed = (prefix == String.Empty) ? typeName : prefix + ":" + typeName;

            // save the subscript
            string subscript;
            typeNamePrefixed = GenericTypeNameScanner.StripSubscript(typeNamePrefixed, out subscript);

            builder.Append(typeNamePrefixed);
            if (type.TypeArguments != null)
            {
                bool added = false;
                builder.Append("(");
                foreach (XamlType arg in type.TypeArguments)
                {
                    if (added)
                    {
                        builder.Append(", ");
                    }
                    ConvertXamlTypeToStringHelper(arg, builder);
                    added = true;
                }
                builder.Append(")");
            }

            // re-attach the subscript
            if (subscript != null)
            {
                builder.Append(subscript);
            }
        }

        static internal string GetTypeName(XamlType type)
        {
            string typeName = type.Name;
            if (type.IsMarkupExtension && type.Name.EndsWith("Extension", false, TypeConverterHelper.InvariantEnglishUS))
            {
                typeName = type.Name.Substring(0, type.Name.Length - "Extension".Length);
            }
            return typeName;
        }

        class Frame
        {
            Dictionary<string, string> namespaceMap = new Dictionary<string, string>(); //namespace to prefix map
            Dictionary<string, string> prefixMap = new Dictionary<string, string>(); //prefix to namespace map

            public XamlType Type
            {
                get;
                set;
            }

            public XamlMember Member
            {
                get;
                set;
            }

            public XamlPropertySet Members
            {
                get;
                set;
            }

            public XamlNodeType AllocatingNodeType
            {
                get;
                set;
            }

            public bool IsObjectFromMember
            {
                get;
                set;
            }

            public bool IsContent
            {
                get;
                set;
            }

            public bool TryLookupPrefix(string ns, out string prefix)
            {
                if (ns == XamlLanguage.Xml1998Namespace)
                {
                    prefix = "xml";
                    return true;
                }
                return namespaceMap.TryGetValue(ns, out prefix);
            }

            public bool TryLookupNamespace(string prefix, out string ns)
            {
                if (prefix == "xml")
                {
                    ns = XamlLanguage.Xml1998Namespace;
                    return true;
                }
                return prefixMap.TryGetValue(prefix, out ns);
            }

            public void AssignNamespacePrefix(string ns, string prefix)
            {
                if (prefixMap.ContainsKey(prefix))
                {
                    // we don't allow re-defining the same prefix-to-namespace mapping twice
                    throw new XamlXmlWriterException(SR.Get(SRID.XamlXmlWriterPrefixAlreadyDefinedInCurrentScope, prefix));
                }

                if (namespaceMap.ContainsKey(ns))
                {
                    throw new XamlXmlWriterException(SR.Get(SRID.XamlXmlWriterNamespaceAlreadyHasPrefixInCurrentScope, ns));
                }

                prefixMap[prefix] = ns;
                namespaceMap[ns] = prefix;
            }

            public bool IsEmpty()
            {
                return (namespaceMap.Count == 0);
            }

            public List<KeyValuePair<string, string>> GetSortedPrefixMap()
            {
                List<KeyValuePair<string, string>> prefixMapList = new List<KeyValuePair<string, string>>();
                foreach (var pair in prefixMap)
                {
                    prefixMapList.Add(pair);
                }
                prefixMapList.Sort(CompareByKey);
                return prefixMapList;
            }

            static int CompareByKey(KeyValuePair<string, string> x, KeyValuePair<string, string> y)
            {
                return string.Compare(x.Key, y.Key, false, TypeConverterHelper.InvariantEnglishUS);
            }
        }

        abstract class WriterState
        {
            public virtual void WriteObject(XamlXmlWriter writer, XamlType type, bool isObjectFromMember)
            {
                throw new XamlXmlWriterException(SR.Get(SRID.XamlXmlWriterWriteNotSupportedInCurrentState, "WriteObject"));
            }

            public virtual void WriteEndObject(XamlXmlWriter writer)
            {
                throw new XamlXmlWriterException(SR.Get(SRID.XamlXmlWriterWriteNotSupportedInCurrentState, "WriteEndObject"));
            }

            public virtual void WriteStartMember(XamlXmlWriter writer, XamlMember property)
            {
                throw new XamlXmlWriterException(SR.Get(SRID.XamlXmlWriterWriteNotSupportedInCurrentState, "WriteStartMember"));
            }

            public virtual void WriteEndMember(XamlXmlWriter writer)
            {
                throw new XamlXmlWriterException(SR.Get(SRID.XamlXmlWriterWriteNotSupportedInCurrentState, "WriteEndMember"));
            }

            public virtual void WriteValue(XamlXmlWriter writer, string value)
            {
                throw new XamlXmlWriterException(SR.Get(SRID.XamlXmlWriterWriteNotSupportedInCurrentState, "WriteValue"));
            }

            public virtual void WriteNamespace(XamlXmlWriter writer, NamespaceDeclaration namespaceDeclaration)
            {
                throw new XamlXmlWriterException(SR.Get(SRID.XamlXmlWriterWriteNotSupportedInCurrentState, "WriteNamespace"));
            }

            protected static void WriteMemberAsElement(XamlXmlWriter writer)
            {
                Debug.Assert(writer.namespaceScopes.Count > 1);

                Frame frame = writer.namespaceScopes.Peek();
                Debug.Assert(frame.AllocatingNodeType == XamlNodeType.StartMember);

                XamlType type = frame.Type;
                XamlMember property = frame.Member;

                string ns;

                XamlType xamlType = property.IsAttachable ? property.DeclaringType : type;
                string prefix = property.IsAttachable || property.IsDirective ? writer.FindPrefix(property.GetXamlNamespaces(), out ns) : writer.FindPrefix(type.GetXamlNamespaces(), out ns);
                string local = (property.IsDirective) ? property.Name : XamlXmlWriter.GetTypeName(xamlType) + "." + property.Name;
                writer.output.WriteStartElement(prefix, local, ns);
            }

            protected static void WriteMemberAsAttribute(XamlXmlWriter writer)
            {
                Debug.Assert(writer.namespaceScopes.Count > 1);

                Frame frame = writer.namespaceScopes.Peek();
                Debug.Assert(frame.AllocatingNodeType == XamlNodeType.StartMember);

                XamlType owningType = frame.Type;
                XamlMember property = frame.Member;

                string local = property.Name;

                if (property.IsDirective)
                {
                    string ns;
                    string prefix = writer.FindPrefix(property.GetXamlNamespaces(), out ns);

                    WriteStartAttribute(writer, prefix, local, ns);
                }
                else if (property.IsAttachable)
                {
                    string ns;
                    string prefix = writer.FindPrefix(property.GetXamlNamespaces(), out ns);

                    if (property.DeclaringType == owningType)
                    {
                        local = property.Name;
                    }
                    else
                    {
                        local = XamlXmlWriter.GetTypeName(property.DeclaringType) + "." + property.Name;
                    }
                    WriteStartAttribute(writer, prefix, local, ns);
                }
                else
                {
                    writer.output.WriteStartAttribute(local);
                }
            }

            protected static void WriteStartElementForObject(XamlXmlWriter writer, XamlType type)
            {
                string local = XamlXmlWriter.GetTypeName(type);

                string ns;
                string prefix = writer.FindPrefix(type.GetXamlNamespaces(), out ns);

                writer.output.WriteStartElement(prefix, local, ns);
            }

            static void WriteStartAttribute(XamlXmlWriter writer, string prefix, string local, string ns)
            {
                if (prefix == String.Empty)
                {
                    writer.output.WriteStartAttribute(local);
                }
                else
                {
                    writer.output.WriteStartAttribute(prefix, local, ns);
                }
            }

            protected internal void WriteNode(XamlXmlWriter writer, XamlNode node)
            {
                switch (node.NodeType)
                {
                    case XamlNodeType.NamespaceDeclaration:
                        writer.currentState.WriteNamespace(writer, node.NamespaceDeclaration);
                        break;

                    case XamlNodeType.StartObject:
                        writer.currentState.WriteObject(writer, node.XamlType, false);
                        break;

                    case XamlNodeType.GetObject:
                        XamlType type = null;
                        Frame frame = writer.namespaceScopes.Peek();

                        if (frame.AllocatingNodeType == XamlNodeType.StartMember)
                        {
                            type = frame.Member.Type;
                        }
                        writer.currentState.WriteObject(writer, type, true);
                        break;

                    case XamlNodeType.EndObject:
                        writer.currentState.WriteEndObject(writer);
                        break;

                    case XamlNodeType.StartMember:
                        writer.currentState.WriteStartMember(writer, node.Member);
                        break;

                    case XamlNodeType.EndMember:
                        writer.currentState.WriteEndMember(writer);
                        break;

                    case XamlNodeType.Value:
                        writer.currentState.WriteValue(writer, node.Value as string);
                        break;

                    case XamlNodeType.None:
                        break;

                    default:
                        throw new NotSupportedException(SR.Get(SRID.MissingCaseXamlNodes));
                }
            }
        }

        class Start : WriterState
        {
            static WriterState state = new Start();
            Start()
            {
            }
            public static WriterState State
            {
                get { return state; }
            }

            public override void WriteNamespace(XamlXmlWriter writer, NamespaceDeclaration namespaceDeclaration)
            {
                Debug.Assert(writer.namespaceScopes.Count == 1);
                writer.AssignNamespacePrefix(namespaceDeclaration.Namespace, namespaceDeclaration.Prefix);
            }

            public override void WriteObject(XamlXmlWriter writer, XamlType type, bool isObjectFromMember)
            {
                writer.namespaceScopes.Peek().Type = type;
                writer.namespaceScopes.Peek().IsObjectFromMember = isObjectFromMember;

                if (isObjectFromMember)
                {
                    // The root element cannot be from member
                    throw new XamlXmlWriterException(SR.Get(SRID.XamlXmlWriterWriteObjectNotSupportedInCurrentState));
                }
                else
                {
                    WriteStartElementForObject(writer, type);
                    writer.currentState = InRecordTryAttributes.State;
                }
            }
        }

        class End : WriterState
        {
            static WriterState state = new End();
            End()
            {
            }
            public static WriterState State
            {
                get { return state; }
            }
        }

        class InRecord : WriterState
        {
            static WriterState state = new InRecord();
            InRecord()
            {
            }
            public static WriterState State
            {
                get { return state; }
            }

            public override void WriteNamespace(XamlXmlWriter writer, NamespaceDeclaration namespaceDeclaration)
            {
                Debug.Assert(writer.namespaceScopes.Count > 0);

                if (writer.namespaceScopes.Peek().AllocatingNodeType != XamlNodeType.StartMember)
                {
                    writer.namespaceScopes.Push(new Frame
                    {
                        AllocatingNodeType = XamlNodeType.StartMember,
                        Type = writer.namespaceScopes.Peek().Type
                    });
                }

                writer.AssignNamespacePrefix(namespaceDeclaration.Namespace, namespaceDeclaration.Prefix);
            }

            public override void WriteStartMember(XamlXmlWriter writer, XamlMember property)
            {
                writer.CheckMemberForUniqueness(property);

                if (writer.namespaceScopes.Peek().AllocatingNodeType != XamlNodeType.StartMember)
                {
                    writer.namespaceScopes.Push(new Frame
                    {
                        AllocatingNodeType = XamlNodeType.StartMember,
                        Type = writer.namespaceScopes.Peek().Type,
                    });
                }
                writer.namespaceScopes.Peek().Member = property;

                XamlType parentType = writer.namespaceScopes.Peek().Type;
                if ((property == XamlLanguage.Items && parentType != null && parentType.IsWhitespaceSignificantCollection) ||
                    (property == XamlLanguage.UnknownContent))
                {
                    writer.isFirstElementOfWhitespaceSignificantCollection = true;
                }

                XamlType containingType = writer.namespaceScopes.Peek().Type;
                if (IsImplicit(property))
                {
                    if (!writer.namespaceScopes.Peek().IsEmpty())
                    {
                        throw new InvalidOperationException(SR.Get(SRID.XamlXmlWriterWriteNotSupportedInCurrentState, "WriteStartMember"));
                    }
                }
                else if (property == containingType.ContentProperty)
                {
                    if (!writer.namespaceScopes.Peek().IsEmpty())
                    {
                        throw new InvalidOperationException(SR.Get(SRID.XamlXmlWriterWriteNotSupportedInCurrentState, "WriteStartMember"));
                    }
                    else
                    {
                        writer.currentState = TryContentProperty.State;
                        return;
                    }
                }
                else
                {
                    WriteMemberAsElement(writer);
                    writer.WriteDeferredNamespaces(XamlNodeType.StartMember);
                }

                if (property == XamlLanguage.PositionalParameters)
                {
                    // the writer is not in a state where it can write markup extensions in curly form
                    // so it expands the positional parameters as properties.

                    writer.namespaceScopes.Pop();

                    // but in order to expand parameters, the markup extension needs to have a default constructor

                    if (containingType != null && containingType.ConstructionRequiresArguments)
                    {
                        throw new XamlXmlWriterException(SR.Get(SRID.ExpandPositionalParametersinTypeWithNoDefaultConstructor));
                    }

                    writer.ppStateInfo.ReturnState = InRecord.State;
                    writer.currentState = ExpandPositionalParameters.State;
                }
                else
                {
                    writer.currentState = InMember.State;
                }
            }

            public override void WriteEndObject(XamlXmlWriter writer)
            {
                Debug.Assert(writer.namespaceScopes.Count > 0);
                Frame frame = writer.namespaceScopes.Pop();
                if (frame.AllocatingNodeType != XamlNodeType.StartObject &&
                    frame.AllocatingNodeType != XamlNodeType.GetObject)
                {
                    throw new InvalidOperationException(SR.Get(SRID.XamlXmlWriterWriteNotSupportedInCurrentState, "WriteEndObject"));
                }

                if (!frame.IsObjectFromMember)
                {
                    writer.output.WriteEndElement();
                }

                if (writer.namespaceScopes.Count > 0)
                {
                    writer.currentState = InMemberAfterEndObject.State;
                }
                else
                {
                    writer.Flush();
                    writer.currentState = End.State;
                }
            }
        }

        class InRecordTryAttributes : WriterState
        {
            static WriterState state = new InRecordTryAttributes();
            InRecordTryAttributes()
            {
            }
            public static WriterState State
            {
                get { return state; }
            }

            public override void WriteNamespace(XamlXmlWriter writer, NamespaceDeclaration namespaceDeclaration)
            {
                writer.currentState = InRecord.State;
                writer.WriteDeferredNamespaces(XamlNodeType.StartObject);

                writer.currentState.WriteNamespace(writer, namespaceDeclaration);
            }

            public override void WriteStartMember(XamlXmlWriter writer, XamlMember property)
            {
                XamlType parentType = writer.namespaceScopes.Peek().Type;
                if ((property == XamlLanguage.Items && parentType != null && parentType.IsWhitespaceSignificantCollection) ||
                    (property == XamlLanguage.UnknownContent))
                {
                    writer.isFirstElementOfWhitespaceSignificantCollection = true;
                }

                if (property.IsAttachable || property.IsDirective)
                {
                    string chosenNamespace;
                    string prefix = writer.LookupPrefix(property.GetXamlNamespaces(), out chosenNamespace);

                    if (prefix == null || writer.IsShadowed(chosenNamespace, prefix))
                    {
                        // if the property's prefix is not already defined, or it's shadowed
                        // we need to write this property as an element so that the prefix can be defined in the property's scope.

                        writer.currentState = InRecord.State;
                        writer.WriteDeferredNamespaces(XamlNodeType.StartObject);
                        writer.currentState.WriteStartMember(writer, property);
                        return;
                    }
                }

                writer.CheckMemberForUniqueness(property);

                Debug.Assert(writer.namespaceScopes.Count > 0);
                Debug.Assert(writer.namespaceScopes.Peek().AllocatingNodeType == XamlNodeType.StartObject ||
                             writer.namespaceScopes.Peek().AllocatingNodeType == XamlNodeType.GetObject);

                writer.namespaceScopes.Push(new Frame
                {
                    AllocatingNodeType = XamlNodeType.StartMember,
                    Type = writer.namespaceScopes.Peek().Type,
                    Member = property
                });

                XamlType containingType = writer.namespaceScopes.Peek().Type;
                if (property == XamlLanguage.PositionalParameters)
                {
                    // the writer is not in a state where it can write markup extensions in curly form
                    // so it expands the positional parameters as properties.

                    writer.namespaceScopes.Pop();
                    // but in order to expand properties, the markup extension needs to have a default constructor

                    if (containingType != null && containingType.ConstructionRequiresArguments)
                    {
                        throw new XamlXmlWriterException(SR.Get(SRID.ExpandPositionalParametersinTypeWithNoDefaultConstructor));
                    }

                    writer.ppStateInfo.ReturnState = InRecordTryAttributes.State;
                    writer.currentState = ExpandPositionalParameters.State;
                }
                else if (IsImplicit(property))
                {
                    // Stop trying attributes

                    writer.WriteDeferredNamespaces(XamlNodeType.StartObject);
                    writer.currentState = InMember.State;
                }
                else if (property == containingType.ContentProperty)
                {
                    writer.currentState = TryContentPropertyInTryAttributesState.State;
                }
                else if (property.IsDirective && (property.Type != null && (property.Type.IsCollection || property.Type.IsDictionary)))
                {
                    writer.WriteDeferredNamespaces(XamlNodeType.StartObject);
                    WriteMemberAsElement(writer);
                    writer.currentState = InMember.State;
                }
                else
                {
                    // Whether it is a directive or not, postpone write and
                    // try attribute
                    //
                    writer.currentState = InMemberTryAttributes.State;
                }

            }

            public override void WriteEndObject(XamlXmlWriter writer)
            {
                writer.currentState = InRecord.State;
                writer.WriteDeferredNamespaces(XamlNodeType.StartObject);
                writer.currentState.WriteEndObject(writer);
            }
        }

        // Follows InObject after Start Member
        //
        class InMember : WriterState
        {
            static WriterState state = new InMember();
            InMember()
            {
            }
            public static WriterState State
            {
                get { return state; }
            }

            public override void WriteNamespace(XamlXmlWriter writer, NamespaceDeclaration namespaceDeclaration)
            {
                Debug.Assert(writer.namespaceScopes.Count > 0);
                if (writer.namespaceScopes.Peek().AllocatingNodeType != XamlNodeType.StartObject &&
                    writer.namespaceScopes.Peek().AllocatingNodeType != XamlNodeType.GetObject)
                {
                    writer.namespaceScopes.Push(new Frame { AllocatingNodeType = XamlNodeType.StartObject });
                }
                writer.AssignNamespacePrefix(namespaceDeclaration.Namespace, namespaceDeclaration.Prefix);
            }

            public override void WriteValue(XamlXmlWriter writer, string value)
            {
                Frame frame = writer.namespaceScopes.Peek();
                if (frame.AllocatingNodeType != XamlNodeType.StartMember)
                {
                    throw new InvalidOperationException(SR.Get(SRID.XamlXmlWriterWriteNotSupportedInCurrentState, "WriteValue"));
                }

                if (frame.Member.DeclaringType == XamlLanguage.XData)
                {
                    writer.output.WriteRaw(value);
                    writer.currentState = InMemberAfterValue.State;
                }
                else
                {
                    // If we have significant white space, we write out xml:space='preserve'
                    // except if it is a WhiteSpace Significant collection. In that case
                    // we write that out only if
                    // 1. It has 2 consecutive spaces or non space whitespace (tabs, new line, etc)
                    // 2. First element has leading whitespace
                    // 3. Last Element has trailing whitespace
                    if (HasSignificantWhitespace(value))
                    {
                        XamlType containingXamlType = GetContainingXamlType(writer);
                        //Treat unknown types as WhitespaceSignificantCollections
                        if (containingXamlType != null && !containingXamlType.IsWhitespaceSignificantCollection)
                        {
                            WriteXmlSpaceOrThrow(writer, value);
                            writer.output.WriteValue(value);
                            writer.currentState = InMemberAfterValue.State;
                        }
                        else if (ContainsConsecutiveInnerSpaces(value) ||
                                 ContainsWhitespaceThatIsNotSpace(value))
                        {
                            if (writer.isFirstElementOfWhitespaceSignificantCollection)
                            {
                                WriteXmlSpaceOrThrow(writer, value);
                                writer.output.WriteValue(value);
                                writer.currentState = InMemberAfterValue.State;
                            }
                            else
                            {
                                throw new InvalidOperationException(SR.Get(SRID.WhiteSpaceInCollection, value, containingXamlType.Name));
                            }
                        }
                        else
                        {
                            if (ContainsLeadingSpace(value) && writer.isFirstElementOfWhitespaceSignificantCollection)
                            {
                                WriteXmlSpaceOrThrow(writer, value);
                                writer.output.WriteValue(value);
                                writer.currentState = InMemberAfterValue.State;
                            }
                            if (ContainsTrailingSpace(value))
                            {
                                writer.deferredValue = value;
                                writer.currentState = InMemberAfterValueWithSignificantWhitespace.State;
                            }
                            else
                            {
                                writer.output.WriteValue(value);
                                writer.currentState = InMemberAfterValue.State;
                            }
                        }
                    }
                    else
                    {
                        writer.output.WriteValue(value);
                        writer.currentState = InMemberAfterValue.State;
                    }
                }
                if (writer.currentState != InMemberAfterValueWithSignificantWhitespace.State)
                {
                    writer.isFirstElementOfWhitespaceSignificantCollection = false;
                }
            }

            void WriteXmlSpaceOrThrow(XamlXmlWriter writer, string value)
            {
                var frameWithXmlSpacePreserve = FindFrameWithXmlSpacePreserve(writer);
                if (frameWithXmlSpacePreserve.AllocatingNodeType == XamlNodeType.StartMember)
                {
                    throw new XamlXmlWriterException(SR.Get(SRID.CannotWriteXmlSpacePreserveOnMember, frameWithXmlSpacePreserve.Member, value));
                }

                WriteXmlSpace(writer);
            }

            // this method finds the SO or SM where "xml:space = preserve" will actually be attached to
            Frame FindFrameWithXmlSpacePreserve(XamlXmlWriter writer)
            {
                var frameEnumerator = writer.namespaceScopes.GetEnumerator();

                while (frameEnumerator.MoveNext())
                {
                    var frame = frameEnumerator.Current;
                    if (frame.AllocatingNodeType == XamlNodeType.GetObject)
                    {
                        continue;
                    }
                    if (frame.AllocatingNodeType == XamlNodeType.StartMember)
                    {
                        if (frame.IsContent)
                        {
                            continue;
                        }
                        var member = frame.Member;
                        if (XamlXmlWriter.IsImplicit(member))
                        {
                            continue;
                        }
                    }
                    break;
                }
                return frameEnumerator.Current;
            }

            public override void WriteObject(XamlXmlWriter writer, XamlType type, bool isObjectFromMember)
            {
                Debug.Assert(writer.namespaceScopes.Count > 0);
                if (writer.namespaceScopes.Peek().AllocatingNodeType != XamlNodeType.StartObject &&
                    writer.namespaceScopes.Peek().AllocatingNodeType != XamlNodeType.GetObject)
                {
                    writer.namespaceScopes.Push(new Frame { AllocatingNodeType = isObjectFromMember ? XamlNodeType.GetObject : XamlNodeType.StartObject });
                }

                writer.namespaceScopes.Peek().Type = type;
                writer.namespaceScopes.Peek().IsObjectFromMember = isObjectFromMember;

                writer.isFirstElementOfWhitespaceSignificantCollection = false;

                if (isObjectFromMember)
                {
                    if (!writer.namespaceScopes.Peek().IsEmpty())
                    {
                        throw new InvalidOperationException(SR.Get(SRID.XamlXmlWriterWriteObjectNotSupportedInCurrentState));
                    }

                    Frame tempFrame = writer.namespaceScopes.Pop();
                    Frame frame = writer.namespaceScopes.Peek();
                    writer.namespaceScopes.Push(tempFrame);

                    if (frame.AllocatingNodeType == XamlNodeType.StartMember)
                    {
                        XamlType memberType = frame.Member.Type;
                        if (memberType != null && !memberType.IsCollection && !memberType.IsDictionary)
                        {
                            throw new InvalidOperationException(SR.Get(SRID.XamlXmlWriterIsObjectFromMemberSetForArraysOrNonCollections));
                        }
                    }

                    writer.currentState = InRecord.State;
                }
                else
                {
                    WriteStartElementForObject(writer, type);
                    writer.currentState = InRecordTryAttributes.State;
                 }

            }
        }

        // Follows InMember after an Atom and prevents writing two atoms
        // in a row
        //
        class InMemberAfterValue : WriterState
        {
            static WriterState state = new InMemberAfterValue();
            InMemberAfterValue()
            {
            }
            public static WriterState State
            {
                get { return state; }
            }

            public override void WriteNamespace(XamlXmlWriter writer, NamespaceDeclaration namespaceDeclaration)
            {
                writer.currentState = InMember.State;
                writer.currentState.WriteNamespace(writer, namespaceDeclaration);
            }

            public override void WriteEndMember(XamlXmlWriter writer)
            {
                Debug.Assert(writer.namespaceScopes.Count > 0);
                Frame memberFrame = writer.namespaceScopes.Pop();
                Debug.Assert(memberFrame.AllocatingNodeType == XamlNodeType.StartMember ||
                    memberFrame.AllocatingNodeType == XamlNodeType.GetObject);

                if (!IsImplicit(memberFrame.Member) && !memberFrame.IsContent)
                {
                    writer.output.WriteEndElement();
                }
                writer.currentState = InRecord.State;
            }

            public override void WriteObject(XamlXmlWriter writer, XamlType type, bool isObjectFromMember)
            {
                writer.currentState = InMember.State;
                writer.currentState.WriteObject(writer, type, isObjectFromMember);
            }
        }

        // We reach this state if a value element of a whitespace significant collection
        // has a trailing whitespace. In that case we need to write out xml:space="preserve"
        // or throw depending on whether the next element is another collection element of
        // end member
        //
        class InMemberAfterValueWithSignificantWhitespace : WriterState
        {
            static WriterState state = new InMemberAfterValueWithSignificantWhitespace();
            InMemberAfterValueWithSignificantWhitespace()
            {
            }
            public static WriterState State
            {
                get { return state; }
            }

            public override void WriteNamespace(XamlXmlWriter writer, NamespaceDeclaration namespaceDeclaration)
            {
                writer.currentState = InMemberAfterValue.State;
                writer.currentState.WriteNamespace(writer, namespaceDeclaration);
            }

            public override void WriteEndMember(XamlXmlWriter writer)
            {
                if (writer.isFirstElementOfWhitespaceSignificantCollection)
                {
                    WriteXmlSpace(writer);
                    writer.output.WriteValue(writer.deferredValue);

                    writer.currentState = InMemberAfterValue.State;
                    writer.currentState.WriteEndMember(writer);

                    writer.isFirstElementOfWhitespaceSignificantCollection = false;
                }
                else
                {
                    XamlType containingXamlType = GetContainingXamlType(writer);
                    throw new InvalidOperationException(SR.Get(SRID.WhiteSpaceInCollection, writer.deferredValue, containingXamlType.Name));
                }
            }

            public override void WriteObject(XamlXmlWriter writer, XamlType type, bool isObjectFromMember)
            {
                writer.output.WriteValue(writer.deferredValue);

                writer.currentState = InMemberAfterValue.State;
                writer.currentState.WriteObject(writer, type, isObjectFromMember);
            }
        }

        // Follows InObject after an End Object.
        // Like InMember but also allows End Member.
        //
        class InMemberAfterEndObject : WriterState
        {
            static WriterState state = new InMemberAfterEndObject();
            InMemberAfterEndObject()
            {
            }
            public static WriterState State
            {
                get { return state; }
            }

            public override void WriteNamespace(XamlXmlWriter writer, NamespaceDeclaration namespaceDeclaration)
            {
                writer.currentState = InMember.State;
                writer.currentState.WriteNamespace(writer, namespaceDeclaration);
            }

            public override void WriteValue(XamlXmlWriter writer, string value)
            {
                writer.currentState = InMember.State;
                writer.currentState.WriteValue(writer, value);
            }

            public override void WriteEndMember(XamlXmlWriter writer)
            {
                writer.currentState = InMemberAfterValue.State;
                writer.currentState.WriteEndMember(writer);
            }

            public override void WriteObject(XamlXmlWriter writer, XamlType type, bool isObjectFromMember)
            {
                writer.currentState = InMember.State;
                writer.currentState.WriteObject(writer, type, isObjectFromMember);
            }
        }

        // From InMemberTryAttributesAfterAtom, we are sure that this is an attributable member.
        class InMemberAttributedMember : WriterState
        {
            static WriterState state = new InMemberAttributedMember();
            InMemberAttributedMember()
            {
            }
            public static WriterState State
            {
                get { return state; }
            }

            public override void WriteEndMember(XamlXmlWriter writer)
            {
                WriteMemberAsAttribute(writer);
                if (!writer.deferredValueIsME && StringStartsWithCurly(writer.deferredValue))
                {
                    writer.output.WriteValue("{}" + writer.deferredValue);
                }
                else
                {
                    writer.output.WriteValue(writer.deferredValue);
                }


                Debug.Assert(writer.namespaceScopes.Count > 0);
                Frame memberFrame = writer.namespaceScopes.Pop();
                Debug.Assert(memberFrame.AllocatingNodeType == XamlNodeType.StartMember ||
                    memberFrame.AllocatingNodeType == XamlNodeType.GetObject);

                writer.output.WriteEndAttribute();
                writer.currentState = InRecordTryAttributes.State;
            }
        }

        class InMemberTryAttributes : WriterState
        {
            static WriterState state = new InMemberTryAttributes();
            InMemberTryAttributes()
            {
            }
            public static WriterState State
            {
                get { return state; }
            }

            public override void WriteNamespace(XamlXmlWriter writer, NamespaceDeclaration namespaceDeclaration)
            {
                // We don't currently allow WriteNamespace before WriteValue,
                // so we are done trying to write this as an attribute
                writer.WriteDeferredNamespaces(XamlNodeType.StartObject);
                WriteMemberAsElement(writer);

                writer.currentState = InMember.State;
                writer.currentState.WriteNamespace(writer, namespaceDeclaration);
            }

            public override void WriteValue(XamlXmlWriter writer, string value)
            {
                writer.deferredValue = value;
                writer.deferredValueIsME = false;
                writer.currentState = InMemberTryAttributesAfterValue.State;
                writer.isFirstElementOfWhitespaceSignificantCollection = false;
            }

            public override void WriteObject(XamlXmlWriter writer, XamlType type, bool isObjectFromMember)
            {
                //  We should remove the !type.IsGeneric check once
                //  XamlReader is fixed to handle Generic MEs.
                if (type != null && type.IsMarkupExtension && !type.IsGeneric)
                {
                    writer.meWriter.Reset();
                    writer.meNodesStack.Push(new List<XamlNode>());
                    writer.currentState = TryCurlyForm.State;
                    writer.currentState.WriteObject(writer, type, isObjectFromMember);
                }
                else
                {
                    writer.WriteDeferredNamespaces(XamlNodeType.StartObject);
                    WriteMemberAsElement(writer);

                    writer.currentState = InMember.State;
                    writer.currentState.WriteObject(writer, type, isObjectFromMember);
                }
                writer.isFirstElementOfWhitespaceSignificantCollection = false;
            }
        }

        // From InMemberTryAttributes, and at this point, both write start
        // member and write atom have been deferred in case we see a start
        // record -- for mixed content -- which would force us out of
        // attribute form.
        //
        class InMemberTryAttributesAfterValue : WriterState
        {
            static WriterState state = new InMemberTryAttributesAfterValue();
            InMemberTryAttributesAfterValue()
            {
            }
            public static WriterState State
            {
                get { return state; }
            }

            public override void WriteNamespace(XamlXmlWriter writer, NamespaceDeclaration namespaceDeclaration)
            {
                // This call proceeds a call to WriteObject()
                // so we are done trying writing this as an attribute

                writer.WriteDeferredNamespaces(XamlNodeType.StartObject);
                WriteMemberAsElement(writer);
                writer.output.WriteValue(writer.deferredValue);

                writer.currentState = InMember.State;
                writer.currentState.WriteNamespace(writer, namespaceDeclaration);
            }

            public override void WriteEndMember(XamlXmlWriter writer)
            {
                writer.currentState = InMemberAttributedMember.State;
                writer.currentState.WriteEndMember(writer);
            }

            public override void WriteObject(XamlXmlWriter writer, XamlType type, bool isObjectFromMember)
            {
                // Flush out all of the namespaces that might have been allocated for attribute-type members.
                //
                writer.WriteDeferredNamespaces(XamlNodeType.StartObject);
                WriteMemberAsElement(writer);
                writer.output.WriteValue(writer.deferredValue);

                writer.isFirstElementOfWhitespaceSignificantCollection = false;

                writer.currentState = InMember.State;
                writer.currentState.WriteObject(writer, type, isObjectFromMember);
            }
        }

        class TryContentProperty : WriterState
        {
            static WriterState state = new TryContentProperty();
            TryContentProperty()
            {
            }
            public static WriterState State
            {
                get { return state; }
            }

            public override void WriteNamespace(XamlXmlWriter writer, NamespaceDeclaration namespaceDeclaration)
            {
                writer.namespaceScopes.Peek().IsContent = true;
                writer.currentState = InMember.State;
                writer.currentState.WriteNamespace(writer, namespaceDeclaration);
            }

            public override void WriteValue(XamlXmlWriter writer, string value)
            {
                var property = writer.namespaceScopes.Peek().Member;
                if (XamlLanguage.String.CanAssignTo(property.Type))
                {
                    writer.namespaceScopes.Peek().IsContent = true;
                }
                else
                {
                    writer.namespaceScopes.Peek().IsContent = false;
                    WriteMemberAsElement(writer);
                }
                writer.currentState = InMember.State;
                writer.currentState.WriteValue(writer, value);
            }

            public override void WriteObject(XamlXmlWriter writer, XamlType type, bool isObjectFromMember)
            {
                writer.namespaceScopes.Peek().IsContent = true;
                writer.currentState = InMember.State;
                writer.currentState.WriteObject(writer, type, isObjectFromMember);
            }
        }

        class TryContentPropertyInTryAttributesState : WriterState
        {
            static WriterState state = new TryContentPropertyInTryAttributesState();
            TryContentPropertyInTryAttributesState()
            {
            }
            public static WriterState State
            {
                get { return state; }
            }

            public override void WriteNamespace(XamlXmlWriter writer, NamespaceDeclaration namespaceDeclaration)
            {
                writer.namespaceScopes.Peek().IsContent = true;

                // We don't currently allow WriteNamespace before WriteValue,
                // so we are done trying to write this as an attribute
                writer.WriteDeferredNamespaces(XamlNodeType.StartObject);
                writer.currentState = InMember.State;
                writer.currentState.WriteNamespace(writer, namespaceDeclaration);
            }

            public override void WriteValue(XamlXmlWriter writer, string value)
            {
                var property = writer.namespaceScopes.Peek().Member;
                if (XamlLanguage.String.CanAssignTo(property.Type) && value != string.Empty)
                {
                    writer.namespaceScopes.Peek().IsContent = true;
                    writer.WriteDeferredNamespaces(XamlNodeType.StartObject);
                    writer.currentState = InMember.State;
                    writer.currentState.WriteValue(writer, value);
                }
                else
                {
                    writer.namespaceScopes.Peek().IsContent = false;
                    writer.currentState = InMemberTryAttributes.State;
                    writer.currentState.WriteValue(writer, value);
                }
            }

            public override void WriteObject(XamlXmlWriter writer, XamlType type, bool isObjectFromMember)
            {
                writer.namespaceScopes.Peek().IsContent = true;
                writer.WriteDeferredNamespaces(XamlNodeType.StartObject);
                writer.currentState = InMember.State;
                writer.currentState.WriteObject(writer, type, isObjectFromMember);
            }
        }

        class TryCurlyForm : WriterState
        {
            static WriterState state = new TryCurlyForm();
            TryCurlyForm()
            {
            }
            public static WriterState State
            {
                get { return state; }
            }

            void WriteNodesInXmlForm(XamlXmlWriter writer)
            {
                writer.WriteDeferredNamespaces(XamlNodeType.StartObject);
                WriteMemberAsElement(writer);

                writer.currentState = InMember.State;

                var meNodes = writer.meNodesStack.Pop();
                foreach (var node in meNodes)
                {
                    writer.currentState.WriteNode(writer, node);
                }
            }

            public override void WriteObject(XamlXmlWriter writer, XamlType type, bool isObjectFromMember)
            {
                if (!isObjectFromMember)
                {
                    writer.meNodesStack.Peek().Add(new XamlNode(XamlNodeType.StartObject, type));
                    writer.meWriter.WriteStartObject(type);
                }
                else
                {
                    writer.meNodesStack.Peek().Add(new XamlNode(XamlNodeType.GetObject));
                    writer.meWriter.WriteGetObject();
                }

                if (writer.meWriter.Failed)
                {
                    WriteNodesInXmlForm(writer);
                }
            }

            public override void WriteEndObject(XamlXmlWriter writer)
            {
                writer.meNodesStack.Peek().Add(new XamlNode(XamlNodeType.EndObject));

                writer.meWriter.WriteEndObject();

                if (writer.meWriter.Failed)
                {
                    WriteNodesInXmlForm(writer);
                }

                // Did writing the markup extension succeed?
                if (writer.meWriter.MarkupExtensionString != null)
                {
                    writer.meNodesStack.Pop();
                    writer.deferredValue = writer.meWriter.MarkupExtensionString;
                    writer.deferredValueIsME = true;
                    writer.currentState = InMemberTryAttributesAfterValue.State;
                }
            }

            public override void WriteStartMember(XamlXmlWriter writer, XamlMember property)
            {
                writer.meNodesStack.Peek().Add(new XamlNode(XamlNodeType.StartMember, property));

                writer.meWriter.WriteStartMember(property);

                if (writer.meWriter.Failed)
                {
                    WriteNodesInXmlForm(writer);
                }
            }

            public override void WriteEndMember(XamlXmlWriter writer)
            {
                writer.meNodesStack.Peek().Add(new XamlNode(XamlNodeType.EndMember));

                writer.meWriter.WriteEndMember();

                if (writer.meWriter.Failed)
                {
                    WriteNodesInXmlForm(writer);
                }
            }

            public override void WriteNamespace(XamlXmlWriter writer, NamespaceDeclaration namespaceDeclaration)
            {
                writer.meNodesStack.Peek().Add(new XamlNode(XamlNodeType.NamespaceDeclaration, namespaceDeclaration));

                writer.meWriter.WriteNamespace(namespaceDeclaration);

                if (writer.meWriter.Failed)
                {
                    WriteNodesInXmlForm(writer);
                }
            }

            public override void WriteValue(XamlXmlWriter writer, string value)
            {
                writer.meNodesStack.Peek().Add(new XamlNode(XamlNodeType.Value, value));

                writer.meWriter.WriteValue(value);

                if (writer.meWriter.Failed)
                {
                    WriteNodesInXmlForm(writer);
                }
            }
        }

        class ExpandPositionalParameters : WriterState
        {
            static WriterState state = new ExpandPositionalParameters();
            ExpandPositionalParameters()
            {
            }

            public static WriterState State
            {
                get { return state; }
            }

            void ExpandPositionalParametersIntoProperties(XamlXmlWriter writer)
            {
                Frame frame = writer.namespaceScopes.Peek();
                Debug.Assert(frame.AllocatingNodeType == XamlNodeType.StartObject);

                XamlType objectXamlType = frame.Type;
                Debug.Assert(objectXamlType != null);

                Type objectClrType = objectXamlType.UnderlyingType;
                if (objectClrType == null)
                {
                    throw new XamlXmlWriterException(SR.Get(
                        SRID.ExpandPositionalParametersWithoutUnderlyingType, objectXamlType.GetQualifiedName()));
                }

                int numOfParameters = writer.ppStateInfo.NodesList.Count;

                ParameterInfo[] constructorParameters = GetParametersInfo(objectXamlType, numOfParameters);
                List<XamlMember> ctorArgProps = GetAllPropertiesWithCAA(objectXamlType);

                // If there aren't the same number of parameters then we throw
                if (constructorParameters.Length != ctorArgProps.Count)
                {
                    throw new XamlXmlWriterException(SR.Get(SRID.ConstructorNotFoundForGivenPositionalParameters));
                }

                for (int i = 0; i < constructorParameters.Length; i++)
                {
                    ParameterInfo paraminfo = constructorParameters[i];

                    XamlMember matchingProperty = null;
                    foreach (var potentialProperty in ctorArgProps)
                    {
                        if ((potentialProperty.Type.UnderlyingType == paraminfo.ParameterType) &&
                            (XamlObjectReader.GetConstructorArgument(potentialProperty) == paraminfo.Name))
                        {
                            matchingProperty = potentialProperty;
                            break;
                        }
                    }

                    if (matchingProperty == null)
                    {
                        throw new XamlXmlWriterException(SR.Get(SRID.ConstructorNotFoundForGivenPositionalParameters));
                    }

                    XamlMember member = objectXamlType.GetMember(matchingProperty.Name);
                    Debug.Assert(member != null);

                    if (member.IsReadOnly)
                    {
                        throw new XamlXmlWriterException(SR.Get(SRID.ExpandPositionalParametersWithReadOnlyProperties));
                    }
                    writer.ppStateInfo.NodesList[i].Insert(0, new XamlNode(XamlNodeType.StartMember, member));
                    writer.ppStateInfo.NodesList[i].Add(new XamlNode(XamlNodeType.EndMember));
                }
            }

            ParameterInfo[] GetParametersInfo(XamlType objectXamlType, int numOfParameters)
            {
                IList<XamlType> paramXamlTypes = objectXamlType.GetPositionalParameters(numOfParameters);

                if (paramXamlTypes == null)
                {
                    throw new XamlXmlWriterException(SR.Get(SRID.ConstructorNotFoundForGivenPositionalParameters));
                }

                Type[] paramClrTypes = new Type[numOfParameters];

                int i = 0;
                foreach (var xamlType in paramXamlTypes)
                {
                    Type underlyingType = xamlType.UnderlyingType;
                    if (underlyingType != null)
                    {
                        paramClrTypes[i++] = underlyingType;
                    }
                    else
                    {
                        throw new XamlXmlWriterException(SR.Get(SRID.ConstructorNotFoundForGivenPositionalParameters));
                    }
                }

                ConstructorInfo constructor = objectXamlType.GetConstructor(paramClrTypes);

                if (constructor == null)
                {
                    throw new XamlXmlWriterException(SR.Get(SRID.ConstructorNotFoundForGivenPositionalParameters));
                }

                return constructor.GetParameters();
            }

            List<XamlMember> GetAllPropertiesWithCAA(XamlType objectXamlType)
            {
                // Pull out all the properties that are attributed with ConstructorArgumentAttribute
                //
                var properties = objectXamlType.GetAllMembers();
                var readOnlyProperties = objectXamlType.GetAllExcludedReadOnlyMembers();
                var ctorArgProps = new List<XamlMember>();

                foreach (XamlMember p in properties)
                {
                    if (!string.IsNullOrEmpty(XamlObjectReader.GetConstructorArgument(p)))
                    {
                        ctorArgProps.Add(p);
                    }
                }
                foreach (XamlMember p in readOnlyProperties)
                {
                    if (!string.IsNullOrEmpty(XamlObjectReader.GetConstructorArgument(p)))
                    {
                        ctorArgProps.Add(p);
                    }
                }
                return ctorArgProps;
            }

            void WriteNodes(XamlXmlWriter writer)
            {
                var ppNodesList = writer.ppStateInfo.NodesList;
                writer.ppStateInfo.Reset();
                writer.currentState = writer.ppStateInfo.ReturnState;

                foreach (List<XamlNode> nodesList in ppNodesList)
                {
                    foreach (XamlNode node in nodesList)
                    {
                        writer.currentState.WriteNode(writer, node);
                    }
                }
            }

            void ThrowIfFailed(bool fail, string operation)
            {
                if (fail)
                {
                    throw new InvalidOperationException(SR.Get(SRID.XamlXmlWriterWriteNotSupportedInCurrentState, operation));
                }
            }

            public override void WriteObject(XamlXmlWriter writer, XamlType type, bool isObjectFromMember)
            {
                if (!isObjectFromMember)
                {
                    writer.ppStateInfo.Writer.WriteStartObject(type);
                    ThrowIfFailed(writer.ppStateInfo.Writer.Failed, "WriteStartObject");

                    XamlNode node = new XamlNode(XamlNodeType.StartObject, type);
                    if (writer.ppStateInfo.CurrentDepth == 0)
                    {
                        writer.ppStateInfo.NodesList.Add(new List<XamlNode> { node });
                    }
                    else
                    {
                        writer.ppStateInfo.NodesList[writer.ppStateInfo.NodesList.Count - 1].Add(node);
                    }

                    writer.ppStateInfo.CurrentDepth++;
                }
                else
                {
                    throw new InvalidOperationException(SR.Get(SRID.XamlXmlWriterWriteNotSupportedInCurrentState, "WriteGetObject"));
                }
            }

            public override void WriteEndObject(XamlXmlWriter writer)
            {
                writer.ppStateInfo.Writer.WriteEndObject();
                ThrowIfFailed(writer.ppStateInfo.Writer.Failed, "WriteEndObject");

                XamlNode node = new XamlNode(XamlNodeType.EndObject);

                Debug.Assert(writer.ppStateInfo.CurrentDepth != 0);
                Debug.Assert(writer.ppStateInfo.NodesList.Count != 0);
                writer.ppStateInfo.NodesList[writer.ppStateInfo.NodesList.Count - 1].Add(node);
                writer.ppStateInfo.CurrentDepth--;
            }

            public override void WriteStartMember(XamlXmlWriter writer, XamlMember property)
            {
                writer.ppStateInfo.Writer.WriteStartMember(property);
                ThrowIfFailed(writer.ppStateInfo.Writer.Failed, "WriteStartMember");

                XamlNode node = new XamlNode(XamlNodeType.StartMember, property);

                if (writer.ppStateInfo.CurrentDepth == 0)
                {
                    writer.ppStateInfo.NodesList.Add(new List<XamlNode> { node });
                }
                else
                {
                    writer.ppStateInfo.NodesList[writer.ppStateInfo.NodesList.Count - 1].Add(node);
                }
            }

            public override void WriteEndMember(XamlXmlWriter writer)
            {
                writer.ppStateInfo.Writer.WriteEndMember();
                ThrowIfFailed(writer.ppStateInfo.Writer.Failed, "WriteEndMember");

                if (writer.ppStateInfo.CurrentDepth == 0)
                {
                    // we are done collecting all positional parameters
                    ExpandPositionalParametersIntoProperties(writer);
                    WriteNodes(writer);
                }
            }

            public override void WriteValue(XamlXmlWriter writer, string value)
            {
                writer.ppStateInfo.Writer.WriteValue(value);
                ThrowIfFailed(writer.ppStateInfo.Writer.Failed, "WriteValue");

                XamlNode node = new XamlNode(XamlNodeType.Value, value);
                if (writer.ppStateInfo.CurrentDepth == 0)
                {
                    writer.ppStateInfo.NodesList.Add(new List<XamlNode> { node });
                }
                else
                {
                    writer.ppStateInfo.NodesList[writer.ppStateInfo.NodesList.Count - 1].Add(node);
                }
            }
        }

        class PositionalParameterStateInfo
        {
            public PositionalParameterStateInfo(XamlXmlWriter xamlXmlWriter)
            {
                Writer = new XamlMarkupExtensionWriter(xamlXmlWriter, new XamlMarkupExtensionWriterSettings { ContinueWritingWhenPrefixIsNotFound = true });
                Reset();
            }

            // A list of lists that stores nodes we have tried to write in positional parameters so far.
            // each list represents one parameter.  If the parameter is a value, then the list contains a single node;
            // however, the parameter can also be an object, in which case the list contains all the nodes of the object.
            public List<List<XamlNode>> NodesList
            {
                get;
                set;
            }

            // this writer ensures all state transitions in positional parameters writing are valid
            // XamlXmlWriter does not actually write the positional parameters using this writer,
            // only the state transitioning logic in the markup extension writer is used.
            public XamlMarkupExtensionWriter Writer
            {
                get;
                set;
            }

            // this variable is zero when we are writing value positional parameters
            // in object positional parameters, this stores the depth of the hierarchy we are current at.
            // depth = number of SO nodes - number of EO nodes
            // if we have the following nodes: SO, SM, SO, we are at depth 2, since we have seen two
            // start object nodes but no end object node.
            public int CurrentDepth
            {
                get;
                set;
            }

            public WriterState ReturnState
            {
                get;
                set;
            }

            public void Reset()
            {
                NodesList = new List<List<XamlNode>>();

                Writer.Reset();
                Writer.WriteStartObject(XamlLanguage.MarkupExtension);
                Writer.WriteStartMember(XamlLanguage.PositionalParameters);

                CurrentDepth = 0;
            }
        }
    }

    // need to implement our own Set class to alleviate ties to System.Core.dll
    // HashSet<T> lives in System.Core.dll
    internal class XamlPropertySet
    {
        Dictionary<XamlMember, bool> dictionary = new Dictionary<XamlMember, bool>();

        public bool Contains(XamlMember member)
        {
            return dictionary.ContainsKey(member);
        }

        public void Add(XamlMember member)
        {
            dictionary.Add(member, true);
        }
    }
}
