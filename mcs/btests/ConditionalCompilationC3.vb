'BC30012: '#If' must end with a matching '#EndIf'

Imports System
Module ConditionalCompilation
	Sub Main()
		#If False
			Console.WriteLine("Hello World 1")
		#End If

		#If True
			Console.WriteLine("Hello World 2")
	End Sub
End Module
