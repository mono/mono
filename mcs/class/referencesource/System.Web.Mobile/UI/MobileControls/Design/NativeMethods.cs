//------------------------------------------------------------------------------
// <copyright file="NativeMethods.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls 
{
    using System.Runtime.InteropServices;
    using System;
    using System.Security.Permissions;
    using System.Collections;
    using System.IO;
    using System.Text;

    [System.Runtime.InteropServices.ComVisible(false)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class NativeMethods {

        internal const int WS_EX_STATICEDGE = 0x00020000;
        internal const int S_OK =      0x00000000;
        internal const int S_FALSE =   0x00000001;
        internal const int E_NOTIMPL = unchecked((int)0x80004001);
        internal const int E_NOINTERFACE = unchecked((int)0x80004002);
        internal const int E_INVALIDARG = unchecked((int)0x80070057);
        internal const int E_FAIL = unchecked((int)0x80004005);

        internal const int
            OLEIVERB_PRIMARY = 0,
            OLEIVERB_SHOW = -1,
            OLEIVERB_OPEN = -2,
            OLEIVERB_HIDE = -3,
            OLEIVERB_UIACTIVATE = -4,
            OLEIVERB_INPLACEACTIVATE = -5,
            OLEIVERB_DISCARDUNDOSTATE = -6,
            OLEIVERB_PROPERTIES = -7;

        internal const int
            OLECLOSE_SAVEIFDIRTY = 0,
            OLECLOSE_NOSAVE = 1,
            OLECLOSE_PROMPTSAVE = 2;

        internal const int
            PM_NOREMOVE = 0x0000,
            PM_REMOVE = 0x0001;

        internal const int
            WM_CHAR = 0x0102;

        [DllImport(ExternDll.User32, ExactSpelling=true, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
        internal static extern bool GetClientRect(IntPtr hWnd, [In, Out] ref RECT rect);

        [DllImport(ExternDll.User32, ExactSpelling=true, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
        internal static extern bool GetClientRect(IntPtr hWnd, [In, Out] COMRECT rect);

        [StructLayout(LayoutKind.Sequential)]
        [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
        internal class POINT 
        {
            internal int x;
            internal int y;

            internal POINT() 
            {
            }

            internal POINT(int x, int y) 
            {
                this.x = x;
                this.y = y;
            }
        }
        [StructLayout(LayoutKind.Sequential)]
        internal struct POINTL 
        {
            internal int x;
            internal int y;
        }

        [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
        internal class DOCHOSTUIDBLCLICK 
        {
            internal const int DEFAULT = 0x0;
            internal const int SHOWPROPERTIES = 0x1;
            internal const int SHOWCODE = 0x2;
        }

        [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
        internal class DOCHOSTUIFLAG 
        {
            internal const int DIALOG = 0x1;
            internal const int DISABLE_HELP_MENU = 0x2;
            internal const int NO3DBORDER = 0x4;
            internal const int SCROLL_NO = 0x8;
            internal const int DISABLE_SCRIPT_INACTIVE = 0x10;
            internal const int OPENNEWWIN = 0x20;
            internal const int DISABLE_OFFSCREEN = 0x40;
            internal const int FLAT_SCROLLBAR = 0x80;
            internal const int DIV_BLOCKDEFAULT = 0x100;
            internal const int ACTIVATE_CLIENTHIT_ONLY = 0x200;
            internal const int DISABLE_COOKIE = 0x400;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct RECT 
        {
            internal int left;
            internal int top;
            internal int right;
            internal int bottom;

            internal RECT(int left, int top, int right, int bottom) 
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }

            internal static RECT FromXYWH(int x, int y, int width, int height) 
            {
                return new RECT(x,
                    y,
                    x + width,
                    y + height);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct MSG 
        {
            internal IntPtr hwnd;
            internal int  message;
            internal IntPtr wParam;
            internal IntPtr lParam;
            internal int  time;
            // pt was a by-value POINT structure
            internal int  pt_x;
            internal int  pt_y;
        }

        [System.Runtime.InteropServices.ComVisible(false), StructLayout(LayoutKind.Sequential)]
        internal sealed class FORMATETC 
        {
            [MarshalAs(UnmanagedType.I4)]
            internal   int cfFormat;
            [MarshalAs(UnmanagedType.I4)]
            internal   IntPtr ptd;
            [MarshalAs(UnmanagedType.I4)]
            internal   int dwAspect;
            [MarshalAs(UnmanagedType.I4)]
            internal   int lindex;
            [MarshalAs(UnmanagedType.I4)]
            internal   int tymed;
        }

        [System.Runtime.InteropServices.ComVisible(false), StructLayout(LayoutKind.Sequential)]
        [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class STGMEDIUM 
        {
            [MarshalAs(UnmanagedType.I4)]
            internal   int tymed;
            internal   IntPtr unionmember;
            internal   IntPtr pUnkForRelease;
        }

        [System.Runtime.InteropServices.ComVisible(false), StructLayout(LayoutKind.Sequential)/*leftover(noAutoOffset)*/]
        internal sealed class tagOLEVERB 
        {
            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.I4)/*leftover(offset=0, lVerb)*/]
            internal int lVerb;

            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)/*leftover(offset=4, customMarshal="UniStringMarshaller", lpszVerbName)*/]
            internal String lpszVerbName;

            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.U4)/*leftover(offset=8, fuFlags)*/]
            internal int fuFlags;

            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.U4)/*leftover(offset=12, grfAttribs)*/]
            internal int grfAttribs;
        }

        [System.Runtime.InteropServices.ComVisible(false), StructLayout(LayoutKind.Sequential)]
        internal sealed class OLECMD 
        {
            [MarshalAs(UnmanagedType.U4)]
            internal   int cmdID;
            [MarshalAs(UnmanagedType.U4)]
            internal   int cmdf;
        }

        [System.Runtime.InteropServices.ComVisible(false), StructLayout(LayoutKind.Sequential)]
        internal sealed class tagLOGPALETTE 
        {
            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.U2)]
            internal short palVersion;
            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.U2)]
            internal short palNumEntries;

            // UNMAPPABLE: palPalEntry: Cannot be used as a structure field.
            //  /* @com.structmap(UNMAPPABLE palPalEntry) */
            //  internal UNMAPPABLE palPalEntry;
        }

        [System.Runtime.InteropServices.ComVisible(true), StructLayout(LayoutKind.Sequential)]
        internal sealed class tagSIZEL 
        {
            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.I4)]
            internal int cx;
            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.I4)]
            internal int cy;
        }

        [StructLayout(LayoutKind.Sequential)]
        [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class COMRECT 
        {
            internal int left;
            internal int top;
            internal int right;
            internal int bottom;

            internal COMRECT() 
            {
            }

            internal COMRECT(int left, int top, int right, int bottom) 
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }

            internal static COMRECT FromXYWH(int x, int y, int width, int height) 
            {
                return new COMRECT(x,
                    y,
                    x + width,
                    y + height);
            }
        }

        [System.Runtime.InteropServices.ComVisible(false), StructLayout(LayoutKind.Sequential)]
        internal sealed class tagSIZE 
        {
            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.I4)]
            internal int cx;
            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.I4)]
            internal int cy;
        }

        [System.Runtime.InteropServices.ComVisible(false), StructLayout(LayoutKind.Sequential)]
        internal sealed class tagOIFI 
        {
            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.U4)]
            internal int cb;
            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.I4)]
            internal int fMDIApp;
            internal IntPtr hwndFrame;
            internal IntPtr hAccel;
            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.U4)]
            internal int cAccelEntries;
        }

        [System.Runtime.InteropServices.ComVisible(true), StructLayout(LayoutKind.Sequential)]
        [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class DOCHOSTUIINFO 
        {
            [MarshalAs(UnmanagedType.U4)]
            internal int cbSize;
            [MarshalAs(UnmanagedType.I4)]
            internal int dwFlags;
            [MarshalAs(UnmanagedType.I4)]
            internal int dwDoubleClick;
            [MarshalAs(UnmanagedType.I4)]
            internal int dwReserved1;
            [MarshalAs(UnmanagedType.I4)]
            internal int dwReserved2;
        }

        [System.Runtime.InteropServices.ComVisible(true), System.Runtime.InteropServices.ComImport(), Guid("0000000C-0000-0000-C000-000000000046"), System.Runtime.InteropServices.InterfaceTypeAttribute(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IStream 
        {

            [return: MarshalAs(UnmanagedType.I4)]
            int Read(
                [In] 
                IntPtr buf,
                [In, MarshalAs(UnmanagedType.I4)] 
                int len);

            [return: MarshalAs(UnmanagedType.I4)]
            int Write(
                [In] 
                IntPtr buf,
                [In, MarshalAs(UnmanagedType.I4)] 
                int len);

            [return: MarshalAs(UnmanagedType.I8)]
            long Seek(
                [In, MarshalAs(UnmanagedType.I8)] 
                long dlibMove,
                [In, MarshalAs(UnmanagedType.I4)] 
                int dwOrigin);

            
            void SetSize(
                [In, MarshalAs(UnmanagedType.I8)] 
                long libNewSize);

            [return: MarshalAs(UnmanagedType.I8)]
            long CopyTo(
                [In, MarshalAs(UnmanagedType.Interface)] 
                IStream pstm,
                [In, MarshalAs(UnmanagedType.I8)] 
                long cb,
                [Out, MarshalAs(UnmanagedType.LPArray)] 
                long[] pcbRead);

            
            void Commit(
                [In, MarshalAs(UnmanagedType.I4)] 
                int grfCommitFlags);

            
            void Revert();

            
            void LockRegion(
                [In, MarshalAs(UnmanagedType.I8)] 
                long libOffset,
                [In, MarshalAs(UnmanagedType.I8)] 
                long cb,
                [In, MarshalAs(UnmanagedType.I4)] 
                int dwLockType);

            
            void UnlockRegion(
                [In, MarshalAs(UnmanagedType.I8)] 
                long libOffset,
                [In, MarshalAs(UnmanagedType.I8)] 
                long cb,
                [In, MarshalAs(UnmanagedType.I4)] 
                int dwLockType);

            
            void Stat(
                [In] 
                IntPtr pStatstg,
                [In, MarshalAs(UnmanagedType.I4)] 
                int grfStatFlag);

            [return: MarshalAs(UnmanagedType.Interface)]
            IStream Clone();
        }

		[System.Runtime.InteropServices.ComVisible(true), System.Runtime.InteropServices.ComImport(), Guid("3050F1FF-98B5-11CF-BB82-00AA00BDCE0B"), System.Runtime.InteropServices.InterfaceTypeAttribute(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsDual)]
		internal interface IHTMLElement 
		{
			void SetAttribute(
				[In, MarshalAs(UnmanagedType.BStr)]
				string strAttributeName,
				[In, MarshalAs(UnmanagedType.Struct)]
				Object AttributeValue,
				[In, MarshalAs(UnmanagedType.I4)]
				int lFlags);

            
			void GetAttribute(
				[In, MarshalAs(UnmanagedType.BStr)]
				string strAttributeName,
				[In, MarshalAs(UnmanagedType.I4)]
				int lFlags,
				[Out, MarshalAs(UnmanagedType.LPArray)]
				Object[] pvars);

			[return: MarshalAs(UnmanagedType.Bool)]
			bool RemoveAttribute(
				[In, MarshalAs(UnmanagedType.BStr)]
				string strAttributeName,
				[In, MarshalAs(UnmanagedType.I4)]
				int lFlags);

            
			void SetClassName(
				[In, MarshalAs(UnmanagedType.BStr)]
				string p);

			[return: MarshalAs(UnmanagedType.BStr)]
			string GetClassName();

            
			void SetId(
				[In, MarshalAs(UnmanagedType.BStr)]
				string p);

			[return: MarshalAs(UnmanagedType.BStr)]
			string GetId();

			[return: MarshalAs(UnmanagedType.BStr)]
			string GetTagName();

			[return: MarshalAs(UnmanagedType.Interface)]
			IHTMLElement GetParentElement();

			[return: MarshalAs(UnmanagedType.Interface)]
			IHTMLStyle GetStyle();

            
			void SetOnhelp(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object p);

			[return: MarshalAs(UnmanagedType.Struct)]
			Object GetOnhelp();

            
			void SetOnclick(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object p);

			[return: MarshalAs(UnmanagedType.Struct)]
			Object GetOnclick();

            
			void SetOndblclick(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object p);

			[return: MarshalAs(UnmanagedType.Struct)]
			Object GetOndblclick();

            
			void SetOnkeydown(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object p);

			[return: MarshalAs(UnmanagedType.Struct)]
			Object GetOnkeydown();

            
			void SetOnkeyup(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object p);

			[return: MarshalAs(UnmanagedType.Struct)]
			Object GetOnkeyup();

            
			void SetOnkeypress(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object p);

			[return: MarshalAs(UnmanagedType.Struct)]
			Object GetOnkeypress();

            
			void SetOnmouseout(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object p);

			[return: MarshalAs(UnmanagedType.Struct)]
			Object GetOnmouseout();

            
			void SetOnmouseover(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object p);

			[return: MarshalAs(UnmanagedType.Struct)]
			Object GetOnmouseover();

            
			void SetOnmousemove(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object p);

			[return: MarshalAs(UnmanagedType.Struct)]
			Object GetOnmousemove();

            
			void SetOnmousedown(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object p);

			[return: MarshalAs(UnmanagedType.Struct)]
			Object GetOnmousedown();

            
			void SetOnmouseup(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object p);

			[return: MarshalAs(UnmanagedType.Struct)]
			Object GetOnmouseup();

			[return: MarshalAs(UnmanagedType.Interface)]
			object GetDocument();

            
			void SetTitle(
				[In, MarshalAs(UnmanagedType.BStr)]
				string p);

			[return: MarshalAs(UnmanagedType.BStr)]
			string GetTitle();

            
			void SetLanguage(
				[In, MarshalAs(UnmanagedType.BStr)]
				string p);

			[return: MarshalAs(UnmanagedType.BStr)]
			string GetLanguage();

            
			void SetOnselectstart(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object p);

			[return: MarshalAs(UnmanagedType.Struct)]
			Object GetOnselectstart();

            
			void ScrollIntoView(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object varargStart);

			[return: MarshalAs(UnmanagedType.Bool)]
			bool Contains(
				[In, MarshalAs(UnmanagedType.Interface)]
				IHTMLElement pChild);

			[return: MarshalAs(UnmanagedType.I4)]
			int GetSourceIndex();

			[return: MarshalAs(UnmanagedType.Struct)]
			Object GetRecordNumber();

            
			void SetLang(
				[In, MarshalAs(UnmanagedType.BStr)]
				string p);

			[return: MarshalAs(UnmanagedType.BStr)]
			string GetLang();

			[return: MarshalAs(UnmanagedType.I4)]
			int GetOffsetLeft();

			[return: MarshalAs(UnmanagedType.I4)]
			int GetOffsetTop();

			[return: MarshalAs(UnmanagedType.I4)]
			int GetOffsetWidth();

			[return: MarshalAs(UnmanagedType.I4)]
			int GetOffsetHeight();

			[return: MarshalAs(UnmanagedType.Interface)]
			IHTMLElement GetOffsetParent();

            
			void SetInnerHTML(
				[In, MarshalAs(UnmanagedType.BStr)]
				string p);

			[return: MarshalAs(UnmanagedType.BStr)]
			string GetInnerHTML();

            
			void SetInnerText(
				[In, MarshalAs(UnmanagedType.BStr)]
				string p);

			[return: MarshalAs(UnmanagedType.BStr)]
			string GetInnerText();

            
			void SetOuterHTML(
				[In, MarshalAs(UnmanagedType.BStr)]
				string p);

			[return: MarshalAs(UnmanagedType.BStr)]
			string GetOuterHTML();

            
			void SetOuterText(
				[In, MarshalAs(UnmanagedType.BStr)]
				string p);

			[return: MarshalAs(UnmanagedType.BStr)]
			string GetOuterText();

            
			void InsertAdjacentHTML(
				[In, MarshalAs(UnmanagedType.BStr)]
				string @where,
				[In, MarshalAs(UnmanagedType.BStr)]
				string html);

            
			void InsertAdjacentText(
				[In, MarshalAs(UnmanagedType.BStr)]
				string @where,
				[In, MarshalAs(UnmanagedType.BStr)]
				string text);

			[return: MarshalAs(UnmanagedType.Interface)]
			IHTMLElement GetParentTextEdit();

			[return: MarshalAs(UnmanagedType.Bool)]
			bool GetIsTextEdit();

            
			void Click();

			[return: MarshalAs(UnmanagedType.Interface)]
			object GetFilters();
			// 

            
			void SetOndragstart(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object p);

			[return: MarshalAs(UnmanagedType.Struct)]
			Object GetOndragstart();

			[return: MarshalAs(UnmanagedType.BStr)]
			string toString();

            
			void SetOnbeforeupdate(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object p);

			[return: MarshalAs(UnmanagedType.Struct)]
			Object GetOnbeforeupdate();

            
			void SetOnafterupdate(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object p);

			[return: MarshalAs(UnmanagedType.Struct)]
			Object GetOnafterupdate();

            
			void SetOnerrorupdate(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object p);

			[return: MarshalAs(UnmanagedType.Struct)]
			Object GetOnerrorupdate();

            
			void SetOnrowexit(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object p);

			[return: MarshalAs(UnmanagedType.Struct)]
			Object GetOnrowexit();

            
			void SetOnrowenter(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object p);

			[return: MarshalAs(UnmanagedType.Struct)]
			Object GetOnrowenter();

            
			void SetOndatasetchanged(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object p);

			[return: MarshalAs(UnmanagedType.Struct)]
			Object GetOndatasetchanged();

            
			void SetOndataavailable(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object p);

			[return: MarshalAs(UnmanagedType.Struct)]
			Object GetOndataavailable();

            
			void SetOndatasetcomplete(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object p);

			[return: MarshalAs(UnmanagedType.Struct)]
			Object GetOndatasetcomplete();

            
			void SetOnfilterchange(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object p);

			[return: MarshalAs(UnmanagedType.Struct)]
			Object GetOnfilterchange();

			[return: MarshalAs(UnmanagedType.Interface)]
			object GetChildren();

			[return: MarshalAs(UnmanagedType.Interface)]
			object GetAll();
		}

		[System.Runtime.InteropServices.ComVisible(true), System.Runtime.InteropServices.ComImport(), Guid("332C4425-26CB-11D0-B483-00C04FD90119"), System.Runtime.InteropServices.InterfaceTypeAttribute(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsDual)]
		internal interface IHTMLDocument2 
		{
			[return: MarshalAs(UnmanagedType.Interface)]
			object GetScript();

			[return: MarshalAs(UnmanagedType.Interface)]
			IHTMLElementCollection GetAll();

			[return: MarshalAs(UnmanagedType.Interface)]
			IHTMLElement GetBody();

			[return: MarshalAs(UnmanagedType.Interface)]
			IHTMLElement GetActiveElement();

			[return: MarshalAs(UnmanagedType.Interface)]
			IHTMLElementCollection GetImages();

			[return: MarshalAs(UnmanagedType.Interface)]
			IHTMLElementCollection GetApplets();

			[return: MarshalAs(UnmanagedType.Interface)]
			IHTMLElementCollection GetLinks();

			[return: MarshalAs(UnmanagedType.Interface)]
			IHTMLElementCollection GetForms();

			[return: MarshalAs(UnmanagedType.Interface)]
			IHTMLElementCollection GetAnchors();

            
			void SetTitle(
				[In, MarshalAs(UnmanagedType.BStr)]
				string p);

			[return: MarshalAs(UnmanagedType.BStr)]
			string GetTitle();

			[return: MarshalAs(UnmanagedType.Interface)]
			IHTMLElementCollection GetScripts();

            
			void SetDesignMode(
				[In, MarshalAs(UnmanagedType.BStr)]
				string p);

			[return: MarshalAs(UnmanagedType.BStr)]
			string GetDesignMode();

			[return: MarshalAs(UnmanagedType.Interface)]
			object GetSelection();
			// 

			[return: MarshalAs(UnmanagedType.BStr)]
			string GetReadyState();

			[return: MarshalAs(UnmanagedType.Interface)]
			object GetFrames();
			// 

			[return: MarshalAs(UnmanagedType.Interface)]
			IHTMLElementCollection GetEmbeds();

			[return: MarshalAs(UnmanagedType.Interface)]
			IHTMLElementCollection GetPlugins();

            
			void SetAlinkColor(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object p);

			[return: MarshalAs(UnmanagedType.Struct)]
			object GetAlinkColor();

            
			void SetBgColor(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object p);

			[return: MarshalAs(UnmanagedType.Struct)]
			object GetBgColor();

            
			void SetFgColor(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object p);

			[return: MarshalAs(UnmanagedType.Struct)]
			object GetFgColor();

            
			void SetLinkColor(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object p);

			[return: MarshalAs(UnmanagedType.Struct)]
			object GetLinkColor();

            
			void SetVlinkColor(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object p);

			[return: MarshalAs(UnmanagedType.Struct)]
			Object GetVlinkColor();

			[return: MarshalAs(UnmanagedType.BStr)]
			string GetReferrer();

			[return: MarshalAs(UnmanagedType.Interface)]
			object GetLocation();
			// 

			[return: MarshalAs(UnmanagedType.BStr)]
			string GetLastModified();

            
			void SetURL(
				[In, MarshalAs(UnmanagedType.BStr)]
				string p);

			[return: MarshalAs(UnmanagedType.BStr)]
			string GetURL();

            
			void SetDomain(
				[In, MarshalAs(UnmanagedType.BStr)]
				string p);

			[return: MarshalAs(UnmanagedType.BStr)]
			string GetDomain();

            
			void SetCookie(
				[In, MarshalAs(UnmanagedType.BStr)]
				string p);

			[return: MarshalAs(UnmanagedType.BStr)]
			string GetCookie();

            
			void SetExpando(
				[In, MarshalAs(UnmanagedType.Bool)]
				bool p);

			[return: MarshalAs(UnmanagedType.Bool)]
			bool GetExpando();

            
			void SetCharset(
				[In, MarshalAs(UnmanagedType.BStr)]
				string p);

			[return: MarshalAs(UnmanagedType.BStr)]
			string GetCharset();

            
			void SetDefaultCharset(
				[In, MarshalAs(UnmanagedType.BStr)]
				string p);

			[return: MarshalAs(UnmanagedType.BStr)]
			string GetDefaultCharset();

			[return: MarshalAs(UnmanagedType.BStr)]
			string GetMimeType();

			[return: MarshalAs(UnmanagedType.BStr)]
			string GetFileSize();

			[return: MarshalAs(UnmanagedType.BStr)]
			string GetFileCreatedDate();

			[return: MarshalAs(UnmanagedType.BStr)]
			string GetFileModifiedDate();

			[return: MarshalAs(UnmanagedType.BStr)]
			string GetFileUpdatedDate();

			[return: MarshalAs(UnmanagedType.BStr)]
			string GetSecurity();

			[return: MarshalAs(UnmanagedType.BStr)]
			string GetProtocol();

			[return: MarshalAs(UnmanagedType.BStr)]
			string GetNameProp();

            
			void DummyWrite(
				[In, MarshalAs(UnmanagedType.I4)]
				int psarray);

            
			void DummyWriteln(
				[In, MarshalAs(UnmanagedType.I4)]
				int psarray);

			[return: MarshalAs(UnmanagedType.Interface)]
			object Open(
				[In, MarshalAs(UnmanagedType.BStr)]
				string URL,
				[In, MarshalAs(UnmanagedType.Struct)]
				Object name,
				[In, MarshalAs(UnmanagedType.Struct)]
				Object features,
				[In, MarshalAs(UnmanagedType.Struct)]
				Object replace);

            
			void Close();

            
			void Clear();

			[return: MarshalAs(UnmanagedType.Bool)]
			bool QueryCommandSupported(
				[In, MarshalAs(UnmanagedType.BStr)]
				string cmdID);

			[return: MarshalAs(UnmanagedType.Bool)]
			bool QueryCommandEnabled(
				[In, MarshalAs(UnmanagedType.BStr)]
				string cmdID);

			[return: MarshalAs(UnmanagedType.Bool)]
			bool QueryCommandState(
				[In, MarshalAs(UnmanagedType.BStr)]
				string cmdID);

			[return: MarshalAs(UnmanagedType.Bool)]
			bool QueryCommandIndeterm(
				[In, MarshalAs(UnmanagedType.BStr)]
				string cmdID);

			[return: MarshalAs(UnmanagedType.BStr)]
			string QueryCommandText(
				[In, MarshalAs(UnmanagedType.BStr)]
				string cmdID);

			[return: MarshalAs(UnmanagedType.Struct)]
			Object QueryCommandValue(
				[In, MarshalAs(UnmanagedType.BStr)]
				string cmdID);

			[return: MarshalAs(UnmanagedType.Bool)]
			bool ExecCommand(
				[In, MarshalAs(UnmanagedType.BStr)]
				string cmdID,
				[In, MarshalAs(UnmanagedType.Bool)]
				bool showUI,
				[In, MarshalAs(UnmanagedType.Struct)]
				Object value);

			[return: MarshalAs(UnmanagedType.Bool)]
			bool ExecCommandShowHelp(
				[In, MarshalAs(UnmanagedType.BStr)]
				string cmdID);

			[return: MarshalAs(UnmanagedType.Interface)]
			IHTMLElement CreateElement(
				[In, MarshalAs(UnmanagedType.BStr)]
				string eTag);

            
			void SetOnhelp(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object p);

			[return: MarshalAs(UnmanagedType.Struct)]
			Object GetOnhelp();

            
			void SetOnclick(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object p);

			[return: MarshalAs(UnmanagedType.Struct)]
			Object GetOnclick();

            
			void SetOndblclick(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object p);

			[return: MarshalAs(UnmanagedType.Struct)]
			Object GetOndblclick();

            
			void SetOnkeyup(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object p);

			[return: MarshalAs(UnmanagedType.Struct)]
			Object GetOnkeyup();

            
			void SetOnkeydown(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object p);

			[return: MarshalAs(UnmanagedType.Struct)]
			Object GetOnkeydown();

            
			void SetOnkeypress(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object p);

			[return: MarshalAs(UnmanagedType.Struct)]
			Object GetOnkeypress();

            
			void SetOnmouseup(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object p);

			[return: MarshalAs(UnmanagedType.Struct)]
			Object GetOnmouseup();

            
			void SetOnmousedown(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object p);

			[return: MarshalAs(UnmanagedType.Struct)]
			Object GetOnmousedown();

            
			void SetOnmousemove(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object p);

			[return: MarshalAs(UnmanagedType.Struct)]
			Object GetOnmousemove();

            
			void SetOnmouseout(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object p);

			[return: MarshalAs(UnmanagedType.Struct)]
			Object GetOnmouseout();

            
			void SetOnmouseover(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object p);

			[return: MarshalAs(UnmanagedType.Struct)]
			Object GetOnmouseover();

            
			void SetOnreadystatechange(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object p);

			[return: MarshalAs(UnmanagedType.Struct)]
			Object GetOnreadystatechange();

            
			void SetOnafterupdate(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object p);

			[return: MarshalAs(UnmanagedType.Struct)]
			Object GetOnafterupdate();

            
			void SetOnrowexit(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object p);

			[return: MarshalAs(UnmanagedType.Struct)]
			Object GetOnrowexit();

            
			void SetOnrowenter(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object p);

			[return: MarshalAs(UnmanagedType.Struct)]
			Object GetOnrowenter();

            
			void SetOndragstart(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object p);

			[return: MarshalAs(UnmanagedType.Struct)]
			Object GetOndragstart();

            
			void SetOnselectstart(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object p);

			[return: MarshalAs(UnmanagedType.Struct)]
			Object GetOnselectstart();

			[return: MarshalAs(UnmanagedType.Interface)]
			IHTMLElement ElementFromPoint(
				[In, MarshalAs(UnmanagedType.I4)]
				int x,
				[In, MarshalAs(UnmanagedType.I4)]
				int y);

			[return: MarshalAs(UnmanagedType.Interface)]
				/*IHTMLWindow2*/ object GetParentWindow();

			[return: MarshalAs(UnmanagedType.Interface)]
			object GetStyleSheets();
			// 

            
			void SetOnbeforeupdate(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object p);

			[return: MarshalAs(UnmanagedType.Struct)]
			Object GetOnbeforeupdate();

            
			void SetOnerrorupdate(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object p);

			[return: MarshalAs(UnmanagedType.Struct)]
			Object GetOnerrorupdate();

			[return: MarshalAs(UnmanagedType.BStr)]
			string toString();

			[return: MarshalAs(UnmanagedType.Interface)]
			IHTMLStyleSheet CreateStyleSheet(
				[In, MarshalAs(UnmanagedType.BStr)]
				string bstrHref,
				[In, MarshalAs(UnmanagedType.I4)]
				int lIndex);
		}

		[System.Runtime.InteropServices.ComVisible(true), System.Runtime.InteropServices.ComImport(), Guid("3050F485-98B5-11CF-BB82-00AA00BDCE0B"), System.Runtime.InteropServices.InterfaceTypeAttribute(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsDual)]
		internal interface IHTMLDocument3 
		{
			void ReleaseCapture();

            
			void Recalc(
				[In, MarshalAs(UnmanagedType.Bool)]
				bool fForce);

			[return: MarshalAs(UnmanagedType.Interface)]
			IHTMLDOMNode CreateTextNode(
				[In, MarshalAs(UnmanagedType.BStr)]
				string text);

			[return: MarshalAs(UnmanagedType.Interface)]
			IHTMLElement GetDocumentElement();

			[return: MarshalAs(UnmanagedType.BStr)]
			string GetUniqueID();

			[return: MarshalAs(UnmanagedType.Bool)]
			bool AttachEvent(
				[In, MarshalAs(UnmanagedType.BStr)]
				string ev,
				[In, MarshalAs(UnmanagedType.Interface)]
				object pdisp);

            
			void DetachEvent(
				[In, MarshalAs(UnmanagedType.BStr)]
				string ev,
				[In, MarshalAs(UnmanagedType.Interface)]
				object pdisp);

            
			void SetOnrowsdelete(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object p);

			[return: MarshalAs(UnmanagedType.Struct)]
			Object GetOnrowsdelete();

            
			void SetOnrowsinserted(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object p);

			[return: MarshalAs(UnmanagedType.Struct)]
			Object GetOnrowsinserted();

            
			void SetOncellchange(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object p);

			[return: MarshalAs(UnmanagedType.Struct)]
			Object GetOncellchange();

            
			void SetOndatasetchanged(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object p);

			[return: MarshalAs(UnmanagedType.Struct)]
			Object GetOndatasetchanged();

            
			void SetOndataavailable(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object p);

			[return: MarshalAs(UnmanagedType.Struct)]
			Object GetOndataavailable();

            
			void SetOndatasetcomplete(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object p);

			[return: MarshalAs(UnmanagedType.Struct)]
			Object GetOndatasetcomplete();

            
			void SetOnpropertychange(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object p);

			[return: MarshalAs(UnmanagedType.Struct)]
			Object GetOnpropertychange();

            
			void SetDir(
				[In, MarshalAs(UnmanagedType.BStr)]
				string p);

			[return: MarshalAs(UnmanagedType.BStr)]
			string GetDir();

            
			void SetOncontextmenu(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object p);

			[return: MarshalAs(UnmanagedType.Struct)]
			Object GetOncontextmenu();

            
			void SetOnstop(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object p);

			[return: MarshalAs(UnmanagedType.Struct)]
			Object GetOnstop();

			[return: MarshalAs(UnmanagedType.Interface)]
			IHTMLDocument2 CreateDocumentFragment();

			[return: MarshalAs(UnmanagedType.Interface)]
			IHTMLDocument2 GetParentDocument();

            
			void SetEnableDownload(
				[In, MarshalAs(UnmanagedType.Bool)]
				bool p);

			[return: MarshalAs(UnmanagedType.Bool)]
			bool GetEnableDownload();

            
			void SetBaseUrl(
				[In, MarshalAs(UnmanagedType.BStr)]
				string p);

			[return: MarshalAs(UnmanagedType.BStr)]
			string GetBaseUrl();

			[return: MarshalAs(UnmanagedType.Interface)]
			object GetChildNodes();

            
			void SetInheritStyleSheets(
				[In, MarshalAs(UnmanagedType.Bool)]
				bool p);

			[return: MarshalAs(UnmanagedType.Bool)]
			bool GetInheritStyleSheets();

            
			void SetOnbeforeeditfocus(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object p);

			[return: MarshalAs(UnmanagedType.Struct)]
			Object GetOnbeforeeditfocus();

			[return: MarshalAs(UnmanagedType.Interface)]
			IHTMLElementCollection GetElementsByName(
				[In, MarshalAs(UnmanagedType.BStr)]
				string v);

			[return: MarshalAs(UnmanagedType.Interface)]
			IHTMLElement GetElementById(
				[In, MarshalAs(UnmanagedType.BStr)]
				string v);

			[return: MarshalAs(UnmanagedType.Interface)]
			IHTMLElementCollection GetElementsByTagName(
				[In, MarshalAs(UnmanagedType.BStr)]
				string v);
		}
		[System.Runtime.InteropServices.ComVisible(true), System.Runtime.InteropServices.ComImport(), Guid("3050F2E3-98B5-11CF-BB82-00AA00BDCE0B"), System.Runtime.InteropServices.InterfaceTypeAttribute(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsDual)]
		internal interface IHTMLStyleSheet 
		{
			void SetTitle(
				[In, MarshalAs(UnmanagedType.BStr)] 
				string p);

			[return: MarshalAs(UnmanagedType.BStr)]
			string GetTitle();

			[return: MarshalAs(UnmanagedType.Interface)]
			IHTMLStyleSheet GetParentStyleSheet();

			[return: MarshalAs(UnmanagedType.Interface)]
			IHTMLElement GetOwningElement();

            
			void SetDisabled(
				[In, MarshalAs(UnmanagedType.Bool)] 
				bool p);

			[return: MarshalAs(UnmanagedType.Bool)]
			bool GetDisabled();

			[return: MarshalAs(UnmanagedType.Bool)]
			bool GetReadOnly();

			[return: MarshalAs(UnmanagedType.Interface)]
			object GetImports();
			// 

            
			void SetHref(
				[In, MarshalAs(UnmanagedType.BStr)] 
				string p);

			[return: MarshalAs(UnmanagedType.BStr)]
			string GetHref();

			[return: MarshalAs(UnmanagedType.BStr)]
			string GetStyleSheetType();

			[return: MarshalAs(UnmanagedType.BStr)]
			string GetId();

			[return: MarshalAs(UnmanagedType.I4)]
			int AddImport(
				[In, MarshalAs(UnmanagedType.BStr)] 
				string bstrURL,
				[In, MarshalAs(UnmanagedType.I4)] 
				int lIndex);

			[return: MarshalAs(UnmanagedType.I4)]
			int AddRule(
				[In, MarshalAs(UnmanagedType.BStr)] 
				string bstrSelector,
				[In, MarshalAs(UnmanagedType.BStr)] 
				string bstrStyle,
				[In, MarshalAs(UnmanagedType.I4)] 
				int lIndex);

            
			void RemoveImport(
				[In, MarshalAs(UnmanagedType.I4)] 
				int lIndex);

            
			void RemoveRule(
				[In, MarshalAs(UnmanagedType.I4)] 
				int lIndex);

            
			void SetMedia(
				[In, MarshalAs(UnmanagedType.BStr)] 
				string p);

			[return: MarshalAs(UnmanagedType.BStr)]
			string GetMedia();

            
			void SetCssText(
				[In, MarshalAs(UnmanagedType.BStr)] 
				string p);

			[return: MarshalAs(UnmanagedType.BStr)]
			string GetCssText();

			[return: MarshalAs(UnmanagedType.Interface)]
			object GetRules();
			// 
		}

        [System.Runtime.InteropServices.ComVisible(true), System.Runtime.InteropServices.ComImport(), Guid("3050F25E-98B5-11CF-BB82-00AA00BDCE0B"), System.Runtime.InteropServices.InterfaceTypeAttribute(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsDual)]
        internal interface IHTMLStyle {
            void SetFontFamily(
                              [In, MarshalAs(UnmanagedType.BStr)]
                              string p);

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetFontFamily();

            
            void SetFontStyle(
                             [In, MarshalAs(UnmanagedType.BStr)]
                             string p);

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetFontStyle();

            
            void SetFontObject(
                              [In, MarshalAs(UnmanagedType.BStr)]
                              string p);

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetFontObject();

            
            void SetFontWeight(
                              [In, MarshalAs(UnmanagedType.BStr)]
                              string p);

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetFontWeight();

            
            void SetFontSize(
                            [In, MarshalAs(UnmanagedType.Struct)]
                            Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetFontSize();

            
            void SetFont(
                        [In, MarshalAs(UnmanagedType.BStr)]
                        string p);

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetFont();

            
            void SetColor(
                         [In, MarshalAs(UnmanagedType.Struct)]
                         Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetColor();

            
            void SetBackground(
                              [In, MarshalAs(UnmanagedType.BStr)]
                              string p);

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetBackground();

            
            void SetBackgroundColor(
                                   [In, MarshalAs(UnmanagedType.Struct)]
                                   Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetBackgroundColor();

            
            void SetBackgroundImage(
                                   [In, MarshalAs(UnmanagedType.BStr)]
                                   string p);

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetBackgroundImage();

            
            void SetBackgroundRepeat(
                                    [In, MarshalAs(UnmanagedType.BStr)]
                                    string p);

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetBackgroundRepeat();

            
            void SetBackgroundAttachment(
                                        [In, MarshalAs(UnmanagedType.BStr)]
                                        string p);

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetBackgroundAttachment();

            
            void SetBackgroundPosition(
                                      [In, MarshalAs(UnmanagedType.BStr)]
                                      string p);

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetBackgroundPosition();

            
            void SetBackgroundPositionX(
                                       [In, MarshalAs(UnmanagedType.Struct)]
                                       Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetBackgroundPositionX();

            
            void SetBackgroundPositionY(
                                       [In, MarshalAs(UnmanagedType.Struct)]
                                       Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetBackgroundPositionY();

            
            void SetWordSpacing(
                               [In, MarshalAs(UnmanagedType.Struct)]
                               Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetWordSpacing();

            
            void SetLetterSpacing(
                                 [In, MarshalAs(UnmanagedType.Struct)]
                                 Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetLetterSpacing();

            
            void SetTextDecoration(
                                  [In, MarshalAs(UnmanagedType.BStr)]
                                  string p);

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetTextDecoration();

            
            void SetTextDecorationNone(
                                      [In, MarshalAs(UnmanagedType.Bool)]
                                      bool p);

            [return: MarshalAs(UnmanagedType.Bool)]
            bool GetTextDecorationNone();

            
            void SetTextDecorationUnderline(
                                           [In, MarshalAs(UnmanagedType.Bool)]
                                           bool p);

            [return: MarshalAs(UnmanagedType.Bool)]
            bool GetTextDecorationUnderline();

            
            void SetTextDecorationOverline(
                                          [In, MarshalAs(UnmanagedType.Bool)]
                                          bool p);

            [return: MarshalAs(UnmanagedType.Bool)]
            bool GetTextDecorationOverline();

            
            void SetTextDecorationLineThrough(
                                             [In, MarshalAs(UnmanagedType.Bool)]
                                             bool p);

            [return: MarshalAs(UnmanagedType.Bool)]
            bool GetTextDecorationLineThrough();

            
            void SetTextDecorationBlink(
                                       [In, MarshalAs(UnmanagedType.Bool)]
                                       bool p);

            [return: MarshalAs(UnmanagedType.Bool)]
            bool GetTextDecorationBlink();

            
            void SetVerticalAlign(
                                 [In, MarshalAs(UnmanagedType.Struct)]
                                 Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetVerticalAlign();

            
            void SetTextTransform(
                                 [In, MarshalAs(UnmanagedType.BStr)]
                                 string p);

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetTextTransform();

            
            void SetTextAlign(
                             [In, MarshalAs(UnmanagedType.BStr)]
                             string p);

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetTextAlign();

            
            void SetTextIndent(
                              [In, MarshalAs(UnmanagedType.Struct)]
                              Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetTextIndent();

            
            void SetLineHeight(
                              [In, MarshalAs(UnmanagedType.Struct)]
                              Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetLineHeight();

            
            void SetMarginTop(
                             [In, MarshalAs(UnmanagedType.Struct)]
                             Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetMarginTop();

            
            void SetMarginRight(
                               [In, MarshalAs(UnmanagedType.Struct)]
                               Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetMarginRight();

            
            void SetMarginBottom(
                                [In, MarshalAs(UnmanagedType.Struct)]
                                Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetMarginBottom();

            
            void SetMarginLeft(
                              [In, MarshalAs(UnmanagedType.Struct)]
                              Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetMarginLeft();

            
            void SetMargin(
                          [In, MarshalAs(UnmanagedType.BStr)]
                          string p);

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetMargin();

            
            void SetPaddingTop(
                              [In, MarshalAs(UnmanagedType.Struct)]
                              Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetPaddingTop();

            
            void SetPaddingRight(
                                [In, MarshalAs(UnmanagedType.Struct)]
                                Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetPaddingRight();

            
            void SetPaddingBottom(
                                 [In, MarshalAs(UnmanagedType.Struct)]
                                 Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetPaddingBottom();

            
            void SetPaddingLeft(
                               [In, MarshalAs(UnmanagedType.Struct)]
                               Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetPaddingLeft();

            
            void SetPadding(
                           [In, MarshalAs(UnmanagedType.BStr)]
                           string p);

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetPadding();

            
            void SetBorder(
                          [In, MarshalAs(UnmanagedType.BStr)]
                          string p);

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetBorder();

            
            void SetBorderTop(
                             [In, MarshalAs(UnmanagedType.BStr)]
                             string p);

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetBorderTop();

            
            void SetBorderRight(
                               [In, MarshalAs(UnmanagedType.BStr)]
                               string p);

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetBorderRight();

            
            void SetBorderBottom(
                                [In, MarshalAs(UnmanagedType.BStr)]
                                string p);

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetBorderBottom();

            
            void SetBorderLeft(
                              [In, MarshalAs(UnmanagedType.BStr)]
                              string p);

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetBorderLeft();

            
            void SetBorderColor(
                               [In, MarshalAs(UnmanagedType.BStr)]
                               string p);

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetBorderColor();

            
            void SetBorderTopColor(
                                  [In, MarshalAs(UnmanagedType.Struct)]
                                  Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetBorderTopColor();

            
            void SetBorderRightColor(
                                    [In, MarshalAs(UnmanagedType.Struct)]
                                    Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetBorderRightColor();

            
            void SetBorderBottomColor(
                                     [In, MarshalAs(UnmanagedType.Struct)]
                                     Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetBorderBottomColor();

            
            void SetBorderLeftColor(
                                   [In, MarshalAs(UnmanagedType.Struct)]
                                   Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetBorderLeftColor();

            
            void SetBorderWidth(
                               [In, MarshalAs(UnmanagedType.BStr)]
                               string p);

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetBorderWidth();

            
            void SetBorderTopWidth(
                                  [In, MarshalAs(UnmanagedType.Struct)]
                                  Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetBorderTopWidth();

            
            void SetBorderRightWidth(
                                    [In, MarshalAs(UnmanagedType.Struct)]
                                    Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetBorderRightWidth();

            
            void SetBorderBottomWidth(
                                     [In, MarshalAs(UnmanagedType.Struct)]
                                     Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetBorderBottomWidth();

            
            void SetBorderLeftWidth(
                                   [In, MarshalAs(UnmanagedType.Struct)]
                                   Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetBorderLeftWidth();

            
            void SetBorderStyle(
                               [In, MarshalAs(UnmanagedType.BStr)]
                               string p);

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetBorderStyle();

            
            void SetBorderTopStyle(
                                  [In, MarshalAs(UnmanagedType.BStr)]
                                  string p);

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetBorderTopStyle();

            
            void SetBorderRightStyle(
                                    [In, MarshalAs(UnmanagedType.BStr)]
                                    string p);

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetBorderRightStyle();

            
            void SetBorderBottomStyle(
                                     [In, MarshalAs(UnmanagedType.BStr)]
                                     string p);

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetBorderBottomStyle();

            
            void SetBorderLeftStyle(
                                   [In, MarshalAs(UnmanagedType.BStr)]
                                   string p);

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetBorderLeftStyle();

            
            void SetWidth(
                         [In, MarshalAs(UnmanagedType.Struct)]
                         Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetWidth();

            
            void SetHeight(
                          [In, MarshalAs(UnmanagedType.Struct)]
                          Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetHeight();

            
            void SetStyleFloat(
                              [In, MarshalAs(UnmanagedType.BStr)]
                              string p);

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetStyleFloat();

            
            void SetClear(
                         [In, MarshalAs(UnmanagedType.BStr)]
                         string p);

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetClear();

            
            void SetDisplay(
                           [In, MarshalAs(UnmanagedType.BStr)]
                           string p);

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetDisplay();

            
            void SetVisibility(
                              [In, MarshalAs(UnmanagedType.BStr)]
                              string p);

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetVisibility();

            
            void SetListStyleType(
                                 [In, MarshalAs(UnmanagedType.BStr)]
                                 string p);

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetListStyleType();

            
            void SetListStylePosition(
                                     [In, MarshalAs(UnmanagedType.BStr)]
                                     string p);

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetListStylePosition();

            
            void SetListStyleImage(
                                  [In, MarshalAs(UnmanagedType.BStr)]
                                  string p);

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetListStyleImage();

            
            void SetListStyle(
                             [In, MarshalAs(UnmanagedType.BStr)]
                             string p);

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetListStyle();

            
            void SetWhiteSpace(
                              [In, MarshalAs(UnmanagedType.BStr)]
                              string p);

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetWhiteSpace();

            
            void SetTop(
                       [In, MarshalAs(UnmanagedType.Struct)]
                       Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetTop();

            
            void SetLeft(
                        [In, MarshalAs(UnmanagedType.Struct)]
                        Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetLeft();

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetPosition();

            
            void SetZIndex(
                          [In, MarshalAs(UnmanagedType.Struct)]
                          Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetZIndex();

            
            void SetOverflow(
                            [In, MarshalAs(UnmanagedType.BStr)]
                            string p);

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetOverflow();

            
            void SetPageBreakBefore(
                                   [In, MarshalAs(UnmanagedType.BStr)]
                                   string p);

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetPageBreakBefore();

            
            void SetPageBreakAfter(
                                  [In, MarshalAs(UnmanagedType.BStr)]
                                  string p);

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetPageBreakAfter();

            
            void SetCssText(
                           [In, MarshalAs(UnmanagedType.BStr)]
                           string p);

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetCssText();

            
            void SetPixelTop(
                            [In, MarshalAs(UnmanagedType.I4)]
                            int p);

            [return: MarshalAs(UnmanagedType.I4)]
            int GetPixelTop();

            
            void SetPixelLeft(
                             [In, MarshalAs(UnmanagedType.I4)]
                             int p);

            [return: MarshalAs(UnmanagedType.I4)]
            int GetPixelLeft();

            
            void SetPixelWidth(
                              [In, MarshalAs(UnmanagedType.I4)]
                              int p);

            [return: MarshalAs(UnmanagedType.I4)]
            int GetPixelWidth();

            
            void SetPixelHeight(
                               [In, MarshalAs(UnmanagedType.I4)]
                               int p);

            [return: MarshalAs(UnmanagedType.I4)]
            int GetPixelHeight();

            
            void SetPosTop(
                          [In, MarshalAs(UnmanagedType.R4)]
                          float p);

            [return: MarshalAs(UnmanagedType.R4)]
            float GetPosTop();

            
            void SetPosLeft(
                           [In, MarshalAs(UnmanagedType.R4)]
                           float p);

            [return: MarshalAs(UnmanagedType.R4)]
            float GetPosLeft();

            
            void SetPosWidth(
                            [In, MarshalAs(UnmanagedType.R4)]
                            float p);

            [return: MarshalAs(UnmanagedType.R4)]
            float GetPosWidth();

            
            void SetPosHeight(
                             [In, MarshalAs(UnmanagedType.R4)]
                             float p);

            [return: MarshalAs(UnmanagedType.R4)]
            float GetPosHeight();

            
            void SetCursor(
                          [In, MarshalAs(UnmanagedType.BStr)]
                          string p);

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetCursor();

            
            void SetClip(
                        [In, MarshalAs(UnmanagedType.BStr)]
                        string p);

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetClip();

            
            void SetFilter(
                          [In, MarshalAs(UnmanagedType.BStr)]
                          string p);

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetFilter();

            
            void SetAttribute(
                             [In, MarshalAs(UnmanagedType.BStr)]
                             string strAttributeName,
                             [In, MarshalAs(UnmanagedType.Struct)]
                             Object AttributeValue,
                             [In, MarshalAs(UnmanagedType.I4)]
                             int lFlags);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetAttribute(
                               [In, MarshalAs(UnmanagedType.BStr)]
                               string strAttributeName,
                               [In, MarshalAs(UnmanagedType.I4)]
                               int lFlags);

            [return: MarshalAs(UnmanagedType.Bool)]
            bool RemoveAttribute(
                                [In, MarshalAs(UnmanagedType.BStr)]
                                string strAttributeName,
                                [In, MarshalAs(UnmanagedType.I4)]
                                int lFlags);

        }

		[System.Runtime.InteropServices.ComVisible(true), System.Runtime.InteropServices.ComImport(), Guid("3050F21F-98B5-11CF-BB82-00AA00BDCE0B"), System.Runtime.InteropServices.InterfaceTypeAttribute(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsDual)]
		internal interface IHTMLElementCollection 
		{
			[return: MarshalAs(UnmanagedType.BStr)]
			string toString();

            
			void SetLength(
				[In, MarshalAs(UnmanagedType.I4)] 
				int p);

			[return: MarshalAs(UnmanagedType.I4)]
			int GetLength();

			[return: MarshalAs(UnmanagedType.Interface)]
			object Get_newEnum();

			[return: MarshalAs(UnmanagedType.Interface)]
			object Item(
				[In, MarshalAs(UnmanagedType.Struct)] 
				Object name,
				[In, MarshalAs(UnmanagedType.Struct)] 
				Object index);

			[return: MarshalAs(UnmanagedType.Interface)]
			object Tags(
				[In, MarshalAs(UnmanagedType.Struct)] 
				Object tagName);
		}

		[System.Runtime.InteropServices.ComVisible(true), System.Runtime.InteropServices.ComImport(), Guid("3050F5DA-98B5-11CF-BB82-00AA00BDCE0B"), System.Runtime.InteropServices.InterfaceTypeAttribute(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsDual)]
		internal interface IHTMLDOMNode 
		{
			[return: MarshalAs(UnmanagedType.I4)]
			int GetNodeType();

			[return: MarshalAs(UnmanagedType.Interface)]
			IHTMLDOMNode GetParentNode();

			[return: MarshalAs(UnmanagedType.Bool)]
			bool HasChildNodes();

			[return: MarshalAs(UnmanagedType.Interface)]
			object GetChildNodes();

			[return: MarshalAs(UnmanagedType.Interface)]
			object GetAttributes();

			[return: MarshalAs(UnmanagedType.Interface)]
			IHTMLDOMNode InsertBefore(
				[In, MarshalAs(UnmanagedType.Interface)]
				IHTMLDOMNode newChild,
				[In, MarshalAs(UnmanagedType.Struct)]
				Object refChild);

			[return: MarshalAs(UnmanagedType.Interface)]
			IHTMLDOMNode RemoveChild(
				[In, MarshalAs(UnmanagedType.Interface)]
				IHTMLDOMNode oldChild);

			[return: MarshalAs(UnmanagedType.Interface)]
			IHTMLDOMNode ReplaceChild(
				[In, MarshalAs(UnmanagedType.Interface)]
				IHTMLDOMNode newChild,
				[In, MarshalAs(UnmanagedType.Interface)]
				IHTMLDOMNode oldChild);

			[return: MarshalAs(UnmanagedType.Interface)]
			IHTMLDOMNode CloneNode(
				[In, MarshalAs(UnmanagedType.Bool)]
				bool fDeep);

			[return: MarshalAs(UnmanagedType.Interface)]
			IHTMLDOMNode RemoveNode(
				[In, MarshalAs(UnmanagedType.Bool)]
				bool fDeep);

			[return: MarshalAs(UnmanagedType.Interface)]
			IHTMLDOMNode SwapNode(
				[In, MarshalAs(UnmanagedType.Interface)]
				IHTMLDOMNode otherNode);

			[return: MarshalAs(UnmanagedType.Interface)]
			IHTMLDOMNode ReplaceNode(
				[In, MarshalAs(UnmanagedType.Interface)]
				IHTMLDOMNode replacement);

			[return: MarshalAs(UnmanagedType.Interface)]
			IHTMLDOMNode AppendChild(
				[In, MarshalAs(UnmanagedType.Interface)]
				IHTMLDOMNode newChild);

			[return: MarshalAs(UnmanagedType.BStr)]
			string GetNodeName();

            
			void SetNodeValue(
				[In, MarshalAs(UnmanagedType.Struct)]
				Object p);

			[return: MarshalAs(UnmanagedType.Struct)]
			Object GetNodeValue();

			[return: MarshalAs(UnmanagedType.Interface)]
			IHTMLDOMNode GetFirstChild();

			[return: MarshalAs(UnmanagedType.Interface)]
			IHTMLDOMNode GetLastChild();

			[return: MarshalAs(UnmanagedType.Interface)]
			IHTMLDOMNode GetPreviousSibling();

			[return: MarshalAs(UnmanagedType.Interface)]
			IHTMLDOMNode GetNextSibling();
		}

        [System.Runtime.InteropServices.ComVisible(true), System.Runtime.InteropServices.ComImport(), Guid("0000011B-0000-0000-C000-000000000046"), System.Runtime.InteropServices.InterfaceTypeAttribute(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IOleContainer 
        {
            void ParseDisplayName(
                [In, MarshalAs(UnmanagedType.Interface)] 
                object pbc,
                [In, MarshalAs(UnmanagedType.BStr)] 
                string pszDisplayName,
                [Out, MarshalAs(UnmanagedType.LPArray)] 
                int[] pchEaten,
                [Out, MarshalAs(UnmanagedType.LPArray)] 
                Object[] ppmkOut);

            
            void EnumObjects(
                [In, MarshalAs(UnmanagedType.U4)] 
                int grfFlags,
                [Out, MarshalAs(UnmanagedType.Interface)] 
                out object ppenum);

            
            void LockContainer(
                [In, MarshalAs(UnmanagedType.I4)] 
                int fLock);
        }

        [System.Runtime.InteropServices.ComVisible(true), System.Runtime.InteropServices.ComImport(), Guid("00000118-0000-0000-C000-000000000046"), System.Runtime.InteropServices.InterfaceTypeAttribute(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IOleClientSite 
        {
            void SaveObject();

            [return: MarshalAs(UnmanagedType.Interface)]
            object GetMoniker(
                [In, MarshalAs(UnmanagedType.U4)] 
                int dwAssign,
                [In, MarshalAs(UnmanagedType.U4)] 
                int dwWhichMoniker);

            [PreserveSig]
            int GetContainer(
                [System.Runtime.InteropServices.Out]
                out IOleContainer ppContainer);

            
            void ShowObject();

            
            void OnShowWindow(
                [In, MarshalAs(UnmanagedType.I4)] 
                int fShow);

            
            void RequestNewObjectLayout();
        }

        [System.Runtime.InteropServices.ComVisible(true), System.Runtime.InteropServices.ComImport(), Guid("B722BCC7-4E68-101B-A2BC-00AA00404770"), System.Runtime.InteropServices.InterfaceTypeAttribute(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IOleDocumentSite 
        {

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int ActivateMe(
                [In, MarshalAs(UnmanagedType.Interface)] 
                IOleDocumentView pViewToActivate);
        }

        [System.Runtime.InteropServices.ComVisible(true), System.Runtime.InteropServices.ComImport(), Guid("B722BCC6-4E68-101B-A2BC-00AA00404770"), System.Runtime.InteropServices.InterfaceTypeAttribute(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IOleDocumentView 
        {
            void SetInPlaceSite(
                [In, MarshalAs(UnmanagedType.Interface)] 
                IOleInPlaceSite pIPSite);

            [return: MarshalAs(UnmanagedType.Interface)]
            IOleInPlaceSite GetInPlaceSite();

            [return: MarshalAs(UnmanagedType.Interface)]
            object GetDocument();

            
            void SetRect(
                [In] 
                COMRECT prcView);

            
            void GetRect(
                [Out] 
                COMRECT prcView);

            
            void SetRectComplex(
                [In] 
                COMRECT prcView,
                [In] 
                COMRECT prcHScroll,
                [In] 
                COMRECT prcVScroll,
                [In] 
                COMRECT prcSizeBox);

            
            void Show(
                [In, MarshalAs(UnmanagedType.I4)] 
                int fShow);

            
            void UIActivate(
                [In, MarshalAs(UnmanagedType.I4)] 
                int fUIActivate);

            
            void Open();

            
            void CloseView(
                [In, MarshalAs(UnmanagedType.U4)] 
                int dwReserved);

            
            void SaveViewState(
                [In, MarshalAs(UnmanagedType.Interface)] 
                IStream pstm);

            
            void ApplyViewState(
                [In, MarshalAs(UnmanagedType.Interface)] 
                IStream pstm);

            
            void Clone(
                [In, MarshalAs(UnmanagedType.Interface)] 
                IOleInPlaceSite pIPSiteNew,
                [Out, MarshalAs(UnmanagedType.LPArray)] 
                IOleDocumentView[] ppViewNew);
        }

        [System.Runtime.InteropServices.ComVisible(true), System.Runtime.InteropServices.ComImport(), Guid("00000119-0000-0000-C000-000000000046"), System.Runtime.InteropServices.InterfaceTypeAttribute(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IOleInPlaceSite 
        {
            IntPtr GetWindow();

            void ContextSensitiveHelp(
                [In, MarshalAs(UnmanagedType.I4)] 
                int fEnterMode);

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int CanInPlaceActivate();

            
            void OnInPlaceActivate();

            
            void OnUIActivate();

            
            void GetWindowContext(
                [Out]
                out IOleInPlaceFrame ppFrame,
                [Out]
                out IOleInPlaceUIWindow ppDoc,
                [Out] 
                COMRECT lprcPosRect,
                [Out] 
                COMRECT lprcClipRect,
                [In, Out] 
                tagOIFI lpFrameInfo);

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int Scroll(
                [In, MarshalAs(UnmanagedType.U4)] 
                tagSIZE scrollExtant);

            
            void OnUIDeactivate(
                [In, MarshalAs(UnmanagedType.I4)] 
                int fUndoable);

            
            void OnInPlaceDeactivate();

            
            void DiscardUndoState();

            
            void DeactivateAndUndo();

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int OnPosRectChange(
                [In] 
                COMRECT lprcPosRect);
        }

        [System.Runtime.InteropServices.ComVisible(true), System.Runtime.InteropServices.ComImport(), Guid("00000116-0000-0000-C000-000000000046"), System.Runtime.InteropServices.InterfaceTypeAttribute(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IOleInPlaceFrame 
        {
            IntPtr GetWindow();

            
            void ContextSensitiveHelp(
                [In, MarshalAs(UnmanagedType.I4)]
                int fEnterMode);

            
            void GetBorder(
                [Out]
                COMRECT lprectBorder);

            
            void RequestBorderSpace(
                [In]
                COMRECT pborderwidths);

            
            void SetBorderSpace(
                [In]
                COMRECT pborderwidths);

            
            void SetActiveObject(
                [In, MarshalAs(UnmanagedType.Interface)]
                IOleInPlaceActiveObject pActiveObject,
                [In, MarshalAs(UnmanagedType.LPWStr)]
                string pszObjName);

            
            void InsertMenus(
                [In]
                IntPtr hmenuShared,
                [In, Out]
                object lpMenuWidths);

            
            void SetMenu(
                [In]
                IntPtr hmenuShared,
                [In]
                IntPtr holemenu,
                [In]
                IntPtr hwndActiveObject);

            
            void RemoveMenus(
                [In]
                IntPtr hmenuShared);

            
            void SetStatusText(
                [In, MarshalAs(UnmanagedType.BStr)]
                string pszStatusText);

            
            void EnableModeless(
                [In, MarshalAs(UnmanagedType.I4)]
                int fEnable);

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int TranslateAccelerator(
                [In]
                ref MSG lpmsg,
                [In, MarshalAs(UnmanagedType.U2)]
                short wID);
        }

        [System.Runtime.InteropServices.ComVisible(true), System.Runtime.InteropServices.ComImport(), Guid("00000115-0000-0000-C000-000000000046"), System.Runtime.InteropServices.InterfaceTypeAttribute(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IOleInPlaceUIWindow 
        {
            IntPtr GetWindow();

            
            void ContextSensitiveHelp(
                [In, MarshalAs(UnmanagedType.I4)] 
                int fEnterMode);

            
            void GetBorder(
                [Out] 
                COMRECT lprectBorder);

            
            void RequestBorderSpace(
                [In] 
                COMRECT pborderwidths);

            
            void SetBorderSpace(
                [In] 
                COMRECT pborderwidths);

            
            void SetActiveObject(
                [In, MarshalAs(UnmanagedType.Interface)] 
                IOleInPlaceActiveObject pActiveObject,
                [In, MarshalAs(UnmanagedType.LPWStr)] 
                string pszObjName);
        }

        [System.Runtime.InteropServices.ComVisible(true), System.Runtime.InteropServices.ComImport(), Guid("BD3F23C0-D43E-11CF-893B-00AA00BDCE1A"), System.Runtime.InteropServices.InterfaceTypeAttribute(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IDocHostUIHandler 
        {

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int ShowContextMenu(
                [In, MarshalAs(UnmanagedType.U4)]
                int dwID,
                [In]
                POINT pt,
                [In, MarshalAs(UnmanagedType.Interface)]
                object pcmdtReserved,
                [In, MarshalAs(UnmanagedType.Interface)]
                object pdispReserved);

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int GetHostInfo(
                [In, Out]
                DOCHOSTUIINFO info);

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int ShowUI(
                [In, MarshalAs(UnmanagedType.I4)]
                int dwID,
                [In]
                IOleInPlaceActiveObject activeObject,
                [In]
                IOleCommandTarget commandTarget,
                [In]
                IOleInPlaceFrame frame,
                [In]
                IOleInPlaceUIWindow doc);

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int HideUI();

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int UpdateUI();

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int EnableModeless(
                [In, MarshalAs(UnmanagedType.Bool)]
                bool fEnable);

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int OnDocWindowActivate(
                [In, MarshalAs(UnmanagedType.Bool)]
                bool fActivate);

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int OnFrameWindowActivate(
                [In, MarshalAs(UnmanagedType.Bool)]
                bool fActivate);

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int ResizeBorder(
                [In]
                COMRECT rect,
                [In]
                IOleInPlaceUIWindow doc,
                bool fFrameWindow);

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int TranslateAccelerator(
                [In]
                ref MSG msg,
                [In]
                ref Guid group,
                [In, MarshalAs(UnmanagedType.I4)]
                int nCmdID);

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int GetOptionKeyPath(
                [Out, MarshalAs(UnmanagedType.LPArray)]
                String[] pbstrKey,
                [In, MarshalAs(UnmanagedType.U4)]
                int dw);

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int GetDropTarget(
                [In, MarshalAs(UnmanagedType.Interface)]
                IOleDropTarget pDropTarget,
                [Out, MarshalAs(UnmanagedType.Interface)]
                out IOleDropTarget ppDropTarget);

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int GetExternal(
                [Out, MarshalAs(UnmanagedType.Interface)]
                out object ppDispatch);

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int TranslateUrl(
                [In, MarshalAs(UnmanagedType.U4)]
                int dwTranslate,
                [In, MarshalAs(UnmanagedType.LPWStr)]
                string strURLIn,
                [Out, MarshalAs(UnmanagedType.LPWStr)]
                out string pstrURLOut);

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int FilterDataObject(
                [In, MarshalAs(UnmanagedType.Interface)]
                IOleDataObject pDO,
                [Out, MarshalAs(UnmanagedType.Interface)]
                out IOleDataObject ppDORet);
        }

        [System.Runtime.InteropServices.ComVisible(true), System.Runtime.InteropServices.ComImport(), Guid("00000117-0000-0000-C000-000000000046"), System.Runtime.InteropServices.InterfaceTypeAttribute(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IOleInPlaceActiveObject 
        {

            int GetWindow(out IntPtr hwnd);

            
            void ContextSensitiveHelp(
                [In, MarshalAs(UnmanagedType.I4)]
                int fEnterMode);

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int TranslateAccelerator(
                [In]
                ref MSG lpmsg);

            
            void OnFrameWindowActivate(
                [In, MarshalAs(UnmanagedType.I4)]
                int fActivate);

            
            void OnDocWindowActivate(
                [In, MarshalAs(UnmanagedType.I4)]
                int fActivate);

            
            void ResizeBorder(
                [In]
                COMRECT prcBorder,
                [In]
                IOleInPlaceUIWindow pUIWindow,
                [In, MarshalAs(UnmanagedType.I4)]
                int fFrameWindow);

            
            void EnableModeless(
                [In, MarshalAs(UnmanagedType.I4)]
                int fEnable);
        }

        [System.Runtime.InteropServices.ComVisible(true), System.Runtime.InteropServices.ComImport(), Guid("00000112-0000-0000-C000-000000000046"), System.Runtime.InteropServices.InterfaceTypeAttribute(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IOleObject 
        {

            [PreserveSig]
            int SetClientSite(
                [In, MarshalAs(UnmanagedType.Interface)]
                IOleClientSite pClientSite);

            [PreserveSig]
            int GetClientSite(out IOleClientSite site);

            [PreserveSig]
            int SetHostNames(
                [In, MarshalAs(UnmanagedType.LPWStr)]
                string szContainerApp,
                [In, MarshalAs(UnmanagedType.LPWStr)]
                string szContainerObj);

            [PreserveSig]
            int Close(
                [In, MarshalAs(UnmanagedType.I4)]
                int dwSaveOption);

            [PreserveSig]
            int SetMoniker(
                [In, MarshalAs(UnmanagedType.U4)]
                int dwWhichMoniker,
                [In, MarshalAs(UnmanagedType.Interface)]
                object pmk);

            [PreserveSig]
            int GetMoniker(
                [In, MarshalAs(UnmanagedType.U4)]
                int dwAssign,
                [In, MarshalAs(UnmanagedType.U4)]
                int dwWhichMoniker,
                out object moniker);

            [PreserveSig]
            int InitFromData(
                [In, MarshalAs(UnmanagedType.Interface)]
                IOleDataObject pDataObject,
                [In, MarshalAs(UnmanagedType.I4)]
                int fCreation,
                [In, MarshalAs(UnmanagedType.U4)]
                int dwReserved);

            [PreserveSig]
            int GetClipboardData(
                [In, MarshalAs(UnmanagedType.U4)]
                int dwReserved,
                out IOleDataObject data);

            [PreserveSig]
            int DoVerb(
                [In, MarshalAs(UnmanagedType.I4)]
                int iVerb,
                [In]
                IntPtr lpmsg,
                [In, MarshalAs(UnmanagedType.Interface)]
                IOleClientSite pActiveSite,
                [In, MarshalAs(UnmanagedType.I4)]
                int lindex,
                [In]
                IntPtr hwndParent,
                [In]
                COMRECT lprcPosRect);

            [PreserveSig]
            int EnumVerbs(out NativeMethods.IEnumOLEVERB e);

            [PreserveSig]
            int OleUpdate();

            [PreserveSig]
            int IsUpToDate();

            [PreserveSig]
            int GetUserClassID(
                [In, Out]
                ref Guid pClsid);

            [PreserveSig]
            int GetUserType(
                [In, MarshalAs(UnmanagedType.U4)]
                int dwFormOfType,
                [Out, MarshalAs(UnmanagedType.LPWStr)]
                out string userType);

            [PreserveSig]
            int SetExtent(
                [In, MarshalAs(UnmanagedType.U4)]
                int dwDrawAspect,
                [In]
                tagSIZEL pSizel);

            [PreserveSig]
            int GetExtent(
                [In, MarshalAs(UnmanagedType.U4)]
                int dwDrawAspect,
                [Out]
                tagSIZEL pSizel);

            [PreserveSig]
            int Advise(
                [In, MarshalAs(UnmanagedType.Interface)]
                IAdviseSink pAdvSink,
                out int cookie);

            [PreserveSig]
            int Unadvise(
                [In, MarshalAs(UnmanagedType.U4)]
                int dwConnection);

            [PreserveSig]
            int EnumAdvise(out object e);

            [PreserveSig]
            int GetMiscStatus(
                [In, MarshalAs(UnmanagedType.U4)]
                int dwAspect,
                out int misc);

            [PreserveSig]
            int SetColorScheme(
                [In]
                tagLOGPALETTE pLogpal);
        }

        [System.Runtime.InteropServices.ComVisible(true), System.Runtime.InteropServices.ComImport(), Guid("B722BCCB-4E68-101B-A2BC-00AA00404770"), System.Runtime.InteropServices.InterfaceTypeAttribute(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IOleCommandTarget 
        {

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int QueryStatus(
                ref Guid pguidCmdGroup,
                int cCmds,
                [In, Out] 
                OLECMD prgCmds,
                [In, Out] 
                string pCmdText);

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int Exec(
                ref Guid pguidCmdGroup,
                int nCmdID,
                int nCmdexecopt,
                // we need to have this an array because callers need to be able to specify NULL or VT_NULL
                [In, MarshalAs(UnmanagedType.LPArray)]
                Object[] pvaIn,
                IntPtr pvaOut);
        }

        [System.Runtime.InteropServices.ComVisible(true), System.Runtime.InteropServices.ComImport(), Guid("00000122-0000-0000-C000-000000000046"), System.Runtime.InteropServices.InterfaceTypeAttribute(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IOleDropTarget 
        {

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int OleDragEnter(
                [In, MarshalAs(UnmanagedType.Interface)]
                IOleDataObject pDataObj,
                [In, MarshalAs(UnmanagedType.U4)]
                int grfKeyState,
                [In]
                POINTL pt,
                [In, Out, MarshalAs(UnmanagedType.I4)]
                ref int pdwEffect);

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int OleDragOver(
                [In, MarshalAs(UnmanagedType.U4)]
                int grfKeyState,
                [In]
                POINTL pt,
                [In, Out, MarshalAs(UnmanagedType.I4)]
                ref int pdwEffect);

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int OleDragLeave();

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int OleDrop(
                [In, MarshalAs(UnmanagedType.Interface)]
                IOleDataObject pDataObj,
                [In, MarshalAs(UnmanagedType.U4)]
                int grfKeyState,
                [In]
                POINTL pt,
                [In, Out, MarshalAs(UnmanagedType.I4)]
                ref int pdwEffect);
        }

        [System.Runtime.InteropServices.ComVisible(true), System.Runtime.InteropServices.ComImport(), Guid("0000010E-0000-0000-C000-000000000046"), System.Runtime.InteropServices.InterfaceTypeAttribute(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IOleDataObject 
        {
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int OleGetData(
                FORMATETC pFormatetc,
                [Out] 
                STGMEDIUM pMedium);

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int OleGetDataHere(
                FORMATETC pFormatetc,
                [In, Out] 
                STGMEDIUM pMedium);

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int OleQueryGetData(
                FORMATETC pFormatetc);

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int OleGetCanonicalFormatEtc(
                FORMATETC pformatectIn,
                [Out] 
                FORMATETC pformatetcOut);

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int OleSetData(
                FORMATETC pFormatectIn,
                STGMEDIUM pmedium,
                [In, MarshalAs(UnmanagedType.I4)] 
                int fRelease);

            [return: MarshalAs(UnmanagedType.Interface)]
            object OleEnumFormatEtc(
                [In, MarshalAs(UnmanagedType.U4)] 
                int dwDirection);

            [PreserveSig]
            int OleDAdvise(
                FORMATETC pFormatetc,
                [In, MarshalAs(UnmanagedType.U4)] 
                int advf,
                [In, MarshalAs(UnmanagedType.Interface)] 
                object pAdvSink,
                [Out, MarshalAs(UnmanagedType.LPArray)] 
                int[] pdwConnection);

            [PreserveSig]
            int OleDUnadvise(
                [In, MarshalAs(UnmanagedType.U4)] 
                int dwConnection);

            [PreserveSig]
            int OleEnumDAdvise(
                [Out, MarshalAs(UnmanagedType.LPArray)] 
                Object[] ppenumAdvise);
        }

        [ComImport(), Guid("00000104-0000-0000-C000-000000000046"), System.Runtime.InteropServices.InterfaceTypeAttribute(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IEnumOLEVERB 
        {
            [PreserveSig]
            int Next(
                [MarshalAs(UnmanagedType.U4)] 
                int celt,
                [Out]
                NativeMethods.tagOLEVERB rgelt,
                [Out, MarshalAs(UnmanagedType.LPArray)]
                int[] pceltFetched);

            [PreserveSig]
            int Skip(
                [In, MarshalAs(UnmanagedType.U4)] 
                int celt);


            void Reset();


            void Clone(
                out NativeMethods.IEnumOLEVERB ppenum);
        }

        [System.Runtime.InteropServices.ComVisible(true), System.Runtime.InteropServices.ComImport(), Guid("0000010F-0000-0000-C000-000000000046"), System.Runtime.InteropServices.InterfaceTypeAttribute(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IAdviseSink 
        {
            void OnDataChange(
                [In]
                FORMATETC pFormatetc,
                [In]
                STGMEDIUM pStgmed);

            
            void OnViewChange(
                [In, MarshalAs(UnmanagedType.U4)]
                int dwAspect,
                [In, MarshalAs(UnmanagedType.I4)]
                int lindex);

            
            void OnRename(
                [In, MarshalAs(UnmanagedType.Interface)]
                object pmk);

            
            void OnSave();

            
            void OnClose();
        }

        [System.Runtime.InteropServices.ComVisible(true), System.Runtime.InteropServices.ComImport(), Guid("3050F1D8-98B5-11CF-BB82-00AA00BDCE0B"), System.Runtime.InteropServices.InterfaceTypeAttribute(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsDual)]
        internal interface IHTMLBodyElement 
        {
            void SetBackground(
                [In, MarshalAs(UnmanagedType.BStr)]
                string p);

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetBackground();

            
            void SetBgProperties(
                [In, MarshalAs(UnmanagedType.BStr)]
                string p);

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetBgProperties();

            
            void SetLeftMargin(
                [In, MarshalAs(UnmanagedType.Struct)]
                Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetLeftMargin();

            
            void SetTopMargin(
                [In, MarshalAs(UnmanagedType.Struct)]
                Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetTopMargin();

            
            void SetRightMargin(
                [In, MarshalAs(UnmanagedType.Struct)]
                Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetRightMargin();

            
            void SetBottomMargin(
                [In, MarshalAs(UnmanagedType.Struct)]
                Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetBottomMargin();

            
            void SetNoWrap(
                [In, MarshalAs(UnmanagedType.Bool)]
                bool p);

            [return: MarshalAs(UnmanagedType.Bool)]
            bool GetNoWrap();

            
            void SetBgColor(
                [In, MarshalAs(UnmanagedType.Struct)]
                Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetBgColor();

            
            void SetText(
                [In, MarshalAs(UnmanagedType.Struct)]
                Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetText();

            
            void SetLink(
                [In, MarshalAs(UnmanagedType.Struct)]
                Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetLink();

            
            void SetVLink(
                [In, MarshalAs(UnmanagedType.Struct)]
                Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetVLink();

            
            void SetALink(
                [In, MarshalAs(UnmanagedType.Struct)]
                Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetALink();

            
            void SetOnload(
                [In, MarshalAs(UnmanagedType.Struct)]
                Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetOnload();

            
            void SetOnunload(
                [In, MarshalAs(UnmanagedType.Struct)]
                Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetOnunload();

            
            void SetScroll(
                [In, MarshalAs(UnmanagedType.BStr)]
                string p);

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetScroll();

            
            void SetOnselect(
                [In, MarshalAs(UnmanagedType.Struct)]
                Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetOnselect();

            
            void SetOnbeforeunload(
                [In, MarshalAs(UnmanagedType.Struct)]
                Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetOnbeforeunload();

            [return: MarshalAs(UnmanagedType.Interface)]
            object CreateTextRange();
        }

        [System.Runtime.InteropServices.ComVisible(true), System.Runtime.InteropServices.ComImport(), Guid("7FD52380-4E07-101B-AE2D-08002B2EC713"), System.Runtime.InteropServices.InterfaceTypeAttribute(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IPersistStreamInit 
        {
            void GetClassID(
                [In, Out] 
                ref Guid pClassID);

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int IsDirty();

            
            void Load(
                [In, MarshalAs(UnmanagedType.Interface)] 
                IStream pstm);

            
            void Save(
                [In, MarshalAs(UnmanagedType.Interface)] 
                IStream pstm,
                [In, MarshalAs(UnmanagedType.Bool)] 
                bool fClearDirty);

            
            void GetSizeMax(
                [Out, MarshalAs(UnmanagedType.LPArray)] 
                long pcbSize);

            
            void InitNew();
        }

        [System.Runtime.InteropServices.ComVisible(true), System.Runtime.InteropServices.ComImport(), Guid("25336920-03F9-11CF-8FD0-00AA00686F13")]
        [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class HTMLDocument 
        {
        }

        [System.Runtime.InteropServices.ComVisible(false), System.Runtime.InteropServices.ComImport(), Guid("3050F434-98B5-11CF-BB82-00AA00BDCE0B"), System.Runtime.InteropServices.InterfaceTypeAttribute(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsDual)]
        internal interface IHTMLElement2 
        {

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetScopeName();

            
            void SetCapture(
                [In, MarshalAs(UnmanagedType.Bool)]
                bool containerCapture);

            
            void ReleaseCapture();

            
            void SetOnlosecapture(
                [In, MarshalAs(UnmanagedType.Struct)]
                Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetOnlosecapture();

            [return: MarshalAs(UnmanagedType.BStr)]
            string ComponentFromPoint(
                [In, MarshalAs(UnmanagedType.I4)]
                int x,
                [In, MarshalAs(UnmanagedType.I4)]
                int y);

            
            void DoScroll(
                [In, MarshalAs(UnmanagedType.Struct)]
                Object component);

            
            void SetOnscroll(
                [In, MarshalAs(UnmanagedType.Struct)]
                Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetOnscroll();

            
            void SetOndrag(
                [In, MarshalAs(UnmanagedType.Struct)]
                Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetOndrag();

            
            void SetOndragend(
                [In, MarshalAs(UnmanagedType.Struct)]
                Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetOndragend();

            
            void SetOndragenter(
                [In, MarshalAs(UnmanagedType.Struct)]
                Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetOndragenter();

            
            void SetOndragover(
                [In, MarshalAs(UnmanagedType.Struct)]
                Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetOndragover();

            
            void SetOndragleave(
                [In, MarshalAs(UnmanagedType.Struct)]
                Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetOndragleave();

            
            void SetOndrop(
                [In, MarshalAs(UnmanagedType.Struct)]
                Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetOndrop();

            
            void SetOnbeforecut(
                [In, MarshalAs(UnmanagedType.Struct)]
                Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetOnbeforecut();

            
            void SetOncut(
                [In, MarshalAs(UnmanagedType.Struct)]
                Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetOncut();

            
            void SetOnbeforecopy(
                [In, MarshalAs(UnmanagedType.Struct)]
                Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetOnbeforecopy();

            
            void SetOncopy(
                [In, MarshalAs(UnmanagedType.Struct)]
                Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetOncopy();

            
            void SetOnbeforepaste(
                [In, MarshalAs(UnmanagedType.Struct)]
                Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetOnbeforepaste();

            
            void SetOnpaste(
                [In, MarshalAs(UnmanagedType.Struct)]
                Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetOnpaste();

            [return: MarshalAs(UnmanagedType.Interface)]
            IHTMLCurrentStyle GetCurrentStyle();

            
            void SetOnpropertychange(
                [In, MarshalAs(UnmanagedType.Struct)]
                Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetOnpropertychange();

            [return: MarshalAs(UnmanagedType.Interface)]
            IHTMLRectCollection GetClientRects();

            [return: MarshalAs(UnmanagedType.Interface)]
            IHTMLRect GetBoundingClientRect();

            
            void SetExpression(
                [In, MarshalAs(UnmanagedType.BStr)]
                string propname,
                [In, MarshalAs(UnmanagedType.BStr)]
                string expression,
                [In, MarshalAs(UnmanagedType.BStr)]
                string language);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetExpression(
                [In, MarshalAs(UnmanagedType.BStr)]
                Object propname);

            [return: MarshalAs(UnmanagedType.Bool)]
            bool RemoveExpression(
                [In, MarshalAs(UnmanagedType.BStr)]
                string propname);

            
            void SetTabIndex(
                [In, MarshalAs(UnmanagedType.I2)]
                short p);

            [return: MarshalAs(UnmanagedType.I2)]
            short GetTabIndex();

            
            void Focus();

            
            void SetAccessKey(
                [In, MarshalAs(UnmanagedType.BStr)]
                string p);

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetAccessKey();

            
            void SetOnblur(
                [In, MarshalAs(UnmanagedType.Struct)]
                Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetOnblur();

            
            void SetOnfocus(
                [In, MarshalAs(UnmanagedType.Struct)]
                Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetOnfocus();

            
            void SetOnresize(
                [In, MarshalAs(UnmanagedType.Struct)]
                Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetOnresize();

            
            void Blur();

            
            void AddFilter(
                [In, MarshalAs(UnmanagedType.Interface)]
                object pUnk);

            
            void RemoveFilter(
                [In, MarshalAs(UnmanagedType.Interface)]
                object pUnk);

            [return: MarshalAs(UnmanagedType.I4)]
            int GetClientHeight();

            [return: MarshalAs(UnmanagedType.I4)]
            int GetClientWidth();

            [return: MarshalAs(UnmanagedType.I4)]
            int GetClientTop();

            [return: MarshalAs(UnmanagedType.I4)]
            int GetClientLeft();

            [return: MarshalAs(UnmanagedType.Bool)]
            bool AttachEvent(
                [In, MarshalAs(UnmanagedType.BStr)]
                string ev,
                [In, MarshalAs(UnmanagedType.Interface)]
                object pdisp);

            
            void DetachEvent(
                [In, MarshalAs(UnmanagedType.BStr)]
                string ev,
                [In, MarshalAs(UnmanagedType.Interface)]
                object pdisp);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetReadyState();

            
            void SetOnreadystatechange(
                [In, MarshalAs(UnmanagedType.Struct)]
                Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetOnreadystatechange();

            
            void SetOnrowsdelete(
                [In, MarshalAs(UnmanagedType.Struct)]
                Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetOnrowsdelete();

            
            void SetOnrowsinserted(
                [In, MarshalAs(UnmanagedType.Struct)]
                Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetOnrowsinserted();

            
            void SetOncellchange(
                [In, MarshalAs(UnmanagedType.Struct)]
                Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetOncellchange();

            
            void SetDir(
                [In, MarshalAs(UnmanagedType.BStr)]
                string p);

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetDir();

            [return: MarshalAs(UnmanagedType.Interface)]
            object CreateControlRange();

            [return: MarshalAs(UnmanagedType.I4)]
            int GetScrollHeight();

            [return: MarshalAs(UnmanagedType.I4)]
            int GetScrollWidth();

            
            void SetScrollTop(
                [In, MarshalAs(UnmanagedType.I4)]
                int p);

            [return: MarshalAs(UnmanagedType.I4)]
            int GetScrollTop();

            
            void SetScrollLeft(
                [In, MarshalAs(UnmanagedType.I4)]
                int p);

            [return: MarshalAs(UnmanagedType.I4)]
            int GetScrollLeft();

            
            void ClearAttributes();

            
            void MergeAttributes(
                [In, MarshalAs(UnmanagedType.Interface)]
                IHTMLElement mergeThis);

            
            void SetOncontextmenu(
                [In, MarshalAs(UnmanagedType.Struct)]
                Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetOncontextmenu();

            [return: MarshalAs(UnmanagedType.Interface)]
            IHTMLElement InsertAdjacentElement(
                [In, MarshalAs(UnmanagedType.BStr)]
                string @where,
                [In, MarshalAs(UnmanagedType.Interface)]
                IHTMLElement insertedElement);

            [return: MarshalAs(UnmanagedType.Interface)]
            IHTMLElement ApplyElement(
                [In, MarshalAs(UnmanagedType.Interface)]
                IHTMLElement apply,
                [In, MarshalAs(UnmanagedType.BStr)]
                string @where);

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetAdjacentText(
                [In, MarshalAs(UnmanagedType.BStr)]
                string @where);

            [return: MarshalAs(UnmanagedType.BStr)]
            string ReplaceAdjacentText(
                [In, MarshalAs(UnmanagedType.BStr)]
                string @where,
                [In, MarshalAs(UnmanagedType.BStr)]
                string newText);

            [return: MarshalAs(UnmanagedType.Bool)]
            bool GetCanHaveChildren();

            [return: MarshalAs(UnmanagedType.I4)]
            int AddBehavior(
                [In, MarshalAs(UnmanagedType.BStr)]
                string bstrUrl,
                [In]
                ref Object pvarFactory);

            [return: MarshalAs(UnmanagedType.Bool)]
            bool RemoveBehavior(
                [In, MarshalAs(UnmanagedType.I4)]
                int cookie);

            [return: MarshalAs(UnmanagedType.Interface)]
            IHTMLStyle GetRuntimeStyle();

            [return: MarshalAs(UnmanagedType.Interface)]
            object GetBehaviorUrns();

            
            void SetTagUrn(
                [In, MarshalAs(UnmanagedType.BStr)]
                string p);

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetTagUrn();

            
            void SetOnbeforeeditfocus(
                [In, MarshalAs(UnmanagedType.Struct)]
                Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetOnbeforeeditfocus();

            [return: MarshalAs(UnmanagedType.I4)]
            int GetReadyStateValue();

            [return: MarshalAs(UnmanagedType.Interface)]
            IHTMLElementCollection GetElementsByTagName(
                [In, MarshalAs(UnmanagedType.BStr)]
                string v);

            [return: MarshalAs(UnmanagedType.Interface)]
            IHTMLStyle GetBaseStyle();

            [return: MarshalAs(UnmanagedType.Interface)]
            IHTMLCurrentStyle GetBaseCurrentStyle();

            [return: MarshalAs(UnmanagedType.Interface)]
            IHTMLStyle GetBaseRuntimeStyle();

            
            void SetOnmousehover(
                [In, MarshalAs(UnmanagedType.Struct)]
                Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetOnmousehover();

            
            void SetOnkeydownpreview(
                [In, MarshalAs(UnmanagedType.Struct)]
                Object p);

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetOnkeydownpreview();

            [return: MarshalAs(UnmanagedType.Interface)]
            object GetBehavior(
                [In, MarshalAs(UnmanagedType.BStr)]
                string bstrName,
                [In, MarshalAs(UnmanagedType.BStr)]
                string bstrUrn);
        }

        [System.Runtime.InteropServices.ComVisible(true), System.Runtime.InteropServices.ComImport(), Guid("3050F4A4-98B5-11CF-BB82-00AA00BDCE0B"), System.Runtime.InteropServices.InterfaceTypeAttribute(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsDual)]
        internal interface IHTMLRectCollection 
        {

            [return: MarshalAs(UnmanagedType.I4)]
            int GetLength();

            [return: MarshalAs(UnmanagedType.Interface)]
            object Get_newEnum();

            [return: MarshalAs(UnmanagedType.Struct)]
            Object Item(
                [In]
                ref Object pvarIndex);

        }

        [System.Runtime.InteropServices.ComVisible(true), System.Runtime.InteropServices.ComImport(), Guid("3050F3DB-98B5-11CF-BB82-00AA00BDCE0B"), System.Runtime.InteropServices.InterfaceTypeAttribute(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsDual)]
        internal interface IHTMLCurrentStyle 
        {

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetPosition();

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetStyleFloat();

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetColor();

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetBackgroundColor();

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetFontFamily();

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetFontStyle();

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetFontObject();

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetFontWeight();

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetFontSize();

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetBackgroundImage();

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetBackgroundPositionX();

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetBackgroundPositionY();

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetBackgroundRepeat();

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetBorderLeftColor();

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetBorderTopColor();

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetBorderRightColor();

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetBorderBottomColor();

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetBorderTopStyle();

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetBorderRightStyle();

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetBorderBottomStyle();

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetBorderLeftStyle();

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetBorderTopWidth();

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetBorderRightWidth();

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetBorderBottomWidth();

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetBorderLeftWidth();

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetLeft();

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetTop();

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetWidth();

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetHeight();

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetPaddingLeft();

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetPaddingTop();

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetPaddingRight();

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetPaddingBottom();

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetTextAlign();

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetTextDecoration();

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetDisplay();

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetVisibility();

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetZIndex();

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetLetterSpacing();

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetLineHeight();

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetTextIndent();

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetVerticalAlign();

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetBackgroundAttachment();

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetMarginTop();

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetMarginRight();

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetMarginBottom();

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetMarginLeft();

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetClear();

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetListStyleType();

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetListStylePosition();

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetListStyleImage();

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetClipTop();

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetClipRight();

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetClipBottom();

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetClipLeft();

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetOverflow();

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetPageBreakBefore();

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetPageBreakAfter();

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetCursor();

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetTableLayout();

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetBorderCollapse();

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetDirection();

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetBehavior();

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetAttribute(
                [In, MarshalAs(UnmanagedType.BStr)]
                string strAttributeName,
                [In, MarshalAs(UnmanagedType.I4)]
                int lFlags);

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetUnicodeBidi();

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetRight();

            [return: MarshalAs(UnmanagedType.Struct)]
            Object GetBottom();

        }

        [System.Runtime.InteropServices.ComVisible(true), System.Runtime.InteropServices.ComImport(), Guid("3050F4A3-98B5-11CF-BB82-00AA00BDCE0B"), System.Runtime.InteropServices.InterfaceTypeAttribute(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsDual)]
        internal interface IHTMLRect 
        {
            void SetLeft(
                [In, MarshalAs(UnmanagedType.I4)] 
                int p);

            [return: MarshalAs(UnmanagedType.I4)]
            int GetLeft();

            
            void SetTop(
                [In, MarshalAs(UnmanagedType.I4)] 
                int p);

            [return: MarshalAs(UnmanagedType.I4)]
            int GetTop();

            
            void SetRight(
                [In, MarshalAs(UnmanagedType.I4)] 
                int p);

            [return: MarshalAs(UnmanagedType.I4)]
            int GetRight();

            
            void SetBottom(
                [In, MarshalAs(UnmanagedType.I4)] 
                int p);

            [return: MarshalAs(UnmanagedType.I4)]
            int GetBottom();

        }
    }
}
