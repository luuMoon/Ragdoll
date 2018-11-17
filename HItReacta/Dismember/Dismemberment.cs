using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Game;
using FrameWork;

public class Dismemberment : MonoBehaviour
{
    public readonly string[] defaultCutName = { "Cut_H", "Cut_K", "Cut_LB", "Cut_LB2",
        "Cut_LT", "Cut_RB", "Cut_RB2", "Cut_RT"};

    public enum BodyPartHint
    {
        None = 0,
        HEAD = 1,
        UP_BODY = 2,
        DOWN_BODY = 3,
    }

    #region Params
    private class RandomParam
    {
        public float friction;
        public float angleFriction;
        public Vector3 speed;
        public Vector3 angleSpeed;
    }

    [System.Serializable]
    public class ParticleParms
    {
        public string ParticleName;
        public Transform particleRoot;
        public Vector3 initPos;
        public Vector3 initEurler;
        public Vector3 initScale;
    }

    [System.Serializable]
    public class ForceParams
    {
        public float drag;
        public float angleDrag;
        public Vector3 force;
        public Vector3 torque;
    }


    [System.Serializable]
    public class BoomParams
    {
        public GameObject targetObj;
        public BodyPartHint bodyPart;
        //Transform
        [HideInInspector]
        public Transform parentTrans;
        [HideInInspector]
        public Vector3 position;
        [HideInInspector]
        public Quaternion rotation;
        [HideInInspector]
        public Vector3 scale;
        [HideInInspector]
        public Rigidbody jointRigidbody;
        public CharacterJoint mainJoint;

        //Colider
        public Collider collider;
        public Rigidbody rigidbody;
        public List<ParticleParms> particles;
        //public List<Transform> ignoreRagdollTrans;
        public List<ForceParams> forceParams;

        //public int IgnoreRagdollEffectNum
        //{
        //    get
        //    {
        //        if (ignoreRagdollTrans != null)
        //            return ignoreRagdollTrans.Count;
        //        else
        //            return 0;
        //    }
        //    set
        //    {
        //        if (ignoreRagdollTrans == null)
        //        {
        //            ignoreRagdollTrans = new List<Transform>(value);
        //        }
        //        else
        //        {
        //            if (ignoreRagdollTrans.Count < value)
        //            {
        //                int count = ignoreRagdollTrans.Count;
        //                for (int i = 0; i < value - count; i++)
        //                {
        //                    ignoreRagdollTrans.Add(null);
        //                }
        //            }
        //            else
        //            {
        //                int count = ignoreRagdollTrans.Count;
        //                for (int i = 0; i < count - value; i++)
        //                {
        //                    ignoreRagdollTrans.RemoveAt(ignoreRagdollTrans.Count - 1);
        //                }
        //            }
        //        }
        //    }
        //}

        public int ParticleNum
        {
            get
            {
                if (particles != null)
                    return particles.Count;
                else
                    return 0;
            }
            set
            {
                if (particles == null)
                {
                    particles = new List<ParticleParms>(value);
                }
                else
                {
                    if (particles.Count < value)
                    {
                        int count = particles.Count;
                        for (int i = 0; i < value - count; i++)
                        {
                            ParticleParms temp = new ParticleParms();
                            particles.Add(temp);
                        }
                    }
                    else
                    {
                        int count = particles.Count;
                        for (int i = 0; i < count - value; i++)
                        {
                            particles.RemoveAt(particles.Count - 1);
                        }
                    }
                }
            }
        }

        public int ForceNum
        {
            get
            {
                if (forceParams != null)
                    return forceParams.Count;
                else
                    return 0;
            }
            set
            {
                if (forceParams == null)
                {
                    forceParams = new List<ForceParams>(value);
                }
                else
                {
                    if (forceParams.Count < value)
                    {
                        int count = forceParams.Count;
                        for (int i = 0; i < value - count; i++)
                        {
                            ForceParams temp = new ForceParams();
                            forceParams.Add(temp);
                        }
                    }
                    else
                    {
                        int count = forceParams.Count;
                        for (int i = 0; i < count - value; i++)
                        {
                            forceParams.RemoveAt(forceParams.Count - 1);
                        }
                    }
                }
            }
        }

    }
    #endregion

