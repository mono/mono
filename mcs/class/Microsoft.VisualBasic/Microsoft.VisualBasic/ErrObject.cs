//
// ErrObject.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net) 
//   Francesco Delfino (pluto@tipic.com)
//
// (C) 2002 Chris J Breisch
//     2002 pluto@tipic.com
//

using System;
using Microsoft.VisualBasic.CompilerServices;

namespace Microsoft.VisualBasic 
{
	sealed public class ErrObject {

		private System.Exception pException;
		private int pErl;
		private int pNumber;
		private string pSource;
		private string pDescription;
		private string pHelpFile;
		private int pHelpContext;

		private int pLastDllError;

		private bool m_bDontMapException = false;
		private bool NumberIsSet = false;
		private bool ClearOnCapture = false;
		private bool SourceIsSet = false;
		private bool DescriptionIsSet = false;
		private bool HelpFileIsSet = false;
		private bool HelpContextIsSet = false;

		public ErrObject()
	        {
			Clear();
		}

		public void Clear () 
		{
			pException = null;
			pErl = 0;
			pNumber = 0;
			pSource = "";
			pDescription = "";
			pHelpFile = "";
			pHelpContext = 0;

			m_bDontMapException = false;
			NumberIsSet = false;
			ClearOnCapture = false;
			SourceIsSet = false;
			DescriptionIsSet = false;
			HelpFileIsSet = false;
			HelpContextIsSet = false;
			ClearOnCapture = true;
		}

		public void CaptureException (Exception ex)
		{
			if(ex == pException)
				return;
			if(ClearOnCapture == true)
				Clear();
			else
				ClearOnCapture = true;

			pException = ex;
		}

		public void CaptureException (Exception ex, int lErl)
		{
			CaptureException(ex);
			pErl = lErl;
		}

		public Exception CreateException (int Number, String Description)
		{
			Exception ex;

			Clear();
			this.Number = Number;
			if(Number == 0)
				throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", "Number"));
			ex = MapNumberToException(pNumber, Description);
			this.ClearOnCapture = false;
			return ex;
		}

		private String FilterDefaultMessage(String Msg)
		{
			if(pException == null)
				return Msg;

			string Msg1 = Utils.GetResourceString(this.Number);

			if(Msg == null || Msg.Length == 0)
				return Msg1;
			else if(String.CompareOrdinal("Exception from HRESULT: 0x", 0, Msg, 0, System.Math.Min(Msg.Length, 26)) == 0) {
				return (Msg1 != null ? Msg1 : Msg);
			}
			else
				return Msg;
		}

		private Exception MapNumberToException (int Number, String Description)
		{
			bool Ignored = false;
			return ExceptionUtils.BuildException(Number, Description, ref Ignored);
		}


		// This function needs to be reviewed
		private int MapExceptionToNumber (Exception ex)
		{
			int hResult = 0;

			if(ex is ArgumentException) {
				hResult = unchecked((int)0x80070057);
			}
			else if(ex is ArithmeticException) {
				if(ex is NotFiniteNumberException) { 
					if((ex as NotFiniteNumberException).OffendingNumber == 0)
						return 11;
					else
						return 6;
				}
				else {
					hResult = unchecked((int)0x80070216);     
				}
			}
			else if(ex is ArrayTypeMismatchException) {
				hResult = unchecked((int)0x80131503);
			}
			// else if(exType.Equals(IndexOutOfRangeException)) {
			//	hResult = (exType.Equals(IndexOutOfRangeException)).HResult;     
			// }
			else if(ex is InvalidCastException) {
				hResult = unchecked((int)0x80004002);
			}
			else if(ex is NotSupportedException) {
				hResult = unchecked((int)0x80131515);
			}
			else if(ex is NullReferenceException) {
				hResult = unchecked((int)0x80004003);
			}
			else if(ex is UnauthorizedAccessException) {
				hResult = unchecked((int)0x80131500);
			}

			else {
				hResult = unchecked((int)0x80004005);
			}

			hResult = ExceptionUtils.getVBFromDotNet(hResult);
			if(hResult != 0 )
				return hResult;
			else
				return 5;
		}

		private void ParseHelpLink (String HelpLink)
		{
			int ind;

			if(HelpLink == null || HelpLink.Length == 0) {
				if(HelpContextIsSet == false)
					this.HelpContext = 0;

				if (HelpFileIsSet == false)
					this.HelpFile = "";
			}
			else {
				ind = HelpLink.IndexOf("#");

				if(ind != -1) {
					if(HelpContextIsSet == false) {
						if(ind < HelpLink.Length)
							this.HelpContext = IntegerType.FromString(HelpLink.Substring(ind + 1));
						else
							this.HelpContext = 0;
					}
					if (HelpFileIsSet == false)
						this.HelpFile = HelpLink.Substring(0, ind);
				}
				else {
					if (HelpContextIsSet == false)
						this.HelpContext = 0;

					if (!this.HelpFileIsSet)
						this.HelpFile = HelpLink;
				}
			}
		}

