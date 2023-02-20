using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Achonor.WhoElse
{
    public partial class GameManager : NetworkBehaviour
    {
        public static GameManager Instance = null;
        public static GameManager S => Instance; 
        
        private static Player[] _players = new Player[2];

        [SerializeField] private List<PlayBase> _plays;

        public static void ForPlayers(Func<int, Player, bool> callback)
        {
            if (null == callback)
            {
                return;
            }
            for (int i = 0; i < _players.Length; i++)
            {
                try
                {
                    if (!callback.Invoke(i, _players[i]))
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                }
            }
        }

        private void Awake()
        {
            Instance = this;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsClient)
            {
                Player player = new Player();
                player.Name = SystemInfo.deviceName;
                player.ClientId = NetworkManager.LocalClientId;
                SendPlayerServerRpc(player);
            }
        }
    }
}
