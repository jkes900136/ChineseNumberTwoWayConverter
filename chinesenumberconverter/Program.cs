using System.Text;
using System.Text.RegularExpressions;

namespace chinesenumberconverter
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            Console.WriteLine(ChtNumConverter.ParseChtNum("2千萬美元"));
            Console.WriteLine(ChtNumConverter.ParseChtNum("2千萬"));
            Console.WriteLine(ChtNumConverter.ParseChtNum("兩千萬"));
                        
            Console.ReadKey();
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
                    int numberGroupCounter = 0;
                    int originalDigit = 0;
                    int[] numberGroups = Regex
                    .Matches(s, "[0-9]+") // groups of integer numbers
                    .OfType<Match>()
                    .Select(match => int.Parse(match.Value))
                    .ToArray();
                    foreach (var rawChar in s)
                    {
                        var c = rawChar.ToString().Replace("〇", "零").Replace("兩", "二");
                        if (ChtNums.Contains(c))
                        {
                            lastDigit = (long)ChtNums.IndexOf(c);
                        }
                        else if (int.TryParse(c, out originalDigit))
                        {
                            lastDigit = numberGroups[numberGroupCounter];
                            numberGroupCounter++;
                        }
                        else if (ChtUnits.ContainsKey(c))
                        {
                            if (c == "十" && lastDigit == 0) lastDigit = 1;
                            long unit = ChtUnits[c];
                            subNum += lastDigit * unit;
                            lastDigit = 0;
                        }
                        //else
                        //{
                        //    throw new ArgumentException($"包含無法解析的中文數字：{c}");
                        //}
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
