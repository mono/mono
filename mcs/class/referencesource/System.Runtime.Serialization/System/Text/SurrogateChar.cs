//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.Text
{
    using System.Globalization;
    using System.Runtime.Serialization; // Just for SR

    struct SurrogateChar
    {
        char lowChar;
        char highChar;

        public const int MinValue = 0x10000;
        public const int MaxValue = MinValue + (1 << 20) - 1;

        const char surHighMin = (char)0xd800;
        const char surHighMax = (char)0xdbff;
        const char surLowMin = (char)0xdc00;
        const char surLowMax = (char)0xdfff;

        public SurrogateChar(int ch)
        {
            if (ch < MinValue || ch > MaxValue)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.XmlInvalidSurrogate, ch.ToString("X", CultureInfo.InvariantCulture)), "ch"));

            const int mask = ((1 << 10) - 1);

            this.lowChar = (char)(((ch - MinValue) & mask) + surLowMin);
            this.highChar = (char)((((ch - MinValue) >> 10) & mask) + surHighMin);
        }

        public SurrogateChar(char lowChar, char highChar)
        {
            if (lowChar < surLowMin || lowChar > surLowMax)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.XmlInvalidLowSurrogate, ((int)lowChar).ToString("X", CultureInfo.InvariantCulture)), "lowChar"));

            if (highChar < surHighMin || highChar > surHighMax)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.XmlInvalidHighSurrogate, ((int)highChar).ToString("X", CultureInfo.InvariantCulture)), "highChar"));

            this.lowChar = lowChar;
            this.highChar = highChar;
        }

        public char LowChar { get { return lowChar; } }
        public char HighChar { get { return highChar; } }

        public int Char
        {
            get
            {
                return (lowChar - surLowMin) | ((highChar - surHighMin) << 10) + MinValue;
            }
        }
    }
}
