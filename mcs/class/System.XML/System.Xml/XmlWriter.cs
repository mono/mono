// -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
//
// System.Xml.XmlWriter
//
// Author:
//   Daniel Weber (daniel-weber@austin.rr.com)
//
// (C) 2001 Daniel Weber

using System;

namespace System.Xml
{
	/// <summary>
	/// Abstract class XmlWriter
	/// </summary>
	public abstract class XmlWriter
	{
		// Private data members
		//===========================================================================

		// public properties
		//===========================================================================
		/// <summary>
		/// Get the state of the writer.
		/// </summary>
		public abstract WriteState WriteState {get;}

		/// <summary>
		/// Get the current xml:lang scope, or null if not defined.
		/// </summary>
		public abstract string XmlLang {get;}

		/// <summary>
		/// get an XmlSpace representing the current xml:space scope
		/// </summary>
		public abstract XmlSpace XmlSpace {get;}

		// Public Methods
		//===========================================================================
		/// <summary>
		/// When overriden, closes this stream and the underlying stream.
		/// </summary>
		/// <exception cref="InvalidOperationException">A call is made to write more output when the stream is closed.</exception>
		public abstract void Close();

		/// <summary>
		/// Flushes whatever is in the buffer to the underlying streams, and flushes any underlying streams.
		/// </summary>
		public abstract void Flush();

		/// <summary>
		/// Returns closest prefix in current namespace, or null if none found.
		/// </summary>
		/// <param name="ns">namespace URI to find a prefix for.</param>
		/// <exception cref="ArgumentException">ns is null, or string.Empty</exception>
		/// <returns></returns>
		public abstract string LookupPrefix(string ns);

		/// <summary>
		/// Write out all the attributes found at the current position in the XmlReader
		/// </summary>
		/// <param name="reader">XmlReader to read from</param>
		/// <param name="defattr">true to copy default attributes</param>
		/// <exception cref="ArgumentException">Reader is a null reference</exception>
		public virtual void WriteAttributes(
			XmlReader reader,
			bool defattr
			)
		{
			//TODO - implement XmlWriter.WriteAttributes(XmlReader, bool)
			throw new NotImplementedException();
		}


	}
}
