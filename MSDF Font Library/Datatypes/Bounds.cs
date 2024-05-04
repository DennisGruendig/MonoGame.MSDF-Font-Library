namespace MSDF_Font_Library.Datatypes
{
    public struct Bounds
    {
        public Bounds(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }
        public Bounds(BoundsD bounds)
        {
            Left = (int)bounds.Left;
            Top = (int)bounds.Top;
            Right = (int)bounds.Right;
            Bottom = (int)bounds.Bottom;
        }

        public int Left { get; set; }
        public int Top { get; set; }
        public int Right { get; set; }
        public int Bottom { get; set; }
    }
}
