//------------------------------------------------------------------------------
// <copyright file="LiteralText.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Web;
using System.Web.UI;
using System.Web.UI.Design.WebControls;
using System.Web.UI.HtmlControls;
using System.Security.Permissions;

namespace System.Web.UI.MobileControls
{

    /*
     * Literal Text class. This is the control created for literal text in a form.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */
    /// <include file='doc\LiteralText.uex' path='docs/doc[@for="LiteralText"]/*' />
    [
        ControlBuilderAttribute(typeof(LiteralTextControlBuilder)),
        ToolboxItem(false)
    ]
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class LiteralText : PagedControl
    {
        // Note that this value doesn't relate to device specific info
        // because this is simply a unit size to define how many characters
        // to be counted as an item for pagination.  Depending on each
        // device's page weight, different numbers of items will be returned
        // for display.
        private static readonly int PagingUnitSize = ControlPager.DefaultWeight;  // chars

        /// <include file='doc\LiteralText.uex' path='docs/doc[@for="LiteralText.Text"]/*' />
        [
            Bindable(false),
            Browsable(false),
        ]
        public String Text
        {
            // Override MobileControl default behavior for InnerText

            get
            {
                String s = (String)ViewState[MobileControl.InnerTextViewStateKey];
                return s != null ? s : InnerText;
            }

            set
            {
                ViewState[MobileControl.InnerTextViewStateKey] = value;
            }
        }

        //  this attempts to split on word, return or sentence boundaries.
        //  use of '.' to indicate end of sentence assumes western languages
        //  perhaps if we get rid of '.' logic, '\n' preference will be sufficient
        private int CalculateOffset(int itemIndex)
        {
            if (itemIndex == 0)
            {
                return 0;
            }

            int length = Text.Length;
            int itemSize = (length / InternalItemCount) + 1; 
            int baseOffset = itemSize * itemIndex;

            if (baseOffset >= length)
            {
                return length;
            }

            //  this code scans to find an optimal break location.
            String text = this.Text;
            int scanLength = itemSize / 2;
            int scanStop   = baseOffset - scanLength;
            int foundSpace = -1;
            int foundReturn = -1;
            int lastChar   = -1;
            for (int offset = baseOffset; offset > scanStop; offset--)
            {
                char c = text[offset];
                if (c == '.' && Char.IsWhiteSpace((char)lastChar))
                {
                    //  this may exceed baseOffset by 1, but will never exceed totalChars
                    return offset + 1;
                }
                else if (foundReturn < 0 && c == '\n')
                {
                    foundReturn = offset;
                }
                else if (foundSpace < 0 && Char.IsWhiteSpace(c))   // check performance of this
                {
                    foundSpace = offset;
                }
                lastChar = c;
            }

            if (foundReturn > 0)
            {
                return foundReturn;
            }
            else if (foundSpace > 0)
            {
                return foundSpace;
            }

            return baseOffset;
        }

        /// <include file='doc\LiteralText.uex' path='docs/doc[@for="LiteralText.PagedText"]/*' />
        public String PagedText
        {
            get
            {
                int index = FirstVisibleItemIndex;
                int count = VisibleItemCount;
                String text = Text;

                if (count > text.Length)
                {
                    return text;
                }

                int start = CalculateOffset(index);
                int stop = CalculateOffset(index + count);

                // If not at the beginning or end, skip spaces.

                if (start > 0)
                {
                    while (start < stop && Char.IsWhiteSpace(text[start]) )
                    {
                        start++;
                    }
                }

                if (stop < text.Length)
                {
                    while (Char.IsWhiteSpace(text[stop - 1]) && stop > start)
                    {
                        stop--;
                    }
                }

                return (stop > start) ? text.Substring(start, stop - start) : String.Empty;
            }
        }
        

        /// <include file='doc\LiteralText.uex' path='docs/doc[@for="LiteralText.InternalItemCount"]/*' />
        protected override int InternalItemCount
        {
            get
            {
                return ((Text.Length / PagingUnitSize) + (((Text.Length % PagingUnitSize) > 0) ? 1 : 0));
            }
        }

        /// <include file='doc\LiteralText.uex' path='docs/doc[@for="LiteralText.ItemWeight"]/*' />
        protected override int ItemWeight
        {
            get
            {
                return PagingUnitSize;
            }
        }

        internal override bool TrimInnerText
        {
            get
            {
                return false;
            }
        }
    }

    /*
     * Control builder for literal text.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */

    /// <include file='doc\LiteralText.uex' path='docs/doc[@for="LiteralTextControlBuilder"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class LiteralTextControlBuilder : MobileControlBuilder
    {
        /// <include file='doc\LiteralText.uex' path='docs/doc[@for="LiteralTextControlBuilder.AllowWhitespaceLiterals"]/*' />
        public override bool AllowWhitespaceLiterals() 
        {
            return true;
        }
    }

}
