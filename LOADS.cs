using System.Numerics;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.Runtime.Intrinsics;
using System.Diagnostics;

public class TrieNode//Узел
{
    public bool isWord;//конец слова
    public Dictionary<char, TrieNode> Map = new Dictionary<char, TrieNode>(); //ссылка на все ноды
    public BitArray BitIndex = new BitArray(0);//биты узла от кол-ва потомков
}

public class Trie_LOUDS//Префиксное дерево
{
    public BitArray LBS = new BitArray(0);//Общий битсет дерева
    public TrieNode root;
    public Trie_LOUDS()
    {
        root = new TrieNode();
    }

    public void Insert(string word)//Вставка в префиксном дереве
    {
        int len = word.Length;

        TrieNode curNode = root;
        for (int i = 0; i < len; i++)//Идём по всему слову
        {
            char c = word[i];
            if (!curNode.Map.ContainsKey(c))//Если такой буквы нет, то надо создать
            {
                curNode.Map[c] = new TrieNode();
            }
            curNode = curNode.Map[c];

            if (i == len - 1)//Если конец слова, то помечаем
            {
                curNode.isWord = true;
            }
        }
    }
    public bool Search(string word)
    {
        TrieNode curNode = root;
        foreach (var c in word)
        {
            if (!curNode.Map.ContainsKey(c))//Если не найдём букву, значит слова нет
            {
                return false;
            }
            curNode = curNode.Map[c];//идём дальше по слову
        }
        return curNode.isWord;//Если дойдём до конца, значит такое слово есть
    }
    public void MakeBitArray(TrieNode Node) //Метод для создания BitArray
    {
        TrieNode fakeRoot = new TrieNode();//Фейковый рут для создания LBS
        fakeRoot.Map = new Dictionary<char, TrieNode> { { 'a', Node } }; //Отсылаем фейк на истинный корень
        SetBits(fakeRoot);//Создаём биты узлов
        LBS = new BitArray(0);//Сносим старый BitArray
        GetBitArray(fakeRoot);//Создаём новый
    }

    public void SetBits(TrieNode Node)
    {
        Node.BitIndex.Length = Node.Map.Keys.Count+1;//Даём размер массиву относительно кол-во потомков
        for (int i = 0; i < Node.BitIndex.Length; i++)//Даём кол-во 1, равный потомку
        {
            Node.BitIndex[i] = true;
        }

        Node.BitIndex[Node.Map.Keys.Count] = false;//Ластовый индекс всегда false

        if (Node.Map.Keys == null)//Если дошли до конца, то возвращаемся к другим
        {
            return;    
        }

        foreach (var child in Node.Map.Keys)//Рекурсивно пройдёмся по всем нодам
        {
            SetBits(Node.Map[child]);
        }
        return;//Просто выходим
    }
    public void GetBitArray(TrieNode Node)//Создаёт LBS
    {
        Queue queue = new Queue();//Очередь для прохождение по уровням
        queue.Enqueue(Node);//Корень
        int prev_length;//Отслеживает где закончился прошлый BitArray
        while (queue.Count != 0)
        {
            TrieNode curr = (TrieNode)queue.Dequeue();//Получаем новую ноду
            prev_length = LBS.Length;//Сохраняем место остановки
            LBS.Length += curr.BitIndex.Length;//Удлиняем с новым BitArray
            for (int i = 0; prev_length < LBS.Length; prev_length++, i++) //Заполняем LBS
            {
                LBS[prev_length] = curr.BitIndex[i];
            }
            foreach (var child in curr.Map.Keys)//Проходимся по всем нодам
            { 
                queue.Enqueue(curr.Map[child]);
            }
        }
    }
    public int Select(int search, bool bit)
    {
        int res = 0;
        int j = 0;
        for (int i = 0; i < this.LBS.Length; i++)
        {
            if (j==search) return res;
            if (this.LBS[i] == bit) { 
                res = i;
                j++;
            }
        }
        return res;
    }

    public int Rank(int search, bool bit)
    {
        int res = 0;
        for (int i = 0; i <= search; i++)
        {
            if (this.LBS[i] == bit) res++;
        }
        return res;
    }

    public void all_search(int X)//Все виды поиска, подходящие LOADS
    {
        DateTime start = DateTime.Now;
        Console.WriteLine($"Поиск первого потомка для {X}-ого узла: {Select(X+1, false) - X}");
        DateTime end = DateTime.Now;
        using (StreamWriter writer = new StreamWriter("LOADS_time.txt", false))
        {
            writer.WriteLineAsync($"Время поиска первого потомка: {end - start}ms");
        }
        start = new DateTime();
        int q = Select(X + 2, false) - 1;
        if (LBS[q])
        {
            Console.WriteLine($"Поиск последнего потомка для {X}-того узла: {Rank(q - 1, true)}");
        }
        else Console.WriteLine("Узел не имеет потомков");
        end = new DateTime();
        using (StreamWriter writer = new StreamWriter("LOADS_time.txt", true))
        {
            writer.WriteLineAsync($"Время поиска последнего потомка: {end - start}ms");
        }
        start = new DateTime();
        Console.WriteLine($"Кол-во потомка для {X}-того узла: {(Select((X+1)+1, false) - (X+1)) - (Select(X+1, false) - X)}");
        end = new DateTime();
        using (StreamWriter writer = new StreamWriter("LOADS_time.txt", true))
        {
            writer.WriteLineAsync($"Время поиска кол-ва потомков: {end - start}ms");
        }
        start = new DateTime();
        Console.WriteLine($"Поиск родителя для {X}-того узла: {Rank(Select(X+1, true)-1, false)-1}");
        end = new DateTime();
        using (StreamWriter writer = new StreamWriter("LOADS_time.txt", true))
        {
            writer.WriteLineAsync($"Время поиска родителя: {end - start}ms");
        }
    }
}

class Programm
{
    static void Main(string[] args)
    {
        Trie_LOUDS trie = new Trie_LOUDS(); //Дерево
        var reader = new StreamReader(@"random_strings.txt");//Заполняем массив из файла
        string line = reader.ReadToEnd();
        List<string> words = line.Split(" ").ToList();
        //trie.Insert("jpa");
        //trie.Insert("jp");
        //trie.Insert("jca");
        //trie.Insert("jcd");
        foreach (var word in words)//Закидываем слова в массив
        {
            trie.Insert(word);
        }
        trie.MakeBitArray(trie.root);//Создаём битовую последовательность
        Console.Write("Битовое представление префиксного дерева: ");
        foreach (var i in trie.LBS)//Выводим
        { 
            if ((bool)i) Console.Write(1 + " ");
            else Console.Write(0 + " ");
        }
        Console.WriteLine();
        trie.all_search(2);//Проверяем все поиски по номеру узла
        Console.ReadLine();
    }
}
