REM LineNo: 9
REM ExpectedError: BC30036
REM ErrorMessage: Overflow.

Imports System
Module DoubleLiteral
	Sub Main()
		Try
			Dim a As Double=1.23E+400D
			     		                                                                                   	     
		Catch e As Exception
			Console.WriteLine(e.Message)
		End Try
	End Sub
End Module
