using System;
using System.Globalization;


namespace System.Web.Util
{
    internal static class HttpDate
    {
        static readonly int[] s_tensDigit = new int[10] { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90 };
        
        /*++
        
            Converts a 2 character string to integer
        
            Arguments:
                s   String to convert
        
            Returns:
                numeric equivalent, 0 on failure.
        --*/
        static int atoi2(string s, int startIndex)
        {
            try {
                int tens = s[0 + startIndex] - '0';
                int ones = s[1 + startIndex] - '0';
            
                return s_tensDigit[tens] + ones;
            } 
            catch {
                throw new FormatException(SR.GetString(SR.Atio2BadString, s, startIndex));
            }
        }
        
        static readonly string[] s_days = new string [7] {
            "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"
        };
        
        static readonly string[] s_months = new string[12] {
            "Jan", "Feb", "Mar", "Apr",
            "May", "Jun", "Jul", "Aug",
            "Sep", "Oct", "Nov", "Dec"
        };
        
        // Custom table for make_month() for mapping "Apr" to 4
        static readonly sbyte[] s_monthIndexTable = new sbyte[64] {
           -1, (sbyte)'A',          2, 12, -1, -1,         -1,  8, // A to G
           -1,         -1,         -1, -1,  7, -1, (sbyte)'N', -1, // H to O
            9,         -1, (sbyte)'R', -1, 10, -1,         11, -1, // P to W
           -1,          5,         -1, -1, -1, -1,         -1, -1, // X to Z
           -1, (sbyte)'A',          2, 12, -1, -1,         -1,  8, // a to g
           -1,         -1,         -1, -1,  7, -1, (sbyte)'N', -1, // h to o
            9,         -1, (sbyte)'R', -1, 10, -1,         11, -1, // p to w
           -1,          5,         -1, -1, -1, -1,         -1, -1  // x to z
        };
        
        static int make_month(string s, int startIndex)
        {
            int i;
            sbyte monthIndex;
            string monthString;
        
            //
            // use the third character as the index
            //
        
            i = ((int) s[2 + startIndex] - 0x40) & 0x3F;
            monthIndex = s_monthIndexTable[i];
        
            if ( monthIndex >= 13 ) {
            
                //
                // ok, we need to look at the second character
                //
            
                if ( monthIndex == (sbyte) 'N' ) {
            
                    //
                    // we got an N which we need to resolve further
                    //
            
                    //
                    // if s[1] is 'u' then Jun, if 'a' then Jan
                    //


                    if ( s_monthIndexTable[(s[1 + startIndex]-0x40) & 0x3f] == (sbyte) 'A' ) {
                        monthIndex = 1;
                    } else {
                        monthIndex = 6;
                    }
            
                } else if ( monthIndex == (sbyte) 'R' ) {
            
                    //
                    // if s[1] is 'a' then Microsoft, if 'p' then April
                    //
            
                    if ( s_monthIndexTable[(s[1 + startIndex]-0x40) & 0x3f] == (sbyte) 'A' ) {
                        monthIndex = 3;
                    } else {
                        monthIndex = 4;
                    }
                } else {
                    throw new FormatException(SR.GetString(SR.MakeMonthBadString, s, startIndex));
                }
            }

        
            monthString = s_months[monthIndex-1];
        
            if ( (s[0 + startIndex] == monthString[0]) &&
                 (s[1 + startIndex] == monthString[1]) &&
                 (s[2 + startIndex] == monthString[2]) ) {
        
                return(monthIndex);
        
            } else if ( ((Char.ToUpper(s[0 + startIndex], CultureInfo.InvariantCulture)) == monthString[0]) &&
                        ((Char.ToLower(s[1 + startIndex], CultureInfo.InvariantCulture)) == monthString[1]) &&
                        ((Char.ToLower(s[2 + startIndex], CultureInfo.InvariantCulture)) == monthString[2]) ) {
        
                return monthIndex;
            }

            throw new FormatException(SR.GetString(SR.MakeMonthBadString, s, startIndex));

        } // make_month
        
        /*++
        
          Converts a string representation of a GMT time (three different
          varieties) to an NT representation of a file time.
        
          We handle the following variations:
        
            Sun, 06 Nov 1994 08:49:37 GMT   (RFC 822 updated by RFC 1123)
            Sunday, 06-Nov-94 08:49:37 GMT  (RFC 850)
            Sun Nov 06 08:49:37 1994        (ANSI C's asctime() format
        
          Arguments:
            time                String representation of time field
        
          Returns:
            TRUE on success and FALSE on failure.
        
          History:
        
            Johnl       24-Jan-1995     Modified from WWW library
        
        --*/
        static internal DateTime UtcParse(string time)
        {
            int         i;
            int         year, month, day, hour, minute, second;
        
            if (time == null) {
                throw new ArgumentNullException("time");
            }
        
            if ((i = time.IndexOf(',')) != -1) {
        
                //
                // Thursday, 10-Jun-93 01:29:59 GMT
                // or: Thu, 10 Jan 1993 01:29:59 GMT */
                //
        
                int length = time.Length - i;
                while (--length > 0 && time[++i] == ' ') ;

                if (time[i+2] == '-' ) {      /* First format */
        
                    if (length < 18) {
                        throw new FormatException(SR.GetString(SR.UtilParseDateTimeBad, time));
                    }
        
                    day = atoi2(time, i);
                    month = make_month(time, i + 3);
                    year = atoi2(time, i + 7);
                    if ( year < 50 ) {
                        year += 2000;
                    } else {
                        year += 1900;
                    }

                    hour = atoi2(time, i + 10);
                    minute = atoi2(time, i + 13);
                    second = atoi2(time, i +16);
        
                } else {                         /* Second format */
        
                    if (length < 20) {
                        throw new FormatException(SR.GetString(SR.UtilParseDateTimeBad, time));
                    }
        
                    day = atoi2(time, i);
                    month = make_month(time, i + 3);
                    year = atoi2(time, i + 7) * 100 + atoi2(time, i + 9);
                    hour = atoi2(time, i + 12);
                    minute = atoi2(time, i + 15);
                    second = atoi2(time, i + 18);
                }
            } else {    /* Try the other format:  Wed Jun 09 01:29:59 1993 GMT */
        
                i = -1;
                int length = time.Length + 1;
                while (--length > 0 && time[++i] == ' ');
        
                if (length < 24) {
                    throw new FormatException(SR.GetString(SR.UtilParseDateTimeBad, time));
                }

                day = atoi2(time, i + 8);
                month = make_month(time, i + 4);
                year = atoi2(time, i + 20) * 100 + atoi2(time, i + 22);
                hour = atoi2(time, i + 11);
                minute = atoi2(time, i + 14);
                second = atoi2(time, i + 17);
            }
        
            return new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);
        }
    }
}


