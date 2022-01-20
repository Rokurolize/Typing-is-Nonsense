using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro; // �X�N���v�g���� TextMeshPro �̕ύX

public class TweeterManager : MonoBehaviour
{
    public GameObject gameMainManager;

    // ��ԕϐ�
    bool isTweeting = true;

    // Tweeter �\���I�u�W�F�N�g
    public GameObject tweeter;
    public GameObject tweeterButtonTMP;
    public GameObject tweeterInputField;
    public GameObject copyButtonTMP;

    // Start is called before the first frame update
    private void Awake()
    {
        // EventBus �ɃL�[�����C�x���g�������̃��\�b�h���s���˗� *Unsubscribe �Y�ꂸ�I
        EventBus.Instance.SubscribeNormalKeyDown(OnNormalKeyDown);
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnDestroy()
    {
        EventBus.Instance.UnsubscribeNormalKeyDown(OnNormalKeyDown);
    }

    // ���[�e�B���e�B���\�b�h
    /// <summary>
    /// long �ŕۑ����Ă���~���b���A�\���p�� X.XXX s �ɕς���
    /// </summary>
    /// <param name="ms"></param>
    /// <returns></returns>
    string ToFormattedTime(long ms)
    {
        return (ms / 1000).ToString() + "." + (ms % 1000).ToString("000");
    }
    public void ToggleVisible()
    {
        tweeterInputField.GetComponent<TMP_InputField>().text = "";
        copyButtonTMP.GetComponent<TextMeshProUGUI>().text = "Copy to\nClipboard [C]";
        if (isTweeting)
        {
            foreach (Transform item in this.gameObject.transform)
            {
                item.gameObject.SetActive(false);
            }
            isTweeting = false;
            tweeterButtonTMP.GetComponent<TextMeshProUGUI>().text = "Tweet / \nRegistration\n[Space]";
        }
        else
        {
            foreach (Transform item in this.gameObject.transform)
            {
                item.gameObject.SetActive(true);
            }
            isTweeting = true;
            tweeterButtonTMP.GetComponent<TextMeshProUGUI>().text = "Toggle\nT / R\n[Space]";
        }

    }

    public void SetVisible(bool visible)
    {
        if (visible && isTweeting == true || !visible && isTweeting == false) return;
        else
        {
            ToggleVisible();
        }
    }

    void GenerateTweet()
    {
        copyButtonTMP.GetComponent<TextMeshProUGUI>().text = "Copy to\nClipboard [C]";
        System.Tuple<string, TrialData> tup = gameMainManager.GetComponent<GameMainManager>().GetTrialData();
        string gstate = tup.Item1;
        TrialData nowTrialData = tup.Item2;
        if (nowTrialData == null)
        {
            tweeterInputField.GetComponent<TMP_InputField>().text = "There's no record to tweet about.";
        }
        else
        {
            long time = nowTrialData.TotalTime;
            int keys = nowTrialData.TypedKeys;
            double cps = (double)(keys * 1000) / time;
            int miss = nowTrialData.TotalMiss;
            if (gstate == "Completed")
            {
                tweeterInputField.GetComponent<TMP_InputField>().text = $"Completed a trial ({keys} chars) on #TypingIsNonsense !" +
                    $"\nTIME {ToFormattedTime(time)}s ({cps:f3}cps miss{miss}" +
                    $"{(miss > 50 ? " > 50 = Unofficial)." : "). Wow!")}" +
                    $"\nRanking: https://terum.jp/tin/";
            }
            else if (gstate == "Canceled")
            {
                tweeterInputField.GetComponent<TMP_InputField>().text = $"GAVE UP a trial ({keys}/360 chars) on #TypingIsNonsense ..." +
                    $"\nTIME {ToFormattedTime(time)}s ({cps:f3}cps miss{miss})." +
                    $"\nRanking: https://terum.jp/tin/";
            }
            else if (gstate == "Failed")
            {
                tweeterInputField.GetComponent<TMP_InputField>().text = $"FAILED a miss <= {nowTrialData.MissLimit} challenge ({keys}/360 chars) on #TypingIsNonsense . OMG!" +
                    $"\nTIME {ToFormattedTime(time)}s ({cps:f3}cps miss{miss})." +
                    $"\nRanking: https://terum.jp/tin/";
            }
        }
    }
    void CopyToClickboard()
    {
        GUIUtility.systemCopyBuffer = tweeterInputField.GetComponent<TMP_InputField>().text;
        copyButtonTMP.GetComponent<TextMeshProUGUI>().text = "Copied!";
    }
    void JumpToWebsite()
    {
        // �Q�[�����ł��������Ă��܂�����
        if (isTweeting) Application.OpenURL("https://terum.jp/tin/");
    }

    // �C�x���g�n���h��
    public void OnGenerateTweetButtonClick()
    {
        GenerateTweet();
    }
    public void OnCopyToClickboardButtonClick()
    {
        CopyToClickboard();
    }
    public void OnJumpToWebsiteButtonClick() {
        JumpToWebsite();
    }
    [System.Obsolete("�V���[�g�J�b�g�o�C���h�@�\������A�����̔��ʕ��@��ύX�v")]
    public void OnNormalKeyDown(ushort charID)
    {
        if (charID == 7) OnGenerateTweetButtonClick();
        if (charID == 3) OnCopyToClickboardButtonClick();
        if (charID == 10) OnJumpToWebsiteButtonClick();
    }

}
