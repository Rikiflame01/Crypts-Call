using UnityEngine;
using System.Collections;
public class EndOfGame : MonoBehaviour
{
    public Canvas canvas;


    public void EnableCanvas()
    {
        StartCoroutine(enableCanvasCoRoutine());
    }

    private IEnumerator enableCanvasCoRoutine()
    {
        canvas.enabled = true;
        yield return new WaitForSeconds(10);
        canvas.enabled = false;
    }
}
