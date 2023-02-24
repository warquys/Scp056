using System.Collections.Generic;
using Neuron.Core.Meta;
using PlayerRoles;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Map;
using Synapse3.SynapseModule.Player;
using Synapse3.SynapseModule.Role;

namespace Scp056;

[Automatic]
[Role(
    Name = "Scp056",
    Id = 56,
    TeamId = (int)Team.SCPs
)]
public class Scp056PlayerScript : SynapseAbstractRole
{
    private readonly Scp056Plugin _plugin;
    private readonly CassieService _cassie;

    public Scp056PlayerScript(Scp056Plugin plugin, CassieService cassie)
    {
        _plugin = plugin;
        _cassie = cassie;
    }
        
    public override List<uint> GetEnemiesID() => new() { (int)Team.ClassD, (int)Team.FoundationForces, (int)Team.Scientists };

    public override List<uint> GetFriendsID() => _plugin.Config.Ff ? new List<uint>() : new List<uint> { (uint)Team.SCPs };

    protected override IAbstractRoleConfig GetConfig() => _plugin.Config.Scp056Configuration;

    protected override void OnSpawn(IAbstractRoleConfig config)
    {
        Player.SendHint(_plugin.Translation.Get(Player).Spawn.Replace("\\n", "\n"), 20);
        Player.FakeRoleManager.VisibleRole = new RoleInfo(_plugin.Config.Scp056Configuration.VisibleRole, null, null);
        RemoveCustomDisplay();
    }

    protected override void OnDeSpawn(DeSpawnReason reason)
    {
        if (reason is DeSpawnReason.Death or DeSpawnReason.Leave)
            _cassie.AnnounceScpDeath("056", CassieSettings.DisplayText, CassieSettings.Glitched, CassieSettings.Noise);

        Player.FakeRoleManager.VisibleRole = new RoleInfo(RoleTypeId.None, null, null);
    }

    public void SwapRole(RoleTypeId role)
    {
        if (!_plugin.Config.AllowedRoles.Contains(role)) return;
        
        Player.FakeRoleManager.VisibleRole = new RoleInfo(role, null, null);
        Player.SendHint(_plugin.Translation.ChangedRole.Replace("%role%", role.ToString()));
    }
}