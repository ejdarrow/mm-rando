using MMR.Randomizer.Models.Rom;
using MMR.Rom;
using System.Collections.Generic;

namespace MMR.Randomizer
{
    public static class RomData
    {
        public static List<SequenceInfo> SequenceList { get; set; }
        public static List<InstrumentSetInfo> InstrumentSetList { get; set; }
        public static List<SequenceInfo> TargetSequences { get; set; }
        public static List<SequenceInfo> PointerizedSequences { get; set; }
        public static List<SequenceSoundSampleBinaryData> ListOfSamples { get; set; }
        public static int SamplesFileID { get; set; } = 0;
        public static FileTable Files { get; set; }
        public static List<Scene> SceneList { get; set; }
        public static Dictionary<int, GetItemEntry> GetItemList { get; set; }
        public static Dictionary<int, BottleCatchEntry> BottleList { get; set; }
    }
}
