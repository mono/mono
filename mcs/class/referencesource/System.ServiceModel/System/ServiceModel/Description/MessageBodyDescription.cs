//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
using System;
using System.ServiceModel.Channels;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace System.ServiceModel.Description
{
    public class MessageBodyDescription
    {
        private XmlName wrapperName;
        private string wrapperNs;
        private MessagePartDescriptionCollection parts;
        private MessagePartDescription returnValue;

        public MessageBodyDescription()
        {
            parts = new MessagePartDescriptionCollection();
        }

        internal MessageBodyDescription(MessageBodyDescription other)
        {
            this.WrapperName = other.WrapperName;
            this.WrapperNamespace = other.WrapperNamespace;
            this.parts = new MessagePartDescriptionCollection();
            foreach (MessagePartDescription mpd in other.Parts)
            {
                this.Parts.Add(mpd.Clone());
            }
            if (other.ReturnValue != null)
            {
                this.ReturnValue = other.ReturnValue.Clone();
            }
        }

        internal MessageBodyDescription Clone()
        {
            return new MessageBodyDescription(this);
        }

        public MessagePartDescriptionCollection Parts
        {
            get { return parts; }
        }

        [DefaultValue(null)]
        public MessagePartDescription ReturnValue
        {
            get { return returnValue; }
            set { returnValue = value; }
        }

        [DefaultValue(null)]
        public string WrapperName
        {
            get { return wrapperName == null ? null : wrapperName.EncodedName; }
            set { wrapperName = new XmlName(value, true /*isEncoded*/); }
        }

        [DefaultValue(null)]
        public string WrapperNamespace
        {
            get { return wrapperNs; }
            set { wrapperNs = value; }
        }

    }
}
