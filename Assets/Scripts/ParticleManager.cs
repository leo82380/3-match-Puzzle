using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public GameObject clearFXPrefab;
    public GameObject breakFXPrefab;
    public GameObject doubleBreakFXPrefab;
    public GameObject clearBombFXPrefab;

    public void ClearPieceFXAt(int x, int y, int z = 0)
    {
        if (clearFXPrefab != null)
        {
            GameObject clearFX = Instantiate(clearFXPrefab, new Vector3(x, y, z), Quaternion.identity);
            ParticlePlayer particlePlayer = clearFX.GetComponent<ParticlePlayer>();
            
            if(particlePlayer != null)
            {
                particlePlayer.Play();
            }
        }
    }
    public void BreakTileFXAt(int breakableValue, int x, int y, int z = 0)
    {
        if (breakFXPrefab != null || doubleBreakFXPrefab != null)
        {
            switch (breakableValue)
            {
                case 0:
                    var breakFX = Instantiate(breakFXPrefab, new Vector3(x, y, z), Quaternion.identity);
                    var particlePlayer = breakFX.GetComponent<ParticlePlayer>();
                    if(particlePlayer != null)
                    {
                        particlePlayer.Play();
                    }
                    break;
                case 1:
                    var doubleBreakFX = Instantiate(doubleBreakFXPrefab, new Vector3(x, y, z), Quaternion.identity);
                    var doubleParticlePlayer = doubleBreakFX.GetComponent<ParticlePlayer>();
                    if(doubleParticlePlayer != null)
                    {
                        doubleParticlePlayer.Play();
                    }
                    break;
            }
        }
    }

    public void BombFXAt(int x, int y, int z = 0)
    {
        if(clearBombFXPrefab != null)
        {
            var bombFX = Instantiate(clearBombFXPrefab, new Vector3(x, y, z), Quaternion.identity);
            var particlePlayer = bombFX.GetComponent<ParticlePlayer>();
            if(particlePlayer != null)
            {
                particlePlayer.Play();
            }
        }
    }
}
