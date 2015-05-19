//
// System.Web.Mail.MailAttachment.cs
//
// Author:
//    Lawrence Pit (loz@cable.a2000.nl)
//    Per Arneng (pt99par@student.bth.se)
//    Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.IO;
using System.Security;
using System.Security.Permissions;

namespace System.Web.Mail
{
	[Obsolete ("The recommended alternative is System.Net.Mail.Attachment. http://go.microsoft.com/fwlink/?linkid=14202")]
	public class MailAttachment
	{
		string filename;
		MailEncoding encoding;
		
		public MailAttachment (string filename) : 
			this (filename, MailEncoding.Base64) 
		{
		}
		
		public MailAttachment (string filename, MailEncoding encoding) 
		{
			if (SecurityManager.SecurityEnabled) {
				new FileIOPermission (FileIOPermissionAccess.Read, filename).Demand ();
			}

			if (!File.Exists (filename)) {
				string msg = Locale.GetText ("Cannot find file: '{0}'.");
				throw new HttpException (String.Format (msg, filename));
			}

			this.filename = filename;
			this.encoding = encoding;
		}

		// Properties
		public string Filename 
		{
			get { return filename; } 
		}
		
		public MailEncoding Encoding 
		{
			get { return encoding; } 
		}
	}
}
