//
// System.Configuration.IConfigurationSystem
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

namespace System.Configuration
{
	public interface IConfigurationSystem
	{
		object GetConfig (string configKey);
		void Init ();
	}
}

