using System;

namespace QueryFormatter
{

    public class WriteLine
    {
        public string line { get; set; }
        public int intendSize { get; set; }
        public bool isNewLine { get; set; }
        public int wordId { get; set; }
    }

    public class Parenthese
    {
        public int left { get; set; }
        public int right { get; set; }
        public string repl_str { get; set; }
        public int ids { get; set; }
        public bool isProc { get; set; }
        public List<Parenthese> innerPar { get; set; }
        public List<string> innerWords { get; set; }
    }

    class Program
    {
        static List<string> specialIntendWords = new List<string>();
        static List<string> specialUnIntendWords = new List<string>();
        static List<string> specialWords = new List<string>();
        static Dictionary<string, string> comments = new Dictionary<string, string>();


        static string sql_main = "";

        static void InlineCommentDelete(ref string line, ref bool isComment)
        {
            line = line.Trim();
            if (String.IsNullOrEmpty(line)) // пропускаем пустые строки после удаления комментариев
            {
                line = "";
                return;
            }

            int multiCommentIndexStart = line.IndexOf("/*");
            int multiCommentIndexEnd = line.IndexOf("*/");
            int singleCommentIndex = line.IndexOf("--");

            if (isComment && multiCommentIndexEnd == -1) // если это строка мультистрочный комментарий и нет конца этого комментария, то пропускаем
            {
                line = "";
                return;
            }

            // проверяем, что есть мультистроковый комментарий и то, что строка не является комментарием и то, 
            // что единичный комментарий находится после мультистрокового или единичного комментарий вообще нет
            if (multiCommentIndexStart >= 0 && !isComment && (singleCommentIndex > multiCommentIndexStart || singleCommentIndex == -1))
            {
                isComment = true;
                if (multiCommentIndexEnd > 0) // если конец мультистрочного комментария в этой же строке, то вырезаем его
                {
                    isComment = false;
                    line = line.Substring(0, multiCommentIndexStart) + line.Substring(multiCommentIndexEnd + 2, line.Length - (multiCommentIndexEnd + 2));
                    InlineCommentDelete(ref line, ref isComment);
                    return;
                }
                else
                {
                    line = line.Substring(0, multiCommentIndexStart).Trim();
                    return;
                }
            }

            if (multiCommentIndexEnd >= 0 && isComment)
            {
                isComment = false;
                line = line.Substring(multiCommentIndexEnd + 2, line.Length - (multiCommentIndexEnd + 2));
                InlineCommentDelete(ref line, ref isComment);
                return;
            }

            // удаление единичного комментария
            singleCommentIndex = line.IndexOf("--");
            if (singleCommentIndex >= 0)
            {
                line = line.Substring(0, singleCommentIndex).Trim();
                return;
            }
        }

        static void InlineCommentHide(ref string line, ref bool isComment, ref string comment)
        {
            string key = "tnemmoc" + comments.Count() + "comment";
            line = line.Trim();
            if (String.IsNullOrEmpty(line)) // пропускаем пустые строки после удаления комментариев
            {
                line = "";
                return;
            }

            int multiCommentIndexStart = line.IndexOf("/*");
            int multiCommentIndexEnd = line.IndexOf("*/");
            int singleCommentIndex = line.IndexOf("--");

            if (isComment && multiCommentIndexEnd == -1) // если это строка мультистрочный комментарий и нет конца этого комментария, то пропускаем
            {
                comment = comment + "/r/n" + line;
                line = "";
                return;
            }

            // проверяем, что есть мультистроковый комментарий и то, что строка не является комментарием и то, 
            // что единичный комментарий находится после мультистрокового или единичного комментарий вообще нет
            if (multiCommentIndexStart >= 0 && !isComment && (singleCommentIndex > multiCommentIndexStart || singleCommentIndex == -1))
            {
                isComment = true;
                if (multiCommentIndexEnd > 0) // если конец мультистрочного комментария в этой же строке, то вырезаем его
                {
                    isComment = false;
                    comment = line.Substring(multiCommentIndexStart, multiCommentIndexEnd + 2);
                    comments.Add(key, comment);
                    line = line.Substring(0, multiCommentIndexStart) + " " + key + " " + line.Substring(multiCommentIndexEnd + 2, line.Length - (multiCommentIndexEnd + 2));
                    InlineCommentHide(ref line, ref isComment, ref comment);
                    return;
                }
                else
                {

                    line = line.Substring(0, multiCommentIndexStart).Trim();
                    return;
                }
            }

            if (multiCommentIndexEnd >= 0 && isComment)
            {
                isComment = false;
                comment = comment + "/r/n" + line.Substring(0, multiCommentIndexEnd);
                comments.Add(key, comment);
                line = key + " " + line.Substring(multiCommentIndexEnd + 2, line.Length - (multiCommentIndexEnd + 2));
                InlineCommentHide(ref line, ref isComment, ref comment);
                return;
            }

            // удаление единичного комментария
            singleCommentIndex = line.IndexOf("--");
            if (singleCommentIndex >= 0)
            {
                comment = line.Substring(singleCommentIndex, line.Length - singleCommentIndex);
                comments.Add(key, comment);
                line = line.Substring(0, singleCommentIndex).Trim() + " " + key;
                return;
            }
        }