    #region Editor Func

    //private Transform SetIgnoreRagdoll(BoomParams boomParam)
    //{
    //    Transform root = boomParam.targetObj.transform;
    //    if(root.GetComponent<CharacterJoint>() != null)
    //    {
    //        boomParam.ignoreRagdollTrans.Add(root);
    //    }

    //    Stack<Transform> trans = new Stack<Transform>();
    //    trans.Push(root);
    //    while (trans.Count > 0)
    //    {
    //        var temp = trans.Pop();
    //        for (int i = 0; i < temp.childCount; i++)
    //        {
    //            var childTrans = temp.GetChild(i);
    //            if(childTrans.GetComponent<CharacterJoint>() != null)
    //            {
    //                boomParam.ignoreRagdollTrans.Add(childTrans);
    //            }

    //            trans.Push(childTrans);
    //        }
    //    }

    //    return null;
    //}

    /// <summary>
    /// 设置肢解Child MainJoint
    /// </summary>
    public void SetDismemberChildJoint(BoomParams boomParam)
    {
        Transform root = boomParam.targetObj.transform;
        for (int i = 0; i < root.childCount; i++)
        {
            var childTrans = root.GetChild(i);
            if (childTrans.GetComponent<CharacterJoint>() != null)
            {
                boomParam.mainJoint = childTrans.GetComponent<CharacterJoint>();
                break;
            }
        }
    }

    /// <summary>
    /// 默认参数
    /// </summary>
    public void InitDefaultParam()
    {
        boomParam.Clear();
        //Limb Root
        var temp = transform.Find("Limb");
        if (temp == null)
        {
            GameObject go = new GameObject("Limb");
            temp = go.transform;
            temp.transform.SetParent(transform);
        }

        LimbRoot = temp.transform;

        for (int i = 0; i < defaultCutName.Length; i++)
        {
            BoomParams tempParam = new BoomParams();

            var trans = transform.FindRecursive(defaultCutName[i]);
            if(trans == null)
            {
                Debug.LogError("Cant Find : " + defaultCutName[i]);
                continue;
            }

            tempParam.targetObj = transform.FindRecursive(defaultCutName[i]).gameObject;
            //碰撞体
            if (i != 0 && i != 1)
            {
                DestroyImmediate(tempParam.targetObj.GetComponent<Collider>());
            }
            var collider = tempParam.targetObj.GetComponent<Collider>();
            if (collider == null)
            {
                if (i == 0 || i == 1)
                {
                    collider = tempParam.targetObj.AddComponent<SphereCollider>();
                    var sphereCollider = collider as SphereCollider;
                    sphereCollider.radius = 0.14f;
                }
                else
                {
                    collider = tempParam.targetObj.AddComponent<CapsuleCollider>();
                    var cp = (CapsuleCollider)collider;
                    //小臂
                    if (i == 3 || i == 6)
                    {
                        cp.center = new Vector3(-0.1f, 0, 0);
                        cp.height = 0.44f;
                    }
                    //大腿
                    else if (i == 4 || i == 7)
                    {
                        cp.center = new Vector3(0, 0, 0);
                        cp.height = 0.55f;
                    }
                    //上臂
                    else
                    {
                        cp.center = new Vector3(0, 0, 0);
                        cp.height = 0.35f;
                    }

                    cp.radius = 0.06f;
                    cp.direction = 0;
                }

                collider.enabled = false;
            }
            tempParam.collider = collider;

            var rigidbody = tempParam.targetObj.GetComponent<Rigidbody>();
            if (rigidbody == null)
            {
                rigidbody = tempParam.targetObj.AddComponent<Rigidbody>();
                rigidbody.isKinematic = true;
            }
            tempParam.rigidbody = rigidbody;
            
            //particleParam
            if(tempParam.particles == null)
            {
                // head : (0,90,0), body : (0,-90,0)  pos-> (-0.25,0,0)
                tempParam.particles = new List<ParticleParms>(2);
                ParticleParms pp1 = new ParticleParms();
                pp1.ParticleName = "blood_head";
                pp1.particleRoot = tempParam.targetObj.transform;
                pp1.initPos = Vector3.zero;
                pp1.initEurler = new Vector3(0, 90, 0);
                pp1.initScale = Vector3.one;

                ParticleParms pp2 = new ParticleParms();
                pp2.ParticleName = "blood_body";
                pp2.particleRoot = tempParam.targetObj.transform.parent;
                pp2.initPos = new Vector3(-0.25f, 0, 0);
                pp2.initEurler = new Vector3(0, -90, 0);
                pp2.initScale = Vector3.one;

                //CUT_K
                if (i != 1)
                {
                    tempParam.particles.Add(pp1);
                    tempParam.particles.Add(pp2);
                }
                else
                {
                    tempParam.particles.Clear();
                }
            }

            //forceParam
            if (tempParam.forceParams == null)
            {
                tempParam.forceParams = new List<ForceParams>(4);
                for(int j = 0; j < 5; j++)
                {
                    ForceParams fp = new ForceParams();
                    tempParam.forceParams.Add(fp);
                    tempParam.forceParams[j].drag = 0.1f;
                    tempParam.forceParams[j].angleDrag = 0.1f;
                    tempParam.forceParams[j].force = new Vector3(0,5,0);
                    tempParam.forceParams[j].torque = new Vector3(0,5,0);
                }
            }

            //设置Dismember MainJoint
            //大腿,上臂
            if(i == 2 || i == 4 || i == 5 || i == 7)
            {
                SetDismemberChildJoint(tempParam);
            }
            //tempParam.ignoreRagdollTrans = new List<Transform>();
            //SetIgnoreRagdoll(tempParam);
            boomParam.Add(tempParam);
        }
    }

