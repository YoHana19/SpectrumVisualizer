using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioEffecter : MonoBehaviour
{
    [SerializeField] private GameObject cubePrefab;
    [SerializeField] private float amp;
    [SerializeField] private int num;
    [SerializeField] private int threshold;
    private List<Transform> cubes = new List<Transform>();
    private AudioSource audioSource;
    private const int highFreq = 3000; // 表示する周波数の上限
    private float[] spectrum = new float[1024];
    private int unit = 0;
    private float startHue;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        GenerateCubes();
    }

    private void Start()
    {
        num = cubes.Count;
        unit = highFreq / num;
    }

    private void Update()
    {
        SpectrumEffect();
    }

    private void SpectrumEffect()
    {
        var deltaFreq = AudioSettings.outputSampleRate / spectrum.Length;
        audioSource.GetSpectrumData(spectrum, 0, FFTWindow.Blackman);
        var upper = unit;
        var max = 0f;
        for (int i = 0; i < spectrum.Length; i++)
        {
            var freq = deltaFreq * i;
            if (freq < upper)
            {
                if (max < spectrum[i]) max = spectrum[i];
            }
            else
            {
                ChangeCube(max, (upper / unit) - 1);
                upper += unit;
                max = spectrum[i];
            }
        }
    }

    private void GenerateCubes()
    {
        var start = -num / 2;
        for (int i = 0; i < num; i++)
        {
            var cube = Instantiate(cubePrefab);
            var posX = start + i * 2f;
            cube.transform.position = new Vector3(posX, 0, 0);
            float hue = startHue + (float)i * (0.15f / (float)num);
            var r = cube.GetComponent<Renderer>();
            r.material.color = Color.HSVToRGB(hue, 11, 1);
            r.material.SetColor("_EmissionColor", Color.HSVToRGB(hue, 1, 1));
            cubes.Add(cube.transform);
        }
    }

    private void ChangeCube(float power, int index)
    {
        if (index > cubes.Count - 1) return;
        var value = power * amp;
        cubes[index].localScale = new Vector3(1, value, 1);
        cubes[index].position = new Vector3(cubes[index].position.x, value / 2, cubes[index].position.z);
        if (value > threshold) ChangeHue(); // ビートに合わせて色を変える
    }

    private bool isHold;
    private void ChangeHue()
    {
        if (isHold) return;
        StartCoroutine(WaitTime()); // 次に色を変えるまで0.1秒のバッファをとる
        startHue += 0.15f * Random.Range(1, 6);
        if (startHue > 1f) startHue -= 1f;
        for (int i = 0; i < num; i++)
        {
            float hue = startHue + (float)i * (0.15f / (float)num);
            var r = cubes[i].GetComponent<Renderer>();
            r.material.color = Color.HSVToRGB(hue, 1, 1);
            r.material.SetColor("_EmissionColor", Color.HSVToRGB(hue, 1, 1));
        }
    }

    private IEnumerator WaitTime()
    {
        isHold = true;
        yield return new WaitForSeconds(0.1f);
        isHold = false;
    }
}
