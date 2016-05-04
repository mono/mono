//------------------------------------------------------------------------------
// <copyright file="RuntimeLiteralTextParser.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Web.UI;

namespace System.Web.UI.MobileControls
{
    /*
     * RuntimeLiteralTextParser class.
     *
     * This is a specialized version of the LiteralTextParser class.
     * It creates a set of controls from the parsed literal text at.
     * runtime. This class can be used by the TextView control to
     * accept and render dynamic formatted text.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */

/*
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class RuntimeLiteralTextParser : LiteralTextParser
    {
        Control _parentControl;
        bool _elementsAdded = false;

        internal RuntimeLiteralTextParser(Control parentControl)
        {
            _parentControl = parentControl;
        }

        protected override void ProcessElement(LiteralElement element)
        {
            MobileControl ctl;

            switch (element.Type)
            {
                case LiteralElementType.Text:
                {
                    LiteralText textCtl = new LiteralText();
                    textCtl.BreakAfter = element.BreakAfter;
                    ctl = textCtl;
                    break;
                }

                case LiteralElementType.Anchor:
                {
                    Link link = new LiteralLink();
                    link.NavigateUrl = element.GetAttribute ("href");
                    link.BreakAfter = element.BreakAfter;

                    ctl = link;
                    break;
                }

                default:
                    return;
            }

            // Need to add text as a child, so that it can be written out unscathed.

            if (element.Text != null)
            {
                ctl.Controls.Add(new LiteralControl(element.Text));
            }

            if ((element.Format & LiteralFormat.Bold) == LiteralFormat.Bold)
            {
                ctl.Font.Bold = BooleanOption.True;
            }
            if ((element.Format & LiteralFormat.Italic) == LiteralFormat.Italic)
            {
                ctl.Font.Italic = BooleanOption.True;
            }

            _parentControl.Controls.Add (ctl);
            _elementsAdded = true;
        }

        protected override void ProcessTagInnerText(String text)
        {
            Debug.Assert(false);
        }

        protected override bool IgnoreWhiteSpaceElement(LiteralElement element)
        {
            return !_elementsAdded;
        }

    }
*/
}

