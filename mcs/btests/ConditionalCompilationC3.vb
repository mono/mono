REM LineNo: 14
REM ExpectedError: BC30012
REM ErrorMessage: '#If' block must end with a matching '#End If'.



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
