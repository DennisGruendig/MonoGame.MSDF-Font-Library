namespace MSDF_Font_Library.Datatypes
{
    public enum WrapBehaviour
    {
        /// <summary>
        /// Keep text in a single line
        /// </summary>
        None,

        /// <summary>
        /// Wrap with each linebreak '\n'
        /// </summary>
        Default,
        
        /// <summary>
        /// Wrap text by character using max width
        /// </summary>
        Character,

        /// <summary>
        /// Wrap text by full words using max width
        /// </summary>
        Word
    }
}
