using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Achonor.WhoElse
{
    public partial class GameManager
    {
        
        private ulong[] _playerClients = new ulong[2];

        public bool IsFull
        {
            get
            {
                return (0 != _playerClients[0] && 0 != _playerClients[1]);
            }
        }
        
        /// <summary>
        /// 开始游戏
        /// </summary>
        public async void StartGameServer()
        {
            await Task.Delay(1000);
            //随机加载一个玩法
            int playIdx = Random.Range(0, _plays.Count);
            PlayBase play = Instantiate(_plays[playIdx]);
            play.NetworkObject.Spawn();
        }
        
        private void ConnectionApprovalCallbackServer(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            if (IsFull)
            {
                response.Approved = false;
                response.Reason = "房间人数已满";
                return;
            }
            response.Approved = true;
            response.CreatePlayerObject = false;
            response.Position = Vector3.zero;
            response.Rotation = Quaternion.identity;
            return;
        }

        private void OnClientConnectedCallbackServer(ulong clientId)
        {
            if (0 == _playerClients[0])
            {
                _playerClients[0] = clientId;
            }else
            {
                _playerClients[1] = clientId;
            }

            if (IsFull)
            {
                StartGameServer();
            }
        }

        private void OnClientDisconnectCallbackServer(ulong clientId)
        {
            if (clientId == _playerClients[0])
            {
                _players[0] = null;
                _playerClients[0] = 0;
                UpdatePlayerClientRpc(0, null);
            }
            else
            {
                _players[1] = null;
                _playerClients[1] = 0;
                UpdatePlayerClientRpc(1, null);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void SendPlayerServerRpc(Player player)
        {
            if (player.ClientId == _playerClients[0])
            {
                _players[0] = player;
                UpdatePlayerClientRpc(0, _players[0]);
            }
            else
            {
                _players[1] = player;
                UpdatePlayerClientRpc(1, _players[1]);
            }
        }
    }
}
