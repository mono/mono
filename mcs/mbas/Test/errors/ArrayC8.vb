'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 12
REM ExpectedError: BC30036
REM ErrorMessage: Overflow.

module m
	sub main()
		dim Y(1,10000000000000000000) as integer 
	end sub
end module
