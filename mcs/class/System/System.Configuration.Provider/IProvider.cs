//
// System.Configuration.Provider.IProvider
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
//

#if NET_1_2
using System.Collections;
using System.Collections.Specialized;

namespace System.Configuration.Provider {
	public interface IProvider {
		void Initialize (string name, NameValueCollection config);
		string Name { get; }
	}
}
#endif
