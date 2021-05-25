using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallInfoCSV
{
    //ヘッダーレス前提
    public static BallInfo[] Load(string fileName)
    {
        List<BallInfo> result = new List<BallInfo>();
        TextAsset csv = Resources.Load<TextAsset>(fileName);

        if(csv)
        {
            string[] splitText = csv.text.Split(char.Parse("\n"));
            // result = new BallInfo[splitText.Length];

            //splitText[i] = "<Balltype>,<velocity>,<line>,<column>"
            for(int i = 0; i < splitText.Length; i++)
            {
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

    private string GetPath(string fileName)
    {
        string filePath;

#if UNITY_EDITOR
        filePath = Application.dataPath + "/BaseballRefereeTraining/Resources" + fileName;
#else
        filePath = Application.persistentDataPath + "/" + fileName;
#endif

        return filePath;
    }
}

