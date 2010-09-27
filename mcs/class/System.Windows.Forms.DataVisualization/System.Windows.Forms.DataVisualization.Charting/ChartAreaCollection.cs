using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Windows.Forms.DataVisualization.Charting
{
	public class ChartAreaCollection : ChartNamedElementCollection<ChartArea>
	{
		#region Public Methods
		public ChartArea Add (string name)
		{
			ChartArea ca = new ChartArea ();
			ca.Name = name;

			Add (ca);
			return ca;
		}
		#endregion
	}
}
