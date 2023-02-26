using MEC;
using Neuron.Core.Events;
using Neuron.Core.Meta;
using PlayerRoles;
using Synapse3.SynapseModule;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Patching.Patches;
using Synapse3.SynapseModule.Player;
using System.Linq;
using UnityEngine;

namespace Scp056;

[Automatic]
public class EventHandler : Listener
{
    private readonly Scp056Plugin _plugin;
    private readonly PlayerService _player;

    public EventHandler(PlayerEvents playerEvents, RoundEvents roundEvents, PlayerService player,
        Scp056Plugin plugin)
    {
        _player = player;
        _plugin = plugin;
        playerEvents.KeyPress.Subscribe(KeyPress);
        playerEvents.SpeakToPlayer.Subscribe(SpeakToPlayer);
        roundEvents.FirstSpawn.Subscribe(FirstSpawn);
    }

    public void FirstSpawn(FirstSpawnEvent ev)
    {
        if (!_plugin.Config.EnableDefaultSpawnBehavior) return;
        if (_player.PlayersAmount < _plugin.Config.RequiredPlayers) return;
        if (Random.Range(1f, 100f) > _plugin.Config.SpawnChance) return;

        Timing.CallDelayed(0.1f, () => 
        {

            var scp079 = _player.Players.Any(p => p.RoleID == (uint)RoleTypeId.Scp079);
            var scpCount = _player.Players.Count(p => p.Team == Team.SCPs);

            foreach (var player in _player.Players) 
            {
                if (_plugin.Config.ReplaceScp)
                {
                    if (scpCount == 2 && scp079)
                    {
                        if (player.RoleID == (uint)RoleTypeId.Scp079)
                        {
                            player.RoleID = 56;
                            return;
                        }
                    }
                    else if (IsScpID(player.RoleID))
                    {
                        player.RoleID = 56;
                        return;
                    }
                }
                else if (!IsScpID(player.RoleID))
                {
                    player.RoleID = 56;
                    return;
                }
            }

        });
    }

    private bool IsScpID(uint id) => id is (uint)RoleTypeId.Scp173 or (uint)RoleTypeId.Scp049 or (uint)RoleTypeId.Scp0492
        or (uint)RoleTypeId.Scp079 or (uint)RoleTypeId.Scp096 or (uint)RoleTypeId.Scp106 or (uint)RoleTypeId.Scp939;


    private void SpeakToPlayer(SpeakToPlayerEvent ev)
    {
        if (ev.Receiver.RoleID == 56 && ev.Player.TeamID == (uint)Team.SCPs)
        {
            ev.Channel = VoiceChat.VoiceChatChannel.RoundSummary;
            ev.Allow = true;
        }
        else if (ev.Player.RoleID == 56)
        { 
            ev.Channel = ev.Receiver.TeamID == (uint)Team.SCPs 
                    ? VoiceChat.VoiceChatChannel.ScpChat
                    : VoiceChat.VoiceChatChannel.Proximity;
            ev.Allow = true;
        }
    }

    public void KeyPress(KeyPressEvent ev)
    {
        if (ev.Player.RoleID != 56) return;
        RoleTypeId role;

        switch (ev.KeyCode)
        {
            case KeyCode.Keypad1:
                role = RoleTypeId.ClassD;
                break;

            case KeyCode.Keypad2:
                role = RoleTypeId.Scientist;
                break;

            case KeyCode.Keypad3:
                role = RoleTypeId.FacilityGuard;
                break;

            case KeyCode.Keypad4:
                role = RoleTypeId.NtfSergeant;
                break;

            case KeyCode.Keypad5:
                role = RoleTypeId.ChaosRepressor;
                break;
            
            case KeyCode.Keypad6:
                role = RoleTypeId.Scp049;
                break;
            
            case KeyCode.Keypad7:
                role = RoleTypeId.Scp096;
                break;
            
            case KeyCode.Keypad8:
                role = RoleTypeId.Scp173;
                break;

            case KeyCode.Keypad9:
                role = RoleTypeId.Scp939;
                break;
                
            case KeyCode.Keypad0:
                var targets = _player
                    .GetPlayers(x => x.TeamID is (uint)Team.FoundationForces or (uint)Team.ClassD or (uint)Team.Scientists).Count;

                ev.Player.SendBroadcast(
                    _plugin.Translation.Get(ev.Player).Targets.Replace("%targets%", targets.ToString()), 7);
                return;

            default: return;
        }

        (ev.Player.CustomRole as Scp056PlayerScript)?.SwapRole(role);
    }
}