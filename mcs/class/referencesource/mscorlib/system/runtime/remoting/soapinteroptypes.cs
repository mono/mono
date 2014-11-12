// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
**  File:    SoapInteropTypes.cs
** 
**  Purpose: Types for Wsdl and Soap interop
**
**
===========================================================*/

namespace System.Runtime.Remoting.Metadata.W3cXsd2001
{
    using System;
    using System.Globalization;
    using System.Text;


    internal static class SoapType
    {
        internal static String FilterBin64(String value)
        {
            StringBuilder sb = new StringBuilder();
            for (int i=0; i<value.Length; i++)
            {
                if (!(value[i] == ' '|| value[i] == '\n' || value[i] == '\r'))
                    sb.Append(value[i]);
            }
            return sb.ToString();
        }

        internal static String LineFeedsBin64(String value)
        {
            // Add linefeeds every 76 characters
            StringBuilder sb = new StringBuilder();
            for (int i=0; i<value.Length; i++)
            {
                if (i%76 == 0)
                    sb.Append('\n');
                sb.Append(value[i]);
            }
            return sb.ToString();
        }

        internal static String Escape(String value)
        {
            if (value == null || value.Length == 0)
                return value;
                
            StringBuilder stringBuffer = new StringBuilder();
            int index = value.IndexOf('&');
            if (index > -1)
            {
                stringBuffer.Append(value);
                stringBuffer.Replace("&", "&#38;", index, stringBuffer.Length - index);
            }

            index = value.IndexOf('"');
            if (index > -1)
            {
                if (stringBuffer.Length == 0)
                    stringBuffer.Append(value);
                stringBuffer.Replace("\"", "&#34;", index, stringBuffer.Length - index);
            }

            index = value.IndexOf('\'');
            if (index > -1)
            {
                if (stringBuffer.Length == 0)
                    stringBuffer.Append(value);
                stringBuffer.Replace("\'", "&#39;", index, stringBuffer.Length - index);
            }

            index = value.IndexOf('<');
            if (index > -1)
            {
                if (stringBuffer.Length == 0)
                    stringBuffer.Append(value);
                stringBuffer.Replace("<", "&#60;", index, stringBuffer.Length - index);
            }

            index = value.IndexOf('>');
            if (index > -1)
            {
                if (stringBuffer.Length == 0)
                    stringBuffer.Append(value);
                stringBuffer.Replace(">", "&#62;", index, stringBuffer.Length - index);
            }

            index = value.IndexOf(Char.MinValue);
            if (index > -1)
            {
                if (stringBuffer.Length == 0)
                    stringBuffer.Append(value);
                stringBuffer.Replace(Char.MinValue.ToString(), "&#0;", index, stringBuffer.Length - index);
            }

            String returnValue = null;

            if (stringBuffer.Length > 0)
                returnValue = stringBuffer.ToString();
            else
                returnValue = value;

            return returnValue;
        }



        internal static Type typeofSoapTime = typeof(SoapTime);
        internal static Type typeofSoapDate = typeof(SoapDate);
        internal static Type typeofSoapYearMonth = typeof(SoapYearMonth);
        internal static Type typeofSoapYear = typeof(SoapYear);
        internal static Type typeofSoapMonthDay = typeof(SoapMonthDay);
        internal static Type typeofSoapDay = typeof(SoapDay);
        internal static Type typeofSoapMonth = typeof(SoapMonth);
        internal static Type typeofSoapHexBinary = typeof(SoapHexBinary);
        internal static Type typeofSoapBase64Binary = typeof(SoapBase64Binary);
        internal static Type typeofSoapInteger = typeof(SoapInteger);
        internal static Type typeofSoapPositiveInteger = typeof(SoapPositiveInteger);
        internal static Type typeofSoapNonPositiveInteger = typeof(SoapNonPositiveInteger);
        internal static Type typeofSoapNonNegativeInteger = typeof(SoapNonNegativeInteger);
        internal static Type typeofSoapNegativeInteger = typeof(SoapNegativeInteger);
        internal static Type typeofSoapAnyUri = typeof(SoapAnyUri);
        internal static Type typeofSoapQName = typeof(SoapQName);
        internal static Type typeofSoapNotation = typeof(SoapNotation);
        internal static Type typeofSoapNormalizedString = typeof(SoapNormalizedString);
        internal static Type typeofSoapToken = typeof(SoapToken);
        internal static Type typeofSoapLanguage = typeof(SoapLanguage);
        internal static Type typeofSoapName = typeof(SoapName);
        internal static Type typeofSoapIdrefs = typeof(SoapIdrefs);
        internal static Type typeofSoapEntities = typeof(SoapEntities);
        internal static Type typeofSoapNmtoken = typeof(SoapNmtoken);
        internal static Type typeofSoapNmtokens = typeof(SoapNmtokens);
        internal static Type typeofSoapNcName = typeof(SoapNcName);
        internal static Type typeofSoapId = typeof(SoapId);
        internal static Type typeofSoapIdref = typeof(SoapIdref);
        internal static Type typeofSoapEntity = typeof(SoapEntity);    
        internal static Type typeofISoapXsd = typeof(ISoapXsd);    
    }


[System.Runtime.InteropServices.ComVisible(true)]
    public interface ISoapXsd
    {

        String GetXsdType();
    }

