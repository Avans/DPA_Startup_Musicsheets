using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Common.Definitions;
using Common.Models;
using Sanford.Multimedia.Midi;

namespace DPA_Musicsheets.Managers
{
    public static class MidiManager
    {
        public static Score Load(Sequence sequence)
        {
            SymbolGroup symbolGroup = GetMetadataFromTrack(sequence[0]);
            symbolGroup.Symbols = GetSymbolsFromTrack(sequence[1], sequence.Division, symbolGroup.Meter);

            return new Score {
                SymbolGroups = { symbolGroup },
                Clef = Clefs.Treble
            };
        }

        private static List<Symbol> GetSymbolsFromTrack(Track track, int division, TimeSignature timeSignature)
        {
            var symbols = new List<Symbol>();
            int previousNoteAbsoluteTicks = 0;
            double percentageOfBarReached = 0;
            bool startedNoteIsClosed = true;

            foreach (var midiEvent in track.Iterator())
            {
                IMidiMessage midiMessage = midiEvent.MidiMessage;
                // TODO: Split this switch statements and create separate logic.
                // We want to split this so that we can expand our functionality later with new keywords for example.
                // Hint: Command pattern? Strategies? Factory method?
                if (midiMessage.MessageType != MessageType.Channel) continue;
                var channelMessage = midiEvent.MidiMessage as ChannelMessage;

                if (channelMessage?.Command == ChannelCommand.NoteOn)
                {
                    if (channelMessage.Data2 > 0) // Data2 = loudness
                    {
                        // check if there is time between the last note event and this one
                        var delta = midiEvent.AbsoluteTicks - previousNoteAbsoluteTicks;
                        if (delta > 0)
                        {
                            var rest = new Rest();
                            SetDuration(rest, timeSignature, previousNoteAbsoluteTicks, midiEvent.AbsoluteTicks,
                                division, out percentageOfBarReached);
                            symbols.Add(rest);
                            previousNoteAbsoluteTicks = midiEvent.AbsoluteTicks;
                        }

                        // Append the new note.
                        symbols.Add(GetNoteFromMidiKey(channelMessage.Data1));
                        startedNoteIsClosed = false;
                    }
                    else if (!startedNoteIsClosed)
                    {
                        // Finish the previous note with the length.
                        double percentageOfBar;

                        SetDuration(symbols[symbols.Count - 1],
                            timeSignature, previousNoteAbsoluteTicks, midiEvent.AbsoluteTicks, division, out percentageOfBar);

                        previousNoteAbsoluteTicks = midiEvent.AbsoluteTicks;

                        percentageOfBarReached += percentageOfBar;
                        if (percentageOfBarReached >= 1)
                        {
                            percentageOfBarReached -= 1;
                        }

                        startedNoteIsClosed = true;
                    }
                    else
                    {
                        symbols.Add(new Rest { Duration = Durations.Quarter });
                    }
                }
                else if (channelMessage?.Command == ChannelCommand.NoteOff)
                {
                    // Finish the previous note with the length.
                    double percentageOfBar;

                    SetDuration(symbols[symbols.Count - 1],
                        timeSignature, previousNoteAbsoluteTicks, midiEvent.AbsoluteTicks, division, out percentageOfBar);

                    previousNoteAbsoluteTicks = midiEvent.AbsoluteTicks;

                    percentageOfBarReached += percentageOfBar;
                    if (percentageOfBarReached >= 1)
                    {
                        percentageOfBarReached -= 1;
                    }

                    startedNoteIsClosed = true;
                }
            }
            return symbols;
        }

