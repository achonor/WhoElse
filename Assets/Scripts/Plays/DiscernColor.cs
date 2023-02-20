using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Achonor.WhoElse
{
    public class DiscernColor : PlayBase
    {
        public struct Problem : INetworkSerializable
        {
            public int Level;
            public int CubeNumber;
            public Color NormalColor;
            public Color SpecialColor;
            public int SpecialColorIndex;
            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref Level);
                serializer.SerializeValue(ref CubeNumber);
                serializer.SerializeValue(ref NormalColor);
                serializer.SerializeValue(ref SpecialColor);
                serializer.SerializeValue(ref SpecialColorIndex);
            }
        }

        
        [SerializeField] private Animator _countdownAnimator;
        [SerializeField] private Image _colorCubePrefab;
        [SerializeField] private GridLayoutGroup _gamePanelLayout;
        [SerializeField] private List<int> _levelParam1 = new List<int>();
        [SerializeField] private List<int> _levelParam2 = new List<int>();
        [SerializeField] private List<int> _levelParam3 = new List<int>();

        private int _curLevel = 0;
        private List<Problem> _problems = new List<Problem>();

        private List<Image> _allColorCube = new List<Image>();
        public override string GetPlayName()
        {
            return "区分颜色";
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _colorCubePrefab.gameObject.SetActive(false);

            if (IsServer)
            {
                _curLevel = 0;
                _problems.Clear();
                StartPlayServer();
            }

            if (IsClient)
            {
                ClearColorCube();
                //倒计时3秒动画
                _countdownAnimator.SetTrigger("Countdown");
            }
        }

        private Problem GetProblem(int level)
        {
            if (level < _problems.Count)
            {
                return _problems[level];
            }
            //生成
            Problem problem = new Problem();
            problem.Level = level;
            int levelIndex = Mathf.Clamp(level,0, _levelParam1.Count - 1);
            problem.CubeNumber = _levelParam1[levelIndex] * _levelParam1[levelIndex];
            int[] colors = new int[3];
            colors[0] = Random.Range(0, 256);
            colors[1] = Random.Range(0, 256);
            colors[2] = Random.Range(0, 256);
            problem.NormalColor = new Color(colors[0] / 255f, colors[1] / 255f, colors[2] / 255f, 1);
            int param2 = _levelParam2[levelIndex];
            int temp = Random.Range(0, 3);
            int offset = Random.Range(0, 2) == 0 ? -param2 : param2;
            if (colors[temp] + offset < 0 || 255 < colors[temp] + offset)
            {
                offset = -offset;
            }
            colors[temp] += offset;
            problem.SpecialColor = new Color(colors[0] / 255f, colors[1] / 255f, colors[2] / 255f, 1);
            problem.SpecialColorIndex = Random.Range(0, problem.CubeNumber);
            _problems.Add(problem);
            return problem;
        }

        private async void StartPlayServer()
        {
            await Task.Delay(3000);
            StartPlayClientRpc();
            SendProblemClientRpc(GetProblem(0));
            //30秒后结束
            await Task.Delay((int)(GetPlayDuration() * 1000));
            Player winPlayer = null;
            GameManager.ForPlayers((idx, player) =>
            {
                if (null == winPlayer)
                {
                    winPlayer = player;
                }else if (winPlayer.Score < player.Score)
                {
                    winPlayer = player;
                }
                return true;
            });
            EndPlayClientRpc(winPlayer.ClientId);
            //三秒钟后释放
            await Task.Delay(3000);
            NetworkObject.Despawn();
        }

        private void ClearColorCube()
        {
            for (int i = 0; i < _allColorCube.Count; i++)
            {
                Destroy(_allColorCube[i].gameObject);
            }
            _allColorCube.Clear();
        }

        private void OnClickColorCube(int level, int index)
        {
            SubmitAnswerServerRpc(level, index);
        }
        
        
        [ClientRpc]
        private void SendProblemClientRpc(Problem problem, ClientRpcParams clientRpcParams = default)
        {
            Debug.Log($"普通颜色:{problem.NormalColor} 特殊颜色:{problem.SpecialColor} 答案: {problem.SpecialColorIndex}");
            ClearColorCube();
            //显示问题
            int levelIndex = Mathf.Clamp(problem.Level,0, _levelParam1.Count - 1);
            int param1 = _levelParam1[levelIndex];
            _gamePanelLayout.cellSize = new Vector2(1000f / param1, 1000f / param1);
            for (int i = 0; i < problem.CubeNumber; i++)
            {
                Image cell = Instantiate(_colorCubePrefab, _gamePanelLayout.transform);
                cell.color = i == problem.SpecialColorIndex ? problem.SpecialColor : problem.NormalColor;
                cell.gameObject.SetActive(true);
                _allColorCube.Add(cell);
                //回调
                int idx = i;
                int level = problem.Level;
                cell.GetComponent<Button>().onClick.AddListener(() =>
                {
                    OnClickColorCube(level, idx);
                });
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void SubmitAnswerServerRpc(int level, int index, ServerRpcParams serverRpcParams = default)
        {
            if (_problems.Count <= level)
            {
                return;
            }
            Problem problem = GetProblem(level);
            int levelIndex = Mathf.Clamp(problem.Level,0, _levelParam1.Count - 1);
            if (problem.SpecialColorIndex == index)
            {
                //回答正确
                GameManager.ForPlayers((idx, player) =>
                {
                    if (serverRpcParams.Receive.SenderClientId == player.ClientId)
                    {
                        player.Score += _levelParam3[levelIndex];
                        GameManager.S.UpdatePlayerClientRpc(idx, player);
                        RefreshPlayerScoreClientRpc();
                        return false;
                    }
                    return true;
                });
            }

            ClientRpcParams clientRpcParams = new ClientRpcParams()
            {
                Send = new ClientRpcSendParams()
                {
                    TargetClientIds = new[] { serverRpcParams.Receive.SenderClientId }
                }
            };
            //推送下一个问题
            SendProblemClientRpc(GetProblem(problem.Level + 1), clientRpcParams);
        }
    }
}
