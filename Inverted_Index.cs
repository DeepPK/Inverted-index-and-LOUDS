using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Collections;
using Inverted_Index;
using System.IO;
using System.Globalization;

public class Program
{
    public static void Main()
    {
        var index = new InvertedIndex();
        //index.Add("Hello World Bogdan", 132434);
        //index.Add("Hello WORLD Test World", 2131312);
        //index.Add("Test World", 33333);
        //index.Add("Wadf Hello Test World", 41);
        var reader = new StreamReader(@"random_strings.csv");//Заполняем массив из файла
        for (int i = 0; i < 10; i++)
        {
            var line = reader.ReadLine();
            var values = line.Split(',');
            index.Add(values[0], values[1]);
        }

        index.Print_documents();//Выводим все документы
        
        Console.WriteLine();
        DateTime start = DateTime.Now;
        Dictionary<string, List<int>> InvertedIndex = index.SearchEachWord("Hello Big"); //Находим индексы всех документов с этими словами
        DateTime end = DateTime.Now;
        using (StreamWriter writer = new StreamWriter("Inverted_Index_time.txt", false))
        {
            writer.WriteLineAsync($"Время поиска для каждого слова: {end - start}ms");
        }
        foreach (var word in InvertedIndex)
        {
            Console.Write($"{word.Key}: ");
            for (int i = 0; i < word.Value.Count; i++)//Выводим все индексы слов
            {
                Console.Write($"{word.Value[i]} ");
            }
            Console.WriteLine();
        }
        Console.WriteLine();
        start = DateTime.Now;
        List<int> res = index.SearchAnd("Hello Big"); //Поиск всех индексов, которые содержат ВСЕ введённые слова
        end = DateTime.Now;
        using (StreamWriter writer = new StreamWriter("Inverted_Index_time.txt", true))
        {
            writer.WriteLineAsync($"Время поиска индексов со ВСЕМИ словами: {end - start}ms");
        }
        Console.WriteLine("Hello Big AND: ");
        for (int i = 0; i < res.Count; i++)
        {
            Console.Write($"{res[i]} ");//Выводим все индекс слов
        }
        Console.WriteLine();
        start = DateTime.Now;
        res = index.SearchOr("Hello Big"); //Поиск всех индексов, которые сожержат ЛЮБОЕ введённое слово
        end = DateTime.Now;
        using (StreamWriter writer = new StreamWriter("Inverted_Index_time.txt", true))
        {
            writer.WriteLineAsync($"Время поиска индексов с ЛЮБЫМ словом: {end - start}ms");
        }
        Console.WriteLine("Hello Big OR: ");
        for (int i = 0; i < res.Count; i++)
        {
            Console.Write($"{res[i]} ");
        }
        Console.WriteLine();
        start = DateTime.Now;
        res = index.SearchNot("Big"); //Выводит все индексы, где нет ВСЕХ этих слов
        end = DateTime.Now;
        using (StreamWriter writer = new StreamWriter("Inverted_Index_time.txt", true))
        {
            writer.WriteLineAsync($"Время поиска индексов с БЕЗ выбранныъ слов: {end - start}ms");
        }
        Console.WriteLine("Big NOT: ");
        for (int i = 0; i < res.Count; i++)
        {
            Console.Write($"{res[i]} ");
        }
        Console.WriteLine();
    }
}

public class InvertedIndex
{
    private static Dictionary<BitArray, BitArray> all_documents = new Dictionary<BitArray, BitArray>(); //Все закодированные данные
    private static List<Dictionary<HuffmanCompression, HuffmanCompression>> trees_of_documents = new List<Dictionary<HuffmanCompression, HuffmanCompression>>(); //Деревья для кодирования и декодирования
    //private static Dictionary<string, List<int>> words_in_document = new Dictionary<string, List<int>>();
    public void Add(string text, string identifier) //Добавить новый документ
    {
        //var words = text
        //    .Split(' ')
        //    .Select(x => x.ToLowerInvariant())
        //    .ToArray();
        var a = new HuffmanCompression();
        var b = new HuffmanCompression();
        var c = new Dictionary<HuffmanCompression, HuffmanCompression>();
        a.Build(text);
        b.Build(identifier);
        c.Add(a, b);
        trees_of_documents.Add(c); //Строим деревья и добавляем в список
        all_documents.Add(trees_of_documents.Last().Keys.Last().Encode(text), trees_of_documents.Last().Values.Last().Encode(identifier)); //Кодируем и добавляем
    }

