//-----------------------------------------------------------------------------
// <copyright file="MailAddress.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------

namespace System.Net.Mail
{
    using System;
    using System.Text;
    using System.Net.Mime;
    using System.Diagnostics;
    using System.Globalization;

    //
    // This class stores the basic components of an e-mail address as described in RFC 2822 Section 3.4.
    // Any parsing required is done with the MailAddressParser class.
    //
    public class MailAddress
    {
        // These components form an e-mail address when assembled as follows:
        // "EncodedDisplayname" <userName@host>
        private readonly Encoding displayNameEncoding;
        private readonly string displayName;
        private readonly string userName;
        private readonly string host;

        // For internal use only by MailAddressParser.
        // The components were already validated before this is called.
        internal MailAddress(string displayName, string userName, string domain) {
            this.host = domain;
            this.userName = userName;
            this.displayName = displayName;
            this.displayNameEncoding = Encoding.GetEncoding(MimeBasePart.defaultCharSet);

            Debug.Assert(host != null,
                "host was null in internal constructor");

            Debug.Assert(userName != null,
                "userName was null in internal constructor");

            Debug.Assert(displayName != null,
                "displayName was null in internal constructor");
        }

        public MailAddress(string address) : this(address, null, (Encoding)null) {
        }


        public MailAddress(string address, string displayName) : this(address, displayName, (Encoding)null) {
        }

        //
        // This constructor validates and stores the components of an e-mail address.   
        // 
        // Preconditions:
        // - 'address' must not be null or empty.
        // 
        // Postconditions:
        // - The e-mail address components from the given 'address' are parsed, which should be formatted as:
        // "EncodedDisplayname" <username@host>
        // - If a 'displayName' is provided separately, it overrides whatever display name is parsed from the 'address'
        // field.  The display name does not need to be pre-encoded if a 'displayNameEncoding' is provided.
        //
        // A FormatException will be thrown if any of the components in 'address' are invalid.
        public MailAddress(string address, string displayName, Encoding displayNameEncoding) {
            if (address == null){
                throw new ArgumentNullException("address");
            }
            if (address == String.Empty){
                throw new ArgumentException(SR.GetString(SR.net_emptystringcall,"address"), "address");
            }

            this.displayNameEncoding = displayNameEncoding ?? Encoding.GetEncoding(MimeBasePart.defaultCharSet);
            this.displayName = displayName ?? string.Empty;
            
            // Check for bounding quotes
            if (!String.IsNullOrEmpty(this.displayName)) {

                this.displayName = MailAddressParser.NormalizeOrThrow(this.displayName);

                if (this.displayName.Length >= 2 && this.displayName[0] == '\"' 
                    && this.displayName[this.displayName.Length - 1] == '\"') {
                    // Peal bounding quotes, they'll get re-added later.
                    this.displayName = this.displayName.Substring(1, this.displayName.Length - 2);
                }
            }

            MailAddress result = MailAddressParser.ParseAddress(address);
            
            this.host = result.host;
            this.userName = result.userName;

            // If we were not given a display name, use the one parsed from 'address'.
            if (String.IsNullOrEmpty(this.displayName)) {
                this.displayName = result.displayName;
            }
        }


        public string DisplayName
        {
            get
            {
                return displayName;
            }
        }

        public string User
        {
            get
            {
                return this.userName;
            }
        }

        private string GetUser(bool allowUnicode)
        {
            // Unicode usernames cannot be downgraded
            if (!allowUnicode && !MimeBasePart.IsAscii(userName, true))
            {
                throw new SmtpException(SR.GetString(SR.SmtpNonAsciiUserNotSupported, Address));
            }
            return userName;
        }

        public string Host
        {
            get
            {
                return this.host;
            }
        }

        private string GetHost(bool allowUnicode)
        {
            string domain = host;

            // Downgrade Unicode domain names
            if (!allowUnicode && !MimeBasePart.IsAscii(domain, true))
            {
                IdnMapping mapping = new IdnMapping();
                try
                {
                    domain = mapping.GetAscii(domain);
                }
                catch (ArgumentException argEx)
                {
                    throw new SmtpException(SR.GetString(SR.SmtpInvalidHostName, Address), argEx);
                }
            }
            return domain;
        }

        public string Address
        {
            get
            {
                return String.Format(CultureInfo.InvariantCulture, "{0}@{1}", userName, host);
            }
        }

        private string GetAddress(bool allowUnicode)
        {
            return String.Format(CultureInfo.InvariantCulture, "{0}@{1}", 
                GetUser(allowUnicode), GetHost(allowUnicode));
        }

        private string SmtpAddress {
            get{
                return String.Format(CultureInfo.InvariantCulture, "<{0}>", Address);             
            }
        }
        
        internal string GetSmtpAddress(bool allowUnicode)
        {
            return String.Format(CultureInfo.InvariantCulture, "<{0}>", GetAddress(allowUnicode));
        }

        /// <summary>
        /// this returns the full address with quoted display name.
        /// i.e. "some email address display name" <user@host>
        /// if displayname is not provided then this returns only user@host (no angle brackets)
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
         
            if (String.IsNullOrEmpty(DisplayName)) {
                return this.Address;
            }
            else {
                return String.Format("\"{0}\" {1}", DisplayName, SmtpAddress);
            }
        }

        public override bool Equals(object value) {
            if (value == null) {
                return false;
            }
            return ToString().Equals(value.ToString(),StringComparison.InvariantCultureIgnoreCase);
        }

        public override int GetHashCode(){
            return ToString().GetHashCode();
        }

        static EncodedStreamFactory encoderFactory = new EncodedStreamFactory();

        // Encodes the full email address, folding as needed
        internal string Encode(int charsConsumed, bool allowUnicode)
        {
            string encodedAddress = String.Empty;
            IEncodableStream encoder;
            byte[] buffer;

            Debug.Assert(this.Address != null, "address was null");

            //do we need to take into account the Display name?  If so, encode it
            if (!String.IsNullOrEmpty(this.displayName))
            {
                //figure out the encoding type.  If it's all ASCII and contains no CRLF then
                //it does not need to be encoded for parity with other email clients.  We will 
                //however fold at the end of the display name so that the email address itself can
                //be appended.
                if (MimeBasePart.IsAscii(this.displayName, false) || allowUnicode)
                {
                    encodedAddress = String.Format(CultureInfo.InvariantCulture, "\"{0}\"", this.displayName);
                }
                else
                {
                    //encode the displayname since it's non-ascii
                    encoder = encoderFactory.GetEncoderForHeader(this.displayNameEncoding, false, charsConsumed);
                    buffer = displayNameEncoding.GetBytes(this.displayName);
                    encoder.EncodeBytes(buffer, 0, buffer.Length);
                    encodedAddress = encoder.GetEncodedString();
                }

                //address should be enclosed in <> when a display name is present
                encodedAddress += " " + GetSmtpAddress(allowUnicode);
            }
            else
            {
                //no display name, just return the address
                encodedAddress = GetAddress(allowUnicode);
            }

            return encodedAddress;

        }
    }
}
