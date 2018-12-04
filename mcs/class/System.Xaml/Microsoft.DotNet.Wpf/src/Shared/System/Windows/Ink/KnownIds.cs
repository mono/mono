using System;
using System.Collections;
using System.IO;
using MS.Internal.Ink.InkSerializedFormat;

namespace System.Windows.Ink
{
    /// <summary>
    ///    <para>DrawingAttributeIds</para>
    /// </summary>
    public static class DrawingAttributeIds
    {
        /// <summary>
        ///    <para>[To be supplied.]</para>
        /// </summary>
        public static readonly Guid Color = KnownIdCache.OriginalISFIdTable[(int)KnownIdCache.OriginalISFIdIndex.ColorRef];
        /// <summary>
        /// Guid identifying the StylusTip
        /// </summary>
        public static readonly Guid StylusTip = new Guid(0x3526c731, 0xee79, 0x4988, 0xb9, 0x3e, 0x70, 0xd9, 0x2f, 0x89, 0x7, 0xed);
        /// <summary>
        /// Guid identifying the StylusTipTransform
        /// </summary>
        public static readonly Guid StylusTipTransform = new Guid(0x4b63bc16, 0x7bc4, 0x4fd2, 0x95, 0xda, 0xac, 0xff, 0x47, 0x75, 0x73, 0x2d);
        /// <summary>
        ///    <para>The height of the pen tip which affects the stroke rendering.</para>
        /// </summary>
        public static readonly Guid StylusHeight = KnownIdCache.OriginalISFIdTable[(int)KnownIdCache.OriginalISFIdIndex.StylusHeight];
        /// <summary>
        ///    <para>The width of the pen tip which affects the stroke rendering.</para>
        /// </summary>
        public static readonly Guid StylusWidth = KnownIdCache.OriginalISFIdTable[(int)KnownIdCache.OriginalISFIdIndex.StylusWidth];
        /// <summary>
        ///    <para>[To be supplied.]</para>
        /// </summary>
        public static readonly Guid DrawingFlags = KnownIdCache.OriginalISFIdTable[(int)KnownIdCache.OriginalISFIdIndex.DrawingFlags];
        /// <summary>
        /// Guid identifying IsHighlighter
        /// </summary>
        public static readonly Guid IsHighlighter = new Guid(0xce305e1a, 0xe08, 0x45e3, 0x8c, 0xdc, 0xe4, 0xb, 0xb4, 0x50, 0x6f, 0x21);
    }

