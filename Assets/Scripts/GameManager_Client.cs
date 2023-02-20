using System;
using Unity.Netcode;
using UnityEngine;

namespace Achonor.WhoElse
{
    public partial class GameManager
    {
        /// <summary>
        /// 同步玩家数据
        /// </summary>
        /// <param name="index"></param>
        /// <param name="player"></param>
        [ClientRpc]
        public void UpdatePlayerClientRpc(int index, Player player)
        {
            _players[index] = player;
        }
    }
}
