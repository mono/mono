namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.Reflection;
    using System.Collections;
    using System.Collections.Generic;
    using System.Workflow.ComponentModel.Design;
    using System.Xml;

    #region Class ActivityCollectionMarkupSerializer
    internal class ActivityCollectionMarkupSerializer : CollectionMarkupSerializer
    {
        protected internal override IList GetChildren(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            ActivityCollection activityCollection = obj as ActivityCollection;
            if (activityCollection == null)
                throw new ArgumentException(SR.GetString(SR.Error_SerializerTypeMismatch, typeof(ActivityCollection).FullName), "obj");

            CompositeActivity compositeActivity = activityCollection.Owner as CompositeActivity;
            if (compositeActivity != null && Helpers.IsCustomActivity(compositeActivity))
                return null;
            else
                return base.GetChildren(serializationManager, obj);
        }

        protected internal override void ClearChildren(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            //Dont do anything for this call
        }

        protected internal override void AddChild(WorkflowMarkupSerializationManager serializationManager, object obj, object childObj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            if (childObj == null)
                throw new ArgumentNullException("childObj");

            ActivityCollection activityCollection = obj as ActivityCollection;
            if (activityCollection == null)
                throw new ArgumentException(SR.GetString(SR.Error_SerializerTypeMismatch, typeof(ActivityCollection).FullName), "obj");

            Activity activity = childObj as Activity;
            if (activity == null)
                throw new InvalidOperationException(SR.GetString(SR.Error_ActivityCollectionSerializer, childObj.GetType().FullName));

            CompositeActivity compositeActivity = activityCollection.Owner as CompositeActivity;
            if (compositeActivity != null)
            {
                if (Helpers.IsCustomActivity(compositeActivity))
                    throw new InvalidOperationException(SR.GetString(SR.Error_CanNotAddActivityInBlackBoxActivity));

                base.AddChild(serializationManager, obj, childObj);
            }
        }
    }
    #endregion
}
