REM LineNo: 19
REM ExpectedError: BC30408
REM ErrorMessage: Method 'Public Function f1(a As Integer) As Integer' does not have the same signature as delegate 'Delegate Sub SD()'.

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
