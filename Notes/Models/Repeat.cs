using System.Collections.Generic;

namespace Common.Models
{
    public class Repeat
    {
        public int Times { get; set; } // always bigger than 1
        public List<SymbolGroup> Alternatives { get; set; } 
        
        // Alternatives.Count could be different from Times, for example:
        /* -[1-3]-------[4]-------
         * | C D E F :| D C F E |
         */
        // this only has 2 alternatives, but Times = 4

        public Repeat()
        {
            Alternatives = new List<SymbolGroup>();
        }
    }
}
