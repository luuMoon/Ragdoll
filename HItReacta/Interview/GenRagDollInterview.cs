#if UNITY_EDITOR
using UnityEngine;

public class GenRagDollInterview : MonoBehaviour {
    private RagdollManagerGen ragdollManager;
    public int partId = 0;
    public Vector3 force = Vector3.zero;

    private void Start()
    {
        ragdollManager = transform.GetComponent<RagdollManagerGen>();
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 50, 50), "触发Ragdoll"))
        {
            ragdollManager.DisableCustomRagdoll();
            var anim = transform.GetComponent<Animation>();
            if (anim != null)
            {
                anim.enabled = false;
            }

            var characterControl = transform.GetComponent<CharacterController>();
            if (characterControl != null)
            {
                characterControl.enabled = false;
            }

            ragdollManager.CustomRagdoll(partId, force);
        }

        if (GUI.Button(new Rect(100, 0, 50, 50), "恢复Ragdoll"))
        {
            ragdollManager.DisableCustomRagdoll();

            var anim = transform.GetComponent<Animation>();
            if (anim != null)
            {
                anim.enabled = true;
                anim.Play("idle");
            }

            var characterControl = transform.GetComponent<CharacterController>();
            if (characterControl != null)
            {
                characterControl.enabled = true;
            }
        }
    }
}
#endif
