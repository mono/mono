using System;

namespace System.Xml
{
	/// <summary>
	/// Enumeration of node changed actions.
	/// </summary>
	public enum XmlNodeChangedAction
	{

		/// <summary>
		/// A node is being inserted in the tree
		/// </summary>
		Insert = 0,

        /// <summary>
        /// A node is being removed from the tree.
        /// </summary>
		Remove = 1,

		/// <summary>
		/// A node is being changed.
		/// </summary>
        Change  = 2,

	}
}
