// -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
//
// System.Xml.XmlException
//
// Author:
//   Daniel Weber (daniel-weber@austin.rr.com)
//
// (C) 2001 Daniel Weber

using System;
using System.Runtime.Serialization;

namespace System.Xml
{
	/// <summary>
	/// Abstract class XmlNodeList.
	/// </summary>
	public class XmlException : SystemException
	{
		// Private data members
		int FlineNumber;
		int FlinePosition;
		string Fmessage;

		// public properties
		/// <summary>
		/// Get the line number where the exception occured
		/// </summary>
		public int LineNumber 
		{
			get
			{
				return FlineNumber;
			}
		}

		/// <summary>
		/// Get the line position where the exception occured.
		/// </summary>
		public int LinePosition 
		{
			get
			{
				return FlinePosition;
			}
		}

		/// <summary>
		/// Get the error message describing the exception.
		/// </summary>
		public override string Message 
		{
			get
			{
				return Fmessage;
			}
		}

		// Public Methods

		// Constructors
		/// <summary>
		/// Create a new XmlException object.
		/// </summary>
		/// <param name="info">The serializatin object holding all exception information.</param>
		/// <param name="context">The streaming context containing the context of the error</param>
		public XmlException(
			SerializationInfo info,
			StreamingContext context
			)
		{
			FlineNumber = info.GetInt32("lineNumber");
			FlinePosition = info.GetInt32("linePosition");
			Fmessage = info.GetString("message");

		}

		/// <summary>
		/// Create a new XmlException
		/// </summary>
		/// <param name="message">Description of error</param>
		/// <param name="innerException">Exception causing error.  Value can be null.</param>
		public XmlException(
			string message,
			Exception innerException
			)
		{
			Fmessage = message;
		}
	}
}
