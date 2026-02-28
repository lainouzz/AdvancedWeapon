# AdvancedWeapon — Unity FPS Weapon System

A modular weapon system built in Unity featuring weapon firing, recoil, attachment customization, inspection, camera shake, and an event-driven architecture.

---

## Architecture Overview

```mermaid
classDiagram
    direction LR

    %% ?? Core ??
    class Player {
        +InspectScript inspectScript
        +Transform camera
        +Transform playerBody
        +float walkSpeed
        +float runSpeed
        +bool isWalking
        +bool isRunning
        +bool isIdle
        +Vector2 moveInput
        -HandleLookX(float)
        -HandleLookY(float)
        +HandleMoveCheck()
    }

    class M4_Weapon {
        +Camera camera
        +Player player
        +InspectScript inspectScript
        +EventStackHandler eventStackHandler
        +List~WeaponAttachmentModifier~ equippedAttachments
        +int magSize
        +int ammo
        +int mag
        +bool isAiming
        +bool isReloading
        +ApplyRecoil()
        +GetAimedPosition() Vector3
        +GetAimedRotation() Quaternion
        +GetCurrentOriginalPosition() Vector3
        -ProcessInput()
        -HandleFire()
        -Reload()
        -HandleSightIn()
        -HandleAnimations()
        -UpdateUI()
    }

    %% ?? Weapon Handlers ??
    class FireHandle {
        +M4_Weapon weapon
        +RecoilHandle recoilHandle
        +EventStackHandler stackHandler
        +bool canFire
        +Fire()
    }

    class RecoilHandle {
        +M4_Weapon weapon
        +EventStackHandler stackHandler
        +float baseVerticalRecoil
        +float baseHorizontalRecoil
        +float verticalRecoil
        +float horizontalRecoil
        +float fireRate
        +float nextFire
        +bool recoiling
        +bool recovering
        +HipRecoil()
        +AimRecoil()
        +Recovering()
        -ApplyRecoilMovement(Vector3, float)
    }

    class WeaponSwayHandler {
        +InspectScript inspect
        -float smoothSwayAmount
        -float swayMultiplier
        -HandleWeaponSway()
    }

    class WeaponSwitch {
        +bool hasSwitched
        +bool canSwitch
        -HandleSwitchInput()
    }

    class CameraShake {
        +M4_Weapon weapon
        +RecoilHandle recoilHandle
        +InspectScript inspectScript
        +float shakeAmount
        +HandleShake()
    }

    %% ?? Attachment System ??
    class AttachmentHandler {
        +M4_Weapon weapon
        +List~GameObject~ availableSights
        +List~GameObject~ availableGrips
        +List~GameObject~ availableMuzzles
        +List~GameObject~ availableSides
        +SaveAttachments()
        +LoadAttachments()
        +CycleAttachment(string)
        -EquipAttachment(Transform, GameObject, string)
        -SyncModifiersToWeapon()
    }

    class AttachmentSlot {
        +InspectScript inspectScript
        +string slotName
        -AttachmentHandler attachmentHandler
        -OnMouseDown()
    }

    class AttachmentComponent {
        +WeaponAttachmentModifier attachmentData
    }

    class WeaponAttachmentModifier {
        <<ScriptableObject>>
        +string attachmentName
        +float VerticalRecoilModifier
        +float HorizontalRecoilModifier
        +float adsSpeedModifier
        +float movementSpeedModifier
    }

    %% ?? Inspection ??
    class InspectScript {
        +AttachmentHandler attachmentHandler
        +CameraShake cameraShake
        +EventStackHandler eventStackHandler
        +bool isInspecting
        +bool isReturning
        -InspectWeapon()
        -MoveToPosition(Vector3, Quaternion)
    }

    %% ?? Event System ??
    class EventStackHandler {
        <<ScriptableObject>>
        +bool hasFiredEvent
        +bool hasPoppedEvent
        +PushEvent(string)
        +PopEvent() string
        +Peak() string
        +ResetEvent()
    }

    class EventHandler {
        -List~IEvent~ m_eventStack
        +PushEvent(IEvent)
        +RemoveEvent(IEvent)
        #UpdateEvents()
    }

    class IEvent {
        <<interface>>
        +OnBegin(bool)
        +OnUpdate()
        +OnEnd()
        +IsDone() bool
    }

    class GameEvent {
        <<abstract>>
    }

    class GameEventBehaviour {
        <<abstract>>
    }

    %% ?? Relationships ??

    M4_Weapon --> Player : reads movement state
    M4_Weapon --> InspectScript : checks isInspecting
    M4_Weapon --> EventStackHandler : push / pop / reset events
    M4_Weapon --> FireHandle : delegates firing
    M4_Weapon --> RecoilHandle : drives recoil & timing
    M4_Weapon --> WeaponAttachmentModifier : applies modifiers

    FireHandle --> M4_Weapon : reads muzzle / ammo
    FireHandle --> RecoilHandle : triggers recoil state
    FireHandle --> EventStackHandler : push fire event

    RecoilHandle --> M4_Weapon : reads aimed position

    CameraShake --> M4_Weapon : reads ammo
    CameraShake --> RecoilHandle : reads recoiling state
    CameraShake --> InspectScript : checks isInspecting

    WeaponSwayHandler --> InspectScript : checks isInspecting

    InspectScript --> AttachmentHandler : save attachments on close
    InspectScript --> CameraShake : disables during inspect
    InspectScript --> EventStackHandler : push / pop events

    AttachmentHandler --> M4_Weapon : syncs modifiers & calls ApplyRecoil
    AttachmentHandler --> AttachmentComponent : reads from instantiated prefab

    AttachmentComponent --> WeaponAttachmentModifier : holds data reference

    AttachmentSlot --> InspectScript : checks isInspecting
    AttachmentSlot --> AttachmentHandler : triggers CycleAttachment

    EventHandler --> IEvent : manages stack of
    GameEvent ..|> IEvent
    GameEventBehaviour ..|> IEvent
```

