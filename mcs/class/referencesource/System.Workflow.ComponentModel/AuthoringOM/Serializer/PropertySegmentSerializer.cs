namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.Reflection;
    using System.Xml;
    using System.ComponentModel.Design.Serialization;
    using System.Collections;
    using System.Workflow.ComponentModel.Design;
    using System.ComponentModel;

    #region Class PropertySegmentSerializer
    internal sealed class PropertySegmentSerializer : WorkflowMarkupSerializer
    {
        private WorkflowMarkupSerializer containedSerializer = null;
        public PropertySegmentSerializer(WorkflowMarkupSerializer containedSerializer)
        {
            this.containedSerializer = containedSerializer;
        }

        protected internal override PropertyInfo[] GetProperties(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            if (this.containedSerializer != null)
                return this.containedSerializer.GetProperties(serializationManager, obj);

            if (obj != null && obj.GetType() == typeof(PropertySegment))
                return (obj as PropertySegment).GetProperties(serializationManager);
            else
                return base.GetProperties(serializationManager, obj);
        }

        protected override object CreateInstance(WorkflowMarkupSerializationManager serializationManager, Type type)
        {
            if (typeof(PropertySegment) == type)
                return Activator.CreateInstance(type, new object[] { serializationManager as IServiceProvider, serializationManager.Context.Current });
            else
                return base.CreateInstance(serializationManager, type);
        }

        protected internal override bool ShouldSerializeValue(WorkflowMarkupSerializationManager serializationManager, object value)
        {
            return true;
        }

        protected internal override bool CanSerializeToString(WorkflowMarkupSerializationManager serializationManager, object value)
        {
            bool canSerializeToString = false;
            if (value != null)
            {
                ITypeDescriptorContext context = null;
                TypeConverter converter = GetTypeConversionInfoForPropertySegment(serializationManager, value.GetType(), out context);
                if (converter != null)
                    canSerializeToString = converter.CanConvertTo(context, typeof(string));

                if (!canSerializeToString)
                {
                    if (this.containedSerializer != null)
                        canSerializeToString = this.containedSerializer.CanSerializeToString(serializationManager, value);
                    else
                        canSerializeToString = base.CanSerializeToString(serializationManager, value);
                }
            }
            else
            {
                canSerializeToString = true;
            }

            return canSerializeToString;
        }

        protected internal override string SerializeToString(WorkflowMarkupSerializationManager serializationManager, object value)
        {
            String stringValue = String.Empty;
            if (value == null)
            {
                stringValue = "*null";
            }
            else
            {
                ITypeDescriptorContext context = null;
                TypeConverter converter = GetTypeConversionInfoForPropertySegment(serializationManager, value.GetType(), out context);
                if (converter != null && converter.CanConvertTo(context, typeof(string)))
                    stringValue = converter.ConvertToString(context, value);
                else if (this.containedSerializer != null)
                    stringValue = this.containedSerializer.SerializeToString(serializationManager, value);
                else
                    stringValue = base.SerializeToString(serializationManager, value);
            }

            return stringValue;
        }

        protected internal override object DeserializeFromString(WorkflowMarkupSerializationManager serializationManager, Type propertyType, string value)
        {
            if (serializationManager == null)
                throw new ArgumentNullException("serializationManager");
            if (propertyType == null)
                throw new ArgumentNullException("propertyType");
            if (value == null)
                throw new ArgumentNullException("value");

            object convertedValue = null;
            if (string.Equals(value, "*null", StringComparison.Ordinal))
            {
                convertedValue = null;
            }
            else
            {
                ITypeDescriptorContext context = null;
                TypeConverter converter = GetTypeConversionInfoForPropertySegment(serializationManager, propertyType, out context);
                if (converter != null && converter.CanConvertFrom(context, typeof(string)))
                    convertedValue = converter.ConvertFromString(context, value);
                else if (this.containedSerializer != null)
                    convertedValue = this.containedSerializer.DeserializeFromString(serializationManager, propertyType, value);
                else
                    convertedValue = base.DeserializeFromString(serializationManager, propertyType, value);
            }

            return convertedValue;
        }

        protected internal override IList GetChildren(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            if (this.containedSerializer != null)
                return this.containedSerializer.GetChildren(serializationManager, obj);

            return null;
        }

        protected internal override void ClearChildren(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            if (this.containedSerializer != null)
                this.containedSerializer.ClearChildren(serializationManager, obj);
        }

        protected internal override void AddChild(WorkflowMarkupSerializationManager serializationManager, object obj, object childObj)
        {
            if (this.containedSerializer != null)
                this.containedSerializer.AddChild(serializationManager, obj, childObj);
        }

        private TypeConverter GetTypeConversionInfoForPropertySegment(WorkflowMarkupSerializationManager serializationManager, Type propertyType, out ITypeDescriptorContext context)
        {
            TypeConverter converter = null;
            context = null;
            PropertySegmentPropertyInfo propertyInfo = serializationManager.Context[typeof(PropertySegmentPropertyInfo)] as PropertySegmentPropertyInfo;
            if (propertyInfo.PropertySegment != null)
            {
                if (propertyInfo.PropertySegment.PropertyDescriptor != null)
                {
                    context = new TypeDescriptorContext(propertyInfo.PropertySegment.ServiceProvider, propertyInfo.PropertySegment.PropertyDescriptor, propertyInfo.PropertySegment.Object);
                    converter = propertyInfo.PropertySegment.PropertyDescriptor.Converter;
                }
                else if (propertyInfo.PropertySegment.Object != null)
                {
                    PropertyDescriptor propertyDescriptor = TypeDescriptor.GetProperties(propertyInfo.PropertySegment.Object)[propertyInfo.Name];
                    if (propertyDescriptor != null)
                    {
                        context = new TypeDescriptorContext(propertyInfo.PropertySegment.ServiceProvider, propertyDescriptor, propertyInfo.PropertySegment.Object);
                        converter = propertyDescriptor.Converter;
                    }
                }
            }

            if (propertyType != null && converter == null)
            {
                converter = TypeDescriptor.GetConverter(propertyType);
            }

            return converter;
        }
    }
    #endregion
}
