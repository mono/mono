REM LineNo: 9
REM ExpectedError: BC30201
REM ErrorMessage: Expression expected.

Imports System
Module StringLiteral
	Sub Main()
		Try
			Dim a As String='Hello'
		Catch e As Exception
			Console.WriteLine(e.Message)
		End Try
	
	End Sub
End Module
