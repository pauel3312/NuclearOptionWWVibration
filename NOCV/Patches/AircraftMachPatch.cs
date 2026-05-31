using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using NOCV.Helpers;
using UnityEngine;

namespace NOCV.Patches;

/// <summary>
///     Adds vibration near mach
/// </summary>
[HarmonyPatch(typeof(Aircraft))]
public class AircraftMachPatch : VibChannelUser<AircraftMachPatch>
{
    /// <summary>
    ///     Adds vibration after ShakeAircraft call.
    /// </summary>
    /// <param name="instructions"></param>
    /// <returns></returns>
    [HarmonyPatch(nameof(Aircraft.FixedUpdate))]
    [HarmonyTranspiler]
    [HarmonyPriority(Priority.First)]
    public static IEnumerable<CodeInstruction> FixedUpdateTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        var shareAircraftMethod = AccessTools.Method(typeof(Aircraft), nameof(Aircraft.ShakeAircraft));

        foreach (var instr in instructions)
        {
            if (instr.Calls(shareAircraftMethod))
            {
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(AircraftMachPatch), nameof(StartVibration)));
            }

            yield return instr;
        }
    }

    private static float StartVibration(float num, Aircraft? aircraft)
    {
        if (aircraft != null && (aircraft.Player?.IsLocalPlayer ?? false))
        {
            Channel?.SetVibration(0f, num * PluginConfig.MachMultiplier.Value, 2 * Time.fixedDeltaTime);
        }

        return num;
    }
}