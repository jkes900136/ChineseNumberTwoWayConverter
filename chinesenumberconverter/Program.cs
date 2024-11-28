using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace chinesenumberconverter
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            Console.WriteLine(GetChineseNumberToInt("2千萬美元"));
            Console.WriteLine(GetChineseNumberToInt("2千萬"));
            Console.WriteLine(GetChineseNumberToInt("兩千萬"));

            Console.ReadKey();
        }
        public static long GetChineseNumberToInt(string s)
        {
            Dictionary<string, long> digit =
                new Dictionary<string, long>()
                { { "一", 1 },
                  { "二", 2 },
                  { "兩", 2 },
                  { "三", 3 },
                  { "四", 4 },
                  { "五", 5 },
                  { "六", 6 },
                  { "七", 7 },
                  { "八", 8 },
                  { "九", 9 } };
            Dictionary<string, long> word =
                new Dictionary<string, long>()
                { { "百", 100 },
                { "千", 1000 },
                { "萬", 10000 },
                { "億", 100000000 },
                { "兆", 1000000000000 } };

            Dictionary<string, long> ten =
                new Dictionary<string, long>()
                { { "十", 10 } };
            long iResult = 0;

            s = s.Replace("零", "");
            s = s.Replace("〇", "");
            int index = 0;
            long t_l = 0, _t_l = 0;
            string t_s;

            int[] numberGroups = Regex
              .Matches(s, "[0-9]+") // groups of integer numbers
              .OfType<Match>()
              .Select(match => int.Parse(match.Value))
              .ToArray();
            int numberGroupCounter = 0;
            while (s.Length > index)
            {
                t_s = s.Substring(index++, 1);
                int originalDigit = 0;
                // 數字
                if (digit.ContainsKey(t_s))
                {
                    _t_l += digit[t_s];
                }
                else if (int.TryParse(t_s, out originalDigit))
                {
                    _t_l += numberGroups[numberGroupCounter];
                    numberGroupCounter++;
                }
                // 十
                else if (ten.ContainsKey(t_s))
                {
                    _t_l = _t_l == 0 ? 10 : _t_l * 10;
                }
                // 百、千、億、兆 
                else if (word.ContainsKey(t_s))
                {
                    // 碰到千位則使 _t_l 與 t_l 相加乘上目前讀到的數字，
                    // 並將輸出結果累加。
                    if (word[t_s] > word["千"])
                    {
                        iResult += (t_l + _t_l) * word[t_s];
                        t_l = 0;
                        _t_l = 0;

                        continue;
                    }
                    _t_l = _t_l * word[t_s];
                    t_l += _t_l;

                    _t_l = 0;
                }
            }
            // 將殘餘值累加至輸出結果
            iResult += t_l;
            iResult += _t_l;

            return iResult;

        }
    }
}
