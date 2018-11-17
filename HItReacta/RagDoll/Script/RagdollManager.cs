using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// body parts enum
/// </summary>
public enum BodyParts : int
{
    Spine = 0,
    Chest,
    Head,
    LeftShoulder,
    RightShoulder,
    LeftElbow,
    RightElbow,
    LeftHip,
    RightHip,
    LeftKnee,
    RightKnee,
    BODY_PART_COUNT,
    None,
}

/// <summary>
/// 碰撞体类型
/// </summary>
public enum ColliderType
{
    BOX = 1,
    CAPSULE = 2,
    SPHERE = 3
}

/// <summary>
/// base ragdoll and hit reaction manager
/// </summary>
public abstract class RagdollManager : MonoBehaviour
{
    /// <summary>
    ///  Class that holds useful information for each body part
    /// </summary>
    [System.Serializable]
    public class BodyPartInfo
    {
        public BodyParts bodyPart = BodyParts.None;                 // current body part
        public Transform transform = null;                          // transform of body part
        public Transform orig_parent = null;                        // original parent of body part
        public Collider collider = null;                            // collider of body part
        public Rigidbody rigidBody = null;                          // rigidbody of body part
        public CharacterJoint constraintJoint = null;            // constraint to add body parts like legs
        public bool ignoreRagdoll = false;
        public Rigidbody jointConnectBody = null;
    }

    #region Fields
    public float StopTime = 5.0f;    
    /// <summary>
    /// ragdoll transforms with colliders and rigid bodies 
    /// </summary>
    public Transform[] RagdollBones;

    // array of body parts
    [SerializeField]
    protected BodyPartInfo[] m_BodyParts;

    // is ragdoll physics on
    protected bool m_RagdollEnabled = true;

    protected Vector3? m_ForceVel = null; // hit force velocity
    protected bool m_Initialized = false; // is class initialized

    protected Transform m_RootTransform;

    protected int? m_HitParts = null; // body parts hit array

    protected bool m_InRagdoll = false; //in Ragdoll play                             
    protected float currentRagdollTime = 0.0f;
    public float totalRagdollTime = 4.0f;
    public Action<Vector3> onCompelted = null;
    public Action onCreateCompeleted = null;
    #endregion


    #region Properties

    /// <summary>
    /// return true if component is initialized
    /// </summary>
    public bool Initialized { get { return m_Initialized; } }

    /// <summary>
    /// gets spine bone transform
    /// </summary>
    public Transform RootTransform
    {
        get
        {
            return m_RootTransform;
        }
    }

    public Vector3 RootPosition
    {
        get
        {
            return m_RootTransform.position;
        }
    }

    /// <summary>
    /// gets number of bodyparts
    /// </summary>
    public abstract int BodypartCount { get; }

    #endregion

#if UNITY_EDITOR
    //画线：
    public bool DrawDir = false;
    public Vector3 OrignPos = Vector3.zero;
    public Vector3 TestDir = Vector3.zero;
    public Vector3 OrignDir = Vector3.zero;
    private void OnDrawGizmos()
    {
        if (DrawDir)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(OrignPos, OrignPos + new Vector3(TestDir.x, 0, TestDir.z));
            Gizmos.color = Color.red;
            Gizmos.DrawLine(OrignPos, OrignPos + 10 * OrignDir);
        }
    }
