//
// SslStreamTest.cs
//      - Unit tests for System.Net.Security.SslStream
//
// Authors:
//      Maciej Paszta (maciej.paszta@gmail.com)
//      Sebastien Pouliot  <sebastien@xamarin.com>
//      Edward Ned Harvey <edward.harvey.mono@clevertrove.com>
//
// Copyright (c) 2014 Edward Ned Harvey <edward.harvey.mono@clevertrove.com>
// Copyright (C) Maciej Paszta, 2012
// Copyright 2014 Xamarin Inc. (http://www.xamarin.com)
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
//

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Security.Authentication;

using MonoX509Certificate = Mono.Security.X509.X509Certificate;
using MonoX509StoreManager = Mono.Security.X509.X509StoreManager;

namespace MonoTests.System.Net.Security
{

[TestFixture]
public class SslStreamTest {

	byte[] m_serverCertRaw = { 48, 130, 5, 165, 2, 1, 3, 48, 130, 5, 95, 6, 9, 42, 134, 72, 134, 247, 13, 1, 7, 1, 160, 130, 5, 80, 4, 130, 5, 76, 48, 130, 5, 72, 48, 130, 2, 87, 6, 9, 42, 134, 72, 134, 247, 13, 1, 7, 6, 160, 130, 2, 72, 48, 130, 2, 68, 2, 1, 0, 48, 130, 2, 61, 6, 9, 42, 134, 72, 134, 247, 13, 1, 7, 1, 48, 28, 6, 10, 42, 134, 72, 134, 247, 13, 1, 12, 1, 3, 48, 14, 4, 8, 211, 176, 234, 3, 252, 26, 32, 15, 2, 2, 7, 208, 128, 130, 2, 16, 183, 149, 35, 180, 127, 95, 163, 122, 138, 244, 29, 177, 220, 173, 46, 73, 208, 217, 211, 190, 164, 183, 21, 110, 33, 122, 98, 163, 251, 16, 23, 106, 154, 14, 52, 177, 3, 12, 248, 226, 48, 123, 211, 6, 216, 6, 192, 175, 203, 142, 141, 143, 252, 178, 7, 162, 81, 232, 159, 42, 56, 177, 191, 53, 7, 146, 189, 236, 75, 140, 210, 143, 11, 103, 64, 58, 10, 73, 123, 39, 97, 119, 166, 114, 123, 65, 68, 214, 42, 17, 156, 122, 8, 58, 184, 134, 255, 48, 64, 20, 229, 247, 196, 12, 130, 56, 176, 69, 179, 254, 216, 45, 25, 244, 240, 116, 88, 137, 66, 13, 18, 202, 199, 59, 200, 245, 19, 175, 232, 217, 211, 12, 191, 222, 26, 162, 253, 73, 201, 48, 61, 3, 248, 117, 16, 71, 233, 183, 90, 110, 91, 116, 56, 133, 223, 148, 19, 78, 140, 123, 159, 203, 78, 15, 172, 39, 190, 39, 71, 180, 155, 48, 156, 116, 212, 52, 1, 231, 201, 196, 73, 87, 68, 104, 208, 40, 104, 32, 218, 235, 245, 84, 136, 168, 51, 9, 93, 126, 46, 80, 180, 240, 144, 79, 88, 87, 159, 24, 108, 186, 9, 20, 48, 100, 148, 250, 4, 163, 115, 131, 44, 13, 38, 222, 117, 196, 196, 128, 114, 149, 97, 93, 37, 191, 3, 192, 231, 88, 80, 218, 147, 8, 192, 165, 27, 206, 56, 42, 157, 230, 223, 130, 253, 169, 182, 245, 192, 181, 18, 212, 133, 168, 73, 92, 66, 197, 117, 245, 107, 127, 23, 146, 249, 41, 66, 219, 210, 207, 221, 205, 205, 15, 110, 92, 12, 207, 76, 239, 4, 13, 129, 127, 170, 205, 253, 148, 208, 24, 129, 24, 210, 220, 85, 45, 179, 137, 66, 134, 142, 22, 112, 48, 160, 236, 232, 38, 83, 101, 55, 51, 18, 110, 99, 69, 41, 173, 107, 233, 11, 199, 23, 61, 135, 222, 94, 74, 29, 219, 80, 128, 167, 186, 254, 235, 42, 96, 134, 5, 13, 90, 59, 231, 137, 195, 207, 28, 165, 12, 218, 5, 72, 102, 61, 135, 198, 73, 250, 97, 89, 214, 179, 244, 194, 23, 142, 157, 4, 243, 90, 69, 54, 10, 139, 76, 95, 40, 225, 219, 59, 15, 54, 182, 206, 142, 228, 248, 79, 156, 129, 246, 63, 6, 6, 236, 44, 67, 116, 213, 170, 47, 193, 186, 139, 25, 80, 166, 57, 99, 231, 156, 191, 117, 65, 76, 7, 243, 244, 127, 225, 210, 190, 164, 141, 46, 36, 99, 111, 203, 133, 127, 80, 28, 61, 160, 36, 132, 182, 16, 41, 39, 185, 232, 123, 32, 57, 189, 100, 152, 38, 205, 5, 189, 240, 65, 3, 191, 73, 85, 12, 209, 180, 1, 194, 70, 124, 57, 71, 48, 230, 235, 122, 175, 157, 35, 233, 83, 40, 20, 169, 224, 14, 11, 216, 48, 194, 105, 25, 187, 210, 182, 6, 184, 73, 95, 85, 210, 227, 113, 58, 10, 186, 175, 254, 25, 102, 39, 3, 2, 200, 194, 197, 200, 224, 77, 164, 8, 36, 114, 48, 130, 2, 233, 6, 9, 42, 134, 72, 134, 247, 13, 1, 7, 1, 160, 130, 2, 218, 4, 130, 2, 214, 48, 130, 2, 210, 48, 130, 2, 206, 6, 11, 42, 134, 72, 134, 247, 13, 1, 12, 10, 1, 2, 160, 130, 2, 166, 48, 130, 2, 162, 48, 28, 6, 10, 42, 134, 72, 134, 247, 13, 1, 12, 1, 3, 48, 14, 4, 8, 178, 13, 52, 135, 85, 49, 79, 105, 2, 2, 7, 208, 4, 130, 2, 128, 21, 84, 227, 109, 230, 144, 140, 170, 117, 250, 179, 207, 129, 100, 126, 126, 29, 231, 94, 140, 45, 26, 168, 45, 240, 4, 170, 73, 98, 115, 109, 96, 177, 206, 6, 80, 170, 22, 237, 144, 58, 95, 59, 26, 85, 135, 178, 69, 184, 44, 122, 81, 213, 135, 149, 198, 246, 83, 68, 129, 2, 186, 118, 33, 44, 214, 227, 240, 220, 51, 175, 220, 220, 180, 113, 216, 101, 138, 81, 54, 38, 0, 216, 30, 29, 187, 213, 230, 12, 181, 130, 21, 241, 98, 120, 41, 150, 176, 69, 37, 169, 249, 123, 212, 254, 135, 154, 214, 127, 39, 105, 149, 180, 218, 41, 207, 75, 70, 105, 169, 185, 169, 132, 173, 188, 82, 251, 71, 234, 136, 5, 254, 110, 223, 34, 4, 145, 7, 19, 51, 123, 140, 75, 226, 0, 21, 220, 228, 223, 218, 8, 169, 210, 194, 139, 93, 218, 55, 40, 174, 50, 238, 38, 166, 222, 103, 0, 209, 88, 131, 51, 222, 154, 217, 18, 172, 73, 17, 133, 54, 173, 208, 118, 104, 167, 113, 153, 223, 251, 154, 120, 176, 18, 127, 51, 206, 164, 77, 86, 9, 82, 212, 86, 162, 206, 230, 79, 217, 178, 42, 217, 162, 152, 188, 217, 59, 212, 117, 200, 135, 75, 74, 43, 1, 42, 79, 180, 164, 250, 122, 103, 103, 157, 11, 14, 33, 48, 8, 108, 155, 46, 124, 223, 204, 169, 124, 104, 11, 246, 213, 226, 16, 125, 17, 228, 15, 178, 141, 79, 78, 115, 76, 131, 122, 166, 124, 154, 1, 174, 178, 176, 213, 208, 188, 71, 118, 220, 168, 64, 218, 176, 134, 38, 229, 14, 109, 162, 125, 16, 57, 249, 201, 180, 17, 182, 143, 184, 12, 248, 113, 65, 70, 109, 79, 249, 34, 170, 35, 228, 219, 121, 202, 228, 121, 127, 255, 22, 173, 202, 171, 33, 232, 4, 240, 142, 216, 80, 56, 177, 83, 93, 123, 217, 213, 157, 99, 34, 194, 61, 228, 239, 194, 20, 27, 9, 53, 132, 79, 19, 97, 107, 31, 51, 39, 176, 223, 90, 88, 67, 138, 194, 169, 176, 144, 202, 119, 146, 74, 27, 118, 63, 129, 230, 101, 104, 75, 116, 49, 223, 254, 225, 70, 206, 183, 11, 134, 148, 10, 55, 57, 50, 178, 144, 164, 139, 233, 169, 109, 186, 211, 95, 123, 75, 111, 192, 187, 127, 240, 45, 226, 194, 240, 128, 10, 79, 178, 192, 66, 21, 197, 24, 171, 141, 255, 185, 230, 84, 206, 151, 9, 93, 115, 162, 12, 115, 129, 218, 103, 219, 183, 142, 123, 3, 110, 139, 208, 4, 146, 76, 99, 246, 240, 32, 169, 148, 16, 146, 172, 230, 36, 56, 145, 23, 94, 209, 92, 38, 244, 127, 70, 121, 253, 66, 55, 36, 140, 98, 105, 233, 112, 24, 23, 230, 112, 62, 244, 12, 48, 30, 51, 0, 18, 244, 139, 66, 245, 234, 203, 195, 52, 119, 255, 84, 82, 204, 100, 176, 167, 24, 224, 8, 127, 214, 148, 115, 242, 56, 190, 72, 221, 68, 252, 36, 74, 254, 57, 52, 96, 20, 173, 32, 236, 87, 15, 16, 76, 9, 48, 3, 61, 2, 137, 137, 9, 68, 213, 99, 163, 63, 201, 83, 241, 98, 7, 117, 108, 4, 123, 170, 18, 10, 19, 198, 31, 170, 15, 247, 216, 145, 172, 239, 137, 181, 80, 160, 24, 11, 35, 131, 58, 218, 22, 250, 215, 52, 160, 246, 197, 183, 92, 137, 0, 245, 63, 49, 183, 246, 195, 58, 63, 4, 75, 10, 92, 131, 181, 59, 78, 247, 44, 150, 49, 49, 107, 211, 62, 71, 62, 222, 159, 161, 118, 236, 55, 219, 49, 0, 3, 82, 236, 96, 20, 83, 39, 245, 208, 240, 245, 174, 218, 49, 21, 48, 19, 6, 9, 42, 134, 72, 134, 247, 13, 1, 9, 21, 49, 6, 4, 4, 1, 0, 0, 0, 48, 61, 48, 33, 48, 9, 6, 5, 43, 14, 3, 2, 26, 5, 0, 4, 20, 30, 154, 48, 126, 198, 239, 114, 62, 12, 58, 129, 172, 67, 156, 76, 214, 62, 205, 89, 28, 4, 20, 135, 177, 105, 83, 79, 93, 181, 149, 169, 49, 112, 201, 70, 212, 153, 79, 198, 163, 137, 90, 2, 2, 7, 208 };
	byte[] m_clientCertRaw = { 48, 130, 5, 173, 2, 1, 3, 48, 130, 5, 103, 6, 9, 42, 134, 72, 134, 247, 13, 1, 7, 1, 160, 130, 5, 88, 4, 130, 5, 84, 48, 130, 5, 80, 48, 130, 2, 95, 6, 9, 42, 134, 72, 134, 247, 13, 1, 7, 6, 160, 130, 2, 80, 48, 130, 2, 76, 2, 1, 0, 48, 130, 2, 69, 6, 9, 42, 134, 72, 134, 247, 13, 1, 7, 1, 48, 28, 6, 10, 42, 134, 72, 134, 247, 13, 1, 12, 1, 3, 48, 14, 4, 8, 35, 249, 113, 131, 30, 42, 21, 176, 2, 2, 7, 208, 128, 130, 2, 24, 78, 185, 144, 242, 231, 15, 133, 251, 122, 86, 61, 132, 148, 253, 47, 83, 198, 14, 11, 70, 79, 14, 21, 66, 91, 72, 147, 159, 95, 245, 240, 210, 194, 174, 25, 112, 171, 126, 126, 143, 64, 173, 63, 224, 49, 172, 100, 129, 84, 86, 91, 50, 28, 29, 118, 139, 22, 251, 248, 181, 110, 246, 226, 92, 108, 178, 25, 199, 62, 90, 12, 5, 189, 249, 22, 230, 37, 230, 190, 97, 50, 12, 252, 4, 66, 204, 92, 12, 98, 222, 69, 230, 221, 64, 163, 106, 194, 113, 223, 40, 81, 138, 123, 212, 171, 160, 178, 153, 29, 108, 64, 110, 166, 82, 26, 157, 63, 69, 66, 93, 231, 232, 228, 189, 85, 63, 11, 53, 192, 171, 124, 148, 0, 31, 106, 146, 207, 71, 16, 138, 214, 79, 0, 103, 133, 199, 116, 45, 127, 230, 199, 230, 11, 179, 9, 253, 45, 23, 194, 122, 217, 20, 200, 214, 127, 138, 133, 190, 29, 110, 129, 29, 20, 186, 106, 182, 114, 134, 120, 170, 120, 137, 111, 200, 137, 10, 43, 139, 183, 217, 245, 38, 165, 126, 142, 233, 20, 238, 238, 185, 12, 71, 4, 54, 128, 28, 70, 139, 94, 119, 25, 243, 241, 161, 125, 97, 132, 19, 225, 249, 117, 226, 108, 58, 163, 221, 126, 111, 192, 157, 65, 104, 134, 83, 92, 26, 143, 23, 112, 12, 94, 111, 59, 138, 79, 93, 98, 49, 239, 77, 99, 119, 89, 127, 176, 12, 217, 67, 46, 84, 74, 10, 63, 227, 18, 153, 118, 104, 92, 31, 198, 187, 91, 139, 239, 231, 154, 111, 254, 75, 172, 166, 87, 251, 152, 231, 61, 101, 115, 121, 190, 52, 95, 195, 134, 176, 248, 143, 13, 145, 141, 107, 166, 175, 231, 243, 27, 105, 150, 61, 179, 89, 134, 182, 140, 243, 116, 170, 255, 110, 26, 137, 79, 102, 45, 225, 160, 67, 75, 19, 58, 188, 168, 11, 98, 149, 139, 164, 93, 236, 115, 245, 59, 183, 177, 3, 115, 218, 35, 117, 62, 172, 172, 179, 230, 209, 116, 119, 41, 144, 90, 242, 74, 107, 153, 130, 250, 38, 236, 33, 11, 117, 51, 42, 213, 15, 24, 57, 193, 250, 76, 41, 79, 229, 249, 215, 236, 131, 136, 160, 186, 142, 7, 70, 197, 21, 148, 57, 136, 70, 89, 15, 157, 231, 130, 24, 80, 99, 64, 144, 75, 210, 255, 101, 51, 200, 237, 180, 238, 195, 173, 187, 225, 177, 212, 99, 176, 28, 51, 33, 37, 230, 79, 112, 142, 174, 75, 183, 125, 207, 108, 88, 9, 76, 173, 254, 165, 193, 97, 39, 245, 80, 0, 131, 225, 116, 179, 67, 168, 171, 143, 11, 49, 153, 244, 185, 253, 9, 42, 40, 53, 225, 137, 184, 37, 31, 53, 121, 28, 140, 27, 145, 84, 182, 40, 176, 152, 135, 77, 232, 20, 144, 74, 81, 227, 29, 26, 179, 50, 80, 244, 181, 54, 146, 224, 25, 233, 70, 0, 153, 227, 72, 140, 142, 185, 141, 177, 127, 252, 107, 240, 146, 255, 122, 194, 92, 147, 69, 52, 67, 124, 144, 207, 146, 182, 131, 48, 130, 2, 233, 6, 9, 42, 134, 72, 134, 247, 13, 1, 7, 1, 160, 130, 2, 218, 4, 130, 2, 214, 48, 130, 2, 210, 48, 130, 2, 206, 6, 11, 42, 134, 72, 134, 247, 13, 1, 12, 10, 1, 2, 160, 130, 2, 166, 48, 130, 2, 162, 48, 28, 6, 10, 42, 134, 72, 134, 247, 13, 1, 12, 1, 3, 48, 14, 4, 8, 46, 213, 31, 185, 121, 55, 235, 182, 2, 2, 7, 208, 4, 130, 2, 128, 62, 51, 182, 78, 208, 241, 24, 1, 167, 56, 187, 181, 138, 26, 252, 10, 43, 143, 17, 4, 102, 205, 177, 108, 52, 174, 60, 135, 233, 89, 184, 112, 5, 43, 87, 209, 148, 146, 224, 83, 167, 26, 165, 130, 202, 139, 251, 183, 156, 167, 251, 209, 127, 169, 91, 124, 18, 171, 5, 47, 145, 51, 113, 161, 84, 123, 26, 149, 11, 79, 8, 14, 242, 162, 215, 239, 51, 120, 85, 183, 144, 208, 130, 198, 4, 98, 217, 54, 29, 168, 103, 60, 50, 72, 92, 160, 51, 107, 153, 40, 15, 143, 75, 78, 212, 77, 206, 188, 176, 134, 213, 101, 109, 116, 238, 215, 26, 90, 33, 134, 160, 56, 21, 200, 6, 27, 185, 239, 8, 193, 188, 61, 114, 101, 76, 224, 75, 28, 18, 149, 83, 33, 100, 103, 59, 246, 21, 236, 141, 241, 126, 163, 126, 236, 180, 106, 98, 6, 196, 11, 19, 12, 81, 153, 79, 221, 230, 199, 176, 95, 8, 124, 189, 242, 151, 182, 126, 250, 227, 53, 55, 86, 39, 85, 171, 57, 157, 14, 215, 226, 204, 195, 59, 121, 85, 54, 213, 45, 101, 164, 38, 112, 114, 168, 20, 28, 152, 139, 43, 146, 15, 84, 64, 46, 39, 55, 56, 110, 160, 32, 120, 156, 253, 64, 79, 163, 3, 156, 85, 80, 197, 214, 26, 250, 200, 63, 212, 4, 119, 96, 32, 25, 1, 121, 112, 170, 87, 75, 163, 32, 175, 195, 82, 64, 74, 247, 4, 152, 203, 18, 129, 201, 221, 98, 35, 84, 148, 57, 15, 121, 90, 195, 79, 50, 99, 73, 163, 162, 131, 26, 203, 106, 237, 135, 203, 239, 43, 253, 187, 68, 33, 82, 101, 121, 61, 9, 223, 54, 67, 138, 11, 146, 175, 102, 163, 112, 51, 63, 124, 248, 183, 89, 81, 250, 15, 159, 161, 201, 38, 6, 243, 224, 61, 143, 117, 144, 157, 184, 242, 248, 155, 150, 17, 13, 158, 1, 91, 33, 107, 65, 106, 153, 211, 18, 7, 138, 230, 8, 84, 56, 110, 227, 0, 47, 33, 181, 141, 185, 119, 93, 72, 192, 100, 76, 145, 40, 163, 185, 96, 154, 151, 172, 86, 249, 167, 237, 97, 28, 137, 27, 127, 114, 218, 49, 106, 92, 40, 201, 252, 219, 52, 129, 17, 105, 198, 29, 166, 30, 229, 103, 216, 102, 84, 146, 210, 114, 32, 186, 205, 252, 253, 142, 103, 75, 83, 122, 72, 42, 118, 210, 41, 113, 227, 206, 27, 79, 83, 5, 31, 201, 245, 165, 18, 210, 112, 215, 144, 78, 91, 84, 3, 61, 236, 192, 152, 78, 16, 254, 242, 67, 46, 228, 98, 102, 20, 2, 43, 134, 97, 180, 17, 189, 30, 214, 167, 32, 128, 106, 61, 227, 166, 41, 81, 51, 208, 245, 114, 147, 66, 34, 212, 35, 152, 26, 173, 133, 0, 207, 88, 5, 171, 175, 5, 75, 207, 50, 153, 141, 141, 2, 47, 236, 252, 132, 87, 173, 163, 208, 119, 213, 77, 58, 145, 12, 21, 40, 4, 23, 114, 204, 89, 136, 152, 123, 159, 205, 149, 51, 21, 146, 219, 75, 25, 199, 22, 210, 203, 66, 19, 10, 188, 98, 152, 60, 161, 234, 122, 109, 232, 197, 79, 77, 185, 80, 210, 87, 120, 232, 158, 103, 124, 88, 110, 47, 4, 67, 123, 72, 230, 160, 33, 1, 146, 163, 54, 149, 79, 54, 21, 124, 163, 210, 38, 150, 176, 100, 53, 56, 220, 190, 98, 203, 250, 122, 34, 213, 17, 101, 203, 37, 231, 176, 182, 65, 196, 42, 169, 76, 21, 27, 87, 95, 88, 82, 52, 234, 179, 82, 207, 62, 185, 251, 85, 225, 70, 245, 220, 29, 177, 64, 146, 94, 216, 101, 226, 10, 116, 209, 44, 10, 49, 21, 48, 19, 6, 9, 42, 134, 72, 134, 247, 13, 1, 9, 21, 49, 6, 4, 4, 1, 0, 0, 0, 48, 61, 48, 33, 48, 9, 6, 5, 43, 14, 3, 2, 26, 5, 0, 4, 20, 200, 225, 57, 176, 214, 252, 236, 240, 126, 231, 49, 34, 77, 228, 178, 235, 151, 135, 242, 52, 4, 20, 251, 225, 117, 50, 254, 96, 240, 190, 40, 228, 34, 104, 253, 203, 163, 169, 25, 46, 239, 103, 2, 2, 7, 208 };	
	X509Certificate2 m_serverCert;
	X509Certificate2 m_clientCert;

