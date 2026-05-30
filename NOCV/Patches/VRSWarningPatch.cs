using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NOCV.Helpers;

namespace NOCV.Patches;

/// <summary>
/// Adds vibration feedback to the VRS warning
/// </summary>
[HarmonyPatch(typeof(VRSWarning))]
// ReSharper disable once InconsistentNaming
public class VRSWarningPatch: VibChannelUser<VRSWarningPatch>
{
    private static bool _isVibOn;
    // ReSharper disable once InconsistentNaming
    private static void VRSVibration(float value)
    {
        if (_isVibOn && value < PluginConfig.VRSThreshold.Value)
        {
            _isVibOn = false;
            Channel!.Disable();
            return;
        }
        if (value < PluginConfig.VRSThreshold.Value) return;
        _isVibOn = true;
        Channel!.SetVibration(0, value*PluginConfig.VRSMult.Value);
    }

    
    /// <summary>
    /// Adds vibration feedback on Refresh of the VRS detector. 
    /// </summary>
    /// <param name="instructions"></param>
    /// <returns></returns>
    [HarmonyPatch(nameof(VRSWarning.Refresh))]
    [HarmonyPriority(Priority.First)]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> RefreshTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        // ReSharper disable once InconsistentNaming
        var VRSFactorField = AccessTools.Field(typeof(VRSWarning), nameof(VRSWarning.VRSFactor));
    
        CodeInstruction? prev = null;
    
        foreach (var instr in instructions)
        {
            if (prev != null)
            {
                if (prev.opcode == OpCodes.Div &&
                    instr.opcode == OpCodes.Stfld &&
                    (FieldInfo)instr.operand == VRSFactorField)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, VRSFactorField);
                    yield return new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(VRSWarningPatch), nameof(VRSVibration)));
                }
            }            
            prev = instr;
            yield return instr;
        }
    
    }
}