using System;

namespace Embed {
	class MyType {
		int val = 5;
		string str = "hello";

		MyType () {
			Console.WriteLine ("In ctor val is: {0}", val);
			Console.WriteLine ("In ctor str is: {0}", str);
		}
		
		MyType (int v, byte[] array) {
			Console.WriteLine ("In ctor (int, byte[]) got value: {0}, array len: {1}", v, array.Length);
		}

		void method () {
			Console.WriteLine ("In method val is {0}", val);
			Console.WriteLine ("In method str is: {0}", str);
		}

		int Value {
			get {
				return val;
			}
		}

		string Message {
			get {
				return str;
			}
		}

		void Values (ref int v, ref string s) {
			Console.WriteLine ("In Values () v is {0}", v);
			Console.WriteLine ("In Values () s is: {0}", s);
			v = val;
			s = str;
		}

		static void Fail () {
			throw new Exception ();
		}
		
		static void Main () {
			/* we do nothing here... */
		}
	}
}

