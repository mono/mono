#pragma warning disable 649 // Disable CS0649: "field is never assigned to"

//------------------------------------------------------------------------------
//------------------------------------------------------------------------------

namespace MS.Win32 {
    using Accessibility;
    using System.Runtime.InteropServices;
    using System;
    using System.Security.Permissions;
    using System.Collections;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using MS.Internal;
#if !DRT && !UIAUTOMATIONTYPES
    using MS.Internal.Interop;
#endif
    using Microsoft.Win32;
    using System.Security;
    // The SecurityHelper class differs between assemblies and could not actually be
    //  shared, so it is duplicated across namespaces to prevent name collision.
#if WINDOWS_BASE
    using MS.Internal.WindowsBase;
#elif PRESENTATION_CORE
    using MS.Internal.PresentationCore;
#elif PRESENTATIONFRAMEWORK
    using MS.Internal.PresentationFramework;
#elif UIAUTOMATIONTYPES
    using MS.Internal.UIAutomationTypes;
#elif DRT
    using MS.Internal.Drt;
#else
#error Attempt to use a class (duplicated across multiple namespaces) from an unknown assembly.
#endif

    internal partial class NativeMethods {
 #if !FRAMEWORK_NATIVEMETHODS
        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class XFORM {
            public float eM11;
            public float eM12;
            public float eM21;
            public float eM22;
            public float eDx;
            public float eDy;

            public XFORM()
            {
                this.eM11 = this.eM22 = 1; // identity matrix.
            }

            public XFORM( float em11, float em12, float em21, float em22, float edx, float edy )
            {
                this.eM11 = em11;
                this.eM12 = em12;
                this.eM21 = em21;
                this.eM22 = em22;
                this.eDx  = edx;
                this.eDy  = edy;
            }

            public XFORM( float[] elements )
            {
                this.eM11 = elements[0];
                this.eM12 = elements[1];
                this.eM21 = elements[2];
                this.eM22 = elements[3];
                this.eDx  = elements[4];
                this.eDy  = elements[5];
            }

            public override string ToString()
            {
                return String.Format(System.Globalization.CultureInfo.CurrentCulture,"[{0}, {1}, {2}, {3}, {4}, {5}]", this.eM11, this.eM12, this.eM21, this.eM22, this.eDx, this.eDy );
            }

            public override bool Equals( object obj )
            {
                XFORM xform = obj as XFORM;

                if( xform == null )
                {
                    return false;
                }

                return this.eM11 == xform.eM11 &&
                       this.eM12 == xform.eM12 &&
                       this.eM21 == xform.eM21 &&
                       this.eM22 == xform.eM22 &&
                       this.eDx  == xform.eDx  &&
                       this.eDy  == xform.eDy;
            }

            public override int GetHashCode()
            {
                return this.ToString().GetHashCode();
            }

        }
#endif

        public static IntPtr InvalidIntPtr = (IntPtr)(-1);
        public static IntPtr LPSTR_TEXTCALLBACK = (IntPtr)(-1);
        public static HandleRef NullHandleRef = new HandleRef(null, IntPtr.Zero);

        public const int ERROR = 0;

        public const int BITMAPINFO_MAX_COLORSIZE = 256;
#if never
        public const int BI_BITFIELDS = 3;

        public enum RegionFlags {
            ERROR = 0,
            NULLREGION = 1,
            SIMPLEREGION = 2,
            COMPLEXREGION = 3,
        }

        public const int
        /* FONT WEIGHT (BOLD) VALUES */
        FW_DONTCARE         = 0,
        FW_NORMAL           = 400,
        FW_BOLD             = 700,
        // some others...

        /* FONT CHARACTER SET */
        ANSI_CHARSET        = 0,
        DEFAULT_CHARSET     = 1,
        // plus others ....

        /* Font OutPrecision */
        OUT_DEFAULT_PRECIS  = 0,
        OUT_TT_PRECIS       = 4,
        OUT_TT_ONLY_PRECIS  = 7,

        /* polygon fill mode */
        ALTERNATE = 1,
        WINDING = 2,

        // text align
        TA_DEFAULT = 0,

        // brush
        BS_SOLID = 0,
        HOLLOW_BRUSH = 5,

        // Binary raster operations.
        R2_BLACK            = 1,  /*  0       */
        R2_NOTMERGEPEN      = 2,  /* DPon     */
        R2_MASKNOTPEN       = 3,  /* DPna     */
        R2_NOTCOPYPEN       = 4,  /* PN       */
        R2_MASKPENNOT       = 5,  /* PDna     */
        R2_NOT              = 6,  /* Dn       */
        R2_XORPEN           = 7,  /* DPx      */
        R2_NOTMASKPEN       = 8,  /* DPan     */
        R2_MASKPEN          = 9,  /* DPa      */
        R2_NOTXORPEN        = 10, /* DPxn     */
        R2_NOP              = 11, /* D        */
        R2_MERGENOTPEN      = 12, /* DPno     */
        R2_COPYPEN          = 13, /* P        */
        R2_MERGEPENNOT      = 14, /* PDno     */
        R2_MERGEPEN         = 15, /* DPo      */
        R2_WHITE            = 16 /*  1       */;


        public const int
        /* SetGraphicsMode(hdc, iMode ) */
        GM_COMPATIBLE       = 1,
        GM_ADVANCED         = 2,
        MWT_IDENTITY        = 1;

#endif
        public const int
//        PAGE_READONLY = 0x02,
        PAGE_READWRITE = 0x04,
//        PAGE_WRITECOPY = 0x08,
//              FILE_MAP_COPY = 0x0001,
//              FILE_MAP_WRITE = 0x0002,
                FILE_MAP_READ = 0x0004;

        public const int APPCOMMAND_BROWSER_BACKWARD       = 1;
        public const int APPCOMMAND_BROWSER_FORWARD        = 2;
        public const int APPCOMMAND_BROWSER_REFRESH        = 3;
        public const int APPCOMMAND_BROWSER_STOP           = 4;
        public const int APPCOMMAND_BROWSER_SEARCH         = 5;
        public const int APPCOMMAND_BROWSER_FAVORITES      = 6;
        public const int APPCOMMAND_BROWSER_HOME           = 7;
        public const int APPCOMMAND_VOLUME_MUTE            = 8;
        public const int APPCOMMAND_VOLUME_DOWN            = 9;
        public const int APPCOMMAND_VOLUME_UP              = 10;
        public const int APPCOMMAND_MEDIA_NEXTTRACK        = 11;
        public const int APPCOMMAND_MEDIA_PREVIOUSTRACK    = 12;
        public const int APPCOMMAND_MEDIA_STOP             = 13;
        public const int APPCOMMAND_MEDIA_PLAY_PAUSE       = 14;
        public const int APPCOMMAND_LAUNCH_MAIL            = 15;
        public const int APPCOMMAND_LAUNCH_MEDIA_SELECT    = 16;
        public const int APPCOMMAND_LAUNCH_APP1            = 17;
        public const int APPCOMMAND_LAUNCH_APP2            = 18;
        public const int APPCOMMAND_BASS_DOWN              = 19;
        public const int APPCOMMAND_BASS_BOOST             = 20;
        public const int APPCOMMAND_BASS_UP                = 21;
        public const int APPCOMMAND_TREBLE_DOWN            = 22;
        public const int APPCOMMAND_TREBLE_UP              = 23;
        public const int APPCOMMAND_MICROPHONE_VOLUME_MUTE = 24;
        public const int APPCOMMAND_MICROPHONE_VOLUME_DOWN = 25;
        public const int APPCOMMAND_MICROPHONE_VOLUME_UP   = 26;
        public const int APPCOMMAND_HELP                   = 27;
        public const int APPCOMMAND_FIND                   = 28;
        public const int APPCOMMAND_NEW                    = 29;
        public const int APPCOMMAND_OPEN                   = 30;
        public const int APPCOMMAND_CLOSE                  = 31;
        public const int APPCOMMAND_SAVE                   = 32;
        public const int APPCOMMAND_PRINT                  = 33;
        public const int APPCOMMAND_UNDO                   = 34;
        public const int APPCOMMAND_REDO                   = 35;
        public const int APPCOMMAND_COPY                   = 36;
        public const int APPCOMMAND_CUT                    = 37;
        public const int APPCOMMAND_PASTE                  = 38;
        public const int APPCOMMAND_REPLY_TO_MAIL          = 39;
        public const int APPCOMMAND_FORWARD_MAIL           = 40;
        public const int APPCOMMAND_SEND_MAIL              = 41;
        public const int APPCOMMAND_SPELL_CHECK            = 42;
        public const int APPCOMMAND_DICTATE_OR_COMMAND_CONTROL_TOGGLE    = 43;
        public const int APPCOMMAND_MIC_ON_OFF_TOGGLE      = 44;
        public const int APPCOMMAND_CORRECTION_LIST        = 45;
        public const int APPCOMMAND_MEDIA_PLAY             = 46;
        public const int APPCOMMAND_MEDIA_PAUSE            = 47;
        public const int APPCOMMAND_MEDIA_RECORD           = 48;
        public const int APPCOMMAND_MEDIA_FAST_FORWARD     = 49;
        public const int APPCOMMAND_MEDIA_REWIND           = 50;
        public const int APPCOMMAND_MEDIA_CHANNEL_UP       = 51;
        public const int APPCOMMAND_MEDIA_CHANNEL_DOWN     = 52;
        public const int FAPPCOMMAND_MOUSE = 0x8000;
        public const int FAPPCOMMAND_KEY   = 0;
        public const int FAPPCOMMAND_OEM   = 0x1000;
        public const int FAPPCOMMAND_MASK  = 0xF000;

#if never

        public const int SHGFI_ICON = 0x000000100  ,   // get icon
        SHGFI_DISPLAYNAME       = 0x000000200,     // get display name
        SHGFI_TYPENAME          = 0x000000400,     // get type name
        SHGFI_ATTRIBUTES        = 0x000000800,     // get attributes
        SHGFI_ICONLOCATION      = 0x000001000,     // get icon location
        SHGFI_EXETYPE           = 0x000002000,     // return exe type
        SHGFI_SYSICONINDEX      = 0x000004000,     // get system icon index
        SHGFI_LINKOVERLAY       = 0x000008000,     // put a link overlay on icon
        SHGFI_SELECTED          = 0x000010000,     // show icon in selected state
        SHGFI_ATTR_SPECIFIED    = 0x000020000,     // get only specified attributes
        SHGFI_LARGEICON         = 0x000000000,     // get large icon
        SHGFI_SMALLICON         = 0x000000001,     // get small icon
        SHGFI_OPENICON          = 0x000000002,     // get open icon
        SHGFI_SHELLICONSIZE     = 0x000000004,     // get shell size icon
        SHGFI_PIDL              = 0x000000008,     // pszPath is a pidl
        SHGFI_USEFILEATTRIBUTES = 0x000000010,     // use passed dwFileAttribute
        SHGFI_ADDOVERLAYS       = 0x000000020,     // apply the appropriate overlays
        SHGFI_OVERLAYINDEX      = 0x000000040;     // Get the index of the overlay

        public const int DM_DISPLAYORIENTATION = 0x00000080;

        public const int AUTOSUGGEST = 0x10000000,
        AUTOSUGGEST_OFF = 0x20000000,
        AUTOAPPEND =  0x40000000,
        AUTOAPPEND_OFF = (unchecked((int)0x80000000));

        public const int ARW_BOTTOMLEFT = 0x0000,
        ARW_BOTTOMRIGHT = 0x0001,
        ARW_TOPLEFT = 0x0002,
        ARW_TOPRIGHT = 0x0003,
        ARW_LEFT = 0x0000,
        ARW_RIGHT = 0x0000,
        ARW_UP = 0x0004,
        ARW_DOWN = 0x0004,
        ARW_HIDE = 0x0008,
        ACM_OPENA = (0x0400+100),
        ACM_OPENW = (0x0400+103),
        ADVF_NODATA = 1,
        ADVF_ONLYONCE = 4,
        ADVF_PRIMEFIRST = 2;
                // Note: ADVF_ONLYONCE and ADVF_PRIMEFIRST values now conform with objidl.dll but are backwards from
                // Platform SDK documentation as of 07/21/2003.
        // http://msdn.microsoft.com/library/default.asp?url=/library/en-us/com/htm/oen_a2z_8jxi.asp.
        // See VSWhidbey bug#96162.

        public const int BCM_GETIDEALSIZE = 0x1601;
#endif
        public const int BI_RGB = 0;
#if never
        BS_PATTERN = 3,
#endif
        public const int BITSPIXEL = 12;
#if never
        BDR_RAISEDOUTER = 0x0001,
        BDR_SUNKENOUTER = 0x0002,
        BDR_RAISEDINNER = 0x0004,
        BDR_SUNKENINNER = 0x0008,
        BDR_RAISED = 0x0005,
        BDR_SUNKEN = 0x000a,
        BF_LEFT = 0x0001,
        BF_TOP = 0x0002,
        BF_RIGHT = 0x0004,
        BF_BOTTOM = 0x0008,
        BF_ADJUST = 0x2000,
        BF_FLAT = 0x4000,
        BF_MIDDLE = 0x0800,
        BFFM_INITIALIZED = 1,
        BFFM_SELCHANGED = 2,
        BFFM_SETSELECTION = 0x400+103,
        BFFM_ENABLEOK = 0x400+101,
        BS_PUSHBUTTON = 0x00000000,
        BS_DEFPUSHBUTTON = 0x00000001,
        BS_MULTILINE = 0x00002000,
        BS_PUSHLIKE = 0x00001000,
        BS_OWNERDRAW = 0x0000000B,
        BS_RADIOBUTTON = 0x00000004,
        BS_3STATE = 0x00000005,
        BS_GROUPBOX = 0x00000007,
        BS_LEFT = 0x00000100,
        BS_RIGHT = 0x00000200,
        BS_CENTER = 0x00000300,
        BS_TOP = 0x00000400,
        BS_BOTTOM = 0x00000800,
        BS_VCENTER = 0x00000C00,
        BS_RIGHTBUTTON = 0x00000020,
        BN_CLICKED = 0,
        BM_SETCHECK = 0x00F1,
        BM_SETSTATE = 0x00F3,
        BM_CLICK    = 0x00F5;

        public const int CDERR_DIALOGFAILURE = 0xFFFF,
        CDERR_STRUCTSIZE = 0x0001,
        CDERR_INITIALIZATION = 0x0002,
        CDERR_NOTEMPLATE = 0x0003,
        CDERR_NOHINSTANCE = 0x0004,
        CDERR_LOADSTRFAILURE = 0x0005,
        CDERR_FINDRESFAILURE = 0x0006,
        CDERR_LOADRESFAILURE = 0x0007,
        CDERR_LOCKRESFAILURE = 0x0008,
        CDERR_MEMALLOCFAILURE = 0x0009,
        CDERR_MEMLOCKFAILURE = 0x000A,
        CDERR_NOHOOK = 0x000B,
        CDERR_REGISTERMSGFAIL = 0x000C,
        CFERR_NOFONTS = 0x2001,
        CFERR_MAXLESSTHANMIN = 0x2002,
        CC_RGBINIT = 0x00000001,
        CC_FULLOPEN = 0x00000002,
        CC_PREVENTFULLOPEN = 0x00000004,
        CC_SHOWHELP = 0x00000008,
        CC_ENABLEHOOK = 0x00000010,
        CC_SOLIDCOLOR = 0x00000080,
        CC_ANYCOLOR = 0x00000100,
        CF_SCREENFONTS = 0x00000001,
        CF_SHOWHELP = 0x00000004,
        CF_ENABLEHOOK = 0x00000008,
        CF_INITTOLOGFONTSTRUCT = 0x00000040,
        CF_EFFECTS = 0x00000100,
        CF_APPLY = 0x00000200,
        CF_SCRIPTSONLY = 0x00000400,
        CF_NOVECTORFONTS = 0x00000800,
        CF_NOSIMULATIONS = 0x00001000,
        CF_LIMITSIZE = 0x00002000,
        CF_FIXEDPITCHONLY = 0x00004000,
        CF_FORCEFONTEXIST = 0x00010000,
        CF_TTONLY = 0x00040000,
        CF_SELECTSCRIPT = 0x00400000,
        CF_NOVERTFONTS = 0x01000000,
        CP_WINANSI = 1004;
#endif
        public const int
        cmb4 = 0x0473,
        CS_DBLCLKS = 0x0008,
        CS_DROPSHADOW = 0x00020000,
        CS_SAVEBITS = 0x0800,
        CF_TEXT = 1,
        CF_BITMAP = 2,
        CF_METAFILEPICT = 3,
        CF_SYLK = 4,
        CF_DIF = 5,
        CF_TIFF = 6,
        CF_OEMTEXT = 7,
        CF_DIB = 8,
        CF_PALETTE = 9,
        CF_PENDATA = 10,
        CF_RIFF = 11,
        CF_WAVE = 12,
        CF_UNICODETEXT = 13,
        CF_ENHMETAFILE = 14,
        CF_HDROP = 15,
        CF_LOCALE = 16,
        CLSCTX_INPROC_SERVER    = 0x1,
        CLSCTX_LOCAL_SERVER     = 0x4,
        CW_USEDEFAULT = (unchecked((int)0x80000000)),
        CWP_SKIPINVISIBLE = 0x0001,
        COLOR_WINDOW = 5,
        CB_ERR = (-1),
        CBN_SELCHANGE = 1,
        CBN_DBLCLK = 2,
        CBN_EDITCHANGE = 5,
        CBN_EDITUPDATE = 6,
        CBN_DROPDOWN = 7,
        CBN_CLOSEUP  = 8,
        CBN_SELENDOK = 9,
        CBS_SIMPLE = 0x0001,
        CBS_DROPDOWN = 0x0002,
        CBS_DROPDOWNLIST = 0x0003,
        CBS_OWNERDRAWFIXED = 0x0010,
        CBS_OWNERDRAWVARIABLE = 0x0020,
        CBS_AUTOHSCROLL = 0x0040,
        CBS_HASSTRINGS = 0x0200,
        CBS_NOINTEGRALHEIGHT = 0x0400,
        CB_GETEDITSEL = 0x0140,
        CB_LIMITTEXT = 0x0141,
        CB_SETEDITSEL = 0x0142,
        CB_ADDSTRING = 0x0143,
        CB_DELETESTRING = 0x0144,
        CB_GETCURSEL = 0x0147,
        CB_GETLBTEXT = 0x0148,
        CB_GETLBTEXTLEN = 0x0149,
        CB_INSERTSTRING = 0x014A,
        CB_RESETCONTENT = 0x014B,
        CB_FINDSTRING = 0x014C,
        CB_SETCURSEL = 0x014E,
        CB_SHOWDROPDOWN = 0x014F,
        CB_GETITEMDATA = 0x0150,
        CB_SETITEMHEIGHT = 0x0153,
        CB_GETITEMHEIGHT = 0x0154,
        CB_GETDROPPEDSTATE = 0x0157,
        CB_FINDSTRINGEXACT = 0x0158,
        CB_SETDROPPEDWIDTH = 0x0160,
        CDRF_DODEFAULT = 0x00000000,
        CDRF_NEWFONT = 0x00000002,
        CDRF_SKIPDEFAULT = 0x00000004,
        CDRF_NOTIFYPOSTPAINT = 0x00000010,
        CDRF_NOTIFYITEMDRAW = 0x00000020,
        CDRF_NOTIFYSUBITEMDRAW = CDRF_NOTIFYITEMDRAW,
        CDDS_PREPAINT = 0x00000001,
        CDDS_POSTPAINT = 0x00000002,
        CDDS_ITEM = 0x00010000,
        CDDS_SUBITEM = 0x00020000,
        CDDS_ITEMPREPAINT = (0x00010000|0x00000001),
        CDDS_ITEMPOSTPAINT = (0x00010000|0x00000002),
        CDIS_SELECTED = 0x0001,
        CDIS_GRAYED = 0x0002,
        CDIS_DISABLED = 0x0004,
        CDIS_CHECKED = 0x0008,
        CDIS_FOCUS = 0x0010,
        CDIS_DEFAULT = 0x0020,
        CDIS_HOT = 0x0040,
        CDIS_MARKED = 0x0080,
        CDIS_INDETERMINATE = 0x0100,
        CDIS_SHOWKEYBOARDCUES = 0x0200,
        CLR_NONE = unchecked((int)0xFFFFFFFF),
        CLR_DEFAULT = unchecked((int)0xFF000000),
        CCM_SETVERSION = (0x2000+0x7),
        CCM_GETVERSION = (0x2000+0x8),
        CCS_NORESIZE = 0x00000004,
        CCS_NOPARENTALIGN = 0x00000008,
        CCS_NODIVIDER = 0x00000040,
        CBEM_INSERTITEMA = (0x0400+1),
        CBEM_GETITEMA = (0x0400+4),
        CBEM_SETITEMA = (0x0400+5),
        CBEM_INSERTITEMW = (0x0400+11),
        CBEM_SETITEMW = (0x0400+12),
        CBEM_GETITEMW = (0x0400+13),
        CBEN_ENDEDITA = ((0-800)-5),
        CBEN_ENDEDITW = ((0-800)-6),
        CONNECT_E_NOCONNECTION = unchecked((int)0x80040200),
        CONNECT_E_CANNOTCONNECT = unchecked((int)0x80040202),
        CTRLINFO_EATS_RETURN    = 1,
        CTRLINFO_EATS_ESCAPE    = 2,
        CSIDL_DESKTOP                    = 0x0000,        // <desktop>
        CSIDL_INTERNET                   = 0x0001,        // Internet Explorer (icon on desktop)
        CSIDL_PROGRAMS                   = 0x0002,        // Start Menu\Programs
        CSIDL_PERSONAL                   = 0x0005,        // My Documents
        CSIDL_FAVORITES                  = 0x0006,        // <user name>\Favorites
        CSIDL_STARTUP                    = 0x0007,        // Start Menu\Programs\Startup
        CSIDL_RECENT                     = 0x0008,        // <user name>\Recent
        CSIDL_SENDTO                     = 0x0009,        // <user name>\SendTo
        CSIDL_STARTMENU                  = 0x000b,        // <user name>\Start Menu
        CSIDL_DESKTOPDIRECTORY           = 0x0010,        // <user name>\Desktop
        CSIDL_TEMPLATES                  = 0x0015,
        CSIDL_APPDATA                    = 0x001a,        // <user name>\Application Data
        CSIDL_LOCAL_APPDATA              = 0x001c,        // <user name>\Local Settings\Applicaiton Data (non roaming)
        CSIDL_INTERNET_CACHE             = 0x0020,
        CSIDL_COOKIES                    = 0x0021,
        CSIDL_HISTORY                    = 0x0022,
        CSIDL_COMMON_APPDATA             = 0x0023,        // All Users\Application Data
        CSIDL_SYSTEM                     = 0x0025,        // GetSystemDirectory()
        CSIDL_PROGRAM_FILES              = 0x0026,        // C:\Program Files
        CSIDL_PROGRAM_FILES_COMMON       = 0x002b;        // C:\Program Files\Common

        public const int DUPLICATE = 0x06,
        DISPID_VALUE = 0,
        DISPID_UNKNOWN = (-1),
        DISPID_PROPERTYPUT = (-3),
        DISPATCH_METHOD = 0x1,
        DISPATCH_PROPERTYGET = 0x2,
        DISPATCH_PROPERTYPUT = 0x4,
        DISPATCH_PROPERTYPUTREF = 0x8,
        DV_E_DVASPECT = unchecked((int)0x8004006B),
        DEFAULT_GUI_FONT = 17,
        DIB_RGB_COLORS = 0,
        DRAGDROP_E_NOTREGISTERED = unchecked((int)0x80040100),
        DRAGDROP_E_ALREADYREGISTERED = unchecked((int)0x80040101),
        DUPLICATE_SAME_ACCESS = 0x00000002,
        DFC_CAPTION = 1,
        DFC_MENU = 2,
        DFC_SCROLL = 3,
        DFC_BUTTON = 4,
        DFCS_CAPTIONCLOSE = 0x0000,
        DFCS_CAPTIONMIN = 0x0001,
        DFCS_CAPTIONMAX = 0x0002,
        DFCS_CAPTIONRESTORE = 0x0003,
        DFCS_CAPTIONHELP = 0x0004,
        DFCS_MENUARROW = 0x0000,
        DFCS_MENUCHECK = 0x0001,
        DFCS_MENUBULLET = 0x0002,
        DFCS_SCROLLUP = 0x0000,
        DFCS_SCROLLDOWN = 0x0001,
        DFCS_SCROLLLEFT = 0x0002,
        DFCS_SCROLLRIGHT = 0x0003,
        DFCS_SCROLLCOMBOBOX = 0x0005,
        DFCS_BUTTONCHECK = 0x0000,
        DFCS_BUTTONRADIO = 0x0004,
        DFCS_BUTTON3STATE = 0x0008,
        DFCS_BUTTONPUSH = 0x0010,
        DFCS_INACTIVE = 0x0100,
        DFCS_PUSHED = 0x0200,
        DFCS_CHECKED = 0x0400,
        DFCS_FLAT = 0x4000,
        DT_LEFT = 0x00000000,
        DT_RIGHT = 0x00000002,
        DT_VCENTER = 0x00000004,
        DT_SINGLELINE = 0x00000020,
        DT_NOCLIP = 0x00000100,
        DT_CALCRECT = 0x00000400,
        DT_NOPREFIX = 0x00000800,
        DT_EDITCONTROL = 0x00002000,
        DT_EXPANDTABS  = 0x00000040,
        DT_END_ELLIPSIS = 0x00008000,
        DT_RTLREADING = 0x00020000,
        DT_WORDBREAK = 0x00000010,
        DCX_WINDOW = 0x00000001,
        DCX_CACHE = 0x00000002,
        DCX_LOCKWINDOWUPDATE = 0x00000400,
        DI_NORMAL = 0x0003,
        DLGC_WANTARROWS = 0x0001,
        DLGC_WANTTAB = 0x0002,
        DLGC_WANTALLKEYS = 0x0004,
        DLGC_WANTCHARS = 0x0080,
        DTM_GETSYSTEMTIME = (0x1000+1),
        DTM_SETSYSTEMTIME = (0x1000+2),
        DTM_SETRANGE = (0x1000+4),
        DTM_SETFORMATA = (0x1000+5),
        DTM_SETFORMATW = (0x1000+50),
        DTM_SETMCCOLOR = (0x1000+6),
        DTM_SETMCFONT = (0x1000+9),
        DTS_UPDOWN = 0x0001,
        DTS_SHOWNONE = 0x0002,
        DTS_LONGDATEFORMAT = 0x0004,
        DTS_TIMEFORMAT = 0x0009,
        DTS_RIGHTALIGN = 0x0020,
        DTN_DATETIMECHANGE = ((0-760)+1),
        DTN_USERSTRINGA = ((0-760)+2),
        DTN_USERSTRINGW = ((0-760)+15),
        DTN_WMKEYDOWNA = ((0-760)+3),
        DTN_WMKEYDOWNW = ((0-760)+16),
        DTN_FORMATA = ((0-760)+4),
        DTN_FORMATW = ((0-760)+17),
        DTN_FORMATQUERYA = ((0-760)+5),
        DTN_FORMATQUERYW = ((0-760)+18),
        DTN_DROPDOWN = ((0-760)+6),
        DTN_CLOSEUP = ((0-760)+7),
        DVASPECT_CONTENT   = 1,
        DVASPECT_TRANSPARENT = 32,
        DVASPECT_OPAQUE    = 16;

        public const int E_NOTIMPL = unchecked((int)0x80004001),
        E_OUTOFMEMORY = unchecked((int)0x8007000E),
        E_INVALIDARG = unchecked((int)0x80070057),
        E_NOINTERFACE = unchecked((int)0x80004002),
        E_FAIL = unchecked((int)0x80004005),
        E_ABORT = unchecked((int)0x80004004),
        E_UNEXPECTED = unchecked((int)0x8000FFFF),
        INET_E_DEFAULT_ACTION = unchecked((int)0x800C0011),
        ETO_OPAQUE = 0x0002,
        ETO_CLIPPED = 0x0004,
        EMR_POLYTEXTOUTA = 96,
        EMR_POLYTEXTOUTW = 97,
        EDGE_RAISED = (0x0001|0x0004),
        EDGE_SUNKEN = (0x0002|0x0008),
        EDGE_ETCHED = (0x0002|0x0004),
        EDGE_BUMP = (0x0001|0x0008),
        ES_LEFT = 0x0000,
        ES_CENTER = 0x0001,
        ES_RIGHT = 0x0002,
        ES_MULTILINE = 0x0004,
        ES_UPPERCASE = 0x0008,
        ES_LOWERCASE = 0x0010,
        ES_AUTOVSCROLL = 0x0040,
        ES_AUTOHSCROLL = 0x0080,
        ES_NOHIDESEL = 0x0100,
        ES_READONLY = 0x0800,
        ES_PASSWORD = 0x0020,
        EN_CHANGE = 0x0300,
        EN_UPDATE = 0x0400,
        EN_HSCROLL = 0x0601,
        EN_VSCROLL = 0x0602,
        EN_ALIGN_LTR_EC = 0x0700,
        EN_ALIGN_RTL_EC = 0x0701,
        EC_LEFTMARGIN = 0x0001,
        EC_RIGHTMARGIN = 0x0002,
        EM_GETSEL = 0x00B0,
        EM_SETSEL = 0x00B1,
        EM_SCROLL = 0x00B5,
        EM_SCROLLCARET = 0x00B7,
        EM_GETMODIFY = 0x00B8,
        EM_SETMODIFY = 0x00B9,
        EM_GETLINECOUNT = 0x00BA,
        EM_REPLACESEL = 0x00C2,
        EM_GETLINE = 0x00C4,
        EM_LIMITTEXT = 0x00C5,
        EM_CANUNDO = 0x00C6,
        EM_UNDO = 0x00C7,
        EM_SETPASSWORDCHAR = 0x00CC,
        EM_GETPASSWORDCHAR = 0x00D2,
        EM_EMPTYUNDOBUFFER = 0x00CD,
        EM_SETREADONLY = 0x00CF,
        EM_SETMARGINS = 0x00D3,
        EM_POSFROMCHAR = 0x00D6,
        EM_CHARFROMPOS = 0x00D7,
        EM_LINEFROMCHAR = 0x00C9,
        EM_LINEINDEX = 0x00BB;
#if never


        public const int ERROR_CLASS_ALREADY_EXISTS = 1410;
#endif

        public const int FNERR_SUBCLASSFAILURE = 0x3001,
        FNERR_INVALIDFILENAME = 0x3002,
        FNERR_BUFFERTOOSMALL = 0x3003;

#if never
        public const int FRERR_BUFFERLENGTHZERO = 0x4001,
        FADF_BSTR = (0x100),
        FADF_UNKNOWN = (0x200),
        FADF_DISPATCH = (0x400),
        FADF_VARIANT = (unchecked((int)0x800)),
        FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000,
        FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200,
        FVIRTKEY = 0x01,
        FSHIFT = 0x04,
        FALT = 0x10;
#endif

        public const int GMEM_MOVEABLE = 0x0002,
        GMEM_ZEROINIT = 0x0040,
        GMEM_DDESHARE = 0x2000,
        GCL_WNDPROC = (-24),
        GWL_WNDPROC = (-4),
        GWL_HWNDPARENT = (-8),
        GWL_STYLE = (-16),
        GWL_EXSTYLE = (-20),
        GWL_ID = (-12),
        GW_HWNDFIRST = 0,
        GW_HWNDLAST = 1,
        GW_HWNDNEXT = 2,
        GW_HWNDPREV = 3,
        GW_OWNER = 4,
        GW_CHILD = 5,
        GMR_VISIBLE = 0,
        GMR_DAYSTATE = 1,
        GDI_ERROR = (unchecked((int)0xFFFFFFFF)),
        GDTR_MIN = 0x0001,
        GDTR_MAX = 0x0002,
        GDT_VALID = 0,
        GDT_NONE = 1,
        GA_PARENT = 1,
        GA_ROOT   = 2;

        // ImmGetCompostionString index.
        public const int
        GCS_COMPREADSTR       = 0x0001,
        GCS_COMPREADATTR      = 0x0002,
        GCS_COMPREADCLAUSE    = 0x0004,
        GCS_COMPSTR           = 0x0008,
        GCS_COMPATTR          = 0x0010,
        GCS_COMPCLAUSE        = 0x0020,
        GCS_CURSORPOS         = 0x0080,
        GCS_DELTASTART        = 0x0100,
        GCS_RESULTREADSTR     = 0x0200,
        GCS_RESULTREADCLAUSE  = 0x0400,
        GCS_RESULTSTR         = 0x0800,
        GCS_RESULTCLAUSE      = 0x1000,

        // attribute for COMPOSITIONSTRING Structure
        ATTR_INPUT               = 0x00,
        ATTR_TARGET_CONVERTED    = 0x01,
        ATTR_CONVERTED           = 0x02,
        ATTR_TARGET_NOTCONVERTED = 0x03,
        ATTR_INPUT_ERROR         = 0x04,
        ATTR_FIXEDCONVERTED      = 0x05,

        // dwAction for ImmNotifyIME
        NI_COMPOSITIONSTR = 0x0015,

        // wParam of report message WM_IME_NOTIFY
        IMN_CLOSESTATUSWINDOW           = 0x0001,
        IMN_OPENSTATUSWINDOW            = 0x0002,
        IMN_CHANGECANDIDATE             = 0x0003,
        IMN_CLOSECANDIDATE              = 0x0004,
        IMN_OPENCANDIDATE               = 0x0005,
        IMN_SETCONVERSIONMODE           = 0x0006,
        IMN_SETSENTENCEMODE             = 0x0007,
        IMN_SETOPENSTATUS               = 0x0008,
        IMN_SETCANDIDATEPOS             = 0x0009,
        IMN_SETCOMPOSITIONFONT          = 0x000A,
        IMN_SETCOMPOSITIONWINDOW        = 0x000B,
        IMN_SETSTATUSWINDOWPOS          = 0x000C,
        IMN_GUIDELINE                   = 0x000D,
        IMN_PRIVATE                     = 0x000E,

        // dwIndex for ImmNotifyIME/NI_COMPOSITIONSTR
        CPS_COMPLETE = 0x01,
        CPS_CANCEL   = 0x04,

        // dwStyle for CANDIDATEFORM
        CFS_DEFAULT                     = 0x0000,
        CFS_RECT                        = 0x0001,
        CFS_POINT                       = 0x0002,
        CFS_FORCE_POSITION              = 0x0020,
        CFS_CANDIDATEPOS                = 0x0040,
        CFS_EXCLUDE                     = 0x0080,

        // bit field for conversion mode
        IME_CMODE_ALPHANUMERIC          = 0x0000,
        IME_CMODE_NATIVE                = 0x0001,
        IME_CMODE_CHINESE               = 0x0001,  // IME_CMODE_NATIVE,
        IME_CMODE_HANGEUL               = 0x0001,  // IME_CMODE_NATIVE,
        IME_CMODE_HANGUL                = 0x0001,  // IME_CMODE_NATIVE,
        IME_CMODE_JAPANESE              = 0x0001,  // IME_CMODE_NATIVE,
        IME_CMODE_KATAKANA              = 0x0002,  // only effect under IME_CMODE_NATIVE
        IME_CMODE_LANGUAGE              = 0x0003,
        IME_CMODE_FULLSHAPE             = 0x0008,
        IME_CMODE_ROMAN                 = 0x0010,
        IME_CMODE_CHARCODE              = 0x0020,
        IME_CMODE_HANJACONVERT          = 0x0040,
        IME_CMODE_SOFTKBD               = 0x0080,
        IME_CMODE_NOCONVERSION          = 0x0100,
        IME_CMODE_EUDC                  = 0x0200,
        IME_CMODE_SYMBOL                = 0x0400,
        IME_CMODE_FIXED                 = 0x0800,
        IME_CMODE_RESERVED          = unchecked((int)0xF0000000),

