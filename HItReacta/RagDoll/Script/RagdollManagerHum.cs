using UnityEngine;
using FrameWork;

/// <summary>
/// ragdoll and hit reaction manager
/// </summary>
[System.Serializable]
public class RagdollManagerHum : RagdollManager
{
    /// <summary>
    /// gets number of bodyparts
    /// </summary>
    public override int BodypartCount
    {
        get { return (int) BodyParts.BODY_PART_COUNT; }
    }

    #region 初始化
    public void DefaultInitialize()
    {
        RagdollBuilder builder = new RagdollBuilder();
        builder.InitParams(transform);
        builder.SynchronousRagdoll();

        builder.OnRagdollCreated = () =>
        {
            RagdollBones = new Transform[(int) BodyParts.BODY_PART_COUNT];
            RagdollBones[(int) BodyParts.Spine] = builder.pelvis;
            RagdollBones[(int) BodyParts.Chest] = builder.middleSpine;
            RagdollBones[(int) BodyParts.Head] = builder.head;
            RagdollBones[(int) BodyParts.LeftShoulder] = builder.leftArm;
            RagdollBones[(int) BodyParts.LeftElbow] = builder.leftElbow;
            RagdollBones[(int) BodyParts.RightShoulder] = builder.rightArm;
            RagdollBones[(int) BodyParts.RightElbow] = builder.rightElbow;
            RagdollBones[(int) BodyParts.LeftHip] = builder.leftHips;
            RagdollBones[(int) BodyParts.LeftKnee] = builder.leftKnee;
            RagdollBones[(int) BodyParts.RightHip] = builder.rightHips;
            RagdollBones[(int) BodyParts.RightKnee] = builder.rightKnee;

            builder.OnRagdollCreated = null;
            Initialize();
            if (onCreateCompeleted != null)
            {
                onCreateCompeleted();
            }
        };
        builder.OnCreated();
    }

    /// <summary>
    /// initialize class instance
    /// </summary>
    public override void Initialize()
    {
        if (m_Initialized) return;

        base.Initialize();

        // keep track of colliders and rigid bodies
        m_BodyParts = new BodyPartInfo[(int) BodyParts.BODY_PART_COUNT];

        bool ragdollComplete = true;
        for (int i = 0; i < (int) BodyParts.BODY_PART_COUNT; ++i)
        {
            Rigidbody rb = RagdollBones[i].GetComponent<Rigidbody>();
            Collider col = RagdollBones[i].GetComponent<Collider>();
            if (rb == null || col == null)
            {
                ragdollComplete = false;
#if DEBUG_INFO
                    Debug.LogError("missing ragdoll part: " + ((BodyParts)i).ToString());
#endif
            }

            m_BodyParts[i] = new BodyPartInfo();
            m_BodyParts[i].transform = RagdollBones[i];
            m_BodyParts[i].rigidBody = rb;
            m_BodyParts[i].collider = col;
            m_BodyParts[i].bodyPart = (BodyParts) i;
            m_BodyParts[i].orig_parent = RagdollBones[i].parent;
            CharacterJoint cj = RagdollBones[i].GetComponent<CharacterJoint>();
            if (cj != null)
            {
                m_BodyParts[i].constraintJoint = cj;
                m_BodyParts[i].jointConnectBody = cj.connectedBody;
                cj.enableProjection = true;
            }
        }

        if (!ragdollComplete)
        {
            Debug.LogError("ragdoll is incomplete or missing");
            return;
        }

        m_RootTransform = m_BodyParts[(int) BodyParts.Spine].transform;
        //setLayer
        var trans = m_RootTransform.GetComponentsInChildren<Transform>();
        for (int i = 0; i < trans.Length; i++)
        {
            trans[i].gameObject.layer = Game.Const.LAYER_RAGDOLL;
        }

        m_Initialized = true;
        disableRagdoll();
    }
    #endregion

    #region Unity Event

    public void Awake()
    {
        m_RootTransform = m_BodyParts[(int) BodyParts.Chest].transform;
        m_RagdollEnabled = true;
        disableRagdoll();
    }

    public void Update()
    {
        if (m_InRagdoll)
        {
            if (currentRagdollTime >= totalRagdollTime)
            {
                currentRagdollTime = 0.0f;
                m_InRagdoll = false;
                if (onCompelted != null)
                {
                    onCompelted(RootPosition);
                }
            }

            currentRagdollTime += Time.deltaTime;
        }
    }

    #endregion

    #region Func
    public override void DisableCustomRagdoll()
    {
#if UNITY_EDITOR
        DrawDir = false;
#endif
        disableRagdoll();

        for (int i = 1; i < BodypartCount; i++)
        {
            if (m_BodyParts[i].ignoreRagdoll == true) continue;
            BodyPartInfo b = m_BodyParts[i];
            b.transform.SetParent(b.orig_parent);
        }
    }

    public override void CustomRagdoll(int? hit_parts = null, Vector3? hitForce = null)
    {
        Vector3 ragdollSpeed = Vector3.zero;
        if (Game.Config.Instance != null)
        {
            ragdollSpeed = Game.Config.Instance.GetRagdollSpeedRandom();
        }
        var dir = hitForce.Value;
        //画线: 
#if UNITY_EDITOR
        DrawDir = true;
        OrignPos = transform.position;
        TestDir = dir;
#endif

        base.SetRagdollParam(hit_parts, new Vector3(
                Global.RandomRange(dir.x * (1 - ragdollSpeed.x), dir.x * (1 + ragdollSpeed.x)),
                Global.RandomRange(dir.y * (1 - ragdollSpeed.y), dir.y * (1 + ragdollSpeed.y)),
                Global.RandomRange(dir.z * (1 - ragdollSpeed.z), dir.z * (1 + ragdollSpeed.z)))
                );
        RunByRecover();
    }
    #endregion
}