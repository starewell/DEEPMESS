using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SpriteRenderer))]
public class ParallaxImageScroll : MonoBehaviour
{

    SpriteRenderer _image;
    [SerializeField] Vector2 scrollSpeed;
    void Start() {
        _image = GetComponent<SpriteRenderer>();
        //_image.material = new Material(_image.material);
    }

    public void ScrollParallax(int pos)
    {
        _image.material.mainTextureOffset += scrollSpeed * pos;
    }
}
