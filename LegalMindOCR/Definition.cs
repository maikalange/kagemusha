using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegalMindOCR
{
    public class Definition
    {
        public string Term { get; set; }
        public string Meaning { get; set; }
        public Definition(string term, string meaning)
        {
            Term = term;
            Meaning = meaning;
        }
    }
}
