//
// System.Threading.Interlocked.cs
//
// Author:
//   Dick Porter (dick@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//


namespace System.Threading
{
	public sealed class Interlocked 
	{
		private Interlocked () {}

		public static int CompareExchange(ref int location1, int value, int comparand) {
			// lock
			if(comparand==location1) {
				int ret;
				
				ret=location1;
				location1=value;
				return(ret);
			}
			return(location1);
		}

		public static object CompareExchange(ref object location1, object value, object comparand) {
			// lock
			if(comparand==location1) {
				object ret;
				
				ret=location1;
				location1=value;
				return(ret);
			}
			return(location1);
		}

		public static float CompareExchange(ref float location1, float value, float comparand) {
			// lock
			if(comparand==location1) {
				float ret;
				
				ret=location1;
				location1=value;
				return(ret);
			}
			return(location1);
		}

		public static int Decrement(ref int location) {
			// lock
			location--;
			return(location);
		}

		public static long Decrement(ref long location) {
			// lock
			location--;
			return(location);
		}

		public static int Exchange(ref int location1, int value) {
			// lock
			int ret;
			
			ret=location1;
			location1=value;
			return(ret);
		}

		public static object Exchange(ref object location1, object value) {
			// lock
			object ret;
			
			ret=location1;
			location1=value;
			return(ret);
		}

		public static float Exchange(ref float location1, float value) {
			// lock
			float ret;
			
			ret=location1;
			location1=value;
			return(ret);
		}

		public static int Increment(ref int location) {
			// lock
			location++;
			return(location);
		}

		public static long Increment(ref long location) {
			// lock
			location++;
			return(location);
		}
	}
}

