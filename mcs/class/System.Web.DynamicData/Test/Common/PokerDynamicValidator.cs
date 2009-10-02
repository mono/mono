using System;
using System.Web.DynamicData;

namespace MonoTests.Common
{
	public class PokerDynamicValidator : DynamicValidator
	{
		public Exception GetValidationException ()
		{
			return ValidationException;
		}

		public void SetValidationException (Exception ex)
		{
			ValidationException = ex;
		}

		public bool CallControlPropertiesValid ()
		{
			return ControlPropertiesValid ();
		}

		public bool CallEvaluateIsValid ()
		{
			return EvaluateIsValid ();
		}

		public void CallValidateException (Exception ex)
		{
			ValidateException (ex);
		}
	}
}
