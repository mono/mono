//
// System.Drawing.Drawing2D.Matrix.cs
//
// Author:
//   Stefan Maierhofer <sm@cg.tuwien.ac.at>
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System.Drawing.Drawing2D
{
   
    public enum CombineMode
    {
        Complement,
        Exclude,
        Intersect,
        Replace,
        Union,
        Xor
    }
    
    public enum CompositingMode
    {
        SourceCopy,
        SourceOver
    }
    
    public enum CompositingQuality
    {
        AssumeLinear,
        Default,
        GammaCorrected,
        HighQuality,
        HighSpeed,
        Invalid
    }
    
    public enum CoordinateSpace
    {
        Device,
        Page,
        World
    }
    
    public enum DashCap
    {
        Flat,
        Round,
        Triangle
    }
    
    public enum DashStyle
    {
        Custom,
        Dash,
        DashDot,
        DashDotDot,
        Dot,
        Solid
    }
    
    public enum FillMode
    {
        Alternate,
        Winding
    }
    
    public enum FlushIntention
    {
        Flush,
        Sync
    }
    
    public enum HatchStyle
    {
        BackwardDiagonal,
        Cross,
        DarkDownwardDiagonal,
        DarkHorizontal,
        DarkUpwardDiagonal,
        DarkVertical,
        DashedDownwardDiagonal,
        DashedHorizontal,
        DashedUpwardDiagonal,
        DashedVertical,
        DiagonalBrick,
        DiagonalCross,
        Divot,
        DottedDiamond,
        DottedGrid,
        ForwardDiagonal,
        Horizontal,
        HorizontalBrick,
        LargeCheckerBoard,
        LargeConfetti,
        LargeGrid,
        LightDownwardDiagonal,
        LightHorizontal,
        LightUpwardDiagonal,
        LightVertical,
        Max,
        Min,
        NarrowHorizontal,
        NarrowVertical,
        OutlinedDiamond,
        Percent05,
        Percent10,
        Percent20,
        Percent25,
        Percent30,
        Percent40,
        Percent50,
        Percent60,
        Percent70,
        Percent75,
        Percent80,
        Percent90,
        Plaid,
        Shingle,
        SmallCheckerBoard,
        SmallConfetti,
        SmallGrid,
        SolidDiamond,
        Sphere,
        Trellis,
        Vertical,
        Wave,
        Weave,
        WideDownwardDiagonal,
        WideUpwardDiagonal,
        ZigZag
    }
    
    public enum InterpolationMode
    {
        Bicubic,
        Bilinear,
        Default,
        High,
        HighQualityBicubic,
        HighQualityBilinear,
        Invalid,
        Low,
        NearestNeighbour
    }
    
    public enum LinearGradientMode
    {
        BackwardDiagonal,
        ForwardDiagonal,
        Horizontal,
        Vertical
    }
    
    public enum LineCap
    {
        AnchorMask,
        ArrowAnchor,
        Custom,
        DiamondAnchor,
        Flat,
        NoAnchor,
        Round,
        RoundAnchor,
        Square,
        SquareAnchor,
        Triangle
    }
    
    public enum LineJoin
    {
        Bevel,
        Miter,
        MiterClipped,
        Round
    }
    
    public enum MatrixOrder
    {
        Append,
        Prepend
    }
    
    public enum PathPointType
    {
        Bezier,
        Bezier3,
        CloseSubpath,
        DashMode,
        Line,
        PathMarker,
        PathTypeMask,
        Start
    }
    
    public enum PenAlignment
    {
        Center,
        Inset,
        Left,
        Outset,
        Right
    }
    
    public enum PenType
    {
        HatchFill,
        LinearGradient,
        PathGradient,
        SolidColor,
        TextureFill
    }
    
    public enum PixelOffsetMode
    {
        Default,
        Half,
        HighQuality,
        HighSpeed,
        Invalid,
        None
    }
    
    public enum QualityMode
    {
        Default,
        Hight,
        Invalid,
        Low
    }
    
    public enum SmoothingMode
    {
        AntiAlias,
        Default,
        HighQuality,
        HighSpeed,
        Invalid,
        None
    }
    
    public enum WarpMode
    {
        Bilinear,
        Perspective
    }
    
    public enum WrapMode
    {
        Clamp,
        Tile,
        TileFlipX,
        TileFlipXY,
        TileFlipY
    }
    
}
