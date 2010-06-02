// CS0205: Cannot call an abstract base member `A.Foobar.set'
// Line: 13

public abstract class A
{
        public abstract int Foobar { set; }
}

public class B: A
{
		public override int Foobar  {
			set {
				base.Foobar = value;
			}
		}
}

