namespace ConsoleApp1
{
    internal class Program
    {
        public static int Distance(string str1Param, string str2Param)
        {
            if ((str1Param == null) || (str2Param == null)) return -1;
            int str1Len = str1Param.Length;
            int str2Len = str2Param.Length;

            if ((str1Len == 0) & (str2Len == 0)) return 0;
            if (str1Len == 0) return str2Len;
            if (str2Len == 0) return str1Len;

            string str1 = str1Param.ToUpper();
            string str2 = str2Param.ToUpper();

            int[,] matrix = new int[str1Len + 1, str2Len + 1];
            for (int i = 0; i <= str1Len; i++) matrix[i, 0] = i;
            for (int j = 0; j <= str2Len; j++) matrix[0, j] = j;

            for (int i = 1; i <= str1Len; i++)
            {
                for (int j = 1; j <= str2Len; j++)
                {
                    int SymbEqual = (
                        (str1.Substring(i - 1, 1) == str2.Substring(j - 1, 1)) ? 0 : 1);
                    int ins = matrix[i, j - 1] + 1;
                    int del = matrix[i - 1, j] + 1;
                    int rep = matrix[i - 1, j - 1] + SymbEqual;

                    matrix[i, j] = Math.Min(del, Math.Min(ins, rep));

                    if ((i > 1) && (j > 1) &&
                        (str1.Substring((i - 1), 1) == str2.Substring((j - 2), 1)) &&
                        (str1.Substring((i - 2), 1) == str2.Substring((j - 1), 1)))
                    {
                        matrix[i, j] = Math.Min(matrix[i, j], matrix[i - 2, j - 2] + SymbEqual);
                    }
                }
            }

            return matrix[str1Len, str2Len];
        }
        public static void WriteDistance(string str1Param, string str2Param)
        {
            int a = Distance(str1Param, str2Param);
            Console.WriteLine($"{str1Param}, {str2Param} -> {a}");
        }
        private static void Main(string[] args)
        {
            Console.WriteLine("Введите 1 строку (для выхода напишите exit): ");
            string? s1 = Console.ReadLine();
            while ((s1 != null) && (s1 != "exit"))
            {
                Console.WriteLine("Введите 2 строку: ");
                string? s2 = Console.ReadLine();
                if (s2 == null) break;
                WriteDistance(s1, s2);
                Console.WriteLine("Введите 1 строку (для выхода напишите exit): ");
                s1 = Console.ReadLine();
            }
        }
    }
}