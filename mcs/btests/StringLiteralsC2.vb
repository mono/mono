REM LineNo: 13
REM ExpectedError: BC30201
REM ErrorMessage: Expression expected.

REM LineNo: 14
REM ExpectedError: BC30451
REM ErrorMessage: Name 'Hello' is not declared.

Imports System
Module StringLiteral
	Sub Main()
		Try
			Dim a As String='Hello'
			Dim b As String=Hello
		Catch e As Exception
			Console.WriteLine(e.Message)
		End Try
	
	End Sub
End Module
