namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.Reflection;
    using System.Collections;
    using System.Collections.Generic;
    using System.Workflow.ComponentModel.Design;
    using System.Xml;

    #region Class CompositeActivityMarkupSerializer
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class CompositeActivityMarkupSerializer : ActivityMarkupSerializer
    {
        internal override void OnBeforeSerializeContents(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            base.OnBeforeSerializeContents(serializationManager, obj);

            //For root activity we will go through all the nested activities and put the namespaces at the top level
            CompositeActivity compositeActivity = obj as CompositeActivity;
            XmlWriter writer = serializationManager.WorkflowMarkupStack[typeof(XmlWriter)] as XmlWriter;
            if (compositeActivity.Parent == null && writer != null)
            {
                Dictionary<string, Activity> writtenMappings = new Dictionary<string, Activity>();

                string prefix = String.Empty;
                XmlQualifiedName xmlQualifiedName = serializationManager.GetXmlQualifiedName(compositeActivity.GetType(), out prefix);
                writtenMappings.Add(xmlQualifiedName.Namespace, compositeActivity);

                foreach (Activity containedActivity in Helpers.GetNestedActivities(compositeActivity))
                {
                    prefix = String.Empty;
                    xmlQualifiedName = serializationManager.GetXmlQualifiedName(containedActivity.GetType(), out prefix);
                    if (!writtenMappings.ContainsKey(xmlQualifiedName.Namespace))
                    {
                        writer.WriteAttributeString("xmlns", prefix, null, xmlQualifiedName.Namespace);
                        writtenMappings.Add(xmlQualifiedName.Namespace, containedActivity);
                    }
                }
            }
        }
    }
    #endregion

}
