using System;
using System.Collections.Generic;
using System.Linq;
using NOCV.Features;
using UnityEngine;

namespace NOCV.Helpers;

internal sealed class VibForAudioSourceParams(AudioSource source, int motorIndex, float maxMagnitude)
{
    internal AudioSource Source = source;
    internal int MotorIndex = motorIndex;
    internal float MaxMagnitude = maxMagnitude;
}

/// <summary>
///     Starts vibration on audioSource play.
/// </summary>
public class VibOnAudioSources : MonoBehaviour
{
    private static readonly HashSet<VibForAudioSourceParams> PlayingSources = [];


    private static VibOnAudioSources? Instance { get; set; }

    private static VibrationChannel? _channel;

    /// <summary>
    ///     Initializes the service.
    /// </summary>
    public static void Initialize()
    {
        if (Instance != null) return;

        var go = new GameObject("VibOnAudioSourcesService");
        Instance = go.AddComponent<VibOnAudioSources>();
        if (_channel != null) return;
        _channel = new VibrationChannel();
        VibrationService.AddChannel(_channel);
    }

    /// <summary>
    ///     Destroy the vibration service
    /// </summary>
    public static void Destroy()
    {
        if (Instance == null)
            return;
        Destroy(Instance.gameObject);
        Instance = null;
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    ///     Start playing vibration from an audio source's clip.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="motorIndex"></param>
    /// <param name="maxMagnitude"></param>
    /// <returns> the given source for transpiler convenience </returns>
    public static AudioSource StartPlaying(AudioSource source, int motorIndex, float maxMagnitude)
    {
        // NOCV.Logger.LogDebug($"Added source. Now has {PlayingSources.Count} sources, with max magnitude {maxMagnitude}.");
        PlayingSources.Add(new VibForAudioSourceParams(source, motorIndex, maxMagnitude));
        return source;
    }

    private void FixedUpdate()
    {
        var sourcesToRemove = PlayingSources
            .Where(ps => ps.Source == null || !ps.Source.isPlaying).ToList();

        foreach (var source in sourcesToRemove)
        {
            // NOCV.Logger.LogDebug($"Removed source. Now has {PlayingSources.Count} sources.");
            _channel?.Disable();
            PlayingSources.Remove(source);
        }

        var motorMagnitudes = new Dictionary<int, float>();
        foreach (var playingSource in PlayingSources)
        {
            if (!motorMagnitudes.ContainsKey(playingSource.MotorIndex))
                motorMagnitudes[playingSource.MotorIndex] = 0;
            motorMagnitudes[playingSource.MotorIndex] += playingSource.MaxMagnitude * playingSource.Source.volume;
        }

        foreach (var kvp in motorMagnitudes)
        {
            var motor = kvp.Key;
            var magnitude = kvp.Value;
            _channel?.setVibrationOnMotorIndex(motor, magnitude);
        }
    }
}