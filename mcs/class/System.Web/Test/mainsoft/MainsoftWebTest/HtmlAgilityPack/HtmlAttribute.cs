// HtmlAgilityPack V1.0 - Simon Mourier <simonm@microsoft.com>
using System;
using System.Collections;

namespace HtmlAgilityPack
{
	/// <summary>
	/// Represents an HTML attribute.
	/// </summary>
	public class HtmlAttribute: IComparable
	{
		internal int _line = 0;
		internal int _lineposition = 0;
		internal int _streamposition = 0;
		internal int _namestartindex = 0;
		internal int _namelength = 0;
		internal int _valuestartindex = 0;
		internal int _valuelength = 0;
		internal HtmlDocument _ownerdocument; // attribute can exists without a node
		internal HtmlNode _ownernode;
		internal string _name;
		internal string _value;

		internal HtmlAttribute(HtmlDocument ownerdocument)
		{
			_ownerdocument = ownerdocument;
		}

		/// <summary>
		/// Creates a duplicate of this attribute.
		/// </summary>
		/// <returns>The cloned attribute.</returns>
		public HtmlAttribute Clone()
		{
			HtmlAttribute att = new HtmlAttribute(_ownerdocument);
			att.Name = Name;
			att.Value = Value;
			return att;
		}

		/// <summary>
		/// Compares the current instance with another attribute. Comparison is based on attributes' name.
		/// </summary>
		/// <param name="obj">An attribute to compare with this instance.</param>
		/// <returns>A 32-bit signed integer that indicates the relative order of the names comparison.</returns>
		public int CompareTo(object obj)
		{
			HtmlAttribute att = obj as HtmlAttribute;
			if (att == null)
			{
				throw new ArgumentException("obj");
			}
			return Name.CompareTo(att.Name);
		}

		internal string XmlName
		{
			get
			{
				return HtmlDocument.GetXmlName(Name);
			}
		}

		internal string XmlValue
		{
			get
			{
				return Value;
			}
		}

		/// <summary>
		/// Gets the qualified name of the attribute.
		/// </summary>
		public string Name
		{
			get
			{
				if (_name == null)
				{
					_name = _ownerdocument._text.Substring(_namestartindex, _namelength).ToLower();
				}
				return _name;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}
				_name = value.ToLower();
				if (_ownernode != null)
				{
					_ownernode._innerchanged = true;
					_ownernode._outerchanged = true;
				}
			}
		}

		/// <summary>
		/// Gets or sets the value of the attribute.
		/// </summary>
		public string Value
		{
			get
			{
				if (_value == null)
				{
					_value = _ownerdocument._text.Substring(_valuestartindex, _valuelength);
				}
				return _value;
			}
			set
			{
				_value = value;
				if (_ownernode != null)
				{
					_ownernode._innerchanged = true;
					_ownernode._outerchanged = true;
				}
			}
		}

		/// <summary>
		/// Gets the line number of this attribute in the document.
		/// </summary>
		public int Line
		{
			get
			{
				return _line;
			}
		}

		/// <summary>
		/// Gets the column number of this attribute in the document.
		/// </summary>
		public int LinePosition
		{
			get
			{
				return _lineposition;
			}
		}

		/// <summary>
		/// Gets the stream position of this attribute in the document, relative to the start of the document.
		/// </summary>
		public int StreamPosition
		{
			get
			{
				return _streamposition;
			}
		}

		/// <summary>
		/// Gets the HTML node to which this attribute belongs.
		/// </summary>
		public HtmlNode OwnerNode
		{
			get
			{
				return _ownernode;
			}
		}

