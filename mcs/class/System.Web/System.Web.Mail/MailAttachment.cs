//
// System.Web.Mail.MailAttachment.cs
//
// Author:
//    Lawrence Pit (loz@cable.a2000.nl)
//    Per Arneng (pt99par@student.bth.se)
//

namespace System.Web.Mail
{
	public class MailAttachment
	{
		private string filename;
		private MailEncoding encoding;
		
		public MailAttachment (string filename) : 
			this (filename, MailEncoding.Base64) 
		{
		}
		
		public MailAttachment (string filename, MailEncoding encoding) 
		{
			this.filename = filename;
			this.encoding = encoding;
			try {
				System.IO.File.OpenRead (filename).Close ();
			} catch (Exception) {
			    throw new System.Web.HttpException ("Cannot find file: '" + 
								filename + "'." );
			}			
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
	
} //namespace System.Web.Mail
