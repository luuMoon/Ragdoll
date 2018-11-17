## 怪物的布娃娃以及肢解方案实现

* 没有做与动画融合处理,在死亡时,关闭动画animation及CharacterController
* 肢解方案最初设想为代码分离骨骼节点，最后改为skinmesh中实现骨骼分离，程序中只做父节点转移
* 游戏中采用GameObj对象池循环利用物体,恢复父节点并恢复joint及ragidbody参数


1. RagdollManager为布娃娃基类，包含初始化，ragdoll的开启，关闭
2. RagdollManagerHum：人形怪的布娃娃，可利用Unity自带Ragdoll系统生成人形rigidbody等
3. RagdollManagerGen: 普通怪的布娃娃，手动实现rigidbody，joint等设置
4. Dismember：处理肢解，如头部、腿部等分离，且处理血液贴画
5. WeaponDismember: 武器肢解，持有武器的怪物，肢解时武器脱手
6. DismemberConfig: 肢解，布娃娃等编辑器
7. DismemberDataImport： 处理相关参数，策划导表配置


使用方式：

```cs
       /// <summary>
        /// 
        /// </summary>
        /// <param name="boomPart">肢解部位</param>
        /// <param name="power">Power : 力参数</param>
        /// <param name="otherboomNum">其余肢解数量</param>
        /// <param name="power">布娃娃肢解大小</param>
        /// <param name="x">布娃娃方向X</param>
        /// <param name="y">布娃娃方向Y</param>
        /// <param name="z">布娃娃方向Z</param>
        public void Dismember(int boomPart, int power, int otherboomNum, float x, float y, float z)
        {
            if(isDeathShow)
                return;
            ReadyForDeathShow();
            if(dismemberment == null)
            {
                return;
            }

            List<int> otherBoomId;
            int boomId = dismemberment.FindBoomPart(boomPart);
            //boomId为-1 : 找不到肢解部位则返回
            if (boomId != -1)
            {
                dismemberment.FindOtherDismemberId(boomId, otherboomNum, out otherBoomId);
                otherBoomId.Add(boomId);
                if (theInfo.weaponDismembers != null)
                {
                    for (int i = 0; i < theInfo.weaponDismembers.Length; i++)
                    {
                        theInfo.weaponDismembers[i].Do();
                    }
                }
                if (null != dismemberment)
                    dismemberment.Dismember(otherBoomId.ToArray(), power);
            }
            else
            {
                FrameWork.Debugger.LogError("Have dismember error partID:",boomPart);
                //确保ragdoll肢解
                boomId = 1;
            }
            if (null!=ragdollManager)
            {
#if UNITY_EDITOR
                ragdollManager.OrignDir = new Vector3(x, 0, z);
#endif  
                int ragdollId = Config.Instance.GetBodyPartId(boomId);
                if (x == 0 && z == 0)
                {
#if UNITY_EDITOR
                    ragdollManager.OrignDir = ragdollManager.transform.forward;
#endif
                    ragdollManager.CustomRagdoll(ragdollId, ragdollManager.transform.forward);
                    return;
                }
                var dir = Config.Instance.GetRagdollDir(ragdollId, power);
               // Debug.LogError("力方向参数:" + dir);
                //var dirLength = dir.x * dir.x + dir.z * dir.z;
                //var dirSin = z / Mathf.Sqrt(x * x + z * z);
                float dirDegree =
                    Mathf.Acos(Mathf.Clamp(Vector2.Dot(new Vector2(0, 1), new Vector2(x, z).normalized), -1, 1)) *
                    Mathf.Rad2Deg;
                var dirLength = Mathf.Sqrt(dir.x * dir.x + dir.z * dir.z);
                var dirSin = z / Mathf.Sqrt(x * x + z * z);
                var dirCos = x / Mathf.Sqrt(x * x + z * z);
#if UNITY_EDITOR
                Debug.Log("<color=red>RagdollPart:</color>" + ragdollId);
                Debug.Log("<color=red>RagdollDir:</color>" + new Vector3(dirLength * dirCos, dir.y, dirLength * dirSin));
#endif
                ragdollManager.CustomRagdoll(ragdollId, new Vector3(dirLength * dirCos, dir.y, dirLength * dirSin));
                 
               // Debug.LogError("肢解方向:" + new Vector3(Mathf.Sqrt(dirLength * (1 - dirSin * dirSin)), dir.y, dirSin * Mathf.Sqrt(dirLength)));
               // ragdollManager.CustomRagdoll(ragdollId, new Vector3(Mathf.Sqrt(dirLength * (1 - dirSin * dirSin)), dir.y, dirSin * Mathf.Sqrt(dirLength)));
            }
        }
        //布娃娃
        public void Ragdoll(int boomPart,int power, float x, float y, float z)
        {
            if (isDeathShow)
                return;
            ReadyForDeathShow();
            if(null!=ragdollManager)
            {
                if (theInfo.weaponDismembers != null)
                {
                    for (int i = 0; i < theInfo.weaponDismembers.Length; i++)
                    {
                        theInfo.weaponDismembers[i].Do();
                    }
                }
#if UNITY_EDITOR
                ragdollManager.OrignDir = new Vector3(x, 0, z);
#endif
                //Debug.LogError("原始方向:" + ragdollManager.OrignDir);
                int ragdollId = Config.Instance.GetRagdollPartId(boomPart);
                if (x == 0 && z == 0)
                {
#if UNITY_EDITOR
                    ragdollManager.OrignDir = ragdollManager.transform.forward;
#endif
                    ragdollManager.CustomRagdoll(ragdollId, ragdollManager.transform.forward);
                    return;
                }
                var dir = Config.Instance.GetRagdollDir(ragdollId, power);
                //Debug.LogError("力方向参数:" + dir);
                var dirLength = Mathf.Sqrt(dir.x * dir.x + dir.z * dir.z);
                var dirSin = z / Mathf.Sqrt(x * x + z * z);
                var dirCos = x / Mathf.Sqrt(x * x + z * z);
#if UNITY_EDITOR
                Debug.Log("<color=red>RagdollPart:</color>" + ragdollId);
                Debug.Log("<color=red>RagdollDir:</color>" + new Vector3(dirLength * dirCos, dir.y, dirLength * dirSin));
#endif
                ragdollManager.CustomRagdoll(ragdollId, new Vector3(dirLength * dirCos, dir.y, dirLength * dirSin));
            }
        }

        public void RecoverFromDeathShow()
        {
            if(!isDeathShow)
                return;
            isDeathShow = false;
            if (dismemberment != null)
            {
                dismemberment.Recover();
            }

            if (ragdollManager != null)
            {
                ragdollManager.DisableCustomRagdoll();
            }

            if (theInfo.weaponDismembers != null)
            {
                for (int i = 0; i < theInfo.weaponDismembers.Length; i++)
                {
                    theInfo.weaponDismembers[i].UnDo();
                }
            }

            if (null != animation)
                animation.enabled = true;
            if (null != cct)
                cct.enabled = true;
        }
        
        private void OnRagdollComplete(Vector3 rootPos)
        {
            if (null != notifyPosSet)
            {
                notifyPosSet(rootPos.x, rootPos.y, rootPos.z);
            }
        }
        private void ReadyForDeathShow()
        {
            isDeathShow = true;
            if(animation != null)
            {
                animation.Stop();
                animation.enabled = false;
            }
            if(cct != null)
            {
                cct.enabled = false;
            }
        }

```

