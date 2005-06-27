
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
/**
 * Namespace:   System.Web.UI.Design.WebControls
 * Class:       BaseValidatorDesigner
 *
 * Author:      Gaurav Vaish
 * Maintainer:  gvaish_mono@lycos.com
 *
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.Web.UI.WebControls;
using System.Web.UI.Design;

namespace System.Web.UI.Design.WebControls
{
	public class BaseValidatorDesigner : ControlDesigner
	{
		public BaseValidatorDesigner()
		{
		}
		
		public override string GetDesignTimeHtml()
		{
			BaseValidator validator = (BaseValidator)Component;
			validator.IsValid = false;
			string errMsg = validator.ErrorMessage;
			ValidatorDisplay dispBeh = validator.Display;
			bool toSetErrMesg = true;
			if(dispBeh != ValidatorDisplay.None &&
			   (errMsg.Length > 0 || validator.Text.Trim().Length > 0))
			{
				toSetErrMesg = false;
			}
			if(toSetErrMesg)
			{
				validator.ErrorMessage = '[' + validator.ID + ']';
				validator.Display = ValidatorDisplay.Static;
			}
			string retVal = base.GetDesignTimeHtml();
			if(toSetErrMesg)
			{
				validator.ErrorMessage = errMsg;
				validator.Display = dispBeh;
			}
			return retVal;
		}
	}
}
