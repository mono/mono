//
// generator.cs: Generator interfaces.
//
// Author: Miguel de Icaza (miguel@gnu.org)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001 Ximian, Inc (http://www.ximian.com)
//

using System.IO;

namespace CIR {

	public interface IGenerator {
		int GenerateFromTree (Tree tree, StreamWriter output);
		void ParseOptions (string options);
	}
}
