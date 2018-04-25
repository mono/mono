using System;
using System.Collections.Specialized;
using System.Net.Mail;
using System.Globalization;

namespace System.Net.Mime
{
    /// <summary>
    /// Summary description for HeaderCollection.
    /// </summary>


    internal class HeaderCollection: NameValueCollection {
        MimeBasePart part = null;


        // default constructor
        // intentionally override the default comparer in the derived base class 
        internal HeaderCollection() : base(StringComparer.OrdinalIgnoreCase) {
        }


        public override void Remove(string name) {
            if(Logging.On)Logging.PrintInfo(Logging.Web, this, "Remove", name);
            if (name == null)
                throw new ArgumentNullException("name");

            if (name == string.Empty)
                throw new ArgumentException(SR.GetString(SR.net_emptystringcall,"name"), "name");

            MailHeaderID id = MailHeaderInfo.GetID(name);

            if (id == MailHeaderID.ContentType && part != null) {
                part.ContentType = null;
            } else if (id == MailHeaderID.ContentDisposition && part is MimePart) {
                ((MimePart)part).ContentDisposition = null;
            }
                
            base.Remove(name);
        }


        public override string Get(string name) {
            if(Logging.On)Logging.PrintInfo(Logging.Web, this, "Get", name);
            if (name == null)
                throw new ArgumentNullException("name");

            if (name == string.Empty)
                throw new ArgumentException(SR.GetString(SR.net_emptystringcall,"name"), "name");

            MailHeaderID id = MailHeaderInfo.GetID(name);

            if (id == MailHeaderID.ContentType && part != null) {
                part.ContentType.PersistIfNeeded(this,false);
            } else if (id == MailHeaderID.ContentDisposition && part is MimePart) {
                ((MimePart)part).ContentDisposition.PersistIfNeeded(this, false);
            }
            return base.Get(name);
        }


        
        public override string[] GetValues(string name) {
            if(Logging.On)Logging.PrintInfo(Logging.Web, this, "Get", name);
            if (name == null)
                throw new ArgumentNullException("name");

            if (name == string.Empty)
                throw new ArgumentException(SR.GetString(SR.net_emptystringcall,"name"), "name");

            MailHeaderID id = MailHeaderInfo.GetID(name);

            if (id == MailHeaderID.ContentType && part != null) {
                part.ContentType.PersistIfNeeded(this,false);
            } else if (id == MailHeaderID.ContentDisposition && part is MimePart) {
                ((MimePart)part).ContentDisposition.PersistIfNeeded(this, false);
            }
            return base.GetValues(name);
        }


        internal void InternalRemove(string name){
            base.Remove(name);
        }
        
        //set an existing header's value
        internal void InternalSet(string name, string value) {
            base.Set(name, value);
        }

        //add a new header and set its value
        internal void InternalAdd(string name, string value) {
            if (MailHeaderInfo.IsSingleton(name)) {
                base.Set(name, value);
            } 
            else {
                base.Add(name, value);
            }
        }

        public override void Set(string name, string value) {
            if(Logging.On)Logging.PrintInfo(Logging.Web, this, "Set", name.ToString() + "=" + value.ToString());
            if (name == null)
                throw new ArgumentNullException("name");

            if (value == null)
                throw new ArgumentNullException("value");

            if (name == string.Empty)
                throw new ArgumentException(SR.GetString(SR.net_emptystringcall,"name"), "name");

            if (value == string.Empty)
                throw new ArgumentException(SR.GetString(SR.net_emptystringcall,"value"), "name");

            if (!MimeBasePart.IsAscii(name,false)) {
                throw new FormatException(SR.GetString(SR.InvalidHeaderName));
            }

            // normalize the case of well known headers
            name = MailHeaderInfo.NormalizeCase(name);
            
            MailHeaderID id = MailHeaderInfo.GetID(name);

            value = value.Normalize(Text.NormalizationForm.FormC);

            if (id == MailHeaderID.ContentType && part != null) {
                part.ContentType.Set(value.ToLower(CultureInfo.InvariantCulture), this);
            } else if (id == MailHeaderID.ContentDisposition && part is MimePart) {
                ((MimePart)part).ContentDisposition.Set(value.ToLower(CultureInfo.InvariantCulture), this);
            } else {
                base.Set(name, value);
            }
        }


        public override void Add(string name, string value) {
            if(Logging.On)Logging.PrintInfo(Logging.Web, this, "Add", name.ToString() + "=" + value.ToString());
            if (name == null)
                throw new ArgumentNullException("name");

            if (value == null)
                throw new ArgumentNullException("value");

            if (name == string.Empty)
                throw new ArgumentException(SR.GetString(SR.net_emptystringcall,"name"), "name");

            if (value == string.Empty)
                throw new ArgumentException(SR.GetString(SR.net_emptystringcall,"value"), "name");

            MailBnfHelper.ValidateHeaderName(name);
            
            // normalize the case of well known headers
            name = MailHeaderInfo.NormalizeCase(name);

            MailHeaderID id = MailHeaderInfo.GetID(name);

            value = value.Normalize(Text.NormalizationForm.FormC);

            if(id == MailHeaderID.ContentType && part != null) {
                part.ContentType.Set(value.ToLower(CultureInfo.InvariantCulture), this);
            } else if (id == MailHeaderID.ContentDisposition && part is MimePart) {
                ((MimePart)part).ContentDisposition.Set(value.ToLower(CultureInfo.InvariantCulture), this);
            } else {
                InternalAdd(name, value);
            }
        }
    }
}
