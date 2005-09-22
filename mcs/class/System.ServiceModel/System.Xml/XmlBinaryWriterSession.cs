namespace System.Xml
{
	public class XmlBinaryWriterSession
	{
		bool emitStrings;

		public XmlBinaryWriterSession ()
		{
		}

		public XmlBinaryWriterSession (bool emitStrings)
		{
			this.emitStrings = emitStrings;
		}

		public bool EmitStrings {
			get { return emitStrings; }
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
