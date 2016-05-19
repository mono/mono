using System;
using System.Globalization;
using System.Workflow.ComponentModel;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Xml;
using System.Xml.Schema;

namespace System.Workflow.Runtime.Tracking
{
    #region Tracking Exceptions
    /// <summary>
    /// The exception that is thrown when a tracking profile in xml form cannot be deserialized to a TrackingProfile object.
    /// </summary>
    [Serializable]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class TrackingProfileDeserializationException : System.SystemException
    {
        private List<ValidationEventArgs> _args = new List<ValidationEventArgs>();

        public TrackingProfileDeserializationException()
        {
        }

        public TrackingProfileDeserializationException(string message)
            : base(message)
        {
        }

        public TrackingProfileDeserializationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        private TrackingProfileDeserializationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            if (null == info)
                throw new ArgumentNullException("info");

            _args = (List<ValidationEventArgs>)info.GetValue("__TrackingProfileDeserializationException_args__", typeof(List<ValidationEventArgs>));
        }

        //ISerializable override to store custom state
        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (null == info)
                throw new ArgumentNullException("info");

            base.GetObjectData(info, context);
            info.AddValue("__TrackingProfileDeserializationException_args__", _args);
        }

        public IList<ValidationEventArgs> ValidationEventArgs
        {
            get { return _args; }
        }
    }

    #endregion
}
