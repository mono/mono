'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module M
Class A
	Shared public i as Integer
End Class
	Sub Main()
		A.i = A.i+1
		fun()
	End Sub
	Sub fun()
		A.i = A.i+1
		if A.i<>2
			Throw new System.Exception("Shared variable not workin") 
		end if
	End Sub
End Module
