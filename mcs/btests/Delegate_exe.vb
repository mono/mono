Imports System
Imports NSDelegate

Class C1
	dim x1 as new C()
	sub __f()
		System.Console.WriteLine("__f called")
	End sub


	public sub s()
		x1.callSD(AddressOf Me.__f)
	End sub
End Class

Module M
	Sub main
		dim x as new C1()
		x.s()
	End Sub
End Module
