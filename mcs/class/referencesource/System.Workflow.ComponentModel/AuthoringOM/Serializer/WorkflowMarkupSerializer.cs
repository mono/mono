namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.IO;
    using System.CodeDom;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Collections;
    using System.Xml;
    using System.Xml.Serialization;
    using System.Reflection;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using System.Globalization;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Design;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Collections.ObjectModel;
    using System.Drawing;
    using System.Diagnostics.CodeAnalysis;

    #region Class WorkflowMarkupSerializer
    //Main serialization class for persisting the XOML
    [DefaultSerializationProvider(typeof(WorkflowMarkupSerializationProvider))]
    public class WorkflowMarkupSerializer
    {
        // x:Class & x:Code property.  public so it can be set from outside.
        public static readonly DependencyProperty XClassProperty = DependencyProperty.RegisterAttached("XClass", typeof(string), typeof(WorkflowMarkupSerializer), new PropertyMetadata(DependencyPropertyOptions.Metadata, new Attribute[] { new BrowsableAttribute(false), new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden) }));
        public static readonly DependencyProperty XCodeProperty = DependencyProperty.RegisterAttached("XCode", typeof(CodeTypeMemberCollection), typeof(WorkflowMarkupSerializer), new PropertyMetadata(DependencyPropertyOptions.Metadata, new Attribute[] { new BrowsableAttribute(false), new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden) }));

        public static readonly DependencyProperty EventsProperty = DependencyProperty.RegisterAttached("Events", typeof(Hashtable), typeof(WorkflowMarkupSerializer), new PropertyMetadata(null, new Attribute[] { new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden) }));
        public static readonly DependencyProperty ClrNamespacesProperty = DependencyProperty.RegisterAttached("ClrNamespaces", typeof(List<String>), typeof(WorkflowMarkupSerializer), new PropertyMetadata(null, new Attribute[] { new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden) }));

        #region Public Methods
        public object Deserialize(XmlReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            DesignerSerializationManager designerSerializationManager = new DesignerSerializationManager();
            using (designerSerializationManager.CreateSession())
            {
                return Deserialize(designerSerializationManager, reader);
            }
        }

        public object Deserialize(IDesignerSerializationManager serializationManager, XmlReader reader)
        {
            if (serializationManager == null)
                throw new ArgumentNullException("serializationManager");
            if (reader == null)
                throw new ArgumentNullException("reader");

            WorkflowMarkupSerializationManager markupSerializationManager = serializationManager as WorkflowMarkupSerializationManager;
            if (markupSerializationManager == null)
                markupSerializationManager = new WorkflowMarkupSerializationManager(serializationManager);

            string fileName = markupSerializationManager.Context[typeof(string)] as string;
            if (fileName == null)
                fileName = String.Empty;

            markupSerializationManager.FoundDefTag += delegate(object sender, WorkflowMarkupElementEventArgs eventArgs)
            {
                if (eventArgs.XmlReader.LookupNamespace(eventArgs.XmlReader.Prefix) == StandardXomlKeys.Definitions_XmlNs)
                    WorkflowMarkupSerializationHelpers.ProcessDefTag(markupSerializationManager, eventArgs.XmlReader, markupSerializationManager.Context.Current as Activity, false, fileName);
            };
            object obj = DeserializeXoml(markupSerializationManager, reader);

            // Copy the mappingPI to schedule user data
            Activity rootActivity = obj as Activity;
            if (rootActivity != null)
            {
                List<String> clrMappings = rootActivity.GetValue(WorkflowMarkupSerializer.ClrNamespacesProperty) as List<String>;
                if (clrMappings == null)
                {
                    clrMappings = new List<String>();
                    rootActivity.SetValue(WorkflowMarkupSerializer.ClrNamespacesProperty, clrMappings);
                }

                foreach (WorkflowMarkupSerializerMapping mapping in markupSerializationManager.ClrNamespaceBasedMappings.Values)
                    clrMappings.Add(mapping.ClrNamespace);

                rootActivity.SetValue(ActivityCodeDomSerializer.MarkupFileNameProperty, fileName);

                // If Name is not set and there is an XClass, set the name to that.
                if ((string.IsNullOrEmpty(rootActivity.Name) || rootActivity.Name == rootActivity.GetType().Name) && !string.IsNullOrEmpty(rootActivity.GetValue(WorkflowMarkupSerializer.XClassProperty) as string))
                {
                    string name = rootActivity.GetValue(WorkflowMarkupSerializer.XClassProperty) as string;
                    if (name.Contains("."))
                        rootActivity.Name = name.Substring(name.LastIndexOf('.') + 1);
                    else
                        rootActivity.Name = name;
                }
            }

            return obj;
        }

        private object DeserializeXoml(WorkflowMarkupSerializationManager serializationManager, XmlReader xmlReader)
        {
            if (serializationManager == null)
                throw new ArgumentNullException("serializationManager");
            if (xmlReader == null)
                throw new ArgumentNullException("xmlReader");

            Object obj = null;
            //xmlReader.WhitespaceHandling = WhitespaceHandling.None;
            serializationManager.WorkflowMarkupStack.Push(xmlReader);

            try
            {
                // 
                while (xmlReader.Read() && xmlReader.NodeType != XmlNodeType.Element && xmlReader.NodeType != XmlNodeType.ProcessingInstruction);
                if (xmlReader.EOF)
                    return null;
                obj = DeserializeObject(serializationManager, xmlReader);

                // Read until the end of the xml stream i.e past the </XomlDocument> tag. 
                // If there are any exceptions log them as errors.
                while (xmlReader.Read() && !xmlReader.EOF);
            }
            catch (XmlException xmlException)
            {
                throw new WorkflowMarkupSerializationException(xmlException.Message, xmlException, xmlException.LineNumber, xmlException.LinePosition);
            }
            catch (Exception e)
            {
                throw CreateSerializationError(e, xmlReader);
            }
            finally
            {
                serializationManager.WorkflowMarkupStack.Pop();
            }

            return obj;
        }

        public void Serialize(XmlWriter writer, object obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            if (writer == null)
                throw new ArgumentNullException("writer");

            DesignerSerializationManager designerSerializationManager = new DesignerSerializationManager();
            using (designerSerializationManager.CreateSession())
            {
                Serialize(designerSerializationManager, writer, obj);
            }
        }

        public void Serialize(IDesignerSerializationManager serializationManager, XmlWriter writer, object obj)
        {
            if (serializationManager == null)
                throw new ArgumentNullException("serializationManager");
            if (obj == null)
                throw new ArgumentNullException("obj");
            if (writer == null)
                throw new ArgumentNullException("writer");

            WorkflowMarkupSerializationManager markupSerializationManager = serializationManager as WorkflowMarkupSerializationManager;
            if (markupSerializationManager == null)
                markupSerializationManager = new WorkflowMarkupSerializationManager(serializationManager);

            StringWriter xomlStringWriter = new StringWriter(CultureInfo.InvariantCulture);
            XmlWriter xmlWriter = Helpers.CreateXmlWriter(xomlStringWriter);
            markupSerializationManager.WorkflowMarkupStack.Push(xmlWriter);
            markupSerializationManager.WorkflowMarkupStack.Push(xomlStringWriter);

            try
            {
                SerializeObject(markupSerializationManager, obj, xmlWriter);
            }
            finally
            {
                xmlWriter.Close();
                writer.WriteRaw(xomlStringWriter.ToString());
                writer.Flush();
                markupSerializationManager.WorkflowMarkupStack.Pop();
                markupSerializationManager.WorkflowMarkupStack.Pop();
            }
        }
        #endregion

        #region Protected Methods (Non-overridable)
        internal object DeserializeObject(WorkflowMarkupSerializationManager serializationManager, XmlReader reader)
        {
            if (serializationManager == null)
                throw new ArgumentNullException("serializationManager");
            if (reader == null)
                throw new ArgumentNullException("reader");

            object obj = null;
            try
            {
                serializationManager.WorkflowMarkupStack.Push(reader);

                AdvanceReader(reader);
                if (reader.NodeType != XmlNodeType.Element)
                {
                    serializationManager.ReportError(CreateSerializationError(SR.GetString(SR.Error_InvalidDataFound), reader));
                }
                else
                {
                    // Lets ignore the Definition tags if nobody is interested
                    //
                    string decodedName = XmlConvert.DecodeName(reader.LocalName);
                    XmlQualifiedName xmlQualifiedName = new XmlQualifiedName(decodedName, reader.LookupNamespace(reader.Prefix));
                    if (xmlQualifiedName.Namespace.Equals(StandardXomlKeys.Definitions_XmlNs, StringComparison.Ordinal) &&
                        !IsMarkupExtension(xmlQualifiedName) &&
                        !ExtendedPropertyInfo.IsExtendedProperty(serializationManager, xmlQualifiedName))
                    {
                        int initialDepth = reader.Depth;
                        serializationManager.FireFoundDefTag(new WorkflowMarkupElementEventArgs(reader));
                        if ((initialDepth + 1) < reader.Depth)
                        {
                            while (reader.Read() && (initialDepth + 1) < reader.Depth);
                        }
                    }
                    else
                    {
                        obj = CreateInstance(serializationManager, xmlQualifiedName, reader);
                        reader.MoveToElement();
                        if (obj != null)
                        {
                            serializationManager.Context.Push(obj);
                            try
                            {
                                DeserializeContents(serializationManager, obj, reader);
                            }
                            finally
                            {
                                Debug.Assert(serializationManager.Context.Current == obj, "Serializer did not remove an object it pushed into stack.");
                                serializationManager.Context.Pop();
                            }
                        }
                    }
                }
            }
            finally
            {
                serializationManager.WorkflowMarkupStack.Pop();
            }
            return obj;
        }

        private void DeserializeContents(WorkflowMarkupSerializationManager serializationManager, object obj, XmlReader reader)
        {
            if (serializationManager == null)
                throw new ArgumentNullException("serializationManager");
            if (obj == null)
                throw new ArgumentNullException("obj");
            if (reader == null)
                throw new ArgumentNullException("reader");

            if (reader.NodeType != XmlNodeType.Element)
                return;

            // get the serializer
            WorkflowMarkupSerializer serializer = serializationManager.GetSerializer(obj.GetType(), typeof(WorkflowMarkupSerializer)) as WorkflowMarkupSerializer;
            if (serializer == null)
            {
                serializationManager.ReportError(CreateSerializationError(SR.GetString(SR.Error_SerializerNotAvailable, obj.GetType().FullName), reader));
                return;
            }

            try
            {
                serializer.OnBeforeDeserialize(serializationManager, obj);
            }
            catch (Exception e)
            {
                serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString(SR.Error_SerializerThrewException, obj.GetType().FullName, e.Message), e));
                return;
            }

            bool isEmptyElement = reader.IsEmptyElement;
            string elementNamespace = reader.NamespaceURI;

            List<PropertyInfo> props = new List<PropertyInfo>();
            List<EventInfo> events = new List<EventInfo>();
            // Add the extended properties for primitive types
            if (obj.GetType().IsPrimitive || obj.GetType() == typeof(string) || obj.GetType() == typeof(decimal) ||
                obj.GetType().IsEnum || obj.GetType() == typeof(DateTime) || obj.GetType() == typeof(TimeSpan) ||
                obj.GetType() == typeof(Guid))
            {
                props.AddRange(serializationManager.GetExtendedProperties(obj));
            }
            else
            {
                try
                {
                    props.AddRange(serializer.GetProperties(serializationManager, obj));
                    props.AddRange(serializationManager.GetExtendedProperties(obj));
                    events.AddRange(serializer.GetEvents(serializationManager, obj));
                }
                catch (Exception e)
                {
                    serializationManager.ReportError(CreateSerializationError(SR.GetString(SR.Error_SerializerThrewException, obj.GetType(), e.Message), e, reader));
                    return;
                }
            }
            //First we try to deserialize simple properties
            if (reader.HasAttributes)
            {
                while (reader.MoveToNextAttribute())
                {
                    // 
                    if (reader.LocalName.Equals("xmlns", StringComparison.Ordinal) || reader.Prefix.Equals("xmlns", StringComparison.Ordinal))
                        continue;

                    //
                    XmlQualifiedName xmlQualifiedName = new XmlQualifiedName(reader.LocalName, reader.LookupNamespace(reader.Prefix));
                    if (xmlQualifiedName.Namespace.Equals(StandardXomlKeys.Definitions_XmlNs, StringComparison.Ordinal) &&
                        !IsMarkupExtension(xmlQualifiedName) &&
                        !ExtendedPropertyInfo.IsExtendedProperty(serializationManager, props, xmlQualifiedName) &&
                        !ExtendedPropertyInfo.IsExtendedProperty(serializationManager, xmlQualifiedName))
                    {
                        serializationManager.FireFoundDefTag(new WorkflowMarkupElementEventArgs(reader));
                        continue;
                    }

                    //For simple properties we assume that if . indicates
                    string propName = XmlConvert.DecodeName(reader.LocalName);
                    string propVal = reader.Value;
                    DependencyProperty dependencyProperty = ResolveDependencyProperty(serializationManager, reader, obj, propName);
                    if (dependencyProperty != null)
                    {
                        serializationManager.Context.Push(dependencyProperty);
                        try
                        {
                            if (dependencyProperty.IsEvent)
                                DeserializeEvent(serializationManager, reader, obj, propVal);
                            else
                                DeserializeSimpleProperty(serializationManager, reader, obj, propVal);
                        }
                        finally
                        {
                            Debug.Assert(serializationManager.Context.Current == dependencyProperty, "Serializer did not remove an object it pushed into stack.");
                            serializationManager.Context.Pop();
                        }
                    }
                    else
                    {
                        PropertyInfo property = WorkflowMarkupSerializer.LookupProperty(props, propName);
                        if (property != null)
                        {
                            serializationManager.Context.Push(property);
                            try
                            {
                                DeserializeSimpleProperty(serializationManager, reader, obj, propVal);
                            }
                            finally
                            {
                                Debug.Assert((PropertyInfo)serializationManager.Context.Current == property, "Serializer did not remove an object it pushed into stack.");
                                serializationManager.Context.Pop();
                            }
                        }
                        else
                        {
                            EventInfo evt = WorkflowMarkupSerializer.LookupEvent(events, propName);
                            if (events != null && evt != null)
                            {
                                serializationManager.Context.Push(evt);
                                try
                                {
                                    DeserializeEvent(serializationManager, reader, obj, propVal);
                                }
                                finally
                                {
                                    Debug.Assert((EventInfo)serializationManager.Context.Current == evt, "Serializer did not remove an object it pushed into stack.");
                                    serializationManager.Context.Pop();
                                }
                            }
                            else
                                serializationManager.ReportError(CreateSerializationError(SR.GetString(SR.Error_SerializerNoMemberFound, new object[] { propName, obj.GetType().FullName }), reader));
                        }
                    }
                }
            }

            try
            {
                serializer.OnBeforeDeserializeContents(serializationManager, obj);
            }
            catch (Exception e)
            {
                serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString(SR.Error_SerializerThrewException, obj.GetType().FullName, e.Message), e));
                return;
            }

            //Now deserialize compound properties
            try
            {
                serializer.ClearChildren(serializationManager, obj);
            }
            catch (Exception e)
            {
                serializationManager.ReportError(CreateSerializationError(SR.GetString(SR.Error_SerializerThrewException, obj.GetType(), e.Message), e, reader));
                return;
            }

            using (ContentProperty contentProperty = new ContentProperty(serializationManager, serializer, obj))
            {
                List<ContentInfo> contents = new List<ContentInfo>();
                if (!isEmptyElement)
                {
                    reader.MoveToElement();
                    int initialDepth = reader.Depth;
                    XmlQualifiedName extendedPropertyQualifiedName = new XmlQualifiedName(reader.LocalName, reader.LookupNamespace(reader.Prefix));
                    do
                    {
                        // Extended property should be deserialized, this is required for primitive types which have extended property as children
                        // We should  not ignore 
                        if (extendedPropertyQualifiedName != null && !ExtendedPropertyInfo.IsExtendedProperty(serializationManager, extendedPropertyQualifiedName))
                        {
                            extendedPropertyQualifiedName = null;
                            continue;
                        }
                        // this will make it to skip all the nodes
                        if ((initialDepth + 1) < reader.Depth)
                        {
                            bool unnecessaryXmlFound = false;
                            while (reader.Read() && ((initialDepth + 1) < reader.Depth))
                            {
                                // Ignore comments and whitespaces
                                if (reader.NodeType != XmlNodeType.Comment && reader.NodeType != XmlNodeType.Whitespace && !unnecessaryXmlFound)
                                {
                                    serializationManager.ReportError(CreateSerializationError(SR.GetString(SR.Error_InvalidDataFoundForType, obj.GetType().FullName), reader));
                                    unnecessaryXmlFound = true;
                                }
                            }
                        }

                        //Push all the PIs into stack so that they are available for type resolution
                        AdvanceReader(reader);
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            //We should only support A.B syntax for compound properties, all others are treated as content
                            XmlQualifiedName xmlQualifiedName = new XmlQualifiedName(reader.LocalName, reader.LookupNamespace(reader.Prefix));
                            int index = reader.LocalName.IndexOf('.');
                            if (index > 0 || ExtendedPropertyInfo.IsExtendedProperty(serializationManager, xmlQualifiedName))
                            {
                                string propertyName = reader.LocalName.Substring(reader.LocalName.IndexOf('.') + 1);
                                PropertyInfo property = WorkflowMarkupSerializer.LookupProperty(props, propertyName);
                                DependencyProperty dependencyProperty = ResolveDependencyProperty(serializationManager, reader, obj, reader.LocalName);
                                if (dependencyProperty == null && property == null)
                                    serializationManager.ReportError(CreateSerializationError(SR.GetString(SR.Error_InvalidElementFoundForType, reader.LocalName, obj.GetType().FullName), reader));
                                else if (dependencyProperty != null)
                                {
                                    PropertyInfo prop = WorkflowMarkupSerializer.LookupProperty(props, dependencyProperty.Name);
                                    //Deserialize the dynamic property
                                    serializationManager.Context.Push(dependencyProperty);
                                    try
                                    {
                                        DeserializeCompoundProperty(serializationManager, reader, obj);
                                    }
                                    finally
                                    {
                                        Debug.Assert(serializationManager.Context.Current == dependencyProperty, "Serializer did not remove an object it pushed into stack.");
                                        serializationManager.Context.Pop();
                                    }
                                }
                                else if (property != null)
                                {
                                    //Deserialize the compound property
                                    serializationManager.Context.Push(property);
                                    try
                                    {
                                        DeserializeCompoundProperty(serializationManager, reader, obj);
                                    }
                                    finally
                                    {
                                        Debug.Assert((PropertyInfo)serializationManager.Context.Current == property, "Serializer did not remove an object it pushed into stack.");
                                        serializationManager.Context.Pop();
                                    }
                                }
                            }
                            else
                            {
                                //Deserialize the children
                                int lineNumber = (reader is IXmlLineInfo) ? ((IXmlLineInfo)reader).LineNumber : 1;
                                int linePosition = (reader is IXmlLineInfo) ? ((IXmlLineInfo)reader).LinePosition : 1;
                                object obj2 = DeserializeObject(serializationManager, reader);
                                if (obj2 != null)
                                {
                                    obj2 = GetValueFromMarkupExtension(serializationManager, obj2);
                                    if (obj2 != null && obj2.GetType() == typeof(string) && ((string)obj2).StartsWith("{}", StringComparison.Ordinal))
                                        obj2 = ((string)obj2).Substring(2);
                                    contents.Add(new ContentInfo(obj2, lineNumber, linePosition));
                                }
                            }
                        }
                        else if (reader.NodeType == XmlNodeType.Text && contentProperty.Property != null)
                        {
                            //If we read the string then we should not advance the reader further instead break
                            int lineNumber = (reader is IXmlLineInfo) ? ((IXmlLineInfo)reader).LineNumber : 1;
                            int linePosition = (reader is IXmlLineInfo) ? ((IXmlLineInfo)reader).LinePosition : 1;
                            contents.Add(new ContentInfo(reader.ReadString(), lineNumber, linePosition));
                            if (initialDepth >= reader.Depth)
                                break;
                        }
                        else
                        {
                            if (reader.NodeType == XmlNodeType.Entity ||
                                reader.NodeType == XmlNodeType.Text ||
                                reader.NodeType == XmlNodeType.CDATA)
                                serializationManager.ReportError(CreateSerializationError(SR.GetString(SR.Error_InvalidDataFound, reader.Value.Trim(), obj.GetType().FullName), reader));
                        }
                    } while (reader.Read() && initialDepth < reader.Depth);
                }
                //Make sure that we set contents
                contentProperty.SetContents(contents);
            }
            try
            {
                serializer.OnAfterDeserialize(serializationManager, obj);
            }
            catch (Exception e)
            {
                serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString(SR.Error_SerializerThrewException, obj.GetType().FullName, e.Message), e));
                return;
            }
        }

        internal void SerializeObject(WorkflowMarkupSerializationManager serializationManager, object obj, XmlWriter writer)
        {
            if (serializationManager == null)
                throw new ArgumentNullException("serializationManager");
            if (obj == null)
                throw new ArgumentNullException("obj");
            if (writer == null)
                throw new ArgumentNullException("writer");

            try
            {
                serializationManager.WorkflowMarkupStack.Push(writer);
                using (new SafeXmlNodeWriter(serializationManager, obj, null, XmlNodeType.Element))
                {
                    DictionaryEntry? entry = null;
                    if (serializationManager.WorkflowMarkupStack[typeof(DictionaryEntry)] != null)
                        entry = (DictionaryEntry)serializationManager.WorkflowMarkupStack[typeof(DictionaryEntry)];

                    // To handle the case when the key and value are same in the dictionary
                    bool key = entry.HasValue && ((!entry.Value.GetType().IsValueType && entry.Value.Key == entry.Value.Value && entry.Value.Value == obj) ||
                                                    (entry.Value.GetType().IsValueType && entry.Value.Key.Equals(entry.Value.Value) && entry.Value.Value.Equals(obj))) &&
                                                    serializationManager.SerializationStack.Contains(obj);
                    if (key || !serializationManager.SerializationStack.Contains(obj))
                    {
                        serializationManager.Context.Push(obj);
                        serializationManager.SerializationStack.Push(obj);
                        try
                        {
                            SerializeContents(serializationManager, obj, writer, key);
                        }
                        finally
                        {
                            Debug.Assert(serializationManager.Context.Current == obj, "Serializer did not remove an object it pushed into stack.");
                            serializationManager.Context.Pop();
                            serializationManager.SerializationStack.Pop();
                        }
                    }
                    else
                        throw new WorkflowMarkupSerializationException(SR.GetString(SR.Error_SerializerStackOverflow, obj.ToString(), obj.GetType().FullName), 0, 0);
                }
            }
            finally
            {
                serializationManager.WorkflowMarkupStack.Pop();
            }
        }

        internal void SerializeContents(WorkflowMarkupSerializationManager serializationManager, object obj, XmlWriter writer, bool dictionaryKey)
        {
            if (serializationManager == null)
                throw new ArgumentNullException("serializationManager");
            if (obj == null)
                throw new ArgumentNullException("obj");
            if (writer == null)
                throw new ArgumentNullException("writer");

            WorkflowMarkupSerializer serializer = null;
            try
            {
                //Now get the serializer to persist the properties, if the serializer is not found then we dont serialize the properties
                serializer = serializationManager.GetSerializer(obj.GetType(), typeof(WorkflowMarkupSerializer)) as WorkflowMarkupSerializer;

            }
            catch (Exception e)
            {
                serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString(SR.Error_SerializerThrewException, obj.GetType().FullName, e.Message), e));
                return;

            }

            if (serializer == null)
            {
                serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString(SR.Error_SerializerNotAvailableForSerialize, obj.GetType().FullName)));
                return;
            }

            try
            {
                serializer.OnBeforeSerialize(serializationManager, obj);
            }
            catch (Exception e)
            {
                serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString(SR.Error_SerializerThrewException, obj.GetType().FullName, e.Message), e));
                return;
            }

            Hashtable allProperties = new Hashtable();
            ArrayList complexProperties = new ArrayList();

            IDictionary<DependencyProperty, object> dependencyProperties = null;
            List<PropertyInfo> properties = new List<PropertyInfo>();
            List<EventInfo> events = new List<EventInfo>();
            Hashtable designTimeTypeNames = null;

            // Serialize the extended properties for primitive types also
            if (obj.GetType().IsPrimitive || obj.GetType() == typeof(string) || obj.GetType() == typeof(decimal) ||
                obj.GetType() == typeof(DateTime) || obj.GetType() == typeof(TimeSpan) || obj.GetType().IsEnum ||
                obj.GetType() == typeof(Guid))
            {
                if (obj.GetType() == typeof(char) || obj.GetType() == typeof(byte) ||
                    obj.GetType() == typeof(System.Int16) || obj.GetType() == typeof(decimal) ||
                    obj.GetType() == typeof(DateTime) || obj.GetType() == typeof(TimeSpan) ||
                    obj.GetType().IsEnum || obj.GetType() == typeof(Guid))
                {
                    //These non CLS-compliant are not supported in the XmlWriter 
                    if ((obj.GetType() != typeof(char)) || (char)obj != '\0')
                    {
                        //These non CLS-compliant are not supported in the XmlReader 
                        string stringValue = String.Empty;
                        if (obj.GetType() == typeof(DateTime))
                        {
                            stringValue = ((DateTime)obj).ToString("o", CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            TypeConverter typeConverter = TypeDescriptor.GetConverter(obj.GetType());
                            if (typeConverter != null && typeConverter.CanConvertTo(typeof(string)))
                                stringValue = typeConverter.ConvertTo(null, CultureInfo.InvariantCulture, obj, typeof(string)) as string;
                            else
                                stringValue = Convert.ToString(obj, CultureInfo.InvariantCulture);
                        }

                        writer.WriteValue(stringValue);
                    }
                }
                else if (obj.GetType() == typeof(string))
                {
                    string attribValue = obj as string;
                    attribValue = attribValue.Replace('\0', ' ');
                    if (!(attribValue.StartsWith("{", StringComparison.Ordinal) && attribValue.EndsWith("}", StringComparison.Ordinal)))
                        writer.WriteValue(attribValue);
                    else
                        writer.WriteValue("{}" + attribValue);
                }
                else
                {
                    writer.WriteValue(obj);
                }
                // For Key properties, we don;t want to get the extended properties
                if (!dictionaryKey)
                    properties.AddRange(serializationManager.GetExtendedProperties(obj));
            }
            else
            {
                // Serialize properties
                //We first get all the properties, once we have them all, we start distinguishing between
                //simple and complex properties, the reason for that is XmlWriter needs to write attributes
                //first and elements later

                // Dependency events are treated as the same as dependency properties.


                try
                {
                    if (obj is DependencyObject && ((DependencyObject)obj).UserData.Contains(UserDataKeys.DesignTimeTypeNames))
                        designTimeTypeNames = ((DependencyObject)obj).UserData[UserDataKeys.DesignTimeTypeNames] as Hashtable;
                    dependencyProperties = serializer.GetDependencyProperties(serializationManager, obj);
                    properties.AddRange(serializer.GetProperties(serializationManager, obj));
                    // For Key properties, we don;t want to get the extended properties
                    if (!dictionaryKey)
                        properties.AddRange(serializationManager.GetExtendedProperties(obj));
                    events.AddRange(serializer.GetEvents(serializationManager, obj));
                }
                catch (Exception e)
                {
                    serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString(SR.Error_SerializerThrewException, obj.GetType().FullName, e.Message), e));
                    return;
                }
            }
            if (dependencyProperties != null)
            {
                // For attached properties that does not have a corresponding real property on the object, if the value is a design time
                // type, it may not be set through dependency property SetValue, therefore will not be present in the dependencyProperties
                // collection, we'll have to get the dependency property object ourselves.
                if (designTimeTypeNames != null)
                {
                    foreach (object key in designTimeTypeNames.Keys)
                    {
                        DependencyProperty dependencyProperty = key as DependencyProperty;
                        if (dependencyProperty != null && !dependencyProperties.ContainsKey(dependencyProperty))
                            dependencyProperties.Add(dependencyProperty, designTimeTypeNames[dependencyProperty]);
                    }
                }

                // Add all dependency properties to the master collection.
                foreach (DependencyProperty dependencyProperty in dependencyProperties.Keys)
                {
                    string propertyName = String.Empty;
                    if (dependencyProperty.IsAttached)
                    {
                        string prefix = String.Empty;
                        XmlQualifiedName qualifiedName = serializationManager.GetXmlQualifiedName(dependencyProperty.OwnerType, out prefix);
                        propertyName = qualifiedName.Name + "." + dependencyProperty.Name;
                    }
                    else
                    {
                        propertyName = dependencyProperty.Name;
                    }

                    if (dependencyProperty.IsAttached || !dependencyProperty.DefaultMetadata.IsMetaProperty)
                        allProperties.Add(propertyName, dependencyProperty);
                }
            }

            if (properties != null)
            {
                foreach (PropertyInfo propInfo in properties)
                {
                    // Do not serialize properties that have corresponding dynamic properties.
                    if (propInfo != null && !allProperties.ContainsKey(propInfo.Name))
                        allProperties.Add(propInfo.Name, propInfo);
                }
            }

            if (events != null)
            {
                foreach (EventInfo eventInfo in events)
                {
                    // Do not serialize events that have corresponding dynamic properties.
                    if (eventInfo != null && !allProperties.ContainsKey(eventInfo.Name))
                        allProperties.Add(eventInfo.Name, eventInfo);
                }
            }

            using (ContentProperty contentProperty = new ContentProperty(serializationManager, serializer, obj))
            {
                foreach (object propertyObj in allProperties.Values)
                {
                    string propertyName = String.Empty;
                    object propertyValue = null;
                    Type propertyInfoType = null;

                    try
                    {
                        if (propertyObj is PropertyInfo)
                        {
                            PropertyInfo property = propertyObj as PropertyInfo;

                            // If the property has parameters we can not serialize it , we just move on.
                            ParameterInfo[] indexParameters = property.GetIndexParameters();
                            if (indexParameters != null && indexParameters.Length > 0)
                                continue;

                            propertyName = property.Name;
                            propertyValue = null;
                            if (property.CanRead)
                            {
                                propertyValue = property.GetValue(obj, null);
                                if (propertyValue == null && TypeProvider.IsAssignable(typeof(Type), property.PropertyType))
                                {
                                    // See if we have a design time value for the property
                                    DependencyProperty dependencyProperty = DependencyProperty.FromName(property.Name, property.ReflectedType);
                                    if (dependencyProperty != null)
                                        propertyValue = Helpers.GetDesignTimeTypeName(obj, dependencyProperty);

                                    if (propertyValue == null)
                                    {
                                        string key = property.ReflectedType.FullName + "." + property.Name;
                                        propertyValue = Helpers.GetDesignTimeTypeName(obj, key);
                                    }
                                    if (propertyValue != null)
                                        propertyValue = new TypeExtension((string)propertyValue);
                                }
                            }
                            propertyInfoType = property.PropertyType;
                        }
                        else if (propertyObj is EventInfo)
                        {
                            EventInfo evt = propertyObj as EventInfo;
                            propertyName = evt.Name;
                            propertyValue = WorkflowMarkupSerializationHelpers.GetEventHandlerName(obj, evt.Name);
                            if ((propertyValue == null || (propertyValue is string && string.IsNullOrEmpty((string)propertyValue)))
                                && obj is DependencyObject)
                            {
                                // The object is not created through deserialization, we should check to see if the delegate is 
                                // created and added to list.  We can only serialize the handler if its target type is the same
                                // as the activity type that's be designed.
                                DependencyProperty dependencyProperty = DependencyProperty.FromName(propertyName, obj.GetType());
                                if (dependencyProperty != null)
                                {
                                    Activity activity = serializationManager.Context[typeof(Activity)] as Activity;
                                    Delegate handler = ((DependencyObject)obj).GetHandler(dependencyProperty) as Delegate;
                                    if (handler != null && activity != null && TypeProvider.Equals(handler.Target.GetType(), Helpers.GetRootActivity(activity).GetType()))
                                        propertyValue = handler;
                                }
                            }
                            propertyInfoType = evt.EventHandlerType;
                        }
                        else if (propertyObj is DependencyProperty)
                        {
                            DependencyProperty dependencyProperty = propertyObj as DependencyProperty;
                            propertyName = dependencyProperty.Name;
                            propertyValue = dependencyProperties[dependencyProperty];
                            propertyInfoType = dependencyProperty.PropertyType;
                        }
                    }
                    catch (Exception e)
                    {
                        while (e is TargetInvocationException && e.InnerException != null)
                            e = e.InnerException;

                        serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString(SR.Error_SerializerPropertyGetFailed, new object[] { propertyName, obj.GetType().FullName, e.Message })));
                        continue;
                    }

                    if (propertyObj is PropertyInfo && contentProperty.Property == (PropertyInfo)propertyObj)
                        continue;

                    Type propertyValueType = null;
                    if (propertyValue != null)
                    {
                        propertyValue = GetMarkupExtensionFromValue(propertyValue);
                        propertyValueType = propertyValue.GetType();
                    }
                    else if (propertyObj is PropertyInfo)
                    {
                        propertyValue = new NullExtension();
                        propertyValueType = propertyValue.GetType();
                        Attribute[] attributes = Attribute.GetCustomAttributes(propertyObj as PropertyInfo, typeof(DefaultValueAttribute), true);
                        if (attributes.Length > 0)
                        {
                            DefaultValueAttribute defaultValueAttr = attributes[0] as DefaultValueAttribute;
                            if (defaultValueAttr.Value == null)
                                propertyValue = null;
                        }
                    }
                    if (propertyValue != null)
                        propertyValueType = propertyValue.GetType();

                    //Now get the serializer to persist the properties, if the serializer is not found then we dont serialize the properties
                    serializationManager.Context.Push(propertyObj);
                    WorkflowMarkupSerializer propValueSerializer = null;
                    try
                    {
                        propValueSerializer = serializationManager.GetSerializer(propertyValueType, typeof(WorkflowMarkupSerializer)) as WorkflowMarkupSerializer;
                    }
                    catch (Exception e)
                    {
                        serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString(SR.Error_SerializerThrewException, obj.GetType().FullName, e.Message), e));
                        serializationManager.Context.Pop();
                        continue;
                    }
                    if (propValueSerializer == null)
                    {
                        serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString(SR.Error_SerializerNotAvailableForSerialize, propertyValueType.FullName)));
                        serializationManager.Context.Pop();
                        continue;
                    }

                    // ask serializer if we can serialize
                    try
                    {
                        if (propValueSerializer.ShouldSerializeValue(serializationManager, propertyValue))
                        {
                            //NOTE: THE FOLLOWING CONDITION ABOUT propertyInfoType != typeof(object) is VALID AS WE SHOULD NOT SERIALIZE A PROPERTY OF TYPE OBJECT TO STRING
                            //IF WE DO THAT THEN WE DO NOT KNOWN WHAT WAS THE TYPE OF ORIGINAL OBJECT AND SERIALIZER WONT BE ABLE TO GET THE STRING BACK INTO THE CORRECT TYPE,
                            //AS THE TYPE INFORMATION IS LOST
                            if (propValueSerializer.CanSerializeToString(serializationManager, propertyValue) && propertyInfoType != typeof(object))
                            {
                                using (new SafeXmlNodeWriter(serializationManager, obj, propertyObj, XmlNodeType.Attribute))
                                {
                                    //This is a work around to special case the markup extension serializer as it writes to the stream using writer
                                    if (propValueSerializer is MarkupExtensionSerializer)
                                    {
                                        propValueSerializer.SerializeToString(serializationManager, propertyValue);
                                    }
                                    else
                                    {
                                        string stringValue = propValueSerializer.SerializeToString(serializationManager, propertyValue);
                                        if (!string.IsNullOrEmpty(stringValue))
                                        {
                                            stringValue = stringValue.Replace('\0', ' ');
                                            if (propertyValue is MarkupExtension || !(stringValue.StartsWith("{", StringComparison.Ordinal) && stringValue.EndsWith("}", StringComparison.Ordinal)))
                                                writer.WriteString(stringValue);
                                            else
                                                writer.WriteString("{}" + stringValue);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                complexProperties.Add(propertyObj);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString(SR.Error_SerializerNoSerializeLogic, new object[] { propertyName, obj.GetType().FullName }), e));
                    }
                    finally
                    {
                        Debug.Assert(serializationManager.Context.Current == propertyObj, "Serializer did not remove an object it pushed into stack.");
                        serializationManager.Context.Pop();
                    }
                }

                try
                {
                    serializer.OnBeforeSerializeContents(serializationManager, obj);
                }
                catch (Exception e)
                {
                    serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString(SR.Error_SerializerThrewException, obj.GetType().FullName, e.Message), e));
                    return;
                }

                // serialize compound properties as child elements of the current node.
                foreach (object propertyObj in complexProperties)
                {
                    // get value and check for null
                    string propertyName = String.Empty;
                    object propertyValue = null;
                    Type ownerType = null;
                    bool isReadOnly = false;

                    try
                    {
                        if (propertyObj is PropertyInfo)
                        {
                            PropertyInfo property = propertyObj as PropertyInfo;
                            propertyName = property.Name;
                            propertyValue = property.CanRead ? property.GetValue(obj, null) : null;
                            ownerType = obj.GetType();
                            isReadOnly = (!property.CanWrite);
                        }
                        else if (propertyObj is DependencyProperty)
                        {
                            DependencyProperty dependencyProperty = propertyObj as DependencyProperty;
                            propertyName = dependencyProperty.Name;
                            propertyValue = dependencyProperties[dependencyProperty];
                            ownerType = dependencyProperty.OwnerType;
                            isReadOnly = ((dependencyProperty.DefaultMetadata.Options & DependencyPropertyOptions.ReadOnly) == DependencyPropertyOptions.ReadOnly);
                        }
                    }
                    catch (Exception e)
                    {
                        while (e is TargetInvocationException && e.InnerException != null)
                            e = e.InnerException;

                        serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString(SR.Error_SerializerPropertyGetFailed, propertyName, ownerType.FullName, e.Message)));
                        continue;
                    }

                    if (propertyObj is PropertyInfo && (PropertyInfo)propertyObj == contentProperty.Property)
                        continue;

                    if (propertyValue != null)
                    {
                        propertyValue = GetMarkupExtensionFromValue(propertyValue);

                        WorkflowMarkupSerializer propValueSerializer = serializationManager.GetSerializer(propertyValue.GetType(), typeof(WorkflowMarkupSerializer)) as WorkflowMarkupSerializer;
                        if (propValueSerializer != null)
                        {
                            using (new SafeXmlNodeWriter(serializationManager, obj, propertyObj, XmlNodeType.Element))
                            {
                                if (isReadOnly)
                                    propValueSerializer.SerializeContents(serializationManager, propertyValue, writer, false);
                                else
                                    propValueSerializer.SerializeObject(serializationManager, propertyValue, writer);
                            }
                        }
                        else
                        {
                            serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString(SR.Error_SerializerNotAvailableForSerialize, propertyValue.GetType().FullName)));
                        }
                    }
                }

                // serialize the contents
                try
                {
                    object contents = contentProperty.GetContents();
                    if (contents != null)
                    {
                        contents = GetMarkupExtensionFromValue(contents);

                        WorkflowMarkupSerializer propValueSerializer = serializationManager.GetSerializer(contents.GetType(), typeof(WorkflowMarkupSerializer)) as WorkflowMarkupSerializer;
                        if (propValueSerializer == null)
                        {
                            serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString(SR.Error_SerializerNotAvailableForSerialize, contents.GetType())));
                        }
                        else
                        {
                            //



                            //NOTE: THE FOLLOWING CONDITION ABOUT contentProperty.Property.PropertyType != typeof(object) is VALID AS WE SHOULD NOT SERIALIZE A PROPERTY OF TYPE OBJECT TO STRING
                            //IF WE DO THAT THEN WE DO NOT KNOWN WHAT WAS THE TYPE OF ORIGINAL OBJECT AND SERIALIZER WONT BE ABLE TO GET THE STRING BACK INTO THE CORRECT TYPE,
                            //AS THE TYPE INFORMATION IS LOST
                            if (propValueSerializer.CanSerializeToString(serializationManager, contents) &&
                                (contentProperty.Property == null || contentProperty.Property.PropertyType != typeof(object)))
                            {
                                string stringValue = propValueSerializer.SerializeToString(serializationManager, contents);
                                if (!string.IsNullOrEmpty(stringValue))
                                {
                                    stringValue = stringValue.Replace('\0', ' ');
                                    if (contents is MarkupExtension || !(stringValue.StartsWith("{", StringComparison.Ordinal) && stringValue.EndsWith("}", StringComparison.Ordinal)))
                                        writer.WriteString(stringValue);
                                    else
                                        writer.WriteString("{}" + stringValue);
                                }
                            }
                            else if (CollectionMarkupSerializer.IsValidCollectionType(contents.GetType()))
                            {
                                if (contentProperty.Property == null)
                                {
                                    IEnumerable enumerableContents = contents as IEnumerable;
                                    foreach (object childObj in enumerableContents)
                                    {
                                        if (childObj == null)
                                        {
                                            SerializeObject(serializationManager, new NullExtension(), writer);
                                        }
                                        else
                                        {
                                            object childObj2 = childObj;
                                            bool dictionaryEntry = (childObj2 is DictionaryEntry);
                                            try
                                            {
                                                if (dictionaryEntry)
                                                {
                                                    serializationManager.WorkflowMarkupStack.Push(childObj);
                                                    childObj2 = ((DictionaryEntry)childObj2).Value;
                                                }
                                                childObj2 = GetMarkupExtensionFromValue(childObj2);
                                                WorkflowMarkupSerializer childObjectSerializer = serializationManager.GetSerializer(childObj2.GetType(), typeof(WorkflowMarkupSerializer)) as WorkflowMarkupSerializer;
                                                if (childObjectSerializer != null)
                                                    childObjectSerializer.SerializeObject(serializationManager, childObj2, writer);
                                                else
                                                    serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString(SR.Error_SerializerNotAvailableForSerialize, childObj2.GetType())));
                                            }
                                            finally
                                            {
                                                if (dictionaryEntry)
                                                    serializationManager.WorkflowMarkupStack.Pop();
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    propValueSerializer.SerializeContents(serializationManager, contents, writer, false);
                                }
                            }
                            else
                            {
                                propValueSerializer.SerializeObject(serializationManager, contents, writer);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString(SR.Error_SerializerThrewException, obj.GetType().FullName, e.Message), e));
                    return;
                }
            }

            try
            {
                serializer.OnAfterSerialize(serializationManager, obj);
            }
            catch (Exception e)
            {
                serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString(SR.Error_SerializerThrewException, obj.GetType().FullName, e.Message), e));
                return;
            }
        }
        #endregion

        #region Overridable Methods
        protected virtual void OnBeforeSerialize(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
        }

        internal virtual void OnBeforeSerializeContents(WorkflowMarkupSerializationManager serializationManager, object obj)
        {

        }

        protected virtual void OnAfterSerialize(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
        }

        protected virtual void OnBeforeDeserialize(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
        }

        internal virtual void OnBeforeDeserializeContents(WorkflowMarkupSerializationManager serializationManager, object obj)
        {

        }

        protected virtual void OnAfterDeserialize(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
        }

        protected internal virtual bool ShouldSerializeValue(WorkflowMarkupSerializationManager serializationManager, object value)
        {
            if (serializationManager == null)
                throw new ArgumentNullException("serializationManager");

            if (value == null)
                return false;

            try
            {
                PropertyInfo property = serializationManager.Context.Current as PropertyInfo;
                if (property != null)
                {
                    Attribute[] attributes = Attribute.GetCustomAttributes(property, typeof(DefaultValueAttribute), true);
                    if (attributes.Length > 0)
                    {
                        DefaultValueAttribute defaultValueAttr = attributes[0] as DefaultValueAttribute;
                        if (defaultValueAttr.Value is IConvertible && value is IConvertible && object.Equals(Convert.ChangeType(defaultValueAttr.Value, property.PropertyType, CultureInfo.InvariantCulture), Convert.ChangeType(value, property.PropertyType, CultureInfo.InvariantCulture)))
                            return false;
                    }
                }
            }
            catch
            {
                //We purposely eat all the exceptions as Convert.ChangeType can throw but in that case
                //we continue with serialization
            }

            return true;
        }

        protected internal virtual bool CanSerializeToString(WorkflowMarkupSerializationManager serializationManager, object value)
        {
            if (serializationManager == null)
                throw new ArgumentNullException("serializationManager");
            if (value == null)
                throw new ArgumentNullException("value");

            Type valueType = value.GetType();
            if (valueType.IsPrimitive || valueType == typeof(System.String) || valueType.IsEnum
                || typeof(Delegate).IsAssignableFrom(valueType) || typeof(IConvertible).IsAssignableFrom(valueType)
                || valueType == typeof(TimeSpan) || valueType == typeof(Guid) || valueType == typeof(DateTime))
                return true;

            return false;
        }

        protected internal virtual string SerializeToString(WorkflowMarkupSerializationManager serializationManager, object value)
        {
            if (serializationManager == null)
                throw new ArgumentNullException("serializationManager");
            if (value == null)
                throw new ArgumentNullException("value");

            if (typeof(Delegate).IsAssignableFrom(value.GetType()))
                return ((Delegate)value).Method.Name;
            else if (typeof(DateTime).IsAssignableFrom(value.GetType()))
                return ((DateTime)value).ToString("o", CultureInfo.InvariantCulture);
            else
                return Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        protected internal virtual object DeserializeFromString(WorkflowMarkupSerializationManager serializationManager, Type propertyType, string value)
        {
            return InternalDeserializeFromString(serializationManager, propertyType, value);
        }

        private object InternalDeserializeFromString(WorkflowMarkupSerializationManager serializationManager, Type propertyType, string value)
        {
            if (serializationManager == null)
                throw new ArgumentNullException("serializationManager");
            if (propertyType == null)
                throw new ArgumentNullException("propertyType");
            if (value == null)
                throw new ArgumentNullException("value");

            object propVal = null;
            XmlReader reader = serializationManager.WorkflowMarkupStack[typeof(XmlReader)] as XmlReader;
            if (reader == null)
            {
                Debug.Assert(false, "XmlReader not available.");
                return null;
            }
            if (IsValidCompactAttributeFormat(value))
            {
                propVal = DeserializeFromCompactFormat(serializationManager, reader, value);
            }
            else
            {
                if (value.StartsWith("{}", StringComparison.Ordinal))
                    value = value.Substring(2);
                // Check for Nullable types
                if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    Type genericType = (Type)propertyType.GetGenericArguments()[0];
                    Debug.Assert(genericType != null);
                    propertyType = genericType;
                }
                if (propertyType.IsPrimitive || propertyType == typeof(System.String))
                {
                    propVal = Convert.ChangeType(value, propertyType, CultureInfo.InvariantCulture);
                }
                else if (propertyType.IsEnum)
                {
                    propVal = Enum.Parse(propertyType, value, true);
                }
                else if (typeof(Delegate).IsAssignableFrom(propertyType))
                {
                    // Just return the method name.  This must happen after Bind syntax has been checked.
                    propVal = value;
                }
                else if (typeof(TimeSpan) == propertyType)
                {
                    propVal = TimeSpan.Parse(value, CultureInfo.InvariantCulture);
                }
                else if (typeof(DateTime) == propertyType)
                {
                    propVal = DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                }
                else if (typeof(Guid) == propertyType)
                {
                    propVal = Utility.CreateGuid(value);
                }
                else if (typeof(Type).IsAssignableFrom(propertyType))
                {
                    propVal = serializationManager.GetType(value);
                    if (propVal != null)
                    {
                        Type type = propVal as Type;
                        if (type.IsPrimitive || type.IsEnum || type == typeof(System.String))
                            return type;
                    }
                    ITypeProvider typeProvider = serializationManager.GetService(typeof(ITypeProvider)) as ITypeProvider;
                    if (typeProvider != null)
                    {
                        Type type = typeProvider.GetType(value);
                        if (type != null)
                            return type;
                    }
                    return value;
                }
                else if (typeof(IConvertible).IsAssignableFrom(propertyType))
                {
                    propVal = Convert.ChangeType(value, propertyType, CultureInfo.InvariantCulture);
                }
                else if (propertyType.IsAssignableFrom(value.GetType()))
                {
                    propVal = value;
                }
                else
                {
                    throw CreateSerializationError(SR.GetString(SR.Error_SerializerPrimitivePropertyNoLogic, new object[] { "", value.Trim(), "" }), reader);
                }
            }
            return propVal;
        }

        protected internal virtual IList GetChildren(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            if (serializationManager == null)
                throw new ArgumentNullException("serializationManager");
            if (obj == null)
                throw new ArgumentNullException("obj");

            return null;
        }

        protected internal virtual void ClearChildren(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            if (serializationManager == null)
                throw new ArgumentNullException("serializationManager");
            if (obj == null)
                throw new ArgumentNullException("obj");
        }

        protected internal virtual void AddChild(WorkflowMarkupSerializationManager serializationManager, object parentObject, object childObj)
        {
            if (serializationManager == null)
                throw new ArgumentNullException("serializationManager");
            if (parentObject == null)
                throw new ArgumentNullException("parentObject");
            if (childObj == null)
                throw new ArgumentNullException("childObj");

            throw new Exception(SR.GetString(SR.Error_SerializerNoChildNotion, new object[] { parentObject.GetType().FullName }));
        }

        protected virtual object CreateInstance(WorkflowMarkupSerializationManager serializationManager, Type type)
        {
            if (serializationManager == null)
                throw new ArgumentNullException("serializationManager");
            if (type == null)
                throw new ArgumentNullException("type");

            return Activator.CreateInstance(type);
        }

        protected internal virtual PropertyInfo[] GetProperties(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            if (serializationManager == null)
                throw new ArgumentNullException("serializationManager");

            List<PropertyInfo> properties = new List<PropertyInfo>();

            object[] attributes = obj.GetType().GetCustomAttributes(typeof(RuntimeNamePropertyAttribute), true);
            string name = null;
            if (attributes.Length > 0)
                name = (attributes[0] as RuntimeNamePropertyAttribute).Name;

            foreach (PropertyInfo property in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy))
            {
                DesignerSerializationVisibility visibility = Helpers.GetSerializationVisibility(property);
                if (visibility == DesignerSerializationVisibility.Hidden)
                    continue;

                if (visibility != DesignerSerializationVisibility.Content && (!property.CanWrite || property.GetSetMethod() == null))
                {
                    // work around for CodeObject which are ICollection needs to be serialized.
                    if (!(obj is CodeObject) || !typeof(ICollection).IsAssignableFrom(property.PropertyType))
                        continue;
                }

                TypeProvider typeProvider = serializationManager.GetService(typeof(ITypeProvider)) as TypeProvider;

                if (typeProvider != null)
                {
                    if (!typeProvider.IsSupportedProperty(property, obj))
                    {
                        continue;
                    }
                }

                if (name == null || !name.Equals(property.Name))
                    properties.Add(property);
                else
                    properties.Add(new ExtendedPropertyInfo(property, OnGetRuntimeNameValue, OnSetRuntimeNameValue, OnGetRuntimeQualifiedName));
            }

            return properties.ToArray();
        }

        protected internal virtual EventInfo[] GetEvents(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            if (serializationManager == null)
                throw new ArgumentNullException("serializationManager");

            List<EventInfo> events = new List<EventInfo>();
            foreach (EventInfo evt in obj.GetType().GetEvents(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy))
            {
                if (Helpers.GetSerializationVisibility(evt) == DesignerSerializationVisibility.Hidden)
                    continue;

                events.Add(evt);
            }

            return events.ToArray();
        }

        internal virtual ExtendedPropertyInfo[] GetExtendedProperties(WorkflowMarkupSerializationManager manager, object extendee)
        {
            return new ExtendedPropertyInfo[0];
        }
        private object OnGetRuntimeNameValue(ExtendedPropertyInfo extendedProperty, object extendee)
        {
            return extendedProperty.RealPropertyInfo.GetValue(extendee, null);
        }

        private void OnSetRuntimeNameValue(ExtendedPropertyInfo extendedProperty, object extendee, object value)
        {
            if (extendee != null && value != null)
                extendedProperty.RealPropertyInfo.SetValue(extendee, value, null);
        }

        private XmlQualifiedName OnGetRuntimeQualifiedName(ExtendedPropertyInfo extendedProperty, WorkflowMarkupSerializationManager manager, out string prefix)
        {
            prefix = StandardXomlKeys.Definitions_XmlNs_Prefix;
            return new XmlQualifiedName(extendedProperty.Name, StandardXomlKeys.Definitions_XmlNs);
        }

        #endregion

        #region Dependency Properties
        // 

        private IDictionary<DependencyProperty, object> GetDependencyProperties(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            if (serializationManager == null)
                throw new ArgumentNullException("serializationManager");
            if (obj == null)
                throw new ArgumentNullException("obj");

            List<PropertyInfo> pis = new List<PropertyInfo>();
            pis.AddRange(GetProperties(serializationManager, obj));
            List<EventInfo> eis = new List<EventInfo>();
            eis.AddRange(GetEvents(serializationManager, obj));

            Dictionary<DependencyProperty, object> dependencyProperties = new Dictionary<DependencyProperty, object>();
            DependencyObject dependencyObject = obj as DependencyObject;
            if (dependencyObject != null)
            {
                foreach (DependencyProperty dependencyProperty in dependencyObject.MetaDependencyProperties)
                {
                    Attribute[] visibilityAttrs = dependencyProperty.DefaultMetadata.GetAttributes(typeof(DesignerSerializationVisibilityAttribute));
                    if (visibilityAttrs.Length > 0 && ((DesignerSerializationVisibilityAttribute)visibilityAttrs[0]).Visibility == DesignerSerializationVisibility.Hidden)
                        continue;

                    //If the dependency property is readonly and we have not marked it with DesignerSerializationVisibility.Content attribute the we should not
                    //serialize it
                    if ((dependencyProperty.DefaultMetadata.Options & DependencyPropertyOptions.ReadOnly) == DependencyPropertyOptions.ReadOnly)
                    {
                        object[] serializationVisibilityAttribute = dependencyProperty.DefaultMetadata.GetAttributes(typeof(DesignerSerializationVisibilityAttribute));
                        if (serializationVisibilityAttribute == null ||
                            serializationVisibilityAttribute.Length == 0 ||
                            ((DesignerSerializationVisibilityAttribute)serializationVisibilityAttribute[0]).Visibility != DesignerSerializationVisibility.Content)
                        {
                            continue;
                        }
                    }

                    object obj1 = null;
                    if (!dependencyProperty.IsAttached && !dependencyProperty.DefaultMetadata.IsMetaProperty)
                    {
                        if (dependencyProperty.IsEvent)
                            obj1 = LookupEvent(eis, dependencyProperty.Name);
                        else
                            obj1 = LookupProperty(pis, dependencyProperty.Name);
                        if (obj1 == null)
                        {
                            serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString(SR.Error_MissingCLRProperty, dependencyProperty.Name, obj.GetType().FullName)));
                            continue;
                        }
                    }
                    if (dependencyObject.IsBindingSet(dependencyProperty))
                    {
                        dependencyProperties.Add(dependencyProperty, dependencyObject.GetBinding(dependencyProperty));
                    }
                    else if (!dependencyProperty.IsEvent)
                    {
                        object propValue = null;
                        propValue = dependencyObject.GetValue(dependencyProperty);
                        if (!dependencyProperty.IsAttached && !dependencyProperty.DefaultMetadata.IsMetaProperty)
                        {
                            PropertyInfo propertyInfo = obj1 as PropertyInfo;
                            // if the propertyValue is assignable to the type in of the .net property then call the .net property's getter also
                            // else add the keep the value that we got 
                            if (propValue != null && propertyInfo.PropertyType.IsAssignableFrom(propValue.GetType()))
                                propValue = (obj1 as PropertyInfo).GetValue(dependencyObject, null);
                        }
                        dependencyProperties.Add(dependencyProperty, propValue);
                    }
                    else
                    {
                        dependencyProperties.Add(dependencyProperty, dependencyObject.GetHandler(dependencyProperty));
                    }
                }
                foreach (DependencyProperty dependencyProperty in dependencyObject.DependencyPropertyValues.Keys)
                {
                    Attribute[] visibilityAttrs = dependencyProperty.DefaultMetadata.GetAttributes(typeof(DesignerSerializationVisibilityAttribute));
                    if (visibilityAttrs.Length > 0 && ((DesignerSerializationVisibilityAttribute)visibilityAttrs[0]).Visibility == DesignerSerializationVisibility.Hidden)
                        continue;

                    if (!dependencyProperty.DefaultMetadata.IsMetaProperty && dependencyProperty.IsAttached && VerifyAttachedPropertyConditions(dependencyProperty))
                        dependencyProperties.Add(dependencyProperty, dependencyObject.GetValue(dependencyProperty));
                }
            }
            return dependencyProperties;
        }

        private static bool VerifyAttachedPropertyConditions(DependencyProperty dependencyProperty)
        {
            if (dependencyProperty.IsEvent)
            {
                if (dependencyProperty.OwnerType.GetField(dependencyProperty.Name + "Event", BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly) == null)
                    return false;
                MethodInfo methodInfo = dependencyProperty.OwnerType.GetMethod("Add" + dependencyProperty.Name + "Handler", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
                if (methodInfo == null)
                    return false;
                ParameterInfo[] parameters = methodInfo.GetParameters();
                if (parameters != null && parameters.Length == 2 && parameters[0].ParameterType == typeof(object) && parameters[1].ParameterType == typeof(object))
                    return true;
            }
            else
            {
                if (dependencyProperty.OwnerType.GetField(dependencyProperty.Name + "Property", BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly) == null)
                    return false;
                MethodInfo methodInfo = dependencyProperty.OwnerType.GetMethod("Set" + dependencyProperty.Name, BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
                if (methodInfo == null)
                    return false;
                ParameterInfo[] parameters = methodInfo.GetParameters();
                if (parameters != null && parameters.Length == 2 && parameters[0].ParameterType == typeof(object) && parameters[1].ParameterType == typeof(object))
                    return true;
            }
            return false;
        }

        private void SetDependencyPropertyValue(WorkflowMarkupSerializationManager serializationManager, object obj, DependencyProperty dependencyProperty, object value)
        {
            if (dependencyProperty == null)
                throw new ArgumentNullException("dependencyProperty");
            if (obj == null)
                throw new ArgumentNullException("obj");

            DependencyObject dependencyObject = obj as DependencyObject;
            if (dependencyObject == null)
                throw new ArgumentException(SR.GetString(SR.Error_InvalidArgumentValue), "obj");

            if (dependencyProperty.IsEvent)
            {
                if (value is ActivityBind)
                    dependencyObject.SetBinding(dependencyProperty, value as ActivityBind);
                else if (dependencyProperty.IsAttached)
                {
                    MethodInfo methodInfo = dependencyProperty.OwnerType.GetMethod("Add" + dependencyProperty.Name + "Handler", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
                    if (methodInfo != null)
                    {
                        ParameterInfo[] parameters = methodInfo.GetParameters();
                        if (parameters == null || parameters.Length != 2 || parameters[0].ParameterType != typeof(object) || parameters[1].ParameterType != typeof(object))
                            methodInfo = null;
                    }
                    if (methodInfo != null)
                        WorkflowMarkupSerializationHelpers.SetEventHandlerName(dependencyObject, dependencyProperty.Name, value as string);
                    else
                        serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString(SR.Error_MissingAddHandler, dependencyProperty.Name, dependencyProperty.OwnerType.FullName)));
                }
                else
                    WorkflowMarkupSerializationHelpers.SetEventHandlerName(dependencyObject, dependencyProperty.Name, value as string);
            }
            else
            {
                if (value is ActivityBind)
                    dependencyObject.SetBinding(dependencyProperty, value as ActivityBind);
                else if (value is string && TypeProvider.IsAssignable(typeof(Type), dependencyProperty.PropertyType))
                    Helpers.SetDesignTimeTypeName(obj, dependencyProperty, value as string);
                else if (dependencyProperty.IsAttached)
                {
                    MethodInfo methodInfo = dependencyProperty.OwnerType.GetMethod("Set" + dependencyProperty.Name, BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
                    if (methodInfo != null)
                    {
                        ParameterInfo[] parameters = methodInfo.GetParameters();
                        if (parameters == null || parameters.Length != 2 || parameters[0].ParameterType != typeof(object) || parameters[1].ParameterType != typeof(object))
                            methodInfo = null;
                    }
                    if (methodInfo != null)
                        methodInfo.Invoke(null, new object[] { dependencyObject, value });
                    else
                        serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString(SR.Error_MissingSetAccessor, dependencyProperty.Name, dependencyProperty.OwnerType.FullName)));
                }
                else
                {
                    List<PropertyInfo> pis = new List<PropertyInfo>();
                    pis.AddRange(GetProperties(serializationManager, obj));
                    //The following condition is workaround for the partner team as they depend on the dependencyObject.SetValue being called
                    //for non assignable property values
                    PropertyInfo pi = LookupProperty(pis, dependencyProperty.Name);
                    if (pi != null &&
                        (value == null || pi.PropertyType.IsAssignableFrom(value.GetType())))
                    {
                        if (pi.CanWrite)
                        {
                            pi.SetValue(obj, value, null);
                        }
                        else if (typeof(ICollection<string>).IsAssignableFrom(value.GetType()))
                        {
                            ICollection<string> propVal = pi.GetValue(obj, null) as ICollection<string>;
                            ICollection<string> deserializedValue = value as ICollection<string>;
                            if (propVal != null && deserializedValue != null)
                            {
                                foreach (string content in deserializedValue)
                                    propVal.Add(content);
                            }
                        }
                    }
                    else
                    {
                        dependencyObject.SetValue(dependencyProperty, value);
                    }
                }
            }
        }
        #endregion

        #region Private Helpers

        #region Reader/Writer
        private void AdvanceReader(XmlReader reader)
        {
            //Compressed what process mapping pi used to do
            while (reader.NodeType != XmlNodeType.EndElement && reader.NodeType != XmlNodeType.Element && reader.NodeType != XmlNodeType.Text && reader.Read());
        }
        #endregion

        #region Exception Handling
        internal static WorkflowMarkupSerializationException CreateSerializationError(Exception e, XmlReader reader)
        {
            return CreateSerializationError(null, e, reader);
        }
        internal static WorkflowMarkupSerializationException CreateSerializationError(string message, XmlReader reader)
        {
            return CreateSerializationError(message, null, reader);
        }
        internal static WorkflowMarkupSerializationException CreateSerializationError(string message, Exception e, XmlReader reader)
        {
            string errorMsg = message;
            if (string.IsNullOrEmpty(errorMsg))
                errorMsg = e.Message;

            IXmlLineInfo xmlLineInfo = reader as IXmlLineInfo;
            if (xmlLineInfo != null)
                return new WorkflowMarkupSerializationException(errorMsg, xmlLineInfo.LineNumber, xmlLineInfo.LinePosition);
            else
                return new WorkflowMarkupSerializationException(errorMsg, 0, 0);
        }
        #endregion

        #region Type Creation Support
        private static string GetClrFullName(WorkflowMarkupSerializationManager serializationManager, XmlQualifiedName xmlQualifiedName)
        {
            string xmlns = xmlQualifiedName.Namespace;
            string typeName = xmlQualifiedName.Name;

            List<WorkflowMarkupSerializerMapping> xmlnsMappings = null;
            if (!serializationManager.XmlNamespaceBasedMappings.TryGetValue(xmlns, out xmlnsMappings) || xmlnsMappings.Count == 0)
                return xmlQualifiedName.Namespace + "." + xmlQualifiedName.Name;

            WorkflowMarkupSerializerMapping xmlnsMapping = xmlnsMappings[0];

            string assemblyName = xmlnsMapping.AssemblyName;
            string dotNetnamespaceName = xmlnsMapping.ClrNamespace;

            // append dot net namespace name
            string fullTypeName = xmlQualifiedName.Name;
            if (dotNetnamespaceName.Length > 0)
                fullTypeName = (dotNetnamespaceName + "." + xmlQualifiedName.Name);

            return fullTypeName;
        }

        private object CreateInstance(WorkflowMarkupSerializationManager serializationManager, XmlQualifiedName xmlQualifiedName, XmlReader reader)
        {
            object obj = null;
            // resolve the type
            Type type = null;
            try
            {
                type = serializationManager.GetType(xmlQualifiedName);
            }
            catch (Exception e)
            {
                serializationManager.ReportError(CreateSerializationError(SR.GetString(SR.Error_SerializerTypeNotResolvedWithInnerError, new object[] { GetClrFullName(serializationManager, xmlQualifiedName), e.Message }), e, reader));
                return null;
            }
            if (type == null && !xmlQualifiedName.Name.EndsWith("Extension", StringComparison.Ordinal))
            {
                string typename = xmlQualifiedName.Name + "Extension";
                try
                {
                    type = serializationManager.GetType(new XmlQualifiedName(typename, xmlQualifiedName.Namespace));
                }
                catch (Exception e)
                {
                    serializationManager.ReportError(CreateSerializationError(SR.GetString(SR.Error_SerializerTypeNotResolvedWithInnerError, new object[] { GetClrFullName(serializationManager, xmlQualifiedName), e.Message }), e, reader));
                    return null;
                }
            }

            if (type == null)
            {
                serializationManager.ReportError(CreateSerializationError(SR.GetString(SR.Error_SerializerTypeNotResolved, new object[] { GetClrFullName(serializationManager, xmlQualifiedName) }), reader));
                return null;
            }

            if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal) || type == typeof(DateTime) ||
                type == typeof(TimeSpan) || type.IsEnum || type == typeof(Guid))
            {
                try
                {
                    string stringValue = reader.ReadString();
                    if (type == typeof(DateTime))
                    {
                        obj = DateTime.Parse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                    }
                    else if (type.IsPrimitive || type == typeof(decimal) || type == typeof(TimeSpan) || type.IsEnum || type == typeof(Guid))
                    {
                        //These non CLS-compliant are not supported in the XmlReader 
                        TypeConverter typeConverter = TypeDescriptor.GetConverter(type);
                        if (typeConverter != null && typeConverter.CanConvertFrom(typeof(string)))
                            obj = typeConverter.ConvertFrom(null, CultureInfo.InvariantCulture, stringValue);
                        else if (typeof(IConvertible).IsAssignableFrom(type))
                            obj = Convert.ChangeType(stringValue, type, CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        obj = stringValue;
                    }
                }
                catch (Exception e)
                {
                    serializationManager.ReportError(CreateSerializationError(SR.GetString(SR.Error_SerializerCreateInstanceFailed, e.Message), reader));
                    return null;
                }
            }
            else
            {
                // get the serializer
                WorkflowMarkupSerializer serializer = serializationManager.GetSerializer(type, typeof(WorkflowMarkupSerializer)) as WorkflowMarkupSerializer;
                if (serializer == null)
                {
                    serializationManager.ReportError(CreateSerializationError(SR.GetString(SR.Error_SerializerNotAvailable, type.FullName), reader));
                    return null;
                }

                // create an instance
                try
                {
                    obj = serializer.CreateInstance(serializationManager, type);
                }
                catch (Exception e)
                {
                    serializationManager.ReportError(CreateSerializationError(SR.GetString(SR.Error_SerializerCreateInstanceFailed, type.FullName, e.Message), reader));
                    return null;
                }
            }
            return obj;
        }
        #endregion

        #region Simple and Complex property Serialization Support
        private void DeserializeCompoundProperty(WorkflowMarkupSerializationManager serializationManager, XmlReader reader, object obj)
        {
            string propertyName = reader.LocalName;
            bool isReadOnly = false;

            DependencyProperty dependencyProperty = serializationManager.Context.Current as DependencyProperty;
            PropertyInfo property = serializationManager.Context.Current as PropertyInfo;
            if (dependencyProperty != null)
                isReadOnly = ((dependencyProperty.DefaultMetadata.Options & DependencyPropertyOptions.ReadOnly) == DependencyPropertyOptions.ReadOnly);
            else if (property != null)
                isReadOnly = !property.CanWrite;
            else
            {
                Debug.Assert(false);
                return;
            }

            //Deserialize compound properties
            if (isReadOnly)
            {
                object propValue = null;
                if (dependencyProperty != null && obj is DependencyObject)
                {
                    if (((DependencyObject)obj).IsBindingSet(dependencyProperty))
                        propValue = ((DependencyObject)obj).GetBinding(dependencyProperty);
                    else if (!dependencyProperty.IsEvent)
                        propValue = ((DependencyObject)obj).GetValue(dependencyProperty);
                    else
                        propValue = ((DependencyObject)obj).GetHandler(dependencyProperty);
                }
                else if (property != null)
                    propValue = property.CanRead ? property.GetValue(obj, null) : null;

                if (propValue != null)
                    DeserializeContents(serializationManager, propValue, reader);
                else
                    serializationManager.ReportError(CreateSerializationError(SR.GetString(SR.Error_SerializerReadOnlyPropertyAndValueIsNull, propertyName, obj.GetType().FullName), reader));
            }
            else if (!reader.IsEmptyElement)
            {
                //
                if (reader.HasAttributes)
                {
                    //We allow xmlns on the complex property nodes
                    while (reader.MoveToNextAttribute())
                    {
                        // 
                        if (string.Equals(reader.LocalName, "xmlns", StringComparison.Ordinal) || string.Equals(reader.Prefix, "xmlns", StringComparison.Ordinal))
                            continue;
                        else
                            serializationManager.ReportError(CreateSerializationError(SR.GetString(SR.Error_SerializerAttributesFoundInComplexProperty, propertyName, obj.GetType().FullName), reader));
                    }
                }

                do
                {
                    if (!reader.Read())
                        return;
                } while (reader.NodeType != XmlNodeType.Text && reader.NodeType != XmlNodeType.Element && reader.NodeType != XmlNodeType.ProcessingInstruction && reader.NodeType != XmlNodeType.EndElement);

                if (reader.NodeType == XmlNodeType.Text)
                {
                    this.DeserializeSimpleProperty(serializationManager, reader, obj, reader.Value);
                }
                else
                {
                    AdvanceReader(reader);
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        object propValue = DeserializeObject(serializationManager, reader);
                        if (propValue != null)
                        {
                            propValue = GetValueFromMarkupExtension(serializationManager, propValue);

                            if (propValue != null && propValue.GetType() == typeof(string) && ((string)propValue).StartsWith("{}", StringComparison.Ordinal))
                                propValue = ((string)propValue).Substring(2);

                            if (dependencyProperty != null)
                            {
                                //Get the serializer for the property type
                                WorkflowMarkupSerializer objSerializer = serializationManager.GetSerializer(obj.GetType(), typeof(WorkflowMarkupSerializer)) as WorkflowMarkupSerializer;
                                if (objSerializer == null)
                                {
                                    serializationManager.ReportError(CreateSerializationError(SR.GetString(SR.Error_SerializerNotAvailable, obj.GetType().FullName), reader));
                                    return;
                                }

                                try
                                {
                                    objSerializer.SetDependencyPropertyValue(serializationManager, obj, dependencyProperty, propValue);
                                }
                                catch (Exception e)
                                {
                                    serializationManager.ReportError(CreateSerializationError(SR.GetString(SR.Error_SerializerThrewException, obj.GetType().FullName, e.Message), e, reader));
                                    return;
                                }
                            }
                            else if (property != null)
                            {
                                try
                                {
                                    property.SetValue(obj, propValue, null);
                                }
                                catch
                                {
                                    serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString(SR.Error_SerializerComplexPropertySetFailed, new object[] { propertyName, propertyName, obj.GetType().Name })));
                                }
                            }
                        }
                    }
                }
            }
        }

        private void DeserializeSimpleProperty(WorkflowMarkupSerializationManager serializationManager, XmlReader reader, object obj, string value)
        {
            Type propertyType = null;
            bool isReadOnly = false;

            DependencyProperty dependencyProperty = serializationManager.Context.Current as DependencyProperty;
            PropertyInfo property = serializationManager.Context.Current as PropertyInfo;
            if (dependencyProperty != null)
            {
                propertyType = dependencyProperty.PropertyType;
                isReadOnly = ((dependencyProperty.DefaultMetadata.Options & DependencyPropertyOptions.ReadOnly) == DependencyPropertyOptions.ReadOnly);
            }
            else if (property != null)
            {
                propertyType = property.PropertyType;
                isReadOnly = !property.CanWrite;
            }
            else
            {
                Debug.Assert(false);
                return;
            }

            if (isReadOnly && !typeof(ICollection<string>).IsAssignableFrom(propertyType))
            {
                serializationManager.ReportError(CreateSerializationError(SR.GetString(SR.Error_SerializerPrimitivePropertyReadOnly, new object[] { property.Name, property.Name, obj.GetType().FullName }), reader));
                return;
            }

            DeserializeSimpleMember(serializationManager, propertyType, reader, obj, value);
        }

        private void DeserializeEvent(WorkflowMarkupSerializationManager serializationManager, XmlReader reader, object obj, string value)
        {
            Type eventType = null;

            EventInfo evt = serializationManager.Context.Current as EventInfo;
            DependencyProperty dependencyEvent = serializationManager.Context.Current as DependencyProperty;
            if (dependencyEvent != null)
                eventType = dependencyEvent.PropertyType;
            else if (evt != null)
                eventType = evt.EventHandlerType;
            else
            {
                Debug.Assert(false);
                return;
            }

            DeserializeSimpleMember(serializationManager, eventType, reader, obj, value);
        }

        private void DeserializeSimpleMember(WorkflowMarkupSerializationManager serializationManager, Type memberType, XmlReader reader, object obj, string value)
        {
            //Get the serializer for the member type
            WorkflowMarkupSerializer memberSerializer = serializationManager.GetSerializer(memberType, typeof(WorkflowMarkupSerializer)) as WorkflowMarkupSerializer;
            if (memberSerializer == null)
            {
                serializationManager.ReportError(CreateSerializationError(SR.GetString(SR.Error_SerializerNotAvailable, memberType.FullName), reader));
                return;
            }

            //Try to deserialize
            object memberValue = null;
            try
            {
                memberValue = memberSerializer.DeserializeFromString(serializationManager, memberType, value);
                memberValue = GetValueFromMarkupExtension(serializationManager, memberValue);

                DependencyProperty dependencyProperty = serializationManager.Context.Current as DependencyProperty;
                if (dependencyProperty != null)
                {
                    //Get the serializer for the property type
                    WorkflowMarkupSerializer objSerializer = serializationManager.GetSerializer(obj.GetType(), typeof(WorkflowMarkupSerializer)) as WorkflowMarkupSerializer;
                    if (objSerializer == null)
                    {
                        serializationManager.ReportError(CreateSerializationError(SR.GetString(SR.Error_SerializerNotAvailable, obj.GetType().FullName), reader));
                        return;
                    }

                    objSerializer.SetDependencyPropertyValue(serializationManager, obj, dependencyProperty, memberValue);
                }
                else
                {
                    EventInfo evt = serializationManager.Context.Current as EventInfo;
                    if (evt != null)
                    {
                        try
                        {
                            WorkflowMarkupSerializationHelpers.SetEventHandlerName(obj, evt.Name, memberValue as string);
                        }
                        catch (Exception e)
                        {
                            while (e is TargetInvocationException && e.InnerException != null)
                                e = e.InnerException;
                            serializationManager.ReportError(CreateSerializationError(SR.GetString(SR.Error_SerializerMemberSetFailed, new object[] { reader.LocalName, reader.Value, reader.LocalName, obj.GetType().FullName, e.Message }), e, reader));
                        }
                    }
                    else
                    {
                        PropertyInfo property = serializationManager.Context.Current as PropertyInfo;
                        if (property != null)
                        {
                            try
                            {
                                if (memberValue is string && TypeProvider.IsAssignable(typeof(Type), property.PropertyType))
                                {
                                    string key = property.ReflectedType.FullName + "." + property.Name;
                                    Helpers.SetDesignTimeTypeName(obj, key, memberValue as string);
                                }
                                else if (property.CanWrite)
                                {
                                    property.SetValue(obj, memberValue, null);
                                }
                                else if (typeof(ICollection<string>).IsAssignableFrom(memberValue.GetType()))
                                {
                                    ICollection<string> propVal = property.GetValue(obj, null) as ICollection<string>;
                                    ICollection<string> deserializedValue = memberValue as ICollection<string>;
                                    if (propVal != null && deserializedValue != null)
                                    {
                                        foreach (string content in deserializedValue)
                                            propVal.Add(content);
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                while (e is TargetInvocationException && e.InnerException != null)
                                    e = e.InnerException;
                                serializationManager.ReportError(CreateSerializationError(SR.GetString(SR.Error_SerializerMemberSetFailed, new object[] { reader.LocalName, reader.Value, reader.LocalName, obj.GetType().FullName, e.Message }), e, reader));
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                while (e is TargetInvocationException && e.InnerException != null)
                    e = e.InnerException;
                serializationManager.ReportError(CreateSerializationError(SR.GetString(SR.Error_SerializerMemberSetFailed, new object[] { reader.LocalName, reader.Value, reader.LocalName, obj.GetType().FullName, e.Message }), e, reader));
            }
        }
        #endregion

        #region DependencyProperty Support

        [SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison", MessageId = "System.String.IndexOf(System.String)", Justification = "string comparisons are just used to lookup method/property names from compiled system.type which should not cause any issue irrespective of comparisons mode used")]
        private DependencyProperty ResolveDependencyProperty(WorkflowMarkupSerializationManager serializationManager, XmlReader reader, object attachedObj, string fullPropertyName)
        {
            Type ownerType = null;
            string propertyName = String.Empty;
            int separatorIndex = fullPropertyName.IndexOf(".");
            if (separatorIndex != -1)
            {
                string ownerTypeName = fullPropertyName.Substring(0, separatorIndex);
                propertyName = fullPropertyName.Substring(separatorIndex + 1);
                if (!String.IsNullOrEmpty(ownerTypeName) && !String.IsNullOrEmpty(propertyName))
                    ownerType = serializationManager.GetType(new XmlQualifiedName(ownerTypeName, reader.LookupNamespace(reader.Prefix)));
            }
            else
            {
                ownerType = attachedObj.GetType();
                propertyName = fullPropertyName;
            }

            if (ownerType == null)
                return null;

            //We need to make sure that the register method is always called for the dynamic property before we try to resolve it
            //In cases of attached properties if this statement is not there then the dynamic property wont be found as it is
            //not registered till the first access of the static field
            DependencyProperty dependencyProperty = null;

            FieldInfo fieldInfo = ownerType.GetField(propertyName + "Property", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            if (fieldInfo == null)
                fieldInfo = ownerType.GetField(propertyName + "Event", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            if (fieldInfo != null)
            {
                dependencyProperty = fieldInfo.GetValue(attachedObj) as DependencyProperty;
                if (dependencyProperty != null)
                {
                    object[] attributes = dependencyProperty.DefaultMetadata.GetAttributes(typeof(DesignerSerializationVisibilityAttribute));
                    if (attributes.Length > 0)
                    {
                        DesignerSerializationVisibilityAttribute serializationVisibilityAttribute = attributes[0] as DesignerSerializationVisibilityAttribute;
                        if (serializationVisibilityAttribute.Visibility == DesignerSerializationVisibility.Hidden)
                            dependencyProperty = null;
                    }
                }
            }

            return dependencyProperty;
        }
        #endregion

        #region SafeXmlNodeWriter
        private sealed class SafeXmlNodeWriter : IDisposable
        {
            private XmlNodeType xmlNodeType = XmlNodeType.None;
            private WorkflowMarkupSerializationManager serializationManager = null;

            public SafeXmlNodeWriter(WorkflowMarkupSerializationManager serializationManager, object owner, object property, XmlNodeType xmlNodeType)
            {
                this.serializationManager = serializationManager;
                this.xmlNodeType = xmlNodeType;

                XmlWriter writer = serializationManager.WorkflowMarkupStack[typeof(XmlWriter)] as XmlWriter;
                if (writer == null)
                    throw new InvalidOperationException(SR.GetString(SR.Error_InternalSerializerError));

                string prefix = String.Empty, tagName = String.Empty, xmlns = String.Empty;

                DependencyProperty dependencyProperty = property as DependencyProperty;
                if (dependencyProperty != null)
                {
                    if (!dependencyProperty.IsAttached && xmlNodeType == XmlNodeType.Attribute)
                    {
                        tagName = dependencyProperty.Name;
                        xmlns = String.Empty;
                    }
                    else
                    {
                        XmlQualifiedName qualifiedName = this.serializationManager.GetXmlQualifiedName(dependencyProperty.OwnerType, out prefix);
                        tagName = qualifiedName.Name + "." + dependencyProperty.Name;
                        xmlns = qualifiedName.Namespace;
                    }
                }
                else if (property is MemberInfo)
                {
                    ExtendedPropertyInfo extendedProperty = property as ExtendedPropertyInfo;
                    if (extendedProperty != null)
                    {
                        XmlQualifiedName qualifiedName = extendedProperty.GetXmlQualifiedName(this.serializationManager, out prefix);
                        tagName = qualifiedName.Name;
                        xmlns = qualifiedName.Namespace;
                    }
                    else if (this.xmlNodeType == XmlNodeType.Element)
                    {
                        XmlQualifiedName qualifiedName = this.serializationManager.GetXmlQualifiedName(owner.GetType(), out prefix);
                        tagName = qualifiedName.Name + "." + ((MemberInfo)property).Name;
                        xmlns = qualifiedName.Namespace;
                    }
                    else
                    {
                        tagName = ((MemberInfo)property).Name;
                        xmlns = String.Empty;
                    }
                }
                else
                {
                    XmlQualifiedName qualifiedName = this.serializationManager.GetXmlQualifiedName(owner.GetType(), out prefix);
                    tagName = qualifiedName.Name;
                    xmlns = qualifiedName.Namespace;
                }

                //verify the node name is valid. This may happen for design time names as 
                // "(Parameter) PropName"
                tagName = XmlConvert.EncodeName(tagName);

                if (this.xmlNodeType == XmlNodeType.Element)
                {
                    writer.WriteStartElement(prefix, tagName, xmlns);
                    this.serializationManager.WriterDepth += 1;
                }
                else if (this.xmlNodeType == XmlNodeType.Attribute)
                {
                    writer.WriteStartAttribute(prefix, tagName, xmlns);
                }
            }

            #region IDisposable Members
            void IDisposable.Dispose()
            {
                XmlWriter writer = this.serializationManager.WorkflowMarkupStack[typeof(XmlWriter)] as XmlWriter;
                if (writer != null && writer.WriteState != WriteState.Error)
                {
                    if (this.xmlNodeType == XmlNodeType.Element)
                    {
                        writer.WriteEndElement();
                        this.serializationManager.WriterDepth -= 1;
                    }
                    else if (writer.WriteState == WriteState.Attribute)
                    {
                        writer.WriteEndAttribute();
                    }
                }
            }
            #endregion
        }
        #endregion

        #region Property Info Lookup
        private static PropertyInfo LookupProperty(IList<PropertyInfo> properties, string propertyName)
        {
            if (properties != null && !string.IsNullOrEmpty(propertyName))
            {
                foreach (PropertyInfo property in properties)
                {
                    if (property.Name == propertyName)
                        return property;
                }
            }
            return null;
        }

        private static EventInfo LookupEvent(IList<EventInfo> events, string eventName)
        {
            if (events != null && !string.IsNullOrEmpty(eventName))
            {
                foreach (EventInfo evt in events)
                {
                    if (evt.Name == eventName)
                        return evt;
                }
            }
            return null;
        }
        #endregion

        #endregion

        #region Compact Attribute Support

        internal bool IsValidCompactAttributeFormat(string attributeValue)
        {
            return (attributeValue.Length > 0 && attributeValue.StartsWith("{", StringComparison.Ordinal) && !attributeValue.StartsWith("{}", StringComparison.Ordinal) && attributeValue.EndsWith("}", StringComparison.Ordinal));
        }

        // This function parses the data bind syntax (markup extension in xaml terms).  The syntax is:
        // {ObjectTypeName arg1, arg2, name3=arg3, name4=arg4, ...}
        // For example, an ActivityBind would have the syntax as the following:
        // {wcm:ActivityBind ID=Workflow1, Path=error1}
        // We also support positional arguments, so the above expression is equivalent to 
        // {wcm:ActivityBind Workflow1, Path=error1} or {wcm:ActivityBind Workflow1, error1}
        // Notice that the object must have the appropriate constructor to support positional arugments.
        // There should be no constructors that takes the same number of arugments, regardless of their types.
        internal object DeserializeFromCompactFormat(WorkflowMarkupSerializationManager serializationManager, XmlReader reader, string attrValue)
        {
            if (attrValue.Length == 0 || !attrValue.StartsWith("{", StringComparison.Ordinal) || !attrValue.EndsWith("}", StringComparison.Ordinal))
            {
                serializationManager.ReportError(CreateSerializationError(SR.GetString(SR.IncorrectSyntax, attrValue), reader));
                return null;
            }

            // check for correct format:  typename name=value name=value
            int argIndex = attrValue.IndexOf(" ", StringComparison.Ordinal);
            if (argIndex == -1)
                argIndex = attrValue.IndexOf("}", StringComparison.Ordinal);

            string typename = attrValue.Substring(1, argIndex - 1).Trim();
            string arguments = attrValue.Substring(argIndex + 1, attrValue.Length - (argIndex + 1));
            // lookup the type of the target
            string prefix = String.Empty;
            int typeIndex = typename.IndexOf(":", StringComparison.Ordinal);
            if (typeIndex >= 0)
            {
                prefix = typename.Substring(0, typeIndex);
                typename = typename.Substring(typeIndex + 1);
            }

            Type type = serializationManager.GetType(new XmlQualifiedName(typename, reader.LookupNamespace(prefix)));
            if (type == null && !typename.EndsWith("Extension", StringComparison.Ordinal))
            {
                typename = typename + "Extension";
                type = serializationManager.GetType(new XmlQualifiedName(typename, reader.LookupNamespace(prefix)));
            }
            if (type == null)
            {
                serializationManager.ReportError(CreateSerializationError(SR.GetString(SR.Error_MarkupSerializerTypeNotResolved, typename), reader));
                return null;
            }

            // Break apart the argument string.
            object obj = null;
            Dictionary<string, object> namedArgs = new Dictionary<string, object>();
            ArrayList argTokens = null;
            try
            {
                argTokens = TokenizeAttributes(serializationManager, arguments, (reader is IXmlLineInfo) ? ((IXmlLineInfo)reader).LineNumber : 1, (reader is IXmlLineInfo) ? ((IXmlLineInfo)reader).LinePosition : 1);
            }
            catch (Exception error)
            {
                serializationManager.ReportError(CreateSerializationError(SR.GetString(SR.Error_MarkupExtensionDeserializeFailed, attrValue, error.Message), reader));
                return null;
            }
            if (argTokens != null)
            {
                // Process the positional arugments and find the correct constructor to call.
                ArrayList positionalArgs = new ArrayList();
                bool firstEqual = true;
                for (int i = 0; i < argTokens.Count; i++)
                {
                    char token = (argTokens[i] is char) ? (char)argTokens[i] : '\0';
                    if (token == '=')
                    {
                        if (positionalArgs.Count > 0 && firstEqual)
                            positionalArgs.RemoveAt(positionalArgs.Count - 1);
                        firstEqual = false;
                        namedArgs.Add(argTokens[i - 1] as string, argTokens[i + 1] as string);
                        i++;
                    }
                    if (token == ',')
                        continue;

                    if (namedArgs.Count == 0)
                        positionalArgs.Add(argTokens[i] as string);
                }

                if (positionalArgs.Count > 0)
                {
                    ConstructorInfo matchConstructor = null;
                    ConstructorInfo[] constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                    ParameterInfo[] matchParameters = null;
                    foreach (ConstructorInfo ctor in constructors)
                    {
                        ParameterInfo[] parameters = ctor.GetParameters();
                        if (parameters.Length == positionalArgs.Count)
                        {
                            matchConstructor = ctor;
                            matchParameters = parameters;
                            break;
                        }
                    }

                    if (matchConstructor != null)
                    {
                        for (int i = 0; i < positionalArgs.Count; i++)
                        {
                            positionalArgs[i] = XmlConvert.DecodeName((string)positionalArgs[i]);
                            string argVal = (string)positionalArgs[i];
                            RemoveEscapes(ref argVal);
                            positionalArgs[i] = InternalDeserializeFromString(serializationManager, matchParameters[i].ParameterType, argVal);
                            positionalArgs[i] = GetValueFromMarkupExtension(serializationManager, positionalArgs[i]);
                        }

                        obj = Activator.CreateInstance(type, positionalArgs.ToArray());
                    }
                }
                else
                    obj = Activator.CreateInstance(type);
            }
            else
                obj = Activator.CreateInstance(type);

            if (obj == null)
            {
                serializationManager.ReportError(CreateSerializationError(SR.GetString(SR.Error_CantCreateInstanceOfBaseType, type.FullName), reader));
                return null;
            }

            if (namedArgs.Count > 0)
            {
                WorkflowMarkupSerializer serializer = serializationManager.GetSerializer(obj.GetType(), typeof(WorkflowMarkupSerializer)) as WorkflowMarkupSerializer;
                if (serializer == null)
                {
                    serializationManager.ReportError(CreateSerializationError(SR.GetString(SR.Error_SerializerNotAvailable, obj.GetType().FullName), reader));
                    return obj;
                }
                List<PropertyInfo> properties = new List<PropertyInfo>();
                try
                {
                    properties.AddRange(serializer.GetProperties(serializationManager, obj));
                    properties.AddRange(serializationManager.GetExtendedProperties(obj));
                }
                catch (Exception e)
                {
                    serializationManager.ReportError(CreateSerializationError(SR.GetString(SR.Error_SerializerThrewException, obj.GetType().FullName, e.Message), e, reader));
                    return obj;
                }

                foreach (string key in namedArgs.Keys)
                {
                    string argName = key;
                    string argVal = namedArgs[key] as string;
                    RemoveEscapes(ref argName);
                    RemoveEscapes(ref argVal);

                    PropertyInfo property = WorkflowMarkupSerializer.LookupProperty(properties, argName);
                    if (property != null)
                    {
                        serializationManager.Context.Push(property);
                        try
                        {
                            DeserializeSimpleProperty(serializationManager, reader, obj, argVal);
                        }
                        finally
                        {
                            Debug.Assert((PropertyInfo)serializationManager.Context.Current == property, "Serializer did not remove an object it pushed into stack.");
                            serializationManager.Context.Pop();
                        }
                    }
                    else
                    {
                        serializationManager.ReportError(CreateSerializationError(SR.GetString(SR.Error_SerializerPrimitivePropertyNoLogic, new object[] { argName, argName, obj.GetType().FullName }), reader));
                    }
                }
            }

            return obj;
        }

        // This function splits the argument string into an array of tokens.
        // For example: ID=Workflow1, Path=error1} would become an array that contains the following elements
        // {ID} {=} {Workflwo1} {,} {Path} {=} {error1}
        // Note that the input string should start with the first argument and end with '}'.
        private ArrayList TokenizeAttributes(WorkflowMarkupSerializationManager serializationManager, string args, int lineNumber, int linePosition)
        {
            ArrayList list = null;
            int length = args.Length;
            bool inQuotes = false;
            bool gotEscape = false;
            bool nonWhitespaceFound = false;
            Char quoteChar = '\'';
            int leftCurlies = 0;
            bool collectionIndexer = false;

            StringBuilder stringBuilder = null;
            int i = 0;

            // Loop through the args, creating a list of arguments and known delimiters.
            // This loop does limited syntax checking, and serves to tokenize the argument
            // string into chunks that are validated in greater detail in the next phase.
            for (; i < length; i++)
            {
                // Escape character is always in effect for everything inside
                // a MarkupExtension.  We have to remember that the next character is 
                // escaped, and is not treated as a quote or delimiter.
                if (!gotEscape && args[i] == '\\')
                {
                    gotEscape = true;
                    continue;
                }

                if (!nonWhitespaceFound && !Char.IsWhiteSpace(args[i]))
                {
                    nonWhitespaceFound = true;
                }

                // Process all characters that are not whitespace or are between quotes
                if (inQuotes || leftCurlies > 0 || nonWhitespaceFound)
                {
                    // We have a non-whitespace character, so ensure we have
                    // a string builder to accumulate characters and a list to collect
                    // attributes and delimiters.  These are lazily
                    // created so that simple cases that have no parameters do not
                    // create any extra objects.
                    if (stringBuilder == null)
                    {
                        stringBuilder = new StringBuilder(length);
                        list = new ArrayList(1);
                    }

                    // If the character is escaped, then it is part of the attribute
                    // being collected, regardless of its value and is not treated as
                    // a delimiter or special character.  Write back the escape
                    // character since downstream processing will need it to determine
                    // whether the value is a MarkupExtension or not, and to prevent
                    // multiple escapes from being lost by recursive processing.
                    if (gotEscape)
                    {
                        stringBuilder.Append('\\');
                        stringBuilder.Append(args[i]);
                        gotEscape = false;
                        continue;
                    }

                    // If this characters is not escaped, then look for quotes and
                    // delimiters.
                    if (inQuotes || leftCurlies > 0)
                    {
                        if (inQuotes && args[i] == quoteChar)
                        {
                            // If we're inside quotes, then only an end quote that is not
                            // escaped is special, and will act as a delimiter.
                            inQuotes = false;
                            list.Add(stringBuilder.ToString());
                            stringBuilder.Length = 0;
                            nonWhitespaceFound = false;
                        }
                        else
                        {
                            if (leftCurlies > 0 && args[i] == '}')
                            {
                                leftCurlies--;
                            }
                            else if (args[i] == '{')
                            {
                                leftCurlies++;
                            }
                            stringBuilder.Append(args[i]);
                        }
                    }
                    else
                    {
                        if (args[i] == '"' || args[i] == '\'')
                        {
                            // If we're not inside quotes, then a start quote can only
                            // occur as the first non-whitespace character in a name or value.
                            if (collectionIndexer && i < args.Length - 1 && args[i + 1] == ']')
                            {
                                collectionIndexer = false;
                                stringBuilder.Append(args[i]);
                            }
                            else if (i > 0 && args[i - 1] == '[')
                            {
                                collectionIndexer = true;
                                stringBuilder.Append(args[i]);
                            }
                            else
                            {
                                if (stringBuilder.Length != 0)
                                    return null;

                                inQuotes = true;
                                quoteChar = args[i];
                            }
                        }
                        else if (args[i] == ',' || args[i] == '=')
                        {
                            // If there is something in the stringbuilder, then store it
                            if (stringBuilder != null && stringBuilder.Length > 0)
                            {
                                list.Add(stringBuilder.ToString().Trim());
                                stringBuilder.Length = 0;
                            }
                            else if (list.Count == 0 || list[list.Count - 1] is Char)
                            {
                                // Can't have two delimiters in a row, so check what is on
                                // the list and complain if the last item is a character, or if
                                // a delimiter is the first item.
                                return null;
                            }

                            // Append known delimiters.
                            list.Add(args[i]);
                            nonWhitespaceFound = false;
                        }
                        else if (args[i] == '}')
                        {
                            // If we hit the outside right curly brace, then end processing.  If
                            // there is a delimiter on the top of the stack and we haven't
                            // hit another non-whitespace character, then its an error
                            if (stringBuilder != null)
                            {
                                if (stringBuilder.Length > 0)
                                {
                                    list.Add(stringBuilder.ToString().Trim());
                                    stringBuilder.Length = 0;
                                }
                                else if (list.Count > 0 && (list[list.Count - 1] is Char))
                                    return null;
                            }
                            break;
                        }
                        else
                        {
                            if (args[i] == '{')
                            {
                                leftCurlies++;
                            }
                            // Must just be a plain old character, so add it to the stringbuilder
                            stringBuilder.Append(args[i]);
                        }
                    }
                }

            }


            // If we've accumulated content but haven't hit a terminating '}' then the
            // format is bad, so complain.
            if (stringBuilder != null && stringBuilder.Length > 0)
                throw new Exception(SR.GetString(SR.Error_MarkupExtensionMissingTerminatingCharacter));
            else if (i < length)
            {
                // If there is non-whitespace text left that we haven't processes yet, 
                // then there is junk after the closing '}', so complain
                for (i = i + 1; i < length; i++)
                {
                    if (!Char.IsWhiteSpace(args[i]))
                        throw new Exception(SR.GetString(SR.Error_ExtraCharacterFoundAtEnd));
                }
            }

            return list;
        }

        // Remove any '\' escape characters from the passed string.  This does a simple
        // pass through the string and won't do anything if there are no '\' characters.
        private void RemoveEscapes(ref string value)
        {
            StringBuilder builder = null;
            bool noEscape = true;
            for (int i = 0; i < value.Length; i++)
            {
                if (noEscape && value[i] == '\\')
                {
                    if (builder == null)
                    {
                        builder = new StringBuilder(value.Length);
                        builder.Append(value.Substring(0, i));
                    }
                    noEscape = false;
                }
                else if (builder != null)
                {
                    builder.Append(value[i]);
                    noEscape = true;
                }
            }

            if (builder != null)
            {
                value = builder.ToString();
            }
        }
        #endregion

        #region ContentProperty Support
        private class ContentProperty : IDisposable
        {
            private WorkflowMarkupSerializationManager serializationManager;
            private WorkflowMarkupSerializer parentObjectSerializer;
            private object parentObject;

            private PropertyInfo contentProperty;
            private WorkflowMarkupSerializer contentPropertySerializer;

            public ContentProperty(WorkflowMarkupSerializationManager serializationManager, WorkflowMarkupSerializer parentObjectSerializer, object parentObject)
            {
                this.serializationManager = serializationManager;
                this.parentObjectSerializer = parentObjectSerializer;
                this.parentObject = parentObject;

                this.contentProperty = GetContentProperty(this.serializationManager, this.parentObject);
                if (this.contentProperty != null)
                {
                    this.contentPropertySerializer = this.serializationManager.GetSerializer(this.contentProperty.PropertyType, typeof(WorkflowMarkupSerializer)) as WorkflowMarkupSerializer;
                    if (this.contentPropertySerializer != null)
                    {
                        try
                        {
                            XmlReader reader = this.serializationManager.WorkflowMarkupStack[typeof(XmlReader)] as XmlReader;
                            object contentPropertyValue = null;
                            if (reader == null)
                            {
                                contentPropertyValue = this.contentProperty.GetValue(this.parentObject, null);
                            }
                            else if (!this.contentProperty.PropertyType.IsValueType &&
                                    !this.contentProperty.PropertyType.IsPrimitive &&
                                    this.contentProperty.PropertyType != typeof(string) &&
                                    !IsMarkupExtension(this.contentProperty.PropertyType) &&
                                    this.contentProperty.CanWrite)
                            {
                                WorkflowMarkupSerializer serializer = serializationManager.GetSerializer(this.contentProperty.PropertyType, typeof(WorkflowMarkupSerializer)) as WorkflowMarkupSerializer;
                                if (serializer == null)
                                {
                                    serializationManager.ReportError(CreateSerializationError(SR.GetString(SR.Error_SerializerNotAvailable, this.contentProperty.PropertyType.FullName), reader));
                                    return;
                                }
                                try
                                {
                                    contentPropertyValue = serializer.CreateInstance(serializationManager, this.contentProperty.PropertyType);
                                }
                                catch (Exception e)
                                {
                                    serializationManager.ReportError(CreateSerializationError(SR.GetString(SR.Error_SerializerCreateInstanceFailed, this.contentProperty.PropertyType.FullName, e.Message), reader));
                                    return;
                                }
                                this.contentProperty.SetValue(this.parentObject, contentPropertyValue, null);
                            }

                            if (contentPropertyValue != null)
                            {
                                if (reader != null)
                                {
                                    this.contentPropertySerializer.OnBeforeDeserialize(this.serializationManager, contentPropertyValue);
                                    this.contentPropertySerializer.OnBeforeDeserializeContents(this.serializationManager, contentPropertyValue);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            this.serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString(SR.Error_SerializerThrewException, this.parentObject.GetType(), e.Message), e));
                        }
                    }
                    else
                    {
                        this.serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString(SR.Error_SerializerNotAvailableForSerialize, this.contentProperty.PropertyType.FullName)));
                    }
                }
            }

            void IDisposable.Dispose()
            {
                XmlReader reader = this.serializationManager.WorkflowMarkupStack[typeof(XmlReader)] as XmlReader;
                if (reader != null && this.contentProperty != null && this.contentPropertySerializer != null)
                {
                    try
                    {
                        object contentPropertyValue = this.contentProperty.GetValue(this.parentObject, null);
                        if (contentPropertyValue != null)
                            this.contentPropertySerializer.OnAfterDeserialize(this.serializationManager, contentPropertyValue);
                    }
                    catch (Exception e)
                    {
                        this.serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString(SR.Error_SerializerThrewException, this.parentObject.GetType(), e.Message), e));
                    }
                }
            }

            internal PropertyInfo Property
            {
                get
                {
                    return this.contentProperty;
                }
            }

            internal object GetContents()
            {
                object value = null;
                if (this.contentProperty != null)
                    value = this.contentProperty.GetValue(this.parentObject, null);
                else
                    value = this.parentObjectSerializer.GetChildren(this.serializationManager, this.parentObject);
                return value;
            }

            internal void SetContents(IList<ContentInfo> contents)
            {
                if (contents.Count == 0)
                    return;

                if (this.contentProperty == null)
                {
                    int i = 0;
                    try
                    {
                        foreach (ContentInfo contentInfo in contents)
                        {
                            this.parentObjectSerializer.AddChild(this.serializationManager, this.parentObject, contentInfo.Content);
                            i += 1;
                        }
                    }
                    catch (Exception e)
                    {
                        this.serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString(SR.Error_SerializerThrewException, this.parentObject.GetType(), e.Message), e, contents[i].LineNumber, contents[i].LinePosition));
                    }
                }
                else if (this.contentPropertySerializer != null)
                {
                    object propertyValue = this.contentProperty.GetValue(this.parentObject, null);
                    if (CollectionMarkupSerializer.IsValidCollectionType(this.contentProperty.PropertyType))
                    {
                        if (propertyValue == null)
                        {
                            this.serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString(SR.Error_ContentPropertyCanNotBeNull, this.contentProperty.Name, this.parentObject.GetType().FullName)));
                            return;
                        }

                        //Notify serializer about begining of deserialization process
                        int i = 0;
                        try
                        {
                            foreach (ContentInfo contentInfo in contents)
                            {
                                this.contentPropertySerializer.AddChild(this.serializationManager, propertyValue, contentInfo.Content);
                                i = i + 1;
                            }
                        }
                        catch (Exception e)
                        {
                            this.serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString(SR.Error_SerializerThrewException, this.parentObject.GetType(), e.Message), e, contents[i].LineNumber, contents[i].LinePosition));
                        }
                    }
                    else
                    {
                        if (!this.contentProperty.CanWrite)
                        {
                            this.serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString(SR.Error_ContentPropertyNoSetter, this.contentProperty.Name, this.parentObject.GetType()), contents[0].LineNumber, contents[0].LinePosition));
                            return;
                        }

                        if (contents.Count > 1)
                            this.serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString(SR.Error_ContentPropertyNoMultipleContents, this.contentProperty.Name, this.parentObject.GetType()), contents[1].LineNumber, contents[1].LinePosition));

                        object content = contents[0].Content;
                        if (!this.contentProperty.PropertyType.IsAssignableFrom(content.GetType()) && typeof(string).IsAssignableFrom(content.GetType()))
                        {
                            try
                            {
                                content = this.contentPropertySerializer.DeserializeFromString(this.serializationManager, this.contentProperty.PropertyType, content as string);
                                content = WorkflowMarkupSerializer.GetValueFromMarkupExtension(this.serializationManager, content);
                            }
                            catch (Exception e)
                            {
                                this.serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString(SR.Error_SerializerThrewException, this.parentObject.GetType(), e.Message), e, contents[0].LineNumber, contents[0].LinePosition));
                                return;
                            }
                        }

                        if (content == null)
                        {
                            this.serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString(SR.Error_ContentCanNotBeConverted, content as string, contentProperty.Name, this.parentObject.GetType().FullName, this.contentProperty.PropertyType.FullName), contents[0].LineNumber, contents[0].LinePosition));
                        }
                        else if (!contentProperty.PropertyType.IsAssignableFrom(content.GetType()))
                        {
                            this.serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString(SR.Error_ContentPropertyValueInvalid, content.GetType(), this.contentProperty.Name, this.contentProperty.PropertyType.FullName), contents[0].LineNumber, contents[0].LinePosition));
                        }
                        else
                        {
                            try
                            {
                                if (this.contentProperty.PropertyType == typeof(string))
                                {
                                    content = new WorkflowMarkupSerializer().DeserializeFromString(this.serializationManager, this.contentProperty.PropertyType, content as string);
                                    content = WorkflowMarkupSerializer.GetValueFromMarkupExtension(this.serializationManager, content);
                                }
                                this.contentProperty.SetValue(this.parentObject, content, null);
                            }
                            catch (Exception e)
                            {
                                this.serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString(SR.Error_SerializerThrewException, this.parentObject.GetType(), e.Message), e, contents[0].LineNumber, contents[0].LinePosition));
                            }
                        }
                    }
                }
            }

            private PropertyInfo GetContentProperty(WorkflowMarkupSerializationManager serializationManager, object parentObject)
            {
                PropertyInfo contentProperty = null;

                string contentPropertyName = String.Empty;
                object[] contentPropertyAttributes = parentObject.GetType().GetCustomAttributes(typeof(ContentPropertyAttribute), true);
                if (contentPropertyAttributes != null && contentPropertyAttributes.Length > 0)
                    contentPropertyName = ((ContentPropertyAttribute)contentPropertyAttributes[0]).Name;

                if (!String.IsNullOrEmpty(contentPropertyName))
                {
                    contentProperty = parentObject.GetType().GetProperty(contentPropertyName, BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Public);
                    if (contentProperty == null)
                        serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString(SR.Error_ContentPropertyCouldNotBeFound, contentPropertyName, parentObject.GetType().FullName)));
                }

                return contentProperty;
            }
        }

        private struct ContentInfo
        {
            public int LineNumber;
            public int LinePosition;
            public object Content;

            public ContentInfo(object content, int lineNumber, int linePosition)
            {
                this.Content = content;
                this.LineNumber = lineNumber;
                this.LinePosition = linePosition;
            }
        }
        #endregion

        #region MarkupExtension Support
        internal static string EnsureMarkupExtensionTypeName(Type type)
        {
            string extensionName = type.Name;
            if (extensionName.EndsWith(StandardXomlKeys.MarkupExtensionSuffix, StringComparison.OrdinalIgnoreCase))
                extensionName = extensionName.Substring(0, extensionName.Length - StandardXomlKeys.MarkupExtensionSuffix.Length);
            return extensionName;
        }

        internal static string EnsureMarkupExtensionTypeName(XmlQualifiedName xmlQualifiedName)
        {
            string typeName = xmlQualifiedName.Name;
            if (xmlQualifiedName.Namespace.Equals(StandardXomlKeys.Definitions_XmlNs, StringComparison.Ordinal))
            {
                if (typeName.Equals(typeof(Array).Name, StringComparison.Ordinal))
                    typeName = typeof(ArrayExtension).Name;
            }
            return typeName;
        }

        private static bool IsMarkupExtension(Type type)
        {
            return (typeof(MarkupExtension).IsAssignableFrom(type) ||
                    typeof(System.Type).IsAssignableFrom(type) ||
                    typeof(System.Array).IsAssignableFrom(type));
        }

        private static bool IsMarkupExtension(XmlQualifiedName xmlQualifiedName)
        {
            bool markupExtension = false;
            if (xmlQualifiedName.Namespace.Equals(StandardXomlKeys.Definitions_XmlNs, StringComparison.Ordinal))
            {
                if (xmlQualifiedName.Name.Equals(typeof(Array).Name) || string.Equals(xmlQualifiedName.Name, "Null", StringComparison.Ordinal) || string.Equals(xmlQualifiedName.Name, typeof(NullExtension).Name, StringComparison.Ordinal) || string.Equals(xmlQualifiedName.Name, "Type", StringComparison.Ordinal) || string.Equals(xmlQualifiedName.Name, typeof(TypeExtension).Name, StringComparison.Ordinal))
                    markupExtension = true;
            }
            return markupExtension;
        }

        private static object GetMarkupExtensionFromValue(object value)
        {
            if (value == null)
                return new NullExtension();
            if (value is System.Type)
                return new TypeExtension(value as System.Type);
            if (value is Array)
                return new ArrayExtension(value as Array);

            return value;
        }

        private static object GetValueFromMarkupExtension(WorkflowMarkupSerializationManager manager, object extension)
        {
            object value = extension;
            MarkupExtension markupExtension = extension as MarkupExtension;
            if (markupExtension != null)
                value = markupExtension.ProvideValue(manager);
            return value;
        }
        #endregion
    }
    #endregion
}

