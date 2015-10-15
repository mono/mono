// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Interface:  IAppDomainSetup
** 
** <OWNER>mray</OWNER>
**
**
** Purpose: Properties exposed to COM
**
** 
===========================================================*/
namespace System {

    using System.Runtime.InteropServices;

    [GuidAttribute("27FFF232-A7A8-40dd-8D4A-734AD59FCD41")]
    [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
[System.Runtime.InteropServices.ComVisible(true)]
    public interface IAppDomainSetup
    {
        String ApplicationBase {
            get;
            set;
        }

        String ApplicationName
        {
            get;
            set;
        }

        String CachePath
        {
            get;
            set;
        }

        String ConfigurationFile {
            get;
            set;
        }

        String DynamicBase
        {
            get;
            set;
        }

        String LicenseFile
        {
            get;
            set;
        }

        String PrivateBinPath
        {
            get;
            set;
        }

        String PrivateBinPathProbe
        {
            get;
            set;
        }

        String ShadowCopyDirectories
        {
            get;
            set;
        }

        String ShadowCopyFiles
        {
            get;
            set;
        }

    }
}
