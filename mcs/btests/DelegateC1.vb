REM LineNo: 15
REM ExpectedError: BC30408
REM ErrorMessage: Method 'Public Sub f(i As Integer)' does not have the same signature as delegate 'Delegate Sub SD()'.

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
