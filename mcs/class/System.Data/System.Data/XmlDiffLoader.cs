//
// mcs/class/System.Data/System.Data/XmlDiffLoader.cs
//
// Purpose: Loads XmlDiffGrams to DataSet 
//
// class: XmlDiffLoader
// assembly: System.Data.dll
// namespace: System.Data
//
// Author:
//     Ville Palo <vi64pa@koti.soon.fi>
//
// (c)copyright 2003 Ville Palo
//
using System;
using System.Data;
using System.Xml;
using System.Xml.XPath;
using System.Collections;
using System.Globalization;

namespace System.Data {

        internal class XmlDiffLoader
	{

	        #region Fields

	        private DataSet DSet;
		private Hashtable DiffGrRows = new Hashtable ();
		private Hashtable ErrorRows = new Hashtable ();

		#endregion // Fields

		#region ctors

		public XmlDiffLoader (DataSet DSet)
		{
			this.DSet = DSet;
		}

		#endregion //ctors

		#region Public methods

		public void Load (XmlReader Reader)
		{
			XmlTextReader TextReader = new XmlTextReader (Reader.BaseURI);
			XmlDocument Document = new XmlDocument ();
			Document.Load (TextReader);
			TextReader.Close ();

			XPathNavigator Navigator = Document.CreateNavigator ();
			LoadBefore (Navigator);
			LoadCurrent (Navigator);
			LoadErrors (Navigator);
		}

		#endregion // Public methods

		#region Private methods

		private void LoadCurrent (XPathNavigator Navigator)
		{			
			Navigator.MoveToRoot ();

			if (Navigator.MoveToFirstChild ()) {

				if (Navigator.Name == "diffgr:diffgram") {

					if (Navigator.MoveToFirstChild ()) {

						if (Navigator.MoveToFirstChild ()) {

							if (DSet.Tables.Contains (Navigator.LocalName)) {

								DataTable Table = DSet.Tables [Navigator.LocalName];
								DataRow Row = null;
								bool NewRow = false;
								bool HasErrors = false;
								string id = "";
								
								if (Navigator.MoveToFirstAttribute ()) {
									
									do {
										// Find out was there same row in 'before' section
										if (Navigator.LocalName == "id") {
											id = Navigator.Value;
											if (DiffGrRows.Contains (id))
												Row = (DataRow)DiffGrRows [id];

										}
										else if (Navigator.LocalName == "hasErrors" && String.Compare (Navigator.Value, "true", true) == 0)
										   HasErrors = true;
									} while (Navigator.MoveToNextAttribute ());

									// back to business
									Navigator.MoveToParent ();
								}

								if (Row == null) {
									
									Row = Table.NewRow ();
									NewRow = true;
								}
																
								LoadColumns (Table, Row, Navigator, NewRow);

								if (HasErrors) // If row had errors add row to hashtable for later use
									ErrorRows.Add (id, Row);
							}
						}
					}
				}
			}
		}

		private void LoadBefore (XPathNavigator Navigator)
		{
			Navigator.MoveToRoot ();

			if (!Navigator.MoveToFirstChild ())
				return; // FIXME: exception
			
			if (Navigator.Name != "diffgr:diffgram")
				return; // FIXME: exception

			if (Navigator.MoveToFirstChild ()) {

				while (Navigator.Name != "diffgr:before") {

					if (!Navigator.MoveToNext ()) // there is no before
						return;
				}

				if (Navigator.MoveToFirstChild ()) {

					do {
						if (DSet.Tables.Contains (Navigator.LocalName)) {

							String id = null;
							DataTable Table = DSet.Tables [Navigator.LocalName];
							DataRow Row = Table.NewRow ();
							
							if (Navigator.MoveToFirstAttribute ()) {
								
								do {
									if (Navigator.Name == "diffgr:id")
										id = Navigator.Value;
								       
								} while (Navigator.MoveToNextAttribute ());
								
								Navigator.MoveToParent ();
							}
														
							LoadColumns (Table, Row, Navigator, true);
							DiffGrRows.Add (id, Row); // for later use
							Row.AcceptChanges ();
						} 
						else {
							throw new DataException (Locale.GetText ("Cannot load diffGram. Table '" + Navigator.LocalName + "' is missing in the destination dataset"));
						}
					} while (Navigator.MoveToNext ());
				}
			}
		}				 
				
					   
		private void LoadErrors (XPathNavigator Navigator)
		{
			Navigator.MoveToRoot ();

			if (!Navigator.MoveToFirstChild ())
				return; // FIXME: exception
			
			if (Navigator.Name != "diffgr:diffgram")
				return; // FIXME: exception

			if (Navigator.MoveToFirstChild ()) {
				
				while (Navigator.Name != "diffgr:errors") {
					if (!Navigator.MoveToNext ())
						return;
				}

				if (Navigator.MoveToFirstChild ()) {

					DataRow Row = null;

					// find the row in 'current' section
					if (Navigator.MoveToFirstAttribute ()) {

						do {
							if (Navigator.Name == "diffgr:id") {
								
								if (ErrorRows.Contains (Navigator.Value))
									Row = (DataRow)ErrorRows [Navigator.Value];
							}

						} while (Navigator.MoveToNextAttribute ());
						
						Navigator.MoveToParent ();						
					}

					if (Navigator.MoveToFirstChild ()) {

						string Error = "";
						
						do {
							if (Navigator.MoveToFirstAttribute ()) {
								do {
									if (Navigator.Name == "diffgr:Error")
										Error = Navigator.Value;

								} while (Navigator.MoveToNextAttribute ());
								
								Navigator.MoveToParent ();
							}

							Row.SetColumnError (Navigator.LocalName, Error);

						} while (Navigator.MoveToNext ());
					}
				}
			}
		}

		private void LoadColumns (DataTable Table, DataRow Row, XPathNavigator Navigator, bool NewRow)
		{
			if (Navigator.MoveToFirstChild ()) {

				do {
					if (Table.Columns.Contains (Navigator.LocalName))
						Row [Navigator.LocalName] = Navigator.Value;
										
				} while (Navigator.MoveToNext ());
				
				if (NewRow)
					Table.Rows.Add (Row);
			}
		}


		#endregion // Private methods
	}
}
