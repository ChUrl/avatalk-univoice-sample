using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Voice
{

    public class PeerListDriver : MonoBehaviour
    {
        private readonly Dictionary<short, GameObject> peerListEntries = new();

        [SerializeField] private GameObject peerListEntryPrefab;

        public void AddPeer(short id)
        {
            if (peerListEntries.ContainsKey(id))
            {
                Debug.Log("Peer " + id + " already in PeerList!");
                return;
            }

            GameObject entry = Instantiate(peerListEntryPrefab, transform);
            entry.GetComponentInChildren<TMP_Text>().text = "Peer " + id;

            peerListEntries[id] = entry;
        }

        public void AddPeers(List<short> peers)
        {
            peers.ForEach(AddPeer);
        }

        public void RemovePeer(short id)
        {
            if (!peerListEntries.ContainsKey(id))
            {
                Debug.Log("Peer " + id + " not in PeerList!");
                return;
            }

            Destroy(peerListEntries[id]);
            peerListEntries.Remove(id);
        }

        public void Clear()
        {
            foreach (GameObject entry in peerListEntries.Values)
            {
                Destroy(entry);
            }
            peerListEntries.Clear();
        }
    }

}
