'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 12
REM ExpectedError: BC30439
REM ErrorMessage:  Constant expression not representable in type 'Integer'.

module m
	sub main()
		dim Y(Double.NaN) as integer 
	end sub
end module
