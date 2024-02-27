using System;
using System.Collections;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.IO.Compression;


//写入xzx的文件结构
public struct xzxElement
{
    public int frameCount;//当前帧数
    public Vector3 cameraPosition;//摄像机的位置信息
    public Quaternion cameraRotation;//摄像机的旋转信息
    public string mqttText;//mqtt协议的信息
    public string voiceText;//语音信息
    public List<float> rightHandTransform;//右手关节位置信息
    public List<float> leftHandTransform;//左手关节位置信息

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
    [Header("录制相关")]
    public static RecordGame instance;
    [SerializeField]
    private bool isStop= false;//是否停止录制
    private float totalFileTime=0;//录制的时间

    [Header("xzx文件相关")]
    xzxElement xzxContent = new xzxElement();//需要写入xzx文件的xzxElement，在一开始就初始化，避免频繁的初始化
    string xzxSavedPath = "";//该路径是文件夹，用来保存生成的所有finalXzxPath
    string tmpxzxSavePath="";//临时保存的位置
    string finalXzxPath = "";//最终二次压缩文件保存的位置
    xzx currentXzxFile;
    string modeString = "10";//本地端1为练习模式，2为考核模式。服务器端10为练习模式，20为考核模式
    string userName = "xiaofu";//用户名，一般从服务器端获取
    string dic = "";//记录扣分项次数的字典

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        xzxSavedPath = Path.Combine(Application.persistentDataPath, "xzxFiles");
        if (!Directory.Exists(xzxSavedPath))//若路径不存在，则创建该路径
        {
            Directory.CreateDirectory(xzxSavedPath);
        }
        tmpxzxSavePath = Path.Combine(xzxSavedPath, "tmp.xzx");
        currentXzxFile = CreateXzxFileForUser("小夫", "练习");
    }


    private void Update()
    {
        totalFileTime += Time.unscaledDeltaTime;//记录该录制文件的总时长
    }

    #region*******************************外部方法*********************

    /// <summary>
    /// 开始调用
    /// </summary>
    public void StartRecord()
    {
        isStop = false;//开始录制
        StartCoroutine(GetStateByFrameIE());
    }

    /// <summary>
    /// 停止录制
    /// </summary>
    /// <param name="mode">本地端1为练习模式，2为考核模式。服务器端10为练习模式，20为考核模式</param>
    public void StopRecord(int mode)
    {
        isStop = true;//停止录制
        modeString = (mode * 10).ToString(); //本地端1为练习模式，2为考核模式。服务器端10为练习模式，20为考核模式
    }

    /// <summary>
    /// 创建用户的xzx文件
    /// </summary>
    /// <param name="userAccount">用户的账户，该属性需要从后端获取</param>
    /// <param name="moudle">模块的选择，有exa(考核),pra(练习)两个模块,例：使用方法为Moudle.exa</param>
    /// <returns>返回生成的xzx文件</returns>
    public xzx CreateXzxFileForUser(string userAccount, string moudle)
    {
        modeString = moudle;
        userName = userAccount;
        xzx userXzxFlie = new xzx(tmpxzxSavePath);
        return userXzxFlie;
    }

    /// <summary>
    /// 记录扣分的各项次数，保存为字典
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
    /// 逐帧进行录制
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
                currentXzxFile.AppendContent(xzxContent);//将内容添加进xzx文件中
            }
            else
            {
                string dateTimeString = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");//获取当前时间
                currentXzxFile.AddContentAtLast(totalFileTime, dateTimeString, modeString, userName, dic);//将内容添加进xzx文件中
                finalXzxPath = Path.Combine(xzxSavedPath, dateTimeString + modeString + userName + ".xzx");
                CompressFile(tmpxzxSavePath, finalXzxPath);//压缩文本文件
                currentXzxFile.DeleteFile();
                yield break;
            }
            yield return null;
        }
    }

    /// <summary>
    /// 使用压缩算法压缩文本文件
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

