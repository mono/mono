//
// System.Runtime.Remoting.Contexts.IContextProperty..cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//


namespace System.Runtime.Remoting.Contexts {

	public interface IContextProperty {

		string Name {
			get;
		}

		void Freeze (Context ctx);

		bool IsNewContextOK (Context ctx);
	}
}
