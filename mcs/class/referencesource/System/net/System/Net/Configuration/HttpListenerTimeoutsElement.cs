
using System.Configuration;

namespace System.Net.Configuration
{
    public sealed class HttpListenerTimeoutsElement : ConfigurationElement
    {
        private static ConfigurationPropertyCollection properties;
        private static readonly ConfigurationProperty entityBody;
        private static readonly ConfigurationProperty drainEntityBody;
        private static readonly ConfigurationProperty requestQueue;
        private static readonly ConfigurationProperty idleConnection;
        private static readonly ConfigurationProperty headerWait;
        private static readonly ConfigurationProperty minSendBytesPerSecond;

        static HttpListenerTimeoutsElement()
        {
            entityBody = CreateTimeSpanProperty(ConfigurationStrings.EntityBody);
            drainEntityBody = CreateTimeSpanProperty(ConfigurationStrings.DrainEntityBody);
            requestQueue = CreateTimeSpanProperty(ConfigurationStrings.RequestQueue);
            idleConnection = CreateTimeSpanProperty(ConfigurationStrings.IdleConnection);
            headerWait = CreateTimeSpanProperty(ConfigurationStrings.HeaderWait);
            
            minSendBytesPerSecond = new ConfigurationProperty(ConfigurationStrings.MinSendBytesPerSecond, 
                typeof(long), 0L, null, new LongValidator(), ConfigurationPropertyOptions.None);

            properties = new ConfigurationPropertyCollection();
            properties.Add(entityBody);
            properties.Add(drainEntityBody);
            properties.Add(requestQueue);
            properties.Add(idleConnection);
            properties.Add(headerWait);
            properties.Add(minSendBytesPerSecond);
        }

        private static ConfigurationProperty CreateTimeSpanProperty(string name)
        {
            return new ConfigurationProperty(name, typeof(TimeSpan), TimeSpan.Zero, null, new TimeSpanValidator(),
                ConfigurationPropertyOptions.None);
        }

        [ConfigurationProperty(ConfigurationStrings.EntityBody, DefaultValue = 0, IsRequired = false)]
        public TimeSpan EntityBody
        {
            get { return (TimeSpan)this[entityBody]; }
        }

        [ConfigurationProperty(ConfigurationStrings.DrainEntityBody, DefaultValue = 0, IsRequired = false)]
        public TimeSpan DrainEntityBody
        {
            get { return (TimeSpan)this[drainEntityBody]; }
        }

        [ConfigurationProperty(ConfigurationStrings.RequestQueue, DefaultValue = 0, IsRequired = false)]
        public TimeSpan RequestQueue
        {
            get { return (TimeSpan)this[requestQueue]; }
        }

        [ConfigurationProperty(ConfigurationStrings.IdleConnection, DefaultValue = 0, IsRequired = false)]
        public TimeSpan IdleConnection
        {
            get { return (TimeSpan)this[idleConnection]; }
        }

        [ConfigurationProperty(ConfigurationStrings.HeaderWait, DefaultValue = 0, IsRequired = false)]
        public TimeSpan HeaderWait
        {
            get { return (TimeSpan)this[headerWait]; }
        }
        
        [ConfigurationProperty(ConfigurationStrings.MinSendBytesPerSecond, DefaultValue = 0L, IsRequired = false)]
        public long MinSendBytesPerSecond
        {
            get { return (long)this[minSendBytesPerSecond]; }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get { return properties; }
        }

        internal long[] GetTimeouts()
        {
            long[] timeouts = new long[6];
            timeouts[0] = Convert.ToInt64(EntityBody.TotalSeconds);
            timeouts[1] = Convert.ToInt64(DrainEntityBody.TotalSeconds);
            timeouts[2] = Convert.ToInt64(RequestQueue.TotalSeconds);
            timeouts[3] = Convert.ToInt64(IdleConnection.TotalSeconds);
            timeouts[4] = Convert.ToInt64(HeaderWait.TotalSeconds);
            timeouts[5] = MinSendBytesPerSecond;
            return timeouts;
        }

        private class TimeSpanValidator : ConfigurationValidatorBase
        {
            public override bool CanValidate(Type type)
            {
                return type == typeof(TimeSpan);
            }

            public override void Validate(object value)
            {
                TimeSpan timeout = (TimeSpan)value;

                // We don't worry about rounding error if they specified something smaller than seconds.
                Int64 seconds = Convert.ToInt64(timeout.TotalSeconds);

                // All timeouts are defined as USHORT in native layer. Make sure that timeout value is within range.
                if (seconds < 0 || seconds > ushort.MaxValue)
                {
                    throw new ArgumentOutOfRangeException("value", timeout,
                        SR.GetString(SR.ArgumentOutOfRange_Bounds_Lower_Upper, "0:0:0", "18:12:15"));
                }
            }
        }

        private class LongValidator : ConfigurationValidatorBase
        {
            public override bool CanValidate(Type type)
            {
                return type == typeof(long);
            }

            public override void Validate(object value)
            {
                long input = (long)value;

                // We need to use long as the public CLS compliant value, but really this is a UInt32
                if (input < 0 || input > UInt32.MaxValue)
                {
                    throw new ArgumentOutOfRangeException("value", input, 
                        SR.GetString(SR.ArgumentOutOfRange_Bounds_Lower_Upper, 0, UInt32.MaxValue));
                }
            }
        }
    }
}

