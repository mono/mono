//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.Globalization;
    using System.Runtime;
    using System.Xml;

    sealed class SecurityTimestamp
    {
        const string DefaultFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
        //                            012345678901234567890123

        internal static readonly TimeSpan defaultTimeToLive = SecurityProtocolFactory.defaultTimestampValidityDuration;
        char[] computedCreationTimeUtc;
        char[] computedExpiryTimeUtc;
        DateTime creationTimeUtc;
        DateTime expiryTimeUtc;
        readonly string id;
        readonly string digestAlgorithm;
        readonly byte[] digest;

        public SecurityTimestamp(DateTime creationTimeUtc, DateTime expiryTimeUtc, string id)
            : this(creationTimeUtc, expiryTimeUtc, id, null, null)
        {
        }

        internal SecurityTimestamp(DateTime creationTimeUtc, DateTime expiryTimeUtc, string id, string digestAlgorithm, byte[] digest)
        {
            Fx.Assert(creationTimeUtc.Kind == DateTimeKind.Utc, "creation time must be in UTC");
            Fx.Assert(expiryTimeUtc.Kind == DateTimeKind.Utc, "expiry time must be in UTC");

            if (creationTimeUtc > expiryTimeUtc)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new ArgumentOutOfRangeException("recordedExpiryTime", SR.GetString(SR.CreationTimeUtcIsAfterExpiryTime)));
            }

            this.creationTimeUtc = creationTimeUtc;
            this.expiryTimeUtc = expiryTimeUtc;
            this.id = id;

            this.digestAlgorithm = digestAlgorithm;
            this.digest = digest;
        }

        public DateTime CreationTimeUtc
        {
            get
            {
                return this.creationTimeUtc;
            }
        }

        public DateTime ExpiryTimeUtc
        {
            get
            {
                return this.expiryTimeUtc;
            }
        }

        public string Id
        {
            get
            {
                return this.id;
            }
        }

        public string DigestAlgorithm
        {
            get
            {
                return this.digestAlgorithm;
            }
        }

        internal byte[] GetDigest()
        {
            return this.digest;
        }

        internal char[] GetCreationTimeChars()
        {
            if (this.computedCreationTimeUtc == null)
            {
                this.computedCreationTimeUtc = ToChars(ref this.creationTimeUtc);
            }
            return this.computedCreationTimeUtc;
        }

        internal char[] GetExpiryTimeChars()
        {
            if (this.computedExpiryTimeUtc == null)
            {
                this.computedExpiryTimeUtc = ToChars(ref this.expiryTimeUtc);
            }
            return this.computedExpiryTimeUtc;
        }

        static char[] ToChars(ref DateTime utcTime)
        {
            char[] buffer = new char[DefaultFormat.Length];
            int offset = 0;

            ToChars(utcTime.Year, buffer, ref offset, 4);
            buffer[offset++] = '-';

            ToChars(utcTime.Month, buffer, ref offset, 2);
            buffer[offset++] = '-';

            ToChars(utcTime.Day, buffer, ref offset, 2);
            buffer[offset++] = 'T';

            ToChars(utcTime.Hour, buffer, ref offset, 2);
            buffer[offset++] = ':';

            ToChars(utcTime.Minute, buffer, ref offset, 2);
            buffer[offset++] = ':';

            ToChars(utcTime.Second, buffer, ref offset, 2);
            buffer[offset++] = '.';

            ToChars(utcTime.Millisecond, buffer, ref offset, 3);
            buffer[offset++] = 'Z';

            return buffer;
        }

        static void ToChars(int n, char[] buffer, ref int offset, int count)
        {
            for (int i = offset + count - 1; i >= offset; i--)
            {
                buffer[i] = (char)('0' + (n % 10));
                n /= 10;
            }
            Fx.Assert(n == 0, "Overflow in encoding timestamp field");
            offset += count;
        }

        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "SecurityTimestamp: Id={0}, CreationTimeUtc={1}, ExpirationTimeUtc={2}",
                this.Id,
                XmlConvert.ToString(this.CreationTimeUtc, XmlDateTimeSerializationMode.RoundtripKind),
                XmlConvert.ToString(this.ExpiryTimeUtc, XmlDateTimeSerializationMode.RoundtripKind));
        }

        /// <summary>
        /// Internal method that checks if the timestamp is fresh with respect to the
        /// timeToLive and allowedClockSkew values passed in.
        /// Throws if the timestamp is stale.
        /// </summary>
        /// <param name="timeToLive"></param>
        /// <param name="allowedClockSkew"></param>
        internal void ValidateRangeAndFreshness(TimeSpan timeToLive, TimeSpan allowedClockSkew)
        {
            // Check that the creation time is less than expiry time
            if (this.CreationTimeUtc >= this.ExpiryTimeUtc)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.TimeStampHasCreationAheadOfExpiry, this.CreationTimeUtc.ToString(DefaultFormat, CultureInfo.CurrentCulture), this.ExpiryTimeUtc.ToString(DefaultFormat, CultureInfo.CurrentCulture))));
            }

            ValidateFreshness(timeToLive, allowedClockSkew);
        }

        internal void ValidateFreshness(TimeSpan timeToLive, TimeSpan allowedClockSkew)
        {
            DateTime now = DateTime.UtcNow;
            // check that the message has not expired
            if (this.ExpiryTimeUtc <= TimeoutHelper.Subtract(now, allowedClockSkew))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.TimeStampHasExpiryTimeInPast, this.ExpiryTimeUtc.ToString(DefaultFormat, CultureInfo.CurrentCulture), now.ToString(DefaultFormat, CultureInfo.CurrentCulture), allowedClockSkew)));
            }

            // check that creation time is not in the future (modulo clock skew)
            if (this.CreationTimeUtc >= TimeoutHelper.Add(now, allowedClockSkew))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.TimeStampHasCreationTimeInFuture, this.CreationTimeUtc.ToString(DefaultFormat, CultureInfo.CurrentCulture), now.ToString(DefaultFormat, CultureInfo.CurrentCulture), allowedClockSkew)));
            }

            // check that the creation time is not more than timeToLive in the past
            if (this.CreationTimeUtc <= TimeoutHelper.Subtract(now, TimeoutHelper.Add(timeToLive, allowedClockSkew)))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.TimeStampWasCreatedTooLongAgo, this.CreationTimeUtc.ToString(DefaultFormat, CultureInfo.CurrentCulture), now.ToString(DefaultFormat, CultureInfo.CurrentCulture), timeToLive, allowedClockSkew)));
            }

            // this is a fresh timestamp
        }
    }
}
