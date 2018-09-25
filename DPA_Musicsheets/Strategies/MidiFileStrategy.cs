using Common.Interfaces;
using DPA_Musicsheets.Managers;
using Sanford.Multimedia.Midi;

namespace DPA_Musicsheets.Strategies
{
    class MidiFileStrategy : IFileStrategy
    {
        private readonly IViewManagerPool _pool;
        public Sequence MidiSequence { get; set; }

        public MidiFileStrategy(IViewManagerPool pool)
        {
            _pool = pool;
        }

        public void Handle(string filename)
        {
            // TODO: Nadenken over de flow, want PSAMViewManager heeft een score nodig (dat wordt gegenereerd door een Sequence en MidiPlayer gebruikt juist de score om het om te zetten naar Midi (kortom twee keer hetzelfde gebeurd terwijl de sequence al hebben
            MidiSequence = new Sequence();
            MidiSequence.Load(filename);

            // MidiPlayerViewModel.MidiSequence = MidiSequence;

            // TODO: load lilypond text 
            // this.LilypondText = LoadMidiIntoLilypond(MidiSequence);
            // this.LilypondViewModel.LilypondTextLoaded(this.LilypondText);

            var score = MidiManager.Load(MidiSequence);
            foreach (var viewManager in _pool)
            {
                viewManager.Load(score);
            }
        }
    }
}
