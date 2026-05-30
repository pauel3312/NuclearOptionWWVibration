using HarmonyLib;
using NOCV.Helpers;
using NuclearOption.Networking;
using UnityEngine;

namespace NOCV.Patches;


/// <summary>
/// Adds vibrations when stuff breaks.
/// </summary>
[HarmonyPatch(typeof(Unit))]
public class DetachPartPatch: VibChannelUser<DetachPartPatch>
{
    /// <summary>
    /// Adds vibration to detach part.
    /// </summary>
    /// <param name="__instance"></param>
    /// <param name="partID"></param>
    /// <param name="velocity"></param>
    /// <param name="relativePos"></param>
    [HarmonyPrefix]
    [HarmonyPatch(nameof(Unit.DetachPart))]
    public static void DetachPartPrefix(Unit? __instance, byte partID, Vector3 velocity, Vector3 relativePos)
    {
        if (!(__instance.GetPlayer()?.IsLocalPlayer ?? false)) return;
        Channel!.SetVibration(0f, PluginConfig.DetachPartVibrationValue.Value, PluginConfig.DetachPartVibrationDuration.Value);
    }
}