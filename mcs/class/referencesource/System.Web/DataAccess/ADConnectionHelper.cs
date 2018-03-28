//------------------------------------------------------------------------------
// <copyright file="ADConnectionHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.DataAccess 
{
    using  System.Net;
    using  System.Diagnostics;
    using  System.Web.Hosting;
    using  System.Web.Security;
    using  System.DirectoryServices;
    using  System.DirectoryServices.Protocols;
   
    internal static class ActiveDirectoryConnectionHelper
    {

        internal static DirectoryEntryHolder GetDirectoryEntry(DirectoryInformation directoryInfo, string objectDN, bool revertImpersonation)
        {
            Debug.Assert ((objectDN != null) && (objectDN.Length != 0));

            //
            // Get the adspath and create a directory entry holder
            //
            DirectoryEntryHolder holder = new DirectoryEntryHolder(new DirectoryEntry (
                                                                                                        directoryInfo.GetADsPath(objectDN), 
                                                                                                        directoryInfo.GetUsername(), 
                                                                                                        directoryInfo.GetPassword(), 
                                                                                                        directoryInfo.AuthenticationTypes));
            //
            // If  revertImpersonation is true, we need to revert
            //
            holder.Open(null,  revertImpersonation);
            return holder;
        }
    }

    internal sealed class DirectoryEntryHolder 
    {
        private ImpersonationContext ctx = null;
        private bool opened;
        private DirectoryEntry entry;

        internal DirectoryEntryHolder (DirectoryEntry entry)
        {
            Debug.Assert (entry != null);
            this.entry = entry;
        }

        internal void Open (HttpContext context, bool revertImpersonate)
        {
            if (opened)
                return; // Already opened

            //
            // Revert client impersonation if required
            //
            if (revertImpersonate)
            {
                ctx = new ApplicationImpersonationContext();
            }
            else
            {
                ctx = null;
            }

            opened = true; // Open worked!
        }

        internal void Close ()
        {
            if (!opened) // Not open!
                return;

            entry.Dispose();
            RestoreImpersonation();
            opened = false;
        }

        internal void RestoreImpersonation() {
            // Restore impersonation
            if (ctx != null)
            {
                ctx.Undo();
                ctx = null;
            }
        }

        internal DirectoryEntry DirectoryEntry
        {
            get { return entry; }
        }
    }
}
