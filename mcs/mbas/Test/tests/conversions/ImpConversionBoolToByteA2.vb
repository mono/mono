Module ImpConversionofBooltoByte
	Sub Main()
		Dim b as Boolean = True
		Dim a as Byte = b 
		if a <> 255 then 
			Throw New System.Exception("Implicit Conversion of Bool(True) to Byte has Failed. Expected 255 got " & a)
		End if				
	End Sub
End Module
