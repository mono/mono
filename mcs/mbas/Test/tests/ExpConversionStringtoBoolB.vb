'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2003 Ximian, Inc.

Module ExpConversionStringtoBooleanA
	Sub Main()
			Dim a as Boolean
			Dim b as String= "False"
			a = CBool(b)
			if a <> False
				Throw new System.Exception("Conversion of String to Boolean not working. Expected False but got " &a) 
			End if		
	End Sub
End Module
