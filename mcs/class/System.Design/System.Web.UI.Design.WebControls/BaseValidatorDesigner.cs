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