    #endregion

    #region Var
    private RandomParam rp;
    public Transform LimbRoot;
    public List<BoomParams> boomParam = new List<BoomParams>();
    private List<int> cachedDismember;
    private RagdollManagerHum ragdollManager;
    public List<int> filterId;
    private Dictionary<int, Effect> cachedEffect;
    private Action<Effect> onEffectOver;
    #endregion

    #region Property

    public int BoomParamsNum
    {
        get
        {
            if (boomParam != null)
                return boomParam.Count;
            else
                return 0;
        }
        set
        {
            if (boomParam == null)
                boomParam = new List<BoomParams>(value);
            else
            {
                if (boomParam.Count < value)
                {
                    int count = boomParam.Count;
                    for (int i = 0; i < value - count; i++)
                    {
                        BoomParams temp = new BoomParams();
                        boomParam.Add(temp);
                    }
                }
                else
                {
                    int count = boomParam.Count;
                    for (int i = 0; i < count - value; i++)
                    {
                        boomParam.RemoveAt(boomParam.Count - 1);
                    }
                }
            }
        }
    }

    #endregion


    #region UnityEvent

    private void Start()
    {
        cachedDismember = new List<int>();
        ragdollManager = GetComponent<RagdollManagerHum>();
        InitDismemberParams();
        InitRandomParam();
        filterId = new List<int>();
        for (int i = 0; i < BoomParamsNum; i++)
        {
            if (boomParam[i].bodyPart != BodyPartHint.None)
            {
                filterId.Add(i);
            }
        }
    }

    private void OnDestroy()
    {
        cachedDismember = null;
        ReleaseEffects();
    }

    #endregion


    #region Public Method

#if UNITY_EDITOR
    private bool isTestDismembering = false;
    public void TestDismember(int boomid)
    {
        if (!isTestDismembering)
        {
            isTestDismembering = true;
            StartCoroutine("TestDismemberEffect", boomid);
        }
    }

    private IEnumerator TestDismemberEffect(object boomid)
    {
        int partId = (int)boomid / 10;
        int forceId = (int)boomid % 10;
        Do(partId, forceId, false);
        yield return new WaitForSeconds(3.0f);
        ReCoverOne((int)boomid/10);
        yield return new WaitForSeconds(0.5f);
        isTestDismembering = false;
    }
#endif

