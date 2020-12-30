using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using QueryFormatter.Models;

namespace QueryFormatter
{

    delegate void Operation(Action<string> oper);

    /// <summary>
    /// Обёртка над StringBuilder
    /// для того, чтобы у него был с StreamWriter был общий void метод WriteLine
    /// </summary>
    public class StringStream
    {
        public StringBuilder text { get; set; }

        public StringStream()
        {
            text = new StringBuilder();
        }

        public void Writeline(string line)
        {
            text.AppendLine(line);
        }
    }

    public class Formatter
    {
        static List<string> specialWords = "select,from,where,by,and,order,group,to_char,to_date,to_number,rownum,nvl,coalesce,trim,lpad,rpad,or,in,max,min,case,when,then,else,end,not,exists,trunc,as,substr,is,null,sum,fetch,first,rows,only,add_months,month,year,extract,start,with,connect,level,round,decode,distinct,count,months_between,table,upper,lower,union,all,grouping,sets,last_day,over,partition,having,cube,rollup".Split(',').ToList();
        static List<string> parNextLineWords = "from,where,as,select,sets".Split(',').ToList();
        static List<string> parGroupWords = "cube,rollup,grouping".Split(',').ToList();
        static List<string> parSpaceWords = ",;in;exists;case;else;when;then;=;and;join;>;<;group".Split(';').ToList();
        static List<string> fromFunctions = "trim,extract".Split(',').ToList();
        static List<string> joinTypes = "outer,inner,right,left,full,cross".Split(',').ToList();
        static List<string> prevWords = "select,from,by,where,with,sets".Split(',').ToList();
        static StringComparison cmp = StringComparison.InvariantCultureIgnoreCase;
        private Dictionary<string, string> comments;
        private Dictionary<int, string> words;
        private List<Parenthese> parentheses;
        private List<WriteWord> writeWords;
        private Dictionary<string, string> singleQuotes;
        private List<string> SQ;
        private string sql_main;

        private void CleanData()
        {
            comments = new Dictionary<string, string>();
            words = new Dictionary<int, string>();
            parentheses = new List<Parenthese>();
            writeWords = new List<WriteWord>();
            singleQuotes = new Dictionary<string, string>();
            SQ = new List<string>();
            sql_main = "";
        }


        /// <summary>
        /// Удаление комментариев
        /// </summary>
        /// <param name="line">Строка</param>
        /// <param name="isComment">Признак комментария</param>
        private void InlineCommentDelete(ref string line, ref bool isComment)
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

