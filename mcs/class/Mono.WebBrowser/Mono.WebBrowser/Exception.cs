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
// Copyright (c) 2007, 2008 Novell, Inc.
//
// Authors:
//	Andreia Gaita (avidigal@novell.com)
//

using System;
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
			messages.Insert ((int) ErrorCodes.PrefService, String.Intern ("Error obtaining a handle to the preferences service."));
			messages.Insert ((int) ErrorCodes.StreamNotOpen, String.Intern ("Stream is not open for writing. Call OpenStream before appending."));
			messages.Insert ((int) ErrorCodes.Navigation, String.Intern ("An error occurred while initializing the navigation object."));
			messages.Insert ((int) ErrorCodes.AccessibilityService, String.Intern ("Error obtaining a handle to the accessibility service."));
			messages.Insert ((int) ErrorCodes.DocumentEncoderService, String.Intern ("Error obtaining a handle to the document encoder service."));
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
			PrefService,
			StreamNotOpen,
			Navigation,
			AccessibilityService,
			DocumentEncoderService
		}
	}
}
