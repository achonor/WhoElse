using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Achonor.WhoElse
{
    public class Player : INetworkSerializable
    {
        private ulong _clientId;

        public ulong ClientId
        {
            get { return _clientId; }
            set { _clientId = value; }
        }
        
        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private int _score;
        public int Score
        {
            get { return _score; }
            set { _score = value; }
        }
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _clientId);
            serializer.SerializeValue(ref _name);
            serializer.SerializeValue(ref _score);
        }
    }
}
