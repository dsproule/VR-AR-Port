# API Documentation

### These are the set of functions provided as is. They act as examples for further expansion but also the minimum likely needed for a VR-AR port. 

---

## SpawnRequest

```csharp
void SpawnRequest(string NetPrefabName, Vector3 pos, string targetObj=null, 
                  string tag=null, bool grabbable=false);
```

- `NetPrefabName` — Name of the prefab to instantiate.
- `pos` — Position in world space where the object should spawn.
- `targetObj` *(optional)* — Label for tracking or parenting later.
- `tag` *(optional)* — Tag to apply to the spawned GameObject.
- `grabbable` *(optional)* — If `true`, adds grab functionality on spawn.

---

## ActiveSet

```csharp
void ActiveSet(GameObject gameObject, bool setting);
```

- Sets the active state of the given networked object on both server and client.

---

## DespawnRequest

```csharp
void DespawnRequest(GameObject gameObject);
```

- Requests to remove a networked object. Must be called from the client, and the server will destroy the object.
- Also removes the object from the internal registry if it was labeled.

---

## TextUpdatesStatusOn

```csharp
void TextUpdatesStatusOn(bool status);
```

- Enables or disables status text in the AR headset.
- `status` — If `true`, shows text updates; if `false`, hides them.

---

## TextUpdates

```csharp
void TextUpdates(string content);
```

- Updates the current text being displayed in the AR headset.

---

## SwitchScene

```csharp
void SwitchScene(string sceneName);
```

- Changes the scene on both client and server.
- `sceneName` — Name of the scene to switch to.

---

## ReparentObj

```csharp
void ReparentObj(string childRegisName, string parentRegisName);
```

- Parents a network object to another.
- `childRegisName` — Label of the child object.
- `parentRegisName` — Label of the parent object.

---

## RetrRegObj

```csharp
GameObject RetrRegObj(string objLabel);
```

- Retrieves a GameObject from the registered object dictionary.
- `objLabel` — Label assigned during `SpawnRequest`.
