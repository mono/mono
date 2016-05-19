//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.XamlIntegration
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime;
    using System.Windows.Markup;
    using System.Xaml;
    using System.Xaml.Schema;
    using System.Xml;

    // This class rewrites an <ActivityBuilder to <Activity x:Class
    // ActivityBuilder.Properties is rewritten to x:Members
    // ActivityBuilder.Name is rewritten as x:Class
    // ActivityBuilder.Implementation is rewritten as Activity.Implementation
    // 
    // Because of our [DependsOn] annotations, Name is followed by Attributes, Properties,
    // Constraints and, lastly, Implementation. The first few relationships are assumed
    // and enforced through our state machine here to avoid buffering the whole node stream
    // in common cases (such as no attributes specified).
    class ActivityBuilderXamlWriter : XamlWriter
    {
        readonly XamlWriter innerWriter;

        // These may be a closed generic type in the Activity<T> case (or null if not an ActivityBuilder),
        // so we need to compute this value dynamically
        XamlType activityBuilderXamlType;
        XamlType activityXamlType;

        XamlType activityPropertyXamlType;
        XamlType xamlTypeXamlType;
        XamlType typeXamlType;
        XamlType activityPropertyReferenceXamlType;

        XamlMember activityPropertyType;
        XamlMember activityPropertyName;
        XamlMember activityPropertyValue;
        XamlMember activityBuilderName;
        XamlMember activityBuilderAttributes;
        XamlMember activityBuilderProperties;
        XamlMember activityBuilderPropertyReference;
        XamlMember activityBuilderPropertyReferences;

        bool notRewriting;
        int currentDepth;

        // we need to accrue namespace so that we can resolve DynamicActivityProperty.Type
        // and correctly strip superfluous wrapper nodes around default values
        NamespaceTable namespaceTable;
        BuilderXamlNode currentState;
        Stack<BuilderXamlNode> pendingStates;

        public ActivityBuilderXamlWriter(XamlWriter innerWriter)
            : base()
        {
            this.innerWriter = innerWriter;
            this.currentState = new RootNode(this);
            this.namespaceTable = new NamespaceTable();
        }

        public override XamlSchemaContext SchemaContext
        {
            get
            {
                return this.innerWriter.SchemaContext;
            }
        }

        void SetActivityType(XamlType activityXamlType, XamlType activityBuilderXamlType)
        {
            if (activityXamlType == null)
            {
                this.notRewriting = true;
            }
            else
            {
                this.activityXamlType = activityXamlType;
                this.activityBuilderXamlType = activityBuilderXamlType;
                this.xamlTypeXamlType = this.SchemaContext.GetXamlType(typeof(XamlType));
                this.typeXamlType = this.SchemaContext.GetXamlType(typeof(Type));

                this.activityPropertyXamlType = this.SchemaContext.GetXamlType(typeof(DynamicActivityProperty));
                this.activityPropertyType = this.activityPropertyXamlType.GetMember("Type");
                this.activityPropertyName = this.activityPropertyXamlType.GetMember("Name");
                this.activityPropertyValue = this.activityPropertyXamlType.GetMember("Value");

                this.activityBuilderName = this.activityBuilderXamlType.GetMember("Name");
                this.activityBuilderAttributes = this.activityBuilderXamlType.GetMember("Attributes");
                this.activityBuilderProperties = this.activityBuilderXamlType.GetMember("Properties");
                this.activityBuilderPropertyReference = this.SchemaContext.GetXamlType(typeof(ActivityBuilder)).GetAttachableMember("PropertyReference");
                this.activityBuilderPropertyReferences = this.SchemaContext.GetXamlType(typeof(ActivityBuilder)).GetAttachableMember("PropertyReferences");
                this.activityPropertyReferenceXamlType = this.SchemaContext.GetXamlType(typeof(ActivityPropertyReference));
            }
        }

        public override void WriteNamespace(NamespaceDeclaration namespaceDeclaration)
        {
            if (this.notRewriting)
            {
                this.innerWriter.WriteNamespace(namespaceDeclaration);
                return;
            }

            if (this.namespaceTable != null)
            {
                this.namespaceTable.AddNamespace(namespaceDeclaration);
            }
            this.currentState.WriteNamespace(namespaceDeclaration);
        }

        public override void WriteValue(object value)
        {
            if (this.notRewriting)
            {
                this.innerWriter.WriteValue(value);
                return;
            }

            this.currentState.WriteValue(value);
        }

        public override void WriteStartObject(XamlType xamlType)
        {
            if (this.notRewriting)
            {
                this.innerWriter.WriteStartObject(xamlType);
                return;
            }

            EnterDepth();
            this.currentState.WriteStartObject(xamlType);
        }

        public override void WriteGetObject()
        {
            if (this.notRewriting)
            {
                this.innerWriter.WriteGetObject();
                return;
            }

            EnterDepth();
            this.currentState.WriteGetObject();
        }

        public override void WriteEndObject()
        {
            if (this.notRewriting)
            {
                this.innerWriter.WriteEndObject();
                return;
            }

            this.currentState.WriteEndObject();
            ExitDepth();
        }

        public override void WriteStartMember(XamlMember xamlMember)
        {
            if (this.notRewriting)
            {
                this.innerWriter.WriteStartMember(xamlMember);
                return;
            }

            EnterDepth();
            this.currentState.WriteStartMember(xamlMember);
        }

        public override void WriteEndMember()
        {
            if (this.notRewriting)
            {
                this.innerWriter.WriteEndMember();
                return;
            }

            this.currentState.WriteEndMember();
            ExitDepth();
        }

        void PushState(BuilderXamlNode state)
        {
            if (this.pendingStates == null)
            {
                this.pendingStates = new Stack<BuilderXamlNode>();
            }
            this.pendingStates.Push(this.currentState);
            this.currentState = state;
        }

        void EnterDepth()
        {
            Fx.Assert(!this.notRewriting, "we only use depth calculation if we're rewriting");
            this.currentDepth++;
            if (this.namespaceTable != null)
            {
                this.namespaceTable.EnterScope();
            }
        }

        void ExitDepth()
        {
            Fx.Assert(!this.notRewriting, "we only use depth calculation if we're rewriting");
            if (this.currentState.Depth == this.currentDepth)
            {
                // complete the current state
                this.currentState.Complete();

                // and pop off the next state to look for
                if (this.pendingStates.Count > 0)
                {
                    this.currentState = this.pendingStates.Pop();
                }
            }
            this.currentDepth--;
            if (this.namespaceTable != null)
            {
                this.namespaceTable.ExitScope();
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                ((IDisposable)this.innerWriter).Dispose();
            }
        }

        abstract class BuilderXamlNode
        {
            protected BuilderXamlNode(ActivityBuilderXamlWriter writer)
            {
                this.Depth = writer.currentDepth;
                this.Writer = writer;
                this.CurrentWriter = writer.innerWriter;
            }

            public int Depth
            {
                get;
                private set;
            }

            // a lot of nodes just redirect output, this
            // allows them to avoid overriding everything just for that
            public XamlWriter CurrentWriter
            {
                get;
                protected set;
            }

            protected ActivityBuilderXamlWriter Writer
            {
                get;
                private set;
            }

            protected internal virtual void Complete()
            {
            }

            protected internal virtual void WriteNamespace(NamespaceDeclaration namespaceDeclaration)
            {
                CurrentWriter.WriteNamespace(namespaceDeclaration);
            }

            protected internal virtual void WriteStartObject(XamlType xamlType)
            {
                CurrentWriter.WriteStartObject(xamlType);
            }

            protected internal virtual void WriteGetObject()
            {
                CurrentWriter.WriteGetObject();
            }

            protected internal virtual void WriteEndObject()
            {
                CurrentWriter.WriteEndObject();
            }

            protected internal virtual void WriteStartMember(XamlMember xamlMember)
            {
                CurrentWriter.WriteStartMember(xamlMember);
            }

            protected internal virtual void WriteEndMember()
            {
                CurrentWriter.WriteEndMember();
            }

            protected internal virtual void WriteValue(object value)
            {
                CurrentWriter.WriteValue(value);
            }
        }

        // RootNode needs to buffer nodes until we finish processing Name + Properties
        // because we need to insert our namespace _before_ the first StartObject.
        // this is the starting value for ActivityBuilderXamlWriter.currentNode
        class RootNode : BuilderXamlNode
        {
            const string PreferredXamlNamespaceAlias = "x";
            const string PreferredClassAlias = "this";

            bool wroteXamlNamespace;
            HashSet<string> rootLevelPrefixes;

            XamlNodeQueue pendingNodes;

            public RootNode(ActivityBuilderXamlWriter writer)
                : base(writer)
            {
                this.pendingNodes = new XamlNodeQueue(writer.SchemaContext);
                base.CurrentWriter = this.pendingNodes.Writer;
            }

            protected internal override void WriteNamespace(NamespaceDeclaration namespaceDeclaration)
            {
                if (Writer.currentDepth == 0 && !this.wroteXamlNamespace)
                {
                    if (namespaceDeclaration.Namespace == XamlLanguage.Xaml2006Namespace)
                    {
                        this.wroteXamlNamespace = true;
                    }
                    else
                    {
                        if (this.rootLevelPrefixes == null)
                        {
                            this.rootLevelPrefixes = new HashSet<string>();
                        }
                        this.rootLevelPrefixes.Add(namespaceDeclaration.Prefix);
                    }
                }
                base.WriteNamespace(namespaceDeclaration);
            }

            protected internal override void WriteStartObject(XamlType xamlType)
            {
                if (Writer.currentDepth == 1)
                {
                    XamlType activityXamlType = null;

                    // root object: see if we're serializing an ActivityBuilder
                    if (xamlType.UnderlyingType == typeof(ActivityBuilder))
                    {
                        activityXamlType = Writer.SchemaContext.GetXamlType(typeof(Activity));
                    }
                    // or an ActivityBuilder<TResult>
                    else if (xamlType.IsGeneric && xamlType.UnderlyingType != null
                        && xamlType.UnderlyingType.GetGenericTypeDefinition() == typeof(ActivityBuilder<>))
                    {
                        Type activityType = xamlType.TypeArguments[0].UnderlyingType;
                        activityXamlType = Writer.SchemaContext.GetXamlType(typeof(Activity<>).MakeGenericType(activityType));
                    }

                    Writer.SetActivityType(activityXamlType, xamlType);

                    if (activityXamlType != null)
                    {
                        Writer.PushState(new BuilderClassNode(this, Writer));
                        return;
                    }
                    else
                    {
                        // we should be a pass through. Flush any buffered nodes and get out of the way
                        FlushPendingNodes(null);
                    }
                }
                base.WriteStartObject(xamlType);
            }

            public void FlushPendingNodes(string classNamespace)
            {
                base.CurrentWriter = this.Writer.innerWriter;
                if (!Writer.notRewriting)
                {
                    // make sure we have any required namespaces
                    if (!this.wroteXamlNamespace)
                    {
                        string xamlNamespaceAlias = GenerateNamespacePrefix(PreferredXamlNamespaceAlias);
                        this.WriteNamespace(new NamespaceDeclaration(XamlLanguage.Xaml2006Namespace, xamlNamespaceAlias));
                    }

                    // If there's an x:Class="Foo.Bar", add a namespace declaration for Foo in the local assembly so we can 
                    // say stuff like this:Bar.MyProperty later on. DON'T add the namespace declaration if somebody has already 
                    // declared the namespace in the nodestream though (duplicates are an error).
                    if (classNamespace != null)
                    {
                        bool sawClassNamespace = false;

                        XamlReader reader = this.pendingNodes.Reader;
                        XamlWriter writer = this.Writer.innerWriter;
                        while (reader.Read() && reader.NodeType == XamlNodeType.NamespaceDeclaration)
                        {
                            if (classNamespace.Equals(reader.Namespace.Namespace))
                            {
                                sawClassNamespace = true;
                            }
                            writer.WriteNode(reader);
                        }

                        if (!sawClassNamespace)
                        {
                            string classNamespaceAlias = GenerateNamespacePrefix(PreferredClassAlias);
                            writer.WriteNamespace(new NamespaceDeclaration(classNamespace, classNamespaceAlias));
                        }

                        // We may have consumed the first non-namespace node off the reader in order 
                        // to check it for being a NamespaceDeclaration. Make sure it still gets written.
                        if (!reader.IsEof)
                        {
                            writer.WriteNode(reader);
                        }
                    }

                    this.rootLevelPrefixes = null; // not needed anymore
                }

                XamlServices.Transform(this.pendingNodes.Reader, this.Writer.innerWriter, false);
                this.pendingNodes = null;
            }

            string GenerateNamespacePrefix(string desiredPrefix)
            {
                string aliasPostfix = string.Empty;
                // try postfixing 1-1000 first
                for (int i = 1; i <= 1000; i++)
                {
                    string alias = desiredPrefix + aliasPostfix;
                    if (!this.rootLevelPrefixes.Contains(alias))
                    {
                        return alias;
                    }
                    aliasPostfix = i.ToString(CultureInfo.InvariantCulture);
                }

                // fall back to GUID
                return desiredPrefix + Guid.NewGuid().ToString();
            }
        }

        // <ActivityBuilder>...</ActivityBuilder>
        class BuilderClassNode : BuilderXamlNode
        {
            RootNode rootNode;

            string xClassNamespace;
            XamlType xClassXamlType;
            XamlNodeQueue xClassNodes;
            XamlNodeQueue xClassAttributeNodes;
            XamlNodeQueue xPropertiesNodes;
            XamlNodeQueue otherNodes;
            List<KeyValuePair<string, XamlNodeQueue>> defaultValueNodes;

            public BuilderClassNode(RootNode rootNode, ActivityBuilderXamlWriter writer)
                : base(writer)
            {
                this.rootNode = rootNode;

                // by default, if we're not in a special sub-tree, ferret the nodes away on the side
                this.otherNodes = new XamlNodeQueue(writer.SchemaContext);
                base.CurrentWriter = this.otherNodes.Writer;
            }

            public void SetXClass(string builderName, XamlNodeQueue nameNodes)
            {
                this.xClassNodes = new XamlNodeQueue(Writer.SchemaContext);
                this.xClassNodes.Writer.WriteStartMember(XamlLanguage.Class);
                this.xClassNamespace = null;
                string xClassName = builderName;
                if (string.IsNullOrEmpty(xClassName))
                {
                    xClassName = string.Format(CultureInfo.CurrentCulture, "_{0}", Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 4));
                }

                if (nameNodes != null)
                {
                    XamlServices.Transform(nameNodes.Reader, this.xClassNodes.Writer, false);
                }
                else
                {
                    this.xClassNodes.Writer.WriteValue(xClassName);
                    this.xClassNodes.Writer.WriteEndMember();
                }

                int nameStartIndex = xClassName.LastIndexOf('.');
                if (nameStartIndex > 0)
                {
                    this.xClassNamespace = builderName.Substring(0, nameStartIndex);
                    xClassName = builderName.Substring(nameStartIndex + 1);
                }

                this.xClassNamespace = string.Format(CultureInfo.CurrentUICulture, "clr-namespace:{0}", this.xClassNamespace ?? string.Empty);
                this.xClassXamlType = new XamlType(this.xClassNamespace, xClassName, null, Writer.SchemaContext);
            }

            // Attributes [DependsOn("Name")]
            public void SetAttributes(XamlNodeQueue attributeNodes)
            {
                this.xClassAttributeNodes = attributeNodes;
            }

            // Properties [DependsOn("Attributes")]
            public void SetProperties(XamlNodeQueue propertyNodes, List<KeyValuePair<string, XamlNodeQueue>> defaultValueNodes)
            {
                this.xPropertiesNodes = propertyNodes;
                this.defaultValueNodes = defaultValueNodes;

                // exiting the properties tag. So we've now accrued any instances of Name and Attributes
                // that could possibly be hit flush our preamble
                FlushPreamble();
            }

            void FlushPreamble()
            {
                if (this.otherNodes == null) // already flushed
                {
                    return;
                }
                CurrentWriter = this.Writer.innerWriter;
                string classNamespace = null;
                // first, see if we need to emit a namespace corresponding to our class
                if (this.defaultValueNodes != null)
                {
                    classNamespace = this.xClassNamespace;
                }

                this.rootNode.FlushPendingNodes(classNamespace);
                this.rootNode = null; // not needed anymore

                CurrentWriter.WriteStartObject(this.Writer.activityXamlType);

                // first dump x:Class
                if (this.xClassNodes == null)
                {
                    SetXClass(null, null); // this will setup a default
                }
                XamlServices.Transform(this.xClassNodes.Reader, CurrentWriter, false);

                // String default values get written in attribute form immediately.
                // Other values get deferred until after x:Members, etc.
                XamlNodeQueue deferredPropertyNodes = null;
                if (this.defaultValueNodes != null)
                {
                    foreach (KeyValuePair<string, XamlNodeQueue> defaultValueNode in this.defaultValueNodes)
                    {
                        XamlReader reader = defaultValueNode.Value.Reader;
                        if (reader.Read())
                        {
                            bool isStringValue = false;
                            if (reader.NodeType == XamlNodeType.Value)
                            {
                                string stringValue = reader.Value as string;
                                if (stringValue != null)
                                {
                                    isStringValue = true;
                                }
                            }
                            if (isStringValue)
                            {
                                CurrentWriter.WriteStartMember(new XamlMember(defaultValueNode.Key, this.xClassXamlType, true));
                                CurrentWriter.WriteNode(reader);
                                XamlServices.Transform(defaultValueNode.Value.Reader, CurrentWriter, false);
                                // don't need an EndMember since it will be sitting in the node list (we only needed to strip the StartMember)                                
                            }
                            else
                            {
                                // Else: We'll write this out in a minute, after the x:ClassAttributes and x:Properties
                                if (deferredPropertyNodes == null)
                                {
                                    deferredPropertyNodes = new XamlNodeQueue(Writer.SchemaContext);
                                }
                                deferredPropertyNodes.Writer.WriteStartMember(new XamlMember(defaultValueNode.Key, this.xClassXamlType, true));
                                deferredPropertyNodes.Writer.WriteNode(reader);
                                XamlServices.Transform(defaultValueNode.Value.Reader, deferredPropertyNodes.Writer, false);
                            }
                        }
                    }
                }

                // then dump x:ClassAttributes if we have any
                if (this.xClassAttributeNodes != null)
                {
                    XamlServices.Transform(this.xClassAttributeNodes.Reader, CurrentWriter, false);
                }

                // and x:Properties
                if (this.xPropertiesNodes != null)
                {
                    XamlServices.Transform(this.xPropertiesNodes.Reader, CurrentWriter, false);
                }

                if (deferredPropertyNodes != null)
                {
                    XamlServices.Transform(deferredPropertyNodes.Reader, CurrentWriter, false);
                }

                if (this.otherNodes.Count > 0)
                {
                    XamlServices.Transform(this.otherNodes.Reader, CurrentWriter, false);
                }
                this.otherNodes = null; // done with this
            }

            protected internal override void Complete()
            {
                if (this.otherNodes != null)
                {
                    // need to flush
                    FlushPreamble();
                }
            }

            protected internal override void WriteStartMember(XamlMember xamlMember)
            {
                if (Writer.currentDepth == this.Depth + 1 && !xamlMember.IsAttachable)
                {
                    if (xamlMember == Writer.activityBuilderName)
                    {
                        // record that we're in ActivityBuilder.Name, since we'll need the class name for
                        // default value output
                        Writer.PushState(new BuilderNameNode(this, Writer));
                        return;
                    }
                    else if (xamlMember == Writer.activityBuilderAttributes)
                    {
                        // rewrite ActivityBuilder.Attributes to x:ClassAttributes
                        Writer.PushState(new AttributesNode(this, Writer));
                        return;
                    }
                    else if (xamlMember == Writer.activityBuilderProperties)
                    {
                        // rewrite ActivityBuilder.Properties to x:Members
                        Writer.PushState(new PropertiesNode(this, Writer));
                        return;
                    }
                    else
                    {
                        // any other member means we've passed properties due to [DependsOn] relationships
                        FlushPreamble();
                        if (xamlMember.DeclaringType == Writer.activityBuilderXamlType)
                        {
                            // Rewrite "<ActivityBuilder.XXX>" to "<Activity.XXX>"
                            xamlMember = Writer.activityXamlType.GetMember(xamlMember.Name);
                            if (xamlMember == null)
                            {
                                throw FxTrace.Exception.AsError(new InvalidOperationException(
                                    SR.MemberNotSupportedByActivityXamlServices(xamlMember.Name)));
                            }

                            if (xamlMember.Name == "Implementation")
                            {
                                Writer.PushState(new ImplementationNode(Writer));
                            }
                        }
                    }
                }
                base.WriteStartMember(xamlMember);
            }
        }

        // <ActivityBuilder.Name> node that we'll map to x:Class
        class BuilderNameNode : BuilderXamlNode
        {
            BuilderClassNode classNode;
            string builderName;
            XamlNodeQueue nameNodes;

            public BuilderNameNode(BuilderClassNode classNode, ActivityBuilderXamlWriter writer)
                : base(writer)
            {
                this.classNode = classNode;
                this.nameNodes = new XamlNodeQueue(writer.SchemaContext);
                base.CurrentWriter = this.nameNodes.Writer;
            }

            protected internal override void Complete()
            {
                this.classNode.SetXClass(this.builderName, this.nameNodes);
            }

            protected internal override void WriteValue(object value)
            {
                if (Writer.currentDepth == this.Depth)
                {
                    this.builderName = (string)value;
                }

                base.WriteValue(value);
            }
        }

        // <ActivityBuilder.Attributes> node that we'll map to x:ClassAttributes
        class AttributesNode : BuilderXamlNode
        {
            XamlNodeQueue attributeNodes;
            BuilderClassNode classNode;

            public AttributesNode(BuilderClassNode classNode, ActivityBuilderXamlWriter writer)
                : base(writer)
            {
                this.classNode = classNode;
                this.attributeNodes = new XamlNodeQueue(writer.SchemaContext);
                base.CurrentWriter = this.attributeNodes.Writer;
                CurrentWriter.WriteStartMember(XamlLanguage.ClassAttributes);
            }

            protected internal override void Complete()
            {
                this.classNode.SetAttributes(this.attributeNodes);
            }
        }

        // <ActivityBuilder.Properties> node that we'll map to x:Members
        // since x:Members doesn't have GetObject/StartMember wrappers around the value, we need to eat those
        class PropertiesNode : BuilderXamlNode
        {
            List<KeyValuePair<string, XamlNodeQueue>> defaultValueNodes;
            XamlNodeQueue propertiesNodes;
            BuilderClassNode classNode;
            bool skipGetObject;

            public PropertiesNode(BuilderClassNode classNode, ActivityBuilderXamlWriter writer)
                : base(writer)
            {
                this.classNode = classNode;
                this.propertiesNodes = new XamlNodeQueue(writer.SchemaContext);
                base.CurrentWriter = this.propertiesNodes.Writer;
                CurrentWriter.WriteStartMember(XamlLanguage.Members);
            }

            protected internal override void WriteStartObject(XamlType xamlType)
            {
                if (xamlType == Writer.activityPropertyXamlType && Writer.currentDepth == this.Depth + 3)
                {
                    xamlType = XamlLanguage.Property;
                    Writer.PushState(new PropertyNode(this, Writer));
                }
                base.WriteStartObject(xamlType);
            }

            protected internal override void WriteGetObject()
            {
                if (Writer.currentDepth == this.Depth + 1)
                {
                    this.skipGetObject = true;
                }
                else
                {
                    base.WriteGetObject();
                }
            }

            protected internal override void WriteEndObject()
            {
                if (this.skipGetObject && Writer.currentDepth == this.Depth + 1)
                {
                    this.skipGetObject = false;
                }
                else
                {
                    base.WriteEndObject();
                }
            }

            protected internal override void WriteStartMember(XamlMember xamlMember)
            {
                if (this.skipGetObject && Writer.currentDepth == this.Depth + 2)
                {
                    return;
                }
                base.WriteStartMember(xamlMember);
            }

            protected internal override void WriteEndMember()
            {
                if (this.skipGetObject && Writer.currentDepth == this.Depth + 2)
                {
                    return;
                }
                base.WriteEndMember();
            }

            protected internal override void Complete()
            {
                this.classNode.SetProperties(this.propertiesNodes, this.defaultValueNodes);
            }

            public void AddDefaultValue(string propertyName, XamlNodeQueue value)
            {
                if (this.defaultValueNodes == null)
                {
                    this.defaultValueNodes = new List<KeyValuePair<string, XamlNodeQueue>>();
                }

                if (string.IsNullOrEmpty(propertyName))
                {
                    // default a name if one doesn't exist
                    propertyName = string.Format(CultureInfo.CurrentCulture, "_{0}", Guid.NewGuid().ToString().Replace("-", string.Empty));
                }

                this.defaultValueNodes.Add(new KeyValuePair<string, XamlNodeQueue>(propertyName, value));
            }
        }

        // <DynamicActivityProperty>...</DynamicActivityProperty>
        class PropertyNode : BuilderXamlNode
        {
            PropertiesNode properties;
            string propertyName;
            XamlType propertyType;
            XamlNodeQueue defaultValue;

            public PropertyNode(PropertiesNode properties, ActivityBuilderXamlWriter writer)
                : base(writer)
            {
                this.properties = properties;
                base.CurrentWriter = properties.CurrentWriter;
            }

            public void SetName(string name)
            {
                this.propertyName = name;
            }

            public void SetType(XamlType type)
            {
                this.propertyType = type;
            }

            public void SetDefaultValue(XamlNodeQueue defaultValue)
            {
                this.defaultValue = defaultValue;
            }

            protected internal override void WriteStartMember(XamlMember xamlMember)
            {
                if (xamlMember.DeclaringType == Writer.activityPropertyXamlType && Writer.currentDepth == this.Depth + 1)
                {
                    if (xamlMember == Writer.activityPropertyName)
                    {
                        // record that we're in a property name, since we'll need this for default value output
                        Writer.PushState(new PropertyNameNode(this, Writer));
                        xamlMember = DynamicActivityXamlReader.xPropertyName;
                    }
                    else if (xamlMember == Writer.activityPropertyType)
                    {
                        // record that we're in a property type, since we'll need this for default value output
                        Writer.PushState(new PropertyTypeNode(this, Writer));
                        xamlMember = DynamicActivityXamlReader.xPropertyType;
                    }
                    else if (xamlMember == Writer.activityPropertyValue)
                    {
                        // record that we're in a property value, since we'll need this for default value output.
                        // don't write anything since we'll dump the default values after we exit ActivityBuilder.Properties
                        Writer.PushState(new PropertyValueNode(this, Writer));
                        xamlMember = null;
                    }
                }

                if (xamlMember != null)
                {
                    base.WriteStartMember(xamlMember);
                }
            }

            protected internal override void Complete()
            {
                if (this.defaultValue != null)
                {
                    if (string.IsNullOrEmpty(this.propertyName))
                    {
                        // default a name if one doesn't exist
                        this.propertyName = string.Format(CultureInfo.CurrentCulture, "_{0}", Guid.NewGuid().ToString().Replace("-", string.Empty));
                    }

                    if (this.defaultValue != null && this.propertyType != null)
                    {
                        // post-process the default value nodes to strip out 
                        // StartObject+StartMember _Initialization+EndMember+EndObject 
                        // wrapper nodes if the type of the object matches the 
                        // property Type (since we are moving from "object Value" to "T Value"
                        this.defaultValue = StripTypeWrapping(this.defaultValue, this.propertyType);
                    }

                    this.properties.AddDefaultValue(this.propertyName, this.defaultValue);
                }
            }

            static XamlNodeQueue StripTypeWrapping(XamlNodeQueue valueNodes, XamlType propertyType)
            {
                XamlNodeQueue targetNodes = new XamlNodeQueue(valueNodes.Reader.SchemaContext);
                XamlReader source = valueNodes.Reader;
                XamlWriter target = targetNodes.Writer;
                int depth = 0;
                bool consumeWrapperEndTags = false;
                bool hasBufferedStartObject = false;

                while (source.Read())
                {
                    switch (source.NodeType)
                    {
                        case XamlNodeType.StartObject:
                            depth++;
                            // only strip the wrapping type nodes if we have exactly this sequence:
                            // StartObject StartMember(Intialization) Value EndMember EndObject.
                            if (targetNodes.Count == 0 && depth == 1 && source.Type == propertyType && valueNodes.Count == 5)
                            {
                                hasBufferedStartObject = true;
                                continue;
                            }
                            break;

                        case XamlNodeType.GetObject:
                            depth++;
                            break;

                        case XamlNodeType.StartMember:
                            depth++;
                            if (hasBufferedStartObject)
                            {
                                if (depth == 2 && source.Member == XamlLanguage.Initialization)
                                {
                                    consumeWrapperEndTags = true;
                                    continue;
                                }
                                else
                                {
                                    hasBufferedStartObject = false;
                                    targetNodes.Writer.WriteStartObject(propertyType);
                                }
                            }
                            break;

                        case XamlNodeType.EndMember:
                            depth--;
                            if (consumeWrapperEndTags && depth == 1)
                            {
                                continue;
                            }
                            break;

                        case XamlNodeType.EndObject:
                            depth--;
                            if (consumeWrapperEndTags && depth == 0)
                            {
                                consumeWrapperEndTags = false;
                                continue;
                            }
                            break;
                    }

                    target.WriteNode(source);
                }

                return targetNodes;
            }
        }

        // <DynamicActivityProperty.Name>...</DynamicActivityProperty.Name>
        class PropertyNameNode : BuilderXamlNode
        {
            PropertyNode property;

            public PropertyNameNode(PropertyNode property, ActivityBuilderXamlWriter writer)
                : base(writer)
            {
                this.property = property;
                base.CurrentWriter = property.CurrentWriter;
            }

            protected internal override void WriteValue(object value)
            {
                if (Writer.currentDepth == this.Depth)
                {
                    property.SetName((string)value);
                }

                base.WriteValue(value);
            }
        }

        // <DynamicActivityProperty.Type>...</DynamicActivityProperty.Type>
        class PropertyTypeNode : BuilderXamlNode
        {
            PropertyNode property;

            public PropertyTypeNode(PropertyNode property, ActivityBuilderXamlWriter writer)
                : base(writer)
            {
                this.property = property;
                base.CurrentWriter = property.CurrentWriter;
            }

            protected internal override void WriteValue(object value)
            {
                if (Writer.currentDepth == this.Depth)
                {
                    // We only support property type as an attribute
                    XamlTypeName xamlTypeName = XamlTypeName.Parse(value as string, Writer.namespaceTable);
                    XamlType xamlType = Writer.SchemaContext.GetXamlType(xamlTypeName);
                    property.SetType(xamlType); // supports null
                }

                base.WriteValue(value);
            }
        }

        // <DynamicActivityProperty.Value>...</DynamicActivityProperty.Value>
        class PropertyValueNode : BuilderXamlNode
        {
            PropertyNode property;
            XamlNodeQueue valueNodes;

            public PropertyValueNode(PropertyNode property, ActivityBuilderXamlWriter writer)
                : base(writer)
            {
                this.property = property;
                this.valueNodes = new XamlNodeQueue(writer.SchemaContext);
                base.CurrentWriter = this.valueNodes.Writer;
            }

            protected internal override void Complete()
            {
                this.property.SetDefaultValue(this.valueNodes);
                base.Complete();
            }
        }

        // <ActivityBuilder.Implementation>...</ActivityBuilder.Implementation>
        // We need to convert any <ActivityBuilder.PropertyReferences> inside here into <PropertyReferenceExtension>.       
        class ImplementationNode : BuilderXamlNode
        {
            Stack<ObjectFrame> objectStack;

            public ImplementationNode(ActivityBuilderXamlWriter writer)
                : base(writer)
            {
                this.objectStack = new Stack<ObjectFrame>();
            }

            internal void AddPropertyReference(ActivityPropertyReference propertyReference)
            {
                ObjectFrame currentFrame = this.objectStack.Peek();
                Fx.Assert(currentFrame.Type != null, "Should only create PropertyReferencesNode inside a StartObject");
                if (currentFrame.PropertyReferences == null)
                {
                    currentFrame.PropertyReferences = new List<ActivityPropertyReference>();
                }
                currentFrame.PropertyReferences.Add(propertyReference);
            }

            internal void SetUntransformedPropertyReferences(XamlMember propertyReferencesMember, XamlNodeQueue untransformedNodes)
            {
                ObjectFrame currentFrame = this.objectStack.Peek();
                Fx.Assert(currentFrame.Type != null, "Should only create PropertyReferencesNode inside a StartObject");
                currentFrame.AddMember(propertyReferencesMember, untransformedNodes);
            }

            protected internal override void WriteStartMember(XamlMember xamlMember)
            {
                ObjectFrame currentFrame = this.objectStack.Peek();
                if (currentFrame.Type == null)
                {
                    base.WriteStartMember(xamlMember);
                }
                else if (xamlMember == Writer.activityBuilderPropertyReference || xamlMember == Writer.activityBuilderPropertyReferences)
                {
                    // Parse out the contents of <ActivityBuilder.PropertyReferences> using a PropertyReferencesNode
                    Writer.PushState(new PropertyReferencesNode(Writer, xamlMember, this));
                }
                else
                {
                    this.CurrentWriter = currentFrame.StartMember(xamlMember, CurrentWriter);
                }
            }

            protected internal override void WriteStartObject(XamlType xamlType)
            {
                this.objectStack.Push(new ObjectFrame { Type = xamlType });
                base.WriteStartObject(xamlType);
            }

            protected internal override void WriteGetObject()
            {
                this.objectStack.Push(new ObjectFrame());
                base.WriteGetObject();
            }

            protected internal override void WriteEndObject()
            {
                ObjectFrame frame = this.objectStack.Pop();
                frame.FlushMembers(CurrentWriter);
                base.WriteEndObject();
            }

            protected internal override void WriteEndMember()
            {
                // Stack can be empty here if this is the EndMember that closes out the Node
                ObjectFrame currentFrame = this.objectStack.Count > 0 ? this.objectStack.Peek() : null;
                if (currentFrame == null || currentFrame.Type == null)
                {
                    base.WriteEndMember();
                }
                else
                {
                    CurrentWriter = currentFrame.EndMember();
                }
            }

            class ObjectFrame
            {
                XamlWriter parentWriter;
                XamlNodeQueue currentMemberNodes;

                public XamlType Type { get; set; }
                public XamlMember CurrentMember { get; set; }
                public List<KeyValuePair<XamlMember, XamlNodeQueue>> Members { get; set; }
                public List<ActivityPropertyReference> PropertyReferences { get; set; }

                public XamlWriter StartMember(XamlMember member, XamlWriter parentWriter)
                {
                    this.CurrentMember = member;
                    this.parentWriter = parentWriter;
                    this.currentMemberNodes = new XamlNodeQueue(parentWriter.SchemaContext);
                    return this.currentMemberNodes.Writer;
                }

                public XamlWriter EndMember()
                {
                    AddMember(this.CurrentMember, this.currentMemberNodes);
                    this.CurrentMember = null;
                    this.currentMemberNodes = null;
                    XamlWriter parentWriter = this.parentWriter;
                    this.parentWriter = null;
                    return parentWriter;
                }

                public void AddMember(XamlMember member, XamlNodeQueue content)
                {
                    if (this.Members == null)
                    {
                        this.Members = new List<KeyValuePair<XamlMember, XamlNodeQueue>>();
                    }
                    this.Members.Add(new KeyValuePair<XamlMember, XamlNodeQueue>(member, content));
                }

                public void FlushMembers(XamlWriter parentWriter)
                {
                    if (this.Type == null)
                    {
                        Fx.Assert(Members == null, "We shouldn't buffer members on GetObject");
                        return;
                    }
                    if (Members != null)
                    {
                        foreach (KeyValuePair<XamlMember, XamlNodeQueue> member in Members)
                        {
                            parentWriter.WriteStartMember(member.Key);
                            XamlServices.Transform(member.Value.Reader, parentWriter, false);
                            parentWriter.WriteEndMember();
                        }
                    }
                    if (PropertyReferences != null)
                    {
                        foreach (ActivityPropertyReference propertyReference in PropertyReferences)
                        {
                            XamlMember targetProperty = this.Type.GetMember(propertyReference.TargetProperty) ??
                                new XamlMember(propertyReference.TargetProperty, this.Type, false);
                            parentWriter.WriteStartMember(targetProperty);
                            WritePropertyReference(parentWriter, targetProperty, propertyReference.SourceProperty);
                            parentWriter.WriteEndMember();
                        }
                    }
                }

                void WritePropertyReference(XamlWriter parentWriter, XamlMember targetProperty, string sourceProperty)
                {
                    Type propertyReferenceType = typeof(PropertyReferenceExtension<>).MakeGenericType(targetProperty.Type.UnderlyingType ?? typeof(object));
                    XamlType propertyReferenceXamlType = parentWriter.SchemaContext.GetXamlType(propertyReferenceType);
                    parentWriter.WriteStartObject(propertyReferenceXamlType);

                    if (sourceProperty != null)
                    {
                        parentWriter.WriteStartMember(propertyReferenceXamlType.GetMember("PropertyName"));
                        parentWriter.WriteValue(sourceProperty);
                        parentWriter.WriteEndMember();
                    }

                    parentWriter.WriteEndObject();
                }
            }
        }

        // <ActivityBuilder.PropertyReference(s)> is stripped out and the inner
        // <ActivityPropertyReference>s map to PropertyReferenceNodes
        class PropertyReferencesNode : BuilderXamlNode
        {
            XamlNodeQueue untransformedNodes; // nodes that couldn't be transformed to PropertyReference form
            XamlMember originalStartMember;

            public PropertyReferencesNode(ActivityBuilderXamlWriter writer, XamlMember originalStartMember, ImplementationNode parent)
                : base(writer)
            {
                this.untransformedNodes = new XamlNodeQueue(Writer.SchemaContext);
                this.originalStartMember = originalStartMember;
                this.Parent = parent;
                base.CurrentWriter = this.untransformedNodes.Writer;
            }

            public bool HasUntransformedChildren { get; set; }

            public ImplementationNode Parent { get; private set; }

            public XamlWriter UntransformedNodesWriter { get { return this.untransformedNodes.Writer; } }

            protected internal override void WriteStartObject(XamlType xamlType)
            {
                if (xamlType == Writer.activityPropertyReferenceXamlType)
                {
                    Writer.PushState(new PropertyReferenceNode(this.Writer, this));
                    return;
                }
                base.WriteStartObject(xamlType);
            }

            protected internal override void WriteEndMember()
            {
                // We only want the untransformedNodes writer to contain our member contents, not the
                // Start/End members, so don't write our closing EM
                if (Writer.currentDepth != this.Depth)
                {
                    base.WriteEndMember();
                }
            }

            protected internal override void Complete()
            {
                if (this.HasUntransformedChildren)
                {
                    // Some ActivityPropertyReferences couldn't be transformed to properties. Leave them unchanged.
                    this.Parent.SetUntransformedPropertyReferences(this.originalStartMember, this.untransformedNodes);
                }
            }
        }

        // <ActivityPropertyReference TargetProperty="Foo" SourceProperty="RootActivityProperty"> maps to
        // <SomeClass.Foo><PropertyReference x:TypeArguments='targetType' PropertyName='RootActivityProperty'/></SomeClass.Foo>
        class PropertyReferenceNode : BuilderXamlNode
        {
            XamlNodeQueue propertyReferenceNodes;
            PropertyReferencesNode parent;
            string sourceProperty;
            string targetProperty;
            bool inSourceProperty;
            bool inTargetProperty;

            public PropertyReferenceNode(ActivityBuilderXamlWriter writer, PropertyReferencesNode parent)
                : base(writer)
            {
                this.propertyReferenceNodes = new XamlNodeQueue(writer.SchemaContext);
                this.parent = parent;

                // save the untransformed output in case we're not able to perform the transformation
                base.CurrentWriter = this.propertyReferenceNodes.Writer;
            }

            protected internal override void WriteStartMember(XamlMember xamlMember)
            {
                if (Writer.currentDepth == this.Depth + 1 // SM
                    && xamlMember.DeclaringType == Writer.activityPropertyReferenceXamlType)
                {
                    if (xamlMember.Name == "SourceProperty")
                    {
                        this.inSourceProperty = true;
                    }
                    else if (xamlMember.Name == "TargetProperty")
                    {
                        this.inTargetProperty = true;
                    }
                }
                base.WriteStartMember(xamlMember); // save output just in case
            }

            protected internal override void WriteValue(object value)
            {
                if (this.inSourceProperty)
                {
                    this.sourceProperty = (string)value;
                }
                else if (this.inTargetProperty)
                {
                    this.targetProperty = (string)value;
                }
                base.WriteValue(value); // save output just in case
            }

            protected internal override void WriteEndMember()
            {
                if (Writer.currentDepth == this.Depth + 1)
                {
                    this.inSourceProperty = false;
                    this.inTargetProperty = false;
                }
                base.WriteEndMember(); // save output just in case
            }

            protected internal override void Complete()
            {
                if (this.targetProperty == null)
                {
                    // can't transform to <Foo.></Foo.>, dump original nodes <ActivityBuilder.PropertyReference(s) .../>
                    this.parent.HasUntransformedChildren = true;
                    this.parent.UntransformedNodesWriter.WriteStartObject(Writer.activityPropertyReferenceXamlType);
                    XamlServices.Transform(this.propertyReferenceNodes.Reader, this.parent.UntransformedNodesWriter, false);
                }
                else
                {
                    ActivityPropertyReference propertyReference = new ActivityPropertyReference
                    {
                        SourceProperty = this.sourceProperty,
                        TargetProperty = this.targetProperty
                    };
                    parent.Parent.AddPropertyReference(propertyReference);
                }
            }
        }
    }
}
