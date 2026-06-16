using UnityEngine;
using UnityEngine.InputSystem;

public class HandGripController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionProperty gripAction;

    [Header("Finger Bones")]
    [SerializeField] private Transform[] fingerRoots;
    [SerializeField] private Transform thumbRoot;

    [Header("Grip Rotation")]
    [SerializeField] private Vector3 openRotation = Vector3.zero;
    [SerializeField] private Vector3 closedRotation = new Vector3(70f, 0f, 0f);
    [SerializeField] private Vector3 thumbOpenRotation = Vector3.zero;
    [SerializeField] private Vector3 thumbClosedRotation = new Vector3(0f, 40f, 0f);

    private void OnEnable() => gripAction.action?.Enable();
    private void OnDisable() => gripAction.action?.Disable();

    private void Update()
    {
        bool isGripping = gripAction.action?.IsPressed() ?? false;
        Quaternion rot = Quaternion.Euler(isGripping ? closedRotation : openRotation);

        foreach (Transform finger in fingerRoots)
            ApplyToChain(finger, rot);

        if (thumbRoot != null)
        {
            Quaternion thumbRot = Quaternion.Euler(isGripping ? thumbClosedRotation : thumbOpenRotation);
            ApplyToChain(thumbRoot, thumbRot);
        }
    }

    private void ApplyToChain(Transform bone, Quaternion rotation)
    {
        if (bone == null) return;
        bone.localRotation = rotation;
        foreach (Transform child in bone)
            ApplyToChain(child, rotation);
    }
}