#endif

    public virtual void Initialize(){}
    public virtual void CustomRagdoll(int? hit_parts = null, Vector3? hitForce = null) { }
    public virtual void DisableCustomRagdoll() { }

    protected virtual void enableRagdoll(bool gravity = true)
    {
#if DEBUG_INFO
            if (m_BodyParts == null) { Debug.LogError("object cannot be null."); return; }
#endif

        if (m_RagdollEnabled)
        {
            return;
        }

        m_InRagdoll = true;

        for (int i = 0; i < m_BodyParts.Length; ++i)
        {
#if DEBUG_INFO
                if (m_BodyParts[i] == null) { Debug.LogError("object cannot be null."); continue; }
#else
            if (m_BodyParts[i] == null || m_BodyParts[i].ignoreRagdoll == true) continue;
#endif
            if (m_BodyParts[i].collider != null)
            {
                m_BodyParts[i].collider.enabled = true;
                m_BodyParts[i].collider.isTrigger = false;
            }
#if DEBUG_INFO
                else Debug.LogWarning("body part collider is null.");
#endif


            if (m_BodyParts[i].rigidBody)
            {
                m_BodyParts[i].rigidBody.useGravity = gravity;
                m_BodyParts[i].rigidBody.isKinematic = false;
            }
#if DEBUG_INFO
                else Debug.LogWarning("body part rigid body is null.");
#endif

        }
        m_RagdollEnabled = true;

    }

    protected virtual void disableRagdoll()
    {
#if DEBUG_INFO
            if (m_BodyParts == null) { Debug.LogError("object cannot be null."); return; }
#endif

        if (!m_RagdollEnabled) return;

        m_InRagdoll = false;

        for (int i = 0; i < m_BodyParts.Length; ++i)
        {
#if DEBUG_INFO
                if (m_BodyParts[i] == null) { Debug.LogError("object cannot be null."); continue; }
#else
            if (m_BodyParts[i] == null) continue;
#endif
            if (m_BodyParts[i].collider != null)
            {
                m_BodyParts[i].collider.enabled = false;
                //m_BodyParts[i].collider.isTrigger = true;
            }
#if DEBUG_INFO
                else Debug.LogWarning("body part collider is null.");
#endif

            if (m_BodyParts[i].rigidBody)
            {
                m_BodyParts[i].rigidBody.useGravity = false;
                m_BodyParts[i].rigidBody.isKinematic = true;
            }
#if DEBUG_INFO
                else Debug.LogWarning("body part rigid body is null.");
#endif
            m_RagdollEnabled = false;
        }
    }

    #region 设置
    public void SetRagdollParam(int? hit_parts = null, Vector3? hitForce = null)
    {
        m_HitParts = hit_parts;
        m_ForceVel = hitForce;
    }

    public void SetIgnoreRagdoll(Transform ragdollTrans, bool ignoreRagdollEffect = false)
    {
        for (int i = 0; i < BodypartCount; i++)
        {
            if (m_BodyParts[i].transform == ragdollTrans)
            {
                m_BodyParts[i].ignoreRagdoll = ignoreRagdollEffect;
                if(ignoreRagdollEffect)
                {
                    m_BodyParts[i].constraintJoint.connectedBody = null;
                }
                else
                {
                    m_BodyParts[i].constraintJoint.connectedBody = m_BodyParts[i].jointConnectBody;
                }
            }
        }
    }
    #endregion

    #region 运行
    protected void RunRagdoll()
    {
#if DEBUG_INFO
            if (m_BodyParts == null) { Debug.LogError("object cannot be null."); return; }
#endif
        enableRagdoll(true);

#if SAVE_ANIMATOR_STATES
            saveAnimatorStates();
#endif

        for (int i = 1; i < BodypartCount; i++)
        {
            if (m_BodyParts[i].ignoreRagdoll == true) continue;
            BodyPartInfo b = m_BodyParts[i];
            b.transform.SetParent(transform);
        }

        if (m_HitParts != null)
        {
            if (m_ForceVel.HasValue)
            {
                BodyPartInfo b = m_BodyParts[m_HitParts.Value];
                b.rigidBody.velocity = m_ForceVel.Value;
            }
        }

        m_ForceVel = null;
        m_HitParts = null;
    }

    protected void RunByRecover()
    {
        StartCoroutine(RunRagdollByDelay());
    }

    protected IEnumerator RunRagdollByDelay()
    {
        RunRagdoll();
        yield return new WaitForSeconds(StopTime);
        disableRagdoll();
    }
    #endregion
}
