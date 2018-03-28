using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Reflection;
using System.Xml;
using System.Workflow.ComponentModel.Serialization;
using System.Drawing;

namespace System.Workflow.ComponentModel.Design
{
    #region Class ActivityDesignerLayoutSerializer
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class ActivityDesignerLayoutSerializer : WorkflowMarkupSerializer
    {
        protected override void OnBeforeSerialize(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            base.OnBeforeSerialize(serializationManager, obj);

            //For root activity we will go through all the nested activities and put the namespaces at the top level
            ActivityDesigner activityDesigner = obj as ActivityDesigner;
            XmlWriter writer = serializationManager.WorkflowMarkupStack[typeof(XmlWriter)] as XmlWriter;
            if (activityDesigner.Activity != null && activityDesigner.Activity.Parent == null && writer != null)
            {
                string prefix = String.Empty;
                XmlQualifiedName xmlQualifiedName = serializationManager.GetXmlQualifiedName(typeof(Point), out prefix);
                writer.WriteAttributeString("xmlns", prefix, null, xmlQualifiedName.Namespace);
            }
        }

        protected override object CreateInstance(WorkflowMarkupSerializationManager serializationManager, Type type)
        {
            if (serializationManager == null)
                throw new ArgumentNullException("serializationManager");
            if (type == null)
                throw new ArgumentNullException("type");

            object designer = null;

            IDesignerHost host = serializationManager.GetService(typeof(IDesignerHost)) as IDesignerHost;
            XmlReader reader = serializationManager.WorkflowMarkupStack[typeof(XmlReader)] as XmlReader;
            if (host != null && reader != null)
            {
                //Find the associated activity
                string associatedActivityName = String.Empty;
                while (reader.MoveToNextAttribute() && !reader.LocalName.Equals("Name", StringComparison.Ordinal));
                if (reader.LocalName.Equals("Name", StringComparison.Ordinal) && reader.ReadAttributeValue())
                    associatedActivityName = reader.Value;
                reader.MoveToElement();

                if (!String.IsNullOrEmpty(associatedActivityName))
                {
                    CompositeActivityDesigner parentDesigner = serializationManager.Context[typeof(CompositeActivityDesigner)] as CompositeActivityDesigner;
                    if (parentDesigner == null)
                    {
                        Activity activity = host.RootComponent as Activity;
                        if (activity != null && !associatedActivityName.Equals(activity.Name, StringComparison.Ordinal))
                        {
                            foreach (IComponent component in host.Container.Components)
                            {
                                activity = component as Activity;
                                if (activity != null && associatedActivityName.Equals(activity.Name, StringComparison.Ordinal))
                                    break;
                            }
                        }

                        if (activity != null)
                            designer = host.GetDesigner(activity);
                    }
                    else
                    {
                        CompositeActivity compositeActivity = parentDesigner.Activity as CompositeActivity;
                        if (compositeActivity != null)
                        {
                            Activity matchingActivity = null;
                            foreach (Activity activity in compositeActivity.Activities)
                            {
                                if (associatedActivityName.Equals(activity.Name, StringComparison.Ordinal))
                                {
                                    matchingActivity = activity;
                                    break;
                                }
                            }

                            if (matchingActivity != null)
                                designer = host.GetDesigner(matchingActivity);
                        }
                    }

                    if (designer == null)
                        serializationManager.ReportError(SR.GetString(SR.Error_LayoutSerializationActivityNotFound, reader.LocalName, associatedActivityName, "Name"));
                }
                else
                {
                    serializationManager.ReportError(SR.GetString(SR.Error_LayoutSerializationAssociatedActivityNotFound, reader.LocalName, "Name"));
                }
            }

            return designer;
        }

        protected internal override PropertyInfo[] GetProperties(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            if (serializationManager == null)
                throw new ArgumentNullException("serializationManager");
            if (obj == null)
                throw new ArgumentNullException("obj");

            List<PropertyInfo> properties = new List<PropertyInfo>(base.GetProperties(serializationManager, obj));

            ActivityDesigner activityDesigner = obj as ActivityDesigner;
            if (activityDesigner != null)
            {
                PropertyInfo nameProperty = activityDesigner.GetType().GetProperty("Name", BindingFlags.Instance | BindingFlags.NonPublic);
                if (nameProperty != null)
                    properties.Insert(0, nameProperty);
            }

            return properties.ToArray();
        }
    }
    #endregion

