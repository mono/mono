#if NET_2_0
namespace System.Runtime.Serialization
{
	public interface IUnknownSerializationData
	{
		UnknownSerializationData UnknownData { get; set; }
	}
}
#endif
