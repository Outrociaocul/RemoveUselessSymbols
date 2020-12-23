using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoveUselessSymbols
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.IO;

    namespace UnusedSymbs
    {
        class Rule
        {
            // Левая часть продукции 
            public char Symbol { get; set; }
            // Список правых частей продукции
            public List<string> Productions { get; set; }

            public Rule(char s, List<string> l)
            {
                Symbol = s;
                Productions = l;
            }
        }

        class Program
        {
            public static void Main(string[] args)
            {
                // Наш файл с грамматикой
                string file;

                // Можно задать аргументом командной строки
                if (args.Length > 1)
                {
                    file = args[1];
                }
                // Или записать в консоли
                else
                {
                    Console.Write("Введите путь к файлу с грамматикой: ");
                    file = Console.ReadLine();
                }

                // Если такого файла нет - Ошибка
                if (!File.Exists(file))
                {
                    Console.WriteLine("Указанный файл не найден");
                    goto fin;
                }

                // Считываем текст из файла
                string text = File.ReadAllText(file);

                // Множество нетерминалов
                SortedSet<char> N = new SortedSet<char>(text.Where(x => char.IsUpper(x)));
                // Множество терминалов
                SortedSet<char> Sigma = new SortedSet<char>(text.Where(x => char.IsLower(x)));

                // Записываем наши правила
                List<Rule> rules = text
                    .Split(' ')
                    .Select(line => new Rule
                    (
                        line.Split(new string[] { "->" }, StringSplitOptions.RemoveEmptyEntries)[0][0],
                        line.Split(new string[] { "->" }, StringSplitOptions.RemoveEmptyEntries)[1].Split('|').ToList()
                    ))
                    .ToList();

                step1: // Шаг первый - Определяем начальное множество нетерминальных символов (пустое)
                List<SortedSet<char>> used = new List<SortedSet<char>>();
                used.Add(new SortedSet<char>());
                int i = 1;

                step2: // Шаг два - добавляем в новое множество те же символы плюс те, из которых уже записанные можно получить
                SortedSet<char> set = new SortedSet<char>(used.ElementAt(i - 1));
                foreach (var rule in rules)
                {
                    // Если среди правых частей для данного сивола есть те, которые состоят только из подходящих символов
                    if (rule.Productions.Any(x => x.All(c => Sigma.Contains(c) || used.ElementAt(i - 1).Contains(c))))
                        // Добавляем левый символ в множество
                        set.Add(rule.Symbol);
                }
                // Добавляем полученное множество в список
                used.Add(set);

                step3:
                // Если полученное множество и исходное не равны
                if (!used.ElementAt(i).SetEquals(used.ElementAt(i - 1)))
                {
                    // Идем снова на шаг два
                    ++i;
                    goto step2;
                }
                // Иначе 
                else
                    // Идём на шаг 4
                    goto step4;

                step4:
                // Новое множество нетерминалов состоит из последнего полученного множества в списке
                SortedSet<char> new_N = new SortedSet<char>(used.ElementAt(i));
                // Выводим его на экран
                Console.WriteLine("New N = " + string.Join("", new_N));
                // Удаляем те продукции, где левая часть - бесполезный символ
                rules.RemoveAll(x => !new_N.Contains(x.Symbol));
                // В списке правых частей каждой из оставшихся продукций
                foreach (var rule in rules)
                    // Удаляем те правые части, в которых есть хотя бы 1 бесполезный символ 
                    rule.Productions.RemoveAll(x => x.Any(c => !new_N.Contains(c) && !Sigma.Contains(c)));

                // Выводим список новых правил грамматики
                foreach (var rule in rules)
                {
                    Console.WriteLine(rule.Symbol + " -> " + string.Join("|", rule.Productions));
                }

                fin:
                Console.WriteLine("Нажмите чтобы выйти");
                Console.ReadLine();
            }
        }
    }

}
