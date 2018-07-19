//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;

    class SynchronizedRandom : Random
    {

        public SynchronizedRandom()
            : base()
        {
            this.ThisLock = new object();
        }

        public SynchronizedRandom(int seed)
            : base(seed)
        {
            this.ThisLock = new object();
        }

        protected object ThisLock
        {
            get;
            private set;
        }

        public override int Next(int minValue, int maxValue)
        {
            lock (this.ThisLock)
            {
                return base.Next(minValue, maxValue);
            }
        }

        public override int Next()
        {
            lock (this.ThisLock)
            {
                return base.Next();
            }
        }

        public override int Next(int maxValue)
        {
            lock (this.ThisLock)
            {
                return base.Next(maxValue);
            }
        }

        public override void NextBytes(byte[] buffer)
        {
            lock (this.ThisLock)
            {
                base.NextBytes(buffer);
            }
        }

        public override double NextDouble()
        {
            lock (this.ThisLock)
            {
                return base.NextDouble();
            }
        }

        protected override double Sample()
        {
            lock (this.ThisLock)
            {
                return base.Sample();
            }
        }
    }
}
