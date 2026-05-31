using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using NOCV.Helpers;
using UnityEngine;

namespace NOCV.Patches;

/// <summary>
///     Adds vibration to all sorts of landing gear actions.
/// </summary>
[HarmonyPatch(typeof(LandingGear))]
public class GearVibrationPatch: VibChannelUser<GearVibrationPatch>
{
    /// <summary>
    ///     Adds vibration to latch events.
    /// </summary>
    /// <param name="__instance"></param>
    [HarmonyPrefix]
    [HarmonyPatch(nameof(LandingGear.PlayLatchSound))]
    public static void LatchSoundVibration(LandingGear? __instance)
    {
            if (__instance == null || !(__instance.aircraft.Player?.IsLocalPlayer ?? false) ||
                __instance.latchSound == null) return;
            Channel?.SetVibration(0f, PluginConfig.LatchGearVibrationAmount.Value, __instance.latchSound.length / 2);
    }

    /// <summary>
    /// Transpiles fixed update to add vibration on tire noise and tire skid
    /// </summary>
    /// <param name="instructions"></param>
    /// <returns></returns>
    [HarmonyTranspiler]
    [HarmonyPriority(Priority.First)]
    [HarmonyPatch(nameof(LandingGear.FixedUpdate))]
    public static IEnumerable<CodeInstruction> FixedUpdateTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        var audioSourcePlayMethod = AccessTools.Method(typeof(AudioSource), nameof(AudioSource.Play));
        var tireNoiseSourceField = AccessTools.Field(typeof(LandingGear), nameof(LandingGear.tireNoiseSound));
        var tireSlidSourceField = AccessTools.Field(typeof(LandingGear), nameof(LandingGear.tireSkidSound));

        var vibCallMethod = AccessTools.Method(typeof(GearVibrationPatch), nameof(CheckAndCallStartPlay));
        
        CodeInstruction prev = null!;
        
        foreach (var instr in instructions)
        {
            
            if (prev != null && instr.Calls(audioSourcePlayMethod))
            {
                if (prev.OperandIs(tireNoiseSourceField))
                {
                    yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                    yield return new CodeInstruction(OpCodes.Ldc_R4, PluginConfig.RollingVibMax.Value);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, vibCallMethod);
                } else if (prev.OperandIs(tireSlidSourceField))
                {
                    yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                    yield return new CodeInstruction(OpCodes.Ldc_R4, PluginConfig.SliddingVibMax.Value);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, vibCallMethod);
                }            }
            prev = instr;
            yield return instr;
        }
    }

    /// <summary>
    /// Add vibrations to the gear in/out animation.
    /// </summary>
    /// <param name="instructions"></param>
    /// <returns></returns>
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(LandingGear), nameof(LandingGear.LandingGear_OnSetGear))]
    [HarmonyPriority(Priority.First)]
    public static IEnumerable<CodeInstruction> OnsetGearTranspiler(IEnumerable<CodeInstruction> instructions) {
        var audioSourcePlayMethod = AccessTools.Method(typeof(AudioSource), nameof(AudioSource.Play));
        var foldSourceField = AccessTools.Field(typeof(LandingGear), nameof(LandingGear.foldSoundSource));
        var vibCallMethod = AccessTools.Method(typeof(GearVibrationPatch), nameof(CheckAndCallStartPlay));

        CodeInstruction prev = null!;

        foreach (var instr in instructions)
        {
            if (prev != null && instr.Calls(audioSourcePlayMethod) && prev.OperandIs(foldSourceField))
            {
                yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                yield return new CodeInstruction(OpCodes.Ldc_R4, PluginConfig.GearMovingMax.Value);
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Call, vibCallMethod);
            }
            
            prev = instr;
            yield return instr;
        }
    }

    private static AudioSource CheckAndCallStartPlay(AudioSource source, int motorIndex, float maxMagnitude, LandingGear gear)
    {
        if (gear.aircraft is null) return source;
        return !GameManager.IsLocalAircraft(gear.aircraft) ? source : VibOnAudioSources.StartPlaying(source, motorIndex, maxMagnitude);
    }
    
    
}