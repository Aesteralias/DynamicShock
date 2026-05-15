# DynamicShock Plugin

Plugin for MultiShock that adds WebSocket Endpoints, Callable Shock Configs, Interactive Editor Pages, and Flow Nodes.

## WebSocket Endpoints
- ws://localhost:4569/Events
-- Register Events
-- Activate Event

- ws://localhost:4569/Raw
-- Activate shock with Raw Data
-- TODO: Compatiblity with old MultiShock websocket


## Nodes
- Await All: For aligning multiple paths of nodes where all data needs to reach a point simultaniously.
- Dynamic Transform: Exposes the internal transform mechanism for Flows.
- Rising Edge Gate: Only allows the first of continious triggers to go through until a grace period is complete.
- Shocker Selector: Provides dynamic selection services instead of split nodes.
- Use Stored Event: Translates from event name to data required to drive a shock event.
- WebSocket Event Name: Exposes Event WebSocket messages to Flows
- WebSocket Raw: Exposes Raw WebSocket messages to Flows


### Emitter/Recievers
Emitters and recievers are a way of allowing for a flow to interact with itself recursively and without blocking the flow from future executions, 
as well as providing tunnels to other flows.

- Delay Emitter: Just works as a delay but doesn't lock the flow on use.
- Falling Edge Emitter: Only triggers once a signal was present then left alone for the time period.
- Signal Trigger: Recieves events along configured signal path from any emitter.

## Installation

1. Download the latest release from the [Releases](../../releases) page
2. Place the .msplugin to MultiShock's `Plugins` folder
3. Restart MultiShock

## Plugin Info

| Property | Value |
|----------|-------|
| **ID** | `Aes-DynamicShock` |
| **Name** | DynamicShock |
| **Version** | 1.0.1 |
| **Route** | /plugins/Aes-DynamicShock |

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for version history.
