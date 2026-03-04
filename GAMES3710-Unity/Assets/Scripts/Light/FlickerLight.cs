using UnityEngine;
using System.Collections;

public class FlickerLight : MonoBehaviour
{
    [Header("Visual")]
    public Material lightMaterial;
    public Light spotLight;                 // 加这个
    public Color emissionColor = Color.white;

    public float brightIntensity = 8f;
    public float weakIntensity = 1f;

    [Header("Light Intensity")]
    public float brightLightIntensity = 8f; // Spotlight 亮的时候
    public float weakLightIntensity = 0.8f; // Spotlight 暗的时候

    [Header("Audio")]
    public AudioSource humAudio;
    public AudioSource glitchSource;
    public AudioClip[] glitchClips;

    [Header("Hum Settings")]
    public float humMinVolume = 0.05f;
    public float humMaxVolume = 0.15f;
    public float humNoiseSpeed = 0.2f;

    [Header("Silence Settings")]
    public float silenceChance = 0.3f;
    public float silenceMinTime = 1f;
    public float silenceMaxTime = 3f;

    [Header("Glitch Settings")]
    public float glitchMinInterval = 3f;
    public float glitchMaxInterval = 8f;

    bool isSilent = false;

    void Awake()
    {
        // 如果你忘了拖，自动找一下
        if (spotLight == null) spotLight = GetComponentInChildren<Light>();
    }

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
        // 确保发光关键字开着（内置/部分材质需要）
        if (lightMaterial != null) lightMaterial.EnableKeyword("_EMISSION");

        while (true)
        {
            int flickerCount = Random.Range(5, 15);

            for (int i = 0; i < flickerCount; i++)
            {
                bool bright = Random.value > 0.5f;

                float matIntensity = bright ? brightIntensity : weakIntensity;
                float lightIntensity = bright ? brightLightIntensity : weakLightIntensity;

                if (lightMaterial != null)
                    lightMaterial.SetColor("_EmissionColor", emissionColor * matIntensity);

                if (spotLight != null)
                    spotLight.intensity = lightIntensity;

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

            if (glitchSource != null && glitchClips != null && glitchClips.Length > 0)
            {
                AudioClip clip = glitchClips[Random.Range(0, glitchClips.Length)];
                if (clip != null) glitchSource.PlayOneShot(clip);
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

                // 静音时也让灯更暗一点更像断电
                if (spotLight != null) spotLight.intensity = 0f;
                if (lightMaterial != null) lightMaterial.SetColor("_EmissionColor", emissionColor * 0f);

                yield return new WaitForSeconds(Random.Range(silenceMinTime, silenceMaxTime));

                isSilent = false;
            }
        }
    }
}