'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module M
	Sub Main()
		dim i as object = new date
		dim count as integer
		for i   = 1 to 10 step 1
			count = count+1 
		next i
		if count<>10
			throw new System.Exception("#A1 For not working" )		
		end if
	end sub
End Module
