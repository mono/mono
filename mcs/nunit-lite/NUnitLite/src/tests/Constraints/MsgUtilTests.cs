// ***********************************************************************
// Copyright (c) 2007 Charlie Poole
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

namespace NUnit.Framework.Constraints.Tests
{
	/// <summary>
	/// Summary description for MsgUtilTests.
	/// </summary>
	[TestFixture]
	public class MsgUtilTests
	{
        [TestCase("\n", "\\n")]
        [TestCase("\n\n", "\\n\\n")]
        [TestCase("\n\n\n", "\\n\\n\\n")]
        [TestCase("\r", "\\r")]
        [TestCase("\r\r", "\\r\\r")]
        [TestCase("\r\r\r", "\\r\\r\\r")]
        [TestCase("\r\n", "\\r\\n")]
        [TestCase("\n\r", "\\n\\r")]
        [TestCase("This is a\rtest message", "This is a\\rtest message")]
        [TestCase("", "")]
#if CLR_2_0 || CLR_4_0
		[TestCase(null, null)]
#endif
        [TestCase("\t", "\\t")]
        [TestCase("\t\n", "\\t\\n")]
        [TestCase("\\r\\n", "\\\\r\\\\n")]
        // TODO: Figure out why this fails in Mono
        //[TestCase("\0", "\\0")]
        [TestCase("\a", "\\a")]
        [TestCase("\b", "\\b")]
        [TestCase("\f", "\\f")]
        [TestCase("\v", "\\v")]
        [TestCase("\x0085", "\\x0085", Description = "Next line character")]
        [TestCase("\x2028", "\\x2028", Description = "Line separator character")]
        [TestCase("\x2029", "\\x2029", Description = "Paragraph separator character")]
        public void EscapeControlCharsTest(string input, string expected)
		{
            Assert.That( MsgUtils.EscapeControlChars(input), Is.EqualTo(expected) );
		}

        [Test]
        public void EscapeNullCharInString()
        {
            Assert.That(MsgUtils.EscapeControlChars("\0"), Is.EqualTo("\\0"));
        }

        private const string s52 = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

        [TestCase(s52, 52, 0, s52, TestName="NoClippingNeeded")]
        [TestCase(s52, 29, 0, "abcdefghijklmnopqrstuvwxyz...", TestName="ClipAtEnd")]
        [TestCase(s52, 29, 26, "...ABCDEFGHIJKLMNOPQRSTUVWXYZ", TestName="ClipAtStart")]
        [TestCase(s52, 28, 26, "...ABCDEFGHIJKLMNOPQRSTUV...", TestName="ClipAtStartAndEnd")]
        public void TestClipString(string input, int max, int start, string result)
        {
            System.Console.WriteLine("input=  \"{0}\"", input);
            System.Console.WriteLine("result= \"{0}\"", result);
            Assert.That(MsgUtils.ClipString(input, max, start), Is.EqualTo(result));
        }

        //[TestCase('\0')]
        //[TestCase('\r')]
        //public void CharacterArgumentTest(char c)
        //{
        //}

        [Test]
        public void ClipExpectedAndActual_StringsFitInLine()
        {
            string eClip = s52;
            string aClip = "abcde";
            MsgUtils.ClipExpectedAndActual(ref eClip, ref aClip, 52, 5);
            Assert.That(eClip, Is.EqualTo(s52));
            Assert.That(aClip, Is.EqualTo("abcde"));

            eClip = s52;
            aClip = "abcdefghijklmno?qrstuvwxyz";
            MsgUtils.ClipExpectedAndActual(ref eClip, ref aClip, 52, 15);
            Assert.That(eClip, Is.EqualTo(s52));
            Assert.That(aClip, Is.EqualTo("abcdefghijklmno?qrstuvwxyz"));
        }

        [Test]
        public void ClipExpectedAndActual_StringTailsFitInLine()
        {
            string s1 = s52;
            string s2 = s52.Replace('Z', '?');
            MsgUtils.ClipExpectedAndActual(ref s1, ref s2, 29, 51);
            Assert.That(s1, Is.EqualTo("...ABCDEFGHIJKLMNOPQRSTUVWXYZ"));
        }

        [Test]
        public void ClipExpectedAndActual_StringsDoNotFitInLine()
        {
            string s1 = s52;
            string s2 = "abcdefghij";
            MsgUtils.ClipExpectedAndActual(ref s1, ref s2, 29, 10);
            Assert.That(s1, Is.EqualTo("abcdefghijklmnopqrstuvwxyz..."));
            Assert.That(s2, Is.EqualTo("abcdefghij"));

            s1 = s52;
            s2 = "abcdefghijklmno?qrstuvwxyz";
            MsgUtils.ClipExpectedAndActual(ref s1, ref s2, 25, 15);
            Assert.That(s1, Is.EqualTo("...efghijklmnopqrstuvw..."));
            Assert.That(s2, Is.EqualTo("...efghijklmno?qrstuvwxyz"));
        }
	}
}
