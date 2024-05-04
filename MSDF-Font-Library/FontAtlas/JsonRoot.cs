using System.Collections.Generic;

namespace MSDF_Font_Library.FontAtlas
{
    public class JsonRoot
    {
        public JsonAtlas Atlas { get; set; } = new();
        public JsonMetrics Metrics { get; set; } = new();
        public List<JsonGlyph> Glyphs { get; set; } = new();
        public List<JsonKerning> Kerning { get; set; } = new();
        public bool IgnoreKerning { get; set; } = false;
    }
}
