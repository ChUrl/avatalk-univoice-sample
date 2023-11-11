using Adrenak.UniVoice;
using Adrenak.UniVoice.AirPeerNetwork;
using Adrenak.UniVoice.AudioSourceOutput;
using Adrenak.UniVoice.TelepathyNetwork;
using Adrenak.UniVoice.UniMicInput;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Networking
{

    public class UniVoiceAgent : MonoBehaviour
    {
        ChatroomAgent agent;

        enum NetworkBackend
        {
            AIRPEER,
            TELEPATHY
        }

        [Header("Network")]

        [Tooltip("AirPeer uses WebRTC based peer-to-peer networking (RTP-UDP), which requires an external signaling server (AirSignal) for connection establishment. Telepathy uses message-based client-host networking (TCP).")]
        [SerializeField] private NetworkBackend networkBackend = NetworkBackend.TELEPATHY;

        [Tooltip("The AirSignal signaling server's public websocket address, required only for Network Backend: AIRPEER.")]
        [SerializeField] private string airsignalIpAddress = "ws://vps.chriphost.de:12776";

        [Tooltip("The Telepathy port, required only for Network Backend: TELEPATHY.")]
        [SerializeField] private int telepathyPort = 12777;


        [Header("UI")]

        [SerializeField] private TMP_InputField roomNameInput;
        [SerializeField] private Button hostRoomButton;
        [SerializeField] private Button joinRoomButton;
        [SerializeField] private Button leaveRoomButton;
        [SerializeField] private TMP_Text connectionStatusText;
        [SerializeField] private TMP_Text networkBackendText;

        [SerializeField] private GameObject peerList;


        [Header("Audio")]

        [SerializeField] private int audioSourceId = 0;
        [SerializeField] private int audioSourceSampleRate = 16000;
        [SerializeField] private int audioSourceSampleLength = 100;

        void InitializeServer()
        {
            // Don't know if this is required, Unity should request required permissions automatically....
            // UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Microphone);
            // UnityEngine.Android.Permission.RequestUserPermission("INTERNET");

            if (networkBackend == NetworkBackend.TELEPATHY)
            {
                agent = new ChatroomAgent(
                    UniVoiceTelepathyNetwork.New(telepathyPort),
                    new UniVoiceUniMicInput(audioSourceId, audioSourceSampleRate, audioSourceSampleLength),
                    new UniVoiceAudioSourceOutput.Factory()
                );
            }
            else
            {
                agent = new ChatroomAgent(
                    new UniVoiceAirPeerNetwork(airsignalIpAddress),
                    new UniVoiceUniMicInput(audioSourceId, audioSourceSampleRate, audioSourceSampleLength),
                    new UniVoiceAudioSourceOutput.Factory()
                );
            }

            // TODO: Probably need to check here if agent creation was successful.


            // Hosting events
            agent.Network.OnCreatedChatroom += () =>
            {
                connectionStatusText.text = "Hosting Chatroom";
                Debug.Log("Created Chatroom (you are peer " + agent.Network.OwnID + ")!");
            };

            agent.Network.OnChatroomCreationFailed += (Exception e) =>
            {
                connectionStatusText.text = "Failed to host Chatroom";
                Debug.Log("Failed to create Chatroom!");
                Debug.Log(e.Message);
            };

            agent.Network.OnClosedChatroom += () =>
            {
                connectionStatusText.text = "Disconnected";
                Debug.Log("Closed Chatroom!");
            };

            // Joining events
            agent.Network.OnJoinedChatroom += (short id) =>
            {
                connectionStatusText.text = "Joined Chatroom " + id;
                Debug.Log("Joined Chatroom " + id + "!");
            };

            agent.Network.OnChatroomJoinFailed += (Exception e) =>
            {
                connectionStatusText.text = "Failed to join Chatroom";
                Debug.Log("Failed to join Chatroom!");
                Debug.Log(e.Message);
            };

            agent.Network.OnLeftChatroom += () =>
            {
                connectionStatusText.text = "Disconnected";
                Debug.Log("Left Chatroom!");
            };

            // Peer events
            agent.Network.OnPeerJoinedChatroom += (short id) =>
            {
                Debug.Log("Peer joined Chatroom (peer id " + id + ")!");
            };

            agent.Network.OnPeerLeftChatroom += (short id) =>
            {
                Debug.Log("Peer left Chatroom (peer id " + id + ")!");
            };

            // Audio events
            // agent.Network.OnAudioReceived
            // agent.Network.OnAudioSent


            PeerListDriver peerListDriver = peerList.GetComponentInChildren<PeerListDriver>();

            // Hosting events
            agent.Network.OnCreatedChatroom += () => { peerListDriver.addPeer(agent.Network.OwnID); }; // You create a Chatroom
            agent.Network.OnClosedChatroom += peerListDriver.clear;

            // Joining events
            agent.Network.OnJoinedChatroom += (short id) =>
            {
                peerListDriver.addPeer(agent.Network.OwnID);
                peerListDriver.addPeers(agent.Network.PeerIDs);
            };
            agent.Network.OnLeftChatroom += peerListDriver.clear;

            // Peer events
            agent.Network.OnPeerJoinedChatroom += peerListDriver.addPeer; // Someone else joins your Chatroom/the Chatroom you're in
            agent.Network.OnPeerLeftChatroom += peerListDriver.removePeer;
        }

        void InitializeInput()
        {
            hostRoomButton.onClick.AddListener(HostChatroom);
            joinRoomButton.onClick.AddListener(JoinChatroom);
            leaveRoomButton.onClick.AddListener(LeaveChatroom);
        }

        void HostChatroom()
        {
            string name = roomNameInput.text;
            agent.Network.HostChatroom(name);
        }

        void JoinChatroom()
        {
            string name = roomNameInput.text;
            agent.Network.JoinChatroom(name);
        }

        void LeaveChatroom()
        {
            if (agent.CurrentMode == ChatroomAgentMode.Host)
            {
                agent.Network.CloseChatroom();
            }
            else
            {
                agent.Network.LeaveChatroom();
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            InitializeInput();
            InitializeServer();

            connectionStatusText.text = "Disconnected";
            networkBackendText.text = "Running with " + (networkBackend == NetworkBackend.TELEPATHY ? "TELEPATHY" : "AIRPEER") + " backend.";

            agent.MuteOthers = false;
            agent.MuteSelf = false;
        }
    }

}
