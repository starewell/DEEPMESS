using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Equipment/BHammer")]
[System.Serializable]
public class BHammerData : HammerData
{

    public GridElement target1 = null;


    
    public override List<Vector2> TargetEquipment(GridElement user, int mod = 0) {

        if (target1 == null) {
            Debug.Log("Target strike");
            List<Vector2> validCoords = EquipmentAdjacency.GetAdjacent(user, range + mod, this, targetTypes);
            user.grid.DisplayValidCoords(validCoords, gridColor);
            if (user is PlayerUnit pu) pu.ui.ToggleEquipmentButtons();
            for (int i = validCoords.Count - 1; i >= 0; i--) {
                bool occupied = false;
                foreach (GridElement ge in FloorManager.instance.currentFloor.CoordContents(validCoords[i])) {
                    if (ge is not GroundElement) occupied = true;
                    bool remove = true;
                    foreach(GridElement target in targetTypes) {
                        if (ge.GetType() == target.GetType()) {
                            remove = false;
                            if (ge is EnemyUnit)
                                ge.elementCanvas.ToggleStatsDisplay(true);
                        }
                    }
                    if (remove || !occupied) {
                        if (validCoords.Count >= i)
                            validCoords.Remove(validCoords[i]);
                    }
                } 
            }
            return validCoords;
        } else {
            Debug.Log("Target lob");
            List<GridElement> targets = new List<GridElement>(); targets.Add(user);
            return EquipmentAdjacency.OfTypeOnBoardAdjacency(user, targets, user.coord);        
        }        
    }

    public override IEnumerator UseEquipment(GridElement user, GridElement target = null)
    {
        if (target1 != null) {
            Debug.Log("Click lob");
        // REPLACE w/ BASE.USEEQUIPMENT IF BROUGHT BACK INTO WORKING SCRIPTS
            user.energyCurrent -= energyCost;
                if (user is PlayerUnit pu) {
                    PlayerManager manager = (PlayerManager)pu.manager;
                    manager.undoableMoves = new Dictionary<Unit, Vector2>();
                    manager.undoOrder = new List<Unit>();
                }
            user.elementCanvas.UpdateStatsDisplay();
            yield return user.StartCoroutine(ThrowHammer((PlayerUnit)user, target1, (PlayerUnit)target));
        } else {
            Debug.Log("Click strike");
            target1 = target;
            Unit unit = (Unit)user;
            unit.grid.DisableGridHighlight();
            unit.validActionCoords = TargetEquipment(user);
            unit.grid.DisplayValidCoords(unit.validActionCoords, gridColor);
            foreach (Unit u in unit.manager.units) {
                if (u is PlayerUnit)
                    u.TargetElement(true);
            }
            yield return null;
        }
    }

    public override void EquipEquipment(GridElement user)
    {
        base.EquipEquipment(user);
    }

    public override IEnumerator ThrowHammer(PlayerUnit user, GridElement target, Unit passTo) {
        
        user.manager.DeselectUnit();

        PlayerManager manager = (PlayerManager)user.manager;
        if (target.gfx[0].sortingOrder > user.gfx[0].sortingOrder)
            hammer.GetComponentInChildren<SpriteRenderer>().sortingOrder = target.gfx[0].sortingOrder;
        AudioManager.PlaySound(AudioAtlas.Sound.hammerPass, user.transform.position);
    
// Lerp hammer to target
        float timer = 0;
        while (timer < animDur / 2) {
            hammer.transform.position = Vector3.Lerp(hammer.transform.position, FloorManager.instance.currentFloor.PosFromCoord(target.coord), timer/animDur);
            yield return null;
            timer += Time.deltaTime;
        }

        AudioManager.PlaySound(AudioAtlas.Sound.attackStrike, user.transform.position);
// Attack target if unit
        if (target is EnemyUnit) {
            target.StartCoroutine(target.TakeDamage(1));
        }

// Lerp hammer to passTo unit
        if (passTo.gfx[0].sortingOrder > hammer.GetComponentInChildren<SpriteRenderer>().sortingOrder)  
            hammer.GetComponentInChildren<SpriteRenderer>().sortingOrder = passTo.gfx[0].sortingOrder;
        timer = 0;
        while (timer < animDur) {
            hammer.transform.position = Vector3.Lerp(hammer.transform.position, FloorManager.instance.currentFloor.PosFromCoord(passTo.coord), timer/animDur);
            yield return null;
            timer += Time.deltaTime;
        }
    
        PassHammer((PlayerUnit)user, (PlayerUnit)passTo);

        if (target is Nail)
            manager.TriggerDescent();

    }
}
