Imports System

Module M
	Delegate Sub SD()
	sub f()
	End sub

	Public Delegate Function SF(a as integer) as Integer
	Public Function f1(a as integer) as Integer
		return a
	End Function

	Sub Main()
		dim d1 as SD 
		d1= new SD(AddressOf f1)
		d1.Invoke()
	End Sub
End Module
