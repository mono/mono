REM LineNo: 24
REM ExpectedError: BC32030
REM ErrorMessage: '#ElseIf' cannot follow '#Else' as part of a '#If' block.



Imports System
Module ConditionalCompilation
	Sub Main()
		Console.WriteLine("Hello World 1")
	End Sub
#If False
	Sub A()
		Console.WriteLine("Hello World 2")
	End Sub
#ElseIf True
	Sub B()
		Console.WriteLine("Hello World 3")
	End Sub
#Else
	Sub C()
		Console.WriteLine("Hello World 4")
	End Sub
#ElseIf False
	Sub D()
		Console.WriteLine("Hello World 5")
	End Sub
#End If
End Module
