//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Runtime
{
    using System;
    using System.Activities.DynamicUpdate;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Collections.ObjectModel;

    [DataContract]
    class ActivityInstanceReference : ActivityInstanceMap.IActivityReference
    {
        ActivityInstance activityInstance;

        internal ActivityInstanceReference(ActivityInstance activity)
        {
            this.activityInstance = activity;
        }

        [DataMember(Name = "activityInstance")]
        internal ActivityInstance SerializedActivityInstance
        {
            get { return this.activityInstance; }
            set { this.activityInstance = value; }
        }

        Activity ActivityInstanceMap.IActivityReference.Activity
        {
            get
            {
                return this.activityInstance.Activity;
            }
        }


        public ActivityInstance ActivityInstance
        {
            get
            {
                return this.activityInstance;
            }
        }

        void ActivityInstanceMap.IActivityReference.Load(Activity activity, ActivityInstanceMap instanceMap)
        {
            // The conditional calling of ActivityInstance.Load is the value
            // added by this wrapper class.  This is because we can't guarantee
            // that multiple activities won't have a reference to the same
            // ActivityInstance.
            if (this.activityInstance.Activity == null)
            {
                ((ActivityInstanceMap.IActivityReference)this.activityInstance).Load(activity, instanceMap);
            }
        }
    }
}


