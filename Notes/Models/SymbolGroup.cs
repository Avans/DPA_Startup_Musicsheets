using System.Collections.Generic;

namespace Common.Models
{
    public class SymbolGroup
    {
        public List<Symbol> Symbols { get; set; }
        public int Tempo { get; set; }
        public TimeSignature Meter { get; set; }

        public Repeat Repeat { get; set; }

        public SymbolGroup()
        {
            Symbols = new List<Symbol>();
        }
    }
}
