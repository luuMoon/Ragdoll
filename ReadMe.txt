##怪物的布娃娃以及肢解方案实现


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