    public void Print_documents() //Выводит все документы
    {
        foreach (var sent in trees_of_documents.Zip(all_documents, Tuple.Create))
        {
            Console.WriteLine($"{sent.Item1.Keys.Last().Decode(sent.Item2.Key)} : {sent.Item1.Values.Last().Decode(sent.Item2.Value)}");
        }
    }

    public Dictionary<string, List<int>> SearchEachWord(string text)
    {
        var words = text
          .Split(' ')
          .Select(x => x.ToLowerInvariant())
          .ToArray();
        
        var result = new Dictionary<string, List<int>>();

        for (int i = 0; i < words.Length; i++) //Проходимся по всем введённым словам
        {
            result.Add(words[i], []); //Добавляем ключ, потом дубем добавлять значение
            foreach (var sent in trees_of_documents.Zip(all_documents, Tuple.Create)) //Итератор двигается по двум коллекциям одновременно
            {
                if (sent.Item1.Keys.Last().Decode(sent.Item2.Key).ToLower().Contains(words[i])) //Если разкодированное слово есть в тексте...
                {
                    result[words[i]].Add(int.Parse(sent.Item1.Values.Last().Decode(sent.Item2.Value)));//То добавляем к значению результата
                }
            }
        }
        return result;
    }

    public List<int> SearchAnd(string text)
    {
        var words = text
            .Split(' ')
            .Select(x => x.ToLowerInvariant())
            .ToArray();

        List<int> res = new List<int>();
        foreach (var word in trees_of_documents.Zip(all_documents, Tuple.Create))
        {

            if (words.All(s => word.Item1.Keys.Last().Decode(word.Item2.Key).ToLower().Contains(s)))//Если присутствуют все слова в тексте 
            {
                res.Add(int.Parse(word.Item1.Values.Last().Decode(word.Item2.Value)));
            }
        }
        return res;
    }

    public List<int> SearchOr(string text)
    {
        var words = text
            .Split(' ')
            .Select(x => x.ToLowerInvariant())
            .ToArray();

        List<int> res = new List<int>();
        foreach (var word in trees_of_documents.Zip(all_documents, Tuple.Create))
        {
            if (words.Any(s => word.Item1.Keys.Last().Decode(word.Item2.Key).ToLower().Contains(s)))//Если любой присутствует
            {
                res.Add(int.Parse(word.Item1.Values.Last().Decode(word.Item2.Value)));
            }
        }
        return res;
    }
    public List<int> SearchNot(string text)
    {
        var words = text
            .Split(' ')
            .Select(x => x.ToLowerInvariant())
            .ToArray();

        List<int> res = new List<int>();
        foreach (var word in trees_of_documents.Zip(all_documents, Tuple.Create))
        {
            for (int i = 0; i < words.Length; i++)
            {
                if (!word.Item1.Keys.Last().Decode(word.Item2.Key).ToLower().Contains(words[i])) //Если слова нет, то...
                {
                    if (i == words.Length - 1)//Если дошли до конца, то добавляем в результат
                    {
                        res.Add(int.Parse(word.Item1.Values.Last().Decode(word.Item2.Value)));
                    }
                    else//иначе продолжаем проверять слова
                    {
                        continue;
                    }
                }
                else//Если есть, то больше не проверяем и идём к следующему документу
                {
                    break;
                }
            }
        }
        return res;
    }
}
