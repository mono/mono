// Compiler options: -unsafe

// Cloning tests

using System;

unsafe class UnsafeClass
{
	public int* GetUnsafeValue ()
	{
		return null;
	}
}

public class C
{
	delegate void D ();
	
	static void Test (D d)
	{
	}
	
	unsafe static void UnsafeTests ()
	{
		UnsafeClass v = new UnsafeClass ();
		Test (delegate () {
			int i = *v.GetUnsafeValue ();
		});
	}
	
	
	public static void Main ()
	{
		Exception diffException;
		
		Test (delegate () {
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
		
		Test (delegate () {
				foreach (int t in i_a) {
				}
			});
			
		Test (delegate () {
			Console.WriteLine (typeof (void));
		});

	}
}

