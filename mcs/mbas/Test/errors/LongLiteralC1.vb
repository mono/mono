REM LineNo: 10
REM ExpectedError: BC30035
REM ErrorMessage: Syntax error.

Imports System
Module LongLiteral
	Sub Main()
	Try
		Dim a As Long
		a=&H
	Catch e As Exception
		Console.WriteLine(e.Message)
	End Try
	End Sub
End Module
