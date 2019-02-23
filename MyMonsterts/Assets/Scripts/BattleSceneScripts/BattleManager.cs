using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.EventSystems;

public enum GameState{
	TurnStart,
	EnemyAttack,
	PlayerAttack,
	Command,
	BattleEnd
}

public class BattleManager : MonoBehaviour {

	public GameObject[] player;
	public GameObject[] enemy;

	public GameObject enemyImage;
	public GameObject myImage;

	public GameObject commandButton;

	private GameState battleGameState = GameState.TurnStart;
	//public Slider playerSlider;
	//public Slider enemySlider;

	private int skillPower = 80; 

	private int attackNum = 0;
	private int enemyAttackNum = 0;
	private int playerAttackNum = 0;

	private int selectedEnemyNum = 0;

	List<GameObject> sortedMonsters = new List<GameObject>();

	// Use this for initialization
	void Start () {
		for(int i =0;i<enemy.Length;i++){
			enemy[i].transform.GetChild(2).GetComponent<Text>().text = "Lv."+enemy[i].transform.GetChild(0).GetComponent<Monster>().Level;
			enemy[i].transform.GetChild(1).GetComponent<Slider>().maxValue = enemy[i].transform.GetChild(0).GetComponent<Monster>().Hp;
			enemy[i].transform.GetChild(1).GetComponent<Slider>().value = enemy[i].transform.GetChild(0).GetComponent<Monster>().Hp;
		}

		for(int i =0;i<player.Length;i++){
			player[i].transform.GetChild(2).GetComponent<Text>().text = "Lv."+player[i].transform.GetChild(0).GetComponent<Monster>().Level;
			player[i].transform.GetChild(1).GetComponent<Slider>().maxValue = player[i].transform.GetChild(0).GetComponent<Monster>().Hp;
			player[i].transform.GetChild(1).GetComponent<Slider>().value = player[i].transform.GetChild(0).GetComponent<Monster>().Hp;
		}

		Sort();

		if(sortedMonsters[0].GetComponent<Monster>().isEnemy){
			StartCoroutine("EnemyAttack",0);
		}else{
			//コマンド表示
			battleGameState = GameState.PlayerAttack;
			commandButton.SetActive(true);
			enemyImage.SetActive(false);
		}
	}
	
	// Update is called once per frame
	void Update () {
		if(battleGameState != GameState.BattleEnd){
			if(battleGameState == GameState.EnemyAttack){
				StartCoroutine ("EnemyAttack",attackNum);
				enemyImage.SetActive(true);
				battleGameState = GameState.PlayerAttack;
			}
		}

		if (Input.GetMouseButtonDown(0)) {
    		Vector2 tapPoint  = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    		Collider2D collition2d = Physics2D.OverlapPoint(tapPoint);
    		if (collition2d) {
        		RaycastHit2D hitObject = Physics2D.Raycast(tapPoint,-Vector2.up);
        		if (hitObject) {
            		selectedEnemyNum = int.Parse(hitObject.collider.gameObject.name.Substring(12));
					Color col = hitObject.collider.gameObject.transform.GetChild(3).GetComponent<SpriteRenderer>().color;
					col.a = 255;
					hitObject.collider.gameObject.transform.GetChild(3).GetComponent<SpriteRenderer>().color = col;
        		}
    		}
		}
	}

	void Sort(){
		Dictionary<GameObject, int> monsterSortDic = new Dictionary<GameObject, int>();
		for(int i = 0;i<player.Length;i++){
			monsterSortDic.Add(player[i].transform.GetChild(0).gameObject,player[i].transform.GetChild(0).GetComponent<Monster>().Spd); 
		}
		for(int i = 0;i<enemy.Length;i++){
			monsterSortDic.Add(enemy[i].transform.GetChild(0).gameObject,enemy[i].transform.GetChild(0).GetComponent<Monster>().Spd);
		}
		var sortedMonstersDic = monsterSortDic.OrderByDescending((x) => x.Value);
		foreach(var v in sortedMonstersDic){
			sortedMonsters.Add(v.Key);
		}
	}

