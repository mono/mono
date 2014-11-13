//------------------------------------------------------------------------------
// <copyright file="ImageClickEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */

namespace System.Web.UI {

    using System;

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public sealed class ImageClickEventArgs : EventArgs {

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int X;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int Y;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public double XRaw;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public double YRaw;


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public ImageClickEventArgs(int x,int y) {
            this.X = x;
            this.Y = y;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public ImageClickEventArgs(int x, int y, double xRaw, double yRaw) {
            this.X = x;
            this.Y = y;
            this.XRaw = xRaw;
            this.YRaw = yRaw;
        }

    }
}
