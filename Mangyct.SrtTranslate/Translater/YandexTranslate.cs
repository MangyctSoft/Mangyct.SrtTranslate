using System.Threading.Tasks;
using YandexTranslateCSharpSdk;

namespace Mangyct.SrtTranslate.Translater
{
    public class YandexTranslate : ITranslate
    {
        public async Task<string> TextTranslate(string text, string lang)
        {
            YandexTranslateSdk wrapper = new YandexTranslateSdk
            {
                ApiKey = "[PastApiKey]"
            };
            return await wrapper.TranslateText(text, lang);
        }
    }
}
