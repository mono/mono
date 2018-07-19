//------------------------------------------------------------------------------
// <copyright file="Sec.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * Sec
 * 
 * Copyright (c) 1998-1999, Microsoft Corporation
 * 
 */

namespace System.Web.Util {

    internal static class Sec {
        internal const int ONE_SECOND     = 1;            
        internal const int ONE_MINUTE     = ONE_SECOND * 60; 
        internal const int ONE_HOUR       = ONE_MINUTE * 60; 
        internal const int ONE_DAY        = ONE_HOUR * 24;   
        internal const int ONE_WEEK       = ONE_DAY * 7;     
        internal const int ONE_YEAR       = ONE_DAY * 365;
        internal const int ONE_LEAP_YEAR  = ONE_DAY * 366;
    }

}
