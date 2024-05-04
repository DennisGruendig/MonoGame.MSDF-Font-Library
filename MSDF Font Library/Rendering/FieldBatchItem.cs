using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MSDF_Font_Library.Content;
using MSDF_Font_Library.Datatypes;
using System;

namespace MSDF_Font_Library.Rendering
{
    internal class FieldBatchItem : IComparable<FieldBatchItem>
    {
        public float SortKey;
        public Texture2D? Texture;

        public FieldBatchItem() { SortKey = 0; }

        public void Set(ref FieldVertex[] vertexArray, int startIndex, float positionX, float positionY, RectangleF sourceRectangle, FieldFont font, float scale = 1, Color? color = null, bool smoothing = true, float depth = 0)
        {
            Vector2 texSize = font.Texture.Bounds.Size.ToVector2();
            Vector2 factor = Vector2.One / texSize;

            vertexArray[startIndex++] = new(
                new Vector3(positionX, positionY, depth), color ?? Color.Black,
                factor * new Vector2(sourceRectangle.X, sourceRectangle.Y),
                texSize, font.DistanceRange, smoothing);

            vertexArray[startIndex++] = new(
                new Vector3(positionX + sourceRectangle.Width * scale, positionY, depth), color ?? Color.Black,
                factor * new Vector2(sourceRectangle.X + sourceRectangle.Width, sourceRectangle.Y),
                texSize, font.DistanceRange, smoothing);

            vertexArray[startIndex++] = new(
                new Vector3(positionX, positionY + sourceRectangle.Height * scale, depth), color ?? Color.Black,
                factor * new Vector2(sourceRectangle.X, sourceRectangle.Y + sourceRectangle.Height),
                texSize, font.DistanceRange, smoothing);

            vertexArray[startIndex++] = new(
                new Vector3(positionX + sourceRectangle.Width * scale, positionY + sourceRectangle.Height * scale, depth), color ?? Color.Black,
                factor * new Vector2(sourceRectangle.X + sourceRectangle.Width, sourceRectangle.Y + sourceRectangle.Height),
                texSize, font.DistanceRange, smoothing);

            Texture = font.Texture;
            SortKey = 0;
        }

        public int CompareTo(FieldBatchItem? other)
        {
            return SortKey.CompareTo(other?.SortKey);
        }
    }
}
