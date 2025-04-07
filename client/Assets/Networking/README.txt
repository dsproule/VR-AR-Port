To Implement Networking:
	1. Change movement/behavioral script to NetworkBehaviour
	2. Confirm CustomNetworkManager is in scene (It's a prefab in the Networking directory)
	3. Attach NetworkObject, ClientNetworkTransform to the desired prefab

To Use Networking:
	1. Confirm both parts are on the same internet
	2. Connect it
	3. If doesn't work, be sad :(

Minimum Requirements for Functionality:
	CustomNetworkManager

Need To Be Synced:
	Scenes
	NetworkObjects
	NetworkManager
	NetworkingFolder