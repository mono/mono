'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 13
REM ExpectedError: BC30337
REM ErrorMessage: 'For' loop control variable cannot be of type 'Date'.

Module M
	Sub Main()
		dim count as integer
		for i as Date  = 1 to 10 step -1
			count = count+1 
		next i
	end sub
End Module
