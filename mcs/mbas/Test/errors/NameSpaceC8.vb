'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2003 Ximian, Inc.

REM LineNo: 25
REM ExpectedError: BC30562
REM ErrorMessage: 'C' is ambiguous between declarations in Modules 'A.B' and 'A.B1'.

Imports A

Namespace A
	Public Module B
		Sub C(a as integer)
		End Sub
	End Module
	Public Module B1
		Sub C(a as integer)
		End Sub
	End Module
End Namespace

Module NamespaceA	
	Sub Main()
		C(1)
	End Sub
End Module
