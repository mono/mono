namespace System.Diagnostics.Eventing.Reader
{
    using Microsoft.Win32;
    using System;
    using System.Collections.Generic;
    using System.Security;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public class EventLogPropertySelector : IDisposable
    {
        private EventLogHandle renderContextHandleValues;

        [SecurityCritical]
        public EventLogPropertySelector(IEnumerable<string> propertyQueries)
        {
            string[] strArray;
            EventLogPermissionHolder.GetEventLogPermission().Demand();
            if (propertyQueries == null)
            {
                throw new ArgumentNullException("propertyQueries");
            }
            ICollection<string> is2 = propertyQueries as ICollection<string>;
            if (is2 != null)
            {
                strArray = new string[is2.Count];
                is2.CopyTo(strArray, 0);
            }
            else
            {
                strArray = new List<string>(propertyQueries).ToArray();
            }
            this.renderContextHandleValues = NativeWrapper.EvtCreateRenderContext(strArray.Length, strArray, Microsoft.Win32.UnsafeNativeMethods.EvtRenderContextFlags.EvtRenderContextValues);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        [SecuritySafeCritical]
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                EventLogPermissionHolder.GetEventLogPermission().Demand();
            }
            if ((this.renderContextHandleValues != null) && !this.renderContextHandleValues.IsInvalid)
            {
                this.renderContextHandleValues.Dispose();
            }
        }

        internal EventLogHandle Handle
        {
            get
            {
                return this.renderContextHandleValues;
            }
        }
    }
}

