// CS0658: `class' is invalid attribute target. All attributes in this attribute section will be ignored
// Line : 8
// Compiler options: -warnaserror -warn:1

using System;

[class:Serializable]
public class C
{
    public static void Main () {}
}
