REM LineNo: 9
REM ExpectedError: BC30248
REM ErrorMessage: 'If', 'ElseIf', 'Else', 'End If', or 'Const' expected.

Imports System
Module ConditionalConstants
	Sub Main()
		Dim value As Integer
		#A=True		' const missing
		#If A
			value=10
		#End If
			
	End Sub
End Module