    #region Class CompositeActivityDesignerLayoutSerializer
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class CompositeActivityDesignerLayoutSerializer : ActivityDesignerLayoutSerializer
    {
        protected internal override PropertyInfo[] GetProperties(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            List<PropertyInfo> properties = new List<PropertyInfo>(base.GetProperties(serializationManager, obj));
            properties.Add(typeof(CompositeActivityDesigner).GetProperty("Designers", BindingFlags.Instance | BindingFlags.NonPublic));
            return properties.ToArray();
        }
    }
    #endregion

    #region Class FreeformActivityDesignerLayoutSerializer
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class FreeformActivityDesignerLayoutSerializer : CompositeActivityDesignerLayoutSerializer
    {
        protected internal override PropertyInfo[] GetProperties(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            if (serializationManager == null)
                throw new ArgumentNullException("serializationManager");
            if (obj == null)
                throw new ArgumentNullException("obj");

            XmlWriter writer = serializationManager.WorkflowMarkupStack[typeof(XmlWriter)] as XmlWriter;
            PropertyInfo[] properties = base.GetProperties(serializationManager, obj);
            FreeformActivityDesigner freeformDesigner = obj as FreeformActivityDesigner;
            if (freeformDesigner != null)
            {
                List<PropertyInfo> serializableProperties = new List<PropertyInfo>();
                foreach (PropertyInfo property in properties)
                {
                    //Only filter this property out when we are writting
                    if (writer != null &&
                        property.Name.Equals("AutoSizeMargin", StringComparison.Ordinal) &&
                        freeformDesigner.AutoSizeMargin == FreeformActivityDesigner.DefaultAutoSizeMargin)
                    {
                        continue;
                    }

                    serializableProperties.Add(property);
                }

                serializableProperties.Add(typeof(FreeformActivityDesigner).GetProperty("DesignerConnectors", BindingFlags.Instance | BindingFlags.NonPublic));
                properties = serializableProperties.ToArray();
            }

            return properties;
        }
    }
    #endregion

