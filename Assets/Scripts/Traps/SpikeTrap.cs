// SpikeTrap.cs
// Implements trap of spikes deployed and retracted from floor or ceiling
// Author:  Dan Blackford
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeTrap : TriggerTrap
{
    public float deployTime;
    public float stayTime;
    public float retractTime;

    const float minSpikeScale = 0.1f;
    const float maxSpikeScale = 3.0f;

    protected override SoundId GetTriggerSound()
    {
        return SoundId.DeploySpikes;
    }

    void Start()
    {
        OnStart();
    }

    void Update()
    {
        if (triggerStart > 0)
        {
            float elapse = (Time.time - triggerStart);

            if (elapse < deployTime)
            {
                //Deploy spikes by slowly increasing scale from base
                float t = (elapse / deployTime);
                t = Mathf.Sin(t * Mathf.PI * 0.5f);
                float y = Mathf.Lerp(minSpikeScale, maxSpikeScale, t);

                SetSpikeScale(y);
            }
            else 
            {
                elapse -= deployTime;
                
                if (elapse < stayTime)
                {
                    //Trap holding at deployed scale
                }
                else
                {
                    elapse -= stayTime;

                    if (elapse < retractTime)
                    {
                        //Retract spikes by decreasing scale from base
                        float t = (elapse / retractTime);
                        t = Mathf.Cos(t * Mathf.PI * 0.5f);
                        float x = Mathf.Lerp(minSpikeScale, maxSpikeScale, t);

                        SetSpikeScale(x);
                    }
                    else
                    {
                        //Spikes retracted, disable damage
                        Retract();
                    }
                }
            }
        }
    }

    private void SetSpikeScale(float x)
    {
        for (int i = 0; i < missles.Length; i++)
        {
            Vector3 localScale = missles[i].transform.localScale;
            localScale.x = x;
            missles[i].transform.localScale = localScale;
        }
    }
}