    /// <summary>
    ///    <para>[To be supplied.]</para>
    /// </summary>
    internal static class KnownIds
    {
        #region Public Ids
        /// <summary>
        ///    <para>[To be supplied.]</para>
        /// </summary>
        internal static readonly Guid X = KnownIdCache.OriginalISFIdTable[(int)KnownIdCache.OriginalISFIdIndex.X];
        /// <summary>
        ///    <para>[To be supplied.]</para>
        /// </summary>
        internal static readonly Guid Y = KnownIdCache.OriginalISFIdTable[(int)KnownIdCache.OriginalISFIdIndex.Y];
        /// <summary>
        ///    <para>[To be supplied.]</para>
        /// </summary>
        internal static readonly Guid Z = KnownIdCache.OriginalISFIdTable[(int)KnownIdCache.OriginalISFIdIndex.Z];
        /// <summary>
        ///    <para>[To be supplied.]</para>
        /// </summary>
        internal static readonly Guid PacketStatus = KnownIdCache.OriginalISFIdTable[(int)KnownIdCache.OriginalISFIdIndex.PacketStatus];
        /// <summary>
        ///    <para>[To be supplied.]</para>
        /// </summary>
        internal static readonly Guid TimerTick = KnownIdCache.OriginalISFIdTable[(int)KnownIdCache.OriginalISFIdIndex.TimerTick];
        /// <summary>
        ///    <para>[To be supplied.]</para>
        /// </summary>
        internal static readonly Guid SerialNumber = KnownIdCache.OriginalISFIdTable[(int)KnownIdCache.OriginalISFIdIndex.SerialNumber];
        /// <summary>
        ///    <para>[To be supplied.]</para>
        /// </summary>
        internal static readonly Guid NormalPressure = KnownIdCache.OriginalISFIdTable[(int)KnownIdCache.OriginalISFIdIndex.NormalPressure];
        /// <summary>
        ///    <para>[To be supplied.]</para>
        /// </summary>
        internal static readonly Guid TangentPressure = KnownIdCache.OriginalISFIdTable[(int)KnownIdCache.OriginalISFIdIndex.TangentPressure];
        /// <summary>
        ///    <para>[To be supplied.]</para>
        /// </summary>
        internal static readonly Guid ButtonPressure = KnownIdCache.OriginalISFIdTable[(int)KnownIdCache.OriginalISFIdIndex.ButtonPressure];
        /// <summary>
        ///    <para>[To be supplied.]</para>
        /// </summary>
        internal static readonly Guid XTiltOrientation = KnownIdCache.OriginalISFIdTable[(int)KnownIdCache.OriginalISFIdIndex.XTiltOrientation];
        /// <summary>
        ///    <para>[To be supplied.]</para>
        /// </summary>
        internal static readonly Guid YTiltOrientation = KnownIdCache.OriginalISFIdTable[(int)KnownIdCache.OriginalISFIdIndex.YTiltOrientation];
        /// <summary>
        ///    <para>[To be supplied.]</para>
        /// </summary>
        internal static readonly Guid AzimuthOrientation = KnownIdCache.OriginalISFIdTable[(int)KnownIdCache.OriginalISFIdIndex.AzimuthOrientation];
        /// <summary>
        ///    <para>[To be supplied.]</para>
        /// </summary>
        internal static readonly Guid AltitudeOrientation = KnownIdCache.OriginalISFIdTable[(int)KnownIdCache.OriginalISFIdIndex.AltitudeOrientation];
        /// <summary>
        ///    <para>[To be supplied.]</para>
        /// </summary>
        internal static readonly Guid TwistOrientation = KnownIdCache.OriginalISFIdTable[(int)KnownIdCache.OriginalISFIdIndex.TwistOrientation];
        /// <summary>
        ///    <para>[To be supplied.]</para>
        /// </summary>
        internal static readonly Guid PitchRotation = KnownIdCache.OriginalISFIdTable[(int)KnownIdCache.OriginalISFIdIndex.PitchRotation];
        /// <summary>
        ///    <para>[To be supplied.]</para>
        /// </summary>
        internal static readonly Guid RollRotation = KnownIdCache.OriginalISFIdTable[(int)KnownIdCache.OriginalISFIdIndex.RollRotation];
        /// <summary>
        ///    <para>[To be supplied.]</para>
        /// </summary>
        internal static readonly Guid YawRotation = KnownIdCache.OriginalISFIdTable[(int)KnownIdCache.OriginalISFIdIndex.YawRotation];
        /// <summary>
        ///    <para>[To be supplied.]</para>
        /// </summary>
        internal static readonly Guid Color = KnownIdCache.OriginalISFIdTable[(int)KnownIdCache.OriginalISFIdIndex.ColorRef];
        /// <summary>
        ///    <para>[To be supplied.]</para>
        /// </summary>
        internal static readonly Guid DrawingFlags = KnownIdCache.OriginalISFIdTable[(int)KnownIdCache.OriginalISFIdIndex.DrawingFlags];
        /// <summary>
        ///    <para>[To be supplied.]</para>
        /// </summary>
        internal static readonly Guid CursorId = KnownIdCache.OriginalISFIdTable[(int)KnownIdCache.OriginalISFIdIndex.CursorId];
        /// <summary>
        ///    <para>[To be supplied.]</para>
        /// </summary>
        internal static readonly Guid WordAlternates = KnownIdCache.OriginalISFIdTable[(int)KnownIdCache.OriginalISFIdIndex.WordAlternates];
        /// <summary>
        ///    <para>[To be supplied.]</para>
        /// </summary>
        internal static readonly Guid CharacterAlternates = KnownIdCache.OriginalISFIdTable[(int)KnownIdCache.OriginalISFIdIndex.CharAlternates];
        /// <summary>
        ///    <para>[To be supplied.]</para>
        /// </summary>
        internal static readonly Guid InkMetrics = KnownIdCache.OriginalISFIdTable[(int)KnownIdCache.OriginalISFIdIndex.InkMetrics];
        /// <summary>
        ///    <para>[To be supplied.]</para>
        /// </summary>
        internal static readonly Guid GuideStructure = KnownIdCache.OriginalISFIdTable[(int)KnownIdCache.OriginalISFIdIndex.GuideStructure];
        /// <summary>
        ///    <para>[To be supplied.]</para>
        /// </summary>
        internal static readonly Guid Timestamp = KnownIdCache.OriginalISFIdTable[(int)KnownIdCache.OriginalISFIdIndex.Timestamp];
        /// <summary>
        ///    <para>[To be supplied.]</para>
        /// </summary>
        internal static readonly Guid Language = KnownIdCache.OriginalISFIdTable[(int)KnownIdCache.OriginalISFIdIndex.Language];
        /// <summary>
        ///    <para>[To be supplied.]</para>
        /// </summary>
        internal static readonly Guid Transparency = KnownIdCache.OriginalISFIdTable[(int)KnownIdCache.OriginalISFIdIndex.Transparency];
        /// <summary>
        ///    <para>[To be supplied.]</para>
        /// </summary>
        internal static readonly Guid CurveFittingError = KnownIdCache.OriginalISFIdTable[(int)KnownIdCache.OriginalISFIdIndex.CurveFittingError];
        /// <summary>
        ///    <para>[To be supplied.]</para>
        /// </summary>
        internal static readonly Guid RecognizedLattice = KnownIdCache.OriginalISFIdTable[(int)KnownIdCache.OriginalISFIdIndex.RecoLattice];
        /// <summary>
        ///    <para>[To be supplied.]</para>
        /// </summary>
        internal static readonly Guid CursorDown = KnownIdCache.OriginalISFIdTable[(int)KnownIdCache.OriginalISFIdIndex.CursorDown];
        /// <summary>
        ///    <para>[To be supplied.]</para>
        /// </summary>
        internal static readonly Guid SecondaryTipSwitch = KnownIdCache.OriginalISFIdTable[(int)KnownIdCache.OriginalISFIdIndex.SecondaryTipSwitch];
        /// <summary>
        ///    <para>[To be supplied.]</para>
        /// </summary>
        internal static readonly Guid TabletPick = KnownIdCache.OriginalISFIdTable[(int)KnownIdCache.OriginalISFIdIndex.TabletPick];
        /// <summary>
        ///    <para>[To be supplied.]</para>
        /// </summary>
        internal static readonly Guid BarrelDown = KnownIdCache.OriginalISFIdTable[(int)KnownIdCache.OriginalISFIdIndex.BarrelDown];
        /// <summary>
        ///    <para>[To be supplied.]</para>
        /// </summary>
        internal static readonly Guid RasterOperation = KnownIdCache.OriginalISFIdTable[(int)KnownIdCache.OriginalISFIdIndex.RasterOperation];

