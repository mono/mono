/**
 * Project   : Mono
 * Namespace : System.Web.UI.MobileControls.Adapters
 * Class     : MobileTextWriter
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System;
using System.IO;
using System.Web.Mobile;

namespace System.Web.UI.MobileControls.Adapters
{
	public class MobileTextWriter : MultiPartWriter
	{
		private MobileCapabilities device;
		
		public MobileTextWriter(TextWriter writer,
		                 MobileCapabilities capabilities) : base(writer)
		{
			this.device = capabilities;
			throw new NotImplementedException();
		}
		
		public MobileCapabilities Device
		{
			get
			{
				return device;
			}
		}
	}
}
