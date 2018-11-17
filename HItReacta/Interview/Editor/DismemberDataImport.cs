using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using LitJson;

public class DismemberDataImport : MonoBehaviour
{

    [Serializable]
    public class dismemberItemInfo
    {
        public string prefab;
        public int bodyPartHint;
        public int dismemberType;
        public int powerType;
        public double mass;
        public double anglemass;
        public double[] speed;
        public double[] anglespeed;
    }

    [Serializable]
    public class DismemberItemsInfos
    {
        public List<dismemberItemInfo> dismemberItems;
        public DismemberItemsInfos()
        {
            dismemberItems = new List<dismemberItemInfo>();
        }

        public void AddItem(dismemberItemInfo testInfo)
        {
            dismemberItems.Add(testInfo);
        }
    }

    /// <summary>
    /// 恢复肢解param,碰撞体需要手调
    /// </summary>
    [MenuItem("游戏拓展/肢解/肢解数据初始化")]
    static void RecoverDismemberParam()
    {
        var dismemberObjs = FindObjectsOfType(typeof(Dismemberment)) as Dismemberment[];
        foreach (var dismemberItem in dismemberObjs)
        {
            dismemberItem.InitDefaultParam();
            if (PrefabUtility.GetPrefabType(dismemberItem.gameObject) == PrefabType.PrefabInstance)
            {
                UnityEngine.Object parentObject = PrefabUtility.GetPrefabParent(dismemberItem.gameObject);
                PrefabUtility.ReplacePrefab(dismemberItem.gameObject, parentObject, ReplacePrefabOptions.ConnectToPrefab);
            }
        }
    }

    //肢解参数导入
    [MenuItem("游戏拓展/肢解/数据导入")]
    static void ImportDismemberData()
    {
        string pathJson = Application.dataPath + "/LevelEditor/Input/cfg.json";
        var dataString = string.Empty;
        using (var sr = File.OpenText(pathJson))
        {
            dataString = sr.ReadToEnd();
        }
        if (string.IsNullOrEmpty(dataString))
        {
            Debug.LogError("json is null");
            return;
        }

        var jsonMap = JsonMapper.ToObject(dataString);
        var dismemberItems = jsonMap["dismemberItems"] as JsonData;
        var items = JsonMapper.ToObject<Dictionary<string, List<dismemberItemInfo>>>(dismemberItems.ToJson());
        var dismemberObjs = FindObjectsOfType(typeof(Dismemberment)) as Dismemberment[];
        foreach (var tempItem in items)
        {
            var name = tempItem.Value[0].prefab;
            foreach (var dismemberObj in dismemberObjs)
            {
                if (dismemberObj.name == name)
                {
                    {
                        var boomParam = dismemberObj.boomParam;
                        for (int i = 0; i < tempItem.Value.Count; i++)
                        {
                            var dismemberItem = tempItem.Value[i];
                            //JsonMapper.ToObject<dismemberItemInfo>(dismemberItems[i].ToJson());
                            boomParam[i / 5].bodyPart = (Dismemberment.BodyPartHint)dismemberItem.bodyPartHint;
                            boomParam[i / 5].forceParams[i % 5].drag = (float)dismemberItem.mass;
                            boomParam[i / 5].forceParams[i % 5].angleDrag = (float)dismemberItem.anglemass;
                            boomParam[i / 5].forceParams[i % 5].force = new Vector3((float)dismemberItem.speed[0], (float)dismemberItem.speed[1], (float)dismemberItem.speed[2]);
                            boomParam[i / 5].forceParams[i % 5].torque = new Vector3((float)dismemberItem.anglespeed[0], (float)dismemberItem.anglespeed[1], (float)dismemberItem.anglespeed[2]);
                        }

                        if (PrefabUtility.GetPrefabType(dismemberObj.gameObject) == PrefabType.PrefabInstance)
                        {
                            UnityEngine.Object parentObject = PrefabUtility.GetPrefabParent(dismemberObj.gameObject);
                            PrefabUtility.ReplacePrefab(dismemberObj.gameObject, parentObject, ReplacePrefabOptions.ConnectToPrefab);
                        }
                    }
                }
            }
        }
    }

    //肢解参数导入
    [MenuItem("游戏拓展/肢解/布娃娃参数")]
    static void InitRagdollParam()
    {
        var ragdollObjs = FindObjectsOfType(typeof(RagdollManager)) as RagdollManager[];
        foreach (var ragdollItem in ragdollObjs)
        {
            ragdollItem.Initialize();
            if (PrefabUtility.GetPrefabType(ragdollItem.gameObject) == PrefabType.PrefabInstance)
            {
                UnityEngine.Object parentObject = PrefabUtility.GetPrefabParent(ragdollItem.gameObject);
                PrefabUtility.ReplacePrefab(ragdollItem.gameObject, parentObject, ReplacePrefabOptions.ConnectToPrefab);
            }
        }
    }
}
