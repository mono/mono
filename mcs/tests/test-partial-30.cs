// Partial parser tests, contextual sensitivity

namespace PartialProblems
{
	class Classes
	{
		class partial
		{
		}
		
		void M1 (partial formalParameter)
		{
		}

		partial M3 ()
		{
			return null;
		}

		partial field;
		
		public static void Main ()
		{
		}
	}
}
