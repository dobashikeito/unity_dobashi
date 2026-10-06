//x→いじらない、何もなし→場合によって変える
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class sample_cp_trigger : MonoBehaviour
{
    public GameObject[] checkpoints;//x
    public GameObject[] astm_checkpoints;//x inspector上に直接アタッチする。対称ステップフィールドで学習する環境のやつの経路を表すgameobjectが格納されたgameobjectを。
    public GameObject[] stairs_checkpoints;//x 上の階段ver
    public GameObject[] steps_checkpoints;//x 上の段差ver
    public GameObject eight_wall;//x 使っていない
    public GameObject[] cp_list;//x 表示されるウェイポイントが１つずつ格納される。
    public GameObject[] eight_wall_list;//x 使っていない
    public int now_cp=0;//x 次にエージェントが触れるべきウェイポイントのindex番号が保存される。
    public int cp_index=0;//x この番号のindexの経路が使われる。
    [Header("経路を固定したいときに使用、使わないときは10にする")]
    public int cp_index_fixed;//x defaultでは、経路がランダムに変わる。固定した時に使用。
    public int count_check_point;//x ウェイポイントの数はあるだけ。20個と固定値じゃなくてもいい。そのために使用。
    public int count_eight_wall;//x 
    public bool cp_through=false;//エージェントファイル側で、ウェイポイントに触れたかどうかを判断するのに使用。
    public bool touch_eight_wall=false;//x 使っていないが、体が、下で指定されている名前にぶつかっているときtrueになる。これを利用して、報酬を作ることができる。
    public bool touch_clickpoint=false;//x click pointモードで使用。学習には使用しないので一旦無視。
    public float cp_reward=1.0f;//ウェイポイントに触れたときに報酬を与えるを作っていた時に使用。ウェイポイントすべてに触れたときに得られる報酬を示している。20個すべてに触れた計1点与えられる。使用していない
    public float turn_reward=0.0f;//x 総ウェイポイントの数の半分を通ったときに、得られる報酬。使っていない
    private int click_point_index=1;//x click pointモードで使用。学習には使用しないので一旦無視。

    //使う経路を決定。エージェントファイル内で実行される。地形ごとで使う経路は変わるので、field_numberでif処理をしている。
    public void make_cp(int field_number)
    {
        //使う経路gameobjectが保存されたgameobjectのリストを、if処理で場合分け
        if(field_number == 0){
            checkpoints = astm_checkpoints;
        }
        else if(field_number == 1){
            checkpoints = stairs_checkpoints;
        }
        else if(field_number == 2){
            checkpoints = steps_checkpoints;
        }
        //使う経路gameobjectが保存されたgameobjectのリストの中から、ランダムで使う経路を選ぶ。
        cp_index=Random.Range(0,checkpoints.Length);
        if(cp_index_fixed!=10){
            cp_index=cp_index_fixed;//brainで経路固定したいときに使用
        }
        
        //使う経路内のウェイポイントそれぞれの初期化。cp_list内にそれぞれ格納する。
        checkpoints[cp_index].SetActive(true);
        count_check_point=checkpoints[cp_index].transform.childCount;
        cp_list=new GameObject[count_check_point];
        for(int i=0;i<count_check_point;i++){
            cp_list[i]=checkpoints[cp_index].transform.GetChild(i).gameObject;//checkpointのgbをここに補完
        }
        cp_reward=cp_reward/count_check_point;
        
        
    }

    //ウェイポイントに触れたときの処理。
    void OnTriggerEnter(Collider other){
        // 触れたものの名前を取得して、if処理で場合分け
        //ウェイポイントに触れた場合
        if(other.name=="check"+now_cp&&now_cp<count_check_point){
            // 触れたウェイポイントをfalseに
            cp_list[now_cp].SetActive(false);
            cp_through=true;
            turn_reward=0;
            if(now_cp==9){
                turn_reward=1.0f;
            }
            else if(now_cp==20){//cpをリセットする。
                for(int i=0;i<count_check_point;i++){//cpのリセット
                    cp_list[i].SetActive(true);
                }
                checkpoints[cp_index].SetActive(false);
            }
            //次のウェイポイントのindexに。
            now_cp++;
        }
        //click mode
        if(other.name=="click_point"+click_point_index){
            click_point_index++;
            touch_clickpoint=true;
        }
        //壁に触れた場合
        if(other.name=="backR"||other.name=="backL"||other.name=="frontR"||other.name=="frontL"){//壁に当たったら。
            touch_eight_wall=true;
        }
    }
    

}
