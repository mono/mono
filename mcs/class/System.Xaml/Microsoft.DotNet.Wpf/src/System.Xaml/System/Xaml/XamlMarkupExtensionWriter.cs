// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Xaml
{
    using System.Collections.Generic;
    using System.Text;
    using System.Windows.Markup;

    class XamlMarkupExtensionWriter : XamlWriter
    {
        StringBuilder sb;
        Stack<Node> nodes;
        WriterState currentState;
        XamlXmlWriter xamlXmlWriter;
        XamlXmlWriterSettings settings;
        XamlMarkupExtensionWriterSettings meSettings;
        bool failed;

        public XamlMarkupExtensionWriter(XamlXmlWriter xamlXmlWriter)
        {
            Initialize(xamlXmlWriter);
        }

        public XamlMarkupExtensionWriter(XamlXmlWriter xamlXmlWriter, XamlMarkupExtensionWriterSettings meSettings)
        {
            this.meSettings = meSettings;
            Initialize(xamlXmlWriter);
        }

        void Initialize(XamlXmlWriter xamlXmlWriter)
        {
            this.xamlXmlWriter = xamlXmlWriter;
            this.settings = xamlXmlWriter.Settings; // This will clone, only want to do this once
            this.meSettings = this.meSettings ?? new XamlMarkupExtensionWriterSettings();
            currentState = Start.State;
            sb = new StringBuilder();
            nodes = new Stack<Node>();
            failed = false;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        public override XamlSchemaContext SchemaContext
        {
            get
            {
                return this.xamlXmlWriter.SchemaContext;
            }
        }

        public void Reset()
        {
            currentState = Start.State;
            sb = new StringBuilder();
            nodes.Clear();
            failed = false;
        }

        // MarkupExtensionString is used to obtain the curly-formatted markup extension string.
        // It should be called after calling the final WriteEndObject().
        // If MarkupExtensionString is not called before writing the next markup extension string
        // in curly syntax, the previous markup extension string is lost.
        public string MarkupExtensionString
        {
            get
            {
                if (this.nodes.Count == 0)
                {
                    return sb.ToString();
                }
                else
                {
                    return null;
                }
            }
        }

        // This is set to true when the Markup Extension Writer fails to write
        // the given node stream in curly form.
        public bool Failed
        {
            get
            {
                return failed;
            }
        }

        string LookupPrefix(XamlType type)
        {
            string chosenNamespace;
            string prefix = this.xamlXmlWriter.LookupPrefix(type.GetXamlNamespaces(), out chosenNamespace);

            if (prefix == null)
            {
                if (!this.meSettings.ContinueWritingWhenPrefixIsNotFound)
                {
                    // the prefix is not found and curly syntax has no way of defining a prefix
                    failed = true;
                    return String.Empty; // what we return here is not important, since Failed has set to be true
                }
            }

            return prefix;
        }

        string LookupPrefix(XamlMember property)
        {
            string chosenNamespace;
            string prefix = this.xamlXmlWriter.LookupPrefix(property.GetXamlNamespaces(), out chosenNamespace);

            if (prefix == null)
            {
                if (!this.meSettings.ContinueWritingWhenPrefixIsNotFound)
                {
                    failed = true;
                    // the prefix is not found and curly syntax has no way of defining a prefix
                    return String.Empty; // what we return here is not important, since Failed has set to be true
                }
            }

            return prefix;
        }

        void CheckMemberForUniqueness(Node objectNode, XamlMember property)
        {
            if (!this.settings.AssumeValidInput)
            {
                if (objectNode.Members == null)
                {
                    objectNode.Members = new XamlPropertySet();
                }
                else if (objectNode.Members.Contains(property))
                {
                    throw new InvalidOperationException(SR.Get(SRID.XamlMarkupExtensionWriterDuplicateMember, property.Name));
                }
                objectNode.Members.Add(property);
            }
        }

        public override void WriteStartObject(XamlType type)
        {
            this.currentState.WriteStartObject(this, type);
        }

        public override void WriteGetObject()
        {
            this.currentState.WriteGetObject(this);
        }

        public override void WriteEndObject()
        {
            this.currentState.WriteEndObject(this);
        }

        public override void WriteStartMember(XamlMember property)
        {
            this.currentState.WriteStartMember(this, property);
        }

        public override void WriteEndMember()
        {
            this.currentState.WriteEndMember(this);
        }

        public override void WriteNamespace(NamespaceDeclaration namespaceDeclaration)
        {
            this.currentState.WriteNamespace(this, namespaceDeclaration);
        }

        public override void WriteValue(object value)
        {
            string s = value as string;

            if (s == null)
            {
                throw new ArgumentException(SR.Get(SRID.XamlMarkupExtensionWriterCannotWriteNonstringValue));
            }

            this.currentState.WriteValue(this, s);
        }

        class Node
        {
            public XamlMember XamlProperty
            {
                get;
                set;
            }
            public XamlPropertySet Members
            {
                get;
                set;
            }
            public XamlNodeType NodeType
            {
                get;
                set;
            }
            public XamlType XamlType
            {
                get;
                set;
            }
        }

        abstract class WriterState
        {
            //according to the BNF, CharactersToEscape ::= ['",={}\]
            static char[] specialChars = new char[] { '\'', '"', ',', '=', '{', '}', '\\', ' ' };

            public virtual void WriteStartObject(XamlMarkupExtensionWriter writer, XamlType type)
            {
                writer.failed = true;
            }

            public virtual void WriteGetObject(XamlMarkupExtensionWriter writer)
            {
                writer.failed = true;
            }

            public virtual void WriteEndObject(XamlMarkupExtensionWriter writer)
            {
                writer.failed = true;
            }

            public virtual void WriteStartMember(XamlMarkupExtensionWriter writer, XamlMember property)
            {
                writer.failed = true;
            }

            public virtual void WriteEndMember(XamlMarkupExtensionWriter writer)
            {
                writer.failed = true;
            }

            public virtual void WriteValue(XamlMarkupExtensionWriter writer, string value)
            {
                writer.failed = true;
            }

            public virtual void WriteNamespace(XamlMarkupExtensionWriter writer, NamespaceDeclaration namespaceDeclaration)
            {
                writer.failed = true;
            }

            protected static bool ContainCharacterToEscape(string s)
            {
                return s.IndexOfAny(specialChars) >= 0;
            }

            protected static string FormatStringInCorrectSyntax(string s)
            {
                StringBuilder sb = new StringBuilder("\"");
                for (int i = 0; i < s.Length; i++)
                {
                    // BNF: DoubleQuotedValue ::= '"' ((Char - ["\]) | '\"' | '\\')+ '"'
                    // so the only characters we need to skip are the backslash and the double quote.

                    if (s[i] == '\\' || s[i] == '"')
                    {
                        sb.Append("\\");
                    }
                    sb.Append(s[i]);
                }

                sb.Append("\"");
                return sb.ToString();
            }

            protected void WritePrefix(XamlMarkupExtensionWriter writer, string prefix)
            {
                if (prefix != "")
                {
                    writer.sb.Append(prefix);
                    writer.sb.Append(":");
                }
            }

            public void WriteString(XamlMarkupExtensionWriter writer, string value)
            {
                if (ContainCharacterToEscape(value) || value == String.Empty)
                {
                    value = FormatStringInCorrectSyntax(value);
                }
                writer.sb.Append(value);
            }
        }

        // XamlMarkupExtensionWriter returns to this state after a markup extension has been completed,
        // i.e. when the number of closing curly bracket "}" matches the number of opening curly bracket "{".
        // At this state, XamlMarkupExtensionWriter is ready to start writing a markup extension
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

            public override void WriteStartObject(XamlMarkupExtensionWriter writer, XamlType type)
            {
                writer.Reset();

                string prefix = writer.LookupPrefix(type);

                writer.sb.Append("{");
                WritePrefix(writer, prefix);
                writer.sb.Append(XamlXmlWriter.GetTypeName(type));

                writer.nodes.Push(new Node { NodeType = XamlNodeType.StartObject, XamlType = type });
                writer.currentState = InObjectBeforeMember.State;
            }
        }

        abstract class InObject : WriterState
        {
            protected InObject()
            {
            }

            public abstract string Delimiter
            {
                get;
            }

            public override void WriteEndObject(XamlMarkupExtensionWriter writer)
            {
                if (writer.nodes.Count == 0)
                {
                    throw new InvalidOperationException(SR.Get(SRID.XamlMarkupExtensionWriterInputInvalid));
                }

                Node node = writer.nodes.Pop();

                if (node.NodeType != XamlNodeType.StartObject)
                {
                    throw new InvalidOperationException(SR.Get(SRID.XamlMarkupExtensionWriterInputInvalid));
                }

                writer.sb.Append("}");

                if (writer.nodes.Count == 0)
                {
                    writer.currentState = Start.State;
                }
                else
                {
                    Node member = writer.nodes.Peek();
                    if (member.NodeType != XamlNodeType.StartMember)
                    {
                        throw new InvalidOperationException(SR.Get(SRID.XamlMarkupExtensionWriterInputInvalid));
                    }

                    if (member.XamlProperty == XamlLanguage.PositionalParameters)
                    {
                        writer.currentState = InPositionalParametersAfterValue.State;
                    }
                    else
                    {
                        writer.currentState = InMemberAfterValueOrEndObject.State;
                    }
                }
            }

            protected void UpdateStack(XamlMarkupExtensionWriter writer, XamlMember property)
            {
                if (writer.nodes.Count == 0)
                {
                    throw new InvalidOperationException(SR.Get(SRID.XamlMarkupExtensionWriterInputInvalid));
                }

                Node objectNode = writer.nodes.Peek();

                if (objectNode.NodeType != XamlNodeType.StartObject)
                {
                    throw new InvalidOperationException(SR.Get(SRID.XamlMarkupExtensionWriterInputInvalid));
                }

                writer.CheckMemberForUniqueness(objectNode, property);

                writer.nodes.Push(new Node
                {
                    NodeType = XamlNodeType.StartMember,
                    XamlType = objectNode.XamlType,
                    XamlProperty = property
                });
            }

            protected void WriteNonPositionalParameterMember(XamlMarkupExtensionWriter writer, XamlMember property)
            {
                if (XamlXmlWriter.IsImplicit(property) ||
                    (property.IsDirective && (property.Type.IsCollection || property.Type.IsDictionary)))
                {
                    writer.failed = true;
                    return;
                }

                if (property.IsDirective)
                {
                    writer.sb.Append(Delimiter);
                    WritePrefix(writer, writer.LookupPrefix(property));
                    writer.sb.Append(property.Name);
                }
                else if (property.IsAttachable)
                {
                    writer.sb.Append(Delimiter);
                    WritePrefix(writer, writer.LookupPrefix(property));
                    string local = property.DeclaringType.Name + "." + property.Name;
                    writer.sb.Append(local);
                }
                else
                {
                    writer.sb.Append(Delimiter);
                    writer.sb.Append(property.Name);
                }

                writer.sb.Append("=");

                writer.currentState = InMember.State;
            }
        }

        class InObjectBeforeMember : InObject
        {
            static WriterState state = new InObjectBeforeMember();
            InObjectBeforeMember()
            {
            }
            public static WriterState State
            {
                get { return state; }
            }

            public override string Delimiter
            {
                get { return " "; }
            }

            public override void WriteStartMember(XamlMarkupExtensionWriter writer, XamlMember property)
            {
                UpdateStack(writer, property);
                if (property == XamlLanguage.PositionalParameters)
                {
                    writer.currentState = InPositionalParametersBeforeValue.State;
                }
                else
                {
                    WriteNonPositionalParameterMember(writer, property);
                }
            }
        }

        class InObjectAfterMember : InObject
        {
            static WriterState state = new InObjectAfterMember();
            InObjectAfterMember()
            {
            }
            public static WriterState State
            {
                get { return state; }
            }

            public override string Delimiter
            {
                get { return ", "; }
            }

            public override void WriteStartMember(XamlMarkupExtensionWriter writer, XamlMember property)
            {
                UpdateStack(writer, property);
                WriteNonPositionalParameterMember(writer, property);
            }
        }

        abstract class InPositionalParameters : WriterState
        {
            protected InPositionalParameters()
            {
            }

            public abstract string Delimiter
            {
                get;
            }

            public override void WriteValue(XamlMarkupExtensionWriter writer, string value)
            {
                writer.sb.Append(Delimiter);
                WriteString(writer, value);
                writer.currentState = InPositionalParametersAfterValue.State;
            }

            public override void WriteStartObject(XamlMarkupExtensionWriter writer, XamlType type)
            {
                writer.sb.Append(Delimiter);
                writer.currentState = InMember.State;
                writer.currentState.WriteStartObject(writer, type);
            }
        }

        class InPositionalParametersBeforeValue : InPositionalParameters
        {
            static WriterState state = new InPositionalParametersBeforeValue();
            InPositionalParametersBeforeValue()
            {
            }
            public static WriterState State
            {
                get { return state; }
            }

            public override string Delimiter
            {
                get { return " "; }
            }
        }

        class InPositionalParametersAfterValue : InPositionalParameters
        {
            static WriterState state = new InPositionalParametersAfterValue();
            InPositionalParametersAfterValue()
            {
            }
            public static WriterState State
            {
                get { return state; }
            }

            public override string Delimiter
            {
                get { return ", "; }
            }

            public override void WriteEndMember(XamlMarkupExtensionWriter writer)
            {
                Node node = writer.nodes.Pop();

                if (node.NodeType != XamlNodeType.StartMember || node.XamlProperty != XamlLanguage.PositionalParameters)
                {
                    throw new InvalidOperationException(SR.Get(SRID.XamlMarkupExtensionWriterInputInvalid));
                }
                writer.currentState = InObjectAfterMember.State;
            }
        }

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

            public override void WriteValue(XamlMarkupExtensionWriter writer, string value)
            {
                WriteString(writer, value);
                writer.currentState = InMemberAfterValueOrEndObject.State;
            }

            public override void WriteStartObject(XamlMarkupExtensionWriter writer, XamlType type)
            {
                if (!type.IsMarkupExtension)
                {
                    // can not write a non-ME object in this state in curly form
                    writer.failed = true;
                    return;
                }
                string prefix = writer.LookupPrefix(type);

                writer.sb.Append("{");
                WritePrefix(writer, prefix);
                writer.sb.Append(XamlXmlWriter.GetTypeName(type));

                writer.nodes.Push(new Node { NodeType = XamlNodeType.StartObject, XamlType = type });
                writer.currentState = InObjectBeforeMember.State;
            }
        }

        class InMemberAfterValueOrEndObject : WriterState
        {
            static WriterState state = new InMemberAfterValueOrEndObject();
            InMemberAfterValueOrEndObject()
            {
            }
            public static WriterState State
            {
                get { return state; }
            }

            public override void WriteEndMember(XamlMarkupExtensionWriter writer)
            {
                if (writer.nodes.Count == 0)
                {
                    throw new InvalidOperationException(SR.Get(SRID.XamlMarkupExtensionWriterInputInvalid));
                }

                Node member = writer.nodes.Pop();

                if (member.NodeType != XamlNodeType.StartMember)
                {
                    throw new InvalidOperationException(SR.Get(SRID.XamlMarkupExtensionWriterInputInvalid));
                }

                writer.currentState = InObjectAfterMember.State;
            }
        }
    }

    internal class XamlMarkupExtensionWriterSettings
    {
        public bool ContinueWritingWhenPrefixIsNotFound
        {
            get;
            set;
        }
    }
}
