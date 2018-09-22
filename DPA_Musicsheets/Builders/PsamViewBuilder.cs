using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DPA_Musicsheets.Exceptions;
using Common.Definitions;
using Common.Interfaces;
using Common.Models;
using PSAMControlLibrary;
using PSAMTimeSignature = PSAMControlLibrary.TimeSignature;
using PSAMNote = PSAMControlLibrary.Note;
using Note = Common.Models.Note;
using Rest = Common.Models.Rest;
using Clefs = Common.Definitions.Clefs;
using PSAMRest = PSAMControlLibrary.Rest;
using TimeSignature = Common.Models.TimeSignature;


namespace DPA_Musicsheets.Builders
{
    public class PsamViewBuilder : IViewBuilder<MusicalSymbol>
    {
        internal class NoteBeams
        {
            public Note Note { get; set; }
            public List<NoteBeamType> Beams { get; set; }

            internal NoteBeams()
            {
                Beams = new List<NoteBeamType>();
            }
        }

        private List<MusicalSymbol> _notes;
        private List<MusicalSymbol> _symbols;
        private List<NoteBeams> _buffer;

        private TimeSignature _meter;

        public PsamViewBuilder()
        {
            _notes = new List<MusicalSymbol>();
            _symbols = new List<MusicalSymbol>();
            _buffer = new List<NoteBeams>();
        }

        public void Reset()
        {
            _notes.Clear();
            _symbols.Clear();
            _buffer.Clear();
        }

        public void AddNote(Note note)
        {
            // set modifier; negative for flats, positive for sharp
            int modifier = 0;
            if (note.Modifier == Modifiers.Flat) modifier = -1;
            else if (note.Modifier == Modifiers.Sharp) modifier = 1;

            // set stem direction
            var direction = GetStemDirection(note);

            var amount = AmountOfBeams(note.Duration);
            if (amount > 0)
            {
                _buffer.Add(new NoteBeams
                {
                    Note = note,
                    Beams = Enumerable.Repeat(NoteBeamType.Single, amount).ToList()
                });

                if (_buffer.Count > 3)
                {
                    FlushBuffer();
                }
                return; // don't add anything till buffer is full or flushed
            }

            // if buffer has items, flush before adding new notes
            if (_buffer.Count > 0) FlushBuffer();

            var psamNote = new PSAMNote(note.Name.ToString(), modifier, (int)note.Octave,
                (MusicalSymbolDuration)note.Duration, direction, NoteTieType.None, new List<NoteBeamType> { NoteBeamType.Single });

            // set dots
            psamNote.NumberOfDots = note.Dots;

            _notes.Add(psamNote);
        }

        private void AddBeamedNotes(List<NoteBeams> noteBeams)
        {
            // set same directions
            var direction = noteBeams.Select(note => GetStemDirection(note.Note)).Max(d => d);

            // last note to end
            var last = noteBeams[noteBeams.Count - 1];
            for (var i = 0; i < last.Beams.Count; i++)
            {
                if (last.Beams[i] == NoteBeamType.Continue)
                {
                    last.Beams[i] = NoteBeamType.End;
                }
            }

            foreach (var note in noteBeams)
            {
                // set modifier; negative for flats, positive for sharp
                int modifier = 0;
                if (note.Note.Modifier == Modifiers.Flat) modifier = -1;
                else if (note.Note.Modifier == Modifiers.Sharp) modifier = 1;

                var psamNote = new PSAMNote(note.Note.Name.ToString(), modifier, (int)note.Note.Octave,
                    (MusicalSymbolDuration)note.Note.Duration, direction, NoteTieType.None, note.Beams);

                // set dots
                psamNote.NumberOfDots = note.Note.Dots;

                _notes.Add(psamNote);
            }
        }

