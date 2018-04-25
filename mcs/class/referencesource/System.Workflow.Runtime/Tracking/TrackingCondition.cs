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
    public abstract class TrackingCondition
    {
        #region Properties

        public abstract string Value { get; set; }

        public abstract string Member { get; set; }

        public abstract ComparisonOperator Operator { get; set; }

        #endregion

        #region Internal Abstract Match Methods

        internal abstract bool Match(object obj);

        #endregion

    }

    /// <summary>
    /// Describes critieria that is used constrain locations.
    /// </summary>
    [Serializable]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class ActivityTrackingCondition : TrackingCondition
    {
        #region Private Data Members

        private string _property;
        private string _val;
        private ComparisonOperator _op = ComparisonOperator.Equals;

        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public ActivityTrackingCondition()
        {
        }

        /// <summary>
        /// Constuct with a list of property names and a value.
        /// </summary>
        /// <param name="propertyName">"." delineated list of property names.</param>
        /// <param name="value">Value for the condition.</param>
        /// <remarks>Throws ArgumentNullException, ArgumentException.</remarks>
        public ActivityTrackingCondition(string member, string value)
        {
            //
            // value can be null but the propery name(s) cannot
            if (null == member)
                throw new ArgumentNullException("member");

            _property = member;

            SetValue(value);
        }

        #endregion

        #region Properties

        public override string Value
        {
            get { return _val; }
            set { SetValue(value); }
        }

        public override string Member
        {
            get { return _property; }
            set { _property = value; }
        }

        public override ComparisonOperator Operator
        {
            get { return _op; }
            set { _op = value; }
        }

        #endregion

        #region Internal Methods

        internal override bool Match(object obj)
        {
            if (null == obj)
                throw new ArgumentNullException("obj");

            object o = PropertyHelper.GetProperty(_property, obj);

            if (ComparisonOperator.Equals == _op)
            {
                if (null == o)
                    return (null == _val);
                else
                    return (0 == string.Compare(o.ToString(), _val, StringComparison.Ordinal));
            }
            else
            {
                if (null == o)
                    return (null != _val);
                else
                    return (0 != string.Compare(o.ToString(), _val, StringComparison.Ordinal));
            }
        }

        #endregion

        #region Private Methods

        private void SetValue(string value)
        {
            _val = value;
        }

        #endregion
    }

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public enum ComparisonOperator
    {
        Equals = 0,
        NotEquals = 1,
    }
}
