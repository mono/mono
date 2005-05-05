'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 13
REM ExpectedError: BC30439
REM ErrorMessage: Constant expression not representable in type 'Byte'.

Module M
	Sub Main()
		dim count as integer
		for i as Byte  = 15 to 10 step -1
			count = count+1 
		next i
		if count<>0
			throw new System.Exception("#A1 For not working")		
		end if
	end sub
End Module