	/* I don't see any documentation about how the above certs were generated; furthermore, they're not quite sufficient to run
	 * the SelfCompatibilityTest, so I don't want to mess around with the above, and I just have to add more certs.  The Junk
	 * certs below were generated via https://github.com/rahvee/MonoSslStreamServerBug/blob/master/certs/junkca.sh
	 */
	private const string JunkRootCABase64 = 
		"MIIEDDCCAvSgAwIBAgIJAMR45YEoF/npMA0GCSqGSIb3DQEBBQUAMGExCzAJBgNV" + "BAYTAkFVMRMwEQYDVQQIEwpTb21lLVN0YXRlMSEwHwYDVQQKExhJbnRlcm5ldCBX" +
		"aWRnaXRzIFB0eSBMdGQxGjAYBgNVBAMTEWp1bmtjYS5qdW5rZG9tYWluMB4XDTE0" + "MTIxNzE3MTAzNloXDTQ0MTIwOTE3MTAzNlowYTELMAkGA1UEBhMCQVUxEzARBgNV" +
		"BAgTClNvbWUtU3RhdGUxITAfBgNVBAoTGEludGVybmV0IFdpZGdpdHMgUHR5IEx0" + "ZDEaMBgGA1UEAxMRanVua2NhLmp1bmtkb21haW4wggEiMA0GCSqGSIb3DQEBAQUA" +
		"A4IBDwAwggEKAoIBAQDSRfptt3+IvwBYyCVC5whpFXTDph7yCkT6tzLTYq1KxhHL" + "QRbqBsSHBWC9KSNpOUVzbkL1snYvyZBObwa9yFOX3d/9yTNpt/klFNrWOA3VqLSs" +
		"5ie8pGBZK+s25DjzXcd7ebATyVG/JRn9bHOe3wei3a59f5vRpIsK9UP8+Fve5uaE" + "BfTziqrUiDOkNcgmhqbr8qQUfIsoXwbOMTcRWipf8/BxVqoqN0HPN8YFaHCGJB3x" +
		"OeqR0XwVGDrj+Uv9YSvyVucKRUxAgepnOkfZ7hzU4nRGu1HDbRlyda4O7VWSEn0f" + "55xht2wv+YRW7UCLFRBU4UdLQvQSTZWUTeiQTI07AgMBAAGjgcYwgcMwHQYDVR0O" +
		"BBYEFJ5QF1x47N6WzsFm4exIbYEkQDQZMIGTBgNVHSMEgYswgYiAFJ5QF1x47N6W" + "zsFm4exIbYEkQDQZoWWkYzBhMQswCQYDVQQGEwJBVTETMBEGA1UECBMKU29tZS1T" +
		"dGF0ZTEhMB8GA1UEChMYSW50ZXJuZXQgV2lkZ2l0cyBQdHkgTHRkMRowGAYDVQQD" + "ExFqdW5rY2EuanVua2RvbWFpboIJAMR45YEoF/npMAwGA1UdEwQFMAMBAf8wDQYJ" +
		"KoZIhvcNAQEFBQADggEBAEMkV52DI9/O8VnwQiUZsjV9KRoauuqUh4uPwE5G8WjP" + "ZFpyfInwOkmofTLuwezRKurMg79QCl9c0W39uSo+4fYUZDRKbDakKwg3G10oHVDE" +
		"fXQ1/HoDsT9wMshZSw2acGLLxniawAgWz3/cH2XHfJfiqDRajvRXFYv17cgwdeaH" + "MlfHb3R4v7R38YuyyIGr3butxnrhTkxMN9TKnlUJZJQb0tFTD988Q4zLO+zhnUfj" +
		"+l/DRoiWEvMEnGL2JPJa/Q5ccK6yVjyixQBWkhDmAnPtTdsuaeaJKRhlIy0IpX5g" + "5dYUlIvQ24skCbXEYRkg3Rp9wZP540x+bFhFP1v7T94=";
	private const string JunkIntermediateBase64 = 
		"MIIEGTCCAwGgAwIBAgIJAMR45YEoF/nqMA0GCSqGSIb3DQEBBQUAMGExCzAJBgNV" + "BAYTAkFVMRMwEQYDVQQIEwpTb21lLVN0YXRlMSEwHwYDVQQKExhJbnRlcm5ldCBX" +
		"aWRnaXRzIFB0eSBMdGQxGjAYBgNVBAMTEWp1bmtjYS5qdW5rZG9tYWluMB4XDTE0" + "MTIxNzE3MTAzN1oXDTQ0MTIwOTE3MTAzN1owbjELMAkGA1UEBhMCQVUxEzARBgNV" +
		"BAgTClNvbWUtU3RhdGUxITAfBgNVBAoTGEludGVybmV0IFdpZGdpdHMgUHR5IEx0" + "ZDEnMCUGA1UEAxMeaW50ZXJtZWRpYXRlLmp1bmtjYS5qdW5rZG9tYWluMIIBIjAN" +
		"BgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA56twciV/NEZYKGMoLzjFIz2towkp" + "Kl8O4Jx34YzKcaCdfHKPc3A+7PQg+2Otc/Qo2DG3buj5Dob4CcYrbMW3L+0SWhO9" +
		"v5YIYZah9s5XigQrygSeGU9/7KCk5Mypmn3i1tZT1T9CcZHGNVEX5FB+FugNIKrC" + "trFJ8FLw/eT00Jl69RgV1NOw29iPtmlBhCGQKrjiiuns1I2zItYpJJ5W/uCZaiin" +
		"KzktRErxzkB+lstYLQVcQVE7euwd+cP8jEq/0wzlN6YqTnKFyeRNxJUfd5maPfQB" + "qqUtvgEU69Fxsj4XXvSQOyTqusjfjNEn4TntIcarT+zEVNRUmZ2HTMAKQwIDAQAB" +
		"o4HGMIHDMB0GA1UdDgQWBBSgU2W23O/fdM5l5TNk2s1qmy+JsjCBkwYDVR0jBIGL" + "MIGIgBSeUBdceOzels7BZuHsSG2BJEA0GaFlpGMwYTELMAkGA1UEBhMCQVUxEzAR" +
		"BgNVBAgTClNvbWUtU3RhdGUxITAfBgNVBAoTGEludGVybmV0IFdpZGdpdHMgUHR5" + "IEx0ZDEaMBgGA1UEAxMRanVua2NhLmp1bmtkb21haW6CCQDEeOWBKBf56TAMBgNV" +
		"HRMEBTADAQH/MA0GCSqGSIb3DQEBBQUAA4IBAQBjzJWRsSVCyj21ZRyNLS+WxhNU" + "EBKwUEtrEgxRwCIcPgcKYj2uBBfXWmmJwlngUWMHWr+T/7wWWpfuJdLtYJy6DxEM" +
		"3NIL+m0xnPVq637eBEvnz+LgrqzjFnxskHjaJeBEko1bPxHUiz77uVxvhF+ikb94" + "SJN9G7I/caQQuOPLVg4xITbIG9hKNmYg+R+xui+xiPfsAEcqT3Nbj7oAc7Dzc9yv" +
		"6Yatj0qaJ/XzcLV+uei69oTnrsUhWzd0EAb+I93eNtGznyG9nBFxbGLFhQxopSnQ" + "gELZRm/ovyyy6+7kJKSh1x5KMfN8Nvz8JODJK5uT7djBRFd2Y31ufd7J/7D9";
	private const string JunkWwwBase64 =
		"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" + "9QICCAA=";

