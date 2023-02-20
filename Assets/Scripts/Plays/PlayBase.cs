using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        [SerializeField] private Text _player0Score;
        [SerializeField] private Text _player1Score;
        [SerializeField] private Text _playName;
        [SerializeField] private Text _lastTime;
        [SerializeField] private GameObject _winGo;
        [SerializeField] private GameObject _loseGo;

        private bool _isGaming = false;
        
        public virtual string GetPlayName()
        {
            return string.Empty;
        }

        public virtual float GetPlayDuration()
        {
            return 30;
        }
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsClient)
            {
                _playName.text = GetPlayName();
                GameManager.ForPlayers((idx, player) =>
                {
                    if (0 == idx)
                    {
                        _player0Name.text = player.Name;
                        _player0Score.text = player.Score.ToString();
                    }else if (1 == idx)
                    {
                        _player1Name.text = player.Name;
                        _player1Score.text = player.Score.ToString();
                    }
                    return true;
                });
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            //断开连接
            if (!IsServer)
            {
                NetworkManager.Singleton.Shutdown();
            }
        }

        protected async void ShowLastTime()
        {
            DateTime dateTime = DateTime.Now.AddSeconds(GetPlayDuration());
            while (true)
            {
                int lastTime = (int)((dateTime - DateTime.Now).TotalSeconds);
                _lastTime.text = lastTime.ToString();
                if (lastTime <= 0)
                {
                    break;
                }
                await Task.Delay(1000);
            }
        }
        
        
        [ClientRpc]
        protected void StartPlayClientRpc()
        {
            _isGaming = true;
            ShowLastTime();
        }
        
        [ClientRpc]
        protected void EndPlayClientRpc(ulong winClientId)
        {
            if (NetworkManager.LocalClientId == winClientId)
            {
                _winGo.gameObject.SetActive(true);
            }
            else
            {
                _loseGo.gameObject.SetActive(true);
            }
        }
        
        [ClientRpc]
        protected void RefreshPlayerScoreClientRpc()
        {
            GameManager.ForPlayers((idx, player) =>
            {
                if (0 == idx)
                {
                    _player0Score.text = player.Score.ToString();
                }else if (1 == idx)
                {
                    _player1Score.text = player.Score.ToString();
                }
                return true;
            });
        }
    }
}
