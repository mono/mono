REM LineNo: 12
REM ExpectedError: BC30625
REM ErrorMessage: 'Module' statement must end with a matching 'End Module'.

REM LineNo: 14
REM ExpectedError: BC30012
REM ErrorMessage: '#If' block must end with a matching '#End If'.

'Line 6, BC30012: '#If' must end with a matching '#End If'

Imports System
Module ConditionalCompilation

#If True
	Sub Main()
		Console.WriteLine("Hello World 1")
	End Sub
#ElseIf False
	Sub R()
		Console.WriteLine("Hello World 2")
	End Sub
End Module
