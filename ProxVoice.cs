using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;

namespace ProxVoice;

[MinimumApiVersion(175)]
public class ProximityVoicePlugin : BasePlugin
{
	public override string ModuleName => "Proximity Voice";
	public override string ModuleVersion => "1.0.0";
	public override string ModuleAuthor => "ShiNxz & ChatGPT";
	public override string ModuleDescription => "Simple proximity-based voice chat using ListenOverride.";

	public static class ConVars
	{
		public static readonly FakeConVar<bool> PxEnable = new(
			"px_enable",
			"Enable/disable proximity voice (0/1).",
			true,
			ConVarFlags.FCVAR_NONE
		);
	}

	private const float ProximityRange = 800.0f;
	private const float ProximityRangeSq = ProximityRange * ProximityRange;

	private const int TicksPerUpdate = 3;
	private int _tickCounter = 0;

	public override void Load(bool hotReload)
	{
		RegisterFakeConVars(typeof(ConVars));

		Logger.LogInformation("[ProximityVoice] Loaded. px_enable = {PxEnable}", ConVars.PxEnable.Value);

		ConVars.PxEnable.ValueChanged += (_, value) =>
		{
			Logger.LogInformation("[ProximityVoice] px_enable changed to {PxEnable}", value);

			if (!value)
			{
				ResetAllOverrides();
			}
		};

		RegisterListener<Listeners.OnTick>(OnTick);
	}

	public override void Unload(bool hotReload)
	{
		ResetAllOverrides();
	}

	private void OnTick()
	{
		_tickCounter++;
		if (_tickCounter < TicksPerUpdate)
			return;

		_tickCounter = 0;

		if (!ConVars.PxEnable.Value)
			return;

		UpdateProximityVoice();
	}

	private void UpdateProximityVoice()
	{
		// All valid human players
		var allPlayers = Utilities.GetPlayers()
			.Where(p => p is { IsValid: true, IsBot: false, IsHLTV: false })
			.ToList();

		if (allPlayers.Count == 0)
			return;

		// Alive players with valid pawns (for proximity)
		var alivePlayers = allPlayers
			.Where(p => p.PlayerPawn.Value is { IsValid: true } && p.PawnIsAlive)
			.ToList();

		// 1) Reset overrides where either side is dead / no pawn
		ResetDeadOrSpectatorOverrides(allPlayers);

		// 2) Apply proximity only between alive players
		ApplyProximityForAlive(alivePlayers);
	}

	/// <summary>
	/// For every listener/speaker pair:
	/// if either has no valid pawn or is not alive, force override to Default.
	/// This stops dead players from being "stuck" as Hear from earlier.
	/// </summary>
	private void ResetDeadOrSpectatorOverrides(List<CCSPlayerController> allPlayers)
	{
		int count = allPlayers.Count;

		for (int i = 0; i < count; i++)
		{
			var listener = allPlayers[i];

			var listenerPawn = listener.PlayerPawn.Value;
			bool listenerAlive = listenerPawn is { IsValid: true } && listener.PawnIsAlive;

			for (int j = 0; j < count; j++)
			{
				var speaker = allPlayers[j];
				if (listener == speaker)
					continue;

				var speakerPawn = speaker.PlayerPawn.Value;
				bool speakerAlive = speakerPawn is { IsValid: true } && speaker.PawnIsAlive;

				// If either side isn't a live pawn, don't use proximity override between them
				if (!listenerAlive || !speakerAlive)
				{
					if (listener.GetListenOverride(speaker) != ListenOverride.Default)
					{
						listener.SetListenOverride(speaker, ListenOverride.Default);
					}
				}
			}
		}
	}

	/// <summary>
	/// Proximity logic strictly between alive players with valid pawns.
	/// Respects VoiceFlags.Muted on listener and speaker.
	/// </summary>
	private void ApplyProximityForAlive(List<CCSPlayerController> alivePlayers)
	{
		int count = alivePlayers.Count;
		if (count == 0)
			return;

		// Cache positions
		var positions = new Dictionary<CCSPlayerController, Vector>(count);
		foreach (var p in alivePlayers)
		{
			var pawn = p.PlayerPawn.Value!;
			positions[p] = pawn.AbsOrigin;
		}

		for (int i = 0; i < count; i++)
		{
			var listener = alivePlayers[i];

			// If listener is muted, don't touch their hearing
			if (listener.VoiceFlags.HasFlag(VoiceFlags.Muted))
				continue;

			var listenerPos = positions[listener];

			for (int j = 0; j < count; j++)
			{
				var speaker = alivePlayers[j];
				if (listener == speaker)
					continue;

				// If speaker is muted, respect that
				if (speaker.VoiceFlags.HasFlag(VoiceFlags.Muted))
					continue;

				var speakerPos = positions[speaker];

				float dx = listenerPos.X - speakerPos.X;
				float dy = listenerPos.Y - speakerPos.Y;
				float dz = listenerPos.Z - speakerPos.Z;
				float distSq = dx * dx + dy * dy + dz * dz;

				var desiredOverride = distSq <= ProximityRangeSq
					? ListenOverride.Hear
					: ListenOverride.Mute;

				var currentOverride = listener.GetListenOverride(speaker);

				if (currentOverride != desiredOverride)
				{
					listener.SetListenOverride(speaker, desiredOverride);
				}
			}
		}
	}

	private void ResetAllOverrides()
	{
		var players = Utilities.GetPlayers()
			.Where(p => p is { IsValid: true, IsBot: false, IsHLTV: false })
			.ToList();

		int count = players.Count;

		for (int i = 0; i < count; i++)
		{
			var listener = players[i];
			for (int j = 0; j < count; j++)
			{
				var speaker = players[j];
				if (listener == speaker)
					continue;

				if (listener.GetListenOverride(speaker) != ListenOverride.Default)
				{
					listener.SetListenOverride(speaker, ListenOverride.Default);
				}
			}
		}

		Logger.LogInformation("[ProximityVoice] All listen overrides reset.");
	}
}
