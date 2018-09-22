using Common.Definitions;

namespace Common.Models
{
    public class Note : Symbol
    {
        public Modifiers? Modifier { get; set; }
        public Names Name { get; set; }
        public Octaves Octave { get; set; }

        public Note(Names name, Octaves octave = Octaves.Four, Durations duration = Durations.Quarter) : base(duration)
        {
            Name = name;
            Octave = octave;
        }
    }
}
