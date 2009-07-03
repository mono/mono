using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;

namespace MonoTests.Common
{
	class AFieldTemplate : ITemplate
	{
		public List <Control> Controls { get; private set; }

		public AFieldTemplate ()
		{
			Controls = new List<Control> ();
		}

		public void InstantiateIn (Control container)
		{
			if (container == null)
				return;

			List <Control> controls = Controls;
			if (controls.Count == 0)
				return;

			foreach (Control c in controls)
				container.Controls.Add (c);
		}
	}
}
