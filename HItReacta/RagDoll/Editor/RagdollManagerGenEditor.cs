using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RagdollManagerGen))]
public class RagdollManagerGenEditor : Editor
{
    public override void OnInspectorGUI()
    {
        RagdollManagerGen ragMan = (RagdollManagerGen)target;

        DrawDefaultInspector();

        bool defaultRagdoll = GUILayout.Button("设置通用Ragdoll");
        if (defaultRagdoll)
        {
            ragMan.DefaultInitialize();

            //for (int i = 0; i < ragMan.RagdollBones.Length; i++)
            //{
            //    Transform t = ragMan.RagdollBones[i];
            //    if (!t) continue;
            //    CharacterJoint[] t_joints = t.GetComponents<CharacterJoint>();
            //    Collider[] t_cols = t.GetComponents<Collider>();
            //    Rigidbody[] t_rbs = t.GetComponents<Rigidbody>();

            //    foreach (CharacterJoint cj in t_joints)
            //        DestroyImmediate(cj);
            //    foreach (Collider c in t_cols)
            //        DestroyImmediate(c);
            //    foreach (Rigidbody rb in t_rbs)
            //        DestroyImmediate(rb);
            //    ragMan.RagdollBones[i] = null;
            //}
            //ragMan.RagdollBones = null;

            //EditorUtility.SetDirty(ragMan);
            //serializedObject.ApplyModifiedProperties();
        }


        if (GUI.changed)
        {
            EditorUtility.SetDirty(ragMan);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
