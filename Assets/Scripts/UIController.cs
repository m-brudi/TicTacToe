using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] Button playBtn;
    [SerializeField] Button smartSwitchBtn;
    [SerializeField] TMP_Text turnTxt;
    [SerializeField] TMP_Text headerTxt;
    [SerializeField] TMP_Text smartSwitchTxt;

    [SerializeField] Controller controller;

    public void SetupPanel() {
        panel.SetActive(true);
        playBtn.onClick.RemoveAllListeners();
        smartSwitchBtn.onClick.RemoveAllListeners();
        playBtn.onClick.AddListener(() => {
            panel.SetActive(false);
            controller.SetupGame();
        });
        smartSwitchBtn.onClick.AddListener(() => {
            controller.SmarterComputer = !controller.SmarterComputer;
            SetupSmartSwitchTxt();
        });
        turnTxt.text = "";
        SetupSmartSwitchTxt();
    }

    public void SetHeaderText(string txt) {
        headerTxt.text = txt;
    }

    public void SetTurnText(string txt) {
        turnTxt.text = txt;
    }

    void SetupSmartSwitchTxt() {
        if (controller.SmarterComputer) smartSwitchTxt.text = "ON";
        else smartSwitchTxt.text = "OFF";
    }
}
