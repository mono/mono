REM LineNo: 24
REM ExpectedError: BC30028
REM ErrorMessage: '#Else' must be preceded by a matching '#If' or '#ElseIf'.

REM LineNo: 28
REM ExpectedError: BC32030
REM ErrorMessage: '#ElseIf' cannot follow '#Else' as part of a '#If' block.

'BC32030: '#Else' cannot follow another '#Else' in a conditional compilation block

Imports System
Module ConditionalCompilation
	Sub Main()
		Console.WriteLine("Hello World 1")
	End Sub
#If False
	Sub A()
		Console.WriteLine("Hello World 2")
	End Sub
#Else
	Sub B()
		Console.WriteLine("Hello World 3")
	End Sub
#Else
	Sub C()
		Console.WriteLine("Hello World 4")
	End Sub
#ElseIf True
	Sub D()
		Console.WriteLine("Hello World 5")
	End Sub
#End If
End Module
