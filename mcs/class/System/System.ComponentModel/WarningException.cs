// 
// System.ComponentModel.WarningException.cs
//
// Author:
//   Asier Llano Palacios (asierllano@infonegocio.com)
//

using System;
using System.Security;

namespace System.ComponentModel {

	[SuppressUnmanagedCodeSecurity]
	public class WarningException : SystemException
	{
		private string helpUrl;
		private string helpTopic;
	
		public WarningException( string message ) 
			: base( message ) {
			helpUrl = null;
			helpTopic = null;
		}

		public WarningException( string message, string helpUrl )
			: base( message ) {
			this.helpUrl = helpUrl;
			this.helpTopic = null;
		}

		public WarningException( string message, string helpUrl, string helpTopic ) {
			this.helpUrl = helpUrl;
			this.helpTopic = helpTopic;
		}

		public string HelpTopic {
			get {
				return helpTopic;
			}
		}

		public string HelpUrl {
			get {
				return helpUrl;
			}
		}
	}

}

