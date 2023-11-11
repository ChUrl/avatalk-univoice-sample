using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Networking
{

    public class PeerListDriver : MonoBehaviour
    {
        private Dictionary<short, GameObject> peerListEntries = new Dictionary<short, GameObject>();

        [SerializeField] private GameObject peerListEntryPrefab;

        // GameObject entry = Instantiate<GameObject>(peerListEntryPrefab, transform);
        // entry.GetComponentInChildren<TMP_Text>().text = "Hello";

        public void addPeer(short id)
        {
            if (peerListEntries.ContainsKey(id))
            {
                Debug.Log("Peer " + id + " already in PeerList!");
                return;
            }

            GameObject entry = Instantiate<GameObject>(peerListEntryPrefab, transform);
            entry.GetComponentInChildren<TMP_Text>().text = "Peer " + id;

            peerListEntries[id] = entry;
        }

        public void addPeers(List<short> peers)
        {
            peers.ForEach(addPeer);
        }

        public void removePeer(short id)
        {
            if (!peerListEntries.ContainsKey(id))
            {
                Debug.Log("Peer " + id + " not in PeerList!");
                return;
            }

            Destroy(peerListEntries[id]);
            peerListEntries.Remove(id);
        }

        public void clear()
        {
            foreach (GameObject entry in peerListEntries.Values)
            {
                Destroy(entry);
            }
            peerListEntries.Clear();
        }
    }

}
