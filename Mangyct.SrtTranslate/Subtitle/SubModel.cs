namespace Mangyct.SrtTranslate.Subtitle
{
    /// <summary>
    /// Информация для перевода субтитров.
    /// </summary>
    public class SubModel
    {
        /// <summary>
        /// Номер строки.
        /// </summary>
        public int       NumberLine { get; set; }
        /// <summary>
        /// Текст для перевода.
        /// </summary>
        public string    Text { get; set; }
        /// <summary>
        /// Последний символ в строке.
        /// </summary>
        public char      LastChar { get; set; }
        /// <summary>
        /// Количество слов в строке.
        /// </summary>
        public int       CountWord { get; set; }
    }
}
