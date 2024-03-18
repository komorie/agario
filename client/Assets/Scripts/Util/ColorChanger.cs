using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ColorChanger 
{
    public static void ChangeMaterialColor(GameObject gameObject, Color color) //컬러 변경
    {
        Renderer renderer = gameObject.GetComponent<Renderer>();
        MaterialPropertyBlock propBlock = new MaterialPropertyBlock();

        renderer.GetPropertyBlock(propBlock);

        propBlock.SetColor("_Color", color);

        renderer.SetPropertyBlock(propBlock);
    }

    public static IEnumerator LerpMaterialColor(GameObject gameObject, float time, Color start, Color end) //컬러 서서히 변경
    {
        float elapsed = 0; 
        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / time;
            ChangeMaterialColor(gameObject, Color.Lerp(start, end, normalizedTime));
            yield return null;
        }
    }

}
