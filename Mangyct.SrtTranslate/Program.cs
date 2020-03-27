using System;
using System.IO;
using Mangyct.SrtTranslate.Subtitle;
using Mangyct.SrtTranslate.Translater;

namespace Mangyct.SrtTranslate
{
    class Program
    {
        /*
         *
         * Программа ищет в указанном месте файлы субтитры.
         * Формат:
         *
         * 1
         * 00:00:00.05 --> 00:00:01.08
         * Текст субтитров
         * <пустая строка между субтитрами>
         *
         *
         * И переводит их при помощи Яндекс переводчика.
         * Используется https://github.com/anovik/YandexTranslateCSharpSdk
         * (В классе YandexTranslate необходимо вставить ApiKey от Яндекса)
         *
         * Текст переводится целиком. Потом дробиться на строки.
         *
         * Известные баги:
         *  - не точный перевод,
         *  - не точное дробление на строки
         *      (короткие, длинные субтитры, не соответствие таймингу)
         *
         */
        static void Main(string[] args)
        {
            Console.WriteLine("Введите путь к srt-файлам с субтитрами:");
            string pathRoot = Console.ReadLine();

            string[] files = Directory.GetFiles(pathRoot, "*.srt", SearchOption.AllDirectories);

            if (files.Length > 0)
            {
                Console.WriteLine($"Найдено : {files.Length}.");
            }

            var translate = new YandexTranslate();
            var parser = new SubStrParser(translate);

            foreach (var srtFile in files)
            {
                parser.CreateRuSubtitles(srtFile, "ru");
            }

            Console.ReadLine();
        }
    }
}