        static int Instr(string text, string substring, int appereance)
        {
            int x_start = text.IndexOf(substring);
            if (appereance == 1)
                return x_start;
            int x_itter = appereance;
            while (x_itter > 1)
            {
                x_start = text.IndexOf(substring, x_start + 1);
                if (x_start < 0)
                    return -1;
                x_itter = x_itter - 1;
            }

            return x_start;
        }


        static void Main(string[] args)
        {
            string pathIn = @"D:\SQL\input.sql";
            string pathOut = @"D:\SQL\output.sql";
            Dictionary<int, string> words = new Dictionary<int, string>();
            List<Parenthese> parentheses = new List<Parenthese>();
            List<WriteLine> lines = new List<WriteLine>();

            StringComparison cmp = StringComparison.InvariantCultureIgnoreCase;
            bool isComment = false;
            string comment = "";
            specialIntendWords.Add("select");
            specialIntendWords.Add("from");
            specialIntendWords.Add("where");
            specialIntendWords.Add("by");

            using (StreamReader sr = new StreamReader(pathIn, System.Text.Encoding.Default))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (String.IsNullOrEmpty(line)) // пропускаем пустые строки
                        continue;
                    //InlineCommentDelete(ref line, ref isComment);
                    InlineCommentHide(ref line, ref isComment, ref comment);

                    line = line.Trim();
                    if (String.IsNullOrEmpty(line)) // пропускаем пустые строки после удаления комментариев
                        continue;

                    sql_main = sql_main + line + " ";
                }
                sql_main = sql_main.Trim();
                sql_main = sql_main.Replace("(", " ( ");
                sql_main = sql_main.Replace(")", " ) ");
                sql_main = sql_main.Replace("||", " || ");
                sql_main = sql_main.Replace("-", " - ");
                sql_main = sql_main.Replace("+", " + ");
                sql_main = sql_main.Replace(",", " , ");
                sql_main = sql_main.Replace(";", " ; ");
                sql_main = sql_main.Replace("'", " ' ");
                sql_main = sql_main.Replace("<", " < ");
                sql_main = sql_main.Replace(">", " > ");
                sql_main = sql_main.Replace("=", " = ");
                foreach (string sw in specialIntendWords)
                    sql_main = sql_main.Replace(sw, sw, cmp);

                for (int i = 0; i < 11; i++)
                {
                    sql_main = sql_main.Replace("  ", " ");
                }
            }

            string[] tempWords = sql_main.Split(' ');

            for (int i = 0; i < tempWords.Count(); i++)
                words.Add(i, tempWords[i]);

            // ищем все открывающие скобки
            int[] lpList =
                (
                from lp in words
                where
                    lp.Value.Equals("(")
                select
                    lp.Key
                ).ToArray();

