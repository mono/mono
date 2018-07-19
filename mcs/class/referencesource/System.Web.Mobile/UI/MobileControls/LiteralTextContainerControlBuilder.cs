//------------------------------------------------------------------------------
// <copyright file="LiteralTextContainerControlBuilder.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.Web;
using System.Web.UI;
using System.Web.UI.Design.WebControls;
using System.Web.UI.WebControls;
using System.Security.Permissions;

namespace System.Web.UI.MobileControls
{
    /*
     * Control builder for containers of literal text
     *
     * Copyright (c) 2000 Microsoft Corporation
     */

    /// <include file='doc\LiteralTextContainerControlBuilder.uex' path='docs/doc[@for="LiteralTextContainerControlBuilder"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class LiteralTextContainerControlBuilder : MobileControlBuilder
    {
        private CompileLiteralTextParser _textParser = null;
        private bool _controlsInserted = false;

        internal LiteralTextContainerControlBuilder()
        {
        }

        internal CompileLiteralTextParser TextParser
        {
            get
            {
                if (_textParser == null)
                {
                    _textParser = 
                        new CompileLiteralTextParser(Parser, this, String.Empty, 1);
                    if (_controlsInserted)
                    {
                        _textParser.ResetBreaking();
                        _textParser.ResetNewParagraph();
                    }
                }
                return _textParser;
            }
        }

        /// <include file='doc\LiteralTextContainerControlBuilder.uex' path='docs/doc[@for="LiteralTextContainerControlBuilder.AppendLiteralString"]/*' />
        public override void AppendLiteralString(String text)
        {
            if (InDesigner)
            {
                base.AppendLiteralString(text);
            }
            else
            {
                if (LiteralTextParser.IsValidText(text))
                {
                    TextParser.Parse(text);
                }
            }
        }

        /// <include file='doc\LiteralTextContainerControlBuilder.uex' path='docs/doc[@for="LiteralTextContainerControlBuilder.AppendSubBuilder"]/*' />
        public override void AppendSubBuilder(ControlBuilder subBuilder)
        {
            if (InDesigner)
            {
                base.AppendSubBuilder(subBuilder);
            }

            // The first one is used if ASP.NET is compiled with FAST_DATABINDING off. The second
            // is used if it is compiled with FAST_DATABINDING on. Note: We can't do a type 
            // comparison because CodeBlockBuilder is internal.
            //else if (typeof(DataBoundLiteralControl).IsAssignableFrom(subBuilder.ControlType))
            else if (subBuilder.GetType().FullName == "System.Web.UI.CodeBlockBuilder")
            {
                TextParser.AddDataBinding(subBuilder);
            }
            else
            {
                base.AppendSubBuilder(subBuilder);
                if (subBuilder.ControlType != typeof(LiteralText))
                {
                    if (_textParser != null)
                    {
                        _textParser.ResetBreaking();
                    }
                    else
                    {
                        _controlsInserted = true;
                    }
                }
            }
        }
    }
}


