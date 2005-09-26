#if NET_2_0
namespace System.Xml
{
	public class XmlDictionaryReaderQuotas
	{
		static XmlDictionaryReaderQuotas std, max;

		static XmlDictionaryReaderQuotas ()
		{
			std = new XmlDictionaryReaderQuotas ();
			max = new XmlDictionaryReaderQuotas ();
		}

		readonly bool is_readonly;
		int array_len, bytes, depth, nt_chars, text_len;

		public XmlDictionaryReaderQuotas ()
			: this (false)
		{
		}

		private XmlDictionaryReaderQuotas (bool isReadOnly)
		{
			is_readonly = isReadOnly;
			array_len = DefaultMaxArrayLength;
			bytes = DefaultMaxBytesPerRead;
			depth = DefaultMaxDepth;
			nt_chars = DefaultMaxNameTableCharCount;
			text_len = DefaultMaxStringContentLength;
		}

		public const int DefaultMaxArrayLength = int.MaxValue;

		public const int DefaultMaxBytesPerRead = int.MaxValue;

		public const int DefaultMaxDepth = int.MaxValue;

		public const int DefaultMaxNameTableCharCount = int.MaxValue;

		public const int DefaultMaxStringContentLength = int.MaxValue;

		public static XmlDictionaryReaderQuotas Default {
			get { return std; }
		}

		public static XmlDictionaryReaderQuotas Max {
			get { return max; }
		}

		public int MaxArrayLength {
			get { return array_len; }
			set { array_len = Check (value); }
		}

		public int MaxBytesPerRead {
			get { return bytes; }
			set { bytes = Check (value); }
		}

		public int MaxDepth {
			get { return depth; }
			set { depth = Check (value); }
		}

		public int MaxNameTableCharCount {
			get { return nt_chars; }
			set { nt_chars = Check (value); }
		}

		public int MaxStringContentLength {
			get { return text_len; }
			set { text_len = Check (value); }
		}

		private int Check (int value)
		{
			if (is_readonly)
				throw new InvalidOperationException ("This quota is read-only.");
			if (value <= 0)
				throw new ArgumentException ("Value must be positive integer.");
			return value;
		}

		public void CopyTo (XmlDictionaryReaderQuotas quota)
		{
			quota.array_len = array_len;
			quota.bytes = bytes;
			quota.depth = depth;
			quota.nt_chars = nt_chars;
			quota.text_len = text_len;
		}
	}
}
#endif
