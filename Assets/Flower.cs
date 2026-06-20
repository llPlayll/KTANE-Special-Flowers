using System;
using System.Collections;
using UnityEngine;
using Rnd = UnityEngine.Random;

public class Flower : MonoBehaviour
{
    Vector3 fullScale = new Vector3(0.035f, 0.035f);
    Vector3 initRotation = new Vector3(90, 0);
    Vector3 spinRotation;
    SpriteRenderer sprite;
    Collider fCollider;
    GameObject Label;
    public int flowerNumber;
    public specialFlowers Module;
    public bool flowerHit;
    RaycastHit[] allHit;

    void Start ()
    {
        sprite = GetComponent<SpriteRenderer>();
        fCollider = GetComponent<SphereCollider>();
        Label = transform.GetChild(0).gameObject;

        spinRotation = new Vector3(90, 0, Rnd.Range(80, 135));
        transform.localScale = Vector3.zero;
        Label.SetActive(false);
    }

    public IEnumerator Appear()
    {
        flowerHit = false;
        if (Module.sequencePlayedOnce)
        {
            if (Module.tpActive)
            {
                if (Module.TPShouldCheckInput)
                {
                    int idx = Module.sequence.IndexOf(x => x == flowerNumber);
                    if (Module.TPInput[idx] == flowerNumber)
                    {
                        flowerHit = true;
                        StartCoroutine("FadeToColor", Color.green);
                    }
                    else StartCoroutine("FadeToColor", Color.red);
                }
            }
            else StartCoroutine("CheckCollision");
        }
        for (float t = 0; t < 1f; t += Time.deltaTime / (60 / 145.35f))
        {
            transform.localScale = Vector3.Lerp(Vector3.zero, fullScale, OutCirc(t));
            transform.localEulerAngles = Vector3.Lerp(initRotation, spinRotation, OutCirc(t));
            yield return null;
        }
        transform.localScale = fullScale;
        transform.localEulerAngles = spinRotation;
    }

    public IEnumerator ShowLabel()
    {
        Label.SetActive(true);
        for (float t = 0; t < 1f; t += Time.deltaTime / (180 / 145.35f))
        {
            transform.localScale = Vector3.Lerp(Vector3.zero, fullScale, OutCirc(t));
            yield return null;
        }
        transform.localScale = fullScale;
        for (float t = 0; t < 1f; t += Time.deltaTime / 3) yield return null;
        for (float t = 0; t < 1f; t += Time.deltaTime / (180 / 145.35f))
        {
            transform.localScale = Vector3.Lerp(fullScale, Vector3.zero, InCirc(t));
            yield return null;
        }
        transform.localScale = Vector3.zero;
        Label.SetActive(false);
        Module.animating = false;
    }

    public IEnumerator CheckCollision()
    {
        for (float t = 0; t < 1f; t += Time.deltaTime / 0.2f)
        {
            allHit = Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition));
            foreach (RaycastHit hit in allHit)
            {
                if (fCollider.name == hit.collider.name)
                {
                    flowerHit = true;
                    StartCoroutine("FadeToColor", Color.green);
                    break;
                }
            }
            yield return null;
        }
        if (!flowerHit) StartCoroutine("FadeToColor", Color.red); ;
    }

    public IEnumerator FadeOut()
    {
        Color initColor = sprite.color;
        for (float t = 0; t < 1f; t += Time.deltaTime / (180 / 145.35f))
        {
            sprite.color = Color.Lerp(initColor, Color.black, t);
            yield return null;
        }
        transform.localScale = Vector3.zero;
        transform.localEulerAngles = initRotation;
        sprite.color = Color.white;
    }

    IEnumerator FadeToColor(Color finalColor)
    {
        Color initColor = sprite.color;
        for (float t = 0; t < 1f; t += Time.deltaTime / (30 / 145.35f))
        {
            sprite.color = Color.Lerp(initColor, finalColor, t);
            yield return null;
        }
        sprite.color = finalColor;
    }

    public static float InCirc(float t) => -((float)Math.Sqrt(1 - t * t) - 1);
    public static float OutCirc(float t) => 1 - InCirc(1 - t);
}
