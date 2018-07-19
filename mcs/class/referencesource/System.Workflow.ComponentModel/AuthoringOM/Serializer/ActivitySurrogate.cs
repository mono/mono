namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Reflection;
    using System.Collections;
    using System.Diagnostics;
    using System.Globalization;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;

    internal sealed class ActivitySurrogate : ISerializationSurrogate
    {
        public ActivitySurrogate()
        {
        }
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            if (Activity.ContextIdToActivityMap == null)
                throw new InvalidOperationException(SR.GetString(SR.Error_ActivitySaveLoadNotCalled));

            Activity activity = (Activity)obj;
            bool isSurroundingActivity = false;
            bool isDanglingActivity = IsDanglingActivity(activity, out isSurroundingActivity);
            if (isSurroundingActivity)
            {
                // if this object is in parent chain then replace it with token
                if (activity.ContextActivity != null)
                    info.AddValue("cid", activity.ContextId);

                info.AddValue("id", activity.DottedPath);
                info.SetType(typeof(ActivityRef));
            }
            else if (!isDanglingActivity)
            {
                info.AddValue("id", activity.DottedPath);

                string[] names = null;
                MemberInfo[] members = FormatterServicesNoSerializableCheck.GetSerializableMembers(obj.GetType(), out names);
                object[] memberDatas = FormatterServices.GetObjectData(obj, members);
                // To improve performance, we specialize the case where there are only 2 fields.  One is the 
                // instance dependency property values dictionary and the other is the "disposed" event.
                if (memberDatas != null && memberDatas.Length == 2)
                {
                    Debug.Assert(members[0].Name == "dependencyPropertyValues" && members[1].Name == "disposed");
                    IDictionary<DependencyProperty, object> instanceProperties = (IDictionary<DependencyProperty, object>)memberDatas[0];
                    if (instanceProperties != null && instanceProperties.Count > 0)
                    {
                        foreach (KeyValuePair<DependencyProperty, object> kvp in instanceProperties)
                        {
                            if (kvp.Key != null && !kvp.Key.DefaultMetadata.IsNonSerialized)
                            {
                                info.AddValue("memberData", memberDatas[0]);
                                break;
                            }
                        }
                    }
                    if (memberDatas[1] != null)
                        info.AddValue("disposed", memberDatas[1]);
                }
                else
                {
                    info.AddValue("memberNames", names);
                    info.AddValue("memberDatas", memberDatas);
                }

                // for root activity serialize the change actions if there are any
                if (obj is Activity && ((Activity)obj).Parent == null)
                {
                    string wMarkup = activity.GetValue(Activity.WorkflowXamlMarkupProperty) as string;
                    if (!string.IsNullOrEmpty(wMarkup))
                    {
                        info.AddValue("workflowMarkup", wMarkup);

                        //if we got rules in XAML Load case, serialize them as well
                        string rMarkup = activity.GetValue(Activity.WorkflowRulesMarkupProperty) as string;
                        if (!string.IsNullOrEmpty(rMarkup))
                            info.AddValue("rulesMarkup", rMarkup);
                    }
                    else
                        info.AddValue("type", activity.GetType());

                    Activity workflowDefinition = (Activity)activity.GetValue(Activity.WorkflowDefinitionProperty);
                    if (workflowDefinition != null)
                    {
                        ArrayList changeActions = (ArrayList)workflowDefinition.GetValue(WorkflowChanges.WorkflowChangeActionsProperty);
                        if (changeActions != null)
                        {
                            Guid changeVersion = (Guid)workflowDefinition.GetValue(WorkflowChanges.WorkflowChangeVersionProperty);
                            info.AddValue("workflowChangeVersion", changeVersion);
                            using (StringWriter changeActionsStringWriter = new StringWriter(CultureInfo.InvariantCulture))
                            {
                                using (XmlWriter xmlWriter = Design.Helpers.CreateXmlWriter(changeActionsStringWriter))
                                {
                                    new WorkflowMarkupSerializer().Serialize(xmlWriter, changeActions);
                                    info.AddValue("workflowChanges", changeActionsStringWriter.ToString());
                                }
                            }
                        }
                    }
                }
                info.SetType(typeof(ActivitySerializedRef));
            }
            else
            {
                info.AddValue("id", activity.Name);
                info.AddValue("type", activity.GetType());
                info.SetType(typeof(DanglingActivityRef));
            }
        }
        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            return null;
        }
        private bool IsDanglingActivity(Activity activity, out bool isSurrounding)
        {
            isSurrounding = false;
            bool isDangling = false;
            do
            {
                if (Activity.ActivityRoots.Contains(activity))
                {
                    isDangling = false;
                    break;
                }

                if (activity.Parent == null)
                {
                    isDangling = ((Activity)Activity.ActivityRoots[0]).RootActivity != activity;
                    break;
                }

                if (!activity.Parent.Activities.Contains(activity))
                {
                    IList<Activity> activeContextActivities = null;
                    if (activity.Parent.ContextActivity != null)
                        activeContextActivities = (IList<Activity>)activity.Parent.ContextActivity.GetValue(Activity.ActiveExecutionContextsProperty);

                    if (activeContextActivities == null || !activeContextActivities.Contains(activity))
                    {
                        isDangling = true;
                        break;
                    }
                }
                activity = activity.Parent;
            } while (activity != null);

            isSurrounding = (!isDangling && !Activity.ActivityRoots.Contains(activity));
            return isDangling;
        }

        [Serializable]
        private sealed class ActivityRef : IObjectReference
        {
            [OptionalField]
            private int cid = 0;
            private string id = string.Empty;

            Object IObjectReference.GetRealObject(StreamingContext context)
            {
                if (Activity.ContextIdToActivityMap == null)
                    throw new InvalidOperationException(SR.GetString(SR.Error_ActivitySaveLoadNotCalled));

                Activity contextActivity = (Activity)Activity.ContextIdToActivityMap[this.cid];
                return contextActivity.TraverseDottedPathFromRoot(this.id);
            }
        }

        [Serializable]
        private sealed class ActivitySerializedRef : IObjectReference, IDeserializationCallback
        {
            private string id = string.Empty;

            [OptionalField]
            private object memberData = null;

            [OptionalField]
            private object[] memberDatas = null;

            [OptionalField]
            private string[] memberNames = null;

            [OptionalField]
            private Type type = null;

            [OptionalField]
            private string workflowMarkup = null;

            [OptionalField]
            private string rulesMarkup = null;

            [OptionalField]
            private string workflowChanges = null;

            [OptionalField]
            private Guid workflowChangeVersion = Guid.Empty;

            [OptionalField]
            private EventHandler disposed = null;

            [NonSerialized]
            private Activity cachedDefinitionActivity = null;

            [NonSerialized]
            private Activity cachedActivity = null;

            [NonSerialized]
            private int lastPosition = 0;

            object IObjectReference.GetRealObject(StreamingContext context)
            {
                // if definition activity has not been yet deserialized then return null
                if (Activity.DefinitionActivity == null)
                {
                    if (this.type == null && string.IsNullOrEmpty(this.workflowMarkup))
                        return null;

                    Activity rootActivityDefinition = null;
                    // We always call into runtime to resolve an activity.  The runtime may return an existing cached workflow definition
                    // or it may return a new one if none exists.
                    // When we have dynamic updates, we ask runtime to always return us a new instance of the workflow definition.
                    // This new instance should not be initialized for runtime.  We must apply workflow changes first
                    // before we initialize it for runtime.
                    bool createNewDef = (this.workflowChanges != null);
                    rootActivityDefinition = Activity.OnResolveActivityDefinition(this.type, this.workflowMarkup, this.rulesMarkup, createNewDef, !createNewDef, null);
                    if (rootActivityDefinition == null)
                        throw new NullReferenceException(SR.GetString(SR.Error_InvalidRootForWorkflowChanges));

                    if (createNewDef)
                    {
                        ArrayList changeActions = Activity.OnResolveWorkflowChangeActions(this.workflowChanges, rootActivityDefinition);
                        foreach (WorkflowChangeAction changeAction in changeActions)
                        {
                            bool result = changeAction.ApplyTo(rootActivityDefinition);
                            Debug.Assert(result, "ApplyTo failed");
                        }

                        rootActivityDefinition.SetValue(WorkflowChanges.WorkflowChangeActionsProperty, changeActions);
                        rootActivityDefinition.SetValue(WorkflowChanges.WorkflowChangeVersionProperty, this.workflowChangeVersion);
                        ((IDependencyObjectAccessor)rootActivityDefinition).InitializeDefinitionForRuntime(null);
                    }

                    // assign it over to the thread static guy so others can access it as well.
                    Activity.DefinitionActivity = rootActivityDefinition;
                }

                if (this.cachedActivity == null)
                {
                    this.cachedDefinitionActivity = Activity.DefinitionActivity.TraverseDottedPathFromRoot(this.id);
                    this.cachedActivity = (Activity)FormatterServices.GetUninitializedObject(this.cachedDefinitionActivity.GetType());
                }
                return this.cachedActivity;
            }
            void IDeserializationCallback.OnDeserialization(object sender)
            {
                if (this.cachedActivity != null)
                {
                    bool done = false;
                    string[] currentMemberNames = null;
                    MemberInfo[] members = FormatterServicesNoSerializableCheck.GetSerializableMembers(this.cachedActivity.GetType(), out currentMemberNames);
                    if (members.Length == 2)
                    {
                        Debug.Assert(members[0].Name == "dependencyPropertyValues" && members[1].Name == "disposed");
                        // To improve performance, we specialize the case where there are only 2 fields.  One is the 
                        // instance dependency property values dictionary and the other is the "disposed" event.
                        if (this.memberData != null && this.disposed != null)
                        {
                            FormatterServices.PopulateObjectMembers(this.cachedActivity, members, new object[] { this.memberData, this.disposed });
                            done = true;
                        }
                        else if (this.memberData != null)
                        {
                            FormatterServices.PopulateObjectMembers(this.cachedActivity, new MemberInfo[] { members[0] }, new object[] { this.memberData });
                            done = true;
                        }
                        else if (this.disposed != null)
                        {
                            FormatterServices.PopulateObjectMembers(this.cachedActivity, new MemberInfo[] { members[1] }, new object[] { this.disposed });
                            done = true;
                        }
                    }

                    if (!done && this.memberDatas != null)
                    {
                        // re-order the member datas if needed
                        Object[] currentMemberDatas = new object[members.Length];
                        for (int index = 0; index < currentMemberNames.Length; index++)
                            currentMemberDatas[index] = this.memberDatas[Position(currentMemberNames[index])];

                        // populate the object
                        FormatterServices.PopulateObjectMembers(this.cachedActivity, members, currentMemberDatas);
                    }
                    this.cachedActivity.FixUpMetaProperties(this.cachedDefinitionActivity);
                    this.cachedActivity = null;
                }
            }
            private int Position(String name)
            {
                if (this.memberNames.Length > 0 && this.memberNames[this.lastPosition].Equals(name))
                {
                    return this.lastPosition;
                }
                else if ((++this.lastPosition < this.memberNames.Length) && (this.memberNames[this.lastPosition].Equals(name)))
                {
                    return this.lastPosition;
                }
                else
                {
                    // Search for name
                    for (int i = 0; i < this.memberNames.Length; i++)
                    {
                        if (this.memberNames[i].Equals(name))
                        {
                            this.lastPosition = i;
                            return this.lastPosition;
                        }
                    }

                    //throw new SerializationException(String.Format(Environment.GetResourceString("Serialization_MissingMember"),name,objectType));
                    this.lastPosition = 0;
                    return -1;
                }
            }
        }
        [Serializable]
        private class DanglingActivityRef : IObjectReference
        {
            private string id = string.Empty;
            private Type type = null;

            [NonSerialized]
            private Activity activity = null;

            object IObjectReference.GetRealObject(StreamingContext context)
            {
                if (this.activity == null)
                {
                    // meta properties and other instance properties, parent-child relation ships are lost
                    this.activity = (Activity)Activator.CreateInstance(this.type);
                    this.activity.Name = this.id;
                }
                return this.activity;
            }
        }
    }
}
