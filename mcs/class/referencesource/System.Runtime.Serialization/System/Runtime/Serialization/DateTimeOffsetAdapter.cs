//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.Runtime.Serialization
{
    using System;
    using System.Reflection;
    using System.Xml;
    using System.Globalization;

    [DataContract(Name = "DateTimeOffset", Namespace = "http://schemas.datacontract.org/2004/07/System")]
#if USE_REFEMIT
    public struct DateTimeOffsetAdapter
#else
    internal struct DateTimeOffsetAdapter
#endif
    {
        DateTime utcDateTime;
        short offsetMinutes;

        public DateTimeOffsetAdapter(DateTime dateTime, short offsetMinutes)
        {
            this.utcDateTime = dateTime;
            this.offsetMinutes = offsetMinutes;
        }

        [DataMember(Name = "DateTime", IsRequired = true)]
        public DateTime UtcDateTime
        {
            get { return utcDateTime; }
            set { utcDateTime = value; }
        }

        [DataMember(Name = "OffsetMinutes", IsRequired = true)]
        public short OffsetMinutes
        {
            get { return offsetMinutes; }
            set { offsetMinutes = value; }
        }

        public static DateTimeOffset GetDateTimeOffset(DateTimeOffsetAdapter value)
        {
            try
            {
                switch (value.UtcDateTime.Kind)
                {
                    case DateTimeKind.Unspecified:
                        return new DateTimeOffset(value.UtcDateTime, new TimeSpan(0, value.OffsetMinutes, 0));

                    //DateTimeKind.Utc and DateTimeKind.Local
                    //Read in deserialized DateTime portion of the DateTimeOffsetAdapter and convert DateTimeKind to Unspecified.
                    //Apply ofset information read from OffsetMinutes portion of the DateTimeOffsetAdapter.
                    //Return converted DateTimeoffset object.
                    default:
                        DateTimeOffset deserialized = new DateTimeOffset(value.UtcDateTime);
                        return deserialized.ToOffset(new TimeSpan(0, value.OffsetMinutes, 0));
                }
            }
            catch (ArgumentException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(value.ToString(CultureInfo.InvariantCulture), "DateTimeOffset", exception));
            }
        }

        public static DateTimeOffsetAdapter GetDateTimeOffsetAdapter(DateTimeOffset value)
        {
            return new DateTimeOffsetAdapter(value.UtcDateTime, (short)value.Offset.TotalMinutes);
        }

        public string ToString(IFormatProvider provider)
        {
            return "DateTime: " + this.UtcDateTime + ", Offset: " + this.OffsetMinutes;
        }

    }
}
