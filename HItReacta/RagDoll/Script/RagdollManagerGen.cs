using FrameWork;
using System;
using UnityEngine;

/// <summary>
/// ragdoll --> gen
/// </summary>
[System.Serializable]
public class RagdollManagerGen : RagdollManager
{
    public float forceFactor = 1.0f;
    /// <summary>
    /// gets number of bodyparts
    /// </summary>
    public override int BodypartCount
    {
        get
        {
            if (m_BodyParts == null) return 0;
            return m_BodyParts.Length;
        }
    }

    #region Unity_Event
    public void Awake()
    {
        m_RootTransform = m_BodyParts[0].transform;
        m_RagdollEnabled = true;
        disableRagdoll();
    }
    #endregion

    #region Init
    /// <summary>
    /// 初始化默认动物骨骼
    /// 分为11部分 --> 按照顺序映射joint关系
    ///spine(1) --> pelvis(0) , head(2) --> spine(1) ,  arm --> spine(1) , leg --> pelvis(0)
    /// </summary>
    public void DefaultInitialize()
    {
        if(null == RagdollBones || RagdollBones.Length < 1)
        {
            Debug.LogError("没有设置骨骼位置");
            return;
        }

        //根据m_BodyParts设置collider,joint
        //InitRootBone();
        if (RagdollBones.Length > 2)
        {
            //InitHeadBone();
        }
        for(int i = 0; i < RagdollBones.Length; i++)
        {
            //Pevils
            if(i == 0)
            {
                InitBones(RagdollBones[i], ColliderType.BOX, -1, 0, 0.1f, 0.1f, new Vector3(0.15f,0.15f,0.15f));
            }
            //body
            else if(i == 1)
            {
                InitBones(RagdollBones[i], ColliderType.BOX, 0, 0, 0.1f, 0.1f, new Vector3(0.15f, 0.15f, 0.15f));
            }
            //Head
            else if (i == 2)
            {
                InitBones(RagdollBones[i], ColliderType.SPHERE, 1, 0, 0.1f, 0.1f, Vector3.zero);
            }
            //Leg
            else if (i == 3 || i == 4)
            {
                InitBones(RagdollBones[i], ColliderType.CAPSULE, 0, 0, 0.05f, 0.25f, Vector3.zero);
            }
            //Arm
            else if(i == 5 || i == 6)
            {
                InitBones(RagdollBones[i], ColliderType.CAPSULE, 1, 0, 0.05f, 0.25f, Vector3.zero);
            }

            else if(i == 11)
            {
                InitBones(RagdollBones[i], ColliderType.CAPSULE, 0, 0, 0.05f, 0.4f, Vector3.zero);
            }
            //FrontArm,Leg
            else
            {
                InitBones(RagdollBones[i], ColliderType.CAPSULE, i - 4, 0, 0.03f, 0.2f, Vector3.zero);
            }
        }
        InitBodyParts();
    }

    /// <summary>
    /// body初始化参数,初始化CapsuleCollider
    /// </summary>
    /// <param name="boneTrans">父节点</param>
    /// <param name="parentIndex">CharacterJoint连接点</param>
    /// <param name="axis">轴向</param>
    /// <param name="radius">半径</param>
    /// <param name="height">长度</param>
    private void InitBones(Transform boneTrans, ColliderType colliderType, int parentIndex,int axis, float radius, float height, Vector3 size)
    {
        var gameObj = boneTrans.gameObject;
        //Collider
        var collider = gameObj.GetComponent<Collider>();
        if (collider == null)
        {
            if (colliderType == ColliderType.SPHERE)
                collider = gameObj.AddComponent<SphereCollider>();
            if(colliderType == ColliderType.BOX)
                collider = gameObj.AddComponent<BoxCollider>();
            if (colliderType == ColliderType.CAPSULE)
                collider = gameObj.AddComponent<CapsuleCollider>();
        }
        if(colliderType == ColliderType.CAPSULE)
        {
            var tempCollider = collider as CapsuleCollider;
            tempCollider.direction = axis;
            tempCollider.radius = radius;
            tempCollider.height = height;
        }
        else if(colliderType == ColliderType.BOX)
        {
            var tempCollider = collider as BoxCollider;
            tempCollider.size = size;
        }
        else
        {
            var tempCollider = collider as SphereCollider;
            tempCollider.radius = 0.1f;
        }
        collider.enabled = false;

        //RigidBody
        var rigid = gameObj.GetComponent<Rigidbody>();
        if(rigid == null)
        {
            rigid = gameObj.AddComponent<Rigidbody>();
        }
        rigid.useGravity = false;
        rigid.isKinematic = true;

        //Joint
        if (parentIndex >= 0)
        {
            var joint = gameObj.GetComponent<CharacterJoint>();
            if (joint == null)
            {
                joint = gameObj.AddComponent<CharacterJoint>();
            }
            {
                joint.connectedBody = RagdollBones[parentIndex].GetComponent<Rigidbody>();
            }
        }
    }   

    private void InitBodyParts()
    {
        int boneCount = RagdollBones.Length;
        if (m_BodyParts.Length > 0)
        {
            Array.Clear(m_BodyParts, 0, boneCount);
        }
        m_BodyParts = new BodyPartInfo[boneCount];
        for (int i = 0; i < boneCount; i++)
        {
            m_BodyParts[i] = new BodyPartInfo();
            m_BodyParts[i].transform = RagdollBones[i].transform;
            m_BodyParts[i].orig_parent = RagdollBones[i].transform.parent;
            m_BodyParts[i].collider = RagdollBones[i].GetComponent<Collider>();
            m_BodyParts[i].rigidBody = RagdollBones[i].GetComponent<Rigidbody>();
        }
        var trans = m_BodyParts[0].transform.GetComponentsInChildren<Transform>();
        for(int i = 0; i < trans.Length; i++)
        {
            trans[i].gameObject.layer = Game.Const.LAYER_RAGDOLL;
        }
    }

    //todo:简单设置joint,以后与DefaultInit统一
    public override void Initialize()
    {
        DefaultInitialize();
        for (int i = 0; i < (int)BodyParts.BODY_PART_COUNT; ++i)
        {
            CharacterJoint cj = RagdollBones[i].GetComponent<CharacterJoint>();
            if (cj != null)
            {
                m_BodyParts[i].constraintJoint = cj;
                m_BodyParts[i].jointConnectBody = cj.connectedBody;
                cj.enableProjection = true;
                cj.enablePreprocessing = false;
            }
        }
        //DefaultInitialize();
    }

    #endregion

    #region Func

    public override void DisableCustomRagdoll()
    {
        disableRagdoll();

        for (int i = 1; i < BodypartCount; i++)
        {
            BodyPartInfo b = m_BodyParts[i];
            b.transform.SetParent(b.orig_parent);
        }
    }

    public override void CustomRagdoll(int? hit_parts = null, Vector3? hitForce = null)
    {
        //0~1部位随机
        int hitPart = Global.RandomRange(0,1);
        base.SetRagdollParam(hitPart, hitForce.Value * forceFactor);
        RunByRecover();
    }
    #endregion
}
