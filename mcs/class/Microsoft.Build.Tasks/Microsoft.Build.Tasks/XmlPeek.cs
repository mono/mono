// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//-----------------------------------------------------------------------
// </copyright>
// <summary>Returns the value specified by XPath.</summary>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Build.Tasks
{
	/// <summary>
	/// A task that returns values as specified by XPath Query
	/// from an XML file.
	/// </summary>
	public class XmlPeek : TaskExtension
	{
		#region Members

		/// <summary>
		/// The XML input as a file path.
		/// </summary>
		private ITaskItem _xmlInputPath;

		/// <summary>
		/// The XML input as a string.
		/// </summary>
		private string _xmlContent;

		/// <summary>
		/// The XPath Query.
		/// </summary>
		private string _query;

		/// <summary>
		/// The results that this task will return.
		/// </summary>
		private ITaskItem[] _result;

		/// <summary>
		/// The namespaces for XPath query's prefixes.
		/// </summary>
		private string _namespaces;

		#endregion

		#region Properties

		/// <summary>
		/// The XML input as a file path.
		/// </summary>
		public ITaskItem XmlInputPath {
			get {
				return _xmlInputPath;
			}

			set {
				_xmlInputPath = value;
			}
		}

		/// <summary>
		/// The XML input as a string.
		/// </summary>
		public string XmlContent {
			get {
				return _xmlContent;
			}

			set {
				_xmlContent = value;
			}
		}

		/// <summary>
		/// The XPath Query.
		/// </summary>
		public string Query {
			get {
				if (_query == null)
					throw new ArgumentNullException ("Query");
				return _query;
			}

			set {
				_query = value;
			}
		}

		/// <summary>
		/// The results returned by this task.
		/// </summary>
		[Output]
		public ITaskItem[] Result {
			get {
				return _result;
			}
		}

		/// <summary>
		/// The namespaces for XPath query's prefixes.
		/// </summary>
		public string Namespaces {
			get {
				return _namespaces;
			}

			set {
				_namespaces = value;
			}
		}

		#endregion


		internal static bool IsCriticalException (Exception e)
		{
			if (e is StackOverflowException
			    || e is OutOfMemoryException
			    || e is AccessViolationException) {
				return true;
			}
			return false;
		}

		/// <summary>
		/// Executes the XMLPeek task.
		/// </summary>
		/// <returns>true if transformation succeeds.</returns>
		public override bool Execute ()
		{
			XmlInput xmlinput;
			if (_query == null)
				throw new ArgumentNullException ("Query");

			try {
				xmlinput = new XmlInput (_xmlInputPath, _xmlContent);
			} catch (Exception e) {
				if (IsCriticalException (e)) {
					throw;
				}

				Log.LogError ("MSB3741: Unable to load arguments for the XmlPeek task. {0}", e.Message);
				return false;
			}

			XPathDocument xpathdoc;
			try {
				// Load the XPath Document
				using (XmlReader xr = xmlinput.CreateReader ()) {
					xpathdoc = new XPathDocument (xr);
					xr.Close ();
				}
			} catch (Exception e) {
				if (IsCriticalException (e)) {
					throw;
				}

				Log.LogError ("MSB3733: Input file \"{0}\" cannot be opened. {1}", _xmlInputPath.ItemSpec, e.Message);
				return false;
			} finally {
				xmlinput.CloseReader ();
			}

			XPathNavigator nav = xpathdoc.CreateNavigator ();
			XPathExpression expr = null;
			try {
				// Create the expression from query
				expr = nav.Compile (_query);
			} catch (Exception e) {
				if (IsCriticalException (e)) {
					throw;
				}

				Log.LogError ("MSB3734: XPath Query \"{0}\" cannot be loaded. {1}", _query, e.Message);
				return false;
			}

			// Create the namespace manager and parse the input.
			XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager (nav.NameTable);

			try {
				LoadNamespaces (ref xmlNamespaceManager, _namespaces);
			} catch (Exception e) {
				if (IsCriticalException (e)) {
					throw;
				}

				Log.LogError ("MSB3742: Unable to process the Namespaces argument for the XmlPeek task. {0}", e.Message);
				return false;
			}

			try {
				expr.SetContext (xmlNamespaceManager);
			} catch (XPathException e) {
				Log.LogError ("MSB3743: Unable to set XPath expression's Context. {0}", e.Message);
				return false;
			}

			XPathNodeIterator iter = nav.Select (expr);

			List<string> peekValues = new List<string> ();
			while (iter.MoveNext ()) {
				if (iter.Current.NodeType == XPathNodeType.Attribute
				                || iter.Current.NodeType == XPathNodeType.Text) {
					peekValues.Add (iter.Current.Value);
				} else {
					peekValues.Add (iter.Current.OuterXml);
				}
			}

			_result = new ITaskItem[peekValues.Count];
			int i = 0;
			foreach (string item in peekValues) {
				_result [i++] = new TaskItem (item);

				// This can be logged a lot, so low importance
				Log.LogMessage (MessageImportance.Low, "Found \"{0}\".", item);
			}

			if (_result.Length == 0) {
				// Logged no more than once per execute of this task
				Log.LogMessage ("The specified XPath query did not capture any nodes.");
			}

			return true;
		}

		/// <summary>
		/// Loads the namespaces specified at Namespaces parameter to XmlNSManager.
		/// </summary>
		/// <param name="namespaceManager">The namespace manager to load namespaces to.</param>
		/// <param name="namepaces">The namespaces as XML snippet.</param>
		private void LoadNamespaces (ref XmlNamespaceManager namespaceManager, string namepaces)
		{
			XmlDocument doc = new XmlDocument ();
			try {
				XmlReaderSettings settings = new XmlReaderSettings ();
				settings.DtdProcessing = DtdProcessing.Ignore;

				using (XmlReader reader = XmlReader.Create (new StringReader ("<Namespaces>" + namepaces + "</Namespaces>"), settings)) {
					doc.Load (reader);
				}
			} catch (XmlException xe) {
				throw new ArgumentException ("The specified Namespaces attribute is not a well-formed XML fragment.", xe);
			}

			XmlNodeList xnl = doc.SelectNodes ("/Namespaces/*[local-name() = 'Namespace']");
			for (int i = 0; i < xnl.Count; i++) {
				XmlNode xn = xnl [i];

				if (xn.Attributes ["Prefix"] == null) {
					throw new ArgumentException (string.Format ("The specified Namespaces attribute doesn't have attribute \"{0}\".", "Name"));
				}

				if (xn.Attributes ["Uri"] == null) {
					throw new ArgumentException (string.Format ("The specified Namespaces attribute doesn't have attribute \"{0}\".", "Uri"));
				}

				namespaceManager.AddNamespace (xn.Attributes ["Prefix"].Value, xn.Attributes ["Uri"].Value);
			}
		}

		/// <summary>
		/// This class prepares XML input from XMLInputPath and XMLContent parameters
		/// </summary>
		internal class XmlInput
		{
			/// <summary>
			/// What XML input type are we at.
			/// </summary>
			private XmlModes _xmlMode;

			/// <summary>
			/// This either contains the raw Xml or the path to Xml file.
			/// </summary>
			private string _data;

			/// <summary>
			/// Filestream used to read XML.
			/// </summary>
			private FileStream _fs;

			/// <summary>
			/// Constructor.
			/// Only one parameter should be non null or will throw ArgumentException.
			/// </summary>
			/// <param name="xmlInputPath">The path to XML file or null.</param>
			/// <param name="xmlContent">The raw XML.</param>
			public XmlInput (ITaskItem xmlInputPath, string xmlContent)
			{
				if (xmlInputPath != null && xmlContent != null) {
					throw new ArgumentException ("Only one of XmlContent or XmlInputPaths arguments can be set.");
				} else if (xmlInputPath == null && xmlContent == null) {
					throw new ArgumentException ("One of XmlContent or XmlInputPaths arguments must be set.");
				}

				if (xmlInputPath != null) {
					_xmlMode = XmlModes.XmlFile;
					_data = xmlInputPath.ItemSpec;
				} else {
					_xmlMode = XmlModes.Xml;
					_data = xmlContent;
				}
			}

			/// <summary>
			/// Possible accepted types of XML input.
			/// </summary>
			public enum XmlModes
			{
				/// <summary>
				/// If the mode is a XML file.
				/// </summary>
				XmlFile,

				/// <summary>
				/// If the mode is a raw XML.
				/// </summary>
				Xml
			}

			/// <summary>
			/// Returns the current mode of the XmlInput
			/// </summary>
			public XmlModes XmlMode {
				get {
					return _xmlMode;
				}
			}

			/// <summary>
			/// Creates correct reader based on the input type.
			/// </summary>
			/// <returns>The XmlReader object</returns>
			public XmlReader CreateReader ()
			{
				if (_xmlMode == XmlModes.XmlFile) {
					_fs = new FileStream (_data, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
					return XmlReader.Create (_fs);
				} else { // xmlModes.Xml 
					return XmlReader.Create (new StringReader (_data));
				}
			}

			/// <summary>
			/// Closes the reader.
			/// </summary>
			public void CloseReader ()
			{
				if (_fs != null) {
					_fs.Close ();
					_fs = null;
				}
			}
		}
	}
}
