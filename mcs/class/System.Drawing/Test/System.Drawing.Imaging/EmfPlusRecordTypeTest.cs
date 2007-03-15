//
// EmfPlusRecordType class unit tests
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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

using System;
using System.Drawing.Imaging;
using System.Security.Permissions;
using NUnit.Framework;

namespace MonoTests.System.Drawing.Imaging {

	[TestFixture]
	[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
	public class EmfPlusRecordTypeTest {

		[Test]
		public void EmfRecords ()
		{
			Assert.AreEqual (1, (int)EmfPlusRecordType.EmfMin, "EmfMin");
			Assert.AreEqual (1, (int)EmfPlusRecordType.EmfHeader, "EmfHeader");
			Assert.AreEqual (2, (int)EmfPlusRecordType.EmfPolyBezier, "EmfPolyBezier");
			Assert.AreEqual (3, (int)EmfPlusRecordType.EmfPolygon, "EmfPolygon");
			Assert.AreEqual (4, (int)EmfPlusRecordType.EmfPolyline, "EmfPolyline");
			Assert.AreEqual (5, (int)EmfPlusRecordType.EmfPolyBezierTo, "EmfPolyBezierTo");
			Assert.AreEqual (6, (int)EmfPlusRecordType.EmfPolyLineTo, "EmfPolyLineTo");
			Assert.AreEqual (7, (int)EmfPlusRecordType.EmfPolyPolyline, "EmfPolyPolyline");
			Assert.AreEqual (8, (int)EmfPlusRecordType.EmfPolyPolygon, "EmfPolyPolygon");
			Assert.AreEqual (9, (int)EmfPlusRecordType.EmfSetWindowExtEx, "EmfSetWindowExtEx");
			Assert.AreEqual (10, (int)EmfPlusRecordType.EmfSetWindowOrgEx, "EmfSetWindowOrgEx");
			Assert.AreEqual (11, (int)EmfPlusRecordType.EmfSetViewportExtEx, "EmfSetViewportExtEx");
			Assert.AreEqual (12, (int)EmfPlusRecordType.EmfSetViewportOrgEx, "EmfSetViewportOrgEx");
			Assert.AreEqual (13, (int)EmfPlusRecordType.EmfSetBrushOrgEx, "EmfSetBrushOrgEx");
			Assert.AreEqual (14, (int)EmfPlusRecordType.EmfEof, "EmfEof");
			Assert.AreEqual (15, (int)EmfPlusRecordType.EmfSetPixelV, "EmfSetPixelV");
			Assert.AreEqual (16, (int)EmfPlusRecordType.EmfSetMapperFlags, "EmfSetMapperFlags");
			Assert.AreEqual (17, (int)EmfPlusRecordType.EmfSetMapMode, "EmfSetMapMode");
			Assert.AreEqual (18, (int)EmfPlusRecordType.EmfSetBkMode, "EmfSetBkMode");
			Assert.AreEqual (19, (int)EmfPlusRecordType.EmfSetPolyFillMode, "EmfSetPolyFillMode");
			Assert.AreEqual (20, (int)EmfPlusRecordType.EmfSetROP2, "EmfSetROP2");
			Assert.AreEqual (21, (int)EmfPlusRecordType.EmfSetStretchBltMode, "EmfSetStretchBltMode");
			Assert.AreEqual (22, (int)EmfPlusRecordType.EmfSetTextAlign, "EmfSetTextAlign");
			Assert.AreEqual (23, (int)EmfPlusRecordType.EmfSetColorAdjustment, "EmfSetColorAdjustment");
			Assert.AreEqual (24, (int)EmfPlusRecordType.EmfSetTextColor, "EmfSetTextColor");
			Assert.AreEqual (25, (int)EmfPlusRecordType.EmfSetBkColor, "EmfSetBkColor");
			Assert.AreEqual (26, (int)EmfPlusRecordType.EmfOffsetClipRgn, "EmfOffsetClipRgn");
			Assert.AreEqual (27, (int)EmfPlusRecordType.EmfMoveToEx, "EmfMoveToEx");
			Assert.AreEqual (28, (int)EmfPlusRecordType.EmfSetMetaRgn, "EmfSetMetaRgn");
			Assert.AreEqual (29, (int)EmfPlusRecordType.EmfExcludeClipRect, "EmfExcludeClipRect");
			Assert.AreEqual (30, (int)EmfPlusRecordType.EmfIntersectClipRect, "EmfIntersectClipRect");
			Assert.AreEqual (31, (int)EmfPlusRecordType.EmfScaleViewportExtEx, "EmfScaleViewportExtEx");
			Assert.AreEqual (32, (int)EmfPlusRecordType.EmfScaleWindowExtEx, "EmfScaleWindowExtEx");
			Assert.AreEqual (33, (int)EmfPlusRecordType.EmfSaveDC, "EmfSaveDC");
			Assert.AreEqual (34, (int)EmfPlusRecordType.EmfRestoreDC, "EmfRestoreDC");
			Assert.AreEqual (35, (int)EmfPlusRecordType.EmfSetWorldTransform, "EmfSetWorldTransform");
			Assert.AreEqual (36, (int)EmfPlusRecordType.EmfModifyWorldTransform, "EmfModifyWorldTransform");
			Assert.AreEqual (37, (int)EmfPlusRecordType.EmfSelectObject, "EmfSelectObject");
			Assert.AreEqual (38, (int)EmfPlusRecordType.EmfCreatePen, "EmfCreatePen");
			Assert.AreEqual (39, (int)EmfPlusRecordType.EmfCreateBrushIndirect, "EmfCreateBrushIndirect");
			Assert.AreEqual (40, (int)EmfPlusRecordType.EmfDeleteObject, "EmfDeleteObject");
			Assert.AreEqual (41, (int)EmfPlusRecordType.EmfAngleArc, "EmfAngleArc");
			Assert.AreEqual (42, (int)EmfPlusRecordType.EmfEllipse, "EmfEllipse");
			Assert.AreEqual (43, (int)EmfPlusRecordType.EmfRectangle, "EmfRectangle");
			Assert.AreEqual (44, (int)EmfPlusRecordType.EmfRoundRect, "EmfRoundRect");
			Assert.AreEqual (45, (int)EmfPlusRecordType.EmfRoundArc, "EmfRoundArc");
			Assert.AreEqual (46, (int)EmfPlusRecordType.EmfChord, "EmfChord");
			Assert.AreEqual (47, (int)EmfPlusRecordType.EmfPie, "EmfPie");
			Assert.AreEqual (48, (int)EmfPlusRecordType.EmfSelectPalette, "EmfSelectPalette");
			Assert.AreEqual (49, (int)EmfPlusRecordType.EmfCreatePalette, "EmfCreatePalette");
			Assert.AreEqual (50, (int)EmfPlusRecordType.EmfSetPaletteEntries, "EmfSetPaletteEntries");
			Assert.AreEqual (51, (int)EmfPlusRecordType.EmfResizePalette, "EmfResizePalette");
			Assert.AreEqual (52, (int)EmfPlusRecordType.EmfRealizePalette, "EmfRealizePalette");
			Assert.AreEqual (53, (int)EmfPlusRecordType.EmfExtFloodFill, "EmfExtFloodFill");
			Assert.AreEqual (54, (int)EmfPlusRecordType.EmfLineTo, "EmfLineTo");
			Assert.AreEqual (55, (int)EmfPlusRecordType.EmfArcTo, "EmfArcTo");
			Assert.AreEqual (56, (int)EmfPlusRecordType.EmfPolyDraw, "EmfPolyDraw");
			Assert.AreEqual (57, (int)EmfPlusRecordType.EmfSetArcDirection, "EmfSetArcDirection");
			Assert.AreEqual (58, (int)EmfPlusRecordType.EmfSetMiterLimit, "EmfSetMiterLimit");
			Assert.AreEqual (59, (int)EmfPlusRecordType.EmfBeginPath, "EmfBeginPath");
			Assert.AreEqual (60, (int)EmfPlusRecordType.EmfEndPath, "EmfEndPath");
			Assert.AreEqual (61, (int)EmfPlusRecordType.EmfCloseFigure, "EmfCloseFigure");
			Assert.AreEqual (62, (int)EmfPlusRecordType.EmfFillPath, "EmfFillPath");
			Assert.AreEqual (63, (int)EmfPlusRecordType.EmfStrokeAndFillPath, "EmfStrokeAndFillPath");
			Assert.AreEqual (64, (int)EmfPlusRecordType.EmfStrokePath, "EmfStrokePath");
			Assert.AreEqual (65, (int)EmfPlusRecordType.EmfFlattenPath, "EmfFlattenPath");
			Assert.AreEqual (66, (int)EmfPlusRecordType.EmfWidenPath, "EmfWidenPath");
			Assert.AreEqual (67, (int)EmfPlusRecordType.EmfSelectClipPath, "EmfSelectClipPath");
			Assert.AreEqual (68, (int)EmfPlusRecordType.EmfAbortPath, "EmfAbortPath");
			Assert.AreEqual (69, (int)EmfPlusRecordType.EmfReserved069, "EmfReserved069");
			Assert.AreEqual (70, (int)EmfPlusRecordType.EmfGdiComment, "EmfGdiComment");
			Assert.AreEqual (71, (int)EmfPlusRecordType.EmfFillRgn, "EmfFillRgn");
			Assert.AreEqual (72, (int)EmfPlusRecordType.EmfFrameRgn, "EmfFrameRgn");
			Assert.AreEqual (73, (int)EmfPlusRecordType.EmfInvertRgn, "EmfInvertRgn");
			Assert.AreEqual (74, (int)EmfPlusRecordType.EmfPaintRgn, "EmfPaintRgn");
			Assert.AreEqual (75, (int)EmfPlusRecordType.EmfExtSelectClipRgn, "EmfExtSelectClipRgn");
			Assert.AreEqual (76, (int)EmfPlusRecordType.EmfBitBlt, "EmfBitBlt");
			Assert.AreEqual (77, (int)EmfPlusRecordType.EmfStretchBlt, "EmfStretchBlt");
			Assert.AreEqual (78, (int)EmfPlusRecordType.EmfMaskBlt, "EmfMaskBlt");
			Assert.AreEqual (79, (int)EmfPlusRecordType.EmfPlgBlt, "EmfPlgBlt");
			Assert.AreEqual (80, (int)EmfPlusRecordType.EmfSetDIBitsToDevice, "EmfSetDIBitsToDevice");
			Assert.AreEqual (81, (int)EmfPlusRecordType.EmfStretchDIBits, "EmfStretchDIBits");
			Assert.AreEqual (82, (int)EmfPlusRecordType.EmfExtCreateFontIndirect, "EmfExtCreateFontIndirect");
			Assert.AreEqual (83, (int)EmfPlusRecordType.EmfExtTextOutA, "EmfExtTextOutA");
			Assert.AreEqual (84, (int)EmfPlusRecordType.EmfExtTextOutW, "EmfExtTextOutW");
			Assert.AreEqual (85, (int)EmfPlusRecordType.EmfPolyBezier16, "EmfPolyBezier16");
			Assert.AreEqual (86, (int)EmfPlusRecordType.EmfPolygon16, "EmfPolygon16");
			Assert.AreEqual (87, (int)EmfPlusRecordType.EmfPolyline16, "EmfPolyline16");
			Assert.AreEqual (88, (int)EmfPlusRecordType.EmfPolyBezierTo16, "EmfPolyBezierTo16");
			Assert.AreEqual (89, (int)EmfPlusRecordType.EmfPolylineTo16, "EmfPolylineTo16");
			Assert.AreEqual (90, (int)EmfPlusRecordType.EmfPolyPolyline16, "EmfPolyPolyline16");
			Assert.AreEqual (91, (int)EmfPlusRecordType.EmfPolyPolygon16, "EmfPolyPolygon16");
			Assert.AreEqual (92, (int)EmfPlusRecordType.EmfPolyDraw16, "EmfPolyDraw16");
			Assert.AreEqual (93, (int)EmfPlusRecordType.EmfCreateMonoBrush, "EmfCreateMonoBrush");
			Assert.AreEqual (94, (int)EmfPlusRecordType.EmfCreateDibPatternBrushPt, "EmfCreateDibPatternBrushPt");
			Assert.AreEqual (95, (int)EmfPlusRecordType.EmfExtCreatePen, "EmfExtCreatePen");
			Assert.AreEqual (96, (int)EmfPlusRecordType.EmfPolyTextOutA, "EmfPolyTextOutA");
			Assert.AreEqual (97, (int)EmfPlusRecordType.EmfPolyTextOutW, "EmfPolyTextOutW");
			Assert.AreEqual (98, (int)EmfPlusRecordType.EmfSetIcmMode, "EmfSetIcmMode");
			Assert.AreEqual (99, (int)EmfPlusRecordType.EmfCreateColorSpace, "EmfCreateColorSpace");
			Assert.AreEqual (100, (int)EmfPlusRecordType.EmfSetColorSpace, "EmfSetColorSpace");
			Assert.AreEqual (101, (int)EmfPlusRecordType.EmfDeleteColorSpace, "EmfDeleteColorSpace");
			Assert.AreEqual (102, (int)EmfPlusRecordType.EmfGlsRecord, "EmfGlsRecord");
			Assert.AreEqual (103, (int)EmfPlusRecordType.EmfGlsBoundedRecord, "EmfGlsBoundedRecord");
			Assert.AreEqual (104, (int)EmfPlusRecordType.EmfPixelFormat, "EmfPixelFormat");
			Assert.AreEqual (105, (int)EmfPlusRecordType.EmfDrawEscape, "EmfDrawEscape");
			Assert.AreEqual (106, (int)EmfPlusRecordType.EmfExtEscape, "EmfExtEscape");
			Assert.AreEqual (107, (int)EmfPlusRecordType.EmfStartDoc, "EmfStartDoc");
			Assert.AreEqual (108, (int)EmfPlusRecordType.EmfSmallTextOut, "EmfSmallTextOut");
			Assert.AreEqual (109, (int)EmfPlusRecordType.EmfForceUfiMapping, "EmfForceUfiMapping");
			Assert.AreEqual (110, (int)EmfPlusRecordType.EmfNamedEscpae, "EmfNamedEscpae");
			Assert.AreEqual (111, (int)EmfPlusRecordType.EmfColorCorrectPalette, "EmfColorCorrectPalette");
			Assert.AreEqual (112, (int)EmfPlusRecordType.EmfSetIcmProfileA, "EmfSetIcmProfileA");
			Assert.AreEqual (113, (int)EmfPlusRecordType.EmfSetIcmProfileW, "EmfSetIcmProfileW");
			Assert.AreEqual (114, (int)EmfPlusRecordType.EmfAlphaBlend, "EmfAlphaBlend");
			Assert.AreEqual (115, (int)EmfPlusRecordType.EmfSetLayout, "EmfSetLayout");
			Assert.AreEqual (116, (int)EmfPlusRecordType.EmfTransparentBlt, "EmfTransparentBlt");
			Assert.AreEqual (117, (int)EmfPlusRecordType.EmfReserved117, "EmfReserved117");
			Assert.AreEqual (118, (int)EmfPlusRecordType.EmfGradientFill, "EmfGradientFill");
			Assert.AreEqual (119, (int)EmfPlusRecordType.EmfSetLinkedUfis, "EmfSetLinkedUfis");
			Assert.AreEqual (120, (int)EmfPlusRecordType.EmfSetTextJustification, "EmfSetTextJustification");
			Assert.AreEqual (121, (int)EmfPlusRecordType.EmfColorMatchToTargetW, "EmfColorMatchToTargetW");
			Assert.AreEqual (122, (int)EmfPlusRecordType.EmfCreateColorSpaceW, "EmfCreateColorSpaceW");
			Assert.AreEqual (122, (int)EmfPlusRecordType.EmfMax, "EmfMax");
		}

		[Test]
		public void EmfPlusRecords ()
		{
			Assert.AreEqual (16384, (int)EmfPlusRecordType.EmfPlusRecordBase, "EmfPlusRecordBase");
			Assert.AreEqual (16384, (int)EmfPlusRecordType.Invalid, "Invalid");
			Assert.AreEqual (16385, (int)EmfPlusRecordType.Min, "Min");
			Assert.AreEqual (16385, (int)EmfPlusRecordType.Header, "Header");
			Assert.AreEqual (16386, (int)EmfPlusRecordType.EndOfFile, "EndOfFile");
			Assert.AreEqual (16387, (int)EmfPlusRecordType.Comment, "Comment");
			Assert.AreEqual (16388, (int)EmfPlusRecordType.GetDC, "GetDC");
			Assert.AreEqual (16389, (int)EmfPlusRecordType.MultiFormatStart, "MultiFormatStart");
			Assert.AreEqual (16390, (int)EmfPlusRecordType.MultiFormatSection, "MultiFormatSection");
			Assert.AreEqual (16391, (int)EmfPlusRecordType.MultiFormatEnd, "MultiFormatEnd");
			Assert.AreEqual (16392, (int)EmfPlusRecordType.Object, "Object");
			Assert.AreEqual (16393, (int)EmfPlusRecordType.Clear, "Clear");
			Assert.AreEqual (16394, (int)EmfPlusRecordType.FillRects, "FillRects");
			Assert.AreEqual (16395, (int)EmfPlusRecordType.DrawRects, "DrawRects");
			Assert.AreEqual (16396, (int)EmfPlusRecordType.FillPolygon, "FillPolygon");
			Assert.AreEqual (16397, (int)EmfPlusRecordType.DrawLines, "DrawLines");
			Assert.AreEqual (16398, (int)EmfPlusRecordType.FillEllipse, "FillEllipse");
			Assert.AreEqual (16399, (int)EmfPlusRecordType.DrawEllipse, "DrawEllipse");
			Assert.AreEqual (16400, (int)EmfPlusRecordType.FillPie, "FillPie");
			Assert.AreEqual (16401, (int)EmfPlusRecordType.DrawPie, "DrawPie");
			Assert.AreEqual (16402, (int)EmfPlusRecordType.DrawArc, "DrawArc");
			Assert.AreEqual (16403, (int)EmfPlusRecordType.FillRegion, "FillRegion");
			Assert.AreEqual (16404, (int)EmfPlusRecordType.FillPath, "FillPath");
			Assert.AreEqual (16405, (int)EmfPlusRecordType.DrawPath, "DrawPath");
			Assert.AreEqual (16406, (int)EmfPlusRecordType.FillClosedCurve, "FillClosedCurve");
			Assert.AreEqual (16407, (int)EmfPlusRecordType.DrawClosedCurve, "DrawClosedCurve");
			Assert.AreEqual (16408, (int)EmfPlusRecordType.DrawCurve, "DrawCurve");
			Assert.AreEqual (16409, (int)EmfPlusRecordType.DrawBeziers, "DrawBeziers");
			Assert.AreEqual (16410, (int)EmfPlusRecordType.DrawImage, "DrawImage");
			Assert.AreEqual (16411, (int)EmfPlusRecordType.DrawImagePoints, "DrawImagePoints");
			Assert.AreEqual (16412, (int)EmfPlusRecordType.DrawString, "DrawString");
			Assert.AreEqual (16413, (int)EmfPlusRecordType.SetRenderingOrigin, "SetRenderingOrigin");
			Assert.AreEqual (16414, (int)EmfPlusRecordType.SetAntiAliasMode, "SetAntiAliasMode");
			Assert.AreEqual (16415, (int)EmfPlusRecordType.SetTextRenderingHint, "SetTextRenderingHint");
			Assert.AreEqual (16416, (int)EmfPlusRecordType.SetTextContrast, "SetTextContrast");
			Assert.AreEqual (16417, (int)EmfPlusRecordType.SetInterpolationMode, "SetInterpolationMode");
			Assert.AreEqual (16418, (int)EmfPlusRecordType.SetPixelOffsetMode, "SetPixelOffsetMode");
			Assert.AreEqual (16419, (int)EmfPlusRecordType.SetCompositingMode, "SetCompositingMode");
			Assert.AreEqual (16420, (int)EmfPlusRecordType.SetCompositingQuality, "SetCompositingQuality");
			Assert.AreEqual (16421, (int)EmfPlusRecordType.Save, "Save");
			Assert.AreEqual (16422, (int)EmfPlusRecordType.Restore, "Restore");
			Assert.AreEqual (16423, (int)EmfPlusRecordType.BeginContainer, "BeginContainer");
			Assert.AreEqual (16424, (int)EmfPlusRecordType.BeginContainerNoParams, "BeginContainerNoParams");
			Assert.AreEqual (16425, (int)EmfPlusRecordType.EndContainer, "EndContainer");
			Assert.AreEqual (16426, (int)EmfPlusRecordType.SetWorldTransform, "SetWorldTransform");
			Assert.AreEqual (16427, (int)EmfPlusRecordType.ResetWorldTransform, "ResetWorldTransform");
			Assert.AreEqual (16428, (int)EmfPlusRecordType.MultiplyWorldTransform, "MultiplyWorldTransform");
			Assert.AreEqual (16429, (int)EmfPlusRecordType.TranslateWorldTransform, "TranslateWorldTransform");
			Assert.AreEqual (16430, (int)EmfPlusRecordType.ScaleWorldTransform, "ScaleWorldTransform");
			Assert.AreEqual (16431, (int)EmfPlusRecordType.RotateWorldTransform, "RotateWorldTransform");
			Assert.AreEqual (16432, (int)EmfPlusRecordType.SetPageTransform, "SetPageTransform");
			Assert.AreEqual (16433, (int)EmfPlusRecordType.ResetClip, "ResetClip");
			Assert.AreEqual (16434, (int)EmfPlusRecordType.SetClipRect, "SetClipRect");
			Assert.AreEqual (16435, (int)EmfPlusRecordType.SetClipPath, "SetClipPath");
			Assert.AreEqual (16436, (int)EmfPlusRecordType.SetClipRegion, "SetClipRegion");
			Assert.AreEqual (16437, (int)EmfPlusRecordType.OffsetClip, "OffsetClip");
			Assert.AreEqual (16438, (int)EmfPlusRecordType.DrawDriverString, "DrawDriverString");
			Assert.AreEqual (16438, (int)EmfPlusRecordType.Max, "Max");
			Assert.AreEqual (16439, (int)EmfPlusRecordType.Total, "Total");
		}

		[Test]
		public void WmfRecords ()
		{
			Assert.AreEqual (65536, (int)EmfPlusRecordType.WmfRecordBase, "WmfRecordBase");
			Assert.AreEqual (65566, (int)EmfPlusRecordType.WmfSaveDC, "WmfSaveDC");
			Assert.AreEqual (65589, (int)EmfPlusRecordType.WmfRealizePalette, "WmfRealizePalette");
			Assert.AreEqual (65591, (int)EmfPlusRecordType.WmfSetPalEntries, "WmfSetPalEntries");
			Assert.AreEqual (65783, (int)EmfPlusRecordType.WmfCreatePalette, "WmfCreatePalette");
			Assert.AreEqual (65794, (int)EmfPlusRecordType.WmfSetBkMode, "WmfSetBkMode");
			Assert.AreEqual (65795, (int)EmfPlusRecordType.WmfSetMapMode, "WmfSetMapMode");
			Assert.AreEqual (65796, (int)EmfPlusRecordType.WmfSetROP2, "WmfSetROP2");
			Assert.AreEqual (65797, (int)EmfPlusRecordType.WmfSetRelAbs, "WmfSetRelAbs");
			Assert.AreEqual (65798, (int)EmfPlusRecordType.WmfSetPolyFillMode, "WmfSetPolyFillMode");
			Assert.AreEqual (65799, (int)EmfPlusRecordType.WmfSetStretchBltMode, "WmfSetStretchBltMode");
			Assert.AreEqual (65800, (int)EmfPlusRecordType.WmfSetTextCharExtra, "WmfSetTextCharExtra");
			Assert.AreEqual (65831, (int)EmfPlusRecordType.WmfRestoreDC, "WmfRestoreDC");
			Assert.AreEqual (65834, (int)EmfPlusRecordType.WmfInvertRegion, "WmfInvertRegion");
			Assert.AreEqual (65835, (int)EmfPlusRecordType.WmfPaintRegion, "WmfPaintRegion");
			Assert.AreEqual (65836, (int)EmfPlusRecordType.WmfSelectClipRegion, "WmfSelectClipRegion");
			Assert.AreEqual (65837, (int)EmfPlusRecordType.WmfSelectObject, "WmfSelectObject");
			Assert.AreEqual (65838, (int)EmfPlusRecordType.WmfSetTextAlign, "WmfSetTextAlign");
			Assert.AreEqual (65849, (int)EmfPlusRecordType.WmfResizePalette, "WmfResizePalette");
			Assert.AreEqual (65858, (int)EmfPlusRecordType.WmfDibCreatePatternBrush, "WmfDibCreatePatternBrush");
			Assert.AreEqual (65865, (int)EmfPlusRecordType.WmfSetLayout, "WmfSetLayout");
			Assert.AreEqual (66032, (int)EmfPlusRecordType.WmfDeleteObject, "WmfDeleteObject");
			Assert.AreEqual (66041, (int)EmfPlusRecordType.WmfCreatePatternBrush, "WmfCreatePatternBrush");
			Assert.AreEqual (66049, (int)EmfPlusRecordType.WmfSetBkColor, "WmfSetBkColor");
			Assert.AreEqual (66057, (int)EmfPlusRecordType.WmfSetTextColor, "WmfSetTextColor");
			Assert.AreEqual (66058, (int)EmfPlusRecordType.WmfSetTextJustification, "WmfSetTextJustification");
			Assert.AreEqual (66059, (int)EmfPlusRecordType.WmfSetWindowOrg, "WmfSetWindowOrg");
			Assert.AreEqual (66060, (int)EmfPlusRecordType.WmfSetWindowExt, "WmfSetWindowExt");
			Assert.AreEqual (66061, (int)EmfPlusRecordType.WmfSetViewportOrg, "WmfSetViewportOrg");
			Assert.AreEqual (66062, (int)EmfPlusRecordType.WmfSetViewportExt, "WmfSetViewportExt");
			Assert.AreEqual (66063, (int)EmfPlusRecordType.WmfOffsetWindowOrg, "WmfOffsetWindowOrg");
			Assert.AreEqual (66065, (int)EmfPlusRecordType.WmfOffsetViewportOrg, "WmfOffsetViewportOrg");
			Assert.AreEqual (66067, (int)EmfPlusRecordType.WmfLineTo, "WmfLineTo");
			Assert.AreEqual (66068, (int)EmfPlusRecordType.WmfMoveTo, "WmfMoveTo");
			Assert.AreEqual (66080, (int)EmfPlusRecordType.WmfOffsetCilpRgn, "WmfOffsetCilpRgn");
			Assert.AreEqual (66088, (int)EmfPlusRecordType.WmfFillRegion, "WmfFillRegion");
			Assert.AreEqual (66097, (int)EmfPlusRecordType.WmfSetMapperFlags, "WmfSetMapperFlags");
			Assert.AreEqual (66100, (int)EmfPlusRecordType.WmfSelectPalette, "WmfSelectPalette");
			Assert.AreEqual (66298, (int)EmfPlusRecordType.WmfCreatePenIndirect, "WmfCreatePenIndirect");
			Assert.AreEqual (66299, (int)EmfPlusRecordType.WmfCreateFontIndirect, "WmfCreateFontIndirect");
			Assert.AreEqual (66300, (int)EmfPlusRecordType.WmfCreateBrushIndirect, "WmfCreateBrushIndirect");
			Assert.AreEqual (66340, (int)EmfPlusRecordType.WmfPolygon, "WmfPolygon");
			Assert.AreEqual (66341, (int)EmfPlusRecordType.WmfPolyline, "WmfPolyline");
			Assert.AreEqual (66576, (int)EmfPlusRecordType.WmfScaleWindowExt, "WmfScaleWindowExt");
			Assert.AreEqual (66578, (int)EmfPlusRecordType.WmfScaleViewportExt, "WmfScaleViewportExt");
			Assert.AreEqual (66581, (int)EmfPlusRecordType.WmfExcludeClipRect, "WmfExcludeClipRect");
			Assert.AreEqual (66582, (int)EmfPlusRecordType.WmfIntersectClipRect, "WmfIntersectClipRect");
			Assert.AreEqual (66584, (int)EmfPlusRecordType.WmfEllipse, "WmfEllipse");
			Assert.AreEqual (66585, (int)EmfPlusRecordType.WmfFloodFill, "WmfFloodFill");
			Assert.AreEqual (66587, (int)EmfPlusRecordType.WmfRectangle, "WmfRectangle");
			Assert.AreEqual (66591, (int)EmfPlusRecordType.WmfSetPixel, "WmfSetPixel");
			Assert.AreEqual (66601, (int)EmfPlusRecordType.WmfFrameRegion, "WmfFrameRegion");
			Assert.AreEqual (66614, (int)EmfPlusRecordType.WmfAnimatePalette, "WmfAnimatePalette");
			Assert.AreEqual (66849, (int)EmfPlusRecordType.WmfTextOut, "WmfTextOut");
			Assert.AreEqual (66872, (int)EmfPlusRecordType.WmfPolyPolygon, "WmfPolyPolygon");
			Assert.AreEqual (66888, (int)EmfPlusRecordType.WmfExtFloodFill, "WmfExtFloodFill");
			Assert.AreEqual (67100, (int)EmfPlusRecordType.WmfRoundRect, "WmfRoundRect");
			Assert.AreEqual (67101, (int)EmfPlusRecordType.WmfPatBlt, "WmfPatBlt");
			Assert.AreEqual (67110, (int)EmfPlusRecordType.WmfEscape, "WmfEscape");
			Assert.AreEqual (67327, (int)EmfPlusRecordType.WmfCreateRegion, "WmfCreateRegion");
			Assert.AreEqual (67607, (int)EmfPlusRecordType.WmfArc, "WmfArc");
			Assert.AreEqual (67610, (int)EmfPlusRecordType.WmfPie, "WmfPie");
			Assert.AreEqual (67632, (int)EmfPlusRecordType.WmfChord, "WmfChord");
			Assert.AreEqual (67874, (int)EmfPlusRecordType.WmfBitBlt, "WmfBitBlt");
			Assert.AreEqual (67904, (int)EmfPlusRecordType.WmfDibBitBlt, "WmfDibBitBlt");
			Assert.AreEqual (68146, (int)EmfPlusRecordType.WmfExtTextOut, "WmfExtTextOut");
			Assert.AreEqual (68387, (int)EmfPlusRecordType.WmfStretchBlt, "WmfStretchBlt");
			Assert.AreEqual (68417, (int)EmfPlusRecordType.WmfDibStretchBlt, "WmfDibStretchBlt");
			Assert.AreEqual (68915, (int)EmfPlusRecordType.WmfSetDibToDev, "WmfSetDibToDev");
			Assert.AreEqual (69443, (int)EmfPlusRecordType.WmfStretchDib, "WmfStretchDib");
		}
	}
}
