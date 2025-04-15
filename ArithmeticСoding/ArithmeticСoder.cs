using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ArithmeticCoding
{
    /// <summary>
    /// Класс для выполнения арифметического кодирования строк
    /// </summary>
    public class ArithmeticCoder
    {
        // Словарь вероятностей символов
        private Dictionary<char, double> _probabilities;

        // Словарь диапазонов для каждого символа на интервале [0, 1)
        private Dictionary<char, (double low, double high)> _ranges;

        /// <summary>
        /// Конструктор с автоматическим расчетом вероятностей
        /// </summary>
        /// <param name="input">Входная строка для анализа</param>
        public ArithmeticCoder(string input)
        {
            if (string.IsNullOrEmpty(input)) //проверка на пустоту
                throw new ArgumentException("Входная строка некорректна");

            CalculateProbabilities(input); //считаем вероятности
            CalculateRanges();//считаем диапозоны символов
        }
        /// <summary>
        /// Конструктор с заданным словарем вероятностей
        /// </summary>
        public ArithmeticCoder(Dictionary<char, double> probabilities)
        {
            if (probabilities == null || !probabilities.Any())
                throw new ArgumentException("Словарь вероятностей некорректен");

            // Проверяем, что сумма вероятностей примерно равна 1
            double sum = probabilities.Values.Sum();
            if (Math.Abs(sum - 1.0) > 1e-6)
                throw new ArgumentException("Сумма вероятностей должна быть равна 1");

            _probabilities = new Dictionary<char, double>(probabilities);
            CalculateRanges();//считаем диапозоны символов
        }
        /// <summary>
        /// Вычисляет вероятности символов во входной строке
        /// </summary>
        /// <param name="input">Входная строка</param>
        private void CalculateProbabilities(string input)
        {
            _probabilities = new Dictionary<char, double>();
            int totalChars = input.Length;//всего символов
            //считаем частоту появления символоа в строке
            foreach (char c in input)
            {
                if (_probabilities.ContainsKey(c))
                    _probabilities[c]++;
                else
                    _probabilities[c] = 1;
            }

            foreach (var key in _probabilities.Keys.ToList())
            {
                _probabilities[key] /= totalChars;//делим частоту появления на общее
                                                  //количество символов в строке
            }
        }

        /// <summary>
        /// Вычисляет диапазоны для каждого символа на интервале [0, 1)
        /// </summary>
        private void CalculateRanges()
        {
            _ranges = new Dictionary<char, (double low, double high)>();
            double low = 0.0;
            //сортируем по алфавиту 
            foreach (var pair in _probabilities.OrderBy(p => p.Key))
            {
                _ranges[pair.Key] = (low, low + pair.Value);//к нижней ранице добавляем вероятность появления
                low += pair.Value;//устанавливаем новую верхнюю границу
            }
        }

        /// <summary>
        /// Находит число с наименьшим количеством знаков в заданном интервале
        /// </summary>
        /// <param name="low">Нижняя граница интервала</param>
        /// <param name="high">Верхняя граница интервала</param>
        /// <returns>Число с минимальным представлением</returns>
        private double FindShortestNumberInInterval(double low, double high)
        {
            for (int precision = 1; precision <= 15; precision++)
            {
                double factor = Math.Pow(10, precision);
                long lowInt = (long)(low * factor);
                long highInt = (long)(high * factor);

                for (long num = lowInt; num <= highInt; num++)
                {
                    double value = num / factor;
                    if (value >= low && value <= high)
                    {
                        return value;
                    }
                }
            }

            return (low + high) / 2;
        }

        /// <summary>
        /// Кодирует входную строку методом арифметического кодирования
        /// </summary>
        /// <param name="input">Входная строка для кодирования</param>
        /// <returns>Кортеж: закодированное значение и длина в битах</returns>
        public (double encodedValue, int bitLength) Encode(string input)
        {
            if (string.IsNullOrEmpty(input)) //проверка на пустоту
                throw new ArgumentException("Строка пуста");

            double low = 0.0;
            double high = 1.0;
            double range = 1.0;

            foreach (char symbol in input)//читаем по одному каждый символ из строки
            {
                if (!_ranges.ContainsKey(symbol))
                    throw new ArgumentException($"Символ '{symbol}' не найден");

                var symbolRange = _ranges[symbol]; //достаем нижнюю и вернхнюю границы символа
                double newLow = low + range * symbolRange.low;
                double newHigh = low + range * symbolRange.high;

                low = newLow;
                high = newHigh;
                range = high - low;
            }

            double encodedValue = FindShortestNumberInInterval(low, high); //ищеи число из диапазона
                                                                           //с наименьшим количнством символов
            int bitLength = CalculateBitLengthByDigits(encodedValue); ; //считаем длину хакодированного слова в битах

            return (encodedValue, bitLength); //возвращаем закодиванное слово и длину в битах
        }

        /// <summary>
        /// Декодирует закодированное значение в исходную строку
        /// </summary>
        /// <param name="encodedValue">Закодированное значение</param>
        /// <param name="length">Длина исходной строки</param>
        /// <returns>Раскодированная строка</returns>
        public string Decode(double encodedValue, int length)
        {
            if (length <= 0)
                throw new ArgumentException("Длина строки должна быть положительной");

            string result = ""; //строка с результатом
            double value = encodedValue;//текущее число (закодированное слово)

            for (int i = 0; i < length; i++) //длина исходного слова
            {
                foreach (var pair in _ranges) 
                {
                    char symbol = pair.Key;
                    var (low, high) = pair.Value;

                    if (value >= low && value < high)//смотрим к какому символу попадает в диапозон
                    {
                        result += symbol; //заносим этот символ в результат
                        value = (value - low) / (high - low); //меняем значение текущего числа 
                        break;
                    }
                }
            }

            return result; //возращаем раскодированную строку
        }

        /// <summary>
        /// Считает количество бит в закодированной строке
        /// </summary>
        /// <param name="value">Число для конвертации</param>
        /// <returns>число бит в дробной части</returns>
        private int CalculateBitLengthByDigits(double value)
        {
            // Получаем строковое представление числа
            string valueStr = value.ToString("F15").TrimEnd('0');
            Console.WriteLine(valueStr);
            // Извлекаем дробную часть
            string[] parts = valueStr.Split(',');
            if (parts.Length < 2 || string.IsNullOrEmpty(parts[1]))
                return 0; // Нет дробной части

            // Считаем количество цифр в дробной части
            int digitCount = parts[1].Length;
            // Умножаем на 4
            return digitCount * 4;
        }

        /// <summary>
        /// Вычисляет коэффициент сжатия
        /// </summary>
        /// <param name="input">Исходная строка</param>
        /// <param name="bitLength">Длина закодированного значения в битах</param>
        /// <returns>Коэффициент сжатия</returns>
        public double CalculateCompressionRatio(string input, int bitLength)
        {
            if (string.IsNullOrEmpty(input))
                return 0.0;

            int originalBitLength = input.Length * 8;
            return bitLength == 0 ? 0.0 : (double)originalBitLength / bitLength;
        }

        /// <summary>
        /// Возвращает словарь вероятностей символов
        /// </summary>
        public Dictionary<char, double> GetProbabilities()
        {
            return _probabilities;
        }
    }
}