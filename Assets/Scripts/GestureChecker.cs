
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GestureChecker : MonoBehaviour
{

    public float threshold = 1;
    public GameObject fullscreenShaderQuad;
    public GameObject timerTextGO;
    public int timeAtStart = 10;
    public float timeDecreaseMultiplier = 0.9f;
    public UnityEvent onLost;
    public GameObject bestResultTextGO;

    bool isSavingPoints = false;
    List<Vector2> savedPoints = new List<Vector2>();
    Rect? bounds = null;
    GameObject currentGesture;
    float timeForCurrentGesture;
    float currentGestureStartTime;
    float startTime;

    void Start()
    {
        timeForCurrentGesture = timeAtStart / timeDecreaseMultiplier;
        fullscreenShaderQuad.GetComponent<Gesture>().SetPoints(VectorHelper.GetVec4ListFilled(GetMaxGestureLength()));
        SetRandomGesture();
        startTime = Time.time;
    }

    void OnEnable()
    {
        Start();
    }

    void Update()
    {
        if(Time.time - currentGestureStartTime > timeForCurrentGesture)
        {
            //Debug.Log("LOST");
            string[] arr = bestResultTextGO.GetComponent<Text>().text.Split(':');
            bestResultTextGO.GetComponent<Text>().text = arr[0] +
                string.Format(": {0:0}", Mathf.Max(Time.time - startTime, System.Convert.ToInt32(arr[1])));
            onLost.Invoke();
            return;
        }
        timerTextGO.GetComponent<Text>().text = timerTextGO.GetComponent<Text>().text.Split(':')[0] +
            string.Format(": {0:0}", timeForCurrentGesture - (Time.time - currentGestureStartTime));
        if (!isSavingPoints)
        {
            if (Input.GetMouseButton(0))
            {
                savedPoints.Clear();
                bounds = null;
            }
        }
        else
        {
            if (!Input.GetMouseButton(0))
            {
                GestureTask gestureTask = currentGesture.GetComponent<GestureTask>();
                List<bool> touchedPoints = new List<bool>();
                for (int i = 0; i < gestureTask.gestureVertices.Count; i++)
                {
                    touchedPoints.Add(false);
                }
                for (int i = 0; i < savedPoints.Count; i++)
                {
                    savedPoints[i] = new Vector2((savedPoints[i].x - bounds.Value.xMin) * gestureTask.bounds.width / bounds.Value.width + gestureTask.bounds.xMin,
                        (savedPoints[i].y - bounds.Value.yMin) * gestureTask.bounds.height / bounds.Value.height + gestureTask.bounds.yMin);
                    for (int j = 0; j < gestureTask.gestureVertices.Count; j++)
                    {
                        if ((savedPoints[i] - gestureTask.gestureVertices[j]).magnitude < threshold)
                        {
                            touchedPoints[j] = true;
                        }
                    }
                }
                bool isAllPointsTouched = true;
                for (int i = 0; i < touchedPoints.Count; i++)
                {
                    if (!touchedPoints[i])
                    {
                        isAllPointsTouched = false;
                        break;
                    }
                }
                if (isAllPointsTouched)
                {
                    //Debug.Log("TIME TO CHECK LINES");
                    bool isCloseToLine = false;
                    bool isAllPointsCloseToLine = true;
                    for (int i = 0; i < savedPoints.Count; i++)
                    {
                        isCloseToLine = false;
                        for (int j = 1; j < gestureTask.gestureVertices.Count; j++)
                        {
                            Vector2 line = gestureTask.gestureVertices[j] - gestureTask.gestureVertices[j - 1];
                            if ((Vector2.Dot(savedPoints[i] - gestureTask.gestureVertices[j - 1], line) * line / Mathf.Pow(line.magnitude, 2) +
                                gestureTask.gestureVertices[j - 1] - savedPoints[i]).magnitude < threshold)
                            {
                                isCloseToLine = true;
                                break;
                            }
                        }
                        if (!isCloseToLine)
                        {
                            isAllPointsCloseToLine = false;
                            break;
                        }
                    }
                    if (isAllPointsCloseToLine)
                    {
                        //Debug.Log("VICTORY");
                        SetNextGesture();
                    }
                }
            }
            else
            {
                Vector2 point = Camera.main.ScreenToViewportPoint(Input.mousePosition);
                savedPoints.Add(point);
                bounds = DrawGL.UpdateBounds(bounds, point);
            }
        }
        isSavingPoints = Input.GetMouseButton(0);
    }

    void SetNextGesture()
    {
        currentGesture.SetActive(false);
        SetRandomGesture();
    }

    void SetRandomGesture()
    {
        currentGesture = transform.GetChild((int)(Random.value * transform.childCount)).gameObject;
        currentGesture.SetActive(true);
        fullscreenShaderQuad.GetComponent<Gesture>().SetPoints(VectorHelper.GetVec4ListFilled(GetMaxGestureLength()));
        fullscreenShaderQuad.GetComponent<Gesture>().SetPoints(VectorHelper.Vec4FromVec2(currentGesture.GetComponent<GestureTask>().gestureVertices));
        currentGestureStartTime = Time.time;
        timeForCurrentGesture *= timeDecreaseMultiplier;
    }

    int GetMaxGestureLength()
    {
        int maxCount = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            maxCount = Mathf.Max(transform.GetChild(i).gameObject.GetComponent<GestureTask>().gestureVertices.Count, maxCount);
        }
        return maxCount;
    }

}
