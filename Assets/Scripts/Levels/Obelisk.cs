using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obelisk : MonoBehaviour {

    [System.Serializable]
    public class HealthGlyph
    {
        public GameObject glyph = null;
        public GameObject glow = null;
    }

    public HealthGlyph[] healthGlyphs = new HealthGlyph[3];

    public int displayedHealth = -1;
    Animator animator = null;

    public void SetHealth(int health)
    {
        if(displayedHealth != health)
        {
            displayedHealth = health;
            for(int i = 0; i < healthGlyphs.Length; i++)
            {
                bool show = i+1 <= health;
                healthGlyphs[i].glyph.SetActive(!show);
                healthGlyphs[i].glow.SetActive(show);
            }
            if(health == 0 && animator)
            {
                animator.SetTrigger("OnDeath");
            }
        }
    }

	// Use this for initialization
	void Start () {
        animator = GetComponent<Animator>();
    }
	
	// Update is called once per frame
	void Update ()
    {
		
	}
}
