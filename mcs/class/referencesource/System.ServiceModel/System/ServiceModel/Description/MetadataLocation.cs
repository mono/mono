//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Description
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;

    [XmlRoot(ElementName = MetadataStrings.MetadataExchangeStrings.Location, Namespace = MetadataStrings.MetadataExchangeStrings.Namespace)]
    public class MetadataLocation
    {
        string location;

        public MetadataLocation()
        {
        }

        public MetadataLocation(string location)
        {
            this.Location = location;
        }

        [XmlText]
        public string Location
        {
            get { return this.location; }
            set
            {
                if (value != null)
                {
                    Uri uri;
                    if (!Uri.TryCreate(value, UriKind.RelativeOrAbsolute, out uri))
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.SFxMetadataReferenceInvalidLocation, value));
                }

                this.location = value;
            }
        }
    }
}
