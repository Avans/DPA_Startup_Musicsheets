using Common.Definitions;

namespace Common.Models
{
    public abstract class Symbol
    {
        public Durations Duration { get; set; }
        public int Dots { get; set; } = 0;

        protected Symbol(Durations duration)
        {
            Duration = duration;
        }
    }
}
