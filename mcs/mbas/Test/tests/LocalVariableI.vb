'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Option Explicit off
Module F		
	Sub Main()	
		if fun<>Nothing then
			Throw new System.Exception("Local Variables not working properly. Expected Nothing but got"&fun)
		End if
	End Sub
End Module