		public int MapErrorNumber (int Number)
		{
			if(Number > 65535)
				throw new ArgumentException(VBUtils.GetResourceString("Argument_InvalidValue1", "Number"));


			if(Number >= 0)
				return Number;
			else
				return ExceptionUtils.fromDotNetToVB(Number);
		}

		private String MakeHelpLink(String HelpFile, int HelpContext)
		{
			return HelpFile + "#" + StringType.FromInteger(HelpContext);
		}

		[MonoTODO]
		public void Raise (System.Int32 Number, 
				   [System.Runtime.InteropServices.Optional] 
				   [System.ComponentModel.DefaultValue(null)] System.Object Source, 
				   [System.Runtime.InteropServices.Optional] 
				   [System.ComponentModel.DefaultValue(null)] System.Object Description, 
				   [System.Runtime.InteropServices.Optional] 
				   [System.ComponentModel.DefaultValue(null)] System.Object HelpFile, 
				   [System.Runtime.InteropServices.Optional] 
				   [System.ComponentModel.DefaultValue(null)] System.Object HelpContext) 
		{ 
			Exception e;
			
		        if(Number == 0)
				throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", "Number"));

			this.Number = Number;

			if(Source != null)
				this.Source = StringType.FromObject(Source);
			else
				this.Source = "";

			if(HelpFile != null)
				this.HelpFile = StringType.FromObject(HelpFile);

			if(HelpContext != null)
				this.HelpContext = IntegerType.FromObject(HelpContext);

			if(Description != null)
				this.Description = StringType.FromObject(Description);
			else if (DescriptionIsSet == false)
				this.Description = Utils.GetResourceString(pNumber);

			e = MapNumberToException(pNumber, pDescription);


			e.Source = pSource;
			e.HelpLink = MakeHelpLink(pHelpFile, pHelpContext);


			ClearOnCapture = false;
			throw e;
		} 

		public void SetUnmappedError (int Number)
		{
			Clear();
			this.Number = Number;
			ClearOnCapture = false;
		}

		public System.Exception GetException () 
		{
			return pException;
		}

		public System.String Description {  
			get { 
				if(DescriptionIsSet)
					return pDescription;

				if(pException == null)
					return "";

				if(pException is Exception)
					this.Description = FilterDefaultMessage(pException.Message);
				else
					this.Description = FilterDefaultMessage(pException.Message);
			
				return pDescription; 
			} 
			set { 
				pDescription = value;
				DescriptionIsSet = true;
			} 
		}

		public System.Int32 Erl {  
			get { 
				return pErl;
			} 
		}

		public System.Int32 HelpContext 
		{ 
			get { 
				if(HelpContextIsSet)
					return pHelpContext; 

				if(pException != null)
				{
					if(pException is Exception)
						ParseHelpLink(pException.HelpLink);
					else
						ParseHelpLink("");

					return this.pHelpContext;
				}
				return 0;
			}

			set { 
				pHelpContext = value;
				HelpContextIsSet = true;
			} 
		}

		public System.String HelpFile {  
			get { 
				if (HelpFileIsSet == true)
					return pHelpFile;
        
				if(pException != null) {
					if(pException is Exception)
						ParseHelpLink((pException as Exception).HelpLink);
					else
						ParseHelpLink("");

					return pHelpFile;
				}
				return "";
			} 
			set { 
				pHelpFile = value;
				HelpFileIsSet = true;
			} 
		}

		[MonoTODO]
		public System.Int32 LastDllError {  
			get { 
				return 0; 
			} 
		}

		public System.Int32 Number {  
			get { 
				if(NumberIsSet)
					return pNumber;

				if(pException == null)
					return 0;

				this.Number = MapExceptionToNumber(pException);
				return pNumber;
			} 
			set { 
				pNumber = MapErrorNumber(value);
				NumberIsSet = true;
			} 
		}

		[MonoTODO]
		public System.String Source {  
			get { 
				if(SourceIsSet)
					return pSource;

				if(pException == null)
					return "";

				if(pException is Exception)
					this.Source = pException.Source;
				else
					this.Source = "";

				return pSource;
			} 
			set { 
				pSource = value;
				SourceIsSet = true;
			} 
		}


	}
}

