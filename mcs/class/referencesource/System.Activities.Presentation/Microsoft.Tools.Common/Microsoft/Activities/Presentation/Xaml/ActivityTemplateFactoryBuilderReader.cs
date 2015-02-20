// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace Microsoft.Activities.Presentation.Xaml
{
    using System;
    using System.Activities.Presentation.Toolbox;
    using System.Xaml;

    // ActivityTemplateFactoryBuilderReader is a XamlReader that support <ActivityTemplateFactory x:Class ... 
    //
    // Think of this class (and any other XamlReader) as a XAML node stream editor
    // XAML node are *not* objects, they are represented as this. For example, when the reader encounter a StartObject node, its NodeType will become StartObject, and its Type will become the type of the starting object.
    // The writer will then edit the stream and send the nodes to the underlying stream (by calling the methods on the underlying writer)
    // 
    // The editing algorithm goes as follow:
    // 
    // Initially, the first node is read from the underlying reader, if the first node is <ActivityTemplateFactory, then we start buffering nodes, otherwise we simply switch to the Bypass state
    // We transform and buffer the transformed nodes until we reach the StartMember of Implementation Node, then we yield the control and switch to the ReadingFromBuffer state.
    //
    // All the external calls are then delegated to the reader provided by the buffer.
    //
    // Eventually, the buffer will used up, and we will switch to the Bypass state.
    internal sealed class ActivityTemplateFactoryBuilderReader : XamlReader, IXamlLineInfo
    {
        private XamlSchemaContext schemaContext;
        private XamlReader underlyingReader;
        private XamlNodeQueue queuedNodes;
        private XamlType activityTemplateFactoryBuilderType;
        private XamlMember activityTemplateFactoryBuilderImplementationMember;
        private XamlMember activityTemplateFactoryBuilderNameMember;
        private XamlMember activityTemplateFactoryBuilderTargetTypeMember;

        private bool hasLineInfo;
        private ActivityTemplateFactoryBuilderReaderStates currentState = ActivityTemplateFactoryBuilderReaderStates.InitialState;

        public ActivityTemplateFactoryBuilderReader(XamlReader underlyingReader, XamlSchemaContext schemaContext)
        {
            this.underlyingReader = underlyingReader;
            this.schemaContext = schemaContext;
            this.hasLineInfo = this.underlyingReader is IXamlLineInfo;
        }

        private enum ActivityTemplateFactoryBuilderReaderStates
        {
            InitialState,
            ReadingFromBufferState,
            BypassState,
        }

        public override bool IsEof
        {
            get
            {
                if (this.currentState == ActivityTemplateFactoryBuilderReaderStates.ReadingFromBufferState)
                {
                    return false;
                }
                else
                {
                    return this.underlyingReader.IsEof;
                }
            }
        }

        public override XamlMember Member
        {
            get { return this.CurrentReader.Member; }
        }

        public override NamespaceDeclaration Namespace
        {
            get { return this.CurrentReader.Namespace; }
        }

        public override XamlNodeType NodeType
        {
            get { return this.CurrentReader.NodeType; }
        }

        public override XamlSchemaContext SchemaContext
        {
            get { return this.schemaContext; }
        }

        public override XamlType Type
        {
            get { return this.CurrentReader.Type; }
        }

        public override object Value
        {
            get { return this.CurrentReader.Value; }
        }

        public bool HasLineInfo
        {
            get { return this.hasLineInfo; }
        }

        public int LineNumber
        {
            get
            {
                if (this.HasLineInfo)
                {
                    return this.CurrentLineInfo.LineNumber;
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
                if (this.HasLineInfo)
                {
                    return this.CurrentLineInfo.LinePosition;
                }
                else
                {
                    return 0;
                }
            }
        }

        private XamlReader CurrentReader
        {
            get
            {
                switch (this.currentState)
                {
                    case ActivityTemplateFactoryBuilderReaderStates.InitialState:
                    case ActivityTemplateFactoryBuilderReaderStates.BypassState:
                        return this.underlyingReader;

                    default:
                        SharedFx.Assert(this.currentState == ActivityTemplateFactoryBuilderReaderStates.ReadingFromBufferState, "This is the only remaining ActivityTemplateFactoryBuilderReaderStates.");
                        return this.queuedNodes.Reader;
                }
            }
        }

        private IXamlLineInfo CurrentLineInfo
        {
            get { return (IXamlLineInfo)this.CurrentReader; }
        }

        private XamlType ActivityTemplateFactoryBuilderType
        {
            get
            {
                if (this.activityTemplateFactoryBuilderType == null)
                {
                    this.activityTemplateFactoryBuilderType = new XamlType(typeof(ActivityTemplateFactoryBuilder), this.schemaContext);
                }

                return this.activityTemplateFactoryBuilderType;
            }
        }

        private XamlMember ActivityTemplateFactoryBuilderImplementationMember
        {
            get
            {
                if (this.activityTemplateFactoryBuilderImplementationMember == null)
                {
                    this.activityTemplateFactoryBuilderImplementationMember = ActivityTemplateFactoryBuilderXamlMembers.ActivityTemplateFactoryBuilderImplementationMember(this.schemaContext);
                }

                return this.activityTemplateFactoryBuilderImplementationMember;
            }
        }

        private XamlMember ActivityTemplateFactoryBuilderNameMember
        {
            get
            {
                if (this.activityTemplateFactoryBuilderNameMember == null)
                {
                    this.activityTemplateFactoryBuilderNameMember = ActivityTemplateFactoryBuilderXamlMembers.ActivityTemplateFactoryBuilderNameMember(this.schemaContext);
                }

                return this.activityTemplateFactoryBuilderNameMember;
            }
        }

        private XamlMember ActivityTemplateFactoryBuilderTargetTypeMember
        {
            get
            {
                if (this.activityTemplateFactoryBuilderTargetTypeMember == null)
                {
                    this.activityTemplateFactoryBuilderTargetTypeMember = ActivityTemplateFactoryBuilderXamlMembers.ActivityTemplateFactoryBuilderTargetTypeMember(this.schemaContext);
                }

                return this.activityTemplateFactoryBuilderTargetTypeMember;
            }
        }

        public override bool Read()
        {
            switch (this.currentState)
            {
                case ActivityTemplateFactoryBuilderReaderStates.InitialState:
                    bool hasMoreNodes = this.underlyingReader.Read();
                    if (this.underlyingReader.NodeType == XamlNodeType.StartObject && IsActivityTemplateFactoryType(this.underlyingReader.Type))
                    {
                        Type underlyingType = this.underlyingReader.Type.UnderlyingType;
                        Type targetType = underlyingType.IsGenericType ? underlyingType.GetGenericArguments()[0] : null;

                        this.currentState = ActivityTemplateFactoryBuilderReaderStates.ReadingFromBufferState;
                        this.queuedNodes = new XamlNodeQueue(this.schemaContext);
                        this.queuedNodes.Writer.WriteStartObject(this.ActivityTemplateFactoryBuilderType, (IXamlLineInfo)this.underlyingReader);

                        string className;

                        while (this.underlyingReader.Read())
                        {
                            if (this.underlyingReader.NodeType == XamlNodeType.StartMember && this.underlyingReader.Member == XamlLanguage.Class)
                            {
                                this.underlyingReader.Read();
                                className = (string)this.underlyingReader.Value;
                                this.underlyingReader.Read();
                                this.queuedNodes.Writer.WriteStartMember(this.ActivityTemplateFactoryBuilderNameMember, (IXamlLineInfo)this.underlyingReader);
                                this.queuedNodes.Writer.WriteValue(className, (IXamlLineInfo)this.underlyingReader);
                                this.queuedNodes.Writer.WriteEndMember((IXamlLineInfo)this.underlyingReader);
                                if (targetType != null)
                                {
                                    this.queuedNodes.Writer.WriteStartMember(this.ActivityTemplateFactoryBuilderTargetTypeMember, (IXamlLineInfo)this.underlyingReader);
                                    object targetTypeString = targetType;
                                    this.queuedNodes.Writer.WriteValue(targetTypeString);
                                    this.queuedNodes.Writer.WriteEndMember();
                                }
                            }
                            else if (this.underlyingReader.NodeType == XamlNodeType.StartMember && this.IsActivityTemplateFactoryImplementationMember(this.underlyingReader.Member))
                            {
                                this.queuedNodes.Writer.WriteStartMember(this.ActivityTemplateFactoryBuilderImplementationMember, (IXamlLineInfo)this.underlyingReader);
                                return true;
                            }
                        }
                    }

                    return hasMoreNodes;

                case ActivityTemplateFactoryBuilderReaderStates.ReadingFromBufferState:
                    if (this.queuedNodes.Reader.Read())
                    {
                        return true;
                    }
                    else
                    {
                        this.currentState = ActivityTemplateFactoryBuilderReaderStates.BypassState;
                        this.queuedNodes = null;
                        return this.underlyingReader.Read();
                    }

                default:
                    SharedFx.Assert(this.currentState == ActivityTemplateFactoryBuilderReaderStates.BypassState, "This is the only remaining ActivityTemplateFactoryBuilderReaderStates.");
                    return this.underlyingReader.Read();
            }
        }

        private static bool IsActivityTemplateFactoryType(XamlType xamlType)
        {
            if (xamlType.UnderlyingType == null)
            {
                return false;
            }

            return xamlType.UnderlyingType == typeof(ActivityTemplateFactory) || (xamlType.UnderlyingType.IsGenericType && xamlType.UnderlyingType.GetGenericTypeDefinition() == typeof(ActivityTemplateFactory<>));
        }

        private bool IsActivityTemplateFactoryImplementationMember(XamlMember xamlMember)
        {
            return IsActivityTemplateFactoryType(xamlMember.DeclaringType) && xamlMember == ActivityTemplateFactoryBuilderXamlMembers.ActivityTemplateFactoryImplementationMemberForReader(xamlMember.DeclaringType.UnderlyingType, this.schemaContext);
        }
    }
}
