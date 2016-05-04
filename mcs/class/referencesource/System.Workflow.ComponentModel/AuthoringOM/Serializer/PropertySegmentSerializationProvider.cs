namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.Reflection;
    using System.Xml;
    using System.ComponentModel.Design.Serialization;
    using System.Collections;
    using System.Workflow.ComponentModel.Design;
    using System.ComponentModel;

    #region Class PropertySegmentSerializationProvider
    internal sealed class PropertySegmentSerializationProvider : WorkflowMarkupSerializationProvider
    {
        #region IDesignerSerializationProvider Members
        public override object GetSerializer(IDesignerSerializationManager manager, object currentSerializer, Type objectType, Type serializerType)
        {
            if (serializerType.IsAssignableFrom(typeof(WorkflowMarkupSerializer)))
            {
                if (currentSerializer is PropertySegmentSerializer)
                    return currentSerializer;
                else if (objectType == typeof(PropertySegment))
                    return new PropertySegmentSerializer(null);
                else if (currentSerializer is WorkflowMarkupSerializer)
                    return new PropertySegmentSerializer(currentSerializer as WorkflowMarkupSerializer);
                else
                    return null;
            }
            else
            {
                return base.GetSerializer(manager, currentSerializer, objectType, serializerType);
            }
        }
        #endregion
    }
    #endregion

}
