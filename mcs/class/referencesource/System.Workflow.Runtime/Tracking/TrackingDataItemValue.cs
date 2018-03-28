using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Reflection;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.ComponentModel.Design.Serialization;

using System.Workflow.Runtime;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Serialization;
using System.Workflow.Runtime.Hosting;

namespace System.Workflow.Runtime.Tracking
{
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class TrackingDataItemValue
    {
        private string _name = null;
        private string _value = null;
        private string _id = null;

        public TrackingDataItemValue() { }

        public TrackingDataItemValue(string qualifiedName, string fieldName, string dataValue)
        {
            _name = fieldName;
            _value = dataValue;
            _id = qualifiedName;
        }

        public string FieldName
        {
            get { return _name; }
            set { _name = value; }
        }
        public string DataValue
        {
            get { return _value; }
            set { _value = value; }
        }
        public string QualifiedName
        {
            get { return _id; }
            set { _id = value; }
        }
    }
}
