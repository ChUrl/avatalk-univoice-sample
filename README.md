# AvaTalk Univoice Sample

This sample demonstrates a simple voice-chat via the UniVoice plugin.

## Features

- Supports AirPeer and Telepathy backends
- Network settings can be set at runtime by the server and are automatically synchronized to the client (via NetCode for GameObjects)

## Overview

UniVoice provides a set of interfaces handling chatroom operations, like hosting, joining or muting:

- Audiosource interface
- Audiooutput interface
- Network communications interface

The following interface implementations are used in this sample:

- Single microphone audiosource
- Audioclip audiosource
- Single speaker audiooutput
- AirPeer network
- Telepathy network

## Roadmap

- The client needs to connect via NetCode manually, as the sample doesn't wait for a completed connection before attempting to join a voice channel
- Adapt the client to VR certain platforms
- The server and client apps should be split, as a dedicated server will probably be used in the future
