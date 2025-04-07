# API Documentation

### These are the set of functions provided as is. They act as examples for further expansion but also the minimum likely needed for a VR-AR port. 

To make a component responsive to the following API, I assume we have a connection to the RpcHandler and the Object has been registered. If not, go to the bottom for a tutorial on how to do this.

---

## SpawnRequest

```csharp
void SpawnRequest(string NetPrefabName, Vector3 pos, string targetObj=null, 
                  string tag=null, bool grabbable=false);
```
Spawns the networked object.

- `NetPrefabName`          — Name of the prefab to instantiate as it exists in prefabList.
- `pos`                    — Position in world space where the object should spawn.
- `targetObj` *(optional)* — Label for tracking or parenting later.
- `tag` *(optional)*       — Tag to apply to the spawned GameObject.
- `grabbable` *(optional)* — If `true`, adds grab functionality on spawn.

The optional parameters allow for a lot of flexibility here. The `targetObj` allows us to refernce the spawned object through
the RetrObj API call. The `tag` is to assign any custom tags we may have in the scene. The `grabbable` allows for detection if
the hand is within grabbing range. 

---

## ActiveSet

```csharp
void ActiveSet(GameObject gameObject, bool setting);

```
Sets the active state of the given networked object on both server and client.

- `gameObject` - Gameobject that we want to set the .active property of
- `setting`    - `true`/`false` of whether we'd like to turn it on or off


---

## DespawnRequest

```csharp
void DespawnRequest(GameObject gameObject);
```
Requests to remove a networked object. Also removes the object from the internal registry if it was labeled.

- `gameObject` - Gameobject of the item we'd like to despawn 

---

## TextUpdatesStatusOn

```csharp
void TextUpdatesStatusOn(bool status);
```
Enables or disables status text in the AR headset.

- `status` — If `true`, shows text updates; if `false`, hides them.

---

## TextUpdates

```csharp
void TextUpdates(string content);
```
Updates the current text being displayed in the AR headset.

- content - what we'd like to display to the user.
---

## SwitchScene

```csharp
void SwitchScene(string sceneName);
```
Changes the scene on both client and server.

- `sceneName` — Name of the scene to switch to.

---

## ReparentObj

```csharp
void ReparentObj(string childRegisName, string parentRegisName);
```
Parents a network object to another. Mostly used for the grab functionality but allows the user to reparent objects.

- `childRegisName` — Label of the child object.
- `parentRegisName` — Label of the parent object.

---

## RetrRegObj

```csharp
GameObject RetrRegObj(string objLabel);
```
Retrieves a GameObject from the registered object dictionary.

- `objLabel` — Label assigned during `SpawnRequest`.

To connect a prefab to the API:

1. Attach **Client Network Transform** and **Network Object** in the components section of the prefab
![Screenshot to show network components](ref_images/network_components.png)
2. Insert the prefab as a new entry to **Network Prefabs List**
![Screenshot of prefab list](ref_images/prefab_list.png)