        private void FlushBuffer()
        {
            var count = 1;

            // set first to 'start'
            var previous = _buffer[0];
            for (var i = 0; i < previous.Beams.Count; i++)
            {
                previous.Beams[i] = NoteBeamType.Start;
            }

            for (var i = 1; i < _buffer.Count; i++)
            {
                var note = _buffer[i];
                if (previous.Beams.Count <= note.Beams.Count)
                {
                    for (var j = 0; j < previous.Beams.Count; j++)
                    {
                        if (previous.Beams[j] != NoteBeamType.Start && previous.Beams[j] != NoteBeamType.Continue)
                        {
                            previous.Beams[j] = NoteBeamType.Continue;
                        }

                        if (_buffer.Count - 1 == i)
                        {
                            note.Beams[j] = NoteBeamType.End;
                        }
                        else
                        {
                            note.Beams[j] = NoteBeamType.Continue;
                        }
                    }

                    count++;
                }
                else
                {
                    // cut it off and beam them up
                    AddBeamedNotes(_buffer.GetRange(0, count));
                    _buffer = _buffer.GetRange(i, _buffer.Count - count);

                    FlushBuffer();
                    return;
                }

                // if it has more
                var amount = note.Beams.Count - previous.Beams.Count;
                if (amount > 0)
                {
                    NoteBeams next = null;
                    if (i + 1 < _buffer.Count - 1)
                    {
                        next = _buffer[i + 1];
                    }


                    for (var k = note.Beams.Count - 1; k >= amount; k--)
                    {
                        if (next != null)
                        {
                            if (next.Beams.Count >= note.Beams.Count)
                            {
                                note.Beams[k] = NoteBeamType.Start;
                            }
                            else
                            {
                                note.Beams[k] = NoteBeamType.BackwardHook;
                            }
                        }
                    }
                }
                previous = note;
            }

            AddBeamedNotes(_buffer);
            _buffer.Clear();
        }

        private NoteStemDirection GetStemDirection(Note note)
        {
            var direction = NoteStemDirection.Up;
            if (note.Octave >= Octaves.Five || (note.Name == Names.B && note.Octave == Octaves.Four))
            {
                direction = NoteStemDirection.Down;
            }

            return direction;
        }

        private int AmountOfBeams(Durations duration)
        {
            switch (duration)
            {
                case Durations.Eight:
                    return 1;
                case Durations.Sixteenth:
                    return 2;
                case Durations.ThirtySecond:
                    return 3;
                default:
                    return 0;
            }
        }

        public void AddRest(Rest rest)
        {
            var psamRest = new PSAMRest((MusicalSymbolDuration)rest.Duration);
            _notes.Add(psamRest);
        }

        public void AddClef(Clefs clef)
        {
            if (_buffer.Count > 0) FlushBuffer();
            if (_notes.Count > 0) Build();

            switch (clef)
            {
                case Clefs.Alto:
                    _symbols.Add(new Clef(ClefType.CClef, (int)clef));
                    break;
                case Clefs.Bass:
                    _symbols.Add(new Clef(ClefType.FClef, (int)clef));
                    break;
                case Clefs.Treble:
                    _symbols.Add(new Clef(ClefType.GClef, (int)clef));
                    break;
                default:
                    throw new ClefNotFoundException();
            }
        }

        public void AddTimeSignature(TimeSignature ts)
        {
            if (_buffer.Count > 0) FlushBuffer();
            if (_notes.Count > 0) Build();

            if (_meter != ts) // only add meter when it is different from the previous one
            {
                _symbols.Add(new PSAMTimeSignature(TimeSignatureType.Numbers, (uint)ts.Ticks,
                    (uint)ts.Beat));
                _meter = ts;
            }
        }

        /*
         * Als maatsoort 4/4 is, dan heeft een achtste noot een duur van 0,5 tellen.
         * Als een achtste noot één dot heeft, dan duurt de noot 0,5 + (0,5/2) = 0,75 tellen.
         * Als een achtste noot twee dots heeft, dan duurt de noot 0,75 + (0,25/2) = 0,875 tellen:
         */
        private double GetProgressDuration(double duration, int dots)
        {
            return GetProgressDurationHelper(duration, duration, dots);
        }

        private double GetProgressDurationHelper(double duration, double alterDuration, int dots)
        {
            if (dots == 0) return duration;
            return GetProgressDurationHelper(duration + (alterDuration / 2), (alterDuration / 2), --dots);
        }

        public IList<MusicalSymbol> Build()
        {
            double progress = _meter.Ticks; // set progress to ticks, e.g. 4

            foreach (var symbol in _notes)
            {
                if (symbol is PSAMNote note)
                {
                    var duration = GetProgressDuration((double) _meter.Beat / (double) note.Duration,
                        note.NumberOfDots);
                    progress -= duration; // subtract duration from progress                    
                }

                if (symbol is PSAMRest rest)
                {
                    var duration = GetProgressDuration((double)_meter.Beat / (double)rest.Duration,
                        rest.NumberOfDots);
                    progress -= duration; // subtract duration from progress    
                }

                _symbols.Add(symbol);

                if (progress <= 0) // draw barline when progress = 0
                {
                    _symbols.Add(new Barline());
                    progress = _meter.Ticks;
                }
            }

            _notes = new List<MusicalSymbol>();
            return _symbols;
        }
    }
}
