using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MSDF_Font_Library.Content;
using MSDF_Font_Library.Datatypes;
using MSDF_Font_Library.FontAtlas;
using System;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace MSDF_Font_Library.Rendering
{
    public class FieldBatch : IDisposable
    {
        private const int InitialBatchSize = 256;
        private const int MaxBatchSize = 5461;
        private readonly GraphicsDevice _device;
        private readonly Effect _effect;
        private bool _isDisposed;
        private bool _beginCalled;

        private SpriteSortMode _sortMode;
        private BlendState _blendState;
        private SamplerState _samplerState;
        private DepthStencilState _depthStencilState;
        private RasterizerState _rasterizerState;

        private int _batchItemCount;
        private uint[] _indexArray;
        private FieldVertex[] _vertexArray;
        private FieldBatchItem[] _batchItemArray;

        DynamicVertexBuffer? _vertexBuffer;
        IndexBuffer? _indexBuffer;

        public FieldBatch(GraphicsDevice graphicsDevice, Effect effect, int batchCapacity = 0)
        {
            if (graphicsDevice is null)
                throw new ArgumentNullException(nameof(GraphicsDevice), "The graphics device must not be null when creating new resources.");
            if (effect is null)
                throw new ArgumentNullException(nameof(Effect), "The effect must not be null when creating new resources.");

            _device = graphicsDevice;
            _effect = effect;

            _blendState = BlendState.AlphaBlend;
            _samplerState = SamplerState.AnisotropicClamp;
            _depthStencilState = DepthStencilState.None;
            _rasterizerState = RasterizerState.CullCounterClockwise;

            _indexArray = Array.Empty<uint>();
            _vertexArray = Array.Empty<FieldVertex>();
            _batchItemArray = Array.Empty<FieldBatchItem>();

            CheckArraySize(batchCapacity);
        }

        private void CheckArraySize(int batchCapacity)
        {
            if (batchCapacity > MaxBatchSize)
                throw new Exception("Maximum batch size has been exceeded.");

            batchCapacity = (batchCapacity > 0) ? ((batchCapacity + 63) & -64) : InitialBatchSize;
            if (6 * batchCapacity > _indexArray.Length)
            {
                uint[] array = new uint[6 * batchCapacity];
                _indexArray.CopyTo(array, 0);
                for (int i = _indexArray.Length; i < batchCapacity; i++)
                {
                    array[i * 6 + 0] = (uint)(i * 4 + 0);
                    array[i * 6 + 1] = (uint)(i * 4 + 1);
                    array[i * 6 + 2] = (uint)(i * 4 + 2);
                    array[i * 6 + 3] = (uint)(i * 4 + 1);
                    array[i * 6 + 4] = (uint)(i * 4 + 3);
                    array[i * 6 + 5] = (uint)(i * 4 + 2);
                }
                _indexArray = array;
                _vertexArray = new FieldVertex[4 * batchCapacity];
                _batchItemArray = new FieldBatchItem[batchCapacity];
                for (int i = 0; i < _batchItemArray.Length; i++)
                {
                    _batchItemArray[i] = new();
                }
            }
        }

        public void Begin(SpriteSortMode sortMode = SpriteSortMode.Deferred, BlendState? blendState = null, SamplerState? samplerState = null, DepthStencilState? depthStencilState = null, RasterizerState? rasterizerState = null, Matrix? transformMatrix = null)
        {
            if (_beginCalled)
                throw new InvalidOperationException("Begin cannot be called again until End has been successfully called.");

            _sortMode = sortMode;
            _blendState = blendState ?? _blendState;
            _samplerState = samplerState ?? _samplerState;
            _depthStencilState = depthStencilState ?? _depthStencilState;
            _rasterizerState = rasterizerState ?? _rasterizerState;
            _effect.Parameters["WorldViewProjection"].SetValue(Matrix.CreateTranslation(-0.5f, -0.5f, 0) * Matrix.CreateOrthographicOffCenter(0, _device.Viewport.Width, _device.Viewport.Height, 0, 0, 1));

            if (sortMode == SpriteSortMode.Immediate)
                DeviceSetup();

            _beginCalled = true;
        }

        public void End()
        {
            if (!_beginCalled)
                throw new InvalidOperationException("Begin must be called before calling End.");

            if (_sortMode != SpriteSortMode.Immediate)
                DeviceSetup();

            DrawBatch();
            _beginCalled = false;
        }

        public void DrawString(FieldFont font, string text, Vector2 position, Color color, float height = 10f, TextAlignment alignment = TextAlignment.BaseLeft, WrapBehaviour wrap = WrapBehaviour.Default, float wrapWidth = 0, bool smoothing = false)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            float scale = font.GetScale(height);
            TextLine[] lines = font.WrapString(text, scale, wrap, wrapWidth);
            if (lines.Length == 0) return;
            Vector2 size = new(lines[0].Width, lines.Length * font.ActualHeight);
            int indexStart = _batchItemCount;
            float offsetH = 0;

            switch (alignment)
            {
                default: break;

                case TextAlignment.MiddleLeft:
                case TextAlignment.MiddleCenter:
                case TextAlignment.MiddleRight:
                    position.Y -= size.Y * 0.5f * scale;
                    break;

                case TextAlignment.BaseLeft:
                case TextAlignment.BaseCenter:
                case TextAlignment.BaseRight:
                    position.Y -= font.ActualBaseLine * scale;
                    break;

                case TextAlignment.BottomLeft:
                case TextAlignment.BottomCenter:
                case TextAlignment.BottomRight:
                    position.Y -= size.Y * scale;
                    break;
            }

            for (int i = 0; i < lines.Length; i++)
            {
                if (!string.IsNullOrEmpty(lines[i].Text))
                {
                    Glyph[] glyphs = lines[i].Text.Select(x => font.GetGlyph(x)).ToArray();
                    double cursorTravel = 0;
                    bool isLast;
                    char? nextChar;
                    double advance;

                    switch (alignment)
                    {
                        default: break;

                        case TextAlignment.BaseCenter:
                        case TextAlignment.TopCenter:
                        case TextAlignment.MiddleCenter:
                        case TextAlignment.BottomCenter:
                            offsetH = -lines[i].Width * 0.5f;
                            break;

                        case TextAlignment.BaseRight:
                        case TextAlignment.TopRight:
                        case TextAlignment.MiddleRight:
                        case TextAlignment.BottomRight:
                            offsetH = -lines[i].Width;
                            break;
                    }

                    for (int j = 0; j < glyphs.Length; j++)
                    {
                        isLast = j == glyphs.Length - 1;
                        nextChar = isLast ? null : glyphs[j + 1].Character;
                        advance = (isLast ? glyphs[j].CursorBounds.Width : glyphs[j].GetAdvance(nextChar)) * scale;

                        if (!char.IsWhiteSpace(glyphs[j].Character))
                        {
                            _batchItemArray[_batchItemCount].Set(
                                ref _vertexArray, 4 * _batchItemCount,
                                (float)(position.X + offsetH + cursorTravel + glyphs[j].CursorBounds.X * scale),
                                (float)(position.Y + glyphs[j].CursorBounds.Y * scale),
                                glyphs[j].AtlasSource,
                                font, scale, color, smoothing, 0);
                            _batchItemCount++;
                        }

                        cursorTravel += advance;
                    }
                }
                position.Y += font.ActualHeight * scale;
            }
            FlushIfNeeded();
        }

        private void DrawBatch()
        {
            if (_batchItemCount == 0) return;

            if ((uint)(_sortMode - 2) <= 2u)
                Array.Sort(_batchItemArray, 0, _batchItemCount);

            _vertexBuffer?.Dispose();
            _indexBuffer?.Dispose();

            _vertexBuffer = new DynamicVertexBuffer(_device, typeof(FieldVertex), _vertexArray.Length, BufferUsage.WriteOnly);
            _indexBuffer = new IndexBuffer(_device, typeof(uint), _indexArray.Length, BufferUsage.WriteOnly);

            _indexBuffer.SetData(_indexArray);
            _vertexBuffer.SetData(_vertexArray);

            _device.SetVertexBuffer(_vertexBuffer);
            _device.Indices = _indexBuffer;

            int index = 0;
            while (index < _batchItemCount)
            {
                int start = index;
                if (_batchItemArray[index].Texture is not null)
                {
                    Texture2D lastUsedTexture = _batchItemArray[index].Texture!;
                    while (index < _batchItemCount && lastUsedTexture == _batchItemArray[index].Texture)
                    {
                        index++;
                    }
                    FlushVertexArray(start, index, lastUsedTexture);
                }
            }
            _batchItemCount = 0;
        }

        private void FlushVertexArray(int start, int end, Texture2D texture)
        {
            if (start >= end) return;

            foreach (EffectPass pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                _device.Textures[0] = texture;
                _device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, start * 6, (end - start) * 2);
            }
        }

        private void DeviceSetup()
        {
            _device.BlendState = _blendState;
            _device.DepthStencilState = _depthStencilState;
            _device.RasterizerState = _rasterizerState;
            _device.SamplerStates[0] = _samplerState;
        }

        private void FlushIfNeeded()
        {
            if (_sortMode == SpriteSortMode.Immediate)
            {
                DrawBatch();
            }
        }

        ~FieldBatch()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (_isDisposed) return;
            if (disposing)
            {
                _effect?.Dispose();
                _vertexBuffer?.Dispose();
                _indexBuffer?.Dispose();
            }
            _isDisposed = true;
        }
    }
}
