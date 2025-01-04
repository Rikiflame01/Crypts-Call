using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonProgressionManager : MonoBehaviour
{
    public AudioClip doorOpen;
    public AudioSource doorAudioSource;

    [Header("List of Door Pairs")]
    public List<DoorPair> doorPairs = new List<DoorPair>();

    private const string DOOR_INDEX_PREF = "DungeonDoorIndex";

    private int currentDoorIndex = 0;

    public float rotationDuration = 2f;

    [System.Serializable]
    public class DoorPair
    {
        public DoorRotation door1;
        public DoorRotation door2;
    }

    [System.Serializable]
    public class DoorRotation
    {
        [Tooltip("The door GameObject to rotate.")]
        public GameObject door;

        [Tooltip("Rotation axis (e.g., Vector3.up for Y-axis).")]
        public Vector3 rotationAxis = Vector3.up;

        [Tooltip("Rotation angle in degrees. Positive for one direction, negative for the opposite.")]
        public float rotationAngle = 90f;

        [Tooltip("Initial rotation offset from the door's current rotation.")]
        public Vector3 initialRotationOffset = Vector3.zero;
    }

    private void Awake()
    {
        currentDoorIndex = PlayerPrefs.GetInt(DOOR_INDEX_PREF, 0);

        AudioSource source = GetComponent<AudioSource>();
        source.clip = doorOpen;
    }

    public void OpenNextDoor()
    {
        if (currentDoorIndex < doorPairs.Count)
        {
            doorAudioSource.Play();
            DoorPair pairToOpen = doorPairs[currentDoorIndex];

            StartCoroutine(RotateDoor(pairToOpen.door1));
            StartCoroutine(RotateDoor(pairToOpen.door2));

            currentDoorIndex++;

            PlayerPrefs.SetInt(DOOR_INDEX_PREF, currentDoorIndex);
            PlayerPrefs.Save();
        }
        else
        {
            Debug.Log("All doors have been opened!");
        }
    }

    private IEnumerator RotateDoor(DoorRotation doorRotation)
    {
        if (doorRotation.door == null)
        {
            Debug.LogWarning("Door GameObject is null!");
            yield break;
        }

        Transform doorTransform = doorRotation.door.transform;

        if (doorRotation.initialRotationOffset != Vector3.zero)
        {
            doorTransform.rotation = Quaternion.Euler(
                doorTransform.eulerAngles + doorRotation.initialRotationOffset
            );
        }

        Quaternion initialRotation = doorTransform.rotation;
        Quaternion rotationDelta = Quaternion.AngleAxis(
            doorRotation.rotationAngle,
            doorRotation.rotationAxis.normalized
        );
        Quaternion targetRotation = initialRotation * rotationDelta;

        float elapsedTime = 0f;

        while (elapsedTime < rotationDuration)
        {
            doorTransform.rotation = Quaternion.Slerp(
                initialRotation,
                targetRotation,
                elapsedTime / rotationDuration
            );
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        doorTransform.rotation = targetRotation;
    }

    void OnApplicationQuit()
    {
        PlayerPrefs.SetInt("HasBeenToTown", 0);

        PlayerPrefs.SetInt("DungeonDoorIndex", 0);
        PlayerPrefs.Save();
        Debug.Log("PlayerPrefs reset on application quit.");
    }
}
