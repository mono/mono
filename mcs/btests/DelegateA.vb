Imports System

Module M
	Delegate Sub SD()
	sub f()
		System.Console.WriteLine("s called")
	End sub

	Public Delegate Function SF(a as integer) as Integer
	Public Function f1(a as integer) as Integer
		System.Console.WriteLine("f1 called")
		return a
	End Function

	Public Function TD(d as SD) as Integer
		d.Invoke()
	End Function

	Sub Main()
		dim d1 as SD 
		d1 = new SD(AddressOf f)
		d1.Invoke()

		dim d2 as SD
		d2 = AddressOf f 
		d2.Invoke()

		dim d3 as new SD(AddressOf f)
		d3.Invoke()

		dim d4 as SF
		d4 = new SF(AddressOf f1)
		Dim i as Integer = d4.Invoke(10)
		if i <> 10 then
         		Throw new System.Exception ("#A1, Unexpected result")
                end if

		TD(AddressOf f)
		
	End Sub
End Module
