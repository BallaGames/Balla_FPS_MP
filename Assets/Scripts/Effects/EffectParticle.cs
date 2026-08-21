using UnityEngine;
using UnityEngine.VFX;

public class EffectParticle : MonoBehaviour
{
    public VisualEffect vfx;

    public int poolID, globalID, localID;

    public Transform followTransform;

    public void Play()
    {
        if(vfx != null)
            vfx.Play();
    }
    public void Stop()
    {
        if (vfx != null)
            vfx.Stop();
    }

    private void Update()
    {
        if(followTransform != null)
        {
            transform.position = followTransform.position;
        }
    }
}