    public void Dismember(int[] boomids, int forceId)
    {
        for (int i = 0; i < boomids.Length; i++)
        {
            if (boomids[i] >= 0 && boomids[i] < BoomParamsNum && !cachedDismember.Contains(boomids[i]))
            {
                cachedDismember.Add(boomids[i]);
                Do(boomids[i], forceId);
            }
        }
    }

    public void DismemberOne(int boomId, int forceId)
    {
        Do(boomId, forceId);
    }

    public void Crush(int forceId)
    {
        for (int i = 0; i < boomParam.Count; i++)
        {
            Do(i, forceId);
        }
    }

    public void Recover()
    {
        foreach (int temp in cachedDismember)
        {
            UnDo(temp);
        }

        cachedDismember.Clear();
    }

    public void ReCoverDefault()
    {
        for (int i = 0; i < boomParam.Count; i++)
        {
            UnDo(i);
        }
    }

    public void ReCoverOne(int boomId)
    {
        UnDo(boomId);
    }

    public int FindBoomPart(int boomPart)
    {
        var parts = new List<int>();
        for (int i = 0; i < boomParam.Count; i++)
        {
            if(boomParam[i].bodyPart == (BodyPartHint)boomPart)
            {
                parts.Add(i);
            }
        }
        
        if(parts.Count == 0)
        {
            return -1;
        }
        int id = Global.RandomRange(0, parts.Count);
        return parts[id];
    }

    public void FindOtherDismemberId(int boomId, int num, out List<int> otherId)
    {
        var tempFilter = new List<int>(filterId);
        tempFilter.Remove(boomId);
        otherId = new List<int>();
        if (tempFilter.Count >= num)
        {
            for (int i = 0; i < num; i++)
            {
                int tempId = Global.RandomRange(0, tempFilter.Count);
                otherId.Add(tempFilter[tempId]);
                tempFilter.RemoveAt(tempId);
            }
        }
    }

    #endregion

    #region Func

    private void Do(int boomId, int forceId, bool useEffect = true)
    {
        if (boomId >= boomParam.Count)
            return;

        BoomParams tempParam = boomParam[boomId];
        GameObject obj = tempParam.targetObj;
        string name = obj.name;

        tempParam.parentTrans = obj.transform.parent;
        tempParam.position = obj.transform.localPosition;
        tempParam.rotation = obj.transform.localRotation;
        tempParam.scale = obj.transform.localScale;
        obj.transform.SetParent(LimbRoot);
        if (null == onEffectOver)
            onEffectOver = OnEffectOver;
        var tempParticle = tempParam.particles;
        if (tempParticle != null && useEffect)
        {
            for (int i = 0; i < tempParticle.Count; i++)
            {
                var temp = tempParticle[i];
                Effect effect = Effect.PlayAttach(temp.ParticleName, temp.particleRoot,
                                  (tempEffect) =>
                                  {
                                      tempEffect.SetPos(temp.initPos, true);
                                      tempEffect.SetRot(temp.initEurler, true);
                                      tempEffect.SetScale(temp.initScale.x, temp.initScale.y, temp.initScale.z, true);
                                      tempEffect.Play();
                                  },onEffectOver);
                if(null == cachedEffect)
                {
                    cachedEffect = new Dictionary<int, Effect>();
                }
                cachedEffect.Add(effect.oid, effect);
            }
        }

        //rigidbody setting
        tempParam.collider.enabled = true;
        tempParam.rigidbody.isKinematic = false;
        //force Params
        var drag = tempParam.forceParams[forceId].drag;
        tempParam.rigidbody.drag = Global.RandomRange(drag * (1 - rp.friction), drag * (1 + rp.friction));
        var angledrag = tempParam.forceParams[forceId].angleDrag;
        tempParam.rigidbody.angularDrag = Global.RandomRange(angledrag * (1 - rp.angleFriction), angledrag * (1 + rp.angleFriction));
        var force = transform.TransformDirection(tempParam.forceParams[forceId].force);
        tempParam.rigidbody.velocity = new Vector3(Global.RandomRange(force.x * (1 - rp.speed.x), force.x * (1 + rp.speed.x)),
                                            Global.RandomRange(force.y * (1 - rp.speed.y), force.y * (1 + rp.speed.y))
                                            ,Global.RandomRange(force.z * (1 - rp.speed.z), force.z * (1 + rp.speed.z)));
        var torque = transform.TransformDirection(tempParam.forceParams[forceId].torque);
        tempParam.rigidbody.angularVelocity = new Vector3(Global.RandomRange(torque.x * (1 - rp.angleSpeed.x), torque.x * (1 + rp.angleSpeed.x)),
                                            Global.RandomRange(force.y * (1 - rp.angleSpeed.y), torque.y * (1 + rp.angleSpeed.y))
                                            , Global.RandomRange(torque.z * (1 - rp.angleSpeed.z), torque.z * (1 + rp.angleSpeed.z)));

        ////ignore ragdoll effect
        //for (int i = 0; i < tempParam.ignoreRagdollTrans.Count; i++)
        //{
        //    ragdollManager.SetIgnoreRagdoll(tempParam.ignoreRagdollTrans[i], true);
        //}

        //set limb joint
        if (tempParam.mainJoint)
        {
            tempParam.jointRigidbody = tempParam.mainJoint.connectedBody;
            tempParam.mainJoint.connectedBody = tempParam.rigidbody;
            var collider = tempParam.mainJoint.GetComponent<Collider>();
            var rigidbody = tempParam.mainJoint.GetComponent<Rigidbody>();
            if(collider && rigidbody)
            {
                collider.enabled = true;
                rigidbody.isKinematic = false;
                rigidbody.useGravity = true;
            }
        }
    }

