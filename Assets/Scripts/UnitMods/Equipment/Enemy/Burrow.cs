using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Equipment/Attack/Burrow")]
public class Burrow : EquipmentData
{

    public int dmg;

    public override List<Vector2> TargetEquipment(GridElement user, int mod = 0) {
        List<Vector2> validCoords = EquipmentAdjacency.GetAdjacent(user.coord, range + mod, this, targetTypes);
        user.grid.DisplayValidCoords(validCoords, gridColor);
        
        bool valid = false;
        for (int i = validCoords.Count - 1; i >= 0; i--) {
            if (user.grid.CoordContents(validCoords[i]).Count > 0) {
                foreach (GridElement ge in user.grid.CoordContents(validCoords[i])) {
                    if (filters.Find(g => g.GetType() == ge.GetType()) != null) {
                        valid = true;
                        if (ge is Unit u) {
                            if (u.conditions.Contains(Unit.Status.Disabled)) valid = false;
                        }
                    }
                }
            }
            if (!valid) validCoords.Remove(validCoords[i]);
        }

        return validCoords;
    }

    
    public override IEnumerator UseEquipment(GridElement user, GridElement target = null)
    {
        yield return base.UseEquipment(user);
        yield return user.StartCoroutine(BurrowOnSelf(user));
        
    }

    public IEnumerator BurrowOnSelf(GridElement user) {
// Apply damage to units in AOE
        SpriteRenderer sr = Instantiate(vfx, user.grid.PosFromCoord(user.coord), Quaternion.identity).GetComponent<SpriteRenderer>();
        sr.sortingOrder = user.grid.SortOrderFromCoord(user.coord);
        List<Vector2> aoe = EquipmentAdjacency.GetAdjacent(user.coord, range, this, targetTypes);
        List<Coroutine> affectedCo = new();
        foreach (Vector2 coord in aoe) {
            if (user.grid.CoordContents(coord).Count > 0) {
                foreach (GridElement ge in user.grid.CoordContents(coord)) {
                    if (ge is Unit tu && ge != user) {
                        affectedCo.Add(tu.StartCoroutine(tu.TakeDamage(dmg)));
                    }
                }
            }
        }
        
        for (int i = affectedCo.Count - 1; i >= 0; i--) {
            if (affectedCo[i] != null) {
                yield return affectedCo[i];
            }
            else
                affectedCo.RemoveAt(i);
        }
        
        FloorManager.instance.Descend(false, true, user.coord);
    }

}
