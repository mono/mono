//
// System.Web.UI.Design.DesignTimeData
//
// Authors:
//      Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2004 Novell
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Collections;
using System.ComponentModel;
using System.Data;

namespace System.Web.UI.Design
{
	public sealed class DesignTimeData
	{
		private DesignTimeData ()
		{
		}

		[MonoTODO]
		public static DataTable CreateDummyDataBoundDataTable ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static DataTable CreateDummyDataTable ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static DataTable CreateSampleDataTable (IEnumerable referenceData)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static DataTable CreateSampleDataTable (IEnumerable referenceData, bool useDataBoundData)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static PropertyDescriptorCollection GetDataFields (IEnumerable dataSource)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static IEnumerable GetDataMember (IListSource dataSource, string dataMember)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static string[] GetDataMembers (object dataSource)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static IEnumerable GetDesignTimeDataSource (DataTable dataTable, int minimumRows)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static object GetSelectedDataSource (IComponent component, string dataSource)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static IEnumerable GetSelectedDataSource (IComponent component, string dataSource, string dataMember)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		private static void OnDataBind (object sender, EventArgs e)
		{
			throw new NotImplementedException ();
		}


		public static readonly EventHandler DataBindingHandler =
			new EventHandler (DesignTimeData.OnDataBind);
	}
}
