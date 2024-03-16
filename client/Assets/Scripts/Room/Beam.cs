using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Beam : MonoBehaviour
{
    public List<int> HitPlayers { get; set; } = new List<int>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Player player))
        {
            HitPlayers.Add(player.PlayerId);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out Player player))
        {
            HitPlayers.Remove(player.PlayerId);
        }
    }
}
