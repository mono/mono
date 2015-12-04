// Copyright (c) Microsoft Corporation. All rights reserved. 
//  
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// WHETHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED 
// WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// THE ENTIRE RISK OF USE OR RESULTS IN CONNECTION WITH THE USE OF THIS CODE 
// AND INFORMATION REMAINS WITH THE USER. 


/*********************************************************************
 * NOTE: A copy of this file exists at: WF\Activities\Common
 * The two files must be kept in [....].  Any change made here must also
 * be made to WF\Activities\Common\UserDataKeys.cs
*********************************************************************/
namespace System.Workflow.ComponentModel
{
    using System;

    internal static class UserDataKeys
    {
        internal static readonly Guid LookupPaths = new Guid("B56CB191-96AE-40fd-A640-955A6ABD733F");

        internal static readonly Guid BindDataSource = new Guid("0d40b274-9ff3-490d-b026-3e946269ca73");
        internal static readonly Guid BindDataContextActivity = new Guid("56897aed-3065-4a58-866d-35279d843e97");

        // definitions
        internal static readonly Guid CodeSegment_New = new Guid("4BA4C3CF-2B73-4fd8-802D-C3746B7885A8");
        internal static readonly Guid CodeSegment_ColumnNumber = new Guid("9981A4D3-0766-4295-BF61-BF252DF28B5E");

        //activity-bind related
        internal static readonly Guid CustomActivityDefaultName = new Guid("8bcd6c40-7bf6-4e60-8eea-bbf40bed92da");

        //Design time keys
        internal static readonly Guid NewBaseType = new Guid("C4ED69B4-DAFC-4faf-A3F8-D7D559ADDC21");
        internal static readonly Guid DesignTimeTypeNames = new Guid("8B018FBD-A60E-4378-8A79-8A190AE13EBA");
        internal static readonly Guid CustomActivity = new Guid("298CF3E0-E9E0-4d41-A11B-506E9132EB27");
    }
}
