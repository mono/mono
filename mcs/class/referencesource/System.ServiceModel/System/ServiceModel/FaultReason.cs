//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel
{
    using System.Collections.Generic;
    using System.Globalization;

    public class FaultReason
    {
        SynchronizedReadOnlyCollection<FaultReasonText> translations;

        public FaultReason(FaultReasonText translation)
        {
            if (translation == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("translation");

            Init(translation);
        }

        public FaultReason(string text)
        {
            // Let FaultReasonText constructor throw
            Init(new FaultReasonText(text));
        }

        internal FaultReason(string text, string xmlLang)
        {
            // Let FaultReasonText constructor throw
            Init(new FaultReasonText(text, xmlLang));
        }

        internal FaultReason(string text, CultureInfo cultureInfo)
        {
            // Let FaultReasonText constructor throw
            Init(new FaultReasonText(text, cultureInfo));
        }

        public FaultReason(IEnumerable<FaultReasonText> translations)
        {
            if (translations == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("translations"));
            int count = 0;
            foreach (FaultReasonText faultReasonText in translations)
                count++;
            if (count == 0)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.AtLeastOneFaultReasonMustBeSpecified), "translations"));
            FaultReasonText[] array = new FaultReasonText[count];
            int index = 0;
            foreach (FaultReasonText faultReasonText in translations)
            {
                if (faultReasonText == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("translations", SR.GetString(SR.NoNullTranslations));

                array[index++] = faultReasonText;
            }
            Init(array);
        }

        void Init(FaultReasonText translation)
        {
            Init(new FaultReasonText[] { translation });
        }

        void Init(FaultReasonText[] translations)
        {
            this.translations = new SynchronizedReadOnlyCollection<FaultReasonText>(new object(), Array.AsReadOnly<FaultReasonText>(translations));
        }

        public FaultReasonText GetMatchingTranslation()
        {
            return GetMatchingTranslation(CultureInfo.CurrentCulture);
        }

        // [....], This function should always return a translation so that a fault can be surfaced.
        public FaultReasonText GetMatchingTranslation(CultureInfo cultureInfo)
        {
            if (cultureInfo == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("cultureInfo"));

            // If there's only one translation, use it
            if (translations.Count == 1)
                return translations[0];

            // Search for an exact match
            for (int i = 0; i < translations.Count; i++)
                if (translations[i].Matches(cultureInfo))
                    return translations[i];

            // If no exact match is found, proceed by looking for the a translation with a language that is a parent of the current culture

            if (translations.Count == 0)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.NoMatchingTranslationFoundForFaultText)));

            // Search for a more general language
#pragma warning suppress 56506
            string localLang = cultureInfo.Name;
            while (true)
            {
                int idx = localLang.LastIndexOf('-');

                // We don't want to accept xml:lang=""
                if (idx == -1)
                    break;

                // Clip off the last subtag and look for a match
                localLang = localLang.Substring(0, idx);

                for (int i = 0; i < translations.Count; i++)
                    if (translations[i].XmlLang == localLang)
                        return translations[i];
            }

            // Return the first translation if no match is found
            return translations[0];
        }

        public SynchronizedReadOnlyCollection<FaultReasonText> Translations
        {
            get { return translations; }
        }

        public override string ToString()
        {
            if (translations.Count == 0)
                return string.Empty;

            return GetMatchingTranslation().Text;
        }
    }
}
