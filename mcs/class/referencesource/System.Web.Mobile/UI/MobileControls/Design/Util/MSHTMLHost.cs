//------------------------------------------------------------------------------
// <copyright file="MSHTMLHost.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

// MSHTMLHost.cs
//
// 12/17/98: Created: Microsoft
//

namespace System.Web.UI.Design.MobileControls.Util {
    using System.Runtime.Serialization.Formatters;
    
    using System.Runtime.InteropServices;
    using System.ComponentModel;

    using System.Diagnostics;

    using System;
    
    using Microsoft.Win32;    
    using System.Windows.Forms;

    /// <include file='doc\MSHTMLHost.uex' path='docs/doc[@for="MSHTMLHost"]/*' />
    /// <devdoc>
    ///    Control that hosts a Trident DocObject.
    /// </devdoc>
    /// <internalonly/>
    // 


    [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand, Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal sealed class MSHTMLHost : Control {
        private TridentSite tridentSite;

        internal MSHTMLHost() : base() {
        }

        public NativeMethods.IHTMLDocument2 GetDocument() {
            Debug.Assert(tridentSite != null,
                         "Cannot call getDocument before calling createTrident");

            return tridentSite.GetDocument();
        }

        protected override CreateParams CreateParams {
             get {
                CreateParams cp = base.CreateParams;

                cp.ExStyle |= NativeMethods.WS_EX_STATICEDGE;
                return cp;
            }
        }

        internal bool CreateTrident() {
            Debug.Assert(Handle != IntPtr.Zero,
                         "MSHTMLHost must first be created before createTrident is called");

            try {
                tridentSite = new TridentSite(this);
            }
            catch (Exception e) {
                Debug.WriteLine("Exception caught in MSHTMLHost::CreateTrident\n\t" + e.ToString());
                return false;
            }
            return true;
        }

        internal void ActivateTrident() {
            Debug.Assert(tridentSite != null,
                         "cannot call activateTrident before calling createTrident");

            tridentSite.Activate();
        }
    }


    /// <include file='doc\MSHTMLHost.uex' path='docs/doc[@for="TridentSite"]/*' />
    /// <devdoc>
    ///    Implements the client site for Trident DocObject
    /// </devdoc>
    [ClassInterface(ClassInterfaceType.None)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class TridentSite : NativeMethods.IOleClientSite, NativeMethods.IOleDocumentSite, NativeMethods.IOleInPlaceSite, NativeMethods.IOleInPlaceFrame, NativeMethods.IDocHostUIHandler {

        protected Control parentControl;
        protected NativeMethods.IOleDocumentView tridentView;
        protected NativeMethods.IOleObject tridentOleObject;
        protected NativeMethods.IHTMLDocument2 tridentDocument;

        protected EventHandler resizeHandler;

        internal TridentSite(Control parent) {
            Debug.Assert((parent != null) && (parent.Handle != IntPtr.Zero),
                         "Invalid control passed in as parent of Trident window");

            parentControl = parent;
            resizeHandler = new EventHandler(this.OnParentResize);
            parentControl.Resize += resizeHandler;

            CreateDocument();
        }

        public NativeMethods.IHTMLDocument2 GetDocument() {
            return tridentDocument;
        }

        internal void Activate() {
            ActivateDocument();
        }

        protected virtual void OnParentResize(object src, EventArgs e) {
            if (tridentView != null) {
                NativeMethods.COMRECT r = new NativeMethods.COMRECT();

                NativeMethods.GetClientRect(parentControl.Handle, r);
                tridentView.SetRect(r);
            }
        }


        ///////////////////////////////////////////////////////////////////////////
        // IOleClientSite Implementation

        public virtual void SaveObject() {
        }

        public virtual object GetMoniker(int dwAssign, int dwWhichMoniker) {
            throw new COMException(String.Empty, NativeMethods.E_NOTIMPL);
        }

        public virtual int GetContainer(out NativeMethods.IOleContainer ppContainer) {
            ppContainer = null;
            return NativeMethods.E_NOINTERFACE;
        }

        public virtual void ShowObject() {
        }

        public virtual void OnShowWindow(int fShow) {
        }

        public virtual void RequestNewObjectLayout() {
        }


        ///////////////////////////////////////////////////////////////////////////
        // IOleDocumentSite Implementation

        public virtual int ActivateMe(NativeMethods.IOleDocumentView pViewToActivate) {
            Debug.Assert(pViewToActivate != null,
                         "Expected the view to be non-null");
            if (pViewToActivate == null)
                return NativeMethods.E_INVALIDARG;

            NativeMethods.COMRECT r = new NativeMethods.COMRECT();

            NativeMethods.GetClientRect(parentControl.Handle, r);

            tridentView = pViewToActivate;
            tridentView.SetInPlaceSite((NativeMethods.IOleInPlaceSite)this);
            tridentView.UIActivate(1);
            tridentView.SetRect(r);
            tridentView.Show(1);

            return NativeMethods.S_OK;
        }


        ///////////////////////////////////////////////////////////////////////////
        // IOleInPlaceSite Implementation

        public virtual IntPtr GetWindow() {
            return parentControl.Handle;
        }

        public virtual void ContextSensitiveHelp(int fEnterMode) {
            throw new COMException(String.Empty, NativeMethods.E_NOTIMPL);
        }

        public virtual int CanInPlaceActivate() {
            return NativeMethods.S_OK;
        }

        public virtual void OnInPlaceActivate() {
        }

        public virtual void OnUIActivate() {
        }

        public virtual void GetWindowContext(out NativeMethods.IOleInPlaceFrame ppFrame, out NativeMethods.IOleInPlaceUIWindow ppDoc, NativeMethods.COMRECT lprcPosRect, NativeMethods.COMRECT lprcClipRect, NativeMethods.tagOIFI lpFrameInfo) {

            ppFrame = (NativeMethods.IOleInPlaceFrame)this;
            ppDoc = null;

            NativeMethods.GetClientRect(parentControl.Handle, lprcPosRect);
            NativeMethods.GetClientRect(parentControl.Handle, lprcClipRect);

            lpFrameInfo.cb = System.Runtime.InteropServices.Marshal.SizeOf(typeof(NativeMethods.tagOIFI));
            lpFrameInfo.fMDIApp = 0;
            lpFrameInfo.hwndFrame = parentControl.Handle;
            lpFrameInfo.hAccel = IntPtr.Zero;
            lpFrameInfo.cAccelEntries = 0;
        }

        public virtual int Scroll(NativeMethods.tagSIZE scrollExtant) {
            return(NativeMethods.E_NOTIMPL);
        }

        public virtual void OnUIDeactivate(int fUndoable) {
            // NOTE, Microsoft, 7/99: Don't return E_NOTIMPL. Somehow doing nothing and returning S_OK
            //    fixes trident hosting in Win2000.
        }

        public virtual void OnInPlaceDeactivate() {
        }

        public virtual void DiscardUndoState() {
            throw new COMException(SR.GetString(SR.MSHTMLHost_Not_Implemented), NativeMethods.E_NOTIMPL);
        }

        public virtual void DeactivateAndUndo() {
        }

        public virtual int OnPosRectChange(NativeMethods.COMRECT lprcPosRect) {
            return NativeMethods.S_OK;
        }


        ///////////////////////////////////////////////////////////////////////////
        // IOleInPlaceFrame Implementation

        public virtual void GetBorder(NativeMethods.COMRECT lprectBorder) {
            throw new COMException(String.Empty, NativeMethods.E_NOTIMPL);
        }

        public virtual void RequestBorderSpace(NativeMethods.COMRECT pborderwidths) {
            throw new COMException(String.Empty, NativeMethods.E_NOTIMPL);
        }

        public virtual void SetBorderSpace(NativeMethods.COMRECT pborderwidths) {
            throw new COMException(String.Empty, NativeMethods.E_NOTIMPL);
        }

        public virtual void SetActiveObject(NativeMethods.IOleInPlaceActiveObject pActiveObject, string pszObjName) {
            // NOTE, Microsoft, 7/99: Don't return E_NOTIMPL. Somehow doing nothing and returning S_OK
            //    fixes trident hosting in Win2000.
            // throw new COMException(String.Empty, NativeMethods.E_NOTIMPL);
        }

        public virtual void InsertMenus(IntPtr hmenuShared, object lpMenuWidths) {
            throw new COMException(String.Empty, NativeMethods.E_NOTIMPL);
        }

        public virtual void SetMenu(IntPtr hmenuShared, IntPtr holemenu, IntPtr hwndActiveObject) {
            throw new COMException(String.Empty, NativeMethods.E_NOTIMPL);
        }

        public virtual void RemoveMenus(IntPtr hmenuShared) {
            throw new COMException(String.Empty, NativeMethods.E_NOTIMPL);
        }

        public virtual void SetStatusText(string pszStatusText) {
        }

        public virtual void EnableModeless(int fEnable) {
        }

        public virtual int TranslateAccelerator(ref NativeMethods.MSG lpmsg, short wID) {
            return NativeMethods.S_FALSE;
        }


        ///////////////////////////////////////////////////////////////////////////
        // IDocHostUIHandler Implementation

        public virtual int ShowContextMenu(int dwID, NativeMethods.POINT pt, object pcmdtReserved, object pdispReserved) {
            return NativeMethods.S_OK;
        }

        public virtual int GetHostInfo(NativeMethods.DOCHOSTUIINFO info) {
            info.dwDoubleClick = NativeMethods.DOCHOSTUIDBLCLICK.DEFAULT;
            info.dwFlags = NativeMethods.DOCHOSTUIFLAG.FLAT_SCROLLBAR |
                           NativeMethods.DOCHOSTUIFLAG.NO3DBORDER |
                           NativeMethods.DOCHOSTUIFLAG.DIALOG |
                           NativeMethods.DOCHOSTUIFLAG.DISABLE_SCRIPT_INACTIVE;

            return NativeMethods.S_OK;
        }

        public virtual int EnableModeless(bool fEnable) {
            return NativeMethods.S_OK;
        }

        public virtual int ShowUI(int dwID, NativeMethods.IOleInPlaceActiveObject activeObject, NativeMethods.IOleCommandTarget commandTarget, NativeMethods.IOleInPlaceFrame frame, NativeMethods.IOleInPlaceUIWindow doc) {
            return NativeMethods.S_OK;
        }

        public virtual int HideUI() {
            return NativeMethods.S_OK;
        }

        public virtual int UpdateUI() {
            return NativeMethods.S_OK;
        }

        public virtual int OnDocWindowActivate(bool fActivate) {
            return NativeMethods.E_NOTIMPL;
        }

        public virtual int OnFrameWindowActivate(bool fActivate) {
            return NativeMethods.E_NOTIMPL;
        }

        public virtual int ResizeBorder(NativeMethods.COMRECT rect, NativeMethods.IOleInPlaceUIWindow doc, bool fFrameWindow) {
            return NativeMethods.E_NOTIMPL;
        }

        public virtual int GetOptionKeyPath(string[] pbstrKey, int dw) {
            pbstrKey[0] = null;
            return NativeMethods.S_OK;
        }

        public virtual int GetDropTarget(NativeMethods.IOleDropTarget pDropTarget, out NativeMethods.IOleDropTarget ppDropTarget) {
            ppDropTarget = null;
            return NativeMethods.S_FALSE;
        }

        public virtual int GetExternal(out object ppDispatch) {
            ppDispatch = null;
            return NativeMethods.S_OK;
        }

        public virtual int TranslateAccelerator(ref NativeMethods.MSG msg, ref Guid group, int nCmdID) {
            return NativeMethods.S_OK;
        }

        public virtual int TranslateUrl(int dwTranslate, string strUrlIn, out string pstrUrlOut) {
            pstrUrlOut = null;
            return NativeMethods.E_NOTIMPL;
        }

        public virtual int FilterDataObject(NativeMethods.IOleDataObject pDO, out NativeMethods.IOleDataObject ppDORet) {
            ppDORet = null;
            return NativeMethods.S_OK;
        }


        ///////////////////////////////////////////////////////////////////////////
        // Implementation

        /// <include file='doc\MSHTMLHost.uex' path='docs/doc[@for="TridentSite.CreateDocument"]/*' />
        /// <devdoc>
        ///     Creates a new instance of mshtml and initializes it as a new document
        ///     using its IPersistStreamInit.
        /// </devdoc>
        protected void CreateDocument() {

            try {
                // Create an instance of Trident
                tridentDocument = (NativeMethods.IHTMLDocument2)new NativeMethods.HTMLDocument();
                tridentOleObject = (NativeMethods.IOleObject)tridentDocument;

                // Initialize its client site
                tridentOleObject.SetClientSite((NativeMethods.IOleClientSite)this);

                // Initialize it
                NativeMethods.IPersistStreamInit psi = (NativeMethods.IPersistStreamInit)tridentDocument;
                psi.InitNew();
            }
            catch (Exception e) {
                Debug.Fail(e.ToString());
                throw e;
            }
        }

        /// <include file='doc\MSHTMLHost.uex' path='docs/doc[@for="TridentSite.ActivateDocument"]/*' />
        /// <devdoc>
        ///     Activates the mshtml instance
        /// </devdoc>
        protected void ActivateDocument() {
            Debug.Assert(tridentOleObject != null,
                         "How'd we get here when trident is null!");

            try {
                NativeMethods.COMRECT r = new NativeMethods.COMRECT();

                NativeMethods.GetClientRect(parentControl.Handle, r);
                tridentOleObject.DoVerb(NativeMethods.OLEIVERB_UIACTIVATE, IntPtr.Zero, (NativeMethods.IOleClientSite)this, 0, parentControl.Handle, r);
            }
            catch (Exception e) {
                Debug.Fail(e.ToString());
            }
        }
    }
}
