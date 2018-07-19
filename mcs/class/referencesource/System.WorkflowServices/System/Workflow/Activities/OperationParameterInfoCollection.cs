//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Workflow.Activities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.Design.Serialization;
    using System.Diagnostics.CodeAnalysis;
    using System.ServiceModel;
    using System.Workflow.ComponentModel.Serialization;

    [Serializable]
    [DesignerSerializer(typeof(CollectionMarkupSerializer), typeof(WorkflowMarkupSerializer))]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class OperationParameterInfoCollection : List<OperationParameterInfo>,
        IList<OperationParameterInfo>,
        IList
    {
        [SuppressMessage("Microsoft.Usage", "CA2235:MarkAllNonSerializableFields")]
        OperationInfoBase owner = null;

        public OperationParameterInfoCollection()
        {
        }

        public OperationParameterInfoCollection(OperationInfoBase owner)
        {
            if (owner == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("owner");
            }

            this.owner = owner;
        }

        public new int Count
        {
            get
            {
                return ((ICollection<OperationParameterInfo>) this).Count;
            }
        }

        int ICollection<OperationParameterInfo>.Count
        {
            get
            {
                return base.Count;
            }
        }

        bool ICollection<OperationParameterInfo>.IsReadOnly
        {
            get
            {
                return false;
            }
        }
        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        object ICollection.SyncRoot
        {
            get { return this; }
        }

        bool IList.IsFixedSize
        {
            get
            {
                return false;
            }
        }

        bool IList.IsReadOnly
        {
            get
            {
                return ((IList<OperationParameterInfo>) this).IsReadOnly;
            }
        }

        public new OperationParameterInfo this[int index]
        {
            get
            {
                return ((IList<OperationParameterInfo>) this)[index];
            }
            set
            {
                ((IList<OperationParameterInfo>) this)[index] = value;
            }
        }

        public OperationParameterInfo this[string key]
        {
            get
            {
                if (key == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("key");
                }

                for (int index = 0; index < this.Count; index++)
                {
                    if (string.Equals(this[index].Name, key, StringComparison.Ordinal))
                    {
                        return this[index];
                    }
                }
                return null;
            }
        }

        OperationParameterInfo IList<OperationParameterInfo>.this[int index]
        {
            get
            {
                return base[index];
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");
                }

                base[index] = value;
            }
        }
        object IList.this[int index]
        {
            get
            {
                return ((IList<OperationParameterInfo>) this)[index];
            }

            set
            {
                if (!(value is OperationParameterInfo))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(
                        "value",
                        SR2.GetString(SR2.Error_InvalidListItem, typeof(OperationParameterInfo).FullName));
                }
                ((IList<OperationParameterInfo>) this)[index] = (OperationParameterInfo) value;
            }
        }

        public new void Add(OperationParameterInfo item)
        {
            ((IList<OperationParameterInfo>) this).Add(item);
        }

        public new void Clear()
        {
            ((IList<OperationParameterInfo>) this).Clear();
        }

        public new bool Contains(OperationParameterInfo item)
        {
            return ((IList<OperationParameterInfo>) this).Contains(item);
        }

        public new IEnumerator<OperationParameterInfo> GetEnumerator()
        {
            return ((IList<OperationParameterInfo>) this).GetEnumerator();
        }

        void ICollection<OperationParameterInfo>.Add(OperationParameterInfo item)
        {
            if (item == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");
            }

            base.Add(item);
        }

        void ICollection<OperationParameterInfo>.Clear()
        {
            base.Clear();
        }

        bool ICollection<OperationParameterInfo>.Contains(OperationParameterInfo item)
        {
            return base.Contains(item);
        }
        void ICollection<OperationParameterInfo>.CopyTo(OperationParameterInfo[] array, int arrayIndex)
        {
            base.CopyTo(array, arrayIndex);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            if (array == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("array");
            }

            for (int loop = 0; loop < this.Count; loop++)
            {
                array.SetValue(this[loop], loop + index);
            }
        }

        bool ICollection<OperationParameterInfo>.Remove(OperationParameterInfo item)
        {
            if (!base.Contains(item))
            {
                return false;
            }

            int index = base.IndexOf(item);
            if (index >= 0)
            {
                base.Remove(item);
                return true;
            }
            return false;
        }


        IEnumerator<OperationParameterInfo> IEnumerable<OperationParameterInfo>.GetEnumerator()
        {
            return base.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)((IList<OperationParameterInfo>) this).GetEnumerator();
        }

        int IList.Add(object value)
        {
            if (!(value is OperationParameterInfo))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(
                    "value",
                    SR2.GetString(SR2.Error_InvalidListItem, typeof(OperationParameterInfo).FullName));
            }
            ((IList<OperationParameterInfo>) this).Add((OperationParameterInfo) value);
            return this.Count - 1;
        }

        void IList.Clear()
        {
            ((IList<OperationParameterInfo>) this).Clear();
        }

        bool IList.Contains(object value)
        {
            if (!(value is OperationParameterInfo))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(
                    "value",
                    SR2.GetString(SR2.Error_InvalidListItem, typeof(OperationParameterInfo).FullName));
            }
            return (((IList<OperationParameterInfo>) this).Contains((OperationParameterInfo) value));
        }
        int IList<OperationParameterInfo>.IndexOf(OperationParameterInfo item)
        {
            return base.IndexOf(item);
        }

        int IList.IndexOf(object value)
        {
            if (!(value is OperationParameterInfo))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(
                    "value",
                    SR2.GetString(SR2.Error_InvalidListItem, typeof(OperationParameterInfo).FullName));
            }
            return ((IList<OperationParameterInfo>) this).IndexOf((OperationParameterInfo) value);
        }

        void IList<OperationParameterInfo>.Insert(int index, OperationParameterInfo item)
        {
            if (index < 0 || index > base.Count)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("index"));
            }
            if (item == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");
            }

            base.Insert(index, item);
        }

        void IList.Insert(int index, object value)
        {
            if (!(value is OperationParameterInfo))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(
                    "value",
                    SR2.GetString(SR2.Error_InvalidListItem, typeof(OperationParameterInfo).FullName));
            }
            ((IList<OperationParameterInfo>) this).Insert(index, (OperationParameterInfo) value);
        }

        void IList.Remove(object value)
        {
            if (!(value is OperationParameterInfo))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(
                    "value",
                    SR2.GetString(SR2.Error_InvalidListItem, typeof(OperationParameterInfo).FullName));
            }
            ((IList<OperationParameterInfo>) this).Remove((OperationParameterInfo) value);
        }

        void IList<OperationParameterInfo>.RemoveAt(int index)
        {
            if (index < 0 || index >= base.Count)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("index"));
            }

            base.RemoveAt(index);
        }

        public new int IndexOf(OperationParameterInfo item)
        {
            return ((IList<OperationParameterInfo>) this).IndexOf(item);
        }

        public new void Insert(int index, OperationParameterInfo item)
        {
            ((IList<OperationParameterInfo>) this).Insert(index, item);
        }

        public new bool Remove(OperationParameterInfo item)
        {
            return ((IList<OperationParameterInfo>) this).Remove(item);
        }

        public new void RemoveAt(int index)
        {
            ((IList<OperationParameterInfo>) this).RemoveAt(index);
        }
    }
}
