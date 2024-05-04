using System.IO;

namespace MSDF_Font_Library.Content
{
    public class FieldFontImportData
    {
        public string FontGenerator = string.Empty;
        public string AtlasGenerator = string.Empty;
        public string FontFile = string.Empty;
        public string TempFolder = string.Empty;

        public string Name => Path.GetFileNameWithoutExtension(FontFile);
        public string Charset => Path.Combine(TempFolder, "Charset.txt");
        public string Json => Path.Combine(TempFolder, "Output.json");
        public string AtlasImage => Path.Combine(TempFolder, "Atlas.png");

    }
}
