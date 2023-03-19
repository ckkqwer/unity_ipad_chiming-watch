using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class AudioController : MonoBehaviour
{
    [Header("Build Debug")]
    public TMP_Text capturedTime;
    public TMP_Text loopTimeText;
    public TMP_InputField hourIntervalInput;
    public TMP_InputField quarterIntervalInput;
    public TMP_InputField remainMinuteIntervalInput;
    public TMP_Text hourIntervalText;
    public TMP_Text quarterIntervalText;
    public TMP_Text remainMinuteIntervalText;
    public Button confirmInterval;
    public Button resetInterval;

    [Header("Current")]
    public int capturedHour;
    public int capturedMinute;

    [Header("Status")]
    public bool isPlayingSound;

    [Header("Options")]
    public float hourInterval = 0.4f;
    public float quarterInterval = 0.9f;
    public float remainMinuteInterval = 0.4f;

    [Header("Default")]
    public float defaultHrInterval = 0.4f;
    public float defaultQuarterInterval = 0.9f;
    public float defaultRemainMinuteInterval = 0.4f;

    [Header("References")]
    public AudioSource hourSource;
    public AudioSource quarterSource;
    public AudioSource remainMinuteSource;
    public GameController gameController;

    int hourCount = 0;
    int quarterCount = 0;
    int remainMinuteCount = 0;

    bool overriding;

    private void Start()
    {
        confirmInterval.onClick.AddListener(SetSoundInterval);
        resetInterval.onClick.AddListener(ResetSoundInterval);

        UpdateSoundInterval();
        if(hourInterval == 0 || quarterInterval == 0|| remainMinuteInterval == 0)
        {
            ResetSoundInterval();
        }
    }

    #region Build Debug
    //true = confirm, false = reset
    private void SetSoundInterval()
    {
        if (overriding)
        {
            return;
        }
        float hr, quarter, reMin;
        try
        {
            hr = float.Parse(hourIntervalInput.text);
        }
        catch(FormatException fe)
        {
            print(fe);
            hr = defaultHrInterval;
        }

        try
        {
            quarter = float.Parse(quarterIntervalInput.text);
        }
        catch (FormatException fe)
        {
            print(fe);
            quarter = defaultQuarterInterval;
        }

        try
        {
            reMin = float.Parse(remainMinuteIntervalInput.text);
        }
        catch (FormatException fe)
        {
            print(fe);
            reMin = defaultRemainMinuteInterval;
        }

        overriding = true;
        
        if(hourIntervalInput.text != "")
        {
            PlayerPrefs.SetFloat("HourInterval", hr);
        }
        if(quarterIntervalInput.text != "")
        {
            PlayerPrefs.SetFloat("QuarterInterval", quarter);
        }
        if(remainMinuteIntervalInput.text != "")
        {
            PlayerPrefs.SetFloat("RemainMinuteInterval", reMin);
        }
        UpdateSoundInterval();
        overriding = false;
    }

    private void ResetSoundInterval()
    {
        PlayerPrefs.SetFloat("HourInterval", defaultHrInterval);
        PlayerPrefs.SetFloat("QuarterInterval", defaultQuarterInterval);
        PlayerPrefs.SetFloat("RemainMinuteInterval", defaultRemainMinuteInterval);
        UpdateSoundInterval();
        ClearAudioInputField();
    }

    private void UpdateSoundInterval()
    {
        hourInterval = PlayerPrefs.GetFloat("HourInterval");
        quarterInterval = PlayerPrefs.GetFloat("QuarterInterval");
        remainMinuteInterval = PlayerPrefs.GetFloat("RemainMinuteInterval");

        hourIntervalText.text = hourInterval.ToString();
        quarterIntervalText.text = quarterInterval.ToString();
        remainMinuteIntervalText.text = remainMinuteInterval.ToString();

        if (gameController.clearInputOnConfirm == true)
            ClearAudioInputField();
    }

    private void ClearAudioInputField()
    {
        hourIntervalInput.text = "";
        quarterIntervalInput.text = "";
        remainMinuteIntervalInput.text = "";
    }
    #endregion

    public void PlaySound()
    {
        isPlayingSound = true;

        capturedHour = gameController.currentHour;
        capturedMinute = gameController.currentMinute;

        capturedTime.text = "Last Captured Time: \n" + capturedHour + "h : " + capturedMinute + "m";
        loopTimeText.text = $"Hour x{capturedHour}, Quarter x{GetQuarter()}, RemainMinute x{GetRemainMinute()}";

        InvokeRepeating("Hour", 0, hourInterval);
    }

    private void Hour()
    {
        if(hourCount < capturedHour)
        {
            hourSource.Play();
            hourCount++;
            print($"Play Hour {hourCount}");
        }
        else
        {
            CancelInvoke();
            hourCount = 0;
            InvokeRepeating("Quarter", 0, quarterInterval);
        }
    }

    private void Quarter()
    {
        if (quarterCount < GetQuarter() )
        {
            quarterSource.Play();
            quarterCount++;
            print($"Play Quarter {quarterCount}");
        }
        else
        {
            CancelInvoke();
            quarterCount = 0;
            InvokeRepeating("RemainMinute", 0, remainMinuteInterval);
        }
    }

    private void RemainMinute()
    {
        if (remainMinuteCount < GetRemainMinute() )
        {
            remainMinuteSource.Play();
            remainMinuteCount++;
            print($"Play RemainMinute {remainMinuteCount}");
        }
        else
        {
            remainMinuteCount = 0;
            CancelInvoke();
            gameController.pressButton.image.color = gameController.idleColor;
            gameController.buttonText.color = gameController.textIdleColor;
            isPlayingSound = false;
        }
    }

    private int GetQuarter()
    {
        if(capturedMinute >= 0 && capturedMinute < 15)
        {
            return 0;
        }
        else if(capturedMinute >=15 && capturedMinute < 30)
        {
            return 1;
        }
        else if(capturedMinute >=30 && capturedMinute < 45)
        {
            return 2;
        }
        else
        {
            return 3;
        }
    }

    private int GetRemainMinute()
    {
        int subtraction = GetQuarter() * 15;
        return capturedMinute - subtraction;
    }
}
