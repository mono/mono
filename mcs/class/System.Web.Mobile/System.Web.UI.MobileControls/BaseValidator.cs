/**
 * Project   : Mono
 * Namespace : System.Web.UI.MobileControls
 * Class     : BaseValidator
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System.ComponentModel;
using System.Web.UI;
using System.Web.Mobile;
using System.Web.UI.WebControls;

namespace System.Web.UI.MobileControls
{
	public abstract class BaseValidator : TextControl, IValidator
	{
		private System.Web.UI.WebControls.BaseValidator webBaseValidator;
		private bool isValid = true;

		protected BaseValidator()
		{
			StyleReference = "error";
			webBaseValidator = CreateWebValidator();
			if(webBaseValidator == null)
				this.webBaseValidator = new DefaultWebValidator();
			Controls.Add(webBaseValidator);
			webBaseValidator.Display = ValidatorDisplay.Dynamic;
		}

		private class DefaultWebValidator
		                  : System.Web.UI.WebControls.BaseValidator
		{
			protected override bool EvaluateIsValid()
			{
				return false;
			}
		}

		protected virtual System.Web.UI.WebControls.BaseValidator CreateWebValidator()
		{
			return null;
		}

		public string ErrorMessage
		{
			get
			{
				return webBaseValidator.ErrorMessage;
			}
			set
			{
				webBaseValidator.ErrorMessage = value;
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
			if(Visible)
			{
				Control parent = Parent;
				bool visible = true;
				while(parent != null)
				{
					if(!parent.Visible)
					{
						visible = false;
						break;
					}
					parent = parent.Parent;
				}
				if(visible)
					EvaluateIsValid();
			} else
			{
				IsValid = true;
			}
		}

		protected abstract bool EvaluateIsValid();

		public string ControlToValidate
		{
			get
			{
				return webBaseValidator.ControlToValidate;
			}
			set
			{
				webBaseValidator.ControlToValidate = value;
			}
		}

		public ValidatorDisplay Display
		{
			get
			{
				return webBaseValidator.Display;
			}
			set
			{
				webBaseValidator.Display = value;
			}
		}

		public override string StyleReference
		{
			get
			{
				return base.StyleReference;
			}
			set
			{
				base.StyleReference = value;
			}
		}

		public override int VisibleWeight
		{
			get
			{
				return 0;
			}
		}

		protected void CheckControlValidationProperty(string name,
		                                         string propertyName)
		{
			Control ctrl = NamingContainer.FindControl(name);
			if(ctrl == null)
			{
				// FIXME
				throw new ArgumentException("BaseValidator_ControlNotFound");
			}
			PropertyDescriptor pd = System.Web.UI.WebControls.BaseValidator.GetValidationProperty(ctrl);
			if(pd == null)
			{
				// FIXME
				throw new ArgumentException("BaseValidator_BadControlType");
			}
		}

		protected virtual bool ControlPropertiesValid()
		{
			string ctrl = ControlToValidate;
			if(ctrl.Length == 0)
			{
				// FIXME
				throw new ArgumentException("BaseValidator_ControlToValidateBlank");
			}
			CheckControlValidationProperty(ctrl, "ControlToValidate");
			return true;
		}

		internal bool EvaluateIsValidInternal()
		{
			try
			{
				webBaseValidator.Validate();
			} catch(Exception)
			{
				string thisID = ID;
				ID = webBaseValidator.ID;
				webBaseValidator.ID = thisID;
				try
				{
					webBaseValidator.Validate();
				} finally
				{
					thisID = ID;
					ID = webBaseValidator.ID;
					webBaseValidator.ID = thisID;
				}
			}
			return webBaseValidator.IsValid;
		}

		protected override void OnInit(EventArgs e)
		{
			Page.Validators.Add(this);
			base.OnInit(e);
		}

		protected override void OnPreRender(EventArgs e)
		{
			if(MobilePage.ActiveForm == Form)
				ControlPropertiesValid();
			base.OnPreRender(e);
		}
	}
}
