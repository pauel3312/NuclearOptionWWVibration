using HarmonyLib;
using NOCV.Helpers;
using NuclearOption.Networking;
using UnityEngine;

namespace NOCV.Patches;

/// <summary>
/// Add vibration to missile launches
/// </summary>
[HarmonyPatch(typeof(MountedMissile))]
public class MountedMissilePatches: VibChannelUser<MountedMissilePatches>
{
    /// <summary>
    /// Add vibration on missile shots.
    /// </summary>
    /// <param name="__instance"></param>
    /// <param name="owner"></param>
    /// <param name="target"></param>
    /// <param name="inheritedVelocity"></param>
    /// <param name="weaponStation"></param>
    /// <param name="aimpoint"></param>
    [HarmonyPrefix]
    [HarmonyPatch(nameof(MountedMissile.Fire))]
    // ReSharper disable once InconsistentNaming
    public static void FirePatch(MountedMissile? __instance, Unit owner, Unit target, Vector3 inheritedVelocity,
        WeaponStation weaponStation, GlobalPosition aimpoint)
    {
        if (!(owner.GetPlayer()?.IsLocalPlayer ?? false) || (__instance?.fired ?? true)) return;
        Channel!.SetVibration(0f, PluginConfig.MissileFiringAmount.Value, PluginConfig.MissileFiringDuration.Value);
    }
}
