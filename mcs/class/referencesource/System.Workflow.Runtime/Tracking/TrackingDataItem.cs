using System;
using System.Collections;
using System.Collections.Generic;
using System.Workflow.ComponentModel;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Workflow.Runtime.Hosting;

namespace System.Workflow.Runtime.Tracking
{
    /// <summary>
    /// Contain a single piece of tracking data and any associated annotations.
    /// </summary>
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class TrackingDataItem
    {
        private TrackingAnnotationCollection _annotations = new TrackingAnnotationCollection();
        private object _data = null;
        private string _fieldName = null;

        public TrackingDataItem()
        {
        }

        public TrackingAnnotationCollection Annotations
        {
            get { return _annotations; }
            //set{ _annotations = value; }
        }

        public string FieldName
        {
            get { return _fieldName; }
            set { _fieldName = value; }
        }

        public object Data
        {
            get { return _data; }
            set { _data = value; }
        }
    }
}
