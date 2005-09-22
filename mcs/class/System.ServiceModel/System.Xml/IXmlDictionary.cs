#if NET_2_0
namespace System.Xml
{
	public interface IXmlDictionary
	{
		bool TryLookup (int key, out XmlDictionaryString result);
		bool TryLookup (string value, out XmlDictionaryString result);
		bool TryLookup (XmlDictionaryString value,
			out XmlDictionaryString result);
	}
}
#endif
