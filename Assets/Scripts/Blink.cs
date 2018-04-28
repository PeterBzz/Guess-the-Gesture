
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

enum ComponentType
{
    none, text, image
}

public class Blink : MonoBehaviour
{

    public bool isBlinkingOverTime = false;
    public Color firstColor = Color.black;
    public Color secondColor = Color.white;
    public float speed = 1;
    public UnityEvent onBlinkingFinished;

    ComponentType componentType = ComponentType.none;
    MaskableGraphic blinkingComponent;
    float blinkingStartTime = 0;
    int blinksCount = 0;

    void Start()
    {
        blinkingComponent = this.gameObject.GetComponent<Text>();
        if (blinkingComponent)
        {
            componentType = ComponentType.text;
        }
        else
        {
            blinkingComponent = this.gameObject.GetComponent<Image>();
            if (blinkingComponent)
            {
                componentType = ComponentType.image;
            }
        }
    }
    
    void OnEnable()
    {
        Start();
    }

    void Update()
    {
        float time = (Time.time - blinkingStartTime) * speed;
        Color newColor = ColorSmoothstep(firstColor, secondColor, 1 - Mathf.Abs(1 - time % 2));
        if ((blinksCount != 0 && time < blinksCount || isBlinkingOverTime) && componentType != ComponentType.none)
        {
            blinkingComponent.color = newColor;
        }
        else
        {
            blinksCount = 0;
            onBlinkingFinished.Invoke();
        }
    }

    public void BlinkTimes(int blinksCount)
    {
        blinkingStartTime = Time.time;
        this.blinksCount = blinksCount;
        gameObject.GetComponent<Blink>().enabled = true;
    }

    Color ColorSmoothstep(Color firstColor, Color secondColor, float x)
    {
        return new Color(Mathf.SmoothStep(firstColor.r, secondColor.r, x),
            Mathf.SmoothStep(firstColor.g, secondColor.g, x),
            Mathf.SmoothStep(firstColor.b, secondColor.b, x),
            Mathf.SmoothStep(firstColor.a, secondColor.a, x));
    }

}
