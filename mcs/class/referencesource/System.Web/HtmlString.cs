//------------------------------------------------------------------------------
// <copyright file="HtmlString.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web {
    //  Simple objects that wraps a string that is assumed to not need HTML encoded.
    //  It implements IHtmlString, so when calling HttpUtility.HtmlEncode(htmlString), the string won't be encoded.
    public class HtmlString: IHtmlString {

        private string _htmlString;

        public HtmlString(string value) {
            _htmlString = value;
        }

        public string ToHtmlString() {
            return _htmlString;
        }

        public override string ToString() {
            return _htmlString;
        }
    }
}
