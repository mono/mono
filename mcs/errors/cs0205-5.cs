// CS0205: Cannot call an abstract base member `A.this[int].set'
// Line: 13

public abstract class A
{
        public abstract int this[int i] { set; }
}

public class B: A
{
		public override int this[int i]  {
			set {
				base[i] = value;
			}
		}
}
