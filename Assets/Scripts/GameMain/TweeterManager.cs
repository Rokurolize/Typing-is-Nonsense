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
            long time = nowTrialData.totalTime;
            int keys = nowTrialData.typedKeys;
            double cps = (double)(keys * 1000) / time;
            int miss = nowTrialData.totalMiss;
            if (gstate == "Completed")
            {
                tweeterInputField.GetComponent<TMP_InputField>().text = $"Completed a trial ({keys} chars) on #TypingIsNonsense !" +
                    $"\nTIME {time / 1000:f3}s ({cps:f3}cps miss{miss}" +
                    $"{(miss > 50 ? " > 50 = Unofficial)." : "). Wow!")}" +
                    $"\nRanking: https://terum.jp/tin/";
            }
            else if (gstate == "Canceled")
            {
                tweeterInputField.GetComponent<TMP_InputField>().text = $"GAVE UP a trial ({keys}/360 chars) on #TypingIsNonsense ..." +
                    $"\nTIME {time / 1000:f3}s ({cps:f3}cps miss{miss})." +
                    $"\nRanking: https://terum.jp/tin/";
            }
            else if (gstate == "Failed")
            {
                tweeterInputField.GetComponent<TMP_InputField>().text = $"FAILED a miss <= {nowTrialData.missLimit} challenge ({keys}/360 chars) on #TypingIsNonsense . OMG!" +
                    $"\nTIME {time / 1000:f3}s ({cps:f3}cps miss{miss})." +
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
    [System.Obsolete("�o�C���h�@�\������A�����̔��ʕ��@��ύX�v")]
    public void OnNormalKeyDown(ushort charID)
    {
        char c = MyInputManager.ToChar_FromCharID(charID);
        if (c == 'g' || c == 'G') OnGenerateTweetButtonClick();
        if (c == 'c' || c == 'C') OnCopyToClickboardButtonClick();
        if (c == 'j' || c == 'J') OnJumpToWebsiteButtonClick();
    }

}