    #region ConnectorLayoutSerializer
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class ConnectorLayoutSerializer : WorkflowMarkupSerializer
    {
        protected internal override PropertyInfo[] GetProperties(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            if (serializationManager == null)
                throw new ArgumentNullException("serializationManager");
            if (obj == null)
                throw new ArgumentNullException("obj");

            List<PropertyInfo> properties = new List<PropertyInfo>(base.GetProperties(serializationManager, obj));
            properties.Add(typeof(Connector).GetProperty("SourceActivity", BindingFlags.Instance | BindingFlags.NonPublic));
            properties.Add(typeof(Connector).GetProperty("SourceConnectionIndex", BindingFlags.Instance | BindingFlags.NonPublic));
            properties.Add(typeof(Connector).GetProperty("SourceConnectionEdge", BindingFlags.Instance | BindingFlags.NonPublic));
            properties.Add(typeof(Connector).GetProperty("TargetActivity", BindingFlags.Instance | BindingFlags.NonPublic));
            properties.Add(typeof(Connector).GetProperty("TargetConnectionIndex", BindingFlags.Instance | BindingFlags.NonPublic));
            properties.Add(typeof(Connector).GetProperty("TargetConnectionEdge", BindingFlags.Instance | BindingFlags.NonPublic));
            properties.Add(typeof(Connector).GetProperty("Segments", BindingFlags.Instance | BindingFlags.NonPublic));
            return properties.ToArray();
        }

        protected override object CreateInstance(WorkflowMarkupSerializationManager serializationManager, Type type)
        {
            if (serializationManager == null)
                throw new ArgumentNullException("serializationManager");
            if (type == null)
                throw new ArgumentNullException("type");

            Connector connector = null;

            IReferenceService referenceService = serializationManager.GetService(typeof(IReferenceService)) as IReferenceService;
            FreeformActivityDesigner freeformDesigner = serializationManager.Context[typeof(FreeformActivityDesigner)] as FreeformActivityDesigner;
            if (freeformDesigner != null && referenceService != null)
            {
                ConnectionPoint sourceConnection = null;
                ConnectionPoint targetConnection = null;

                try
                {
                    Dictionary<string, string> constructionArguments = GetConnectorConstructionArguments(serializationManager, type);

                    if (constructionArguments.ContainsKey("SourceActivity") &&
                        constructionArguments.ContainsKey("SourceConnectionIndex") &&
                        constructionArguments.ContainsKey("SourceConnectionEdge"))
                    {
                        ActivityDesigner sourceDesigner = ActivityDesigner.GetDesigner(referenceService.GetReference(constructionArguments["SourceActivity"] as string) as Activity);
                        DesignerEdges sourceEdge = (DesignerEdges)Enum.Parse(typeof(DesignerEdges), constructionArguments["SourceConnectionEdge"] as string);
                        int sourceIndex = Convert.ToInt32(constructionArguments["SourceConnectionIndex"] as string, System.Globalization.CultureInfo.InvariantCulture);
                        if (sourceDesigner != null && sourceEdge != DesignerEdges.None && sourceIndex >= 0)
                            sourceConnection = new ConnectionPoint(sourceDesigner, sourceEdge, sourceIndex);
                    }

                    if (constructionArguments.ContainsKey("TargetActivity") &&
                        constructionArguments.ContainsKey("TargetConnectionIndex") &&
                        constructionArguments.ContainsKey("TargetConnectionEdge"))
                    {
                        ActivityDesigner targetDesigner = ActivityDesigner.GetDesigner(referenceService.GetReference(constructionArguments["TargetActivity"] as string) as Activity);
                        DesignerEdges targetEdge = (DesignerEdges)Enum.Parse(typeof(DesignerEdges), constructionArguments["TargetConnectionEdge"] as string);
                        int targetIndex = Convert.ToInt32(constructionArguments["TargetConnectionIndex"] as string, System.Globalization.CultureInfo.InvariantCulture);
                        if (targetDesigner != null && targetEdge != DesignerEdges.None && targetIndex >= 0)
                            targetConnection = new ConnectionPoint(targetDesigner, targetEdge, targetIndex);
                    }
                }
                catch
                {
                }

                if (sourceConnection != null && targetConnection != null)
                    connector = freeformDesigner.AddConnector(sourceConnection, targetConnection);
            }

            return connector;
        }

        protected override void OnAfterDeserialize(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            base.OnAfterDeserialize(serializationManager, obj);

            //The following code is needed in order to making sure that we set the modification flag correctly after deserialization
            Connector connector = obj as Connector;
            if (connector != null)
                connector.SetConnectorModified(true);
        }

        protected Dictionary<string, string> GetConnectorConstructionArguments(WorkflowMarkupSerializationManager serializationManager, Type type)
        {
            Dictionary<string, string> argumentDictionary = new Dictionary<string, string>();

            XmlReader reader = serializationManager.WorkflowMarkupStack[typeof(XmlReader)] as XmlReader;
            if (reader != null && reader.NodeType == XmlNodeType.Element)
            {
                while (reader.MoveToNextAttribute())
                {
                    string attributeName = reader.LocalName;
                    if (!argumentDictionary.ContainsKey(attributeName))
                    {
                        reader.ReadAttributeValue();
                        argumentDictionary.Add(attributeName, reader.Value);
                    }
                }
                reader.MoveToElement();
            }

            return argumentDictionary;
        }
    }
    #endregion

    #region Class ActivityDesignerLayoutSerializerProvider
    internal sealed class ActivityDesignerLayoutSerializerProvider : IDesignerSerializationProvider
    {
        #region IDesignerSerializationProvider Members
        object IDesignerSerializationProvider.GetSerializer(IDesignerSerializationManager manager, object currentSerializer, Type objectType, Type serializerType)
        {
            if (typeof(System.Drawing.Color) == objectType)
                currentSerializer = new ColorMarkupSerializer();
            else if (typeof(System.Drawing.Size) == objectType)
                currentSerializer = new SizeMarkupSerializer();
            else if (typeof(System.Drawing.Point) == objectType)
                currentSerializer = new PointMarkupSerializer();
            return currentSerializer;
        }
        #endregion
    }
    #endregion

    #region Class ColorMarkupSerializer
    internal sealed class ColorMarkupSerializer : WorkflowMarkupSerializer
    {
        protected internal override bool CanSerializeToString(WorkflowMarkupSerializationManager serializationManager, object value)
        {
            return (value is System.Drawing.Color);
        }

        protected internal override string SerializeToString(WorkflowMarkupSerializationManager serializationManager, object value)
        {
            if (serializationManager == null)
                throw new ArgumentNullException("serializationManager");
            if (value == null)
                throw new ArgumentNullException("value");

            string stringValue = String.Empty;
            if (value is System.Drawing.Color)
            {
                System.Drawing.Color color = (System.Drawing.Color)value;
                long colorValue = (long)((uint)(color.A << 24 | color.R << 16 | color.G << 8 | color.B)) & 0xFFFFFFFF;
                stringValue = "0X" + colorValue.ToString("X08", System.Globalization.CultureInfo.InvariantCulture);
            }
            return stringValue;
        }

