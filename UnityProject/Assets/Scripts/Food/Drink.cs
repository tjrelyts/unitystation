﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drink : Edible {

	// Use this for initialization
	public override void TryEat()
	{
        isDrink = true;
        base.TryEat();
	}
}