		/// <summary>
		/// Gets the HTML document to which this attribute belongs.
		/// </summary>
		public HtmlDocument OwnerDocument
		{
			get
			{
				return _ownerdocument;
			}
		}

	}

	/// <summary>
	/// Represents a combined list and collection of HTML nodes.
	/// </summary>
	public class HtmlAttributeCollection: IEnumerable
	{
		internal Hashtable _hashitems = new Hashtable();
		private ArrayList _items = new ArrayList();
		private HtmlNode _ownernode;

		internal HtmlAttributeCollection(HtmlNode ownernode)
		{
			_ownernode = ownernode;
		}

		/// <summary>
		/// Inserts the specified attribute as the last attribute in the collection.
		/// </summary>
		/// <param name="newAttribute">The attribute to insert. May not be null.</param>
		/// <returns>The appended attribute.</returns>
		public HtmlAttribute Append(HtmlAttribute newAttribute)
		{
			if (newAttribute == null)
			{
				throw new ArgumentNullException("newAttribute");
			}

			_hashitems[newAttribute.Name] = newAttribute;
			newAttribute._ownernode = _ownernode;
			_items.Add(newAttribute);

			_ownernode._innerchanged = true;
			_ownernode._outerchanged = true;
			return newAttribute;
		}

		/// <summary>
		/// Creates and inserts a new attribute as the last attribute in the collection.
		/// </summary>
		/// <param name="name">The name of the attribute to insert.</param>
		/// <returns>The appended attribute.</returns>
		public HtmlAttribute Append(string name)
		{
			HtmlAttribute att = _ownernode._ownerdocument.CreateAttribute(name);
			return Append(att);
		}

		/// <summary>
		/// Creates and inserts a new attribute as the last attribute in the collection.
		/// </summary>
		/// <param name="name">The name of the attribute to insert.</param>
		/// <param name="value">The value of the attribute to insert.</param>
		/// <returns>The appended attribute.</returns>
		public HtmlAttribute Append(string name, string value)
		{
			HtmlAttribute att = _ownernode._ownerdocument.CreateAttribute(name, value);
			return Append(att);
		}

		/// <summary>
		/// Inserts the specified attribute as the first node in the collection.
		/// </summary>
		/// <param name="newAttribute">The attribute to insert. May not be null.</param>
		/// <returns>The prepended attribute.</returns>
		public HtmlAttribute Prepend(HtmlAttribute newAttribute)
		{
			if (newAttribute == null)
			{
				throw new ArgumentNullException("newAttribute");
			}

			_hashitems[newAttribute.Name] = newAttribute;
			newAttribute._ownernode = _ownernode;
			_items.Insert(0, newAttribute);

			_ownernode._innerchanged = true;
			_ownernode._outerchanged = true;
			return newAttribute;
		}

		/// <summary>
		/// Removes the attribute at the specified index.
		/// </summary>
		/// <param name="index">The index of the attribute to remove.</param>
		public void RemoveAt(int index)
		{
			HtmlAttribute att = (HtmlAttribute)_items[index];
			_hashitems.Remove(att.Name);
			_items.RemoveAt(index);

			_ownernode._innerchanged = true;
			_ownernode._outerchanged = true;
		}

		/// <summary>
		/// Removes a given attribute from the list.
		/// </summary>
		/// <param name="attribute">The attribute to remove. May not be null.</param>
		public void Remove(HtmlAttribute attribute)
		{
			if (attribute == null)
			{
				throw new ArgumentNullException("attribute");
			}
			int index = GetAttributeIndex(attribute);
			if (index == -1)
			{
				throw new IndexOutOfRangeException();
			}
			RemoveAt(index);
		}

		/// <summary>
		/// Removes an attribute from the list, using its name. If there are more than one attributes with this name, they will all be removed.
		/// </summary>
		/// <param name="name">The attribute's name. May not be null.</param>
		public void Remove(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}

			string lname = name.ToLower();
			for(int i=0;i<_items.Count;i++)
			{
				HtmlAttribute att = (HtmlAttribute)_items[i];
				if (att.Name == lname)
				{
					RemoveAt(i);
				}
			}
		}

		/// <summary>
		/// Remove all attributes in the list.
		/// </summary>
		public void RemoveAll()
		{
			_hashitems.Clear();
			_items.Clear();

			_ownernode._innerchanged = true;
			_ownernode._outerchanged = true;
		}

		/// <summary>
		/// Gets the number of elements actually contained in the list.
		/// </summary>
		public int Count
		{
			get
			{
				return _items.Count;
			}
		}

		internal int GetAttributeIndex(HtmlAttribute attribute)
		{
			if (attribute == null)
			{
				throw new ArgumentNullException("attribute");
			}
			for(int i=0;i<_items.Count;i++)
			{
				if (((HtmlAttribute)_items[i])==attribute)
					return i;
			}
			return -1;
		}

		internal int GetAttributeIndex(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			string lname = name.ToLower();
			for(int i=0;i<_items.Count;i++)
			{
				if (((HtmlAttribute)_items[i]).Name==lname)
					return i;
			}
			return -1;
		}

		/// <summary>
		/// Gets a given attribute from the list using its name.
		/// </summary>
		public HtmlAttribute this[string name]
		{
			get
			{
				if (name == null)
				{
					throw new ArgumentNullException("name");
				}
				return _hashitems[name.ToLower()] as HtmlAttribute;
			}
		}

		/// <summary>
		/// Gets the attribute at the specified index.
		/// </summary>
		public HtmlAttribute this[int index]
		{
			get
			{
				return _items[index] as HtmlAttribute;
			}
		}

		internal void Clear()
		{
			_hashitems.Clear();
			_items.Clear();
		}

		/// <summary>
		/// Returns an enumerator that can iterate through the list.
		/// </summary>
		/// <returns>An IEnumerator for the entire list.</returns>
		public HtmlAttributeEnumerator GetEnumerator() 
		{
			return new HtmlAttributeEnumerator(_items);
		}

		IEnumerator IEnumerable.GetEnumerator() 
		{
			return GetEnumerator();
		}

		/// <summary>
		/// Represents an enumerator that can iterate through the list.
		/// </summary>
		public class HtmlAttributeEnumerator: IEnumerator 
		{
			int _index;
			ArrayList _items;

			internal HtmlAttributeEnumerator(ArrayList items) 
			{
				_items = items;
				_index = -1;
			}

			/// <summary>
			/// Sets the enumerator to its initial position, which is before the first element in the collection.
			/// </summary>
			public void Reset() 
			{
				_index = -1;
			}

			/// <summary>
			/// Advances the enumerator to the next element of the collection.
			/// </summary>
			/// <returns>true if the enumerator was successfully advanced to the next element, false if the enumerator has passed the end of the collection.</returns>
			public bool MoveNext() 
			{
				_index++;
				return (_index<_items.Count);
			}

			/// <summary>
			/// Gets the current element in the collection.
			/// </summary>
			public HtmlAttribute Current 
			{
				get 
				{
					return (HtmlAttribute)(_items[_index]);
				}
			}

			/// <summary>
			/// Gets the current element in the collection.
			/// </summary>
			object IEnumerator.Current 
			{
				get 
				{
					return (Current);
				}
			}
		}
	}

}