        // bit field for sentence mode
        IME_SMODE_NONE                  = 0x0000,
        IME_SMODE_PLAURALCLAUSE         = 0x0001,
        IME_SMODE_SINGLECONVERT         = 0x0002,
        IME_SMODE_AUTOMATIC             = 0x0004,
        IME_SMODE_PHRASEPREDICT         = 0x0008,
        IME_SMODE_CONVERSATION          = 0x0010,
        IME_SMODE_RESERVED          = 0x0000F000,

        IME_CAND_UNKNOWN                = 0x0000,
        IME_CAND_READ                   = 0x0001,
        IME_CAND_CODE                   = 0x0002,
        IME_CAND_MEANING                = 0x0003,
        IME_CAND_RADICAL                = 0x0004,
        IME_CAND_STROKE                 = 0x0005,

        IMR_COMPOSITIONWINDOW           = 0x0001,
        IMR_CANDIDATEWINDOW             = 0x0002,
        IMR_COMPOSITIONFONT             = 0x0003,
        IMR_RECONVERTSTRING             = 0x0004,
        IMR_CONFIRMRECONVERTSTRING      = 0x0005,
        IMR_QUERYCHARPOSITION           = 0x0006,
        IMR_DOCUMENTFEED                = 0x0007,

        IME_CONFIG_GENERAL              = 1,
        IME_CONFIG_REGISTERWORD         = 2,
        IME_CONFIG_SELECTDICTIONARY     = 3,

        IGP_GETIMEVERSION               = (-4),
        IGP_PROPERTY                    = 0x00000004,
        IGP_CONVERSION                  = 0x00000008,
        IGP_SENTENCE                    = 0x0000000c,
        IGP_UI                          = 0x00000010,
        IGP_SETCOMPSTR                  = 0x00000014,
        IGP_SELECT                      = 0x00000018,

        IME_PROP_AT_CARET               = 0x00010000,
        IME_PROP_SPECIAL_UI             = 0x00020000,
        IME_PROP_CANDLIST_START_FROM_1  = 0x00040000,
        IME_PROP_UNICODE                = 0x00080000,
        IME_PROP_COMPLETE_ON_UNSELECT   = 0x00100000;

        // CANDIDATEFORM structures
        [StructLayout(LayoutKind.Sequential)]
        public struct CANDIDATEFORM
        {
            public int    dwIndex;
            public int    dwStyle;
            public POINT  ptCurrentPos;
            public RECT   rcArea;
        }

        // COMPOSITIONFORM structures
        [StructLayout(LayoutKind.Sequential)]
        public struct COMPOSITIONFORM
        {
            public int    dwStyle;
            public POINT  ptCurrentPos;
            public RECT   rcArea;
        }

        // RECONVERTSTRING structures
        [StructLayout(LayoutKind.Sequential)]
        public struct RECONVERTSTRING
        {
            public int dwSize;
            public int dwVersion;
            public int dwStrLen;
            public int dwStrOffset;
            public int dwCompStrLen;
            public int dwCompStrOffset;
            public int dwTargetStrLen;
            public int dwTargetStrOffset;
        }

        // REGISTERWORD structures
        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public struct REGISTERWORD
        {
            public string lpReading;
            public string lpWord;
        }

        public const int
        HC_ACTION = 0,
        HC_GETNEXT = 1,
        HC_SKIP = 2,
        HTNOWHERE = 0,
        HTCLIENT = 1,
        HTBOTTOM = 15,
        HTTRANSPARENT = (-1),
        HTBOTTOMLEFT = 16,
        HTBOTTOMRIGHT = 17,
        HELPINFO_WINDOW = 0x0001,
        HCF_HIGHCONTRASTON = 0x00000001,
        HDI_ORDER = 0x0080,
        HDI_WIDTH = 0x0001,
        HDM_GETITEMCOUNT = (0x1200+0),
        HDM_INSERTITEMA = (0x1200+1),
        HDM_INSERTITEMW = (0x1200+10),
        HDM_GETITEMA = (0x1200+3),
        HDM_GETITEMW = (0x1200+11),
        HDM_SETITEMA = (0x1200+4),
        HDM_SETITEMW = (0x1200+12),
        HDN_ITEMCHANGINGA = ((0-300)-0),
        HDN_ITEMCHANGINGW = ((0-300)-20),
        HDN_ITEMCHANGEDA = ((0-300)-1),
        HDN_ITEMCHANGEDW = ((0-300)-21),
        HDN_ITEMCLICKA = ((0-300)-2),
        HDN_ITEMCLICKW = ((0-300)-22),
        HDN_ITEMDBLCLICKA = ((0-300)-3),
        HDN_ITEMDBLCLICKW = ((0-300)-23),
        HDN_DIVIDERDBLCLICKA = ((0-300)-5),
        HDN_DIVIDERDBLCLICKW = ((0-300)-25),
        HDN_BEGINTDRAG = ((0-300)-10),
        HDN_BEGINTRACKA = ((0-300)-6),
        HDN_BEGINTRACKW = ((0-300)-26),
        HDN_ENDDRAG = ((0-300)-11),
        HDN_ENDTRACKA = ((0-300)-7),
        HDN_ENDTRACKW = ((0-300)-27),
        HDN_TRACKA = ((0-300)-8),
        HDN_TRACKW = ((0-300)-28),
        HDN_GETDISPINFOA = ((0-300)-9),
        HDN_GETDISPINFOW = ((0-300)-29);
        // HOVER_DEFAULT = Do not use this value ever! It crashes entire servers.
#if never

        public const int HDS_FULLDRAG = 0x0080;

        // Corresponds to bitmaps in MENUITEMINFO
        public const int HBMMENU_CALLBACK = -1,
        HBMMENU_SYSTEM = 1,
        HBMMENU_MBAR_RESTORE = 2,
        HBMMENU_MBAR_MINIMIZE = 3,
        HBMMENU_MBAR_CLOSE = 5,
        HBMMENU_MBAR_CLOSE_D = 6,
        HBMMENU_MBAR_MINIMIZE_D = 7,
        HBMMENU_POPUP_CLOSE = 8,
        HBMMENU_POPUP_RESTORE = 9,
        HBMMENU_POPUP_MAXIMIZE = 10,
        HBMMENU_POPUP_MINIMIZE = 11;
#endif

        public static HandleRef HWND_TOP = new HandleRef(null, (IntPtr)0);
        public static HandleRef HWND_BOTTOM = new HandleRef(null, (IntPtr)1);
        public static HandleRef HWND_TOPMOST = new HandleRef(null, new IntPtr(-1));
        public static HandleRef HWND_NOTOPMOST = new HandleRef(null, new IntPtr(-2));
        // NOTE:  NativeMethodsOther.cs defines the following
        //public static IntPtr HWND_MESSAGE = new IntPtr(-3);


        public const int INPLACE_E_NOTOOLSPACE = unchecked((int)0x800401A1),
        ICON_SMALL = 0,
        ICON_BIG = 1,
        IDC_ARROW = 32512,
        IDC_IBEAM = 32513,
        IDC_WAIT = 32514,
        IDC_CROSS = 32515,
        IDC_SIZEALL = 32646,
        IDC_SIZENWSE = 32642,
        IDC_SIZENESW = 32643,
        IDC_SIZEWE = 32644,
        IDC_SIZENS = 32645,
        IDC_UPARROW = 32516,
        IDC_NO = 32648,
        IDC_APPSTARTING = 32650,
        IDC_HELP = 32651,
        IMAGE_ICON = 1,
        IMAGE_CURSOR = 2,
        ICC_LISTVIEW_CLASSES = 0x00000001,
        ICC_TREEVIEW_CLASSES = 0x00000002,
        ICC_BAR_CLASSES = 0x00000004,
        ICC_TAB_CLASSES = 0x00000008,
        ICC_PROGRESS_CLASS = 0x00000020,
        ICC_DATE_CLASSES = 0x00000100,
        ILC_MASK = 0x0001,
        ILC_COLOR = 0x0000,
        ILC_COLOR4 = 0x0004,
        ILC_COLOR8 = 0x0008,
        ILC_COLOR16 = 0x0010,
        ILC_COLOR24 = 0x0018,
        ILC_COLOR32 = 0x0020,
        ILC_MIRROR = 0x00002000,
        ILD_NORMAL = 0x0000,
        ILD_TRANSPARENT = 0x0001,
        ILD_MASK = 0x0010,
        ILD_ROP = 0x0040,

        // ImageList
        //
        ILP_NORMAL = 0,
        ILP_DOWNLEVEL = 1,
        ILS_NORMAL = 0x0,
        ILS_GLOW = 0x1,
        ILS_SHADOW = 0x2,
        ILS_SATURATE = 0x4,
        ILS_ALPHA = 0x8;

        public const int CSC_NAVIGATEFORWARD = 0x00000001,
        CSC_NAVIGATEBACK = 0x00000002;

#if never

        public const int IDM_PRINT = 27,
        IDM_PAGESETUP = 2004,
        IDM_PRINTPREVIEW = 2003,
        IDM_PROPERTIES = 28,
        IDM_SAVEAS = 71;

        

        public const int STG_E_INVALIDFUNCTION = unchecked((int)0x80030001);
        public const int STG_E_FILENOTFOUND = unchecked((int)0x80030002);
        public const int STG_E_PATHNOTFOUND = unchecked((int)0x80030003);
        public const int STG_E_TOOMANYOPENFILES = unchecked((int)0x80030004);
        public const int STG_E_ACCESSDENIED = unchecked((int)0x80030005);
        public const int STG_E_INVALIDHANDLE = unchecked((int)0x80030006);
        public const int STG_E_INSUFFICIENTMEMORY = unchecked((int)0x80030008);
        public const int STG_E_INVALIDPOINTER = unchecked((int)0x80030009);
        public const int STG_E_NOMOREFILES = unchecked((int)0x80030012);
        public const int STG_E_DISKISWRITEPROTECTED = unchecked((int)0x80030013);
        public const int STG_E_SEEKERROR = unchecked((int)0x80030019);
        public const int STG_E_WRITEFAULT = unchecked((int)0x8003001D);
        public const int STG_E_READFAULT = unchecked((int)0x8003001E);
        public const int STG_E_SHAREVIOLATION = unchecked((int)0x80030020);
        public const int STG_E_LOCKVIOLATION = unchecked((int)0x80030021);
#endif
        public const int STG_E_CANTSAVE = unchecked((int)0x80030103);
#if never

        public const int KEYEVENTF_KEYUP = 0x0002;
#endif

        public const int LOGPIXELSX = 88,
        LOGPIXELSY = 90,
        LB_ERR = (-1),
        LB_ERRSPACE = (-2),
        LBN_SELCHANGE = 1,
        LBN_DBLCLK = 2,
        LB_ADDSTRING = 0x0180,
        LB_INSERTSTRING = 0x0181,
        LB_DELETESTRING = 0x0182,
        LB_RESETCONTENT = 0x0184,
        LB_SETSEL = 0x0185,
        LB_SETCURSEL = 0x0186,
        LB_GETSEL = 0x0187,
        LB_GETCARETINDEX = 0x019F,
        LB_GETCURSEL = 0x0188,
        LB_GETTEXT = 0x0189,
        LB_GETTEXTLEN = 0x018A,
        LB_GETTOPINDEX = 0x018E,
        LB_FINDSTRING = 0x018F,
        LB_GETSELCOUNT = 0x0190,
        LB_GETSELITEMS = 0x0191,
        LB_SETTABSTOPS = 0x0192,
        LB_SETHORIZONTALEXTENT = 0x0194,
        LB_SETCOLUMNWIDTH = 0x0195,
        LB_SETTOPINDEX = 0x0197,
        LB_GETITEMRECT = 0x0198,
        LB_SETITEMHEIGHT = 0x01A0,
        LB_GETITEMHEIGHT = 0x01A1,
        LB_FINDSTRINGEXACT = 0x01A2,
        LB_ITEMFROMPOINT = 0x01A9,
        LB_SETLOCALE = 0x01A5;
#if never

        public const int LBS_NOTIFY = 0x0001,
        LBS_MULTIPLESEL = 0x0008,
        LBS_OWNERDRAWFIXED = 0x0010,
        LBS_OWNERDRAWVARIABLE = 0x0020,
        LBS_HASSTRINGS = 0x0040,
        LBS_USETABSTOPS = 0x0080,
        LBS_NOINTEGRALHEIGHT = 0x0100,
        LBS_MULTICOLUMN = 0x0200,
        LBS_WANTKEYBOARDINPUT = 0x0400,
        LBS_EXTENDEDSEL = 0x0800,
        LBS_DISABLENOSCROLL = 0x1000,
        LBS_NOSEL = 0x4000,
        LOCK_WRITE = 0x1,
        LOCK_EXCLUSIVE = 0x2,
        LOCK_ONLYONCE = 0x4,
        LV_VIEW_TILE = 0x0004,
        LVBKIF_SOURCE_NONE = 0x0000,
        LVBKIF_SOURCE_URL = 0x0002,
        LVBKIF_STYLE_NORMAL = 0x0000,
        LVBKIF_STYLE_TILE = 0x0010,
        LVS_ICON = 0x0000,
        LVS_REPORT = 0x0001,
        LVS_SMALLICON = 0x0002,
        LVS_LIST = 0x0003,
        LVS_SINGLESEL = 0x0004,
        LVS_SHOWSELALWAYS = 0x0008,
        LVS_SORTASCENDING = 0x0010,
        LVS_SORTDESCENDING = 0x0020,
        LVS_SHAREIMAGELISTS = 0x0040,
        LVS_NOLABELWRAP = 0x0080,
        LVS_AUTOARRANGE = 0x0100,
        LVS_EDITLABELS = 0x0200,
        LVS_NOSCROLL = 0x2000,
        LVS_ALIGNTOP = 0x0000,
        LVS_ALIGNLEFT = 0x0800,
        LVS_NOCOLUMNHEADER = 0x4000,
        LVS_NOSORTHEADER = unchecked((int)0x8000),
        LVS_OWNERDATA = 0x1000,
        LVSCW_AUTOSIZE = -1,
        LVSCW_AUTOSIZE_USEHEADER = -2,
        LVM_REDRAWITEMS = (0x1000+21),
        LVM_SCROLL=(0x1000+20),
        LVM_SETBKCOLOR = (0x1000+1),
        LVM_SETBKIMAGEA = (0x1000+68),
        LVM_SETBKIMAGEW = (0x1000+138),
        LVM_SETCALLBACKMASK = (0x1000+11),
        LVM_GETCALLBACKMASK = (0x1000+10),
        LVM_SETINFOTIP = (0x1000+173),
        LVSIL_NORMAL = 0,
        LVSIL_SMALL = 1,
        LVSIL_STATE = 2,
        LVM_SETIMAGELIST = (0x1000+3),
        LVM_SETTOOLTIPS  = (0x1000+74),
        LVIF_TEXT = 0x0001,
        LVIF_IMAGE = 0x0002,
        LVIF_INDENT = 0x0010,
        LVIF_PARAM = 0x0004,
        LVIF_STATE = 0x0008,
        LVIF_GROUPID = 0x0100,
        LVIF_COLUMNS = 0x0200,
        LVIS_FOCUSED = 0x0001,
        LVIS_SELECTED = 0x0002,
        LVIS_CUT = 0x0004,
        LVIS_DROPHILITED = 0x0008,
        LVIS_OVERLAYMASK = 0x0F00,
        LVIS_STATEIMAGEMASK = 0xF000,
        LVM_GETITEMA = (0x1000+5),
        LVM_GETITEMW = (0x1000+75),
        LVM_SETITEMA = (0x1000+6),
        LVM_SETITEMW = (0x1000+76),
        LVM_SETITEMPOSITION32 = (0x01000 + 49),
        LVM_INSERTITEMA = (0x1000+7),
        LVM_INSERTITEMW = (0x1000+77),
        LVM_DELETEITEM = (0x1000+8),
        LVM_DELETECOLUMN = (0x1000+28),
        LVM_DELETEALLITEMS = (0x1000+9),
        LVM_UPDATE = (0x1000+42),
        LVNI_FOCUSED = 0x0001,
        LVNI_SELECTED = 0x0002,
        LVM_GETNEXTITEM = (0x1000+12),
        LVFI_PARAM = 0x0001,
        LVFI_NEARESTXY = 0x0040,
        LVFI_PARTIAL = 0x0008,
        LVFI_STRING = 0x0002,
        LVM_FINDITEMA = (0x1000+13),
        LVM_FINDITEMW = (0x1000+83),
        LVIR_BOUNDS = 0,
        LVIR_ICON = 1,
        LVIR_LABEL = 2,
        LVIR_SELECTBOUNDS = 3,
        LVM_GETITEMPOSITION = (0x1000+16),
        LVM_GETITEMRECT = (0x1000+14),
        LVM_GETSUBITEMRECT = (0x1000+56),
        LVM_GETSTRINGWIDTHA = (0x1000+17),
        LVM_GETSTRINGWIDTHW = (0x1000+87),
        LVHT_NOWHERE = 0x0001,
        LVHT_ONITEMICON = 0x0002,
        LVHT_ONITEMLABEL = 0x0004,
        LVHT_ABOVE = 0x0008,
        LVHT_BELOW = 0x0010,
        LVHT_RIGHT = 0x0020,
        LVHT_LEFT = 0x0040,
        LVHT_ONITEM = (0x0002|0x0004|0x0008),
        LVHT_ONITEMSTATEICON = 0x0008,
        LVM_SUBITEMHITTEST = (0x1000 + 57),
        LVM_HITTEST = (0x1000+18),
        LVM_ENSUREVISIBLE = (0x1000+19),
        LVA_DEFAULT = 0x0000,
        LVA_ALIGNLEFT = 0x0001,
        LVA_ALIGNTOP = 0x0002,
        LVA_SNAPTOGRID = 0x0005,
        LVM_ARRANGE = (0x1000+22),
        LVM_EDITLABELA = (0x1000+23),
        LVM_EDITLABELW = (0x1000+118),
        LVCDI_ITEM = 0x0000,
        LVCF_FMT = 0x0001,
        LVCF_WIDTH = 0x0002,
        LVCF_TEXT = 0x0004,
        LVCF_SUBITEM = 0x0008,
        LVCF_IMAGE = 0x0010,
        LVCF_ORDER = 0x0020,
        LVCFMT_IMAGE = 0x0800,
        LVGA_HEADER_LEFT  =  0x00000001,
        LVGA_HEADER_CENTER = 0x00000002,
        LVGA_HEADER_RIGHT  = 0x00000004,
        LVGA_FOOTER_LEFT   = 0x00000008,
        LVGA_FOOTER_CENTER = 0x00000010,
        LVGA_FOOTER_RIGHT  = 0x00000020,
        LVGF_NONE    =       0x00000000,
        LVGF_HEADER   =      0x00000001,
        LVGF_FOOTER    =     0x00000002,
        LVGF_STATE      =    0x00000004,
        LVGF_ALIGN       =   0x00000008,
        LVGF_GROUPID    =    0x00000010,
        LVGS_NORMAL    = 0x00000000,
        LVGS_COLLAPSED  =    0x00000001,
        LVGS_HIDDEN    =     0x00000002,
        LVIM_AFTER = 0x00000001,
        LVTVIF_FIXEDSIZE = 0x00000003,
        LVTVIM_TILESIZE = 0x00000001,
        LVTVIM_COLUMNS = 0x00000002,
        LVM_ENABLEGROUPVIEW = (0x1000 + 157),
        LVM_MOVEITEMTOGROUP     =       (0x1000 + 154),
        LVM_GETCOLUMNA = (0x1000+25),
        LVM_GETCOLUMNW = (0x1000+95),
        LVM_SETCOLUMNA = (0x1000+26),
        LVM_SETCOLUMNW = (0x1000+96),
        LVM_INSERTCOLUMNA = (0x1000+27),
        LVM_INSERTCOLUMNW = (0x1000+97),
        LVM_INSERTGROUP = (0x1000 + 145),
        LVM_REMOVEGROUP = (0x1000 + 150),
        LVM_INSERTMARKHITTEST = (0x1000 + 168),
        LVM_REMOVEALLGROUPS = (0x1000 + 160),
        LVM_GETCOLUMNWIDTH = (0x1000+29),
        LVM_SETCOLUMNWIDTH = (0x1000+30),
        LVM_SETINSERTMARK = (0x1000 + 166),
        LVM_GETHEADER = (0x1000+31),
        LVM_SETTEXTCOLOR = (0x1000+36),
        LVM_SETTEXTBKCOLOR = (0x1000+38),
        LVM_GETTOPINDEX = (0x1000+39),
        LVM_SETITEMPOSITION = (0x1000+15),
        LVM_SETITEMSTATE = (0x1000+43),
        LVM_GETITEMSTATE = (0x1000+44),
        LVM_GETITEMTEXTA = (0x1000+45),
        LVM_GETITEMTEXTW = (0x1000+115),
        LVM_GETHOTITEM = (0x1000+61),
        LVM_SETITEMTEXTA = (0x1000+46),
        LVM_SETITEMTEXTW = (0x1000+116),
        LVM_SETITEMCOUNT = (0x1000+47),
        LVM_SORTITEMS = (0x1000+48),
        LVM_GETSELECTEDCOUNT = (0x1000+50),
        LVM_GETISEARCHSTRINGA = (0x1000+52),
        LVM_GETISEARCHSTRINGW = (0x1000+117),
        LVM_SETEXTENDEDLISTVIEWSTYLE = (0x1000+54),
        LVM_SETVIEW = (0x1000 + 142),
        LVM_GETGROUPINFO      =   (0x1000 + 149),
        LVM_SETGROUPINFO  =       (0x1000 + 147),
        LVM_HASGROUP = (0x1000 + 161),
        LVM_SETTILEVIEWINFO = (0x1000 + 162),
        LVM_GETTILEVIEWINFO = (0x1000 + 163),
        LVM_GETINSERTMARK = (0x1000 + 167),
        LVM_GETINSERTMARKRECT = (0x1000 + 169),
        LVM_SETINSERTMARKCOLOR = (0x1000 + 170),
        LVM_GETINSERTMARKCOLOR = (0x1000 + 171),
        LVM_ISGROUPVIEWENABLED = (0x1000 + 175),
        LVS_EX_GRIDLINES = 0x00000001,
        LVS_EX_CHECKBOXES = 0x00000004,
        LVS_EX_TRACKSELECT = 0x00000008,
        LVS_EX_HEADERDRAGDROP = 0x00000010,
        LVS_EX_FULLROWSELECT = 0x00000020,
        LVS_EX_ONECLICKACTIVATE = 0x00000040,
        LVS_EX_TWOCLICKACTIVATE = 0x00000080,
        LVS_EX_INFOTIP = 0x00000400,
        LVS_EX_UNDERLINEHOT = 0x00000800,
        LVS_EX_DOUBLEBUFFER = 0x00010000,
        LVN_ITEMCHANGING = ((0-100)-0),
        LVN_ITEMCHANGED = ((0-100)-1),
        LVN_BEGINLABELEDITA = ((0-100)-5),
        LVN_BEGINLABELEDITW = ((0-100)-75),
        LVN_ENDLABELEDITA = ((0-100)-6),
        LVN_ENDLABELEDITW = ((0-100)-76),
        LVN_COLUMNCLICK = ((0-100)-8),
        LVN_BEGINDRAG = ((0-100)-9),
        LVN_BEGINRDRAG = ((0-100)-11),
        LVN_ODFINDITEMA = ((0-100)-52),
        LVN_ODFINDITEMW = ((0-100)-79),
        LVN_ITEMACTIVATE = ((0-100)-14),
        LVN_GETDISPINFOA = ((0-100)-50),
        LVN_GETDISPINFOW = ((0-100)-77),
        LVN_ODCACHEHINT = ((0-100) - 13),
        LVN_ODSTATECHANGED = ((0-100) - 15),
        LVN_SETDISPINFOA = ((0-100)-51),
        LVN_SETDISPINFOW = ((0-100)-78),
        LVN_GETINFOTIPA  = ((0-100)-57),
        LVN_GETINFOTIPW  = ((0-100)- 58),
        LVN_KEYDOWN = ((0-100)-55),

        LWA_COLORKEY            = 0x00000001,
#endif
        public const int LWA_ALPHA = 0x00000002;
#if never

        public const int LANG_NEUTRAL = 0x00,
                         LOCALE_IFIRSTDAYOFWEEK = 0x0000100C;   /* first day of week specifier */

        public const int LOCALE_IMEASURE =              0x0000000D;   // 0 = metric, 1 = US

        public static readonly int LOCALE_USER_DEFAULT = MAKELCID(LANG_USER_DEFAULT);
        public static readonly int LANG_USER_DEFAULT   = MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT);

        public static int MAKELANGID(int primary, int sub) {
            return ((((ushort)(sub)) << 10) | (ushort)(primary));
        }

        /// <include file='doc\NativeMethods.uex' path='docs/doc[@for="NativeMethods.Lang.MAKELCID"]/*' />
        /// <devdoc>
        ///     Creates an LCID from a LangId
        /// </devdoc>
        public static int MAKELCID(int lgid) {
            return MAKELCID(lgid, SORT_DEFAULT);
        }

        /// <include file='doc\NativeMethods.uex' path='docs/doc[@for="NativeMethods.Lang.MAKELCID1"]/*' />
        /// <devdoc>
        ///     Creates an LCID from a LangId
        /// </devdoc>
        public static int MAKELCID(int lgid, int sort) {
            return ((0xFFFF & lgid) | (((0x000f) & sort) << 16));
        }

#endif

        public const int MEMBERID_NIL = (-1),
        MAX_PATH = 260,
        MA_ACTIVATE = 0x0001,
        MA_ACTIVATEANDEAT = 0x0002,
        MA_NOACTIVATE = 0x0003,
        MA_NOACTIVATEANDEAT = 0x0004,
        MM_TEXT = 1,
        MM_ANISOTROPIC = 8,
        MK_LBUTTON = 0x0001,
        MK_RBUTTON = 0x0002,
        MK_SHIFT = 0x0004,
        MK_CONTROL = 0x0008,
        MK_MBUTTON = 0x0010,
        MNC_EXECUTE = 2,
        MNC_SELECT = 3,
        MIIM_STATE = 0x00000001,
        MIIM_ID = 0x00000002,
        MIIM_SUBMENU = 0x00000004,
        MIIM_TYPE = 0x00000010,
        MIIM_DATA = 0x00000020,
        MIIM_STRING = 0x00000040,
        MIIM_BITMAP = 0x00000080,
        MIIM_FTYPE = 0x00000100,
        MB_OK = 0x00000000,
        MF_BYCOMMAND = 0x00000000,
        MF_BYPOSITION = 0x00000400,
        MF_ENABLED = 0x00000000,
        MF_GRAYED = 0x00000001,
        MF_POPUP = 0x00000010,
        MF_SYSMENU = 0x00002000,
        MFS_DISABLED = 0x00000003,
            MFT_MENUBREAK = 0x00000040,
        MFT_SEPARATOR = 0x00000800,
        MFT_RIGHTORDER = 0x00002000,
        MFT_RIGHTJUSTIFY = 0x00004000,
        MDIS_ALLCHILDSTYLES = 0x0001,
        MDITILE_VERTICAL = 0x0000,
        MDITILE_HORIZONTAL = 0x0001,
        MDITILE_SKIPDISABLED = 0x0002,
        MCM_SETMAXSELCOUNT = (0x1000+4),
        MCM_SETSELRANGE = (0x1000+6),
        MCM_GETMONTHRANGE = (0x1000+7),
        MCM_GETMINREQRECT = (0x1000+9),
        MCM_SETCOLOR = (0x1000+10),
        MCM_SETTODAY = (0x1000+12),
        MCM_GETTODAY = (0x1000+13),
        MCM_HITTEST = (0x1000+14),
        MCM_SETFIRSTDAYOFWEEK = (0x1000+15),
        MCM_SETRANGE = (0x1000+18),
        MCM_SETMONTHDELTA = (0x1000+20),
        MCM_GETMAXTODAYWIDTH = (0x1000+21),
        MCHT_TITLE = 0x00010000,
        MCHT_CALENDAR = 0x00020000,
        MCHT_TODAYLINK = 0x00030000,
        MCHT_TITLEBK = (0x00010000),
        MCHT_TITLEMONTH = (0x00010000|0x0001),
        MCHT_TITLEYEAR = (0x00010000|0x0002),
        MCHT_TITLEBTNNEXT = (0x00010000|0x01000000|0x0003),
        MCHT_TITLEBTNPREV = (0x00010000|0x02000000|0x0003),
        MCHT_CALENDARBK = (0x00020000),
        MCHT_CALENDARDATE = (0x00020000|0x0001),
        MCHT_CALENDARDATENEXT = ((0x00020000|0x0001)|0x01000000),
        MCHT_CALENDARDATEPREV = ((0x00020000|0x0001)|0x02000000),
        MCHT_CALENDARDAY = (0x00020000|0x0002),
        MCHT_CALENDARWEEKNUM = (0x00020000|0x0003),
        MCSC_TEXT = 1,
        MCSC_TITLEBK = 2,
        MCSC_TITLETEXT = 3,
        MCSC_MONTHBK = 4,
        MCSC_TRAILINGTEXT = 5,
        MCN_SELCHANGE = ((0-750)+1),
        MCN_GETDAYSTATE = ((0-750)+3),
        MCN_SELECT = ((0-750)+4),
        MCS_DAYSTATE = 0x0001,
        MCS_MULTISELECT = 0x0002,
        MCS_WEEKNUMBERS = 0x0004,
        MCS_NOTODAYCIRCLE = 0x0008,
        MCS_NOTODAY = 0x0010,
        MSAA_MENU_SIG = (unchecked((int) 0xAA0DF00D));

        //ActiveX related defines
        public const int
        OLECONTF_EMBEDDINGS = 0x1,
        OLECONTF_LINKS = 0x2,
        OLECONTF_OTHERS = 0x4,
        OLECONTF_ONLYUSER = 0x8,
        OLECONTF_ONLYIFRUNNING = 0x10,
        OLEMISC_RECOMPOSEONRESIZE = 0x00000001,
        OLEMISC_INSIDEOUT = 0x00000080,
        OLEMISC_ACTIVATEWHENVISIBLE = 0x0000100,
        OLEMISC_ACTSLIKEBUTTON = 0x00001000,
        OLEMISC_SETCLIENTSITEFIRST = 0x00020000,
        OLEIVERB_PRIMARY = 0,
        OLEIVERB_SHOW = -1,
        OLEIVERB_HIDE = -3,
        OLEIVERB_UIACTIVATE = -4,
        OLEIVERB_INPLACEACTIVATE = -5,
        OLEIVERB_DISCARDUNDOSTATE= -6,
        OLEIVERB_PROPERTIES = -7,
        XFORMCOORDS_POSITION = 0x1,
        XFORMCOORDS_SIZE = 0x2,
        XFORMCOORDS_HIMETRICTOCONTAINER = 0x4,
        XFORMCOORDS_CONTAINERTOHIMETRIC = 0x8;

#if never

        public const int NIM_ADD = 0x00000000,
        NIM_MODIFY = 0x00000001,
        NIM_DELETE = 0x00000002,
        NIF_MESSAGE = 0x00000001,
        NIF_ICON = 0x00000002,
        NIF_TIP = 0x00000004,
        NFR_ANSI = 1,
        NFR_UNICODE = 2,
        NM_CLICK = ((0-0)-2),
        NM_DBLCLK = ((0-0)-3),
        NM_RCLICK = ((0-0)-5),
        NM_RDBLCLK = ((0-0)-6),
        NM_CUSTOMDRAW = ((0-0)-12),
        NM_RELEASEDCAPTURE = ((0-0)-16),
        NONANTIALIASED_QUALITY = 3;
#endif

        public const int OFN_READONLY = 0x00000001,
        OFN_OVERWRITEPROMPT = 0x00000002,
        OFN_HIDEREADONLY = 0x00000004,
        OFN_NOCHANGEDIR = 0x00000008,
        //OFN_SHOWHELP = 0x00000010,
        OFN_ENABLEHOOK = 0x00000020,
        OFN_NOVALIDATE = 0x00000100,
        OFN_ALLOWMULTISELECT = 0x00000200,
        OFN_PATHMUSTEXIST = 0x00000800,
        OFN_FILEMUSTEXIST = 0x00001000,
        OFN_CREATEPROMPT = 0x00002000,
        OFN_EXPLORER = 0x00080000,
        OFN_NODEREFERENCELINKS = 0x00100000,
        OFN_ENABLESIZING = 0x00800000,
        OFN_USESHELLITEM = 0x01000000;

#if never
        public const int OLEIVERB_PRIMARY = 0,
        OLEIVERB_SHOW = -1,
        OLEIVERB_HIDE = -3,
        OLEIVERB_UIACTIVATE = -4,
        OLEIVERB_INPLACEACTIVATE = -5,
        OLEIVERB_DISCARDUNDOSTATE= -6,
        OLEIVERB_PROPERTIES = -7,
        OLE_E_NOCONNECTION = unchecked((int)0x80040004),
        OLE_E_PROMPTSAVECANCELLED = unchecked((int)0x8004000C),
        OLEMISC_RECOMPOSEONRESIZE = 0x00000001,
        OLEMISC_INSIDEOUT = 0x00000080,
        OLEMISC_ACTIVATEWHENVISIBLE = 0x0000100,
        OLEMISC_ACTSLIKEBUTTON = 0x00001000,
        OLEMISC_SETCLIENTSITEFIRST = 0x00020000,
        OBJ_PEN = 1,
        OBJ_BRUSH = 2,
        OBJ_DC = 3,
        OBJ_METADC = 4,
        OBJ_PAL = 5,
        OBJ_FONT = 6,
        OBJ_BITMAP = 7,
        OBJ_REGION = 8,
        OBJ_METAFILE = 9,
        OBJ_MEMDC = 10,
        OBJ_EXTPEN = 11,
        OBJ_ENHMETADC = 12,
        ODS_CHECKED = 0x0008,
        ODS_COMBOBOXEDIT = 0x1000,
        ODS_DEFAULT = 0x0020,
        ODS_DISABLED = 0x0004,
        ODS_FOCUS = 0x0010,
        ODS_GRAYED = 0x0002,
        ODS_HOTLIGHT       = 0x0040,
        ODS_INACTIVE       = 0x0080,
        ODS_NOACCEL        = 0x0100,
        ODS_NOFOCUSRECT    = 0x0200,
        ODS_SELECTED = 0x0001,
        OLECLOSE_SAVEIFDIRTY = 0,
        OLECLOSE_PROMPTSAVE = 2;
#endif

