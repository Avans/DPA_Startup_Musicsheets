using Common.Definitions;

namespace Common.Models
{
    public class Rest : Symbol
    {
        public Rest(Durations duration = Durations.Quarter) : base(duration)
        {
        }
    }
}