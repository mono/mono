REM LineNo: 9
REM ExpectedError: BC30451
REM ErrorMessage: Name 'Hello' is not declared.

Imports System
Module StringLiteral
	Sub Main()
		Try
			Dim b As String=Hello
		Catch e As Exception
			Console.WriteLine(e.Message)
		End Try
	
	End Sub
End Module
