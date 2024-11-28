using System.Text;
using System.Text.RegularExpressions;

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

            var numSets = new[] {
                "0000", "0001", "0010", "0012",
                "0100", "0102", "0310", "0123",
                "1000", "1002", "1020", "1023",
                "1200", "1203", "1230", "1234"
            };
            // 列出所有可能的組合
            var count = numSets.Length;
            var idxs = new int[3];
            var lv = idxs.Length - 1;
            while (true)
            {
                var sb = new StringBuilder();
                for (var i = 0; i < idxs.Length; i++)
                {
                    sb.Append(numSets[idxs[i]]);
                }
                long n = long.Parse(sb.ToString());
                var cht = ChtNumConverter.ToChtNum(n);
                var restored = ChtNumConverter.ParseChtNum(cht);
                Console.WriteLine($"{n,16:n0} {restored,16:n0} " +
                    $"\x1b[{(n == restored ? "32mPASS" : "31mFAIL")}\x1b[0m {cht} ");
                if (n != restored)
                    throw new ApplicationException($"數字轉換錯誤 {n} vs {restored}");
                if (idxs.All(o => o == count - 1)) break;
                idxs[lv]++;
                while (idxs[lv] == count)
                {
                    idxs[lv] = 0;
                    lv--;
                    if (lv < 0) break;
                    idxs[lv]++;
                }
                lv = idxs.Length - 1;
            }
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
        public class ChtNumConverter
        {
            public static string ChtNums = "零一二三四五六七八九";
            public static Dictionary<string, long> ChtUnits = new Dictionary<string, long>{
            {"十", 10},
            {"百", 100},
            {"千", 1000},
            {"萬", 10000},
            {"億", 100000000},
            {"兆", 1000000000000}
        };
            // 解析中文數字        
            public static long ParseChtNum(string chtNumString)
            {
                var isNegative = false;
                if (chtNumString.StartsWith("負"))
                {
                    chtNumString = chtNumString.Substring(1);
                    isNegative = true;
                }
                long num = 0;
                // 處理千百十範圍的四位數
                Func<string, long> Parse4Digits = (s) =>
                {
                    long lastDigit = 0;
                    long subNum = 0;
                    foreach (var rawChar in s)
                    {
                        var c = rawChar.ToString().Replace("〇", "零");
                        if (ChtNums.Contains(c))
                        {
                            lastDigit = (long)ChtNums.IndexOf(c);
                        }
                        else if (ChtUnits.ContainsKey(c))
                        {
                            if (c == "十" && lastDigit == 0) lastDigit = 1;
                            long unit = ChtUnits[c];
                            subNum += lastDigit * unit;
                            lastDigit = 0;
                        }
                        else
                        {
                            throw new ArgumentException($"包含無法解析的中文數字：{c}");
                        }
                    }
                    subNum += lastDigit;
                    return subNum;
                };
                // 以兆億萬分割四位值個別解析
                foreach (var splitUnit in "兆億萬".ToArray())
                {
                    var pos = chtNumString.IndexOf(splitUnit);
                    if (pos == -1) continue;
                    var subNumString = chtNumString.Substring(0, pos);
                    chtNumString = chtNumString.Substring(pos + 1);
                    num += Parse4Digits(subNumString) * ChtUnits[splitUnit.ToString()];
                }
                num += Parse4Digits(chtNumString);
                return isNegative ? -num : num;
            }
            // 轉換為中文數字
            public static string ToChtNum(long n)
            {
                var negtive = n < 0;
                if (negtive) n = -n;
                if (n >= 10000 * ChtUnits["兆"])
                    throw new ArgumentException("數字超出可轉換範圍");
                var unitChars = "千百十".ToArray();
                // 處理 0000 ~ 9999 範圍數字
                Func<long, string> Conv4Digits = (subNum) =>
                {
                    var sb = new StringBuilder();
                    foreach (var c in unitChars)
                    {
                        if (subNum >= ChtUnits[c.ToString()])
                        {
                            var digit = subNum / ChtUnits[c.ToString()];
                            subNum = subNum % ChtUnits[c.ToString()];
                            sb.Append($"{ChtNums[(int)digit]}{c}");
                        }
                        else sb.Append("零");
                    }
                    sb.Append(ChtNums[(int)subNum]);
                    return sb.ToString();
                };
                var numString = new StringBuilder();
                var forceRun = false;
                foreach (var splitUnit in "兆億萬".ToArray())
                {
                    var unit = ChtUnits[splitUnit.ToString()];
                    if (n < unit)
                    {
                        if (forceRun) numString.Append("零");
                        continue;
                    }
                    forceRun = true;
                    var subNum = n / unit;
                    n = n % unit;
                    if (subNum > 0)
                        numString.Append(Conv4Digits(subNum).TrimEnd('零') + splitUnit);
                    else numString.Append("零");
                }
                numString.Append(Conv4Digits(n));
                var t = Regex.Replace(numString.ToString(), "[零]+", "零");
                if (t.Length > 1) t = t.Trim('零');
                t = Regex.Replace(t, "^一十", "十");
                return (negtive ? "負" : string.Empty) + t;
            }
        }
        //-----------------------------
    }
}
