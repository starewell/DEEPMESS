using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveManager : MonoBehaviour {

    [SerializeField] ObjectiveTracker tracker;
    [SerializeField] UpgradeManager upgrade;

    [SerializeField] GameObject assignAwardPanel;
    [SerializeField] GameObject objectiveCardParent, objectiveCardPrefab;

    [SerializeField] Button continueButton;

    [Header("Serialized Objective Pools")]
    [SerializeField] List<Objective> packetIObjectives;
    [SerializeField] List<Objective> packetIIObjectives, packetIIIObjectives;
    [SerializeField] List<Sprite> rewardSprites;
    public List<Objective> activeObjectives = new();

    bool reviewingObjectives;

    #region Singleton (and Awake)

    public static ObjectiveManager instance;
    private void Awake() {
        if (ObjectiveManager.instance) {
            Debug.Log("Warning! More than one instance of ObjectiveManager found!");
            return;
        }
        ObjectiveManager.instance = this;
    }
    #endregion

    public IEnumerator AssignSequence() {
        reviewingObjectives = true;
        assignAwardPanel.SetActive(true);
        continueButton.GetComponentInChildren<TMPro.TMP_Text>().text = "ACCEPT OBJECTIVES";

        activeObjectives = new();

// Randomly assign objectives
        ShuffleBag<Objective> rndBag = new();
        for (int i = packetIObjectives.Count - 1; i >= 0; i--)
            rndBag.Add(packetIObjectives[i]);
        for (int u = 0; u <= 2; u++)
            activeObjectives.Add(rndBag.Next());

// Create UI cards
        tracker.AssignObjectives(activeObjectives, rewardSprites);

        for (int i = objectiveCardParent.transform.childCount - 1; i >= 0; i--)
            Destroy(objectiveCardParent.transform.GetChild(i).gameObject);
        
        foreach(Objective ob in activeObjectives) {
            ob.Init();
            ObjectiveCard card = Instantiate(objectiveCardPrefab, objectiveCardParent.transform).GetComponent<ObjectiveCard>();
            card.Init(ob, rewardSprites[(int)ob.reward]);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(objectiveCardParent.GetComponent<RectTransform>());

        while (reviewingObjectives)
            yield return null;

        assignAwardPanel.SetActive(false);
    }

    public IEnumerator RewardSequence() {
        reviewingObjectives = true;
        assignAwardPanel.SetActive(true);
        continueButton.GetComponentInChildren<TMPro.TMP_Text>().text = "COLLECT REWARDS";

        foreach(Objective ob in activeObjectives)
            ob.ProgressCheck(true);


        while (reviewingObjectives)
            yield return null;

// Particles collected, sent to upgrade manager
        List<SlagEquipmentData.UpgradePath> particles = new();
        foreach (Objective ob in activeObjectives) {
            if (ob.succeeded) particles.Add(ob.reward);
        }
        upgrade.CollectParticles(particles);

        assignAwardPanel.SetActive(false);
    }

    public IEnumerator CollectRewards() {
        float timer = 0;
        while (timer <= 1) {

            yield return null;
            timer += Time.deltaTime;
        }
    }

    public void EndObjectiveSequence() {
        reviewingObjectives = false;
    }

}
