/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This software is subject to the Microsoft Public License (Ms-PL). 
 * A copy of the license can be found in the license.htm file included 
 * in this distribution.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

namespace System.Web.Mvc {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Text;
    using System.Web;
    using System.Web.Mvc.Resources;

    public class TagBuilder {
        private string _idAttributeDotReplacement;

        private const string _attributeFormat = @" {0}=""{1}""";
        private const string _elementFormatEndTag = "</{0}>";
        private const string _elementFormatNormal = "<{0}{1}>{2}</{0}>";
        private const string _elementFormatSelfClosing = "<{0}{1} />";
        private const string _elementFormatStartTag = "<{0}{1}>";

        private string _innerHtml;

        public TagBuilder(string tagName) {
            if (String.IsNullOrEmpty(tagName)) {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "tagName");
            }

            TagName = tagName;
            Attributes = new SortedDictionary<string, string>(StringComparer.Ordinal);
        }

        public IDictionary<string, string> Attributes {
            get;
            private set;
        }

        public string IdAttributeDotReplacement {
            get {
                if (String.IsNullOrEmpty(_idAttributeDotReplacement)) {
                    _idAttributeDotReplacement = HtmlHelper.IdAttributeDotReplacement;
                }
                return _idAttributeDotReplacement;
            }
            set {
                _idAttributeDotReplacement = value;
            }
        }

        public string InnerHtml {
            get {
                return _innerHtml ?? String.Empty;
            }
            set {
                _innerHtml = value;
            }
        }

        public string TagName {
            get;
            private set;
        }

        public void AddCssClass(string value) {
            string currentValue;

            if (Attributes.TryGetValue("class", out currentValue)) {
                Attributes["class"] = value + " " + currentValue;
            }
            else {
                Attributes["class"] = value;
            }
        }

        public void GenerateId(string name) {
            if (!String.IsNullOrEmpty(name)) {
                MergeAttribute("id", name.Replace(".", IdAttributeDotReplacement));
            }
        }

        private string GetAttributesString() {
            StringBuilder sb = new StringBuilder();
            foreach (var attribute in Attributes) {
                string key = attribute.Key;
                string value = HttpUtility.HtmlAttributeEncode(attribute.Value);
                sb.AppendFormat(CultureInfo.InvariantCulture, _attributeFormat, key, value);
            }
            return sb.ToString();
        }

        public void MergeAttribute(string key, string value) {
            MergeAttribute(key, value, false /* replaceExisting */);
        }

        public void MergeAttribute(string key, string value, bool replaceExisting) {
            if (String.IsNullOrEmpty(key)) {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "key");
            }

            if (replaceExisting || !Attributes.ContainsKey(key)) {
                Attributes[key] = value;
            }
        }

        public void MergeAttributes<TKey, TValue>(IDictionary<TKey, TValue> attributes) {
            MergeAttributes(attributes, false /* replaceExisting */);
        }

        public void MergeAttributes<TKey, TValue>(IDictionary<TKey, TValue> attributes, bool replaceExisting) {
            if (attributes != null) {
                foreach (var entry in attributes) {
                    string key = Convert.ToString(entry.Key, CultureInfo.InvariantCulture);
                    string value = Convert.ToString(entry.Value, CultureInfo.InvariantCulture);
                    MergeAttribute(key, value, replaceExisting);
                }
            }
        }

        public void SetInnerText(string innerText) {
            InnerHtml = HttpUtility.HtmlEncode(innerText);
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public override string ToString() {
            return ToString(TagRenderMode.Normal);
        }

        public string ToString(TagRenderMode renderMode) {
            switch (renderMode) {
                case TagRenderMode.StartTag:
                    return String.Format(CultureInfo.InvariantCulture, _elementFormatStartTag, TagName, GetAttributesString());
                case TagRenderMode.EndTag:
                    return String.Format(CultureInfo.InvariantCulture, _elementFormatEndTag, TagName);
                case TagRenderMode.SelfClosing:
                    return String.Format(CultureInfo.InvariantCulture, _elementFormatSelfClosing, TagName, GetAttributesString());
                default:
                    return String.Format(CultureInfo.InvariantCulture, _elementFormatNormal, TagName, GetAttributesString(), InnerHtml);
            }
        }
    }
}
