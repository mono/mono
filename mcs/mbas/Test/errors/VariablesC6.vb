'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2003 Ximian, Inc.

REM LineNo: 19
REM ExpectedError: BC30203
REM ErrorMessage: Identifier expected.

REM LineNo: 20
REM ExpectedError: BC30205
REM ErrorMessage: End of statement expected.

Imports System

Module Default1	
	Sub Main()
		Dim _1, _a, a, a1 As Integer 'These are the correct for of declarations		
		Dim 1aa, *aa As Integer ' These are wrong
		Dim aa3.3 As Integer ' This is wrong
	End Sub
End Module
