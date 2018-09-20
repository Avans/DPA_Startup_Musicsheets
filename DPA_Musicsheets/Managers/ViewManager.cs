﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using DPA_Musicsheets.Exceptions;
using Notes.Definitions;
using Notes.Models;
using PSAMControlLibrary;
using Note = Notes.Models.Note;
using PSAMTimeSignature = PSAMControlLibrary.TimeSignature;
using PSAMNote = PSAMControlLibrary.Note;
using TimeSignature = Notes.Models.TimeSignature;

namespace DPA_Musicsheets.Managers
{
    public static class ViewManager
    {
        public static IList<MusicalSymbol> Load(Score score)
        {
            var viewSymbols = new List<MusicalSymbol>();
            var clef = CreateClefSymbol(score.Clef);
            viewSymbols.Add(clef);

            TimeSignature lastMeter = new TimeSignature { Beat = Durations.Quarter, Ticks = 4 }; // set default to bypass null checks
            foreach (var symbolGroup in score.SymbolGroups)
            {
                if (lastMeter != symbolGroup.Meter) // only add meter when it is different from the previous one
                {
                    viewSymbols.Add(new PSAMTimeSignature(TimeSignatureType.Numbers, (uint)symbolGroup.Meter.Ticks,
                        (uint)symbolGroup.Meter.Beat));
                    lastMeter = symbolGroup.Meter;
                }

                double progress = lastMeter.Ticks; // set progress to ticks, e.g. 4

                foreach (var symbol in symbolGroup.Symbols)
                {
                    if (symbol is Note note)
                    {
                        int modifier = 0;
                        if (note.Modifier == Modifiers.Flat)
                        {
                            modifier = -1;
                        }
                        else if (note.Modifier == Modifiers.Sharp)
                        {
                            modifier = 1;
                        }

                        var direction = NoteStemDirection.Up;
                        if (note.Octave >= Octaves.Five || (note.Name == Names.B && note.Octave == Octaves.Four))
                        {
                            direction = NoteStemDirection.Down;
                        }

                        var newestNote = new PSAMNote(note.Name.ToString(), modifier, (int)note.Octave,
                            (MusicalSymbolDuration)note.Duration, direction, NoteTieType.None,
                            new List<NoteBeamType> { NoteBeamType.Single });

                        viewSymbols.Add(newestNote);

                        progress -= (double)lastMeter.Beat / (double)note.Duration; // subtract duration from progress
                        if (progress <= 0) // draw barline when progress = 0
                        {
                            viewSymbols.Add(new Barline());
                            progress = lastMeter.Ticks;
                        }
                    }
                }
            }

            return viewSymbols;
        }

        /*
         * Create clef symbol based on our domain model
         */
        private static Clef CreateClefSymbol(Clefs clef)
        {
            switch (clef)
            {
                case Clefs.Alto:
                    return new Clef(ClefType.CClef, (int)clef);
                case Clefs.Bass:
                    return new Clef(ClefType.FClef, (int)clef);
                case Clefs.Treble:
                    return new Clef(ClefType.GClef, (int)clef);
                default:
                    throw new ClefNotFoundException();
            }
        }
    }
}
