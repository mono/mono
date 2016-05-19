//------------------------------------------------------------------------------
// <copyright file="ControlAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.Mobile;
using RootMobile = System.Web.Mobile;
using System.Web.UI.MobileControls;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Security.Permissions;

// We don't recompile this base class in the shipped source samples, as it
// accesses some internal functionality and is a core utility (rather than an
// extension itself).
#if !COMPILING_FOR_SHIPPED_SOURCE

namespace System.Web.UI.MobileControls.Adapters
{

    /*
     * ControlAdapter base class.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */
    /// <include file='doc\ControlAdapter.uex' path='docs/doc[@for="ControlAdapter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public abstract class ControlAdapter : IControlAdapter
    {
        private static readonly String[] LabelIDs = new String[] {
                                                RootMobile.SR.ControlAdapter_BackLabel,
                                                RootMobile.SR.ControlAdapter_GoLabel,
                                                RootMobile.SR.ControlAdapter_OKLabel,
                                                RootMobile.SR.ControlAdapter_MoreLabel,
                                                RootMobile.SR.ControlAdapter_OptionsLabel,
                                                RootMobile.SR.ControlAdapter_NextLabel,
                                                RootMobile.SR.ControlAdapter_PreviousLabel,
                                                RootMobile.SR.ControlAdapter_LinkLabel,
                                                RootMobile.SR.ControlAdapter_PhoneCallLabel
                                           };

        /// <include file='doc\ControlAdapter.uex' path='docs/doc[@for="ControlAdapter.BackLabel"]/*' />
        protected static readonly int BackLabel     = 0;
        /// <include file='doc\ControlAdapter.uex' path='docs/doc[@for="ControlAdapter.GoLabel"]/*' />
        protected static readonly int GoLabel       = 1;
        /// <include file='doc\ControlAdapter.uex' path='docs/doc[@for="ControlAdapter.OKLabel"]/*' />
        protected static readonly int OKLabel       = 2;
        /// <include file='doc\ControlAdapter.uex' path='docs/doc[@for="ControlAdapter.MoreLabel"]/*' />
        protected static readonly int MoreLabel     = 3;
        /// <include file='doc\ControlAdapter.uex' path='docs/doc[@for="ControlAdapter.OptionsLabel"]/*' />
        protected static readonly int OptionsLabel  = 4;
        /// <include file='doc\ControlAdapter.uex' path='docs/doc[@for="ControlAdapter.NextLabel"]/*' />
        protected static readonly int NextLabel     = 5;
        /// <include file='doc\ControlAdapter.uex' path='docs/doc[@for="ControlAdapter.PreviousLabel"]/*' />
        protected static readonly int PreviousLabel = 6;
        /// <include file='doc\ControlAdapter.uex' path='docs/doc[@for="ControlAdapter.LinkLabel"]/*' />
        protected static readonly int LinkLabel     = 7;
        /// <include file='doc\ControlAdapter.uex' path='docs/doc[@for="ControlAdapter.CallLabel"]/*' />
        protected static readonly int CallLabel     = 8;

        private MobileControl _control;

        /// <include file='doc\ControlAdapter.uex' path='docs/doc[@for="ControlAdapter.Control"]/*' />
        public MobileControl Control
        {
            get
            {
                return _control;
            }
            set
            {
                _control = value;
            }
        }

        /// <include file='doc\ControlAdapter.uex' path='docs/doc[@for="ControlAdapter.Page"]/*' />
        public virtual MobilePage Page
        {
            get
            {
                return Control.MobilePage;
            }
            set
            {
                // Do not expect to be called directly.  Subclasses should
                // override this when needed.
                throw new Exception(
                    SR.GetString(
                        SR.ControlAdapterBasePagePropertyShouldNotBeSet));
            }
        }

        /// <include file='doc\ControlAdapter.uex' path='docs/doc[@for="ControlAdapter.Device"]/*' />
        public virtual MobileCapabilities Device
        {
            get
            {
                return (MobileCapabilities)Page.Request.Browser;
            }
        }

        /// <include file='doc\ControlAdapter.uex' path='docs/doc[@for="ControlAdapter.OnInit"]/*' />
        public virtual void OnInit(EventArgs e){}
        /// <include file='doc\ControlAdapter.uex' path='docs/doc[@for="ControlAdapter.OnLoad"]/*' />
        public virtual void OnLoad(EventArgs e){}
        /// <include file='doc\ControlAdapter.uex' path='docs/doc[@for="ControlAdapter.OnPreRender"]/*' />
        public virtual void OnPreRender(EventArgs e){}
        
        /// <include file='doc\ControlAdapter.uex' path='docs/doc[@for="ControlAdapter.Render"]/*' />
        public virtual void Render(HtmlTextWriter writer)
        {
            RenderChildren(writer);
        }

        /// <include file='doc\ControlAdapter.uex' path='docs/doc[@for="ControlAdapter.OnUnload"]/*' />
        public virtual void OnUnload(EventArgs e){}

        /// <include file='doc\ControlAdapter.uex' path='docs/doc[@for="ControlAdapter.HandlePostBackEvent"]/*' />
        public virtual bool HandlePostBackEvent(String eventArgument)
        {
            return false;
        }

        // By default, always return false, so the control itself will handle
        // it. 
        /// <include file='doc\ControlAdapter.uex' path='docs/doc[@for="ControlAdapter.LoadPostData"]/*' />
        public virtual bool LoadPostData(String key,
                                         NameValueCollection data,
                                         Object controlPrivateData,
                                         out bool dataChanged)
        {
            dataChanged = false;
            return false;
        }

        /// <include file='doc\ControlAdapter.uex' path='docs/doc[@for="ControlAdapter.LoadAdapterState"]/*' />
        public virtual void LoadAdapterState(Object state)
        {
        }

        /// <include file='doc\ControlAdapter.uex' path='docs/doc[@for="ControlAdapter.SaveAdapterState"]/*' />
        public virtual Object SaveAdapterState()
        { 
            return null;
        }

        /// <include file='doc\ControlAdapter.uex' path='docs/doc[@for="ControlAdapter.CreateTemplatedUI"]/*' />
        public virtual void CreateTemplatedUI(bool doDataBind)
        {
            // No device specific templated UI to create.

            Control.CreateDefaultTemplatedUI(doDataBind);
        }

        //  convenience methods here
        /// <include file='doc\ControlAdapter.uex' path='docs/doc[@for="ControlAdapter.Style"]/*' />
        public Style Style
        {
            get
            {
                return Control.Style;
            }
        }

        /// <include file='doc\ControlAdapter.uex' path='docs/doc[@for="ControlAdapter.RenderChildren"]/*' />
        protected void RenderChildren(HtmlTextWriter writer)
        {
            if (Control.HasControls())
            {
                foreach (Control child in Control.Controls)
                {
                    child.RenderControl(writer);
                }
            }
        }

        /// <include file='doc\ControlAdapter.uex' path='docs/doc[@for="ControlAdapter.VisibleWeight"]/*' />
        public virtual int VisibleWeight
        {
            get
            {
                return ControlPager.UseDefaultWeight;
            }
        }

        /// <include file='doc\ControlAdapter.uex' path='docs/doc[@for="ControlAdapter.ItemWeight"]/*' />
        public virtual int ItemWeight
        {
            get
            {
                return ControlPager.UseDefaultWeight;
            }
        }

        // The following method is used by PageAdapter subclasses of
        // ControlAdapter for determining the optimum page weight for
        // a given device.  Algorithm is as follows:
        //     1) First look for the "optimumPageWeight" parameter set
        //        for the device.  If it exists, and can be converted
        //        to an integer, use it.
        //     2) Otherwise, look for the "screenCharactersHeight" parameter.
        //        If it exists, and can be converted to an integer, multiply
        //        it by 100 and use the result.
        //     3) As a last resort, use the default provided by the calling
        //        PageAdapter.
        /// <include file='doc\ControlAdapter.uex' path='docs/doc[@for="ControlAdapter.CalculateOptimumPageWeight"]/*' />
        protected virtual int CalculateOptimumPageWeight(int defaultPageWeight)
        {
            int optimumPageWeight = 0;

            // Pull OptimumPageWeight from the web.config parameter of the same
            // name, when present.
            String pageWeight = Device[Constants.OptimumPageWeightParameter]; 

            if (pageWeight != null)
            {
                try
                {
                    optimumPageWeight = Convert.ToInt32(pageWeight, CultureInfo.InvariantCulture);
                }
                catch
                {
                    optimumPageWeight = 0;
                }
            }

            if (optimumPageWeight <= 0)
            {
                // If OptimumPageWeight isn't established explicitly, attempt to
                // construct it as 100 * number of lines of characters.
                String numLinesStr = Device[Constants.ScreenCharactersHeightParameter];
                if (numLinesStr != null)
                {
                    try
                    {
                        int numLines = Convert.ToInt32(numLinesStr, CultureInfo.InvariantCulture);
                        optimumPageWeight = 100 * numLines;
                    }
                    catch
                    {
                        optimumPageWeight = 0;
                    }
                }
            }

            if (optimumPageWeight <= 0)
            {
                optimumPageWeight = defaultPageWeight;
            }

            return optimumPageWeight;
        }

        private String[] _defaultLabels = null;

        /// <include file='doc\ControlAdapter.uex' path='docs/doc[@for="ControlAdapter.GetDefaultLabel"]/*' />
        protected String GetDefaultLabel(int labelID)
        {
            if ((labelID < 0) || (labelID >= LabelIDs.Length))
            {
                throw new ArgumentException(System.Web.Mobile.SR.GetString(
                                                System.Web.Mobile.SR.ControlAdapter_InvalidDefaultLabel));
            }

            MobilePage page = Page;
            if (page != null)
            {
                ControlAdapter pageAdapter = (ControlAdapter)page.Adapter;
                if (pageAdapter._defaultLabels == null)
                {
                    pageAdapter._defaultLabels = new String[LabelIDs.Length];
                }

                String labelValue = pageAdapter._defaultLabels[labelID];
                if (labelValue == null)
                {
                    labelValue = System.Web.Mobile.SR.GetString(LabelIDs[labelID]);
                    pageAdapter._defaultLabels[labelID] = labelValue;
                }

                return labelValue;
            }
            else
            {
                return System.Web.Mobile.SR.GetString(LabelIDs[labelID]);
            }
        }
    }

    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class EmptyControlAdapter : ControlAdapter {
        internal EmptyControlAdapter() {}
    }

}

#endif
