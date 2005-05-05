'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 18
REM ExpectedError: BC30439
REM ErrorMessage: Constant expression not representable in type 'E'.

Public Enum E as Byte
	A 	
End Enum

Module M
	Sub Main()		
		fun()
	end sub
	sub fun(Optional i as E = -1,Optional j as integer = 20)
		if i<>1 or j<>20
			throw new System.Exception("#A1 Not working")
		end if
	end sub		
End Module
