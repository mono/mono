'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Public Enum E
	A = 11
	B
	C	
End Enum

Module M
	Sub Main()		
		fun()
	end sub
	sub fun(Optional i as E = 1,Optional j as integer = 20)
		if i<>1 or j<>20
			throw new System.Exception("#A1 Not working")
		end if
	end sub		
End Module
