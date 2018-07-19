namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.Xml;
    using System.Reflection;
    using System.Workflow.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Text;
    using System.Diagnostics;
    using System.ComponentModel;
    using System.Collections.Generic;

    // This is called BindMarkupSerializer, but the implementation can be used for a general MarkupExtensionSerializer.
    // The syntax for the serialization conforms to XAML's markup extension.
    #region Class MarkupExtensionSerializer
    internal class MarkupExtensionSerializer : WorkflowMarkupSerializer
    {
        private const string CompactFormatPropertySeperator = ",";
        private const string CompactFormatTypeSeperator = " ";
        private const string CompactFormatNameValueSeperator = "=";
        private const string CompactFormatStart = "{";
        private const string CompactFormatEnd = "}";
        private const string CompactFormatCharacters = "=,\"\'{}\\";

        protected internal sealed override bool CanSerializeToString(WorkflowMarkupSerializationManager serializationManager, object value)
        {
            return true;
        }

        protected internal sealed override string SerializeToString(WorkflowMarkupSerializationManager serializationManager, object value)
        {
            if (serializationManager == null)
                throw new ArgumentNullException("serializationManager");
            XmlWriter writer = serializationManager.WorkflowMarkupStack[typeof(XmlWriter)] as XmlWriter;
            if (writer == null)
                throw new ArgumentNullException("writer");
            if (value == null)
                throw new ArgumentNullException("value");

            writer.WriteString(MarkupExtensionSerializer.CompactFormatStart);

            string prefix = String.Empty;
            XmlQualifiedName qualifiedName = serializationManager.GetXmlQualifiedName(value.GetType(), out prefix);
            writer.WriteQualifiedName(qualifiedName.Name, qualifiedName.Namespace);

            int index = 0;

            Dictionary<string, string> constructorArguments = null;
            InstanceDescriptor instanceDescriptor = this.GetInstanceDescriptor(serializationManager, value);
            if (instanceDescriptor != null)
            {
                ConstructorInfo ctorInfo = instanceDescriptor.MemberInfo as ConstructorInfo;
                if (ctorInfo != null)
                {
                    ParameterInfo[] parameters = ctorInfo.GetParameters();
                    if (parameters != null && parameters.Length == instanceDescriptor.Arguments.Count)
                    {
                        int i = 0;
                        foreach (object argValue in instanceDescriptor.Arguments)
                        {
                            if (constructorArguments == null)
                                constructorArguments = new Dictionary<string, string>();
                            // 
                            if (argValue == null)
                                continue;
                            constructorArguments.Add(parameters[i].Name, parameters[i++].Name);
                            if (index++ > 0)
                                writer.WriteString(MarkupExtensionSerializer.CompactFormatPropertySeperator);
                            else
                                writer.WriteString(MarkupExtensionSerializer.CompactFormatTypeSeperator);
                            if (argValue.GetType() == typeof(string))
                            {
                                writer.WriteString(CreateEscapedValue(argValue as string));
                            }
                            else if (argValue is System.Type)
                            {
                                Type argType = argValue as Type;
                                if (argType.Assembly != null)
                                {
                                    string typePrefix = String.Empty;
                                    XmlQualifiedName typeQualifiedName = serializationManager.GetXmlQualifiedName(argType, out typePrefix);
                                    writer.WriteQualifiedName(XmlConvert.EncodeName(typeQualifiedName.Name), typeQualifiedName.Namespace);
                                }
                                else
                                {
                                    writer.WriteString(argType.FullName);
                                }
                            }
                            else
                            {
                                string stringValue = base.SerializeToString(serializationManager, argValue);
                                if (stringValue != null)
                                    writer.WriteString(stringValue);
                            }
                        }
                    }
                }
            }

            List<PropertyInfo> properties = new List<PropertyInfo>();
            properties.AddRange(GetProperties(serializationManager, value));
            properties.AddRange(serializationManager.GetExtendedProperties(value));
            foreach (PropertyInfo serializableProperty in properties)
            {
                if (Helpers.GetSerializationVisibility(serializableProperty) != DesignerSerializationVisibility.Hidden && serializableProperty.CanRead && serializableProperty.GetValue(value, null) != null)
                {
                    WorkflowMarkupSerializer propSerializer = serializationManager.GetSerializer(serializableProperty.PropertyType, typeof(WorkflowMarkupSerializer)) as WorkflowMarkupSerializer;
                    if (propSerializer == null)
                    {
                        serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString(SR.Error_SerializerNotAvailable, serializableProperty.PropertyType.FullName)));
                        continue;
                    }

                    if (constructorArguments != null)
                    {
                        object[] attributes = serializableProperty.GetCustomAttributes(typeof(ConstructorArgumentAttribute), false);
                        if (attributes.Length > 0 && constructorArguments.ContainsKey((attributes[0] as ConstructorArgumentAttribute).ArgumentName))
                            // Skip this property, it has already been represented by a constructor parameter
                            continue;
                    }

                    //Get the property serializer so that we can convert the bind object to string
                    serializationManager.Context.Push(serializableProperty);
                    try
                    {
                        object propValue = serializableProperty.GetValue(value, null);
                        if (propSerializer.ShouldSerializeValue(serializationManager, propValue))
                        {
                            //We do not allow nested bind syntax
                            if (propSerializer.CanSerializeToString(serializationManager, propValue))
                            {
                                if (index++ > 0)
                                    writer.WriteString(MarkupExtensionSerializer.CompactFormatPropertySeperator);
                                else
                                    writer.WriteString(MarkupExtensionSerializer.CompactFormatTypeSeperator);
                                writer.WriteString(serializableProperty.Name);
                                writer.WriteString(MarkupExtensionSerializer.CompactFormatNameValueSeperator);

                                if (propValue.GetType() == typeof(string))
                                {
                                    writer.WriteString(CreateEscapedValue(propValue as string));
                                }
                                else
                                {
                                    string stringValue = propSerializer.SerializeToString(serializationManager, propValue);
                                    if (stringValue != null)
                                        writer.WriteString(stringValue);
                                }
                            }
                            else
                            {
                                serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString(SR.Error_SerializerNoSerializeLogic, new object[] { serializableProperty.Name, value.GetType().FullName })));
                            }
                        }
                    }
                    catch
                    {
                        serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString(SR.Error_SerializerNoSerializeLogic, new object[] { serializableProperty.Name, value.GetType().FullName })));
                        continue;
                    }
                    finally
                    {
                        Debug.Assert((PropertyInfo)serializationManager.Context.Current == serializableProperty, "Serializer did not remove an object it pushed into stack.");
                        serializationManager.Context.Pop();
                    }
                }
            }
            writer.WriteString(MarkupExtensionSerializer.CompactFormatEnd);
            return string.Empty;

        }

        protected virtual InstanceDescriptor GetInstanceDescriptor(WorkflowMarkupSerializationManager serializationManager, object value)
        {
            MarkupExtension markupExtension = value as MarkupExtension;
            if (markupExtension == null)
                throw new ArgumentException(SR.GetString(SR.Error_UnexpectedArgumentType, typeof(MarkupExtension).FullName), "value");
            return new InstanceDescriptor(markupExtension.GetType().GetConstructor(new Type[0]), null);

        }

        // more escaped characters can be consider here, hence a seperate fn instead of string.Replace
        private string CreateEscapedValue(string value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            StringBuilder sb = new StringBuilder(64);
            int length = value.Length;
            for (int i = 0; i < length; i++)
            {
                if (MarkupExtensionSerializer.CompactFormatCharacters.IndexOf(value[i]) != -1)
                    sb.Append("\\");
                sb.Append(value[i]);
            }
            return sb.ToString();
        }
    }
    #endregion


}
