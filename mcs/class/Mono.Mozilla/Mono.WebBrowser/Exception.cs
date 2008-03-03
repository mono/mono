using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
namespace Mono.WebBrowser
{
	public class Exception : System.Exception
	{
		ErrorCodes code;
		static ArrayList messages = new ArrayList();

		static Exception ()
		{
			messages.Insert ((int) ErrorCodes.Other, String.Intern ("A critical error occurred."));
			messages.Insert ((int) ErrorCodes.GluezillaInit, String.Intern ("An error occurred while initializing gluezilla. Please make sure you have libgluezilla installed."));
			messages.Insert ((int) ErrorCodes.EngineNotSupported, String.Intern ("Browser engine not supported at this time: "));
			messages.Insert ((int) ErrorCodes.ServiceManager, String.Intern ("Error obtaining a handle to the service manager."));
			messages.Insert ((int) ErrorCodes.IOService, String.Intern ("Error obtaining a handle to the io service."));
			messages.Insert ((int) ErrorCodes.DirectoryService, String.Intern ("Error obtaining a handle to the directory service."));
			messages.Insert ((int) ErrorCodes.StreamNotOpen, String.Intern ("Stream is not open for writing. Call OpenStream before appending."));
		}


		internal ErrorCodes ErrorCode
		{
			get { return code; }
		}

		internal Exception (ErrorCodes code)
			: base (GetMessage (code, String.Empty))
		{
			this.code = code;
		}

		internal Exception (ErrorCodes code, string message)
			: base (GetMessage (code, message))
		{
			this.code = code;
		}

		internal Exception (ErrorCodes code, System.Exception innerException)
			: base (GetMessage (code, String.Empty), innerException)
		{
			this.code = code;
		}

		internal Exception (ErrorCodes code, string message, Exception innerException)
			: base (GetMessage (code, message), innerException)
		{
			this.code = code;
		}

		private static string GetMessage (ErrorCodes code, string message)
		{
			string msg = Exception.messages[(int) code] as string;
			return msg + " " + message;
		}

		internal enum ErrorCodes
		{
			Other,
			GluezillaInit,
			EngineNotSupported,
			ServiceManager,
			IOService,
			DirectoryService,
			StreamNotOpen
		}
	}
}