    private void UnDo(int boomId)
    {
        BoomParams tempParam = boomParam[boomId];
        GameObject go = tempParam.targetObj;
        tempParam.collider.enabled = true;
        tempParam.rigidbody.isKinematic = true;
        go.transform.SetParent(boomParam[boomId].parentTrans);
        go.transform.localPosition = tempParam.position;
        go.transform.localRotation= tempParam.rotation;
        go.transform.localScale = tempParam.scale;

        tempParam.collider.enabled = false;
        tempParam.rigidbody.isKinematic = true;

        //for (int i = 0; i < tempParam.ignoreRagdollTrans.Count; i++)
        //{
        //    ragdollManager.SetIgnoreRagdoll(tempParam.ignoreRagdollTrans[i], false);
        //}

        if (tempParam.mainJoint)
        {
            tempParam.mainJoint.connectedBody = tempParam.jointRigidbody;
            var collider = tempParam.mainJoint.GetComponent<Collider>();
            var rigidbody = tempParam.mainJoint.GetComponent<Rigidbody>();
            if (collider && rigidbody)
            {
                collider.enabled = false;
                rigidbody.isKinematic = true;
                rigidbody.useGravity = false;
            }
        }

    }

    private void InitRandomParam()
    {
        rp = new RandomParam();
        rp.friction = Config.Instance.GetFrictionRandom();
        rp.angleFriction = Config.Instance.GetAngleFrictionRandom();
        rp.speed = Config.Instance.GetSpeedRandom();
        rp.angleSpeed = Config.Instance.GetAngleSpeedRandom();
    }

    private void InitDismemberParams()
    {
        foreach (var temp in boomParam)
        {
            if (temp.rigidbody != null)
            {
                temp.rigidbody.isKinematic = true;
            }
            if (temp.collider != null)
            {
                temp.collider.enabled = false;
            }
        }
    }

    private void OnEffectOver(Effect effect)
    {
        if (cachedEffect.ContainsKey(effect.oid))
        {
            cachedEffect.Remove(effect.oid);
        }
        effect.Release();
    }

    private void ReleaseEffects()
    {
        if (null != cachedEffect)
        {
            foreach (var kvp in cachedEffect)
            {
                kvp.Value.Release();
            }
            cachedEffect.Clear();
            cachedEffect = null;
        }
    }


    #endregion

}
