Option Strict Off
Imports System
Module M
	Sub Main()
	Try
		Dim a As Boolean=True
		Dim b As Boolean=False
		Dim c As Boolean
		c=a+b
		If a<>True
			Throw New Exception("BoolLiteralB:Failed")
		End If
	Catch e As Exception
		Console.WriteLine(e.Message)
	End Try	
	End Sub
End Module
