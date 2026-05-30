using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using NOCV.Helpers;
using UnityEngine;


namespace NOCV.Patches;

/// <summary>
///     Add vibration to bay doors. 
/// </summary>
[HarmonyPatch(typeof(BayDoor))]
public class BayDoorPatches: VibChannelUser<BayDoorPatches>
{
    private static void StartVibrationForBayDoor(BayDoor door)
    {
        if (!GameManager.IsLocalAircraft(door.GetComponentInParent<AeroPart>().parentUnit)) return;
        Channel!.SetVibration(PluginConfig.BayDoorVibrationValue.Value, 0f, door.doorAudioSource.clip.length);
    }
    
    /// <summary>
    /// Patches the Update method of the BayDoors in order to add the correct vibrations.
    /// </summary>
    /// <param name="instructions"></param>
    /// <returns></returns>
    [HarmonyTranspiler]
    [HarmonyPriority(Priority.First)]
    [HarmonyPatch(typeof(BayDoor), nameof(BayDoor.Update))]
    public static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        var playMethod = AccessTools.Method(typeof(AudioSource), nameof(AudioSource.Play));
        var vibMethod = AccessTools.Method(typeof(BayDoorPatches), nameof(StartVibrationForBayDoor));
        
        var previousCalls = false;
        foreach (var instr in instructions)
        {
            if (previousCalls)
            { 
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Call, vibMethod);
                previousCalls = false;
            }
            
            if (instr.Calls(playMethod))
            {
                previousCalls = true;
            }
            
            yield return instr;
        }
    }
}