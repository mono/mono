'Line 7, BC30014: '#ElseIf' must be preceded by a matching '#If' or '#ElseIf'

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
