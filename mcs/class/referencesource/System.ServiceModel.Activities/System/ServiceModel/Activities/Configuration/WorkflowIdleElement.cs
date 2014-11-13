//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Activities.Configuration
{
    using System.ComponentModel;
    using System.Configuration;
    using System.Globalization;
    using System.Runtime;
    using System.ServiceModel.Activities.Description;
    using System.ServiceModel.Configuration;

    public sealed class WorkflowIdleElement : BehaviorExtensionElement
    {
        ConfigurationPropertyCollection properties;
        const string TimeToPersistString = "timeToPersist";
        const string TimeToUnloadString = "timeToUnload";

        public WorkflowIdleElement()
        {
        }

        [ConfigurationProperty(TimeToPersistString, DefaultValue = WorkflowIdleBehavior.defaultTimeToPersistString)]
        [TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [ServiceModelTimeSpanValidator(MinValueString = ConfigurationStrings.TimeSpanZero)]
        public TimeSpan TimeToPersist
        {
            get { return (TimeSpan)base[TimeToPersistString]; }
            set { base[TimeToPersistString] = value; }
        }

        [ConfigurationProperty(TimeToUnloadString, DefaultValue = WorkflowIdleBehavior.defaultTimeToUnloadString)]
        [TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [ServiceModelTimeSpanValidator(MinValueString = ConfigurationStrings.TimeSpanZero)]
        public TimeSpan TimeToUnload
        {
            get { return (TimeSpan)base[TimeToUnloadString]; }
            set { base[TimeToUnloadString] = value; }
        }

        protected internal override object CreateBehavior()
        {
            return new WorkflowIdleBehavior() 
            { TimeToPersist = this.TimeToPersist, TimeToUnload = this.TimeToUnload };
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Configuration", "Configuration102:ConfigurationPropertyAttributeRule", MessageId = "System.ServiceModel.Activities.Configuration.WorkflowIdleElement.BehaviorType", Justification = "Not a configurable property; a property that had to be overridden from abstract parent class")]
        public override Type BehaviorType
        {
            get { return typeof(WorkflowIdleBehavior); }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty(TimeToPersistString, typeof(TimeSpan), TimeSpan.MaxValue, new TimeSpanOrInfiniteConverter(), new TimeSpanOrInfiniteValidator(TimeSpan.Zero, TimeSpan.MaxValue), ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty(TimeToUnloadString, typeof(TimeSpan), TimeSpan.Parse(WorkflowIdleBehavior.defaultTimeToUnloadString, CultureInfo.InvariantCulture), new TimeSpanOrInfiniteConverter(), new TimeSpanOrInfiniteValidator(TimeSpan.Zero, TimeSpan.MaxValue), ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }

    }
}




