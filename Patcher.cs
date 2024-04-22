using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace ConfigCIPatcher {
    [HarmonyPatch("Microsoft.SecureBoot.UserConfig.VersionInfo", "BuildPFNDictionary")]
    class BuildPFNDictionaryPatcher {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            var codes = new List<CodeInstruction>(instructions);

            bool found = false;
            int? blockPoint = null;
            int? insertPoint = null;
            object lbl = null;
            for (var i = 0; i < codes.Count; i++) {
                var code = codes[i];
                if (code.blocks.Count > 0 && code.blocks[0].blockType == ExceptionBlockType.BeginCatchBlock && code.blocks[0].catchType == typeof(FileNotFoundException)) {
                    var leave = codes[i + 1];
                    if (leave.blocks.Count > 0 && leave.blocks[0].blockType == ExceptionBlockType.EndExceptionBlock) {
                        insertPoint = i + 2;
                        lbl = leave.operand;
                        for (var j = i; j >= 0; j--) {
                            if (codes[j].blocks.Count > 0 && codes[j].blocks.Any(x => x.blockType == ExceptionBlockType.BeginExceptionBlock)) {
                                blockPoint = j;
                                found = true;
                                break;
                            }
                        }
                    }
                }
            }
            if (found) {
                var beginBlk = new ExceptionBlock(ExceptionBlockType.BeginExceptionBlock);
                codes[blockPoint.Value].blocks.Add(beginBlk);

                var blk = new ExceptionBlock(ExceptionBlockType.BeginCatchBlock, typeof(ArgumentException));
                var pop = new CodeInstruction(OpCodes.Pop);
                pop.blocks.Add(blk);
                codes.Insert(insertPoint.Value, pop);

                var blk2 = new ExceptionBlock(ExceptionBlockType.EndExceptionBlock);
                var leave = new CodeInstruction(OpCodes.Leave_S, lbl);
                leave.blocks.Add(blk2);
                codes.Insert(insertPoint.Value + 1, leave);
            }
            return codes;
        }
    }

    public static class Patcher {
        public static void Patch() {
            var harmony = new Harmony("xyz.tudinh.configcipatcher");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
