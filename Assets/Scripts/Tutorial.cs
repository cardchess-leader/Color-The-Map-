using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Tutorial : MonoBehaviour
{
    public static Tutorial instance;
    public GameObject[] paintArr = new GameObject[4];
    public GameObject[] paintPointerArr = new GameObject[4];
    public GameObject[] regionPointerArr = new GameObject[4];
    public TMP_Text topMessage;
    Color selectedColor;
    Camera mainCamera;
    int nextTutorialStep = 0;
    void Awake()
    {
        instance = this;
    }
    void Start()
    {
        mainCamera = Camera.main;
    }
    void OnEnable()
    {
        StartCoroutine(ShowNextTutorialStep());
    }

    public void HandleMapTouch(float coordX, float coordY)
    {
        if (!this.enabled)
        {
            return;
        }
        Vector2 touchPos = mainCamera.ScreenToWorldPoint(new Vector2(coordX, coordY));
        RaycastHit2D hit = Physics2D.Raycast(touchPos, Vector2.zero);
        if (hit.collider != null)
        {
            SpriteRenderer renderer = hit.collider.GetComponent<SpriteRenderer>();
            renderer.color = selectedColor;
            StartCoroutine(ShowNextTutorialStep("MapTouch"));
        }
    }
    public void SelectColor(int index)
    {
        selectedColor = paintArr[index].GetComponent<Image>().color;
        foreach (GameObject brush in paintArr)
        {
            brush.transform.GetChild(0).GetComponent<Image>().enabled = false;
        }
        paintArr[index].transform.GetChild(0).GetComponent<Image>().enabled = true;
        StartCoroutine(ShowNextTutorialStep("PaintTouch"));
    }

    IEnumerator ShowNextTutorialStep(string inputSource = "")
    {
        switch (nextTutorialStep)
        {
            case 0: // current tutorial step
                // Paint 1
                DeactivateAll();
                ActivatePaintPointer(0);
                topMessage.text = "Pick a color!";
                nextTutorialStep = 1;
                break;
            case 1: // current tutorial step
                if (inputSource == "PaintTouch")
                {
                    // Region 1
                    DeactivateAll();
                    ActivateRegionPointer(0);
                    topMessage.text = "Color the first region!";
                    nextTutorialStep = 2;
                }
                break;
            case 2:
                if (inputSource == "MapTouch")
                {
                    // Paint 2
                    DeactivateAll();
                    ActivatePaintPointer(1);
                    topMessage.text = "Great!\nNow pick another color!";
                    nextTutorialStep = 3;
                }
                break;
            case 3:
                if (inputSource == "PaintTouch")
                {
                    // Region 2
                    DeactivateAll();
                    ActivateRegionPointer(1);
                    topMessage.text = "Color the second region!";
                    nextTutorialStep = 4;
                }
                break;
            case 4:
                if (inputSource == "MapTouch")
                {
                    // Paint 3
                    DeactivateAll();
                    ActivatePaintPointer(2);
                    topMessage.text = "Well done!\nNow color all regions!";
                    nextTutorialStep = 5;
                }
                break;
            case 5:
                if (inputSource == "PaintTouch")
                {
                    // Region 3
                    DeactivateAll();
                    ActivateRegionPointer(2);
                    nextTutorialStep = 6;
                }
                break;
            case 6:
                if (inputSource == "MapTouch")
                {
                    // Paint 4
                    DeactivateAll();
                    ActivatePaintPointer(3);
                    nextTutorialStep = 7;
                }
                break;
            case 7:
                if (inputSource == "PaintTouch")
                {
                    // Region 4
                    DeactivateAll();
                    ActivateRegionPointer(3);
                    nextTutorialStep = 8;
                }
                break;
            case 8:
                if (inputSource == "MapTouch")
                {
                    // Tutorial Over!
                    DeactivateAll();
                    topMessage.text = "Excellent!\nEnd of tutorial!";
                    nextTutorialStep = 9;
                    // Logic To End Tutorial //
                }
                break;

        }
        yield return null;
    }
    void DeactivateAll()
    {
        foreach (GameObject paintPointer in paintPointerArr)
        {
            paintPointer.transform.parent.GetComponent<Button>().enabled = false;
            paintPointer.SetActive(false);
        }
        foreach (GameObject regionPointer in regionPointerArr)
        {
            regionPointer.transform.parent.GetComponent<PolygonCollider2D>().enabled = false;
            regionPointer.SetActive(false);
        }
    }

    void ActivatePaintPointer(int index)
    {
        paintPointerArr[index].transform.parent.GetComponent<Button>().enabled = true;
        paintPointerArr[index].SetActive(true);
    }

    void ActivateRegionPointer(int index)
    {
        regionPointerArr[index].transform.parent.GetComponent<PolygonCollider2D>().enabled = true;
        regionPointerArr[index].SetActive(true);
    }
}
