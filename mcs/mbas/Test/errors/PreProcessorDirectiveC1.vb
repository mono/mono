REM LineNo: 8
REM ExpectedError: BC30248
REM ErrorMessage: 'If', 'ElseIf', 'Else', 'End If', or 'Const' expected.

Imports System
Module PreProcessorDirective
	'BC30248: Unknown Pre-Processor directive encountered
	#UnknownDirective
	Sub Main()
		Console.WriteLine(“In test.aspx”)
	End Sub
End Module
