'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

Imports System
Module ExpressionStringNothing

   Sub main()
		Dim A as String = "Something"
		Dim B as String = Nothing
		Dim C  = A+B
		Dim D= "Something"
		if D <> C Then
			Throw New Exception (" Unexpected Behavior.Nothing is treated as if it were the empty string literal "" and D should be euqal to A+B")
		End if
	End Sub
End Module


