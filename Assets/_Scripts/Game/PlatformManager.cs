using UnityEngine;

public class PlatformManager
{
    private WorldCollision worldCollision;

    public PlatformManager(WorldCollision worldCollide)
    {
        worldCollision = worldCollide;
    }

    /// <summary>
    /// dès qu'il y a un seul objet grippable dans la liste, autorisé !
    /// </summary>
    public bool IsGrippable(out Collider coll)
    {
        for (int i = 0; i < worldCollision.ObjectInCollision.Length; i++)
        {
            if (worldCollision.ObjectInCollision[i] == null)
                continue;

            PlayerController playerController = worldCollision.ObjectInCollision[i].GetComponent<PlayerController>();
            if (playerController)
            {
                continue;
            }

            Platform platform = worldCollision.ObjectInCollision[i].GetComponent<Platform>();

            if (platform == null)
            {
                

                coll = worldCollision.ObjectInCollision[i].GetComponent<Collider>();
                return (true);  //si il n'y a pas de platform, grippable de base
                //continue;
            }

            if (platform.IsGrippable())
            {
                coll = worldCollision.ObjectInCollision[i].GetComponent<Collider>();
                return (true);
            }
        }
        coll = null;
        return (false);
    }

    /// <summary>
    /// dès qu'il y a un seul objet grippable dans la liste, autorisé !
    /// </summary>
    public bool IsWalkable(out Collider coll)
    {
        for (int i = 0; i < worldCollision.ObjectInCollision.Length; i++)
        {
            if (worldCollision.ObjectInCollision[i] == null)
                continue;
            Platform platform = worldCollision.ObjectInCollision[i].GetComponent<Platform>();
            if (platform == null)
            {
                coll = worldCollision.ObjectInCollision[i].GetComponent<Collider>();
                return (true);  //si il n'y a pas de platform, grippable de base
                //continue;
            }

            if (platform.IsWalkable())
            {
                coll = worldCollision.ObjectInCollision[i].GetComponent<Collider>();
                return (true);
            }
                
        }
        coll = null;
        return (false);
    }

    /// <summary>
    /// dès qu'il y a un seul objet grippable dans la liste, autorisé !
    /// </summary>
    public bool IsJumpable(out Collider coll)
    {
        for (int i = 0; i < worldCollision.ObjectInCollision.Length; i++)
        {
            if (worldCollision.ObjectInCollision[i] == null)
                continue;
            Platform platform = worldCollision.ObjectInCollision[i].GetComponent<Platform>();
            if (platform == null)
            {
                coll = worldCollision.ObjectInCollision[i].GetComponent<Collider>();
                return (true);  //si il n'y a pas de platform, grippable de base
                //continue;
            }

            if (platform.IsJumpable())
            {
                coll = worldCollision.ObjectInCollision[i].GetComponent<Collider>();
                return (true);
            }
        }
        coll = null;
        return (false);
    }
}