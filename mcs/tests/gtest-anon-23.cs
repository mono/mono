// Cloning tests

using System;
using System.Collections.Generic;

class MemberAccessData
{
	public volatile uint VolatileValue;
	public string [] StringValues;
	public List<string> ListValues;
	
	int? mt;
	public int? MyTypeProperty {
		set	{
			mt = value;
		}
		get {
			return mt;
		}
	}
}

public class B
{
	protected virtual void BaseM ()
	{
	}
}

public class C : B
{
	delegate void D ();
	
	static void Test (D d)
	{
	}
	
	void InstanceTests ()
	{
		Test (() => base.BaseM ());
	}
	
	public static void Main ()
	{
		Exception diffException;
		
		Test (() => {
			diffException = null;
					try {
					} catch (Exception ex) {
						diffException = ex;
					} finally {
					}
					
					try {
					} catch {
					}
				});
				
		int[] i_a = new int [] { 1,2,3 };
		
		Test (() => {
				foreach (int t in i_a) {
				}
			});
			
		Test (() => {
			Console.WriteLine (typeof (void));
		});
		
		Test (() => {
			Console.WriteLine (typeof (Func<,>));
		});
		
		Test (() => {
			object o = new List<object> { "Hello", "", null, "World", 5 };
		});
		
		Test (() => {
			var v = new MemberAccessData { 
				VolatileValue = 2, StringValues = new string [] { "sv" }, MyTypeProperty = null
			};
		});
		
		var c = new C ();
		c.InstanceTests ();
	}
}
