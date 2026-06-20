using System.Collections;
using UnityEngine;

public class FlowerManager : MonoBehaviour
{
    [SerializeField] specialFlowers Module;
    [SerializeField] Flower[] Flowers;
    float[] waits = new float[] { 1, 1, 0.5f, 0.5f, 5 };
    bool allCorrect;

    void Start()
    {
        for (int i = 0; i < 20; i++) Flowers[i].Module = Module;
    }

    public IEnumerator Sequence(int[] f)
    {
        for (float t = 0; t < 1f; t += Time.deltaTime / (60 / 145.35f)) yield return null;
        for (int g = 0; g < 4; g++)
        {
            for (int i = 0; i < 5; i++)
            {
                Flowers[f[g * 5 + i] - 1].StartCoroutine("Appear");
                for (float t = 0; t < 1f; t += Time.deltaTime / ((g == 3 && i == 4 ? 2f : waits[i]) * 60 / 145.35f)) yield return null;
            }
        }
        foreach (Flower flower in Flowers) flower.StartCoroutine("FadeOut");
        for (float t = 0; t < 1f; t += Time.deltaTime / (180 / 145.35f)) yield return null;
        if (!Module.sequencePlayedOnce)
        {
            Module.sequencePlayedOnce = true;
            Module.animating = false;
        }
        else
        {
            allCorrect = true;
            foreach (Flower flower in Flowers)
            {
                if (!flower.flowerHit)
                {
                    allCorrect = false;
                    break;
                }
            }
            if (allCorrect) Module.SolveModule();
            else Module.animating = false;
        }
    }

    public IEnumerator ShowLabels()
    {
        foreach (Flower flower in Flowers) flower.StartCoroutine("ShowLabel");
        yield return null;
    }
}
