using System.Collections.Generic;
using SaveGame;
using SectionWords = System.Collections.Generic.Dictionary<string, WordPlacement>;

namespace Components
{
    public partial class GameController
    {
        private struct GameState : ISerializable
        {
            public Queue<Section> GeneratedFutureSections;
            public SectionWords CurrentSectionWords;
            public int CurrentSectionIndex;
            public int NewestGeneratedSectionIndex;
            public string CurrentSectionLetters;
            public int Score;
            public Queue<string> RecentlyFoundWords;
            public int BonusHintPoints;
            public bool FirstEverWordCompleted;
            public bool FirstEverHintUsed;

            public void Serialize(ReadOrWriteFileStream stream)
            {
                stream.Visit(ref CurrentSectionIndex);
                stream.Visit(ref NewestGeneratedSectionIndex);
                stream.Visit(ref CurrentSectionLetters);
                stream.Visit(ref CurrentSectionWords);
                stream.Visit(ref GeneratedFutureSections);
                stream.Visit(ref Score);
                stream.Visit(ref RecentlyFoundWords);
                stream.Visit(ref BonusHintPoints);

                // Version 1.1.1 and below
                if (stream.FileFormatVersion < 1)
                {
                    return;
                }

                stream.Visit(ref FirstEverWordCompleted);
                stream.Visit(ref FirstEverHintUsed);
            }
        }
    }
}