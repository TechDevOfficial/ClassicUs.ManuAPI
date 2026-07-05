using System;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace ClassicUs.ManuAPI
{
    public abstract class CustomRole
    {
        public abstract string DisplayName { get; }
        public abstract string RoleTypeName { get; }
        public abstract RoleTeamTypes TeamType { get; }

        public virtual bool CanVent => false;
        public virtual bool CanSabotage => false;
        public virtual bool CanUseKillButton => false;
        public virtual int Count => 1;
        public virtual float RoleChancePercent => 100f;

        public virtual string Description => $"You are {DisplayName}.";
        public virtual string DescriptionShort => DisplayName;
        public virtual Color TeamColor => TeamType == RoleTeamTypes.Impostor
            ? new Color(1f, 0f, 0f, 1f)
            : new Color(0.3f, 0.6f, 1f, 1f);
        public virtual string KillAbilityName => "Kill";
        public virtual string KillAbilityImageName => string.Empty;
        public virtual RoleTeamTypes[] EnemyTeams
        {
            get
            {
                if (!CanUseKillButton) return Array.Empty<RoleTeamTypes>();
                var all = new[] { RoleTeamTypes.Crewmate, RoleTeamTypes.Impostor, RoleTeamTypes.Neutral };
                var enemies = new RoleTeamTypes[all.Length - 1];
                int idx = 0;
                foreach (var t in all)
                    if (t != TeamType) enemies[idx++] = t;
                return enemies;
            }
        }

        public virtual void OnAssigned(RoleBehaviour role, PlayerControl player) { }
        public virtual string EjectionText(string playerName) => null;

        internal string AssignedRoleName { get; set; }
        internal bool IsRegisteredInRoleManager => AssignedRoleName != null;

        public bool Matches(RoleBehaviour role)
        {
            if (role == null) return false;
            if (role.GetIl2CppType().Name == RoleTypeName) return true;
            return AssignedRoleName != null && role.roleCodeName == AssignedRoleName;
        }

        internal void Apply(RoleBehaviour role, PlayerControl player)
        {
            try
            {
                role.RoleTeamType = TeamType;
                role.CanVent = CanVent;
                role.CanSabotage = CanSabotage;
                role.CanUseKillButton = CanUseKillButton;
                ApplyEnemyTeams(role, EnemyTeams);
                OnAssigned(role, player);
            }
            catch (Exception e)
            {
                ManuAPIPlugin.Log.LogError("CustomRole.Apply (" + RoleTypeName + "): " + e);
            }
        }

        private static void ApplyEnemyTeams(RoleBehaviour role, RoleTeamTypes[] configured)
        {
            var template = GetTemplateEnemyTeams(role.RoleTeamType);
            if (template != null)
            {
                role.enemyTeams = template;
                return;
            }

            var enemies = role.enemyTeams;
            if (enemies != null && enemies.Length > 0) return;
            if (configured.Length == 0) return;

            var enemyArray = new Il2CppStructArray<RoleTeamTypes>(configured.Length);
            for (int i = 0; i < configured.Length; i++) enemyArray[i] = configured[i];
            role.enemyTeams = enemyArray;
        }

        private static Il2CppStructArray<RoleTeamTypes> GetTemplateEnemyTeams(RoleTeamTypes team)
        {
            if (!RoleManager.InstanceExists) return null;
            var roles = RoleManager.Instance.allRoles;
            if (roles == null) return null;

            string templateName = team == RoleTeamTypes.Impostor ? "ImpostorRole" : "CrewmateRole";
            for (int i = 0; i < roles.Count; i++)
            {
                var r = roles[i];
                if (r != null && r.GetIl2CppType().Name == templateName)
                    return r.enemyTeams;
            }

            return null;
        }
    }

    public abstract class CustomImpostorRole : CustomRole
    {
        public override RoleTeamTypes TeamType => RoleTeamTypes.Impostor;
        public override bool CanVent => true;
        public override bool CanSabotage => true;
        public override bool CanUseKillButton => true;

        public override void OnAssigned(RoleBehaviour role, PlayerControl player)
        {
            if (HudManager.InstanceExists && HudManager.Instance.KillButton != null)
                HudManager.Instance.KillButton.gameObject.SetActive(true);
        }
    }

    public abstract class CustomCrewmateRole : CustomRole
    {
        public override RoleTeamTypes TeamType => RoleTeamTypes.Crewmate;
    }
}
