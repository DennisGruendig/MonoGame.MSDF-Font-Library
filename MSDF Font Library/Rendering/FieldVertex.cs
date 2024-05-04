using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MSDF_Font_Library.Rendering
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FieldVertex : IVertexType
    {
        public FieldVertex(Vector3 position, Color color, Vector2 textureCoordinate, Vector2 textureSize, float pxRange, bool smoothing = true)
        {
            Position = position;
            Color = color;
            TextureCoordinate = textureCoordinate;
            TextureSize = textureSize;
            PxRange = pxRange;
            Smoothing = smoothing ? 1f : 0f;
        }

        public Vector3 Position;
        public Color Color;
        public Vector2 TextureCoordinate;
        public Vector2 TextureSize;
        public float PxRange = 2;
        public float Smoothing = 1;

        public static readonly VertexDeclaration VertexDeclaration;
        readonly VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

        static FieldVertex()
        {
            int offset = 0;
            var elements = new VertexElement[] {
                GetVertexElement(ref offset, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                GetVertexElement(ref offset, VertexElementFormat.Color, VertexElementUsage.Color, 0),
                GetVertexElement(ref offset, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
                GetVertexElement(ref offset, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1),
                GetVertexElement(ref offset, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 2),
                GetVertexElement(ref offset, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 3),
            };
            VertexDeclaration = new VertexDeclaration(elements);
        }
        private static VertexElement GetVertexElement(ref int offset, VertexElementFormat format, VertexElementUsage usage, int usageIndex)
        {
            return new VertexElement(OffsetInline(ref offset, Offsets[format]), format, usage, usageIndex);
        }
        private static int OffsetInline(ref int value, int offset)
        {
            int old = value;
            value += offset;
            return old;
        }
        private static readonly Dictionary<VertexElementFormat, int> Offsets = new()
        {
            [VertexElementFormat.Single] = 4,
            [VertexElementFormat.Vector2] = 8,
            [VertexElementFormat.Vector3] = 12,
            [VertexElementFormat.Vector4] = 16,
            [VertexElementFormat.Color] = 4,
            [VertexElementFormat.Byte4] = 4,
            [VertexElementFormat.Short2] = 4,
            [VertexElementFormat.Short4] = 8,
            [VertexElementFormat.NormalizedShort2] = 4,
            [VertexElementFormat.NormalizedShort4] = 8,
            [VertexElementFormat.HalfVector2] = 4,
            [VertexElementFormat.HalfVector4] = 8,
        };
    }
}