    // Soap interop xsd types
    //Convert from ISO Date to urt DateTime
    // The form of the Date is yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff or yyyy'-'MM'-'dd' or yyyy'-'MM'-'dd'T'HH':'mm':'ss

[System.Runtime.InteropServices.ComVisible(true)]
    public sealed class SoapDateTime 
    {
     
        public static String XsdType
        {
            get{ return "dateTime";}
        }

        private static String[] formats = 
        {
            "yyyy-MM-dd'T'HH:mm:ss.fffffffzzz", 
            "yyyy-MM-dd'T'HH:mm:ss.ffff",
            "yyyy-MM-dd'T'HH:mm:ss.ffffzzz", 
            "yyyy-MM-dd'T'HH:mm:ss.fff",
            "yyyy-MM-dd'T'HH:mm:ss.fffzzz", 
            "yyyy-MM-dd'T'HH:mm:ss.ff",
            "yyyy-MM-dd'T'HH:mm:ss.ffzzz", 
            "yyyy-MM-dd'T'HH:mm:ss.f",
            "yyyy-MM-dd'T'HH:mm:ss.fzzz", 
            "yyyy-MM-dd'T'HH:mm:ss", 
            "yyyy-MM-dd'T'HH:mm:sszzz",
            "yyyy-MM-dd'T'HH:mm:ss.fffff",
            "yyyy-MM-dd'T'HH:mm:ss.fffffzzz", 
            "yyyy-MM-dd'T'HH:mm:ss.ffffff",
            "yyyy-MM-dd'T'HH:mm:ss.ffffffzzz", 
            "yyyy-MM-dd'T'HH:mm:ss.fffffff",
            "yyyy-MM-dd'T'HH:mm:ss.ffffffff",
            "yyyy-MM-dd'T'HH:mm:ss.ffffffffzzz", 
            "yyyy-MM-dd'T'HH:mm:ss.fffffffff",
            "yyyy-MM-dd'T'HH:mm:ss.fffffffffzzz", 
            "yyyy-MM-dd'T'HH:mm:ss.ffffffffff",
            "yyyy-MM-dd'T'HH:mm:ss.ffffffffffzzz"            
        };


        public static String ToString(DateTime value)
        {
            return value.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffffzzz", CultureInfo.InvariantCulture);
        }


        public static DateTime Parse(String value)
        {
            DateTime dt;
            try
            {
                if (value == null)
                    dt = DateTime.MinValue;
                else
                {
                    String time = value;
                    if (value.EndsWith("Z", StringComparison.Ordinal))
                        time = value.Substring(0, value.Length-1)+"-00:00";
                    dt = DateTime.ParseExact(time, formats, CultureInfo.InvariantCulture,DateTimeStyles.None);
                }

            }
            catch (Exception)
            {
                throw new RemotingException(
                                           String.Format(
                                                        CultureInfo.CurrentCulture, Environment.GetResourceString(
                                                                                     "Remoting_SOAPInteropxsdInvalid"), "xsd:dateTime", value));                
            }

            return dt;
        }
        
    }


[System.Runtime.InteropServices.ComVisible(true)]
    public sealed class SoapDuration
    {
        // Convert from ISO/xsd TimeDuration to urt TimeSpan
        // The form of the time duration is PxxYxxDTxxHxxMxx.xxxS or PxxYxxDTxxHxxMxxS
        // Keep in [....] with Message.cs


        public static String XsdType
        {
            get{ return "duration";}
        }

        // calcuate carryover points by ISO 8601 : 1998 section 5.5.3.2.1 Alternate format
        // algorithm not to exceed 12 months, 30 day
        // note with this algorithm year has 360 days.
        private static void CarryOver(int inDays, out int years, out int months, out int days)
        {
            years = inDays/360;
            int yearDays = years*360;
            months = Math.Max(0, inDays - yearDays)/30;
            int monthDays = months*30;
            days = Math.Max(0, inDays - (yearDays+monthDays)); 
            days = inDays%30;
        }


        [System.Security.SecuritySafeCritical]  // auto-generated
        public static String ToString(TimeSpan timeSpan)
        {
            StringBuilder sb = new StringBuilder(10);
            sb.Length = 0;
            if (TimeSpan.Compare(timeSpan, TimeSpan.Zero) < 1)
            {
                sb.Append('-');
                //timeSpan = timeSpan.Negate(); //negating timespan at top level not at each piece such as Day
            }

            int years = 0;
            int months = 0;
            int days = 0;

            CarryOver(Math.Abs(timeSpan.Days), out years, out months, out days);

            sb.Append('P');
            sb.Append(years);
            sb.Append('Y');
            sb.Append(months);
            sb.Append('M');
            sb.Append(days);
            sb.Append("DT");
            sb.Append(Math.Abs(timeSpan.Hours));
            sb.Append('H');
            sb.Append(Math.Abs(timeSpan.Minutes));
            sb.Append('M');
            sb.Append(Math.Abs(timeSpan.Seconds));
            long timea = Math.Abs(timeSpan.Ticks % TimeSpan.TicksPerDay);
            int t1 = (int)(timea % TimeSpan.TicksPerSecond);
            if (t1 != 0)
            {
                String t2 = ParseNumbers.IntToString(t1, 10, 7, '0', 0);
                sb.Append('.');
                sb.Append(t2);
            }
            sb.Append('S');
            return sb.ToString();
        }


