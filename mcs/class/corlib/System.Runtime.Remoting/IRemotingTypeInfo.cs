//
// System.Runtime.Remoting.IRemotingTypeInfo.cs
//
// AUthor: Duncan Mak  (duncan@ximian.com)
//
// 2002 (C) Copyright. Ximian, Inc.
//

using System.Reflection;
using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting {

	public interface IRemotingTypeInfo
	{
		string TypeName { get; set; }
		bool CanCastTo (Type fromType, object o);
	}

	// fixme: dont know if we really need this
	internal class RemotingTypeInfo : IRemotingTypeInfo
	{

		string type_name;

		public RemotingTypeInfo (Type type)
		{
			type_name = type.AssemblyQualifiedName;
		}
		
		public string TypeName {

			get {
				return type_name;
			}

			set {
				type_name = value;
			}
		}

		public Type GetRealType ()
		{
			string type_name = null;
			Assembly assembly = null;
			
			int pos = type_name.IndexOf (",");
			if (pos >= 0) {
				if (pos != 0) {
					string ass_name = type_name.Substring (0, pos - 1);
					assembly = Assembly.Load (ass_name);
				} 
				type_name = type_name.Substring (pos + 1);
			}
			return assembly.GetType (type_name);
		}
		
		public bool CanCastTo (Type fromType, object o)
		{
			return false;
		}
	}
}

