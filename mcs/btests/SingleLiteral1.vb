REM LineNo: 9
REM ExpectedError: BC30036
REM ErrorMessage: Overflow.

Imports System
Module SingleLiteral
	Sub Main()
		Try
			Dim a As Single=1.23E+40F
			Dim b As Single
			b="Hello"       		                                                                                   	     
		Catch e As Exception
			Console.WriteLine(e.Message)
		End Try
	End Sub
End Module
