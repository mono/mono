// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace Microsoft.Activities.Presentation.Xaml
{
    using System;
    using System.Activities.Presentation.Toolbox;
    using System.Xaml;

    // ActivityTemplateFactoryBuilderWriter is a XamlWriter that support <ActivityTemplateFactory x:Class ... 
    //
    // Think of this class (and any other XamlWriter) as a XAML node stream editor
    // XAML node are *not* objects, they are represented as method calls. For example, when WriteStartObject is called, a StartObject node is send to this writer.
    // The writer will then edit the stream and send the nodes to the underlying stream (by calling the methods on the underlying writer)
    // 
    // The editing algorithm goes as follow:
    //
    // The system starts as the InitialState. There are five states in total: (InitialState, BufferingState, BufferingNameState, BufferingTargetTypeState, BypassState)
    // If the very first StartObject node is ActivityTemplateFactory, then start buffering by going to the buffering state, otherwise simply go to the ByPassState.
    // 
    // In the buffering state, the nodes are buffered in a XamlNodeQueue, until we see the Implementation Node.
    // When we reach the Implementation node, we will flush all the nodes transformed to the underlyingWriter, we will also switch to the ByPass state.
    // 
    // During the buffering, it is possible that we encounter the Name/TargetType node - the name node cannot enter the buffer because editing is required, we will use a separate state to track that.
    internal sealed class ActivityTemplateFactoryBuilderWriter : XamlWriter
    {
        private XamlSchemaContext schemaContext;
        private XamlWriter underlyingWriter;
        private XamlType activityTemplateFactoryType;
        private XamlMember activityTemplateFactoryImplementationMember;
        private XamlMember activityTemplateFactoryBuilderImplementationMember;
        private XamlMember activityTemplateFactoryBuilderNameMember;
        private XamlMember activityTemplateFactoryBuilderTargetTypeMember;

        // Buffering of nodes before starting the Implementation node 
        private ActivityTemplateFactoryBuilderWriterStates currentState = ActivityTemplateFactoryBuilderWriterStates.InitialState;
        private XamlNodeQueue queuedNodes;
        private string className;
        private string targetType;
        private bool xamlLanguageNamespaceWritten = false;

        public ActivityTemplateFactoryBuilderWriter(XamlWriter underlyingWriter, XamlSchemaContext schemaContext)
        {
            this.schemaContext = schemaContext;
            this.underlyingWriter = underlyingWriter;
        }

        private enum ActivityTemplateFactoryBuilderWriterStates
        {
            InitialState,
            BufferingState,
            BufferingNameState,
            BufferingTargetTypeState,
            BypassState,
        }

        public override XamlSchemaContext SchemaContext
        {
            get { return this.schemaContext; }
        }

        private XamlType ActivityTemplateFactoryType
        {
            get
            {
                if (this.activityTemplateFactoryType == null)
                {
                    this.activityTemplateFactoryType = new XamlType(typeof(ActivityTemplateFactory), this.schemaContext);
                }

                return this.activityTemplateFactoryType;
            }
        }

        private XamlMember ActivityTemplateFactoryImplementationMember
        {
            get
            {
                if (this.activityTemplateFactoryImplementationMember == null)
                {
                    this.activityTemplateFactoryImplementationMember = ActivityTemplateFactoryBuilderXamlMembers.ActivityTemplateFactoryImplementationMemberForWriter(this.schemaContext);
                }

                return this.activityTemplateFactoryImplementationMember;
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

        public override void WriteNamespace(NamespaceDeclaration namespaceDeclaration)
        {
            if (namespaceDeclaration.Prefix == "x")
            {
                this.xamlLanguageNamespaceWritten = true;
            }

            this.underlyingWriter.WriteNamespace(namespaceDeclaration);
        }

        public override void WriteStartObject(XamlType type)
        {
            switch (this.currentState)
            {
                case ActivityTemplateFactoryBuilderWriterStates.InitialState:
                    if (type.Equals(new XamlType(typeof(ActivityTemplateFactoryBuilder), this.schemaContext)))
                    {
                        this.queuedNodes = new XamlNodeQueue(this.schemaContext);
                        this.currentState = ActivityTemplateFactoryBuilderWriterStates.BufferingState;
                    }
                    else
                    {
                        this.currentState = ActivityTemplateFactoryBuilderWriterStates.BypassState;
                        this.underlyingWriter.WriteStartObject(type);
                    }

                    break;
                case ActivityTemplateFactoryBuilderWriterStates.BypassState:
                    this.underlyingWriter.WriteStartObject(type);
                    break;
                default:
                    SharedFx.Assert(
                        this.currentState == ActivityTemplateFactoryBuilderWriterStates.BufferingState
                        || this.currentState == ActivityTemplateFactoryBuilderWriterStates.BufferingNameState
                        || this.currentState == ActivityTemplateFactoryBuilderWriterStates.BufferingTargetTypeState,
                        "These are the only possible ActivityTemplateFactoryBuilderWriterStates.");
                    SharedFx.Assert("It is impossible to start any object during the buffering state.");
                    break;
            }
        }

        public override void WriteEndObject()
        {
            switch (this.currentState)
            {
                case ActivityTemplateFactoryBuilderWriterStates.InitialState:
                    SharedFx.Assert("It is impossible to end an object during InitialState");
                    break;
                case ActivityTemplateFactoryBuilderWriterStates.BufferingState:
                    this.queuedNodes.Writer.WriteEndObject();
                    break;
                case ActivityTemplateFactoryBuilderWriterStates.BypassState:
                    this.underlyingWriter.WriteEndObject();
                    break;
                default:
                    SharedFx.Assert(
                        this.currentState == ActivityTemplateFactoryBuilderWriterStates.BufferingNameState 
                        || this.currentState == ActivityTemplateFactoryBuilderWriterStates.BufferingTargetTypeState,
                        "These are the only possible ActivityTemplateFactoryBuilderWriterStates.");
                    SharedFx.Assert("It is impossible to end an object when we are buffering the name / targetType.");
                    break;
            }
        }

        public override void WriteGetObject()
        {
            switch (this.currentState)
            {
                case ActivityTemplateFactoryBuilderWriterStates.InitialState:
                    SharedFx.Assert("It is impossible to end an object during InitialState");
                    break;
                case ActivityTemplateFactoryBuilderWriterStates.BufferingState:
                    this.queuedNodes.Writer.WriteGetObject();
                    break;
                case ActivityTemplateFactoryBuilderWriterStates.BypassState:
                    this.underlyingWriter.WriteGetObject();
                    break;
                default:
                    SharedFx.Assert(
                        this.currentState == ActivityTemplateFactoryBuilderWriterStates.BufferingNameState 
                        || this.currentState == ActivityTemplateFactoryBuilderWriterStates.BufferingTargetTypeState, 
                        "These are the only possible ActivityTemplateFactoryBuilderWriterStates.");
                    SharedFx.Assert("It is impossible to get an object when we are buffering the name / targetType.");
                    break;
            }
        }

        public override void WriteStartMember(XamlMember xamlMember)
        {
            switch (this.currentState)
            {
                case ActivityTemplateFactoryBuilderWriterStates.InitialState:
                    SharedFx.Assert("It is impossible to start a member during InitialState");
                    break;
                case ActivityTemplateFactoryBuilderWriterStates.BufferingState:
                    if (xamlMember == this.ActivityTemplateFactoryBuilderImplementationMember)
                    {
                        xamlMember = this.ActivityTemplateFactoryImplementationMember;

                        if (!this.xamlLanguageNamespaceWritten)
                        {
                            // Required namespace for XAML x:Class 
                            this.underlyingWriter.WriteNamespace(new NamespaceDeclaration("http://schemas.microsoft.com/winfx/2006/xaml", "x"));
                        }

                        this.underlyingWriter.WriteStartObject(this.ActivityTemplateFactoryType);
                        this.underlyingWriter.WriteStartMember(XamlLanguage.Class);
                        this.underlyingWriter.WriteValue(this.className);
                        this.underlyingWriter.WriteEndMember();
                        this.underlyingWriter.WriteStartMember(XamlLanguage.TypeArguments);
                        this.underlyingWriter.WriteValue(this.targetType);
                        this.underlyingWriter.WriteEndMember();
                        this.Transform(this.queuedNodes.Reader, this.underlyingWriter);
                        this.underlyingWriter.WriteStartMember(xamlMember);
                        this.currentState = ActivityTemplateFactoryBuilderWriterStates.BypassState;
                    }

                    if (xamlMember == this.ActivityTemplateFactoryBuilderNameMember)
                    {
                        this.currentState = ActivityTemplateFactoryBuilderWriterStates.BufferingNameState;
                    }
                    else if (xamlMember == this.ActivityTemplateFactoryBuilderTargetTypeMember)
                    {
                        this.currentState = ActivityTemplateFactoryBuilderWriterStates.BufferingTargetTypeState;
                    }
                    else
                    {
                        this.queuedNodes.Writer.WriteStartMember(xamlMember);
                    }

                    break;
                case ActivityTemplateFactoryBuilderWriterStates.BypassState:
                    this.underlyingWriter.WriteStartMember(xamlMember);
                    break;
                default:
                    SharedFx.Assert(
                        this.currentState == ActivityTemplateFactoryBuilderWriterStates.BufferingNameState 
                        || this.currentState == ActivityTemplateFactoryBuilderWriterStates.BufferingTargetTypeState, 
                        "These are the only possible ActivityTemplateFactoryBuilderWriterStates.");
                    SharedFx.Assert("It is impossible to get an object when we are buffering the name / targetType.");
                    break;
            }
        }

        public override void WriteEndMember()
        {
            switch (this.currentState)
            {
                case ActivityTemplateFactoryBuilderWriterStates.InitialState:
                    SharedFx.Assert("It is impossible to end a member during InitialState");
                    break;
                case ActivityTemplateFactoryBuilderWriterStates.BufferingState:
                    this.queuedNodes.Writer.WriteEndMember();
                    break;
                case ActivityTemplateFactoryBuilderWriterStates.BypassState:
                    this.underlyingWriter.WriteEndMember();
                    break;
                default:
                    SharedFx.Assert(
                        this.currentState == ActivityTemplateFactoryBuilderWriterStates.BufferingNameState 
                        || this.currentState == ActivityTemplateFactoryBuilderWriterStates.BufferingTargetTypeState, 
                        "These are the only possible ActivityTemplateFactoryBuilderWriterStates.");

                    // Intentionally skipped the end member of Name / TargetType node
                    this.currentState = ActivityTemplateFactoryBuilderWriterStates.BufferingState;
                    break;
            }
        }

        public override void WriteValue(object value)
        {
            switch (this.currentState)
            {
                case ActivityTemplateFactoryBuilderWriterStates.InitialState:
                    SharedFx.Assert("It is impossible to write a value during InitialState");
                    break;
                case ActivityTemplateFactoryBuilderWriterStates.BufferingState:
                    this.queuedNodes.Writer.WriteValue(value);
                    break;
                case ActivityTemplateFactoryBuilderWriterStates.BufferingNameState:
                    this.className = (string)value;
                    break;
                case ActivityTemplateFactoryBuilderWriterStates.BufferingTargetTypeState:
                    this.targetType = (string)value;
                    break;
                default:
                    SharedFx.Assert(
                        this.currentState == ActivityTemplateFactoryBuilderWriterStates.BypassState, 
                        "This is the only possible ActivityTemplateFactoryBuilderWriterStates");
                    this.underlyingWriter.WriteValue(value);
                    break;
            }
        }

        private void Transform(XamlReader reader, XamlWriter myWriter)
        {
            while (!reader.IsEof)
            {
                reader.Read();
                myWriter.WriteNode(reader);
            }
        }
    }
}
