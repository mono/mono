//------------------------------------------------------------------------------
// <copyright file="MailSettingsSection.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net.Configuration
{
    using System.Configuration;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;    
    public sealed class MailSettingsSectionGroup : ConfigurationSectionGroup
    {
        public MailSettingsSectionGroup() 
        {
        }

        public SmtpSection Smtp
        {
            get { return (SmtpSection)Sections["smtp"]; }
	}
    }

    internal sealed class MailSettingsSectionGroupInternal
    {
        internal MailSettingsSectionGroupInternal()
        {
            this.smtp = SmtpSectionInternal.GetSection();
        }

        internal SmtpSectionInternal Smtp
        {
            get { return this.smtp; }
        }

        static internal MailSettingsSectionGroupInternal GetSection()
        {
            return new MailSettingsSectionGroupInternal();
        }

        SmtpSectionInternal smtp = null;
    }
}
