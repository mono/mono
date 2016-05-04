using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Globalization;

//using System.Workflow.Activities;
using System.Workflow.ComponentModel;
using System.Workflow.Runtime;
using System.Workflow.Runtime.Hosting;
using Hosting = System.Workflow.Runtime.Hosting;


namespace System.Workflow.Runtime.Tracking
{
    [Serializable]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class TrackingConditionCollection : List<TrackingCondition>
    {
        public TrackingConditionCollection()
        {
        }

        public TrackingConditionCollection(IEnumerable<TrackingCondition> conditions)
        {
            //
            // Not using the IEnumerable<T> constructor on the base List<T> so that we can check for null.
            // The code behind AddRange doesn't appear to have a significant perf 
            // overhead compared to the IEnumerable<T> constructor if the list is empty
            // (which it will always be at this point).
            if (null == conditions)
                throw new ArgumentNullException("conditions");

            AddRange(conditions);
        }
    }
}
