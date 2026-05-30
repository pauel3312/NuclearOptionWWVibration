using HarmonyLib;
using NOCV.Helpers;
using UnityEngine;

namespace NOCV.Patches;

/// <summary>
///     Patches the AoA feedback class to add vibration feedback.
/// </summary>
[HarmonyPatch(typeof(AoAFeedback))]
public class AoAFeedbackPatch: VibChannelUser<AoAFeedbackPatch>
{
    /// <summary>
    /// Main AoA feedback patch (this function alone is like 80% of the feeling of vibration feedback lol)
    /// </summary>
    /// <param name="aircraft"></param>
    [HarmonyPatch(nameof(AoAFeedback.RunAoAFeedback))]
    [HarmonyPostfix]
    public static void AoAFeedbackPostfix(Aircraft? aircraft)    
    {
        if (aircraft == null) return;
        if (aircraft.name is "AttackHelo1" or "UtilityHelo1")
        {
            var airspeed = aircraft.cockpit.rb.velocity -
                           NetworkSceneSingleton<LevelInfo>.i.GetWind(aircraft.cockpit.xform.GlobalPosition());
            var incomingAirspeed = aircraft.cockpit.xform.InverseTransformDirection(airspeed);
            var aoaDegrees = Mathf.Atan2(incomingAirspeed.y, incomingAirspeed.z) * 57.295780181884766;
            var speedFactor = Mathf.Max(aircraft.speed - AoAFeedback.aoaEffects.OnsetSpeed, 0.0f) /
                              (AoAFeedback.aoaEffects.FullVolumeSpeed - AoAFeedback.aoaEffects.OnsetSpeed);
            var AoAFactor = Mathf.Max(Mathf.Abs((float)aoaDegrees-10) - AoAFeedback.aoaEffects.OnsetAlpha, 0.0f) /
                            (AoAFeedback.aoaEffects.FullVolumeAlpha - AoAFeedback.aoaEffects.OnsetAlpha);
            // NOCV.Logger.LogDebug($"{aoaDegrees}: {AoAFactor}; {speedFactor}");
            Channel!.SetVibration(speedFactor * AoAFactor * PluginConfig.AoAMultiplier.Value, 0f);
        } else
        {
            Channel!.SetVibration(AoAFeedback.shake * (1 / AoAFeedback.aoaEffects.ShakeFactor) * PluginConfig.AoAMultiplier.Value, 0f);
        }    
    }
}