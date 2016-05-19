// -------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All Rights Reserved.
// -------------------------------------------------------------------
//From \\authoring\Sparkle\Source\1.0.1083.0\Common\Source\Framework\Data
namespace System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework.Data
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Windows;

    // <summary>
    // Workaround for ObservableCollection not supporting IComparers.  The implementation was copied from ObservableCollection.
    // </summary>
    // <typeparam name="T"></typeparam>
    internal sealed class ObservableCollectionWorkaround<T> : ObservableCollection<T>
    {
        public ObservableCollectionWorkaround()
        {
        }

        public ObservableCollectionWorkaround(List<T> list)
            : base(list)
        {
        }

        public ObservableCollectionWorkaround(ICollection collection)
        {
            foreach (T item in collection)
            {
                this.Add(item);
            }
        }

        public int BinarySearch(T value, IComparer<T> comparer)
        {
            return ((List<T>)base.Items).BinarySearch(value, comparer);
        }

        public void Sort()
        {
            ((List<T>)base.Items).Sort();
            this.OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, null, -1));
        }

        public void Sort(IComparer<T> comparer)
        {
            ((List<T>)base.Items).Sort(comparer);
            this.OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, null, -1));
        }

        public void Sort(Comparison<T> comparison)
        {
            ((List<T>)base.Items).Sort(comparison);
            this.OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, null, -1));
        }

        public void Reverse()
        {
            ((List<T>)base.Items).Reverse();
        }

    }
}
