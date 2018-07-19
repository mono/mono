using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Collections;
using System.Reflection;
using System.Xml.Linq;

namespace System.Data.Linq
{
    /// <summary>
    /// Adds sorting feature to BindingList<T>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class SortableBindingList<T> : BindingList<T> {
        internal SortableBindingList(IList<T> list) : base(list) { }

        private bool isSorted = false;
        private PropertyDescriptor sortProperty = null;
        private ListSortDirection sortDirection = ListSortDirection.Ascending;

        protected override void RemoveSortCore() {
            isSorted = false;
            sortProperty = null;
        }

        protected override ListSortDirection SortDirectionCore {
            get { return sortDirection; }
        }
        protected override PropertyDescriptor SortPropertyCore {
            get { return sortProperty; }
        }
        protected override bool IsSortedCore {
            get { return isSorted; }
        }
        protected override bool SupportsSortingCore {
            get { return true; }
        }

        protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction) {
            //Only apply sort if the column is sortable, decision was made not to throw in this case.
            //Don't prevent nullable types from working.
            Type propertyType = prop.PropertyType;

            if (PropertyComparer.IsAllowable(propertyType))
            {
                ((List<T>)this.Items).Sort(new PropertyComparer(prop, direction));
                sortDirection = direction;
                sortProperty = prop;
                isSorted = true;
                OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
            }
        }

        internal class PropertyComparer : Comparer<T> {
            private PropertyDescriptor prop;
            private IComparer comparer;
            private ListSortDirection direction;
            private bool useToString;

            internal PropertyComparer(PropertyDescriptor prop, ListSortDirection direction) {
                if (prop.ComponentType != typeof(T)) {
                    throw new MissingMemberException(typeof(T).Name, prop.Name);
                }
                this.prop = prop;
                this.direction = direction;

                if (OkWithIComparable(prop.PropertyType)) {
                    Type comparerType = typeof(Comparer<>).MakeGenericType(prop.PropertyType);
                    PropertyInfo defaultComparer = comparerType.GetProperty("Default");
                    comparer = (IComparer)defaultComparer.GetValue(null, null);
                    useToString = false;
                }
                else if (OkWithToString(prop.PropertyType)) {
                    comparer = StringComparer.CurrentCultureIgnoreCase;
                    useToString = true;
                }
            }

            public override int Compare(T x, T y) {
                object xValue = prop.GetValue(x);
                object yValue = prop.GetValue(y);

                if (useToString) {
                    xValue = xValue != null ? xValue.ToString() : null;
                    yValue = yValue != null ? yValue.ToString() : null;
                }

                if (direction == ListSortDirection.Ascending) {
                    return comparer.Compare(xValue, yValue);
                }
                else {
                    return comparer.Compare(yValue, xValue);
                }
            }

            protected static bool OkWithToString(Type t) {
                // this is the list of types that behave specially for the purpose of 
                // sorting. if we have a property of this type, and it does not implement
                // IComparable, then this class will sort the properties according to the
                // ToString() method. 

                // In the case of an XNode, the ToString() returns the
                // XML, which is what we care about.
                return (t.Equals(typeof(XNode)) || t.IsSubclassOf(typeof(XNode)));
               
            }

            protected static bool OkWithIComparable(Type t) {
                return (t.GetInterface("IComparable") != null)
                    || (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>));
            }

            public static bool IsAllowable(Type t) {
                return OkWithToString(t) || OkWithIComparable(t);
            }
        }
    }
}
