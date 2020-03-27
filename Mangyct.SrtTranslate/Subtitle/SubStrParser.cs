using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Mangyct.SrtTranslate.Translater;

namespace Mangyct.SrtTranslate.Subtitle
{
    /// <summary>
    /// Класс для работы с srt-файлами субтитров.
    /// </summary>
    public class SubStrParser
    {
        /// <summary>
        /// Начало текста субтитра (первая строка - номер, вторая - тайминг)
        /// </summary>
        const int START_SUBLINE = 3;
        /// <summary>
        /// Минимум слов в строке
        /// </summary>
        const int MIN_WORD_IN_LINE = 4;
        /// <summary>
        /// Переводчик
        /// </summary>
        private readonly ITranslate translate;
        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="_translate">Интерфейс переводчика</param>
        public SubStrParser(ITranslate _translate)
        {
            translate = _translate;
        }
        /// <summary>
        /// Основной метод создания переведенного файла с субтитрами.
        /// </summary>
        /// <param name="pathToSrtFile">Путь к файлу</param>
        /// <param name="lang">Язык для перевода</param>
        public void CreateRuSubtitles(string pathToSrtFile, string lang)
        {
            string fileName = Path.GetFileName(pathToSrtFile);
            string currentPath = Path.GetDirectoryName(pathToSrtFile);
            using (var fileStream = File.OpenRead(pathToSrtFile))
            {
                Console.WriteLine(("").PadRight(100, '='));
                Console.WriteLine($"Файл:{currentPath} {fileName}");
                var items = GetSubtitleLines(fileStream, Encoding.UTF8);
                var text = new StringBuilder();
                // Собираем весь текст для перевода.
                foreach (var obj in items)
                {
                    text.Append(obj.Text + " ");
                }
                Console.WriteLine(("").PadRight(100, '='));
                Console.WriteLine(text);
                // Перевод.
                var response = translate.TextTranslate(text.ToString(), lang).Result;
                Console.WriteLine(("").PadRight(100, '-'));
                Console.WriteLine(response);
                

                SetTranslate(ref items, response);

                fileName = SetFileNameTranslate(fileName, lang);
                SaveSrtFile(fileStream, Encoding.UTF8, items, currentPath + "\\" + fileName);
            }
        }
        /// <summary>
        /// Получаем список из строк из srt-файла,
        /// пропуская первые две строки (номер и тайминг) субтитра.
        /// Разделитель между субтитрами - пустая строка.
        /// Запоминаем номер строки, количество слов и последний символ,
        /// для дальнейшего форматирования и перевода.
        /// </summary>
        /// <param name="fileStream">Srt-файл</param>
        /// <param name="encoding">Кодировка</param>
        /// <returns></returns>
        private List<SubModel> GetSubtitleLines(Stream fileStream, Encoding encoding)
        {
            var list = new List<SubModel>();
            StreamReader streamReader = new StreamReader(fileStream, encoding, true);
            int numberLine = 0;
            int startSubtitle = START_SUBLINE;
            string line;
            while ((line = streamReader.ReadLine()) != null)
            {
                numberLine++;
                
                if (string.IsNullOrEmpty(line.Trim()))
                {
                    startSubtitle = numberLine + START_SUBLINE;
                }
                else
                {
                    if (numberLine == startSubtitle)
                    {
                        var subModel = new SubModel
                        {
                            NumberLine = numberLine,
                            CountWord = CountWordsInLine(line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)),
                            Text = line.Trim(),
                            LastChar = line[line.Length - 1]
                        };
                        list.Add(subModel);
                        startSubtitle++;
                    }
                }
            }
            return list;
        }
        /// <summary>
        /// Подсчитываем слова в строке
        /// </summary>
        /// <param name="arr">Массив слов</param>
        /// <returns></returns>
        private int CountWordsInLine(string[] arr)
        {
            int i = 0;
            foreach (var word in arr)
            {
                if (word.Length > 1)
                {
                    i++;
                }
            }
            return i;
        }
        /// <summary>
        /// Дробим весь переведенный текст на строки.
        /// Заменяем строки текста на переведенные.
        /// </summary>
        /// <param name="subModel">Список с субтитрами</param>
        /// <param name="textTranslate">Перевод</param>
        private void SetTranslate(ref List<SubModel> subModel, string textTranslate)
        {
            //Создаем массив из слов из переведенного текста.
            var wordsArray  = textTranslate.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            // Указатель на слово в массиве.
            var start       = 0;
            // Перебираем список с субтитрами.
            foreach (var line in subModel)
            {
                line.Text = "";
                // Если последний символ в строке был ','(запятая) или '.'(точка).
                if (line.LastChar == ',' || line.LastChar == '.')
                {
                    start--;
                    int i = 0;
                    bool check = false;
                    while (true)
                    {
                        // Собираем строку пока в конце слова нет запятой или точки.
                        start++;
                        if (start >= wordsArray.Length)
                        {
                            start = 0;
                        }
                        line.Text += wordsArray[start] + " ";
                        if (line.Text.Length > 1)
                        {
                            i++;
                        }
                        if (i > line.CountWord )
                        {
                            break;
                        }
                        if (wordsArray[start].EndsWith(line.LastChar))
                        {
                            if (line.LastChar == '.')
                            {
                                break;
                            }
                            check = true;
                        }
                        if (check)
                        {
                            // Минимум слов в строке
                            if (i > MIN_WORD_IN_LINE)
                            {
                                break;
                            }
                        }
                    } 
                    // Переводим указатель на следующее слово.
                    start++;
                }
                else
                {
                    int i;
                    // Собираем строку по количеству слов.
                    for ( i = start; i < line.CountWord + start; i++)
                    {
                        if (i < wordsArray.Length)              // Если в пределах массива,
                        {
                            line.Text += wordsArray[i] + " ";   // собираем строку,
                        }
                        else                                    // иначе, исключительный случай,
                        {
                            start = 0;                          // указатель в начало.
                            break;
                        }
                    }
                    if (i < wordsArray.Length)      // Если в пределах массива,
                    {   
                        start = i;                  // указатель на i = количеству слов.
                    }
                }
            }
        }
        /// <summary>
        /// Сохраняем переведенный файл с субтитрами.
        /// </summary>
        /// <param name="fileStream">Переводимый файл</param>
        /// <param name="encoding">Кодировка</param>
        /// <param name="subModel">Список переведенных строк</param>
        /// <param name="pathFileName">Имя нового файла</param>
        private void SaveSrtFile(Stream fileStream, Encoding encoding, List<SubModel> subModel, string pathFileName)
        {
            StreamReader streamReader = new StreamReader(fileStream, encoding, true);
            int numberLine = 0;
            string line;
            try
            {
                using (StreamWriter sw = new StreamWriter(pathFileName, false, encoding))
                {
                    streamReader.BaseStream.Position = 0;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        numberLine++;

                        var lineTranslate = subModel.FirstOrDefault(n => n.NumberLine == numberLine);
                        sw.WriteLine(lineTranslate == null ? line : lineTranslate.Text);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        /// <summary>
        /// Формирование имени файла с переводом.
        /// </summary>
        /// <param name="pathToSrtFile">Имя переводимого файла</param>
        /// <param name="lang">Язык перевода</param>
        /// <returns></returns>
        private string SetFileNameTranslate(string pathToSrtFile, string lang)
        {
            string srt = ".srt";
            int index = pathToSrtFile.IndexOf(srt);
            pathToSrtFile = pathToSrtFile.Remove(index - 2, 2);
            return pathToSrtFile.Insert(index - 2, lang);
        }
    }
}
