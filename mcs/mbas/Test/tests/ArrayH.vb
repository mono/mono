'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

module m
	sub main()
		dim a1(3) as integer 
		dim a2(3) as Double
		dim a3(3) as Byte 
		dim a4(3) as Boolean 
		dim a5(3) as String 

		if a1(1) <> 0 then	
			Throw New System.Exception("Integer array not working")
		End if 
		if a2(1) <> 0 then	
			Throw New System.Exception("Double array not working")
		End if 
		if a3(1) <> 0 then	
			Throw New System.Exception("Byte array not working")
		End if 
		if a4(1) <> False then	
			Throw New System.Exception("Boolean array not working")
		End if 
		if a5(1) <> "" then	
			Throw New System.Exception("String array not working")
		End if 
	end sub
end module
