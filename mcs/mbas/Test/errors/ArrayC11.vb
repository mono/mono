'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 13
REM ExpectedError: BC30415
REM ErrorMessage:  'ReDim' cannot change the number of dimensions of an array.

module m
	sub main()
		dim Y(2) as integer 
		Redim Y(1,2) as integer 
	end sub
end module
