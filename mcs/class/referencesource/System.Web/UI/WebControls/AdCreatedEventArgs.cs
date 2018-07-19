//------------------------------------------------------------------------------
// <copyright file="AdCreatedEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System.Collections;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Web.Util;


    /// <devdoc>
    /// <para>Provides data for the <see langword='AdCreated'/> event.</para>
    /// </devdoc>
    public class AdCreatedEventArgs : EventArgs {

        internal const string ImageUrlElement = "ImageUrl";
        internal const string NavigateUrlElement = "NavigateUrl";
        internal const string AlternateTextElement = "AlternateText";
        private const string WidthElement = "Width";
        private const string HeightElement = "Height";

        private string imageUrl = String.Empty;
        private string navigateUrl = String.Empty;
        private string alternateText = String.Empty;
        private IDictionary adProperties;

        private bool hasHeight;
        private bool hasWidth;
        private Unit width;
        private Unit height;


        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.AdCreatedEventArgs'/> 
        /// class.</para>
        /// </devdoc>
        public AdCreatedEventArgs(IDictionary adProperties) :
            this(adProperties, null, null, null) {
        }

        /// <devdoc>
        ///     <para>Internal constructor for making use of parameter keys if
        ///     provided.  A note is that we cannot change the constructor
        ///     above because it was made public.</para>
        /// </devdoc>
        internal AdCreatedEventArgs(IDictionary adProperties,
                                    String imageUrlField,
                                    String navigateUrlField,
                                    String alternateTextField) {
            if (adProperties != null) {
                // Initialize the other properties from the dictionary
                this.adProperties = adProperties;
                this.imageUrl = GetAdProperty(ImageUrlElement, imageUrlField);
                this.navigateUrl = GetAdProperty(NavigateUrlElement, navigateUrlField);
                this.alternateText = GetAdProperty(AlternateTextElement, alternateTextField);

                // VSWhidbey 141916: Check validity of Width and Height
                hasWidth = GetUnitValue(adProperties, WidthElement, ref width);
                hasHeight = GetUnitValue(adProperties, HeightElement, ref height);
            }
        }

        /// <devdoc>
        ///    <para>Gets the dictionary containing all the advertisement 
        ///       properties extracted from the XML file after the <see langword='AdCreated '/>
        ///       event is raised.</para>
        /// </devdoc>
        public IDictionary AdProperties {
            get {
                return adProperties;
            }
        }

        /// <devdoc>
        ///    <para> 
        ///       Specifies the alternate text and tooltip (if browser supported) that will be
        ///       rendered in the <see cref='System.Web.UI.WebControls.AdRotator'/>.</para>
        /// </devdoc>
        public string AlternateText {
            get { 
                return alternateText;
            }
            set {
                alternateText = value;
            }
        }

        internal bool HasHeight {
            get {
                return hasHeight;
            }
        }

        internal bool HasWidth {
            get {
                return hasWidth;
            }
        }

        internal Unit Height {
            get {
                return height;
            }
        }

        /// <devdoc>
        /// <para> Specifies the image that will be rendered in the <see cref='System.Web.UI.WebControls.AdRotator'/>.</para>
        /// </devdoc>
        public string ImageUrl {
            get {
                return imageUrl;
            }
            set {
                imageUrl = value;
            }
        }

        /// <devdoc>
        ///    <para> Specifies the target URL that will be rendered in the
        ///    <see cref='System.Web.UI.WebControls.AdRotator'/>.</para>
        /// </devdoc>
        public string NavigateUrl {
            get {
                return navigateUrl;
            }
            set {
                navigateUrl = value;
            }
        }

        internal Unit Width {
            get {
                return width;
            }
        }

        private String GetAdProperty(String defaultIndex, String keyIndex) {
            String index = (String.IsNullOrEmpty(keyIndex)) ? defaultIndex : keyIndex;
            String property = (adProperties == null) ? null : (String) adProperties[index];
            return (property == null) ? String.Empty : property;
        }

        private bool GetUnitValue(IDictionary properties, String keyIndex, ref Unit unitValue) {
            Debug.Assert(properties != null);

            string temp = properties[keyIndex] as string;
            if (!String.IsNullOrEmpty(temp)) {
                try {
                    unitValue = Unit.Parse(temp, CultureInfo.InvariantCulture);
                }
                catch {
                    throw new FormatException(
                        SR.GetString(SR.AdRotator_invalid_integer_format, temp, keyIndex, typeof(Unit).FullName));
                }
                return true;
            }
            return false;
        }
    }   
}
