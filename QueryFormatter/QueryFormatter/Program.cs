using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections.Generic;


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
        static Dictionary<int, string> words = new Dictionary<int, string>();
        static List<Parenthese> parentheses = new List<Parenthese>();
        static List<WriteLine> lines = new List<WriteLine>();
        static StringComparison cmp = StringComparison.InvariantCultureIgnoreCase;
        static Dictionary<string, string> singleQuotes = new Dictionary<string, string>();


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


        static void AddLine(int index, WriteLine line)
        {
            if (index < 0)
                AddLineAtEnd(line);
            else
                AddLineAfter(index, line);
        }


        static void AddLineAtEnd(WriteLine line)
        {
            lines.Add(line);
        }

        static void AddLineAfter(int index, WriteLine line)
        {
            lines.Insert(index, line);
        }

        static int CalcIntendSize(int lineIndex)
        {
            List<WriteLine> lineList = new List<WriteLine>();
            for(int i = lineIndex - 1; i > -1; i--)
            {
                lineList.Add(lines[i]);
                if (lines[i].isNewLine)
                    break;
            }

            if (lineList.Count() == 0)
                return lines[lineIndex].intendSize;
            else
            {
                int itdSize = 0;
                foreach(var v in lineList)
                {
                    itdSize = itdSize + ("".PadLeft(v.intendSize) + v.line).Length;
                }
                return itdSize + lines[lineIndex].intendSize;
            }
        }


        static void Process(int startIndex, int endIndex, int lineIndex)
        {
            int ids = 0;
            int insertIndex = -1;
            List<int> caseIds = new List<int>();
            int curCaseIds = -1;
            if (lineIndex >= 0)
            {
                WriteLine tmpL = lines[lineIndex];
                if (tmpL.isNewLine)
                    ids = lines[lineIndex].intendSize;
                else
                    ids = CalcIntendSize(lineIndex) +1; // Добавляем единицу из-за открывающей скобки
                lines.RemoveAt(lineIndex);
                insertIndex = lineIndex - 1;
            }    
            
            for (int i = startIndex; i < endIndex; i++)
            {
                if (lineIndex >= 0)
                    insertIndex = insertIndex + 1;
                string prevWord = "";
                string nextWord = "";
                string curWord = words[i];

                if (i != 0)
                    prevWord = words[i - 1];
                if (i != words.Count() - 1)
                    nextWord = words[i + 1];


                if (curWord.Equals("(", cmp))
                {
                    if (lineIndex >= 0 && startIndex == i)
                    {
                        if (prevWord.Equals("from", cmp) || prevWord.Equals("where", cmp))
                        {
                            AddLine(insertIndex, new WriteLine { line = curWord, intendSize = ids, isNewLine = true });
                            ids += 1;
                        }
                        else
                        {
                            if (prevWord.Equals(",", cmp) || prevWord.Equals("in", cmp) || prevWord.Equals("exists", cmp) || prevWord.Equals("case", cmp)
                                || prevWord.Equals("else", cmp) || prevWord.Equals("when", cmp) || prevWord.Equals("then", cmp) || prevWord.Equals("=", cmp)
                                 || prevWord.Equals("and", cmp))
                                AddLine(insertIndex, new WriteLine { line = curWord, intendSize = 1, isNewLine = false });
                            else
                                AddLine(insertIndex, new WriteLine { line = curWord, intendSize = 0, isNewLine = false });
                        }
                    }
                    else
                    {
                        Parenthese par = parentheses.Where(p => p.left == i).First();
                        if (prevWord.Equals("from", cmp) || prevWord.Equals("where", cmp))
                            AddLine(insertIndex, new WriteLine { line = par.repl_str, intendSize = ids, isNewLine = true });
                        else if (prevWord.Equals(",", cmp) || prevWord.Equals("in", cmp) || prevWord.Equals("exists", cmp) || prevWord.Equals("case", cmp)
                                 || prevWord.Equals("else", cmp) || prevWord.Equals("when", cmp) || prevWord.Equals("then", cmp) || prevWord.Equals("=", cmp)
                                 || prevWord.Equals("and", cmp))
                            AddLine(insertIndex, new WriteLine { line = par.repl_str, intendSize = 1, isNewLine = false });
                        else
                            AddLine(insertIndex, new WriteLine { line = par.repl_str, intendSize = 0, isNewLine = false });
                        i = par.right;
                    }

                    continue;
                }

                if (curWord.Equals("select", cmp))
                {
                    AddLine(insertIndex, new WriteLine { line = curWord, intendSize = ids, isNewLine = true });
                    ids += 2;
                    continue;
                }

                if (curWord.Equals("from", cmp))
                {
                    AddLine(insertIndex, new WriteLine { line = curWord, intendSize = ids - 2, isNewLine = true });
                    continue;
                }

                if (curWord.Equals("where", cmp))
                {
                    AddLine(insertIndex, new WriteLine { line = curWord, intendSize = ids - 2, isNewLine = true });
                    continue;
                }
                if (curWord.Equals("by", cmp))
                {
                    AddLine(insertIndex, new WriteLine { line = curWord, intendSize = 1, isNewLine = false });
                    continue;
                }

                if (curWord.Equals("case", cmp))
                {
                    if (lineIndex >= 0)
                    {
                        if(prevWord.Equals("("))
                        {
                            AddLine(insertIndex, new WriteLine { line = curWord, intendSize = 0, isNewLine = false });
                            curCaseIds = CalcIntendSize(insertIndex);
                        }
                        else
                        {
                            AddLine(insertIndex, new WriteLine { line = curWord, intendSize = 1, isNewLine = false });
                            curCaseIds = CalcIntendSize(insertIndex);
                        }
                    }
                    else
                    {
                        if (prevWord.Equals("("))
                        {
                            AddLine(insertIndex, new WriteLine { line = curWord, intendSize = 0, isNewLine = false });
                            curCaseIds = CalcIntendSize(lines.Count() - 1);
                        }
                        else
                        {
                            AddLine(insertIndex, new WriteLine { line = curWord, intendSize = 1, isNewLine = false });
                            curCaseIds = CalcIntendSize(lines.Count() - 1);
                        }
                    }
                    
                    caseIds.Add(curCaseIds);
                    continue;
                }

                if (curWord.Equals("when", cmp))
                {
                    AddLine(insertIndex, new WriteLine { line = curWord, intendSize = curCaseIds + 2, isNewLine = true });
                    continue;
                }

                if (curWord.Equals("then", cmp))
                {
                    AddLine(insertIndex, new WriteLine { line = curWord, intendSize = curCaseIds + 4, isNewLine = true });
                    continue;
                }

                if (curWord.Equals("else", cmp))
                {
                    AddLine(insertIndex, new WriteLine { line = curWord, intendSize = curCaseIds + 2, isNewLine = true });
                    continue;
                }

                if (prevWord.Equals("else", cmp))
                {
                    AddLine(insertIndex, new WriteLine { line = curWord, intendSize = curCaseIds + 4, isNewLine = true });
                    continue;
                }

                if (curWord.Equals("end", cmp))
                {
                    AddLine(insertIndex, new WriteLine { line = curWord, intendSize = curCaseIds, isNewLine = true });
                    caseIds.RemoveAt(caseIds.Count() - 1);
                    if (caseIds.Count() > 0)
                        curCaseIds = caseIds.Last();
                    else
                        curCaseIds = -1;
                    continue;
                }

                if (curWord.Equals("group", cmp) || curWord.Equals("order", cmp))
                {
                    AddLine(insertIndex, new WriteLine { line = curWord, intendSize = ids - 2, isNewLine = true });
                    continue;
                }

                if (curWord.Equals("by", cmp))
                {
                    AddLine(insertIndex, new WriteLine { line = curWord, intendSize = 1, isNewLine = false });
                    continue;
                }

                if (curWord.Equals("and", cmp))
                {
                    if (curCaseIds >= 0)
                        AddLine(insertIndex, new WriteLine { line = curWord, intendSize = curCaseIds + 7, isNewLine = true });
                    else
                        AddLine(insertIndex, new WriteLine { line = curWord, intendSize = ids, isNewLine = true });
                    continue;
                }
                if (curWord.Equals("or", cmp))
                {
                    AddLine(insertIndex, new WriteLine { line = curWord, intendSize = ids, isNewLine = true });
                    continue;
                }

                if (lineIndex < 0 && curWord.Equals(","))
                {
                    AddLine(insertIndex, new WriteLine { line = curWord, intendSize = ids, isNewLine = true });
                    continue;
                }

                if (lineIndex > 0 && curWord.Equals(","))
                {
                    if (words[startIndex + 1].Equals("select", cmp))
                        AddLine(insertIndex, new WriteLine { line = curWord, intendSize = ids, isNewLine = true });
                    else
                        AddLine(insertIndex, new WriteLine { line = curWord, intendSize = 0, isNewLine = false });
                    continue;
                }

                if (prevWord.Equals("select", cmp) || prevWord.Equals("from", cmp) || prevWord.Equals("by", cmp) || prevWord.Equals("where", cmp))
                {
                    AddLine(insertIndex, new WriteLine { line = curWord, intendSize = ids, isNewLine = true });
                }
                else if (lineIndex >= 0 && prevWord.Equals("("))
                {
                    if (words[startIndex + 1].Equals("select", cmp) && words[startIndex - 1].Equals("where", cmp))
                    {
                        ids += 1;
                        AddLine(insertIndex, new WriteLine { line = curWord, intendSize = ids, isNewLine = true });
                    }
                    else
                        AddLine(insertIndex, new WriteLine { line = curWord, intendSize = 0, isNewLine = false });
                }
                else if (lineIndex >= 0 && curWord.Equals(")", cmp))
                {
                    if (words[startIndex - 1].Equals("from", cmp)/* || words[startIndex - 1].Equals("where", cmp)*/)
                    {
                        AddLine(insertIndex, new WriteLine { line = curWord, intendSize = lines[startIndex].intendSize, isNewLine = true });
                    }
                    else if (words[startIndex + 1].Equals("select", cmp)
                        && !(words[startIndex - 1].Equals("from", cmp) || words[startIndex - 1].Equals("where", cmp)))
                    {
                        AddLine(insertIndex, new WriteLine { line = curWord, intendSize = CalcIntendSize(startIndex), isNewLine = true });
                    }
                    else
                    {
                        AddLine(insertIndex, new WriteLine { line = curWord, intendSize = 0, isNewLine = false });
                    }
                }
                else
                    AddLine(insertIndex, new WriteLine { line = curWord, intendSize = 1, isNewLine = false });
            }
        }

        static void Main(string[] args)
        {
            string pathIn = @".\SQL\input.sql";
            string pathOut = @".\SQL\output.sql";
            bool isComment = false;
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
                    InlineCommentDelete(ref line, ref isComment);
                    //InlineCommentHide(ref line, ref isComment, ref comment);

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

            int sinqleQuoIndex = sql_main.IndexOf("'");
            int iterQ = 0;

            while (sinqleQuoIndex >= 0)
            {
                iterQ += 1;
                int end = sql_main.IndexOf("'", sinqleQuoIndex + 1);
                string text = sql_main.Substring(sinqleQuoIndex, end - sinqleQuoIndex + 1);
                string keyQ = "etouqelgnis" + iterQ + "singlequote";
                singleQuotes.Add(keyQ, text);
                text = text.Replace(text, keyQ);
                sinqleQuoIndex = sql_main.IndexOf("'", sinqleQuoIndex + 1);
            }

            string[] tempWords = sql_main.Split(' ')   ;

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
                iter += 1;
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
                && parentheses.Where(t =>
                    t.left > tmp.left && t.right < tmp.right
                    && t.left > tmp.left && t.left < x.left).Count() == 0
                ).ToList();
            }

            Process(0, words.Count(), -1);

            int index = lines.FindIndex(l => l.line.StartsWith("esehtnerap"));
            int iterCount = 0;


            while (index >= 0)
            {
                WriteLine tmpL = lines[index];
                Parenthese tmpP = parentheses.Where(p => p.repl_str.Equals(tmpL.line)).First();
                Process(tmpP.left, tmpP.right + 1, index);
                index = lines.FindIndex(l => l.line.StartsWith("esehtnerap"));
                iterCount += 1;
                /*if (iterCount >= 2)
                    break;*/
            }

            foreach(var sq in singleQuotes)
            {
                List<WriteLine> tmpLineList = lines.Where(l => l.line.Equals(sq.Key, cmp)).ToList();
                int lineCount = tmpLineList.Count();

                for(int i = 0; i < lineCount; i++)
                {
                    WriteLine tmp = tmpLineList[i];
                    tmp.line = sq.Value;
                }
            }

            using (StreamWriter w = new StreamWriter(pathOut, false, System.Text.Encoding.Default))
            {
                /*foreach (var t in parentheses)
                {
                    string line = t.left + "-" + t.right;
                    w.WriteLine(line);
                }*/

                string line = "";
                WriteLine nextLine = new WriteLine();
                WriteLine curLine;
                for (int l = 0; l < lines.Count(); l++)
                {
                    curLine = lines[l];
                    if (l != lines.Count() - 1)
                        nextLine = lines[l + 1];
                    if (l == 0)
                        line = "".PadLeft(curLine.intendSize) + curLine.line;
                    else
                        line = line + "".PadLeft(curLine.intendSize) + curLine.line;
                    if (nextLine.isNewLine || l == lines.Count() - 1)
                    {
                        w.WriteLine(line);
                        line = "";
                    }
                }


                /*foreach (var t in words)
                {
                    string line = t.Key + "." + t.Value;
                    w.WriteLine(line);
                }*/
            }
        }
    }
}