	private const int PortNumber = 43343;
	private const string serverMessage = "goo goo ga joob";

	[SetUp]
	public void GetReady () {
		m_serverCert = new X509Certificate2 (m_serverCertRaw, "server");			
		m_clientCert = new X509Certificate2 (m_clientCertRaw, "client");
	}

	[Test] //bug https://bugzilla.novell.com/show_bug.cgi?id=457120
	public void AuthenticateClientAndServer_ClientSendsNoData ()
	{
		AuthenticateClientAndServer (true, true);
	}

	void AuthenticateClientAndServer (bool server, bool client)
	{
		IPEndPoint endPoint = new IPEndPoint (IPAddress.Parse ("127.0.0.1"), 10000);
		ClientServerState state = new ClientServerState ();
		state.Client = new TcpClient ();
		state.Listener = new TcpListener (endPoint);
		state.Listener.Start ();
		state.ServerAuthenticated = new AutoResetEvent (false);
		state.ClientAuthenticated = new AutoResetEvent (false);
		state.ServerIOException = !server;
		try {
			Thread serverThread = new Thread (() => StartServerAndAuthenticate (state));
			serverThread.Start (null);
			Thread clientThread = new Thread (() => StartClientAndAuthenticate (state, endPoint));
			clientThread.Start (null);
			Assert.AreEqual (server, state.ServerAuthenticated.WaitOne (TimeSpan.FromSeconds (2)), 
				"server not authenticated");
			Assert.AreEqual (client, state.ClientAuthenticated.WaitOne (TimeSpan.FromSeconds (2)), 
				"client not authenticated");
		} finally {
			if (state.ClientStream != null)
				state.ClientStream.Dispose ();
			state.Client.Close ();
			if (state.ServerStream != null)
				state.ServerStream.Dispose ();
			if (state.ServerClient != null)
				state.ServerClient.Close ();
			state.Listener.Stop ();
		}
	}

