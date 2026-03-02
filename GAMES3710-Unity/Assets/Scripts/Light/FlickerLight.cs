using UnityEngine;
using System.Collections;

public class FlickerLight : MonoBehaviour
{
    public Material lightMaterial;

    public AudioSource humAudio;
    public AudioSource glitchSource;
    public AudioClip[] glitchClips;

    public float brightIntensity = 8f;
    public float weakIntensity = 1f;

    [Header("Hum Settings")]
    public float humMinVolume = 0.05f;
    public float humMaxVolume = 0.15f;
    public float humNoiseSpeed = 0.2f;

    [Header("Silence Settings")]
    public float silenceChance = 0.3f;      // 静音概率
    public float silenceMinTime = 1f;
    public float silenceMaxTime = 3f;

    [Header("Glitch Settings")]
    public float glitchMinInterval = 3f;
    public float glitchMaxInterval = 8f;

    bool isSilent = false;

    void Start()
    {
        StartCoroutine(FlickerRoutine());
        StartCoroutine(AudioGlitchRoutine());
        StartCoroutine(SilenceRoutine());
    }

    void Update()
    {
        if (humAudio == null) return;

        if (isSilent)
        {
            humAudio.volume = 0f;
        }
        else
        {
            float noise = Mathf.PerlinNoise(Time.time * humNoiseSpeed, 0f);
            humAudio.volume = Mathf.Lerp(humMinVolume, humMaxVolume, noise);
        }
    }

    IEnumerator FlickerRoutine()
    {
        while (true)
        {
            int flickerCount = Random.Range(5, 15);

            for (int i = 0; i < flickerCount; i++)
            {
                if (Random.value > 0.5f)
                    lightMaterial.SetColor("_EmissionColor", Color.white * brightIntensity);
                else
                    lightMaterial.SetColor("_EmissionColor", Color.white * weakIntensity);

                yield return new WaitForSeconds(Random.Range(0.02f, 0.08f));
            }

            yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));
        }
    }

    IEnumerator AudioGlitchRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(glitchMinInterval, glitchMaxInterval));

            if (glitchSource != null && glitchClips.Length > 0)
            {
                AudioClip clip = glitchClips[Random.Range(0, glitchClips.Length)];
                glitchSource.PlayOneShot(clip);
            }
        }
    }

    IEnumerator SilenceRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(4f, 10f));

            if (Random.value < silenceChance)
            {
                isSilent = true;
                yield return new WaitForSeconds(Random.Range(silenceMinTime, silenceMaxTime));
                isSilent = false;
            }
        }
    }
}