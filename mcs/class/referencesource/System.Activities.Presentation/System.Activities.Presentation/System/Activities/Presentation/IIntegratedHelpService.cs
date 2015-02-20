//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.ComponentModel.Design;
    using System.Diagnostics.CodeAnalysis;

    public interface IIntegratedHelpService
    {
        void AddContextAttribute(string name, string value, HelpKeywordType keywordType);
        void RemoveContextAttribute(string name, string value);
        void ShowHelpFromKeyword(string helpKeyword);
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings",
          Justification = "This is to keep consistent with IHelpService")]
        void ShowHelpFromUrl(string helpUrl);
    }
}