        public static TimeSpan Parse(String value)
        {
            int sign = 1;

            try
            {
                if (value == null)
                    return TimeSpan.Zero;

                if (value[0] == '-')
                    sign = -1;


                Char[] c = value.ToCharArray();
                int[] timeValues = new int[7];
                String year = "0";
                String month = "0";
                String day = "0";
                String hour = "0";
                String minute = "0";
                String second = "0";
                String fraction = "0";
                bool btime = false;
                bool bmill = false;
                int beginField = 0;

                for (int i=0; i<c.Length; i++)
                {
                    switch (c[i])
                    {
                        case 'P':
                            beginField = i+1;
                            break;
                        case 'Y':
                            year = new String(c,beginField, i-beginField);
                            beginField = i+1;
                            break;
                        case 'M':
                            if (btime)
                                minute = new String(c, beginField, i-beginField);
                            else
                                month = new String(c, beginField, i-beginField);
                            beginField = i+1;
                            break;
                        case 'D':
                            day = new String(c, beginField, i-beginField);
                            beginField = i+1;
                            break;
                        case 'T':
                            btime = true;
                            beginField = i+1;
                            break;
                        case 'H':
                            hour = new String(c, beginField, i-beginField);
                            beginField = i+1;
                            break;
                        case '.':
                            bmill = true;
                            second = new String(c, beginField, i-beginField);
                            beginField = i+1;
                            break;
                        case 'S':
                            if (!bmill)
                                second = new String(c, beginField, i-beginField);
                            else
                                fraction = new String(c, beginField, i-beginField);
                            break;
                        case 'Z':
                            break;
                        default:
                            // Number continue to loop until end of number
                            break;
                    }                                                                                                                                                                                                                                                                  
                }

                long ticks = sign*
                    (
                     (Int64.Parse(year, CultureInfo.InvariantCulture)*360+Int64.Parse(month, CultureInfo.InvariantCulture)*30+Int64.Parse(day, CultureInfo.InvariantCulture))*TimeSpan.TicksPerDay+
                     Int64.Parse(hour, CultureInfo.InvariantCulture)*TimeSpan.TicksPerHour+
                     Int64.Parse(minute, CultureInfo.InvariantCulture)*TimeSpan.TicksPerMinute+
                     Convert.ToInt64(Double.Parse(second+"."+fraction, CultureInfo.InvariantCulture)*(Double)TimeSpan.TicksPerSecond)
                    );
                return new TimeSpan(ticks);
            }
            catch (Exception)
            {
                throw new RemotingException(
                                           String.Format(
                                                        CultureInfo.CurrentCulture, Environment.GetResourceString(
                                                                                     "Remoting_SOAPInteropxsdInvalid"), "xsd:duration", value));                
            }
        }
    }

