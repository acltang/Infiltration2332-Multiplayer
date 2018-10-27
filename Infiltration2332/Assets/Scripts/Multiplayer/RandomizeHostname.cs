using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // InputField
using System;

public class RandomizeHostname : MonoBehaviour {

	public InputField hostname;
	

	// Use this for initialization
	void Start () {
		hostname.text = "Hostname";
	
		String[] hosts = new String[4];
		hosts[0] = "uw1-320-00.uwb.edu";
		hosts[1] = "uw1-320-01.uwb.edu";
		hosts[2] = "li996-174.members.linode.com";
		hosts[3] = "li1196-130.members.linode.com";
		
		System.Random r = new System.Random();
		
		int hostInt = r.Next(0,4);
		
		hostname.text = hosts[hostInt];		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
