// -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
//
// System.Xml.WriteState
//
// Author:
//   Daniel Weber (daniel-weber@austin.rr.com)
//
// (C) 2001 Daniel Weber

namespace System.Xml 
{


	/// <summary>
	/// </summary>
	public enum WriteState {

		/// <summary>
		/// A write method has not been called.
		/// </summary>
		Start = 0,

		/// <summary>
		/// The prolog is being written.
		/// </summary>
		Prolog = 1,

		/// <summary>
		/// An element start tag is being written.
		/// </summary>
		Element = 2,

		/// <summary>
		/// An attribute is being written.
		/// </summary>
		Attribute = 3,

		/// <summary>
		/// Element content is being written.
		/// </summary>
		Content = 4,

		/// <summary>
		/// The close method has been called.
		/// </summary>
		Closed = 5,





	} 
}
