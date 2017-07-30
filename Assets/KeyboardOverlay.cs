using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyboardOverlay : MonoBehaviour
{
    private Text wText;
    private Text aText;
    private Text sText;
    private Text dText;
    private Text spaceText;
    private Text shiftText;

    void Start()
    {
        this.wText = GameObject.Find("wText").GetComponent<Text>();
        this.aText = GameObject.Find("aText").GetComponent<Text>();
        this.sText = GameObject.Find("sText").GetComponent<Text>();
        this.dText = GameObject.Find("dText").GetComponent<Text>();
        this.spaceText = GameObject.Find("spaceText").GetComponent<Text>();
        this.shiftText = GameObject.Find("shiftText").GetComponent<Text>();
    }

    void Update()
    {
        this.wText.gameObject.SetActive(Input.GetAxisRaw("Vertical") > 0.0f);
        this.aText.gameObject.SetActive(Input.GetAxisRaw("Horizontal") < 0.0f);
        this.sText.gameObject.SetActive(Input.GetAxisRaw("Vertical") < 0.0f);
        this.dText.gameObject.SetActive(Input.GetAxisRaw("Horizontal") > 0.0f);
        this.spaceText.gameObject.SetActive(Input.GetButton("Jump"));
        this.shiftText.gameObject.SetActive(Input.GetKey(KeyCode.LeftShift));
    }
}
