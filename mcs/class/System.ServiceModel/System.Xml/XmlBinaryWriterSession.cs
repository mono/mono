#if NET_2_0
namespace System.Xml
{
	public class XmlBinaryWriterSession
	{
		bool emit_strings;

		public XmlBinaryWriterSession ()
		{
		}

		public XmlBinaryWriterSession (bool emitStrings)
		{
			emit_strings = emitStrings;
		}

		public bool EmitStrings {
			get { return emit_strings; }
		}

		[MonoTODO]
		public void Reset ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool TryAdd (XmlDictionaryString value,
			out int key)
		{
			throw new NotImplementedException ();
		}
	}
}
#endif
