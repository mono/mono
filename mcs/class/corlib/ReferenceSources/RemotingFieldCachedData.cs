using System.Reflection;
using System.Runtime.Serialization;

namespace System.Runtime.Remoting.Metadata
{
	class RemotingCachedData
	{

	}

	class RemotingFieldCachedData
	{
		internal RemotingFieldCachedData(RuntimeFieldInfo ri)
		{
		}

		internal RemotingFieldCachedData(SerializationFieldInfo ri)
		{
		}		
	}
}