        /// <summary>
        ///    <para>The height of the pen tip which affects the stroke rendering.</para>
        /// </summary>
        internal static readonly Guid StylusHeight = KnownIdCache.OriginalISFIdTable[(int)KnownIdCache.OriginalISFIdIndex.StylusHeight];

        /// <summary>
        ///    <para>The width of the pen tip which affects the stroke rendering.</para>
        /// </summary>
        internal static readonly Guid StylusWidth = KnownIdCache.OriginalISFIdTable[(int)KnownIdCache.OriginalISFIdIndex.StylusWidth];

        /// <summary>
        /// Guid identifying the highlighter property
        /// </summary>
        internal static readonly Guid Highlighter = KnownIdCache.TabletInternalIdTable[(int)KnownIdCache.TabletInternalIdIndex.Highlighter];
        /// <summary>
        /// Guid identifying the Ink properties
        /// </summary>
        internal static readonly Guid InkProperties = KnownIdCache.TabletInternalIdTable[(int)KnownIdCache.TabletInternalIdIndex.InkProperties];
        /// <summary>
        /// Guid identifying the Ink Style bold property
        /// </summary>
        internal static readonly Guid InkStyleBold = KnownIdCache.TabletInternalIdTable[(int)KnownIdCache.TabletInternalIdIndex.InkStyleBold];
        /// <summary>
        /// Guid identifying the ink style italics property
        /// </summary>
        internal static readonly Guid InkStyleItalics = KnownIdCache.TabletInternalIdTable[(int)KnownIdCache.TabletInternalIdIndex.InkStyleItalics];
        /// <summary>
        /// Guid identifying the stroke timestamp property
        /// </summary>
        internal static readonly Guid StrokeTimestamp = KnownIdCache.TabletInternalIdTable[(int)KnownIdCache.TabletInternalIdIndex.StrokeTimestamp];
        /// <summary>
        /// Guid identifying the stroke timeid property
        /// </summary>
        internal static readonly Guid StrokeTimeId = KnownIdCache.TabletInternalIdTable[(int)KnownIdCache.TabletInternalIdIndex.StrokeTimeId];

        /// <summary>
        /// Guid identifying the StylusTip
        /// </summary>
        internal static readonly Guid StylusTip = new Guid(0x3526c731, 0xee79, 0x4988, 0xb9, 0x3e, 0x70, 0xd9, 0x2f, 0x89, 0x7, 0xed);

        /// <summary>
        /// Guid identifying the StylusTipTransform
        /// </summary>
        internal static readonly Guid StylusTipTransform = new Guid(0x4b63bc16, 0x7bc4, 0x4fd2, 0x95, 0xda, 0xac, 0xff, 0x47, 0x75, 0x73, 0x2d);


