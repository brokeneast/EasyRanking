/*
 * 單一長條圖控制器。
 */
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BarPanelController : MonoBehaviour
{
    RankingManager rankingManager;//排名管理

    [SerializeField] Image barRect = null;//長條
    [SerializeField] Text nameLabel = null;//顯示名
    [SerializeField] Text scoreLabel = null;//分數欄
    [SerializeField] ParticleSystem ribbonParticle = null;//粒子
    [SerializeField] LayoutElement barLayout = null;
    [SerializeField] LayoutElement scoreLayout = null;

    public int index { get; private set; }
    public enum Status { DATA_SETTED, BAR_IS_RUNNING, DONE };
    public Status status{get; private set;}

    //Speed
    private float _speed = 10f;
    public float speed//增長速度
    {
        get { return _speed; }
        set
        {
            if (value < 1f)
            {
                _speed = 1f;
            }
            else if (value > 10f)
            {
                _speed = 10f;
            }
        }
    }

    private float delay = 0.01f;

    //Score
    public int score { get; private set; }//分數
    public int currentScore { get; private set; }//temp
    private int addingScore = 10;//分數增加量

    //Bar
    private float _addingRatio = 0.001f;
    public float addingRatio//長條比例增加
    {
        get { return _addingRatio; }
        set
        {
            if (value < 0.001f)
            {
                _addingRatio = 0.001f;
            }
            else if (value > 0.01f)
            {
                _addingRatio = 0.01f;
            }
        }
    }
    private float ratio = 0;//長條圖比例
    private float _ratio = 0;//temp
    private int highScore = 1;//目前總體最高分

    private void Awake()
    {
        rankingManager = FindObjectOfType<RankingManager>();
        ribbonParticle.Stop();
    }


    /// <summary>
    /// 先設定分數值及資訊。
    /// </summary>
    public void SetData(int index, string team, int score, float speed,Color color)
    {
        this.index = index;
        status = Status.DATA_SETTED;
        nameLabel.text = team;
        this.score = score;
        barRect.color = color;
        nameLabel.color = color;
        scoreLabel.color = color;
        SetSpeed(speed);
        //粒子
        var main = ribbonParticle.main;
        main.startColor = color;
    }

    /// <summary>
    /// 啟動動畫。
    /// </summary>
    public void Action()
    {
        StartRunning();
    }

    /// <summary>
    /// 分數增長至設定值。
    /// </summary>
    public void StartRunning()
    {
        status = Status.BAR_IS_RUNNING;
        ribbonParticle.Play();
        //數值與Bar皆改變
        if (currentScore != score)
        {
            //由設定分數改變Bar
            SetScoreLerp();
        }
    }

    /// <summary>
    /// 設置速度。
    /// </summary>
    private void SetSpeed(float speed)
    {
        this.speed = speed;

        delay = ((10 - speed) + 1) / 100f;
    }

    #region Score
    /// <summary>
    /// 依分數為變動基準。
    /// </summary>
    private void SetScoreLerp()
    {
        StartCoroutine(ScoreChange());
    }

    private IEnumerator ScoreChange()
    {
        if (Math.Ceiling((float)currentScore / addingScore) == Math.Ceiling((float)score / addingScore))//已經等於設定之分數
        {
            currentScore = score;
            CheckHighScore();//檢查最高分
            _ratio = (float)currentScore / highScore;//調整比例
            BarRatioChange(_ratio);

            scoreLabel.text = currentScore.ToString();//設定分數
            ribbonParticle.Stop();//關閉粒子
            status = Status.DONE;

            yield break;
        }
        else
        {
            if (score > currentScore)//尚未超過
            {
                currentScore = currentScore + addingScore;//增加
            }
            else if (score < currentScore)//已超過
            {
                currentScore = currentScore - addingScore;//扣回
            }
            CheckHighScore();//檢查最高分
            _ratio = (float)currentScore / highScore;//調整比例
            BarRatioChange(_ratio);

            scoreLabel.text = currentScore.ToString();
            rankingManager.SortingAndAdjust(index);

            yield return new WaitForSeconds(delay);
            yield return StartCoroutine(ScoreChange());
        }
    }

    /// <summary>
    /// 檢查目前最高分為多少，因為會影響長條圖之顯示比例。
    /// </summary>
    private void CheckHighScore()
    {
        if (rankingManager.highScore == 0)//首次
        {
            rankingManager.CheckHighScoreOrSet(score);
            highScore = score;
        }
        else if (rankingManager.CheckHighScoreOrSet(currentScore))
        {
            highScore = currentScore;
        }
        else
        {
            highScore = rankingManager.highScore;
        }
    }

    #endregion
    #region Bar
    /// <summary>
    /// 依長條比例為變動基準(0~1)。
    /// </summary>
    private void SetBarLerp(float ratio)
    {
        this.ratio = Mathf.Clamp01(ratio);
        StartCoroutine(BarChange());
    }

    private IEnumerator BarChange()
    {
        if (Math.Round(_ratio, 2) == Math.Round(ratio, 2))
        {
            _ratio = ratio;
            BarRatioChange(_ratio);
            yield break;
        }
        else
        {
            if (ratio > _ratio)
            {
                _ratio = _ratio + addingRatio;
            }
            else if (ratio < _ratio)
            {
                _ratio = _ratio - addingRatio;
            }
            BarRatioChange(_ratio);
            yield return new WaitForSeconds(delay);
            yield return StartCoroutine(BarChange());
        }
    }

    /// <summary>
    /// 僅控制Bar比例。
    /// </summary>
    private void BarRatioChange(float r)
    {
        barLayout.flexibleWidth = r;
        scoreLayout.flexibleWidth = 1 - r;
    }

    /// <summary>
    /// 根據目前最高分調整Bar比例。
    /// </summary>
    /// <param name="highScore"></param>
    public void SetBarAccordingByHighScore(int highScore)
    {
        BarRatioChange((float)currentScore / highScore);
    }
    #endregion
}
