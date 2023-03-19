using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [Header("Build Debug")]
    public Button debugTrigger_toOpen;
    public Button debugTrigger_toClose;
    public GameObject debugPanel;
    public GameObject soundIntervalPanel;
    public TMP_Text currentTime;
    public TMP_Text currentPointerMode;
    public TMP_InputField hrInput;
    public TMP_InputField minInput;
    public TMP_InputField secInput;
    public Button confirmInput;
    public Button resetInput;
    public Button toDiscrete;
    public Button toContinuous;
    public Button showChangeSoundInterval;

    [Header("Setting")]
    public bool isDiscrete;
    public bool useSecondCorrection;
    public bool clearInputOnConfirm;

    [Header("Current")]
    public int currentHour;
    public int currentMinute;
    public int currentSecond;
    public bool usingCustomTime;

    [Header("Data")]
    public Color idleColor;
    public Color pressedColor;
    public Color textIdleColor;
    public Color textPressedColor;

    [Header("Pointers")]
    public RectTransform secondPointer;
    public RectTransform minutePointer;
    public RectTransform hourPointer;

    public Button pressButton;
    public TMP_Text buttonText;

    public AudioController audioController;
    //correction values on 45
    const float secondCorr = 2f;
    bool overriding;
    int previousSecond;
    DateTime custom;

    private void Start()
    {
        debugPanel.SetActive(false);
        soundIntervalPanel.SetActive(false);

        pressButton.image.color = idleColor;
        buttonText.color = textIdleColor;

        pressButton.onClick.AddListener(WatchButton);
        //debugTrigger.onClick.AddListener(ShowDebugPanel);
        confirmInput.onClick.AddListener(OverrideTime);
        resetInput.onClick.AddListener(ResetOverride);
        toContinuous.onClick.AddListener(delegate { SwitchMode(true); });
        toDiscrete.onClick.AddListener(delegate { SwitchMode(false); });
        showChangeSoundInterval.onClick.AddListener(ShowChangeSoundInterval);

        if (isDiscrete)
        {
            UpdateDiscrete(currentHour, currentMinute, currentSecond);
            currentPointerMode.text = "D";
        }
        else
        {
            UpdateContinuous(currentHour, currentMinute, currentSecond);
            currentPointerMode.text = "C";
        }

        previousSecond = currentSecond;
    }

    private void Update()
    {
        if (overriding)
            return;

        int nowSecond = DateTime.Now.Second;

        if(nowSecond != previousSecond)
        {
            if (usingCustomTime)
            {
                custom = custom.AddSeconds(1);
                UpdateTime(false);
            }
            else
                UpdateTime(true);
        }
        previousSecond = nowSecond;
    }

    #region System Time
    private void UpdateTime(bool isUsingSystemTime)
    {
        if (isUsingSystemTime)
        {
            DateTime now = DateTime.Now;

            currentSecond = int.Parse(now.ToString("ss"));
            currentMinute = int.Parse(now.ToString("mm"));
            currentHour = int.Parse(now.ToString("hh"));
        }
        else
        {
            currentSecond = int.Parse(custom.ToString("ss"));
            currentMinute = int.Parse(custom.ToString("mm"));
            currentHour = int.Parse(custom.ToString("hh"));
        }

        /*
        currentSecond = Mathf.Abs(int.Parse(now.ToString("ss")) + overrideSecond);
        currentMinute = Mathf.Abs(int.Parse(now.ToString("mm")) + overrideMinute);
        currentHour = Mathf.Abs(int.Parse(now.ToString("hh")) + overrideHour);
        */
        /*
        currentSecond = int.Parse(now.ToString("ss")) + overrideSecond;
        currentMinute = int.Parse(now.ToString("mm")) + overrideMinute;
        currentHour = int.Parse(now.ToString("hh")) + overrideHour;
        */
        if (isDiscrete)
            UpdateDiscrete(currentHour, currentMinute, currentSecond);
        else
            UpdateContinuous(currentHour, currentMinute, currentSecond);

        if (useSecondCorrection)
            SecondCorrection();
        else
            secondPointer.localRotation = Quaternion.Euler(0f, 0f, currentSecond * -6f);

        string type;
        if (isUsingSystemTime)
            type = "System: ";
        else
            type = "Custom: ";
        currentTime.text = type + currentHour + " : " + currentMinute + " : " + currentSecond/* + " " + now.ToString("tt")*/;
        print(currentTime.text);
    }

    /*void UpdateContinuous()
    {
        //set clock rotations
        TimeSpan time = DateTime.Now.TimeOfDay;
        hourPointer.localRotation = Quaternion.Euler(0f, 0f, (float)time.TotalHours * 30f);
        minutePointer.localRotation = Quaternion.Euler(0f, 0f, (float)time.TotalMinutes * 6f);
        secondPointer.localRotation = Quaternion.Euler(0f, 0f, (float)time.TotalSeconds * 6f);
    }*/
    /*private void UpdateContinuous()
    {
        //set clock rotations
        TimeSpan time = DateTime.Now.TimeOfDay;

        continuousHour = (float)time.TotalHours;
        continuousMinute = (float)time.TotalMinutes;
        continuousSecond = (float)time.TotalSeconds;

        hourPointer.localRotation = Quaternion.Euler(0f, 0f, (float)(continuousHour * -30f));
        minutePointer.localRotation = Quaternion.Euler(0f, 0f, (float)(continuousMinute * -6f));
        secondPointer.localRotation = Quaternion.Euler(0f, 0f, (float)(continuousSecond * -6f));

        currentTime.text = currentHour + " : " + currentMinute + " : " + currentSecond + " " + DateTime.Now.ToString("tt");
        print(currentTime.text);
    }*/

    private void UpdateContinuous(int hour, int minute, int second)
    {
        hourPointer.localRotation = Quaternion.Euler(0f, 0f, (float)(hour * -30f + minute * (float)(-30f/60f) + second * (float)(-30f/3600f)) );
        minutePointer.localRotation = Quaternion.Euler(0f, 0f, (float)(minute * -6f + second * (float)(-6f/60f)) );
    }

    private void UpdateDiscrete(int hour, int minute, int second)
    {
        hourPointer.localRotation = Quaternion.Euler(0f, 0f, hour * -30f);
        minutePointer.localRotation = Quaternion.Euler(0f, 0f, minute * -6f);
    }
    #endregion

    private void WatchButton()
    {
        if (!audioController.isPlayingSound)
        {
            audioController.PlaySound();
            pressButton.image.color = pressedColor;
            buttonText.color = textPressedColor;
        }
        else
            print("Sound is already playing!");
    }

    private void SwitchMode(bool isToContinuous)
    {
        if (isToContinuous)
        {
            isDiscrete = false;
            currentPointerMode.text = "C";
        }
        else
        {
            isDiscrete = true;
            currentPointerMode.text = "D";
        }
    }

    private void OverrideTime()
    {
        if (!CheckInputValidity() || overriding)
        {
            return;
        }
        overriding = true;

        int hr, min, sec;
        if (hrInput.text == "")
            hr = 0;
        else
            hr = int.Parse(hrInput.text);

        if (minInput.text == "")
            min = 0;
        else
            min = int.Parse(minInput.text);

        if (secInput.text == "")
            sec = 0;
        else
            sec = int.Parse(secInput.text);

        DateTime now = DateTime.Now;
        custom = new DateTime(now.Year, now.Month, now.Day, hr, min, sec);
        UpdateTime(false);

        /*
        overrideHour = 0;
        overrideMinute = 0;
        overrideSecond = 0;
        UpdateSystemTime();

        overrideHour = int.Parse(hrInput.text) - currentHour;
        overrideMinute = int.Parse(minInput.text) - currentMinute;
        overrideSecond = int.Parse(secInput.text) - currentSecond;
        UpdateSystemTime();*/

        usingCustomTime = true;
        if(clearInputOnConfirm)
            CleatInputField();
        overriding = false;
    }

    private bool CheckInputValidity()
    {
        if (hrInput.text == "" || minInput.text == "" || secInput.text == "")
            return false;

        try
        {
            if (int.Parse(hrInput.text) < 1 || int.Parse(hrInput.text) > 12)
                return false;

            if (int.Parse(minInput.text) < 0 || int.Parse(minInput.text) > 59)
                return false;

            if (int.Parse(secInput.text) < 0 || int.Parse(secInput.text) > 59)
                return false;
        }
        catch (FormatException fe)
        {
            print(fe);
            return false;
        }

        return true;
    }

    private void ResetOverride()
    {
        if (overriding)
            return;
        overriding = true;

        usingCustomTime = false;
        CleatInputField();
        overriding = false;
    }

    public void OpenDebugPanel()
    {
        debugPanel.SetActive(true);
        debugTrigger_toOpen.gameObject.SetActive(false);
    }

    public void CloseDebugPanel()
    {
        debugPanel.SetActive(false);
        debugTrigger_toOpen.gameObject.SetActive(true);
    }

    private void ShowChangeSoundInterval()
    {
        if (!soundIntervalPanel.activeSelf)
            soundIntervalPanel.SetActive(true);
        else
            soundIntervalPanel.SetActive(false);
    }

    private void CleatInputField()
    {
        hrInput.text = "";
        minInput.text = "";
        secInput.text = "";
    }

    private void SecondCorrection()
    {
        //no change
        if (currentSecond >= 0 && currentSecond <= 20)
            secondPointer.localRotation = Quaternion.Euler(0f, 0f, currentSecond * -6f);
        //incremental
        else if (currentSecond > 20 && currentSecond <= 45)
            secondPointer.rotation = Quaternion.Euler(0, 0, (float)((float)(currentSecond * -6f) + (float)Mathf.Lerp(0f, secondCorr, (float)(currentSecond - 20f) / 25f) ));
        //decremental
        else if (currentSecond > 45 && currentSecond <= 59)
            secondPointer.rotation = Quaternion.Euler(0, 0, (float)((float)(currentSecond * -6f) + (float)Mathf.Lerp(secondCorr, 0f, (float)(currentSecond - 45f) / 14f) ));
    }
    /*
    private void UpdateTimer()
    {
        DateTime updateTime = DateTime.Now;

        currentSecond = int.Parse(updateTime.ToString("ss")) + overrideSecond;
        currentMinute = int.Parse(updateTime.ToString("mm")) + overrideMinute;
        currentHour = int.Parse(updateTime.ToString("hh")) + overrideHour;

        
        //reset to 0
        if (currentSecond == 0)
            secondPointer.rotation = Quaternion.identity;
        //no change
        else if (currentSecond > 0 && currentSecond <= 20)
            secondPointer.Rotate(new Vector3(0, 0, -6f), Space.Self);
        //incremental
        else if (currentSecond > 20 && currentSecond <= 45)
            secondPointer.rotation = Quaternion.Euler(0, 0, (float)((float)(currentSecond * -6f) + SecondCorrection(currentSecond, true)));
        //decremental
        else if (currentSecond > 45 && currentSecond <= 59)
            secondPointer.rotation = Quaternion.Euler(0, 0, (float)((float)(currentSecond * -6f) + SecondCorrection(currentSecond, false)));

        minutePointer.Rotate(new Vector3(0, 0, (float)(-6f / 60f)), Space.Self);
        hourPointer.Rotate(new Vector3(0, 0, (float)(-6f / 3600f)), Space.Self);

        //Correction on every hours (hr/min pointers)
        if (currentMinute == 0 && currentSecond == 0)
        {
            minutePointer.rotation = Quaternion.identity;
            hourPointer.rotation = Quaternion.Euler(0, 0, (float)(currentHour * -30f));
            if (currentHour == 12)
                hourPointer.rotation = Quaternion.identity;
        }
    }
    */
}
