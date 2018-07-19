//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel
{
    using System.Globalization;
    using System.Threading;

    public class FaultReasonText
    {
        string xmlLang;
        string text;

        public FaultReasonText(string text)
        {
            if (text == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("text"));
            this.text = text;
            this.xmlLang = CultureInfo.CurrentCulture.Name;
        }

        public FaultReasonText(string text, string xmlLang)
        {
            if (text == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("text"));
            if (xmlLang == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("xmlLang"));
            this.text = text;
            this.xmlLang = xmlLang;
        }

        public FaultReasonText(string text, CultureInfo cultureInfo)
        {
            if (text == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("text"));
            if (cultureInfo == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("cultureInfo"));
            this.text = text;
            this.xmlLang = cultureInfo.Name;
        }

        public bool Matches(CultureInfo cultureInfo)
        {
            if (cultureInfo == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("cultureInfo"));

            return xmlLang == cultureInfo.Name;
        }

        public string XmlLang
        {
            get { return xmlLang; }
        }

        public string Text
        {
            get { return text; }
        }
    }
}
