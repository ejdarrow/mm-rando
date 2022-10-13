using MMR.Randomizer.Extensions;
using MMR.Randomizer.GameObjects;
using MMR.Randomizer.Models.Rom;
using System.Collections.Generic;

namespace MMR.Randomizer.Utils
{
    public class EntranceSwapUtils
    {
        private static Dictionary<int, int> sceneSync = new Dictionary<int, int>()
            {
                { 69, 0 },     // swamp
                { 80, 90 },     // mountain village
                { 93, 94 },     // twin islands
                { 77, 72 }      // goron village
            };
        internal static void WriteNewEntrance(DungeonEntrance exit, DungeonEntrance newSpawn)
        {
            var spawnId = newSpawn.SpawnId();
            foreach (var exitInfo in exit.ExitIndices())
            {
                var sceneNumber = exitInfo.Item1;
                var exitIndex = exitInfo.Item2;
                EntranceUtils.WriteSceneExits(sceneNumber, exitIndex, spawnId);
                if (sceneSync.ContainsKey(sceneNumber))
                {
                    EntranceUtils.WriteSceneExits(sceneSync[sceneNumber], exitIndex, spawnId);
                }
            }
            foreach (var cutsceneExitInfo in exit.ExitCutscenes())
            {
                var sceneNumber = cutsceneExitInfo.Item1;
                var setupIndex = cutsceneExitInfo.Item2;
                var cutsceneIndex = cutsceneExitInfo.Item3;
                EntranceUtils.WriteCutsceneExits(sceneNumber, setupIndex, cutsceneIndex, spawnId);
                if (sceneSync.ContainsKey(sceneNumber))
                {
                    EntranceUtils.WriteCutsceneExits(sceneSync[sceneNumber], setupIndex, cutsceneIndex, spawnId);
                }
            }
            foreach (var address in exit.ExitAddresses())
            {
                var span = RomData.Files.GetSpanAt((uint)address);
                ReadWriteUtils.WriteU16(span, spawnId);
            }
        }

        internal static void WriteSpawnToROM(DungeonEntrance newSpawn)
        {
            var spawnAddress = newSpawn.SpawnId();
            ReadWriteUtils.WriteU16(RomData.Files.GetSpan(FileIndex.code.ToInt(), 0xBDB882), spawnAddress);
            ReadWriteUtils.WriteU16(RomData.Files.GetSpan(FileIndex.SPOT00.ToInt(), 0x02E90FD4), spawnAddress);
            ReadWriteUtils.WriteU16(RomData.Files.GetSpan(FileIndex.SPOT00.ToInt(), 0x02E90FDC), spawnAddress);
        }
    }
}