	//[{(攻撃側のレベル × 2 ÷ 5 + 2) × (威力 × 攻撃側の攻撃or特攻 ÷ 防御側の防御or特防) ÷ 50 + 2} × 乱数幅(0.85,0.86,...0.99,1.00)] × 一致補正など

	private IEnumerator EnemyAttack(int order) {  
        yield return new WaitForSeconds (1f);  
		Debug.Log("敵の攻撃！");

		//ダメージ計算
		int rand = Random.Range(1, 4)-1;
		int damage = (int)(((sortedMonsters[order].GetComponent<Monster>().Level*2/5)+2)*
			((skillPower*sortedMonsters[order].GetComponent<Monster>().Atk/player[rand].transform.GetChild(0).GetComponent<Monster>().Def/50)+2));
		Debug.Log("プレイヤーに"+damage+"ダメージ！");
		player[rand].transform.GetChild(1).GetComponent<Slider>().value -= damage;

		//コマンド表示
		commandButton.SetActive(true);
		enemyImage.SetActive(false);

		//攻撃ターン管理
		if(attackNum == player.Length+enemy.Length-1){
			attackNum = 0;
		}else{
			attackNum++;
		}
		if(enemyAttackNum == enemy.Length-1){
			enemyAttackNum = 0;
		}else{
			enemyAttackNum++;
		}
    }

	public void Attack(){
		StartCoroutine ("PlayerAttack",attackNum);
	}

	private IEnumerator PlayerAttack(int order) {  
        yield return new WaitForSeconds (1f);  
		Debug.Log("プレイヤーの攻撃！");

		int rand = Random.Range(1, 4)-1;

		//ダメージ計算
		if(selectedEnemyNum == 0){
			int damage = (int)(((sortedMonsters[order].GetComponent<Monster>().Level*2/5)+2)*
			((skillPower*sortedMonsters[order].GetComponent<Monster>().Atk/enemy[rand].transform.GetChild(0).GetComponent<Monster>().Def/50)+2));
			Debug.Log("敵に"+damage+"ダメージ！");
			enemy[rand].transform.GetChild(1).GetComponent<Slider>().value -= damage;
		}else if(selectedEnemyNum == 1){
			int damage = (int)(((sortedMonsters[order].GetComponent<Monster>().Level*2/5)+2)*
			((skillPower*sortedMonsters[order].GetComponent<Monster>().Atk/enemy[0].transform.GetChild(0).GetComponent<Monster>().Def/50)+2));
			Debug.Log("敵に"+damage+"ダメージ！");
			enemy[0].transform.GetChild(1).GetComponent<Slider>().value -= damage;
		}else if(selectedEnemyNum == 2){
			int damage = (int)(((sortedMonsters[order].GetComponent<Monster>().Level*2/5)+2)*
			((skillPower*sortedMonsters[order].GetComponent<Monster>().Atk/enemy[1].transform.GetChild(0).GetComponent<Monster>().Def/50)+2));
			Debug.Log("敵に"+damage+"ダメージ！");
			enemy[1].transform.GetChild(1).GetComponent<Slider>().value -= damage;
		}else if(selectedEnemyNum == 3){
			int damage = (int)(((sortedMonsters[order].GetComponent<Monster>().Level*2/5)+2)*
			((skillPower*sortedMonsters[order].GetComponent<Monster>().Atk/enemy[2].transform.GetChild(0).GetComponent<Monster>().Def/50)+2));
			Debug.Log("敵に"+damage+"ダメージ！");
			enemy[2].transform.GetChild(1).GetComponent<Slider>().value -= damage;
		}

		selectedEnemyNum = 0;
		
		for(int i = 0;i<enemy.Length;i++){
			Color col = enemy[i].transform.GetChild(3).GetComponent<SpriteRenderer>().color;
			col.a = 0;
			enemy[i].transform.GetChild(3).GetComponent<SpriteRenderer>().color = col;
		}

		//コマンド表示
		commandButton.SetActive(false);
		battleGameState = GameState.EnemyAttack;
		
		//攻撃ターン管理
		if(attackNum == player.Length+enemy.Length-1){
			attackNum = 0;
		}else{
			attackNum++;
		}
		if(playerAttackNum == player.Length-1){
			playerAttackNum = 0;
		}else{
			playerAttackNum++;
		}
    }

	
}
