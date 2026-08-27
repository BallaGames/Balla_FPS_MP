using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(PlayerMotor))]
public class PlayerMotorEditor : Editor
{
    public VisualTreeAsset vta;
    [SerializeField, HideInInspector] private bool refs, aim, ground, air, slide, ladder, wallrun, downed, misc;
    public override VisualElement CreateInspectorGUI()
    {
        VisualElement inspector = new VisualElement();

        if(vta != null)
        {
            VisualElement uxmlcontent = vta.CloneTree();
            inspector.Add(uxmlcontent);
        }

        return inspector;
    }
}
