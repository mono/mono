'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module stopstmt
	Sub Main()
		#if CAUSEERRORS
			Yield 1
			Yield Stop
		#end if
	End Sub
End Module