        public const int PDERR_SETUPFAILURE = 0x1001,
        PDERR_PARSEFAILURE = 0x1002,
        PDERR_RETDEFFAILURE = 0x1003,
        PDERR_LOADDRVFAILURE = 0x1004,
        PDERR_GETDEVMODEFAIL = 0x1005,
        PDERR_INITFAILURE = 0x1006,
        PDERR_NODEVICES = 0x1007,
        PDERR_NODEFAULTPRN = 0x1008,
        PDERR_DNDMMISMATCH = 0x1009,
        PDERR_CREATEICFAILURE = 0x100A,
        PDERR_PRINTERNOTFOUND = 0x100B,
        PDERR_DEFAULTDIFFERENT = 0x100C,
        PD_ALLPAGES = 0x00000000,
        PD_SELECTION = 0x00000001,
        PD_PAGENUMS = 0x00000002,
        PD_NOSELECTION = 0x00000004,
        PD_NOPAGENUMS = 0x00000008,
        PD_COLLATE = 0x00000010,
        PD_PRINTTOFILE = 0x00000020,
        PD_PRINTSETUP = 0x00000040,
        PD_NOWARNING = 0x00000080,
        PD_RETURNDC = 0x00000100,
        PD_RETURNIC = 0x00000200,
        PD_RETURNDEFAULT = 0x00000400,
        PD_SHOWHELP = 0x00000800,
        PD_ENABLEPRINTHOOK = 0x00001000,
        PD_ENABLESETUPHOOK = 0x00002000,
        PD_ENABLEPRINTTEMPLATE = 0x00004000,
        PD_ENABLESETUPTEMPLATE = 0x00008000,
        PD_ENABLEPRINTTEMPLATEHANDLE = 0x00010000,
        PD_ENABLESETUPTEMPLATEHANDLE = 0x00020000,
        PD_USEDEVMODECOPIES = 0x00040000,
        PD_USEDEVMODECOPIESANDCOLLATE = 0x00040000,
        PD_DISABLEPRINTTOFILE = 0x00080000,
        PD_HIDEPRINTTOFILE = 0x00100000,
        PD_NONETWORKBUTTON = 0x00200000,
        PD_CURRENTPAGE = 0x00400000,
        PD_NOCURRENTPAGE = 0x00800000,
        PD_EXCLUSIONFLAGS = 0x01000000,
        PD_USELARGETEMPLATE = 0x10000000,
        PSD_MINMARGINS = 0x00000001,
        PSD_MARGINS = 0x00000002,
        PSD_INHUNDREDTHSOFMILLIMETERS = 0x00000008,
        PSD_DISABLEMARGINS = 0x00000010,
        PSD_DISABLEPRINTER = 0x00000020,
        PSD_DISABLEORIENTATION = 0x00000100,
        PSD_DISABLEPAPER = 0x00000200,
        PSD_SHOWHELP = 0x00000800,
        PSD_ENABLEPAGESETUPHOOK = 0x00002000,
        PSD_NONETWORKBUTTON = 0x00200000,
        PS_SOLID = 0,
        PS_DOT = 2,
        PLANES = 14,
        PRF_CHECKVISIBLE = 0x00000001,
        PRF_NONCLIENT = 0x00000002,
        PRF_CLIENT = 0x00000004,
        PRF_ERASEBKGND = 0x00000008,
        PRF_CHILDREN = 0x00000010,
        PM_NOREMOVE = 0x0000,
        PM_REMOVE = 0x0001,
        PM_NOYIELD = 0x0002,
        PBM_SETRANGE = (0x0400+1),
        PBM_SETPOS = (0x0400+2),
        PBM_SETSTEP = (0x0400+4),
        PBM_SETRANGE32 = (0x0400+6),
        PBM_SETBARCOLOR = (0x0400+9),
        PBM_SETBKCOLOR  = (0x2000 +1),
        PSM_SETTITLEA = (0x0400+111),
        PSM_SETTITLEW = (0x0400+120),
        PSM_SETFINISHTEXTA = (0x0400+115),
        PSM_SETFINISHTEXTW = (0x0400+121),
        PATCOPY = 0x00F00021,
        PATINVERT = 0x005A0049;
#if never

        public const int PBS_SMOOTH = 0x01;
#endif

        public const int QS_KEY = 0x0001,
        QS_MOUSEMOVE = 0x0002,
        QS_MOUSEBUTTON = 0x0004,
        QS_POSTMESSAGE = 0x0008,
        QS_TIMER = 0x0010,
        QS_PAINT = 0x0020,
        QS_SENDMESSAGE = 0x0040,
        QS_HOTKEY = 0x0080,
        QS_ALLPOSTMESSAGE = 0x0100,
        QS_MOUSE = QS_MOUSEMOVE | QS_MOUSEBUTTON,
        QS_INPUT = QS_MOUSE | QS_KEY,
        QS_ALLEVENTS = QS_INPUT | QS_POSTMESSAGE | QS_TIMER | QS_PAINT | QS_HOTKEY,
        QS_ALLINPUT = QS_INPUT | QS_POSTMESSAGE | QS_TIMER | QS_PAINT | QS_HOTKEY | QS_SENDMESSAGE;
#if never

    //public const int RECO_PASTE = 0x00000000;   // paste from clipboard
    public const int RECO_DROP  = 0x00000001;    // drop
    //public const int RECO_COPY  = 0x00000002;    // copy to the clipboard
    //public const int RECO_CUT   = 0x00000003; // cut to the clipboard
    //public const int RECO_DRAG  = 0x00000004;    // drag

    public const int RPC_E_CHANGED_MODE = unchecked((int)0x80010106),
        RGN_AND = 1,
        RPC_E_CANTCALLOUT_ININPUTSYNCCALL = unchecked((int)0x8001010D),
        RGN_DIFF = 4,
#endif
        public const int RDW_INVALIDATE = 0x0001;
        public const int RDW_ALLCHILDREN = 0x0080;
#if never
        RDW_ERASE = 0x0004,
        RDW_FRAME = 0x0400,
        RB_INSERTBANDA = (0x0400+1),
        RB_INSERTBANDW = (0x0400+10);
#endif

        public const int stc4 = 0x0443,
        SHGFP_TYPE_CURRENT = 0,
        STGM_READ = 0x00000000,
        STGM_WRITE = 0x00000001,
        STGM_READWRITE = 0x00000002,
        STGM_SHARE_EXCLUSIVE = 0x00000010,
        STGM_CREATE = 0x00001000,
        STGM_TRANSACTED = 0x00010000,
        STGM_CONVERT = 0x00020000,
        STGM_DELETEONRELEASE    = 0x04000000,

        STGTY_STORAGE      = 1,
        STGTY_STREAM       = 2,
        STGTY_LOCKBYTES    = 3,
        STGTY_PROPERTY     = 4,

        STARTF_USESHOWWINDOW = 0x00000001,
        SB_HORZ = 0,
        SB_VERT = 1,
        SB_CTL = 2,
        SB_LINEUP = 0,
        SB_LINELEFT = 0,
        SB_LINEDOWN = 1,
        SB_LINERIGHT = 1,
        SB_PAGEUP = 2,
        SB_PAGELEFT = 2,
        SB_PAGEDOWN = 3,
        SB_PAGERIGHT = 3,
        SB_THUMBPOSITION = 4,
        SB_THUMBTRACK = 5,
        SB_LEFT = 6,
        SB_RIGHT = 7,
        SB_ENDSCROLL = 8,
        SB_TOP = 6,
        SB_BOTTOM = 7,
        SIZE_MAXIMIZED = 2,
        ESB_ENABLE_BOTH = 0x0000,
        ESB_DISABLE_BOTH =0x0003,
        SORT_DEFAULT =0x0,
        SUBLANG_DEFAULT = 0x01,
        SW_HIDE = 0,
        SW_NORMAL = 1,
        SW_SHOWMINIMIZED = 2,
        SW_SHOWMAXIMIZED = 3,
        SW_MAXIMIZE = 3,
        SW_SHOWNOACTIVATE = 4,
        SW_SHOW = 5,
        SW_MINIMIZE = 6,
        SW_SHOWMINNOACTIVE = 7,
        SW_SHOWNA = 8,
        SW_RESTORE = 9,
        SW_MAX = 10,
        SWP_NOSIZE = 0x0001,
        SWP_NOMOVE = 0x0002,
        SWP_NOZORDER = 0x0004,
        SWP_NOACTIVATE = 0x0010,
        SWP_SHOWWINDOW = 0x0040,
        SWP_HIDEWINDOW = 0x0080,
        SWP_DRAWFRAME = 0x0020;
#if never

        public const int SND_SYNC = 0000,
       SND_ASYNC = 0x0001,
       SND_NODEFAULT = 0x0002,
       SND_MEMORY = 0x0004,
       SND_LOOP = 0x0008,
       SND_PURGE = 0x0040,
       SND_FILENAME = 0x00020000,
       SND_NOSTOP = 0x0010;
#endif

       public const int MB_ICONHAND = 0x000010,
       MB_ICONQUESTION = 0x000020,
       MB_ICONEXCLAMATION = 0x000030,
       MB_ICONASTERISK = 0x000040;
#if never


       public const int FLASHW_STOP = 0,
       FLASHW_CAPTION      = 0x00000001,
       FLASHW_TRAY         = 0x00000002,
       FLASHW_ALL          = FLASHW_CAPTION | FLASHW_TRAY,
       FLASHW_TIMER        = 0x00000004,
       FLASHW_TIMERNOFG    = 0x0000000C;

        public const int HLP_FILE = 1,
        HLP_KEYWORD = 2,
        HLP_NAVIGATOR = 3,
        HLP_OBJECT = 4;
#endif

        public const int SW_SCROLLCHILDREN = 0x0001,
        SW_INVALIDATE = 0x0002,
        SW_ERASE = 0x0004,
        SW_SMOOTHSCROLL =   0x0010,
        SC_SIZE = 0xF000,
        SC_MINIMIZE = 0xF020,
        SC_MAXIMIZE = 0xF030,
        SC_CLOSE = 0xF060,
        SC_KEYMENU = 0xF100,
        SC_RESTORE = 0xF120,
        SC_MOVE    = 0xF010,
        SS_LEFT = 0x00000000,
        SS_CENTER = 0x00000001,
        SS_RIGHT = 0x00000002,
        SS_OWNERDRAW = 0x0000000D,
        SS_NOPREFIX = 0x00000080,
        SS_SUNKEN = 0x00001000,
        SBS_HORZ = 0x0000,
        SBS_VERT = 0x0001,
        SIF_RANGE = 0x0001,
        SIF_PAGE = 0x0002,
        SIF_POS = 0x0004,
        SIF_TRACKPOS = 0x0010,
        SIF_ALL = (0x0001|0x0002|0x0004|0x0010),
        SPI_GETFONTSMOOTHING = 0x004A,
        SPI_GETDROPSHADOW = 0x1024,
        SPI_GETFLATMENU =   0x1022,
        SPI_GETFONTSMOOTHINGTYPE = 0x200A,
        SPI_GETFONTSMOOTHINGCONTRAST = 0x200C,
        SPI_ICONHORIZONTALSPACING =  0x000D,
        SPI_ICONVERTICALSPACING =   0x0018,
        SPI_GETICONMETRICS =        0x002D,
        SPI_GETICONTITLEWRAP =      0x0019,
        SPI_GETICONTITLELOGFONT =   0x001F,
        SPI_GETKEYBOARDCUES =       0x100A,
        SPI_GETKEYBOARDDELAY =      0x0016,
        SPI_GETKEYBOARDPREF =       0x0044,
        SPI_GETKEYBOARDSPEED =      0x000A,
        SPI_GETMOUSEHOVERWIDTH =    0x0062,
        SPI_GETMOUSEHOVERHEIGHT =   0x0064,
        SPI_GETMOUSEHOVERTIME =     0x0066,
        SPI_GETMOUSESPEED =         0x0070,
        SPI_GETMENUDROPALIGNMENT =  0x001B,
        SPI_GETMENUFADE =           0x1012,
        SPI_GETMENUSHOWDELAY =      0x006A,
        SPI_GETCOMBOBOXANIMATION =  0x1004,
        SPI_GETCLIENTAREAANIMATION = 0x1042,
        SPI_GETGRADIENTCAPTIONS =   0x1008,
        SPI_GETHOTTRACKING =        0x100E,
        SPI_GETLISTBOXSMOOTHSCROLLING =  0x1006,
        SPI_GETMENUANIMATION    =   0x1002,
        SPI_GETSELECTIONFADE =      0x1014,
        SPI_GETTOOLTIPANIMATION =   0x1016,
        SPI_GETUIEFFECTS =          0x103E,
        SPI_GETACTIVEWINDOWTRACKING =       0x1000,
        SPI_GETACTIVEWNDTRKTIMEOUT  =       0x2002,
        SPI_GETANIMATION =          0x0048,
        SPI_GETBORDER  =            0x0005,
        SPI_GETCARETWIDTH =         0x2006,
        SPI_GETMOUSEVANISH =        0x1020,
        SPI_GETDRAGFULLWINDOWS = 38,
        SPI_GETNONCLIENTMETRICS = 41,
        SPI_GETWORKAREA = 48,
        SPI_GETHIGHCONTRAST = 66,
        SPI_GETDEFAULTINPUTLANG = 89,
        SPI_GETSNAPTODEFBUTTON = 95,
        SPI_GETWHEELSCROLLLINES = 104,
        SBARS_SIZEGRIP = 0x0100,
        SB_SETTEXTA = (0x0400+1),
        SB_SETTEXTW = (0x0400+11),
        SB_GETTEXTA = (0x0400+2),
        SB_GETTEXTW = (0x0400+13),
        SB_GETTEXTLENGTHA = (0x0400+3),
        SB_GETTEXTLENGTHW = (0x0400+12),
        SB_SETPARTS = (0x0400+4),
        SB_SIMPLE = (0x0400+9),
        SB_GETRECT = (0x0400+10),
        SB_SETICON = (0x0400+15),
        SB_SETTIPTEXTA = (0x0400+16),
        SB_SETTIPTEXTW = (0x0400+17),
        SB_GETTIPTEXTA = (0x0400+18),
        SB_GETTIPTEXTW = (0x0400+19),
        SBT_OWNERDRAW = 0x1000,
        SBT_NOBORDERS = 0x0100,
        SBT_POPOUT = 0x0200,
        SBT_RTLREADING = 0x0400,
        SRCCOPY = 0x00CC0020,
        SRCAND             = 0x008800C6, /* dest = source AND dest          */
        SRCPAINT           = 0x00EE0086, /* dest = source OR dest           */
        NOTSRCCOPY         = 0x00330008, /* dest = (NOT source)             */
        STATFLAG_DEFAULT = 0x0,
        STATFLAG_NONAME = 0x1,
        STATFLAG_NOOPEN = 0x2,
        STGC_DEFAULT = 0x0,
        STGC_OVERWRITE = 0x1,
        STGC_ONLYIFCURRENT = 0x2,
        STGC_DANGEROUSLYCOMMITMERELYTODISKCACHE = 0x4,
        STREAM_SEEK_SET = 0x0,
        STREAM_SEEK_CUR = 0x1,
        STREAM_SEEK_END = 0x2;

        public const int S_OK =      0x00000000;
        public const int S_FALSE =   0x00000001;

        public static bool Succeeded(int hr) {
            return(hr >= 0);
        }

        public static bool Failed(int hr) {
            return(hr < 0);
        }

        public const int TRANSPARENT = 1,
        OPAQUE = 2,
        TME_HOVER = 0x00000001,
        TME_LEAVE = 0x00000002,
        TPM_LEFTBUTTON = 0x0000,
        TPM_RIGHTBUTTON = 0x0002,
        TPM_LEFTALIGN = 0x0000,
        TPM_RIGHTALIGN = 0x0008,
        TPM_VERTICAL = 0x0040,
        TV_FIRST = 0x1100,
        TBSTATE_CHECKED = 0x01,
        TBSTATE_ENABLED = 0x04,
        TBSTATE_HIDDEN = 0x08,
        TBSTATE_INDETERMINATE = 0x10,
        TBSTYLE_BUTTON = 0x00,
        TBSTYLE_SEP = 0x01,
        TBSTYLE_CHECK = 0x02,
        TBSTYLE_DROPDOWN = 0x08,
        TBSTYLE_TOOLTIPS = 0x0100,
        TBSTYLE_FLAT = 0x0800,
        TBSTYLE_LIST = 0x1000,
        TBSTYLE_EX_DRAWDDARROWS = 0x00000001,
        TB_ENABLEBUTTON = (0x0400+1),
        TB_ISBUTTONCHECKED = (0x0400+10),
        TB_ISBUTTONINDETERMINATE = (0x0400+13),
        TB_ADDBUTTONSA = (0x0400+20),
        TB_ADDBUTTONSW = (0x0400+68),
        TB_INSERTBUTTONA = (0x0400+21),
        TB_INSERTBUTTONW = (0x0400+67),
        TB_DELETEBUTTON = (0x0400+22),
        TB_GETBUTTON = (0x0400+23),
        TB_SAVERESTOREA = (0x0400+26),
        TB_SAVERESTOREW = (0x0400+76),
        TB_ADDSTRINGA = (0x0400+28),
        TB_ADDSTRINGW = (0x0400+77),
        TB_BUTTONSTRUCTSIZE = (0x0400+30),
        TB_SETBUTTONSIZE = (0x0400+31),
        TB_AUTOSIZE = (0x0400+33),
        TB_GETROWS = (0x0400+40),
        TB_GETBUTTONTEXTA = (0x0400+45),
        TB_GETBUTTONTEXTW = (0x0400+75),
        TB_SETIMAGELIST = (0x0400+48),
        TB_GETRECT = (0x0400+51),
        TB_GETBUTTONSIZE = (0x0400+58),
        TB_GETBUTTONINFOW = (0x0400+63),
        TB_SETBUTTONINFOW = (0x0400+64),
        TB_GETBUTTONINFOA = (0x0400+65),
        TB_SETBUTTONINFOA = (0x0400+66),
        TB_MAPACCELERATORA = (0x0400+78),
        TB_SETEXTENDEDSTYLE = (0x0400+84),
        TB_MAPACCELERATORW = (0x0400+90),
        TB_GETTOOLTIPS     = (0x0400 + 35),
        TB_SETTOOLTIPS     = (0x0400 + 36),
        TBIF_IMAGE = 0x00000001,
        TBIF_TEXT = 0x00000002,
        TBIF_STATE = 0x00000004,
        TBIF_STYLE = 0x00000008,
        TBIF_COMMAND = 0x00000020,
        TBIF_SIZE = 0x00000040,
        TBN_GETBUTTONINFOA = ((0-700)-0),
        TBN_GETBUTTONINFOW = ((0-700)-20),
        TBN_QUERYINSERT = ((0-700)-6),
        TBN_DROPDOWN = ((0-700)-10),
        TBN_HOTITEMCHANGE = ((0-700)-13),
        TBN_GETDISPINFOA = ((0-700)-16),
        TBN_GETDISPINFOW = ((0-700)-17),
        TBN_GETINFOTIPA = ((0-700)-18),
        TBN_GETINFOTIPW = ((0-700)-19),
        TTS_ALWAYSTIP = 0x01,
        TTS_NOPREFIX            =0x02,
        TTS_NOANIMATE           =0x10,
        TTS_NOFADE              =0x20,
        TTS_BALLOON             =0x40,
        //TTI_NONE                =0,
        //TTI_INFO                =1,
        TTI_WARNING             =2,
        //TTI_ERROR               =3,
        TTF_IDISHWND = 0x0001,
        TTF_RTLREADING = 0x0004,
        TTF_TRACK = 0x0020,
        TTF_CENTERTIP = 0x0002,
        TTF_SUBCLASS = 0x0010,
        TTF_TRANSPARENT = 0x0100,
        TTF_ABSOLUTE   =  0x0080,
        TTDT_AUTOMATIC = 0,
        TTDT_RESHOW = 1,
        TTDT_AUTOPOP = 2,
        TTDT_INITIAL = 3,
        TTM_TRACKACTIVATE = (0x0400+17),
        TTM_TRACKPOSITION = (0x0400+18),
        TTM_ACTIVATE = (0x0400+1),
        TTM_POP = (0x0400 + 28),
        TTM_ADJUSTRECT = (0x400 + 31),
        TTM_SETDELAYTIME = (0x0400+3),
#if !DRT && !UIAUTOMATIONTYPES
        TTM_SETTITLEA           =((int)WindowMessage.WM_USER + 32),  // wParam = TTI_*, lParam = char* szTitle
        TTM_SETTITLEW           =((int)WindowMessage.WM_USER + 33), // wParam = TTI_*, lParam = wchar* szTitle
#endif
        TTM_ADDTOOLA = (0x0400+4),
        TTM_ADDTOOLW = (0x0400+50),
        TTM_DELTOOLA = (0x0400+5),
        TTM_DELTOOLW = (0x0400+51),
        TTM_NEWTOOLRECTA = (0x0400+6),
        TTM_NEWTOOLRECTW = (0x0400+52),
        TTM_RELAYEVENT = (0x0400+7),
        TTM_GETTIPBKCOLOR = (0x0400+22),
        TTM_SETTIPBKCOLOR =  (0x0400 + 19),
        TTM_SETTIPTEXTCOLOR  = (0x0400 + 20),
        TTM_GETTIPTEXTCOLOR = (0x0400+23),
        TTM_GETTOOLINFOA = (0x0400+8),
        TTM_GETTOOLINFOW = (0x0400+53),
        TTM_SETTOOLINFOA = (0x0400+9),
        TTM_SETTOOLINFOW = (0x0400+54),
        TTM_HITTESTA = (0x0400+10),
        TTM_HITTESTW = (0x0400+55),
        TTM_GETTEXTA = (0x0400+11),
        TTM_GETTEXTW = (0x0400+56),
        TTM_UPDATE = (0x0400+29),
        TTM_UPDATETIPTEXTA = (0x0400+12),
        TTM_UPDATETIPTEXTW = (0x0400+57),
        TTM_ENUMTOOLSA = (0x0400+14),
        TTM_ENUMTOOLSW = (0x0400+58),
        TTM_GETCURRENTTOOLA = (0x0400+15),
        TTM_GETCURRENTTOOLW = (0x0400+59),
        TTM_WINDOWFROMPOINT = (0x0400+16),
        TTM_GETDELAYTIME = (0x0400+21),
        TTM_SETMAXTIPWIDTH = (0x0400+24),
        TTN_GETDISPINFOA = ((0-520)-0),
        TTN_GETDISPINFOW = ((0-520)-10),
        TTN_SHOW = ((0-520)-1),
        TTN_POP = ((0-520)-2),
        TTN_NEEDTEXTA = ((0-520)-0),
        TTN_NEEDTEXTW = ((0-520)-10),
        TBS_AUTOTICKS = 0x0001,
        TBS_VERT = 0x0002,
        TBS_TOP = 0x0004,
        TBS_BOTTOM = 0x0000,
        TBS_BOTH = 0x0008,
        TBS_NOTICKS = 0x0010,
        TBM_GETPOS = (0x0400),
        TBM_SETTIC = (0x0400+4),
        TBM_SETPOS = (0x0400+5),
        TBM_SETRANGE = (0x0400+6),
        TBM_SETRANGEMIN = (0x0400+7),
        TBM_SETRANGEMAX = (0x0400+8),
        TBM_SETTICFREQ = (0x0400+20),
        TBM_SETPAGESIZE = (0x0400+21),
        TBM_SETLINESIZE = (0x0400+23),
        TB_LINEUP = 0,
        TB_LINEDOWN = 1,
        TB_PAGEUP = 2,
        TB_PAGEDOWN = 3,
        TB_THUMBPOSITION = 4,
        TB_THUMBTRACK = 5,
        TB_TOP = 6,
        TB_BOTTOM = 7,
        TB_ENDTRACK = 8,
        TVS_HASBUTTONS = 0x0001,
        TVS_HASLINES = 0x0002,
        TVS_LINESATROOT = 0x0004,
        TVS_EDITLABELS = 0x0008,
        TVS_SHOWSELALWAYS = 0x0020,
        TVS_RTLREADING = 0x0040,
        TVS_CHECKBOXES = 0x0100,
        TVS_TRACKSELECT = 0x0200,
        TVS_FULLROWSELECT = 0x1000,
        TVS_NONEVENHEIGHT = 0x4000,
        TVS_INFOTIP = 0x0800,
        TVS_NOTOOLTIPS = 0x0080,
        TVIF_TEXT = 0x0001,
        TVIF_IMAGE = 0x0002,
        TVIF_PARAM = 0x0004,
        TVIF_STATE = 0x0008,
        TVIF_HANDLE = 0x0010,
        TVIF_SELECTEDIMAGE = 0x0020,
        TVIS_SELECTED = 0x0002,
        TVIS_EXPANDED = 0x0020,
        TVIS_EXPANDEDONCE = 0x0040,
        TVIS_STATEIMAGEMASK = 0xF000,
        TVI_ROOT = (unchecked((int)0xFFFF0000)),
        TVI_FIRST = (unchecked((int)0xFFFF0001)),
        TVM_INSERTITEMA = (0x1100+0),
        TVM_INSERTITEMW = (0x1100+50),
        TVM_DELETEITEM = (0x1100+1),
        TVM_EXPAND = (0x1100+2),
        TVE_COLLAPSE = 0x0001,
        TVE_EXPAND = 0x0002,
        TVM_GETITEMRECT = (0x1100+4),
        TVM_GETINDENT = (0x1100+6),
        TVM_SETINDENT = (0x1100+7),
        TVM_SETIMAGELIST = (0x1100+9),
        TVM_GETNEXTITEM = (0x1100+10),
        TVGN_NEXT = 0x0001,
        TVGN_PREVIOUS = 0x0002,
        TVGN_FIRSTVISIBLE = 0x0005,
        TVGN_NEXTVISIBLE = 0x0006,
        TVGN_PREVIOUSVISIBLE = 0x0007,
        TVGN_CARET = 0x0009,
        TVM_SELECTITEM = (0x1100+11),
        TVM_GETITEMA = (0x1100+12),
        TVM_GETITEMW = (0x1100+62),
        TVM_SETITEMA = (0x1100+13),
        TVM_SETITEMW = (0x1100+63),
        TVM_EDITLABELA = (0x1100+14),
        TVM_EDITLABELW = (0x1100+65),
        TVM_GETEDITCONTROL = (0x1100+15),
        TVM_GETVISIBLECOUNT = (0x1100+16),
        TVM_HITTEST = (0x1100+17),
        TVM_ENSUREVISIBLE = (0x1100+20),
        TVM_ENDEDITLABELNOW = (0x1100+22),
        TVM_GETISEARCHSTRINGA = (0x1100+23),
        TVM_GETISEARCHSTRINGW = (0x1100+64),
        TVM_SETITEMHEIGHT = (0x1100+27),
        TVM_GETITEMHEIGHT = (0x1100+28),
        TVN_SELCHANGINGA = ((0-400)-1),
        TVN_SELCHANGINGW = ((0-400)-50),
        TVN_GETINFOTIPA  = ((0-400)-13),
        TVN_GETINFOTIPW  = ((0-400)-14),
        TVN_SELCHANGEDA = ((0-400)-2),
        TVN_SELCHANGEDW = ((0-400)-51),
        TVC_UNKNOWN = 0x0000,
        TVC_BYMOUSE = 0x0001,
        TVC_BYKEYBOARD = 0x0002,
        TVN_GETDISPINFOA = ((0-400)-3),
        TVN_GETDISPINFOW = ((0-400)-52),
        TVN_SETDISPINFOA = ((0-400)-4),
        TVN_SETDISPINFOW = ((0-400)-53),
        TVN_ITEMEXPANDINGA = ((0-400)-5),
        TVN_ITEMEXPANDINGW = ((0-400)-54),
        TVN_ITEMEXPANDEDA = ((0-400)-6),
        TVN_ITEMEXPANDEDW = ((0-400)-55),
        TVN_BEGINDRAGA = ((0-400)-7),
        TVN_BEGINDRAGW = ((0-400)-56),
        TVN_BEGINRDRAGA = ((0-400)-8),
        TVN_BEGINRDRAGW = ((0-400)-57),
        TVN_BEGINLABELEDITA = ((0-400)-10),
        TVN_BEGINLABELEDITW = ((0-400)-59),
        TVN_ENDLABELEDITA = ((0-400)-11),
        TVN_ENDLABELEDITW = ((0-400)-60),
        TCS_BOTTOM = 0x0002,
        TCS_RIGHT = 0x0002,
        TCS_FLATBUTTONS = 0x0008,
        TCS_HOTTRACK = 0x0040,
        TCS_VERTICAL = 0x0080,
        TCS_TABS = 0x0000,
        TCS_BUTTONS = 0x0100,
        TCS_MULTILINE = 0x0200,
        TCS_RIGHTJUSTIFY = 0x0000,
        TCS_FIXEDWIDTH = 0x0400,
        TCS_RAGGEDRIGHT = 0x0800,
        TCS_OWNERDRAWFIXED = 0x2000,
        TCS_TOOLTIPS = 0x4000,
        TCM_SETIMAGELIST = (0x1300+3),
        TCIF_TEXT = 0x0001,
        TCIF_IMAGE = 0x0002,
        TCM_GETITEMA = (0x1300+5),
        TCM_GETITEMW = (0x1300+60),
        TCM_SETITEMA = (0x1300+6),
        TCM_SETITEMW = (0x1300+61),
        TCM_INSERTITEMA = (0x1300+7),
        TCM_INSERTITEMW = (0x1300+62),
        TCM_DELETEITEM = (0x1300+8),
        TCM_DELETEALLITEMS = (0x1300+9),
        TCM_GETITEMRECT = (0x1300+10),
        TCM_GETCURSEL = (0x1300+11),
        TCM_SETCURSEL = (0x1300+12),
        TCM_ADJUSTRECT = (0x1300+40),
        TCM_SETITEMSIZE = (0x1300+41),
        TCM_SETPADDING = (0x1300+43),
        TCM_GETROWCOUNT = (0x1300+44),
        TCM_GETTOOLTIPS = (0x1300+45),
        TCM_SETTOOLTIPS = (0x1300+46),
        TCN_SELCHANGE = ((0-550)-1),
        TCN_SELCHANGING = ((0-550)-2),
        TBSTYLE_WRAPPABLE = 0x0200,
        TVM_SETBKCOLOR = (TV_FIRST + 29),
        TVM_SETTEXTCOLOR = (TV_FIRST + 30),
        TYMED_NULL = 0,
        TVM_GETLINECOLOR  = (TV_FIRST + 41),
        TVM_SETLINECOLOR  = (TV_FIRST + 40),
        TVM_SETTOOLTIPS   = (TV_FIRST + 24),
        TVSIL_STATE   =          2,
        TVM_SORTCHILDRENCB = (TV_FIRST + 21);

        public const int
        UIS_SET        = 1,
        UIS_CLEAR      = 2,
        UIS_INITIALIZE = 3,
        UISF_HIDEFOCUS = 0x1,
        UISF_HIDEACCEL = 0x2,
        UISF_ACTIVE    = 0x4;

#if never

        public const int TVHT_NOWHERE =  0x0001,
        TVHT_ONITEMICON         = 0x0002,
        TVHT_ONITEMLABEL        = 0x0004,
        TVHT_ONITEM             = (TVHT_ONITEMICON | TVHT_ONITEMLABEL | TVHT_ONITEMSTATEICON),
        TVHT_ONITEMINDENT       = 0x0008,
        TVHT_ONITEMBUTTON       = 0x0010,
        TVHT_ONITEMRIGHT        = 0x0020,
        TVHT_ONITEMSTATEICON    = 0x0040,
        TVHT_ABOVE              = 0x0100,
        TVHT_BELOW              = 0x0200,
        TVHT_TORIGHT            = 0x0400,
        TVHT_TOLEFT             = 0x0800;

        public const int
        USERCLASSTYPE_FULL = 1,
        USERCLASSTYPE_SHORT = 2,
        USERCLASSTYPE_APPNAME = 3,
        UOI_FLAGS = 1;


        public const int VIEW_E_DRAW = unchecked((int)0x80040140),
        VK_LEFT = 0x25,
        VK_UP = 0x26,
        VK_RIGHT = 0x27,
        VK_DOWN = 0x28,
#endif

        public const int VK_TAB = 0x09;
        public const int VK_SHIFT = 0x10;
        public const int VK_CONTROL = 0x11;
        public const int VK_MENU = 0x12;
#if never
        VK_ESCAPE = 0x1B,
        VK_INSERT = 0x002D;

        public const int WAVE_FORMAT_PCM        = 0x0001,
        WAVE_FORMAT_ADPCM                       = 0x0002,
        WAVE_FORMAT_IEEE_FLOAT                  = 0x0003;

        public const int MMIO_READ              = 0x00000000,
        MMIO_ALLOCBUF                           = 0x00010000,
        MMIO_FINDRIFF                           = 0x00000020;
#endif

        public const int WH_JOURNALPLAYBACK = 1,
        WH_GETMESSAGE = 3,
        WH_MOUSE = 7,
        WSF_VISIBLE = 0x0001,
        WA_INACTIVE = 0,
        WA_ACTIVE = 1,
        WA_CLICKACTIVE = 2;

        public const int WHEEL_DELTA = 120,
#if !DRT && !UIAUTOMATIONTYPES
        WM_REFLECT = (int)WindowMessage.WM_USER + 0x1C00,
        WM_CHOOSEFONT_GETLOGFONT = (int)WindowMessage.WM_USER +1,
#endif
        WS_OVERLAPPED = 0x00000000,
        WS_POPUP = unchecked((int)0x80000000),
        WS_CHILD = 0x40000000,
        WS_MINIMIZE = 0x20000000,
        WS_VISIBLE = 0x10000000,
        WS_DISABLED = 0x08000000,
        WS_CLIPSIBLINGS = 0x04000000,
        WS_CLIPCHILDREN = 0x02000000,
        WS_MAXIMIZE = 0x01000000,
        WS_CAPTION = 0x00C00000,
        WS_BORDER = 0x00800000,
        WS_DLGFRAME = 0x00400000,
        WS_VSCROLL = 0x00200000,
        WS_HSCROLL = 0x00100000,
        WS_SYSMENU = 0x00080000,
        WS_THICKFRAME = 0x00040000,
        WS_TABSTOP = 0x00010000,
        WS_MINIMIZEBOX = 0x00020000,
        WS_MAXIMIZEBOX = 0x00010000,
        WS_EX_DLGMODALFRAME = 0x00000001,
        WS_EX_TRANSPARENT = 0x00000020,
        WS_EX_MDICHILD = 0x00000040,
        WS_EX_TOOLWINDOW = 0x00000080,
        WS_EX_WINDOWEDGE = 0x00000100,
        WS_EX_CLIENTEDGE = 0x00000200,
        WS_EX_CONTEXTHELP = 0x00000400,
        WS_EX_RIGHT = 0x00001000,
        WS_EX_LEFT = 0x00000000,
        WS_EX_RTLREADING = 0x00002000,
        WS_EX_LEFTSCROLLBAR = 0x00004000,
        WS_EX_CONTROLPARENT = 0x00010000,
        WS_EX_STATICEDGE = 0x00020000,
        WS_EX_APPWINDOW = 0x00040000,
        WS_EX_LAYERED = 0x00080000,
        WS_EX_TOPMOST = 0x00000008,
        WS_EX_LAYOUTRTL = 0x00400000,
        WS_EX_NOINHERITLAYOUT = 0x00100000,
        WS_EX_COMPOSITED = 0x02000000,
        WPF_SETMINPOSITION = 0x0001,
        WPF_RESTORETOMAXIMIZED = 0x0002;

        public const int WHITE_BRUSH = 0x00000000;
        public const int NULL_BRUSH = 5;

#if never

        public static int  START_PAGE_GENERAL =  unchecked((int)0xffffffff);

