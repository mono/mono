'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)
' Checking F and R 

Imports System
Module SimpleExpressionLiterals
	Sub main()
		Dim A = 1.401298E-45F
		Dim B = 1R
		B = A
		Dim C = B
		If C <> A Then
			Throw New Exception ("Error With Expression. C should be Equal to B = A")
		End If
	End Sub
End Module
