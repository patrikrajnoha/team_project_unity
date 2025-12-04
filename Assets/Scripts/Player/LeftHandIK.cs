using UnityEngine;

public class LeftHandIK : MonoBehaviour
{
    public Animator animator;
    public Transform leftHandTarget;  // LeftHandGrip

    private void OnAnimatorIK(int layerIndex)
    {
        if (animator == null || leftHandTarget == null)
            return;

        // Nastavíme IK pre ľavú ruku
        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1f);

        animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTarget.position);
        animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTarget.rotation);
    }
}