        protected internal override object DeserializeFromString(WorkflowMarkupSerializationManager serializationManager, Type propertyType, string value)
        {
            if (propertyType.IsAssignableFrom(typeof(System.Drawing.Color)))
            {
                string colorValue = value as string;
                if (!String.IsNullOrEmpty(colorValue))
                {
                    if (colorValue.StartsWith("0X", StringComparison.OrdinalIgnoreCase))
                    {
                        long propertyValue = Convert.ToInt64((string)value, 16) & 0xFFFFFFFF;
                        return System.Drawing.Color.FromArgb((Byte)(propertyValue >> 24), (Byte)(propertyValue >> 16), (Byte)(propertyValue >> 8), (Byte)(propertyValue));
                    }
                    else
                    {
                        return base.DeserializeFromString(serializationManager, propertyType, value);
                    }
                }
            }

            return null;
        }
    }
    #endregion

    #region Class SizeMarkupSerializer
    internal sealed class SizeMarkupSerializer : WorkflowMarkupSerializer
    {
        protected internal override bool CanSerializeToString(WorkflowMarkupSerializationManager serializationManager, object value)
        {
            return (value is System.Drawing.Size);
        }

        protected internal override PropertyInfo[] GetProperties(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            List<PropertyInfo> properties = new List<PropertyInfo>();
            if (obj is Size)
            {
                properties.Add(typeof(Size).GetProperty("Width"));
                properties.Add(typeof(Size).GetProperty("Height"));
            }
            return properties.ToArray();
        }

        protected internal override string SerializeToString(WorkflowMarkupSerializationManager serializationManager, object value)
        {
            string convertedValue = String.Empty;

            TypeConverter converter = TypeDescriptor.GetConverter(value);
            if (converter != null && converter.CanConvertTo(typeof(string)))
                convertedValue = converter.ConvertTo(value, typeof(string)) as string;
            else
                convertedValue = base.SerializeToString(serializationManager, value);
            return convertedValue;
        }

        protected internal override object DeserializeFromString(WorkflowMarkupSerializationManager serializationManager, Type propertyType, string value)
        {
            object size = Size.Empty;

            string sizeValue = value as string;
            if (!String.IsNullOrEmpty(sizeValue))
            {
                TypeConverter converter = TypeDescriptor.GetConverter(typeof(Size));
                if (converter != null && converter.CanConvertFrom(typeof(string)) && !IsValidCompactAttributeFormat(sizeValue))
                    size = converter.ConvertFrom(value);
                else
                    size = base.SerializeToString(serializationManager, value);
            }

            return size;
        }
    }
    #endregion

    #region Class PointMarkupSerializer
    internal sealed class PointMarkupSerializer : WorkflowMarkupSerializer
    {
        protected internal override bool CanSerializeToString(WorkflowMarkupSerializationManager serializationManager, object value)
        {
            return (value is Point);
        }

        protected internal override PropertyInfo[] GetProperties(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            List<PropertyInfo> properties = new List<PropertyInfo>();
            if (obj is Point)
            {
                properties.Add(typeof(Point).GetProperty("X"));
                properties.Add(typeof(Point).GetProperty("Y"));
            }
            return properties.ToArray();
        }

        protected internal override string SerializeToString(WorkflowMarkupSerializationManager serializationManager, object value)
        {
            string convertedValue = String.Empty;

            TypeConverter converter = TypeDescriptor.GetConverter(value);
            if (converter != null && converter.CanConvertTo(typeof(string)))
                convertedValue = converter.ConvertTo(value, typeof(string)) as string;
            else
                convertedValue = base.SerializeToString(serializationManager, value);
            return convertedValue;
        }

        protected internal override object DeserializeFromString(WorkflowMarkupSerializationManager serializationManager, Type propertyType, string value)
        {
            object point = Point.Empty;

            string pointValue = value as string;
            if (!String.IsNullOrEmpty(pointValue))
            {
                TypeConverter converter = TypeDescriptor.GetConverter(typeof(Point));
                if (converter != null && converter.CanConvertFrom(typeof(string)) && !IsValidCompactAttributeFormat(pointValue))
                    point = converter.ConvertFrom(value);
                else
                    point = base.SerializeToString(serializationManager, value);
            }

            return point;
        }
    }
    #endregion
}
