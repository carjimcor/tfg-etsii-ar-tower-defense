using UnityEngine;

[System.Serializable]
public struct HexCoordinates
{

    public int X { get; private set; }

    public int Z { get; private set; }

    public int Y
    {
        get
        {
            return -X - Z;
        }
    }

    public HexCoordinates(int x, int z)
    {
        X = x;
        Z = z;
    }

    public static HexCoordinates FromOffsetCoordinates(int x, int z)
    {
        HexCoordinates res = new HexCoordinates(x - z / 2, z);

        return res;
    }

    public static Vector2 Reverse(HexCoordinates coordinates)
    {
        Vector2 res = new Vector2(coordinates.X + coordinates.Z / 2, coordinates.Z);

        return res;
    }

    public static HexCoordinates FromPosition(Vector3 position, Vector2 size, Vector2 offsetAux)
    {
        position.x += offsetAux.x;
        position.z += offsetAux.y;

        float x = position.x / (HexMetrics.doubleInnerRadius);
        float y = -x;

        float offset = position.z / (HexMetrics.outerRadius * 3f);
        x -= offset;
        y -= offset;
        
        float z = -x - y;

        int iX = Mathf.RoundToInt(x);
        int iY = Mathf.RoundToInt(y);
        int iZ = Mathf.RoundToInt(z);

        if (iX + iY + iZ != 0)
        {
            float dX = Mathf.Abs(x - iX);
            float dY = Mathf.Abs(y - iY);
            float dZ = Mathf.Abs(-x - y - iZ);

            if (dX > dY && dX > dZ)
            {
                iX = -iY - iZ;
            }
            else if (dZ > dY)
            {
                iZ = -iX - iY;
            }
        }

        return new HexCoordinates(iX, iZ);

    }

    public override string ToString()
    {
        return "(" + X.ToString() + ", " + Y.ToString() + ", " + Z.ToString() + ")";
    }

    public string ToStringOnSeparateLines()
    {
        return X.ToString() + "\n" + Y.ToString() + "\n" + Z.ToString();
    }

}