        /// <summary>
        /// Guid identifying IsHighlighter
        /// </summary>
        internal static readonly Guid IsHighlighter = new Guid(0xce305e1a, 0xe08, 0x45e3, 0x8c, 0xdc, 0xe4, 0xb, 0xb4, 0x50, 0x6f, 0x21);

//        /// <summary>
//        /// Guid used for identifying the fill-brush for rendering a stroke.
//        /// </summary>
//        public static readonly Guid FillBrush              = new Guid(0x9a547c5c, 0x1fff, 0x4987, 0x8a, 0xb6, 0xbe, 0xed, 0x75, 0xde, 0xa, 0x1d);
//
//        /// <summary>
//        /// Guid used for identifying the pen used for rendering a stroke's outline.
//        /// </summary>
//        public static readonly Guid OutlinePen             = new Guid(0x9967aea6, 0x3980, 0x4337, 0xb7, 0xc6, 0x34, 0xa, 0x33, 0x98, 0x8e, 0x6b);
//
//        /// <summary>
//        /// Guid used for identifying the blend mode used for rendering a stroke (similar to ROP in v1).
//        /// </summary>
//        public static readonly Guid BlendMode              = new Guid(0xd6993943, 0x7a84, 0x4a80, 0x84, 0x68, 0xa8, 0x3c, 0xca, 0x65, 0xb0, 0x5);
//
//        /// <summary>
//        /// Guid used for identifying StylusShape object
//        /// </summary>
//        public static readonly Guid StylusShape = new Guid(0xf998e7f8, 0x7cdb, 0x4c0e, 0xb2, 0xe2, 0x63, 0x2b, 0xca, 0x21, 0x2a, 0x7b);
        #endregion

        #region Internal Ids

        /// <summary>
        ///    <para>The style of the rendering used for the pen tip.</para>
        /// </summary>
        internal static readonly Guid PenStyle = KnownIdCache.OriginalISFIdTable[(int)KnownIdCache.OriginalISFIdIndex.PenStyle];

        /// <summary>
        ///    <para>The shape of the tip of the pen used for stroke rendering.</para>
        /// </summary>
        internal static readonly Guid PenTip = KnownIdCache.OriginalISFIdTable[(int)KnownIdCache.OriginalISFIdIndex.PenTip];

        /// <summary>
        /// Guid used for identifying the Custom Stroke
        /// </summary>
        /// <remarks>NTRAID#T2-17751-2003/11/26-stfisher: SPECISSUE: Should we hide the CustomStrokes and StrokeLattice data?</remarks>
        internal static readonly Guid InkCustomStrokes     = KnownIdCache.TabletInternalIdTable[(int)KnownIdCache.TabletInternalIdIndex.InkCustomStrokes];

        /// <summary>
        /// Guid used for identifying the Stroke Lattice
        /// </summary>
        internal static readonly Guid InkStrokeLattice     = KnownIdCache.TabletInternalIdTable[(int)KnownIdCache.TabletInternalIdIndex.InkStrokeLattice];

#if UNDO_ENABLED
        /// <summary>
        /// Guid used for identifying if an undo/event has already been handled
        /// </summary>
        /// <remarks>{053BF717-DBE7-4e52-805E-64906138FAAD}</remarks>
        internal static readonly Guid UndoEventArgsHandled = new Guid(0x53bf717, 0xdbe7, 0x4e52, 0x80, 0x5e, 0x64, 0x90, 0x61, 0x38, 0xfa, 0xad);
#endif
        #endregion

        #region Known Id Helpers
        private static System.Reflection.MemberInfo[] PublicMemberInfo = null;
        internal static string ConvertToString (Guid id)
        {

            // Assert Reflection permissions shouldn't be required since we are only accessing public members
            if (null == PublicMemberInfo)
            {
                PublicMemberInfo = typeof(KnownIds).FindMembers(System.Reflection.MemberTypes.Field,
                                                System.Reflection.BindingFlags.Static |
                                                System.Reflection.BindingFlags.GetField |
                                                System.Reflection.BindingFlags.Instance |
                                                System.Reflection.BindingFlags.Public |
                                                System.Reflection.BindingFlags.Default,
                                                null, null);
            }
            foreach (System.Reflection.MemberInfo info in PublicMemberInfo)
            {
                if ( id == (Guid)typeof(KnownIds).InvokeMember(info.Name,
                                System.Reflection.BindingFlags.Static |
                                System.Reflection.BindingFlags.GetField |
                                System.Reflection.BindingFlags.Instance |
                                System.Reflection.BindingFlags.Public |
                                System.Reflection.BindingFlags.Default,
                                null, null, new object[]{},
                                System.Globalization.CultureInfo.InvariantCulture) )
                {
                    return info.Name;
                }
            }
            return id.ToString();
        }
        #endregion
    }
}
