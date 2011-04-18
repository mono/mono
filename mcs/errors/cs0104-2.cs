// CS0104: `Graphics' is an ambiguous reference between `Gdk.Graphics' and `System.Drawing.Graphics'
// Line: 16

using Gdk;
using System.Drawing;

public class Plot {
	void M ()
	{
		Graphics g;
	}
	
	
	static void Main ()
	{
	}
}


namespace Gdk {
	public class Graphics {
	}
}

namespace System.Drawing {
	public class Graphics {
	}
}
