using System;
using System.Collections;

namespace System.Xml
{
	/// <summary>
	/// Abstract class XmlNodeList.
	/// </summary>
	public abstract class XmlNodeList : IEnumerable
	{
		// public properties
		public abstract int Count { get; }

		[System.Runtime.CompilerServices.IndexerName("ItemOf")]
		public virtual XmlNode this[int i] 
		{
			get
			{
				throw new NotImplementedException("XmlNodeList indexer must be implemented by derived class.");
			}
		}

		// Public Methods
		/// <summary>
		/// Abstract.  Return the enumerator for the class.
		/// </summary>
		/// <returns>Enumerator</returns>
		public abstract IEnumerator GetEnumerator();

		/// <summary>
		/// Abstract.  Returns the item at index.  Index is 0-based.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public abstract XmlNode Item(int index);
		
		public XmlNodeList()
		{
			// TODO: What should be done in constructor for XmlNodeList.XmlNodeList()? (nothing)
		}
	}
}
