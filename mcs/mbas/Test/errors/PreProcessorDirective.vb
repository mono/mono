REM LineNo: 9
REM ExpectedError: BC30248
REM ErrorMessage: 'If', 'ElseIf', 'Else', 'End If', or 'Const' expected.

'BC30248: Unknown Pre-Processor directive encountered

Imports System
Module PreProcessorDirective
	#UnknownDirective
	Sub Main()
		Console.WriteLine("Hello World")
	End Sub
End Module
