using Adrenak.UniVoice;
using Adrenak.UniVoice.AirPeerNetwork;
using Adrenak.UniVoice.AudioSourceOutput;
using Adrenak.UniVoice.TelepathyNetwork;
using System;
using Adrenak.UniVoice.UniMicInput;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Voice.Radio;

namespace Voice
{

    public class UniVoiceAgent : MonoBehaviour
    {
        private ChatroomAgent agent;
        private string currentChatroomName;
        private PeerListDriver peerListDriver;


        private enum NetworkBackend
        {
            AIRPEER,
            TELEPATHY
        }

        private enum ChatMode
        {
            VOICE,
            RADIO
        }

        [Header("Network")]

        [Tooltip("AirPeer uses WebRTC based peer-to-peer networking (RTP-UDP), which requires an external signaling server (AirSignal) for connection establishment. Telepathy uses message-based client-host networking (TCP).")]
        [SerializeField] private NetworkBackend networkBackend = NetworkBackend.TELEPATHY;

        [Tooltip("The AirSignal signaling server's public websocket address, required only for Network Backend: AIRPEER.")]
        [SerializeField] private string airSignalIpAddress = "ws://vps.chriphost.de:12776";

        [Tooltip("The Telepathy port, required only for Network Backend: TELEPATHY.")]
        [SerializeField] private int telepathyPort = 12777;


        [Header("UI")]

        [SerializeField] private TMP_InputField roomNameInput;
        [SerializeField] private Button hostRoomButton;
        [SerializeField] private Button joinRoomButton;
        [SerializeField] private Button leaveRoomButton;
        [SerializeField] private Button muteSelfButton;
        [SerializeField] private TMP_Text connectionStatusText;
        [SerializeField] private TMP_Text networkBackendText;
        [SerializeField] private GameObject peerList;


        [Header("Audio")]
        
        [Tooltip("VOICE uses the Windows default microphone as input, RADIO an audio clip.")]
        [SerializeField] private ChatMode chatMode;
        
        [SerializeField] private AudioClipMic audioClipMic;
        [SerializeField] private int audioSourceId = 0;

        [Tooltip("Frequency used by the microphone to time-quantize the analog signal. This is important for sound quality.")]
        [SerializeField] private int sampleRate = 16000;

        [Tooltip("Length of the internal sample buffer. A packet containing voice-data is sent only after a full sample buffer can be read, so this is important for latency.")]
        [SerializeField] private int bufferLength = 100;

        private void InitializeServer()
        {
            // Don't know if this is required, Unity should request required permissions automatically....
            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Microphone);
            UnityEngine.Android.Permission.RequestUserPermission("INTERNET");
            UnityEngine.Android.Permission.RequestUserPermission("ACCESS_NETWORK_STATE");

            IChatroomNetwork network = networkBackend == NetworkBackend.AIRPEER
                ? new UniVoiceAirPeerNetwork(airSignalIpAddress)
                : UniVoiceTelepathyNetwork.New(telepathyPort);

            IAudioInput input = chatMode == ChatMode.VOICE
                ? new UniVoiceUniMicInput(audioSourceId, sampleRate, bufferLength)
                : new AudioClipInput(audioClipMic, bufferLength);

            agent = new ChatroomAgent(network, input, new UniVoiceAudioSourceOutput.Factory());

            // TODO: Probably need to check here if agent creation was successful.


            // Hosting events
            agent.Network.OnCreatedChatroom += () =>
            {
                connectionStatusText.text = "Hosting Chatroom: " + currentChatroomName;
                Debug.Log("Created Chatroom (you are peer " + agent.Network.OwnID + ")!");
            };

            agent.Network.OnChatroomCreationFailed += (Exception e) =>
            {
                connectionStatusText.text = "Failed to host Chatroom: " + e.Message;
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
                connectionStatusText.text = "Failed to join Chatroom: " + e.Message;
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
            agent.Network.OnAudioSent += (short seq, ChatroomAudioSegment seg) =>
            {
                // Debug.Log("UniVoiceAgent: Audio sent.");
            };

            agent.Network.OnAudioReceived += (short seq, ChatroomAudioSegment seg) =>
            {
                // Debug.Log("UniVoiceAgent: Audio received.");
            };
        }

        private void InitializeInput()
        {
            hostRoomButton.onClick.AddListener(HostChatroom);
            joinRoomButton.onClick.AddListener(JoinChatroom);
            leaveRoomButton.onClick.AddListener(LeaveChatroom);
            muteSelfButton.onClick.AddListener(ToggleMuteSelf);
        }

        private void InitializePeerList()
        {
            peerListDriver = peerList.GetComponentInChildren<PeerListDriver>();
            if (peerListDriver == null)
            {
                Debug.Log("Couldn't obtain PeerListDriver, no peers will be displayed.");
                return;
            }

            // Hosting events
            agent.Network.OnCreatedChatroom += () => { peerListDriver.AddPeer(agent.Network.OwnID); }; // You create a Chatroom
            agent.Network.OnClosedChatroom += () => { peerListDriver.Clear(); };

            // Joining events
            agent.Network.OnJoinedChatroom += (short id) =>
            {
                peerListDriver.AddPeer(agent.Network.OwnID);
                peerListDriver.AddPeers(agent.Network.PeerIDs);
            };
            agent.Network.OnLeftChatroom += () => { peerListDriver.Clear(); };

            // Peer events
            agent.Network.OnPeerJoinedChatroom += (short id) => { peerListDriver.AddPeer(id); }; // Someone else joins your Chatroom/the Chatroom you're in
            agent.Network.OnPeerLeftChatroom += (short id) => { peerListDriver.RemovePeer(id); };
        }

        private void HostChatroom()
        {
            // "localhost" is TELEPATHY's default IP, reflect that
            if (networkBackend == NetworkBackend.TELEPATHY && roomNameInput.text == "")
            {
                currentChatroomName = "localhost";
            }
            else
            {
                currentChatroomName = roomNameInput.text;
            }

            agent.Network.HostChatroom(currentChatroomName);
        }

        private void JoinChatroom()
        {
            // "localhost" is TELEPATHY's default IP, reflect that
            if (networkBackend == NetworkBackend.TELEPATHY && roomNameInput.text == "")
            {
                currentChatroomName = "localhost";
            }
            else
            {
                currentChatroomName = roomNameInput.text;
            }

            agent.Network.JoinChatroom(currentChatroomName);
        }

        private void LeaveChatroom()
        {
            currentChatroomName = "";

            if (agent.CurrentMode == ChatroomAgentMode.Host)
            {
                agent.Network.CloseChatroom();
            }
            else
            {
                agent.Network.LeaveChatroom();
            }
        }

        private void ToggleMuteSelf()
        {
            if (!agent.MuteSelf) //if should mute self
            {
                Debug.Log("Muted Self.");
                muteSelfButton.GetComponentInChildren<TMP_Text>().text = "Unmute Self";
            }
            else //if you want to unmute yourself
            {
                Debug.Log("Unmuted Self.");
                muteSelfButton.GetComponentInChildren<TMP_Text>().text = "Mute Self";
            }

            agent.MuteSelf = !agent.MuteSelf;
        }

        // Start is called before the first frame update
        private void Start()
        {
            InitializeInput();
            InitializeServer();
            InitializePeerList();

            connectionStatusText.text = "Disconnected";
            networkBackendText.text = "Running with " + (networkBackend == NetworkBackend.TELEPATHY ? "TELEPATHY" : "AIRPEER") + " backend.";

            agent.MuteOthers = false;
            agent.MuteSelf = false;
        }
    }

}
