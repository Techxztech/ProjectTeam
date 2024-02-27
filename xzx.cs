using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public class xzx
{
    private string filePath;
    const string xzxFileID = "This is an xzx file";
    const string fileExtersion = "xzx";
    const string cd = "&&";//ContentDelimiter,���ݷָ���

    public int TotalFrame { get => GetTotalLineCount(); }//������Ҳ�����ܵ�֡��
    public float TotalTime { get => GetTotalTime(); }//��ȡxzx�ļ�����ʱ��


    //���캯��
    //xzx������ӱ�ʶ
    public xzx(string savePath)
    {
        filePath = savePath;
        // ����ļ���չ���Ƿ�Ϊ .xzx
        if (Path.GetExtension(filePath).Equals("."+fileExtersion, StringComparison.OrdinalIgnoreCase))
        {
            // ����ļ��Ƿ���ڣ�����������򴴽��ļ�
            if (!File.Exists(filePath))
            {
                using (StreamWriter writer = File.CreateText(filePath))
                {
                    writer.Write(xzxFileID);//�ڿ�ͷ���xzx�ļ��ı�ʶ
                }
            }
            else
            {
                // ����ļ��Ѵ��ڣ���鲢ȷ���� "This is an xzx file" ��ͷ
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string fileContent = reader.ReadToEnd();
                    if (!fileContent.StartsWith(xzxFileID))
                    {
                        throw new ArgumentException("xzx�ļ���");
                    }
                }
            }
        }
        else
        {
            // ����ļ���չ������ .xzx����������ʵ��Ĵ������߼�
            throw new ArgumentException(savePath + "--���ļ�����xzx���͵��ļ�");
        }
    }

    #region***********************�ⲿ����**********************************
    /// <summary>
    /// ������д��xzx�ļ����Ḳ��֮ǰд�������
    /// </summary>
    /// <param name="content">��Ҫд�������</param>
    public void WriteContent(xzxElement content)
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            string formattedContent = Formattext(content);
            writer.Write(xzxFileID + Environment.NewLine + formattedContent);
        }
    }

    /// <summary>
    /// ��xzx�ļ���������ݣ����Ḳ��ԭ������
    /// </summary>
    /// <param name="content">��Ҫ��ӵ�����</param>
    public void AppendContent(xzxElement content)
    {
        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            string formattedContent = Formattext(content);
            writer.Write(Environment.NewLine + formattedContent);
        }
    }

    /// <summary>
    /// ���ļ����һ���������
    /// </summary>
    /// <param name="content"></param>
    public void AddContentAtLast(float totalTime,string dateTimeString,string modeString,string userName,string dic)
    {
        // �ϳ�ʱ����û������ַ��������� "&&" �ָ�
        string lastLine = $"{totalTime}{cd}{dateTimeString}{cd}{modeString}{cd}{userName}{cd}{dic}";
        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            writer.Write(Environment.NewLine + lastLine);
        }
    }

    /// <summary>
    /// ����xzx�ļ�¼�Ƶ���ʱ��
    /// </summary>
    /// <returns></returns>
    public float GetTotalTime()
    {
        float totalTime = 0;
        string lastContent = ReadLineContent(GetTotalLineCount());
        // �ָ��ַ���
        string[] parts = lastContent.Split(new string[] { cd }, StringSplitOptions.None);
        try 
        {
            float.TryParse(parts[0], out totalTime);
        }
        catch
        {
            throw new ArgumentException("�ļ���ʱ���ȡ����");
        }
        return totalTime;
    }

    /// <summary>
    /// ��ȡxzx����¼�µ�����ʱ��
    /// </summary>
    /// <returns></returns>
    public string GetDateTime()
    {
        string lastContent = ReadLineContent(GetTotalLineCount());
        // �ָ��ַ���
        string[] parts = lastContent.Split(new string[] { cd }, StringSplitOptions.None);

        return parts[1];
    }

    /// <summary>
    /// ��ȡxzx�ļ���¼��ģʽ
    /// </summary>
    /// <returns></returns>
    public string GetUserName()
    {
        string lastContent = ReadLineContent(GetTotalLineCount());
        // �ָ��ַ���
        string[] parts = lastContent.Split(new string[] { cd }, StringSplitOptions.None);

        return parts[3];
    }

    /// <summary>
    /// ��ȡxzx�ļ���¼��ģʽ
    /// </summary>
    /// <returns></returns>
    public string Getmode()
    {
        string lastContent = ReadLineContent(GetTotalLineCount());
        // �ָ��ַ���
        string[] parts = lastContent.Split(new string[] { cd }, StringSplitOptions.None);

        return parts[2];
    }

    public Dictionary<string, int> GetDic()
    {
        Dictionary<string, int> jsonDic = new Dictionary<string, int>();
        string lastContent = ReadLineContent(GetTotalLineCount());
        // �ָ��ַ���
        string[] parts = lastContent.Split(new string[] { cd }, StringSplitOptions.None);

        string dic = parts[4];
        string[] dicParts = dic.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < dicParts.Length / 2; i++)
        {
            jsonDic.Add(dicParts[i], int.Parse(dicParts[i * 2]));
        }
        return jsonDic;
    }

    /// <summary>
    /// ��ȡxzx�ļ��������ݣ�����list
    /// </summary>
    /// <returns>����ȡ�������ݣ��洢��list�У�ÿһ��Ϊһ��Ԫ�أ�ȥ���˻��з�</returns>
    public List<string> ReadContent()
    {
        List<string> lines= new List<string>();
        try
        {
            // �ӵڶ��п�ʼ��ȡ�ļ����ݣ������ȡ��һ�еı�ʶ
            using (StreamReader reader = new StreamReader(filePath))
            {
                string firstLine = reader.ReadLine(); //��ȡ��һ�в�����
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line.Replace(Environment.NewLine, ""));
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("��ȡxzx�ļ����ݳ���������ϢΪ" + e.Message);
        }
        return lines;
    }

    /// <summary>
    /// ��ȡ�������ݣ����������з�
    /// </summary>
    /// <param name="lineNumber">����</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">�׳������쳣</exception>
    public string ReadLineContent(int lineNumber)
    {
        if (lineNumber < 0)
        {
            throw new ArgumentException("�кŴ������");
        }

        if (lineNumber == 0) return null;//��ȡ������1��ʼ��������Ϊ0����ֱ������

        try
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                int currentLine = 0;

                // �ӵ�һ�п�ʼ��ȡ����������ʶ��
                while ((line = reader.ReadLine()) != null)
                {
                    if (currentLine == lineNumber)
                    {
                        // ȥ�����з�
                        return line.Replace(Environment.NewLine, "");
                    }

                    currentLine++;
                }

                return string.Empty; // δ�ҵ�ָ����
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("��ȡ�����ݳ���������ϢΪ" + e.Message);
            return string.Empty;
        }
    }

    /// <summary>
    /// ��������ȡxzx�ļ����ݣ�ǰ���Ϊ�����䣬��ʼ�кͽ����ж����ȡ
    /// </summary>
    /// <param name="startLine">��ʼ��</param>
    /// <param name="endLine">������</param>
    /// <returns>��ȡ��������</returns>
    /// <exception cref="ArgumentException">��������ᵼ���쳣</exception>
    public string ReadLinesContent(int startLine, int endLine)
    {
        if (startLine <= 0 || endLine <= 0 || startLine > endLine)
        {
            throw new ArgumentException("�����������");
        }

        try
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                string content = string.Empty;
                int lineNumber = 0;//�ļ�������

                // �ӵ�һ�п�ʼ��ȡ����������ʶ��
                while ((line = reader.ReadLine()) != null)
                {
                    if (lineNumber > 0) // ������һ�У���ʶ�У�
                    {
                        if (lineNumber >= startLine && lineNumber <= endLine)
                        {
                            content += line + Environment.NewLine;
                        }
                    }

                    if (lineNumber > endLine) // �Ѿ���ȡ��ָ��������ֹͣ
                    {
                        break;
                    }

                    lineNumber++;
                }

                //if (startLine > lineNumber || endLine > lineNumber)
                //{
                //    throw new InvalidOperationException("�ļ�����С�ڴ�������");
                //}

                return content;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("��ȡxzx�ļ����ݳ���������ϢΪ" + e.Message);
            return string.Empty;
        }
    }

    /// <summary>
    /// ��ȡĳ��xzx�ļ���������
    /// </summary>
    /// <returns>����ֵΪ����</returns>
    public int GetTotalLineCount()
    {
        try
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                int totalLines = -1;

                // �ӵ�һ�п�ʼ��ȡ����������ʶ��
                while (reader.ReadLine() != null)
                {
                    totalLines++;
                }

                return totalLines;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("��ȡ�ļ�����������������ϢΪ" + e.Message);
            return -1; // ����ʱ���� -1
        }
    }

    bool ahish = false;
    public xzxElement ParseContent(string content)
    {
        // �ָ��ַ���
        string[] parts = content.Split(new string[] { cd }, StringSplitOptions.None);

        int fc;
        float cpx=0, cpy=0, cpz=0, crx=0, cry=0, crz=0, crw=0;
        //UnityEngine.Debug.Log("parts[0]=="+ parts[0]);
        if (!int.TryParse(parts[0], out fc)
            || !float.TryParse(parts[1], out cpx) || !float.TryParse(parts[2], out cpy) || !float.TryParse(parts[3], out cpz)
            || !float.TryParse(parts[4], out crx) || !float.TryParse(parts[5], out cry) || !float.TryParse(parts[6], out crz) || !float.TryParse(parts[7], out crw)
            )
        {
            throw new ArgumentException("frameCount ��ʽ����");
        }
        //CameraPosition
        Vector3 cp = new Vector3
        {
            x = cpx, y = cpy, z = cpz
        };
        //CameraRotation
        Quaternion cr = new Quaternion()
        {
            x= crx, y= cry, z= crz, w= crw
        };

        //mqttText
        string mt = parts[8];

        //voiceText
        string vt = parts[9];

        List<float> rhj = new List<float>();
        for (int i = 10; i < 73; i++)
        {
            rhj.Add(float.Parse(parts[i]));
        }
       
        List<float> lhj = new List<float>();
        for (int i = 73; i < 136; i++)
        {
            lhj.Add(float.Parse(parts[i]));
        }
        return new xzxElement(fc, cp,cr, rhj,lhj, mt, vt);
    }

    /// <summary>
    /// ���xzx�ļ�����
    /// </summary>
    public void ClearContent()
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            writer.Write(xzxFileID);
        }
    }

    /// <summary>
    /// ɾ����ǰ�ļ�
    /// </summary>
    /// <exception cref="FileNotFoundException">�ļ������ڵ��쳣</exception>
    public void DeleteFile()
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        else
        {
            throw new FileNotFoundException("Ŀ���ļ������ڣ�" + filePath);
        }
    }

    #endregion

    #region***********************˽�з���**********************************
    
    /// <summary>
    /// ��xzxElement������ʽ��Ϊ�ı�
    /// </summary>
    /// <returns></returns>
    private string Formattext(xzxElement xzxContent)
    {
        //FrameCount
        int fc = xzxContent.frameCount;
        //CameraPositionX,Y,Z        
        float cpx = xzxContent.cameraPosition.x;
        float cpy = xzxContent.cameraPosition.y;
        float cpz = xzxContent.cameraPosition.z;
        //CameraRotationX,Y,Z,W
        float crx = xzxContent.cameraRotation.x;
        float cry = xzxContent.cameraRotation.y;
        float crz = xzxContent.cameraRotation.z;
        float crw = xzxContent.cameraRotation.w;
        //rightHandJoint,leftHandJoint
        string rhj = "";
        for (int i = 0; i < xzxContent.rightHandTransform.Count; i++)
        {
            rhj += $"{xzxContent.rightHandTransform[i]}{cd}";
        }
        string lhj = "";
        for (int i = 0; i < xzxContent.leftHandTransform.Count; i++)
        {
            rhj += $"{xzxContent.leftHandTransform[i]}{cd}";
        }

        //MqttText
        string mt = xzxContent.mqttText;

        //voiceText
        string vt= xzxContent.voiceText;
        return $"{fc}{cd}{cpx}{cd}{cpy}{cd}{cpz}{cd}{crx}{cd}{cry}{cd}{crz}{cd}{crw}{cd}{mt}{cd}{vt}{cd}{rhj}{cd}{lhj}";
    }

    private void PlayBackHand(string[] handElement)
    {
        for (int i = 0; i < handElement.Length / 3; i++)
        {
            string[] thereElement = new string[3];
            Array.Copy(handElement, i * 3, thereElement, 0, 3);
            float hpx = 0, hpy = 0, hpz = 0;
            if (!float.TryParse(thereElement[0], out hpx) || !float.TryParse(thereElement[1], out hpy) || !float.TryParse(thereElement[2], out hpz))
            {
                throw new ArgumentException("frameCount ��ʽ����");
            }
            //CameraPosition
            Vector3 hp = new Vector3
            {
                x = hpx,
                y = hpy,
                z = hpz
            };
            //rightJointList[i].transform.position = hp;

        }

    }

    #endregion
}