        private static SymbolGroup GetMetadataFromTrack(Track track)
        {
            var symbolGroup = new SymbolGroup();

            foreach (var e in track.Iterator())
            {
                var message = e.MidiMessage;
                if (message.MessageType != MessageType.Meta) continue;
                var metaMessage = message as MetaMessage;

                switch (metaMessage?.MetaType)
                {
                    case MetaType.TimeSignature:
                        var timeSignatureBytes = metaMessage.GetBytes();
                        symbolGroup.Meter = new TimeSignature
                        {
                            Ticks = timeSignatureBytes[0],
                            Beat = (Durations)(1 / Math.Pow(timeSignatureBytes[1], -2))
                        };
                        break;
                    case MetaType.Tempo:
                        var tempoBytes = metaMessage.GetBytes();
                        var tempo = (tempoBytes[0] & 0xff) << 16 | (tempoBytes[1] & 0xff) << 8 |
                                    (tempoBytes[2] & 0xff);
                        symbolGroup.Tempo = 60000000 / tempo; // bpm
                        break;
                        //                        case MetaType.EndOfTrack:
                        //                            if (previousNoteAbsoluteTicks > 0)
                        //                            {
                        //                                // Finish the last notelength.
                        //                                double percentageOfBar;
                        //                                lilypondContent.Append(MidiToLilyHelper.GetLilypondNoteLength(
                        //                                    previousNoteAbsoluteTicks, midiEvent.AbsoluteTicks, division, _beatNote,
                        //                                    _beatsPerBar, out percentageOfBar));
                        //                                lilypondContent.Append(" ");
                        //                        
                        //                                percentageOfBarReached += percentageOfBar;
                        //                                if (percentageOfBarReached >= 1)
                        //                                {
                        //                                    lilypondContent.AppendLine("|");
                        //                                    percentageOfBar = percentageOfBar - 1;
                        //                                }
                        //                            }
                        //                            break;
                }
            }

            return symbolGroup;
        }

        private static Note GetNoteFromMidiKey(int midiKey)
        {
            Names name;
            var octave = (Octaves)(midiKey / 12 - 1);
            Modifiers? modifier = null;

            switch (midiKey % 12)
            {
                case 0:
                    name = Names.C;
                    break;
                case 1:
                    name = Names.C;
                    modifier = Modifiers.Sharp;
                    break;
                case 2:
                    name = Names.D;
                    break;
                case 3:
                    name = Names.D;
                    modifier = Modifiers.Sharp;
                    break;
                case 4:
                    name = Names.E;
                    break;
                case 5:
                    name = Names.F;
                    break;
                case 6:
                    name = Names.F;
                    modifier = Modifiers.Sharp;
                    break;
                case 7:
                    name = Names.G;
                    break;
                case 8:
                    name = Names.G;
                    modifier = Modifiers.Sharp;
                    break;
                case 9:
                    name = Names.A;
                    break;
                case 10:
                    name = Names.A;
                    modifier = Modifiers.Sharp;
                    break;
                case 11:
                    name = Names.B;
                    break;
                default:
                    name = Names.C;
                    break;
            }

            var note = new Note(name, octave)
            {
                Modifier = modifier
            };

            return note;
        }

        public static Symbol SetDuration(Symbol symbol, TimeSignature timeSignature, int absoluteTicks, int nextNoteAbsoluteTicks, int division, out double percentageOfBar)
        {
            int duration = 0;
            int dots = 0;

            double deltaTicks = nextNoteAbsoluteTicks - absoluteTicks;

            if (deltaTicks <= 0)
            {
                percentageOfBar = 0;
                return symbol;
            }

            double percentageOfBeatNote = deltaTicks / division;
            percentageOfBar = (1.0 / timeSignature.Ticks) * percentageOfBeatNote;

            for (int noteLength = 32; noteLength >= 1; noteLength -= 1)
            {
                double absoluteNoteLength = (1.0 / noteLength);

                if (percentageOfBar <= absoluteNoteLength)
                {
                    if (noteLength < 2)
                        noteLength = 2;

                    int subtractDuration;

                    if (noteLength == 32)
                        subtractDuration = 32;
                    else if (noteLength >= 16)
                        subtractDuration = 16;
                    else if (noteLength >= 8)
                        subtractDuration = 8;
                    else if (noteLength >= 4)
                        subtractDuration = 4;
                    else
                        subtractDuration = 2;

                    if (noteLength >= 17)
                        duration = 32;
                    else if (noteLength >= 9)
                        duration = 16;
                    else if (noteLength >= 5)
                        duration = 8;
                    else if (noteLength >= 3)
                        duration = 4;
                    else
                        duration = 2;

                    double currentTime = 0;

                    while (currentTime < (noteLength - subtractDuration))
                    {
                        var addtime = 1 / ((subtractDuration / (int)timeSignature.Beat) * Math.Pow(2, dots));
                        if (addtime <= 0) break;
                        currentTime += addtime;
                        if (currentTime <= (noteLength - subtractDuration))
                        {
                            dots++;
                        }
                        if (dots >= 4) break;
                    }

                    break;
                }
            }

            symbol.Duration = (Durations)duration;
            symbol.Dots = dots;
            return symbol;
        }
    }
}