        //  Result action ids for PrintDlgEx.
        public const int PD_RESULT_CANCEL               = 0;
        public const int PD_RESULT_PRINT                = 1;
        public const int PD_RESULT_APPLY                = 2;

#endif

        public const int XBUTTON1    =  0x0001;
        public const int XBUTTON2    =  0x0002;
#if never


        // These are initialized in a static constructor for speed.  That way we don't have to
        // evaluate the char size each time.
        //
        public static readonly int CBEM_GETITEM;
        public static readonly int CBEM_SETITEM;
        public static readonly int CBEN_ENDEDIT;
        public static readonly int CBEM_INSERTITEM;
        public static readonly int LVM_GETITEMTEXT;
        public static readonly int LVM_SETITEMTEXT;
        public static readonly int ACM_OPEN;
        public static readonly int DTM_SETFORMAT;
        public static readonly int DTN_USERSTRING;
        public static readonly int DTN_WMKEYDOWN;
        public static readonly int DTN_FORMAT;
        public static readonly int DTN_FORMATQUERY;
        public static readonly int EMR_POLYTEXTOUT;
        public static readonly int HDM_INSERTITEM;
        public static readonly int HDM_GETITEM;
        public static readonly int HDM_SETITEM;
        public static readonly int HDN_ITEMCHANGING;
        public static readonly int HDN_ITEMCHANGED;
        public static readonly int HDN_ITEMCLICK;
        public static readonly int HDN_ITEMDBLCLICK;
        public static readonly int HDN_DIVIDERDBLCLICK;
        public static readonly int HDN_BEGINTRACK;
        public static readonly int HDN_ENDTRACK;
        public static readonly int HDN_TRACK;
        public static readonly int HDN_GETDISPINFO;
        public static readonly int LVM_GETITEM;
        public static readonly int LVM_SETBKIMAGE;
        public static readonly int LVM_SETITEM;
        public static readonly int LVM_INSERTITEM;
        public static readonly int LVM_FINDITEM;
        public static readonly int LVM_GETSTRINGWIDTH;
        public static readonly int LVM_EDITLABEL;
        public static readonly int LVM_GETCOLUMN;
        public static readonly int LVM_SETCOLUMN;
        public static readonly int LVM_GETISEARCHSTRING;
        public static readonly int LVM_INSERTCOLUMN;
        public static readonly int LVN_BEGINLABELEDIT;
        public static readonly int LVN_ENDLABELEDIT;
        public static readonly int LVN_ODFINDITEM;
        public static readonly int LVN_GETDISPINFO;
        public static readonly int LVN_GETINFOTIP;
        public static readonly int LVN_SETDISPINFO;
        public static readonly int PSM_SETTITLE;
        public static readonly int PSM_SETFINISHTEXT;
        public static readonly int RB_INSERTBAND;
        public static readonly int SB_SETTEXT;
        public static readonly int SB_GETTEXT;
        public static readonly int SB_GETTEXTLENGTH;
        public static readonly int SB_SETTIPTEXT;
        public static readonly int SB_GETTIPTEXT;
        public static readonly int TB_SAVERESTORE;
        public static readonly int TB_ADDSTRING;
        public static readonly int TB_GETBUTTONTEXT;
        public static readonly int TB_MAPACCELERATOR;
        public static readonly int TB_GETBUTTONINFO;
        public static readonly int TB_SETBUTTONINFO;
        public static readonly int TB_INSERTBUTTON;
        public static readonly int TB_ADDBUTTONS;
        public static readonly int TBN_GETBUTTONINFO;
        public static readonly int TBN_GETINFOTIP;
        public static readonly int TBN_GETDISPINFO;
        public static readonly int TTM_ADDTOOL;
        public static readonly int TTM_SETTITLE;
        public static readonly int TTM_DELTOOL;
        public static readonly int TTM_NEWTOOLRECT;
        public static readonly int TTM_GETTOOLINFO;
        public static readonly int TTM_SETTOOLINFO;
        public static readonly int TTM_HITTEST;
        public static readonly int TTM_GETTEXT;
        public static readonly int TTM_UPDATETIPTEXT;
        public static readonly int TTM_ENUMTOOLS;
        public static readonly int TTM_GETCURRENTTOOL;
        public static readonly int TTN_GETDISPINFO;
        public static readonly int TTN_NEEDTEXT;
        public static readonly int TVM_INSERTITEM;
        public static readonly int TVM_GETITEM;
        public static readonly int TVM_SETITEM;
        public static readonly int TVM_EDITLABEL;
        public static readonly int TVM_GETISEARCHSTRING;
        public static readonly int TVN_SELCHANGING;
        public static readonly int TVN_SELCHANGED;
        public static readonly int TVN_GETDISPINFO;
        public static readonly int TVN_SETDISPINFO;
        public static readonly int TVN_ITEMEXPANDING;
        public static readonly int TVN_ITEMEXPANDED;
        public static readonly int TVN_BEGINDRAG;
        public static readonly int TVN_BEGINRDRAG;
        public static readonly int TVN_BEGINLABELEDIT;
        public static readonly int TVN_ENDLABELEDIT;
        public static readonly int TCM_GETITEM;
        public static readonly int TCM_SETITEM;
        public static readonly int TCM_INSERTITEM;

        public const string TOOLTIPS_CLASS = "tooltips_class32";

        public const string WC_DATETIMEPICK = "SysDateTimePick32",
        WC_LISTVIEW = "SysListView32",
        WC_MONTHCAL = "SysMonthCal32",
        WC_PROGRESS = "msctls_progress32",
        WC_STATUSBAR = "msctls_statusbar32",
        WC_TOOLBAR = "ToolbarWindow32",
        WC_TRACKBAR = "msctls_trackbar32",
        WC_TREEVIEW = "SysTreeView32",
        WC_TABCONTROL = "SysTabControl32",
        MSH_MOUSEWHEEL = "MSWHEEL_ROLLMSG",
        MSH_SCROLL_LINES = "MSH_SCROLL_LINES_MSG",
        MOUSEZ_CLASSNAME = "MouseZ",
        MOUSEZ_TITLE = "Magellan MSWHEEL";

	    public const int CHILDID_SELF = 0;

        public const int OBJID_QUERYCLASSNAMEIDX = unchecked(unchecked((int)0xFFFFFFF4));
        public const int OBJID_WINDOW            = unchecked(unchecked((int)0x00000000));

        public const string uuid_IAccessible  = "{618736E0-3C3D-11CF-810C-00AA00389B71}";
        public const string uuid_IEnumVariant = "{00020404-0000-0000-C000-000000000046}";

        static NativeMethods() {
            if (Marshal.SystemDefaultCharSize == 1) {
                CBEM_GETITEM = NativeMethods.CBEM_GETITEMA;
                CBEM_SETITEM = NativeMethods.CBEM_SETITEMA;
                CBEN_ENDEDIT = NativeMethods.CBEN_ENDEDITA;
                CBEM_INSERTITEM = NativeMethods.CBEM_INSERTITEMA;
                LVM_GETITEMTEXT = NativeMethods.LVM_GETITEMTEXTA;
                LVM_SETITEMTEXT = NativeMethods.LVM_SETITEMTEXTA;
                ACM_OPEN = NativeMethods.ACM_OPENA;
                DTM_SETFORMAT = NativeMethods.DTM_SETFORMATA;
                DTN_USERSTRING = NativeMethods.DTN_USERSTRINGA;
                DTN_WMKEYDOWN = NativeMethods.DTN_WMKEYDOWNA;
                DTN_FORMAT = NativeMethods.DTN_FORMATA;
                DTN_FORMATQUERY = NativeMethods.DTN_FORMATQUERYA;
                EMR_POLYTEXTOUT = NativeMethods.EMR_POLYTEXTOUTA;
                HDM_INSERTITEM = NativeMethods.HDM_INSERTITEMA;
                HDM_GETITEM = NativeMethods.HDM_GETITEMA;
                HDM_SETITEM = NativeMethods.HDM_SETITEMA;
                HDN_ITEMCHANGING = NativeMethods.HDN_ITEMCHANGINGA;
                HDN_ITEMCHANGED = NativeMethods.HDN_ITEMCHANGEDA;
                HDN_ITEMCLICK = NativeMethods.HDN_ITEMCLICKA;
                HDN_ITEMDBLCLICK = NativeMethods.HDN_ITEMDBLCLICKA;
                HDN_DIVIDERDBLCLICK = NativeMethods.HDN_DIVIDERDBLCLICKA;
                HDN_BEGINTRACK = NativeMethods.HDN_BEGINTRACKA;
                HDN_ENDTRACK = NativeMethods.HDN_ENDTRACKA;
                HDN_TRACK = NativeMethods.HDN_TRACKA;
                HDN_GETDISPINFO = NativeMethods.HDN_GETDISPINFOA;
                LVM_SETBKIMAGE = NativeMethods.LVM_SETBKIMAGEA;
                LVM_GETITEM = NativeMethods.LVM_GETITEMA;
                LVM_SETITEM = NativeMethods.LVM_SETITEMA;
                LVM_INSERTITEM = NativeMethods.LVM_INSERTITEMA;
                LVM_FINDITEM = NativeMethods.LVM_FINDITEMA;
                LVM_GETSTRINGWIDTH = NativeMethods.LVM_GETSTRINGWIDTHA;
                LVM_EDITLABEL = NativeMethods.LVM_EDITLABELA;
                LVM_GETCOLUMN = NativeMethods.LVM_GETCOLUMNA;
                LVM_SETCOLUMN = NativeMethods.LVM_SETCOLUMNA;
                LVM_GETISEARCHSTRING = NativeMethods.LVM_GETISEARCHSTRINGA;
                LVM_INSERTCOLUMN = NativeMethods.LVM_INSERTCOLUMNA;
                LVN_BEGINLABELEDIT = NativeMethods.LVN_BEGINLABELEDITA;
                LVN_ENDLABELEDIT = NativeMethods.LVN_ENDLABELEDITA;
                LVN_ODFINDITEM = NativeMethods.LVN_ODFINDITEMA;
                LVN_GETDISPINFO = NativeMethods.LVN_GETDISPINFOA;
                LVN_GETINFOTIP = NativeMethods.LVN_GETINFOTIPA;
                LVN_SETDISPINFO = NativeMethods.LVN_SETDISPINFOA;
                PSM_SETTITLE = NativeMethods.PSM_SETTITLEA;
                PSM_SETFINISHTEXT = NativeMethods.PSM_SETFINISHTEXTA;
                RB_INSERTBAND = NativeMethods.RB_INSERTBANDA;
                SB_SETTEXT = NativeMethods.SB_SETTEXTA;
                SB_GETTEXT = NativeMethods.SB_GETTEXTA;
                SB_GETTEXTLENGTH = NativeMethods.SB_GETTEXTLENGTHA;
                SB_SETTIPTEXT = NativeMethods.SB_SETTIPTEXTA;
                SB_GETTIPTEXT = NativeMethods.SB_GETTIPTEXTA;
                TB_SAVERESTORE = NativeMethods.TB_SAVERESTOREA;
                TB_ADDSTRING = NativeMethods.TB_ADDSTRINGA;
                TB_GETBUTTONTEXT = NativeMethods.TB_GETBUTTONTEXTA;
                TB_MAPACCELERATOR = NativeMethods.TB_MAPACCELERATORA;
                TB_GETBUTTONINFO = NativeMethods.TB_GETBUTTONINFOA;
                TB_SETBUTTONINFO = NativeMethods.TB_SETBUTTONINFOA;
                TB_INSERTBUTTON = NativeMethods.TB_INSERTBUTTONA;
                TB_ADDBUTTONS = NativeMethods.TB_ADDBUTTONSA;
                TBN_GETBUTTONINFO = NativeMethods.TBN_GETBUTTONINFOA;
                TBN_GETINFOTIP = NativeMethods.TBN_GETINFOTIPA;
                TBN_GETDISPINFO = NativeMethods.TBN_GETDISPINFOA;
                TTM_ADDTOOL = NativeMethods.TTM_ADDTOOLA;
                TTM_SETTITLE = NativeMethods.TTM_SETTITLEA;
                TTM_DELTOOL = NativeMethods.TTM_DELTOOLA;
                TTM_NEWTOOLRECT = NativeMethods.TTM_NEWTOOLRECTA;
                TTM_GETTOOLINFO = NativeMethods.TTM_GETTOOLINFOA;
                TTM_SETTOOLINFO = NativeMethods.TTM_SETTOOLINFOA;
                TTM_HITTEST = NativeMethods.TTM_HITTESTA;
                TTM_GETTEXT = NativeMethods.TTM_GETTEXTA;
                TTM_UPDATETIPTEXT = NativeMethods.TTM_UPDATETIPTEXTA;
                TTM_ENUMTOOLS = NativeMethods.TTM_ENUMTOOLSA;
                TTM_GETCURRENTTOOL = NativeMethods.TTM_GETCURRENTTOOLA;
                TTN_GETDISPINFO = NativeMethods.TTN_GETDISPINFOA;
                TTN_NEEDTEXT = NativeMethods.TTN_NEEDTEXTA;
                TVM_INSERTITEM = NativeMethods.TVM_INSERTITEMA;
                TVM_GETITEM = NativeMethods.TVM_GETITEMA;
                TVM_SETITEM = NativeMethods.TVM_SETITEMA;
                TVM_EDITLABEL = NativeMethods.TVM_EDITLABELA;
                TVM_GETISEARCHSTRING = NativeMethods.TVM_GETISEARCHSTRINGA;
                TVN_SELCHANGING = NativeMethods.TVN_SELCHANGINGA;
                TVN_SELCHANGED = NativeMethods.TVN_SELCHANGEDA;
                TVN_GETDISPINFO = NativeMethods.TVN_GETDISPINFOA;
                TVN_SETDISPINFO = NativeMethods.TVN_SETDISPINFOA;
                TVN_ITEMEXPANDING = NativeMethods.TVN_ITEMEXPANDINGA;
                TVN_ITEMEXPANDED = NativeMethods.TVN_ITEMEXPANDEDA;
                TVN_BEGINDRAG = NativeMethods.TVN_BEGINDRAGA;
                TVN_BEGINRDRAG = NativeMethods.TVN_BEGINRDRAGA;
                TVN_BEGINLABELEDIT = NativeMethods.TVN_BEGINLABELEDITA;
                TVN_ENDLABELEDIT = NativeMethods.TVN_ENDLABELEDITA;
                TCM_GETITEM = NativeMethods.TCM_GETITEMA;
                TCM_SETITEM = NativeMethods.TCM_SETITEMA;
                TCM_INSERTITEM = NativeMethods.TCM_INSERTITEMA;
            }
            else {
                CBEM_GETITEM = NativeMethods.CBEM_GETITEMW;
                CBEM_SETITEM = NativeMethods.CBEM_SETITEMW;
                CBEN_ENDEDIT = NativeMethods.CBEN_ENDEDITW;
                CBEM_INSERTITEM = NativeMethods.CBEM_INSERTITEMW;
                LVM_GETITEMTEXT = NativeMethods.LVM_GETITEMTEXTW;
                LVM_SETITEMTEXT = NativeMethods.LVM_SETITEMTEXTW;
                ACM_OPEN = NativeMethods.ACM_OPENW;
                DTM_SETFORMAT = NativeMethods.DTM_SETFORMATW;
                DTN_USERSTRING = NativeMethods.DTN_USERSTRINGW;
                DTN_WMKEYDOWN = NativeMethods.DTN_WMKEYDOWNW;
                DTN_FORMAT = NativeMethods.DTN_FORMATW;
                DTN_FORMATQUERY = NativeMethods.DTN_FORMATQUERYW;
                EMR_POLYTEXTOUT = NativeMethods.EMR_POLYTEXTOUTW;
                HDM_INSERTITEM = NativeMethods.HDM_INSERTITEMW;
                HDM_GETITEM = NativeMethods.HDM_GETITEMW;
                HDM_SETITEM = NativeMethods.HDM_SETITEMW;
                HDN_ITEMCHANGING = NativeMethods.HDN_ITEMCHANGINGW;
                HDN_ITEMCHANGED = NativeMethods.HDN_ITEMCHANGEDW;
                HDN_ITEMCLICK = NativeMethods.HDN_ITEMCLICKW;
                HDN_ITEMDBLCLICK = NativeMethods.HDN_ITEMDBLCLICKW;
                HDN_DIVIDERDBLCLICK = NativeMethods.HDN_DIVIDERDBLCLICKW;
                HDN_BEGINTRACK = NativeMethods.HDN_BEGINTRACKW;
                HDN_ENDTRACK = NativeMethods.HDN_ENDTRACKW;
                HDN_TRACK = NativeMethods.HDN_TRACKW;
                HDN_GETDISPINFO = NativeMethods.HDN_GETDISPINFOW;
                LVM_SETBKIMAGE = NativeMethods.LVM_SETBKIMAGEW;
                LVM_GETITEM = NativeMethods.LVM_GETITEMW;
                LVM_SETITEM = NativeMethods.LVM_SETITEMW;
                LVM_INSERTITEM = NativeMethods.LVM_INSERTITEMW;
                LVM_FINDITEM = NativeMethods.LVM_FINDITEMW;
                LVM_GETSTRINGWIDTH = NativeMethods.LVM_GETSTRINGWIDTHW;
                LVM_EDITLABEL = NativeMethods.LVM_EDITLABELW;
                LVM_GETCOLUMN = NativeMethods.LVM_GETCOLUMNW;
                LVM_SETCOLUMN = NativeMethods.LVM_SETCOLUMNW;
                LVM_GETISEARCHSTRING = NativeMethods.LVM_GETISEARCHSTRINGW;
                LVM_INSERTCOLUMN = NativeMethods.LVM_INSERTCOLUMNW;
                LVN_BEGINLABELEDIT = NativeMethods.LVN_BEGINLABELEDITW;
                LVN_ENDLABELEDIT = NativeMethods.LVN_ENDLABELEDITW;
                LVN_ODFINDITEM = NativeMethods.LVN_ODFINDITEMW;
                LVN_GETDISPINFO = NativeMethods.LVN_GETDISPINFOW;
                LVN_GETINFOTIP = NativeMethods.LVN_GETINFOTIPW;
                LVN_SETDISPINFO = NativeMethods.LVN_SETDISPINFOW;
                PSM_SETTITLE = NativeMethods.PSM_SETTITLEW;
                PSM_SETFINISHTEXT = NativeMethods.PSM_SETFINISHTEXTW;
                RB_INSERTBAND = NativeMethods.RB_INSERTBANDW;
                SB_SETTEXT = NativeMethods.SB_SETTEXTW;
                SB_GETTEXT = NativeMethods.SB_GETTEXTW;
                SB_GETTEXTLENGTH = NativeMethods.SB_GETTEXTLENGTHW;
                SB_SETTIPTEXT = NativeMethods.SB_SETTIPTEXTW;
                SB_GETTIPTEXT = NativeMethods.SB_GETTIPTEXTW;
                TB_SAVERESTORE = NativeMethods.TB_SAVERESTOREW;
                TB_ADDSTRING = NativeMethods.TB_ADDSTRINGW;
                TB_GETBUTTONTEXT = NativeMethods.TB_GETBUTTONTEXTW;
                TB_MAPACCELERATOR = NativeMethods.TB_MAPACCELERATORW;
                TB_GETBUTTONINFO = NativeMethods.TB_GETBUTTONINFOW;
                TB_SETBUTTONINFO = NativeMethods.TB_SETBUTTONINFOW;
                TB_INSERTBUTTON = NativeMethods.TB_INSERTBUTTONW;
                TB_ADDBUTTONS = NativeMethods.TB_ADDBUTTONSW;
                TBN_GETBUTTONINFO = NativeMethods.TBN_GETBUTTONINFOW;
                TBN_GETINFOTIP = NativeMethods.TBN_GETINFOTIPW;
                TBN_GETDISPINFO = NativeMethods.TBN_GETDISPINFOW;
                TTM_ADDTOOL = NativeMethods.TTM_ADDTOOLW;
                TTM_SETTITLE = NativeMethods.TTM_SETTITLEW;
                TTM_DELTOOL = NativeMethods.TTM_DELTOOLW;
                TTM_NEWTOOLRECT = NativeMethods.TTM_NEWTOOLRECTW;
                TTM_GETTOOLINFO = NativeMethods.TTM_GETTOOLINFOW;
                TTM_SETTOOLINFO = NativeMethods.TTM_SETTOOLINFOW;
                TTM_HITTEST = NativeMethods.TTM_HITTESTW;
                TTM_GETTEXT = NativeMethods.TTM_GETTEXTW;
                TTM_UPDATETIPTEXT = NativeMethods.TTM_UPDATETIPTEXTW;
                TTM_ENUMTOOLS = NativeMethods.TTM_ENUMTOOLSW;
                TTM_GETCURRENTTOOL = NativeMethods.TTM_GETCURRENTTOOLW;
                TTN_GETDISPINFO = NativeMethods.TTN_GETDISPINFOW;
                TTN_NEEDTEXT = NativeMethods.TTN_NEEDTEXTW;
                TVM_INSERTITEM = NativeMethods.TVM_INSERTITEMW;
                TVM_GETITEM = NativeMethods.TVM_GETITEMW;
                TVM_SETITEM = NativeMethods.TVM_SETITEMW;
                TVM_EDITLABEL = NativeMethods.TVM_EDITLABELW;
                TVM_GETISEARCHSTRING = NativeMethods.TVM_GETISEARCHSTRINGW;
                TVN_SELCHANGING = NativeMethods.TVN_SELCHANGINGW;
                TVN_SELCHANGED = NativeMethods.TVN_SELCHANGEDW;
                TVN_GETDISPINFO = NativeMethods.TVN_GETDISPINFOW;
                TVN_SETDISPINFO = NativeMethods.TVN_SETDISPINFOW;
                TVN_ITEMEXPANDING = NativeMethods.TVN_ITEMEXPANDINGW;
                TVN_ITEMEXPANDED = NativeMethods.TVN_ITEMEXPANDEDW;
                TVN_BEGINDRAG = NativeMethods.TVN_BEGINDRAGW;
                TVN_BEGINRDRAG = NativeMethods.TVN_BEGINRDRAGW;
                TVN_BEGINLABELEDIT = NativeMethods.TVN_BEGINLABELEDITW;
                TVN_ENDLABELEDIT = NativeMethods.TVN_ENDLABELEDITW;
                TCM_GETITEM = NativeMethods.TCM_GETITEMW;
                TCM_SETITEM = NativeMethods.TCM_SETITEMW;
                TCM_INSERTITEM = NativeMethods.TCM_INSERTITEMW;
            }
        }

        /*
        * MISCELLANEOUS
        */


        [StructLayout(LayoutKind.Sequential), CLSCompliant(false)]
        public class OLECMD {
            [MarshalAs(UnmanagedType.U4)]
            public   uint cmdID;
            [MarshalAs(UnmanagedType.U4)]
            public   uint cmdf;
        }

        /// <SecurityNote>
        /// Critical : Elevates to UnmanagedCode permissions
        /// </SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [SuppressUnmanagedCodeSecurity]
        [ComVisible(true), ComImport(), Guid("B722BCCB-4E68-101B-A2BC-00AA00404770"),
        InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown), CLSCompliantAttribute(false)]
        public interface IOleCommandTarget
        {

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int QueryStatus(
                ref Guid pguidCmdGroup,
                int cCmds,
                [In, Out]
                OLECMD prgCmds,
                [In, Out]
                IntPtr pCmdText);

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int Exec(
                ref Guid pguidCmdGroup,
                int nCmdID,
                int nCmdexecopt,
                // we need to have this an array because callers need to be able to specify NULL or VT_NULL
                [In, MarshalAs(UnmanagedType.LPArray)]
                Object[] pvaIn,
                int pvaOut);
        }
#endif
        public static int SignedHIWORD(int n)
        {
            int i = (int)(short)((n >> 16) & 0xffff);

            return i;
        }

        public static int SignedLOWORD(int n)
        {
            int i = (int)(short)(n & 0xFFFF);

            return i;
        }
