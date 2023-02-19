using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

namespace Achonor.WhoElse
{
    /// <summary>
    /// 匹配界面
    /// </summary>
    public class UIMatch : MonoBehaviour
    {
        [SerializeField] private Button _startHostBtn;
        [SerializeField] private Button _startConnectBtn;
        [SerializeField] private InputField _ipInputField;
        [SerializeField] private Text _localIPTextTemplate;

        [SerializeField] private Text _startHostBtnText;

        private void Awake()
        {
            var hostEntry = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in hostEntry.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    var ipText = Instantiate(_localIPTextTemplate, _localIPTextTemplate.transform.parent);
                    ipText.text = ip.ToString();
                    ipText.gameObject.SetActive(true);
                }
            }
            
            _startHostBtn.onClick.AddListener(StartHostClick);
            _startConnectBtn.onClick.AddListener(StartConnectClick);
        }

        private void Update()
        {
            _startHostBtnText.text = NetworkManager.Singleton.IsListening ? "等待连接" : "启动主机";
        }

        /// <summary>
        /// 启动主机
        /// </summary>
        private void StartHostClick()
        {
            if (NetworkManager.Singleton.IsListening)
            {
                NetworkManager.Singleton.Shutdown();
            }
            else
            {
                NetworkManager.Singleton.StartHost();
            }
        }

        private void StartConnectClick()
        {
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.ConnectionData.Address = _ipInputField.text;
            NetworkManager.Singleton.StartClient();
        }
    }
}
