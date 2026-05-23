using UnityEngine;

[RequireComponent(typeof(Animator))]
public class HumanoidFootFix : MonoBehaviour
{
    [Header("Left Foot Fix")]
    [SerializeField] private bool fixLeftFoot = true;
    [SerializeField] private Vector3 leftFootRotationOffset = new Vector3(0f, 0f, 0f);

    [Header("Right Foot Fix (if needed)")]
    [SerializeField] private bool fixRightFoot = false;
    [SerializeField] private Vector3 rightFootRotationOffset = new Vector3(0f, 0f, 0f);

    [Header("When to Apply")]
    [SerializeField] private bool onlyDuringRun = true;

    private Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (!anim.isHuman) return;

        bool shouldApply = !onlyDuringRun || IsRunning();

        if (fixLeftFoot && shouldApply)
        {
            Quaternion offset = Quaternion.Euler(leftFootRotationOffset);
            Quaternion current = anim.GetBoneTransform(HumanBodyBones.LeftFoot).localRotation;
            anim.SetBoneLocalRotation(HumanBodyBones.LeftFoot, offset * current);
        }

        if (fixRightFoot && shouldApply)
        {
            Quaternion offset = Quaternion.Euler(rightFootRotationOffset);
            Quaternion current = anim.GetBoneTransform(HumanBodyBones.RightFoot).localRotation;
            anim.SetBoneLocalRotation(HumanBodyBones.RightFoot, offset * current);
        }
    }

    private bool IsRunning()
    {
        return anim.GetFloat("Speed") > 0.6f;
    }
}