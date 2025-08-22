using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UILineDrawer : MonoBehaviour
{
    public Image lineImage;      
    public float drawTime = 0.5f;
    

    public ParticleSystem confetti;

    public void PlayConfetti()
    {
        if (confetti != null)
        {
            confetti.Play();
            Debug.Log("confetti");
        }
    }

    public void StopConfetti()
    {
        if (confetti != null)
        {
            confetti.Stop();
            Debug.Log("stop confetti");
        }
    }

    public void DrawLine(RectTransform pointA, RectTransform pointB)
    {
        lineImage.gameObject.SetActive(true);
        StartCoroutine(DrawLineAnimated(pointA,pointB));
        
    }
    public void ClearLine()
    {
        if (lineImage != null)
        {
            lineImage.gameObject.SetActive(false);
        }
    }
    IEnumerator DrawLineAnimated(RectTransform pointA, RectTransform pointB)
    {
        Vector3 startPos = pointA.position;
        Vector3 endPos = pointB.position;

        // Çizginin yönünü ve pozisyonunu ayarla
        Vector3 dir = (endPos - startPos).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        lineImage.rectTransform.rotation = Quaternion.Euler(0, 0, angle);
        lineImage.rectTransform.position = startPos;

        float totalDistance = Vector3.Distance(startPos, endPos);
        float elapsed = 0f;

        while (elapsed < drawTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / drawTime);

            // O anki uzunluðu hesapla
            float currentLength = Mathf.Lerp(0, totalDistance, t);
            lineImage.rectTransform.sizeDelta = new Vector2(currentLength, lineImage.rectTransform.sizeDelta.y);

            // Baþlangýçtan ileriye doðru çiz
            lineImage.rectTransform.position = startPos + dir * (currentLength / 2f);

            yield return null;
        }

        // Son pozisyon ve boyut tamamlansýn
        lineImage.rectTransform.sizeDelta = new Vector2(totalDistance, lineImage.rectTransform.sizeDelta.y);
        lineImage.rectTransform.position = (startPos + endPos) / 2f;
    }
   
}
