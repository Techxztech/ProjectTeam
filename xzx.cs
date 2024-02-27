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
    const string cd = "&&";//ContentDelimiter,内容分隔符

    public int TotalFrame { get => GetTotalLineCount(); }//总行数也就是总的帧数
    public float TotalTime { get => GetTotalTime(); }//获取xzx文件的总时间


    //构造函数
    //xzx，并添加标识
    public xzx(string savePath)
    {
        filePath = savePath;
        // 检查文件扩展名是否为 .xzx
        if (Path.GetExtension(filePath).Equals("."+fileExtersion, StringComparison.OrdinalIgnoreCase))
        {
            // 检查文件是否存在，如果不存在则创建文件
            if (!File.Exists(filePath))
            {
                using (StreamWriter writer = File.CreateText(filePath))
                {
                    writer.Write(xzxFileID);//在开头添加xzx文件的标识
                }
            }
            else
            {
                // 如果文件已存在，检查并确保以 "This is an xzx file" 开头
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string fileContent = reader.ReadToEnd();
                    if (!fileContent.StartsWith(xzxFileID))
                    {
                        throw new ArgumentException("xzx文件损坏");
                    }
                }
            }
        }
        else
        {
            // 如果文件扩展名不是 .xzx，可以添加适当的错误处理逻辑
            throw new ArgumentException(savePath + "--该文件不是xzx类型的文件");
        }
    }

    #region***********************外部方法**********************************
    /// <summary>
    /// 将内容写入xzx文件，会覆盖之前写入的内容
    /// </summary>
    /// <param name="content">需要写入的内容</param>
    public void WriteContent(xzxElement content)
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            string formattedContent = Formattext(content);
            writer.Write(xzxFileID + Environment.NewLine + formattedContent);
        }
    }

    /// <summary>
    /// 向xzx文件内添加内容，不会覆盖原有内容
    /// </summary>
    /// <param name="content">需要添加的内容</param>
    public void AppendContent(xzxElement content)
    {
        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            string formattedContent = Formattext(content);
            writer.Write(Environment.NewLine + formattedContent);
        }
    }

    /// <summary>
    /// 在文件最后一列添加内容
    /// </summary>
    /// <param name="content"></param>
    public void AddContentAtLast(float totalTime,string dateTimeString,string modeString,string userName,string dic)
    {
        // 合成时间和用户名称字符串，并用 "&&" 分割
        string lastLine = $"{totalTime}{cd}{dateTimeString}{cd}{modeString}{cd}{userName}{cd}{dic}";
        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            writer.Write(Environment.NewLine + lastLine);
        }
    }

    /// <summary>
    /// 返回xzx文件录制的总时间
    /// </summary>
    /// <returns></returns>
    public float GetTotalTime()
    {
        float totalTime = 0;
        string lastContent = ReadLineContent(GetTotalLineCount());
        // 分割字符串
        string[] parts = lastContent.Split(new string[] { cd }, StringSplitOptions.None);
        try 
        {
            float.TryParse(parts[0], out totalTime);
        }
        catch
        {
            throw new ArgumentException("文件总时间读取错误");
        }
        return totalTime;
    }

    /// <summary>
    /// 获取xzx最后记录下的日期时间
    /// </summary>
    /// <returns></returns>
    public string GetDateTime()
    {
        string lastContent = ReadLineContent(GetTotalLineCount());
        // 分割字符串
        string[] parts = lastContent.Split(new string[] { cd }, StringSplitOptions.None);

        return parts[1];
    }

    /// <summary>
    /// 获取xzx文件记录的模式
    /// </summary>
    /// <returns></returns>
    public string GetUserName()
    {
        string lastContent = ReadLineContent(GetTotalLineCount());
        // 分割字符串
        string[] parts = lastContent.Split(new string[] { cd }, StringSplitOptions.None);

        return parts[3];
    }

    /// <summary>
    /// 获取xzx文件记录的模式
    /// </summary>
    /// <returns></returns>
    public string Getmode()
    {
        string lastContent = ReadLineContent(GetTotalLineCount());
        // 分割字符串
        string[] parts = lastContent.Split(new string[] { cd }, StringSplitOptions.None);

        return parts[2];
    }

    public Dictionary<string, int> GetDic()
    {
        Dictionary<string, int> jsonDic = new Dictionary<string, int>();
        string lastContent = ReadLineContent(GetTotalLineCount());
        // 分割字符串
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
    /// 读取xzx文件所有内容，返回list
    /// </summary>
    /// <returns>所读取到的内容，存储在list中，每一行为一个元素，去除了换行符</returns>
    public List<string> ReadContent()
    {
        List<string> lines= new List<string>();
        try
        {
            // 从第二行开始读取文件内容，避免读取第一行的标识
            using (StreamReader reader = new StreamReader(filePath))
            {
                string firstLine = reader.ReadLine(); //读取第一行并忽略
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line.Replace(Environment.NewLine, ""));
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("读取xzx文件内容出错，错误信息为" + e.Message);
        }
        return lines;
    }

    /// <summary>
    /// 读取单行内容，不包含换行符
    /// </summary>
    /// <param name="lineNumber">行数</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">抛出行数异常</exception>
    public string ReadLineContent(int lineNumber)
    {
        if (lineNumber < 0)
        {
            throw new ArgumentException("行号传入错误");
        }

        if (lineNumber == 0) return null;//读取行数从1开始，若行数为0，则直接跳过

        try
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                int currentLine = 0;

                // 从第一行开始读取，不包括标识行
                while ((line = reader.ReadLine()) != null)
                {
                    if (currentLine == lineNumber)
                    {
                        // 去除换行符
                        return line.Replace(Environment.NewLine, "");
                    }

                    currentLine++;
                }

                return string.Empty; // 未找到指定行
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("读取行内容出错，错误信息为" + e.Message);
            return string.Empty;
        }
    }

    /// <summary>
    /// 按行数读取xzx文件内容，前后皆为闭区间，起始行和结束行都会读取
    /// </summary>
    /// <param name="startLine">起始行</param>
    /// <param name="endLine">结束行</param>
    /// <returns>读取到的内容</returns>
    /// <exception cref="ArgumentException">行数出错会导致异常</exception>
    public string ReadLinesContent(int startLine, int endLine)
    {
        if (startLine <= 0 || endLine <= 0 || startLine > endLine)
        {
            throw new ArgumentException("行数传入错误");
        }

        try
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                string content = string.Empty;
                int lineNumber = 0;//文件总行数

                // 从第一行开始读取，不包括标识行
                while ((line = reader.ReadLine()) != null)
                {
                    if (lineNumber > 0) // 跳过第一行（标识行）
                    {
                        if (lineNumber >= startLine && lineNumber <= endLine)
                        {
                            content += line + Environment.NewLine;
                        }
                    }

                    if (lineNumber > endLine) // 已经读取到指定行数，停止
                    {
                        break;
                    }

                    lineNumber++;
                }

                //if (startLine > lineNumber || endLine > lineNumber)
                //{
                //    throw new InvalidOperationException("文件行数小于传入行数");
                //}

                return content;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("读取xzx文件内容出错，错误信息为" + e.Message);
            return string.Empty;
        }
    }

    /// <summary>
    /// 获取某个xzx文件的总行数
    /// </summary>
    /// <returns>返回值为行数</returns>
    public int GetTotalLineCount()
    {
        try
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                int totalLines = -1;

                // 从第一行开始读取，不包括标识行
                while (reader.ReadLine() != null)
                {
                    totalLines++;
                }

                return totalLines;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("获取文件总行数出错，错误信息为" + e.Message);
            return -1; // 出错时返回 -1
        }
    }

    bool ahish = false;
    public xzxElement ParseContent(string content)
    {
        // 分割字符串
        string[] parts = content.Split(new string[] { cd }, StringSplitOptions.None);

        int fc;
        float cpx=0, cpy=0, cpz=0, crx=0, cry=0, crz=0, crw=0;
        //UnityEngine.Debug.Log("parts[0]=="+ parts[0]);
        if (!int.TryParse(parts[0], out fc)
            || !float.TryParse(parts[1], out cpx) || !float.TryParse(parts[2], out cpy) || !float.TryParse(parts[3], out cpz)
            || !float.TryParse(parts[4], out crx) || !float.TryParse(parts[5], out cry) || !float.TryParse(parts[6], out crz) || !float.TryParse(parts[7], out crw)
            )
        {
            throw new ArgumentException("frameCount 格式错误");
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
    /// 清空xzx文件内容
    /// </summary>
    public void ClearContent()
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            writer.Write(xzxFileID);
        }
    }

    /// <summary>
    /// 删除当前文件
    /// </summary>
    /// <exception cref="FileNotFoundException">文件不存在的异常</exception>
    public void DeleteFile()
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        else
        {
            throw new FileNotFoundException("目标文件不存在：" + filePath);
        }
    }

    #endregion

    #region***********************私有方法**********************************
    
    /// <summary>
    /// 将xzxElement变量格式化为文本
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
                throw new ArgumentException("frameCount 格式错误");
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