    [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    public sealed class SoapTime : ISoapXsd
    {
        DateTime _value = DateTime.MinValue;

        public static String XsdType
        {
            get{ return "time";}
        }


        public String GetXsdType()
        {
            return XsdType;
        }

        private static String[] formats = 
        {
            "HH:mm:ss.fffffffzzz",
            "HH:mm:ss.ffff",
            "HH:mm:ss.ffffzzz",
            "HH:mm:ss.fff",
            "HH:mm:ss.fffzzz",
            "HH:mm:ss.ff",
            "HH:mm:ss.ffzzz",
            "HH:mm:ss.f",
            "HH:mm:ss.fzzz",
            "HH:mm:ss", 
            "HH:mm:sszzz",
            "HH:mm:ss.fffff",
            "HH:mm:ss.fffffzzz",
            "HH:mm:ss.ffffff",
            "HH:mm:ss.ffffffzzz",
            "HH:mm:ss.fffffff",
            "HH:mm:ss.ffffffff",
            "HH:mm:ss.ffffffffzzz",
            "HH:mm:ss.fffffffff",
            "HH:mm:ss.fffffffffzzz",
            "HH:mm:ss.fffffffff",
            "HH:mm:ss.fffffffffzzz"
        };


        public SoapTime()
        {
        }


        public SoapTime(DateTime value)
        {
            _value = value;
        }


        public DateTime Value
        {
            get {return _value;}
            set {_value = new DateTime(1, 1, 1, value.Hour, value.Minute, value.Second, value.Millisecond);}
        }


        public override String ToString()
        {
            return _value.ToString("HH:mm:ss.fffffffzzz", CultureInfo.InvariantCulture);
        }


        public static SoapTime Parse(String value)
        {
            String time = value;
            if (value.EndsWith("Z", StringComparison.Ordinal))
                time = value.Substring(0, value.Length-1)+"-00:00";
            SoapTime dt = new SoapTime(DateTime.ParseExact(time, formats, CultureInfo.InvariantCulture,DateTimeStyles.None));
            return dt;
        }
    }


    [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    public sealed class SoapDate : ISoapXsd
    {

        DateTime _value = DateTime.MinValue.Date;
        int _sign = 0;


        public static String XsdType
        {
            get{ return "date";}
        }



        public String GetXsdType()
        {
            return XsdType;
        }

        private static String[] formats = 
        {
            "yyyy-MM-dd",
            "'+'yyyy-MM-dd",
            "'-'yyyy-MM-dd",
            "yyyy-MM-ddzzz",
            "'+'yyyy-MM-ddzzz",
            "'-'yyyy-MM-ddzzz"
        };



        public SoapDate()
        {
        }


        public SoapDate(DateTime value)
        {
            _value = value;
        }


        public SoapDate(DateTime value, int sign)
        {
            _value = value;
            _sign = sign;
        }


        public DateTime Value
        {
            get {return _value;}
            set {_value = value.Date;}
        }


        public int Sign
        {
            get {return _sign;}
            set {_sign = value;}
        }


        public override String ToString()
        {
            if (_sign < 0)
                return _value.ToString("'-'yyyy-MM-dd", CultureInfo.InvariantCulture);
            else
                return _value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }


        public static SoapDate Parse(String value)
        {
            int sign = 0;
            if (value[0] == '-')
                sign = -1;
            return new SoapDate(DateTime.ParseExact(value, formats, CultureInfo.InvariantCulture,DateTimeStyles.None), sign);
        }
    }


    [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    public sealed class SoapYearMonth : ISoapXsd
    {

        DateTime _value = DateTime.MinValue;
        int _sign = 0;


        public static String XsdType
        {
            get{ return "gYearMonth";}
        }


        public String GetXsdType()
        {
            return XsdType;
        }

        private static String[] formats = 
        {
            "yyyy-MM",
            "'+'yyyy-MM",
            "'-'yyyy-MM",
            "yyyy-MMzzz",
            "'+'yyyy-MMzzz",
            "'-'yyyy-MMzzz"
        };



        public SoapYearMonth()
        {
        }


        public SoapYearMonth(DateTime value)
        {
            _value = value;
        }


        public SoapYearMonth(DateTime value, int sign)
        {
            _value = value;
            _sign = sign;
        }



        public DateTime Value
        {
            get {return _value;}
            set {_value = value;}
        }


        public int Sign
        {
            get {return _sign;}
            set {_sign = value;}
        }


        public override String ToString()
        {
            if (_sign < 0)
                return _value.ToString("'-'yyyy-MM", CultureInfo.InvariantCulture);
            else
                return _value.ToString("yyyy-MM", CultureInfo.InvariantCulture);
        }


        public static SoapYearMonth Parse(String value)
        {
            int sign = 0;
            if (value[0] == '-')
                sign = -1;
            return new SoapYearMonth(DateTime.ParseExact(value, formats, CultureInfo.InvariantCulture,DateTimeStyles.None), sign);
        }
    }


    [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    public sealed class SoapYear : ISoapXsd
    {

        DateTime _value = DateTime.MinValue;
        int _sign = 0;


        public static String XsdType
        {
            get{ return "gYear";}
        }


        public String GetXsdType()
        {
            return XsdType;
        }

        private static String[] formats = 
        {
            "yyyy",
            "'+'yyyy",
            "'-'yyyy",
            "yyyyzzz",
            "'+'yyyyzzz",
            "'-'yyyyzzz"
        };



        public SoapYear()
        {
        }


        public SoapYear(DateTime value)
        {
            _value = value;
        }


        public SoapYear(DateTime value, int sign)
        {
            _value = value;
            _sign = sign;
        }



        public DateTime Value
        {
            get {return _value;}
            set {_value = value;}
        }


        public int Sign
        {
            get {return _sign;}
            set {_sign = value;}
        }


        public override String ToString()
        {
            if (_sign < 0)
                return _value.ToString("'-'yyyy", CultureInfo.InvariantCulture);
            else
                return _value.ToString("yyyy", CultureInfo.InvariantCulture);
        }


        public static SoapYear Parse(String value)
        {
            int sign = 0;
            if (value[0] == '-')
                sign = -1;
            return new SoapYear(DateTime.ParseExact(value, formats, CultureInfo.InvariantCulture,DateTimeStyles.None), sign);
        }
    }


    [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    public sealed class SoapMonthDay : ISoapXsd
    {
        DateTime _value = DateTime.MinValue;


        public static String XsdType
        {
            get{ return "gMonthDay";}
        }


        public String GetXsdType()
        {
            return XsdType;
        }

        private static String[] formats = 
        {
            "--MM-dd",
            "--MM-ddzzz"
        };



        public SoapMonthDay()
        {
        }


        public SoapMonthDay(DateTime value)
        {
            _value = value;
        }


        public DateTime Value
        {
            get {return _value;}
            set {_value = value;}
        }


        public override String ToString()
        {
            return _value.ToString("'--'MM'-'dd", CultureInfo.InvariantCulture);
        }


        public static SoapMonthDay Parse(String value)
        {
            return new SoapMonthDay(DateTime.ParseExact(value, formats, CultureInfo.InvariantCulture,DateTimeStyles.None));
        }
    }


    [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    public sealed class SoapDay : ISoapXsd
    {
        DateTime _value = DateTime.MinValue;


        public static String XsdType
        {
            get{ return "gDay";}
        }


        public String GetXsdType()
        {
            return XsdType;
        }


        private static String[] formats = 
        {
            "---dd",
            "---ddzzz"
        };


        public SoapDay()
        {
        }


        public SoapDay(DateTime value)
        {
            _value = value;
        }


        public DateTime Value
        {
            get {return _value;}
            set {_value = value;}
        }


        public override String ToString()
        {
            return _value.ToString("---dd", CultureInfo.InvariantCulture);
        }


        public static SoapDay Parse(String value)
        {
            return new SoapDay(DateTime.ParseExact(value, formats, CultureInfo.InvariantCulture,DateTimeStyles.None));
        }
    }


    [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    public sealed class SoapMonth : ISoapXsd
    {
        DateTime _value = DateTime.MinValue;


        public static String XsdType
        {
            get{ return "gMonth";}
        }


        public String GetXsdType()
        {
            return XsdType;
        }

        private static String[] formats = 
        {
            "--MM--",
            "--MM--zzz"
        };



        public SoapMonth()
        {
        }


        public SoapMonth(DateTime value)
        {
            _value = value;
        }


        public DateTime Value
        {
            get {return _value;}
            set {_value = value;}
        }


        public override String ToString()
        {
            return _value.ToString("--MM--", CultureInfo.InvariantCulture);
        }


        public static SoapMonth Parse(String value)
        {
            return new SoapMonth(DateTime.ParseExact(value, formats, CultureInfo.InvariantCulture,DateTimeStyles.None));
        }
    }


    [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    public sealed class SoapHexBinary : ISoapXsd
    {
        Byte[] _value;

        public static String XsdType
        {
            get{ return "hexBinary";}
        }


        public String GetXsdType()
        {
            return XsdType;
        }


        public SoapHexBinary()
        {
        }


        public SoapHexBinary(Byte[] value)
        {
            _value = value;
        }


        public Byte[] Value
        {
            get {return _value;}
            set {_value = value;}
        }

        StringBuilder sb = new StringBuilder(100);

        public override String ToString()
        {
            sb.Length = 0;
            for (int i=0; i<_value.Length; i++)
            {
                String s = _value[i].ToString("X", CultureInfo.InvariantCulture);
                if (s.Length == 1)
                    sb.Append('0');
                sb.Append(s);
            }
            return sb.ToString();
        }


        public static SoapHexBinary Parse(String value)
        {
            return new SoapHexBinary(ToByteArray(SoapType.FilterBin64(value)));
        }



        private static Byte[] ToByteArray(String value)
        {
            Char[] cA = value.ToCharArray();
            if (cA.Length%2 != 0)
            {
                throw new RemotingException(
                                           String.Format(
                                                        CultureInfo.CurrentCulture, Environment.GetResourceString(
                                                                                     "Remoting_SOAPInteropxsdInvalid"), "xsd:hexBinary", value));                
            }
            Byte[] bA = new Byte[cA.Length/2];
            for (int i = 0; i< cA.Length/2; i++)
            {
                bA[i] = (Byte)(ToByte(cA[i*2], value)*16+ToByte(cA[i*2+1], value));
            }

            return bA;
        }

        private static Byte ToByte(Char c, String value)
        {
            Byte b = (Byte)0;
            String s = c.ToString();
            try
            {
                s = c.ToString();
                b = Byte.Parse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                throw new RemotingException(Environment.GetResourceString("Remoting_SOAPInteropxsdInvalid", "xsd:hexBinary", value));                
            }

            return b;
        }
    }


    [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    public sealed class SoapBase64Binary : ISoapXsd
    {
        Byte[] _value;


        public static String XsdType
        {
            get{ return "base64Binary";}
        }


        public String GetXsdType()
        {
            return XsdType;
        }


        public SoapBase64Binary()
        {
        }


        public SoapBase64Binary(Byte[] value)
        {
            _value = value;
        }


        public Byte[] Value
        {
            get {return _value;}
            set {_value = value;}
        }


        public override String ToString()
        {
            if (_value == null)
                return null;

            // Put in line feeds every 76 characters.
            return SoapType.LineFeedsBin64(Convert.ToBase64String(_value));
        }

        public static SoapBase64Binary Parse(String value)
        {
            if (value == null || value.Length == 0)
                return new SoapBase64Binary(new Byte[0]);

            Byte[] bA;
            try
            {
                bA = Convert.FromBase64String(SoapType.FilterBin64(value));
            }
            catch (Exception)
            {
                throw new RemotingException(
                                           String.Format(
                                                        CultureInfo.CurrentCulture, Environment.GetResourceString(
                                                                                     "Remoting_SOAPInteropxsdInvalid"), "base64Binary", value));                
            }
            return new SoapBase64Binary(bA);
        }
    }


    [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    public sealed class SoapInteger : ISoapXsd
    {
        Decimal _value;


        public static String XsdType
        {
            get{ return "integer";}
        }


        public String GetXsdType()
        {
            return XsdType;
        }


        public SoapInteger()
        {
        }


        public SoapInteger (Decimal value)
        {
            _value = Decimal.Truncate(value);
        }


        public Decimal Value
        {
            get {return _value;}
            set {_value = Decimal.Truncate(value);}
        }


        public override String ToString()
        {
            return _value.ToString(CultureInfo.InvariantCulture);
        }


        public static SoapInteger Parse(String value)
        {
            return new SoapInteger(Decimal.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture));
        }
    }


    [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    public sealed class SoapPositiveInteger : ISoapXsd
    {
        Decimal _value;


        public static String XsdType
        {
            get{ return "positiveInteger";}
        }


        public String GetXsdType()
        {
            return XsdType;
        }


        public SoapPositiveInteger()
        {
        }



        public SoapPositiveInteger (Decimal value)
        {
            _value = Decimal.Truncate(value);
            if (_value < Decimal.One)
                throw new RemotingException(
                                           String.Format(
                                                        CultureInfo.CurrentCulture, Environment.GetResourceString(
                                                                                     "Remoting_SOAPInteropxsdInvalid"), "xsd:positiveInteger", value));
        }


        public Decimal Value
        {
            get {return _value;}
            set {
                _value = Decimal.Truncate(value);
                if (_value < Decimal.One)
                    throw new RemotingException(
                                               String.Format(
                                                            CultureInfo.CurrentCulture, Environment.GetResourceString(
                                                                                         "Remoting_SOAPInteropxsdInvalid"), "xsd:positiveInteger", value));
            }
        }


        public override String ToString()
        {
            return _value.ToString(CultureInfo.InvariantCulture);
        }


        public static SoapPositiveInteger Parse(String value)
        {
            return new SoapPositiveInteger(Decimal.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture));
        }
    }


    [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    public sealed class SoapNonPositiveInteger : ISoapXsd
    {
        Decimal _value;


        public static String XsdType
        {
            get{ return "nonPositiveInteger";}
        }


        public String GetXsdType()
        {
            return XsdType;
        }


        public SoapNonPositiveInteger()
        {
        }


        public SoapNonPositiveInteger (Decimal value)
        {
            _value = Decimal.Truncate(value);
            if (_value > Decimal.Zero)
                throw new RemotingException(
                                           String.Format(
                                                        CultureInfo.CurrentCulture, Environment.GetResourceString(
                                                                                     "Remoting_SOAPInteropxsdInvalid"), "xsd:nonPositiveInteger", value));
        }


        public Decimal Value
        {
            get {return _value;}
            set {
                _value = Decimal.Truncate(value);
                if (_value > Decimal.Zero)
                    throw new RemotingException(
                                               String.Format(
                                                            CultureInfo.CurrentCulture, Environment.GetResourceString(
                                                                                         "Remoting_SOAPInteropxsdInvalid"), "xsd:nonPositiveInteger", value));
            }
        }


        public override String ToString()
        {
            return  _value.ToString(CultureInfo.InvariantCulture);
        }


        public static SoapNonPositiveInteger Parse(String value)
        {
            return new SoapNonPositiveInteger(Decimal.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture));
        }
    }


    [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    public sealed class SoapNonNegativeInteger : ISoapXsd
    {
        Decimal _value;


        public static String XsdType
        {
            get{ return "nonNegativeInteger";}
        }



        public String GetXsdType()
        {
            return XsdType;
        }


        public SoapNonNegativeInteger()
        {
        }

        public SoapNonNegativeInteger (Decimal value)
        {
            _value = Decimal.Truncate(value);
            if (_value < Decimal.Zero)
                throw new RemotingException(
                                           String.Format(
                                                        CultureInfo.CurrentCulture, Environment.GetResourceString(
                                                                                     "Remoting_SOAPInteropxsdInvalid"), "xsd:nonNegativeInteger", value));
        }


        public Decimal Value
        {
            get {return _value;}
            set {
                _value = Decimal.Truncate(value);
                if (_value < Decimal.Zero)
                    throw new RemotingException(
                                               String.Format(
                                                            CultureInfo.CurrentCulture, Environment.GetResourceString(
                                                                                         "Remoting_SOAPInteropxsdInvalid"), "xsd:nonNegativeInteger", value));
            }
        }


        public override String ToString()
        {
            return _value.ToString(CultureInfo.InvariantCulture);
        }


        public static SoapNonNegativeInteger Parse(String value)
        {
            return new SoapNonNegativeInteger(Decimal.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture));
        }
    }


    [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    public sealed class SoapNegativeInteger : ISoapXsd
    {
        Decimal _value;


        public static String XsdType
        {
            get{ return "negativeInteger";}
        }


        public String GetXsdType()
        {
            return XsdType;
        }


        public SoapNegativeInteger()
        {
        }


        public SoapNegativeInteger (Decimal value)
        {
            _value = Decimal.Truncate(value);
            if (value > Decimal.MinusOne)
                throw new RemotingException(
                                           String.Format(
                                                        CultureInfo.CurrentCulture, Environment.GetResourceString(
                                                                                     "Remoting_SOAPInteropxsdInvalid"), "xsd:negativeInteger", value));
        }


        public Decimal Value
        {
            get {return _value;}
            set {
                _value = Decimal.Truncate(value);
                if (_value > Decimal.MinusOne)
                    throw new RemotingException(
                                               String.Format(
                                                            CultureInfo.CurrentCulture, Environment.GetResourceString(
                                                                                         "Remoting_SOAPInteropxsdInvalid"), "xsd:negativeInteger", value));
            }
        }


        public override String ToString()
        {
            return _value.ToString(CultureInfo.InvariantCulture);
        }


        public static SoapNegativeInteger Parse(String value)
        {
            return new SoapNegativeInteger(Decimal.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture));
        }
    }

    [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    public sealed class SoapAnyUri : ISoapXsd
    {
        String _value;


        public static String XsdType
        {
            get{ return "anyURI";}
        }


        public String GetXsdType()
        {
            return XsdType;
        }


        public SoapAnyUri()
        {
        }


        public SoapAnyUri (String value)
        {
            _value = value;
        }


        public String Value
        {
            get {return _value;}
            set {_value = value;}
        }


        public override String ToString()
        {
            return _value;
        }


        public static SoapAnyUri Parse(String value)
        {
            return new SoapAnyUri(value);
        }
    }


    [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    public sealed class SoapQName : ISoapXsd
    {
        String _name;
        String _namespace;
        String _key;


        public static String XsdType
        {
            get{ return "QName";}
        }


        public String GetXsdType()
        {
            return XsdType;
        }


        public SoapQName()
        {
        }


        public SoapQName(String value)
        {
            _name = value;
        }



        public SoapQName (String key, String name)
        {
            _name = name;
            _key = key;
        }


        public SoapQName (String key, String name, String namespaceValue)
        {
            _name = name;
            _namespace = namespaceValue;
            _key = key;
        }


        public String Name
        {
            get {return _name;}
            set {_name = value;}
        }


        public String Namespace
        {
            get {
                /*
                if (_namespace == null || _namespace.Length == 0)
                    throw new RemotingException(String.Format(Environment.GetResourceString("Remoting_SOAPQNameNamespace"), _name));
                    */

                return _namespace;
                }
            set {_namespace = value;}
        }


        public String Key
        {
            get {return _key;}
            set {_key = value;}
        }



        public override String ToString()
        {
            if (_key == null || _key.Length == 0)
                return _name;
            else
                return _key+":"+_name;
        }


        public static SoapQName Parse(String value)
        {
            if (value == null)
                return new SoapQName();

            String key = "";
            String name = value;

            int index = value.IndexOf(':');
            if (index > 0)
            {
                key = value.Substring(0,index);
                name = value.Substring(index+1);
            }

            return new SoapQName(key, name);
        }
    }


    [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    public sealed class SoapNotation : ISoapXsd
    {
        String _value;


        public static String XsdType
        {
            get{ return "NOTATION";}
        }


        public String GetXsdType()
        {
            return XsdType;
        }


        public SoapNotation()
        {
        }


        public SoapNotation (String value)
        {
            _value = value;
        }


        public String Value
        {
            get {return _value;}
            set {_value = value;}
        }


        public override String ToString()
        {
            return _value;
        }


        public static SoapNotation Parse(String value)
        {
            return new SoapNotation(value);
        }
    }


    // Used to pass a string to xml which won't be escaped.

    [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    public sealed class SoapNormalizedString : ISoapXsd
    {
        String _value;


        public static String XsdType
        {
            get{ return "normalizedString";}
        }


        public String GetXsdType()
        {
            return XsdType;
        }


        public SoapNormalizedString()
        {
        }

        public SoapNormalizedString (String value)
        {
            _value = Validate(value);
        }

        public String Value
        {
            get {return _value;}
            set {_value = Validate(value);}
        }

        public override String ToString()
        {
            return SoapType.Escape(_value);
        }

        public static SoapNormalizedString Parse(String value)
        {
            return new SoapNormalizedString(value);
        }

        private String Validate(String value)
        {
            if (value == null || value.Length == 0)
                return value;

            Char[] validateChar = {(Char)0xD, (Char)0xA, (Char)0x9};

            int index = value.LastIndexOfAny(validateChar); 

            if (index > -1)
                throw new RemotingException(Environment.GetResourceString("Remoting_SOAPInteropxsdInvalid", "xsd:normalizedString", value));

            return value;
        }

    }

    [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    public sealed class SoapToken : ISoapXsd
    {
        String _value;

        public static String XsdType
        {
            get{return "token";}
        }

        public String GetXsdType()
        {
            return XsdType;
        }

        public SoapToken()
        {
        }

        public SoapToken (String value)
        {
            _value = Validate(value);
        }

        public String Value
        {
            get {return _value;}
            set {_value = Validate(value);}
        }

        public override String ToString()
        {
            return SoapType.Escape(_value);
        }

        public static SoapToken Parse(String value)
        {
            return new SoapToken(value);
        }

        private String Validate(String value)
        {
            if (value == null || value.Length == 0)
                return value;

            Char[] validateChar = {(Char)0xD, (Char)0x9};

            int index = value.LastIndexOfAny(validateChar); 

            if (index > -1)
                throw new RemotingException(Environment.GetResourceString("Remoting_SOAPInteropxsdInvalid", "xsd:token", value));

            if (value.Length > 0)
            {
                if (Char.IsWhiteSpace(value[0]) || Char.IsWhiteSpace(value[value.Length - 1]))
                    throw new RemotingException(Environment.GetResourceString("Remoting_SOAPInteropxsdInvalid", "xsd:token", value));
            }

            index = value.IndexOf("  ");
            if (index > -1)
                throw new RemotingException(Environment.GetResourceString("Remoting_SOAPInteropxsdInvalid", "xsd:token", value));

            return value;
        }
    }


    [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    public sealed class SoapLanguage : ISoapXsd
    {
        String _value;

        public static String XsdType
        {
            get{return "language";}
        }

        public String GetXsdType()
        {
            return XsdType;
        }

        public SoapLanguage()
        {
        }

        public SoapLanguage (String value)
        {
            _value = value;
        }

        public String Value
        {
            get {return _value;}
            set {_value = value;}
        }

        public override String ToString()
        {
            return SoapType.Escape(_value);
        }

        public static SoapLanguage Parse(String value)
        {
            return new SoapLanguage(value);
        }
    }

    [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    public sealed class SoapName : ISoapXsd
    {
        String _value;

        public static String XsdType
        {
            get{return "Name";}
        }

        public String GetXsdType()
        {
            return XsdType;
        }

        public SoapName()
        {
        }

        public SoapName (String value)
        {
            _value = value;
        }

        public String Value
        {
            get {return _value;}
            set {_value = value;}
        }

        public override String ToString()
        {
            return SoapType.Escape(_value);
        }

        public static SoapName Parse(String value)
        {
            return new SoapName(value);
        }
    }

    [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    public sealed class SoapIdrefs : ISoapXsd
    {
        String _value;

        public static String XsdType
        {
            get{return "IDREFS";}
        }

        public String GetXsdType()
        {
            return XsdType;
        }

        public SoapIdrefs()
        {
        }

        public SoapIdrefs (String value)
        {
            _value = value;
        }

        public String Value
        {
            get {return _value;}
            set {_value = value;}
        }

        public override String ToString()
        {
            return SoapType.Escape(_value);
        }

        public static SoapIdrefs Parse(String value)
        {
            return new SoapIdrefs(value);
        }
    }

    [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    public sealed class SoapEntities : ISoapXsd
    {
        String _value;

        public static String XsdType
        {
            get{return "ENTITIES";}
        }

        public String GetXsdType()
        {
            return XsdType;
        }


        public SoapEntities()
        {
        }

        public SoapEntities (String value)
        {
            _value = value;
        }

        public String Value
        {
            get {return _value;}
            set {_value = value;}
        }

        public override String ToString()
        {
            return SoapType.Escape(_value);
        }

        public static SoapEntities Parse(String value)
        {
            return new SoapEntities(value);
        }
    }

    [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    public sealed class SoapNmtoken : ISoapXsd
    {
        String _value;

        public static String XsdType
        {
            get{return "NMTOKEN";}
        }

        public String GetXsdType()
        {
            return XsdType;
        }

        public SoapNmtoken()
        {
        }

        public SoapNmtoken (String value)
        {
            _value = value;
        }

        public String Value
        {
            get {return _value;}
            set {_value = value;}
        }

        public override String ToString()
        {
            return SoapType.Escape(_value);
        }

        public static SoapNmtoken Parse(String value)
        {
            return new SoapNmtoken(value);
        }
    }

    [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    public sealed class SoapNmtokens : ISoapXsd
    {
        String _value;

        public static String XsdType
        {
            get{return "NMTOKENS";}
        }

        public String GetXsdType()
        {
            return XsdType;
        }

        public SoapNmtokens()
        {
        }

        public SoapNmtokens (String value)
        {
            _value = value;
        }

        public String Value
        {
            get {return _value;}
            set {_value = value;}
        }

        public override String ToString()
        {
            return SoapType.Escape(_value);
        }

        public static SoapNmtokens Parse(String value)
        {
            return new SoapNmtokens(value);
        }
    }

    [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    public sealed class SoapNcName : ISoapXsd
    {
        String _value;

        public static String XsdType
        {
            get{return "NCName";}
        }



        public String GetXsdType()
        {
            return XsdType;
        }

        public SoapNcName()
        {
        }

        public SoapNcName (String value)
        {
            _value = value;
        }

        public String Value
        {
            get {return _value;}
            set {_value = value;}
        }

        public override String ToString()
        {
            return SoapType.Escape(_value);
        }

        public static SoapNcName Parse(String value)
        {
            return new SoapNcName(value);
        }
    }

    [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    public sealed class SoapId : ISoapXsd
    {
        String _value;

        public static String XsdType
        {
            get{return "ID";}
        }

        public String GetXsdType()
        {
            return XsdType;
        }

        public SoapId()
        {
        }

        public SoapId (String value)
        {
            _value = value;
        }

        public String Value
        {
            get {return _value;}
            set {_value = value;}
        }

        public override String ToString()
        {
            return SoapType.Escape(_value);
        }

        public static SoapId Parse(String value)
        {
            return new SoapId(value);
        }
    }

    [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    public sealed class SoapIdref : ISoapXsd
    {
        String _value;

        public static String XsdType
        {
            get{return "IDREF";}
        }

        public String GetXsdType()
        {
            return XsdType;
        }

        public SoapIdref()
        {
        }

        public SoapIdref (String value)
        {
            _value = value;
        }

        public String Value
        {
            get {return _value;}
            set {_value = value;}
        }

        public override String ToString()
        {
            return SoapType.Escape(_value);
        }

        public static SoapIdref Parse(String value)
        {
            return new SoapIdref(value);
        }
    }

    [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    public sealed class SoapEntity : ISoapXsd
    {
        String _value;

        public static String XsdType
        {
            get{return "ENTITY";}
        }

        public String GetXsdType()
        {
            return XsdType;
        }

        public SoapEntity()
        {
        }

        public SoapEntity (String value)
        {
            _value = value;
        }

        public String Value
        {
            get {return _value;}
            set {_value = value;}
        }

        public override String ToString()
        {
            return SoapType.Escape(_value);
        }

        public static SoapEntity Parse(String value)
        {
            return new SoapEntity(value);
        }
    }
        }

    // namespace System.Runtime.Remoting.Metadata




    
