// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Runtime.Serialization
{
    using System.Globalization;

    /// <summary>
    /// This class is used to customize the way DateTime is
    /// serialized or deserialized by <see cref="Json.DataContractJsonSerializer"/> 
    /// </summary>
    public class DateTimeFormat
    {
        private string formatString;
        private IFormatProvider formatProvider;
        private DateTimeStyles dateTimeStyles;

        /// <summary>
        /// Initailizes a new <see cref="DateTimeFormat"/> with the specified
        /// formatString and DateTimeFormatInfo.CurrentInfo as the
        /// formatProvider.
        /// </summary>
        /// <param name="formatString">Specifies the formatString to be used.</param>
        public DateTimeFormat(string formatString) : this(formatString, DateTimeFormatInfo.CurrentInfo)
        {
        }

        /// <summary>
        /// Initailizes a new <see cref="DateTimeFormat"/> with the specified
        /// formatString and formatProvider.
        /// </summary>
        /// <param name="formatString">Specifies the formatString to be used.</param>
        /// <param name="formatProvider">Specifies the formatProvider to be used.</param>
        public DateTimeFormat(string formatString, IFormatProvider formatProvider)
        {
            if (formatString == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("formatString");
            }

            if (formatProvider == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("formatProvider");
            }

            this.formatString = formatString;
            this.formatProvider = formatProvider;
            this.dateTimeStyles = DateTimeStyles.RoundtripKind;
        }

        /// <summary>
        /// Gets the FormatString set on this instance.
        /// </summary>
        public string FormatString
        {
            get
            {
                return this.formatString;
            }
        }

        /// <summary>
        /// Gets the FormatProvider set on this instance.
        /// </summary>
        public IFormatProvider FormatProvider
        {
            get
            {
                return this.formatProvider;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="DateTimeStyles"/> on this instance.  
        /// </summary>        
        public DateTimeStyles DateTimeStyles
        {
            get
            {
                return this.dateTimeStyles;
            }

            set
            {
                this.dateTimeStyles = value;
            }
        }
    }
}
