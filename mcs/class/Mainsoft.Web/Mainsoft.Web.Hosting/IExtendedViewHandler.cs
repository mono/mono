using System;
using System.Collections.Generic;
using System.Text;
using javax.faces.context;

namespace Mainsoft.Web.Hosting
{
	public interface IExtendedViewHandler
	{
		string EncodeNamespace (FacesContext facesContext, string value);
	}
}
