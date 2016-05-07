//------------------------------------------------------------------------------
// <copyright file="Msec.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * Msec
 * 
 * Copyright (c) 1998-1999, Microsoft Corporation
 * 
 */

namespace System.Web.Util {

    internal class Msec {
        internal const int ONE_SECOND     = 1000;            
        internal const int ONE_MINUTE     = ONE_SECOND * 60; 
        internal const int ONE_HOUR       = ONE_MINUTE * 60; 
        internal const int ONE_DAY        = ONE_HOUR * 24;   
        internal const int ONE_WEEK       = ONE_DAY * 7;     
        internal const long ONE_YEAR      = ONE_DAY * 365L;
        internal const long ONE_LEAP_YEAR = ONE_DAY * 366L;

        private Msec() {}
    }
}
