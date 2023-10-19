using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mergepins.Network
{
    public class TopIndexSelector : MonoBehaviour
    {
        private Player my;
        private Player enemy;

        public RawImage myIconImage;
        public RawImage enemyIconImage;

        public Text myNameText;
        public Text enemyNameText;

        public GameObject myLayoutGroup;
        public GameObject enemyLayoutGroup;

        public GameObject AllImages;

        public Text text;
        public Sprite[] pinSprites = new Sprite[10];

        private void OnEnable()
        {
            my = TurnManager.instance.my;
            enemy = TurnManager.instance.enemy;

            for (int i = 0; i < 6; i++)
            {
                GameObject partyUI = new GameObject("MyPartyMember" + i);
                partyUI.transform.SetParent(myLayoutGroup.transform);
                Image image = partyUI.AddComponent<Image>();
                image.sprite = pinSprites[my.GetTypeByIndex(i)];
                CustomButton button = partyUI.AddComponent<CustomButton>();
                button.SetImage(image);
                button.pressedColor = new Color(1, 1, 1, 0.8f);
                int j = i;
                button.OnClick.AddListener(() => ButtonPushed(j));
            }
            for (int i = 0; i < 6; i++)
            {
                GameObject partyUI = new GameObject("EnemyPartyMember" + i);
                partyUI.transform.SetParent(enemyLayoutGroup.transform);
                Image image = partyUI.AddComponent<Image>();
                image.sprite = pinSprites[enemy.GetTypeByIndex(i)];
            }

            AllImages.SetActive(true);
        }

        private void OnDisable()
        {
            text.text = "";
            DestroyAll();
        }

        private void ButtonPushed(int index)
        {
            text.text = "相手の選択を待っています";
            AllImages.SetActive(false);
            DestroyAll();
            MatchController.instance.CmdRegisterTopIndex(index);
        }

        private void DestroyAll()
        {
            foreach (Transform child in myLayoutGroup.transform)
                Destroy(child.gameObject);
            foreach (Transform child in enemyLayoutGroup.transform)
                Destroy(child.gameObject);
        }
    }
}
