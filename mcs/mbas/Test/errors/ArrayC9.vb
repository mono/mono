'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 12
REM ExpectedError: BC30672
REM ErrorMessage: Explicit initialization is not permitted for arrays declared with explicit bounds.

module m
	sub main()
		dim Y(1) as integer = {1}
	end sub
end module
