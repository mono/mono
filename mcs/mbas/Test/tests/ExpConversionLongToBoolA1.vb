Module ExpConversionLongtoBool
	Sub Main()
		Dim a As Long = 4940656
		Dim b as Boolean
		b = CBool(a)
		if b <> True
			Throw new System.Exception("Long to Boolean Conversion is not working properly. Expected True but got " &b)
		End if	
	End Sub
End Module
