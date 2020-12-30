/*
 * 長條圖管理。
 */
using System.Collections.Generic;
using UnityEngine;

public class RankingManager : MonoBehaviour
{
    [SerializeField] GameObject BarPanel = null;//長條圖
    [SerializeField] GameObject RankingPanel = null;//長條圖放置位置

    [Range(1,10)]
    public float speed = 10;//數值變化速度
    public List<TeamScore> teamScoreList;//組別分數資訊列表
    private List<Transform> barPanelList = new List<Transform>();
    private List<BarPanelController> barControllerList = new List<BarPanelController>();//長條圖控制
    public int highScore { get; private set; }

    void Start()
    {
        Init();
    }


    /// <summary>
    /// 初始化介面及資料。
    /// </summary>
    void Init()
    {
        for (int i = 0; i < teamScoreList.Count; i++)
        {
            GameObject bar = Instantiate(BarPanel, RankingPanel.transform);
            BarPanelController controller = bar.GetComponent<BarPanelController>();
            TeamScore s = teamScoreList[i];
            bar.name = s.name;
            controller.SetData(i, s.name, s.score, speed, s.color);
            //註冊
            barPanelList.Add(bar.transform);
            barControllerList.Add(controller);
        }
    }

    /// <summary>
    /// 根據目前型態調整介面及排序。
    /// </summary>
    public void SortingAndAdjust(int index)
    {
        for (int i = 0; i < barPanelList.Count; i++)
        {

            //位置
            if (barControllerList[index].currentScore > barControllerList[i].currentScore)
            {
                int current = barPanelList[index].GetSiblingIndex();
                int temp = barPanelList[i].GetSiblingIndex();

                if(current > temp)
                {
                    barPanelList[i].SetSiblingIndex(current);
                    barPanelList[index].SetSiblingIndex(temp);
                }
                
            }


            //比例調整，除了當下自己之外
            if (index != i)
            {
                barControllerList[i].SetBarAccordingByHighScore(highScore);
            }
        }
    }
    /// <summary>
    /// 挑戰是否有機會成為最高分。
    /// </summary>
    public bool CheckHighScoreOrSet(int challanger)
    {
        if (challanger > highScore)
        {
            highScore = challanger;//挑戰成功
            return true;
        }

        return false;
    }
}