        /// <summary>
        /// Сокрытие комментарие
        /// </summary>
        /// <param name="line">СТрока</param>
        /// <param name="isComment">Признак комментария</param>
        /// <param name="comment">Комментария</param>
        private void InlineCommentHide(ref string line, ref bool isComment, ref string comment)
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
                comment = comment + "\r\n" + line;
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
                    comment = line.Substring(multiCommentIndexStart, (multiCommentIndexEnd + 2) - multiCommentIndexStart);
                    comments.Add(key, comment);
                    line = line.Substring(0, multiCommentIndexStart) + " " + /*key + " " +*/ line.Substring(multiCommentIndexEnd + 2, line.Length - (multiCommentIndexEnd + 2));
                    InlineCommentHide(ref line, ref isComment, ref comment);
                    return;
                }
                // если конца мультистрочного комментария в этой строке нет, то вырезаем и сохраняем его 
                else
                {
                    comment = line.Substring(multiCommentIndexStart, line.Length - multiCommentIndexStart);
                    line = line.Substring(0, multiCommentIndexStart).Trim();
                    return;
                }
            }

            // проверяем, что нашли конец мультистрочного комментария
            if (multiCommentIndexEnd >= 0 && isComment)
            {
                isComment = false;
                comment = comment + "\r\n" + line.Substring(0, multiCommentIndexEnd + 2);
                comments.Add(key, comment);
                line = /*key + " " +*/ line.Substring(multiCommentIndexEnd + 2, line.Length - (multiCommentIndexEnd + 2));
                InlineCommentHide(ref line, ref isComment, ref comment);
                return;
            }

            // удаление единичного комментария
            singleCommentIndex = line.IndexOf("--");
            if (singleCommentIndex >= 0)
            {
                comment = line.Substring(singleCommentIndex, line.Length - singleCommentIndex);
                comments.Add(key, comment);
                line = line.Substring(0, singleCommentIndex).Trim()/* + " " + key*/;
                return;
            }
        }

        /// <summary>
        /// Добавление слова
        /// </summary>
        /// <param name="index"></param>
        /// <param name="line"></param>
        private void AddWord(int index, WriteWord word)
        {
            if (index < 0)
                AddWordAtEnd(word);
            else
                AddWordAfter(index, word);
        }

        /// <summary>
        /// Добавить слово в конец списка
        /// </summary>
        /// <param name="word">Слово</param>
        private void AddWordAtEnd(WriteWord word)
        {
            writeWords.Add(word);
        }

        /// <summary>
        /// Добавить слово в заданное место
        /// </summary>
        /// <param name="index">Индекс месторасположения</param>
        /// <param name="word">Слово</param>
        private void AddWordAfter(int index, WriteWord word)
        {
            writeWords.Insert(index, word);
        }

        /// <summary>
        /// Вычислить отступ для заданного слова
        /// </summary>
        /// <param name="wordIndex">Индекс слова</param>
        /// <returns></returns>
        private int CalcIntendSize(int wordIndex)
        {
            List<WriteWord> wordList = new List<WriteWord>();
            // ищем все слова до начала строки
            for (int i = wordIndex - 1; i > -1; i--)
            {
                wordList.Add(writeWords[i]);
                if (writeWords[i].isNewLine)
                    break;
            }

            // если слов не нашли, то берём отступ у слова для которого вычисляли отступ
            if (wordList.Count() == 0)
                return writeWords[wordIndex].intendSize;
            else
            {
                int itdSize = 0;
                // собираем все слова в этой строке и вычисляем суммарный отступ
                foreach (var v in wordList)
                {
                    // если есть преобразование одинарных кавычек, то берём истинную длину слова
                    if (v.word.StartsWith("etouqelgnis"))
                    {
                        itdSize = itdSize + ("".PadLeft(v.intendSize) + singleQuotes[v.word]).Length;
                    }
                    else
                    {
                        itdSize = itdSize + ("".PadLeft(v.intendSize) + v.word).Length;
                    }

                }
                return itdSize + writeWords[wordIndex].intendSize;
            }
        }

        /// <summary>
        /// Определяем конец одинарной кавычки
        /// </summary>
        /// <param name="text"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        private int FindSingleQuoteEnd(string text, int start)
        {
            int end = text.IndexOf("'", start + 1);

            if (end < 0)
                throw new Exception("There is error with singleQuote");

            if (text[end + 1] != "'"[0])
            {
                return end;
            }
            else
            {
                return FindSingleQuoteEnd(text, end + 1);
            }
        }

        /// <summary>
        /// Обработка слов
        /// </summary>
        /// <param name="startIndex">Индекс начала</param>
        /// <param name="endIndex">Индекс конца</param>
        /// <param name="WrWordIndex">Индекс слова для записи</param>
        private void Process(int startIndex, int endIndex, int WrWordIndex)
        {
            int ids = 0; // размер отступа
            int insertIndex = -1; // индекс вставки текущего слова
            List<int> caseIds = new List<int>(); // отступы для вложенных case
            int curCaseIds = -1; // текуший отступ для case
            bool isWith = false; // признак того, что запрос вынесен в with
            string prevWordSt = ""; // предыдущее от startIndex слово
            string nextWordSt = ""; // следующее от startIndex слово

            if (startIndex != 0)
                prevWordSt = words[startIndex - 1];
            if (startIndex != words.Count() - 1)
                nextWordSt = words[startIndex + 1];

            if (WrWordIndex >= 0)
            {
                WriteWord tmpW = writeWords[WrWordIndex];
                if (tmpW.isNewLine)
                    ids = writeWords[WrWordIndex].intendSize;
                else
                    ids = CalcIntendSize(WrWordIndex) + 1; // Добавляем единицу из-за открывающей скобки
                writeWords.RemoveAt(WrWordIndex);
                insertIndex = WrWordIndex - 1;
            }

            for (int i = startIndex; i < endIndex; i++)
            {
                if (WrWordIndex >= 0)
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
                    if (WrWordIndex >= 0 && startIndex == i)
                    {
                        if (parNextLineWords.Where(x => x.Equals(prevWord, cmp)).Count() > 0)
                        {
                            AddWord(insertIndex, new WriteWord { word = curWord, intendSize = ids, isNewLine = true });
                            ids += 1;
                        }
                        else
                        {
                            if (parSpaceWords.Where(x => x.Equals(prevWord, cmp)).Count() > 0)
                                AddWord(insertIndex, new WriteWord { word = curWord, intendSize = 1, isNewLine = false });
                            else
                                AddWord(insertIndex, new WriteWord { word = curWord, intendSize = 0, isNewLine = false });
                        }
                    }
                    else
                    {
                        Parenthese par = parentheses.Where(p => p.left == i).First();
                        if (parNextLineWords.Where(x => x.Equals(prevWord, cmp)).Count() > 0)
                            AddWord(insertIndex, new WriteWord { word = par.repl_str, intendSize = ids, isNewLine = true });
                        else
                        {
                            if (parSpaceWords.Where(x => x.Equals(prevWord, cmp)).Count() > 0)
                                AddWord(insertIndex, new WriteWord { word = par.repl_str, intendSize = 1, isNewLine = false });
                            else
                                AddWord(insertIndex, new WriteWord { word = par.repl_str, intendSize = 0, isNewLine = false });
                        }
                        i = par.right;
                    }

                    continue;
                }

                // комментарий. Пока пропускаем
                if (curWord.StartsWith("tnemmoc"))
                {
                    int t = 1 + 1;
                }

                if (curWord.Equals("union", cmp))
                {
                    ids -= 2;
                    AddWord(insertIndex, new WriteWord { word = curWord, intendSize = ids, isNewLine = true });
                    continue;
                }

                if (curWord.Equals("with", cmp))
                {
                    if (prevWord.Equals("start"))
                    {
                        AddWord(insertIndex, new WriteWord { word = curWord, intendSize = 1, isNewLine = false });
                    }
                    else
                    {
                        AddWord(insertIndex, new WriteWord { word = curWord, intendSize = ids, isNewLine = true });
                        ids += 2;
                        isWith = true;
                    }

                    continue;
                }

                if (curWord.Equals("select", cmp))
                {
                    if (isWith)
                    {
                        ids -= 2;
                        isWith = false;
                    }

                    AddWord(insertIndex, new WriteWord { word = curWord, intendSize = ids, isNewLine = true });
                    ids += 2;
                    continue;
                }

                if (curWord.Equals("from", cmp))
                {
                    if (WrWordIndex >= 0 && fromFunctions.Where(x => x.Equals(prevWordSt, cmp)).Count() > 0 )
                        AddWord(insertIndex, new WriteWord { word = curWord, intendSize = 1, isNewLine = false });
                    else
                        AddWord(insertIndex, new WriteWord { word = curWord, intendSize = ids - 2, isNewLine = true });
                    continue;
                }

                if (joinTypes.Where(x => x.Equals(curWord, cmp)).Count() > 0
                    && joinTypes.Where(x => x.Equals(prevWord, cmp)).Count() == 0)
                {
                    AddWord(insertIndex, new WriteWord { word = curWord, intendSize = ids - 2, isNewLine = true });
                    continue;
                }

                if (curWord.Equals("join", cmp) && joinTypes.Where(x => x.Equals(prevWord, cmp)).Count() == 0)
                {
                    AddWord(insertIndex, new WriteWord { word = curWord, intendSize = ids - 2, isNewLine = true });
                    continue;
                }

                if (curWord.Equals("where", cmp))
                {
                    AddWord(insertIndex, new WriteWord { word = curWord, intendSize = ids - 2, isNewLine = true });
                    continue;
                }

                if (curWord.Equals("connect", cmp))
                {
                    AddWord(insertIndex, new WriteWord { word = curWord, intendSize = ids - 2, isNewLine = true });
                    continue;
                }

                if (curWord.Equals("start", cmp))
                {
                    AddWord(insertIndex, new WriteWord { word = curWord, intendSize = ids - 2, isNewLine = true });
                    continue;
                }

                if (curWord.Equals("case", cmp))
                {
                    if (WrWordIndex >= 0)
                    {
                        if (prevWord.Equals("("))
                        {
                            AddWord(insertIndex, new WriteWord { word = curWord, intendSize = 0, isNewLine = false });
                            curCaseIds = CalcIntendSize(insertIndex);
                        }
                        else
                        {
                            AddWord(insertIndex, new WriteWord { word = curWord, intendSize = 1, isNewLine = false });
                            curCaseIds = CalcIntendSize(insertIndex);
                        }
                    }
                    else
                    {
                        if (prevWord.Equals("("))
                        {
                            AddWord(insertIndex, new WriteWord { word = curWord, intendSize = 0, isNewLine = false });
                            curCaseIds = CalcIntendSize(writeWords.Count() - 1);
                        }
                        else
                        {
                            AddWord(insertIndex, new WriteWord { word = curWord, intendSize = 1, isNewLine = false });
                            curCaseIds = CalcIntendSize(writeWords.Count() - 1);
                        }
                    }

                    caseIds.Add(curCaseIds);
                    continue;
                }

                if (curWord.Equals("when", cmp) || curWord.Equals("else", cmp))
                {
                    AddWord(insertIndex, new WriteWord { word = curWord, intendSize = curCaseIds + 2, isNewLine = true });
                    continue;
                }

                if (curWord.Equals("then", cmp) || prevWord.Equals("else", cmp))
                {
                    AddWord(insertIndex, new WriteWord { word = curWord, intendSize = curCaseIds + 4, isNewLine = true });
                    continue;
                }

                if (curWord.Equals("end", cmp))
                {
                    AddWord(insertIndex, new WriteWord { word = curWord, intendSize = curCaseIds, isNewLine = true });
                    caseIds.RemoveAt(caseIds.Count() - 1);
                    if (caseIds.Count() > 0)
                        curCaseIds = caseIds.Last();
                    else
                        curCaseIds = -1;
                    continue;
                }

                if (curWord.Equals("group", cmp))
                {
                    if (prevWord.Equals("within", cmp))
                        AddWord(insertIndex, new WriteWord { word = curWord, intendSize = 1, isNewLine = false });
                    else
                        AddWord(insertIndex, new WriteWord { word = curWord, intendSize = ids - 2, isNewLine = true });
                    continue;
                }

                if (curWord.Equals("order", cmp))
                {
                    if (WrWordIndex >= 0 && (prevWordSt.Equals("group", cmp)))
                        AddWord(insertIndex, new WriteWord { word = curWord, intendSize = 0, isNewLine = false });
                    else
                        AddWord(insertIndex, new WriteWord { word = curWord, intendSize = ids - 2, isNewLine = true });
                    continue;
                }

                if (WrWordIndex >= 0 && fromFunctions.Where(x => x.Equals(prevWordSt, cmp)).Count() > 0)
                {
                    AddWord(insertIndex, new WriteWord { word = curWord, intendSize = 1, isNewLine = false });
                    continue;
                }

                if (curWord.Equals("by", cmp) || curWord.Equals("sets", cmp))
                {
                    AddWord(insertIndex, new WriteWord { word = curWord, intendSize = 1, isNewLine = false });
                    continue;
                }

                if (curWord.Equals("and", cmp) || curWord.Equals("or", cmp))
                {
                    if (curCaseIds >= 0)
                        AddWord(insertIndex, new WriteWord { word = curWord, intendSize = curCaseIds + 7, isNewLine = true });
                    else
                        AddWord(insertIndex, new WriteWord { word = curWord, intendSize = ids, isNewLine = true });
                    continue;
                }

                if (curWord.Equals(","))
                {
                    if (WrWordIndex < 0)
                    {
                        AddWord(insertIndex, new WriteWord { word = curWord, intendSize = ids, isNewLine = true });
                        continue;
                    }

                    if (WrWordIndex > 0)
                    {
                        if (nextWordSt.Equals("select", cmp) || prevWordSt.Equals("by", cmp))
                            AddWord(insertIndex, new WriteWord { word = curWord, intendSize = ids, isNewLine = true });
                        else
                            AddWord(insertIndex, new WriteWord { word = curWord, intendSize = 0, isNewLine = false });
                        continue;
                    }

                }

                if (prevWord.Equals("on", cmp))
                {
                    AddWord(insertIndex, new WriteWord { word = curWord, intendSize = ids, isNewLine = true });
                    continue;
                }

                if (prevWords.Where(x => x.Equals(prevWord, cmp)).Count() > 0)
                {
                    if (WrWordIndex >= 0 && fromFunctions.Where(x => x.Equals(prevWordSt, cmp)).Count() > 0)
                        AddWord(insertIndex, new WriteWord { word = curWord, intendSize = 1, isNewLine = false });
                    else
                    {
                        if(prevWord.Equals("by", cmp) && parGroupWords.Where(x => x.Equals(curWord, cmp)).Count() > 0)
                            AddWord(insertIndex, new WriteWord { word = curWord, intendSize = 1, isNewLine = false });
                        else
                            AddWord(insertIndex, new WriteWord { word = curWord, intendSize = ids, isNewLine = true });
                    }
                    continue;
                }
                else if (WrWordIndex >= 0 && prevWord.Equals("("))
                {
                    if (nextWordSt.Equals("select", cmp) && prevWordSt.Equals("where", cmp))
                    {
                        ids += 1;
                        AddWord(insertIndex, new WriteWord { word = curWord, intendSize = ids, isNewLine = true });
                    }
                    else
                        AddWord(insertIndex, new WriteWord { word = curWord, intendSize = 0, isNewLine = false });
                    continue;
                }
                else if (WrWordIndex >= 0 && curWord.Equals(")", cmp))
                {
                    if (prevWordSt.Equals("from", cmp) || prevWordSt.Equals("as", cmp) || prevWordSt.Equals("select", cmp))
                    {
                        AddWord(insertIndex, new WriteWord { word = curWord, intendSize = writeWords[startIndex].intendSize, isNewLine = true });
                    }
                    else if (nextWordSt.Equals("select", cmp)
                        && !(prevWordSt.Equals("from", cmp) || prevWordSt.Equals("where", cmp) || prevWordSt.Equals("select", cmp)))
                    {
                        AddWord(insertIndex, new WriteWord { word = curWord, intendSize = CalcIntendSize(startIndex), isNewLine = true });
                    }
                    else
                    {
                        AddWord(insertIndex, new WriteWord { word = curWord, intendSize = 0, isNewLine = false });
                    }
                    continue;
                }
                else
                    AddWord(insertIndex, new WriteWord { word = curWord, intendSize = 1, isNewLine = false });
            }
        }

        /// <summary>
        /// Получить данные из файла
        /// </summary>
        /// <param name="sqlInPath">Путь к файлу</param>
        private void GetDataFromFile(string sqlInPath)
        {
            bool isComment = false;
            string comment = "";

            using (StreamReader sr = new StreamReader(sqlInPath, Encoding.Default))
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
            }
        }

        /// <summary>
        /// Получить данные из строки
        /// </summary>
        /// <param name="sqlIn">Строка с запросом</param>
        private void GetDataFromString(string sqlIn)
        {
            bool isComment = false;
            string comment = "";
            string[] lines = sqlIn.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string l in lines)
            {
                string line;
                line = l.Trim();
                if (String.IsNullOrEmpty(line)) // пропускаем пустые строки
                    continue;
                //InlineCommentDelete(ref line, ref isComment);
                InlineCommentHide(ref line, ref isComment, ref comment);

                line = line.Trim();
                if (String.IsNullOrEmpty(line)) // пропускаем пустые строки после удаления комментариев
                    continue;

                sql_main = sql_main + line + " ";
            }
        }

        /// <summary>
        /// Разделить строку на слова и обработать
        /// </summary>
        private void ParseData()
        {
            int sinqleQuoIndex = sql_main.IndexOf("'");

            while (sinqleQuoIndex >= 0)
            {
                int end = FindSingleQuoteEnd(sql_main, sinqleQuoIndex);
                SQ.Add(sql_main.Substring(sinqleQuoIndex, end - sinqleQuoIndex + 1));
                sinqleQuoIndex = sql_main.IndexOf("'", end + 1);
            }

            SQ = SQ.Distinct().OrderByDescending(s => s.Length).ToList();

            int iterQ = 0;
            foreach (string s in SQ)
            {
                iterQ += 1;
                string keyQ = "etouqelgnis" + iterQ + "singlequote";
                singleQuotes.Add(keyQ, s);
                sql_main = sql_main.Replace(s, keyQ);
            }

            sql_main = sql_main.Replace("(", " ( ");
            sql_main = sql_main.Replace(")", " ) ");
            sql_main = sql_main.Replace("||", " || ");
            sql_main = sql_main.Replace("-", " - ");
            sql_main = sql_main.Replace("+", " + ");
            sql_main = sql_main.Replace(",", " , ");
            sql_main = sql_main.Replace(";", " ; ");
            sql_main = sql_main.Replace("<", " < ");
            sql_main = sql_main.Replace(">", " > ");
            sql_main = sql_main.Replace("=", " = ");
            sql_main = sql_main.Trim();

            /*foreach (string sw in specialWords)
                sql_main = sql_main.Replace(sw, sw, cmp);*/

            for (int i = 0; i < 11; i++)
            {
                sql_main = sql_main.Replace("  ", " ");
            }

            string[] tempWords = sql_main.Split(' ');

            for (int i = 0; i < tempWords.Count(); i++)
            {
                var tmpW = specialWords.Where(x => x.Equals(tempWords[i], cmp));
                if (tmpW.Count() > 0)
                    words.Add(i, tmpW.First());
                else
                    words.Add(i, tempWords[i]);
            }


            // ищем все открывающие скобки
            int[] lpList =
                (
                from lp in words
                where
                    lp.Value.Equals("(")
                select
                    lp.Key
                ).ToArray();

            int[] rpList =
                (
                from rp in words
                where
                    rp.Value.Equals(")")
                select
                    rp.Key
                ).ToArray();

            int iter = 0;
            //DateTime beginDate = DateTime.Now;
            foreach (int idx in lpList)
            {
                iter += 1;
                /*
                 * Для каждой открывающей скобки ищем такую закрывающую, которая
                 * 1. будет после открывающей
                 * 2. количество открывающих и закрывающих скобок между ними будет равно
                 */
                /*int rp =
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
                    ).First();*/
                int rp =
                    (
                    from p in rpList
                    where
                        p > idx
                        && lpList.Where(x => x > idx && x < p).Count() == rpList.Where(x => x > idx && x < p).Count()
                    orderby
                        p
                    select
                        p
                    ).First();
                parentheses.Add(new Parenthese { left = idx, right = rp, repl_str = "esehtnerap" + iter + "parenthese", ids = 0, isProc = false, innerPar = new List<Parenthese>() });
            }
            /*DateTime endDate = DateTime.Now;
            TimeSpan rez = endDate - beginDate;
            Console.WriteLine("{0}:{1}:{2}", rez.Minutes, rez.Seconds, rez.Milliseconds);
            Console.ReadLine();*/

            // ненужная часть

            /*int tmpCount = parentheses.Count();
            for (int i = 0; i < tmpCount; i++)
            {
                Parenthese tmp = parentheses[i];
                tmp.innerPar = parentheses.Where(x => x.left > tmp.left && x.right < tmp.right
                && parentheses.Where(t =>
                    t.left > tmp.left && t.right < tmp.right
                    && t.left > tmp.left && t.left < x.left).Count() == 0
                ).ToList();
            }*/
        }

        /// <summary>
        /// Форматирование слов
        /// </summary>
        private void ProcessData()
        {
            Process(0, words.Count(), -1);

            int index = writeWords.FindIndex(l => l.word.StartsWith("esehtnerap"));
            int iterCount = 0;

            while (index >= 0)
            {
                WriteWord tmpW = writeWords[index];
                Parenthese tmpP = parentheses.Where(p => p.repl_str.Equals(tmpW.word)).First();
                Process(tmpP.left, tmpP.right + 1, index);
                index = writeWords.FindIndex(l => l.word.StartsWith("esehtnerap"));
                iterCount += 1;
                if (iterCount >= 549)
                    break;
            }

            foreach (var sq in singleQuotes)
            {
                List<WriteWord> tmpLineList = writeWords.Where(l => l.word.Equals(sq.Key, cmp)).ToList();
                int lineCount = tmpLineList.Count();

                for (int i = 0; i < lineCount; i++)
                {
                    WriteWord tmp = tmpLineList[i];
                    tmp.word = sq.Value;
                }
            }
        }

        /// <summary>
        /// Сохранение результата
        /// </summary>
        /// <param name="writer">Действие для записи</param>
        private void SaveSQL(Action<string> writer)
        {

            /*foreach (var t in parentheses)
            {
                string line = t.left + "-" + t.right;
                writer(line);
            }*/

            string line = "";
            WriteWord nextWord = new WriteWord();
            WriteWord curWord;
            for (int l = 0; l < writeWords.Count(); l++)
            {
                curWord = writeWords[l];
                if (l != writeWords.Count() - 1)
                    nextWord = writeWords[l + 1];
                if (l == 0)
                    line = "".PadLeft(curWord.intendSize) + curWord.word;
                else
                    line = line + "".PadLeft(curWord.intendSize) + curWord.word;
                if (nextWord.isNewLine || l == writeWords.Count() - 1)
                {
                    writer(line);
                    line = "";
                }
            }

            /*foreach (var t in words)
            {
                string line = t.Key + "." + t.Value;
                writer(line);
            }*/
        }


        /// <summary>
        /// Сохранение комментариев
        /// </summary>
        /// <param name="writer">Действие для записи</param>
        private void SaveComments(Action<string> writer)
        {
            foreach (var t in comments)
            {
                string line = t.Value;
                writer(line);
            }
        }

        /// <summary>
        /// Сохранение в файл
        /// </summary>
        /// <param name="sqlOutPath">Путь к файлу</param>
        /// <param name="oper">Функция для записи</param>
        private void SaveToFile(string sqlOutPath, Operation oper)
        {
            using (StreamWriter stream = new StreamWriter(sqlOutPath, false, Encoding.Default))
            {
                Action<string> writer = stream.WriteLine;
                oper(writer);
            }
        }

        /// <summary>
        /// Сохранение в строку
        /// </summary>
        /// <param name="oper"></param>
        /// <returns>Строка с результатом</returns>
        private string SaveToString(Operation oper)
        {
            StringStream ss = new StringStream();
            Action<string> writer = ss.Writeline;
            oper(writer);
            return ss.text.ToString();
        }

        /// <summary>
        /// Форматирование запроса из файла
        /// </summary>
        /// <param name="sqlInPath">Путь к запросу</param>
        /// <param name="sqlOutPath">Путь к результату</param>
        /// <param name="commentsPath">Путь к комментариям</param>
        /// <param name="error">Ошибка</param>
        public bool FormateFromFile(string sqlInPath, string sqlOutPath, string commentsPath, out Exception error)
        {
            error = null;
            try
            {
                CleanData();
                GetDataFromFile(sqlInPath);
                ParseData();
                ProcessData();
                Operation oper = SaveSQL;
                SaveToFile(sqlOutPath, SaveSQL);
                oper = SaveComments;
                SaveToFile(commentsPath, oper);
            }
            catch (Exception ex)
            {
                error = ex;
                return false;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputSql">Текст sql-запроса</param>
        /// <param name="resultSql">Текст обработанного sql-запроса</param>
        /// <param name="comments">Текст комментарие</param>
        /// <param name="error">Ошибка</param>
        /// <returns></returns>
        public bool FormateFromString(string inputSql, out string resultSql, out string comments, out Exception error)
        {
            resultSql = string.Empty;
            comments = string.Empty;
            error = null;

            try
            {
                CleanData();
                GetDataFromString(inputSql);
                ParseData();
                ProcessData();
                Operation oper = SaveSQL;
                resultSql = SaveToString(oper);
                oper = SaveComments;
                comments = SaveToString(oper);
            }
            catch (Exception ex)
            {
                error = ex;
                return false;
            }
            return true;

        }
    }

}