	/// <summary>
	/// Adapted from https://github.com/rahvee/MonoSslStreamServerBug
	/// SelfCompatibilityTest is a mono-only subset of the complete mono vs .Net compatibility tests in MonoSslStreamServerBug
	/// </summary>
	[Category("NotWorking")]    // Works in .Net, doesn't work in mono.
	[Test()]
	public void SelfCompatibilityTest()
	{
		Exception[] thrownExceptions = null;

		// Mono.Security is required on the build system. Not necessarily present if the run system is not mono, but that's ok
		if (Type.GetType("Mono.Runtime") == null) {
			// Not running on mono.  Assuming .Net

			var JunkRootCert = new X509Certificate2(Convert.FromBase64String(JunkRootCABase64));
			var JunkIntermediateCert = new X509Certificate2(Convert.FromBase64String(JunkIntermediateBase64));

			CertImporter importCerts = () => {
				var store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
				store.Open(OpenFlags.ReadWrite);
				store.Add(JunkRootCert);
				store.Close();
				store = new X509Store(StoreName.CertificateAuthority, StoreLocation.CurrentUser);
				store.Open(OpenFlags.ReadWrite);
				store.Remove(JunkIntermediateCert); // If it was previously cached, we want to remove it
				store.Close();
			};
			CertRemover removeCerts = () => {
				var store = new X509Store(StoreName.CertificateAuthority, StoreLocation.CurrentUser);
				store.Open(OpenFlags.ReadWrite);
				store.Remove(JunkIntermediateCert);
				store.Close();
				store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
				store.Open(OpenFlags.ReadWrite);
				store.Remove(JunkRootCert);
				store.Close();
			};

			thrownExceptions = DoSelfCompatibilityTest(importCerts, removeCerts);
		}
		else {
			// Running on mono

			// The norm on windows is to have the root & intermediate both in the OS. On mono, the norm is just root.
			var JunkRootCert = new MonoX509Certificate(Convert.FromBase64String(JunkRootCABase64));

			CertImporter importCerts = () => {
				MonoX509StoreManager.CurrentUser.TrustedRoot.Import(JunkRootCert);
				MonoX509StoreManager.CurrentUser.TrustedRoot.Clear();   // refresh from disk after changes were made above
			};
			CertRemover removeCerts = () => {
				MonoX509StoreManager.CurrentUser.TrustedRoot.Remove(JunkRootCert);
				MonoX509StoreManager.CurrentUser.TrustedRoot.Clear();   // refresh from disk after changes were made above
			};

			thrownExceptions = DoSelfCompatibilityTest(importCerts, removeCerts);
		}

		/* If there is a failure, I wonder if there's a better way to capture it?
		using (var fs = new global::System.IO.StreamWriter(new global::System.IO.FileStream("/path/to/some/out.txt",global::System.IO.FileMode.Append))) {
			foreach(var ex in thrownExceptions) {
				fs.Write(ex.ToString());
				fs.WriteLine("\n-----------------------");
			}
		}
		*/
		Assert.IsNotNull(thrownExceptions, "SslStreamTest.SelfCompatibilityTest Null Result Failure");
		Assert.AreEqual(expected: 0, actual: thrownExceptions.Length, message: "SslStreamTest.SelfCompatibilityTest Caught Exceptions Failure");
	}
	private delegate void CertImporter();
	private delegate void CertRemover();
	private static Exception[] DoSelfCompatibilityTest ( CertImporter importCerts, CertRemover removeCerts ) {
		var thrownExceptions = new List<Exception>();
		SelfContainedServer myServer = null;

		importCerts();
		try {
			using (var are = new AutoResetEvent(false)) {
				myServer = new SelfContainedServer(are);   // This never excepts, so it's safe to assume myServer got set to non-null
				are.WaitOne();    // Give the server a chance to start, before starting client
				Thread.Sleep(1);  // Give the server a chance to start, before starting client
				DoClient();
			}
		}
		catch (Exception e) {
			thrownExceptions.Add(e);
		}

		try {
			myServer.Dispose();
			myServer.ServerThread.Join();
		}
		catch (Exception e) {
			thrownExceptions.Add(e);
		}

		removeCerts();

		if (myServer.ThrownException != null) {
			thrownExceptions.Add(myServer.ThrownException);
		}

		// Is linq available? It's easier for me to write this trivial foreach, rather than figure it out.
		var retVal = new Exception[thrownExceptions.Count];
		int i = 0;
		foreach (var ex in thrownExceptions) {
			retVal[i] = ex;
		}
		return retVal;
	}
	private static void DoClient()
	{
		TcpClient client = new TcpClient();
		client.Connect(IPAddress.Loopback, PortNumber);
		byte[] buf = new byte[4096];
		int bytesRead;
		// Un-commenting these lines will squelch certificate errors, thus causing the Test to pass
		// which proves that the certificate errors are the only errors.
		// 
		// using (var mySslStream = new SslStream(client.GetStream(), false, 
		// 	(object o, X509Certificate c, X509Chain h, SslPolicyErrors e) => { return true; })
		// 	) {
		using (var mySslStream = new SslStream(client.GetStream(), leaveInnerStreamOpen: false)) {
			// "www.junkca.junkdomain" is the name of the cert generated in junkca.sh
			mySslStream.AuthenticateAsClient(targetHost: "www.junkca.junkdomain", clientCertificates: null,
				enabledSslProtocols: SslProtocols.Tls, checkCertificateRevocation: false
				);
			bytesRead = mySslStream.Read(buf, 0, buf.Length);
		}
		string message = global::System.Text.Encoding.UTF8.GetString(buf, 0, bytesRead);
		if (message != serverMessage) {
			throw new Exception("Unexpected serverMessage");
		}
	}
	private sealed class SelfContainedServer : IDisposable
	{
		public Thread ServerThread {get; set;}
		public Exception ThrownException {get; set;}
		private global::System.Timers.Timer timeoutTimer;
		public SelfContainedServer(AutoResetEvent are)
		{
			this.ServerThread = new Thread(new ThreadStart( () => { ServerThreadRun(are); } ));
			this.ServerThread.IsBackground = true;
			this.ServerThread.Start();
			this.timeoutTimer = new global::System.Timers.Timer(10000);  // 10 sec timeout
			this.timeoutTimer.AutoReset = false;
			this.timeoutTimer.Elapsed += (object sender, global::System.Timers.ElapsedEventArgs e) =>  {
				try {
					this.ServerThread.Abort();
				}
				catch
				{
				}
			};
		}
		private void ServerThreadRun(AutoResetEvent are)
		{
			try {
				var JunkWwwCert = new X509Certificate2(Convert.FromBase64String(JunkWwwBase64), "junkpass");   // "junkpass" from junkca.sh
				var tcpListener = new TcpListener(IPAddress.Loopback, PortNumber);
				try {
					tcpListener.Start();
					are.Set();
					TcpClient tcpClient = tcpListener.AcceptTcpClient();   // will be disposed upon SslStream closing
					using (var mySslStream = new SslStream(tcpClient.GetStream(), leaveInnerStreamOpen: false)) {
						mySslStream.AuthenticateAsServer(JunkWwwCert, clientCertificateRequired: false, 
							enabledSslProtocols: SslProtocols.Tls, checkCertificateRevocation: false
							);
						byte[] buf = global::System.Text.Encoding.UTF8.GetBytes(serverMessage);
						mySslStream.Write(buf, 0, buf.Length);
						mySslStream.Flush();
					}
				}
				catch (Exception e) {
					// We don't want exception to go unhandled.  Return exception back to the main thread.
					this.ThrownException = e;
				}
			}
			finally {
				try {
					are.Set();
				}
				catch
				{
					// might have been disposed, or whatever. I just need the "are.Set()"
					// in the finally clause, to guarantee I'm not creating a deadlock
				}
			}
		}
		public void Dispose()
		{
			if (this.timeoutTimer != null) {
				try {
					this.timeoutTimer.Dispose();
				}
				catch
				{
				}
				this.timeoutTimer = null;
			}
			try {
				this.ServerThread.Abort();
			}
			catch
			{
			}
		}
		~SelfContainedServer()
		{
			Dispose();
		}
	}

