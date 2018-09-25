using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Definitions;
using Common.Exceptions;
using Common.Interfaces;
using Common.Models;
using DPA_Musicsheets.ViewModels;
using Sanford.Multimedia.Midi;

namespace DPA_Musicsheets.Managers.View
{
    class MidiPlayerViewManager : IViewManager
    {
        private MidiPlayerViewModel _viewModel;

        public MidiPlayerViewManager()
        {
        }

        public void RegisterViewModel(MidiPlayerViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public void Load(Score score)
        {
            if (_viewModel == null) throw new ViewModelNotFoundException();

            List<string> notesOrderWithCrosses = new List<string>() { "c", "cis", "d", "dis", "e", "f", "fis", "g", "gis", "a", "ais", "b" };
            int absoluteTicks = 0;

            Sequence sequence = new Sequence();

            Track metaTrack = new Track();
            sequence.Add(metaTrack);

            foreach (var symbolGroup in score.SymbolGroups)
            {
                // TODO: Matthias klopt de symbolGroup.Meter?
                int _bpm = 120;                                 // Aantal beatnotes per minute.
                int _beatNote = (int)symbolGroup.Meter.Beat;    // De waarde van een beatnote.
                int _beatsPerBar = symbolGroup.Meter.Ticks;     // Aantal beatnotes per maat.

                int speed = (60000000 / _bpm);
                byte[] tempo = new byte[3];
                tempo[0] = (byte)((speed >> 16) & 0xff);
                tempo[1] = (byte)((speed >> 8) & 0xff);
                tempo[2] = (byte)(speed & 0xff);

                metaTrack.Insert(0 /* Insert at 0 ticks*/, new MetaMessage(MetaType.Tempo, tempo));

                byte[] timeSignature = new byte[4];
                timeSignature[0] = (byte)_beatsPerBar;
                timeSignature[1] = (byte)(Math.Log(_beatNote) / Math.Log(2));
                metaTrack.Insert(absoluteTicks, new MetaMessage(MetaType.TimeSignature, timeSignature));

                Track notesTrack = new Track();
                sequence.Add(notesTrack);

                foreach (var symbol in symbolGroup.Symbols)
                {
                    // Calculate duration
                    double absoluteLength = 1.0 / (double)symbol.Duration;
                    absoluteLength += (absoluteLength / 2.0) * symbol.Dots;

                    double relationToQuartNote = _beatNote / 4.0;
                    double percentageOfBeatNote = (1.0 / _beatNote) / absoluteLength;
                    double deltaTicks = (sequence.Division / relationToQuartNote) / percentageOfBeatNote;

                    // Calculate height
                    // TODO: Wat doen we met rust?
                    if (symbol is Note note)
                    {

                        int noteHeight = notesOrderWithCrosses.IndexOf(note.Name.ToString().ToLower()) + ((int)note.Octave + 1) * 12;

                        int modifier = 0;
                        if (note.Modifier == Modifiers.Flat) modifier = -1;
                        else if (note.Modifier == Modifiers.Sharp) modifier = 1;

                        noteHeight += modifier;
                        notesTrack.Insert(absoluteTicks, new ChannelMessage(ChannelCommand.NoteOn, 1, noteHeight, 90)); // Data2 = volume

                        absoluteTicks += (int)deltaTicks;
                        notesTrack.Insert(absoluteTicks, new ChannelMessage(ChannelCommand.NoteOn, 1, noteHeight, 0)); // Data2 = volume
                    }
                }

                notesTrack.Insert(absoluteTicks, MetaMessage.EndOfTrackMessage);
                metaTrack.Insert(absoluteTicks, MetaMessage.EndOfTrackMessage);
            }

            _viewModel.MidiSequence = sequence;
        }
    }
}
