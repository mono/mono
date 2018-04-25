//------------------------------------------------------------------------------
// <copyright file="HotSpotCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing.Design;
using System.Web.UI;

namespace System.Web.UI.WebControls {


    /// <devdoc>
    /// <para>Collection of HotSpots.</para>
    /// </devdoc>
    [
    Editor("System.Web.UI.Design.WebControls.HotSpotCollectionEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor))
    ]

    public sealed class HotSpotCollection : StateManagedCollection {
        private static readonly Type[] knownTypes = new Type[] {
                                                                typeof(CircleHotSpot),
                                                                typeof(RectangleHotSpot),
                                                                typeof(PolygonHotSpot),
                                                            };



        /// <devdoc>
        /// <para>Returns the HotSpot at a given index.</para>
        /// </devdoc>
        public HotSpot this[int index] {
            get {
                return (HotSpot)((IList)this)[index];
            }
        }


        /// <devdoc>
        /// <para>Adds a HotSpot to the collection.</para>
        /// </devdoc>
        public int Add(HotSpot spot) {
            return ((IList)this).Add(spot);
        }


        /// <devdoc>
        /// <para>Creates a known type of HotSpot.</para>
        /// </devdoc>
        protected override object CreateKnownType(int index) {
             switch (index) {
                case 0:
                    return new CircleHotSpot();
                case 1:
                    return new RectangleHotSpot();
                case 2:
                    return new PolygonHotSpot();
                default:
                    throw new ArgumentOutOfRangeException(SR.GetString(SR.HotSpotCollection_InvalidTypeIndex));
            }        
        }


        /// <devdoc>
        /// <para>Returns an ArrayList of known HotSpot types.</para>
        /// </devdoc>
        protected override Type[] GetKnownTypes() {
            return knownTypes;
        }


        /// <devdoc>
        /// <para>Inserts a HotSpot into the collection.</para>
        /// </devdoc>
        public void Insert(int index, HotSpot spot) {
            ((IList)this).Insert(index, spot);
        }


        /// <devdoc>
        /// <para>Validates that an object is a HotSpot.</para>
        /// </devdoc>
        protected override void OnValidate(object o) {
            base.OnValidate(o);
            if (!(o is HotSpot))
                throw new ArgumentException(SR.GetString(SR.HotSpotCollection_InvalidType));
        }


        /// <devdoc>
        /// <para>Removes a HotSpot from the collection.</para>
        /// </devdoc>
        public void Remove(HotSpot spot) {
            ((IList)this).Remove(spot);
        }


        /// <devdoc>
        /// <para>Removes a HotSpot from the collection at a given index.</para>
        /// </devdoc>
        public void RemoveAt(int index) {
            ((IList)this).RemoveAt(index);
        }


        /// <devdoc>
        /// <para>Marks a HotSpot as dirty so that it will record its entire state into view state.</para>
        /// </devdoc>
        protected override void SetDirtyObject(object o) {
            ((HotSpot)o).SetDirty();
        }
    }
}
