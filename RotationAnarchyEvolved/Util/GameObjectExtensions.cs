using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameObjectExtensions
{

    public static void SetLayerRecursively(this GameObject self, int setLayer)
    {
        self.layer = setLayer;

        for (int i = 0; i < self.transform.childCount; i++)
        {
            self.transform.GetChild(i).gameObject.SetLayerRecursively(setLayer);
        }
    }

    // Thank you! https://forum.unity.com/threads/fit-object-exactly-into-perspective-cameras-field-of-view-focus-the-object.496472/
    public static Bounds GetBoundsWithChildren(this GameObject gameObject)
    {

        Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
        Bounds bounds = renderers.Length > 0 ? renderers[0].bounds : new Bounds();

        for (int i = 1; i < renderers.Length; i++)
        {
            if (renderers[i].enabled)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }
        }

        return bounds;

    }
}
