using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallInfoCSV : IDisposable
{
    // private StreamWriter m_streamWriter;
    private List<string> m_data;
    private string m_filePath;

    public BallInfo[] Load(string fileName)
    {
        List<BallInfo> result = new List<BallInfo>();
        //todo
        TextAsset csv = Resources.Load<TextAsset>(fileName);

        if(csv)
        {
            string[] splitText = csv.text.Split(char.Parse("\n"));
            // result = new BallInfo[splitText.Length];

            //splitText[i] = "<Balltype>,<velocity>,<line>,<column>"
            for(int i = 0; i < splitText.Length; i++)
            {
                if(String.IsNullOrWhiteSpace(splitText[i])) continue;

                string[] texts = splitText[i].Split(',');
                BallInfo info = new BallInfo();

                if(texts.Length != 4)
                    throw new System.Exception("Line " + i + ": Not enough arguments");

                if(!System.Enum.TryParse(texts[0], out info.type) && System.Enum.IsDefined(typeof(BallType), info.type))
                {
                    if(i == 0) continue;
                    throw new System.Exception("Line " + i + ": There is no definition in BallType: " + info.type);
                }

                if(!float.TryParse(texts[1], out info.velocity))
                    throw new System.Exception("Line " + i + ": Input string could not be converted to Velocity <Float>: " + info.velocity);

                if(!int.TryParse(texts[2], out info.line))
                    throw new System.Exception("Line " + i + ": Input string could not be converted to Line <Int>: " + info.line);

                if(!int.TryParse(texts[3], out info.column))
                    throw new System.Exception("Line " + i + ": Input string could not be converted to column <Int>: " + info.column);

                result.Add(info);
            }
        }

        return result.ToArray();
    }

    public void SaveFile()
    {
        // Dispose();
        using (StreamWriter sw = new StreamWriter(m_filePath, true, Encoding.UTF8))
        {
            foreach(string s in m_data)
            {
                sw.WriteLine(s);
            }
        }
    }

    public void NewFile(string fileName = "BaseballRefereeTraining_Result")
    {
        // m_streamWriter = new StreamWriter(GetPath(AddDate(fileName)), true, Encoding.UTF8) { AutoFlush = true };
        // m_streamWriter.WriteLine("BallType,Velocity,Line,Column,IsCorrect,JudgementTime");
        m_filePath = GetPath(AddDate(fileName));
        m_data = new List<string>();
        m_data.Add("BallType,Velocity,Line,Column,IsCorrect,JudgementTime");
    }

    public void Write(BallInfo ballInfo, bool isCorrect, float time)
    {
        string[] line = new string[6];

        line[0] = ballInfo.type.ToString();
        line[1] = ballInfo.velocity.ToString();
        line[2] = ballInfo.line.ToString();
        line[3] = ballInfo.column.ToString();
        line[4] = isCorrect ? "Correct" : "Incorrect";
        line[5] = time.ToString();

        // m_streamWriter.WriteLine(string.Join(",", line));
        m_data.Add(string.Join(",", line));
    }

    private string AddDate(string fileName)
    {
        fileName += "_" + DateTime.Now.Year.ToString("D4") + DateTime.Now.Month.ToString("D2") + DateTime.Now.Day.ToString("D2");
        fileName += "_" + DateTime.Now.Hour.ToString("D2") + DateTime.Now.Minute.ToString("D2");
        return fileName;
    }

    private string GetPath(string fileName)
    {
        string filePath;

#if UNITY_EDITOR
        filePath = Application.dataPath + "/Resources/" + fileName + ".csv";
#else
        filePath = Application.persistentDataPath + "/" + fileName + ".csv";
#endif

        return filePath;
    }

    public void Dispose()
    {
        // m_streamWriter.Flush();
        // m_streamWriter.Close();
        // m_streamWriter.Dispose();
    }
}

