Imports System

Module M
	Delegate Sub SD()
	sub f(i as integer)
	End sub


	Sub Main()
		dim d1 as SD 
		d1= new SD(AddressOf f)
		d1.Invoke()
	End Sub
End Module
