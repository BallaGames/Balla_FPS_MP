using UnityEngine;

public class PMatQueryTest : MonoBehaviour
{
    Collider col;
    public bool doTest;

    private void OnValidate()
    {
        if(col == null)
            col = GetComponent<Collider>();

        if (doTest)
        {
            doTest = false;
            Test();
        }
    }
    void Test()
    {
        Debug.Log(col.sharedMaterial.name);
    }
}
