//
// System.Configuration.ConfigurationException.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Xml;

namespace System.Configuration
{

	/// <summary>
	///		ConfigurationException class.
	/// </summary>
	/// <remarks>
	///   Longer description
	/// </remarks>
	public class ConfigurationException : SystemException
	{

		private static string _stringBareMessage;
		private static string _stringFilename;
		private static int _intLine;
		private static string _stringMessage;

		/// <summary>
		///		ConfigurationException Constructor.
		/// </summary>
		public ConfigurationException ()
		{
			_stringBareMessage = null;
			_stringFilename = null;
			_intLine = 0;
			_stringMessage = null;
		}

		/// <summary>
		///		ConfigurationException Constructor.
		/// </summary>
		public ConfigurationException (string message)
		{
			_stringBareMessage = message;
			_stringFilename = null;
			_intLine = 0;
			_stringMessage = null;
		}

		/// <summary>
		///		ConfigurationException Constructor.
		/// </summary>
		public ConfigurationException (string message, Exception inner)
		{
			_stringBareMessage = message + " " + inner.ToString();
			_stringFilename = null;
			_intLine = 0;
			_stringMessage = null;
		}

		/// <summary>
		///		ConfigurationException Constructor.
		/// </summary>
		public ConfigurationException (string message, XmlNode node)
		{
			_stringBareMessage = message;
			_stringFilename = GetXmlNodeFilename(node);
			_intLine = GetXmlNodeLineNumber(node);
			_stringMessage = _stringFilename + " " + _intLine;
		}

		/// <summary>
		///		ConfigurationException Constructor.
		/// </summary>
		public ConfigurationException (string message, Exception inner, XmlNode node)
		{
			_stringBareMessage = message + " " + inner.ToString();
			_stringFilename = GetXmlNodeFilename(node);
			_intLine = GetXmlNodeLineNumber(node);
			_stringMessage = _stringFilename + " " + _intLine;
		}

		/// <summary>
		///		ConfigurationException Constructor.
		/// </summary>
		public ConfigurationException (string message, string filename, int line)
		{
			_stringBareMessage = message;
			_stringFilename = filename;
			_intLine = line;
			_stringMessage = _stringFilename + " " + _intLine;
		}

		/// <summary>
		///		ConfigurationException Constructor.
		/// </summary>
		public ConfigurationException (string message, Exception inner, string filename, int line)
		{
			_stringBareMessage = message + " " + inner.ToString();
			_stringFilename = filename;
			_intLine = line;
			_stringMessage = _stringFilename + " " + _intLine;
		}



		/// <summary>
		///		Returns the name of the file containing the configuration section node.
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		public static string GetXmlNodeFilename(XmlNode node)
		{
			_stringFilename = node.OwnerDocument.Name;
			return _stringFilename;
		}

		/// <summary>
		///		Returns the line number containing the configuration section node.
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		public static int GetXmlNodeLineNumber(XmlNode node)
		{

			//FIXME: not sure how this should work.
			return 0;
		}


		/// <summary>
		///		Gets the base error message.
		/// </summary>
		public string BareMessage
		{
			get
			{
				return _stringBareMessage;
			}
		}

		/// <summary>
		///		Gets the name of the configuration file where the error occurred.
		/// </summary>
		public string Filename
		{
			get
			{
				return _stringFilename;
			}
		}

		/// <summary>
		///		Returns the line number where the error occurred.
		/// </summary>
		public int Line
		{
			get
			{
				return _intLine;
			}
		}

		/// <summary>
		///		Gets a string containing the concatenated file name and line number where the error occurred.
		/// </summary>
		public override string Message
		{
			get
			{
				return _stringMessage;
			}
		}
	}
}


