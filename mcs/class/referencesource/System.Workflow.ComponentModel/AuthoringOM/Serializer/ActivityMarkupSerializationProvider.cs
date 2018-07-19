namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.ComponentModel.Design.Serialization;

    #region Class ActivityMarkupSerializationProvider
    internal sealed class ActivityMarkupSerializationProvider : WorkflowMarkupSerializationProvider
    {
        public override object GetSerializer(IDesignerSerializationManager manager, object currentSerializer, Type objectType, Type serializerType)
        {
            // If this isn't a serializer type we recognize, do nothing.  Also, if metadata specified
            // a custom serializer, then use it.
            if (serializerType != typeof(WorkflowMarkupSerializer) || currentSerializer != null)
                return null;

            if (typeof(CompositeActivity).IsAssignableFrom(objectType))
                return new CompositeActivityMarkupSerializer();

            if (typeof(ItemList<>).IsAssignableFrom(objectType))
                return new CollectionMarkupSerializer();

            // Ask the base class if it has a specialized serializer class for this object type.  If it returns
            // its default serializer, return our default serializer instead.
            IDesignerSerializationProvider baseProvider = new WorkflowMarkupSerializationProvider() as IDesignerSerializationProvider;
            object baseSerializer = baseProvider.GetSerializer(manager, currentSerializer, objectType, serializerType);
            if (baseSerializer.GetType() != typeof(WorkflowMarkupSerializer))
                return baseSerializer;

            return new ActivityMarkupSerializer();
        }
    }
    #endregion
}
