using System;
using System.Collections.Generic;
using System.Linq;

namespace from
{
	interface ITest
	{
		int Id { get; }
	}

	class C
	{
		// This is pure grammar/parser test
		public static void Main ()
		{
			int[] i2 = new int [] { 0, 1 };
			int[] i_b = new int [] { 0, 1 };
			ITest[] join_test = new ITest[0];
			
			IEnumerable<int> e;
			IEnumerable<IGrouping<int,int>> g;

			// FROM
			e = from i in i2 select i;
			e = from int i in i2 select i;
			var e2 = from bool? i in i2 select i;
			e = from i in new int[0] select i;
			e = from x in i2 from y in i2 select y;

			// WHERE
			e = from i in i2 where i % 3 != 0 select i;

			// ORDER BY
			e = from i in i2 orderby i select i;
			e = from i in i2 orderby i, i, i, i select i;
			e = from i in i2 orderby i descending select i;
			e = from i in i2 orderby i ascending select i;
			
			// JOIN
			e = from i in i2 join j in join_test on i equals j.Id select i;
			e = from i in i2 join ITest j in join_test on i equals j.Id select i;
			e = from i in i2 join j in join_test on i equals j.Id
				join j2 in join_test on i equals j2.Id select i;
			e = from i in i2 join j in i_b on i equals j into j select i;
			e = from i in i2 join int j in i_b on i equals j into j select i;

			// GROUP BY
			g = from i in i2 group i by 2;
			g = from i in i2 group i by 2 into i select i;

			// LET
			e = from i in i2 let l = i + 4 select i;
			e = from i in i2 let l = i + 4 let l2 = l + 2 select i;
			
			// INTO
			g = from i in i2 group i by 2 into i9
				from i in i2 group i by 2 into i9
				from i in i2 group i by 2 into i9
				from i in i2 group i by 2 into i9
				select i9;
				
			// NESTED
			var e3 = from a in
					from b in i2
					select b
				orderby a
				select a;
				
			var e4 = from i in
					from x in i2
					select x
				let l = from x2 in i2 select x2
				let l2 = 50
				select 1;

			int from = 0;
			bool let = false;
			
			// TODO: Re-enable when we fix cast of query expression
			//object o = (object)from i in i2 select i;
		}
		
		void Foo (int from, bool where)
		{
			int Foo = 0;
			if (from < Foo) // This tests generics micro-parser awareness
				return;
		}
		
		int foo;
		void Do (string[] args)
		{
			int from = this.foo;
			Console.WriteLine (args [from]);
		}		
	}
	
	class D
	{
		public bool check (object from, object to)
		{
			if (from is int && to is int) return true;
				return false;
		}
	}
}

namespace FromProblems2
{
	class from
	{
	}
	
	class C
	{
		void M1 ()
		{
			from local = new from ();
		}
		
		from M2 ()
		{
			return null;
		}
	}
}

