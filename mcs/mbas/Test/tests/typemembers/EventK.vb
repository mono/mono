'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module C
        Private Event E
        Sub S()
               RaiseEvent E
        End Sub
End Module

Module A
	Sub Main()
	End Sub
End Module
