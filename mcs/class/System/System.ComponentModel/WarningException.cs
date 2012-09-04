// 
// System.ComponentModel.WarningException.cs
//
// Authors:
//	Asier Llano Palacios (asierllano@infonegocio.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
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

using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.ComponentModel {

	[Serializable]
	public class WarningException : SystemException
	{
		private string helpUrl;
		private string helpTopic;
	
		public WarningException (string message)
			: base (message)
		{
		}

		public WarningException (string message, string helpUrl)
			: base (message)
		{
			this.helpUrl = helpUrl;
		}

		public WarningException (string message, string helpUrl, string helpTopic)
			: base (message)
		{
			this.helpUrl = helpUrl;
			this.helpTopic = helpTopic;
		}

		public WarningException () 
			: base (Locale.GetText ("Warning"))
		{
		}

		public WarningException (string message, Exception innerException) 
			: base (message, innerException)
		{
		}

		protected WarningException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
			try {
				helpTopic = info.GetString ("helpTopic");
				helpUrl = info.GetString ("helpUrl");
			}
			catch (SerializationException) {
				//For compliance with previously serialized version:
				helpTopic = info.GetString ("HelpTopic");
				helpUrl = info.GetString ("HelpUrl");
			}
		}

		[SecurityPermission (SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");

			base.GetObjectData (info, context);
			info.AddValue ("helpTopic", helpTopic);
			info.AddValue ("helpUrl", helpUrl);
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
