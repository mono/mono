//------------------------------------------------------------------------------
// <copyright file="CaseSensitiveStringDictionary.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
On UNIX systems the environment variable names are case sensitive (as the opposite from Windows). 
Thus using StringDictionary to store the environment settings is wrong 
(StringDictionary converts key values to lower case).
CaseSensitiveStringDictionary is derived from the StringDictionary and it does the same thing, 
except the conversion of the key to lower case. So its fully usable for UNIX systems.
This class is used to create the StringDictionary object everywhere 
its used for environment settings storage (only ProcessStartInfo.cs and Executor.cs). 
This change enables the correct UNIX behavior along with not changing public API.
Author: [....]
*/

#if PLATFORM_UNIX

namespace System.Collections.Specialized
{
    using System.Runtime.InteropServices;
    using System.Diagnostics;
    using System;
    using System.Collections;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.Security.Permissions;

    // This subclass of StringDictionary ensures that we do the lookups in a case-sensitive manner,
    // so we can match any string comparisons that are done on Unix.
    internal class CaseSensitiveStringDictionary : StringDictionary
    {
        public CaseSensitiveStringDictionary () {
        }

        public override string this[ string key ]
        {
            get
            {
                if ( key == null )
                {
                    throw new ArgumentNullException ( "key" );
                }

                return (string) contents[ key ];
            }
            set
            {
                if ( key == null )
                {
                    throw new ArgumentNullException ( "key" );
                }

                contents[ key ] = value;
            }
        }

        public override void Add ( string key, string value )
        {
            if ( key == null )
            {
                throw new ArgumentNullException ( "key" );
            }

            contents.Add ( key , value );
        }

        public override bool ContainsKey ( string key )
        {
            if ( key == null )
            {
                throw new ArgumentNullException ( "key" );
            }

            return contents.ContainsKey ( key );
        }

        public override void Remove ( string key )
        {
            if ( key == null )
            {
                throw new ArgumentNullException ( "key" );
            }

            contents.Remove ( key );
        }
    }
}


#endif // PLATFORM_UNIX
