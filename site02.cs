﻿/* Jump Map (ver. Alpha 0.0.1) */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features;
using UnityEngine;

namespace site02
{
    public class site02 : Plugin<Config>
    {
        public static site02 Instance;

        public List<string> Owner = new List<string>() { "76561198447505804@steam" };
        public Dictionary<string, string> Stage = new Dictionary<string, string>();
        public Dictionary<string, int> HealingCooldown = new Dictionary<string, int>(); // ID, 힐 쿨타임

        public override void OnEnabled()
        {
            Instance = this;

            Exiled.Events.Handlers.Server.WaitingForPlayers += OnWaitingForPlayers;
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;

            Exiled.Events.Handlers.Player.Verified += OnVerified;
            Exiled.Events.Handlers.Player.Died += OnDied;
            Exiled.Events.Handlers.Player.SearchingPickup += OnSearchingPickup;
            Exiled.Events.Handlers.Player.DroppedItem += OnDroppedItem;
            Exiled.Events.Handlers.Player.SpawnedRagdoll += OnSpawnedRagdoll;
            Exiled.Events.Handlers.Player.Landing += OnLanding;
            Exiled.Events.Handlers.Player.Hurt += OnHurt;
            Exiled.Events.Handlers.Player.FlippingCoin += OnFlippingCoin;
        }

        public override void OnDisabled()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers -= OnWaitingForPlayers;
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;

            Exiled.Events.Handlers.Player.Verified -= OnVerified;
            Exiled.Events.Handlers.Player.Died -= OnDied;
            Exiled.Events.Handlers.Player.SearchingPickup -= OnSearchingPickup;
            Exiled.Events.Handlers.Player.DroppedItem -= OnDroppedItem;
            Exiled.Events.Handlers.Player.SpawnedRagdoll -= OnSpawnedRagdoll;
            Exiled.Events.Handlers.Player.Landing -= OnLanding;
            Exiled.Events.Handlers.Player.Hurt -= OnHurt;
            Exiled.Events.Handlers.Player.FlippingCoin -= OnFlippingCoin;

            Instance = null;
        }

        public void OnWaitingForPlayers()
        {
            Server.ExecuteCommand($"/decontamination disable");
            Server.FriendlyFire = true;
            Round.IsLocked = true;
            Round.Start();
        }

        public async void OnRoundStarted()
        {
            MapEditorReborn.API.Features.Serializable.MapSchematic mapByName = MapEditorReborn.API.Features.MapUtils.GetMapByName("jm");
            MapEditorReborn.API.API.CurrentLoadedMap = mapByName; 
            
            while (true)
            {
                foreach (var player in Player.List)
                {
                    if (HealingCooldown[player.UserId] <= 0)
                    {
                        player.Heal(10);
                    }
                    else
                    {
                        HealingCooldown[player.UserId] -= 1;
                    }
                }

                await Task.Delay(1000);
            }
        }

        public async void OnVerified(Exiled.Events.EventArgs.Player.VerifiedEventArgs ev)
        {
            ev.Player.Role.Set(PlayerRoles.RoleTypeId.ClassD);
            ev.Player.Position = new Vector3(80.45463f, 1053.379f, -42.54824f);

            Stage.Add(ev.Player.UserId, "1");
            HealingCooldown.Add(ev.Player.UserId, 0);

            while (ev.Player != null)
            {
                if (!ev.Player.IsDead)
                {
                    ev.Player.ShowHint($"<b>Stage {Stage[ev.Player.UserId]}</b>", 1);
                }
                await Task.Delay(1000);
            }
        }

        public async void OnDied(Exiled.Events.EventArgs.Player.DiedEventArgs ev)
        {
            Stage[ev.Player.UserId] = "1";

            for (int i=1; i<5; i++)
            {
                ev.Player.ShowHint($"{5 - i}초 뒤 부활합니다.", 1);
                await Task.Delay(1000);
            }

            ev.Player.Role.Set(PlayerRoles.RoleTypeId.ClassD);
            ev.Player.Position = new Vector3(80.45463f, 1053.379f, -42.54824f);
        }

        public void OnSearchingPickup(Exiled.Events.EventArgs.Player.SearchingPickupEventArgs ev)
        {
            ev.Player.AddItem(ev.Pickup);
        }

        public async void OnDroppedItem(Exiled.Events.EventArgs.Player.DroppedItemEventArgs ev)
        {
            await Task.Delay(10000);
            ev.Pickup.Destroy();
        }

        public async void OnSpawnedRagdoll(Exiled.Events.EventArgs.Player.SpawnedRagdollEventArgs ev)
        {
            await Task.Delay(10000);
            ev.Ragdoll.Destroy();
        }

        public void OnLanding(Exiled.Events.EventArgs.Player.LandingEventArgs ev)
        {
            if (Physics.Raycast(ev.Player.Position, Vector3.down, out RaycastHit hit, 1, (LayerMask)1))
            {
                string StageName = hit.transform.parent.name;

                if (StageName.StartsWith("Stage "))
                {
                    Stage[ev.Player.UserId] = StageName.Replace("Stage ", "");
                }
            }
        }

        public void OnHurt(Exiled.Events.EventArgs.Player.HurtEventArgs ev)
        {
            HealingCooldown[ev.Player.UserId] = 5;
        }

        public void OnFlippingCoin(Exiled.Events.EventArgs.Player.FlippingCoinEventArgs ev)
        {
            ServerConsole.AddLog($"{ev.Player.Nickname}의 위치 : {ev.Player.Position.x} {ev.Player.Position.y} {ev.Player.Position.z}", ConsoleColor.DarkMagenta);
        }

    }
}

