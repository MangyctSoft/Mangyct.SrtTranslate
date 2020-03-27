using System.Threading.Tasks;

namespace Mangyct.SrtTranslate.Translater
{
    public interface ITranslate
    {
        Task<string> TextTranslate(string text, string lang);
    }
}
