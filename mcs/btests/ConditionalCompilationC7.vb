REM LineNo: 15
REM ExpectedError: BC30012
REM ErrorMessage: '#If' block must end with a matching '#End If'.


REM LineNo: 13
REM ExpectedError: BC30625
REM ErrorMessage: 'Module' statement must end with a matching 'End Module'.



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
