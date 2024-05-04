using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using MSDF_Font_Library.Content;
using MSDF_Font_Library.Datatypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MSDF_Font_Library.FontAtlas
{
    public class Glyph
    {
        [ContentSerializer] private readonly char _Character;
        [ContentSerializer] private readonly float _Advance;
        [ContentSerializer] private readonly Dictionary<char, float> _KerningAdvances;
        [ContentSerializer] private readonly RectangleF _AtlasSource;
        [ContentSerializer] private readonly RectangleF _CursorBounds;

        public Glyph()
        {
            _KerningAdvances = new();
        }

        public Glyph(FieldFont font, JsonGlyph glyph, List<JsonKerning> kernings)
        {
            _Character = (char)glyph.Unicode;
            _Advance = (float)(glyph.Advance * font.FontSize);

            _KerningAdvances = kernings
                .Where(x => x.Unicode1 == glyph.Unicode)
                .ToDictionary(x => (char)x.Unicode2, x => (float)(x.Advance * font.FontSize));

            _AtlasSource = new RectangleF(
                (float)(glyph.AtlasBounds.Left),
                (float)(font.AtlasSize.Y - glyph.AtlasBounds.Bottom - (glyph.AtlasBounds.Top - glyph.AtlasBounds.Bottom)),
                (float)(glyph.AtlasBounds.Right - glyph.AtlasBounds.Left),
                (float)(glyph.AtlasBounds.Top - glyph.AtlasBounds.Bottom));

            _CursorBounds = new RectangleF(
                (float)(glyph.PlaneBounds.Left * font.FontSize),
                (float)(font.Ascender * font.FontSize - glyph.PlaneBounds.Top * font.FontSize),
                (float)(glyph.PlaneBounds.Right * font.FontSize),
                (float)(-glyph.PlaneBounds.Bottom * font.FontSize));
        }

        public char Character { get => _Character; }
        public RectangleF AtlasSource { get => _AtlasSource; }
        public RectangleF CursorBounds { get => _CursorBounds; }

        public float GetAdvance(Glyph? nextChar)
        {
            if (nextChar is null) return _Advance;
            return GetAdvance(nextChar.Character);
        }
        public float GetAdvance(char? nextChar = null)
        {
            if (nextChar is not null && _KerningAdvances is not null)
            {
                _KerningAdvances.TryGetValue(nextChar.Value, out float advance);
                return _Advance + advance;
            }
            return _Advance;
        }
    }
}
