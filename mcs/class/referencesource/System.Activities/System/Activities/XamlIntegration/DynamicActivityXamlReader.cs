//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.XamlIntegration
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Xaml;
    using System.Xaml.Schema;

    // This Xaml Reader converts an <Activity x:Class=Foo to <DynamicActivity Name=Foo
    // it does the folowing
    // Rewrites any record of type "Activity" to "DynamicActivity"
    // Rewrites any member of type "Activity" to member "DynamicActivity"
    // Rewrites x:Class to DynamicActivity.Name
    // Recognizes DynamicActivity<T>.
    //
    // This Xaml Reader also supports ActivityBuilder, which has the same basic node structure
    class DynamicActivityXamlReader : XamlReader, IXamlLineInfo
    {
        internal static readonly XamlMember xPropertyType = XamlLanguage.Property.GetMember("Type");
        internal static readonly XamlMember xPropertyName = XamlLanguage.Property.GetMember("Name");
        internal static readonly XamlMember xPropertyAttributes = XamlLanguage.Property.GetMember("Attributes");

        // These may be a closed generic types in the Activity<T> case, so we compute them dynamically
        XamlType activityReplacementXamlType;
        XamlType activityXamlType;

        readonly XamlType baseActivityXamlType;
        readonly XamlType activityPropertyXamlType;
        readonly XamlType xamlTypeXamlType;
        readonly XamlType typeXamlType;

        readonly XamlMember activityPropertyType;
        readonly XamlMember activityPropertyName;
        readonly XamlMember activityPropertyAttributes;
        readonly XamlMember activityPropertyValue;

        readonly XamlReader innerReader;
        readonly NamespaceTable namespaceTable;

        const string clrNamespacePart = "clr-namespace:";

        int depth;
        bool notRewriting;

        int inXClassDepth;

        XamlTypeName xClassName;
        IXamlLineInfo nodeReaderLineInfo;
        IXamlLineInfo innerReaderLineInfo;

        bool frontLoadedDirectives;
        XamlSchemaContext schemaContext;
        bool isBuilder;
        bool hasLineInfo;

        // we pull off of the innerReader and into this nodeList, where we use its reader
        XamlNodeQueue nodeQueue;
        XamlReader nodeReader;

        // Properties are tricky since they support default values, and those values
        // can appear anywhere in the XAML document. So we need to buffer their XAML 
        // nodes and present them only at the end of the document (right before the 
        // document end tag), when we have both the declaration and the value realized.
        BufferedPropertyList bufferedProperties;

        // in the ActivityBuilder case we need to jump through some extra hoops to 
        // support PropertyReferenceExtension, since in the ActivityBuilder case
        // Implementation isn't a template (Func<Activity>), so we need to map
        // such members into attached properties on their parent object
        BuilderStack builderStack;

        public DynamicActivityXamlReader(XamlReader innerReader)
            : this(innerReader, null)
        {
        }

        public DynamicActivityXamlReader(XamlReader innerReader, XamlSchemaContext schemaContext)
            : this(false, innerReader, schemaContext)
        {
        }

        public DynamicActivityXamlReader(bool isBuilder, XamlReader innerReader, XamlSchemaContext schemaContext)
            : base()
        {
            this.isBuilder = isBuilder;
            this.innerReader = innerReader;
            this.schemaContext = schemaContext ?? innerReader.SchemaContext;

            this.xamlTypeXamlType = this.schemaContext.GetXamlType(typeof(XamlType));
            this.typeXamlType = this.schemaContext.GetXamlType(typeof(Type));

            this.baseActivityXamlType = this.schemaContext.GetXamlType(typeof(Activity));
            this.activityPropertyXamlType = this.schemaContext.GetXamlType(typeof(DynamicActivityProperty));
            this.activityPropertyType = this.activityPropertyXamlType.GetMember("Type");
            this.activityPropertyName = this.activityPropertyXamlType.GetMember("Name");
            this.activityPropertyValue = this.activityPropertyXamlType.GetMember("Value");
            this.activityPropertyAttributes = this.activityPropertyXamlType.GetMember("Attributes");

            this.namespaceTable = new NamespaceTable();
            this.frontLoadedDirectives = true;

            // we pump items through this node-list when rewriting
            this.nodeQueue = new XamlNodeQueue(this.schemaContext);
            this.nodeReader = this.nodeQueue.Reader;
            IXamlLineInfo lineInfo = innerReader as IXamlLineInfo;
            if (lineInfo != null && lineInfo.HasLineInfo)
            {
                this.innerReaderLineInfo = lineInfo;
                this.nodeReaderLineInfo = (IXamlLineInfo)nodeQueue.Reader;
                this.hasLineInfo = true;
            }
        }

        public override XamlType Type
        {
            get
            {
                return this.nodeReader.Type;
            }
        }

        public override NamespaceDeclaration Namespace
        {
            get
            {
                return this.nodeReader.Namespace;
            }
        }

        public override object Value
        {
            get
            {
                return this.nodeReader.Value;
            }
        }

        public override bool IsEof
        {
            get
            {
                return this.nodeReader.IsEof;
            }
        }

        public override XamlMember Member
        {
            get
            {
                return this.nodeReader.Member;
            }
        }

        public override XamlSchemaContext SchemaContext
        {
            get
            {
                return this.schemaContext;
            }
        }

        public override XamlNodeType NodeType
        {
            get
            {
                return this.nodeReader.NodeType;
            }
        }

        public bool HasLineInfo
        {
            get
            {
                return this.hasLineInfo;
            }
        }

        public int LineNumber
        {
            get
            {
                if (this.hasLineInfo)
                {
                    return this.nodeReaderLineInfo.LineNumber;
                }
                else
                {
                    return 0;
                }
            }
        }

        public int LinePosition
        {
            get
            {
                if (this.hasLineInfo)
                {
                    return this.nodeReaderLineInfo.LinePosition;
                }
                else
                {
                    return 0;
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    this.innerReader.Close();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        static XamlException CreateXamlException(string message, IXamlLineInfo lineInfo)
        {
            if (lineInfo != null && lineInfo.HasLineInfo)
            {
                return new XamlException(message, null, lineInfo.LineNumber, lineInfo.LinePosition);
            }
            else
            {
                return new XamlException(message);
            }
        }

        // perf optimization to efficiently support non-Activity types
        void DisableRewrite()
        {
            this.notRewriting = true;
            this.nodeReader = this.innerReader;
            this.nodeReaderLineInfo = this.innerReader as IXamlLineInfo;
        }

        public override bool Read()
        {
            if (this.notRewriting)
            {
                Fx.Assert(object.ReferenceEquals(this.innerReader, this.nodeReader), "readers must match at this point");
                return this.nodeReader.Read();
            }

            // for properties, we'll store nodes "on the side"
            bool innerReaderResult = this.innerReader.Read();
            bool continueProcessing = true;
            while (continueProcessing && !this.innerReader.IsEof)
            {
                // ProcessCurrentNode will only return true if it has advanced the innerReader
                continueProcessing = ProcessCurrentNode();
            }

            // rewriting may have been disabled under ProcessCurrentNode
            if (this.notRewriting)
            {
                return innerReaderResult;
            }
            else
            {
                // now that we've mapped the innerReader into (at least) one node entry, pump that reader as well
                return this.nodeReader.Read();
            }
        }

        // pull on our inner reader, map the results as necessary, and pump
        // mapped results into the streaming node reader that we're offering up.
        // return true if we need to keep pumping (because we've buffered some nodes on the side)
        bool ProcessCurrentNode()
        {
            bool processedNode = false;
            this.namespaceTable.ManageNamespace(this.innerReader);

            switch (this.innerReader.NodeType)
            {
                case XamlNodeType.StartMember:
                    XamlMember currentMember = this.innerReader.Member;
                    // find out if the member is a default value for one of
                    // our declared properties. If it is, then we have a complex case
                    // where we need to:
                    // 1) read the nodes into a side list 
                    // 2) interleave these nodes with the DynamicActivityProperty nodes
                    //    since they need to appear as DynamicActivityProperty.Value
                    // 3) right before we hit the last node, we'll dump the side node-lists
                    //    reflecting a zipped up representation of the Properties
                    if (IsXClassName(currentMember.DeclaringType))
                    {
                        if (this.bufferedProperties == null)
                        {
                            this.bufferedProperties = new BufferedPropertyList(this);
                        }
                        this.bufferedProperties.BufferDefaultValue(currentMember.Name, this.activityPropertyValue, this.innerReader, this.innerReaderLineInfo);
                        return true; // output cursor didn't move forward
                    }
                    else if (this.frontLoadedDirectives && currentMember == XamlLanguage.FactoryMethod)
                    {
                        DisableRewrite();
                        return false;
                    }
                    else
                    {
                        this.depth++;
                        if (this.depth == 2)
                        {
                            if (currentMember.DeclaringType == this.activityXamlType || currentMember.DeclaringType == this.baseActivityXamlType)
                            {
                                // Rewrite "<Activity.XXX>" to "<DynamicActivity.XXX>"
                                XamlMember member = this.activityReplacementXamlType.GetMember(currentMember.Name);
                                if (member == null)
                                {
                                    throw FxTrace.Exception.AsError(CreateXamlException(SR.MemberNotSupportedByActivityXamlServices(currentMember.Name), this.innerReaderLineInfo));
                                }

                                this.nodeQueue.Writer.WriteStartMember(member, this.innerReaderLineInfo);

                                if (member.Name == "Constraints")
                                {
                                    WriteWrappedMember(true);
                                    processedNode = true;
                                    return true;
                                }

                                processedNode = true;

                                // if we're in ActivityBuilder.Implementation, start buffering nodes
                                if (this.isBuilder && member.Name == "Implementation")
                                {
                                    this.builderStack = new BuilderStack(this);
                                }
                            }
                            else if (currentMember == XamlLanguage.Class)
                            {
                                this.inXClassDepth = this.depth;

                                // Rewrite x:Class to DynamicActivity.Name
                                this.nodeQueue.Writer.WriteStartMember(this.activityReplacementXamlType.GetMember("Name"), this.innerReaderLineInfo);
                                processedNode = true;
                            }
                            else if (currentMember == XamlLanguage.Members)
                            {
                                // Rewrite "<x:Members>" to "<DynamicActivity.Properties>"
                                if (this.bufferedProperties == null)
                                {
                                    this.bufferedProperties = new BufferedPropertyList(this);
                                }
                                this.bufferedProperties.BufferDefinitions(this);
                                this.depth--;
                                return true; // output cursor didn't move forward
                            }
                            else if (currentMember == XamlLanguage.ClassAttributes)
                            {
                                // Rewrite x:ClassAttributes to DynamicActivity.Attributes
                                this.nodeQueue.Writer.WriteStartMember(this.activityReplacementXamlType.GetMember("Attributes"), this.innerReaderLineInfo);
                                // x:ClassAttributes directive has no following GetObject, but Attributes does since it's not a directive
                                WriteWrappedMember(false);
                                processedNode = true;
                                return true;
                            }
                        }
                    }
                    break;

                case XamlNodeType.StartObject:
                    {
                        EnterObject();
                        if (this.depth == 1)
                        {
                            // see if we're deserializing an Activity
                            if (this.innerReader.Type.UnderlyingType == typeof(Activity))
                            {
                                // Rewrite "<Activity>" to "<DynamicActivity>"
                                this.activityXamlType = this.innerReader.Type;
                                if (this.isBuilder)
                                {
                                    this.activityReplacementXamlType = SchemaContext.GetXamlType(typeof(ActivityBuilder));
                                }
                                else
                                {
                                    this.activityReplacementXamlType = SchemaContext.GetXamlType(typeof(DynamicActivity));
                                }
                            }
                            // or an Activity<TResult>
                            else if (this.innerReader.Type.IsGeneric && this.innerReader.Type.UnderlyingType != null
                                && this.innerReader.Type.UnderlyingType.GetGenericTypeDefinition() == typeof(Activity<>))
                            {
                                // Rewrite "<Activity typeArgument=T>" to "<DynamicActivity typeArgument=T>" 
                                this.activityXamlType = this.innerReader.Type;

                                Type activityType = this.innerReader.Type.TypeArguments[0].UnderlyingType;
                                Type activityReplacementGenericType;
                                if (this.isBuilder)
                                {
                                    activityReplacementGenericType = typeof(ActivityBuilder<>).MakeGenericType(activityType);
                                }
                                else
                                {
                                    activityReplacementGenericType = typeof(DynamicActivity<>).MakeGenericType(activityType);
                                }

                                this.activityReplacementXamlType = SchemaContext.GetXamlType(activityReplacementGenericType);
                            }
                            // otherwise disable rewriting so that we're a pass through
                            else
                            {
                                DisableRewrite();
                                return false;
                            }

                            this.nodeQueue.Writer.WriteStartObject(this.activityReplacementXamlType, this.innerReaderLineInfo);
                            processedNode = true;
                        }

                    }
                    break;

                case XamlNodeType.GetObject:
                    EnterObject();
                    break;

                case XamlNodeType.EndObject:
                case XamlNodeType.EndMember:
                    ExitObject();
                    break;

                case XamlNodeType.Value:
                    if (this.inXClassDepth >= this.depth && this.xClassName == null)
                    {
                        string fullName = (string)this.innerReader.Value;
                        string xClassNamespace = "";
                        string xClassName = fullName;

                        int nameStartIndex = fullName.LastIndexOf('.');
                        if (nameStartIndex > 0)
                        {
                            xClassNamespace = fullName.Substring(0, nameStartIndex);
                            xClassName = fullName.Substring(nameStartIndex + 1);
                        }

                        this.xClassName = new XamlTypeName(xClassNamespace, xClassName);
                    }
                    break;
            }

            if (!processedNode)
            {
                if (this.builderStack != null)
                {
                    bool writeNode = true;
                    this.builderStack.ProcessNode(this.innerReader, this.innerReaderLineInfo, this.nodeQueue.Writer, out writeNode);
                    if (!writeNode)
                    {
                        this.innerReader.Read();
                        return true;
                    }
                }
                this.nodeQueue.Writer.WriteNode(this.innerReader, this.innerReaderLineInfo);
            }
            return false;
        }

        // used for a number of cases when wrapping we need to add a GetObject/StartMember(_Items) since XAML directives intrinsically
        // take care of it
        void WriteWrappedMember(bool stripWhitespace)
        {
            this.nodeQueue.Writer.WriteGetObject(this.innerReaderLineInfo);
            this.nodeQueue.Writer.WriteStartMember(XamlLanguage.Items, this.innerReaderLineInfo);
            XamlReader subReader = this.innerReader.ReadSubtree();

            // 1) Read past the start member since we wrote it above
            subReader.Read();

            // 2) copy over the rest of the subnodes, possibly discarding top-level whitespace from WhitespaceSignificantCollection
            subReader.Read();
            while (!subReader.IsEof)
            {
                bool isWhitespaceNode = false;
                if (subReader.NodeType == XamlNodeType.Value)
                {
                    string stringValue = subReader.Value as string;
                    if (stringValue != null && stringValue.Trim().Length == 0)
                    {
                        isWhitespaceNode = true;
                    }
                }

                if (isWhitespaceNode && stripWhitespace)
                {
                    subReader.Read();
                }
                else
                {
                    XamlWriterExtensions.Transform(subReader.ReadSubtree(), this.nodeQueue.Writer, this.innerReaderLineInfo, false);
                }
            }


            // close the GetObject added above. Note that we are doing EndObject/EndMember after the last node (EndMember) 
            // rather than inserting EndMember/EndObject before the last EndMember since all EndMembers are interchangable from a state perspective
            this.nodeQueue.Writer.WriteEndObject(this.innerReaderLineInfo);
            this.nodeQueue.Writer.WriteEndMember(this.innerReaderLineInfo);

            subReader.Close();

            // we hand exited a member where we had increased the depth manually, so record that fact
            ExitObject();
        }

        // when Read hits StartObject or GetObject
        void EnterObject()
        {
            this.depth++;
            if (this.depth >= 2)
            {
                this.frontLoadedDirectives = false;
            }
        }

        // when Read hits EndObject or EndMember
        void ExitObject()
        {
            if (this.depth <= this.inXClassDepth)
            {
                this.inXClassDepth = 0;
            }

            this.depth--;
            this.frontLoadedDirectives = false;

            if (this.depth == 1)
            {
                this.builderStack = null;
            }
            else if (this.depth == 0)
            {
                // we're about to write out the last tag. Dump our accrued properties 
                // as no more property values are forthcoming. 
                if (this.bufferedProperties != null)
                {
                    this.bufferedProperties.FlushTo(this.nodeQueue, this);
                }
            }
        }

        bool IsXClassName(XamlType xamlType)
        {
            if (xamlType == null || this.xClassName == null || xamlType.Name != this.xClassName.Name)
            {
                return false;
            }

            // this code is kept for back compatible
            string preferredNamespace = xamlType.PreferredXamlNamespace;
            if (preferredNamespace.Contains(clrNamespacePart))
            {
                return IsXClassName(preferredNamespace);
            }

            // GetXamlNamespaces is a superset of PreferredXamlNamespace, it's not a must for the above code
            // to check for preferredXamlNamespace, but since the old code uses .Contains(), which was a minor bug,
            // we decide to use StartsWith in new code and keep the old code for back compatible reason.
            IList<string> namespaces = xamlType.GetXamlNamespaces();
            foreach (string ns in namespaces)
            {
                if (ns.StartsWith(clrNamespacePart, StringComparison.Ordinal))
                {
                    return IsXClassName(ns);
                }
            }

            return false;
        }

        bool IsXClassName(string ns)
        {
            string clrNamespace = ns.Substring(clrNamespacePart.Length);

            int lastIndex = clrNamespace.IndexOf(';');
            if (lastIndex < 0 || lastIndex > clrNamespace.Length)
            {
                lastIndex = clrNamespace.Length;
            }

            string @namespace = clrNamespace.Substring(0, lastIndex);
            return this.xClassName.Namespace == @namespace;
        }

        static void IncrementIfPositive(ref int a)
        {
            if (a > 0)
            {
                a++;
            }
        }

        static void DecrementIfPositive(ref int a)
        {
            if (a > 0)
            {
                a--;
            }
        }

        // This class tracks the information we need to be able to convert
        // <PropertyReferenceExtension> into <ActivityBuilder.PropertyReferences>
        class BuilderStack
        {
            readonly XamlType activityPropertyReferenceXamlType;
            readonly XamlMember activityBuilderPropertyReferencesMember;
            readonly XamlMember activityPropertyReferenceSourceProperty;
            readonly XamlMember activityPropertyReferenceTargetProperty;

            MemberInformation bufferedMember;
            DynamicActivityXamlReader parent;
            Stack<Frame> stack;

            public BuilderStack(DynamicActivityXamlReader parent)
            {
                this.parent = parent;
                this.stack = new Stack<Frame>();
                this.activityPropertyReferenceXamlType = parent.schemaContext.GetXamlType(typeof(ActivityPropertyReference));
                this.activityPropertyReferenceSourceProperty = this.activityPropertyReferenceXamlType.GetMember("SourceProperty");
                this.activityPropertyReferenceTargetProperty = this.activityPropertyReferenceXamlType.GetMember("TargetProperty");
                XamlType typeOfActivityBuilder = parent.schemaContext.GetXamlType(typeof(ActivityBuilder));
                this.activityBuilderPropertyReferencesMember = typeOfActivityBuilder.GetAttachableMember("PropertyReferences");
            }

            string ReadPropertyReferenceExtensionPropertyName(XamlReader reader)
            {
                string sourceProperty = null;
                reader.Read();
                while (!reader.IsEof && reader.NodeType != XamlNodeType.EndObject)
                {
                    if (IsExpectedPropertyReferenceMember(reader))
                    {
                        string propertyName = ReadPropertyName(reader);
                        if (propertyName != null)
                        {
                            sourceProperty = propertyName;
                        }
                    }
                    else
                    {
                        // unexpected members.
                        // For compat with 4.0, unexpected members on PropertyReferenceExtension
                        // are silently ignored
                        reader.Skip();
                    }
                }

                return sourceProperty;
            }

            // Whenever we encounter a StartMember, we buffer it (and any namespace nodes folllowing it)
            // until we see its contents (SO/GO/V).
            // If the content is a PropertyReferenceExtension, then we convert it to an ActivityPropertyReference
            // in the parent object's ActivityBuilder.PropertyReference collection, and dont' write out the member.
            // If the content is not a PropertyReferenceExtension, or there's no content (i.e. we hit an EM),
            // we flush the buffered SM + NS*, and continue as normal.
            public void ProcessNode(XamlReader reader, IXamlLineInfo lineInfo, XamlWriter targetWriter, out bool writeNodeToOutput)
            {
                writeNodeToOutput = true;

                switch (reader.NodeType)
                {
                    case XamlNodeType.StartMember:
                        this.bufferedMember = new MemberInformation(reader.Member, lineInfo);
                        writeNodeToOutput = false;
                        break;

                    case XamlNodeType.EndMember:
                        FlushBufferedMember(targetWriter);
                        if (this.stack.Count > 0)
                        {
                            Frame curFrame = this.stack.Peek();
                            if (curFrame.SuppressNextEndMember)
                            {
                                writeNodeToOutput = false;
                                curFrame.SuppressNextEndMember = false;
                            }
                        }
                        break;

                    case XamlNodeType.StartObject:
                        Frame newFrame;
                        if (IsPropertyReferenceExtension(reader.Type) && this.bufferedMember.IsSet)
                        {                            
                            MemberInformation targetMember = this.bufferedMember;
                            this.bufferedMember = MemberInformation.None;                            
                            WritePropertyReferenceFrameToParent(targetMember, ReadPropertyReferenceExtensionPropertyName(reader), this.stack.Peek(), lineInfo);
                            writeNodeToOutput = false;
                            break;                            
                        }
                        else
                        {
                            FlushBufferedMember(targetWriter);
                            newFrame = new Frame();
                        }
                        this.stack.Push(newFrame);
                        break;

                    case XamlNodeType.GetObject:
                        FlushBufferedMember(targetWriter);
                        this.stack.Push(new Frame());
                        break;

                    case XamlNodeType.EndObject:
                        Frame frame = this.stack.Pop();
                        if (frame.PropertyReferences != null)
                        {
                            WritePropertyReferenceCollection(frame.PropertyReferences, targetWriter, lineInfo);
                        }
                        break;

                    case XamlNodeType.Value:
                        FlushBufferedMember(targetWriter);
                        break;

                    case XamlNodeType.NamespaceDeclaration:
                        if (this.bufferedMember.IsSet)
                        {
                            if (this.bufferedMember.FollowingNamespaces == null)
                            {
                                this.bufferedMember.FollowingNamespaces = new XamlNodeQueue(this.parent.schemaContext);
                            }
                            this.bufferedMember.FollowingNamespaces.Writer.WriteNode(reader, lineInfo);
                            writeNodeToOutput = false;
                        }
                        break;
                }
            }

            void FlushBufferedMember(XamlWriter targetWriter)
            {
                if (this.bufferedMember.IsSet)
                {
                    this.bufferedMember.Flush(targetWriter);
                    this.bufferedMember = MemberInformation.None;
                }
            }

            bool IsPropertyReferenceExtension(XamlType type)
            {
                return type != null && type.IsGeneric && type.UnderlyingType != null && type.Name == "PropertyReferenceExtension"
                    && type.UnderlyingType.GetGenericTypeDefinition() == typeof(PropertyReferenceExtension<>);
            }

            bool IsExpectedPropertyReferenceMember(XamlReader reader)
            {
                return reader.NodeType == XamlNodeType.StartMember && IsPropertyReferenceExtension(reader.Member.DeclaringType) && reader.Member.Name == "PropertyName";
            }

            string ReadPropertyName(XamlReader reader)
            {
                Fx.Assert(reader.Member.Name == "PropertyName", "Exepcted PropertyName member");
                string result = null;
                while (reader.Read() && reader.NodeType != XamlNodeType.EndMember)
                {
                    // For compat with 4.0, we only need to support PropertyName as Value node
                    if (reader.NodeType == XamlNodeType.Value)
                    {
                        string propertyName = reader.Value as string;
                        if (propertyName != null)
                        {
                            result = propertyName;
                        }
                    }
                }
                if (reader.NodeType == XamlNodeType.EndMember)
                {
                    // Our parent will never see this EndMember node so we need to force its
                    // depth count to decrement
                    this.parent.ExitObject();
                }
                return result;
            }

            void WritePropertyReferenceCollection(XamlNodeQueue serializedReferences, XamlWriter targetWriter, IXamlLineInfo lineInfo)
            {
                targetWriter.WriteStartMember(this.activityBuilderPropertyReferencesMember, lineInfo);
                targetWriter.WriteGetObject(lineInfo);
                targetWriter.WriteStartMember(XamlLanguage.Items, lineInfo);
                XamlServices.Transform(serializedReferences.Reader, targetWriter, false);
                targetWriter.WriteEndMember(lineInfo);
                targetWriter.WriteEndObject(lineInfo);
                targetWriter.WriteEndMember(lineInfo);
            }

            void WritePropertyReferenceFrameToParent(MemberInformation targetMember, string sourceProperty, Frame parentFrame, IXamlLineInfo lineInfo)
            {
                if (parentFrame.PropertyReferences == null)
                {
                    parentFrame.PropertyReferences = new XamlNodeQueue(this.parent.schemaContext);
                }
                WriteSerializedPropertyReference(parentFrame.PropertyReferences.Writer, lineInfo, targetMember.Member.Name, sourceProperty);
                
                // we didn't write out the target
                // StartMember, so suppress the EndMember
                parentFrame.SuppressNextEndMember = true;
            }

            void WriteSerializedPropertyReference(XamlWriter targetWriter, IXamlLineInfo lineInfo, string targetName, string sourceName)
            {
                // Line Info for the entire <ActivityPropertyReference> element 
                // comes from the end of the <PropertyReference> tag
                targetWriter.WriteStartObject(this.activityPropertyReferenceXamlType, lineInfo);
                targetWriter.WriteStartMember(this.activityPropertyReferenceTargetProperty, lineInfo);
                targetWriter.WriteValue(targetName, lineInfo);
                targetWriter.WriteEndMember(lineInfo);
                if (sourceName != null)
                {
                    targetWriter.WriteStartMember(this.activityPropertyReferenceSourceProperty, lineInfo);
                    targetWriter.WriteValue(sourceName, lineInfo);
                    targetWriter.WriteEndMember(lineInfo);
                }
                targetWriter.WriteEndObject(lineInfo);
            }

            struct MemberInformation
            {
                public static MemberInformation None = new MemberInformation();

                public XamlMember Member { get; set; }
                public int LineNumber { get; set; }
                public int LinePosition { get; set; }
                public XamlNodeQueue FollowingNamespaces { get; set; }

                public MemberInformation(XamlMember member, IXamlLineInfo lineInfo)
                    : this()
                {
                    Member = member;
                    if (lineInfo != null)
                    {
                        LineNumber = lineInfo.LineNumber;
                        LinePosition = lineInfo.LinePosition;
                    }
                }

                public bool IsSet
                {
                    get { return this.Member != null; }
                }

                public void Flush(XamlWriter targetWriter)
                {
                    targetWriter.WriteStartMember(Member, LineNumber, LinePosition);
                    if (FollowingNamespaces != null)
                    {
                        XamlServices.Transform(FollowingNamespaces.Reader, targetWriter, false);
                    }
                }
            }

            class Frame
            {
                public XamlNodeQueue PropertyReferences { get; set; }
                public bool SuppressNextEndMember { get; set; }
            }
        }

        // This class exists to "zip" together <x:Member> property definitions (to be rewritten as <DynamicActivityProperty> nodes)
        // with their corresponding default values <MyClass.Foo> (to be rewritten as <DynamicActivityProperty.Value> nodes).
        // Definitions come all at once, but values could come anywhere in the XAML document, so we save them all almost until the end of
        // the document and write them all out at once using BufferedPropertyList.CopyTo().
        class BufferedPropertyList
        {
            Dictionary<string, ActivityPropertyHolder> propertyHolders;
            Dictionary<string, ValueHolder> valueHolders;
            XamlNodeQueue outerNodes;
            DynamicActivityXamlReader parent;

            bool alreadyBufferedDefinitions;

            public BufferedPropertyList(DynamicActivityXamlReader parent)
            {
                this.parent = parent;
                this.outerNodes = new XamlNodeQueue(parent.SchemaContext);
            }

            // Called inside of an x:Members--read up to </x:Members>, buffering definitions
            public void BufferDefinitions(DynamicActivityXamlReader parent)
            {
                XamlReader subReader = parent.innerReader.ReadSubtree();
                IXamlLineInfo readerLineInfo = parent.innerReaderLineInfo;

                // 1) swap out the start member with <DynamicActivity.Properties>
                subReader.Read();
                Fx.Assert(subReader.NodeType == XamlNodeType.StartMember && subReader.Member == XamlLanguage.Members,
                    "Should be inside of x:Members before calling BufferDefinitions");
                this.outerNodes.Writer.WriteStartMember(parent.activityReplacementXamlType.GetMember("Properties"), readerLineInfo);

                // x:Members directive has no following GetObject, but Properties does since it's not a directive
                this.outerNodes.Writer.WriteGetObject(readerLineInfo);
                this.outerNodes.Writer.WriteStartMember(XamlLanguage.Items, readerLineInfo);

                // 2) process the subnodes and store them in either ActivityPropertyHolders,
                // or exigent nodes in the outer node list
                bool continueReading = subReader.Read();
                while (continueReading)
                {
                    if (subReader.NodeType == XamlNodeType.StartObject
                        && subReader.Type == XamlLanguage.Property)
                    {
                        // we found an x:Property. Store it in an ActivityPropertyHolder
                        ActivityPropertyHolder newProperty = new ActivityPropertyHolder(parent, subReader.ReadSubtree());
                        this.PropertyHolders.Add(newProperty.Name, newProperty);

                        // and stash away a proxy node to map later
                        this.outerNodes.Writer.WriteValue(newProperty, readerLineInfo);

                        // ActivityPropertyHolder consumed the subtree, so we don't need to pump a Read() in this path
                    }
                    else
                    {
                        // it's not an x:Property. Store it in our extra node list
                        this.outerNodes.Writer.WriteNode(subReader, readerLineInfo);
                        continueReading = subReader.Read();
                    }
                }

                // close the GetObject added above. Note that we are doing EndObject/EndMember after the last node (EndMember) 
                // rather than inserting EndMember/EndObject before the last EndMember since all EndMembers are interchangable from a state perspective
                this.outerNodes.Writer.WriteEndObject(readerLineInfo);
                this.outerNodes.Writer.WriteEndMember(readerLineInfo);
                subReader.Close();

                this.alreadyBufferedDefinitions = true;
                FlushValueHolders();
            }

            void FlushValueHolders()
            {
                // We've seen all the property definitions we're going to see. Write out any values already accumulated.

                // If we have picked up any values already before definitions, process them immediately 
                // (and throw as usual if corresponding definition doesn't exist)
                if (this.valueHolders != null)
                {
                    foreach (KeyValuePair<string, ValueHolder> propertyNameAndValue in this.valueHolders)
                    {
                        ProcessDefaultValue(propertyNameAndValue.Key, propertyNameAndValue.Value.PropertyValue, propertyNameAndValue.Value.ValueReader, propertyNameAndValue.Value.ValueReader as IXamlLineInfo);
                    }
                    this.valueHolders = null; // So we don't flush it again at close
                }
            }

            Dictionary<string, ActivityPropertyHolder> PropertyHolders
            {
                get
                {
                    if (this.propertyHolders == null)
                    {
                        this.propertyHolders = new Dictionary<string, ActivityPropertyHolder>();
                    }

                    return this.propertyHolders;
                }
            }

            public void BufferDefaultValue(string propertyName, XamlMember propertyValue, XamlReader reader, IXamlLineInfo lineInfo)
            {
                if (this.alreadyBufferedDefinitions)
                {
                    ProcessDefaultValue(propertyName, propertyValue, reader.ReadSubtree(), lineInfo);
                }
                else
                {
                    if (this.valueHolders == null)
                    {
                        this.valueHolders = new Dictionary<string, ValueHolder>();
                    }
                    ValueHolder savedValue = new ValueHolder(this.parent.SchemaContext, propertyValue, reader, lineInfo);
                    valueHolders[propertyName] = savedValue;
                }
            }

            public void ProcessDefaultValue(string propertyName, XamlMember propertyValue, XamlReader reader, IXamlLineInfo lineInfo)
            {
                ActivityPropertyHolder propertyHolder;
                if (!this.PropertyHolders.TryGetValue(propertyName, out propertyHolder))
                {
                    throw FxTrace.Exception.AsError(CreateXamlException(SR.InvalidProperty(propertyName), lineInfo));
                }

                propertyHolder.ProcessDefaultValue(propertyValue, reader, lineInfo);
            }

            public void FlushTo(XamlNodeQueue targetNodeQueue, DynamicActivityXamlReader parent)
            {
                FlushValueHolders();

                XamlReader sourceReader = this.outerNodes.Reader;
                IXamlLineInfo sourceReaderLineInfo = parent.hasLineInfo ? sourceReader as IXamlLineInfo : null;
                while (sourceReader.Read())
                {
                    if (sourceReader.NodeType == XamlNodeType.Value)
                    {
                        ActivityPropertyHolder propertyHolder = sourceReader.Value as ActivityPropertyHolder;
                        if (propertyHolder != null)
                        {
                            // replace ActivityPropertyHolder with its constituent nodes
                            propertyHolder.CopyTo(targetNodeQueue, sourceReaderLineInfo);
                            continue;
                        }
                    }

                    targetNodeQueue.Writer.WriteNode(sourceReader, sourceReaderLineInfo);
                }
            }

            // Buffer property values until we can match them with definitions
            class ValueHolder
            {
                XamlNodeQueue nodes;

                public ValueHolder(XamlSchemaContext schemaContext, XamlMember propertyValue, XamlReader reader, IXamlLineInfo lineInfo)
                {
                    this.nodes = new XamlNodeQueue(schemaContext);
                    this.PropertyValue = propertyValue;
                    XamlWriterExtensions.Transform(reader.ReadSubtree(), this.nodes.Writer, lineInfo, true);
                }

                public XamlMember PropertyValue { get; private set; }

                public XamlReader ValueReader
                {
                    get
                    {
                        return this.nodes.Reader;
                    }
                }
            }

            class ActivityPropertyHolder
            {
                // the nodes that we'll pump at the end
                XamlNodeQueue nodes;
                DynamicActivityXamlReader parent;

                public ActivityPropertyHolder(DynamicActivityXamlReader parent, XamlReader reader)
                {
                    this.parent = parent;
                    this.nodes = new XamlNodeQueue(parent.SchemaContext);
                    IXamlLineInfo readerLineInfo = parent.innerReaderLineInfo;

                    // parse the subtree, and extract out the Name and Type for now.
                    // keep the node-list open for now, just in case a default value appears 
                    // later in the document

                    // Rewrite "<x:Property>" to "<DynamicActivityProperty>"
                    reader.Read();
                    this.nodes.Writer.WriteStartObject(parent.activityPropertyXamlType, readerLineInfo);
                    int depth = 1;
                    int nameDepth = 0;
                    int typeDepth = 0;
                    bool continueReading = reader.Read();
                    while (continueReading)
                    {
                        switch (reader.NodeType)
                        {
                            case XamlNodeType.StartMember:
                                // map <x:Property> membes to the appropriate <DynamicActivity.Property> members
                                if (reader.Member.DeclaringType == XamlLanguage.Property)
                                {
                                    XamlMember mappedMember = reader.Member;

                                    if (mappedMember == xPropertyName)
                                    {
                                        mappedMember = parent.activityPropertyName;
                                        if (nameDepth == 0)
                                        {
                                            nameDepth = 1;
                                        }
                                    }
                                    else if (mappedMember == xPropertyType)
                                    {
                                        mappedMember = parent.activityPropertyType;
                                        if (typeDepth == 0)
                                        {
                                            typeDepth = 1;
                                        }
                                    }
                                    else if (mappedMember == xPropertyAttributes)
                                    {
                                        mappedMember = parent.activityPropertyAttributes;
                                    }
                                    else
                                    {
                                        throw FxTrace.Exception.AsError(CreateXamlException(SR.PropertyMemberNotSupportedByActivityXamlServices(mappedMember.Name), readerLineInfo));
                                    }
                                    this.nodes.Writer.WriteStartMember(mappedMember, readerLineInfo);
                                    continueReading = reader.Read();
                                    continue;
                                }
                                break;

                            case XamlNodeType.Value:
                                if (nameDepth == 1)
                                {
                                    // We only support property name as an attribute (nameDepth == 1)
                                    this.Name = reader.Value as string;
                                }
                                else if (typeDepth == 1)
                                {
                                    // We only support property type as an attribute (typeDepth == 1)
                                    XamlTypeName xamlTypeName = XamlTypeName.Parse(reader.Value as string, parent.namespaceTable);
                                    XamlType xamlType = parent.SchemaContext.GetXamlType(xamlTypeName);
                                    if (xamlType == null)
                                    {
                                        throw FxTrace.Exception.AsError(CreateXamlException(SR.InvalidPropertyType(reader.Value as string, this.Name), readerLineInfo));
                                    }
                                    this.Type = xamlType;
                                }
                                break;

                            case XamlNodeType.StartObject:
                            case XamlNodeType.GetObject:
                                depth++;
                                IncrementIfPositive(ref nameDepth);
                                IncrementIfPositive(ref typeDepth);
                                if (typeDepth > 0 && reader.Type == parent.xamlTypeXamlType)
                                {
                                    this.nodes.Writer.WriteStartObject(parent.typeXamlType, readerLineInfo);
                                    continueReading = reader.Read();
                                    continue;
                                }
                                break;

                            case XamlNodeType.EndObject:
                                depth--;
                                if (depth == 0)
                                {
                                    continueReading = reader.Read();
                                    continue; // skip this node, we'll close it by hand in CopyTo()
                                }
                                DecrementIfPositive(ref nameDepth);
                                DecrementIfPositive(ref typeDepth);
                                break;

                            case XamlNodeType.EndMember:
                                DecrementIfPositive(ref nameDepth);
                                DecrementIfPositive(ref typeDepth);
                                break;
                        }

                        // if we didn't continue (from a mapped case), just copy over
                        this.nodes.Writer.WriteNode(reader, readerLineInfo);
                        continueReading = reader.Read();
                    }

                    reader.Close();
                }

                public string Name
                {
                    get;
                    private set;
                }

                public XamlType Type
                {
                    get;
                    private set;
                }

                // called when we've reached the end of the activity and need
                // to extract out the resulting data into our activity-wide node list
                public void CopyTo(XamlNodeQueue targetNodeQueue, IXamlLineInfo readerInfo)
                {
                    // first copy any buffered nodes
                    XamlServices.Transform(this.nodes.Reader, targetNodeQueue.Writer, false);

                    // then write the end node for this property
                    targetNodeQueue.Writer.WriteEndObject(readerInfo);
                }

                public void ProcessDefaultValue(XamlMember propertyValue, XamlReader subReader, IXamlLineInfo lineInfo)
                {
                    bool addedStartObject = false;

                    // 1) swap out the start member with <ActivityProperty.Value>
                    subReader.Read();
                    if (!subReader.Member.IsNameValid)
                    {
                        throw FxTrace.Exception.AsError(CreateXamlException(SR.InvalidXamlMember(subReader.Member.Name), lineInfo));
                    }

                    this.nodes.Writer.WriteStartMember(propertyValue, lineInfo);

                    // temporary hack: read past GetObject/StartMember nodes that are added by 
                    // the XAML stack. This has been fixed in the WPF branch, but we haven't FI'ed that yet
                    XamlReader valueReader;
                    subReader.Read();
                    if (subReader.NodeType == XamlNodeType.GetObject)
                    {
                        subReader.Read();
                        subReader.Read();
                        valueReader = subReader.ReadSubtree();
                        valueReader.Read();
                    }
                    else
                    {
                        valueReader = subReader;
                    }

                    // Add SO tag if necessary UNLESS there's no value to wrap (which means we're already at EO)
                    if (valueReader.NodeType != XamlNodeType.EndMember && valueReader.NodeType != XamlNodeType.StartObject)
                    {
                        addedStartObject = true;
                        // Add <TypeOfProperty> nodes so that type converters work correctly
                        this.nodes.Writer.WriteStartObject(this.Type, lineInfo);
                        this.nodes.Writer.WriteStartMember(XamlLanguage.Initialization, lineInfo);
                    }

                    // 3) copy over the value 
                    while (!valueReader.IsEof)
                    {
                        this.nodes.Writer.WriteNode(valueReader, lineInfo);
                        valueReader.Read();
                    }

                    valueReader.Close();

                    // 4) close up the extra nodes 
                    if (!object.ReferenceEquals(valueReader, subReader))
                    {
                        subReader.Read();
                        while (subReader.Read())
                        {
                            this.nodes.Writer.WriteNode(subReader, lineInfo);
                        }

                    }

                    if (addedStartObject)
                    {
                        this.nodes.Writer.WriteEndObject(lineInfo);
                        this.nodes.Writer.WriteEndMember(lineInfo);
                    }
                    subReader.Close();
                }
            }
        }
    }
}
