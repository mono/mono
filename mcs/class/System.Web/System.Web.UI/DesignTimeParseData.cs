//
// System.Web.UI.DesignTimeParseData.cs
//
// Authors:
//   Gaurav Vaish (gvaish@iitk.ac.in)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2001 Gaurav Vaish
// (C) 2003 Andreas Nahr
//

using System;
using System.Web;
using System.ComponentModel.Design;

namespace System.Web.UI
{
	public sealed class DesignTimeParseData
	{
		private static bool inDesigner = false;
		private EventHandler dataBindingHandler;
		private IDesignerHost designerHost;
		private string documentUrl;
		private string parseText;

		public DesignTimeParseData(IDesignerHost designerHost, string parseText)
		{
			if (parseText == null)
				throw new ArgumentNullException("parseText");
			if (parseText.Length == 0)
				throw new ArgumentException("parseText cannot be an empty string.", "parseText");

			DesignTimeParseData.inDesigner = true;
			this.designerHost = designerHost;
			this.parseText = parseText;
			this.documentUrl = string.Empty;
		} 

		public EventHandler DataBindingHandler {
			get {return dataBindingHandler;} 
			set {dataBindingHandler = value;}
		}

		public IDesignerHost DesignerHost {
			get {return designerHost;}
		}

		public string DocumentUrl {
			get {return documentUrl;}
			set {
				if (value == null)
					documentUrl = string.Empty;
				else
					documentUrl = value;
			}
		}

		public string ParseText {
			get {return parseText;}
		}

		internal static bool InDesigner {
			get {return inDesigner;}
		}
	}
}
