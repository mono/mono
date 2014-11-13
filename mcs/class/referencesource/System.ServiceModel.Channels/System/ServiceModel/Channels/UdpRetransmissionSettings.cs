//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime;

    public sealed class UdpRetransmissionSettings
    {
        int maxUnicastRetransmitCount;
        int maxMulticastRetransmitCount;
        TimeSpan delayLowerBound;
        TimeSpan delayUpperBound;
        TimeSpan maxDelayPerRetransmission;
        int delayLowerBoundMilliseconds;
        int delayUpperBoundMilliseconds;
        int maxDelayMilliseconds;

        //this constructor disables retransmission
        public UdpRetransmissionSettings()
            : this(0, 0)
        {
        }

        public UdpRetransmissionSettings(int maxUnicastRetransmitCount, int maxMulticastRetransmitCount)
            : this(maxUnicastRetransmitCount, maxMulticastRetransmitCount, 
            UdpConstants.Defaults.DelayLowerBoundTimeSpan, UdpConstants.Defaults.DelayUpperBoundTimeSpan, UdpConstants.Defaults.MaxDelayPerRetransmissionTimeSpan)
        {
        }

        public UdpRetransmissionSettings(int maxUnicastRetransmitCount, int maxMulticastRetransmitCount, TimeSpan delayLowerBound, TimeSpan delayUpperBound, TimeSpan maxDelayPerRetransmission)
        {
            if (maxUnicastRetransmitCount < 0)
            {
                throw FxTrace.Exception.ArgumentOutOfRange("maxUnicastRetransmitCount", maxUnicastRetransmitCount,
                    SR.ArgumentOutOfMinRange(0));
            }

            if (maxMulticastRetransmitCount < 0)
            {
                throw FxTrace.Exception.ArgumentOutOfRange("maxMulticastRetransmitCount", maxMulticastRetransmitCount,
                    SR.ArgumentOutOfMinRange(0));
            }


            if (delayLowerBound < TimeSpan.Zero)
            {
                throw FxTrace.Exception.ArgumentOutOfRange("delayLowerBound", delayLowerBound, SR.ArgumentOutOfMinRange(TimeSpan.Zero));
            }

            if (delayUpperBound < TimeSpan.Zero)
            {
                throw FxTrace.Exception.ArgumentOutOfRange("delayUpperBound", delayUpperBound, SR.ArgumentOutOfMinRange(TimeSpan.Zero));
            }

            if (maxDelayPerRetransmission < TimeSpan.Zero)
            {
                throw FxTrace.Exception.ArgumentOutOfRange("maxDelayPerRetransmission", maxDelayPerRetransmission,
                    SR.ArgumentOutOfMinRange(TimeSpan.Zero));
            }

            this.maxUnicastRetransmitCount = maxUnicastRetransmitCount;
            this.maxMulticastRetransmitCount = maxMulticastRetransmitCount;
            this.delayLowerBound = delayLowerBound;
            this.delayUpperBound = delayUpperBound;
            this.maxDelayPerRetransmission = maxDelayPerRetransmission;

            this.delayLowerBoundMilliseconds = TimeoutHelper.ToMilliseconds(this.delayLowerBound);
            this.delayUpperBoundMilliseconds = TimeoutHelper.ToMilliseconds(this.delayUpperBound);
            this.maxDelayMilliseconds = TimeoutHelper.ToMilliseconds(this.maxDelayPerRetransmission);

            ValidateSettings();
        }

        UdpRetransmissionSettings(UdpRetransmissionSettings other)
            : this(other.maxUnicastRetransmitCount, other.maxMulticastRetransmitCount, other.delayLowerBound, other.delayUpperBound, other.maxDelayPerRetransmission)
        {
        }

        [DefaultValue(UdpConstants.Defaults.MaxUnicastRetransmitCount)]
        public int MaxUnicastRetransmitCount
        {
            get
            {
                return this.maxUnicastRetransmitCount;
            }
            set
            {
                const int min = 0;
                if (value < min)
                {
                    throw FxTrace.Exception.ArgumentOutOfRange("value", value, SR.ArgumentOutOfMinRange(min));
                }
                this.maxUnicastRetransmitCount = value;
            }
        }

        [DefaultValue(UdpConstants.Defaults.MaxMulticastRetransmitCount)]
        public int MaxMulticastRetransmitCount
        {
            get
            {
                return this.maxMulticastRetransmitCount;
            }
            set
            {
                const int min = 0;
                if (value < min)
                {
                    throw FxTrace.Exception.ArgumentOutOfRange("value", value, SR.ArgumentOutOfMinRange(min));
                }
                this.maxMulticastRetransmitCount = value;
            }
        }

        public TimeSpan DelayLowerBound
        {
            get
            {
                return this.delayLowerBound;
            }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw FxTrace.Exception.ArgumentOutOfRange("value", value, SR.ArgumentOutOfMinRange(TimeSpan.Zero));
                }

                this.delayLowerBound = value;
                this.delayLowerBoundMilliseconds = TimeoutHelper.ToMilliseconds(this.delayLowerBound);
            }
        }

        public TimeSpan DelayUpperBound
        {
            get
            {
                return this.delayUpperBound;
            }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw FxTrace.Exception.ArgumentOutOfRange("value", value, SR.ArgumentOutOfMinRange(TimeSpan.Zero));
                }

                this.delayUpperBound = value;
                this.delayUpperBoundMilliseconds = TimeoutHelper.ToMilliseconds(this.delayUpperBound);
            }
        }

        public TimeSpan MaxDelayPerRetransmission
        {
            get
            {
                return this.maxDelayPerRetransmission;
            }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw FxTrace.Exception.ArgumentOutOfRange("value", value, SR.ArgumentOutOfMinRange(TimeSpan.Zero));
                }

                this.maxDelayPerRetransmission = value;
                this.maxDelayMilliseconds = TimeoutHelper.ToMilliseconds(this.maxDelayPerRetransmission);
            }
        }
        
        public bool ShouldSerializeDelayLowerBound()
        {
            return !TimeSpansEqual(this.delayLowerBound, UdpConstants.Defaults.DelayLowerBoundTimeSpan);
        }

        public bool ShouldSerializeDelayUpperBound()
        {
            return !TimeSpansEqual(this.delayUpperBound, UdpConstants.Defaults.DelayUpperBoundTimeSpan);
        }

        public bool ShouldSerializeMaxDelayPerRetransmission()
        {
            return !TimeSpansEqual(this.maxDelayPerRetransmission, UdpConstants.Defaults.MaxDelayPerRetransmissionTimeSpan);
        }


        //called at send time to avoid repeated rounding and casting
        internal int GetDelayLowerBound()
        {
            return this.delayLowerBoundMilliseconds;
        }

        //called at send time to avoid repeated rounding and casting
        internal int GetDelayUpperBound()
        {
            return this.delayUpperBoundMilliseconds;
        }

        //called at send time to avoid repeated rounding and casting
        internal int GetMaxDelayPerRetransmission()
        {
            return this.maxDelayMilliseconds;
        }

        internal bool Enabled
        { 
            get 
            { 
                return this.maxUnicastRetransmitCount > 0 || this.maxMulticastRetransmitCount > 0; 
            } 
        }

        internal void ValidateSettings()
        {
            if (this.delayLowerBound > this.delayUpperBound)
            {
                throw FxTrace.Exception.ArgumentOutOfRange("DelayLowerBound", this.delayLowerBound, SR.Property1LessThanOrEqualToProperty2("DelayLowerBound", this.delayLowerBound, "DelayUpperBound", this.delayUpperBound));
            }


            if (this.delayUpperBound > this.maxDelayPerRetransmission)
            {
                throw FxTrace.Exception.ArgumentOutOfRange("DelayUpperBound", this.delayUpperBound, SR.Property1LessThanOrEqualToProperty2("DelayUpperBound", this.delayUpperBound, "MaxDelayPerRetransmission", this.maxDelayPerRetransmission));
            }
        }

        internal UdpRetransmissionSettings Clone()
        {
            return new UdpRetransmissionSettings(this);
        }

        internal bool IsMatch(UdpRetransmissionSettings udpRetransmissionSettings)
        {
            if (this.DelayLowerBound != udpRetransmissionSettings.DelayLowerBound)
            {
                return false;
            }

            if (this.DelayUpperBound != udpRetransmissionSettings.DelayUpperBound)
            {
                return false;
            }

            if (this.MaxDelayPerRetransmission != udpRetransmissionSettings.MaxDelayPerRetransmission)
            {
                return false;
            }

            if (this.MaxMulticastRetransmitCount != udpRetransmissionSettings.MaxMulticastRetransmitCount)
            {
                return false;
            }

            if (this.MaxUnicastRetransmitCount != udpRetransmissionSettings.MaxUnicastRetransmitCount)
            {
                return false;
            }

            return true;
        }

        bool TimeSpansEqual(TimeSpan ts1, TimeSpan ts2)
        {
            long diff = Math.Abs(ts1.Ticks - ts2.Ticks);
            long max = Math.Max(Math.Abs(ts1.Ticks), Math.Abs(ts2.Ticks));
            return diff < TimeSpan.FromMilliseconds(1).Ticks || (max > 0 && diff / (double) max < 1e-3);
        }
    }
}
