//
// System.Data.PropertyCollection.cs
//
// Author:
//    Daniel Morgan <danmorg@sc.rr.com>
//
// (c) Ximian, Inc. 2002
//

using System;
using System.Collections;
using System.ComponentModel;

namespace System.Data
{
	/// <summary>
	/// a collection of properties that can be added to 
	/// DataColumn, DataSet, or DataTable.
	/// The ExtendedProperties property of a 
	/// DataColumn, DataSet, or DataTable class can
	/// retrieve a PropertyCollection.
	/// </summary>
	public class PropertyCollection : Hashtable {
		public PropertyCollection() {
		}

		// the only public methods and properties 
		// are all inherited from Hashtable
	}
}