	[Test]
	public void ClientCipherSuitesCallback ()
	{
		try {
			ServicePointManager.ClientCipherSuitesCallback += (SecurityProtocolType p, IEnumerable<string> allCiphers) => {
				string prefix = p == SecurityProtocolType.Tls ? "TLS_" : "SSL_";
				return new List<string> { prefix + "RSA_WITH_AES_128_CBC_SHA" };
			};
			// client will only offers AES 128 - that's fine since the server support it (and many more ciphers)
			AuthenticateClientAndServer_ClientSendsNoData ();
		}
		finally {
			ServicePointManager.ClientCipherSuitesCallback = null;
		}
	}

	[Test]
	public void ServerCipherSuitesCallback ()
	{
		try {
			ServicePointManager.ServerCipherSuitesCallback += (SecurityProtocolType p, IEnumerable<string> allCiphers) => {
				string prefix = p == SecurityProtocolType.Tls ? "TLS_" : "SSL_";
					return new List<string> { prefix + "RSA_WITH_AES_256_CBC_SHA" };
			};
			// server only accept AES 256 - that's fine since the client support it (and many more ciphers)
			AuthenticateClientAndServer_ClientSendsNoData ();
		}
		finally {
			ServicePointManager.ServerCipherSuitesCallback = null;
		}
	}

