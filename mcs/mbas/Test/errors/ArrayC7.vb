'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 13
REM ExpectedError: BC30611
REM ErrorMessage: Array dimensions cannot have a negative size.

module m
	sub main()
		dim x(1,5.3434) as integer 
		dim Y(1,-6) as integer 
	end sub
end module
