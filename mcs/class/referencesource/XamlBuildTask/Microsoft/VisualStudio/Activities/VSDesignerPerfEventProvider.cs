//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace Microsoft.VisualStudio.Activities
{
    using System;
    using System.Diagnostics.Eventing;

    internal class VSDesignerPerfEventProvider
    {
        private EventProvider provider = null;

        public VSDesignerPerfEventProvider()
        {
            try
            {
                this.provider = new EventProvider(new Guid("{92C79DA3-CA7D-43d6-BF20-BBD15E7A4E49}"));
            }
            catch (PlatformNotSupportedException)
            {
                this.provider = null;
            }
        }

        internal void WriteEvent(VSDesignerPerfEvents perfEvent)
        {
            if (this.IsEnabled())
            {
                this.WriteEventHelper((int)perfEvent);
            }
        }

        private bool IsEnabled()
        {
            bool isEnabled = false;
            if (this.provider != null)
            {
                isEnabled = this.provider.IsEnabled();
            }

            return isEnabled;
        }

        private void WriteEventHelper(int eventId)
        {
            if (this.provider != null)
            {
                EventDescriptor descriptor = new EventDescriptor(eventId, 0, 0, 0, 0, 0, 0);
                this.provider.WriteEvent(ref descriptor);
            }
        }
    }
}
