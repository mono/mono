/**
 * Namespace: System.Web.UI.WebControls
 * Class:     BaseValidator
 * 
 * Author:  Gaurav Vaish
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Status:  20%
 * 
 * (C) Gaurav Vaish (2001)
 */

using System;
using System.Web;
using System.Web.UI;
using System.Drawing;

namespace System.Web.UI.WebControls
{
	public abstract class BaseValidator: Label, IValidator
	{
		//
		private PropertyDescriptor pDesc;
		private string ctValid = String.Empty;
		private ValidatorDisplay vDisp = ValidatorDisplay.Static;
		private bool enableClientScript; //TODO: check the default value := false;
		private bool enabled = true;
		private string errorMessage = String.Empty;
		private Color foreColor = Color.Red;
		private bool isValid = true;
		private bool propertiesValid;
		private bool renderUplevel;

		public static PropertyDescriptor GetValidationProperty(object component)
		{
			//TODO: Have to workout this one!
			return null;
		}
		
		public string ControlToValidate
		{
			get
			{
				return ctValid;
			}
			set
			{
				ctValid = value;
			}
		}
		
		public ValidatorDisplay Display
		{
			get
			{
				return vDisp;
			}
			set
			{
				//TODO: Throw new exception ArgumentException("....") if the value is not valid
				vDisp = value;
			}
		}
		
		public bool EnableClientScript
		{
			get
			{
				return enableClientScript;
			}
			set
			{
				enableClientScript = value;
			}
		}
		
		public override bool Enabled
		{
			get
			{
				return enabled;
			}
			set
			{
				enabled = value;
			}
		}
		
		public string ErrorMessage
		{
			get
			{
				return errorMessage;
			}
			set
			{
				errorMessage = value;
			}
		}
		
		public override Color ForeColor
		{
			get
			{
				return foreColor;
			}
			set
			{
				foreColor = value;
			}
		}
		
		public bool IsValid
		{
			get
			{
				return isValid;
			}
			set
			{
				isValid = value;
			}
		}
		
		public void Validate()
		{
			// TODO: write the validation code
			// TODO: update the value of isValid
		}
		
		protected BaseValidator()
		{
			// Dummy Constructor
		}
		
		protected bool PropertiesValid
		{
			get
			{
				// TODO: throw HttpException, but when? How do I know about all the controls?
				return propertiesValid;
			}
		}
		
		protected bool RenderUplevel
		{
			get
			{
				//TODO: To set the value of renderUplevel. Problem: when, how?
				return renderUplevel;
			}
		}
		
		protected void CheckControlValidationProperty(string name, string propertyName)
		{
			//TODO: I think it needs to be overridden. I may be wrong!
			//TODO: When to throw HttpException(...)
		}

		protected virtual bool ControlPropertiesValid()
		{
			// Do I need to do anything? But what?
			// What do I do with ControlToValidate?
			return true;
		}

		protected virtual bool DetermineRenderUplevel()
		{
			// From where?
			return true;
		}

		protected abstract bool EvaluateIsValid();

		protected string GetControlRenderID(string name)
		{
			// TODO: What value? What is it?
		}

		protected string GetControlValidationValue(string name)
		{
			// TODO: What value? What is it?
		}

		protected void RegisterValidatorCommonScript()
		{
			// TODO: Still wondering!
			// Note: This method is primarily used by control developers
		}

		protected void RegisterValidatorDeclaration()
		{
			// TODO: Still wondering!
			// Note: This method is primarily used by control developers
			// The documentation in M$ refers to: Page_Validators array
		}
	}
}
