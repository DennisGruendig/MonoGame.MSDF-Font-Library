using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MSDF_Font_Library.FontAtlas;
using MSDF_Font_Library.Datatypes;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace MSDF_Font_Library.Content
{
    public class FieldFont
    {
        [ContentSerializer] private readonly char _Fallback;
        [ContentSerializer] private readonly string _Name;
        [ContentSerializer] private readonly Dictionary<char, Glyph> _Glyphs;
        [ContentSerializer] private readonly byte[] _AtlasBitmap;
        [ContentSerializer] private readonly Vector2 _AtlasSize;
        [ContentSerializer] private readonly int _DistanceRange;
        [ContentSerializer] private readonly float _FontSize;
        [ContentSerializer] private readonly float _Height;
        [ContentSerializer] private readonly float _LineHeight;
        [ContentSerializer] private readonly float _Ascender;
        [ContentSerializer] private readonly float _Descender;
        [ContentSerializer] private readonly float _UnderlineY;
        [ContentSerializer] private readonly float _UnderlineThickness;
        [ContentSerializer] private readonly float _ActualHeight;
        [ContentSerializer] private readonly float _ActualBaseLine;
        [ContentSerializer] private readonly float _ActualLineHeight;

        [ContentSerializer] private Texture2D? _Texture;

        public FieldFont()
        {
            _Name = string.Empty;
            _AtlasBitmap = Array.Empty<byte>();
            _Glyphs = new Dictionary<char, Glyph>();
        }

        public FieldFont(string name, JsonRoot json, byte[] bitmap)
        {
            _Fallback = '';
            _Name = name;
            _AtlasBitmap = bitmap;
            _AtlasSize = new Vector2(json.Atlas.Width, json.Atlas.Height);
            _DistanceRange = json.Atlas.DistanceRange;

            _FontSize = (float)json.Atlas.Size;
            _Ascender = (float)json.Metrics.Ascender;
            _Descender = (float)json.Metrics.Descender;
            _LineHeight = (float)json.Metrics.LineHeight;
            _UnderlineY = (float)json.Metrics.UnderlineY;
            _UnderlineThickness = (float)json.Metrics.UnderlineThickness;

            _Height = Math.Abs(_Ascender) + Math.Abs(_Descender);
            _ActualHeight = _FontSize * (Math.Abs(_Ascender) + Math.Abs(_Descender));
            _ActualBaseLine = _FontSize * _Ascender;
            _ActualLineHeight = _FontSize * _LineHeight;

            if (json.IgnoreKerning)
                json.Kerning.Clear();

            _Glyphs = json.Glyphs
                .Select(x => new Glyph(this, x, json.Kerning))
                .OrderBy(x => x.Character)
                .ToDictionary(x => x.Character, x => x);
        }

        public string Name => _Name;
        public Texture2D Texture => _Texture!;
        public Vector2 AtlasSize => _AtlasSize;
        public int DistanceRange => _DistanceRange;
        public float FontSize => _FontSize;
        public float Height => _Height;
        public float LineHeight => _LineHeight;
        public float Ascender => _Ascender;
        public float Descender => _Descender;
        public float UnderlineY => _UnderlineY;
        public float UnderlineThickness => _UnderlineThickness;
        public float ActualHeight => _ActualHeight;
        public float ActualBaseLine => _ActualBaseLine;
        public float ActualLineHeight => _ActualLineHeight;

        public void Initialize(GraphicsDevice graphicsDevice)
        {
            var stream = new MemoryStream(_AtlasBitmap);
            _Texture = Texture2D.FromStream(graphicsDevice, stream);
        }

        /// <summary>
        /// Check if a character exists in this font
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        public bool CheckGlyphAvailability(char character)
        {
            return _Glyphs.TryGetValue(character, out Glyph? glyph);
        }

        /// <summary>
        /// Get the glyph data for a character
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        public Glyph GetGlyph(char character)
        {
            if (_Glyphs.TryGetValue(character, out Glyph? glyph))
                return glyph;
            if (_Glyphs.TryGetValue(_Fallback, out Glyph? glyphFallback))
                return glyphFallback;
            return new Glyph();
        }

        /// <summary>
        /// Returns the factor needed to scale the font to the input height
        /// </summary>
        /// <param name="fontHeight"></param>
        /// <returns></returns>
        public float GetScale(float fontHeight)
        {
            return 1 / FontSize * fontHeight;
        }

        /// <summary>
        /// Returns the actual line height of a relative font height
        /// </summary>
        /// <param name="fontHeight"></param>
        /// <returns></returns>
        public float GetScaledLineHeight(float fontHeight)
        {
            return 1 / FontSize * ActualLineHeight * fontHeight;
        }

        /// <summary>
        /// Measure the size of a text string
        /// </summary>
        /// <param name="text"></param>
        /// <param name="fontHeight"></param>
        /// <param name="wrap"></param>
        /// <param name="maxWidth"></param>
        public Vector2 MeasureString(string text, float fontHeight, WrapBehaviour wrap = WrapBehaviour.Default, float maxWidth = 0)
        {
            float scale = GetScale(fontHeight);
            var lines = WrapString(text, scale, wrap, maxWidth);
            return new Vector2(lines.Max(x => x.Width), lines.Length * ActualLineHeight * scale);
        }

        /// <summary>
        /// Measure the bounds rectangle of a text string
        /// </summary>
        /// <param name="position"></param>
        /// <param name="text"></param>
        /// <param name="fontHeight"></param>
        /// <param name="wrap"></param>
        /// <param name="maxWidth"></param>
        public RectangleF MeasureString(Vector2 position, string text, float fontHeight, TextAlignment alignment = TextAlignment.BaseLeft, WrapBehaviour wrap = WrapBehaviour.Default, float maxWidth = 0)
        {
            float scale = GetScale(fontHeight);
            var lines = WrapString(text, scale, wrap, maxWidth);
            Vector2 size = new(lines.Max(x => x.Width), ActualLineHeight * scale * lines.Length);

            switch (alignment)
            {
                default: break;
                case TextAlignment.MiddleLeft: 
                case TextAlignment.MiddleCenter:
                case TextAlignment.MiddleRight:
                    position.Y -= size.Y * 0.5f;
                    break;
                case TextAlignment.BaseLeft:
                case TextAlignment.BaseCenter:
                case TextAlignment.BaseRight:
                    position.Y -= ActualBaseLine * scale;
                    break;
                case TextAlignment.BottomLeft:
                case TextAlignment.BottomCenter:
                case TextAlignment.BottomRight:
                    position.Y -= size.Y;
                    break;
            }

            switch (alignment)
            {
                default: break;
                case TextAlignment.BaseCenter: 
                case TextAlignment.TopCenter: 
                case TextAlignment.MiddleCenter: 
                case TextAlignment.BottomCenter:
                    position.X -= size.X * 0.5f; 
                    break;
                case TextAlignment.BaseRight: 
                case TextAlignment.TopRight:
                case TextAlignment.MiddleRight:
                case TextAlignment.BottomRight:
                    position.X -= size.X;
                    break;
            }

            return new RectangleF(position, size);
        }

        /// <summary>
        /// Automatically wrap text into multiple lines
        /// </summary>
        /// <param name="text"></param>
        /// <param name="scale"></param>
        /// <param name="wrap"></param>
        /// <param name="maxWidth"></param>
        /// <returns></returns>
        public TextLine[] WrapString(string text, float scale = 1, WrapBehaviour wrap = WrapBehaviour.Default, float maxWidth = 0)
        {
            maxWidth = maxWidth < 0 ? 0 : maxWidth;
            switch (wrap)
            {
                default:
                case WrapBehaviour.Default:
                    {
                        TextLine[] lines = text.Split('\n').Select(x => new TextLine(x)).ToArray();
                        for (int i = 0; i < lines.Length; i++)
                        {
                            Glyph[] glyphs = lines[i].Text.Select(x => GetGlyph(x)).ToArray();
                            for (int j = 0; j < glyphs.Length; j++)
                            {
                                bool isLast = j == glyphs.Length - 1;
                                char? nextChar = isLast ? null : glyphs[j + 1].Character;
                                lines[i].Width += scale * (isLast ? glyphs[j].CursorBounds.Width : glyphs[j].GetAdvance(nextChar));
                            }
                        }
                        return lines;
                    }

                case WrapBehaviour.None:
                    {
                        TextLine line = new TextLine(text.Replace("\n", " "));
                        Glyph[] glyphs = line.Text.Select(x => GetGlyph(x)).ToArray();
                        for (int j = 0; j < glyphs.Length; j++)
                        {
                            bool isLast = j == glyphs.Length - 1;
                            char? nextChar = isLast ? null : glyphs[j + 1].Character;
                            line.Width += scale * (isLast ? glyphs[j].CursorBounds.Width : glyphs[j].GetAdvance(nextChar));
                        }
                        return new TextLine[] { line };
                    }

                case WrapBehaviour.Character:
                    {
                        List<TextLine> result = new();
                        TextLine line = new();
                        Glyph[] glyphs = text.Select(x => GetGlyph(x)).ToArray();
                        float advance = 0;

                        for (int i = 0; i < glyphs.Length; i++)
                        {
                            bool isLast = i == glyphs.Length - 1;
                            char? next = isLast ? null : glyphs[i + 1].Character;

                            if (glyphs[i].Character == '\n')
                            {
                                if (!isLast)
                                {
                                    result.Add(line);
                                    line = new();
                                }
                            }
                            else
                            {
                                advance = scale * (isLast ? glyphs[i].CursorBounds.Width : glyphs[i].GetAdvance(next));
                                if (!string.IsNullOrWhiteSpace(line.Text) && line.Width + advance >= maxWidth)
                                {
                                    result.Add(line);
                                    if (char.IsWhiteSpace(glyphs[i].Character))
                                        line = new();
                                    else
                                        line = new(glyphs[i].Character.ToString(), advance);
                                }
                                else
                                {
                                    line.Text += glyphs[i].Character;
                                    line.Width += advance;
                                    if (isLast) result.Add(line);
                                }
                            }
                        }

                        return result.ToArray();
                    }

                case WrapBehaviour.Word:
                    {
                        List<TextLine> result = new();
                        TextLine line = new();
                        TextLine word = new();
                        Glyph[] glyphs = text.Select(x => GetGlyph(x)).ToArray();

                        for (int i = 0; i < glyphs.Length; i++)
                        {
                            bool isLast = i == glyphs.Length - 1;
                            char? next = isLast ? null : glyphs[i + 1].Character;

                            if (glyphs[i].Character == '\n')
                            {
                                if (line.Width + word.Width <= maxWidth || maxWidth <= 0)
                                {
                                    result.Add(line + word);
                                }
                                else
                                {
                                    result.Add(line);
                                    result.Add(word);
                                }
                                line = new();
                                word = new();
                            }
                            else if (char.IsWhiteSpace(glyphs[i].Character) || isLast)
                            {
                                word.Text += glyphs[i].Character;
                                word.Width += scale * (isLast ? glyphs[i].CursorBounds.Width : glyphs[i].GetAdvance(next));

                                if (string.IsNullOrEmpty(line.Text) || line.Width + word.Width <= maxWidth)
                                {
                                    line += word;
                                    if (isLast)
                                        result.Add(line);
                                }
                                else
                                {
                                    if (isLast)
                                    {
                                        result.Add(line);
                                        result.Add(word);
                                    }
                                    else
                                    {
                                        result.Add(line);
                                        line = word;
                                    }
                                }
                                word = new();
                            }
                            else
                            {
                                word.Text += glyphs[i].Character;
                                word.Width += scale * (isLast ? glyphs[i].CursorBounds.Width : glyphs[i].GetAdvance(next));
                            }
                        }
                        return result.ToArray();
                    }
            }
        }
    }
}
