using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;
using System.Runtime;

namespace System.ServiceModel.Security
{
    public abstract class NonceCache
    {
        TimeSpan cachingTime;
        int maxCachedNonces;

        /// <summary>
        /// TThe max timespan after which a Nonce is deleted from the NonceCache. This value should be atleast twice the maxclock Skew added to the replayWindow size.
        /// </summary>
        public TimeSpan CachingTimeSpan
        {
            get
            {
                return this.cachingTime;
            }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.SFxTimeoutOutOfRange0)));
                }

                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.SFxTimeoutOutOfRangeTooBig)));
                }

                this.cachingTime = value;
            }
        }

        /// <summary>
        /// The maximum size of the NonceCache.
        /// </summary>
        public int CacheSize
        {
            get
            {
                return this.maxCachedNonces;
            }
            set
            {
                if (value < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                                                    SR.GetString(SR.ValueMustBeNonNegative)));
                }
                this.maxCachedNonces = value;

            }
        }

        public abstract bool TryAddNonce(byte[] nonce);
        public abstract bool CheckNonce(byte[] nonce);
    }
}
