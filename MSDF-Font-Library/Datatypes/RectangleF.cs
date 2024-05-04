using Microsoft.Xna.Framework;

namespace MSDF_Font_Library.Datatypes
{
    public struct RectangleF
    {
        public RectangleF(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
        public RectangleF(Vector2 position, Vector2 size)
        {
            X = position.X;
            Y = position.Y;
            Width = size.X;
            Height = size.Y;
        }

        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }

        public readonly Vector2 Position => new Vector2(X, Y);
        public readonly Vector2 Size => new Vector2(Width, Height);

        public static RectangleF operator +(RectangleF value1, RectangleF value2)
        {
            value1.X += value2.X;
            value1.Y += value2.Y;
            value1.Width += value2.Width;
            value1.Height += value2.Height;
            return value1;
        }

        public static RectangleF operator -(RectangleF value1, RectangleF value2)
        {
            value1.X -= value2.X;
            value1.Y -= value2.Y;
            value1.Width -= value2.Width;
            value1.Height -= value2.Height;
            return value1;
        }
    }
}