	[Test]
	public void CipherSuitesCallbacks ()
	{
		try {
			ServicePointManager.ClientCipherSuitesCallback += (SecurityProtocolType p, IEnumerable<string> allCiphers) => {
				string prefix = p == SecurityProtocolType.Tls ? "TLS_" : "SSL_";
				return new List<string> { prefix + "RSA_WITH_AES_128_CBC_SHA", prefix + "RSA_WITH_AES_256_CBC_SHA" };
			};
			ServicePointManager.ServerCipherSuitesCallback += (SecurityProtocolType p, IEnumerable<string> allCiphers) => {
				string prefix = p == SecurityProtocolType.Tls ? "TLS_" : "SSL_";
				return new List<string> { prefix + "RSA_WITH_AES_128_CBC_SHA", prefix + "RSA_WITH_AES_256_CBC_SHA" };
			};
			// both client and server supports AES (128 and 256) - server will select 128 (first choice)
			AuthenticateClientAndServer_ClientSendsNoData ();
		}
		finally {
			ServicePointManager.ClientCipherSuitesCallback = null;
			ServicePointManager.ServerCipherSuitesCallback = null;
		}
	}

	[Test]
	public void MismatchedCipherSuites ()
	{
		try {
			ServicePointManager.ClientCipherSuitesCallback += (SecurityProtocolType p, IEnumerable<string> allCiphers) => {
				string prefix = p == SecurityProtocolType.Tls ? "TLS_" : "SSL_";
				return new List<string> { prefix + "RSA_WITH_AES_128_CBC_SHA" };
			};
			ServicePointManager.ServerCipherSuitesCallback += (SecurityProtocolType p, IEnumerable<string> allCiphers) => {
				string prefix = p == SecurityProtocolType.Tls ? "TLS_" : "SSL_";
				return new List<string> { prefix + "RSA_WITH_AES_256_CBC_SHA" };
			};
			// mismatch! server will refuse and send back an alert
			AuthenticateClientAndServer (false, false);
		}
		finally {
			ServicePointManager.ClientCipherSuitesCallback = null;
			ServicePointManager.ServerCipherSuitesCallback = null;
		}
	}

