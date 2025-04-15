using ArithmeticCoding;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ConsoleArithmeticCoding
{
    internal class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("\nМеню арифметического кодирования:");
                Console.WriteLine("1. Закодировать текст из файла coding.txt");
                Console.WriteLine("2. Раскодировать текст из файла decoding.txt");
                Console.WriteLine("3. Показать словарь вероятностей для символов (coding.txt)");
                Console.WriteLine("4. Показать степень сжатия (coding.txt)");
                Console.WriteLine("5. Показать число бит в исходной и закодированной строке (coding.txt)");
                Console.WriteLine("6. Выход");
                Console.Write("Выберите опцию (1-6): ");

                string choice = Console.ReadLine();

                try
                {
                    switch (choice)
                    {
                        case "1":
                            EncodeFromFile();
                            break;
                        case "2":
                            DecodeFromFile();
                            break;
                        case "3":
                            ShowProbabilities();
                            break;
                        case "4":
                            ShowCompressionRatio();
                            break;
                        case "5":
                            ShowBitLengths();
                            break;
                        case "6":
                            return;
                        default:
                            Console.WriteLine("Неверный выбор. Попробуйте снова.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                }
            }
        }

        static void EncodeFromFile()
        {
            string filePath = "Coding.txt";
            Console.ForegroundColor = ConsoleColor.Magenta;
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Файл {filePath} не найден. Создайте файл coding.txt с текстом для кодирования.");
                return;
            }

            string input = File.ReadAllText(filePath);
            var coder = new ArithmeticCoder(input);
            var (encodedValue, bitLength) = coder.Encode(input);

            // Выводим только закодированное значение в консоль
            Console.WriteLine($"Закодированное значение: {encodedValue}");

            // Получаем словарь вероятностей
            var probabilities = coder.GetProbabilities();
            string probString = string.Join(";", probabilities.OrderBy(p => p.Key)
                .Select(p => $"{p.Key}:{p.Value}"));

            // Сохраняем в decoding.txt
            string outputPath = "Decoding.txt";

            try
            {
                File.WriteAllText(outputPath, $"{encodedValue}\n{input.Length}\n{probString}");
                Console.WriteLine($"Результат сохранён в {outputPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении файла {outputPath}: {ex.Message}");
            }
            Console.ResetColor();
        }

        static void DecodeFromFile()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            string filePath = "Decoding.txt";

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Файл {filePath} не найден. Создайте файл decoding.txt с закодированными данными.");
                return;
            }

            string[] lines = File.ReadAllLines(filePath);
            if (lines.Length < 3 ||
                !double.TryParse(lines[0], out double encodedValue) ||
                !int.TryParse(lines[1], out int length))
            {
                Console.WriteLine("Неверный формат файла decoding.txt. Ожидается: закодированное значение, длина строки, словарь вероятностей.");
                return;
            }

            // Читаем словарь вероятностей
            Dictionary<char, double> probabilities = new Dictionary<char, double>();
            try
            {
                string[] probPairs = lines[2].Split(';', StringSplitOptions.RemoveEmptyEntries);
                foreach (string pair in probPairs)
                {
                    var parts = pair.Split(':');
                    if (parts.Length != 2 || parts[0].Length != 1 || !double.TryParse(parts[1], out double prob))
                    {
                        throw new FormatException("Неверный формат словаря вероятностей.");
                    }
                    probabilities[parts[0][0]] = prob;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка чтения словаря из {filePath}: {ex.Message}");
                return;
            }

            var coder = new ArithmeticCoder(probabilities);
            string decodedText = coder.Decode(encodedValue, length);

            // Выводим раскодированный текст в консоль
            Console.WriteLine($"Раскодированный текст: {decodedText}");

            // Сохраняем в coding.txt
            string outputPath = "Coding.txt";

            try
            {
                File.WriteAllText(outputPath, decodedText);
                Console.WriteLine($"Раскодированный текст сохранён в {outputPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении файла {outputPath}: {ex.Message}");
            }
            Console.ResetColor();
        }

        static void ShowProbabilities()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            string filePath = "Coding.txt";

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Файл {filePath} не найден. Создайте файл coding.txt с текстом.");
                return;
            }

            string input = File.ReadAllText(filePath);
            var coder = new ArithmeticCoder(input);
            var probabilities = coder.GetProbabilities();

            Console.WriteLine("Словарь вероятностей:");
            foreach (var pair in probabilities.OrderBy(p => p.Key))
            {
                Console.WriteLine($"Символ '{pair.Key}': {pair.Value:F6}");
            }
            Console.ResetColor();
        }

        static void ShowCompressionRatio()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            string filePath = "Coding.txt";

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Файл {filePath} не найден. Создайте файл coding.txt с текстом.");
                return;
            }

            string input = File.ReadAllText(filePath);
            var coder = new ArithmeticCoder(input);
            var (encodedValue, bitLength) = coder.Encode(input);
            double ratio = coder.CalculateCompressionRatio(input, bitLength);

            Console.WriteLine($"Коэффициент сжатия: {ratio:F2}");
            Console.WriteLine($"(Длина закодированной строки: {bitLength} бит, по 4 бита на цифру дробной части)");
            Console.ResetColor();
        }

        static void ShowBitLengths()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            string filePath = "Coding.txt";

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Файл {filePath} не найден. Создайте файл coding.txt с текстом.");
                return;
            }

            string input = File.ReadAllText(filePath);
            var coder = new ArithmeticCoder(input);
            var (encodedValue, bitLength) = coder.Encode(input);
            int originalBitLength = input.Length * 8;

            Console.WriteLine($"Число бит в исходной строке: {originalBitLength}");
            Console.WriteLine($"Число бит в закодированной строке (по 4 бита на цифру дробной части): {bitLength}");
            Console.ResetColor();
        }
    }
}