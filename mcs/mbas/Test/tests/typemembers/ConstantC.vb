'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

'Checking on all possible acessors on const
Imports System

Module Test
	Const a as Integer = 1
	Public Const a1 as Integer = 1
	Private Const a2 as Integer = 1
	Class C
		Protected Const a3 as Integer = 1
	End Class
	Public Sub Main()						
	End Sub
End Module

