using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Achonor.WhoElse
{
    /// <summary>
    /// 玩法基类
    /// </summary>
    public class PlayBase : NetworkBehaviour
    {
        [SerializeField] private Text _player0Name;
        [SerializeField] private Text _player1Name;
        [SerializeField] private Text _playName;
        public virtual string GetPlayName()
        {
            return string.Empty;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsClient)
            {
                _playName.text = GetPlayName();
                GameManager.S.ForPlayers((idx, player) =>
                {
                    if (0 == idx)
                    {
                        _player0Name.text = player.Name;
                    }else if (1 == idx)
                    {
                        _player1Name.text = player.Name;
                    }
                    return true;
                });
            }
        }
    }
}
