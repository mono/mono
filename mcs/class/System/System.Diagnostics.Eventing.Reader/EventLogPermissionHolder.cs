namespace System.Diagnostics.Eventing.Reader
{
    using System;
    using System.Diagnostics;

    internal class EventLogPermissionHolder
    {
        public static EventLogPermission GetEventLogPermission()
        {
            EventLogPermission permission = new EventLogPermission();
            EventLogPermissionEntry entry = new EventLogPermissionEntry(EventLogPermissionAccess.Administer, ".");
            permission.PermissionEntries.Add(entry);
            return permission;
        }
    }
}

