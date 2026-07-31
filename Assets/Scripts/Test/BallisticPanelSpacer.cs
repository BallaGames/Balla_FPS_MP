using UnityEngine;

[SelectionBase]
public class BallisticPanelSpacer : MonoBehaviour
{
    public PhysicsMaterial[] materials;
    public bool next, previous;
    public bool getColliders;
    [SerializeField] int index;

    [HideInInspector] public Collider[] colliders;

    public Transform panelRoot;
    [Range(0f, 8f)]
    public float panelDistance;

    [Range(0.5f, 8f)]
    public float panelSpread = 3f;
    [Range(0.05f, 1f)]
    public float panelThickness = .2f;

    public Vector2 xyPanelSize;
    

    private void Start()
    {
        getColliders = true;
        Setup();
    }
    void Setup()
    {

        if (getColliders)
        {
            colliders = GetComponentsInChildren<Collider>();
            getColliders = false;
        }

        panelRoot.localPosition = -transform.forward * panelDistance;

        if (colliders != null)
        {
            for(int i = 0; i < colliders.Length; i++)
            {
                Collider c = colliders[i];
                c.transform.localPosition = i * panelSpread * -transform.forward;
                c.transform.localScale = new()
                {
                    x = xyPanelSize.x,
                    y = xyPanelSize.y,
                    z = panelThickness
                };
                c.sharedMaterial = materials[index];
            }
        }
    }


    private void OnValidate()
    {



        if (next)
            index++;
        if(previous)
            index--;
        next = previous = false;
        index = (int)Mathf.Repeat(index, materials.Length);
        Setup();
    }
}
