'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

Imports System
Imports Microsoft.Visualbasic

Module ExpressionParenthesis
	Sub Main()
		Dim y = 2
		Dim z = 3
		Dim  x  = (4 * (y + z)) ^ (4 / 2) * 5 + 5*(z-y)
		If x <> 2005
			Throw New Exception ("Unexpected value for the Expression. x should be Equal to 2005 but got x=" &x)	
		End If
	End Sub
End Module
