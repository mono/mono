'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2003 Ximian, Inc.

REM LineNo: 29
REM ExpectedError: BC30561
REM ErrorMessage: 'C' is ambiguous, imported from the namespaces or types 'AA, A'.

Imports A
Imports AA

Namespace A
	Public Module B
		Sub C(a as integer)
		End Sub
	End Module
End Namespace

Namespace AA
	Public Module B
		Sub C(a as Integer)
		End Sub
	End Module
End Namespace

Module NamespaceA	
	Sub Main()
		C(1)
	End Sub
End Module
