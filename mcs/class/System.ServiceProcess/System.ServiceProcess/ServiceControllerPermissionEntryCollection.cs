//
// System.ServiceProcess.ServiceControllerPermissionEntry.cs
//
// Author:
//      Duncan Mak (duncan@ximian.com)
//
// (C) 2003, Ximian Inc.
//

using System;
using System.Collections;

namespace System.ServiceProcess {

        [Serializable]
        public class ServiceControllerPermissionEntryCollection : CollectionBase
        {
                
                public ServiceControllerPermissionEntry this [int index] {

                        get { return base.List [index] as ServiceControllerPermissionEntry; }

                        set { base.List [index] = value; }

                }

                public int Add (ServiceControllerPermissionEntry value)
                {
                        return base.List.Add (value);
                }

                public void AddRange (ServiceControllerPermissionEntry [] value)
                {
                        foreach (ServiceControllerPermissionEntry entry in value)
                                base.List.Add (entry);
                }

                public void AddRange (ServiceControllerPermissionEntryCollection value)
                {
                        foreach (ServiceControllerPermissionEntry entry in value)
                                base.List.Add (entry);
                }

                public bool Contains (ServiceControllerPermissionEntry value)
                {
                        return base.List.Contains (value);
                }

                public void CopyTo (ServiceControllerPermissionEntry [] array, int index)
                {
                        base.List.CopyTo (array, index);
                }

                public int IndexOf (ServiceControllerPermissionEntry value)
                {
                        return base.List.IndexOf (value);
                }

                public void Insert (int index, ServiceControllerPermissionEntry value)
                {
                        base.List.Insert (index, value);
                }

                public void Remove (ServiceControllerPermissionEntry value)
                {
                        base.List.Remove (value);
                }

                protected override void OnClear () {}

                protected override void OnInsert (int index, object value) {}

                protected override void OnRemove (int index, object value) {}

                protected override void OnSet (int index, object oldValue, object newValue) {}
        }
}
