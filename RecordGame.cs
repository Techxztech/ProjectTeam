using System;
using System.Collections;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.IO.Compression;


//д��xzx���ļ��ṹ
public struct xzxElement
{
    public int frameCount;//��ǰ֡��
    public Vector3 cameraPosition;//�������λ����Ϣ
    public Quaternion cameraRotation;//���������ת��Ϣ
    public string mqttText;//mqttЭ�����Ϣ
    public string voiceText;//������Ϣ
    public List<float> rightHandTransform;//���ֹؽ�λ����Ϣ
    public List<float> leftHandTransform;//���ֹؽ�λ����Ϣ

    public xzxElement(int frameCount, Vector3 cameraPosition, Quaternion cameraRotation, List<float> rightHandTransform, List<float> leftHandTransform, string mqttText = null, string voiceText=null)
    {
        this.frameCount = frameCount;
        this.cameraPosition = cameraPosition;
        this.cameraRotation = cameraRotation;
        this.rightHandTransform = rightHandTransform;
        this.leftHandTransform = leftHandTransform;
        this.mqttText = mqttText;
        this.voiceText = voiceText;
    }
}
public class RecordGame : MonoBehaviour
{
    [Header("¼�����")]
    public static RecordGame instance;
    [SerializeField]
    private bool isStop= false;//�Ƿ�ֹͣ¼��
    private float totalFileTime=0;//¼�Ƶ�ʱ��

    [Header("xzx�ļ����")]
    xzxElement xzxContent = new xzxElement();//��Ҫд��xzx�ļ���xzxElement����һ��ʼ�ͳ�ʼ��������Ƶ���ĳ�ʼ��
    string xzxSavedPath = "";//��·�����ļ��У������������ɵ�����finalXzxPath
    string tmpxzxSavePath="";//��ʱ�����λ��
    string finalXzxPath = "";//���ն���ѹ���ļ������λ��
    xzx currentXzxFile;
    string modeString = "10";//���ض�1Ϊ��ϰģʽ��2Ϊ����ģʽ����������10Ϊ��ϰģʽ��20Ϊ����ģʽ
    string userName = "xiaofu";//�û�����һ��ӷ������˻�ȡ
    string dic = "";//��¼�۷���������ֵ�

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        xzxSavedPath = Path.Combine(Application.persistentDataPath, "xzxFiles");
        if (!Directory.Exists(xzxSavedPath))//��·�������ڣ��򴴽���·��
        {
            Directory.CreateDirectory(xzxSavedPath);
        }
        tmpxzxSavePath = Path.Combine(xzxSavedPath, "tmp.xzx");
        currentXzxFile = CreateXzxFileForUser("С��", "��ϰ");
    }


    private void Update()
    {
        totalFileTime += Time.unscaledDeltaTime;//��¼��¼���ļ�����ʱ��
    }

    #region*******************************�ⲿ����*********************

    /// <summary>
    /// ��ʼ����
    /// </summary>
    public void StartRecord()
    {
        isStop = false;//��ʼ¼��
        StartCoroutine(GetStateByFrameIE());
    }

    /// <summary>
    /// ֹͣ¼��
    /// </summary>
    /// <param name="mode">���ض�1Ϊ��ϰģʽ��2Ϊ����ģʽ����������10Ϊ��ϰģʽ��20Ϊ����ģʽ</param>
    public void StopRecord(int mode)
    {
        isStop = true;//ֹͣ¼��
        modeString = (mode * 10).ToString(); //���ض�1Ϊ��ϰģʽ��2Ϊ����ģʽ����������10Ϊ��ϰģʽ��20Ϊ����ģʽ
    }

    /// <summary>
    /// �����û���xzx�ļ�
    /// </summary>
    /// <param name="userAccount">�û����˻�����������Ҫ�Ӻ�˻�ȡ</param>
    /// <param name="moudle">ģ���ѡ����exa(����),pra(��ϰ)����ģ��,����ʹ�÷���ΪMoudle.exa</param>
    /// <returns>�������ɵ�xzx�ļ�</returns>
    public xzx CreateXzxFileForUser(string userAccount, string moudle)
    {
        modeString = moudle;
        userName = userAccount;
        xzx userXzxFlie = new xzx(tmpxzxSavePath);
        return userXzxFlie;
    }

    /// <summary>
    /// ��¼�۷ֵĸ������������Ϊ�ֵ�
    /// </summary>
    /// <param name="DeductPointsDict"></param>
    /// <returns></returns>
    public string RecordDeductPointsCount(Dictionary<string,int> DeductPointsDict)
    {
        dic = "";
        foreach (var pair in DeductPointsDict)
        {
            dic += $"{pair.Key}:{pair.Value}:";
        }
        return dic;
    }


    #endregion
    /// <summary>
    /// ��֡����¼��
    /// </summary>
    /// <returns></returns>
    private IEnumerator GetStateByFrameIE()
    {
        while (true)
        {
            if (!isStop)
            {
                xzxContent.frameCount = Time.frameCount;
                xzxContent.cameraPosition = Camera.main.transform.position;
                xzxContent.cameraRotation = Camera.main.transform.rotation;
                xzxContent.rightHandTransform = HandTest.instance.RightJointPosArr;
                xzxContent.leftHandTransform = HandTest.instance.LeftJointPosArr;
                xzxContent.mqttText = MqttController.instance.cacheMessage;
                xzxContent.voiceText = VoiceManager.instance.VoiceMessage;
                currentXzxFile.AppendContent(xzxContent);//��������ӽ�xzx�ļ���
            }
            else
            {
                string dateTimeString = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");//��ȡ��ǰʱ��
                currentXzxFile.AddContentAtLast(totalFileTime, dateTimeString, modeString, userName, dic);//��������ӽ�xzx�ļ���
                finalXzxPath = Path.Combine(xzxSavedPath, dateTimeString + modeString + userName + ".xzx");
                CompressFile(tmpxzxSavePath, finalXzxPath);//ѹ���ı��ļ�
                currentXzxFile.DeleteFile();
                yield break;
            }
            yield return null;
        }
    }

    /// <summary>
    /// ʹ��ѹ���㷨ѹ���ı��ļ�
    /// </summary>
    /// <param name="sourceFilePath"></param>
    /// <param name="compressedFilePath"></param>
    void CompressFile(string sourceFilePath, string compressedFilePath)
    {
        using (FileStream sourceFileStream = File.OpenRead(sourceFilePath))
        {
            using (FileStream compressedFileStream = File.Create(compressedFilePath))
            {
                using (GZipStream gzipStream = new GZipStream(compressedFileStream, CompressionMode.Compress))
                {
                    sourceFileStream.CopyTo(gzipStream);
                }
            }
        }
    }

}

