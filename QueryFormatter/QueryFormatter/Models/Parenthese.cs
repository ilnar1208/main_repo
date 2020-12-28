using System;
using System.Collections.Generic;
using System.Text;

namespace QueryFormatter.Models
{
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
}