#if never

        /// <include file='doc\NativeMethods.uex' path='docs/doc[@for="NativeMethods.SHFILEINFO"]/*' />
        /// <devdoc>
        /// This is a new class used in Imagelist to get the system Imagelist List for Small and Large Icons.
        /// </devdoc>
        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class SHFILEINFO {
            public IntPtr   hIcon;
            public int      iIcon;
            public int      cbWndExtra;
            public int      dwAttributes;
            public string   szDisplayName;
            public string   szTyoeName;
        }


        /// <include file='doc\NativeMethods.uex' path='docs/doc[@for="NativeMethods.FONTDESC"]/*' />
        /// <devdoc>
        /// </devdoc>
        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        public class FONTDESC {
            public int      cbSizeOfStruct = SizeOf();
            public string   lpstrName;
            public long     cySize;
            public short    sWeight;
            public short    sCharset;
            public bool     fItalic;
            public bool     fUnderline;
            public bool     fStrikethrough;
            
            /// <SecurityNote>
            ///  Critical : Calls critical Marshal.SizeOf
            ///  Safe     : Calls method with trusted input (well known safe type)
            /// </SecurityNote>
            private static int SizeOf()
            {
                return Marshal.SizeOf(typeof(FONTDESC));
            }
        }


        /// <include file='doc\NativeMethods.uex' path='docs/doc[@for="NativeMethods.FLASHWINFO"]/*' />
        /// <devdoc>
        /// </devdoc>
        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        public class FLASHWINFO {
            public int      cbSize = SizeOf();
            public IntPtr   hWnd;
            public int      dwFlags;
            public int      uCount;
            public int      dwTimeOut;

            /// <SecurityNote>
            ///  Critical : Calls critical Marshal.SizeOf
            ///  Safe     : Calls method with trusted input (well known safe type)
            /// </SecurityNote>
            private static int SizeOf()
            {
                return Marshal.SizeOf(typeof(FLASHWINFO));
            }
        }

        /// <include file='doc\NativeMethods.uex' path='docs/doc[@for="NativeMethods.PICTDESCbmp"]/*' />
        /// <devdoc>
        /// </devdoc>
        [StructLayout(LayoutKind.Sequential)]
        public class PICTDESCbmp {
            internal int cbSizeOfStruct = SizeOf();
            internal int picType = Ole.PICTYPE_BITMAP;
            internal IntPtr hbitmap = IntPtr.Zero;
            internal IntPtr hpalette = IntPtr.Zero;
            internal int unused = 0;

            public PICTDESCbmp(System.Drawing.Bitmap bitmap) {
                hbitmap = bitmap.GetHbitmap();
                // gpr: What about palettes?
            }
            
            /// <SecurityNote>
            ///  Critical : Calls critical Marshal.SizeOf
            ///  Safe     : Calls method with trusted input (well known safe type)
            /// </SecurityNote>
            private static int SizeOf()
            {
                return Marshal.SizeOf(typeof(PICTDESCbmp));
            }
        }

        /// <include file='doc\NativeMethods.uex' path='docs/doc[@for="NativeMethods.PICTDESCicon"]/*' />
        /// <devdoc>
        /// </devdoc>
        [StructLayout(LayoutKind.Sequential)]
        public class PICTDESCicon {
            internal int cbSizeOfStruct = SizeOf();
            internal int picType = Ole.PICTYPE_ICON;
            internal IntPtr hicon = IntPtr.Zero;
            internal int unused1 = 0;
            internal int unused2 = 0;

            public PICTDESCicon(System.Drawing.Icon icon) {
                hicon = SafeNativeMethods.CopyImage(new HandleRef(icon, icon.Handle), NativeMethods.IMAGE_ICON, icon.Size.Width, icon.Size.Height, 0);
            }
            
            /// <SecurityNote>
            ///  Critical : Calls critical Marshal.SizeOf
            ///  Safe     : Calls method with trusted input (well known safe type)
            /// </SecurityNote>
            private static int SizeOf()
            {
                return Marshal.SizeOf(typeof(PICTDESCicon));
            }
        }

        /// <include file='doc\NativeMethods.uex' path='docs/doc[@for="NativeMethods.PICTDESCemf"]/*' />
        /// <devdoc>
        /// </devdoc>
        [StructLayout(LayoutKind.Sequential)]
        public class PICTDESCemf {
            internal int cbSizeOfStruct = SizeOf();
            internal int picType = Ole.PICTYPE_ENHMETAFILE;
            internal IntPtr hemf = IntPtr.Zero;
            internal int unused1 = 0;
            internal int unused2 = 0;

            public PICTDESCemf(System.Drawing.Imaging.Metafile metafile) {
                //gpr                hemf = metafile.CopyHandle();
            }
            
            /// <SecurityNote>
            ///  Critical : Calls critical Marshal.SizeOf
            ///  Safe     : Calls method with trusted input (well known safe type)
            /// </SecurityNote>
            private static int SizeOf()
            {
                return Marshal.SizeOf(typeof(PICTDESCemf));
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public class USEROBJECTFLAGS {
            public int fInherit = 0;
            public int fReserved = 0;
            public int dwFlags = 0;
        }

        [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Auto)]
        internal class SYSTEMTIMEARRAY {
            public short wYear1;
            public short wMonth1;
            public short wDayOfWeek1;
            public short wDay1;
            public short wHour1;
            public short wMinute1;
            public short wSecond1;
            public short wMilliseconds1;
            public short wYear2;
            public short wMonth2;
            public short wDayOfWeek2;
            public short wDay2;
            public short wHour2;
            public short wMinute2;
            public short wSecond2;
            public short wMilliseconds2;
        }

        public delegate bool EnumChildrenCallback(IntPtr hwnd, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class HH_AKLINK {
            internal int       cbStruct=SizeOf();
            internal bool      fReserved;
            internal string    pszKeywords;
            internal string    pszUrl;
            internal string    pszMsgText;
            internal string    pszMsgTitle;
            internal string    pszWindow;
            internal bool      fIndexOnFail;
            
            /// <SecurityNote>
            ///  Critical : Calls critical Marshal.SizeOf
            ///  Safe     : Calls method with trusted input (well known safe type)
            /// </SecurityNote>
            private static int SizeOf()
            {
                return Marshal.SizeOf(typeof(HH_AKLINK));
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class HH_POPUP {
            internal int       cbStruct=SizeOf();
            internal IntPtr    hinst = IntPtr.Zero;
            internal int       idString = 0;
            internal IntPtr    pszText;
            internal POINT     pt;
            internal int       clrForeground = -1;
            internal int       clrBackground = -1;
            internal RECT      rcMargins = RECT.FromXYWH(-1, -1, -1, -1);     // amount of space between edges of window and text, -1 for each member to ignore
            internal string    pszFont = null;
            
            /// <SecurityNote>
            ///  Critical : Calls critical Marshal.SizeOf
            ///  Safe     : Calls method with trusted input (well known safe type)
            /// </SecurityNote>
            private static int SizeOf()
            {
                return Marshal.SizeOf(typeof(HH_POPUP));
            }
        }

        public static readonly int HH_FTS_DEFAULT_PROXIMITY = -1;

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class HH_FTS_QUERY {
            internal int       cbStruct = SizeOf();
            internal bool      fUniCodeStrings;
            [MarshalAs(UnmanagedType.LPStr)]
            internal string    pszSearchQuery;
            internal int       iProximity = NativeMethods.HH_FTS_DEFAULT_PROXIMITY;
            internal bool      fStemmedSearch;
            internal bool      fTitleOnly;
            internal bool      fExecute = true;
            [MarshalAs(UnmanagedType.LPStr)]
            internal string    pszWindow;
            
            /// <SecurityNote>
            ///  Critical : Calls critical Marshal.SizeOf
            ///  Safe     : Calls method with trusted input (well known safe type)
            /// </SecurityNote>
            private static int SizeOf()
            {
                return Marshal.SizeOf(typeof(HH_FTS_QUERY));
            }
        }
#endif

        [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Auto, Pack=4)]
        public class MONITORINFOEX {
            internal int     cbSize = SizeOf();
            internal RECT    rcMonitor = new RECT();
            internal RECT    rcWork = new RECT();
            internal int     dwFlags = 0;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=32)]
            internal char[]  szDevice = new char[32];
            
            /// <SecurityNote>
            ///  Critical : Calls critical Marshal.SizeOf
            ///  Safe     : Calls method with trusted input (well known safe type)
            /// </SecurityNote>
            private static int SizeOf()
            {
                return Marshal.SizeOf(typeof(MONITORINFOEX));
            }
        }
#if never
        [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Auto, Pack=4)]
        public class MONITORINFO {
            internal int     cbSize = SizeOf();
            internal RECT    rcMonitor = new RECT();
            internal RECT    rcWork = new RECT();
            internal int     dwFlags = 0;
            
            /// <SecurityNote>
            ///  Critical : Calls critical Marshal.SizeOf
            ///  Safe     : Calls method with trusted input (well known safe type)
            /// </SecurityNote>
            private static int SizeOf()
            {
                return Marshal.SizeOf(typeof(MONITORINFO));
            }
        }

        public delegate bool EnumChildrenProc(IntPtr hwnd, IntPtr lParam);
        public delegate int EditStreamCallback(IntPtr dwCookie, IntPtr buf, int cb, out int transferred);

        [StructLayout(LayoutKind.Sequential)]
        public class EDITSTREAM {
            public IntPtr  dwCookie = IntPtr.Zero;
            public int  dwError = 0;
            public EditStreamCallback   pfnCallback = null;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class EDITSTREAM64 {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=20)]
            public byte[] contents = new byte[20];
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct DEVMODE
        {
                private const int CCHDEVICENAME = 32;
                private const int CCHFORMNAME = 32;

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
                public string dmDeviceName;
                public short dmSpecVersion;
                public short dmDriverVersion;
                public short dmSize;
                public short dmDriverExtra;
                public int dmFields;
                public int dmPositionX;
                public int dmPositionY;
                public ScreenOrientation dmDisplayOrientation;
                public int dmDisplayFixedOutput;
                public short dmColor;
                public short dmDuplex;
                public short dmYResolution;
                public short dmTTOption;
                public short dmCollate;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHFORMNAME)]
                public string dmFormName;
                public short dmLogPixels;
                public int dmBitsPerPel;
                public int dmPelsWidth;
                public int dmPelsHeight;
                public int dmDisplayFlags;
                public int dmDisplayFrequency;
                public int dmICMMethod;
                public int dmICMIntent;
                public int dmMediaType;
                public int dmDitherType;
                public int dmReserved1;
                public int dmReserved2;
                public int dmPanningWidth;
                public int dmPanningHeight;
        }

        /// <SecurityNote>
        /// Critical : Elevates to UnmanagedCode permissions
        /// </SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [SuppressUnmanagedCodeSecurity]
		[ComImport(), Guid("0FF510A3-5FA5-49F1-8CCC-190D71083F3E"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IVsPerPropertyBrowsing {
            // hides the property at the given dispid from the properties window
            // implmentors should can return E_NOTIMPL to show all properties that
            // are otherwise browsable.

            [PreserveSig]
            int HideProperty(int dispid,ref bool pfHide);

            // will have the "+" expandable glyph next to them and can be expanded or collapsed by the user
            // Returning a non-S_OK return code or false for pfDisplay will suppress this feature

            [PreserveSig]
            int DisplayChildProperties(int dispid,
                                       ref bool pfDisplay);

            // retrieves the localized name and description for a property.
            // returning a non-S_OK return code will display the default values

            [PreserveSig]
            int GetLocalizedPropertyInfo(int dispid, int localeID,
                                         [Out, MarshalAs(UnmanagedType.LPArray)]
                                         string[] pbstrLocalizedName,
                                         [Out, MarshalAs(UnmanagedType.LPArray)]
                                         string[] pbstrLocalizeDescription);

            // determines if the given (usually current) value for a property is the default.  If it is not default,
            // the property will be shown as bold in the browser to indcate that it has been modified from the default.

            [PreserveSig]
            int HasDefaultValue(int dispid,
                               ref bool fDefault);

            // determines if a property should be made read only.  This only applies to properties that are writeable,
            [PreserveSig]
            int IsPropertyReadOnly(int dispid,
                                   ref bool fReadOnly);


            // returns the classname for this object.  The class name is the non-bolded text that appears in the
            // properties window selection combo.  If this method returns a non-S_OK return code, the default
            // will be used.  The default is the name string from a call to ITypeInfo::GetDocumentation(MEMID_NIL, ...);
            [PreserveSig]
            int GetClassName([In, Out]ref string pbstrClassName);

            // checks whether the given property can be reset to some default value.  If return value is non-S_OK or *pfCanReset is
            //
            [PreserveSig]
            int CanResetPropertyValue(int dispid, [In, Out]ref bool pfCanReset);

            // given property.  If the return value is S_OK, the property's value will then be refreshed to the new default
            // values.
            [PreserveSig]
            int ResetPropertyValue(int dispid);
       }

        /// <SecurityNote>
        /// Critical : Elevates to UnmanagedCode permissions
        /// </SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [SuppressUnmanagedCodeSecurity]
		[ComImport(), Guid("7494683C-37A0-11d2-A273-00C04F8EF4FF"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IManagedPerPropertyBrowsing {


            [PreserveSig]
            int GetPropertyAttributes(int dispid,
                                      ref int  pcAttributes,
                                      ref IntPtr pbstrAttrNames,
                                      ref IntPtr pvariantInitValues);
        }

        /// <SecurityNote>
        /// Critical : Elevates to UnmanagedCode permissions
        /// </SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [SuppressUnmanagedCodeSecurity]
		[ComImport(), Guid("33C0C1D8-33CF-11d3-BFF2-00C04F990235"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IProvidePropertyBuilder {

             [PreserveSig]
             int MapPropertyToBuilder(
                int dispid,
                [In, Out, MarshalAs(UnmanagedType.LPArray)]
                int[] pdwCtlBldType,
                [In, Out, MarshalAs(UnmanagedType.LPArray)]
                string[] pbstrGuidBldr,

            [In, Out, MarshalAs(UnmanagedType.Bool)]
                ref bool builderAvailable);

            [PreserveSig]
            int ExecuteBuilder(
                int dispid,
                [In, MarshalAs(UnmanagedType.BStr)]
                string bstrGuidBldr,
                [In, MarshalAs(UnmanagedType.Interface)]
                object pdispApp,

                HandleRef hwndBldrOwner,
                [Out, In, MarshalAs(UnmanagedType.Struct)]
                ref object pvarValue,
                [In, Out, MarshalAs(UnmanagedType.Bool)]
                ref bool actionCommitted);
        }

        [StructLayout(LayoutKind.Sequential)]
        public class INITCOMMONCONTROLSEX {
            public int  dwSize = SizeOf();
            public int  dwICC;
            
            /// <SecurityNote>
            ///  Critical : Calls critical Marshal.SizeOf
            ///  Safe     : Calls method with trusted input (well known safe type)
            /// </SecurityNote>
            private static int SizeOf()
            {
                return Marshal.SizeOf(typeof(MONITORINFO));
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public class IMAGELISTDRAWPARAMS {
            public int      cbSize = SizeOf();
            public IntPtr   himl;
            public int      i;
            public IntPtr   hdcDst;
            public int      x;
            public int      y;
            public int      cx;
            public int      cy;
            public int      xBitmap;
            public int      yBitmap;
            public int      rgbBk;
            public int      rgbFg;
            public int      fStyle;
            public int      dwRop;
            public int      fState;
            public int      Frame;
            public int      crEffect;
            
            /// <SecurityNote>
            ///  Critical : Calls critical Marshal.SizeOf
            ///  Safe     : Calls method with trusted input (well known safe type)
            /// </SecurityNote>
            private static int SizeOf()
            {
                return Marshal.SizeOf(typeof(IMAGELISTDRAWPARAMS));
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public class IMAGEINFO {
            public IntPtr   hbmImage;
            public IntPtr   hbmMask;
            public int      Unused1;
            public int      Unused2;
            // rcImage was a by-value RECT structure
            public int      rcImage_left;
            public int      rcImage_top;
            public int      rcImage_right;
            public int      rcImage_bottom;
        }
#endif

        [StructLayout(LayoutKind.Sequential)]
        public class TRACKMOUSEEVENT {
                public int      cbSize = SizeOf();
                public int      dwFlags = 0;
                public IntPtr   hwndTrack = IntPtr.Zero;
                public int      dwHoverTime = 100; // Never set this to field ZERO, or to HOVER_DEFAULT, ever!
            
            /// <SecurityNote>
            ///  Critical : Calls critical Marshal.SizeOf
            ///  Safe     : Calls method with trusted input (well known safe type)
            /// </SecurityNote>
            private static int SizeOf()
            {
                return Marshal.SizeOf(typeof(TRACKMOUSEEVENT));
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public class POINT {
            public int x;
            public int y;

            public POINT() {
            }

            public POINT(int x, int y) {
                this.x = x;
                this.y = y;
            }
#if DEBUG
            public override string ToString() {
                return "{x=" + x + ", y=" + y + "}";
            }
#endif
        }

        // use this in cases where the Native API takes a POINT not a POINT*
        // classes marshal by ref.
        [StructLayout(LayoutKind.Sequential)]
        public struct POINTSTRUCT {
            public int x;
            public int y;
            public POINTSTRUCT(int x, int y) {
              this.x = x;
              this.y = y;
            }
      }

        public delegate IntPtr WndProc(IntPtr hWnd, Int32 msg, IntPtr wParam, IntPtr lParam);


#if never
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT {
            public int left;
            public int top;
            public int right;
            public int bottom;

            public RECT(int left, int top, int right, int bottom) {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }

            public RECT(System.Drawing.Rectangle r) {
                this.left = r.Left;
                this.top = r.Top;
                this.right = r.Right;
                this.bottom = r.Bottom;
            }

            public static RECT FromXYWH(int x, int y, int width, int height) {
                return new RECT(x, y, x + width, y + height);
            }

            public System.Drawing.Size Size {
                get {
                    return new System.Drawing.Size(this.right - this.left, this.bottom - this.top);
                }
            }
        }
#endif
        [StructLayout(LayoutKind.Sequential)]
        public struct MARGINS {
            public int cxLeftWidth;
            public int cxRightWidth;
            public int cyTopHeight;
            public int cyBottomHeight;
        }
#if never

        public delegate int ListViewCompareCallback(IntPtr lParam1, IntPtr lParam2, IntPtr lParamSort);

        public delegate int TreeViewCompareCallback(IntPtr lParam1, IntPtr lParam2, IntPtr lParamSort);


        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class WNDCLASS {
            public int style = 0;
            public IntPtr lpfnWndProc = IntPtr.Zero;
            public int cbClsExtra = 0;
            public int cbWndExtra = 0;
            public IntPtr hInstance = IntPtr.Zero;
            public IntPtr hIcon = IntPtr.Zero;
            public IntPtr hCursor = IntPtr.Zero;
            public IntPtr hbrBackground = IntPtr.Zero;
            public string lpszMenuName = null;
            public string lpszClassName = null;
        }
        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class WNDCLASS_I {
            public int style = 0;
            public IntPtr lpfnWndProc = IntPtr.Zero;
            public int cbClsExtra = 0;
            public int cbWndExtra = 0;
            public IntPtr hInstance = IntPtr.Zero;
            public IntPtr hIcon = IntPtr.Zero;
            public IntPtr hCursor = IntPtr.Zero;
            public IntPtr hbrBackground = IntPtr.Zero;
            public IntPtr lpszMenuName = IntPtr.Zero;
            public IntPtr lpszClassName = IntPtr.Zero;
        }
#endif

        [StructLayout(LayoutKind.Sequential)]
        public class NONCLIENTMETRICS {
            public int cbSize = SizeOf();
            public int iBorderWidth = 0;
            public int iScrollWidth = 0;
            public int iScrollHeight = 0;
            public int iCaptionWidth = 0;
            public int iCaptionHeight = 0;
            [MarshalAs(UnmanagedType.Struct)]
            public LOGFONT lfCaptionFont = null;
            public int iSmCaptionWidth = 0;
            public int iSmCaptionHeight = 0;
            [MarshalAs(UnmanagedType.Struct)]
            public LOGFONT lfSmCaptionFont = null;
            public int iMenuWidth = 0;
            public int iMenuHeight = 0;
            [MarshalAs(UnmanagedType.Struct)]
            public LOGFONT lfMenuFont = null;
            [MarshalAs(UnmanagedType.Struct)]
            public LOGFONT lfStatusFont = null;
            [MarshalAs(UnmanagedType.Struct)]
            public LOGFONT lfMessageFont = null;
            
            /// <SecurityNote>
            ///  Critical : Calls critical Marshal.SizeOf
            ///  Safe     : Calls method with trusted input (well known safe type)
            /// </SecurityNote>
            private static int SizeOf()
            {
                return Marshal.SizeOf(typeof(NONCLIENTMETRICS));
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public class ICONMETRICS {
            public int      cbSize = SizeOf();
            public int      iHorzSpacing = 0;
            public int      iVertSpacing = 0;
            public int      iTitleWrap = 0;
            [MarshalAs(UnmanagedType.Struct)]
            public LOGFONT  lfFont = null;
            
            /// <SecurityNote>
            ///  Critical : Calls critical Marshal.SizeOf
            ///  Safe     : Calls method with trusted input (well known safe type)
            /// </SecurityNote>
            private static int SizeOf()
            {
                return Marshal.SizeOf(typeof(ICONMETRICS));
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PAINTSTRUCT {
            public IntPtr   hdc;
            public bool     fErase;
            // rcPaint was a by-value RECT structure
            public int      rcPaint_left;
            public int      rcPaint_top;
            public int      rcPaint_right;
            public int      rcPaint_bottom;
            public bool     fRestore;
            public bool     fIncUpdate;
            public int      reserved1;
            public int      reserved2;
            public int      reserved3;
            public int      reserved4;
            public int      reserved5;
            public int      reserved6;
            public int      reserved7;
            public int      reserved8;
        }
#if never

        [StructLayout(LayoutKind.Sequential)]
        public class SCROLLINFO {
            public int cbSize = SizeOf();
            public int fMask;
            public int nMin;
            public int nMax;
            public int nPage;
            public int nPos;
            public int nTrackPos;

            public SCROLLINFO() {
            }

            public SCROLLINFO(int mask, int min, int max, int page, int pos) {
                fMask = mask;
                nMin = min;
                nMax = max;
                nPage = page;
                nPos = pos;
            }
            
            /// <SecurityNote>
            ///  Critical : Calls critical Marshal.SizeOf
            ///  Safe     : Calls method with trusted input (well known safe type)
            /// </SecurityNote>
            private static int SizeOf()
            {
                return Marshal.SizeOf(typeof(SCROLLINFO));
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public class TPMPARAMS {
            public int  cbSize = SizeOf();
            // rcExclude was a by-value RECT structure
            public int  rcExclude_left;
            public int  rcExclude_top;
            public int  rcExclude_right;
            public int  rcExclude_bottom;
            
            /// <SecurityNote>
            ///  Critical : Calls critical Marshal.SizeOf
            ///  Safe     : Calls method with trusted input (well known safe type)
            /// </SecurityNote>
            [SecuritySafeCritical]
            private static int SizeOf()
            {
                return Marshal.SizeOf(typeof(TPMPARAMS));
            }
        }
#endif

#if FRAMEWORK_NATIVEMETHODS || CORE_NATIVEMETHODS || BASE_NATIVEMETHODS || DRT_SEE_NATIVEMETHODS || UIAUTOMATIONTYPES

        [StructLayout(LayoutKind.Sequential)]
        public class SIZE {
            public int cx;
            public int cy;

            public SIZE()
            {
            }

            public SIZE(int cx, int cy) {
                this.cx = cx;
                this.cy = cy;
            }

        }

#endif

        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWPLACEMENT {
            public int  length;
            public int  flags;
            public int  showCmd;
            // ptMinPosition was a by-value POINT structure
            public int  ptMinPosition_x;
            public int  ptMinPosition_y;
            // ptMaxPosition was a by-value POINT structure
            public int  ptMaxPosition_x;
            public int  ptMaxPosition_y;
            // rcNormalPosition was a by-value RECT structure
            public int  rcNormalPosition_left;
            public int  rcNormalPosition_top;
            public int  rcNormalPosition_right;
            public int  rcNormalPosition_bottom;
        }
#if never

        [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Auto)]
        public class STARTUPINFO {
            public int      cb;
            public string   lpReserved;
            public string   lpDesktop;
            public string   lpTitle;
            public int      dwX;
            public int      dwY;
            public int      dwXSize;
            public int      dwYSize;
            public int      dwXCountChars;
            public int      dwYCountChars;
            public int      dwFillAttribute;
            public int      dwFlags;
            public short    wShowWindow;
            public short    cbReserved2;
            public IntPtr   lpReserved2;
            public IntPtr   hStdInput;
            public IntPtr   hStdOutput;
            public IntPtr   hStdError;
        }

        [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Auto)]
        public class STARTUPINFO_I {
            public int      cb;
            public IntPtr   lpReserved;
            public IntPtr   lpDesktop;
            public IntPtr   lpTitle;
            public int      dwX;
            public int      dwY;
            public int      dwXSize;
            public int      dwYSize;
            public int      dwXCountChars;
            public int      dwYCountChars;
            public int      dwFillAttribute;
            public int      dwFlags;
            public short    wShowWindow;
            public short    cbReserved2;
            public IntPtr   lpReserved2;
            public IntPtr   hStdInput;
            public IntPtr   hStdOutput;
            public IntPtr   hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class PAGESETUPDLG {
            public int      lStructSize;
            public IntPtr   hwndOwner;
            public IntPtr   hDevMode;
            public IntPtr   hDevNames;
            public int      Flags;

            //POINT           ptPaperSize;
            public int      paperSizeX;
            public int      paperSizeY;

            // RECT            rtMinMargin;
            public int      minMarginLeft;
            public int      minMarginTop;
            public int      minMarginRight;
            public int      minMarginBottom;

            // RECT            rtMargin;
            public int      marginLeft;
            public int      marginTop;
            public int      marginRight;
            public int      marginBottom;

            public IntPtr   hInstance;
            public IntPtr   lCustData;
            public WndProc  lpfnPageSetupHook;
            public WndProc  lpfnPagePaintHook;
            public string   lpPageSetupTemplateName;
            public IntPtr   hPageSetupTemplate;
        }



        // x86 requires EXPLICIT packing of 1.
        [StructLayout(LayoutKind.Sequential, Pack=1, CharSet=CharSet.Auto)]
        public class PRINTDLG {
            public   int    lStructSize;

            public   IntPtr hwndOwner;
            public   IntPtr hDevMode;
            public   IntPtr hDevNames;
            public   IntPtr hDC;

            public   int    Flags;

            public   short  nFromPage;
            public   short  nToPage;
            public   short  nMinPage;
            public   short  nMaxPage;
            public   short  nCopies;

            public   IntPtr hInstance;
            public   IntPtr lCustData;

            public   WndProc lpfnPrintHook;
            public   WndProc lpfnSetupHook;

            public   string  lpPrintTemplateName;
            public   string  lpSetupTemplateName;

            public   IntPtr hPrintTemplate;
            public   IntPtr hSetupTemplate;
        }


        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class PRINTDLGEX {
            public   int        lStructSize;

            public   IntPtr     hwndOwner;
            public   IntPtr     hDevMode;
            public   IntPtr     hDevNames;
            public   IntPtr     hDC;

            public   int        Flags;
            public   int        Flags2;

            public   int        ExclusionFlags;

            public   int        nPageRanges;
            public   int        nMaxPageRanges;

            public   IntPtr     pageRanges;

            public   int        nMinPage;
            public   int        nMaxPage;
            public   int        nCopies;

            public   IntPtr     hInstance;
            public   string     lpPrintTemplateName;

            public   WndProc    lpCallback;

            public   int        nPropertyPages;

            public   IntPtr     lphPropertyPages;

            public   int        nStartPage;
            public   int        dwResultAction;

        }

        // x86 requires EXPLICIT packing of 1.
        [StructLayout(LayoutKind.Sequential, Pack=1, CharSet=CharSet.Auto)]
        public class PRINTPAGERANGE {
            public int nFromPage;
            public int nToPage;
        }



        [StructLayout(LayoutKind.Sequential)]
        public class PICTDESC
        {
            internal int cbSizeOfStruct;
            public int picType;
            internal IntPtr union1;
            internal int union2;
            internal int union3;

            public static PICTDESC CreateBitmapPICTDESC(IntPtr hbitmap, IntPtr hpal) {
                PICTDESC pictdesc = new PICTDESC();
                pictdesc.cbSizeOfStruct = 16;
                pictdesc.picType = Ole.PICTYPE_BITMAP;
                pictdesc.union1 = hbitmap;
                pictdesc.union2 = (int)(((long)hpal) & 0xffffffff);
                pictdesc.union3 = (int)(((long)hpal) >> 32);
                return pictdesc;
            }

            public static PICTDESC CreateIconPICTDESC(IntPtr hicon) {
                PICTDESC pictdesc = new PICTDESC();
                pictdesc.cbSizeOfStruct = 12;
                pictdesc.picType = Ole.PICTYPE_ICON;
                pictdesc.union1 = hicon;
                return pictdesc;
            }

            public static PICTDESC CreateEnhMetafilePICTDESC(IntPtr hEMF) {
                PICTDESC pictdesc = new PICTDESC();
                pictdesc.cbSizeOfStruct = 12;
                pictdesc.picType = Ole.PICTYPE_ENHMETAFILE;
                pictdesc.union1 = hEMF;
                return pictdesc;
            }

            public static PICTDESC CreateWinMetafilePICTDESC(IntPtr hmetafile, int x, int y) {
                PICTDESC pictdesc = new PICTDESC();
                pictdesc.cbSizeOfStruct = 20;
                pictdesc.picType = Ole.PICTYPE_METAFILE;
                pictdesc.union1 = hmetafile;
                pictdesc.union2 = x;
                pictdesc.union3 = y;
                return pictdesc;
            }

            public virtual IntPtr GetHandle() {
                return union1;
            }

            public virtual IntPtr GetHPal() {
                if (picType == Ole.PICTYPE_BITMAP)
                    return (IntPtr)((uint)union2 | (((long)union3) << 32));
                else
                    return IntPtr.Zero;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public  sealed class tagFONTDESC {
             public uint cbSizeofstruct = SizeOf();

             [MarshalAs(UnmanagedType.LPWStr)]
             public string lpstrName;

             [MarshalAs(UnmanagedType.I8)]
             public long cySize;

             [MarshalAs(UnmanagedType.I2)]
             public short sWeight;

             [MarshalAs(UnmanagedType.I2)]
             public short sCharset;

             [MarshalAs(UnmanagedType.Bool)]
             public bool  fItalic;

             [MarshalAs(UnmanagedType.Bool)]
             public bool  fUnderline;

             [MarshalAs(UnmanagedType.Bool)]
             public bool fStrikethrough;
             
            /// <SecurityNote>
            ///  Critical : Calls critical Marshal.SizeOf
            ///  Safe     : Calls method with trusted input (well known safe type)
            /// </SecurityNote>
            [SecuritySafeCritical]
            private static int SizeOf()
            {
                return Marshal.SizeOf(typeof(tagFONTDESC));
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class MMCKINFO {
            public int      ckID;
            public int      cksize;
            public int      fccType;
            public int      dwDataOffset;
            public int      dwFlags;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class WAVEFORMATEX {
            public System.Int16     wFormatTag;
            public System.Int16     nChannels;
            public int              nSamplesPerSec;
            public int              nAvgBytesPerSec;
            public System.Int16     nBlockAlign;
            public System.Int16     wBitsPerSample;
            public System.Int16     cbSize;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class CHOOSECOLOR {
            public int      lStructSize = SizeOf(); //ndirect.DllLib.sizeOf(this);
            public IntPtr   hwndOwner;
            public IntPtr   hInstance;
            public int      rgbResult;
            public IntPtr   lpCustColors;
            public int      Flags;
            public IntPtr   lCustData;
            public WndProc  lpfnHook;
            public string   lpTemplateName;
            
            /// <SecurityNote>
            ///  Critical : Calls critical Marshal.SizeOf
            ///  Safe     : Calls method with trusted input (well known safe type)
            /// </SecurityNote>
            [SecuritySafeCritical]
            private static int SizeOf()
            {
                return Marshal.SizeOf(typeof(CHOOSECOLOR));
            }
        }

        //public delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);
#endif

        [StructLayout(LayoutKind.Sequential)]
        public class BITMAP {
            public int bmType = 0;
            public int bmWidth = 0;
            public int bmHeight = 0;
            public int bmWidthBytes = 0;
            public short bmPlanes = 0;
            public short bmBitsPixel = 0;
            public int bmBits = 0;
        }


#if NEVER
        [StructLayout(LayoutKind.Sequential)]
        public class ICONINFO {
                public int fIcon;
                public int xHotspot;
                public int yHotspot;
                public IntPtr hbmMask;
                public IntPtr hbmColor;
        }


        [StructLayout(LayoutKind.Sequential)]
        public class DIBSECTION {
            public BITMAP dsBm;
            public BITMAPINFOHEADER dsBmih;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=3)]
            public int[] dsBitfields;
            public IntPtr dshSection;
            public int dsOffset;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class LOGPEN {
            public int  lopnStyle;
            // lopnWidth was a by-value POINT structure
            public int  lopnWidth_x;
            public int  lopnWidth_y;
            public int  lopnColor;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class LOGBRUSH {
                public int lbStyle;
                public int lbColor;
                public IntPtr lbHatch;
        }
#endif

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        public class LOGFONT {
            public LOGFONT() {
            }
            public LOGFONT( LOGFONT lf )
            {
                if (lf == null)
                {
                    throw new ArgumentNullException("lf");
                }

                this.lfHeight           = lf.lfHeight;
                this.lfWidth            = lf.lfWidth;
                this.lfEscapement       = lf.lfEscapement;
                this.lfOrientation      = lf.lfOrientation;
                this.lfWeight           = lf.lfWeight;
                this.lfItalic           = lf.lfItalic;
                this.lfUnderline        = lf.lfUnderline;
                this.lfStrikeOut        = lf.lfStrikeOut;
                this.lfCharSet          = lf.lfCharSet;
                this.lfOutPrecision     = lf.lfOutPrecision;
                this.lfClipPrecision    = lf.lfClipPrecision;
                this.lfQuality          = lf.lfQuality;
                this.lfPitchAndFamily   = lf.lfPitchAndFamily;
                this.lfFaceName         = lf.lfFaceName;
            }
            public int lfHeight;
            public int lfWidth;
            public int lfEscapement;
            public int lfOrientation;
            public int lfWeight;
            public byte lfItalic;
            public byte lfUnderline;
            public byte lfStrikeOut;
            public byte lfCharSet;
            public byte lfOutPrecision;
            public byte lfClipPrecision;
            public byte lfQuality;
            public byte lfPitchAndFamily;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=32)]
            public string   lfFaceName;
        }

#if NEVER
        [StructLayout(LayoutKind.Sequential)]
        public class LOGPALETTE {
            public short palVersion;
            public short palNumEntries;
            public int palPalEntry;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        public struct TEXTMETRIC
        {
            public int  tmHeight;
            public int  tmAscent;
            public int  tmDescent;
            public int  tmInternalLeading;
            public int  tmExternalLeading;
            public int  tmAveCharWidth;
            public int  tmMaxCharWidth;
            public int  tmWeight;
            public int  tmOverhang;
            public int  tmDigitizedAspectX;
            public int  tmDigitizedAspectY;
            public char tmFirstChar;
            public char tmLastChar;
            public char tmDefaultChar;
            public char tmBreakChar;
            public byte tmItalic;
            public byte tmUnderlined;
            public byte tmStruckOut;
            public byte tmPitchAndFamily;
            public byte tmCharSet;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Ansi)]
        public struct TEXTMETRICA
        {
            public int  tmHeight;
            public int  tmAscent;
            public int  tmDescent;
            public int  tmInternalLeading;
            public int  tmExternalLeading;
            public int  tmAveCharWidth;
            public int  tmMaxCharWidth;
            public int  tmWeight;
            public int  tmOverhang;
            public int  tmDigitizedAspectX;
            public int  tmDigitizedAspectY;
            public byte tmFirstChar;
            public byte tmLastChar;
            public byte tmDefaultChar;
            public byte tmBreakChar;
            public byte tmItalic;
            public byte tmUnderlined;
            public byte tmStruckOut;
            public byte tmPitchAndFamily;
            public byte tmCharSet;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class NOTIFYICONDATA {
            public int      cbSize = SizeOf();
            public IntPtr   hWnd;
            public int      uID;
            public int      uFlags;
            public int      uCallbackMessage;
            public IntPtr   hIcon;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=64)]
            public string   szTip;
            
            /// <SecurityNote>
            ///  Critical : Calls critical Marshal.SizeOf
            ///  Safe     : Calls method with trusted input (well known safe type)
            /// </SecurityNote>
            [SecuritySafeCritical]
            private static int SizeOf()
            {
                return Marshal.SizeOf(typeof(NOTIFYICONDATA));
            }
        }
#endif

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class MENUITEMINFO_T
        {
            public int cbSize = SizeOf();
            public int fMask = 0;
            public int fType = 0;
            public int fState = 0;
            public int wID = 0;
            public IntPtr hSubMenu = IntPtr.Zero;
            public IntPtr hbmpChecked = IntPtr.Zero;
            public IntPtr hbmpUnchecked = IntPtr.Zero;
            public int dwItemData = 0;
            public string dwTypeData = null;
            public int cch = 0;
            
            /// <SecurityNote>
            ///  Critical : Calls critical Marshal.SizeOf
            ///  Safe     : Calls method with trusted input (well known safe type)
            /// </SecurityNote>
            [SecuritySafeCritical]
            private static int SizeOf()
            {
                return Marshal.SizeOf(typeof(MENUITEMINFO_T));
            }
        }
#if never

        // This version allows you to read the string that's stuffed
        // in the native menu item.  You have to do the marshaling on
        // your own though.
        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class MENUITEMINFO_T_RW
        {
            public int      cbSize = SizeOf();
            public int      fMask;
            public int      fType;
            public int      fState;
            public int      wID;
            public IntPtr   hSubMenu;
            public IntPtr   hbmpChecked;
            public IntPtr   hbmpUnchecked;
            public IntPtr   dwItemData;
            public IntPtr   dwTypeData;
            public int      cch;
            public IntPtr   hbmpItem;  // requires WINVER > 5
            
            /// <SecurityNote>
            ///  Critical : Calls critical Marshal.SizeOf
            ///  Safe     : Calls method with trusted input (well known safe type)
            /// </SecurityNote>
            [SecuritySafeCritical]
            private static int SizeOf()
            {
                return Marshal.SizeOf(typeof(MENUITEMINFO_T_RW));
            }        
        }



        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        public struct MSAAMENUINFO
        {
            public int dwMSAASignature;
            public int cchWText;
            public string pszWText;

            public MSAAMENUINFO(string text) {
                dwMSAASignature = unchecked((int) MSAA_MENU_SIG);
                cchWText = text.Length;
                pszWText = text;
            }
        }
#endif

        public delegate bool EnumThreadWindowsCallback(IntPtr hWnd, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        internal class OFNOTIFY
        {
            // hdr was a by-value NMHDR structure
            internal IntPtr hdr_hwndFrom;
            internal IntPtr hdr_idFrom;
            internal int hdr_code;

            internal IntPtr lpOFN;
            internal IntPtr pszFile;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        public class OPENFILENAME_I
        {
            public int      lStructSize = SizeOf(); //ndirect.DllLib.sizeOf(this);
            public IntPtr   hwndOwner;
            public IntPtr   hInstance;
            public string   lpstrFilter;   // use embedded nulls to separate filters
            public IntPtr   lpstrCustomFilter;
            public int      nMaxCustFilter;
            public int      nFilterIndex;
            public IntPtr   lpstrFile;
            public int      nMaxFile = NativeMethods.MAX_PATH;
            public IntPtr   lpstrFileTitle;
            public int      nMaxFileTitle = NativeMethods.MAX_PATH;
            public string   lpstrInitialDir;
            public string   lpstrTitle;
            public int      Flags;
            public short    nFileOffset;
            public short    nFileExtension;
            public string   lpstrDefExt;
            public IntPtr   lCustData;
            public WndProc  lpfnHook;
            public string   lpTemplateName;
            public IntPtr   pvReserved;
            public int      dwReserved;
            public int      FlagsEx;
            
            /// <SecurityNote>
            ///  Critical : Calls critical Marshal.SizeOf
            ///  Safe     : Calls method with trusted input (well known safe type)
            /// </SecurityNote>
            [SecuritySafeCritical]
            private static int SizeOf()
            {
                return Marshal.SizeOf(typeof(OPENFILENAME_I));
            }        
        }

        // constants related to the OPENFILENAME structure and file open/save dialogs
        public const int CDN_FIRST          = unchecked((int)(0U-601U));

        public const int CDN_INITDONE       = (CDN_FIRST - 0x0000);
        public const int CDN_SELCHANGE      = (CDN_FIRST - 0x0001);
        public const int CDN_SHAREVIOLATION = (CDN_FIRST - 0x0003);
        public const int CDN_FILEOK         = (CDN_FIRST - 0x0005);

#if !DRT && !UIAUTOMATIONTYPES
        public const int CDM_FIRST          = (int)WindowMessage.WM_USER + 100;

        public const int CDM_GETSPEC        = (CDM_FIRST + 0x0000);
        public const int CDM_GETFILEPATH    = (CDM_FIRST + 0x0001);
#endif

        public const int DWL_MSGRESULT = 0;

        [StructLayout(LayoutKind.Sequential)]
        public struct STYLESTRUCT {
            public int styleOld;
            public int styleNew;
        }

#if never
        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto), CLSCompliantAttribute(false)]
        public class CHOOSEFONT {
            public int      lStructSize = SizeOf();   // ndirect.DllLib.sizeOf(this);
            public IntPtr   hwndOwner;
            public IntPtr   hDC;
            public IntPtr   lpLogFont;
            public int      iPointSize;
            public int      Flags;
            public int      rgbColors;
            public IntPtr   lCustData;
            public WndProc  lpfnHook;
            public string   lpTemplateName;
            public IntPtr   hInstance;
            public string   lpszStyle;
            public short    nFontType;
            public short    ___MISSING_ALIGNMENT__;
            public int      nSizeMin;
            public int      nSizeMax;
            
            /// <SecurityNote>
            ///  Critical : Calls critical Marshal.SizeOf
            ///  Safe     : Calls method with trusted input (well known safe type)
            /// </SecurityNote>
            [SecuritySafeCritical]
            private static int SizeOf()
            {
                return Marshal.SizeOf(typeof(CHOOSEFONT));
            }        
        }

        [StructLayout(LayoutKind.Sequential)]
        public class BITMAPINFO {
            // bmiHeader was a by-value BITMAPINFOHEADER structure
            public int      bmiHeader_biSize = 40;  // ndirect.DllLib.sizeOf( BITMAPINFOHEADER.class );
            public int      bmiHeader_biWidth;
            public int      bmiHeader_biHeight;
            public short    bmiHeader_biPlanes;
            public short    bmiHeader_biBitCount;
            public int      bmiHeader_biCompression;
            public int      bmiHeader_biSizeImage;
            public int      bmiHeader_biXPelsPerMeter;
            public int      bmiHeader_biYPelsPerMeter;
            public int      bmiHeader_biClrUsed;
            public int      bmiHeader_biClrImportant;

            // bmiColors was an embedded array of RGBQUAD structures
            public byte     bmiColors_rgbBlue;
            public byte     bmiColors_rgbGreen;
            public byte     bmiColors_rgbRed;
            public byte     bmiColors_rgbReserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class BITMAPINFOHEADER {
            public int biSize = 40;    // ndirect.DllLib.sizeOf( this );
            public int biWidth = 0;
            public int biHeight = 0;
            public short biPlanes = 0;
            public short biBitCount = 0;
            public int biCompression = 0;
            public int biSizeImage = 0;
            public int biXPelsPerMeter = 0;
            public int biYPelsPerMeter = 0;
            public int biClrUsed = 0;
            public int biClrImportant = 0;
        }


        public class Ole {
            public const int PICTYPE_UNINITIALIZED = -1;
            public const int PICTYPE_NONE          =  0;
            public const int PICTYPE_BITMAP        =  1;
            public const int PICTYPE_METAFILE      =  2;
            public const int PICTYPE_ICON          =  3;
            public const int PICTYPE_ENHMETAFILE   =  4;
            public const int STATFLAG_DEFAULT = 0;
            public const int STATFLAG_NONAME = 1;
        }

#endif
        [StructLayout(LayoutKind.Sequential)]
        public class STATSTG
        {

            [MarshalAs(UnmanagedType.LPWStr)]
            public string pwcsName = null;

            public int type = 0;
            [MarshalAs(UnmanagedType.I8)]
            public long cbSize = 0;
            [MarshalAs(UnmanagedType.I8)]
            public long mtime = 0;
            [MarshalAs(UnmanagedType.I8)]
            public long ctime = 0;
            [MarshalAs(UnmanagedType.I8)]
            public long atime = 0;
            [MarshalAs(UnmanagedType.I4)]
            public int grfMode = 0;
            [MarshalAs(UnmanagedType.I4)]
            public int grfLocksSupported = 0;

            public int clsid_data1 = 0;
            [MarshalAs(UnmanagedType.I2)]
            public short clsid_data2 = 0;
            [MarshalAs(UnmanagedType.I2)]
            public short clsid_data3 = 0;
            [MarshalAs(UnmanagedType.U1)]
            public byte clsid_b0 = 0;
            [MarshalAs(UnmanagedType.U1)]
            public byte clsid_b1 = 0;
            [MarshalAs(UnmanagedType.U1)]
            public byte clsid_b2 = 0;
            [MarshalAs(UnmanagedType.U1)]
            public byte clsid_b3 = 0;
            [MarshalAs(UnmanagedType.U1)]
            public byte clsid_b4 = 0;
            [MarshalAs(UnmanagedType.U1)]
            public byte clsid_b5 = 0;
            [MarshalAs(UnmanagedType.U1)]
            public byte clsid_b6 = 0;
            [MarshalAs(UnmanagedType.U1)]
            public byte clsid_b7 = 0;
            [MarshalAs(UnmanagedType.I4)]
            public int grfStateBits = 0;
            [MarshalAs(UnmanagedType.I4)]
            public int reserved = 0;
        }
#if never

        [StructLayout(LayoutKind.Sequential)]
        public class FILETIME {
            public int dwLowDateTime;
            public int dwHighDateTime;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class OVERLAPPED {
            public int Internal;
            public int InternalHigh;
            public int Offset;
            public int OffsetHigh;
            public IntPtr hEvent;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class SYSTEMTIME {
            public short wYear;
            public short wMonth;
            public short wDayOfWeek;
            public short wDay;
            public short wHour;
            public short wMinute;
            public short wSecond;
            public short wMilliseconds;

            public override string ToString() {
                return "[SYSTEMTIME: "
                + wDay.ToString() +"/" + wMonth.ToString() + "/" + wYear.ToString()
                + " " + wHour.ToString() + ":" + wMinute.ToString() + ":" + wSecond.ToString()
                + "]";
            }
        }

        [
        StructLayout(LayoutKind.Sequential),
        CLSCompliantAttribute(false)
        ]
        public sealed class  _POINTL {
            public   int x;
            public   int y;

        }

        [StructLayout(LayoutKind.Sequential)]
        public sealed class tagSIZE {
            public   int cx;
            public   int cy;

        }
#endif

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public class COMRECT {
            public int left;
            public int top;
            public int right;
            public int bottom;

            public COMRECT(int x, int y, int right, int bottom)
            {
                this.left = x;
                this.top = y;
                this.right = right;
                this.bottom = bottom;
            }

//            public COMRECT(System.Drawing.Rectangle r) {
//                this.left = r.X;
//                this.top = r.Y;
//                this.right = r.Right;
//                this.bottom = r.Bottom;
//            }

            public COMRECT(RECT rect) {
                this.left = rect.left;
                this.top = rect.top;
                this.bottom = rect.bottom;
                this.right = rect.right;
            }

            public void CopyTo(COMRECT destRect) {
                destRect.left = left;
                destRect.right = right;
                destRect.top = top;
                destRect.bottom = bottom;
            }

            public bool IsEmpty { get { return left == right && top == bottom; } }

//            public RECT ToRECT() {
//                return new RECT(left, top, right, bottom);
//            }

            public override string ToString() {
                return "Left = " + left + " Top " + top + " Right = " + right + " Bottom = " + bottom;
            }
        }

        [StructLayout(LayoutKind.Sequential)/*leftover(noAutoOffset)*/]
        public sealed class tagOleMenuGroupWidths {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=6)/*leftover(offset=0, widths)*/]
            public int[] widths = new int[6];
        }

#if never
        [StructLayout(LayoutKind.Sequential)]
        [Serializable]
        public class MSOCRINFOSTRUCT {
            public int cbSize = SizeOf();              // size of MSOCRINFO structure in bytes.
            public int uIdleTimeInterval;   // If olecrfNeedPeriodicIdleTime is registered
                                            // in grfcrf, component needs to perform
                                            // periodic idle time tasks during an idle phase
                                            // every uIdleTimeInterval milliseconds.
            public int grfcrf;              // bit flags taken from olecrf values (above)
            public int grfcadvf;            // bit flags taken from olecadvf values (above)
            
            /// <SecurityNote>
            ///  Critical : Calls critical Marshal.SizeOf
            ///  Safe     : Calls method with trusted input (well known safe type)
            /// </SecurityNote>
            [SecuritySafeCritical]
            private static int SizeOf()
            {
                return Marshal.SizeOf(typeof(MSOCRINFOSTRUCT));
            }        
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NMLISTVIEW
        {
            public NMHDR hdr;
            public int   iItem;
            public int   iSubItem;
            public int   uNewState;
            public int   uOldState;
            public int   uChanged;
            public IntPtr lParam;
        }

        public class ConnectionPointCookie
        {
            private UnsafeNativeMethods.IConnectionPoint connectionPoint;
            private int cookie;
            private static bool connected;
            #if DEBUG
            private string callStack;
            #endif

            /// <include file='doc\NativeMethods.uex' path='docs/doc[@for="NativeMethods.ConnectionPointCookie.ConnectionPointCookie"]/*' />
            /// <devdoc>
            /// Creates a connection point to of the given interface type.
            /// which will call on a managed code sink that implements that interface.
            /// </devdoc>
            public ConnectionPointCookie(object source, object sink, Type eventInterface) : this(source, sink, eventInterface, true, out connected){
            }

            /// <include file='doc\NativeMethods.uex' path='docs/doc[@for="NativeMethods.ConnectionPointCookie.ConnectionPointCookie1"]/*' />
            /// <devdoc>
            /// Creates a connection point to of the given interface type.
            /// which will call on a managed code sink that implements that interface.
            /// </devdoc>
            public ConnectionPointCookie(object source, object sink, Type eventInterface, bool throwException, out bool connected){
                connected = false;
                Exception ex = null;
                if (source is UnsafeNativeMethods.IConnectionPointContainer) {
                    UnsafeNativeMethods.IConnectionPointContainer cpc = (UnsafeNativeMethods.IConnectionPointContainer)source;

                    try {
                        Guid tmp = eventInterface.GUID;
                        if (cpc.FindConnectionPoint(ref tmp, out connectionPoint) != NativeMethods.S_OK) {
                            connectionPoint = null;
                        }
                    }
                    catch (Exception) {
                        connectionPoint = null;
                    }

                    if (connectionPoint == null) {
                        ex = new ArgumentException(SR.GetString(SR.ConnPointSourceIF, eventInterface.Name ));
                    }
                    else if (sink == null || !eventInterface.IsInstanceOfType(sink)) {
                        ex = new InvalidCastException(SR.GetString(SR.ConnPointSinkIF));
                    }
                    else {
                        int hr = connectionPoint.Advise(sink, ref cookie);
                        if (hr != S_OK) {
                            cookie = 0;
                            Marshal.ReleaseComObject(connectionPoint);
                            connectionPoint = null;
                            ex = new ExternalException(SR.GetString(SR.ConnPointAdviseFailed, eventInterface.Name, hr ));
                        }
                        else {
                            connected = true;
                        }
                    }
                }
                else {
                    ex = new InvalidCastException(SR.GetString(SR.ConnPointSourceIF, "IConnectionPointContainer"));
                }


                if (throwException && (connectionPoint == null || cookie == 0)) {
                    if (connectionPoint != null) {
                        Marshal.ReleaseComObject(connectionPoint);
                    }

                    if (ex == null) {
                        throw new ArgumentException(SR.GetString(SR.ConnPointCouldNotCreate, eventInterface.Name ));
                    }
                    else {
                        throw ex;
                    }
                }

                #if DEBUG
                new EnvironmentPermission(PermissionState.Unrestricted).Assert();
                try {
                    callStack = Environment.StackTrace;
                }
                finally {
                    System.Security.CodeAccessPermission.RevertAssert();
                }
                #endif
            }

            /// <include file='doc\NativeMethods.uex' path='docs/doc[@for="NativeMethods.ConnectionPointCookie.Disconnect"]/*' />
            /// <devdoc>
            /// Disconnect the current connection point.  If the object is not connected,
            /// this method will do nothing.
            /// </devdoc>
            public void Disconnect() {
                Disconnect(false);
            }

            /// <include file='doc\NativeMethods.uex' path='docs/doc[@for="NativeMethods.ConnectionPointCookie.Disconnect1"]/*' />
            /// <devdoc>
            /// Disconnect the current connection point.  If the object is not connected,
            /// this method will do nothing.
            /// </devdoc>
            public void Disconnect(bool release) {
                if (connectionPoint != null && cookie != 0) {
                    connectionPoint.Unadvise(cookie);
                    cookie = 0;

                    if (release) {
                        Marshal.ReleaseComObject(connectionPoint);
                    }

                    connectionPoint = null;
                }
            }

            /// <include file='doc\NativeMethods.uex' path='docs/doc[@for="NativeMethods.ConnectionPointCookie.Finalize"]/*' />
            /// <internalonly/>
            ~ConnectionPointCookie(){
                //System.Diagnostics.Debug.Assert(connectionPoint == null || cookie == 0, "We should never finalize an active connection point");
                //Disconnect();
            }
        }
#endif

        [StructLayout(LayoutKind.Sequential)/*leftover(noAutoOffset)*/]
        public sealed class POINTF
        {
          [MarshalAs(UnmanagedType.R4)/*leftover(offset=0, x)*/]
          public float x;

          [MarshalAs(UnmanagedType.R4)/*leftover(offset=4, y)*/]
          public float y;

        }

        [StructLayout(LayoutKind.Sequential)/*leftover(noAutoOffset)*/]
        public sealed class OLEINPLACEFRAMEINFO
        {
          [MarshalAs(UnmanagedType.U4)/*leftover(offset=0, cb)*/]
          public uint cb;

          public bool fMDIApp;
          public IntPtr hwndFrame;
          public IntPtr hAccel;

          [MarshalAs(UnmanagedType.U4)/*leftover(offset=16, cAccelEntries)*/]
          public uint cAccelEntries;

        }

#if never
        [StructLayout(LayoutKind.Sequential)]
        public struct NMLVDISPINFO_UNSAFE
        {
            // public NMHDR  hdr;
            public IntPtr hwndFrom;
            public IntPtr idFrom;
            public int code;
            // public LVITEM item;
            public int      mask;
            public int      iItem;
            public int      iSubItem;
            public int      state;
            public int      stateMask;
            public IntPtr   pszText;
            public int      cchTextMax;
            public int      iImage;
            public IntPtr   lParam;
            public int      iIndent;
            public int      iGroupId;
            public int      cColumns; // tile view columns
            public IntPtr   puColumns;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NMHDR
        {
            public IntPtr hwndFrom;
            public IntPtr idFrom; //This is declared as UINT_PTR in winuser.h
            public int code;
        }

        /// <SecurityNote>
        /// Critical : Elevates to UnmanagedCode permissions
        /// </SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [SuppressUnmanagedCodeSecurity]
		[ComImport(), Guid("376BD3AA-3845-101B-84ED-08002B2EC713"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IPerPropertyBrowsing {
             [PreserveSig]
             int GetDisplayString(
                int dispID,
                [Out, MarshalAs(UnmanagedType.LPArray)]
                string[] pBstr);

             [PreserveSig]
             int MapPropertyToPage(
                int dispID,
                [Out]
                out Guid pGuid);

             [PreserveSig]
             int GetPredefinedStrings(
                int dispID,
                [Out]
                CA_STRUCT pCaStringsOut,
                [Out]
                CA_STRUCT pCaCookiesOut);

             [PreserveSig]
             int GetPredefinedValue(
                int dispID,
                [In, MarshalAs(UnmanagedType.U4)]
                uint dwCookie,
                [Out]
                VARIANT pVarOut);
        }

        /// <SecurityNote>
        /// Critical : Elevates to UnmanagedCode permissions
        /// </SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [SuppressUnmanagedCodeSecurity]
		[ComImport(), Guid("4D07FC10-F931-11CE-B001-00AA006884E5"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        public interface ICategorizeProperties {

             [PreserveSig]
             int MapPropertyToCategory(
                int dispID,
                ref int categoryID);

             [PreserveSig]
             int GetCategoryName(
                int propcat,
                [In, MarshalAs(UnmanagedType.U4)]
                uint lcid,
                out string categoryName);
        }

        [StructLayout(LayoutKind.Sequential)/*leftover(noAutoOffset)*/]
        public sealed class tagSIZEL
        {
            public int cx = 0;
            public int cy = 0;
        }
#endif

        [StructLayout(LayoutKind.Sequential)/*leftover(noAutoOffset)*/]
        public sealed class tagOLEVERB
        {
            public int lVerb = 0;

            [MarshalAs(UnmanagedType.LPWStr)] // leftover(offset=4, customMarshal="UniStringMarshaller", lpszVerbName)
            public string lpszVerbName = null;

            [MarshalAs(UnmanagedType.U4)] // leftover(offset=8, fuFlags)
            public uint fuFlags = 0;

            [MarshalAs(UnmanagedType.U4)] // leftover(offset=12, grfAttribs)
            public uint grfAttribs = 0;
        }

        [StructLayout(LayoutKind.Sequential)/*leftover(noAutoOffset)*/]
        public sealed class tagLOGPALETTE
        {
            [MarshalAs(UnmanagedType.U2)] // leftover(offset=0, palVersion)
            public ushort palVersion = 0;

            [MarshalAs(UnmanagedType.U2)] // leftover(offset=2, palNumEntries)
            public ushort palNumEntries = 0;
        }

        [StructLayout(LayoutKind.Sequential)/*leftover(noAutoOffset)*/]
        public sealed class tagCONTROLINFO
        {
            [MarshalAs(UnmanagedType.U4)/*leftover(offset=0, cb)*/]
            public uint cb = (uint)SizeOf();

            public IntPtr hAccel;

            [MarshalAs(UnmanagedType.U2)/*leftover(offset=8, cAccel)*/]
            public ushort cAccel;

            [MarshalAs(UnmanagedType.U4)/*leftover(offset=10, dwFlags)*/]
            public uint dwFlags;
            
            /// <SecurityNote>
            ///  Critical : Calls critical Marshal.SizeOf
            ///  Safe     : Calls method with trusted input (well known safe type)
            /// </SecurityNote>
            [SecuritySafeCritical]
            private static int SizeOf()
            {
                return Marshal.SizeOf(typeof(tagCONTROLINFO));
            }        
        }
#if never
        [StructLayout(LayoutKind.Sequential)/*leftover(noAutoOffset)*/]
        public sealed class CA_STRUCT
        {
            public int cElems;
            public IntPtr pElems;
        }
#endif
        [StructLayout(LayoutKind.Sequential)]
        public sealed class VARIANT {
            [MarshalAs(UnmanagedType.I2)]
            public short vt;
            [MarshalAs(UnmanagedType.I2)]
            public short reserved1;
            [MarshalAs(UnmanagedType.I2)]
            public short reserved2;
            [MarshalAs(UnmanagedType.I2)]
            public short reserved3;

            /// <SecurityNote>
            ///     Critical: This data is critical for set because it is used to make calls to Marshal.*
            /// </SecurityNote>
            public SecurityCriticalDataForSet<IntPtr> data1;

            /// <SecurityNote>
            ///     Critical: This data is critical for set because it is used to make calls to Marshal.*
            /// </SecurityNote>
            public SecurityCriticalDataForSet<IntPtr> data2;


            public bool Byref{
                get{
                    return 0 != (vt & (int)tagVT.VT_BYREF);
                }
            }

            /// <SecurityNote>
            ///     Critical: This calls into Marshal.Release which is link demand protected
            ///     TreatAsSafe: This is instance based and the internal pointer it is releasing is critical for set
            /// </SecurityNote>
            [SecurityCritical, SecurityTreatAsSafe]
            public void Clear() {
                if ((this.vt == (int)tagVT.VT_UNKNOWN || this.vt == (int)tagVT.VT_DISPATCH) && this.data1.Value != IntPtr.Zero) {
                    Marshal.Release(this.data1.Value);
                }

                if (this.vt == (int)tagVT.VT_BSTR && this.data1.Value != IntPtr.Zero) {
                    SysFreeString(this.data1.Value);
                }

                this.data1.Value = this.data2.Value = IntPtr.Zero;
                this.vt = (int)tagVT.VT_EMPTY;
            }

            ~VARIANT() {
                Clear();
            }

            public void SuppressFinalize()
            {
                // Called if this VARIANT is returned to the caller in native world which is supposed to call
                // VariantClear().
                // GC does not have to clear it.
                GC.SuppressFinalize(this);
            }

#if never
            public static VARIANT FromObject(Object var) {
                VARIANT v = new VARIANT();

                if (var == null) {
                    v.vt = (int)tagVT.VT_EMPTY;
                }
                else if (Convert.IsDBNull(var)) {
                }
                else {
                    Type t = var.GetType();

                    if (t == typeof(bool)) {
                        v.vt = (int)tagVT.VT_BOOL;
                    }
                    else if (t == typeof(byte)) {
                        v.vt = (int)tagVT.VT_UI1;
                        v.data1 = (IntPtr)Convert.ToByte(var);
                    }
                    else if (t == typeof(char)) {
                        v.vt = (int)tagVT.VT_UI2;
                        v.data1 = (IntPtr)Convert.ToChar(var);
                    }
                    else if (t == typeof(string)) {
                        v.vt = (int)tagVT.VT_BSTR;
                        v.data1 = SysAllocString(Convert.ToString(var));
                    }
                    else if (t == typeof(short)) {
                        v.vt = (int)tagVT.VT_I2;
                        v.data1 = (IntPtr)Convert.ToInt16(var);
                    }
                    else if (t == typeof(int)) {
                        v.vt = (int)tagVT.VT_I4;
                        v.data1 = (IntPtr)Convert.ToInt32(var);
                    }
                    else if (t == typeof(long)) {
                        v.vt = (int)tagVT.VT_I8;
                        v.SetLong(Convert.ToInt64(var));
                    }
                    else if (t == typeof(Decimal)) {
                        v.vt = (int)tagVT.VT_CY;
                        Decimal c = (Decimal)var;
                        // SBUrke, it's bizzare that we need to call this as a static!
                        v.SetLong(Decimal.ToInt64(c));
                    }
                    else if (t == typeof(decimal)) {
                        v.vt = (int)tagVT.VT_DECIMAL;
                        Decimal d = Convert.ToDecimal(var);
                        v.SetLong(Decimal.ToInt64(d));
                    }
                    else if (t == typeof(double)) {
                        v.vt = (int)tagVT.VT_R8;
                        // how do we handle double?
                    }
                    else if (t == typeof(float) || t == typeof(Single)) {
                        v.vt = (int)tagVT.VT_R4;
                        // how do we handle float?
                    }
                    else if (t == typeof(DateTime)) {
                        v.vt = (int)tagVT.VT_DATE;
                        v.SetLong(Convert.ToDateTime(var).ToFileTime());
                    }
                    else if (t == typeof(SByte)) {
                        v.vt = (int)tagVT.VT_I1;
                        v.data1 = (IntPtr)Convert.ToSByte(var);
                    }
                    else if (t == typeof(UInt16)) {
                        v.vt = (int)tagVT.VT_UI2;
                        v.data1 = (IntPtr)Convert.ToUInt16(var);
                    }
                    else if (t == typeof(UInt32)) {
                        v.vt = (int)tagVT.VT_UI4;
                        v.data1 = (IntPtr)Convert.ToUInt32(var);
                    }
                    else if (t == typeof(UInt64)) {
                        v.vt = (int)tagVT.VT_UI8;
                        v.SetLong((long)Convert.ToUInt64(var));
                    }
                    else if (t == typeof(object) || t == typeof(UnsafeNativeMethods.IDispatch) || t.IsCOMObject) {
                        v.vt = (t == typeof(UnsafeNativeMethods.IDispatch) ? (short)tagVT.VT_DISPATCH : (short)tagVT.VT_UNKNOWN);
                        v.data1 = Marshal.GetIUnknownForObject(var);
                    }
                    else {
                        Invariant.Assert(false, "Unsupported object type!");
                    }
                }
                return v;
            }
#endif

            [DllImport(ExternDll.Oleaut32,CharSet=CharSet.Auto)]
            private static extern IntPtr SysAllocString([In, MarshalAs(UnmanagedType.LPWStr)]string s);

            [DllImport(ExternDll.Oleaut32,CharSet=CharSet.Auto)]
            private static extern void SysFreeString(IntPtr pbstr);
            /// <SecurityNote>
            ///     Critical: Sets the pointer to an arbitrary long
            /// </SecurityNote>
            [SecurityCritical]
            public void SetLong(long lVal) {
                data1.Value = (IntPtr)(lVal & 0xFFFFFFFF);
                data2.Value = (IntPtr)((lVal >> 32) & 0xFFFFFFFF);
            }

            /// <SecurityNote>
            ///     Critical: Calls Marshal.AllocCoTaskMem, .WriteInt16 and .WriteInt32 which have LinkDemands.
            ///               Writes to unmanaged memory and returns a pointer to it.
            /// </SecurityNote>
            [SecurityCritical]
            public IntPtr ToCoTaskMemPtr() {
                IntPtr mem = Marshal.AllocCoTaskMem(16);
                Marshal.WriteInt16(mem, vt);
                Marshal.WriteInt16(mem, 2, reserved1);
                Marshal.WriteInt16(mem, 4, reserved2);
                Marshal.WriteInt16(mem, 6, reserved3);
                Marshal.WriteInt32(mem, 8, (int) data1.Value);
                Marshal.WriteInt32(mem, 12, (int) data2.Value);
                return mem;
            }

            /// <SecurityNote>
            ///     Critical: Converts an intptr to an object , it acceses PtrToStruct which is critical
            /// </SecurityNote>
            [SecurityCritical]
            public object ToObject() {
                IntPtr val = data1.Value;
                long longVal;

                int vtType = (int)(this.vt & (short)tagVT.VT_TYPEMASK);

                switch (vtType) {
                case (int)tagVT.VT_EMPTY:
                    return null;
                case (int)tagVT.VT_NULL:
                    return Convert.DBNull;

                case (int)tagVT.VT_I1:
                    if (Byref) {
                        val = (IntPtr) Marshal.ReadByte(val);
                    }
                    return (SByte) (0xFF & (SByte) val);

                case (int)tagVT.VT_UI1:
                    if (Byref) {
                        val = (IntPtr) Marshal.ReadByte(val);
                    }

                    return (byte) (0xFF & (byte) val);

                case (int)tagVT.VT_I2:
                    if (Byref) {
                        val = (IntPtr) Marshal.ReadInt16(val);
                    }
                    return (short)(0xFFFF & (short) val);

                case (int)tagVT.VT_UI2:
                    if (Byref) {
                        val = (IntPtr) Marshal.ReadInt16(val);
                    }
                    return (UInt16)(0xFFFF & (UInt16) val);

                case (int)tagVT.VT_I4:
                case (int)tagVT.VT_INT:
                    if (Byref) {
                        val = (IntPtr) Marshal.ReadInt32(val);
                    }
                    return (int)val;

                case (int)tagVT.VT_UI4:
                case (int)tagVT.VT_UINT:
                    if (Byref) {
                        val = (IntPtr) Marshal.ReadInt32(val);
                    }
                    return (UInt32)val;

                case (int)tagVT.VT_I8:
                case (int)tagVT.VT_UI8:
                    if (Byref) {
                        longVal = Marshal.ReadInt64(val);
                    }
                    else {
                        longVal = ((uint)data1.Value & 0xffffffff) | ((uint)data2.Value << 32);
                    }

                    if (vt == (int)tagVT.VT_I8) {
                        return (long)longVal;
                    }
                    else {
                        return (UInt64)longVal;
                    }
                }

                if (Byref) {
                    val = GetRefInt(val);
                }

                switch (vtType) {
                case (int)tagVT.VT_R4:
                case (int)tagVT.VT_R8:

                    // can I use unsafe here?
                    throw new FormatException(/*SR.GetString(SR.CannotConvertIntToFloat)*/);

                case (int)tagVT.VT_CY:
                    // internally currency is 8-byte int scaled by 10,000
                    longVal = ((uint)data1.Value & 0xffffffff) | ((uint)data2.Value << 32);
                    return new Decimal(longVal);
                case (int)tagVT.VT_DATE:
                    throw new FormatException(/*SR.GetString(SR.CannotConvertDoubleToDate)*/);

                case (int)tagVT.VT_BSTR:
                case (int)tagVT.VT_LPWSTR:
                    return Marshal.PtrToStringUni(val);

                case (int)tagVT.VT_LPSTR:
                    return Marshal.PtrToStringAnsi(val);

                case (int)tagVT.VT_DISPATCH:
                case (int)tagVT.VT_UNKNOWN:
                    {
                        return Marshal.GetObjectForIUnknown(val);
                    }

                case (int)tagVT.VT_HRESULT:
                    return val;

                case (int)tagVT.VT_DECIMAL:
                    longVal = ((uint)data1.Value & 0xffffffff) | ((uint)data2.Value << 32);
                    return new Decimal(longVal);

                case (int)tagVT.VT_BOOL:
                    return (val != IntPtr.Zero);

                case (int)tagVT.VT_VARIANT:
                    VARIANT varStruct = (VARIANT)UnsafeNativeMethods.PtrToStructure(val, typeof(VARIANT));
                    return varStruct.ToObject();
                case (int)tagVT.VT_CLSID:
                    //Debug.Fail("PtrToStructure will not work with System.Guid...");
                    Guid guid =(Guid)UnsafeNativeMethods.PtrToStructure(val, typeof(Guid));
                    return guid;

                case (int)tagVT.VT_FILETIME:
                    longVal = ((uint)data1.Value & 0xffffffff) | ((uint)data2.Value << 32);
                    return new DateTime(longVal);

                case (int)tagVT.VT_ARRAY:
                    //gSAFEARRAY sa = (tagSAFEARRAY)Marshal.PtrToStructure(val), typeof(tagSAFEARRAY));
                    //return GetArrayFromSafeArray(sa);

                case (int)tagVT.VT_USERDEFINED:
                case (int)tagVT.VT_VOID:
                case (int)tagVT.VT_PTR:
                case (int)tagVT.VT_SAFEARRAY:
                case (int)tagVT.VT_CARRAY:

                case (int)tagVT.VT_RECORD:
                case (int)tagVT.VT_BLOB:
                case (int)tagVT.VT_STREAM:
                case (int)tagVT.VT_STORAGE:
                case (int)tagVT.VT_STREAMED_OBJECT:
                case (int)tagVT.VT_STORED_OBJECT:
                case (int)tagVT.VT_BLOB_OBJECT:
                case (int)tagVT.VT_CF:
                case (int)tagVT.VT_BSTR_BLOB:
                case (int)tagVT.VT_VECTOR:
                case (int)tagVT.VT_BYREF:
                    //case (int)tagVT.VT_RESERVED:
                default:
                    return null;
            }
            }
            /// <SecurityNote>
            ///     Critical: Reads an arbitrary IntPtr
            /// </SecurityNote>
            [SecurityCritical]
            private static IntPtr GetRefInt(IntPtr value) {
                return Marshal.ReadIntPtr(value);
            }
        }

#if never
        [StructLayout(LayoutKind.Sequential)/*leftover(noAutoOffset)*/]
        public sealed class tagLICINFO
        {
          [MarshalAs(UnmanagedType.U4)/*leftover(offset=0, cb)*/]
          public int cbLicInfo = SizeOf();

          public int fRuntimeAvailable;
          public int fLicVerified;
            
            /// <SecurityNote>
            ///  Critical : Calls critical Marshal.SizeOf
            ///  Safe     : Calls method with trusted input (well known safe type)
            /// </SecurityNote>
            [SecuritySafeCritical]
            private static int SizeOf()
            {
                return Marshal.SizeOf(typeof(tagLICINFO));
            }        
        }
#endif

        public enum  tagVT {
            VT_EMPTY = 0,
            VT_NULL = 1,
            VT_I2 = 2,
            VT_I4 = 3,
            VT_R4 = 4,
            VT_R8 = 5,
            VT_CY = 6,
            VT_DATE = 7,
            VT_BSTR = 8,
            VT_DISPATCH = 9,
            VT_ERROR = 10,
            VT_BOOL = 11,
            VT_VARIANT = 12,
            VT_UNKNOWN = 13,
            VT_DECIMAL = 14,
            VT_I1 = 16,
            VT_UI1 = 17,
            VT_UI2 = 18,
            VT_UI4 = 19,
            VT_I8 = 20,
            VT_UI8 = 21,
            VT_INT = 22,
            VT_UINT = 23,
            VT_VOID = 24,
            VT_HRESULT = 25,
            VT_PTR = 26,
            VT_SAFEARRAY = 27,
            VT_CARRAY = 28,
            VT_USERDEFINED = 29,
            VT_LPSTR = 30,
            VT_LPWSTR = 31,
            VT_RECORD = 36,
            VT_FILETIME = 64,
            VT_BLOB = 65,
            VT_STREAM = 66,
            VT_STORAGE = 67,
            VT_STREAMED_OBJECT = 68,
            VT_STORED_OBJECT = 69,
            VT_BLOB_OBJECT = 70,
            VT_CF = 71,
            VT_CLSID = 72,
            VT_BSTR_BLOB = 4095,
            VT_VECTOR = 4096,
            VT_ARRAY = 8192,
            VT_BYREF = 16384,
            VT_RESERVED = 32768,
            VT_ILLEGAL = 65535,
            VT_ILLEGALMASKED = 4095,
            VT_TYPEMASK = 4095
        }

        public delegate void TimerProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
#if never

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class WNDCLASS_D {
            public int      style;
            public WndProc  lpfnWndProc;
            public int      cbClsExtra = 0;
            public int      cbWndExtra = 0;
            public IntPtr   hInstance = IntPtr.Zero;
            public IntPtr   hIcon = IntPtr.Zero;
            public IntPtr   hCursor = IntPtr.Zero;
            public IntPtr   hbrBackground = IntPtr.Zero;
            public string   lpszMenuName = null;
            public string   lpszClassName = null;
        }

        public class MSOCM {
            // MSO Component registration flags
            public const int msocrfNeedIdleTime         = 1;
            public const int msocrfNeedPeriodicIdleTime = 2;
            public const int msocrfPreTranslateKeys     = 4;
            public const int msocrfPreTranslateAll      = 8;
            public const int msocrfNeedSpecActiveNotifs = 16;
            public const int msocrfNeedAllActiveNotifs  = 32;
            public const int msocrfExclusiveBorderSpace = 64;
            public const int msocrfExclusiveActivation = 128;
            public const int msocrfNeedAllMacEvents = 256;
            public const int msocrfMaster           = 512;

            // MSO Component registration advise flags (see msocstate enumeration)
            public const int msocadvfModal              = 1;
            public const int msocadvfRedrawOff          = 2;
            public const int msocadvfWarningsOff        = 4;
            public const int msocadvfRecording          = 8;

            // MSO Component Host flags
            public const int msochostfExclusiveBorderSpace = 1;

            // MSO idle flags, passed to IMsoComponent::FDoIdle and
            // IMsoStdComponentMgr::FDoIdle.
            public const int msoidlefPeriodic    = 1;
            public const int msoidlefNonPeriodic = 2;
            public const int msoidlefPriority    = 4;
            public const int msoidlefAll         = -1;

            // MSO Reasons for pushing a message loop, passed to
            // IMsoComponentManager::FPushMessageLoop and
            // IMsoComponentHost::FPushMessageLoop.  The host should remain in message
            // loop until IMsoComponent::FContinueMessageLoop
            public const int msoloopMain      = -1; // Note this is not an official MSO loop -- it just must be distinct.
            public const int msoloopFocusWait = 1;
            public const int msoloopDoEvents  = 2;
            public const int msoloopDebug     = 3;
            public const int msoloopModalForm = 4;
            public const int msoloopModalAlert = 5;


            /* msocstate values: state IDs passed to
                IMsoComponent::OnEnterState,
                IMsoComponentManager::OnComponentEnterState/FOnComponentExitState/FInState,
                IMsoComponentHost::OnComponentEnterState,
                IMsoStdComponentMgr::OnHostEnterState/FOnHostExitState/FInState.
                When the host or a component is notified through one of these methods that
                another entity (component or host) is entering or exiting a state
                identified by one of these state IDs, the host/component should take
                appropriate action:
                    msocstateModal (modal state):
                        If app is entering modal state, host/component should disable
                        its toplevel windows, and reenable them when app exits this
                        state.  Also, when this state is entered or exited, host/component
                        should notify approprate inplace objects via
                        IOleInPlaceActiveObject::EnableModeless.
                    msocstateRedrawOff (redrawOff state):
                        If app is entering redrawOff state, host/component should disable
                        repainting of its windows, and reenable repainting when app exits
                        this state.
                    msocstateWarningsOff (warningsOff state):
                        If app is entering warningsOff state, host/component should disable
                        the presentation of any user warnings, and reenable this when
                        app exits this state.
                    msocstateRecording (Recording state):
                        Used to notify host/component when Recording is turned on or off. */
            public const int msocstateModal       = 1;
            public const int msocstateRedrawOff   = 2;
            public const int msocstateWarningsOff = 3;
            public const int msocstateRecording   = 4;


            /*             ** Comments on State Contexts **
            IMsoComponentManager::FCreateSubComponentManager allows one to create a
            hierarchical tree of component managers.  This tree is used to maintain
            multiple contexts with regard to msocstateXXX states.  These contexts are
            referred to as 'state contexts'.
            Each component manager in the tree defines a state context.  The
            components registered with a particular component manager or any of its
            descendents live within that component manager's state context.  Calls
            to IMsoComponentManager::OnComponentEnterState/FOnComponentExitState
            can be used to  affect all components, only components within the component
            manager's state context, or only those components that are outside of the
            component manager's state context.  IMsoComponentManager::FInState is used
            to query the state of the component manager's state context at its root.

            msoccontext values: context indicators passed to
            IMsoComponentManager::OnComponentEnterState/FOnComponentExitState.
            These values indicate the state context that is to be affected by the
            state change.
            In IMsoComponentManager::OnComponentEnterState/FOnComponentExitState,
            the comp mgr informs only those components/host that are within the
            specified state context. */
            public const int msoccontextAll    = 0;
            public const int msoccontextMine   = 1;
            public const int msoccontextOthers = 2;

            /*     ** WM_MOUSEACTIVATE Note (for top level compoenents and host) **
            If the active (or tracking) comp's reg info indicates that it
            wants mouse messages, then no MA_xxxANDEAT value should be returned
            from WM_MOUSEACTIVATE, so that the active (or tracking) comp will be able
            to process the resulting mouse message.  If one does not want to examine
            the reg info, no MA_xxxANDEAT value should be returned from
            WM_MOUSEACTIVATE if any comp is active (or tracking).
            One can query the reg info of the active (or tracking) component at any
            time via IMsoComponentManager::FGetActiveComponent. */

            /* msogac values: values passed to
            IMsoComponentManager::FGetActiveComponent. */
            public const int msogacActive    = 0;
            public const int msogacTracking   = 1;
            public const int msogacTrackingOrActive = 2;

            /* msocWindow values: values passed to IMsoComponent::HwndGetWindow. */
            public const int msocWindowFrameToplevel = 0;
            public const int msocWindowFrameOwner = 1;
            public const int msocWindowComponent = 2;
            public const int msocWindowDlgOwner = 3;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class TOOLINFO_T
        {
            public int      cbSize = SizeOf();
            public int      uFlags;
            public IntPtr   hwnd;
            public IntPtr   uId;
            public RECT     rect;
            public IntPtr   hinst;
            public string   lpszText;
            public IntPtr   lParam;
            
            /// <SecurityNote>
            ///  Critical : Calls critical Marshal.SizeOf
            ///  Safe     : Calls method with trusted input (well known safe type)
            /// </SecurityNote>
            [SecuritySafeCritical]
            private static int SizeOf()
            {
                return Marshal.SizeOf(typeof(TOOLINFO_T));
            }        
        }


        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class TOOLINFO_TOOLTIP
        {
            public int      cbSize = SizeOf();
            public int      uFlags;
            public IntPtr   hwnd;
            public IntPtr   uId;
            public RECT     rect;
            public IntPtr   hinst;
            public IntPtr   lpszText;
            public IntPtr   lParam;
            
            /// <SecurityNote>
            ///  Critical : Calls critical Marshal.SizeOf
            ///  Safe     : Calls method with trusted input (well known safe type)
            /// </SecurityNote>
            [SecuritySafeCritical]
            private static int SizeOf()
            {
                return Marshal.SizeOf(typeof(TOOLINFO_TOOLTIP));
            }        
        }


        [StructLayout(LayoutKind.Sequential)]
        public sealed class tagDVTARGETDEVICE {
            [MarshalAs(UnmanagedType.U4)]
            public   uint tdSize;
            [MarshalAs(UnmanagedType.U2)]
            public   ushort tdDriverNameOffset;
            [MarshalAs(UnmanagedType.U2)]
            public   ushort tdDeviceNameOffset;
            [MarshalAs(UnmanagedType.U2)]
            public   ushort tdPortNameOffset;
            [MarshalAs(UnmanagedType.U2)]
            public   ushort tdExtDevmodeOffset;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public struct TV_ITEM {
            public int      mask;
            public IntPtr   hItem;
            public int      state;
            public int      stateMask;
            public IntPtr /* LPTSTR */ pszText;
            public int      cchTextMax;
            public int      iImage;
            public int      iSelectedImage;
            public int      cChildren;
            public IntPtr   lParam;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public struct TVSORTCB {
            public IntPtr                                         hParent;
            public NativeMethods.TreeViewCompareCallback          lpfnCompare;
            public IntPtr                                         lParam;
        }



        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public struct TV_INSERTSTRUCT {
            public IntPtr   hParent;
            public IntPtr   hInsertAfter;
            public int      item_mask;
            public IntPtr   item_hItem;
            public int      item_state;
            public int      item_stateMask;
            public IntPtr /* LPTSTR */ item_pszText;
            public int      item_cchTextMax;
            public int      item_iImage;
            public int      item_iSelectedImage;
            public int      item_cChildren;
            public IntPtr   item_lParam;
            public int      item_iIntegral;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NMTREEVIEW
        {
            public NMHDR    nmhdr;
            public int      action;
            public TV_ITEM  itemOld;
            public TV_ITEM  itemNew;
            public int      ptDrag_X; // This should be declared as POINT
            public int      ptDrag_Y; // we use unsafe blocks to manipulate
                                      // NMTREEVIEW quickly, and POINT is declared
                                      // as a class.  Too much churn to change POINT
                                      // now.
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NMTVGETINFOTIP
        {
            public NMHDR    nmhdr;
            public string   pszText;
            public int      cchTextMax;
            public IntPtr   item;
            public IntPtr   lParam;

        }

        [StructLayout(LayoutKind.Sequential)]
        public class NMTVDISPINFO
        {
            public NMHDR    hdr;
            public TV_ITEM  item;
        }

        [StructLayout(LayoutKind.Sequential)]
        public sealed class POINTL {
            public   int x;
            public   int y;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public struct HIGHCONTRAST {
            public int cbSize;
            public int dwFlags;
            public string lpszDefaultScheme;
        }
#endif

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public struct HIGHCONTRAST_I {
            public int cbSize;
            public int dwFlags;
            public IntPtr lpszDefaultScheme;
        }
#if never

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class TCITEM_T
        {
            public int      mask;
            public int      dwState;
            public int      dwStateMask;
            public string   pszText;
            public int      cchTextMax;
            public int      iImage;
            public IntPtr   lParam;
        }
#endif

        [StructLayout(LayoutKind.Sequential)/*leftover(noAutoOffset)*/]
        public sealed class DISPPARAMS
        {
          public IntPtr rgvarg;
          public IntPtr rgdispidNamedArgs;
          [MarshalAs(UnmanagedType.U4)/*leftover(offset=8, cArgs)*/]
          public uint cArgs;
          [MarshalAs(UnmanagedType.U4)/*leftover(offset=12, cNamedArgs)*/]
          public uint cNamedArgs;
        }

#if never
        public enum  tagINVOKEKIND {
            INVOKE_FUNC = 1,
            INVOKE_PROPERTYGET = 2,
            INVOKE_PROPERTYPUT = 4,
            INVOKE_PROPERTYPUTREF = 8
        }
#endif

        [StructLayout(LayoutKind.Sequential)]
        public class EXCEPINFO {
            [MarshalAs(UnmanagedType.U2)]
            public ushort wCode;
            [MarshalAs(UnmanagedType.U2)]
            public ushort wReserved;
            [MarshalAs(UnmanagedType.BStr)]
            public string bstrSource;
            [MarshalAs(UnmanagedType.BStr)]
            public string bstrDescription;
            [MarshalAs(UnmanagedType.BStr)]
            public string bstrHelpFile;
            [MarshalAs(UnmanagedType.U4)]
            public uint dwHelpContext;

            public IntPtr pvReserved;

            public IntPtr pfnDeferredFillIn;
            [MarshalAs(UnmanagedType.I4)]
            public int scode;
        }

#if never
        public enum  tagDESCKIND {
            DESCKIND_NONE = 0,
            DESCKIND_FUNCDESC = 1,
            DESCKIND_VARDESC = 2,
            DESCKIND_TYPECOMP = 3,
            DESCKIND_IMPLICITAPPOBJ = 4,
            DESCKIND_MAX = 5
        }

        [StructLayout(LayoutKind.Sequential)]
        public sealed class tagFUNCDESC {
            public   int memid;
            [MarshalAs(UnmanagedType.U2)]
            public   short lprgscode;

            // This is marked as NATIVE_TYPE_PTR,
            // but the EE doesn't look for that, tries to handle it as
            // a ELEMENT_TYPE_VALUECLASS and fails because it
            // isn't a NATIVE_TYPE_NESTEDSTRUCT
            /*[MarshalAs(UnmanagedType.PTR)]*/

            public    /*NativeMethods.tagELEMDESC*/ IntPtr lprgelemdescParam;

            // cpb, SBurke, the EE chokes on Enums in structs

            public    /*NativeMethods.tagFUNCKIND*/ int funckind;

            public    /*NativeMethods.tagINVOKEKIND*/ int invkind;

            public    /*NativeMethods.tagCALLCONV*/ int callconv;
            [MarshalAs(UnmanagedType.I2)]
            public   short cParams;
            [MarshalAs(UnmanagedType.I2)]
            public   short cParamsOpt;
            [MarshalAs(UnmanagedType.I2)]
            public   short oVft;
            [MarshalAs(UnmanagedType.I2)]
            public   short cScodes;
            public   NativeMethods.value_tagELEMDESC elemdescFunc;
            [MarshalAs(UnmanagedType.U2)]
            public   ushort wFuncFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        public sealed class tagVARDESC {
            public   int memid;
            public   IntPtr lpstrSchema;
            public   IntPtr unionMember;
            public   NativeMethods.value_tagELEMDESC elemdescVar;
            [MarshalAs(UnmanagedType.U2)]
            public   ushort wVarFlags;
            public    /*NativeMethods.tagVARKIND*/ int varkind;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct  value_tagELEMDESC {
            public    NativeMethods.tagTYPEDESC tdesc;
            public    NativeMethods.tagPARAMDESC paramdesc;
        }

#endif
        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWPOS {
            public IntPtr hwnd;
            public IntPtr hwndInsertAfter;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public int flags;
        }
#if never

        [StructLayout(LayoutKind.Sequential)]
        public class DRAWITEMSTRUCT {
            public int CtlType;
            public int CtlID;
            public int itemID;
            public int itemAction;
            public int itemState;
            public IntPtr hwndItem;
            public IntPtr hDC;
            public RECT   rcItem;
            public IntPtr itemData;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class MEASUREITEMSTRUCT {
            public int CtlType;
            public int CtlID;
            public int itemID;
            public int itemWidth;
            public int itemHeight;
            public IntPtr itemData;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class HELPINFO {
            public int      cbSize = SizeOf();
            public int      iContextType;
            public int      iCtrlId;
            public IntPtr   hItemHandle;
            public int      dwContextId;
            public POINT    MousePos;
            
            /// <SecurityNote>
            ///  Critical : Calls critical Marshal.SizeOf
            ///  Safe     : Calls method with trusted input (well known safe type)
            /// </SecurityNote>
            [SecuritySafeCritical]
            private static int SizeOf()
            {
                return Marshal.SizeOf(typeof(HELPINFO));
            }        
        }

        [StructLayout(LayoutKind.Sequential)]
        public class ACCEL {
            public byte fVirt;
            public short key;
            public short cmd;
        }
#endif

        [StructLayout(LayoutKind.Sequential)]
        public class MINMAXINFO {
            public POINT ptReserved = new POINT();
            public POINT ptMaxSize = new POINT();
            public POINT ptMaxPosition = new POINT();
            public POINT ptMinTrackSize = new POINT();
            public POINT ptMaxTrackSize = new POINT();
        }
#if never
        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class CREATESTRUCT {
            public IntPtr lpCreateParams = IntPtr.Zero;
            public IntPtr hInstance = IntPtr.Zero;
            public IntPtr hMenu = IntPtr.Zero;
            public IntPtr hwndParent = IntPtr.Zero;
            public int cy = 0;
            public int cx = 0;
            public int y = 0;
            public int x = 0;
            public int style = 0;
            public string lpszName = null;
            public string lpszClass = null;
            public int dwExStyle = 0;
        }
#endif

#if never
        [ComImport(), Guid("B196B28B-BAB4-101A-B69C-00AA00341D07"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        public interface ISpecifyPropertyPages {
             void GetPages(
                [Out]
                NativeMethods.tagCAUUID pPages);

        }

        [StructLayout(LayoutKind.Sequential)/*leftover(noAutoOffset)*/]
        public sealed class tagCAUUID
        {
            [MarshalAs(UnmanagedType.U4)/*leftover(offset=0, cElems)*/]
            public uint cElems;
            public IntPtr pElems;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NMTOOLBAR
        {
            public NMHDR    hdr;
            public int      iItem;
            public TBBUTTON tbButton;
            public int      cchText;
            public IntPtr   pszText;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TBBUTTON {
            public int      iBitmap;
            public int      idCommand;
            public byte     fsState;
            public byte     fsStyle;
            public byte     bReserved0;
            public byte     bReserved1;
            public IntPtr   dwData;
            public IntPtr   iString;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class TOOLTIPTEXT
        {
            public NMHDR  hdr;
            public IntPtr lpszText;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=80)]
            public string szText;

            public IntPtr hinst;
            public int    uFlags;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Ansi)]
        public class TOOLTIPTEXTA
        {
            public NMHDR  hdr;
            public IntPtr lpszText;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=80)]
            public string szText;

            public IntPtr hinst;
            public int    uFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NMTBHOTITEM
        {
            public NMHDR    hdr;
            public int      idOld;
            public int      idNew;
            public int      dwFlags;
        }

        public const int HICF_OTHER             = 0x00000000;
        public const int HICF_MOUSE             = 0x00000001;          // Triggered by mouse
        public const int HICF_ARROWKEYS         = 0x00000002;          // Triggered by arrow keys
        public const int HICF_ACCELERATOR       = 0x00000004;          // Triggered by accelerator
        public const int HICF_DUPACCEL          = 0x00000008;          // This accelerator is not unique
        public const int HICF_ENTERING          = 0x00000010;          // idOld is invalid
        public const int HICF_LEAVING           = 0x00000020;          // idNew is invalid
        public const int HICF_RESELECT          = 0x00000040;          // hot item reselected
        public const int HICF_LMOUSE            = 0x00000080;          // left mouse button selected
        public const int HICF_TOGGLEDROPDOWN    = 0x00000100;          // Toggle button's dropdown state


    [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Auto)]
    public class HDITEM
    {
        public int      mask;
        public int      cxy;
        public string   pszText;
        public IntPtr   hbm;
        public int      cchTextMax;
        public int      fmt;
        public IntPtr   lParam;
        public int      iImage;
        public int      iOrder;
        public int      type;
        public IntPtr   pvFilter;
    }

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
    public struct TOOLINFO
    {
        public int      cbSize;
        public int      uFlags;
        public IntPtr   hwnd;
        public IntPtr   uId;
        public RECT     rect;
        public IntPtr   hinst;
        public string   lpszText;
        public IntPtr   lParam;
    }

    [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Auto)]
    public struct TBBUTTONINFO
    {
        public int      cbSize;
        public int      dwMask;
        public int      idCommand;
        public int      iImage;
        public byte     fsState;
        public byte     fsStyle;
        public short    cx;
        public IntPtr   lParam;
        public IntPtr   pszText;
        public int      cchTest;
    }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class TV_HITTESTINFO {
            public int      pt_x;
            public int      pt_y;
            public int      flags;
            public IntPtr   hItem;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class NMTVCUSTOMDRAW
        {
            public NMCUSTOMDRAW    nmcd;
            public int clrText;
            public int clrTextBk;
            public int iLevel;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NMCUSTOMDRAW {
            public NMHDR    nmcd;
            public int      dwDrawStage;
            public IntPtr   hdc;
            public RECT     rc;
            public IntPtr   dwItemSpec;
            public int      uItemState;
            public IntPtr   lItemlParam;
        }

    [StructLayout(LayoutKind.Sequential)]
        public class NMTTCUSTOMDRAW
        {
            public NMCUSTOMDRAW nmcd;
        public int uDrawFlags;
    }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class MCHITTESTINFO {
            public int      cbSize = SizeOf();
            public int      pt_x;
            public int      pt_y;
            public int      uHit;
            public short st_wYear;
            public short st_wMonth;
            public short st_wDayOfWeek;
            public short st_wDay;
            public short st_wHour;
            public short st_wMinute;
            public short st_wSecond;
            public short st_wMilliseconds;
            
            /// <SecurityNote>
            ///  Critical : Calls critical Marshal.SizeOf
            ///  Safe     : Calls method with trusted input (well known safe type)
            /// </SecurityNote>
            [SecuritySafeCritical]
            private static int SizeOf()
            {
                return Marshal.SizeOf(typeof(MCHITTESTINFO));
            }        
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class NMSELCHANGE
        {
            public NMHDR        nmhdr;
            public SYSTEMTIME   stSelStart;
            public SYSTEMTIME   stSelEnd;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class NMDAYSTATE
        {
            public NMHDR        nmhdr;
            public SYSTEMTIME   stStart;
            public int          cDayState;
            public IntPtr       prgDayState;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NMLVCUSTOMDRAW
        {
            public NMCUSTOMDRAW    nmcd;
            public int clrText;
            public int clrTextBk;
            public int iSubItem;
            public int dwItemType;
            // Item Custom Draw
            public int clrFace;
            public int iIconEffect;
            public int iIconPhase;
            public int iPartId;
            public int iStateId;
            // Group Custom Draw
            public RECT rcText;
            public uint uAlign;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class NMLVGETINFOTIP
        {
            public NMHDR    nmhdr;
            public int flags;
            public IntPtr   lpszText;
            public int      cchTextMax;
            public int   item;
            public int subItem;
            public IntPtr   lParam;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class NMLVKEYDOWN
        {
            public NMHDR hdr;
            public short wVKey;
            public uint flags;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class LVHITTESTINFO {
            public int      pt_x;
            public int      pt_y;
            public int      flags;
            public int      iItem;
            public int      iSubItem;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class LVBKIMAGE
        {
            public int ulFlags;
            public IntPtr hBmp; // not used
            public string pszImage;
            public int cchImageMax;
            public int xOffset;
            public int yOffset;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class LVCOLUMN_T
        {
            public int      mask;
            public int      fmt;
            public int      cx;
            public string   pszText;
            public int      cchTextMax;
            public int      iSubItem;
            public int      iImage;
            public int      iOrder;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public struct LVFINDINFO {
            public int      flags;
            public string   psz;
            public IntPtr   lParam;
            public int      ptX; // was POINT pt
            public int      ptY;
            public int      vkDirection;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public struct LVITEM {
            public int      mask;
            public int      iItem;
            public int      iSubItem;
            public int      state;
            public int      stateMask;
            public string   pszText;
            public int      cchTextMax;
            public int      iImage;
            public IntPtr   lParam;
            public int      iIndent;
            public int      iGroupId;
            public int      cColumns; // tile view columns
            public IntPtr   puColumns;

            public unsafe void Reset() {
                pszText = null;
                mask = 0;
                iItem = 0;
                iSubItem = 0;
                stateMask = 0;
                state = 0;
                cchTextMax = 0;
                iImage = 0;
                lParam = IntPtr.Zero;
                iIndent = 0;
                iGroupId = 0;
                cColumns = 0;
                puColumns = IntPtr.Zero;
            }

            public override string ToString() {
                return "LVITEM: pszText = " + pszText
                     + ", iItem = " + iItem.ToString()
                     + ", iSubItem = " + iSubItem.ToString()
                     + ", state = " + state.ToString()
                     + ", iGroupId = " + iGroupId.ToString()
                     + ", cColumns = " + cColumns.ToString();
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public struct LVITEM_NOTEXT {
            public int      mask;
            public int      iItem;
            public int      iSubItem;
            public int      state;
            public int      stateMask;
            public IntPtr /*string*/   pszText;
            public int      cchTextMax;
            public int      iImage;
            public IntPtr   lParam;
            public int      iIndent;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class LVCOLUMN {
            public int      mask;
            public int      fmt;
            public int      cx;
            public IntPtr /* LPWSTR */ pszText;
            public int      cchTextMax;
            public int      iSubItem;
            public int      iImage;
            public int      iOrder;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        public class LVGROUP {
            public uint cbSize = (uint)SizeOf();
            public uint mask;
            public IntPtr pszHeader;
            public int cchHeader;
            public IntPtr pszFooter;
            public int cchFooter;
            public int iGroupId;
            public uint stateMask;
            public uint state;
            public uint uAlign;

            public override string ToString() {
                return "LVGROUP: header = " + pszHeader.ToString() + ", iGroupId = " + iGroupId.ToString();
            }
            
            /// <SecurityNote>
            ///  Critical : Calls critical Marshal.SizeOf
            ///  Safe     : Calls method with trusted input (well known safe type)
            /// </SecurityNote>
            [SecuritySafeCritical]
            private static int SizeOf()
            {
                return Marshal.SizeOf(typeof(LVGROUP));
            }        
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class LVINSERTMARK {
            public uint cbSize = (uint)SizeOf();
            public int dwFlags;
            public int iItem;
            public int dwReserved = 0;
            
            /// <SecurityNote>
            ///  Critical : Calls critical Marshal.SizeOf
            ///  Safe     : Calls method with trusted input (well known safe type)
            /// </SecurityNote>
            [SecuritySafeCritical]
            private static int SizeOf()
            {
                return Marshal.SizeOf(typeof(LVINSERTMARK));
            }        
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class LVTILEVIEWINFO {
            public uint cbSize = (uint)SizeOf();
            public int dwMask;
            public int dwFlags;
            public SIZE sizeTile;
            public int cLines;
            public RECT rcLabelMargin;
            
            /// <SecurityNote>
            ///  Critical : Calls critical Marshal.SizeOf
            ///  Safe     : Calls method with trusted input (well known safe type)
            /// </SecurityNote>
            [SecuritySafeCritical]
            private static int SizeOf()
            {
                return Marshal.SizeOf(typeof(LVTILEVIEWINFO));
            }        
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class NMLVCACHEHINT {
            public NMHDR hdr;
            public int iFrom;
            public int iTo;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class NMLVDISPINFO
        {
            public NMHDR  hdr;
            public LVITEM item;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class NMLVDISPINFO_NOTEXT
        {
            public NMHDR  hdr;
            public LVITEM_NOTEXT item;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class NMLVODSTATECHANGE {
            public NMHDR    hdr;
            public int      iFrom;
            public int      iTo;
            public int      uNewState;
            public int      uOldState;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class CLIENTCREATESTRUCT {
            public IntPtr hWindowMenu;
            public int idFirstChild;

            public CLIENTCREATESTRUCT(IntPtr hmenu, int idFirst) {
                hWindowMenu = hmenu;
                idFirstChild = idFirst;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class NMDATETIMECHANGE
        {
            public NMHDR        nmhdr;
            public int          dwFlags;
            public SYSTEMTIME   st;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class NMDATETIMEFORMAT
        {
            public NMHDR      nmhdr;
            public string     pszFormat;
            public SYSTEMTIME st;
            public string     pszDisplay;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=32)]
            public string     szDisplay;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class NMDATETIMEFORMATQUERY
        {
            public NMHDR  nmhdr;
            public string pszFormat;
            public SIZE   szMax;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class NMDATETIMEWMKEYDOWN
        {
            public NMHDR      nmhdr;
            public int        nVirtKey;
            public string     pszFormat;
            public SYSTEMTIME st;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class COPYDATASTRUCT {
            public int dwData;
            public int cbData;
            public IntPtr lpData;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class NMHEADER {
            public NMHDR nmhdr;
            public int iItem;
            public int iButton;
            public IntPtr pItem;    // HDITEM*
        }

        [StructLayout(LayoutKind.Sequential)]
        public class MOUSEHOOKSTRUCT {
            // pt was a by-value POINT structure
            public int pt_x = 0;
            public int pt_y = 0;
            public IntPtr hWnd = IntPtr.Zero;
            public int wHitTestCode = 0;
            public int dwExtraInfo = 0;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class CHARRANGE
        {
            public int  cpMin;
            public int  cpMax;
        }

        [StructLayout(LayoutKind.Sequential, Pack=4)]
        public class CHARFORMATW
        {
            public int      cbSize = SizeOf();
            public int      dwMask;
            public int      dwEffects;
            public int      yHeight;
            public int      yOffset;
            public int      crTextColor;
            public byte     bCharSet;
            public byte     bPitchAndFamily;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=64)]
            public byte[]   szFaceName = new byte[64];
            
            /// <SecurityNote>
            ///  Critical : Calls critical Marshal.SizeOf
            ///  Safe     : Calls method with trusted input (well known safe type)
            /// </SecurityNote>
            [SecuritySafeCritical]
            private static int SizeOf()
            {
                return Marshal.SizeOf(typeof(CHARFORMATW));
            }        
        }

        [StructLayout(LayoutKind.Sequential, Pack=4)]
        public class CHARFORMATA
        {
            public int      cbSize = SizeOf();
            public int      dwMask;
            public int      dwEffects;
            public int      yHeight;
            public int      yOffset;
            public int      crTextColor;
            public byte     bCharSet;
            public byte     bPitchAndFamily;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=32)]
            public byte[]   szFaceName = new byte[32];
            
            /// <SecurityNote>
            ///  Critical : Calls critical Marshal.SizeOf
            ///  Safe     : Calls method with trusted input (well known safe type)
            /// </SecurityNote>
            [SecuritySafeCritical]
            private static int SizeOf()
            {
                return Marshal.SizeOf(typeof(CHARFORMATA));
            }        
        }

        [StructLayout(LayoutKind.Sequential, Pack=4)]
        public class CHARFORMAT2A
        {
            public int      cbSize = SizeOf();
            public int      dwMask;
            public int      dwEffects;
            public int      yHeight;
            public int      yOffset;
            public int      crTextColor;
            public byte     bCharSet;
            public byte     bPitchAndFamily;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=32)]
            public byte[]   szFaceName = new byte[32];
            public short    wWeight;
            public short    sSpacing;
            public int      crBackColor;
            public int      lcid;
            public int      dwReserved;
            public short    sStyle;
            public short    wKerning;
            public byte     bUnderlineType;
            public byte     bAnimation;
            public byte     bRevAuthor;
            
            /// <SecurityNote>
            ///  Critical : Calls critical Marshal.SizeOf
            ///  Safe     : Calls method with trusted input (well known safe type)
            /// </SecurityNote>
            [SecuritySafeCritical]
            private static int SizeOf()
            {
                return Marshal.SizeOf(typeof(CHARFORMAT2A));
            }        
        }

        [StructLayout(LayoutKind.Sequential)]
        public class TEXTRANGE
        {
            public CHARRANGE    chrg;
            public IntPtr       lpstrText; /* allocated by caller, zero terminated by RichEdit */
        }

        [StructLayout(LayoutKind.Sequential)]
        public class GETTEXTLENGTHEX
        {                               // Taken from richedit.h:
            public uint flags;          // Flags (see GTL_XXX defines)
            public uint codepage;       // Code page for translation (CP_ACP for default, 1200 for Unicode)
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Ansi, Pack=4)]
        public class SELCHANGE {
            public NMHDR nmhdr;
            public CHARRANGE chrg;
            public int seltyp;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class PARAFORMAT
        {
            public int      cbSize = SizeOf();
            public int      dwMask;
            public short    wNumbering;
            public short    wReserved;
            public int      dxStartIndent;
            public int      dxRightIndent;
            public int      dxOffset;
            public short    wAlignment;
            public short    cTabCount;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst=32)]
            public int[]    rgxTabs;
            
            /// <SecurityNote>
            ///  Critical : Calls critical Marshal.SizeOf
            ///  Safe     : Calls method with trusted input (well known safe type)
            /// </SecurityNote>
            [SecuritySafeCritical]
            private static int SizeOf()
            {
                return Marshal.SizeOf(typeof(PARAFORMAT));
            }        
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class FINDTEXT
        {
            public CHARRANGE    chrg;
            public string       lpstrText;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class REPASTESPECIAL
        {
            public int  dwAspect;
            public int  dwParam;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class ENLINK
        {
            public NMHDR    nmhdr;
            public int      msg;
            public IntPtr   wParam;
            public IntPtr   lParam;
            public CHARRANGE charrange;
        }
#endif
        internal abstract class CharBuffer
        {

            /// <SecurityNote>
            ///     Critical: Extensive use of Marshal to allocate and manipulate
            ///             Character buffers.
            /// </SecurityNote>
            [SecurityCritical]
            internal static CharBuffer CreateBuffer(int size)
            {
                if (Marshal.SystemDefaultCharSize == 1)
                {
                    return new AnsiCharBuffer(size);
                }
                return new UnicodeCharBuffer(size);
            }

            internal abstract IntPtr AllocCoTaskMem();
            internal abstract string GetString();
            internal abstract void PutCoTaskMem(IntPtr ptr);
            internal abstract void PutString(string s);
            internal abstract int Length{get;}
        }


        /// <SecurityNote>
        ///     Critical: Extensive use of Marshal to allocate and manipulate
        ///             Character buffers.
        /// </SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        internal class AnsiCharBuffer : CharBuffer
        {

            internal byte[] buffer;
            internal int offset;

            internal AnsiCharBuffer(int size)
            {
                buffer = new byte[size];
            }

            internal override int Length
            {
                get { return buffer.Length; }
            }

            internal override IntPtr AllocCoTaskMem()
            {
                IntPtr result = Marshal.AllocCoTaskMem(buffer.Length);
                Marshal.Copy(buffer, 0, result, buffer.Length);

                return result;
            }

            internal override string GetString()
            {
                int i = offset;
                while (i < buffer.Length && buffer[i] != 0)
                    i++;

                string result = Encoding.Default.GetString(buffer, offset, i - offset);

                if (i < buffer.Length)
                    i++;

                offset = i;

                return result;
            }

            internal override void PutCoTaskMem(IntPtr ptr)
            {
                Marshal.Copy(ptr, buffer, 0, buffer.Length);
                offset = 0;
            }

            internal override void PutString(string s)
            {
                byte[] bytes = Encoding.Default.GetBytes(s);
                int count = Math.Min(bytes.Length, buffer.Length - offset);

                Array.Copy(bytes, 0, buffer, offset, count);

                offset += count;
                if (offset < buffer.Length)
                    buffer[offset++] = 0;
            }
        }

        /// <SecurityNote>
        ///     Critical: Extensive use of Marshal to allocate and manipulate
        ///             Character buffers.
        /// </SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        internal class UnicodeCharBuffer : CharBuffer
        {

            internal char[] buffer;
            internal int offset;

            internal UnicodeCharBuffer(int size)
            {
                buffer = new char[size];
            }

            internal override int Length
            {
                get { return buffer.Length; }
            }

            internal override IntPtr AllocCoTaskMem()
            {
                IntPtr result = Marshal.AllocCoTaskMem(buffer.Length * 2);
                Marshal.Copy(buffer, 0, result, buffer.Length);
                return result;
            }

            internal override String GetString()
            {
                int i = offset;

                while (i < buffer.Length && buffer[i] != 0)
                    i++;

                string result = new string(buffer, offset, i - offset);

                if (i < buffer.Length)
                    i++;

                offset = i;
                return result;
            }

            internal override void PutCoTaskMem(IntPtr ptr)
            {
                Marshal.Copy(ptr, buffer, 0, buffer.Length);
                offset = 0;
            }

            internal override void PutString(string s)
            {
                int count = Math.Min(s.Length, buffer.Length - offset);

                s.CopyTo(0, buffer, offset, count);
                offset += count;

                if (offset < buffer.Length)
                    buffer[offset++] = (char)0;
            }
        }

#if never
        [StructLayout(LayoutKind.Sequential)]
        public class ENDROPFILES
        {
            public NMHDR    nmhdr;
            public IntPtr   hDrop;
            public int      cp;
            public bool     fProtected;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class REQRESIZE
        {
            public NMHDR    nmhdr;
            public RECT     rc;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class ENPROTECTED
        {
            public NMHDR    nmhdr;
            public int      msg;
            public IntPtr   wParam;
            public IntPtr   lParam;
            public CHARRANGE chrg;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class ENPROTECTED64
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=56)]
            public byte[] contents = new byte[56];
        }

        public class ActiveX {
            public const   int OCM__BASE = 0x2000;
            public const   int DISPID_VALUE = unchecked((int)0x0);
            public const   int DISPID_UNKNOWN = unchecked((int)0xFFFFFFFF);
            public const   int DISPID_AUTOSIZE = unchecked((int)0xFFFFFE0C);
            public const   int DISPID_BACKCOLOR = unchecked((int)0xFFFFFE0B);
            public const   int DISPID_BACKSTYLE = unchecked((int)0xFFFFFE0A);
            public const   int DISPID_BORDERCOLOR = unchecked((int)0xFFFFFE09);
            public const   int DISPID_BORDERSTYLE = unchecked((int)0xFFFFFE08);
            public const   int DISPID_BORDERWIDTH = unchecked((int)0xFFFFFE07);
            public const   int DISPID_DRAWMODE = unchecked((int)0xFFFFFE05);
            public const   int DISPID_DRAWSTYLE = unchecked((int)0xFFFFFE04);
            public const   int DISPID_DRAWWIDTH = unchecked((int)0xFFFFFE03);
            public const   int DISPID_FILLCOLOR = unchecked((int)0xFFFFFE02);
            public const   int DISPID_FILLSTYLE = unchecked((int)0xFFFFFE01);
            public const   int DISPID_FONT = unchecked((int)0xFFFFFE00);
            public const   int DISPID_FORECOLOR = unchecked((int)0xFFFFFDFF);
            public const   int DISPID_ENABLED = unchecked((int)0xFFFFFDFE);
            public const   int DISPID_HWND = unchecked((int)0xFFFFFDFD);
            public const   int DISPID_TABSTOP = unchecked((int)0xFFFFFDFC);
            public const   int DISPID_TEXT = unchecked((int)0xFFFFFDFB);
            public const   int DISPID_CAPTION = unchecked((int)0xFFFFFDFA);
            public const   int DISPID_BORDERVISIBLE = unchecked((int)0xFFFFFDF9);
            public const   int DISPID_APPEARANCE = unchecked((int)0xFFFFFDF8);
            public const   int DISPID_MOUSEPOINTER = unchecked((int)0xFFFFFDF7);
            public const   int DISPID_MOUSEICON = unchecked((int)0xFFFFFDF6);
            public const   int DISPID_PICTURE = unchecked((int)0xFFFFFDF5);
            public const   int DISPID_VALID = unchecked((int)0xFFFFFDF4);
            public const   int DISPID_READYSTATE = unchecked((int)0xFFFFFDF3);
            public const   int DISPID_REFRESH = unchecked((int)0xFFFFFDDA);
            public const   int DISPID_DOCLICK = unchecked((int)0xFFFFFDD9);
            public const   int DISPID_ABOUTBOX = unchecked((int)0xFFFFFDD8);
            public const   int DISPID_CLICK = unchecked((int)0xFFFFFDA8);
            public const   int DISPID_DBLCLICK = unchecked((int)0xFFFFFDA7);
            public const   int DISPID_KEYDOWN = unchecked((int)0xFFFFFDA6);
            public const   int DISPID_KEYPRESS = unchecked((int)0xFFFFFDA5);
            public const   int DISPID_KEYUP = unchecked((int)0xFFFFFDA4);
            public const   int DISPID_MOUSEDOWN = unchecked((int)0xFFFFFDA3);
            public const   int DISPID_MOUSEMOVE = unchecked((int)0xFFFFFDA2);
            public const   int DISPID_MOUSEUP = unchecked((int)0xFFFFFDA1);
            public const   int DISPID_ERROREVENT = unchecked((int)0xFFFFFDA0);
            public const   int DISPID_RIGHTTOLEFT = unchecked((int)0xFFFFFD9D);
            public const   int DISPID_READYSTATECHANGE = unchecked((int)0xFFFFFD9F);
            public const   int DISPID_AMBIENT_BACKCOLOR = unchecked((int)0xFFFFFD43);
            public const   int DISPID_AMBIENT_DISPLAYNAME = unchecked((int)0xFFFFFD42);
            public const   int DISPID_AMBIENT_FONT = unchecked((int)0xFFFFFD41);
            public const   int DISPID_AMBIENT_FORECOLOR = unchecked((int)0xFFFFFD40);
            public const   int DISPID_AMBIENT_LOCALEID = unchecked((int)0xFFFFFD3F);
            public const   int DISPID_AMBIENT_MESSAGEREFLECT = unchecked((int)0xFFFFFD3E);
            public const   int DISPID_AMBIENT_SCALEUNITS = unchecked((int)0xFFFFFD3D);
            public const   int DISPID_AMBIENT_TEXTALIGN = unchecked((int)0xFFFFFD3C);
            public const   int DISPID_AMBIENT_USERMODE = unchecked((int)0xFFFFFD3B);
            public const   int DISPID_AMBIENT_UIDEAD = unchecked((int)0xFFFFFD3A);
            public const   int DISPID_AMBIENT_SHOWGRABHANDLES = unchecked((int)0xFFFFFD39);
            public const   int DISPID_AMBIENT_SHOWHATCHING = unchecked((int)0xFFFFFD38);
            public const   int DISPID_AMBIENT_DISPLAYASDEFAULT = unchecked((int)0xFFFFFD37);
            public const   int DISPID_AMBIENT_SUPPORTSMNEMONICS = unchecked((int)0xFFFFFD36);
            public const   int DISPID_AMBIENT_AUTOCLIP = unchecked((int)0xFFFFFD35);
            public const   int DISPID_AMBIENT_APPEARANCE = unchecked((int)0xFFFFFD34);
            public const   int DISPID_AMBIENT_PALETTE = unchecked((int)0xFFFFFD2A);
            public const   int DISPID_AMBIENT_TRANSFERPRIORITY = unchecked((int)0xFFFFFD28);
            public const   int DISPID_AMBIENT_RIGHTTOLEFT = unchecked((int)0xFFFFFD24);
            public const   int DISPID_Name = unchecked((int)0xFFFFFCE0);
            public const   int DISPID_Delete = unchecked((int)0xFFFFFCDF);
            public const   int DISPID_Object = unchecked((int)0xFFFFFCDE);
            public const   int DISPID_Parent = unchecked((int)0xFFFFFCDD);
            public const   int DVASPECT_CONTENT = 0x1;
            public const   int DVASPECT_THUMBNAIL = 0x2;
            public const   int DVASPECT_ICON = 0x4;
            public const   int DVASPECT_DOCPRINT = 0x8;
            public const   int OLEMISC_RECOMPOSEONRESIZE = 0x1;
            public const   int OLEMISC_ONLYICONIC = 0x2;
            public const   int OLEMISC_INSERTNOTREPLACE = 0x4;
            public const   int OLEMISC_STATIC = 0x8;
            public const   int OLEMISC_CANTLINKINSIDE = 0x10;
            public const   int OLEMISC_CANLINKBYOLE1 = 0x20;
            public const   int OLEMISC_ISLINKOBJECT = 0x40;
            public const   int OLEMISC_INSIDEOUT = 0x80;
            public const   int OLEMISC_ACTIVATEWHENVISIBLE = 0x100;
            public const   int OLEMISC_RENDERINGISDEVICEINDEPENDENT = 0x200;
            public const   int OLEMISC_INVISIBLEATRUNTIME = 0x400;
            public const   int OLEMISC_ALWAYSRUN = 0x800;
            public const   int OLEMISC_ACTSLIKEBUTTON = 0x1000;
            public const   int OLEMISC_ACTSLIKELABEL = 0x2000;
            public const   int OLEMISC_NOUIACTIVATE = 0x4000;
            public const   int OLEMISC_ALIGNABLE = 0x8000;
            public const   int OLEMISC_SIMPLEFRAME = 0x10000;
            public const   int OLEMISC_SETCLIENTSITEFIRST = 0x20000;
            public const   int OLEMISC_IMEMODE = 0x40000;
            public const   int OLEMISC_IGNOREACTIVATEWHENVISIBLE = 0x80000;
            public const   int OLEMISC_WANTSTOMENUMERGE = 0x100000;
            public const   int OLEMISC_SUPPORTSMULTILEVELUNDO = 0x200000;
            public const   int QACONTAINER_SHOWHATCHING = 0x1;
            public const   int QACONTAINER_SHOWGRABHANDLES = 0x2;
            public const   int QACONTAINER_USERMODE = 0x4;
            public const   int QACONTAINER_DISPLAYASDEFAULT = 0x8;
            public const   int QACONTAINER_UIDEAD = 0x10;
            public const   int QACONTAINER_AUTOCLIP = 0x20;
            public const   int QACONTAINER_MESSAGEREFLECT = 0x40;
            public const   int QACONTAINER_SUPPORTSMNEMONICS = 0x80;
            public const   int XFORMCOORDS_POSITION = 0x1;
            public const   int XFORMCOORDS_SIZE = 0x2;
            public const   int XFORMCOORDS_HIMETRICTOCONTAINER = 0x4;
            public const   int XFORMCOORDS_CONTAINERTOHIMETRIC = 0x8;
            public const   int PROPCAT_Nil = unchecked((int)0xFFFFFFFF);
            public const   int PROPCAT_Misc = unchecked((int)0xFFFFFFFE);
            public const   int PROPCAT_Font = unchecked((int)0xFFFFFFFD);
            public const   int PROPCAT_Position = unchecked((int)0xFFFFFFFC);
            public const   int PROPCAT_Appearance = unchecked((int)0xFFFFFFFB);
            public const   int PROPCAT_Behavior = unchecked((int)0xFFFFFFFA);
            public const   int PROPCAT_Data = unchecked((int)0xFFFFFFF9);
            public const   int PROPCAT_List = unchecked((int)0xFFFFFFF8);
            public const   int PROPCAT_Text = unchecked((int)0xFFFFFFF7);
            public const   int PROPCAT_Scale = unchecked((int)0xFFFFFFF6);
            public const   int PROPCAT_DDE = unchecked((int)0xFFFFFFF5);
            public const   int GC_WCH_SIBLING = 0x1;
            public const   int GC_WCH_CONTAINER = 0x2;
            public const   int GC_WCH_CONTAINED = 0x3;
            public const   int GC_WCH_ALL = 0x4;
            public const   int GC_WCH_FREVERSEDIR = 0x8000000;
            public const   int GC_WCH_FONLYNEXT = 0x10000000;
            public const   int GC_WCH_FONLYPREV = 0x20000000;
            public const   int GC_WCH_FSELECTED = 0x40000000;
            public const   int ALIGN_MIN = 0x0;
            public const   int ALIGN_NO_CHANGE = 0x0;
            public const   int ALIGN_TOP = 0x1;
            public const   int ALIGN_BOTTOM = 0x2;
            public const   int ALIGN_LEFT = 0x3;
            public const   int ALIGN_RIGHT = 0x4;
            public const   int ALIGN_MAX = 0x4;
            public const   int OLEVERBATTRIB_NEVERDIRTIES = 0x1;
            public const   int OLEVERBATTRIB_ONCONTAINERMENU = 0x2;

#endif

#if NEVER
        public static class Util {
            public static int MAKELONG(int low, int high) {
                return (high << 16) | (low & 0xffff);
            }

            public static IntPtr MAKELPARAM(int low, int high) {
                return (IntPtr)(MAKELONG(low, high));
            }

            public static int HIWORD(int n) {
                return (n >> 16) & 0xffff;
            }

            public static int HIWORD(IntPtr n) {
                return HIWORD( unchecked((int)(long)n) );
            }

            public static int LOWORD(int n) {
                return n & 0xffff;
            }

            public static int LOWORD(IntPtr n) {
                return LOWORD( unchecked((int)(long)n) );
            }

            public static int SignedHIWORD(IntPtr n) {
                return SignedHIWORD( unchecked((int)(long)n) );
            }
            public static int SignedLOWORD(IntPtr n) {
                return SignedLOWORD( unchecked((int)(long)n) );
            }

            public static int SignedHIWORD(int n) {
                int i = (int)(short)((n >> 16) & 0xffff);

                return i;
            }

            public static int SignedLOWORD(int n) {
                int i = (int)(short)(n & 0xFFFF);

                return i;
            }

            /// <devdoc>
            ///     Computes the string size that should be passed to a typical Win32 call.
            ///     This will be the character count under NT, and the ubyte count for Windows 95.
            /// </devdoc>
            public static int GetPInvokeStringLength(String s) {
                if (s == null) {
                    return 0;
                }

                if (Marshal.SystemDefaultCharSize == 2) {
                    return s.Length;
                }
                else {
                    if (s.Length == 0) {
                        return 0;
                    }
                    if (s.IndexOf('\0') > -1) {
                        return GetEmbededNullStringLengthAnsi(s);
                    }
                    else {
                        return lstrlen(s);
                    }
                }
            }

            private static int GetEmbededNullStringLengthAnsi(String s) {
                int n = s.IndexOf('\0');
                if (n > -1) {
                    String left = s.Substring(0, n);
                    String right = s.Substring(n+1);
                    return GetPInvokeStringLength(left) + GetEmbededNullStringLengthAnsi(right) + 1;
                }
                else {
                    return GetPInvokeStringLength(s);
                }
            }

            [DllImport(ExternDll.Kernel32, CharSet=CharSet.Auto, BestFitMapping = false)]
            private static extern int lstrlen(String s);

        }

#endif

#if never

        public enum  tagTYPEKIND {
            TKIND_ENUM = 0,
            TKIND_RECORD = 1,
            TKIND_MODULE = 2,
            TKIND_INTERFACE = 3,
            TKIND_DISPATCH = 4,
            TKIND_COCLASS = 5,
            TKIND_ALIAS = 6,
            TKIND_UNION = 7,
            TKIND_MAX = 8
        }

        [StructLayout(LayoutKind.Sequential)]
        public class  tagTLIBATTR {
            public   Guid guid;
            [MarshalAs(UnmanagedType.U4)]
            public   int lcid;
            public   NativeMethods.tagSYSKIND syskind;
            [MarshalAs(UnmanagedType.U2)]
            public   ushort wMajorVerNum;
            [MarshalAs(UnmanagedType.U2)]
            public   ushort wMinorVerNum;
            [MarshalAs(UnmanagedType.U2)]
            public   ushort wLibFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        public  class  tagTYPEDESC {
            public   IntPtr unionMember;
            public   short vt;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct  tagPARAMDESC {
            public   IntPtr pparamdescex;

            [MarshalAs(UnmanagedType.U2)]
            public   ushort wParamFlags;
        }
#endif
        public static class CommonHandles {
            static CommonHandles() {
            }

            /// <devdoc>
            ///     Handle type for accelerator tables.
            /// </devdoc>
            public static readonly int Accelerator  = HandleCollector.RegisterType("Accelerator", 80, 50);

            /// <devdoc>
            ///     handle type for cursors.
            /// </devdoc>
            public static readonly int Cursor       = HandleCollector.RegisterType("Cursor", 20, 500);

            /// <devdoc>
            ///     Handle type for enhanced metafiles.
            /// </devdoc>
            public static readonly int EMF          = HandleCollector.RegisterType("EnhancedMetaFile", 20, 500);

            /// <devdoc>
            ///     Handle type for file find handles.
            /// </devdoc>
            public static readonly int Find         = HandleCollector.RegisterType("Find", 0, 1000);

            /// <devdoc>
            ///     Handle type for GDI objects.
            /// </devdoc>
            public static readonly int GDI          = HandleCollector.RegisterType("GDI", 50, 500);

            /// <devdoc>
            ///     Handle type for HDC's that count against the Win98 limit of five DC's.  HDC's
            ///     which are not scarce, such as HDC's for bitmaps, are counted as GDIHANDLE's.
            /// </devdoc>
            public static readonly int HDC          = HandleCollector.RegisterType("HDC", 100, 2); // wait for 2 dc's before collecting

            /// <devdoc>
            ///     Handle type for icons.
            /// </devdoc>
            public static readonly int Icon         = HandleCollector.RegisterType("Icon", 20, 500);

            /// <devdoc>
            ///     Handle type for kernel objects.
            /// </devdoc>
            public static readonly int Kernel       = HandleCollector.RegisterType("Kernel", 0, 1000);

            /// <devdoc>
            ///     Handle type for files.
            /// </devdoc>
            public static readonly int Menu         = HandleCollector.RegisterType("Menu", 30, 1000);

            /// <devdoc>
            ///     Handle type for windows.
            /// </devdoc>
            public static readonly int Window       = HandleCollector.RegisterType("Window", 5, 1000);
        }
#if never
        public enum  tagSYSKIND {
            SYS_WIN16 = 0,
            SYS_MAC = 2
        }

        public delegate bool MonitorEnumProc(IntPtr monitor, IntPtr hdc, IntPtr lprcMonitor, IntPtr lParam);

        /// <SecurityNote>
        /// Critical : Elevates to UnmanagedCode permissions
        /// </SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [SuppressUnmanagedCodeSecurity]
		[ComImport(), Guid("A7ABA9C1-8983-11cf-8F20-00805F2CD064"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IProvideMultipleClassInfo {
             // since the inheritance doesn't seem to work...
             // these are from IProvideClassInfo & IProvideClassInfo2
             [PreserveSig]
             UnsafeNativeMethods.ITypeInfo GetClassInfo();

             [PreserveSig]
             int GetGUID(int dwGuidKind, [In, Out] ref Guid pGuid);

             [PreserveSig]
             int GetMultiTypeInfoCount([In, Out] ref int pcti);

             // we use arrays for most of these since we never use them anyway.
             [PreserveSig]
             int GetInfoOfIndex(int iti, int dwFlags,
                                [In, Out]
                                ref UnsafeNativeMethods.ITypeInfo pTypeInfo,
                                int       pTIFlags,
                                int       pcdispidReserved,
                                IntPtr piidPrimary,
                                IntPtr piidSource);
       }

        [StructLayout(LayoutKind.Sequential)]
            public class EVENTMSG {
            public int message;
            public int paramL;
            public int paramH;
            public int time;
            public IntPtr hwnd;
        }

        /// <SecurityNote>
        /// Critical : Elevates to UnmanagedCode permissions
        /// </SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [SuppressUnmanagedCodeSecurity]
		[ComImport(), Guid("B196B283-BAB4-101A-B69C-00AA00341D07"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IProvideClassInfo {
            [return: MarshalAs(UnmanagedType.Interface)]
            UnsafeNativeMethods.ITypeInfo GetClassInfo();
        }

        [StructLayout(LayoutKind.Sequential)]
        public  sealed class  tagTYPEATTR {
            public Guid guid;
            [MarshalAs(UnmanagedType.U4)]
            public   uint lcid;
            [MarshalAs(UnmanagedType.U4)]
            public   uint dwReserved;
            public   int memidConstructor;
            public   int memidDestructor;
            public   IntPtr lpstrSchema;
            [MarshalAs(UnmanagedType.U4)]
            public   uint cbSizeInstance;
            public    /*NativeMethods.tagTYPEKIND*/ int typekind;
            [MarshalAs(UnmanagedType.U2)]
            public   ushort cFuncs;
            [MarshalAs(UnmanagedType.U2)]
            public   ushort cVars;
            [MarshalAs(UnmanagedType.U2)]
            public   ushort cImplTypes;
            [MarshalAs(UnmanagedType.U2)]
            public   ushort cbSizeVft;
            [MarshalAs(UnmanagedType.U2)]
            public   ushort cbAlignment;
            [MarshalAs(UnmanagedType.U2)]
            public   ushort wTypeFlags;
            [MarshalAs(UnmanagedType.U2)]
            public   ushort wMajorVerNum;
            [MarshalAs(UnmanagedType.U2)]
            public   ushort wMinorVerNum;

            // SBurke these are inline too
            //public    NativeMethods.tagTYPEDESC tdescAlias;
            [MarshalAs(UnmanagedType.U4)]
            public   int tdescAlias_unionMember;

            [MarshalAs(UnmanagedType.U2)]
            public   short tdescAlias_vt;

            //public    NativeMethods.tagIDLDESC idldescType;
            [MarshalAs(UnmanagedType.U4)]
            public   int idldescType_dwReserved;

            [MarshalAs(UnmanagedType.U2)]
            public   short idldescType_wIDLFlags;


            public tagTYPEDESC Get_tdescAlias(){
                tagTYPEDESC td = new tagTYPEDESC();
                td.unionMember = (IntPtr)this.tdescAlias_unionMember;
                td.vt = this.tdescAlias_vt;
                return td;
            }

            public tagIDLDESC Get_idldescType(){
                tagIDLDESC id = new tagIDLDESC();
                id.dwReserved = this.idldescType_dwReserved;
                id.wIDLFlags = this.idldescType_wIDLFlags;
                return id;
            }
        }

        public enum tagVARFLAGS {
             VARFLAG_FREADONLY         =    1,
             VARFLAG_FSOURCE           =    0x2,
             VARFLAG_FBINDABLE         =    0x4,
             VARFLAG_FREQUESTEDIT      =    0x8,
             VARFLAG_FDISPLAYBIND      =    0x10,
             VARFLAG_FDEFAULTBIND      =    0x20,
             VARFLAG_FHIDDEN           =    0x40,
             VARFLAG_FDEFAULTCOLLELEM  =    0x100,
             VARFLAG_FUIDEFAULT        =    0x200,
             VARFLAG_FNONBROWSABLE     =    0x400,
             VARFLAG_FREPLACEABLE      =    0x800,
             VARFLAG_FIMMEDIATEBIND    =    0x1000
       }

        [StructLayout(LayoutKind.Sequential)]
        public sealed class tagELEMDESC {
            public    NativeMethods.tagTYPEDESC tdesc;
            public    NativeMethods.tagPARAMDESC paramdesc;
        }

        public enum  tagVARKIND {
            VAR_PERINSTANCE = 0,
            VAR_STATIC = 1,
            VAR_CONST = 2,
            VAR_DISPATCH = 3
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct  tagIDLDESC {
            [MarshalAs(UnmanagedType.U4)]
            public   int dwReserved;
            [MarshalAs(UnmanagedType.U2)]
            public   ushort wIDLFlags;
        }

        public struct RGBQUAD {
            public byte rgbBlue;
            public byte rgbGreen;
            public byte rgbRed;
            public byte rgbReserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class BITMAPINFO_ARRAY {
            public BITMAPINFOHEADER bmiHeader = new BITMAPINFOHEADER();

            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst=BITMAPINFO_MAX_COLORSIZE*4)]
            public byte[] bmiColors; // RGBQUAD structs... Blue-Green-Red-Reserved, repeat...
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PALETTEENTRY {
            public byte peRed;
            public byte peGreen;
            public byte peBlue;
            public byte peFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BITMAPINFO_FLAT {
            public int      bmiHeader_biSize;// = Marshal.SizeOf(typeof(BITMAPINFOHEADER));
            public int      bmiHeader_biWidth;
            public int      bmiHeader_biHeight;
            public short    bmiHeader_biPlanes;
            public short    bmiHeader_biBitCount;
            public int      bmiHeader_biCompression;
            public int      bmiHeader_biSizeImage;
            public int      bmiHeader_biXPelsPerMeter;
            public int      bmiHeader_biYPelsPerMeter;
            public int      bmiHeader_biClrUsed;
            public int      bmiHeader_biClrImportant;

            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst=BITMAPINFO_MAX_COLORSIZE*4)]
            public byte[] bmiColors; // RGBQUAD structs... Blue-Green-Red-Reserved, repeat...
        }


        /// <devdoc>
        ///     This method takes a file URL and converts it to a local path.  The trick here is that
        ///     if there is a '#' in the path, everything after this is treated as a fragment.  So
        ///     we need to append the fragment to the end of the path.
        /// </devdoc>
        internal static string GetLocalPath(string fileName) {
            System.Diagnostics.Debug.Assert(fileName != null && fileName.Length > 0, "Cannot get local path, fileName is not valid");

            Uri uri = new Uri(fileName, true);
            return uri.LocalPath + uri.Fragment;
        }

#endif

        public const int PBT_APMPOWERSTATUSCHANGE = 0x000A;

        [StructLayout(LayoutKind.Sequential)]
        public struct SYSTEM_POWER_STATUS {
            public byte     ACLineStatus;
            public byte     BatteryFlag;
            public byte     BatteryLifePercent;
            public byte     Reserved1;
            public int      BatteryLifeTime;
            public int      BatteryFullLifeTime;
        }

#if never
        [StructLayout(LayoutKind.Sequential)]
        internal class DLLVERSIONINFO
        {
           internal uint cbSize;
           internal uint dwMajorVersion;
           internal uint dwMinorVersion;
           internal uint dwBuildNumber;
           internal uint dwPlatformID;
        }

        public enum OLERENDER
        {
            OLERENDER_NONE = 0,
            OLERENDER_DRAW = 1,
            OLERENDER_FORMAT = 2,
            OLERENDER_ASIS = 3
        }

        // Theming/Visual Styles stuff
        public const int STAP_ALLOW_NONCLIENT    =  (1 << 0);
        public const int STAP_ALLOW_CONTROLS     =  (1 << 1);
        public const int STAP_ALLOW_WEBCONTENT   =  (1 << 2);

        public const int PS_NULL = 5;
        public const int PS_INSIDEFRAME = 6;

        public const int PS_GEOMETRIC = 0x00010000;
        public const int PS_ENDCAP_SQUARE = 0x00000100;

        public const int MM_HIMETRIC = 3;

#endif

        // WinEvent
#if never
        public const int EVENT_MIN  = 0x00000001;
        public const int EVENT_MAX  = 0x7FFFFFFF;

        public const int EVENT_SYSTEM_SOUND =              0x0001;
        public const int EVENT_SYSTEM_ALERT =              0x0002;
        public const int EVENT_SYSTEM_FOREGROUND =         0x0003;
        public const int EVENT_SYSTEM_MENUSTART =          0x0004;
        public const int EVENT_SYSTEM_MENUEND =            0x0005;
        public const int EVENT_SYSTEM_MENUPOPUPSTART =     0x0006;
        public const int EVENT_SYSTEM_MENUPOPUPEND =       0x0007;
        public const int EVENT_SYSTEM_CAPTURESTART =       0x0008;
        public const int EVENT_SYSTEM_CAPTUREEND =         0x0009;
#endif
        public const int EVENT_SYSTEM_MOVESIZESTART =      0x000A;
        public const int EVENT_SYSTEM_MOVESIZEEND =        0x000B;
#if never
        public const int EVENT_SYSTEM_CONTEXTHELPSTART =   0x000C;
        public const int EVENT_SYSTEM_CONTEXTHELPEND =     0x000D;
        public const int EVENT_SYSTEM_DRAGDROPSTART =      0x000E;
        public const int EVENT_SYSTEM_DRAGDROPEND =        0x000F;
        public const int EVENT_SYSTEM_DIALOGSTART =        0x0010;
        public const int EVENT_SYSTEM_DIALOGEND =          0x0011;
        public const int EVENT_SYSTEM_SCROLLINGSTART =     0x0012;
        public const int EVENT_SYSTEM_SCROLLINGEND =       0x0013;
        public const int EVENT_SYSTEM_SWITCHEND =          0x0015;
        public const int EVENT_SYSTEM_MINIMIZESTART =      0x0016;
        public const int EVENT_SYSTEM_MINIMIZEEND =        0x0017;
        public const int EVENT_SYSTEM_PAINT =              0x0019;
        public const int EVENT_CONSOLE_CARET =             0x4001;
        public const int EVENT_CONSOLE_UPDATE_REGION =     0x4002;
        public const int EVENT_CONSOLE_UPDATE_SIMPLE =     0x4003;
        public const int EVENT_CONSOLE_UPDATE_SCROLL =     0x4004;
        public const int EVENT_CONSOLE_LAYOUT =            0x4005;
        public const int EVENT_CONSOLE_START_APPLICATION = 0x4006;
        public const int EVENT_CONSOLE_END_APPLICATION =   0x4007;
        public const int EVENT_OBJECT_CREATE =             0x8000;
        public const int EVENT_OBJECT_DESTROY =            0x8001;
        public const int EVENT_OBJECT_SHOW =               0x8002;
        public const int EVENT_OBJECT_HIDE =               0x8003;
        public const int EVENT_OBJECT_REORDER =            0x8004;
        public const int EVENT_OBJECT_SELECTION =          0x8006;
        public const int EVENT_OBJECT_SELECTIONADD =       0x8007;
        public const int EVENT_OBJECT_SELECTIONREMOVE =    0x8008;
        public const int EVENT_OBJECT_SELECTIONWITHIN =    0x8009;
        public const int EVENT_OBJECT_LOCATIONCHANGE =     0x800B;
        public const int EVENT_OBJECT_NAMECHANGE =         0x800C;
        public const int EVENT_OBJECT_DESCRIPTIONCHANGE =  0x800D;
        public const int EVENT_OBJECT_VALUECHANGE =        0x800E;
        public const int EVENT_OBJECT_PARENTCHANGE =       0x800F;
        public const int EVENT_OBJECT_HELPCHANGE =         0x8010;
        public const int EVENT_OBJECT_DEFACTIONCHANGE =    0x8011;
        public const int EVENT_OBJECT_ACCELERATORCHANGE =  0x8012;
#endif

        public const int EVENT_OBJECT_STATECHANGE = 0x800A;
        public const int EVENT_OBJECT_FOCUS = 0x8005;
        public const int OBJID_CLIENT            = unchecked(unchecked((int)0xFFFFFFFC));
        public const int WINEVENT_OUTOFCONTEXT =           0x0000;
#if never
        public const int WINEVENT_SKIPOWNTHREAD =          0x0001;
        public const int WINEVENT_SKIPOWNPROCESS =         0x0002;
        public const int WINEVENT_INCONTEXT =              0x0004;
#endif

        // the delegate passed to USER for receiving a WinEvent
        internal delegate void WinEventProcDef (int winEventHook, int eventId, IntPtr hwnd, int idObject, int idChild, int eventThread, int eventTime);

        #region WebBrowser Related Definitions
        /// <summary>
        /// Specifies the ReadyState of the WebBrowser control.
        /// Returned by the IWebBrowser2.ReadyState property.
        /// </summary>
        public enum WebBrowserReadyState
        {
            UnInitialized = 0,
            Loading = 1,
            Loaded = 2,
            Interactive = 3,
            Complete = 4
        }

#if never
        public const int URLACTION_JAVA_PERMISSIONS = 0x00001C00,
            URLACTION_CREDENTIALS_USE              = 0x00001A00,
            URLACTION_CHANNEL_SOFTDIST_PERMISSIONS = 0x00001E05,
            URLACTION_HTML_FONT_DOWNLOAD           = 0x00001604;

        public const int URLPOLICY_QUERY          = 0x01,
            URLPOLICY_ALLOW                        = 0x00,
            URLPOLICY_DISALLOW                     = 0x03,
            URLPOLICY_JAVA_PROHIBIT                = 0x00000000,
            URLPOLICY_CREDENTIALS_MUST_PROMPT_USER = 0x00010000,
            URLPOLICY_CHANNEL_SOFTDIST_PROHIBIT    = 0x00010000;
#endif
        #endregion WebBrowser Related Definitions

        [StructLayout(LayoutKind.Sequential)]
        public struct RAWINPUTDEVICELIST
        {
            public IntPtr hDevice;
            public uint   dwType;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RID_DEVICE_INFO_MOUSE
        {
            public uint  dwId;
            public uint  dwNumberOfButtons;
            public uint  dwSampleRate;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RID_DEVICE_INFO_KEYBOARD
        {
            public uint  dwType;
            public uint  dwSubType;
            public uint  dwKeyboardMode;
            public uint  dwNumberOfFunctionKeys;
            public uint  dwNumberOfIndicators;
            public uint  dwNumberOfKeysTotal;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RID_DEVICE_INFO_HID
        {
            public uint  dwVendorId;
            public uint  dwProductId;
            public uint  dwVersionNumber;
            public ushort usUsagePage;
            public ushort usUsage;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct RID_DEVICE_INFO
        {
            [FieldOffset(0)]
            public uint                cbSize;
            [FieldOffset(4)]
            public uint                dwType;
            [FieldOffset(8)]
            public RID_DEVICE_INFO_MOUSE mouse;
            [FieldOffset(8)]
            public RID_DEVICE_INFO_KEYBOARD keyboard;
            [FieldOffset(8)]
            public RID_DEVICE_INFO_HID hid;
        }

        public const uint   RIDI_DEVICEINFO = 0x2000000b;
        public const uint   RIM_TYPEHID = 2;
        public const ushort HID_USAGE_PAGE_DIGITIZER = 0x0D;
        public const ushort HID_USAGE_DIGITIZER_DIGITIZER = 1;
        public const ushort HID_USAGE_DIGITIZER_PEN = 2;
        public const ushort HID_USAGE_DIGITIZER_LIGHTPEN = 3;
        public const ushort HID_USAGE_DIGITIZER_TOUCHSCREEN = 4;

        [StructLayout(LayoutKind.Sequential)]
        public struct BLENDFUNCTION
        {
            public byte BlendOp;
            public byte BlendFlags;
            public byte SourceConstantAlpha;
            public byte AlphaFormat;
        }

        public const int AC_SRC_OVER  = 0x00000000;
        public const int ULW_COLORKEY = 0x00000001;
        public const int ULW_ALPHA    = 0x00000002;
        public const int ULW_OPAQUE   = 0x00000004;

        /// <summary>
        /// Contains values that indicate the type of session information to retrieve 
        /// in a call to the WTSQuerySessionInformation function.
        /// </summary>
        public enum WTS_INFO_CLASS
        {
            /// <summary>
            /// A null-terminated string that contains the name of the initial program that Remote Desktop Services runs when the user logs on.
            /// </summary>
            WTSInitialProgram = 0,
            /// <summary>
            /// A null-terminated string that contains the published name of the application that the session is running.
            /// </summary>
            /// <remarks>
            /// Windows Server 2008 R2, Windows 7, Windows Server 2008 and Windows Vista:  This value is not supported
            /// </remarks>
            WTSApplicationName = 1,
            /// <summary>
            /// A null-terminated string that contains the default directory used when launching the initial program.
            /// </summary>
            WTSWorkingDirectory = 2,
            /// <summary>
            /// This value is not used.
            /// </summary>
            WTSOEMId = 3,
            /// <summary>
            /// A ULONG value that contains the session identifier.
            /// </summary>
            WTSSessionId = 4,
            /// <summary>
            /// A null-terminated string that contains the name of the user associated with the session.
            /// </summary>
            WTSUserName = 5,
            /// <summary>
            /// A null-terminated string that contains the name of the Remote Desktop Services session.
            /// </summary>
            /// <remarks>
            /// Despite its name, specifying this type does not return the window station name. Rather, it returns 
            /// the name of the Remote Desktop Services session. Each Remote Desktop Services session is associated 
            /// with an interactive window station. Because the only supported window station name for an interactive 
            /// window station is "WinSta0", each session is associated with its own "WinSta0" window station. For more 
            /// information, <see cref="https://msdn.microsoft.com/en-us/library/ms687096(v=vs.85).aspx">Window Stations</see>
            /// </remarks>
            WTSWinStationName = 6,
            /// <summary>
            /// A null-terminated string that contains the name of the domain to which the logged-on user belongs.
            /// </summary>
            WTSDomainName = 7,
            /// <summary>
            /// The session's current connection state. For more information, <see cref="WTS_CONNECTSTATE_CLASS"/> 
            /// </summary>
            WTSConnectState = 8,
            /// <summary>
            /// A ULONG value that contains the build number of the client.
            /// </summary>
            WTSClientBuildNumber = 9,
            /// <summary>
            /// A null-terminated string that contains the name of the client.
            /// </summary>
            WTSClientName = 10,
            /// <summary>
            /// A null-terminated string that contains the directory in which the client 
            /// is installed.
            /// </summary>
            WTSClientDirectory = 11,
            /// <summary>
            /// A USHORT client-specific product identifier.
            /// </summary>
            WTSClientProductId = 12,
            /// <summary>
            /// A ULONG value that contains a client-specific hardware identifier. This option 
            /// is reserved 
            /// for future use. 
            /// WTSQuerySessionInformation will always return a value of 0.
            /// </summary>
            WTSClientHardwareId = 13,
            /// <summary>
            /// The network type and network address of the client. For more information, 
            /// see WTS_CLIENT_ADDRESS.
            /// The IP address is offset by two bytes from the start of the Address member of the 
            /// WTS_CLIENT_ADDRESS structure.
            /// </summary>
            WTSClientAddress = 14,
            /// <summary>
            /// Information about the display resolution of the client. For more information, 
            /// see WTS_CLIENT_DISPLAY.
            /// </summary>
            WTSClientDisplay = 15,
            /// <summary>
            /// A USHORT value that specifies information about the protocol type for the session. 
            /// This is one of the following values:
            /// 0 : The console session.
            /// 1 : This value is retained for legacy purposes.
            /// 2 : The RDP protocol.
            /// </summary>
            WTSClientProtocolType = 16,
            /// <summary>
            /// This value returns FALSE. If you call GetLastError to get extended error information, 
            /// GetLastError returns ERROR_NOT_SUPPORTED.
            /// </summary>
            /// <remarks>Windows Server 2008 and Windows Vista:  This value is not used.</remarks>
            WTSIdleTime = 17,
            /// <summary>
            /// This value returns FALSE. If you call GetLastError to get extended error information, 
            /// GetLastError returns ERROR_NOT_SUPPORTED.
            /// </summary>
            /// <remarks>Windows Server 2008 and Windows Vista:  This value is not used.</remarks>
            WTSLogonTime = 18,
            /// <summary>
            /// This value returns FALSE. If you call GetLastError to get extended error information, 
            /// GetLastError returns ERROR_NOT_SUPPORTED.
            /// </summary>
            /// <remarks>Windows Server 2008 and Windows Vista:  This value is not used.</remarks>
            WTSIncomingBytes = 19,
            /// <summary>
            /// This value returns FALSE. If you call GetLastError to get extended error information, 
            /// GetLastError returns ERROR_NOT_SUPPORTED.
            /// </summary>
            /// <remarks>Windows Server 2008 and Windows Vista:  This value is not used.</remarks>
            WTSOutgoingBytes = 20,
            /// <summary>
            /// This value returns FALSE. If you call GetLastError to get extended error information, 
            /// GetLastError returns ERROR_NOT_SUPPORTED.
            /// </summary>
            /// <remarks>Windows Server 2008 and Windows Vista:  This value is not used.</remarks>
            WTSIncomingFrames = 21,
            /// <summary>
            /// This value returns FALSE. If you call GetLastError to get extended error information, 
            /// GetLastError returns ERROR_NOT_SUPPORTED.
            /// </summary>
            /// <remarks>Windows Server 2008 and Windows Vista:  This value is not used.</remarks>
            WTSOutgoingFrames = 22,
            /// <summary>
            /// Information about a Remote Desktop Connection (RDC) client. For more information, 
            /// see WTSCLIENT.
            /// </summary>
            WTSClientInfo = 23,
            /// <summary>
            /// Information about a client session on a RD Session Host server. For more information, 
            /// see WTSINFO.
            /// </summary>
            WTSSessionInfo = 24,
            /// <summary>
            /// Extended information about a session on a RD Session Host server. For more information, 
            /// see WTSINFOEX.
            /// </summary>
            /// <remarks>Windows Server 2008 and Windows Vista:  This value is not supported.</remarks>
            WTSSessionInfoEx = 25,
            /// <summary>
            /// A WTSCONFIGINFO structure that contains information about the configuration of a RD 
            /// Session Host server.
            /// </summary>
            /// <remarks>Windows Server 2008 and Windows Vista:  This value is not supported.</remarks>
            WTSConfigInfo = 26,
            /// <summary>
            /// This value is not supported.
            /// </summary>
            WTSValidationInfo = 27,
            /// <summary>
            /// A WTS_SESSION_ADDRESS structure that contains the IPv4 address assigned to the session. 
            /// If the session does not have a virtual IP address, the WTSQuerySessionInformation function 
            /// returns ERROR_NOT_SUPPORTED.
            /// </summary>
            /// <remarks>Windows Server 2008 and Windows Vista:  This value is not supported.</remarks>
            WTSSessionAddressV4 = 28,
            /// <summary>
            /// Determines whether the current session is a remote session. The WTSQuerySessionInformation 
            /// function returns a value of TRUE to indicate that the current session is a remote session, 
            /// and FALSE to indicate that the current session is a local session. This value can only be 
            /// used for the local machine, so the hServer parameter of the WTSQuerySessionInformation 
            /// function must contain WTS_CURRENT_SERVER_HANDLE.
            /// </summary>
            /// <remarks>Windows Server 2008 and Windows Vista:  This value is not supported.</remarks>
            WTSIsRemoteSession = 29
        }

        /// <summary>
        /// Specifies the connection state of a Remote Desktop Services session.
        /// </summary>;
        /// <remarks>
        /// Only WTSActive represents a fully connected user session. All other
        /// states represent a disconnected user session.
        /// </remarks>
        public enum WTS_CONNECTSTATE_CLASS
        {
            /// <summary>
            /// A user is logged on to the WinStation.
            /// </summary>
            WTSActive = 0,
            /// <summary>
            /// The WinStation is connected to the client.
            /// </summary>
            WTSConnected = 1,
            /// <summary>
            /// The WinStation is in the process of connecting to the client.
            /// </summary>
            WTSConnectQuery = 2,
            /// <summary>
            /// The WinStation is shadowing another WinStation.
            /// </summary>
            WTSShadow = 3,
            /// <summary>
            /// The WinStation is active but the client is disconnected.
            /// </summary>
            WTSDisconnected = 4,
            /// <summary>
            /// The WinStation is waiting for a client to connect.
            /// </summary>
            WTSIdle = 5,
            /// <summary>
            /// The WinStation is listening for a connection. A listener session waits for requests for 
            /// new client connections. 
            /// No user is logged on a listener session. A listener session cannot be reset, shadowed, or 
            /// changed to a regular client session.
            /// </summary>
            WTSListen = 6,
            /// <summary>
            /// The WinStation is being reset.
            /// </summary>
            WTSReset = 7,
            /// <summary>
            /// The WinStation is down due to an error.
            /// </summary>
            WTSDown = 8,
            /// <summary>
            /// The WinStation is initializing.
            /// </summary>
            WTSInit = 9
        }

        /// <summary>
        /// Specifies the current server
        /// </summary>
        public static readonly IntPtr WTS_CURRENT_SERVER_HANDLE = IntPtr.Zero;

        /// <summary>
        /// Specifies the current session (SessionId)
        /// </summary>
        public const int WTS_CURRENT_SESSION = -1;
    }
}

