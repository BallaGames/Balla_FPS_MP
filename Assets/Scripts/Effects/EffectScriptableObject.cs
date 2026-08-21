using UnityEngine;

[CreateAssetMenu(fileName = "EffectScriptableObject", menuName = "Scriptable Objects/EffectScriptableObject")]
public class EffectScriptableObject : ScriptableObject
{
    [SerializeReference] public Effect effect;
    public int effectPoolNum;
    public EffectParticle prefab;
}
