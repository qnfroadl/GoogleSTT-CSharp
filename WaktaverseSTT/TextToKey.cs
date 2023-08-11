using Google.Protobuf.Collections;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaktaverseSTT
{
    // 문자열을 비교해서 Key를 입력하는 기능
    internal class TextToKey
    {
        
        private Dictionary<string, int> textKeyMap;

        //txt파일을 열어서 비교할 문자열 읽어오기
        public void Init(string filePath)
        {
            try
            {
                textKeyMap = new Dictionary<string, int>();

                if (File.Exists(filePath))
                {
                    string contents = File.ReadAllText(filePath);
                    string[] lineText = contents.Split("\n");
                    foreach (string line in lineText)
                    {
                        string[] textKey = line.Split("/");

                        string[] textList = textKey[0].Split(","); // 
                        int keyCode = int.Parse(textKey[1].Trim().Replace("0x",""), System.Globalization.NumberStyles.AllowHexSpecifier | System.Globalization.NumberStyles.HexNumber); // keyCode.

                        foreach (string text in textList)
                        {
                            textKeyMap.Add(text.Trim(), keyCode);
                        }
                    }
                }
            }
            catch(Exception e)
            {
                
                MessageBox.Show("ttk.info의 포멧이 잘못되었습니다. \n" + e.Message);
            }
           
        }

        public int GetKey(string text)
        {
            foreach(string key in textKeyMap.Keys)
            {
                if(text.Contains(key))
                {
                    return textKeyMap[key];
                }
            }
            return 0;
        }
    }
}
