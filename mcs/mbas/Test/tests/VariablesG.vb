'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2003 Ximian, Inc.

'Date and String are loosely typed.

Imports System

Module Default1	
	Sub Main()
		Dim a as Date
		Dim b as String
		b=a
		a=b
	End Sub
End Module