	private void StartClientAndAuthenticate (ClientServerState state, 
						 IPEndPoint endPoint) {
		try {
			state.Client.Connect (endPoint.Address, endPoint.Port);
			NetworkStream s = state.Client.GetStream ();
			state.ClientStream = new SslStream (s, false, 
						(a1, a2, a3, a4) => true,
						(a1, a2, a3, a4, a5) => m_clientCert);
			state.ClientStream.AuthenticateAsClient ("test_host");
			state.ClientAuthenticated.Set ();
		} catch (ObjectDisposedException) { /* this can happen when closing connection it's irrelevant for the test result*/
		} catch (IOException) {
			if (!state.ServerIOException)
				throw;
		}
	}

	private void StartServerAndAuthenticate (ClientServerState state) {
		try {
			state.ServerClient = state.Listener.AcceptTcpClient ();
			NetworkStream s = state.ServerClient.GetStream ();
			state.ServerStream = new SslStream (s, false, 
						(a1, a2, a3, a4) => true, 
						(a1, a2, a3, a4, a5) => m_serverCert);
			state.ServerStream.AuthenticateAsServer (m_serverCert);
			state.ServerAuthenticated.Set ();
		} catch (ObjectDisposedException) { /* this can happen when closing connection it's irrelevant for the test result*/
		} catch (IOException) {
			// The authentication or decryption has failed.
			// ---> Mono.Security.Protocol.Tls.TlsException: Insuficient Security
			// that's fine for MismatchedCipherSuites
			if (!state.ServerIOException)
				throw;
		}
	}
	
	private class ClientServerState {
		public TcpListener Listener { get; set; }
		public TcpClient Client { get; set; }
		public TcpClient ServerClient { get; set; }
		public SslStream ServerStream { get; set; }
		public SslStream ClientStream { get; set; }
		public AutoResetEvent ServerAuthenticated { get; set; }
		public AutoResetEvent ClientAuthenticated { get; set; }
		public bool ServerIOException { get; set; }
	}
}	
}

