REM LineNo: 13
REM ExpectedError: BC30013
REM ErrorMessage: '#ElseIf', '#Else', or '#End If' must be preceded by a matching '#If'.



Imports System
Module ConditionalCompilation
	Sub Main()
		Console.WriteLine("Hello World")
	End Sub
End Module
#End If