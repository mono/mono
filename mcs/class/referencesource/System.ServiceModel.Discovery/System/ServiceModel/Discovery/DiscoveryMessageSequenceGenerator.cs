//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel.Discovery
{
    using System;
    using System.Runtime;
    using SR2 = System.ServiceModel.Discovery.SR;

    public class DiscoveryMessageSequenceGenerator
    {
        static readonly DateTime DT1970 = new DateTime(1970, 1, 1);

        long instanceId;
        Uri sequenceId;

        [Fx.Tag.SynchronizationObject(Blocking = false, Kind = Fx.Tag.SynchronizationKind.InterlockedNoSpin)]
        long messageNumber;

        public DiscoveryMessageSequenceGenerator()
            : this(CreateInstanceId(), null)
        {
        }

        [Fx.Tag.Throws(typeof(ArgumentOutOfRangeException), "instanceId")]
        public DiscoveryMessageSequenceGenerator(long instanceId, Uri sequenceId)
        {
            if (instanceId < 0 || instanceId > UInt32.MaxValue)
            {
                throw FxTrace.Exception.ArgumentOutOfRange("instanceId", instanceId, SR2.DiscoveryAppSequenceInstanceIdOutOfRange);
            }
            this.instanceId = instanceId;
            this.sequenceId = sequenceId;
        }

        static long CreateInstanceId()
        {
            return (long)DateTime.Now.Subtract(DT1970).TotalSeconds;
        }

        [Fx.Tag.InheritThrows(From = "DiscoveryMessageSequenceGenerator", FromDeclaringType = typeof(DiscoveryMessageSequenceGenerator))]
        public DiscoveryMessageSequence Next()
        {
            return new DiscoveryMessageSequence(this.instanceId, this.sequenceId, Threading.Interlocked.Increment(ref this.messageNumber));
        }
    }
}
