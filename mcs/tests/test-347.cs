namespace Whatever.Core {
	public class Project {
	}

	public class A {
		public Project Project {
			get { return new Project(); }
		}
	}
}

namespace SomethingElse.Core {
	public class Project {
	}
}

namespace Whatever.App {
	using Whatever.Core;
	using SomethingElse.Core;

	public class B : A {
		public string Execute() {
			return Project.ToString();
		}

		public static void Main ()
		{
			new B().Execute ();
		}
	}
}