---

## Module Breakdown

| Module | Classes | Responsibility |
|---|---|---|
| **Player** | `Player` | Input handling, camera look, movement state |
| **Weapon Core** | `M4_Weapon` | Central weapon controller — firing, aiming, reload, animation, UI |
| **Weapon Handlers** | `FireHandle`, `RecoilHandle` | Firing logic / VFX spawning, recoil movement & recovery |
| **Weapon Effects** | `WeaponSwayHandler`, `CameraShake` | Mouse-driven weapon sway, screen shake on fire |
| **Weapon Switching** | `WeaponSwitch` | Toggle active weapon via input |
| **Attachments** | `AttachmentHandler`, `AttachmentSlot`, `AttachmentComponent`, `WeaponAttachmentModifier` | Runtime attachment cycling, modifier data, recoil sync |
| **Inspection** | `InspectScript` | Enter/exit inspect mode, save attachments, cursor control |
| **Event System** | `EventHandler`, `EventStackHandler`, `IEvent`, `GameEvent`, `GameEventBehaviour` | Stack-based event management and logging |

---

## Data Flow

```
Player Input
    |
    |--> Player -> movement state -> M4_Weapon (walk/run animation)
    |
    |--> M4_Weapon
    |       -> Fire -> FireHandle -> RecoilHandle (recoil)
    |       |                |-> CameraShake (screen shake)
    |       -> Aim  -> sight-in coroutine
    |       -> Reload -> magazine detach/reattach coroutine
    |
    |--> InspectScript
    |       -> Enter -> disable CameraShake, push event, unlock cursor
    |       -> Exit  -> AttachmentHandler.SaveAttachments(), pop event
    |
    |--> AttachmentSlot.OnMouseDown()
            -> AttachmentHandler.CycleAttachment()
                    -> SyncModifiersToWeapon() -> M4_Weapon.ApplyRecoil()
```