            int iter = 0;
            foreach (int idx in lpList)
            {
                iter = iter++;
                /*
                 * Для каждой открывающей скобки ищем такую закрывающую, которая
                 * 1. будет после открывающей
                 * 2. количество открывающих и закрывающих скобок между ними будет равно
                 */
                int rp =
                    (
                    from w in words
                    where
                        w.Value.Equals(")")
                        && w.Key > idx
                        && words.Where(x => x.Key > idx && x.Key < w.Key && x.Value.Equals("(")).Count() == words.Where(x => x.Key > idx && x.Key < w.Key && x.Value.Equals(")")).Count()
                    orderby
                        w.Key
                    select
                        w.Key
                    ).First();
                parentheses.Add(new Parenthese { left = idx, right = rp, repl_str = "esehtnerap" + iter + "parenthese", ids = 0, isProc = false, innerPar = new List<Parenthese>() });
            }

            int tmpCount = parentheses.Count();

            for (int i = 0; i < tmpCount; i++)
            {
                Parenthese tmp = parentheses[i];
                tmp.innerPar = parentheses.Where(x => x.left > tmp.left && x.right < tmp.right
                && parentheses.Where(t => t.left > tmp.left && t.left < x.left).Count() == 0
                ).ToList();
            }


            int ids = 0;
            string tmpLine = "";
            for (int i = 0; i < words.Count(); i++)
            {
                string prevWord = "";
                string nextWord = "";
                string curWord = words[i];
                if (i != 0)
                    prevWord = words[i - 1];
                if (i != words.Count() - 1)
                    nextWord = words[i + 1];

                if (curWord.Equals("("))
                {
                    lines.Add(new WriteLine { line = curWord, intendSize = ids });
                    if (nextWord.Equals("select"))
                        ids += 1;
                    continue;
                }

                if (curWord.Equals("select"))
                {
                    lines.Add(new WriteLine { line = curWord, intendSize = ids });
                    ids += 2;
                    continue;
                }

                if (curWord.Equals("from"))
                {
                    lines.Add(new WriteLine { line = curWord, intendSize = ids - 2 });
                    continue;
                }

                if (curWord.Equals("by"))
                {
                    lines.Add(new WriteLine { line = prevWord + " " + curWord, intendSize = ids - 2 });
                    continue;
                }

                if (curWord.Equals("group") || curWord.Equals("order"))
                    continue;

                if (tmpLine is null)
                    tmpLine = curWord;
                else
                    tmpLine = tmpLine + " " + curWord;
                //lines.Add(new WriteLine { line = curWord, intendSize = ids});
            }

            using (StreamWriter w = new StreamWriter(pathOut, false, System.Text.Encoding.Default))
            {
                /*foreach (var t in parentheses)
                {
                    string line = t.left + "-" + t.right;
                    w.WriteLine(line);
                }*/
                //w.WriteLine(sql_main);

                /*foreach (var l in lines)
                {
                    w.WriteLine("".PadLeft(l.intendSize) + l.line);
                }*/

                /*int ids = 0;
                string line = " ";
                for (int i = 0; i < words.Count(); i++)
                {
                    if (words[i] == "from")
                    { ids = ids - 2; line = words[i]; }
                    else if (words[i] == "order")
                    { ids = ids - 2; line = words[i]; continue; }
                    else if (words[i] == "group")
                    { ids = ids - 2; line = words[i]; continue; }
                    else if (words[i] == "by")
                    { line = line + " " + words[i]; continue; }
                    else
                        line = words[i];

                    w.WriteLine("".PadLeft(ids) + line);
                    if (words[i] == "select")
                        ids = ids + 2;
                    if (words[i] == "from")
                        ids = ids + 2;
                    if (words[i] == "by")
                        ids = ids + 2;

                }*/

                /*foreach (var t in words)
                {
                    string line = t.Key + "." + t.Value;
                    w.WriteLine(line);
                }*/
            }




            /*using (StreamWriter w = new StreamWriter(pathOut, false, System.Text.Encoding.Default))
            {
                foreach (queryLine ql in queryLines)
                {
                    string line = "".PadLeft(ql.intendSize) + ql.Line;
                    w.WriteLine(line);
                }
            }*/
            Console.WriteLine("test");
        }
    }
}
