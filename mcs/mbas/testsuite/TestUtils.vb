Imports System
Imports System.Text
Imports System.Security.Cryptography

Public Module TestUtils

Public Function GenerateHash(ByVal SourceText As String) As String
	'Create an encoding object to ensure the encoding standard for the source text
	Dim Ue As New UnicodeEncoding()
	
	'Retrieve a byte array based on the source text
	Dim ByteSourceText() As Byte = Ue.GetBytes(SourceText)
	
	'Instantiate an MD5 Provider object
	Dim Md5 As New MD5CryptoServiceProvider()
	
	'Compute the hash value from the source
	Dim ByteHash() As Byte = Md5.ComputeHash(ByteSourceText)
	
	'And convert it to String format for return
	Return Convert.ToBase64String(ByteHash)
End Function

End Module
