//
// System.Web.IHttpMapPath
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

namespace System.Web
{
	interface IHttpMapPath
	{
		string MapPath (string path);
		string MachineConfigPath { get; }
	}

}

