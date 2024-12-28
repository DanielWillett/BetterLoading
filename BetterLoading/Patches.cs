using DanielWillett.ReflectionTools;
using DanielWillett.ReflectionTools.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using SDG.Unturned;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace BetterLoading;

[HarmonyPatch]
internal static class Patches
{
    [HarmonyPatch(typeof(SpawnTableTool), "Resolve", typeof(Guid), typeof(EAssetType), typeof(Func<string>))]
    [HarmonyTranspiler]
    [UsedImplicitly]
    [SuppressMessage("CodeQuality", "IDE0051")]
    private static IEnumerable<CodeInstruction> SpawnTableResolveByGuidTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase method, ILGenerator generator)
    {
        return ReplaceLoadAllAssetsCheckTranspiler(instructions, method, generator);
    }

    [HarmonyPatch(typeof(SpawnTableTool), "Resolve", typeof(ushort), typeof(EAssetType), typeof(Func<string>))]
    [HarmonyTranspiler]
    [UsedImplicitly]
    [SuppressMessage("CodeQuality", "IDE0051")]
    private static IEnumerable<CodeInstruction> SpawnTableResolveByUInt16Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase method, ILGenerator generator)
    {
        return ReplaceLoadAllAssetsCheckTranspiler(instructions, method, generator);
    }

    [HarmonyPatch(typeof(SpawnTableTool), "Resolve", typeof(SpawnAsset), typeof(EAssetType), typeof(Func<string>))]
    [HarmonyTranspiler]
    [UsedImplicitly]
    [SuppressMessage("CodeQuality", "IDE0051")]
    private static IEnumerable<CodeInstruction> SpawnTableResolveByAssetTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase method, ILGenerator generator)
    {
        return ReplaceLoadAllAssetsCheckTranspiler(instructions, method, generator);
    }

    [HarmonyPatch(typeof(ItemManager), "generateItems")]
    [HarmonyTranspiler]
    [UsedImplicitly]
    [SuppressMessage("CodeQuality", "IDE0051")]
    private static IEnumerable<CodeInstruction> ItemManagerGenerateItemsTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase method, ILGenerator generator)
    {
        return ReplaceLoadAllAssetsCheckTranspiler(instructions, method, generator);
    }

    [HarmonyPatch(typeof(ItemManager), "respawnItems")]
    [HarmonyTranspiler]
    [UsedImplicitly]
    [SuppressMessage("CodeQuality", "IDE0051")]
    private static IEnumerable<CodeInstruction> ItemManagerRespawnItemsTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase method, ILGenerator generator)
    {
        return ReplaceLoadAllAssetsCheckTranspiler(instructions, method, generator);
    }

    [HarmonyPatch("SDG.Unturned.LegacyObjectRedirectorMap, Assembly-CSharp", "redirect")]
    [HarmonyTranspiler]
    [UsedImplicitly]
    [SuppressMessage("CodeQuality", "IDE0051")]
    private static IEnumerable<CodeInstruction> LegacyObjectRedirectorMapRedirectTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase method, ILGenerator generator)
    {
        return ReplaceLoadAllAssetsCheckTranspiler(instructions, method, generator);
    }

    [HarmonyPatch(typeof(LevelAnimals), "load")]
    [HarmonyTranspiler]
    [UsedImplicitly]
    [SuppressMessage("CodeQuality", "IDE0051")]
    private static IEnumerable<CodeInstruction> LevelAnimalsLoadTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase method, ILGenerator generator)
    {
        return ReplaceLoadAllAssetsCheckTranspiler(instructions, method, generator);
    }

    [HarmonyPatch(typeof(LevelGround), "loadTrees")]
    [HarmonyTranspiler]
    [UsedImplicitly]
    [SuppressMessage("CodeQuality", "IDE0051")]
    private static IEnumerable<CodeInstruction> LevelGroundLoadTreesTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase method, ILGenerator generator)
    {
        return ReplaceLoadAllAssetsCheckTranspiler(instructions, method, generator);
    }

    [HarmonyPatch(typeof(LevelItems), "load")]
    [HarmonyTranspiler]
    [UsedImplicitly]
    [SuppressMessage("CodeQuality", "IDE0051")]
    private static IEnumerable<CodeInstruction> LevelItemsLoadTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase method, ILGenerator generator)
    {
        return ReplaceLoadAllAssetsCheckTranspiler(instructions, method, generator);
    }

    [HarmonyPatch(typeof(LevelNodes), "load")]
    [HarmonyTranspiler]
    [UsedImplicitly]
    [SuppressMessage("CodeQuality", "IDE0051")]
    private static IEnumerable<CodeInstruction> LevelNodesLoadTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase method, ILGenerator generator)
    {
        return ReplaceLoadAllAssetsCheckTranspiler(instructions, method, generator);
    }

    [HarmonyPatch(typeof(LevelObjects), "load")]
    [HarmonyTranspiler]
    [UsedImplicitly]
    [SuppressMessage("CodeQuality", "IDE0051")]
    private static IEnumerable<CodeInstruction> LevelObjectsLoadTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase method, ILGenerator generator)
    {
        return ReplaceLoadAllAssetsCheckTranspiler(instructions, method, generator);
    }

    [HarmonyPatch(typeof(LevelVehicles), "load")]
    [HarmonyTranspiler]
    [UsedImplicitly]
    [SuppressMessage("CodeQuality", "IDE0051")]
    private static IEnumerable<CodeInstruction> LevelVehiclesLoadTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase method, ILGenerator generator)
    {
        return ReplaceLoadAllAssetsCheckTranspiler(instructions, method, generator);
    }

    [HarmonyPatch("SDG.Unturned.ServerMessageHandler_ValidateAssets, Assembly-CSharp", "ReadMessage")]
    [HarmonyTranspiler]
    [UsedImplicitly]
    [SuppressMessage("CodeQuality", "IDE0051")]
    private static IEnumerable<CodeInstruction> ServerMessageHandlerValidateAssetsReadMessageTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase method, ILGenerator generator)
    {
        return ReplaceLoadAllAssetsCheckTranspiler(instructions, method, generator);
    }

    [HarmonyPatch("SDG.Unturned.TreeRedirectorMap, Assembly-CSharp", "redirect")]
    [HarmonyTranspiler]
    [UsedImplicitly]
    [SuppressMessage("CodeQuality", "IDE0051")]
    private static IEnumerable<CodeInstruction> TreeRedirectorMapRedirectTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase method, ILGenerator generator)
    {
        return ReplaceLoadAllAssetsCheckTranspiler(instructions, method, generator);
    }

    [HarmonyPatch(typeof(VehicleManager), "onLevelLoaded")]
    [HarmonyTranspiler]
    [UsedImplicitly]
    [SuppressMessage("CodeQuality", "IDE0051")]
    private static IEnumerable<CodeInstruction> VehicleManagerOnLevelLoadedTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase method, ILGenerator generator)
    {
        return ReplaceLoadAllAssetsCheckTranspiler(instructions, method, generator);
    }

    private static IEnumerable<CodeInstruction> ReplaceLoadAllAssetsCheckTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase method, ILGenerator generator)
    {
        TranspileContext ctx = new TranspileContext(method, generator, instructions);

        MethodInfo? cmdLineFlagToBoolOp = Operators.FindCast<CommandLineFlag, bool>();
        if (cmdLineFlagToBoolOp == null)
        {
            CommandWindow.LogWarning($"Unable to patch {Accessor.Formatter.Format(method)}, unable to find CommandLineFlag to bool operator.");
            return ctx;
        }

        bool patched = false;
        while (ctx.MoveNext())
        {
            if (!ctx.Instruction.Calls(cmdLineFlagToBoolOp))
                continue;

            --ctx.CaretIndex;
            ctx.Replace(2, emit =>
            {
                emit.Invoke(Accessor.GetMethod(ShouldSendLog)!);
            });
            patched = true;
        }

        if (!patched)
        {
            CommandWindow.LogWarning($"Unable to patch {Accessor.Formatter.Format(method)}, injection point not found.");
        }

        return ctx;
    }

    private static bool ShouldSendLog()
    {
        return Assets.shouldLoadAnyAssets.value && BetterLoadingModule.Instance.LoadedAssets.Count == 0;
    }

    private delegate int IncrementInt(ref int value);

    [HarmonyPatch("SDG.Unturned.AssetsWorker+WorkerThreadState, Assembly-CSharp", "FindAssets")]
    [HarmonyTranspiler]
    [UsedImplicitly]
    [SuppressMessage("CodeQuality", "IDE0051")]
    private static IEnumerable<CodeInstruction> FindAssetsTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase method, ILGenerator generator)
    {
        TranspileContext ctx = new TranspileContext(method, generator, instructions);

        MethodInfo increment = Accessor.GetMethod(new IncrementInt(Interlocked.Increment))!;

        MethodInfo enumeratorMethod = typeof(IEnumerator).GetMethod("MoveNext", BindingFlags.Public | BindingFlags.Instance)!;

        int patched = 0;
        while (ctx.MoveNext())
        {
            if (!ctx.Instruction.Calls(increment))
                continue;

            ++patched;

            ctx.CaretIndex -= 3;
            // find last local (filePath)
            LocalReference lastLocal = default;
            for (int i = ctx.CaretIndex - 1; i >= 0; --i)
            {
                OpCode opc = ctx[i].opcode;
                if (!opc.IsStLoc() && !opc.IsLdLoc(either: true))
                {
                    continue;
                }

                LocalBuilder? lclB = PatchUtility.GetLocal(ctx[i], out int index, opc.IsStLoc());
                lastLocal = lclB != null ? new LocalReference(lclB) : new LocalReference(index);
                break;
            }

            if (lastLocal.Index < 0)
                throw new Exception($"Unable to patch AssetsWorker.WorkerThreadState.FindAssets, unable to find file path local time #{patched}.");

            Label ifNotAllowed = default;
            ctx.EmitAbove(emit =>
            {
                emit.LoadLocalValue(lastLocal)
                    .Invoke(Accessor.GetMethod(ShouldLoad)!)
                    .AddLabel(out Label lbl)
                    .BranchIfFalse(lbl);

                ifNotAllowed = lbl;
            });

            // find next leave, ret, or foreach iteration
            for (int i = ctx.CaretIndex; i < ctx.Count; ++i)
            {
                CodeInstruction ins = ctx[i];
                if (ins.Calls(enumeratorMethod))
                {
                    ctx.CaretIndex = i - 1;
                    break;
                }

                if (ins.opcode != OpCodes.Leave && ins.opcode != OpCodes.Leave_S && ins.opcode != OpCodes.Ret)
                    continue;

                ctx.CaretIndex = i;
                break;
            }

            ctx.MarkLabel(ifNotAllowed);
            ctx.ApplyBlocksAndLabels();
        }

        if (patched < 4)
        {
            throw new Exception("Unable to patch AssetsWorker.WorkerThreadState.FindAssets, unable to find calls to Interlocked.");
        }

        return ctx;
    }

    //[HarmonyPatch]
    //public static class ReaderThreadPatch
    //{
    //    [HarmonyTargetMethod]
    //    public static MethodInfo GetTargetMethod()
    //    {
    //        MethodInfo? entryPoint = Type.GetType("SDG.Unturned.AssetsWorker, Assembly-CSharp", throwOnError: false)?
    //            .GetMethod("ReaderThreadMain", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

    //        if (entryPoint != null && entryPoint.TryGetAttributeSafe(out AsyncStateMachineAttribute stateMachinePtr))
    //        {
    //            entryPoint = Accessor.GetImplementedMethod(stateMachinePtr.StateMachineType, typeof(IAsyncStateMachine).GetMethod(nameof(IAsyncStateMachine.MoveNext), BindingFlags.Public | BindingFlags.Instance)!);
    //        }

    //        if (entryPoint == null)
    //            throw new Exception("Unable to patch AssetsWorker.ReaderThreadMain, unable to find target method.");

    //        return entryPoint;
    //    }

    //    [HarmonyTranspiler]
    //    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase method, ILGenerator generator)
    //    {
    //        TranspileContext ctx = new TranspileContext(method, generator, instructions);

    //        Type? assetsWorkerThreadState = Type.GetType("SDG.Unturned.AssetsWorker+WorkerThreadState, Assembly-CSharp", throwOnError: false);
    //        if (assetsWorkerThreadState == null)
    //        {
    //            throw new Exception("Unable to patch AssetsWorker.ReaderThreadMain, unable to find AssetsWorker type.");
    //        }

    //        MethodInfo? addFoundAsset = assetsWorkerThreadState.GetMethod("AddFoundAsset", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
    //        if (addFoundAsset == null)
    //        {
    //            throw new Exception("Unable to patch AssetsWorker.ReaderThreadMain, unable to find AssetsWorker.WorkerThreadState.AddFoundAsset method.");
    //        }

    //        bool patched = false;
    //        while (ctx.MoveNext())
    //        {
    //            if (!ctx.Instruction.Calls(addFoundAsset))
    //                continue;

    //            patched = true;
    //            int startOfBlock = -1;
    //            for (int i = ctx.CaretIndex; i >= 0; --i)
    //            {
    //                if (ctx[i].opcode != OpCodes.Ldarg_0)
    //                    continue;

    //                startOfBlock = i;
    //                break;
    //            }

    //            if (startOfBlock == -1)
    //            {
    //                throw new Exception("Unable to patch AssetsWorker.ReaderThreadMain, unable to find ldarg.0.");
    //            }

    //            LocalReference lastLocal = default;

    //            FieldInfo? fieldToLoad = null;

    //            bool first = false;
    //            for (int i = ctx.CaretIndex; i >= 0; --i)
    //            {
    //                if (!ctx[i].IsLdloc())
    //                {
    //                    continue;
    //                }

    //                if (!first)
    //                {
    //                    first = true;
    //                    continue;
    //                }

    //                LocalBuilder? lb = PatchUtility.GetLocal(ctx[i], out int index, false);
    //                lastLocal = lb != null ? new LocalReference(lb) : new LocalReference(index);
    //                fieldToLoad = ctx[i + 1].operand as FieldInfo;
    //                break;
    //            }

    //            if (lastLocal.Index < 0)
    //                throw new Exception("Unable to patch AssetsWorker.ReaderThreadMain, unable to find state local index.");

    //            if (fieldToLoad == null)
    //                throw new Exception("Unable to patch AssetsWorker.ReaderThreadMain, unable to find filePath field.");

    //            ctx.CaretIndex = startOfBlock;
    //            Label ifNotAllowed = default;
    //            ctx.EmitAbove(emit =>
    //            {
    //                emit.LoadLocalAddress(lastLocal)
    //                    .LoadInstanceFieldValue(fieldToLoad)
    //                    .Invoke(Accessor.GetMethod(ShouldLoad)!)
    //                    .AddLabel(out Label lbl)
    //                    .BranchIfFalse(lbl);

    //                ifNotAllowed = lbl;
    //            });

    //            for (int i = ctx.CaretIndex; i < ctx.Count; ++i)
    //            {
    //                if (ctx[i].opcode != OpCodes.Leave && ctx[i].opcode != OpCodes.Leave_S)
    //                    continue;

    //                ctx.CaretIndex = i;
    //                break;
    //            }

    //            ctx.MarkLabel(ifNotAllowed);
    //            ctx.ApplyBlocksAndLabels();
    //            break;
    //        }

    //        if (!patched)
    //        {
    //            throw new Exception("Unable to patch AssetsWorker.ReaderThreadMain, unable to find call to AddFoundAsset.");
    //        }

    //        return ctx;
    //    }
    //}

    private static bool ShouldLoad(string assetFile)
    {
        return BetterLoadingModule.Instance.ShouldLoadFile(assetFile);
    }
}
