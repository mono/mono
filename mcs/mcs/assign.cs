//
// assign.cs: Assignment representation for the IL tree.
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace CIR {
	public class Assign : Expression {
		Expression target, source;
		
		public Assign (Expression target, Expression source)
		{
			this.target = target;
			this.source = source;
		}

		public Expression Target {
			get {
				return target;
			}

			set {
				target = value;
			}
		}

		public Expression Source {
			get {
				return source;
			}

			set {
				source = value;
			}
		}
	}
}
