using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class DialController : MonoBehaviour {

    public float speedMin = 0.5f;
    public float speedMax = 0.6f;
    public float accelerateTime = 2.0f;

    private Material dialMaterial;
    private float currentPosition = 0.0f;
    private float gotoPosition = 0.0f;
    private bool spin = false;

    private float currentSpeed = 0.0f;
    private float elapsed = 0.0f;
    
    void Start() {
        Image image = GetComponent<Image>();
        image.material = Instantiate<Material>(image.material);
        dialMaterial = image.material;
        Assert.IsNotNull(dialMaterial);
    }

    void Update() {
        
        if (spin) {
            if (Mathf.Approximately(currentPosition, gotoPosition)) {
                spin = false;
                elapsed = 0.0f;
            } else {
                elapsed += Time.deltaTime;
                currentSpeed = Mathf.Lerp(speedMin, speedMax, elapsed / accelerateTime);
                currentPosition = Mathf.MoveTowards(currentPosition, gotoPosition, currentSpeed * Time.deltaTime);
            }
            
            dialMaterial.SetFloat("_Turn", currentPosition % 1.0f);
        } else {
            if (currentPosition > 100.0) {
                currentPosition %= 1.0f;
                gotoPosition = currentPosition;
            }  
        }
    }

    public void SetTo(int digit) {
        int digitClamp = Math.Abs(digit) % 10;
        gotoPosition = (digitClamp) / 12.0f;
        if (gotoPosition < currentPosition) {
            gotoPosition += 1.0f;
        }
        spin = true;
    }

    public void SetToEmpty() {
        gotoPosition = 11.0f / 12.0f;
        if (gotoPosition < currentPosition) {
            gotoPosition += 1.0f;
        }
        spin = true;
    }
}
