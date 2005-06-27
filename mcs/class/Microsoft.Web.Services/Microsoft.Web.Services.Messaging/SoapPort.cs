//
// Microsoft.Web.Services.Messaging.SoapPort.cs
//
// Author: Todd Berman <tberman@gentoo.org>
//
// (C) 2003 Todd Berman

using Microsoft.Web.Services;

namespace Microsoft.Web.Services.Messaging 
{

        public abstract class SoapPort
        {
		private Pipeline _pipeline;

		public SoapPort () : base ()
		{
			_pipeline = null;
		}

		protected abstract void FilterMessage (SoapEnvelope env);

		public Pipeline Pipeline {
			get {
				if(_pipeline == null) {
					_pipeline = new Pipeline ();
				}
				return _pipeline;
			}
			set { _pipeline = value; }
		}
        }
}
