REM LineNo: 11
REM ExpectedError: BC30014
REM ErrorMessage: '#ElseIf' must be preceded by a matching '#If' or '#ElseIf'.



Imports System
Module ConditionalCompilation
	Sub Main()
			Console.WriteLine("Hello World 1")
		#ElseIf True
			Console.WriteLine("Hello World 2")
		#Else
			Console.WriteLine("Hello World 3")
		#End If


	End Sub
End Module
