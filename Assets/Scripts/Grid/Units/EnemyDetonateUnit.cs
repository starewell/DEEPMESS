using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDetonateUnit : EnemyUnit
{


    public bool primed;
    [SerializeField] Animator explosion;

    public void PrimeSelf() {
        primed = true;
        gfxAnim.SetBool("Primed", true);
    }

    public void Explode() {
        gfxAnim.SetTrigger("Explode");
        explosion.gameObject.SetActive(true);
    }

    public override IEnumerator ScatterTurn()
    {
        if (!primed)
            yield return base.ScatterTurn();
        else
            yield return StartCoroutine(ExplodeCo());
    }

    public override IEnumerator CalculateAction()
    {
        if (!primed)
            yield return base.CalculateAction();
        else 
            yield return StartCoroutine(ExplodeCo());
    }

    IEnumerator ExplodeCo() {
        manager.SelectUnit(this);
        UpdateAction(equipment[1]);
        grid.DisplayValidCoords(validActionCoords, selectedEquipment.gridColor);
        yield return new WaitForSecondsRealtime(0.5f);
        Coroutine co = StartCoroutine(selectedEquipment.UseEquipment(this, null));
        grid.UpdateSelectedCursor(false, Vector2.one * -32);
        grid.DisableGridHighlight();
        StartCoroutine(TakeDamage(hpCurrent));
        yield return co;
        yield return new WaitForSecondsRealtime(0.125f);
        manager.DeselectUnit();
        yield return new WaitForSecondsRealtime(1);
        Debug.Log("ExplosionCo Finish");
    }

    public override IEnumerator DestroyElement(DamageType dmgType)
    {
        if (primed)
            yield return StartCoroutine(ExplodeCo());
        else
            yield return base.DestroyElement(dmgType);
    }

}
